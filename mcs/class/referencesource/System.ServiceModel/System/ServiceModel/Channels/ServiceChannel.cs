//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;

    // This class is sealed because the constructor could call Abort, which is virtual
    sealed class ServiceChannel : CommunicationObject, IChannel, IClientChannel, IDuplexContextChannel, IOutputChannel, IRequestChannel, IServiceChannel
    {

        int activityCount = 0;
        bool allowInitializationUI = true;
        bool allowOutputBatching = false;
        bool autoClose = true;
        CallOnceManager autoDisplayUIManager;
        CallOnceManager autoOpenManager;
        readonly IChannelBinder binder;
        readonly ChannelDispatcher channelDispatcher;
        ClientRuntime clientRuntime;
        readonly bool closeBinder = true;
        bool closeFactory;
        bool didInteractiveInitialization;
        bool doneReceiving;
        EndpointDispatcher endpointDispatcher;
        bool explicitlyOpened;
        ExtensionCollection<IContextChannel> extensions;
        readonly ServiceChannelFactory factory;
        readonly bool hasSession;
        readonly SessionIdleManager idleManager;
        InstanceContext instanceContext;
        ServiceThrottle instanceContextServiceThrottle;
        bool isPending;
        readonly bool isReplyChannel;
        EndpointAddress localAddress;
        readonly MessageVersion messageVersion;
        readonly bool openBinder = false;
        TimeSpan operationTimeout;
        object proxy;
        ServiceThrottle serviceThrottle;
        string terminatingOperationName;
        InstanceContext wmiInstanceContext;
        bool hasChannelStartedAutoClosing;
        bool hasIncrementedBusyCount;
        bool hasCleanedUpChannelCollections;
        EventTraceActivity eventActivity;

        EventHandler<UnknownMessageReceivedEventArgs> unknownMessageReceived;

        ServiceChannel(IChannelBinder binder, MessageVersion messageVersion, IDefaultCommunicationTimeouts timeouts)
        {
            if (binder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binder");
            }

            this.messageVersion = messageVersion;
            this.binder = binder;
            this.isReplyChannel = this.binder.Channel is IReplyChannel;

            IChannel innerChannel = binder.Channel;
            this.hasSession = (innerChannel is ISessionChannel<IDuplexSession>) ||
                        (innerChannel is ISessionChannel<IInputSession>) ||
                        (innerChannel is ISessionChannel<IOutputSession>);

            this.IncrementActivity();
            this.openBinder = (binder.Channel.State == CommunicationState.Created);

            this.operationTimeout = timeouts.SendTimeout;
        }

        internal ServiceChannel(ServiceChannelFactory factory, IChannelBinder binder)
            : this(binder, factory.MessageVersion, factory)
        {
            this.factory = factory;
            this.clientRuntime = factory.ClientRuntime;

            this.SetupInnerChannelFaultHandler();

            DispatchRuntime dispatch = factory.ClientRuntime.DispatchRuntime;
            if (dispatch != null)
            {
                this.autoClose = dispatch.AutomaticInputSessionShutdown;
            }

            factory.ChannelCreated(this);
        }

        internal ServiceChannel(IChannelBinder binder,
                                EndpointDispatcher endpointDispatcher,
                                ChannelDispatcher channelDispatcher,
                                SessionIdleManager idleManager)
            : this(binder, channelDispatcher.MessageVersion, channelDispatcher.DefaultCommunicationTimeouts)
        {
            if (endpointDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointDispatcher");
            }

            this.channelDispatcher = channelDispatcher;
            this.endpointDispatcher = endpointDispatcher;
            this.clientRuntime = endpointDispatcher.DispatchRuntime.CallbackClientRuntime;

            this.SetupInnerChannelFaultHandler();

            this.autoClose = endpointDispatcher.DispatchRuntime.AutomaticInputSessionShutdown;
            this.isPending = true;

            IDefaultCommunicationTimeouts timeouts = channelDispatcher.DefaultCommunicationTimeouts;
            this.idleManager = idleManager;

            if (!binder.HasSession)
                this.closeBinder = false;

            if (this.idleManager != null)
            {
                bool didIdleAbort;
                this.idleManager.RegisterChannel(this, out didIdleAbort);
                if (didIdleAbort)
                {
                    this.Abort();
                }
            }
        }

        CallOnceManager AutoOpenManager
        {
            get
            {
                if (!this.explicitlyOpened && (this.autoOpenManager == null))
                {
                    this.EnsureAutoOpenManagers();
                }
                return this.autoOpenManager;
            }
        }

        CallOnceManager AutoDisplayUIManager
        {
            get
            {
                if (!this.explicitlyOpened && (this.autoDisplayUIManager == null))
                {
                    this.EnsureAutoOpenManagers();
                }
                return this.autoDisplayUIManager;
            }
        }


        internal EventTraceActivity EventActivity
        {
            get
            {
                if (this.eventActivity == null)
                {
                    //Take the id on the thread so that we know the initiating operation.
                    this.eventActivity = EventTraceActivity.GetFromThreadOrCreate();
                }
                return this.eventActivity;
            }
        }

        internal bool CloseFactory
        {
            get { return this.closeFactory; }
            set { this.closeFactory = value; }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.CloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.OpenTimeout; }
        }

        internal DispatchRuntime DispatchRuntime
        {
            get
            {
                if (this.endpointDispatcher != null)
                {
                    return this.endpointDispatcher.DispatchRuntime;
                }
                if (this.clientRuntime != null)
                {
                    return this.clientRuntime.DispatchRuntime;
                }
                return null;
            }
        }

        internal MessageVersion MessageVersion
        {
            get { return this.messageVersion; }
        }

        internal IChannelBinder Binder
        {
            get { return this.binder; }
        }

        internal TimeSpan CloseTimeout
        {
            get
            {
                if (this.IsClient)
                {
                    return factory.InternalCloseTimeout;
                }
                else
                {
                    return this.ChannelDispatcher.InternalCloseTimeout;
                }
            }
        }

        internal ChannelDispatcher ChannelDispatcher
        {
            get { return this.channelDispatcher; }
        }

        internal EndpointDispatcher EndpointDispatcher
        {
            get { return this.endpointDispatcher; }
            set
            {
                lock (this.ThisLock)
                {
                    this.endpointDispatcher = value;
                    this.clientRuntime = value.DispatchRuntime.CallbackClientRuntime;
                }
            }
        }

        internal ServiceChannelFactory Factory
        {
            get { return this.factory; }
        }

        internal IChannel InnerChannel
        {
            get { return this.binder.Channel; }
        }

        internal bool IsPending
        {
            get { return this.isPending; }
            set { this.isPending = value; }
        }

        internal bool HasSession
        {
            get { return hasSession; }
        }

        internal bool IsClient
        {
            get { return this.factory != null; }
        }

        internal bool IsReplyChannel
        {
            get { return this.isReplyChannel; }
        }

        public Uri ListenUri
        {
            get
            {
                return this.binder.ListenUri;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                if (this.localAddress == null)
                {
                    if (this.endpointDispatcher != null)
                    {
                        this.localAddress = this.endpointDispatcher.EndpointAddress;
                    }
                    else
                    {
                        this.localAddress = this.binder.LocalAddress;
                    }
                }
                return this.localAddress;
            }
        }

        internal TimeSpan OpenTimeout
        {
            get
            {
                if (this.IsClient)
                {
                    return factory.InternalOpenTimeout;
                }
                else
                {
                    return this.ChannelDispatcher.InternalOpenTimeout;
                }
            }
        }

        public TimeSpan OperationTimeout
        {
            get { return this.operationTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    string message = SR.GetString(SR.SFxTimeoutOutOfRange0);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }


                this.operationTimeout = value;
            }
        }

        internal object Proxy
        {
            get
            {
                object proxy = this.proxy;
                if (proxy != null)
                    return proxy;
                else
                    return this;
            }
            set
            {
                this.proxy = value;
                base.EventSender = value;   // need to use "proxy" as open/close event source
            }
        }

        internal ClientRuntime ClientRuntime
        {
            get { return this.clientRuntime; }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                IOutputChannel outputChannel = this.InnerChannel as IOutputChannel;
                if (outputChannel != null)
                    return outputChannel.RemoteAddress;

                IRequestChannel requestChannel = this.InnerChannel as IRequestChannel;
                if (requestChannel != null)
                    return requestChannel.RemoteAddress;

                return null;
            }
        }

        ProxyOperationRuntime UnhandledProxyOperation
        {
            get { return this.ClientRuntime.GetRuntime().UnhandledProxyOperation; }
        }

        public Uri Via
        {
            get
            {
                IOutputChannel outputChannel = this.InnerChannel as IOutputChannel;
                if (outputChannel != null)
                    return outputChannel.Via;

                IRequestChannel requestChannel = this.InnerChannel as IRequestChannel;
                if (requestChannel != null)
                    return requestChannel.Via;

                return null;
            }
        }

        internal InstanceContext InstanceContext
        {
            get { return this.instanceContext; }
            set { this.instanceContext = value; }
        }

        internal ServiceThrottle InstanceContextServiceThrottle
        {
            get { return this.instanceContextServiceThrottle; }
            set { this.instanceContextServiceThrottle = value; }
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

        internal InstanceContext WmiInstanceContext
        {
            get { return this.wmiInstanceContext; }
            set { this.wmiInstanceContext = value; }
        }

        void SetupInnerChannelFaultHandler()
        {
            // need to call this method after this.binder and this.clientRuntime are set to prevent a potential 
            // NullReferenceException in this method or in the OnInnerChannelFaulted method; 
            // because this method accesses this.binder and OnInnerChannelFaulted acesses this.clientRuntime.
            this.binder.Channel.Faulted += OnInnerChannelFaulted;
        }

        void BindDuplexCallbacks()
        {
            IDuplexChannel duplexChannel = this.InnerChannel as IDuplexChannel;
            if ((duplexChannel != null) && (this.factory != null) && (this.instanceContext != null))
            {
                if (this.binder is DuplexChannelBinder)
                    ((DuplexChannelBinder)this.binder).EnsurePumping();
            }
        }

        internal bool CanCastTo(Type t)
        {
            if (t.IsAssignableFrom(typeof(IClientChannel)))
                return true;

            if (t.IsAssignableFrom(typeof(IDuplexContextChannel)))
                return this.InnerChannel is IDuplexChannel;

            if (t.IsAssignableFrom(typeof(IServiceChannel)))
                return true;

            return false;
        }

        internal void CompletedIOOperation()
        {
            if (this.idleManager != null)
            {
                this.idleManager.CompletedActivity();
            }
        }

        void EnsureAutoOpenManagers()
        {
            lock (this.ThisLock)
            {
                if (!this.explicitlyOpened)
                {
                    if (this.autoOpenManager == null)
                    {
                        this.autoOpenManager = new CallOnceManager(this, CallOpenOnce.Instance);
                    }
                    if (this.autoDisplayUIManager == null)
                    {
                        this.autoDisplayUIManager = new CallOnceManager(this, CallDisplayUIOnce.Instance);
                    }
                }
            }
        }

        void EnsureDisplayUI()
        {
            CallOnceManager manager = this.AutoDisplayUIManager;
            if (manager != null)
            {
                manager.CallOnce(TimeSpan.MaxValue, null);
            }
            this.ThrowIfInitializationUINotCalled();
        }

        IAsyncResult BeginEnsureDisplayUI(AsyncCallback callback, object state)
        {
            CallOnceManager manager = this.AutoDisplayUIManager;
            if (manager != null)
            {
                return manager.BeginCallOnce(TimeSpan.MaxValue, null, callback, state);
            }
            else
            {
                return new CallOnceCompletedAsyncResult(callback, state);
            }
        }

        void EndEnsureDisplayUI(IAsyncResult result)
        {
            CallOnceManager manager = this.AutoDisplayUIManager;
            if (manager != null)
            {
                manager.EndCallOnce(result);
            }
            else
            {
                CallOnceCompletedAsyncResult.End(result);
            }
            this.ThrowIfInitializationUINotCalled();
        }

        void EnsureOpened(TimeSpan timeout)
        {
            CallOnceManager manager = this.AutoOpenManager;
            if (manager != null)
            {
                manager.CallOnce(timeout, this.autoDisplayUIManager);
            }

            this.ThrowIfOpening();
            this.ThrowIfDisposedOrNotOpen();
        }

        IAsyncResult BeginEnsureOpened(TimeSpan timeout, AsyncCallback callback, object state)
        {
            CallOnceManager manager = this.AutoOpenManager;
            if (manager != null)
            {
                return manager.BeginCallOnce(timeout, this.autoDisplayUIManager, callback, state);
            }
            else
            {
                this.ThrowIfOpening();
                this.ThrowIfDisposedOrNotOpen();

                return new CallOnceCompletedAsyncResult(callback, state);
            }
        }

        void EndEnsureOpened(IAsyncResult result)
        {
            CallOnceManager manager = this.AutoOpenManager;
            if (manager != null)
            {
                manager.EndCallOnce(result);
            }
            else
            {
                CallOnceCompletedAsyncResult.End(result);
            }
        }

        public T GetProperty<T>() where T : class
        {
            IChannel innerChannel = this.InnerChannel;
            if (innerChannel != null)
                return innerChannel.GetProperty<T>();
            return null;
        }

        void PrepareCall(ProxyOperationRuntime operation, bool oneway, ref ProxyRpc rpc)
        {
            OperationContext context = OperationContext.Current;
            // Doing a request reply callback when dispatching in-order deadlocks.
            // We never receive the reply until we finish processing the current message.
            if (!oneway)
            {
                DispatchRuntime dispatchBehavior = this.ClientRuntime.DispatchRuntime;
                if ((dispatchBehavior != null) && (dispatchBehavior.ConcurrencyMode == ConcurrencyMode.Single))
                {
                    if ((context != null) && (!context.IsUserContext) && (context.InternalServiceChannel == this))
                    {
                        if (dispatchBehavior.IsOnServer)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCallbackRequestReplyInOrder1, typeof(ServiceBehaviorAttribute).Name)));
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCallbackRequestReplyInOrder1, typeof(CallbackBehaviorAttribute).Name)));
                        }
                    }
                }
            }

            if ((this.State == CommunicationState.Created) && !operation.IsInitiating)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNonInitiatingOperation1, operation.Name)));
            }

            if (this.terminatingOperationName != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxTerminatingOperationAlreadyCalled1, this.terminatingOperationName)));
            }

            if (this.hasChannelStartedAutoClosing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.SFxClientOutputSessionAutoClosed)));
            }

            operation.BeforeRequest(ref rpc);
            AddMessageProperties(rpc.Request, context);
            if (!oneway && !this.ClientRuntime.ManualAddressing && rpc.Request.Version.Addressing != AddressingVersion.None)
            {
                RequestReplyCorrelator.PrepareRequest(rpc.Request);

                MessageHeaders headers = rpc.Request.Headers;
                EndpointAddress localAddress = this.LocalAddress;
                EndpointAddress replyTo = headers.ReplyTo;

                if (replyTo == null)
                {
                    headers.ReplyTo = localAddress ?? EndpointAddress.AnonymousAddress;
                }

                if (this.IsClient && (localAddress != null) && !localAddress.IsAnonymous)
                {
                    Uri localUri = localAddress.Uri;

                    if ((replyTo != null) && !replyTo.IsAnonymous && (localUri != replyTo.Uri))
                    {
                        string text = SR.GetString(SR.SFxRequestHasInvalidReplyToOnClient, replyTo.Uri, localUri);
                        Exception error = new InvalidOperationException(text);
                        throw TraceUtility.ThrowHelperError(error, rpc.Request);
                    }

                    EndpointAddress faultTo = headers.FaultTo;
                    if ((faultTo != null) && !faultTo.IsAnonymous && (localUri != faultTo.Uri))
                    {
                        string text = SR.GetString(SR.SFxRequestHasInvalidFaultToOnClient, faultTo.Uri, localUri);
                        Exception error = new InvalidOperationException(text);
                        throw TraceUtility.ThrowHelperError(error, rpc.Request);
                    }

                    if (this.messageVersion.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        EndpointAddress from = headers.From;
                        if ((from != null) && !from.IsAnonymous && (localUri != from.Uri))
                        {
                            string text = SR.GetString(SR.SFxRequestHasInvalidFromOnClient, from.Uri, localUri);
                            Exception error = new InvalidOperationException(text);
                            throw TraceUtility.ThrowHelperError(error, rpc.Request);
                        }
                    }
                }
            }

            if (TraceUtility.MessageFlowTracingOnly)
            {
                //always set a new ID if none provided
                if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                {
                    rpc.ActivityId = Guid.NewGuid();
                    FxTrace.Trace.SetAndTraceTransfer(rpc.ActivityId, true);
                }
            }

            if (rpc.Activity != null)
            {
                TraceUtility.SetActivity(rpc.Request, rpc.Activity);
                if (TraceUtility.ShouldPropagateActivity)
                {
                    TraceUtility.AddActivityHeader(rpc.Request);
                }
            }
            else if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
            {
                TraceUtility.AddAmbientActivityToMessage(rpc.Request);
            }
            operation.Parent.BeforeSendRequest(ref rpc);


            //Attach and transfer Activity
            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                TraceClientOperationPrepared(ref rpc);
            }

            TraceUtility.MessageFlowAtMessageSent(rpc.Request, rpc.EventTraceActivity);

            if (MessageLogger.LogMessagesAtServiceLevel)
            {
                MessageLogger.LogMessage(ref rpc.Request, (oneway ? MessageLoggingSource.ServiceLevelSendDatagram : MessageLoggingSource.ServiceLevelSendRequest) | MessageLoggingSource.LastChance);
            }
        }

        private void TraceClientOperationPrepared(ref ProxyRpc rpc)
        {
            //Retrieve the old id on the RPC and attach the id on the message since we have a message id now.
            Guid previousId = rpc.EventTraceActivity != null ? rpc.EventTraceActivity.ActivityId : Guid.Empty;
            EventTraceActivity requestActivity = EventTraceActivityHelper.TryExtractActivity(rpc.Request);
            if (requestActivity == null)
            {
                requestActivity = EventTraceActivity.GetFromThreadOrCreate();
                EventTraceActivityHelper.TryAttachActivity(rpc.Request, requestActivity);
            }
            rpc.EventTraceActivity = requestActivity;

            if (TD.ClientOperationPreparedIsEnabled())
            {
                string remoteAddress = string.Empty;
                if (this.RemoteAddress != null && this.RemoteAddress.Uri != null)
                {
                    remoteAddress = this.RemoteAddress.Uri.AbsoluteUri;
                }
                TD.ClientOperationPrepared(rpc.EventTraceActivity,
                                            rpc.Action,
                                            this.clientRuntime.ContractName,
                                            remoteAddress,
                                            previousId);
            }

        }

        internal static IAsyncResult BeginCall(ServiceChannel channel, ProxyOperationRuntime operation, object[] ins, AsyncCallback callback, object asyncState)
        {
            Fx.Assert(channel != null, "'channel' MUST NOT be NULL.");
            Fx.Assert(operation != null, "'operation' MUST NOT be NULL.");
            return channel.BeginCall(operation.Action, operation.IsOneWay, operation, ins, channel.operationTimeout, callback, asyncState);
        }

        internal IAsyncResult BeginCall(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, AsyncCallback callback, object asyncState)
        {
            return this.BeginCall(action, oneway, operation, ins, this.operationTimeout, callback, asyncState);
        }

        internal IAsyncResult BeginCall(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, TimeSpan timeout, AsyncCallback callback, object asyncState)
        {
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfIdleAborted(operation);
            this.ThrowIfIsConnectionOpened(operation);

            ServiceModelActivity serviceModelActivity = null;

            if (DiagnosticUtility.ShouldUseActivity)
            {
                serviceModelActivity = ServiceModelActivity.CreateActivity(true);
                callback = TraceUtility.WrapExecuteUserCodeAsyncCallback(callback);
            }

            SendAsyncResult result;

            using (Activity boundOperation = ServiceModelActivity.BoundOperation(serviceModelActivity, true))
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(serviceModelActivity, SR.GetString(SR.ActivityProcessAction, action), ActivityType.ProcessAction);
                }

                result = new SendAsyncResult(this, operation, action, ins, oneway, timeout, callback, asyncState);
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    result.Rpc.Activity = serviceModelActivity;
                }

                TraceServiceChannelCallStart(result.Rpc.EventTraceActivity, false);

                result.Begin();
            }

            return result;
        }

        internal object Call(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, object[] outs)
        {
            return this.Call(action, oneway, operation, ins, outs, this.operationTimeout);
        }

        internal object Call(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, object[] outs, TimeSpan timeout)
        {
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfIdleAborted(operation);
            this.ThrowIfIsConnectionOpened(operation);

            ProxyRpc rpc = new ProxyRpc(this, operation, action, ins, timeout);

            TraceServiceChannelCallStart(rpc.EventTraceActivity, true);

            using (rpc.Activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(rpc.Activity, SR.GetString(SR.ActivityProcessAction, action), ActivityType.ProcessAction);
                }

                this.PrepareCall(operation, oneway, ref rpc);

                if (!this.explicitlyOpened)
                {
                    this.EnsureDisplayUI();
                    this.EnsureOpened(rpc.TimeoutHelper.RemainingTime());
                }
                else
                {
                    this.ThrowIfOpening();
                    this.ThrowIfDisposedOrNotOpen();
                }

                try
                {
                    ConcurrencyBehavior.UnlockInstanceBeforeCallout(OperationContext.Current);

                    if (oneway)
                    {
                        this.binder.Send(rpc.Request, rpc.TimeoutHelper.RemainingTime());
                    }
                    else
                    {
                        rpc.Reply = this.binder.Request(rpc.Request, rpc.TimeoutHelper.RemainingTime());

                        if (rpc.Reply == null)
                        {
                            this.ThrowIfFaulted();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SFxServerDidNotReply)));
                        }
                    }
                }
                finally
                {
                    this.CompletedIOOperation();
                    CallOnceManager.SignalNextIfNonNull(this.autoOpenManager);
                    ConcurrencyBehavior.LockInstanceAfterCallout(OperationContext.Current);
                }

                rpc.OutputParameters = outs;
                this.HandleReply(operation, ref rpc);
            }
            return rpc.ReturnValue;
        }

        internal object EndCall(string action, object[] outs, IAsyncResult result)
        {
            SendAsyncResult sendResult = result as SendAsyncResult;
            if (sendResult == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxInvalidCallbackIAsyncResult)));

            using (ServiceModelActivity rpcActivity = sendResult.Rpc.Activity)
            {
                using (ServiceModelActivity.BoundOperation(rpcActivity, true))
                {
                    if (sendResult.Rpc.Activity != null && DiagnosticUtility.ShouldUseActivity)
                    {
                        sendResult.Rpc.Activity.Resume();
                    }
                    if (sendResult.Rpc.Channel != this)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.AsyncEndCalledOnWrongChannel));

                    if (action != MessageHeaders.WildcardAction && action != sendResult.Rpc.Action)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.AsyncEndCalledWithAnIAsyncResult));

                    SendAsyncResult.End(sendResult);

                    sendResult.Rpc.OutputParameters = outs;
                    this.HandleReply(sendResult.Rpc.Operation, ref sendResult.Rpc);

                    if (sendResult.Rpc.Activity != null)
                    {
                        sendResult.Rpc.Activity = null;
                    }
                    return sendResult.Rpc.ReturnValue;
                }
            }
        }

        internal void DecrementActivity()
        {
            int updatedActivityCount = Interlocked.Decrement(ref this.activityCount);

            if (!((updatedActivityCount >= 0)))
            {
                throw Fx.AssertAndThrowFatal("ServiceChannel.DecrementActivity: (updatedActivityCount >= 0)");
            }

            if (updatedActivityCount == 0 && this.autoClose)
            {
                try
                {
                    if (this.State == CommunicationState.Opened)
                    {
                        if (this.IsClient)
                        {
                            ISessionChannel<IDuplexSession> duplexSessionChannel = this.InnerChannel as ISessionChannel<IDuplexSession>;
                            if (duplexSessionChannel != null)
                            {
                                this.hasChannelStartedAutoClosing = true;
                                duplexSessionChannel.Session.CloseOutputSession(this.CloseTimeout);
                            }
                        }
                        else
                        {
                            this.Close(this.CloseTimeout);
                        }
                    }
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
                catch (ObjectDisposedException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (InvalidOperationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
            }
        }

        internal void FireUnknownMessageReceived(Message message)
        {
            EventHandler<UnknownMessageReceivedEventArgs> handler = this.unknownMessageReceived;
            if (handler != null)
                handler(this.proxy, new UnknownMessageReceivedEventArgs(message));
        }

        TimeoutException GetOpenTimeoutException(TimeSpan timeout)
        {
            EndpointAddress address = this.RemoteAddress ?? this.LocalAddress;
            if (address != null)
            {
                return new TimeoutException(SR.GetString(SR.TimeoutServiceChannelConcurrentOpen2, address, timeout));
            }
            else
            {
                return new TimeoutException(SR.GetString(SR.TimeoutServiceChannelConcurrentOpen1, timeout));
            }
        }

        internal void HandleReceiveComplete(RequestContext context)
        {
            if (context == null && HasSession)
            {
                bool first;
                lock (this.ThisLock)
                {
                    first = !this.doneReceiving;
                    this.doneReceiving = true;
                }

                if (first)
                {
                    DispatchRuntime dispatchBehavior = this.ClientRuntime.DispatchRuntime;
                    if (dispatchBehavior != null)
                        dispatchBehavior.GetRuntime().InputSessionDoneReceiving(this);

                    this.DecrementActivity();
                }
            }
        }

        void HandleReply(ProxyOperationRuntime operation, ref ProxyRpc rpc)
        {
            try
            {
                //set the ID after response
                if (TraceUtility.MessageFlowTracingOnly && rpc.ActivityId != Guid.Empty)
                {
                    System.Runtime.Diagnostics.DiagnosticTraceBase.ActivityId = rpc.ActivityId;
                }

                if (rpc.Reply != null)
                {
                    TraceUtility.MessageFlowAtMessageReceived(rpc.Reply, null, rpc.EventTraceActivity, false);

                    if (MessageLogger.LogMessagesAtServiceLevel)
                    {
                        MessageLogger.LogMessage(ref rpc.Reply, MessageLoggingSource.ServiceLevelReceiveReply | MessageLoggingSource.LastChance);
                    }
                    operation.Parent.AfterReceiveReply(ref rpc);

                    if ((operation.ReplyAction != MessageHeaders.WildcardAction) && !rpc.Reply.IsFault && rpc.Reply.Headers.Action != null)
                    {
                        if (String.CompareOrdinal(operation.ReplyAction, rpc.Reply.Headers.Action) != 0)
                        {
                            Exception error = new ProtocolException(SR.GetString(SR.SFxReplyActionMismatch3,
                                                                                  operation.Name,
                                                                                  rpc.Reply.Headers.Action,
                                                                                  operation.ReplyAction));
                            this.TerminateIfNecessary(ref rpc);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                        }
                    }
                    if (operation.DeserializeReply && clientRuntime.IsFault(ref rpc.Reply))
                    {
                        MessageFault fault = MessageFault.CreateFault(rpc.Reply, this.clientRuntime.MaxFaultSize);
                        string action = rpc.Reply.Headers.Action;
                        if (action == rpc.Reply.Version.Addressing.DefaultFaultAction)
                        {
                            action = null;
                        }
                        ThrowIfFaultUnderstood(rpc.Reply, fault, action, rpc.Reply.Version, rpc.Channel.GetProperty<FaultConverter>());
                        FaultException fe = rpc.Operation.FaultFormatter.Deserialize(fault, action);
                        this.TerminateIfNecessary(ref rpc);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(fe);
                    }

                    operation.AfterReply(ref rpc);
                }
            }
            finally
            {
                if (operation.SerializeRequest)
                {
                    rpc.Request.Close();
                }

                OperationContext operationContext = OperationContext.Current;
                bool consumed = ((rpc.Reply != null) && (rpc.Reply.State != MessageState.Created));

                if ((operationContext != null) && operationContext.IsUserContext)
                {
                    operationContext.SetClientReply(rpc.Reply, consumed);
                }
                else if (consumed)
                {
                    rpc.Reply.Close();
                }

                if (TraceUtility.MessageFlowTracingOnly)
                {
                    if (rpc.ActivityId != Guid.Empty)
                    {
                        //reset the ID as it was created internally - ensures each call is uniquely correlatable
                        System.Runtime.Diagnostics.DiagnosticTraceBase.ActivityId = Guid.Empty;
                        rpc.ActivityId = Guid.Empty;
                    }
                }
            }
            this.TerminateIfNecessary(ref rpc);

            if (TD.ServiceChannelCallStopIsEnabled())
            {
                string remoteAddress = string.Empty;
                if (this.RemoteAddress != null && this.RemoteAddress.Uri != null)
                {
                    remoteAddress = this.RemoteAddress.Uri.AbsoluteUri;
                }
                TD.ServiceChannelCallStop(rpc.EventTraceActivity, rpc.Action,
                                            this.clientRuntime.ContractName,
                                            remoteAddress);
            }

        }

        void TerminateIfNecessary(ref ProxyRpc rpc)
        {
            if (rpc.Operation.IsTerminating)
            {
                this.terminatingOperationName = rpc.Operation.Name;
                TerminatingOperationBehavior.AfterReply(ref rpc);
            }
        }

        void ThrowIfFaultUnderstood(Message reply, MessageFault fault, string action, MessageVersion version, FaultConverter faultConverter)
        {
            Exception exception;
            if (faultConverter != null && faultConverter.TryCreateException(reply, fault, out exception))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(exception);
            }

            bool checkSender;
            bool checkReceiver;
            FaultCode code;

            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                checkSender = true;
                checkReceiver = true;
                code = fault.Code;
            }
            else
            {
                checkSender = fault.Code.IsSenderFault;
                checkReceiver = fault.Code.IsReceiverFault;
                code = fault.Code.SubCode;
            }

            if (code == null)
            {
                return;
            }

            if (code.Namespace == null)
            {
                return;
            }

            if (checkSender)
            {
                if (string.Compare(code.Namespace, FaultCodeConstants.Namespaces.NetDispatch, StringComparison.Ordinal) == 0)
                {
                    if (string.Compare(code.Name, FaultCodeConstants.Codes.SessionTerminated, StringComparison.Ordinal) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ChannelTerminatedException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                    }

                    if (string.Compare(code.Name, FaultCodeConstants.Codes.TransactionAborted, StringComparison.Ordinal) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                    }
                }

                // throw SecurityAccessDeniedException explicitly
                if (string.Compare(code.Namespace, SecurityVersion.Default.HeaderNamespace.Value, StringComparison.Ordinal) == 0)
                {
                    if (string.Compare(code.Name, SecurityVersion.Default.FailedAuthenticationFaultCode.Value, StringComparison.Ordinal) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityAccessDeniedException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                    }
                }
            }

            if (checkReceiver)
            {
                if (string.Compare(code.Namespace, FaultCodeConstants.Namespaces.NetDispatch, StringComparison.Ordinal) == 0)
                {
                    if (string.Compare(code.Name, FaultCodeConstants.Codes.InternalServiceFault, StringComparison.Ordinal) == 0)
                    {
                        if (this.HasSession)
                        {
                            this.Fault();
                        }
                        if (fault.HasDetail)
                        {
                            ExceptionDetail detail = fault.GetDetail<ExceptionDetail>();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new FaultException<ExceptionDetail>(detail, fault.Reason, fault.Code, action));
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new FaultException(fault, action));
                    }
                    if (string.Compare(code.Name, FaultCodeConstants.Codes.DeserializationFailed, StringComparison.Ordinal) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ProtocolException(
                            fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                    }
                }
            }
        }

        void ThrowIfIdleAborted(ProxyOperationRuntime operation)
        {
            if (this.idleManager != null && this.idleManager.DidIdleAbort)
            {
                string text = SR.GetString(SR.SFxServiceChannelIdleAborted, operation.Name);
                Exception error = new CommunicationObjectAbortedException(text);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }
        }

        void ThrowIfIsConnectionOpened(ProxyOperationRuntime operation)
        {
            if (operation.IsSessionOpenNotificationEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxServiceChannelCannotBeCalledBecauseIsSessionOpenNotificationEnabled, operation.Name, "Action", OperationDescription.SessionOpenedAction, "Open")));
            }
        }

        void ThrowIfInitializationUINotCalled()
        {
            if (!this.didInteractiveInitialization && (this.ClientRuntime.InteractiveChannelInitializers.Count > 0))
            {
                IInteractiveChannelInitializer example = this.ClientRuntime.InteractiveChannelInitializers[0];
                string text = SR.GetString(SR.SFxInitializationUINotCalled, example.GetType().ToString());
                Exception error = new InvalidOperationException(text);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }
        }

        void ThrowIfDisallowedInitializationUI()
        {
            if (!this.allowInitializationUI)
            {
                this.ThrowIfDisallowedInitializationUICore();
            }
        }

        void ThrowIfDisallowedInitializationUICore()
        {
            if (this.ClientRuntime.InteractiveChannelInitializers.Count > 0)
            {
                IInteractiveChannelInitializer example = this.ClientRuntime.InteractiveChannelInitializers[0];
                string text = SR.GetString(SR.SFxInitializationUIDisallowed, example.GetType().ToString());
                Exception error = new InvalidOperationException(text);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }
        }

        void ThrowIfOpening()
        {
            if (this.State == CommunicationState.Opening)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCannotCallAutoOpenWhenExplicitOpenCalled)));
            }
        }

        internal void IncrementActivity()
        {
            Interlocked.Increment(ref this.activityCount);
        }

        void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            this.Fault();

            if (this.HasSession)
            {
                DispatchRuntime dispatchRuntime = this.ClientRuntime.DispatchRuntime;
                if (dispatchRuntime != null)
                {
                    dispatchRuntime.GetRuntime().InputSessionFaulted(this);
                }
            }

            if (this.autoClose && !this.IsClient)
            {
                this.Abort();
            }
        }

        void AddMessageProperties(Message message, OperationContext context)
        {
            if (this.allowOutputBatching)
            {
                message.Properties.AllowOutputBatching = true;
            }

            if (context != null && context.InternalServiceChannel == this)
            {
                if (!context.OutgoingMessageVersion.IsMatch(message.Headers.MessageVersion))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxVersionMismatchInOperationContextAndMessage2, context.OutgoingMessageVersion, message.Headers.MessageVersion)
                        ));
                }

                if (context.HasOutgoingMessageHeaders)
                {
                    message.Headers.CopyHeadersFrom(context.OutgoingMessageHeaders);
                }

                if (context.HasOutgoingMessageProperties)
                {
                    message.Properties.CopyProperties(context.OutgoingMessageProperties);
                }
            }
        }

        #region IChannel Members
        public void Send(Message message)
        {
            this.Send(message, this.OperationTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            ProxyOperationRuntime operation = UnhandledProxyOperation;
            this.Call(message.Headers.Action, true, operation, new object[] { message }, EmptyArray<object>.Instance, timeout);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.OperationTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ProxyOperationRuntime operation = UnhandledProxyOperation;
            return this.BeginCall(message.Headers.Action, true, operation, new object[] { message }, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.EndCall(MessageHeaders.WildcardAction, EmptyArray<object>.Instance, result);
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.OperationTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            ProxyOperationRuntime operation = UnhandledProxyOperation;
            return (Message)this.Call(message.Headers.Action, false, operation, new object[] { message }, EmptyArray<object>.Instance, timeout);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, this.OperationTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ProxyOperationRuntime operation = this.UnhandledProxyOperation;
            return this.BeginCall(message.Headers.Action, false, operation, new object[] { message }, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return (Message)this.EndCall(MessageHeaders.WildcardAction, EmptyArray<object>.Instance, result);
        }

        protected override void OnAbort()
        {
            if (this.idleManager != null)
            {
                this.idleManager.CancelTimer();
            }

            this.binder.Abort();

            if (this.factory != null)
                this.factory.ChannelDisposed(this);

            if (this.closeFactory)
            {
                if (this.factory != null)
                    this.factory.Abort();
            }

            CleanupChannelCollections();

            ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle != null)
                serviceThrottle.DeactivateChannel();

            //rollback the attached transaction if one is present
            if ((this.instanceContext != null) && this.HasSession)
            {
                if (instanceContext.HasTransaction)
                {
                    instanceContext.Transaction.CompletePendingTransaction(instanceContext.Transaction.Attached, new Exception()); // error!=null forces Tx rollback
                }
            }

            DecrementBusyCount();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.idleManager != null)
            {
                this.idleManager.CancelTimer();
            }

            if (this.factory != null)
            {
                this.factory.ChannelDisposed(this);
            }

            if (this.InstanceContext != null && this.InstanceContext.HasTransaction)
            {
                this.InstanceContext.CompleteAttachedTransaction();
            }

            if (this.closeBinder)
            {
                if (this.closeFactory)
                {
                    return new ChainedAsyncResult(timeout, callback, state,
                        new ChainedBeginHandler(this.InnerChannel.BeginClose), new ChainedEndHandler(this.InnerChannel.EndClose),
                        new ChainedBeginHandler(this.factory.BeginClose), new ChainedEndHandler(this.factory.EndClose));
                }
                else
                {
                    return this.InnerChannel.BeginClose(timeout, callback, state);
                }
            }
            else
            {
                if (this.closeFactory)
                    return factory.BeginClose(timeout, callback, state);
                else
                    return new CompletedAsyncResult(callback, state);
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfInitializationUINotCalled();

            if (this.autoOpenManager == null)
            {
                this.explicitlyOpened = true;
            }

            if (this.HasSession && !this.IsClient)
            {
                IncrementBusyCount();
            }

            this.TraceChannelOpenStarted();

            if (this.openBinder)
            {
                return this.InnerChannel.BeginOpen(timeout, callback, state);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            if (this.idleManager != null)
            {
                this.idleManager.CancelTimer();
            }

            if (this.factory != null)
            {
                this.factory.ChannelDisposed(this);
            }

            if (this.InstanceContext != null && this.InstanceContext.HasTransaction)
            {
                this.InstanceContext.CompleteAttachedTransaction();
            }

            if (this.closeBinder)
                this.InnerChannel.Close(timeoutHelper.RemainingTime());

            if (this.closeFactory)
                this.factory.Close(timeoutHelper.RemainingTime());

            CleanupChannelCollections();

            ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle != null)
            {
                serviceThrottle.DeactivateChannel();
            }

            DecrementBusyCount();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            if (this.closeBinder)
            {
                if (this.closeFactory)
                    ChainedAsyncResult.End(result);
                else
                    this.InnerChannel.EndClose(result);
            }
            else
            {
                if (this.closeFactory)
                    factory.EndClose(result);
                else
                    CompletedAsyncResult.End(result);
            }

            CleanupChannelCollections();

            ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle != null)
            {
                serviceThrottle.DeactivateChannel();
            }

            DecrementBusyCount();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (this.openBinder)
                InnerChannel.EndOpen(result);
            else
                CompletedAsyncResult.End(result);
            this.BindDuplexCallbacks();
            this.CompletedIOOperation();

            this.TraceChannelOpenCompleted();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfInitializationUINotCalled();

            if (this.autoOpenManager == null)
            {
                this.explicitlyOpened = true;
            }

            if (this.HasSession && !this.IsClient)
            {
                IncrementBusyCount();
            }

            this.TraceChannelOpenStarted();

            if (this.openBinder)
            {
                this.InnerChannel.Open(timeout);
            }

            this.BindDuplexCallbacks();
            this.CompletedIOOperation();

            this.TraceChannelOpenCompleted();
        }

        void CleanupChannelCollections()
        {
            if (!this.hasCleanedUpChannelCollections)
            {
                lock (this.ThisLock)
                {
                    if (!this.hasCleanedUpChannelCollections)
                    {
                        if (this.InstanceContext != null)
                        {
                            this.InstanceContext.OutgoingChannels.Remove((IChannel)this.proxy);
                        }

                        if (this.WmiInstanceContext != null)
                        {
                            this.WmiInstanceContext.WmiChannels.Remove((IChannel)this.proxy);
                        }

                        this.hasCleanedUpChannelCollections = true;
                    }
                }
            }
        }

        void IncrementBusyCount()
        {
            lock (this.ThisLock)
            {
                if (this.State == CommunicationState.Opening)
                {
                    AspNetEnvironment.Current.IncrementBusyCount();
                    if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceIncrementBusyCount(this.GetType().FullName);
                    }
                    this.hasIncrementedBusyCount = true;
                }
            }
        }

        void DecrementBusyCount()
        {
            lock (this.ThisLock)
            {
                if (this.hasIncrementedBusyCount)
                {
                    AspNetEnvironment.Current.DecrementBusyCount();
                    if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceDecrementBusyCount(this.GetType().FullName);
                    }
                    this.hasIncrementedBusyCount = false;
                }
            }
        }

        #endregion

        #region IClientChannel Members

        bool IDuplexContextChannel.AutomaticInputSessionShutdown
        {
            get { return this.autoClose; }
            set { this.autoClose = value; }
        }

        bool IClientChannel.AllowInitializationUI
        {
            get { return this.allowInitializationUI; }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.allowInitializationUI = value;
            }
        }

        bool IContextChannel.AllowOutputBatching
        {
            get { return this.allowOutputBatching; }
            set { this.allowOutputBatching = value; }
        }

        bool IClientChannel.DidInteractiveInitialization
        {
            get { return this.didInteractiveInitialization; }
        }

        IAsyncResult IDuplexContextChannel.BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return GetDuplexSessionOrThrow().BeginCloseOutputSession(timeout, callback, state);
        }

        void IDuplexContextChannel.EndCloseOutputSession(IAsyncResult result)
        {
            GetDuplexSessionOrThrow().EndCloseOutputSession(result);
        }

        void IDuplexContextChannel.CloseOutputSession(TimeSpan timeout)
        {
            GetDuplexSessionOrThrow().CloseOutputSession(timeout);
        }

        IDuplexSession GetDuplexSessionOrThrow()
        {
            if (this.InnerChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.channelIsNotAvailable0)));
            }

            ISessionChannel<IDuplexSession> duplexSessionChannel = this.InnerChannel as ISessionChannel<IDuplexSession>;
            if (duplexSessionChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.channelDoesNotHaveADuplexSession0)));
            }

            return duplexSessionChannel.Session;
        }

        IExtensionCollection<IContextChannel> IExtensibleObject<IContextChannel>.Extensions
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.extensions == null)
                        this.extensions = new ExtensionCollection<IContextChannel>((IContextChannel)this.Proxy, this.ThisLock);
                    return this.extensions;
                }
            }
        }

        InstanceContext IDuplexContextChannel.CallbackInstance
        {
            get { return this.instanceContext; }
            set
            {
                lock (this.ThisLock)
                {
                    if (this.instanceContext != null)
                    {
                        this.instanceContext.OutgoingChannels.Remove((IChannel)this.proxy);
                    }

                    this.instanceContext = value;

                    if (this.instanceContext != null)
                    {
                        this.instanceContext.OutgoingChannels.Add((IChannel)this.proxy);
                    }
                }
            }
        }

        IInputSession IContextChannel.InputSession
        {
            get
            {
                if (this.InnerChannel != null)
                {
                    ISessionChannel<IInputSession> inputSession = this.InnerChannel as ISessionChannel<IInputSession>;
                    if (inputSession != null)
                        return inputSession.Session;

                    ISessionChannel<IDuplexSession> duplexSession = this.InnerChannel as ISessionChannel<IDuplexSession>;
                    if (duplexSession != null)
                        return duplexSession.Session;
                }

                return null;
            }
        }

        IOutputSession IContextChannel.OutputSession
        {
            get
            {
                if (this.InnerChannel != null)
                {
                    ISessionChannel<IOutputSession> outputSession = this.InnerChannel as ISessionChannel<IOutputSession>;
                    if (outputSession != null)
                        return outputSession.Session;

                    ISessionChannel<IDuplexSession> duplexSession = this.InnerChannel as ISessionChannel<IDuplexSession>;
                    if (duplexSession != null)
                        return duplexSession.Session;
                }

                return null;
            }
        }

        string IContextChannel.SessionId
        {
            get
            {
                if (this.InnerChannel != null)
                {
                    ISessionChannel<IInputSession> inputSession = this.InnerChannel as ISessionChannel<IInputSession>;
                    if (inputSession != null)
                        return inputSession.Session.Id;

                    ISessionChannel<IOutputSession> outputSession = this.InnerChannel as ISessionChannel<IOutputSession>;
                    if (outputSession != null)
                        return outputSession.Session.Id;

                    ISessionChannel<IDuplexSession> duplexSession = this.InnerChannel as ISessionChannel<IDuplexSession>;
                    if (duplexSession != null)
                        return duplexSession.Session.Id;
                }

                return null;
            }
        }

        event EventHandler<UnknownMessageReceivedEventArgs> IClientChannel.UnknownMessageReceived
        {
            add
            {
                lock (this.ThisLock)
                {
                    this.unknownMessageReceived += value;
                }
            }
            remove
            {
                lock (this.ThisLock)
                {
                    this.unknownMessageReceived -= value;
                }
            }
        }

        public void DisplayInitializationUI()
        {
            this.ThrowIfDisallowedInitializationUI();

            if (this.autoDisplayUIManager == null)
            {
                this.explicitlyOpened = true;
            }

            this.ClientRuntime.GetRuntime().DisplayInitializationUI(this);
            this.didInteractiveInitialization = true;
        }

        public IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state)
        {
            this.ThrowIfDisallowedInitializationUI();

            if (this.autoDisplayUIManager == null)
            {
                this.explicitlyOpened = true;
            }

            return this.ClientRuntime.GetRuntime().BeginDisplayInitializationUI(this, callback, state);
        }

        public void EndDisplayInitializationUI(IAsyncResult result)
        {
            this.ClientRuntime.GetRuntime().EndDisplayInitializationUI(result);
            this.didInteractiveInitialization = true;
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        #endregion

        void TraceChannelOpenStarted()
        {
            if (TD.ClientChannelOpenStartIsEnabled() && this.endpointDispatcher == null)
            {
                TD.ClientChannelOpenStart(this.EventActivity);
            }
            else if (TD.ServiceChannelOpenStartIsEnabled())
            {
                TD.ServiceChannelOpenStart(this.EventActivity);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> values = new Dictionary<string, string>(4);
                bool traceNeeded = false;
                DispatchRuntime behavior = this.DispatchRuntime;
                if (behavior != null)
                {
                    if (behavior.Type != null)
                    {
                        values["ServiceType"] = behavior.Type.AssemblyQualifiedName;
                    }
                    values["ContractNamespace"] = this.clientRuntime.ContractNamespace;
                    values["ContractName"] = this.clientRuntime.ContractName;
                    traceNeeded = true;
                }
                if ((this.endpointDispatcher != null) && (this.endpointDispatcher.ListenUri != null))
                {
                    values["Uri"] = this.endpointDispatcher.ListenUri.ToString();
                    traceNeeded = true;
                }
                if (traceNeeded)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ServiceChannelLifetime,
                        SR.GetString(SR.TraceCodeServiceChannelLifetime),
                        new DictionaryTraceRecord(values), this, null);
                }
            }            
        }

        void TraceChannelOpenCompleted()
        {
            if (this.endpointDispatcher == null && TD.ClientChannelOpenStopIsEnabled())
            {
                TD.ClientChannelOpenStop(this.EventActivity);
            }
            else if (TD.ServiceChannelOpenStopIsEnabled())
            {
                TD.ServiceChannelOpenStop(this.EventActivity);
            }
        }

        static void TraceServiceChannelCallStart(EventTraceActivity eventTraceActivity, bool isSynchronous)
        {
            if (TD.ServiceChannelCallStartIsEnabled())
            {
                if (isSynchronous)
                {
                    TD.ServiceChannelCallStart(eventTraceActivity);
                }
                else
                {
                    TD.ServiceChannelBeginCallStart(eventTraceActivity);
                }
            }
        }

        // Invariants for signalling the CallOnce manager.
        //
        // 1) If a Call, BeginCall, or EndCall on the channel throws,
        //    the manager will SignalNext itself.
        // 2) If a Waiter times out, it will SignalNext its manager
        //    once it is both timed out and signalled.
        // 3) Once Call or EndCall returns successfully, it guarantees
        //    that SignalNext will be called once the // next stage
        //    has sufficiently completed.

        class SendAsyncResult : TraceAsyncResult
        {
            readonly bool isOneWay;
            readonly ProxyOperationRuntime operation;
            internal ProxyRpc Rpc;
            OperationContext operationContext;

            static AsyncCallback ensureInteractiveInitCallback = Fx.ThunkCallback(EnsureInteractiveInitCallback);
            static AsyncCallback ensureOpenCallback = Fx.ThunkCallback(EnsureOpenCallback);
            static AsyncCallback sendCallback = Fx.ThunkCallback(SendCallback);

            internal SendAsyncResult(ServiceChannel channel, ProxyOperationRuntime operation,
                                     string action, object[] inputParameters, bool isOneWay, TimeSpan timeout,
                                     AsyncCallback userCallback, object userState)
                : base(userCallback, userState)
            {
                this.Rpc = new ProxyRpc(channel, operation, action, inputParameters, timeout);
                this.isOneWay = isOneWay;
                this.operation = operation;
                this.operationContext = OperationContext.Current;
            }

            internal void Begin()
            {
                this.Rpc.Channel.PrepareCall(this.operation, this.isOneWay, ref this.Rpc);

                if (this.Rpc.Channel.explicitlyOpened)
                {
                    this.Rpc.Channel.ThrowIfOpening();
                    this.Rpc.Channel.ThrowIfDisposedOrNotOpen();
                    this.StartSend(true);
                }
                else
                {
                    this.StartEnsureInteractiveInit();
                }
            }

            void StartEnsureInteractiveInit()
            {
                IAsyncResult result = this.Rpc.Channel.BeginEnsureDisplayUI(ensureInteractiveInitCallback, this);

                if (result.CompletedSynchronously)
                {
                    this.FinishEnsureInteractiveInit(result, true);
                }
            }

            static void EnsureInteractiveInitCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((SendAsyncResult)result.AsyncState).FinishEnsureInteractiveInit(result, false);
                }
            }

            void FinishEnsureInteractiveInit(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;

                try
                {
                    this.Rpc.Channel.EndEnsureDisplayUI(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.StartEnsureOpen(completedSynchronously);
                }
            }

            void StartEnsureOpen(bool completedSynchronously)
            {
                TimeSpan timeout = this.Rpc.TimeoutHelper.RemainingTime();
                IAsyncResult result = null;
                Exception exception = null;

                try
                {
                    result = this.Rpc.Channel.BeginEnsureOpened(timeout, ensureOpenCallback, this);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishEnsureOpen(result, completedSynchronously);
                }
            }

            static void EnsureOpenCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((SendAsyncResult)result.AsyncState).FinishEnsureOpen(result, false);
                }
            }

            void FinishEnsureOpen(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                using (ServiceModelActivity.BoundOperation(this.Rpc.Activity))
                {
                    try
                    {
                        this.Rpc.Channel.EndEnsureOpened(result);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e) || completedSynchronously)
                        {
                            throw;
                        }
                        exception = e;
                    }

                    if (exception != null)
                    {
                        this.CallComplete(completedSynchronously, exception);
                    }
                    else
                    {
                        this.StartSend(completedSynchronously);
                    }
                }
            }

            void StartSend(bool completedSynchronously)
            {
                TimeSpan timeout = this.Rpc.TimeoutHelper.RemainingTime();
                IAsyncResult result = null;
                Exception exception = null;

                try
                {
                    ConcurrencyBehavior.UnlockInstanceBeforeCallout(this.operationContext);

                    if (this.isOneWay)
                    {
                        result = this.Rpc.Channel.binder.BeginSend(this.Rpc.Request, timeout, sendCallback, this);
                    }
                    else
                    {
                        result = this.Rpc.Channel.binder.BeginRequest(this.Rpc.Request, timeout, sendCallback, this);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (completedSynchronously)
                    {
                        ConcurrencyBehavior.LockInstanceAfterCallout(this.operationContext);
                        throw;
                    }
                    exception = e;
                }
                finally
                {
                    CallOnceManager.SignalNextIfNonNull(this.Rpc.Channel.autoOpenManager);
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishSend(result, completedSynchronously);
                }
            }

            static void SendCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((SendAsyncResult)result.AsyncState).FinishSend(result, false);
                }
            }

            void FinishSend(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;

                try
                {
                    if (this.isOneWay)
                    {
                        this.Rpc.Channel.binder.EndSend(result);
                    }
                    else
                    {
                        this.Rpc.Reply = this.Rpc.Channel.binder.EndRequest(result);

                        if (this.Rpc.Reply == null)
                        {
                            this.Rpc.Channel.ThrowIfFaulted();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SFxServerDidNotReply)));
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (completedSynchronously)
                    {
                        ConcurrencyBehavior.LockInstanceAfterCallout(this.operationContext);
                        throw;
                    }
                    exception = e;
                }

                this.CallComplete(completedSynchronously, exception);
            }

            void CallComplete(bool completedSynchronously, Exception exception)
            {
                this.Rpc.Channel.CompletedIOOperation();
                this.Complete(completedSynchronously, exception);
            }

            public static void End(SendAsyncResult result)
            {
                try
                {
                    AsyncResult.End<SendAsyncResult>(result);
                }
                finally
                {
                    ConcurrencyBehavior.LockInstanceAfterCallout(result.operationContext);
                }
            }
        }

        interface ICallOnce
        {
            void Call(ServiceChannel channel, TimeSpan timeout);
            IAsyncResult BeginCall(ServiceChannel channel, TimeSpan timeout, AsyncCallback callback, object state);
            void EndCall(ServiceChannel channel, IAsyncResult result);
        }

        class CallDisplayUIOnce : ICallOnce
        {
            static CallDisplayUIOnce instance;

            internal static CallDisplayUIOnce Instance
            {
                get
                {
                    if (CallDisplayUIOnce.instance == null)
                    {
                        CallDisplayUIOnce.instance = new CallDisplayUIOnce();
                    }
                    return CallDisplayUIOnce.instance;
                }
            }

            [Conditional("DEBUG")]
            void ValidateTimeoutIsMaxValue(TimeSpan timeout)
            {
                if (timeout != TimeSpan.MaxValue)
                {
                    Fx.Assert("non-MaxValue timeout for displaying interactive initialization UI");
                }
            }

            void ICallOnce.Call(ServiceChannel channel, TimeSpan timeout)
            {
                this.ValidateTimeoutIsMaxValue(timeout);
                channel.DisplayInitializationUI();
            }

            IAsyncResult ICallOnce.BeginCall(ServiceChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.ValidateTimeoutIsMaxValue(timeout);
                return channel.BeginDisplayInitializationUI(callback, state);
            }

            void ICallOnce.EndCall(ServiceChannel channel, IAsyncResult result)
            {
                channel.EndDisplayInitializationUI(result);
            }
        }

        class CallOpenOnce : ICallOnce
        {
            static CallOpenOnce instance;

            internal static CallOpenOnce Instance
            {
                get
                {
                    if (CallOpenOnce.instance == null)
                    {
                        CallOpenOnce.instance = new CallOpenOnce();
                    }
                    return CallOpenOnce.instance;
                }
            }

            void ICallOnce.Call(ServiceChannel channel, TimeSpan timeout)
            {
                channel.Open(timeout);
            }

            IAsyncResult ICallOnce.BeginCall(ServiceChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginOpen(timeout, callback, state);
            }

            void ICallOnce.EndCall(ServiceChannel channel, IAsyncResult result)
            {
                channel.EndOpen(result);
            }
        }

        class CallOnceManager
        {
            readonly ICallOnce callOnce;
            readonly ServiceChannel channel;
            bool isFirst = true;
            Queue<IWaiter> queue;

            static Action<object> signalWaiter = new Action<object>(CallOnceManager.SignalWaiter);

            internal CallOnceManager(ServiceChannel channel, ICallOnce callOnce)
            {
                this.callOnce = callOnce;
                this.channel = channel;
                this.queue = new Queue<IWaiter>();
            }

            object ThisLock
            {
                get { return this; }
            }

            internal void CallOnce(TimeSpan timeout, CallOnceManager cascade)
            {
                SyncWaiter waiter = null;
                bool first = false;

                if (this.queue != null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.queue != null)
                        {
                            if (this.isFirst)
                            {
                                first = true;
                                this.isFirst = false;
                            }
                            else
                            {
                                waiter = new SyncWaiter(this);
                                this.queue.Enqueue(waiter);
                            }
                        }
                    }
                }

                CallOnceManager.SignalNextIfNonNull(cascade);

                if (first)
                {
                    bool throwing = true;
                    try
                    {
                        this.callOnce.Call(this.channel, timeout);
                        throwing = false;
                    }
                    finally
                    {
                        if (throwing)
                        {
                            this.SignalNext();
                        }
                    }
                }
                else if (waiter != null)
                {
                    waiter.Wait(timeout);
                }
            }

            internal IAsyncResult BeginCallOnce(TimeSpan timeout, CallOnceManager cascade,
                                                AsyncCallback callback, object state)
            {
                AsyncWaiter waiter = null;
                bool first = false;

                if (this.queue != null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.queue != null)
                        {
                            if (this.isFirst)
                            {
                                first = true;
                                this.isFirst = false;
                            }
                            else
                            {
                                waiter = new AsyncWaiter(this, timeout, callback, state);
                                this.queue.Enqueue(waiter);
                            }
                        }
                    }
                }

                CallOnceManager.SignalNextIfNonNull(cascade);

                if (first)
                {
                    bool throwing = true;
                    try
                    {
                        IAsyncResult result = this.callOnce.BeginCall(this.channel, timeout, callback, state);
                        throwing = false;
                        return result;
                    }
                    finally
                    {
                        if (throwing)
                        {
                            this.SignalNext();
                        }
                    }
                }
                else if (waiter != null)
                {
                    return waiter;
                }
                else
                {
                    return new CallOnceCompletedAsyncResult(callback, state);
                }
            }

            internal void EndCallOnce(IAsyncResult result)
            {
                if (result is CallOnceCompletedAsyncResult)
                {
                    CallOnceCompletedAsyncResult.End(result);
                }
                else if (result is AsyncWaiter)
                {
                    AsyncWaiter.End(result);
                }
                else
                {
                    bool throwing = true;
                    try
                    {
                        this.callOnce.EndCall(this.channel, result);
                        throwing = false;
                    }
                    finally
                    {
                        if (throwing)
                        {
                            this.SignalNext();
                        }
                    }
                }
            }

            static internal void SignalNextIfNonNull(CallOnceManager manager)
            {
                if (manager != null)
                {
                    manager.SignalNext();
                }
            }

            internal void SignalNext()
            {
                if (this.queue == null)
                {
                    return;
                }

                IWaiter waiter = null;

                lock (this.ThisLock)
                {
                    if (this.queue != null)
                    {
                        if (this.queue.Count > 0)
                        {
                            waiter = this.queue.Dequeue();
                        }
                        else
                        {
                            this.queue = null;
                        }
                    }
                }

                if (waiter != null)
                {
                    ActionItem.Schedule(CallOnceManager.signalWaiter, waiter);
                }
            }

            static void SignalWaiter(object state)
            {
                ((IWaiter)state).Signal();
            }

            interface IWaiter
            {
                void Signal();
            }

            class SyncWaiter : IWaiter
            {
                ManualResetEvent wait = new ManualResetEvent(false);
                CallOnceManager manager;
                bool isTimedOut = false;
                bool isSignaled = false;
                int waitCount = 0;

                internal SyncWaiter(CallOnceManager manager)
                {
                    this.manager = manager;
                }

                bool ShouldSignalNext
                {
                    get { return this.isTimedOut && this.isSignaled; }
                }

                void IWaiter.Signal()
                {
                    wait.Set();
                    this.CloseWaitHandle();

                    bool signalNext;
                    lock (this.manager.ThisLock)
                    {
                        this.isSignaled = true;
                        signalNext = this.ShouldSignalNext;
                    }
                    if (signalNext)
                    {
                        this.manager.SignalNext();
                    }
                }

                internal bool Wait(TimeSpan timeout)
                {
                    try
                    {
                        if (!TimeoutHelper.WaitOne(this.wait, timeout))
                        {
                            bool signalNext;
                            lock (this.manager.ThisLock)
                            {
                                this.isTimedOut = true;
                                signalNext = this.ShouldSignalNext;
                            }
                            if (signalNext)
                            {
                                this.manager.SignalNext();
                            }
                        }
                    }
                    finally
                    {
                        this.CloseWaitHandle();
                    }

                    return !this.isTimedOut;
                }

                void CloseWaitHandle()
                {
                    if (Interlocked.Increment(ref this.waitCount) == 2)
                    {
                        this.wait.Close();
                    }
                }
            }

            class AsyncWaiter : AsyncResult, IWaiter
            {
                static Action<object> timerCallback = new Action<object>(AsyncWaiter.TimerCallback);

                CallOnceManager manager;
                TimeSpan timeout;
                IOThreadTimer timer;

                internal AsyncWaiter(CallOnceManager manager, TimeSpan timeout,
                                     AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.manager = manager;
                    this.timeout = timeout;

                    if (timeout != TimeSpan.MaxValue)
                    {
                        this.timer = new IOThreadTimer(timerCallback, this, false);
                        this.timer.Set(timeout);
                    }
                }

                internal static void End(IAsyncResult result)
                {
                    AsyncResult.End<AsyncWaiter>(result);
                }

                void IWaiter.Signal()
                {
                    if ((this.timer == null) || this.timer.Cancel())
                    {
                        this.Complete(false);
                        this.manager.channel.Closed -= this.OnClosed;
                    }
                    else
                    {
                        this.manager.SignalNext();
                    }
                }

                void OnClosed(object sender, EventArgs e)
                {
                    if ((this.timer == null) || this.timer.Cancel())
                    {
                        this.Complete(false, this.manager.channel.CreateClosedException());
                    }
                }

                static void TimerCallback(object state)
                {
                    AsyncWaiter _this = (AsyncWaiter)state;
                    _this.Complete(false, _this.manager.channel.GetOpenTimeoutException(_this.timeout));
                }
            }
        }

        class CallOnceCompletedAsyncResult : AsyncResult
        {
            internal CallOnceCompletedAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.Complete(true);
            }

            static internal void End(IAsyncResult result)
            {
                AsyncResult.End<CallOnceCompletedAsyncResult>(result);
            }
        }

        internal class SessionIdleManager
        {
            readonly IChannelBinder binder;
            ServiceChannel channel;
            readonly long idleTicks;
            long lastActivity;
            readonly IOThreadTimer timer;
            static Action<object> timerCallback;
            bool didIdleAbort;
            bool isTimerCancelled;
            object thisLock;

            SessionIdleManager(IChannelBinder binder, TimeSpan idle)
            {
                this.binder = binder;
                this.timer = new IOThreadTimer(GetTimerCallback(), this, false);
                this.idleTicks = Ticks.FromTimeSpan(idle);
                this.timer.SetAt(Ticks.Now + this.idleTicks);
                this.thisLock = new Object();
            }

            internal static SessionIdleManager CreateIfNeeded(IChannelBinder binder, TimeSpan idle)
            {
                if (binder.HasSession && (idle != TimeSpan.MaxValue))
                {
                    return new SessionIdleManager(binder, idle);
                }
                else
                {
                    return null;
                }
            }

            internal bool DidIdleAbort
            {
                get
                {
                    lock (thisLock)
                    {
                        return this.didIdleAbort;
                    }
                }
            }

            internal void CancelTimer()
            {
                lock (thisLock)
                {
                    this.isTimerCancelled = true;
                    this.timer.Cancel();
                }
            }

            internal void CompletedActivity()
            {
                Interlocked.Exchange(ref this.lastActivity, Ticks.Now);
            }

            internal void RegisterChannel(ServiceChannel channel, out bool didIdleAbort)
            {
                lock (thisLock)
                {
                    this.channel = channel;
                    didIdleAbort = this.didIdleAbort;
                }
            }

            static Action<object> GetTimerCallback()
            {
                if (SessionIdleManager.timerCallback == null)
                {
                    SessionIdleManager.timerCallback = SessionIdleManager.TimerCallback;
                }
                return SessionIdleManager.timerCallback;
            }

            static void TimerCallback(object state)
            {
                ((SessionIdleManager)state).TimerCallback();
            }

            void TimerCallback()
            {
                // This reads lastActivity atomically without changing its value.
                // (it only sets if it is zero, and then it sets it to zero).
                long last = Interlocked.CompareExchange(ref this.lastActivity, 0, 0);
                long abortTime = last + this.idleTicks;

                lock (thisLock)
                {
                    if (Ticks.Now > abortTime)
                    {
                        if (TD.SessionIdleTimeoutIsEnabled())
                        {
                            string listenUri = string.Empty;
                            if (this.binder.ListenUri != null)
                            {
                                listenUri = this.binder.ListenUri.AbsoluteUri;                                
                            }

                            TD.SessionIdleTimeout(listenUri); 
                        }

                        this.didIdleAbort = true;
                        if (this.channel != null)
                        {
                            this.channel.Abort();
                        }
                        else
                        {
                            this.binder.Abort();
                        }
                    }
                    else
                    {
                        if (!this.isTimerCancelled && binder.Channel.State != CommunicationState.Faulted && binder.Channel.State != CommunicationState.Closed)
                        {
                            this.timer.SetAt(abortTime);
                        }
                    }
                }
            }
        }
    }
}
