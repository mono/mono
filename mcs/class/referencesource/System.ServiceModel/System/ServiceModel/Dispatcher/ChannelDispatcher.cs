//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Text;
    using System.Transactions;

    public class ChannelDispatcher : ChannelDispatcherBase
    {
        ThreadSafeMessageFilterTable<EndpointAddress> addressTable;
        string bindingName;
        SynchronizedCollection<IChannelInitializer> channelInitializers;
        CommunicationObjectManager<IChannel> channels;
        EndpointDispatcherCollection endpointDispatchers;
        Collection<IErrorHandler> errorHandlers;
        EndpointDispatcherTable filterTable;
        ServiceHostBase host;
        bool isTransactedReceive;
        bool asynchronousTransactedAcceptEnabled;
        bool receiveContextEnabled;
        readonly IChannelListener listener;
        ListenerHandler listenerHandler;
        int maxTransactedBatchSize;
        MessageVersion messageVersion;
        SynchronizedChannelCollection<IChannel> pendingChannels; // app has not yet seen these.
        bool receiveSynchronously;
        bool sendAsynchronously;
        int maxPendingReceives;
        bool includeExceptionDetailInFaults;
        ServiceThrottle serviceThrottle;
        bool session;
        SharedRuntimeState shared;
        IDefaultCommunicationTimeouts timeouts;
        IsolationLevel transactionIsolationLevel = ServiceBehaviorAttribute.DefaultIsolationLevel;
        bool transactionIsolationLevelSet;
        TimeSpan transactionTimeout;
        bool performDefaultCloseInput;
        EventTraceActivity eventTraceActivity;
        ErrorBehavior errorBehavior;

        internal ChannelDispatcher(SharedRuntimeState shared)
        {
            this.Initialize(shared);
        }

        public ChannelDispatcher(IChannelListener listener)
            : this(listener, null, null)
        {
        }

        public ChannelDispatcher(IChannelListener listener, string bindingName)
            : this(listener, bindingName, null)
        {
        }

        public ChannelDispatcher(IChannelListener listener, string bindingName, IDefaultCommunicationTimeouts timeouts)
        {
            if (listener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listener");
            }

            this.listener = listener;
            this.bindingName = bindingName;
            this.timeouts = new ImmutableCommunicationTimeouts(timeouts);

            this.session = ((listener is IChannelListener<IInputSessionChannel>) ||
                            (listener is IChannelListener<IReplySessionChannel>) ||
                            (listener is IChannelListener<IDuplexSessionChannel>));

            this.Initialize(new SharedRuntimeState(true));
        }

        void Initialize(SharedRuntimeState shared)
        {
            this.shared = shared;
            this.endpointDispatchers = new EndpointDispatcherCollection(this);
            this.channelInitializers = this.NewBehaviorCollection<IChannelInitializer>();
            this.channels = new CommunicationObjectManager<IChannel>(this.ThisLock);
            this.pendingChannels = new SynchronizedChannelCollection<IChannel>(this.ThisLock);
            this.errorHandlers = new Collection<IErrorHandler>();
            this.isTransactedReceive = false;
            this.asynchronousTransactedAcceptEnabled = false;
            this.receiveSynchronously = false;
            this.serviceThrottle = null;
            this.transactionTimeout = TimeSpan.Zero;
            this.maxPendingReceives = MultipleReceiveBinder.MultipleReceiveDefaults.MaxPendingReceives;
            if (this.listener != null)
            {
                this.listener.Faulted += new EventHandler(OnListenerFaulted);
            }
        }

        public string BindingName
        {
            get { return this.bindingName; }
        }

        public SynchronizedCollection<IChannelInitializer> ChannelInitializers
        {
            get { return this.channelInitializers; }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                if (this.timeouts != null)
                {
                    return this.timeouts.CloseTimeout;
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
                if (this.timeouts != null)
                {
                    return this.timeouts.OpenTimeout;
                }
                else
                {
                    return ServiceDefaults.OpenTimeout;
                }
            }
        }

        internal EndpointDispatcherTable EndpointDispatcherTable
        {
            get { return this.filterTable; }
        }

        internal CommunicationObjectManager<IChannel> Channels
        {
            get { return this.channels; }
        }

        public SynchronizedCollection<EndpointDispatcher> Endpoints
        {
            get { return this.endpointDispatchers; }
        }

        public Collection<IErrorHandler> ErrorHandlers
        {
            get { return this.errorHandlers; }
        }

        public MessageVersion MessageVersion
        {
            get { return this.messageVersion; }
            set
            {
                this.messageVersion = value;
                this.ThrowIfDisposedOrImmutable();
            }
        }

        internal bool Session
        {
            get { return this.session; }
        }

        public override ServiceHostBase Host
        {
            get { return this.host; }
        }

        internal bool EnableFaults
        {
            get { return this.shared.EnableFaults; }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.shared.EnableFaults = value;
            }
        }

        internal bool IsOnServer
        {
            get { return this.shared.IsOnServer; }
        }

        public bool IsTransactedAccept
        {
            get { return this.isTransactedReceive && this.session; }
        }

        public bool IsTransactedReceive
        {
            get
            {
                return this.isTransactedReceive;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.isTransactedReceive = value;
            }
        }

        public bool AsynchronousTransactedAcceptEnabled
        {
            get
            {
                return this.asynchronousTransactedAcceptEnabled;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.asynchronousTransactedAcceptEnabled = value;
            }
        }

        public bool ReceiveContextEnabled
        {
            get
            {
                return this.receiveContextEnabled;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.receiveContextEnabled = value;
            }
        }

        internal bool BufferedReceiveEnabled
        {
            get;
            set;
        }

        public override IChannelListener Listener
        {
            get { return this.listener; }
        }

        public int MaxTransactedBatchSize
        {
            get
            {
                return this.maxTransactedBatchSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
                }

                this.ThrowIfDisposedOrImmutable();
                this.maxTransactedBatchSize = value;
            }
        }

        public ServiceThrottle ServiceThrottle
        {
            get
            {
                return this.serviceThrottle;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.serviceThrottle = value;
            }
        }

        public bool ManualAddressing
        {
            get { return this.shared.ManualAddressing; }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.shared.ManualAddressing = value;
            }
        }

        internal SynchronizedChannelCollection<IChannel> PendingChannels
        {
            get { return this.pendingChannels; }
        }

        public bool ReceiveSynchronously
        {
            get
            {
                return this.receiveSynchronously;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.receiveSynchronously = value;
            }
        }

        public bool SendAsynchronously
        {
            get
            {
                return this.sendAsynchronously;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.sendAsynchronously = value;
            }

        }

        public int MaxPendingReceives
        {
            get
            {
                return this.maxPendingReceives;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.maxPendingReceives = value;
            }
        }

        public bool IncludeExceptionDetailInFaults
        {
            get { return this.includeExceptionDetailInFaults; }
            set
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfDisposedOrImmutable();
                    this.includeExceptionDetailInFaults = value;
                }
            }
        }

        internal IDefaultCommunicationTimeouts DefaultCommunicationTimeouts
        {
            get { return this.timeouts; }
        }

        public IsolationLevel TransactionIsolationLevel
        {
            get { return this.transactionIsolationLevel; }
            set
            {
                switch (value)
                {
                    case IsolationLevel.Serializable:
                    case IsolationLevel.RepeatableRead:
                    case IsolationLevel.ReadCommitted:
                    case IsolationLevel.ReadUncommitted:
                    case IsolationLevel.Unspecified:
                    case IsolationLevel.Chaos:
                    case IsolationLevel.Snapshot:
                        break;

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.ThrowIfDisposedOrImmutable();
                this.transactionIsolationLevel = value;
                this.transactionIsolationLevelSet = true;
            }
        }

        internal bool TransactionIsolationLevelSet
        {
            get { return this.transactionIsolationLevelSet; }
        }

        public TimeSpan TransactionTimeout
        {
            get
            {
                return this.transactionTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.ThrowIfDisposedOrImmutable();
                this.transactionTimeout = value;
            }
        }

        void AbortPendingChannels()
        {
            lock (this.ThisLock)
            {
                for (int i = this.pendingChannels.Count - 1; i >= 0; i--)
                {
                    this.pendingChannels[i].Abort();
                }
            }
        }

        internal override void CloseInput(TimeSpan timeout)
        {
            // we have to perform some slightly convoluted logic here due to 
            // backwards compat. We probably need an IAsyncChannelDispatcher 
            // interface that has timeouts and async
            this.CloseInput();

            if (this.performDefaultCloseInput)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                lock (this.ThisLock)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        for (int i = 0; i < this.endpointDispatchers.Count; i++)
                        {
                            EndpointDispatcher endpointDispatcher = this.endpointDispatchers[i];
                            this.TraceEndpointLifetime(endpointDispatcher, TraceCode.EndpointListenerClose, SR.GetString(SR.TraceCodeEndpointListenerClose));
                        }
                    }

                    ListenerHandler handler = this.listenerHandler;
                    if (handler != null)
                    {
                        handler.CloseInput(timeoutHelper.RemainingTime());
                    }
                }

                if (!this.session)
                {
                    ListenerHandler handler = this.listenerHandler;
                    if (handler != null)
                    {
                        handler.Close(timeoutHelper.RemainingTime());
                    }
                }
            }
        }

        internal void ReleasePerformanceCounters()
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                for (int i = 0; i < this.endpointDispatchers.Count; i++)
                {
                    if (null != this.endpointDispatchers[i])
                    {
                        this.endpointDispatchers[i].ReleasePerformanceCounters();
                    }
                }
            }
        }

        public override void CloseInput()
        {
            this.performDefaultCloseInput = true;
        }

        void OnListenerFaulted(object sender, EventArgs e)
        {
            this.Fault();
        }

        internal bool HandleError(Exception error)
        {
            ErrorHandlerFaultInfo dummy = new ErrorHandlerFaultInfo();
            return this.HandleError(error, ref dummy);
        }

        internal bool HandleError(Exception error, ref ErrorHandlerFaultInfo faultInfo)
        {
            ErrorBehavior behavior;

            lock (this.ThisLock)
            {
                if (this.errorBehavior != null)
                {
                    behavior = this.errorBehavior;
                }
                else
                {
                    behavior = new ErrorBehavior(this);
                }
            }

            if (behavior != null)
            {
                return behavior.HandleError(error, ref faultInfo);
            }
            else
            {
                return false;
            }
        }

        internal void InitializeChannel(IClientChannel channel)
        {
            this.ThrowIfDisposedOrNotOpen();
            try
            {
                for (int i = 0; i < this.channelInitializers.Count; ++i)
                {
                    this.channelInitializers[i].Initialize(channel);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        internal EndpointDispatcher Match(Message message, out bool addressMatched)
        {
            lock (this.ThisLock)
            {
                return this.filterTable.Lookup(message, out addressMatched);
            }
        }

        internal SynchronizedCollection<T> NewBehaviorCollection<T>()
        {
            return new ChannelDispatcherBehaviorCollection<T>(this);
        }

        internal bool HasApplicationEndpoints()
        {
            foreach (EndpointDispatcher endpointDispatcher in this.Endpoints)
            {
                if (!endpointDispatcher.IsSystemEndpoint)
                {
                    return true;
                }
            }
            return false;
        }

        void OnAddEndpoint(EndpointDispatcher endpoint)
        {
            lock (this.ThisLock)
            {
                endpoint.Attach(this);

                if (this.State == CommunicationState.Opened)
                {
                    if (this.addressTable != null)
                    {
                        this.addressTable.Add(endpoint.AddressFilter, endpoint.EndpointAddress, endpoint.FilterPriority);
                    }

                    this.filterTable.AddEndpoint(endpoint);
                }
            }
        }

        void OnRemoveEndpoint(EndpointDispatcher endpoint)
        {
            lock (this.ThisLock)
            {
                if (this.State == CommunicationState.Opened)
                {
                    this.filterTable.RemoveEndpoint(endpoint);

                    if (this.addressTable != null)
                    {
                        this.addressTable.Remove(endpoint.AddressFilter);
                    }
                }

                endpoint.Detach(this);
            }
        }

        protected override void OnAbort()
        {
            if (this.listener != null)
            {
                this.listener.Abort();
            }

            ListenerHandler handler = this.listenerHandler;
            if (handler != null)
            {
                handler.Abort();
            }

            this.AbortPendingChannels();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            if (this.listener != null)
            {
                this.listener.Close(timeoutHelper.RemainingTime());
            }

            ListenerHandler handler = this.listenerHandler;
            if (handler != null)
            {
                handler.Close(timeoutHelper.RemainingTime());
            }

            this.AbortPendingChannels();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<ICommunicationObject> list = new List<ICommunicationObject>();

            if (this.listener != null)
            {
                list.Add(this.listener);
            }

            ListenerHandler handler = this.listenerHandler;
            if (handler != null)
            {
                list.Add(handler);
            }

            return new CloseCollectionAsyncResult(timeout, callback, state, list);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            try
            {
                CloseCollectionAsyncResult.End(result);
            }
            finally
            {
                this.AbortPendingChannels();
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                for (int i = 0; i < this.endpointDispatchers.Count; i++)
                {
                    EndpointDispatcher endpointDispatcher = this.endpointDispatchers[i];
                    this.TraceEndpointLifetime(endpointDispatcher, TraceCode.EndpointListenerClose, SR.GetString(SR.TraceCodeEndpointListenerClose));
                }
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            ThrowIfNotAttachedToHost();
            ThrowIfNoMessageVersion();

            if (this.listener != null)
            {
                try
                {
                    this.listener.Open(timeout);
                }
                catch (InvalidOperationException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateOuterExceptionWithEndpointsInformation(e));
                }
            }
        }

        InvalidOperationException CreateOuterExceptionWithEndpointsInformation(InvalidOperationException e)
        {
            string endpointContractNames = CreateContractListString();

            if (String.IsNullOrEmpty(endpointContractNames))
            {
                return new InvalidOperationException(SR.GetString(SR.SFxChannelDispatcherUnableToOpen1, this.listener.Uri), e);
            }
            else
            {
                return new InvalidOperationException(SR.GetString(SR.SFxChannelDispatcherUnableToOpen2, this.listener.Uri, endpointContractNames), e);
            }

        }

        internal string CreateContractListString()
        {
            const string OpenQuote = "\"";
            const string CloseQuote = "\"";
            const string Space = " ";

            Collection<string> namesSeen = new Collection<string>();
            StringBuilder endpointContractNames = new StringBuilder();

            lock (this.ThisLock)
            {
                foreach (EndpointDispatcher ed in this.Endpoints)
                {
                    if (!namesSeen.Contains(ed.ContractName))
                    {
                        if (endpointContractNames.Length > 0)
                        {
                            endpointContractNames.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                            endpointContractNames.Append(Space);
                        }

                        endpointContractNames.Append(OpenQuote);
                        endpointContractNames.Append(ed.ContractName);
                        endpointContractNames.Append(CloseQuote);

                        namesSeen.Add(ed.ContractName);
                    }
                }
            }

            return endpointContractNames.ToString();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfNotAttachedToHost();
            ThrowIfNoMessageVersion();

            if (this.listener != null)
            {
                try
                {
                    return this.listener.BeginOpen(timeout, callback, state);
                }
                catch (InvalidOperationException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateOuterExceptionWithEndpointsInformation(e));
                }
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (this.listener != null)
            {
                try
                {
                    this.listener.EndOpen(result);
                }
                catch (InvalidOperationException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateOuterExceptionWithEndpointsInformation(e));
                }
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected override void OnOpening()
        {
            ThrowIfNotAttachedToHost();

            if (TD.ListenerOpenStartIsEnabled())
            {
                this.eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
                TD.ListenerOpenStart(this.eventTraceActivity,
                    (this.Listener != null) ? this.Listener.Uri.ToString() : string.Empty,
                    (this.host != null && host.EventTraceActivity != null) ? this.host.EventTraceActivity.ActivityId : Guid.Empty);
            }

            base.OnOpening();
        }

        protected override void OnOpened()
        {
            ThrowIfNotAttachedToHost();
            base.OnOpened();

            if (TD.ListenerOpenStopIsEnabled())
            {
                TD.ListenerOpenStop(this.eventTraceActivity);
                this.eventTraceActivity = null; // clear this since we don't need this anymore.
            }

            this.errorBehavior = new ErrorBehavior(this);

            this.filterTable = new EndpointDispatcherTable(this.ThisLock);
            for (int i = 0; i < this.endpointDispatchers.Count; i++)
            {
                EndpointDispatcher endpoint = this.endpointDispatchers[i];

                // Force a build of the runtime to catch any unexpected errors before we are done opening.
                endpoint.DispatchRuntime.GetRuntime();
                // Lock down the DispatchRuntime.
                endpoint.DispatchRuntime.LockDownProperties();

                this.filterTable.AddEndpoint(endpoint);

                if ((this.addressTable != null) && (endpoint.OriginalAddress != null))
                {
                    this.addressTable.Add(endpoint.AddressFilter, endpoint.OriginalAddress, endpoint.FilterPriority);
                }

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    this.TraceEndpointLifetime(endpoint, TraceCode.EndpointListenerOpen, SR.GetString(SR.TraceCodeEndpointListenerOpen));
                }
            }

            ServiceThrottle throttle = this.serviceThrottle;
            if (throttle == null)
            {
                throttle = this.host.ServiceThrottle;
            }

            IListenerBinder binder = ListenerBinder.GetBinder(this.listener, this.messageVersion);
            this.listenerHandler = new ListenerHandler(binder, this, this.host, throttle, this.timeouts);
            this.listenerHandler.Open();  // This never throws, which is why it's ok for it to happen in OnOpened
        }

        internal void ProvideFault(Exception e, FaultConverter faultConverter, ref ErrorHandlerFaultInfo faultInfo)
        {
            ErrorBehavior behavior;

            lock (this.ThisLock)
            {
                if (this.errorBehavior != null)
                {
                    behavior = this.errorBehavior;
                }
                else
                {
                    behavior = new ErrorBehavior(this);
                }
            }

            behavior.ProvideFault(e, faultConverter, ref faultInfo);
        }

        internal void SetEndpointAddressTable(ThreadSafeMessageFilterTable<EndpointAddress> table)
        {
            if (table == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("table");
            }

            this.ThrowIfDisposedOrImmutable();

            this.addressTable = table;
        }

        internal new void ThrowIfDisposedOrImmutable()
        {
            base.ThrowIfDisposedOrImmutable();
            this.shared.ThrowIfImmutable();
        }

        void ThrowIfNotAttachedToHost()
        {
            // if we are on the server, we need a host
            // if we are on the client, we never call Open(), so this method is not invoked
            if (this.host == null)
            {
                Exception error = new InvalidOperationException(SR.GetString(SR.SFxChannelDispatcherNoHost0));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }
        }

        void ThrowIfNoMessageVersion()
        {
            if (this.messageVersion == null)
            {
                Exception error = new InvalidOperationException(SR.GetString(SR.SFxChannelDispatcherNoMessageVersion));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }
        }

        void TraceEndpointLifetime(EndpointDispatcher endpoint, int traceCode, string traceDescription)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, object> values = new Dictionary<string, object>(3)
                {
                    { "ContractNamespace",  endpoint.ContractNamespace },
                    { "ContractName",  endpoint.ContractName },
                    { "Endpoint",  endpoint.ListenUri }
                };
                TraceUtility.TraceEvent(TraceEventType.Information, traceCode,
                    traceDescription, new DictionaryTraceRecord(values), endpoint, null);
            }
        }

        protected override void Attach(ServiceHostBase host)
        {
            if (host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }

            ServiceHostBase serviceHost = host;

            this.ThrowIfDisposedOrImmutable();

            if (this.host != null)
            {
                Exception error = new InvalidOperationException(SR.GetString(SR.SFxChannelDispatcherMultipleHost0));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }

            this.host = serviceHost;
        }

        protected override void Detach(ServiceHostBase host)
        {
            if (host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }

            if (this.host != host)
            {
                Exception error = new InvalidOperationException(SR.GetString(SR.SFxChannelDispatcherDifferentHost0));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }

            this.ThrowIfDisposedOrImmutable();

            this.host = null;
        }

        class EndpointDispatcherCollection : SynchronizedCollection<EndpointDispatcher>
        {
            ChannelDispatcher owner;

            internal EndpointDispatcherCollection(ChannelDispatcher owner)
                : base(owner.ThisLock)
            {
                this.owner = owner;
            }

            protected override void ClearItems()
            {
                foreach (EndpointDispatcher item in this.Items)
                {
                    this.owner.OnRemoveEndpoint(item);
                }
                base.ClearItems();
            }

            protected override void InsertItem(int index, EndpointDispatcher item)
            {
                if (item == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");

                this.owner.OnAddEndpoint(item);
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                EndpointDispatcher item = this.Items[index];
                base.RemoveItem(index);
                this.owner.OnRemoveEndpoint(item);
            }

            protected override void SetItem(int index, EndpointDispatcher item)
            {
                Exception error = new InvalidOperationException(SR.GetString(SR.SFxCollectionDoesNotSupportSet0));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }
        }

        class ChannelDispatcherBehaviorCollection<T> : SynchronizedCollection<T>
        {
            ChannelDispatcher outer;

            internal ChannelDispatcherBehaviorCollection(ChannelDispatcher outer)
                : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.ThrowIfDisposedOrImmutable();
                base.ClearItems();
            }

            protected override void InsertItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }

                this.outer.ThrowIfDisposedOrImmutable();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.ThrowIfDisposedOrImmutable();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }

                this.outer.ThrowIfDisposedOrImmutable();
                base.SetItem(index, item);
            }
        }
    }
}
