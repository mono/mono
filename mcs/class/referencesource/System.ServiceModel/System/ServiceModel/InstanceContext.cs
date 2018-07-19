//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    public sealed class InstanceContext : CommunicationObject, IExtensibleObject<InstanceContext>
    {
        internal static InstanceContextEmptyCallback NotifyEmptyCallback = new InstanceContextEmptyCallback(InstanceContext.NotifyEmpty);
        internal static InstanceContextIdleCallback NotifyIdleCallback = new InstanceContextIdleCallback(InstanceContext.NotifyIdle);

        bool autoClose;
        InstanceBehavior behavior;
        ServiceChannelManager channels;
        ConcurrencyInstanceContextFacet concurrency;
        ExtensionCollection<InstanceContext> extensions;
        readonly ServiceHostBase host;
        QuotaThrottle quotaThrottle;
        ServiceThrottle serviceThrottle;
        int instanceContextManagerIndex;
        object serviceInstanceLock = new object();
        SynchronizationContext synchronizationContext;
        TransactionInstanceContextFacet transaction;
        object userObject;
        bool wellKnown;
        SynchronizedCollection<IChannel> wmiChannels;
        bool isUserCreated;

        public InstanceContext(object implementation)
            : this(null, implementation)
        {
        }

        public InstanceContext(ServiceHostBase host, object implementation)
            : this(host, implementation, true)
        {
        }

        internal InstanceContext(ServiceHostBase host, object implementation, bool isUserCreated)
            : this(host, implementation, true, isUserCreated)
        {
        }

        internal InstanceContext(ServiceHostBase host, object implementation, bool wellKnown, bool isUserCreated)
        {
            this.host = host;
            if (implementation != null)
            {
                this.userObject = implementation;
                this.wellKnown = wellKnown;
            }
            this.autoClose = false;
            this.channels = new ServiceChannelManager(this);
            this.isUserCreated = isUserCreated;
        }

        public InstanceContext(ServiceHostBase host)
            : this(host, true)
        {
        }

        internal InstanceContext(ServiceHostBase host, bool isUserCreated)
        {
            if (host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("host"));
            }

            this.host = host;
            this.autoClose = true;
            this.channels = new ServiceChannelManager(this, NotifyEmptyCallback);
            this.isUserCreated = isUserCreated;
        }

        internal bool IsUserCreated
        {
            get { return this.isUserCreated; }
            set { this.isUserCreated = value; }
        }

        internal bool IsWellKnown
        {
            get { return this.wellKnown; }
        }

        internal bool AutoClose
        {
            get { return this.autoClose; }
            set { this.autoClose = value; }
        }

        internal InstanceBehavior Behavior
        {
            get { return this.behavior; }
            set
            {
                if (this.behavior == null)
                {
                    this.behavior = value;
                }
            }
        }

        internal ConcurrencyInstanceContextFacet Concurrency
        {
            get
            {
                if (this.concurrency == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.concurrency == null)
                            this.concurrency = new ConcurrencyInstanceContextFacet();
                    }
                }

                return this.concurrency;
            }
        }

        internal static InstanceContext Current
        {
            get { return OperationContext.Current != null ? OperationContext.Current.InstanceContext : null; }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                if (this.host != null)
                {
                    return this.host.CloseTimeout;
                }
                else
                {
                    return ServiceDefaults.CloseTimeout;
                }
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                if (this.host != null)
                {
                    return this.host.OpenTimeout;
                }
                else
                {
                    return ServiceDefaults.OpenTimeout;
                }
            }
        }

        public IExtensionCollection<InstanceContext> Extensions
        {
            get
            {
                this.ThrowIfClosed();
                lock (this.ThisLock)
                {
                    if (this.extensions == null)
                        this.extensions = new ExtensionCollection<InstanceContext>(this, this.ThisLock);
                    return this.extensions;
                }
            }
        }

        internal bool HasTransaction
        {
            get { return (this.transaction != null) && !object.Equals(this.transaction.Attached, null); }
        }

        public ICollection<IChannel> IncomingChannels
        {
            get
            {
                this.ThrowIfClosed();
                return channels.IncomingChannels;
            }
        }

        bool IsBusy
        {
            get
            {
                if (this.State == CommunicationState.Closed)
                    return false;
                return this.channels.IsBusy;
            }
        }

        bool IsSingleton
        {
            get
            {
                return ((this.behavior != null) &&
                        InstanceContextProviderBase.IsProviderSingleton(this.behavior.InstanceContextProvider));
            }
        }

        public ICollection<IChannel> OutgoingChannels
        {
            get
            {
                this.ThrowIfClosed();
                return channels.OutgoingChannels;
            }
        }

        public ServiceHostBase Host
        {
            get
            {
                this.ThrowIfClosed();
                return this.host;
            }
        }

        public int ManualFlowControlLimit
        {
            get { return this.EnsureQuotaThrottle().Limit; }
            set { this.EnsureQuotaThrottle().SetLimit(value); }
        }

        internal QuotaThrottle QuotaThrottle
        {
            get { return this.quotaThrottle; }
        }

        internal ServiceThrottle ServiceThrottle
        {
            get { return this.serviceThrottle; }
            set
            {
                this.ThrowIfDisposed();
                this.serviceThrottle = value;
            }
        }

        internal int InstanceContextManagerIndex
        {
            get { return this.instanceContextManagerIndex; }
            set { this.instanceContextManagerIndex = value; }
        }

        public SynchronizationContext SynchronizationContext
        {
            get { return this.synchronizationContext; }
            set
            {
                this.ThrowIfClosedOrOpened();
                this.synchronizationContext = value;
            }
        }

        new internal object ThisLock
        {
            get { return base.ThisLock; }
        }

        internal TransactionInstanceContextFacet Transaction
        {
            get
            {
                if (this.transaction == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.transaction == null)
                            this.transaction = new TransactionInstanceContextFacet(this);
                    }
                }

                return this.transaction;
            }
        }

        internal object UserObject
        {
            get { return this.userObject; }
        }

        internal ICollection<IChannel> WmiChannels
        {
            get
            {
                if (this.wmiChannels == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.wmiChannels == null)
                        {
                            this.wmiChannels = new SynchronizedCollection<IChannel>();
                        }
                    }
                }
                return this.wmiChannels;
            }
        }

        protected override void OnAbort()
        {
            channels.Abort();
            this.Unload();
        }

        internal IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channels.BeginCloseInput(timeout, callback, state);
        }

        internal void BindRpc(ref MessageRpc rpc)
        {
            this.ThrowIfClosed();
            this.channels.IncrementActivityCount();
            rpc.SuccessfullyBoundInstance = true;
        }

        internal void BindIncomingChannel(ServiceChannel channel)
        {
            this.ThrowIfDisposed();

            channel.InstanceContext = this;
            IChannel proxy = (IChannel)channel.Proxy;
            this.channels.AddIncomingChannel(proxy);

            // CSDMain 265783: Memory Leak on Chat Stress test scenario
            // There's a race condition while on one thread we received a new request from underlying sessionful channel
            // and on another thread we just aborted the channel. So the channel will be added to the IncomingChannels list of 
            // ServiceChannelManager and never get a chance to be removed.
            if (proxy != null)
            {
                CommunicationState state = channel.State;
                if (state == CommunicationState.Closing
                    || state == CommunicationState.Closed
                    || state == CommunicationState.Faulted)
                {
                    this.channels.RemoveChannel(proxy);
                }
            }
        }


        void CloseIfNotBusy()
        {
            if (!(this.State != CommunicationState.Created && this.State != CommunicationState.Opening))
            {
                Fx.Assert("InstanceContext.CloseIfNotBusy: (this.State != CommunicationState.Created && this.State != CommunicationState.Opening)");
            }

            if (this.State != CommunicationState.Opened)
                return;

            if (this.IsBusy)
                return;

            if (this.behavior.CanUnload(this) == false)
                return;

            try
            {
                if (this.State == CommunicationState.Opened)
                    this.Close();
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (InvalidOperationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
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

        internal void CloseInput(TimeSpan timeout)
        {
            channels.CloseInput(timeout);
        }

        internal void EndCloseInput(IAsyncResult result)
        {
            channels.EndCloseInput(result);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void CompleteAttachedTransaction()
        {
            Exception error = null;

            if (!this.behavior.TransactionAutoCompleteOnSessionClose)
            {
                error = new Exception();
                if (DiagnosticUtility.ShouldTraceInformation)
                    TraceUtility.TraceEvent(TraceEventType.Information,
                                                                    TraceCode.TxCompletionStatusAbortedOnSessionClose,
                                                                    SR.GetString(SR.TraceCodeTxCompletionStatusAbortedOnSessionClose,
                                                                                    transaction.Attached.TransactionInformation.LocalIdentifier)
                                                                    );

            }
            else if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information,
                                                                TraceCode.TxCompletionStatusCompletedForTACOSC,
                                                                SR.GetString(SR.TraceCodeTxCompletionStatusCompletedForTACOSC,
                                                                                transaction.Attached.TransactionInformation.LocalIdentifier)
                                                                );
            }

            transaction.CompletePendingTransaction(transaction.Attached, error);
            transaction.Attached = null;
        }

        QuotaThrottle EnsureQuotaThrottle()
        {
            lock (this.ThisLock)
            {
                if (this.quotaThrottle == null)
                {
                    this.quotaThrottle = new QuotaThrottle(ImmutableDispatchRuntime.GotDynamicInstanceContext, this.ThisLock);
                    this.quotaThrottle.Owner = "InstanceContext";
                }
                return this.quotaThrottle;
            }
        }

        internal void FaultInternal()
        {
            this.Fault();
        }

        public object GetServiceInstance()
        {
            return this.GetServiceInstance(null);
        }

        public object GetServiceInstance(Message message)
        {
            lock (this.serviceInstanceLock)
            {
                this.ThrowIfClosedOrNotOpen();

                object current = this.userObject;

                if (current != null)
                {
                    return current;
                }

                if (this.behavior == null)
                {
                    Exception error = new InvalidOperationException(SR.GetString(SR.SFxInstanceNotInitialized));
                    if (message != null)
                    {
                        throw TraceUtility.ThrowHelperError(error, message);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                    }
                }

                object newUserObject;
                if (message != null)
                {
                    newUserObject = this.behavior.GetInstance(this, message);
                }
                else
                {
                    newUserObject = this.behavior.GetInstance(this);
                }
                if (newUserObject != null)
                {
                    this.SetUserObject(newUserObject);
                }

                return newUserObject;
            }
        }

        public int IncrementManualFlowControlLimit(int incrementBy)
        {
            return this.EnsureQuotaThrottle().IncrementLimit(incrementBy);
        }

        void Load()
        {
            if (this.behavior != null)
            {
                this.behavior.Initialize(this);
            }

            if (this.host != null)
            {
                this.host.BindInstance(this);
            }
        }

        static void NotifyEmpty(InstanceContext instanceContext)
        {
            if (instanceContext.autoClose)
            {
                instanceContext.CloseIfNotBusy();
            }
        }

        static void NotifyIdle(InstanceContext instanceContext)
        {
            instanceContext.CloseIfNotBusy();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(timeout, callback, state, this);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            channels.Close(timeout);
            this.Unload();
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            ServiceThrottle throttle = this.serviceThrottle;
            if (throttle != null)
            {
                throttle.DeactivateInstanceContext();
            }
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();

            if (this.IsSingleton && (this.host != null))
            {
                this.host.FaultInternal();
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
        }

        protected override void OnOpening()
        {
            this.Load();
            base.OnOpening();
        }

        public void ReleaseServiceInstance()
        {
            this.ThrowIfDisposedOrNotOpen();
            this.SetUserObject(null);
        }

        void SetUserObject(object newUserObject)
        {
            if (this.behavior != null && !this.wellKnown)
            {
                object oldUserObject = Interlocked.Exchange(ref this.userObject, newUserObject);

                if ((oldUserObject != null) && (this.host != null) && !Object.Equals(oldUserObject, this.host.DisposableInstance))
                {
                    this.behavior.ReleaseInstance(this, oldUserObject);
                }
            }
        }

        internal void UnbindRpc(ref MessageRpc rpc)
        {
            if (rpc.InstanceContext == this && rpc.SuccessfullyBoundInstance)
            {
                this.channels.DecrementActivityCount();
            }
        }

        internal void UnbindIncomingChannel(ServiceChannel channel)
        {
            this.channels.RemoveChannel((IChannel)channel.Proxy);
        }

        void Unload()
        {
            this.SetUserObject(null);

            if (this.host != null)
            {
                this.host.UnbindInstance(this);
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            InstanceContext instanceContext;
            TimeoutHelper timeoutHelper;

            public CloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, InstanceContext instanceContext)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.instanceContext = instanceContext;
                IAsyncResult result = this.instanceContext.channels.BeginClose(this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(new AsyncCompletion(CloseChannelsCallback)), this);
                if (result.CompletedSynchronously && CloseChannelsCallback(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            bool CloseChannelsCallback(IAsyncResult result)
            {
                Fx.Assert(object.ReferenceEquals(this, result.AsyncState), "AsyncState should be this");
                this.instanceContext.channels.EndClose(result);
                this.instanceContext.Unload();
                return true;
            }
        }
    }
}
