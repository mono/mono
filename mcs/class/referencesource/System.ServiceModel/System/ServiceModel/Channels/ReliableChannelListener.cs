//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    // This class tracks the lifetime of the InnerChannelListener (ICL). The ICL must be kept open
    // as long as some communication object uses it. ReliableChannelListener (RCL) and all the
    // channels it produces use it. The RCL + the channel count forms a ref count. If the ref
    // count is 1, the object that wishes to close (since it is the last object to release the
    // reference) must also close the ICL. If the ref count is 0 any object may abort the ICL. This
    // means the last closing object may not release its reference until after the ICL's close.
    abstract class ReliableChannelListenerBase<TChannel>
        : DelegatingChannelListener<TChannel>, IReliableFactorySettings
        where TChannel : class, IChannel
    {
        TimeSpan acknowledgementInterval;
        bool closed = false;

        FaultHelper faultHelper;
        bool flowControlEnabled;
        TimeSpan inactivityTimeout;
        IMessageFilterTable<EndpointAddress> localAddresses;
        int maxPendingChannels;
        int maxRetryCount;
        int maxTransferWindowSize;
        MessageVersion messageVersion;
        bool ordered;
        ReliableMessagingVersion reliableMessagingVersion;

        protected ReliableChannelListenerBase(ReliableSessionBindingElement settings, Binding binding)
            : base(true, binding)
        {
            this.acknowledgementInterval = settings.AcknowledgementInterval;
            this.flowControlEnabled = settings.FlowControlEnabled;
            this.inactivityTimeout = settings.InactivityTimeout;
            this.maxPendingChannels = settings.MaxPendingChannels;
            this.maxRetryCount = settings.MaxRetryCount;
            this.maxTransferWindowSize = settings.MaxTransferWindowSize;
            this.messageVersion = binding.MessageVersion;
            this.ordered = settings.Ordered;
            this.reliableMessagingVersion = settings.ReliableMessagingVersion;
        }

        public TimeSpan AcknowledgementInterval
        {
            get { return this.acknowledgementInterval; }
        }

        protected FaultHelper FaultHelper
        {
            get { return this.faultHelper; }
            set { this.faultHelper = value; }
        }

        public bool FlowControlEnabled
        {
            get { return this.flowControlEnabled; }
        }

        public TimeSpan InactivityTimeout
        {
            get { return this.inactivityTimeout; }
        }

        // Must call under lock.
        protected bool IsAccepting
        {
            get { return this.State == CommunicationState.Opened; }
        }

        public IMessageFilterTable<EndpointAddress> LocalAddresses
        {
            get { return this.localAddresses; }
            set { this.localAddresses = value; }
        }

        public int MaxPendingChannels
        {
            get { return this.maxPendingChannels; }
        }

        public int MaxRetryCount
        {
            get { return this.maxRetryCount; }
        }

        public int MaxTransferWindowSize
        {
            get { return this.maxTransferWindowSize; }
        }

        public MessageVersion MessageVersion
        {
            get { return this.messageVersion; }
        }

        public bool Ordered
        {
            get { return this.ordered; }
        }

        public ReliableMessagingVersion ReliableMessagingVersion
        {
            get { return this.reliableMessagingVersion; }
        }

        public TimeSpan SendTimeout
        {
            get { return this.InternalSendTimeout; }
        }

        protected abstract bool Duplex
        {
            get;
        }

        // Must call under lock.
        protected abstract bool HasChannels();

        // Must call under lock. Must call after the ReliableChannelListener has been opened.
        protected abstract bool IsLastChannel(UniqueId inputId);

        protected override void OnAbort()
        {
            bool abortInnerChannelListener;

            lock (this.ThisLock)
            {
                this.closed = true;
                abortInnerChannelListener = !this.HasChannels();
            }

            if (abortInnerChannelListener)
            {
                this.AbortInnerListener();
            }

            base.OnAbort();
        }

        protected virtual void AbortInnerListener()
        {
            this.faultHelper.Abort();
            this.InnerChannelListener.Abort();
        }

        protected virtual void CloseInnerListener(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.faultHelper.Close(timeoutHelper.RemainingTime());
            this.InnerChannelListener.Close(timeoutHelper.RemainingTime());
        }

        protected virtual IAsyncResult BeginCloseInnerListener(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] {
                new OperationWithTimeoutBeginCallback(this.faultHelper.BeginClose),
                new OperationWithTimeoutBeginCallback(this.InnerChannelListener.BeginClose) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] {
                new OperationEndCallback(this.faultHelper.EndClose),
                new OperationEndCallback(this.InnerChannelListener.EndClose) };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations,
                callback, state);
        }

        protected virtual void EndCloseInnerListener(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.ShouldCloseOnChannelListenerClose())
            {
                this.CloseInnerListener(timeoutHelper.RemainingTime());
                this.closed = true;
            }

            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return new CloseAsyncResult(this, base.OnBeginClose, base.OnEndClose, timeout,
                callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            this.InnerChannelListener.Open(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(
                timeout,
                new OperationWithTimeoutBeginCallback[] 
                {
                    new OperationWithTimeoutBeginCallback(base.OnBeginOpen),
                    new OperationWithTimeoutBeginCallback(this.InnerChannelListener.BeginOpen) 
                },
                new OperationEndCallback[] 
                {
                    new OperationEndCallback(base.OnEndOpen),
                    new OperationEndCallback(this.InnerChannelListener.EndOpen)
                },
                callback, 
                state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        public void OnReliableChannelAbort(UniqueId inputId, UniqueId outputId)
        {
            lock (this.ThisLock)
            {
                this.RemoveChannel(inputId, outputId);

                if (!this.closed || this.HasChannels())
                {
                    return;
                }
            }

            this.AbortInnerListener();
        }

        public void OnReliableChannelClose(UniqueId inputId, UniqueId outputId,
            TimeSpan timeout)
        {
            if (this.ShouldCloseOnReliableChannelClose(inputId, outputId))
            {
                this.CloseInnerListener(timeout);

                lock (this.ThisLock)
                {
                    this.RemoveChannel(inputId, outputId);
                }
            }
        }

        public IAsyncResult OnReliableChannelBeginClose(UniqueId inputId,
            UniqueId outputId, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OnReliableChannelCloseAsyncResult(this, inputId, outputId, timeout,
                callback, state);
        }

        public void OnReliableChannelEndClose(IAsyncResult result)
        {
            OnReliableChannelCloseAsyncResult.End(result);
        }

        // Must call under lock.
        protected abstract void RemoveChannel(UniqueId inputId, UniqueId outputId);

        bool ShouldCloseOnChannelListenerClose()
        {
            lock (this.ThisLock)
            {
                if (!this.HasChannels())
                {
                    return true;
                }
                else
                {
                    this.closed = true;
                    return false;
                }
            }
        }

        bool ShouldCloseOnReliableChannelClose(UniqueId inputId, UniqueId outputId)
        {
            lock (this.ThisLock)
            {
                if (this.closed && this.IsLastChannel(inputId))
                {
                    return true;
                }
                else
                {
                    this.RemoveChannel(inputId, outputId);
                    return false;
                }
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            OperationWithTimeoutBeginCallback baseBeginClose;
            OperationEndCallback baseEndClose;
            ReliableChannelListenerBase<TChannel> parent;
            TimeoutHelper timeoutHelper;

            static AsyncCallback onBaseChannelListenerCloseComplete =
                Fx.ThunkCallback(OnBaseChannelListenerCloseCompleteStatic);
            static AsyncCallback onInnerChannelListenerCloseComplete =
                Fx.ThunkCallback(OnInnerChannelListenerCloseCompleteStatic);

            public CloseAsyncResult(ReliableChannelListenerBase<TChannel> parent,
                OperationWithTimeoutBeginCallback baseBeginClose,
                OperationEndCallback baseEndClose, TimeSpan timeout, AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.baseBeginClose = baseBeginClose;
                this.baseEndClose = baseEndClose;

                bool complete = false;

                if (this.parent.ShouldCloseOnChannelListenerClose())
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);

                    IAsyncResult result = this.parent.BeginCloseInnerListener(
                        timeoutHelper.RemainingTime(), onInnerChannelListenerCloseComplete, this);

                    if (result.CompletedSynchronously)
                    {
                        complete = this.CompleteInnerChannelListenerClose(result);
                    }
                }
                else
                {
                    complete = this.CloseBaseChannelListener(timeout);
                }

                if (complete)
                {
                    this.Complete(true);
                }
            }

            bool CloseBaseChannelListener(TimeSpan timeout)
            {
                IAsyncResult result = this.baseBeginClose(timeout,
                    onBaseChannelListenerCloseComplete, this);

                if (result.CompletedSynchronously)
                {
                    this.baseEndClose(result);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            bool CompleteInnerChannelListenerClose(IAsyncResult result)
            {
                this.parent.EndCloseInnerListener(result);
                this.parent.closed = true;
                this.parent.faultHelper.Abort();
                return this.CloseBaseChannelListener(this.timeoutHelper.RemainingTime());
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            void OnBaseChannelListenerCloseComplete(IAsyncResult result)
            {
                Exception completeException = null;

                try
                {
                    this.baseEndClose(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeException = e;
                }

                this.Complete(false, completeException);
            }

            static void OnBaseChannelListenerCloseCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    CloseAsyncResult closeResult = (CloseAsyncResult)result.AsyncState;
                    closeResult.OnBaseChannelListenerCloseComplete(result);
                }
            }

            void OnInnerChannelListenerCloseComplete(IAsyncResult result)
            {
                bool complete;
                Exception completeException = null;

                try
                {
                    complete = this.CompleteInnerChannelListenerClose(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    complete = true;
                    completeException = e;
                }

                if (complete)
                {
                    this.Complete(false, completeException);
                }
            }

            static void OnInnerChannelListenerCloseCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    CloseAsyncResult closeResult = (CloseAsyncResult)result.AsyncState;
                    closeResult.OnInnerChannelListenerCloseComplete(result);
                }
            }
        }

        class OnReliableChannelCloseAsyncResult : AsyncResult
        {
            ReliableChannelListenerBase<TChannel> channelListener;
            UniqueId inputId;
            UniqueId outputId;

            static AsyncCallback onInnerChannelListenerCloseComplete =
                Fx.ThunkCallback(new AsyncCallback(OnInnerChannelListenerCloseCompleteStatic));

            public OnReliableChannelCloseAsyncResult(
                ReliableChannelListenerBase<TChannel> channelListener, UniqueId inputId,
                UniqueId outputId, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                if (!channelListener.ShouldCloseOnReliableChannelClose(inputId, outputId))
                {
                    this.Complete(true);
                    return;
                }

                this.channelListener = channelListener;
                this.inputId = inputId;
                this.outputId = outputId;

                IAsyncResult result = this.channelListener.BeginCloseInnerListener(timeout,
                    onInnerChannelListenerCloseComplete, this);

                if (result.CompletedSynchronously)
                {
                    this.CompleteInnerChannelListenerClose(result);
                    this.Complete(true);
                }
            }

            void CompleteInnerChannelListenerClose(IAsyncResult result)
            {
                this.channelListener.EndCloseInnerListener(result);


                lock (this.channelListener.ThisLock)
                {
                    this.channelListener.RemoveChannel(this.inputId, this.outputId);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OnReliableChannelCloseAsyncResult>(result);
            }

            void OnInnerChannelListenerCloseComplete(IAsyncResult result)
            {
                Exception completeException = null;

                try
                {
                    this.CompleteInnerChannelListenerClose(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeException = e;
                }

                this.Complete(false, completeException);
            }

            static void OnInnerChannelListenerCloseCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    OnReliableChannelCloseAsyncResult closeResult =
                        (OnReliableChannelCloseAsyncResult)result.AsyncState;

                    closeResult.OnInnerChannelListenerCloseComplete(result);
                }
            }
        }
    }

    abstract class ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>
        : ReliableChannelListenerBase<TChannel>
        where TChannel : class, IChannel
        where TReliableChannel : class, IChannel
        where TInnerChannel : class, IChannel
    {
        Dictionary<UniqueId, TReliableChannel> channelsByInput;
        Dictionary<UniqueId, TReliableChannel> channelsByOutput;
        InputQueueChannelAcceptor<TChannel> inputQueueChannelAcceptor;
        static AsyncCallback onAcceptCompleted = Fx.ThunkCallback(new AsyncCallback(OnAcceptCompletedStatic));
        IChannelListener<TInnerChannel> typedListener;

        protected ReliableChannelListener(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context.Binding)
        {
            this.typedListener = context.BuildInnerChannelListener<TInnerChannel>();
            this.inputQueueChannelAcceptor = new InputQueueChannelAcceptor<TChannel>(this);
            this.Acceptor = this.inputQueueChannelAcceptor;
        }

        internal override IChannelListener InnerChannelListener
        {
            get
            {
                return this.typedListener;
            }
            set
            {
                // until the public setter is removed, throw
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
        }

        IServerReliableChannelBinder CreateBinder(TInnerChannel channel, EndpointAddress localAddress, EndpointAddress remoteAddress)
        {
            return ServerReliableChannelBinder<TInnerChannel>.CreateBinder(channel, localAddress,
                remoteAddress, TolerateFaultsMode.IfNotSecuritySession, this.DefaultCloseTimeout,
                this.DefaultSendTimeout);
        }

        protected abstract TReliableChannel CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder);

        protected void Dispatch()
        {
            this.inputQueueChannelAcceptor.Dispatch();
        }

        // override to hook up events, etc pre-Open
        protected virtual void OnInnerChannelAccepted(TInnerChannel channel)
        {
        }

        protected bool EnqueueWithoutDispatch(TChannel channel)
        {
            return this.inputQueueChannelAcceptor.EnqueueWithoutDispatch(channel, null);
        }

        protected TReliableChannel GetChannel(WsrmMessageInfo info, out UniqueId id)
        {
            id = WsrmUtilities.GetInputId(info);

            lock (this.ThisLock)
            {
                TReliableChannel channel = null;
                if ((id == null) || !this.channelsByInput.TryGetValue(id, out channel))
                {
                    if (this.Duplex)
                    {
                        UniqueId outputId = WsrmUtilities.GetOutputId(this.ReliableMessagingVersion, info);
                        if (outputId != null)
                        {
                            id = outputId;
                            this.channelsByOutput.TryGetValue(id, out channel);
                        }
                    }
                }

                return channel;
            }
        }

        void HandleAcceptComplete(TInnerChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            try
            {
                OnInnerChannelAccepted(channel);
                channel.Open();
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                channel.Abort();
                return;
            }

            this.ProcessChannel(channel);
        }

        protected bool HandleException(Exception e, ICommunicationObject o)
        {
            if ((e is CommunicationException || e is TimeoutException) &&
                (o.State == CommunicationState.Opened))
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);

                return true;
            }

            DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

            return false;
        }

        // Must call under lock.
        protected override bool HasChannels()
        {
            return (this.channelsByInput == null) ? false : (this.channelsByInput.Count > 0);
        }

        bool IsExpectedException(Exception e)
        {
            if (e is ProtocolException)
            {
                return false;
            }
            else
            {
                return e is CommunicationException;
            }
        }

        // Must call under lock. Must call after the ReliableChannelListener has been opened.
        protected override bool IsLastChannel(UniqueId inputId)
        {
            return (this.channelsByInput.Count == 1) ? channelsByInput.ContainsKey(inputId) : false;
        }

        void OnAcceptCompleted(IAsyncResult result)
        {
            TInnerChannel channel = null;
            Exception expectedException = null;
            Exception unexpectedException = null;

            try
            {
                channel = this.typedListener.EndAcceptChannel(result);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (this.IsExpectedException(e))
                {
                    expectedException = e;
                }
                else
                {
                    unexpectedException = e;
                }
            }

            if (channel != null)
            {
                this.HandleAcceptComplete(channel);
                this.StartAccepting();
            }
            else if (unexpectedException != null)
            {
                this.Fault(unexpectedException);
            }
            else if ((expectedException != null)
                && (this.typedListener.State == CommunicationState.Opened))
            {
                DiagnosticUtility.TraceHandledException(expectedException, TraceEventType.Warning);

                this.StartAccepting();
            }
            else if (this.typedListener.State == CommunicationState.Faulted)
            {
                this.Fault(expectedException);
            }
        }

        static void OnAcceptCompletedStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel> listener =
                    (ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>)result.AsyncState;

                try
                {
                    listener.OnAcceptCompleted(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    listener.Fault(e);
                }
            }
        }

        protected override void OnFaulted()
        {
            this.typedListener.Abort();
            this.inputQueueChannelAcceptor.FaultQueue();
            base.OnFaulted();
        }

        protected override void OnOpened()
        {
            base.OnOpened();

            this.channelsByInput = new Dictionary<UniqueId, TReliableChannel>();
            if (this.Duplex)
                this.channelsByOutput = new Dictionary<UniqueId, TReliableChannel>();

            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                try
                {
                    StartAccepting();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    this.Fault(e);
                }
            }
            else
            {
                ActionItem.Schedule(new Action<object>(StartAccepting), this);
            }
        }

        protected TReliableChannel ProcessCreateSequence(WsrmMessageInfo info, TInnerChannel channel, out bool dispatch, out bool newChannel)
        {
            dispatch = false;
            newChannel = false;

            CreateSequenceInfo createSequenceInfo = info.CreateSequenceInfo;
            EndpointAddress acksTo;

            if (!WsrmUtilities.ValidateCreateSequence<TChannel>(info, this, channel, out acksTo))
                return null;

            lock (this.ThisLock)
            {
                UniqueId id;
                TReliableChannel reliableChannel = null;

                if ((createSequenceInfo.OfferIdentifier != null)
                    && this.Duplex
                    && this.channelsByOutput.TryGetValue(createSequenceInfo.OfferIdentifier, out reliableChannel))
                {
                    return reliableChannel;
                }

                if (!this.IsAccepting)
                {
                    info.FaultReply = WsrmUtilities.CreateEndpointNotFoundFault(this.MessageVersion, SR.GetString(SR.RMEndpointNotFoundReason, this.Uri));
                    return null;
                }

                if (this.inputQueueChannelAcceptor.PendingCount >= this.MaxPendingChannels)
                {
                    info.FaultReply = WsrmUtilities.CreateCSRefusedServerTooBusyFault(this.MessageVersion, this.ReliableMessagingVersion, SR.GetString(SR.ServerTooBusy, this.Uri));
                    return null;
                }

                id = WsrmUtilities.NextSequenceId();

                reliableChannel = this.CreateChannel(id, createSequenceInfo,
                    this.CreateBinder(channel, acksTo, createSequenceInfo.ReplyTo));
                this.channelsByInput.Add(id, reliableChannel);
                if (this.Duplex)
                    this.channelsByOutput.Add(createSequenceInfo.OfferIdentifier, reliableChannel);

                dispatch = this.EnqueueWithoutDispatch((TChannel)(object)reliableChannel);
                newChannel = true;

                return reliableChannel;
            }
        }

        protected abstract void ProcessChannel(TInnerChannel channel);

        // Must call under lock.
        protected override void RemoveChannel(UniqueId inputId, UniqueId outputId)
        {
            this.channelsByInput.Remove(inputId);

            if (this.Duplex)
                this.channelsByOutput.Remove(outputId);
        }

        void StartAccepting()
        {
            Exception expectedException = null;
            Exception unexpectedException = null;

            while (this.typedListener.State == CommunicationState.Opened)
            {
                TInnerChannel channel = null;
                expectedException = null;
                unexpectedException = null;

                try
                {
                    IAsyncResult result = this.typedListener.BeginAcceptChannel(TimeSpan.MaxValue, onAcceptCompleted, this);

                    if (!result.CompletedSynchronously)
                        return;

                    channel = this.typedListener.EndAcceptChannel(result);
                    if (channel == null)
                        break;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (this.IsExpectedException(e))
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);

                        expectedException = e;
                        continue;
                    }
                    else
                    {
                        unexpectedException = e;
                        break;
                    }
                }

                this.HandleAcceptComplete(channel);
            }

            if (unexpectedException != null)
            {
                this.Fault(unexpectedException);
            }
            else if (this.typedListener.State == CommunicationState.Faulted)
            {
                this.Fault(expectedException);
            }
        }

        static void StartAccepting(object state)
        {
            ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel> channelListener =
                (ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>)state;

            try
            {
                channelListener.StartAccepting();
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                channelListener.Fault(e);
            }
        }
    }

    abstract class ReliableListenerOverDatagram<TChannel, TReliableChannel, TInnerChannel, TItem>
        : ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>
        where TChannel : class, IChannel
        where TReliableChannel : class, IChannel
        where TInnerChannel : class, IChannel
        where TItem : class, IDisposable
    {
        Action<object> asyncHandleReceiveComplete;
        AsyncCallback onTryReceiveComplete;
        ChannelTracker<TInnerChannel, object> channelTracker;

        protected ReliableListenerOverDatagram(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
            this.asyncHandleReceiveComplete = new Action<object>(this.AsyncHandleReceiveComplete);
            this.onTryReceiveComplete = Fx.ThunkCallback(new AsyncCallback(this.OnTryReceiveComplete));
            this.channelTracker = new ChannelTracker<TInnerChannel, object>();
        }

        void AsyncHandleReceiveComplete(object state)
        {
            try
            {
                IAsyncResult result = (IAsyncResult)state;
                TInnerChannel channel = (TInnerChannel)result.AsyncState;
                TItem item = null;

                try
                {
                    this.EndTryReceiveItem(channel, result, out item);
                    if (item == null)
                        return;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (!this.HandleException(e, channel))
                    {
                        channel.Abort();
                        return;
                    }
                }

                if (item != null && this.HandleReceiveComplete(item, channel))
                    StartReceiving(channel, true);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                this.Fault(e);
            }
        }

        bool BeginProcessItem(TItem item, WsrmMessageInfo info, TInnerChannel channel, out TReliableChannel reliableChannel, out bool newChannel, out bool dispatch)
        {
            dispatch = false;
            reliableChannel = null;
            newChannel = false;
            Message faultReply;

            if (info.FaultReply != null)
            {
                faultReply = info.FaultReply;
            }
            else if (info.CreateSequenceInfo == null)
            {
                UniqueId id;
                reliableChannel = this.GetChannel(info, out id);

                if (reliableChannel != null)
                    return true;

                if (id == null)
                {
                    this.DisposeItem(item);
                    return true;
                }

                faultReply = new UnknownSequenceFault(id).CreateMessage(this.MessageVersion,
                    this.ReliableMessagingVersion);
            }
            else
            {
                reliableChannel = this.ProcessCreateSequence(info, channel, out dispatch, out newChannel);

                if (reliableChannel != null)
                    return true;

                faultReply = info.FaultReply;
            }

            try
            {
                this.SendReply(faultReply, channel, item);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (!this.HandleException(e, channel))
                {
                    channel.Abort();
                    return false;
                }
            }
            finally
            {
                faultReply.Close();
                this.DisposeItem(item);
            }

            return true;
        }

        protected abstract IAsyncResult BeginTryReceiveItem(TInnerChannel channel, AsyncCallback callback, object state);
        protected abstract void DisposeItem(TItem item);
        protected abstract void EndTryReceiveItem(TInnerChannel channel, IAsyncResult result, out TItem item);

        void EndProcessItem(TItem item, WsrmMessageInfo info, TReliableChannel channel, bool dispatch)
        {
            this.ProcessSequencedItem(channel, item, info);

            if (dispatch)
                this.Dispatch();
        }

        protected abstract Message GetMessage(TItem item);

        bool HandleReceiveComplete(TItem item, TInnerChannel channel)
        {
            Message message = null;

            // Minimalist fix for MB60747: GetMessage can call RequestContext.RequestMessage which can throw.
            // If we can handle the exception then keep the receive loop going.
            try
            {
                message = this.GetMessage(item);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (!this.HandleException(e, this))
                    throw;

                item.Dispose();

                return true;
            }

            WsrmMessageInfo info = WsrmMessageInfo.Get(this.MessageVersion, this.ReliableMessagingVersion, channel,
                null, message);

            if (info.ParsingException != null)
            {
                this.DisposeItem(item);
                return true;
            }

            TReliableChannel reliableChannel;

            bool newChannel;
            bool dispatch;

            if (!this.BeginProcessItem(item, info, channel, out reliableChannel, out newChannel, out dispatch))
                return false;

            if (reliableChannel == null)
            {
                this.DisposeItem(item);
                return true;
            }

            // On the one hand the contract of HandleReceiveComplete is that it won't stop the receive loop;
            // it can block, but it will ensure the loop doesn't stall.
            // On the other hand we don't want to take on the cost of blindly jumping threads.
            // So, if we know EndProcessItem might block (dispatch || !newChannel) then we
            // try another receive. If that completes async then we know it is safe for us to block, 
            // if not then we force the receive to complete async and *make* it safe for us to block.             
            if (dispatch || !newChannel)
            {
                this.StartReceiving(channel, false);
                this.EndProcessItem(item, info, reliableChannel, dispatch);
                return false;
            }
            else
            {
                this.EndProcessItem(item, info, reliableChannel, dispatch);
                return true;
            }
        }

        void OnTryReceiveComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    TInnerChannel channel = (TInnerChannel)result.AsyncState;
                    TItem item = null;

                    try
                    {
                        this.EndTryReceiveItem(channel, result, out item);
                        if (item == null)
                            return;
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        if (!this.HandleException(e, channel))
                        {
                            channel.Abort();
                            return;
                        }
                    }

                    if (item != null && this.HandleReceiveComplete(item, channel))
                        StartReceiving(channel, true);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    this.Fault(e);
                }
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, this.channelTracker.BeginOpen, this.channelTracker.EndOpen,
                base.OnBeginOpen, base.OnEndOpen);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.channelTracker.Open(timeoutHelper.RemainingTime());
            base.OnOpen(timeoutHelper.RemainingTime());
        }

        protected override void OnInnerChannelAccepted(TInnerChannel channel)
        {
            base.OnInnerChannelAccepted(channel);
            this.channelTracker.PrepareChannel(channel);
        }

        protected override void ProcessChannel(TInnerChannel channel)
        {
            try
            {
                this.channelTracker.Add(channel, null);
                this.StartReceiving(channel, false);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                this.Fault(e);
            }
        }

        protected override void AbortInnerListener()
        {
            base.AbortInnerListener();
            this.channelTracker.Abort();
        }

        protected override void CloseInnerListener(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.CloseInnerListener(timeoutHelper.RemainingTime());
            this.channelTracker.Close(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult BeginCloseInnerListener(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, base.BeginCloseInnerListener, base.EndCloseInnerListener,
                channelTracker.BeginClose, channelTracker.EndClose);
        }

        protected override void EndCloseInnerListener(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected abstract void ProcessSequencedItem(TReliableChannel reliableChannel, TItem item, WsrmMessageInfo info);
        protected abstract void SendReply(Message reply, TInnerChannel channel, TItem item);

        void StartReceiving(TInnerChannel channel, bool canBlock)
        {
            while (true)
            {
                TItem item = null;

                try
                {
                    IAsyncResult result = this.BeginTryReceiveItem(channel, this.onTryReceiveComplete, channel);
                    if (!result.CompletedSynchronously)
                        break;

                    if (!canBlock)
                    {
                        ActionItem.Schedule(this.asyncHandleReceiveComplete, result);
                        break;
                    }

                    this.EndTryReceiveItem(channel, result, out item);

                    if (item == null)
                        break;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (!this.HandleException(e, channel))
                    {
                        channel.Abort();
                        break;
                    }
                }

                if (item != null && !this.HandleReceiveComplete(item, channel))
                    break;
            }
        }
    }

    abstract class ReliableListenerOverDuplex<TChannel, TReliableChannel> :
        ReliableListenerOverDatagram<TChannel, TReliableChannel, IDuplexChannel, Message>
        where TChannel : class, IChannel
        where TReliableChannel : class, IChannel
    {
        protected ReliableListenerOverDuplex(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
            this.FaultHelper = new SendFaultHelper(context.Binding.SendTimeout, context.Binding.CloseTimeout);
        }

        protected override IAsyncResult BeginTryReceiveItem(IDuplexChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginTryReceive(TimeSpan.MaxValue, callback, state);
        }

        protected override void DisposeItem(Message item)
        {
            ((IDisposable)item).Dispose();
        }

        protected override void EndTryReceiveItem(IDuplexChannel channel, IAsyncResult result, out Message item)
        {
            channel.EndTryReceive(result, out item);
        }

        protected override Message GetMessage(Message item)
        {
            return item;
        }

        protected override void SendReply(Message reply, IDuplexChannel channel, Message item)
        {
            if (FaultHelper.AddressReply(item, reply))
                channel.Send(reply);
        }
    }

    abstract class ReliableListenerOverReply<TChannel, TReliableChannel>
        : ReliableListenerOverDatagram<TChannel, TReliableChannel, IReplyChannel, RequestContext>
        where TChannel : class, IChannel
        where TReliableChannel : class, IChannel
    {
        protected ReliableListenerOverReply(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
            this.FaultHelper = new ReplyFaultHelper(context.Binding.SendTimeout, context.Binding.CloseTimeout);
        }

        protected override IAsyncResult BeginTryReceiveItem(IReplyChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginTryReceiveRequest(TimeSpan.MaxValue, callback, state);
        }

        protected override void DisposeItem(RequestContext item)
        {
            ((IDisposable)item.RequestMessage).Dispose();
            ((IDisposable)item).Dispose();
        }

        protected override void EndTryReceiveItem(IReplyChannel channel, IAsyncResult result, out RequestContext item)
        {
            channel.EndTryReceiveRequest(result, out item);
        }

        protected override Message GetMessage(RequestContext item)
        {
            return item.RequestMessage;
        }

        protected override void SendReply(Message reply, IReplyChannel channel, RequestContext item)
        {
            if (FaultHelper.AddressReply(item.RequestMessage, reply))
                item.Reply(reply);
        }
    }

    abstract class ReliableListenerOverSession<TChannel, TReliableChannel, TInnerChannel, TInnerSession, TItem>
        : ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>
        where TChannel : class, IChannel
        where TReliableChannel : class, IChannel
        where TInnerChannel : class, IChannel, ISessionChannel<TInnerSession>
        where TInnerSession : ISession
        where TItem : IDisposable
    {
        Action<object> asyncHandleReceiveComplete;
        AsyncCallback onReceiveComplete;

        protected ReliableListenerOverSession(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
            this.asyncHandleReceiveComplete = new Action<object>(this.AsyncHandleReceiveComplete);
            this.onReceiveComplete = Fx.ThunkCallback(new AsyncCallback(this.OnReceiveComplete));
        }

        void AsyncHandleReceiveComplete(object state)
        {
            try
            {
                IAsyncResult result = (IAsyncResult)state;
                TInnerChannel channel = (TInnerChannel)result.AsyncState;
                TItem item = default(TItem);

                try
                {
                    this.EndTryReceiveItem(channel, result, out item);
                    if (item == null)
                    {
                        channel.Close();
                        return;
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (!this.HandleException(e, channel))
                    {
                        channel.Abort();
                        return;
                    }
                }

                if (item != null)
                    this.HandleReceiveComplete(item, channel);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                this.Fault(e);
            }
        }

        protected abstract IAsyncResult BeginTryReceiveItem(TInnerChannel channel, AsyncCallback callback, object state);
        protected abstract void DisposeItem(TItem item);
        protected abstract void EndTryReceiveItem(TInnerChannel channel, IAsyncResult result, out TItem item);
        protected abstract Message GetMessage(TItem item);

        void HandleReceiveComplete(TItem item, TInnerChannel channel)
        {
            WsrmMessageInfo info = WsrmMessageInfo.Get(this.MessageVersion, this.ReliableMessagingVersion, channel,
                channel.Session as ISecureConversationSession, this.GetMessage(item));

            if (info.ParsingException != null)
            {
                this.DisposeItem(item);
                channel.Abort();
                return;
            }

            TReliableChannel reliableChannel = null;
            bool dispatch = false;
            bool newChannel = false;

            Message faultReply = null;
            if (info.FaultReply != null)
            {
                faultReply = info.FaultReply;
            }
            else if (info.CreateSequenceInfo == null)
            {
                UniqueId id;
                reliableChannel = this.GetChannel(info, out id);

                if ((reliableChannel == null) && (id == null))
                {
                    this.DisposeItem(item);
                    channel.Abort();
                    return;
                }

                if (reliableChannel == null)
                    faultReply = new UnknownSequenceFault(id).CreateMessage(this.MessageVersion,
                        this.ReliableMessagingVersion);
            }
            else
            {
                reliableChannel = this.ProcessCreateSequence(info, channel, out dispatch, out newChannel);

                if (reliableChannel == null)
                    faultReply = info.FaultReply;
            }

            if (reliableChannel != null)
            {
                this.ProcessSequencedItem(channel, item, reliableChannel, info, newChannel);

                if (dispatch)
                    this.Dispatch();
            }
            else
            {
                try
                {
                    this.SendReply(faultReply, channel, item);
                    channel.Close();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                    channel.Abort();
                }
                finally
                {
                    faultReply.Close();
                    this.DisposeItem(item);
                }
            }
        }

        void OnReceiveComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    TInnerChannel channel = (TInnerChannel)result.AsyncState;
                    TItem item = default(TItem);

                    try
                    {
                        this.EndTryReceiveItem(channel, result, out item);
                        if (item == null)
                        {
                            channel.Close();
                            return;
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        if (!this.HandleException(e, channel))
                        {
                            channel.Abort();
                            return;
                        }
                    }

                    if (item != null)
                        this.HandleReceiveComplete(item, channel);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    this.Fault(e);
                }
            }
        }

        protected override void ProcessChannel(TInnerChannel channel)
        {
            try
            {
                IAsyncResult result = this.BeginTryReceiveItem(channel, this.onReceiveComplete, channel);
                if (result.CompletedSynchronously)
                {
                    ActionItem.Schedule(this.asyncHandleReceiveComplete, result);
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                channel.Abort();
                return;
            }
        }

        protected abstract void ProcessSequencedItem(TInnerChannel channel, TItem item, TReliableChannel reliableChannel, WsrmMessageInfo info, bool newChannel);
        protected abstract void SendReply(Message reply, TInnerChannel channel, TItem item);
    }

    abstract class ReliableListenerOverDuplexSession<TChannel, TReliableChannel>
        : ReliableListenerOverSession<TChannel, TReliableChannel, IDuplexSessionChannel, IDuplexSession, Message>
        where TChannel : class, IChannel
        where TReliableChannel : class, IChannel
    {
        protected ReliableListenerOverDuplexSession(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
            this.FaultHelper = new SendFaultHelper(context.Binding.SendTimeout, context.Binding.CloseTimeout);
        }

        protected override IAsyncResult BeginTryReceiveItem(IDuplexSessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginTryReceive(TimeSpan.MaxValue, callback, channel);
        }

        protected override void DisposeItem(Message item)
        {
            ((IDisposable)item).Dispose();
        }

        protected override void EndTryReceiveItem(IDuplexSessionChannel channel, IAsyncResult result, out Message item)
        {
            channel.EndTryReceive(result, out item);
        }

        protected override Message GetMessage(Message item)
        {
            return item;
        }

        protected override void SendReply(Message reply, IDuplexSessionChannel channel, Message item)
        {
            if (FaultHelper.AddressReply(item, reply))
                channel.Send(reply);
        }
    }

    abstract class ReliableListenerOverReplySession<TChannel, TReliableChannel>
        : ReliableListenerOverSession<TChannel, TReliableChannel, IReplySessionChannel, IInputSession, RequestContext>
        where TChannel : class, IChannel
        where TReliableChannel : class, IChannel
    {
        protected ReliableListenerOverReplySession(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
            this.FaultHelper = new ReplyFaultHelper(context.Binding.SendTimeout, context.Binding.CloseTimeout);
        }

        protected override IAsyncResult BeginTryReceiveItem(IReplySessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginTryReceiveRequest(TimeSpan.MaxValue, callback, channel);
        }

        protected override void DisposeItem(RequestContext item)
        {
            ((IDisposable)item.RequestMessage).Dispose();
            ((IDisposable)item).Dispose();
        }

        protected override void EndTryReceiveItem(IReplySessionChannel channel, IAsyncResult result, out RequestContext item)
        {
            channel.EndTryReceiveRequest(result, out item);
        }

        protected override Message GetMessage(RequestContext item)
        {
            return item.RequestMessage;
        }

        protected override void SendReply(Message reply, IReplySessionChannel channel, RequestContext item)
        {
            if (FaultHelper.AddressReply(item.RequestMessage, reply))
                item.Reply(reply);
        }
    }

    class ReliableDuplexListenerOverDuplex : ReliableListenerOverDuplex<IDuplexSessionChannel, ServerReliableDuplexSessionChannel>
    {
        public ReliableDuplexListenerOverDuplex(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return true; }
        }

        protected override ServerReliableDuplexSessionChannel CreateChannel(
            UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ServerReliableDuplexSessionChannel(this, binder, this.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(ServerReliableDuplexSessionChannel channel, Message message, WsrmMessageInfo info)
        {
            channel.ProcessDemuxedMessage(info);
        }
    }

    class ReliableInputListenerOverDuplex : ReliableListenerOverDuplex<IInputSessionChannel, ReliableInputSessionChannelOverDuplex>
    {
        public ReliableInputListenerOverDuplex(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return false; }
        }

        protected override ReliableInputSessionChannelOverDuplex CreateChannel(UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverDuplex(this, binder, this.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(ReliableInputSessionChannelOverDuplex channel, Message message, WsrmMessageInfo info)
        {
            channel.ProcessDemuxedMessage(info);
        }
    }

    class ReliableDuplexListenerOverDuplexSession : ReliableListenerOverDuplexSession<IDuplexSessionChannel, ServerReliableDuplexSessionChannel>
    {
        public ReliableDuplexListenerOverDuplexSession(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return true; }
        }

        protected override ServerReliableDuplexSessionChannel CreateChannel(UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ServerReliableDuplexSessionChannel(this, binder, this.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(IDuplexSessionChannel channel, Message message, ServerReliableDuplexSessionChannel reliableChannel, WsrmMessageInfo info, bool newChannel)
        {
            if (!newChannel)
            {
                IServerReliableChannelBinder binder = (IServerReliableChannelBinder)reliableChannel.Binder;

                if (!binder.UseNewChannel(channel))
                {
                    message.Close();
                    channel.Abort();
                    return;
                }
            }

            reliableChannel.ProcessDemuxedMessage(info);
        }
    }

    class ReliableInputListenerOverDuplexSession
        : ReliableListenerOverDuplexSession<IInputSessionChannel, ReliableInputSessionChannelOverDuplex>
    {
        public ReliableInputListenerOverDuplexSession(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return false; }
        }

        protected override ReliableInputSessionChannelOverDuplex CreateChannel(UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverDuplex(this, binder, this.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(IDuplexSessionChannel channel, Message message, ReliableInputSessionChannelOverDuplex reliableChannel, WsrmMessageInfo info, bool newChannel)
        {
            if (!newChannel)
            {
                IServerReliableChannelBinder binder = reliableChannel.Binder;

                if (!binder.UseNewChannel(channel))
                {
                    message.Close();
                    channel.Abort();
                    return;
                }
            }

            reliableChannel.ProcessDemuxedMessage(info);
        }
    }

    class ReliableInputListenerOverReply : ReliableListenerOverReply<IInputSessionChannel, ReliableInputSessionChannelOverReply>
    {
        public ReliableInputListenerOverReply(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return false; }
        }

        protected override ReliableInputSessionChannelOverReply CreateChannel(UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverReply(this, binder, this.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(ReliableInputSessionChannelOverReply reliableChannel, RequestContext context, WsrmMessageInfo info)
        {
            reliableChannel.ProcessDemuxedRequest(reliableChannel.Binder.WrapRequestContext(context), info);
        }
    }

    class ReliableReplyListenerOverReply : ReliableListenerOverReply<IReplySessionChannel, ReliableReplySessionChannel>
    {
        public ReliableReplyListenerOverReply(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return true; }
        }

        protected override ReliableReplySessionChannel CreateChannel(UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ReliableReplySessionChannel(this, binder, this.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(ReliableReplySessionChannel reliableChannel, RequestContext context, WsrmMessageInfo info)
        {
            reliableChannel.ProcessDemuxedRequest(reliableChannel.Binder.WrapRequestContext(context), info);
        }
    }

    class ReliableInputListenerOverReplySession : ReliableListenerOverReplySession<IInputSessionChannel, ReliableInputSessionChannelOverReply>
    {
        public ReliableInputListenerOverReplySession(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return false; }
        }

        protected override ReliableInputSessionChannelOverReply CreateChannel(
            UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverReply(this, binder, this.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(IReplySessionChannel channel, RequestContext context, ReliableInputSessionChannelOverReply reliableChannel, WsrmMessageInfo info, bool newChannel)
        {
            if (!newChannel)
            {
                IServerReliableChannelBinder binder = reliableChannel.Binder;

                if (!binder.UseNewChannel(channel))
                {
                    context.RequestMessage.Close();
                    context.Abort();
                    channel.Abort();
                    return;
                }
            }

            reliableChannel.ProcessDemuxedRequest(reliableChannel.Binder.WrapRequestContext(context), info);
        }
    }

    class ReliableReplyListenerOverReplySession : ReliableListenerOverReplySession<IReplySessionChannel, ReliableReplySessionChannel>
    {
        public ReliableReplyListenerOverReplySession(ReliableSessionBindingElement binding, BindingContext context)
            : base(binding, context)
        {
        }

        protected override bool Duplex
        {
            get { return true; }
        }

        protected override ReliableReplySessionChannel CreateChannel(UniqueId id,
            CreateSequenceInfo createSequenceInfo,
            IServerReliableChannelBinder binder)
        {
            binder.Open(this.InternalOpenTimeout);
            return new ReliableReplySessionChannel(this, binder, this.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(IReplySessionChannel channel, RequestContext context, ReliableReplySessionChannel reliableChannel, WsrmMessageInfo info, bool newChannel)
        {
            if (!newChannel)
            {
                IServerReliableChannelBinder binder = reliableChannel.Binder;

                if (!binder.UseNewChannel(channel))
                {
                    context.RequestMessage.Close();
                    context.Abort();
                    channel.Abort();
                    return;
                }
            }

            reliableChannel.ProcessDemuxedRequest(reliableChannel.Binder.WrapRequestContext(context), info);
        }
    }
}
