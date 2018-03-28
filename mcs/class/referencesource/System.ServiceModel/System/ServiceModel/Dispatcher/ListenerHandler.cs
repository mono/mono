//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Transactions;
    using SessionIdleManager = System.ServiceModel.Channels.ServiceChannel.SessionIdleManager;

    class ListenerHandler : CommunicationObject, ISessionThrottleNotification
    {
        static AsyncCallback acceptCallback = Fx.ThunkCallback(new AsyncCallback(ListenerHandler.AcceptCallback));
        static Action<object> initiateChannelPump = new Action<object>(ListenerHandler.InitiateChannelPump);
        static AsyncCallback waitCallback = Fx.ThunkCallback(new AsyncCallback(ListenerHandler.WaitCallback));

        readonly ErrorHandlingAcceptor acceptor;
        readonly ChannelDispatcher channelDispatcher;
        ListenerChannel channel;
        SessionIdleManager idleManager;
        bool acceptedNull;
        bool doneAccepting;
        EndpointDispatcherTable endpoints;
        readonly ServiceHostBase host;
        readonly IListenerBinder listenerBinder;
        readonly ServiceThrottle throttle;
        IDefaultCommunicationTimeouts timeouts;
        WrappedTransaction wrappedTransaction;

        internal ListenerHandler(IListenerBinder listenerBinder, ChannelDispatcher channelDispatcher, ServiceHostBase host, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts)
        {
            this.listenerBinder = listenerBinder;
            if (!((this.listenerBinder != null)))
            {
                Fx.Assert("ListenerHandler.ctor: (this.listenerBinder != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenerBinder");
            }

            this.channelDispatcher = channelDispatcher;
            if (!((this.channelDispatcher != null)))
            {
                Fx.Assert("ListenerHandler.ctor: (this.channelDispatcher != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }

            this.host = host;
            if (!((this.host != null)))
            {
                Fx.Assert("ListenerHandler.ctor: (this.host != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }

            this.throttle = throttle;
            if (!((this.throttle != null)))
            {
                Fx.Assert("ListenerHandler.ctor: (this.throttle != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("throttle");
            }

            this.timeouts = timeouts;

            this.endpoints = channelDispatcher.EndpointDispatcherTable;
            this.acceptor = new ErrorHandlingAcceptor(listenerBinder, channelDispatcher);
        }

        internal ChannelDispatcher ChannelDispatcher
        {
            get { return this.channelDispatcher; }
        }

        internal ListenerChannel Channel
        {
            get { return this.channel; }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.host.CloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.host.OpenTimeout; }
        }

        internal EndpointDispatcherTable Endpoints
        {
            get { return this.endpoints; }
            set { this.endpoints = value; }
        }

        internal ServiceHostBase Host
        {
            get { return this.host; }
        }

        new internal object ThisLock
        {
            get { return base.ThisLock; }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.channelDispatcher.Channels.IncrementActivityCount();
            if (this.channelDispatcher.IsTransactedReceive && this.channelDispatcher.ReceiveContextEnabled && this.channelDispatcher.MaxTransactedBatchSize > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IncompatibleBehaviors)));
            }
            NewChannelPump();
        }

        internal void NewChannelPump()
        {
            ActionItem.Schedule(ListenerHandler.initiateChannelPump, this);
        }

        static void InitiateChannelPump(object state)
        {
            ListenerHandler listenerHandler = state as ListenerHandler;

            if (listenerHandler.ChannelDispatcher.IsTransactedAccept)
            {
                if (listenerHandler.ChannelDispatcher.AsynchronousTransactedAcceptEnabled)
                {
                    listenerHandler.AsyncTransactedChannelPump();
                }
                else
                {
                    listenerHandler.SyncTransactedChannelPump();
                }
            }
            else
            {
                listenerHandler.ChannelPump();
            }
        }

        void ChannelPump()
        {
            IChannelListener listener = this.listenerBinder.Listener;

            for (;;)
            {
                if (this.acceptedNull || (listener.State == CommunicationState.Faulted))
                {
                    this.DoneAccepting();
                    break;
                }

                if (!this.AcceptAndAcquireThrottle())
                {
                    break;
                }

                this.Dispatch();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void SyncTransactedChannelPump()
        {
            IChannelListener listener = this.listenerBinder.Listener;

            for (;;)
            {
                if (this.acceptedNull || (listener.State == CommunicationState.Faulted))
                {
                    this.DoneAccepting();
                    break;
                }

                this.acceptor.WaitForChannel();

                Transaction tx;
                if (this.TransactedAccept(out tx))
                {
                    if (null != tx)
                    {
                        this.wrappedTransaction = new WrappedTransaction(tx);

                        if (!this.AcquireThrottle())
                            break;

                        this.Dispatch();
                    }
                }
            }
        }

        void AsyncTransactedChannelPump()
        {
            IChannelListener listener = this.listenerBinder.Listener;

            for (;;)
            {
                if (this.acceptedNull || (listener.State == CommunicationState.Faulted))
                {
                    this.DoneAccepting();
                    break;
                }

                IAsyncResult result = this.acceptor.BeginWaitForChannel(ListenerHandler.waitCallback, this);

                if (!result.CompletedSynchronously)
                {
                    break;
                }

                this.acceptor.EndWaitForChannel(result);
                if (!this.AcceptChannel(listener))
                {
                    break;
                }
            }
        }

        static void WaitCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            };

            ListenerHandler listenerHandler = (ListenerHandler)result.AsyncState;
            IChannelListener listener = listenerHandler.listenerBinder.Listener;

            listenerHandler.acceptor.EndWaitForChannel(result);
            if (listenerHandler.AcceptChannel(listener))
            {
                listenerHandler.AsyncTransactedChannelPump();
            }
        }

        bool AcceptChannel(IChannelListener listener)
        {
            Transaction tx;

            if (this.TransactedAccept(out tx))
            {
                if (null != tx)
                {
                    this.wrappedTransaction = new WrappedTransaction(tx);

                    if (!this.AcquireThrottle())
                    {
                        return false;
                    }

                    this.Dispatch();
                }
            }

            return true;
        }

        void AbortChannels()
        {
            IChannel[] channels = this.channelDispatcher.Channels.ToArray();
            for (int index = 0; index < channels.Length; index++)
            {
                channels[index].Abort();
            }
        }

        bool AcceptAndAcquireThrottle()
        {
            IAsyncResult result = this.acceptor.BeginTryAccept(TimeSpan.MaxValue, ListenerHandler.acceptCallback, this);
            if (result.CompletedSynchronously)
            {
                return HandleEndAccept(result);
            }
            return false;
        }

        bool TransactedAccept(out Transaction tx)
        {
            tx = null;

            try
            {
                tx = TransactionBehavior.CreateTransaction(this.ChannelDispatcher.TransactionIsolationLevel, this.ChannelDispatcher.TransactionTimeout);

                IChannelBinder binder = null;
                using (TransactionScope scope = new TransactionScope(tx))
                {
                    TimeSpan acceptTimeout = TimeoutHelper.Min(this.ChannelDispatcher.TransactionTimeout, this.ChannelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
                    if (!this.acceptor.TryAccept(TransactionBehavior.NormalizeTimeout(acceptTimeout), out binder))
                    {
                        return false;
                    }
                    scope.Complete();
                }
                if (null != binder)
                {
                    this.channel = new ListenerChannel(binder);
                    this.idleManager = SessionIdleManager.CreateIfNeeded(this.channel.Binder, this.channelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
                    return true;
                }
                else
                {
                    this.AcceptedNull();
                    tx = null;
                    return false;
                }
            }
            catch (CommunicationException e)
            {
                if (null != tx)
                {
                    try
                    {
                        tx.Rollback();
                    }
                    catch (TransactionException ex)
                    {
                        DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
                    }
                }
                tx = null;

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                return false;
            }
            catch (TransactionException e)
            {
                tx = null;

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                return false;
            }
        }

        ListenerChannel CompleteAccept(IAsyncResult result)
        {
            IChannelBinder binder;
            bool valid = this.acceptor.EndTryAccept(result, out binder);

            if (valid)
            {
                if (binder != null)
                {
                    return new ListenerChannel(binder);
                }
                else
                {
                    this.AcceptedNull();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        bool HandleEndAccept(IAsyncResult result)
        {
            this.channel = this.CompleteAccept(result);

            if (this.channel != null)
            {
                Fx.Assert(this.idleManager == null, "There cannot be an existing idle manager");
                this.idleManager = SessionIdleManager.CreateIfNeeded(this.channel.Binder, this.channelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
            }
            else
            {
                this.DoneAccepting();
                return true;
            }

            return this.AcquireThrottle();
        }

        static void AcceptCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            ListenerHandler thisPtr = (ListenerHandler)result.AsyncState;

            if (thisPtr.HandleEndAccept(result))
            {
                thisPtr.Dispatch();
                thisPtr.ChannelPump();
            }
        }

        bool AcquireThrottle()
        {
            if ((this.channel != null) && (this.throttle != null) && (this.channelDispatcher.Session))
            {
                return this.throttle.AcquireSession(this);
            }

            return true;
        }

        // This callback always occurs async and always on a dirty thread
        public void ThrottleAcquired()
        {
            this.Dispatch();
            this.NewChannelPump();
        }

        void CloseChannel(IChannel channel, TimeSpan timeout)
        {
            try
            {
                if (channel.State != CommunicationState.Closing && channel.State != CommunicationState.Closed)
                {
                    CloseChannelState state = new CloseChannelState(this, channel);
                    if (channel is ISessionChannel<IDuplexSession>)
                    {
                        IDuplexSession duplexSession = ((ISessionChannel<IDuplexSession>)channel).Session;
                        IAsyncResult result = duplexSession.BeginCloseOutputSession(timeout, Fx.ThunkCallback(new AsyncCallback(CloseOutputSessionCallback)), state);
                        if (result.CompletedSynchronously)
                            duplexSession.EndCloseOutputSession(result);
                    }
                    else
                    {
                        IAsyncResult result = channel.BeginClose(timeout, Fx.ThunkCallback(new AsyncCallback(CloseChannelCallback)), state);
                        if (result.CompletedSynchronously)
                            channel.EndClose(result);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleError(e);

                if (channel is ISessionChannel<IDuplexSession>)
                {
                    channel.Abort();
                }
            }
        }

        static void CloseChannelCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            CloseChannelState state = (CloseChannelState)result.AsyncState;
            try
            {
                state.Channel.EndClose(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                state.ListenerHandler.HandleError(e);
            }
        }

        public void CloseInput(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            // Close all datagram channels
            IChannel[] channels = this.channelDispatcher.Channels.ToArray();
            for (int index = 0; index < channels.Length; index++)
            {
                IChannel channel = channels[index];
                if (!this.IsSessionChannel(channel))
                {
                    try
                    {
                        channel.Close(timeoutHelper.RemainingTime());
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.HandleError(e);
                    }
                }
            }
        }

        static void CloseOutputSessionCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            CloseChannelState state = (CloseChannelState)result.AsyncState;
            try
            {
                ((ISessionChannel<IDuplexSession>)state.Channel).Session.EndCloseOutputSession(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                state.ListenerHandler.HandleError(e);
                state.Channel.Abort();
            }
        }

        void CloseChannels(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            IChannel[] channels = this.channelDispatcher.Channels.ToArray();
            for (int index = 0; index < channels.Length; index++)
                CloseChannel(channels[index], timeoutHelper.RemainingTime());
        }

        void Dispatch()
        {
            ListenerChannel channel = this.channel;
            SessionIdleManager idleManager = this.idleManager;
            this.channel = null;
            this.idleManager = null;

            try
            {
                if (channel != null)
                {
                    ChannelHandler handler = new ChannelHandler(listenerBinder.MessageVersion, channel.Binder, this.throttle, this, (channel.Throttle != null), this.wrappedTransaction, idleManager);

                    if (!channel.Binder.HasSession)
                    {
                        this.channelDispatcher.Channels.Add(channel.Binder.Channel);
                    }

                    if (channel.Binder is DuplexChannelBinder)
                    {
                        DuplexChannelBinder duplexChannelBinder = channel.Binder as DuplexChannelBinder;
                        duplexChannelBinder.ChannelHandler = handler;
                        duplexChannelBinder.DefaultCloseTimeout = this.DefaultCloseTimeout;

                        if (this.timeouts == null)
                            duplexChannelBinder.DefaultSendTimeout = ServiceDefaults.SendTimeout;
                        else
                            duplexChannelBinder.DefaultSendTimeout = timeouts.SendTimeout;
                    }

                    ChannelHandler.Register(handler);
                    channel = null;
                    idleManager = null;
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleError(e);
            }
            finally
            {
                if (channel != null)
                {
                    channel.Binder.Channel.Abort();
                    if (this.throttle != null && this.channelDispatcher.Session)
                    {
                        this.throttle.DeactivateChannel();
                    }
                    if (idleManager != null)
                    {
                        idleManager.CancelTimer();
                    }
                }
            }
        }

        void AcceptedNull()
        {
            this.acceptedNull = true;
        }

        void DoneAccepting()
        {
            lock (this.ThisLock)
            {
                if (!this.doneAccepting)
                {
                    this.doneAccepting = true;
                    this.channelDispatcher.Channels.DecrementActivityCount();
                }
            }
        }

        bool IsSessionChannel(IChannel channel)
        {
            return (channel is ISessionChannel<IDuplexSession> ||
                    channel is ISessionChannel<IInputSession> ||
                    channel is ISessionChannel<IOutputSession>);
        }

        void CancelPendingIdleManager()
        {
            SessionIdleManager idleManager = this.idleManager;
            if (idleManager != null)
            {
                idleManager.CancelTimer();
            }
        }

        protected override void OnAbort()
        {
            // if there's an idle manager that has not been transferred to the channel handler, cancel it
            CancelPendingIdleManager();

            // Start aborting incoming channels
            this.channelDispatcher.Channels.CloseInput();

            // Abort existing channels
            this.AbortChannels();

            // Wait for channels to finish aborting
            this.channelDispatcher.Channels.Abort();

        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            // if there's an idle manager that has not been cancelled, cancel it
            CancelPendingIdleManager();

            // Start aborting incoming channels
            this.channelDispatcher.Channels.CloseInput();

            // Start closing existing channels
            this.CloseChannels(timeoutHelper.RemainingTime());

            // Wait for channels to finish closing
            return this.channelDispatcher.Channels.BeginClose(timeoutHelper.RemainingTime(), callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            // if there's an idle manager that has not been cancelled, cancel it
            CancelPendingIdleManager();

            // Start aborting incoming channels
            this.channelDispatcher.Channels.CloseInput();

            // Start closing existing channels
            this.CloseChannels(timeoutHelper.RemainingTime());

            // Wait for channels to finish closing
            this.channelDispatcher.Channels.Close(timeoutHelper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.channelDispatcher.Channels.EndClose(result);
        }

        bool HandleError(Exception e)
        {
            return this.channelDispatcher.HandleError(e);
        }

        class CloseChannelState
        {
            ListenerHandler listenerHandler;
            IChannel channel;

            internal CloseChannelState(ListenerHandler listenerHandler, IChannel channel)
            {
                this.listenerHandler = listenerHandler;
                this.channel = channel;
            }

            internal ListenerHandler ListenerHandler
            {
                get { return this.listenerHandler; }
            }

            internal IChannel Channel
            {
                get { return this.channel; }
            }
        }
    }

    class ListenerChannel
    {
        IChannelBinder binder;
        ServiceThrottle throttle;

        public ListenerChannel(IChannelBinder binder)
        {
            this.binder = binder;
        }

        public IChannelBinder Binder
        {
            get { return this.binder; }
        }

        public ServiceThrottle Throttle
        {
            get { return this.throttle; }
            set { this.throttle = value; }
        }
    }

    class WrappedTransaction
    {
        Transaction transaction;

        internal WrappedTransaction(Transaction transaction)
        {
            this.transaction = transaction;
        }

        internal Transaction Transaction
        {
            get { return this.transaction; }
        }
    }
}
