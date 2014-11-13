//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;
    using System.Diagnostics.CodeAnalysis;

    class ChannelDemuxer
    {
        public readonly static TimeSpan UseDefaultReceiveTimeout = TimeSpan.MinValue;

        TypedChannelDemuxer inputDemuxer;
        TypedChannelDemuxer replyDemuxer;
        Dictionary<Type, TypedChannelDemuxer> typeDemuxers;
        TimeSpan peekTimeout;
        int maxPendingSessions;

        public ChannelDemuxer()
        {
            this.peekTimeout = ChannelDemuxer.UseDefaultReceiveTimeout; //use the default receive timeout (original behavior)
            this.maxPendingSessions = 10;
            this.typeDemuxers = new Dictionary<Type, TypedChannelDemuxer>();
        }

        public TimeSpan PeekTimeout
        {
            get
            {
                return this.peekTimeout;
            }
            set
            {
                this.peekTimeout = value;
            }
        }

        public int MaxPendingSessions
        {
            get
            {
                return this.maxPendingSessions;
            }
            set
            {
                this.maxPendingSessions = value;
            }
        }

        public IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
            where TChannel : class, IChannel
        {
            return this.BuildChannelListener<TChannel>(context, new ChannelDemuxerFilter(new MatchAllMessageFilter(), 0));
        }

        public IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context, ChannelDemuxerFilter filter)
            where TChannel : class, IChannel
        {
            return GetTypedDemuxer(typeof(TChannel), context).BuildChannelListener<TChannel>(filter);
        }

        TypedChannelDemuxer CreateTypedDemuxer(Type channelType, BindingContext context)
        {
            if (channelType == typeof(IDuplexChannel))
                return (TypedChannelDemuxer)(object)new DuplexChannelDemuxer(context);
            if (channelType == typeof(IInputSessionChannel))
                return (TypedChannelDemuxer)(object)new InputSessionChannelDemuxer(context, this.peekTimeout, this.maxPendingSessions);
            if (channelType == typeof(IReplySessionChannel))
                return (TypedChannelDemuxer)(object)new ReplySessionChannelDemuxer(context, this.peekTimeout, this.maxPendingSessions);
            if (channelType == typeof(IDuplexSessionChannel))
                return (TypedChannelDemuxer)(object)new DuplexSessionChannelDemuxer(context, this.peekTimeout, this.maxPendingSessions);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "typeDemuxers", Justification = "No need to support type equivalence here.")]
        TypedChannelDemuxer GetTypedDemuxer(Type channelType, BindingContext context)
        {
            TypedChannelDemuxer typeDemuxer = null;
            bool createdDemuxer = false;

            if (channelType == typeof(IInputChannel))
            {
                if (this.inputDemuxer == null)
                {
                    if (context.CanBuildInnerChannelListener<IReplyChannel>())
                        this.inputDemuxer = this.replyDemuxer = new ReplyChannelDemuxer(context);
                    else
                        this.inputDemuxer = new InputChannelDemuxer(context);
                    createdDemuxer = true;
                }
                typeDemuxer = this.inputDemuxer;
            }
            else if (channelType == typeof(IReplyChannel))
            {
                if (this.replyDemuxer == null)
                {
                    this.inputDemuxer = this.replyDemuxer = new ReplyChannelDemuxer(context);
                    createdDemuxer = true;
                }
                typeDemuxer = this.replyDemuxer;
            }
            else if (!this.typeDemuxers.TryGetValue(channelType, out typeDemuxer))
            {
                typeDemuxer = this.CreateTypedDemuxer(channelType, context);
                this.typeDemuxers.Add(channelType, typeDemuxer);
                createdDemuxer = true;
            }

            if (!createdDemuxer)
            {
                context.RemainingBindingElements.Clear();
            }

            return (TypedChannelDemuxer)typeDemuxer;
        }
    }

    abstract class TypedChannelDemuxer
    {
        internal static void AbortMessage(Message message)
        {
            try
            {
                message.Close();
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (TimeoutException e)
            {
                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(e.Message);
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
        }

        public abstract IChannelListener<TChannel> BuildChannelListener<TChannel>(ChannelDemuxerFilter filter)
            where TChannel : class, IChannel;
    }

    //
    // Datagram demuxers
    //

    interface IChannelDemuxer
    {
        void OnOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout);
        IAsyncResult OnBeginOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state);
        void OnEndOuterListenerOpen(IAsyncResult result);
        void OnOuterListenerAbort(ChannelDemuxerFilter filter);
        void OnOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout);
        IAsyncResult OnBeginOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout, AsyncCallback callback, object state);
        void OnEndOuterListenerClose(IAsyncResult result);
    }

    abstract class DatagramChannelDemuxer<TInnerChannel, TInnerItem> : TypedChannelDemuxer, IChannelDemuxer
        where TInnerChannel : class, IChannel
        where TInnerItem : class, IDisposable
    {
        MessageFilterTable<IChannelListener> filterTable;
        TInnerChannel innerChannel;
        IChannelListener<TInnerChannel> innerListener;
        static AsyncCallback onReceiveComplete = Fx.ThunkCallback(new AsyncCallback(OnReceiveCompleteStatic));
        static Action<object> startReceivingStatic = new Action<object>(StartReceivingStatic);
        Action onItemDequeued;
        int openCount;
        IChannelDemuxFailureHandler demuxFailureHandler;
        // since the OnOuterListenerOpen method will be called for every outer listener and we will open
        // the inner listener only once, we need to ensure that all the outer listeners wait till the 
        // inner listener is opened.
        ThreadNeutralSemaphore openSemaphore;
        Exception pendingInnerListenerOpenException;
        bool abortOngoingOpen;

        public DatagramChannelDemuxer(BindingContext context)
        {
            this.filterTable = new MessageFilterTable<IChannelListener>();
            this.innerListener = context.BuildInnerChannelListener<TInnerChannel>();
            if (context.BindingParameters != null)
            {
                this.demuxFailureHandler = context.BindingParameters.Find<IChannelDemuxFailureHandler>();
            }
            this.openSemaphore = new ThreadNeutralSemaphore(1);
        }

        protected TInnerChannel InnerChannel
        {
            get { return this.innerChannel; }
        }

        protected IChannelListener<TInnerChannel> InnerListener
        {
            get { return this.innerListener; }
        }

        protected object ThisLock
        {
            get { return this; }
        }

        protected IChannelDemuxFailureHandler DemuxFailureHandler
        {
            get { return this.demuxFailureHandler; }
        }

        protected abstract void AbortItem(TInnerItem item);
        protected abstract IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract LayeredChannelListener<TChannel> CreateListener<TChannel>(ChannelDemuxerFilter filter) where TChannel : class, IChannel;
        protected abstract void Dispatch(IChannelListener listener);
        protected abstract void EndpointNotFound(TInnerItem item);
        protected abstract TInnerItem EndReceive(IAsyncResult result);
        protected abstract void EnqueueAndDispatch(IChannelListener listener, TInnerItem item, Action dequeuedCallback, bool canDispatchOnThisThread);
        protected abstract void EnqueueAndDispatch(IChannelListener listener, Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread);
        protected abstract Message GetMessage(TInnerItem item);

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(ChannelDemuxerFilter filter)
        {
            LayeredChannelListener<TChannel> listener = this.CreateListener<TChannel>(filter);
            listener.InnerChannelListener = this.innerListener;
            return listener;
        }

        // return false if BeginReceive should be called again
        bool HandleReceiveResult(IAsyncResult result)
        {
            TInnerItem item;
            try
            {
                item = this.EndReceive(result);
            }
            catch (CommunicationObjectFaultedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return true;
            }
            catch (CommunicationObjectAbortedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return true;
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return true;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (TimeoutException e)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(e.Message);
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
                return true;
            }

            if (item == null)
            {
                if (this.innerChannel.State == CommunicationState.Opened)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.PrematureDatagramEof, SR.GetString(SR.TraceCodePrematureDatagramEof),
                            null, this.innerChannel, null);
                    }
                }

                return true;
            }

            try
            {
                return this.ProcessItem(item);
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (TimeoutException e)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(e.Message);
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
                return true;
            }
        }

        IChannelListener MatchListener(Message message)
        {
            IChannelListener matchingListener = null;
            lock (this.ThisLock)
            {
                if (this.filterTable.GetMatchingValue(message, out matchingListener))
                {
                    return matchingListener;
                }
            }
            return null;
        }

        void OnItemDequeued()
        {
            this.StartReceiving();
        }

        static void StartReceivingStatic(object state)
        {
            ((DatagramChannelDemuxer<TInnerChannel, TInnerItem>)state).StartReceiving();
        }

        protected void HandleUnknownException(Exception exception)
        {
            DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);

            IChannelListener listener = null;
            lock (this.ThisLock)
            {
                if (this.filterTable.Count > 0)
                {
                    KeyValuePair<MessageFilter, IChannelListener>[] pairs = new KeyValuePair<MessageFilter, IChannelListener>[this.filterTable.Count];
                    this.filterTable.CopyTo(pairs, 0);
                    listener = pairs[0].Value;

                    if (this.onItemDequeued == null)
                    {
                        this.onItemDequeued = new Action(this.OnItemDequeued);
                    }
                    this.EnqueueAndDispatch(listener, exception, this.onItemDequeued, false);
                }
            }
        }

        void AbortState()
        {
            if (this.innerChannel != null)
            {
                this.innerChannel.Abort();
            }
            this.innerListener.Abort();
        }

        public void OnOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout)
        {
            bool closeInnerChannelAndListener = false;

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        closeInnerChannelAndListener = true;
                    }
                }
            }
            if (closeInnerChannelAndListener)
            {
                bool closeSucceeded = false;
                try
                {
                    if (this.innerChannel != null)
                    {
                        this.innerChannel.Close(timeoutHelper.RemainingTime());
                    }
                    this.innerListener.Close(timeoutHelper.RemainingTime());
                    closeSucceeded = true;
                }
                finally
                {
                    // we should abort the state since calling Abort on the channel demuxer will be a no-op
                    // due to the reference count being 0
                    if (!closeSucceeded)
                    {
                        AbortState();
                    }
                }
            }
        }

        public IAsyncResult OnBeginOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool closeInnerChannelAndListener = false;
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        closeInnerChannelAndListener = true;
                    }
                }
            }
            if (!closeInnerChannelAndListener)
            {
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                return new CloseAsyncResult(this, timeout, callback, state);
            }
        }

        public void OnEndOuterListenerClose(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                CloseAsyncResult.End(result);
            }
        }

        public void OnOuterListenerAbort(ChannelDemuxerFilter filter)
        {
            bool abortInnerChannelAndListener = false;
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        abortInnerChannelAndListener = true;
                        this.abortOngoingOpen = true;
                    }
                }
            }
            if (abortInnerChannelAndListener)
            {
                AbortState();
            }
        }

        void ThrowPendingOpenExceptionIfAny()
        {
            if (this.pendingInnerListenerOpenException != null)
            {
                if (pendingInnerListenerOpenException is CommunicationObjectAbortedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectAbortedException(SR.GetString(SR.PreviousChannelDemuxerOpenFailed, this.pendingInnerListenerOpenException.ToString())));
                }
                else if (pendingInnerListenerOpenException is CommunicationObjectFaultedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectFaultedException(SR.GetString(SR.PreviousChannelDemuxerOpenFailed, this.pendingInnerListenerOpenException.ToString())));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(SR.GetString(SR.PreviousChannelDemuxerOpenFailed, this.pendingInnerListenerOpenException.ToString())));
                }
            }
        }

        bool ShouldOpenInnerListener(ChannelDemuxerFilter filter, IChannelListener listener)
        {
            lock (this.ThisLock)
            {
                // the listener's Abort may be racing with Open
                if (listener.State == CommunicationState.Closed || listener.State == CommunicationState.Closing)
                {
                    return false;
                }
                this.filterTable.Add(filter.Filter, listener, filter.Priority);
                if (++this.openCount == 1)
                {
                    this.abortOngoingOpen = false;
                    return true;
                }
            }
            return false;
        }

        public void OnOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.openSemaphore.Enter(timeoutHelper.RemainingTime());
            try
            {
                bool openInnerListener = ShouldOpenInnerListener(filter, listener);
                if (openInnerListener)
                {
                    try
                    {
                        this.innerListener.Open(timeoutHelper.RemainingTime());
                        this.innerChannel = this.innerListener.AcceptChannel(timeoutHelper.RemainingTime());
                        this.innerChannel.Open(timeoutHelper.RemainingTime());

                        lock (ThisLock)
                        {
                            if (this.abortOngoingOpen)
                            {
                                this.AbortState();
                                return;
                            }
                        }

                        ActionItem.Schedule(startReceivingStatic, this);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        this.pendingInnerListenerOpenException = e;
                        throw;
                    }
                }
                else
                {
                    this.ThrowPendingOpenExceptionIfAny();
                }
            }
            finally
            {
                this.openSemaphore.Exit();
            }
        }

        public IAsyncResult OnBeginOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, filter, listener, timeout, callback, state);
        }

        public void OnEndOuterListenerOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        void OnReceiveComplete(IAsyncResult result)
        {
            if (!this.HandleReceiveResult(result))
            {
                this.StartReceiving();
            }
        }

        static void OnReceiveCompleteStatic(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;
            ((DatagramChannelDemuxer<TInnerChannel, TInnerItem>)result.AsyncState).OnReceiveComplete(result);
        }

        bool ProcessItem(TInnerItem item)
        {
            try
            {
                Message message = this.GetMessage(item);
                IChannelListener matchingListener = null;
                try
                {
                    matchingListener = MatchListener(message);
                }
                // The message may be bad because of which running the listener filters may throw
                // In that case, continue receiving
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return false;
                }
                catch (MultipleFilterMatchesException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return false;
                }
                catch (XmlException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return false;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    this.HandleUnknownException(e);
                    return true;
                }

                if (matchingListener == null)
                {
                    System.ServiceModel.Dispatcher.ErrorBehavior.ThrowAndCatch(
                        new EndpointNotFoundException(SR.GetString(SR.UnableToDemuxChannel, message.Headers.Action)), message);
                    // EndpointNotFound is responsible for closing the item
                    this.EndpointNotFound(item);
                    item = null;
                    return false;
                }

                if (this.onItemDequeued == null)
                {
                    this.onItemDequeued = new Action(this.OnItemDequeued);
                }
                this.EnqueueAndDispatch(matchingListener, item, this.onItemDequeued, false);
                item = null;
                return true;
            }
            finally
            {
                if (item != null)
                {
                    this.AbortItem(item);
                }
            }
        }

        void StartReceiving()
        {
            while (true)
            {
                if (this.innerChannel.State != CommunicationState.Opened)
                {
                    return;
                }

                IAsyncResult result;

                try
                {
                    result = this.BeginReceive(TimeSpan.MaxValue, onReceiveComplete, this);
                }
                catch (CommunicationObjectFaultedException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return;
                }
                catch (CommunicationObjectAbortedException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return;
                }
                catch (ObjectDisposedException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return;
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    continue;
                }
                catch (TimeoutException e)
                {
                    if (TD.ReceiveTimeoutIsEnabled())
                    {
                        TD.ReceiveTimeout(e.Message);
                    }

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    continue;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    this.HandleUnknownException(e);
                    return;
                }

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                if (this.HandleReceiveResult(result))
                {
                    return;
                }
            }
        }

        class OpenAsyncResult : AsyncResult
        {
            static FastAsyncCallback waitOverCallback = new FastAsyncCallback(WaitOverCallback);
            static AsyncCallback openListenerCallback = Fx.ThunkCallback(new AsyncCallback(OpenListenerCallback));
            static AsyncCallback acceptChannelCallback = Fx.ThunkCallback(new AsyncCallback(AcceptChannelCallback));
            static AsyncCallback openChannelCallback = Fx.ThunkCallback(new AsyncCallback(OpenChannelCallback));
            DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer;
            ChannelDemuxerFilter filter;
            IChannelListener listener;
            TimeoutHelper timeoutHelper;
            bool openInnerListener;

            public OpenAsyncResult(DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer, ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channelDemuxer = channelDemuxer;
                this.filter = filter;
                this.listener = listener;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (!this.channelDemuxer.openSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), waitOverCallback, this))
                {
                    return;
                }

                bool onWaitOverSucceeded = false;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.OnWaitOver();
                    onWaitOverSucceeded = true;
                }
                finally
                {
                    if (!onWaitOverSucceeded)
                    {
                        Cleanup();
                    }
                }
                if (completeSelf)
                {
                    Cleanup();
                    Complete(true);
                }
            }

            static void WaitOverCallback(object state, Exception asyncException)
            {
                OpenAsyncResult self = (OpenAsyncResult)state;
                Exception completionException = asyncException;
                bool completeSelf = false;

                if (completionException != null)
                {
                    completeSelf = true;
                }
                else
                {
                    try
                    {
                        completeSelf = self.OnWaitOver();
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        completeSelf = true;
                        completionException = e;
                    }
                }

                if (completeSelf)
                {
                    self.Cleanup();
                    self.Complete(false, completionException);
                }
            }

            bool OnWaitOver()
            {
                this.openInnerListener = this.channelDemuxer.ShouldOpenInnerListener(filter, listener);
                // the semaphore is obtained. Check if the inner listener needs to be opened. If not,
                // check if there is a pending exception obtained while opening the inner listener and throw
                // that
                if (!this.openInnerListener)
                {
                    this.channelDemuxer.ThrowPendingOpenExceptionIfAny();
                    return true;
                }
                else
                {
                    return this.OnOpenInnerListener();
                }
            }

            bool OnInnerListenerEndOpen(IAsyncResult result)
            {
                this.channelDemuxer.innerListener.EndOpen(result);
                result = this.channelDemuxer.innerListener.BeginAcceptChannel(this.timeoutHelper.RemainingTime(), acceptChannelCallback, this);

                if (!result.CompletedSynchronously)
                {
                    return false;
                }

                return this.OnEndAcceptChannel(result);
            }

            bool OnOpenInnerListener()
            {
                try
                {
                    IAsyncResult result = this.channelDemuxer.innerListener.BeginOpen(timeoutHelper.RemainingTime(), openListenerCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.OnInnerListenerEndOpen(result);
                    return true;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    this.channelDemuxer.pendingInnerListenerOpenException = e;
                    throw;
                }
            }

            static void OpenListenerCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                OpenAsyncResult self = (OpenAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    self.OnInnerListenerEndOpen(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    completionException = e;
                }
                if (completionException != null)
                {
                    self.channelDemuxer.pendingInnerListenerOpenException = completionException;
                }
                self.Cleanup();
                self.Complete(false, completionException);
            }

            static void AcceptChannelCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                OpenAsyncResult self = (OpenAsyncResult)result.AsyncState;
                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = self.OnEndAcceptChannel(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    completionException = e;
                    completeSelf = true;
                }
                if (completeSelf)
                {
                    if (completionException != null)
                    {
                        self.channelDemuxer.pendingInnerListenerOpenException = completionException;
                    }
                    self.Cleanup();
                    self.Complete(false, completionException);
                }
            }

            bool OnEndAcceptChannel(IAsyncResult result)
            {
                this.channelDemuxer.innerChannel = this.channelDemuxer.innerListener.EndAcceptChannel(result);
                IAsyncResult openResult = this.channelDemuxer.innerChannel.BeginOpen(this.timeoutHelper.RemainingTime(), acceptChannelCallback, this);

                if (!openResult.CompletedSynchronously)
                {
                    return false;
                }

                this.OnEndOpenChannel(openResult);
                return true;
            }

            static void OpenChannelCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                OpenAsyncResult self = (OpenAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    self.OnEndOpenChannel(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    completionException = e;
                }
                if (completionException != null)
                {
                    self.channelDemuxer.pendingInnerListenerOpenException = completionException;
                }
                self.Cleanup();
                self.Complete(false, completionException);
            }

            void OnEndOpenChannel(IAsyncResult result)
            {
                this.channelDemuxer.innerChannel.EndOpen(result);

                lock (this.channelDemuxer.ThisLock)
                {
                    if (this.channelDemuxer.abortOngoingOpen)
                    {
                        this.channelDemuxer.AbortState();
                        return;
                    }
                }

                ActionItem.Schedule(startReceivingStatic, this.channelDemuxer);
            }

            void Cleanup()
            {
                this.channelDemuxer.openSemaphore.Exit();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            static AsyncCallback sharedCallback = Fx.ThunkCallback(new AsyncCallback(SharedCallback));
            DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer;
            TimeoutHelper timeoutHelper;
            bool closedInnerChannel;

            public CloseAsyncResult(DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channelDemuxer = channelDemuxer;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (channelDemuxer.innerChannel != null)
                {
                    bool closeSucceeded = false;
                    try
                    {
                        IAsyncResult result = channelDemuxer.innerChannel.BeginClose(timeoutHelper.RemainingTime(), sharedCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            closeSucceeded = true;
                            return;
                        }
                        channelDemuxer.innerChannel.EndClose(result);
                        closeSucceeded = true;
                    }
                    finally
                    {
                        if (!closeSucceeded)
                        {
                            // we should abort the state since calling Abort on the channel demuxer will be a no-op
                            // due to the reference count being 0
                            this.channelDemuxer.AbortState();
                        }
                    }
                }
                if (OnInnerChannelClosed())
                {
                    Complete(true);
                }
            }

            bool OnInnerChannelClosed()
            {
                this.closedInnerChannel = true;
                bool closeSucceeded = false;
                try
                {
                    IAsyncResult result = channelDemuxer.innerListener.BeginClose(timeoutHelper.RemainingTime(), sharedCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        closeSucceeded = true;
                        return false;
                    }
                    channelDemuxer.innerListener.EndClose(result);
                    closeSucceeded = true;
                }
                finally
                {
                    if (!closeSucceeded)
                    {
                        // we should abort the state since calling Abort on the channel demuxer will be a no-op
                        // due to the reference count being 0
                        channelDemuxer.AbortState();
                    }
                }
                return true;
            }

            static void SharedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                CloseAsyncResult self = (CloseAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                bool closeSucceeded = false;
                try
                {
                    if (!self.closedInnerChannel)
                    {
                        self.channelDemuxer.innerChannel.EndClose(result);
                        completeSelf = self.OnInnerChannelClosed();
                        closeSucceeded = true;
                    }
                    else
                    {
                        self.channelDemuxer.innerListener.EndClose(result);
                        completeSelf = true;
                        closeSucceeded = true;
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    completeSelf = true;
                    completionException = e;
                }
                finally
                {
                    if (!closeSucceeded)
                    {
                        // we should abort the state since calling Abort on the channel demuxer will be a no-op
                        // due to the reference count being 0
                        self.channelDemuxer.AbortState();
                    }
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }
    }

    class InputChannelDemuxer : DatagramChannelDemuxer<IInputChannel, Message>
    {
        public InputChannelDemuxer(BindingContext context)
            : base(context)
        {
        }

        protected override void AbortItem(Message message)
        {
            AbortMessage(message);
        }

        protected override IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginReceive(timeout, callback, state);
        }

        protected override LayeredChannelListener<IInputChannel> CreateListener<IInputChannel>(ChannelDemuxerFilter filter)
        {
            SingletonChannelListener<IInputChannel, InputChannel, Message> listener = new SingletonChannelListener<IInputChannel, InputChannel, Message>(filter, this);
            listener.Acceptor = (IChannelAcceptor<IInputChannel>)new InputChannelAcceptor(listener);
            return listener;
        }

        protected override void Dispatch(IChannelListener listener)
        {
            SingletonChannelListener<IInputChannel, InputChannel, Message> singletonListener = (SingletonChannelListener<IInputChannel, InputChannel, Message>)listener;
            singletonListener.Dispatch();
        }

        protected override void EndpointNotFound(Message message)
        {
            if (this.DemuxFailureHandler != null)
            {
                this.DemuxFailureHandler.HandleDemuxFailure(message);
            }
            this.AbortItem(message);
        }

        protected override Message EndReceive(IAsyncResult result)
        {
            return this.InnerChannel.EndReceive(result);
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, Message message, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            SingletonChannelListener<IInputChannel, InputChannel, Message> singletonListener = (SingletonChannelListener<IInputChannel, InputChannel, Message>)listener;
            singletonListener.EnqueueAndDispatch(message, dequeuedCallback, canDispatchOnThisThread);
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            SingletonChannelListener<IInputChannel, InputChannel, Message> singletonListener = (SingletonChannelListener<IInputChannel, InputChannel, Message>)listener;
            singletonListener.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
        }

        protected override Message GetMessage(Message message)
        {
            return message;
        }
    }

    class DuplexChannelDemuxer : DatagramChannelDemuxer<IDuplexChannel, Message>
    {
        public DuplexChannelDemuxer(BindingContext context)
            : base(context)
        {
        }

        protected override void AbortItem(Message message)
        {
            AbortMessage(message);
        }

        protected override IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginReceive(timeout, callback, state);
        }

        protected override LayeredChannelListener<IDuplexChannel> CreateListener<IDuplexChannel>(ChannelDemuxerFilter filter)
        {
            SingletonChannelListener<IDuplexChannel, DuplexChannel, Message> listener = new SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>(filter, this);
            listener.Acceptor = (IChannelAcceptor<IDuplexChannel>)new DuplexChannelAcceptor(listener, this);
            return listener;
        }

        protected override void Dispatch(IChannelListener listener)
        {
            SingletonChannelListener<IDuplexChannel, DuplexChannel, Message> singletonListener = (SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>)listener;
            singletonListener.Dispatch();
        }

        protected override void EndpointNotFound(Message message)
        {
            if (this.DemuxFailureHandler != null)
            {
                this.DemuxFailureHandler.HandleDemuxFailure(message);
            }
            this.AbortItem(message);
        }

        protected override Message EndReceive(IAsyncResult result)
        {
            return this.InnerChannel.EndReceive(result);
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, Message message, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            SingletonChannelListener<IDuplexChannel, DuplexChannel, Message> singletonListener = (SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>)listener;
            singletonListener.EnqueueAndDispatch(message, dequeuedCallback, canDispatchOnThisThread);
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            SingletonChannelListener<IDuplexChannel, DuplexChannel, Message> singletonListener = (SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>)listener;
            singletonListener.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
        }

        protected override Message GetMessage(Message message)
        {
            return message;
        }

        class DuplexChannelAcceptor : SingletonChannelAcceptor<IDuplexChannel, DuplexChannel, Message>
        {
            DuplexChannelDemuxer demuxer;

            public DuplexChannelAcceptor(ChannelManagerBase channelManager, DuplexChannelDemuxer demuxer)
                : base(channelManager)
            {
                this.demuxer = demuxer;
            }

            protected override DuplexChannel OnCreateChannel()
            {
                return new DuplexChannelWrapper(this.ChannelManager, demuxer.InnerChannel);
            }

            protected override void OnTraceMessageReceived(Message message)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageReceived, SR.GetString(SR.TraceCodeMessageReceived),
                        MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null);
                }
            }
        }

        class DuplexChannelWrapper : DuplexChannel
        {
            IDuplexChannel innerChannel;

            public DuplexChannelWrapper(ChannelManagerBase channelManager, IDuplexChannel innerChannel)
                : base(channelManager, innerChannel.LocalAddress)
            {
                this.innerChannel = innerChannel;
            }

            public override EndpointAddress RemoteAddress
            {
                get { return this.innerChannel.RemoteAddress; }
            }

            public override Uri Via
            {
                get { return this.innerChannel.Via; }
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                this.innerChannel.Send(message, timeout);
            }

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
            }
        }
    }

    class ReplyChannelDemuxer : DatagramChannelDemuxer<IReplyChannel, RequestContext>
    {
        public ReplyChannelDemuxer(BindingContext context)
            : base(context)
        {
        }

        protected override void AbortItem(RequestContext request)
        {
            AbortMessage(request.RequestMessage);
            request.Abort();
        }

        protected override IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginReceiveRequest(timeout, callback, state);
        }

        protected override LayeredChannelListener<TChannel> CreateListener<TChannel>(ChannelDemuxerFilter filter)
        {
            if (typeof(TChannel) == typeof(IInputChannel))
            {
                SingletonChannelListener<IInputChannel, InputChannel, Message> listener = new SingletonChannelListener<IInputChannel, InputChannel, Message>(filter, this);
                listener.Acceptor = (IChannelAcceptor<IInputChannel>)new InputChannelAcceptor(listener);
                return (LayeredChannelListener<TChannel>)(object)listener;
            }
            else if (typeof(TChannel) == typeof(IReplyChannel))
            {
                SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext> listener = new SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext>(filter, this);
                listener.Acceptor = (IChannelAcceptor<IReplyChannel>)new ReplyChannelAcceptor(listener);
                return (LayeredChannelListener<TChannel>)(object)listener;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        protected override void Dispatch(IChannelListener listener)
        {
            SingletonChannelListener<IInputChannel, InputChannel, Message> inputListener = listener as SingletonChannelListener<IInputChannel, InputChannel, Message>;
            if (inputListener != null)
            {
                inputListener.Dispatch();
                return;
            }
            SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext> replyListener = listener as SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext>;
            if (replyListener != null)
            {
                replyListener.Dispatch();
                return;
            }

            throw Fx.AssertAndThrow("ReplyChannelDemuxer.Dispatch (false)");
        }

        void EndpointNotFoundCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            RequestContext item = (RequestContext)result.AsyncState;
            bool abortItem = true;
            try
            {
                ReplyChannelDemuxFailureAsyncResult.End(result);
                abortItem = false;
            }
            catch (TimeoutException e)
            {
                if (TD.SendTimeoutIsEnabled())
                {
                    TD.SendTimeout(e.Message);
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
            }
            finally
            {
                if (abortItem)
                {
                    this.AbortItem(item);
                }
            }
        }

        protected override void EndpointNotFound(RequestContext request)
        {
            bool abortItem = true;
            try
            {
                if (this.DemuxFailureHandler != null)
                {
                    try
                    {
                        ReplyChannelDemuxFailureAsyncResult result = new ReplyChannelDemuxFailureAsyncResult(this.DemuxFailureHandler, request, Fx.ThunkCallback(new AsyncCallback(this.EndpointNotFoundCallback)), request);
                        result.Start();
                        if (!result.CompletedSynchronously)
                        {
                            abortItem = false;
                            return;
                        }
                        ReplyChannelDemuxFailureAsyncResult.End(result);
                        abortItem = false;
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.SendTimeoutIsEnabled())
                        {
                            TD.SendTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (ObjectDisposedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        this.HandleUnknownException(e);
                    }
                }
            }
            finally
            {
                if (abortItem)
                {
                    this.AbortItem(request);
                }
            }
        }

        protected override RequestContext EndReceive(IAsyncResult result)
        {
            return this.InnerChannel.EndReceiveRequest(result);
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, RequestContext request, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            SingletonChannelListener<IInputChannel, InputChannel, Message> inputListener = listener as SingletonChannelListener<IInputChannel, InputChannel, Message>;
            if (inputListener != null)
            {
                inputListener.EnqueueAndDispatch(request.RequestMessage, dequeuedCallback, canDispatchOnThisThread);

                try
                {
                    request.Close();
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    if (TD.CloseTimeoutIsEnabled())
                    {
                        TD.CloseTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
            }
            SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext> replyListener = listener as SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext>;
            if (replyListener != null)
            {
                replyListener.EnqueueAndDispatch(request, dequeuedCallback, canDispatchOnThisThread);
                return;
            }

            throw Fx.AssertAndThrow("ReplyChannelDemuxer.EnqueueAndDispatch (false)");
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            SingletonChannelListener<IInputChannel, InputChannel, Message> inputListener = listener as SingletonChannelListener<IInputChannel, InputChannel, Message>;
            if (inputListener != null)
            {
                inputListener.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
                return;
            }

            SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext> replyListener = listener as SingletonChannelListener<IReplyChannel, ReplyChannel, RequestContext>;
            if (replyListener != null)
            {
                replyListener.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
                return;
            }

            throw Fx.AssertAndThrow("ReplyChannelDemuxer.EnqueueAndDispatch (false)");
        }

        protected override Message GetMessage(RequestContext request)
        {
            return request.RequestMessage;
        }
    }

    interface IChannelDemuxerFilter
    {
        ChannelDemuxerFilter Filter { get; }
    }

    class SingletonChannelListener<TChannel, TQueuedChannel, TQueuedItem> : DelegatingChannelListener<TChannel>, IChannelDemuxerFilter
        where TChannel : class, IChannel
        where TQueuedChannel : InputQueueChannel<TQueuedItem>
        where TQueuedItem : class, IDisposable
    {
        ChannelDemuxerFilter filter;
        IChannelDemuxer channelDemuxer;

        public SingletonChannelListener(ChannelDemuxerFilter filter, IChannelDemuxer channelDemuxer)
            : base(true)
        {
            this.filter = filter;
            this.channelDemuxer = channelDemuxer;
        }

        public ChannelDemuxerFilter Filter
        {
            get { return this.filter; }
        }

        SingletonChannelAcceptor<TChannel, TQueuedChannel, TQueuedItem> SingletonAcceptor
        {
            get { return (SingletonChannelAcceptor<TChannel, TQueuedChannel, TQueuedItem>)base.Acceptor; }
            set { this.Acceptor = value; }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.channelDemuxer.OnOuterListenerOpen(this.filter, this, timeoutHelper.RemainingTime());
            base.OnOpen(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, this.OnBeginOuterListenerOpen, this.OnEndOuterListenerOpen, base.OnBeginOpen, base.OnEndOpen);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        IAsyncResult OnBeginOuterListenerOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelDemuxer.OnBeginOuterListenerOpen(this.filter, this, timeout, callback, state);
        }

        void OnEndOuterListenerOpen(IAsyncResult result)
        {
            this.channelDemuxer.OnEndOuterListenerOpen(result);
        }

        protected override void OnAbort()
        {
            this.channelDemuxer.OnOuterListenerAbort(this.filter);
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.channelDemuxer.OnOuterListenerClose(this.filter, timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, this.OnBeginOuterListenerClose, this.OnEndOuterListenerClose, base.OnBeginClose, base.OnEndClose);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        IAsyncResult OnBeginOuterListenerClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelDemuxer.OnBeginOuterListenerClose(this.filter, timeout, callback, state);
        }

        void OnEndOuterListenerClose(IAsyncResult result)
        {
            this.channelDemuxer.OnEndOuterListenerClose(result);
        }

        public void Dispatch()
        {
            this.SingletonAcceptor.DispatchItems();
        }

        public void EnqueueAndDispatch(TQueuedItem item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            this.SingletonAcceptor.EnqueueAndDispatch(item, dequeuedCallback, canDispatchOnThisThread);
        }

        public void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            this.SingletonAcceptor.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
        }
    }

    //
    // Session demuxers
    //

    abstract class SessionChannelDemuxer<TInnerChannel, TInnerItem> : TypedChannelDemuxer, IChannelDemuxer
        where TInnerChannel : class, IChannel
        where TInnerItem : class, IDisposable
    {
        IChannelDemuxFailureHandler demuxFailureHandler;
        MessageFilterTable<InputQueueChannelListener<TInnerChannel>> filterTable;
        IChannelListener<TInnerChannel> innerListener;
        static AsyncCallback onAcceptComplete = Fx.ThunkCallback(new AsyncCallback(OnAcceptCompleteStatic));
        static AsyncCallback onPeekComplete = Fx.ThunkCallback(new AsyncCallback(OnPeekCompleteStatic));
        Action onItemDequeued;
        static WaitCallback scheduleAcceptStatic = new WaitCallback(ScheduleAcceptStatic);
        static Action<object> startAcceptStatic = new Action<object>(StartAcceptStatic);
        Action<object> onStartAccepting;
        int openCount;
        ThreadNeutralSemaphore openSemaphore;
        Exception pendingExceptionOnOpen;
        bool abortOngoingOpen;
        FlowThrottle throttle;
        TimeSpan peekTimeout;

        public SessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions)
        {
            if (context.BindingParameters != null)
            {
                this.demuxFailureHandler = context.BindingParameters.Find<IChannelDemuxFailureHandler>();
            }
            this.innerListener = context.BuildInnerChannelListener<TInnerChannel>();
            this.filterTable = new MessageFilterTable<InputQueueChannelListener<TInnerChannel>>();
            this.openSemaphore = new ThreadNeutralSemaphore(1);
            this.peekTimeout = peekTimeout;
            this.throttle = new FlowThrottle(scheduleAcceptStatic, maxPendingSessions, null, null);
        }

        protected object ThisLock
        {
            get { return this; }
        }

        protected IChannelDemuxFailureHandler DemuxFailureHandler
        {
            get { return this.demuxFailureHandler; }
        }

        Action<object> OnStartAccepting
        {
            get
            {
                if (this.onStartAccepting == null)
                {
                    this.onStartAccepting = new Action<object>(OnStartAcceptingCallback);
                }

                return this.onStartAccepting;
            }
        }

        protected abstract void AbortItem(TInnerItem item);
        protected abstract IAsyncResult BeginReceive(TInnerChannel channel, AsyncCallback callback, object state);
        protected abstract IAsyncResult BeginReceive(TInnerChannel channel, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract TInnerChannel CreateChannel(ChannelManagerBase channelManager, TInnerChannel innerChannel, TInnerItem firstItem);
        protected abstract void EndpointNotFound(TInnerChannel channel, TInnerItem item);
        protected abstract TInnerItem EndReceive(TInnerChannel channel, IAsyncResult result);
        protected abstract Message GetMessage(TInnerItem item);

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(ChannelDemuxerFilter filter)
        {
            Fx.Assert(typeof(TChannel) == typeof(TInnerChannel), "SessionChannelDemuxer.BuildChannelListener (typeof(TChannel) == typeof(TInnerChannel))");

            InputQueueChannelListener<TChannel> listener = new InputQueueChannelListener<TChannel>(filter, this);
            listener.InnerChannelListener = this.innerListener;
            return listener;
        }

        // return true if another BeginAcceptChannel should pend
        bool BeginAcceptChannel(bool requiresThrottle, out IAsyncResult result)
        {
            result = null;

            if (requiresThrottle && !this.throttle.Acquire(this))
            {
                return false;
            }

            bool releaseThrottle = true;

            try
            {
                result = this.innerListener.BeginAcceptChannel(TimeSpan.MaxValue, onAcceptComplete, this);
                releaseThrottle = false;
            }
            catch (CommunicationObjectFaultedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (CommunicationObjectAbortedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return true;
            }
            catch (TimeoutException e)
            {
                if (TD.OpenTimeoutIsEnabled())
                {
                    TD.OpenTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return true;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
                releaseThrottle = false;
                return false;
            }
            finally
            {
                if (releaseThrottle)
                {
                    this.throttle.Release();
                }
            }

            return true;
        }

        bool EndAcceptChannel(IAsyncResult result, out TInnerChannel channel)
        {
            channel = null;
            bool releaseThrottle = true;
            try
            {
                channel = this.innerListener.EndAcceptChannel(result);
                releaseThrottle = (channel == null);
            }
            catch (CommunicationObjectFaultedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (CommunicationObjectAbortedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return true;
            }
            catch (TimeoutException e)
            {
                if (TD.OpenTimeoutIsEnabled())
                {
                    TD.OpenTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return true;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
                releaseThrottle = false;
                return false;
            }
            finally
            {
                if (releaseThrottle)
                {
                    throttle.Release();
                }
            }

            return (channel != null);
        }

        void PeekChannel(TInnerChannel channel)
        {
            bool releaseThrottle = true;
            try
            {
                IAsyncResult peekResult = new PeekAsyncResult(this, channel, onPeekComplete, this);
                releaseThrottle = false;
                if (!peekResult.CompletedSynchronously)
                {
                    return;
                }
                channel = null;
                this.HandlePeekResult(peekResult);
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (TimeoutException e)
            {
                if (TD.OpenTimeoutIsEnabled())
                {
                    TD.OpenTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
                releaseThrottle = false;
            }

            if (channel != null)
            {
                channel.Abort();
            }

            if (releaseThrottle)
            {
                this.throttle.Release();
            }
        }

        void HandlePeekResult(IAsyncResult result)
        {
            TInnerChannel channel = null;
            TInnerItem item;
            bool abortChannel = false;
            bool releaseThrottle = true;
            try
            {
                PeekAsyncResult.End(result, out channel, out item);
                releaseThrottle = (item == null);
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                abortChannel = true;
                return;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                abortChannel = true;
                return;
            }
            catch (TimeoutException e)
            {
                if (TD.OpenTimeoutIsEnabled())
                {
                    TD.OpenTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                abortChannel = true;
                return;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
                releaseThrottle = false;
                return;
            }
            finally
            {
                if (abortChannel && channel != null)
                {
                    channel.Abort();
                }

                if (releaseThrottle)
                {
                    this.throttle.Release();
                }
            }

            if (item != null)
            {
                releaseThrottle = true;

                try
                {
                    this.ProcessItem(channel, item);
                    releaseThrottle = false;
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    if (TD.OpenTimeoutIsEnabled())
                    {
                        TD.OpenTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    this.HandleUnknownException(e);
                    releaseThrottle = false;
                }
                finally
                {
                    if (releaseThrottle)
                    {
                        this.throttle.Release();
                    }
                }
            }
        }

        InputQueueChannelListener<TInnerChannel> MatchListener(Message message)
        {
            InputQueueChannelListener<TInnerChannel> matchingListener = null;
            lock (this.ThisLock)
            {
                if (this.filterTable.GetMatchingValue(message, out matchingListener))
                {
                    return matchingListener;
                }
            }
            return null;
        }

        static void OnAcceptCompleteStatic(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            ((SessionChannelDemuxer<TInnerChannel, TInnerItem>)result.AsyncState).OnStartAcceptingCallback(result);
        }

        static void ScheduleAcceptStatic(object state)
        {
            ActionItem.Schedule(startAcceptStatic, state);
        }

        static void StartAcceptStatic(object state)
        {
            ((SessionChannelDemuxer<TInnerChannel, TInnerItem>)state).StartAccepting(false);
        }

        bool ShouldStartAccepting(ChannelDemuxerFilter filter, IChannelListener listener)
        {
            lock (this.ThisLock)
            {
                // the listener's Abort may be racing with Open
                if (listener.State == CommunicationState.Closed || listener.State == CommunicationState.Closing)
                {
                    return false;
                }

                this.filterTable.Add(filter.Filter, (InputQueueChannelListener<TInnerChannel>)(object)listener, filter.Priority);
                if (++this.openCount == 1)
                {
                    this.abortOngoingOpen = false;
                    return true;
                }
            }
            return false;
        }

        void StartAccepting(bool requiresThrottle)
        {
            IAsyncResult acceptResult;
            bool acceptValid = this.BeginAcceptChannel(requiresThrottle, out acceptResult);
            if (acceptValid && (acceptResult == null || acceptResult.CompletedSynchronously))
            {
                // need to spawn another thread to process this completion
                ActionItem.Schedule(OnStartAccepting, acceptResult);
            }
        }

        void OnItemDequeued()
        {
            this.throttle.Release();
        }

        void ThrowPendingOpenExceptionIfAny()
        {
            if (this.pendingExceptionOnOpen != null)
            {
                if (pendingExceptionOnOpen is CommunicationObjectAbortedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectAbortedException(SR.GetString(SR.PreviousChannelDemuxerOpenFailed, this.pendingExceptionOnOpen.ToString())));
                }
                else if (pendingExceptionOnOpen is CommunicationObjectFaultedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectFaultedException(SR.GetString(SR.PreviousChannelDemuxerOpenFailed, this.pendingExceptionOnOpen.ToString())));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(SR.GetString(SR.PreviousChannelDemuxerOpenFailed, this.pendingExceptionOnOpen.ToString())));
                }
            }
        }

        public void OnOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.openSemaphore.Enter(timeoutHelper.RemainingTime());
            try
            {
                bool startAccepting = ShouldStartAccepting(filter, listener);
                if (startAccepting)
                {
                    try
                    {
                        this.innerListener.Open(timeoutHelper.RemainingTime());
                        StartAccepting(true);
                        lock (ThisLock)
                        {
                            if (this.abortOngoingOpen)
                            {
                                this.innerListener.Abort();
                            }
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        this.pendingExceptionOnOpen = e;
                        throw;
                    }
                }
                else
                {
                    this.ThrowPendingOpenExceptionIfAny();
                }
            }
            finally
            {
                this.openSemaphore.Exit();
            }
        }


        public IAsyncResult OnBeginOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, filter, listener, timeout, callback, state);
        }

        public void OnEndOuterListenerOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        bool ShouldCloseInnerListener(ChannelDemuxerFilter filter, bool aborted)
        {
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        if (aborted)
                        {
                            this.abortOngoingOpen = true;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void OnOuterListenerAbort(ChannelDemuxerFilter filter)
        {
            if (ShouldCloseInnerListener(filter, true))
            {
                innerListener.Abort();
            }
        }

        public void OnOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout)
        {
            if (ShouldCloseInnerListener(filter, false))
            {
                bool closeSucceeded = false;
                try
                {
                    innerListener.Close(timeout);
                    closeSucceeded = true;
                }
                finally
                {
                    if (!closeSucceeded)
                    {
                        // we should abort the state since calling Abort on the channel demuxer will be a no-op
                        // due to the reference count being 0
                        innerListener.Abort();
                    }
                }
            }
        }

        public IAsyncResult OnBeginOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (ShouldCloseInnerListener(filter, false))
            {
                bool closeSucceeded = false;
                try
                {
                    IAsyncResult result = this.innerListener.BeginClose(timeout, callback, state);
                    closeSucceeded = true;
                    return result;
                }
                finally
                {
                    if (!closeSucceeded)
                    {
                        // we should abort the state since calling Abort on the channel demuxer will be a no-op
                        // due to the reference count being 0
                        this.innerListener.Abort();
                    }
                }
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        public void OnEndOuterListenerClose(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                bool closeSucceeded = false;
                try
                {
                    this.innerListener.EndClose(result);
                    closeSucceeded = true;
                }
                finally
                {
                    if (!closeSucceeded)
                    {
                        // we should abort the state since calling Abort on the channel demuxer will be a no-op
                        // due to the reference count being 0
                        this.innerListener.Abort();
                    }
                }
            }
        }

        void OnStartAcceptingCallback(object state)
        {
            IAsyncResult result = (IAsyncResult)state;
            TInnerChannel channel = null;

            if (result == null || this.EndAcceptChannel(result, out channel))
            {
                this.StartAccepting(channel);
            }
        }

        static void OnPeekCompleteStatic(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            SessionChannelDemuxer<TInnerChannel, TInnerItem> demuxer
                = (SessionChannelDemuxer<TInnerChannel, TInnerItem>)result.AsyncState;

            bool releaseThrottle = true;

            try
            {
                demuxer.HandlePeekResult(result);
                releaseThrottle = false;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                demuxer.HandleUnknownException(e);
                releaseThrottle = false;
            }
            finally
            {
                if (releaseThrottle)
                {
                    demuxer.throttle.Release();
                }
            }
        }

        void ProcessItem(TInnerChannel channel, TInnerItem item)
        {
            InputQueueChannelListener<TInnerChannel> listener = null;
            TInnerChannel wrappedChannel = null;
            bool releaseThrottle = true;

            try
            {
                Message message = this.GetMessage(item);
                try
                {
                    listener = MatchListener(message);
                    releaseThrottle = (listener == null);
                }
                // MatchListener could run the filters against an untrusted message and could throw.
                // If so, abort the session
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return;
                }
                catch (MultipleFilterMatchesException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return;
                }
                catch (XmlException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return;
                }
                finally
                {
                    if (releaseThrottle)
                    {
                        this.throttle.Release();
                    }
                }

                if (listener == null)
                {
                    try
                    {
                        throw TraceUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(SR.UnableToDemuxChannel, message.Headers.Action)), message);
                    }
                    catch (EndpointNotFoundException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        this.EndpointNotFound(channel, item);
                        // EndpointNotFound is responsible for closing and aborting the channel
                        channel = null;
                        item = null;
                    }
                    return;
                }

                wrappedChannel = this.CreateChannel(listener, channel, item);
                channel = null;
                item = null;
            }
            finally
            {
                if (item != null)
                {
                    this.AbortItem(item);
                }
                if (channel != null)
                {
                    channel.Abort();
                }
            }

            bool enqueueSucceeded = false;
            try
            {
                if (this.onItemDequeued == null)
                {
                    this.onItemDequeued = new Action(this.OnItemDequeued);
                }

                listener.InputQueueAcceptor.EnqueueAndDispatch(wrappedChannel, this.onItemDequeued, false);
                enqueueSucceeded = true;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
            }
            finally
            {
                if (!enqueueSucceeded)
                {
                    this.throttle.Release();
                    wrappedChannel.Abort();
                }
            }
        }

        protected void HandleUnknownException(Exception exception)
        {
            InputQueueChannelListener<TInnerChannel> listener = null;

            lock (this.ThisLock)
            {
                if (this.filterTable.Count > 0)
                {
                    KeyValuePair<MessageFilter, InputQueueChannelListener<TInnerChannel>>[] pairs = new KeyValuePair<MessageFilter, InputQueueChannelListener<TInnerChannel>>[this.filterTable.Count];
                    this.filterTable.CopyTo(pairs, 0);
                    listener = pairs[0].Value;

                    if (this.onItemDequeued == null)
                    {
                        this.onItemDequeued = new Action(OnItemDequeued);
                    }

                    listener.InputQueueAcceptor.EnqueueAndDispatch(exception, this.onItemDequeued, false);
                }
            }
        }

        void StartAccepting(TInnerChannel channelToPeek)
        {
            for (;;)
            {
                IAsyncResult result;
                bool acceptValid = this.BeginAcceptChannel(true, out result);

                if (channelToPeek != null)
                {
                    if (acceptValid && (result == null || result.CompletedSynchronously))
                    {
                        // need to spawn another thread to process this completion
                        // since we're going to process channelToPeek on this thread
                        ActionItem.Schedule(OnStartAccepting, result);
                    }

                    PeekChannel(channelToPeek);
                    return;
                }
                else
                {
                    if (!acceptValid)
                    {
                        return; // we're done, listener is toast
                    }

                    if (result == null)
                    {
                        continue;
                    }

                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }

                    if (!this.EndAcceptChannel(result, out channelToPeek))
                    {
                        return;
                    }
                }
            }
        }

        class PeekAsyncResult : AsyncResult
        {
            TInnerChannel channel;
            SessionChannelDemuxer<TInnerChannel, TInnerItem> demuxer;
            TInnerItem item;
            static AsyncCallback onOpenComplete = Fx.ThunkCallback(new AsyncCallback(OnOpenCompleteStatic));
            static AsyncCallback onReceiveComplete = Fx.ThunkCallback(new AsyncCallback(OnReceiveCompleteStatic));

            public PeekAsyncResult(SessionChannelDemuxer<TInnerChannel, TInnerItem> demuxer, TInnerChannel channel, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.demuxer = demuxer;
                this.channel = channel;
                IAsyncResult result = this.channel.BeginOpen(onOpenComplete, this);
                if (!result.CompletedSynchronously)
                {
                    return;
                }
                if (this.HandleOpenComplete(result))
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result, out TInnerChannel channel, out TInnerItem item)
            {
                PeekAsyncResult peekResult = AsyncResult.End<PeekAsyncResult>(result);
                channel = peekResult.channel;
                item = peekResult.item;
            }

            bool HandleOpenComplete(IAsyncResult result)
            {
                this.channel.EndOpen(result);

                IAsyncResult receiveResult;

                if (this.demuxer.peekTimeout == ChannelDemuxer.UseDefaultReceiveTimeout)
                {
                    //use the default ReceiveTimeout for the channel
                    receiveResult = this.demuxer.BeginReceive(this.channel, onReceiveComplete, this);
                }
                else
                {
                    receiveResult = this.demuxer.BeginReceive(this.channel, this.demuxer.peekTimeout, onReceiveComplete, this);
                }

                if (receiveResult.CompletedSynchronously)
                {
                    this.HandleReceiveComplete(receiveResult);
                    return true;
                }

                return false;
            }

            static void OnOpenCompleteStatic(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                PeekAsyncResult peekAsyncResult = (PeekAsyncResult)result.AsyncState;

                bool completeSelf = false;
                Exception exception = null;

                try
                {
                    completeSelf = peekAsyncResult.HandleOpenComplete(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    exception = e;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    peekAsyncResult.Complete(false, exception);
                }
            }

            void HandleReceiveComplete(IAsyncResult result)
            {
                this.item = this.demuxer.EndReceive(this.channel, result);
            }

            static void OnReceiveCompleteStatic(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                PeekAsyncResult peekAsyncResult = (PeekAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    peekAsyncResult.HandleReceiveComplete(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    exception = e;
                }

                peekAsyncResult.Complete(false, exception);
            }
        }

        class OpenAsyncResult : AsyncResult
        {
            static FastAsyncCallback waitOverCallback = new FastAsyncCallback(WaitOverCallback);
            static AsyncCallback openListenerCallback = Fx.ThunkCallback(new AsyncCallback(OpenListenerCallback));
            SessionChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer;
            ChannelDemuxerFilter filter;
            IChannelListener listener;
            TimeoutHelper timeoutHelper;
            bool startAccepting;

            public OpenAsyncResult(SessionChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer, ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channelDemuxer = channelDemuxer;
                this.filter = filter;
                this.listener = listener;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (!this.channelDemuxer.openSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), waitOverCallback, this))
                {
                    return;
                }

                bool waitOverSucceeded = false;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.OnWaitOver();
                    waitOverSucceeded = true;
                }
                finally
                {
                    if (!waitOverSucceeded)
                    {
                        Cleanup();
                    }
                }
                if (completeSelf)
                {
                    Cleanup();
                    Complete(true);
                }
            }

            static void WaitOverCallback(object state, Exception asyncException)
            {
                OpenAsyncResult self = (OpenAsyncResult)state;
                bool completeSelf = false;
                Exception completionException = asyncException;
                if (completionException != null)
                {
                    completeSelf = true;
                }
                else
                {
                    try
                    {
                        completeSelf = self.OnWaitOver();
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        completeSelf = true;
                        completionException = e;
                    }
                }

                if (completeSelf)
                {
                    self.Cleanup();
                    self.Complete(false, completionException);
                }
            }

            bool OnWaitOver()
            {
                this.startAccepting = this.channelDemuxer.ShouldStartAccepting(this.filter, this.listener);
                if (!this.startAccepting)
                {
                    this.channelDemuxer.ThrowPendingOpenExceptionIfAny();
                    return true;
                }
                else
                {
                    return this.OnStartAccepting();
                }
            }

            void OnEndInnerListenerOpen(IAsyncResult result)
            {
                this.channelDemuxer.innerListener.EndOpen(result);
                this.channelDemuxer.StartAccepting(true);
                lock (this.channelDemuxer.ThisLock)
                {
                    if (this.channelDemuxer.abortOngoingOpen)
                    {
                        this.channelDemuxer.innerListener.Abort();
                    }
                }
            }

            bool OnStartAccepting()
            {
                try
                {
                    IAsyncResult result = this.channelDemuxer.innerListener.BeginOpen(timeoutHelper.RemainingTime(), openListenerCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.OnEndInnerListenerOpen(result);
                    return true;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    this.channelDemuxer.pendingExceptionOnOpen = e;
                    throw;
                }
            }

            static void OpenListenerCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                OpenAsyncResult self = (OpenAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    self.OnEndInnerListenerOpen(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    completionException = e;
                }
                if (completionException != null)
                {
                    self.channelDemuxer.pendingExceptionOnOpen = completionException;
                }
                self.Cleanup();
                self.Complete(false, completionException);
            }

            void Cleanup()
            {
                this.channelDemuxer.openSemaphore.Exit();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }
        }
    }

    class InputSessionChannelDemuxer : SessionChannelDemuxer<IInputSessionChannel, Message>
    {
        public InputSessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions)
            : base(context, peekTimeout, maxPendingSessions)
        {
        }

        protected override void AbortItem(Message message)
        {
            AbortMessage(message);
        }

        protected override IAsyncResult BeginReceive(IInputSessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(callback, state);
        }

        protected override IAsyncResult BeginReceive(IInputSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(timeout, callback, state);
        }

        protected override IInputSessionChannel CreateChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, Message firstMessage)
        {
            return new InputSessionChannelWrapper(channelManager, innerChannel, firstMessage);
        }

        protected override void EndpointNotFound(IInputSessionChannel channel, Message message)
        {
            if (this.DemuxFailureHandler != null)
            {
                this.DemuxFailureHandler.HandleDemuxFailure(message);
            }
            this.AbortItem(message);
            channel.Abort();
        }

        protected override Message EndReceive(IInputSessionChannel channel, IAsyncResult result)
        {
            return channel.EndReceive(result);
        }

        protected override Message GetMessage(Message message)
        {
            return message;
        }
    }

    class InputSessionChannelWrapper : InputChannelWrapper, IInputSessionChannel
    {
        public InputSessionChannelWrapper(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, Message firstMessage)
            : base(channelManager, innerChannel, firstMessage)
        {
        }

        new IInputSessionChannel InnerChannel
        {
            get { return (IInputSessionChannel)base.InnerChannel; }
        }

        public IInputSession Session
        {
            get { return this.InnerChannel.Session; }
        }
    }

    class DuplexSessionChannelDemuxer : SessionChannelDemuxer<IDuplexSessionChannel, Message>
    {
        public DuplexSessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions)
            : base(context, peekTimeout, maxPendingSessions)
        {
        }

        protected override void AbortItem(Message message)
        {
            AbortMessage(message);
        }

        protected override IAsyncResult BeginReceive(IDuplexSessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(callback, state);
        }

        protected override IAsyncResult BeginReceive(IDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(timeout, callback, state);
        }

        protected override IDuplexSessionChannel CreateChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel, Message firstMessage)
        {
            return new DuplexSessionChannelWrapper(channelManager, innerChannel, firstMessage);
        }

        void EndpointNotFoundCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            ChannelAndMessageAsyncState channelAndMessage = (ChannelAndMessageAsyncState)result.AsyncState;
            bool doAbort = true;
            try
            {
                DuplexSessionDemuxFailureAsyncResult.End(result);
                doAbort = false;
            }
            catch (TimeoutException e)
            {
                if (TD.SendTimeoutIsEnabled())
                {
                    TD.SendTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
            }
            finally
            {
                if (doAbort)
                {
                    this.AbortItem(channelAndMessage.message);
                    channelAndMessage.channel.Abort();
                }
            }
        }

        protected override void EndpointNotFound(IDuplexSessionChannel channel, Message message)
        {
            bool doAbort = true;
            try
            {
                if (this.DemuxFailureHandler != null)
                {
                    try
                    {
                        DuplexSessionDemuxFailureAsyncResult result = new DuplexSessionDemuxFailureAsyncResult(this.DemuxFailureHandler, channel, message, Fx.ThunkCallback(new AsyncCallback(this.EndpointNotFoundCallback)), new ChannelAndMessageAsyncState(channel, message));
                        result.Start();
                        if (!result.CompletedSynchronously)
                        {
                            doAbort = false;
                            return;
                        }
                        DuplexSessionDemuxFailureAsyncResult.End(result);
                        doAbort = false;
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.SendTimeoutIsEnabled())
                        {
                            TD.SendTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (ObjectDisposedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        this.HandleUnknownException(e);
                    }
                }
            }
            finally
            {
                if (doAbort)
                {
                    this.AbortItem(message);
                    channel.Abort();
                }
            }
        }

        protected override Message EndReceive(IDuplexSessionChannel channel, IAsyncResult result)
        {
            return channel.EndReceive(result);
        }

        protected override Message GetMessage(Message message)
        {
            return message;
        }

        struct ChannelAndMessageAsyncState
        {
            public IChannel channel;
            public Message message;

            public ChannelAndMessageAsyncState(IChannel channel, Message message)
            {
                this.channel = channel;
                this.message = message;
            }
        }
    }

    class DuplexSessionChannelWrapper : InputChannelWrapper, IDuplexSessionChannel
    {
        public DuplexSessionChannelWrapper(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel, Message firstMessage)
            : base(channelManager, innerChannel, firstMessage)
        {
        }

        new IDuplexSessionChannel InnerChannel
        {
            get { return (IDuplexSessionChannel)base.InnerChannel; }
        }

        public IDuplexSession Session
        {
            get { return InnerChannel.Session; }
        }

        public EndpointAddress RemoteAddress
        {
            get { return InnerChannel.RemoteAddress; }
        }

        public Uri Via
        {
            get { return InnerChannel.Via; }
        }

        public void Send(Message message)
        {
            this.InnerChannel.Send(message);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.InnerChannel.Send(message, timeout);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginSend(message, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.InnerChannel.EndSend(result);
        }
    }

    class ReplySessionChannelDemuxer : SessionChannelDemuxer<IReplySessionChannel, RequestContext>
    {
        public ReplySessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions)
            : base(context, peekTimeout, maxPendingSessions)
        {
        }

        protected override void AbortItem(RequestContext request)
        {
            AbortMessage(request.RequestMessage);
            request.Abort();
        }

        protected override IAsyncResult BeginReceive(IReplySessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginReceiveRequest(callback, state);
        }

        protected override IAsyncResult BeginReceive(IReplySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginReceiveRequest(timeout, callback, state);
        }

        protected override IReplySessionChannel CreateChannel(ChannelManagerBase channelManager, IReplySessionChannel innerChannel, RequestContext firstRequest)
        {
            return new ReplySessionChannelWrapper(channelManager, innerChannel, firstRequest);
        }

        void EndpointNotFoundCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            ChannelAndRequestAsyncState channelAndRequest = (ChannelAndRequestAsyncState)result.AsyncState;
            bool doAbort = true;
            try
            {
                ReplySessionDemuxFailureAsyncResult.End(result);
                doAbort = false;
            }
            catch (TimeoutException e)
            {
                if (TD.SendTimeoutIsEnabled())
                {
                    TD.SendTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                this.HandleUnknownException(e);
            }
            finally
            {
                if (doAbort)
                {
                    this.AbortItem(channelAndRequest.request);
                    channelAndRequest.channel.Abort();
                }
            }
        }

        protected override void EndpointNotFound(IReplySessionChannel channel, RequestContext request)
        {
            bool doAbort = true;
            try
            {
                if (this.DemuxFailureHandler != null)
                {
                    try
                    {
                        ReplySessionDemuxFailureAsyncResult result = new ReplySessionDemuxFailureAsyncResult(this.DemuxFailureHandler, request, channel, Fx.ThunkCallback(new AsyncCallback(this.EndpointNotFoundCallback)), new ChannelAndRequestAsyncState(channel, request));
                        result.Start();
                        if (!result.CompletedSynchronously)
                        {
                            doAbort = false;
                            return;
                        }
                        ReplySessionDemuxFailureAsyncResult.End(result);
                        doAbort = false;
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.SendTimeoutIsEnabled())
                        {
                            TD.SendTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (ObjectDisposedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        this.HandleUnknownException(e);
                    }
                }
            }
            finally
            {
                if (doAbort)
                {
                    this.AbortItem(request);
                    channel.Abort();
                }
            }
        }

        protected override RequestContext EndReceive(IReplySessionChannel channel, IAsyncResult result)
        {
            return channel.EndReceiveRequest(result);
        }

        protected override Message GetMessage(RequestContext request)
        {
            return request.RequestMessage;
        }

        struct ChannelAndRequestAsyncState
        {
            public IChannel channel;
            public RequestContext request;

            public ChannelAndRequestAsyncState(IChannel channel, RequestContext request)
            {
                this.channel = channel;
                this.request = request;
            }
        }
    }

    class ReplySessionChannelWrapper : ReplyChannelWrapper, IReplySessionChannel
    {
        public ReplySessionChannelWrapper(ChannelManagerBase channelManager, IReplySessionChannel innerChannel, RequestContext firstRequest)
            : base(channelManager, innerChannel, firstRequest)
        {
        }

        new IReplySessionChannel InnerChannel
        {
            get { return (IReplySessionChannel)base.InnerChannel; }
        }

        public IInputSession Session
        {
            get { return this.InnerChannel.Session; }
        }
    }

    abstract class ChannelWrapper<TChannel, TItem> : LayeredChannel<TChannel>
        where TChannel : class, IChannel
        where TItem : class, IDisposable
    {
        TItem firstItem;

        public ChannelWrapper(ChannelManagerBase channelManager, TChannel innerChannel, TItem firstItem)
            : base(channelManager, innerChannel)
        {
            this.firstItem = firstItem;
        }

        protected abstract void CloseFirstItem(TimeSpan timeout);

        protected TItem GetFirstItem()
        {
            return Interlocked.Exchange<TItem>(ref this.firstItem, null);
        }

        protected bool HaveFirstItem()
        {
            return (this.firstItem != null);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.CloseFirstItem(TimeSpan.Zero);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.CloseFirstItem(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.CloseFirstItem(timeoutHelper.RemainingTime());
            return base.OnBeginClose(timeoutHelper.RemainingTime(), callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        protected class ReceiveAsyncResult : AsyncResult
        {
            TItem item;

            public ReceiveAsyncResult(TItem item, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.item = item;
                this.Complete(true);
            }

            public static TItem End(IAsyncResult result)
            {
                ReceiveAsyncResult receiveResult = AsyncResult.End<ReceiveAsyncResult>(result);
                return receiveResult.item;
            }
        }

        protected class WaitAsyncResult : AsyncResult
        {
            public WaitAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.Complete(true);
            }

            public static bool End(IAsyncResult result)
            {
                WaitAsyncResult waitResult = AsyncResult.End<WaitAsyncResult>(result);
                return true;
            }
        }
    }

    class InputChannelWrapper : ChannelWrapper<IInputChannel, Message>, IInputChannel
    {
        public InputChannelWrapper(ChannelManagerBase channelManager, IInputChannel innerChannel, Message firstMessage)
            : base(channelManager, innerChannel, firstMessage)
        {
        }

        public EndpointAddress LocalAddress
        {
            get { return this.InnerChannel.LocalAddress; }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void CloseFirstItem(TimeSpan timeout)
        {
            Message message = this.GetFirstItem();
            if (message != null)
            {
                TypedChannelDemuxer.AbortMessage(message);
            }
        }

        public Message Receive()
        {
            Message message = this.GetFirstItem();
            if (message != null)
                return message;
            return this.InnerChannel.Receive();
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = this.GetFirstItem();
            if (message != null)
                return message;
            return this.InnerChannel.Receive(timeout);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            Message message = this.GetFirstItem();
            if (message != null)
                return new ReceiveAsyncResult(message, callback, state);
            return this.InnerChannel.BeginReceive(callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Message message = this.GetFirstItem();
            if (message != null)
                return new ReceiveAsyncResult(message, callback, state);
            return this.InnerChannel.BeginReceive(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            if (result is ReceiveAsyncResult)
                return ReceiveAsyncResult.End(result);
            return this.InnerChannel.EndReceive(result);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = this.GetFirstItem();
            if (message != null)
                return true;
            return this.InnerChannel.TryReceive(timeout, out message);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Message message = this.GetFirstItem();
            if (message != null)
                return new ReceiveAsyncResult(message, callback, state);
            return this.InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (result is ReceiveAsyncResult)
            {
                message = ReceiveAsyncResult.End(result);
                return true;
            }
            return this.InnerChannel.EndTryReceive(result, out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            if (this.HaveFirstItem())
                return true;
            return this.InnerChannel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.HaveFirstItem())
                return new WaitAsyncResult(callback, state);
            return this.InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            if (result is WaitAsyncResult)
                return WaitAsyncResult.End(result);
            return this.InnerChannel.EndWaitForMessage(result);
        }
    }

    class ReplyChannelWrapper : ChannelWrapper<IReplyChannel, RequestContext>, IReplyChannel
    {
        public ReplyChannelWrapper(ChannelManagerBase channelManager, IReplyChannel innerChannel, RequestContext firstRequest)
            : base(channelManager, innerChannel, firstRequest)
        {
        }

        public EndpointAddress LocalAddress
        {
            get { return this.InnerChannel.LocalAddress; }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void CloseFirstItem(TimeSpan timeout)
        {
            RequestContext request = this.GetFirstItem();
            if (request != null)
            {
                try
                {
                    request.RequestMessage.Close();
                    request.Close(timeout);
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    if (TD.CloseTimeoutIsEnabled())
                    {
                        TD.CloseTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
            }
        }

        public RequestContext ReceiveRequest()
        {
            RequestContext request = this.GetFirstItem();
            if (request != null)
                return request;
            return this.InnerChannel.ReceiveRequest();
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            RequestContext request = this.GetFirstItem();
            if (request != null)
                return request;
            return this.InnerChannel.ReceiveRequest(timeout);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            RequestContext request = this.GetFirstItem();
            if (request != null)
                return new ReceiveAsyncResult(request, callback, state);
            return this.InnerChannel.BeginReceiveRequest(callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            RequestContext request = this.GetFirstItem();
            if (request != null)
                return new ReceiveAsyncResult(request, callback, state);
            return this.InnerChannel.BeginReceiveRequest(timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            if (result is ReceiveAsyncResult)
                return ReceiveAsyncResult.End(result);
            return this.InnerChannel.EndReceiveRequest(result);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext request)
        {
            request = this.GetFirstItem();
            if (request != null)
                return true;
            return this.InnerChannel.TryReceiveRequest(timeout, out request);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            RequestContext request = this.GetFirstItem();
            if (request != null)
                return new ReceiveAsyncResult(request, callback, state);
            return this.InnerChannel.BeginTryReceiveRequest(timeout, callback, state);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext request)
        {
            if (result is ReceiveAsyncResult)
            {
                request = ReceiveAsyncResult.End(result);
                return true;
            }
            return this.InnerChannel.EndTryReceiveRequest(result, out request);
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            if (this.HaveFirstItem())
                return true;
            return this.InnerChannel.WaitForRequest(timeout);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.HaveFirstItem())
                return new WaitAsyncResult(callback, state);
            return this.InnerChannel.BeginWaitForRequest(timeout, callback, state);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            if (result is WaitAsyncResult)
                return WaitAsyncResult.End(result);
            return this.InnerChannel.EndWaitForRequest(result);
        }
    }

    class InputQueueChannelListener<TChannel> : DelegatingChannelListener<TChannel>
        where TChannel : class, IChannel
    {
        ChannelDemuxerFilter filter;
        IChannelDemuxer channelDemuxer;

        public InputQueueChannelListener(ChannelDemuxerFilter filter, IChannelDemuxer channelDemuxer)
            : base(true)
        {
            this.filter = filter;
            this.channelDemuxer = channelDemuxer;
            this.Acceptor = new InputQueueChannelAcceptor<TChannel>(this);
        }

        public ChannelDemuxerFilter Filter
        {
            get { return this.filter; }
        }

        public InputQueueChannelAcceptor<TChannel> InputQueueAcceptor
        {
            get { return (InputQueueChannelAcceptor<TChannel>)base.Acceptor; }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.channelDemuxer.OnOuterListenerOpen(this.filter, this, timeoutHelper.RemainingTime());
            base.OnOpen(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, this.OnBeginOuterListenerOpen, this.OnEndOuterListenerOpen, base.OnBeginOpen, base.OnEndOpen);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        IAsyncResult OnBeginOuterListenerOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelDemuxer.OnBeginOuterListenerOpen(this.filter, this, timeout, callback, state);
        }

        void OnEndOuterListenerOpen(IAsyncResult result)
        {
            this.channelDemuxer.OnEndOuterListenerOpen(result);
        }

        protected override void OnAbort()
        {
            this.channelDemuxer.OnOuterListenerAbort(this.filter);
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.channelDemuxer.OnOuterListenerClose(this.filter, timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, this.OnBeginOuterListenerClose, this.OnEndOuterListenerClose, base.OnBeginClose, base.OnEndClose);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        IAsyncResult OnBeginOuterListenerClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelDemuxer.OnBeginOuterListenerClose(this.filter, timeout, callback, state);
        }

        void OnEndOuterListenerClose(IAsyncResult result)
        {
            this.channelDemuxer.OnEndOuterListenerClose(result);
        }
    }

    //
    // Binding element
    //

    class ChannelDemuxerBindingElement : BindingElement
    {
        ChannelDemuxer demuxer;
        CachedBindingContextState cachedContextState;
        bool cacheContextState;

        public ChannelDemuxerBindingElement(bool cacheContextState)
        {
            this.cacheContextState = cacheContextState;
            if (cacheContextState)
            {
                this.cachedContextState = new CachedBindingContextState();
            }
            this.demuxer = new ChannelDemuxer();
        }

        public ChannelDemuxerBindingElement(ChannelDemuxerBindingElement element)
        {
            this.demuxer = element.demuxer;
            this.cacheContextState = element.cacheContextState;
            this.cachedContextState = element.cachedContextState;
        }

        public TimeSpan PeekTimeout
        {
            get
            {
                return this.demuxer.PeekTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero && value != ChannelDemuxer.UseDefaultReceiveTimeout)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.demuxer.PeekTimeout = value;
            }
        }

        public int MaxPendingSessions
        {
            get
            {
                return this.demuxer.MaxPendingSessions;
            }
            set
            {
                if (value < 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(SR.GetString(SR.ValueMustBeGreaterThanZero)));
                }

                this.demuxer.MaxPendingSessions = value;
            }
        }

        void SubstituteCachedBindingContextParametersIfNeeded(BindingContext context)
        {
            if (!this.cacheContextState)
            {
                return;
            }
            if (!this.cachedContextState.IsStateCached)
            {
                foreach (object parameter in context.BindingParameters)
                {
                    this.cachedContextState.CachedBindingParameters.Add(parameter);
                }
                this.cachedContextState.IsStateCached = true;
            }
            else
            {
                context.BindingParameters.Clear();
                foreach (object parameter in this.cachedContextState.CachedBindingParameters)
                {
                    context.BindingParameters.Add(parameter);
                }
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            SubstituteCachedBindingContextParametersIfNeeded(context);
            return context.BuildInnerChannelFactory<TChannel>();
        }


        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            ChannelDemuxerFilter filter = context.BindingParameters.Remove<ChannelDemuxerFilter>();
            SubstituteCachedBindingContextParametersIfNeeded(context);
            if (filter == null)
                return demuxer.BuildChannelListener<TChannel>(context);
            else
                return demuxer.BuildChannelListener<TChannel>(context, filter);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new ChannelDemuxerBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            // augment the context with cached binding parameters
            if (this.cacheContextState && this.cachedContextState.IsStateCached)
            {
                for (int i = 0; i < this.cachedContextState.CachedBindingParameters.Count; ++i)
                {
                    if (!context.BindingParameters.Contains(this.cachedContextState.CachedBindingParameters[i].GetType()))
                    {
                        context.BindingParameters.Add(this.cachedContextState.CachedBindingParameters[i]);
                    }
                }
            }
            return context.GetInnerProperty<T>();
        }

        class CachedBindingContextState
        {
            public bool IsStateCached;
            public BindingParameterCollection CachedBindingParameters;

            public CachedBindingContextState()
            {
                CachedBindingParameters = new BindingParameterCollection();
            }
        }
    }

    //
    // Demuxer filter
    //

    class ChannelDemuxerFilter
    {
        MessageFilter filter;
        int priority;

        public ChannelDemuxerFilter(MessageFilter filter, int priority)
        {
            this.filter = filter;
            this.priority = priority;
        }

        public MessageFilter Filter
        {
            get { return this.filter; }
        }

        public int Priority
        {
            get { return this.priority; }
        }
    }

    class ReplyChannelDemuxFailureAsyncResult : AsyncResult
    {
        static AsyncCallback demuxFailureHandlerCallback = Fx.ThunkCallback(new AsyncCallback(DemuxFailureHandlerCallback));
        IChannelDemuxFailureHandler demuxFailureHandler;
        RequestContext requestContext;

        public ReplyChannelDemuxFailureAsyncResult(IChannelDemuxFailureHandler demuxFailureHandler, RequestContext requestContext, AsyncCallback callback, object state)
            : base(callback, state)
        {
            if (demuxFailureHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("demuxFailureHandler");
            }
            if (requestContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestContext");
            }
            this.demuxFailureHandler = demuxFailureHandler;
            this.requestContext = requestContext;
        }

        public void Start()
        {
            IAsyncResult result = this.demuxFailureHandler.BeginHandleDemuxFailure(requestContext.RequestMessage, requestContext, demuxFailureHandlerCallback, this);
            if (!result.CompletedSynchronously)
            {
                return;
            }
            this.demuxFailureHandler.EndHandleDemuxFailure(result);
            if (this.OnDemuxFailureHandled())
            {
                Complete(true);
            }
        }

        protected virtual bool OnDemuxFailureHandled()
        {
            requestContext.Close();
            return true;
        }

        static void DemuxFailureHandlerCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            ReplyChannelDemuxFailureAsyncResult self = (ReplyChannelDemuxFailureAsyncResult)(result.AsyncState);
            bool completeSelf = false;
            Exception completionException = null;
            try
            {
                self.demuxFailureHandler.EndHandleDemuxFailure(result);
                completeSelf = self.OnDemuxFailureHandled();
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                completeSelf = true;
                completionException = e;
            }
            if (completeSelf)
            {
                self.Complete(false, completionException);
            }
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ReplyChannelDemuxFailureAsyncResult>(result);
        }
    }

    class ReplySessionDemuxFailureAsyncResult : ReplyChannelDemuxFailureAsyncResult
    {
        static AsyncCallback closeChannelCallback = Fx.ThunkCallback(new AsyncCallback(ChannelCloseCallback));
        IReplySessionChannel channel;

        public ReplySessionDemuxFailureAsyncResult(IChannelDemuxFailureHandler demuxFailureHandler, RequestContext requestContext, IReplySessionChannel channel, AsyncCallback callback, object state)
            : base(demuxFailureHandler, requestContext, callback, state)
        {
            this.channel = channel;
        }

        protected override bool OnDemuxFailureHandled()
        {
            base.OnDemuxFailureHandled();
            IAsyncResult result = this.channel.BeginClose(closeChannelCallback, this);
            if (!result.CompletedSynchronously)
            {
                return false;
            }
            this.channel.EndClose(result);
            return true;
        }

        static void ChannelCloseCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            ReplySessionDemuxFailureAsyncResult self = (ReplySessionDemuxFailureAsyncResult)result.AsyncState;
            Exception completionException = null;
            try
            {
                self.channel.EndClose(result);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                completionException = e;
            }
            self.Complete(false, completionException);
        }

        public static new void End(IAsyncResult result)
        {
            AsyncResult.End<ReplySessionDemuxFailureAsyncResult>(result);
        }
    }

    class DuplexSessionDemuxFailureAsyncResult : AsyncResult
    {
        static AsyncCallback demuxFailureHandlerCallback = Fx.ThunkCallback(new AsyncCallback(DemuxFailureHandlerCallback));
        static AsyncCallback channelCloseCallback = Fx.ThunkCallback(new AsyncCallback(ChannelCloseCallback));
        IChannelDemuxFailureHandler demuxFailureHandler;
        IDuplexSessionChannel channel;
        Message message;

        public DuplexSessionDemuxFailureAsyncResult(IChannelDemuxFailureHandler demuxFailureHandler, IDuplexSessionChannel channel, Message message, AsyncCallback callback, object state)
            : base(callback, state)
        {
            if (demuxFailureHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("demuxFailureHandler");
            }
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }
            this.demuxFailureHandler = demuxFailureHandler;
            this.channel = channel;
            this.message = message;
        }

        public void Start()
        {
            IAsyncResult result = this.demuxFailureHandler.BeginHandleDemuxFailure(this.message, this.channel, demuxFailureHandlerCallback, this);
            if (!result.CompletedSynchronously)
            {
                return;
            }
            this.demuxFailureHandler.EndHandleDemuxFailure(result);
            if (this.OnDemuxFailureHandled())
            {
                Complete(true);
            }
        }

        bool OnDemuxFailureHandled()
        {
            IAsyncResult result = this.channel.BeginClose(channelCloseCallback, this);
            if (!result.CompletedSynchronously)
            {
                return false;
            }
            this.channel.EndClose(result);
            this.message.Close();
            return true;
        }

        static void DemuxFailureHandlerCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            DuplexSessionDemuxFailureAsyncResult self = (DuplexSessionDemuxFailureAsyncResult)result.AsyncState;
            bool completeSelf = false;
            Exception completionException = null;
            try
            {
                self.demuxFailureHandler.EndHandleDemuxFailure(result);
                completeSelf = self.OnDemuxFailureHandled();
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                completeSelf = true;
                completionException = e;
            }
            if (completeSelf)
            {
                self.Complete(false, completionException);
            }
        }

        static void ChannelCloseCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            DuplexSessionDemuxFailureAsyncResult self = (DuplexSessionDemuxFailureAsyncResult)result.AsyncState;
            Exception completionException = null;
            try
            {
                self.channel.EndClose(result);
                self.message.Close();
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                completionException = e;
            }
            self.Complete(false, completionException);
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<DuplexSessionDemuxFailureAsyncResult>(result);
        }
    }

    interface IChannelDemuxFailureHandler
    {
        void HandleDemuxFailure(Message message);

        IAsyncResult BeginHandleDemuxFailure(Message message, RequestContext faultContext, AsyncCallback callback, object state);
        IAsyncResult BeginHandleDemuxFailure(Message message, IOutputChannel faultContext, AsyncCallback callback, object state);
        void EndHandleDemuxFailure(IAsyncResult result);
    }
}
