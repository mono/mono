//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.ServiceProcess;
    using System.Threading;
    using Utility = System.ServiceModel.Activation.Utility;

    sealed class SharedConnectionListener : IConnectionListener
    {
        BaseUriWithWildcard baseAddress;
        int queueId;
        Guid token;
        InputQueue<DuplicateConnectionAsyncResult> connectionQueue;
        SharedListenerProxy listenerProxy;
        Action<object> reconnectCallback;
        object syncRoot = new object();
        CommunicationState state;
        ManualResetEvent reconnectEvent;
        Func<Uri, int> onDuplicatedViaCallback;
        static readonly Version ProtocolVersion = new Version(3, 0, 0, 0);

        internal SharedConnectionListener(BaseUriWithWildcard baseAddress, int queueId, Guid token,
            Func<Uri, int> onDuplicatedViaCallback)
        {
            this.baseAddress = baseAddress;
            this.queueId = queueId;
            this.token = token;
            this.onDuplicatedViaCallback = onDuplicatedViaCallback;

            this.connectionQueue = TraceUtility.CreateInputQueue<DuplicateConnectionAsyncResult>();
            this.state = CommunicationState.Created;
            this.reconnectEvent = new ManualResetEvent(true);

            // only attmptStart if doing TCP port sharing
            // for activation we need to wait for the service to start if it crashes before w3wp can hook up
            StartListen(false);
        }

        object ThisLock
        {
            get
            {
                return this.syncRoot;
            }
        }

        void IConnectionListener.Listen()
        {
            // No-op since we have already been started.
        }

        IAsyncResult IConnectionListener.BeginAccept(AsyncCallback callback, object state)
        {
            Fx.Assert(connectionQueue != null, "The connectionQueue should not be null when BeginAccept is called.");
            Debug.Print("SharedConnectionListener.BeginAccept() connectionQueue.PendingCount: " + connectionQueue.PendingCount);
            return connectionQueue.BeginDequeue(TimeSpan.MaxValue, callback, state);
        }

        // Stop the proxy but do not close to let existing connections to be drained.
        public void Stop(TimeSpan timeout)
        {
            Stop(false, timeout);
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The call to System.ServiceProcess.TimeoutException (defined in a non-aptca assembly) is safe.")]
        // Stop the proxy but do not close to let existing connections to be drained.
        public void Stop(bool aborting, TimeSpan timeout)
        {
            bool shouldWait = false;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            lock (ThisLock)
            {
                if (this.state == CommunicationState.Closing ||
                    this.state == CommunicationState.Closed)
                {
                    return;
                }
                else if (this.state == CommunicationState.Opening && !aborting)
                {
                    shouldWait = true;
                }

                this.state = CommunicationState.Closing;
            }

            bool success = false;
            try
            {
                if (shouldWait)
                {
                    if (!this.reconnectEvent.WaitOne(timeoutHelper.RemainingTime()))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new TimeoutException(SR.GetString(SR.TimeoutOnClose, timeoutHelper.OriginalTimeout)));
                    }
                }
                success = true;
            }
            finally
            {
                if (listenerProxy != null)
                {
                    if (aborting || !success)
                    {
                        listenerProxy.Abort();
                    }
                    else
                    {
                        listenerProxy.Close(timeoutHelper.RemainingTime());
                    }
                }
            }
        }

        void Close()
        {
            lock (ThisLock)
            {
                if (this.state == CommunicationState.Closed)
                {
                    return;
                }

                Fx.Assert(this.state == CommunicationState.Closing,
                    "The Stop method must be called first before calling Close.");

                this.state = CommunicationState.Closed;
            }

            if (connectionQueue != null)
            {
                connectionQueue.Close();
            }

            if (this.reconnectEvent != null)
            {
                this.reconnectEvent.Close();
            }
        }

        public void Abort()
        {
            lock (ThisLock)
            {
                if (this.state == CommunicationState.Closed)
                {
                    return;
                }

                if (this.reconnectEvent != null)
                {
                    this.reconnectEvent.Set();
                }

                Stop(true, TimeSpan.Zero);
            }

            Close();
        }

        void OnConnectionAvailable(DuplicateConnectionAsyncResult result)
        {
            // Enqueue the context and dispatch it on a different thread.
            connectionQueue.EnqueueAndDispatch(result, null, false);
        }

        static string GetServiceName(bool isTcp)
        {
            return isTcp ? ListenerConstants.TcpPortSharingServiceName : ListenerConstants.NamedPipeActivationServiceName;
        }

        IConnection IConnectionListener.EndAccept(IAsyncResult result)
        {
            lock (ThisLock)
            {
                if (this.state != CommunicationState.Opening &&
                    this.state != CommunicationState.Opened)
                {
                    return null;
                }

                DuplicateConnectionAsyncResult duplicateAsyncResult = connectionQueue.EndDequeue(result);
                Fx.Assert(duplicateAsyncResult != null, "EndAccept: EndDequeue returned null.");

                // Finish the duplication.
                duplicateAsyncResult.CompleteOperation();

                return duplicateAsyncResult.Connection;
            }
        }

        void OnListenerFaulted(bool shouldReconnect)
        {
            lock (ThisLock)
            {
                if (this.state == CommunicationState.Closing ||
                    this.state == CommunicationState.Closed)
                {
                    return;
                }

                listenerProxy.Abort();

                if (shouldReconnect)
                {
                    this.state = CommunicationState.Opening;
                    this.reconnectEvent.Reset();
                }
                else
                {
                    this.state = CommunicationState.Faulted;
                }
            }

            if (shouldReconnect)
            {
                if (reconnectCallback == null)
                {
                    reconnectCallback = new Action<object>(ReconnectCallback);
                }

                ActionItem.Schedule(reconnectCallback, this);
            }
        }

        void StartListen(bool isReconnecting)
        {
            listenerProxy = new SharedListenerProxy(this);
            if (isReconnecting)
            {
                // Signal the event so that we are safe to close.
                reconnectEvent.Set();
            }

            listenerProxy.Open(isReconnecting);

            lock (ThisLock)
            {
                if (this.state == CommunicationState.Created || this.state == CommunicationState.Opening)
                {
                    this.state = CommunicationState.Opened;
                }
            }
        }

        void ReconnectCallback(object state)
        {
            BackoffTimeoutHelper backoffHelper =
                new BackoffTimeoutHelper(TimeSpan.MaxValue, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(30));

            // Looping until we can connect or when it's closed.
            while (this.state == CommunicationState.Opening)
            {
                bool success = false;
                try
                {
                    StartListen(true);
                    success = true;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);
                }

                // Add backoff when reconnect
                if (this.state == CommunicationState.Opening)
                {
                    Fx.Assert(!success, "The state should be Opened if it is successful.");
                    backoffHelper.WaitAndBackoff();
                }
            }
        }

        void IDisposable.Dispose()
        {
            Debug.Print("SharedConnectionListener.Dispose()");
            Close();
        }

        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
        class SharedListenerProxy : IConnectionDuplicator, IInputSessionShutdown
        {
            const int MaxPendingValidateUriRouteCallsPerProcessor = 10;
            static byte[] drainBuffer;
            SharedConnectionListener parent;
            BaseUriWithWildcard baseAddress;
            int queueId;
            Guid token;
            bool isTcp;
            string serviceName;
            string listenerEndPoint;
            SecurityIdentifier listenerUniqueSid;
            SecurityIdentifier listenerUserSid;
            ChannelFactory channelFactory;
            IDuplexContextChannel controlSessionWithListener;
            IDisposable allowContext;
            string securityEventName;
            ReaderWriterLockSlim readerWriterLock;
            int connectionBufferSize;
            Func<Uri, int> onDuplicatedViaCallback;
            bool listenerClosed;
            bool closed;
            bool opened;
            ConnectionBufferPool connectionBufferPool;
            ThreadNeutralSemaphore validateUriCallThrottle; 

            public SharedListenerProxy(SharedConnectionListener parent)
            {
                this.parent = parent;
                this.baseAddress = parent.baseAddress;
                this.queueId = parent.queueId;
                this.token = parent.token;
                this.onDuplicatedViaCallback = parent.onDuplicatedViaCallback;
                this.isTcp = parent.baseAddress.BaseAddress.Scheme.Equals(Uri.UriSchemeNetTcp);
                this.securityEventName = Guid.NewGuid().ToString();
                this.serviceName = SharedConnectionListener.GetServiceName(isTcp);
                this.readerWriterLock = new ReaderWriterLockSlim();

                //Workaround: Named Pipe stops responding if we push too many requests off to it concurrently.
                this.validateUriCallThrottle = new ThreadNeutralSemaphore(MaxPendingValidateUriRouteCallsPerProcessor * Environment.ProcessorCount, () => { return null; });
            }

            public void Open(bool isReconnecting)
            {
                Debug.Print("SharedListenerProxy.Open() isReconnecting: " + isReconnecting);

                if (this.closed)
                {
                    return;
                }

                // Start the listener service
                this.listenerEndPoint = HandleServiceStart(isReconnecting);
                if (string.IsNullOrEmpty(listenerEndPoint))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                        SR.GetString(SR.Sharing_EmptyListenerEndpoint, this.serviceName)));
                }

                // Check it again after possible backoff
                if (this.closed)
                {
                    return;
                }

                LookupListenerSid();

                EventWaitHandle securityEvent = null;
                bool success = false;

                // Synchronize with Close so that we can ensure cleanness
                using (LockHelper.TakeWriterLock(this.readerWriterLock))
                {
                    try
                    {
                        // Create the control proxy
                        CreateControlProxy();

                        EventWaitHandleSecurity handleSecurity = new EventWaitHandleSecurity();
                        handleSecurity.AddAccessRule(new EventWaitHandleAccessRule(listenerUniqueSid, EventWaitHandleRights.Modify, AccessControlType.Allow));

                        bool createdNew;
                        securityEvent = new EventWaitHandle(false, EventResetMode.ManualReset, ListenerConstants.GlobalPrefix + this.securityEventName, out createdNew, handleSecurity);
                        if (!createdNew)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceSecurityFailed))));
                        }

                        Register();

                        bool signalled = securityEvent.WaitOne(0, false);
                        if (!signalled)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceSecurityFailed))));
                        }

                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PortSharingListening, SR.GetString(SR.TraceCodePortSharingListening));
                        }

                        this.opened = true;
                        success = true;
                    }
                    finally
                    {
                        if (securityEvent != null)
                        {
                            securityEvent.Close();
                        }

                        if (!success)
                        {
                            Cleanup(true, TimeSpan.Zero);

                            // Mark it as closed
                            this.closed = true;
                        }
                    }
                }
            }

            public void Close(TimeSpan timeout)
            {
                Close(false, timeout);
            }

            void Close(bool isAborting, TimeSpan timeout)
            {
                using (LockHelper.TakeWriterLock(this.readerWriterLock))
                {
                    if (this.closed)
                    {
                        return;
                    }

                    bool success = false;
                    try
                    {
                        Cleanup(isAborting, timeout);
                        success = true;
                    }
                    finally
                    {
                        if (!success && !isAborting)
                        {
                            // Abort it
                            Abort();
                        }

                        this.closed = true;
                    }
                }

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PortSharingClosed, SR.GetString(SR.TraceCodePortSharingClosed));
                }
            }

            void Cleanup(bool isAborting, TimeSpan timeout)
            {
                this.validateUriCallThrottle.Abort();

                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                bool success = false;
                if (controlSessionWithListener != null)
                {
                    if (!isAborting)
                    {
                        try
                        {
                            Unregister(timeoutHelper.RemainingTime());
                            controlSessionWithListener.Close(timeoutHelper.RemainingTime());
                            success = true;
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }

                            DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);
                        }
                    }

                    if (isAborting || !success)
                    {
                        controlSessionWithListener.Abort();
                    }
                }

                if (channelFactory != null)
                {
                    success = false;
                    if (!isAborting)
                    {
                        try
                        {
                            channelFactory.Close(timeoutHelper.RemainingTime());
                            success = true;
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }

                            DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);
                        }
                    }

                    if (isAborting || !success)
                    {
                        channelFactory.Abort();
                    }
                }

                if (allowContext != null)
                {
                    allowContext.Dispose();
                }
            }

            public void Abort()
            {
                Close(true, TimeSpan.Zero);
            }

            void Unregister(TimeSpan timeout)
            {
                Debug.Print("SharedListenerProxy.Unregister()");
                controlSessionWithListener.OperationTimeout = timeout;
                ((IConnectionRegister)controlSessionWithListener).Unregister();
            }

            void LookupListenerSid()
            {
                // SECURITY
                // now check with the SCM and get the LogonSid or ServiceSid and the Pid for the listener
                if (OSEnvironmentHelper.IsVistaOrGreater)
                {
                    try
                    {
                        listenerUniqueSid = Utility.GetWindowsServiceSid(serviceName);
                        Debug.Print("SharedListenerProxy.LookupListenerSid() listenerUniqueSid: " + listenerUniqueSid);
                    }
                    catch (Win32Exception exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceSidLookupFailure, exception.NativeErrorCode)), exception));
                    }
                }
                else
                {
                    int listenerPid;
                    try
                    {
                        listenerPid = Utility.GetPidForService(serviceName);
                        Debug.Print("SharedListenerProxy.LookupListenerSid() listenerPid: " + listenerPid);
                    }
                    catch (Win32Exception exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceLookupFailure, exception.NativeErrorCode)), exception));
                    }
                    try
                    {
                        listenerUserSid = Utility.GetUserSidForPid(listenerPid);
                        Debug.Print("SharedListenerProxy.LookupListenerSid() listenerUserSid: " + listenerUserSid);
                    }
                    catch (Win32Exception exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerUserSidLookupFailure, exception.NativeErrorCode)), exception));
                    }
                    try
                    {
                        listenerUniqueSid = Utility.GetLogonSidForPid(listenerPid);
                    }
                    catch (Win32Exception exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerLogonSidLookupFailure, exception.NativeErrorCode)), exception));
                    }
                }

                Debug.Print("SharedListenerProxy.LookupListenerSid() listenerUniqueSid: " + listenerUniqueSid);
            }

            void CreateControlProxy()
            {
                EndpointAddress epa = new EndpointAddress(Utility.FormatListenerEndpoint(this.serviceName,
                    this.listenerEndPoint));

                NamedPipeTransportBindingElement namedPipeBindingElement = new NamedPipeTransportBindingElement();
                CustomBinding customBinding = new CustomBinding(namedPipeBindingElement);
                InstanceContext instanceContext = new InstanceContext(null, this, false);

                ChannelFactory<IConnectionRegisterAsync> registerChannelFactory = new DuplexChannelFactory<IConnectionRegisterAsync>(instanceContext,
                    customBinding, epa);

                registerChannelFactory.Endpoint.Behaviors.Add(new SharedListenerProxyBehavior(this));

                IConnectionRegister connectionRegister = registerChannelFactory.CreateChannel();
                this.channelFactory = registerChannelFactory;
                this.controlSessionWithListener = connectionRegister as IDuplexContextChannel;
            }

            void Register()
            {
                if (TD.SharedListenerProxyRegisterStartIsEnabled())
                {                    
                    TD.SharedListenerProxyRegisterStart((this.baseAddress != null) ? this.baseAddress.ToString() : String.Empty);
                }

                // Tactical fix for CTP. CTP will only ship with the 3.0 port sharing service enabled.
                // Using 3.0.0.0 rather than Assembly.GetExecutingAssembly().GetName().Version.
                // This works since nothing has changed between 3.0 and 4.0 and
                // both 3.0 and 4.0 port sharing service will accept a 3.0 version.
                Version version = ProtocolVersion;
                int myPid = Process.GetCurrentProcess().Id;

                HandleAllowDupHandlePermission(myPid);

                ListenerExceptionStatus status = ((IConnectionRegister)this.controlSessionWithListener).Register(
                        version, myPid, this.baseAddress, this.queueId, this.token, this.securityEventName);

                Debug.Print("SharedListenerProxy.Register() Register returned status: " + status);
                if (status != ListenerExceptionStatus.Success)
                {
                    if (TD.SharedListenerProxyRegisterFailedIsEnabled())
                    {
                        TD.SharedListenerProxyRegisterFailed(status.ToString());
                    }

                    switch (status)
                    {
                        case ListenerExceptionStatus.ConflictingRegistration:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAlreadyInUseException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerConflictingRegistration))));
                        case ListenerExceptionStatus.FailedToListen:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAlreadyInUseException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerFailedToListen))));
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString("SharedManager" + status))));
                    }
                }
                else
                {
                    if (TD.SharedListenerProxyRegisterStopIsEnabled())
                    {
                        TD.SharedListenerProxyRegisterStop();
                    }
                }
            }

            void HandleAllowDupHandlePermission(int myPid)
            {
                Debug.Print("SharedListenerProxy.HandleAllowDupHandlePermission() myPid: " + myPid);
                bool notNecessary = !OSEnvironmentHelper.IsVistaOrGreater && listenerUserSid.Equals(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null));
                Debug.Print("SharedListenerProxy.HandleAllowDupHandlePermission() notNecessary(ServiceRunningAsLocalSystem): " + notNecessary);
                if (notNecessary)
                {
                    return;
                }

                SecurityIdentifier myUserSid;
                try
                {
                    myUserSid = Utility.GetUserSidForPid(myPid);
                }
                catch (Win32Exception exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerCurrentUserSidLookupFailure, exception.NativeErrorCode)), exception));
                }

                Debug.Print("SharedListenerProxy.HandleAllowDupHandlePermission() myPid: " + myPid + " myUserSid: " + myUserSid.Value);
                notNecessary = !OSEnvironmentHelper.IsVistaOrGreater && myUserSid.Equals(listenerUserSid);
                Debug.Print("SharedListenerProxy.HandleAllowDupHandlePermission() notNecessary(RunningUnderTheSameAccount): " + notNecessary);
                if (notNecessary)
                {
                    return;
                }

                try
                {
                    allowContext = AllowHelper.TryAllow(listenerUniqueSid.Value);
                }
                catch (Win32Exception exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase,
                        serviceName, SR.GetString(SR.SharedManagerAllowDupHandleFailed, listenerUniqueSid.Value)), exception));
                }

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PortSharingDupHandleGranted,
                        SR.GetString(SR.TraceCodePortSharingDupHandleGranted, serviceName, listenerUniqueSid.Value));
                }
            }

            IConnection BuildDuplicatedNamedPipeConnection(NamedPipeDuplicateContext duplicateContext, int connectionBufferSize)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.PortSharingDuplicatedPipe, SR.GetString(SR.TraceCodePortSharingDuplicatedPipe));
                }

                PipeHandle duplicated = new PipeHandle(duplicateContext.Handle);
                PipeConnection pipeConnection = new PipeConnection(duplicated, connectionBufferSize, false, true);

                return new NamedPipeValidatingConnection(new PreReadConnection(pipeConnection, duplicateContext.ReadData),
                    this);
            }

            ConnectionBufferPool EnsureConnectionBufferPool(int connectionBufferSize, bool alreadyHoldingLock)
            {
                if (alreadyHoldingLock)
                {
                    return EnsureConnectionBufferPoolCore(connectionBufferSize);
                }
                else
                {
                    using (LockHelper.TakeWriterLock(this.readerWriterLock))
                    {
                        return EnsureConnectionBufferPoolCore(connectionBufferSize);
                    }
                }
            }

            // Don't call directly. Call EnsureConnectionBufferPool instead.
            ConnectionBufferPool EnsureConnectionBufferPoolCore(int connectionBufferSize)
            {
                if (this.connectionBufferPool != null && (connectionBufferSize == this.connectionBufferPool.BufferSize))
                {
                    return this.connectionBufferPool;
                }

                // The pool is refreshed only when the ConnectionBufferSize has been changed. The pool is shared for
                // different connections if they share the same BufferSize.
                this.connectionBufferPool = new ConnectionBufferPool(connectionBufferSize);
                return this.connectionBufferPool;
            }

            IConnection BuildDuplicatedTcpConnection(TcpDuplicateContext duplicateContext, int connectionBufferSize, bool alreadyHoldingLock)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.PortSharingDuplicatedSocket,
                        SR.GetString(SR.TraceCodePortSharingDuplicatedSocket));
                }
                if (TD.PortSharingDuplicatedSocketIsEnabled())
                {
                    EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(OperationContext.Current.IncomingMessage);
                    TD.PortSharingDuplicatedSocket(eventTraceActivity, (duplicateContext.Via != null) ? duplicateContext.Via.ToString() : string.Empty);
                }

                Socket socket = new Socket(duplicateContext.SocketInformation);
                SocketConnection socketConnection = new SocketConnection(socket, EnsureConnectionBufferPool(connectionBufferSize, alreadyHoldingLock), true);

                return new TcpValidatingConnection(new PreReadConnection(socketConnection, duplicateContext.ReadData),
                    this);
            }

            IAsyncResult BeginValidateUriRoute(Uri uri, IPAddress address, int port, AsyncCallback callback, object state)
            {
                //readerWriterLock is taken inside of async result and check for "closed" stte occurs within that lock
                return new ValidateUriRouteAsyncResult(this, uri, address, port, callback, state);                
            }

            bool EndValidateUriRoute(IAsyncResult result)
            {
                CompletedAsyncResult<bool> completedResult = result as CompletedAsyncResult<bool>;
                if (completedResult != null)
                {
                    CompletedAsyncResult<bool>.End(completedResult);
                }

                using (LockHelper.TakeReaderLock(this.readerWriterLock))
                {
                    try
                    {
                        return ValidateUriRouteAsyncResult.End(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception) || !closed)
                        {
                            throw;
                        }

                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);
                        return false;
                    }                    
                }
            }

            class ValidateUriRouteAsyncResult : TypedAsyncResult<bool>
            {
                static AsyncCallback onValidateUriRoute;
                static FastAsyncCallback onEnterThrottle;
                SharedListenerProxy proxy;
                Uri uri;
                IPAddress address;
                int port;
                bool enteredThrottle;
                
                public ValidateUriRouteAsyncResult(SharedListenerProxy proxy, Uri uri, IPAddress address, int port, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.proxy = proxy;
                    this.uri = uri;
                    this.address = address;
                    this.port = port;

                    bool isValidUriRoute = false;
                    bool completeSelf = false;
                    Exception completionException = null;
                    
                    try
                    {
                        completeSelf = BeginEnterThrottle(out isValidUriRoute);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        isValidUriRoute = false;
                        if (!ShouldHandleException(exception))
                        {
                            completionException = exception;
                        }

                        completeSelf = true;
                    }

                    if (completeSelf)
                    {
                        Complete(true, isValidUriRoute, completionException);
                    }
                }

                void Cleanup()
                {
                    if (this.enteredThrottle)
                    {
                        this.enteredThrottle = false;
                        this.proxy.validateUriCallThrottle.Exit();
                    }
                }

                bool BeginEnterThrottle(out bool isValidUriRoute)
                {
                    isValidUriRoute = false;

                    if (this.proxy.closed)
                    {
                        return true;
                    }

                    if (onEnterThrottle == null)
                    {
                        onEnterThrottle = new FastAsyncCallback(OnEnterThrottle);
                    }

                    
                    if (this.proxy.validateUriCallThrottle.EnterAsync(TimeSpan.MaxValue, onEnterThrottle, this))
                    {
                        this.enteredThrottle = true;
                        return BeginValidateUriRoute(out isValidUriRoute);
                    }

                    return false;
                }

                bool BeginValidateUriRoute(out bool isValidUriRoute)
                {
                    isValidUriRoute = false;
                    
                    if (onValidateUriRoute == null)
                    {
                        onValidateUriRoute = Fx.ThunkCallback(new AsyncCallback(OnValidateUriRoute));
                    }

                    using (LockHelper.TakeReaderLock(this.proxy.readerWriterLock))
                    {
                        if (this.proxy.closed)
                        {
                            return true;
                        }

                        IAsyncResult asyncResult = ((IConnectionRegisterAsync)this.proxy.controlSessionWithListener).BeginValidateUriRoute(uri, address, port, onValidateUriRoute, this);

                        if (asyncResult.CompletedSynchronously)
                        {
                            return HandleValidateUriRoute(asyncResult, out isValidUriRoute);
                        }
                    }
                    return false;
                }

                static void OnEnterThrottle(object state, Exception completionException)
                {
                    ValidateUriRouteAsyncResult thisPtr = (ValidateUriRouteAsyncResult)state;
                    thisPtr.enteredThrottle = true;
                    bool completeSelf = completionException != null;
                    bool isValidUriRoute = false;

                    if (!completeSelf)
                    {
                        try
                        {
                            completeSelf = thisPtr.BeginValidateUriRoute(out isValidUriRoute);
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }
                            completeSelf = true;
                            completionException = ex;
                        }
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, isValidUriRoute, completionException);
                    }
                }

                static void OnValidateUriRoute(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    bool completeSelf;
                    bool isValidUriRoute;
                    Exception completionException = null;
                    ValidateUriRouteAsyncResult thisPtr = (ValidateUriRouteAsyncResult)result.AsyncState;

                    try
                    {
                        completeSelf = thisPtr.HandleValidateUriRoute(result, out isValidUriRoute);
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex))
                        {
                            throw;
                        }

                        completeSelf = true;
                        isValidUriRoute = false;
                        if (!ShouldHandleException(ex))
                        {
                            completionException = ex;
                        }
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, isValidUriRoute, completionException);
                    }
                }

                bool HandleValidateUriRoute(IAsyncResult result, out bool isValidUriRoute)
                {
                    isValidUriRoute = ((IConnectionRegisterAsync)this.proxy.controlSessionWithListener).EndValidateUriRoute(result);
                    return true;
                }

                //Traces the exception if handled...
                static bool ShouldHandleException(Exception exception)
                {
                    bool shouldHandleException = false;                    

                    if (exception is CommunicationException ||
                        exception is TimeoutException)
                    {
                        shouldHandleException = true;

                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }

                    return shouldHandleException;
                }

                void Complete(bool completedSynchronously, bool isValidUriRoute, Exception completionException)
                {
                    Cleanup();
                    if (completionException != null)
                    {
                        Fx.Assert(!isValidUriRoute, "isValidUriRoute should always be false when completing with an exception");
                        base.Complete(completedSynchronously, completionException);
                    }
                    else
                    {
                        base.Complete(isValidUriRoute, completedSynchronously);
                    }
                }
            }

            class NamedPipeValidatingConnection : DelegatingConnection
            {
                SharedListenerProxy listenerProxy;
                bool initialValidation;

                public NamedPipeValidatingConnection(IConnection connection, SharedListenerProxy listenerProxy)
                    : base(connection)
                {
                    this.listenerProxy = listenerProxy;
                    this.initialValidation = true;
                }

                public override IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
                {
                    if (this.initialValidation) // optimization for first usage
                    {
                        this.initialValidation = false;
                        return new CompletedAsyncResult<bool>(true, callback, state);
                    }

                    return this.listenerProxy.BeginValidateUriRoute(uri, null, -1, callback, state);
                }

                public override bool EndValidate(IAsyncResult result)
                {
                    if (result is CompletedAsyncResult<bool>)
                    {
                        return CompletedAsyncResult<bool>.End(result);
                    }

                    return this.listenerProxy.EndValidateUriRoute(result);
                }
            }

            class TcpValidatingConnection : DelegatingConnection
            {
                IPAddress ipAddress;
                int port;
                SharedListenerProxy listenerProxy;
                bool initialValidation;

                public TcpValidatingConnection(IConnection connection, SharedListenerProxy listenerProxy)
                    : base(connection)
                {
                    this.listenerProxy = listenerProxy;

                    Socket socket = (Socket)connection.GetCoreTransport();
                    this.ipAddress = ((IPEndPoint)socket.LocalEndPoint).Address;
                    this.port = ((IPEndPoint)socket.LocalEndPoint).Port;
                    this.initialValidation = true;
                }

                public override IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
                {
                    if (this.initialValidation) // optimization for first usage
                    {
                        this.initialValidation = false;
                        return new CompletedAsyncResult<bool>(true, callback, state);
                    }

                    return this.listenerProxy.BeginValidateUriRoute(uri, this.ipAddress, this.port, callback, state);
                }

                public override bool EndValidate(IAsyncResult result)
                {
                    if (result is CompletedAsyncResult<bool>)
                    {
                        return CompletedAsyncResult<bool>.End(result);
                    }

                    return this.listenerProxy.EndValidateUriRoute(result);
                }
            }

            bool ReadEndpoint(string sharedMemoryName, out string listenerEndpoint)
            {
                try
                {
                    if (SharedMemory.Read(sharedMemoryName, out listenerEndpoint))
                    {
                        return true;
                    }

                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SharedManagerServiceEndpointNotExist,
                            SR.GetString(SR.TraceCodeSharedManagerServiceEndpointNotExist, serviceName), null, null);
                    }

                    return false;
                }
                catch (Win32Exception exception)
                {
                    // Wrap unexpected Win32Exception.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WrapEndpointReadingException(exception));
                }
            }

            Exception WrapEndpointReadingException(Win32Exception exception)
            {
                string message;
                if (exception.NativeErrorCode == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND)
                {
                    message = SR.GetString(SR.SharedEndpointReadNotFound, this.baseAddress.BaseAddress.ToString(),
                        this.serviceName);
                }
                else if (exception.NativeErrorCode == UnsafeNativeMethods.ERROR_ACCESS_DENIED)
                {
                    message = SR.GetString(SR.SharedEndpointReadDenied, this.baseAddress.BaseAddress.ToString());
                }
                else
                {
                    message = SR.GetString(SR.SharedManagerBase,
                        serviceName, SR.GetString(SR.SharedManagerServiceEndpointReadFailure, exception.NativeErrorCode));
                }

                return new CommunicationException(message, exception);
            }

            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "ServiceController has demands for ServiceControllerPermission.")]
            string HandleServiceStart(bool isReconnecting)
            {
                string listenerEndpoint = null;
                string sharedMemoryName = isTcp ? ListenerConstants.TcpSharedMemoryName : ListenerConstants.NamedPipeSharedMemoryName;
                serviceName = SharedConnectionListener.GetServiceName(isTcp);

                // Try to read the endpoint only if not reconnecting.
                if (!isReconnecting)
                {
                    if (ReadEndpoint(sharedMemoryName, out listenerEndpoint))
                    {
                        return listenerEndpoint;
                    }
                }

                ServiceController service = new ServiceController(serviceName);
                try
                {
                    ServiceControllerStatus serviceStatus = service.Status;
                    Debug.Print("ListenerServiceHelper.HandleServiceStart() service serviceName: " + serviceName + " is in status serviceStatus: " + serviceStatus);
                    if (isReconnecting)
                    {
                        if (serviceStatus == ServiceControllerStatus.Running)
                        {
                            try
                            {
                                string listenerEndPoint = SharedMemory.Read(sharedMemoryName);
                                if (this.listenerEndPoint != listenerEndPoint)
                                {
                                    // Service restarted.
                                    return listenerEndPoint;
                                }
                            }
                            catch (Win32Exception exception)
                            {
                                Debug.Print("ListenerServiceHelper.HandleServiceStart() failed when reading the shared memory sharedMemoryName: " + sharedMemoryName + " exception: " + exception);
                                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                            }

                            // Wait for the service to exit the running state
                            serviceStatus = ExitServiceStatus(service, 50, 50, ServiceControllerStatus.Running);
                        }
                    }

                    if (serviceStatus != ServiceControllerStatus.Running)
                    {
                        if (!isReconnecting)
                        {
                            try
                            {
                                service.Start();
                            }
                            catch (InvalidOperationException exception)
                            {
                                Win32Exception win32Exception = exception.InnerException as Win32Exception;
                                if (win32Exception != null)
                                {
                                    if (win32Exception.NativeErrorCode == UnsafeNativeMethods.ERROR_SERVICE_DISABLED)
                                    {
                                        // service is disabled in the SCM, be specific
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceStartFailureDisabled, serviceName)), exception));
                                    }
                                    else if (win32Exception.NativeErrorCode != UnsafeNativeMethods.ERROR_SERVICE_ALREADY_RUNNING)
                                    {
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceStartFailure, win32Exception.NativeErrorCode)), exception));
                                    }
                                }
                                else
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceStartFailureNoError)), exception));
                                }
                            }
                        }
                        else if (serviceStatus != ServiceControllerStatus.StartPending)
                        {
                            if (serviceStatus == ServiceControllerStatus.StopPending)
                            {
                                serviceStatus = ExitServiceStatus(service, 50, 1000, serviceStatus);
                            }
                            if (serviceStatus == ServiceControllerStatus.Stopped)
                            {
                                serviceStatus = ExitServiceStatus(service, 50, 1000, serviceStatus);
                            }
                        }

                        service.Refresh();
                        serviceStatus = service.Status;
                        Debug.Print("ListenerServiceHelper.HandleServiceStart() service serviceName: " + serviceName + " is in status serviceStatus: " + serviceStatus);
                        if (serviceStatus == ServiceControllerStatus.StartPending)
                        {
                            serviceStatus = ExitServiceStatus(service, 50, 50, ServiceControllerStatus.StartPending);
                        }
                    }

                    Debug.Print("ListenerServiceHelper.HandleServiceStart() final, service serviceName: " + serviceName + " is in status serviceStatus: " + serviceStatus);
                    if (serviceStatus != ServiceControllerStatus.Running)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(
                            SR.SharedManagerBase, serviceName, SR.GetString(SR.SharedManagerServiceStartFailureNoError))));
                    }
                }
                finally
                {
                    service.Close();
                }

                try
                {
                    return SharedMemory.Read(sharedMemoryName);
                }
                catch (Win32Exception exception)
                {
                    Debug.Print("ListenerServiceHelper.HandleServiceStart() final, failed when reading the shared memory sharedMemoryName: " + sharedMemoryName + " exception: " + exception);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WrapEndpointReadingException(exception));
                }
            }

            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The ServiceController.Status property demands ServiceControllerPermission.")]
            ServiceControllerStatus ExitServiceStatus(ServiceController service, int pollMin, int pollMax, ServiceControllerStatus status)
            {
                Debug.Print("ListenerServiceHelper.ExitServiceStatus() pollMin: " + pollMin + " pollMax: " + pollMax + " exit serviceStatus: " + status);
                int poll = pollMin;
                BackoffTimeoutHelper backoffHelper =
                    new BackoffTimeoutHelper(TimeSpan.MaxValue, TimeSpan.FromMilliseconds(pollMax), TimeSpan.FromMilliseconds(pollMin));
                for (;;)
                {
                    if (this.closed)
                    {
                        // Break from backoff
                        return service.Status;
                    }

                    backoffHelper.WaitAndBackoff();
                    service.Refresh();
                    ServiceControllerStatus serviceStatus = service.Status;
                    if (serviceStatus != status)
                    {
                        return serviceStatus;
                    }
                }
            }

            void SendFault(IConnection connection, string faultCode)
            {
                try
                {
                    if (drainBuffer == null)
                    {
                        drainBuffer = new byte[1024];
                    }

                    // return fault and close connection
                    InitialServerConnectionReader.SendFault(connection, faultCode, drainBuffer,
                        ListenerConstants.SharedSendTimeout, ListenerConstants.SharedMaxDrainSize);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);
                }
            }

            bool HandleOnVia(DuplicateContext duplicateContext)
            {
                if (this.onDuplicatedViaCallback == null)
                {
                    return true;
                }

                // This is synchronized so that only the first service initializes the transport manager etc.
                // Subsequent services are skipped here.
                using (LockHelper.TakeWriterLock(this.readerWriterLock))
                {
                    if (this.onDuplicatedViaCallback == null)
                    {
                        return true;
                    }

                    int connectionBufferSize;
                    if (this.onDuplicatedViaCallback != null)
                    {
                        try
                        {
                            connectionBufferSize = onDuplicatedViaCallback(duplicateContext.Via);

                            // We completed the initialization.
                            this.connectionBufferSize = connectionBufferSize;
                            this.onDuplicatedViaCallback = null;
                        }
                        catch (Exception e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                            string faultCode = null;
                            if (e is ServiceActivationException)
                            {
                                faultCode = FramingEncodingString.ServiceActivationFailedFault;
                            }
                            else if (e is EndpointNotFoundException)
                            {
                                faultCode = FramingEncodingString.EndpointNotFoundFault;
                            }

                            IConnection connection = BuildConnectionFromData(duplicateContext, ConnectionOrientedTransportDefaults.ConnectionBufferSize, true);
                            if (faultCode != null)
                            {
                                SendFault(connection, faultCode);
                                return false;
                            }
                            else
                            {
                                connection.Abort();
                                if (e is CommunicationObjectAbortedException)
                                {
                                    return false;
                                }

                                throw;
                            }
                        }
                    }
                }

                return true;
            }

            IConnection BuildConnectionFromData(DuplicateContext duplicateContext, int connectionBufferSize, bool alreadyHoldingLock)
            {
                if (isTcp)
                {
                    return BuildDuplicatedTcpConnection((TcpDuplicateContext)duplicateContext, connectionBufferSize, alreadyHoldingLock);
                }
                else
                {
                    return BuildDuplicatedNamedPipeConnection((NamedPipeDuplicateContext)duplicateContext, connectionBufferSize);
                }
            }

            IAsyncResult IConnectionDuplicator.BeginDuplicate(DuplicateContext duplicateContext, AsyncCallback callback, object state)
            {
                try
                {
                    DuplicateConnectionAsyncResult result;
                    if (!HandleOnVia(duplicateContext))
                    {
                        return new DuplicateConnectionAsyncResult(callback, state);
                    }

                    result = new DuplicateConnectionAsyncResult(BuildConnectionFromData(duplicateContext,
                        this.connectionBufferSize, false), callback, state);

                    parent.OnConnectionAvailable(result);

                    return result;
                }
                catch (Exception exception)
                {
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);

                    throw;
                }
            }

            void IConnectionDuplicator.EndDuplicate(IAsyncResult result)
            {
                DuplicateConnectionAsyncResult.End(result);
            }

            void IInputSessionShutdown.ChannelFaulted(IDuplexContextChannel channel)
            {
                OnControlChannelShutdown();
            }

            void IInputSessionShutdown.DoneReceiving(IDuplexContextChannel channel)
            {
                OnControlChannelShutdown();
            }

            void OnControlChannelShutdown()
            {
                if (this.listenerClosed || !this.opened)
                {
                    return;
                }

                using (LockHelper.TakeWriterLock(this.readerWriterLock))
                {
                    if (this.listenerClosed || !this.opened)
                    {
                        return;
                    }

                    listenerClosed = true;
                }

                // Only reconnect in non-activation case.
                this.parent.OnListenerFaulted(queueId == 0);
            }

            class SharedListenerProxyBehavior : IEndpointBehavior
            {
                SharedListenerProxy proxy;

                public SharedListenerProxyBehavior(SharedListenerProxy proxy)
                {
                    this.proxy = proxy;
                }

                public void Validate(ServiceEndpoint serviceEndpoint) { }
                public void AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters) { }
                public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher) { }

                public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
                {
                    behavior.DispatchRuntime.InputSessionShutdownHandlers.Add(this.proxy);
                }
            }
        }

        class DuplicateConnectionAsyncResult : AsyncResult
        {
            IConnection connection;
            public DuplicateConnectionAsyncResult(IConnection connection, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.connection = connection;
            }

            public DuplicateConnectionAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.Complete(true);
            }

            public IConnection Connection
            {
                get
                {
                    return this.connection;
                }
            }

            public void CompleteOperation()
            {
                Complete(false);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DuplicateConnectionAsyncResult>(result);
            }
        }
    }

    class AllowHelper : MarshalByRefObject
    {
        // this is the real instance in the default AppDomain, otherwise it's a proxy
        static AllowHelper singleton;
        static Dictionary<string, RegistrationRefCount> processWideRefCount;
        static object thisLock = new object();
        static object ThisLock { get { return thisLock; } }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        static void EnsureInitialized()
        {
            if (singleton != null)
            {
                return;
            }

            lock (ThisLock)
            {
                if (singleton != null)
                {
                    return;
                }

                if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                {
                    processWideRefCount = new Dictionary<string, RegistrationRefCount>();
                    singleton = new AllowHelper();
                }
                else
                {
                    Guid rclsid = new Guid("CB2F6723-AB3A-11D2-9C40-00C04FA30A3E");
                    Guid riid = new Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E");
                    ListenerUnsafeNativeMethods.ICorRuntimeHost corRuntimeHost;

                    corRuntimeHost = (ListenerUnsafeNativeMethods.ICorRuntimeHost)System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeInterfaceAsObject(rclsid, riid);

                    object defaultDomainAsObject;
                    corRuntimeHost.GetDefaultDomain(out defaultDomainAsObject);
                    AppDomain defaultDomain = (AppDomain)defaultDomainAsObject;
                    if (!defaultDomain.IsDefaultAppDomain())
                    {
                        throw Fx.AssertAndThrowFatal("AllowHelper..ctor() GetDefaultDomain did not return the default domain!");
                    }
                    singleton = defaultDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AllowHelper).FullName) as AllowHelper;
                }
            }
        }

        public static IDisposable TryAllow(string newSid)
        {
            EnsureInitialized();
            singleton.TryAllowCore(newSid);
            return new RegistrationForAllow(singleton, newSid);
        }

        void TryAllowCore(string newSid)
        {
            // In DefaultAppDomain, need to initialize.
            EnsureInitialized();

            lock (ThisLock)
            {
                RegistrationRefCount registration;
                if (!processWideRefCount.TryGetValue(newSid, out registration))
                {
                    registration = new RegistrationRefCount(newSid);
                }

                registration.AddRef();
            }
        }

        void UndoAllow(string grantedSid)
        {
            lock (ThisLock)
            {
                RegistrationRefCount registration = processWideRefCount[grantedSid];
                registration.RemoveRef();
            }
        }

        // This type is not thread-safe. The caller needs to provide synchronization mechanism.
        class RegistrationRefCount
        {
            int refCount;
            string grantedSid;

            public RegistrationRefCount(string grantedSid)
            {
                this.grantedSid = grantedSid;
            }

            public void AddRef()
            {
                if (refCount == 0)
                {
                    Utility.AddRightGrantedToAccount(new SecurityIdentifier(grantedSid), ListenerUnsafeNativeMethods.PROCESS_DUP_HANDLE);
                    processWideRefCount.Add(grantedSid, this);
                }

                refCount++;
            }

            public void RemoveRef()
            {
                refCount--;
                if (refCount == 0)
                {
                    Utility.RemoveRightGrantedToAccount(new SecurityIdentifier(grantedSid), ListenerUnsafeNativeMethods.PROCESS_DUP_HANDLE);
                    processWideRefCount.Remove(grantedSid);
                }
            }
        }

        class RegistrationForAllow : IDisposable
        {
            string grantedSid;
            AllowHelper singleton;

            public RegistrationForAllow(AllowHelper singleton, string grantedSid)
            {
                this.singleton = singleton;
                this.grantedSid = grantedSid;
            }

            void IDisposable.Dispose()
            {
                singleton.UndoAllow(grantedSid);
            }
        }
    }
}
