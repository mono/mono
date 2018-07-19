//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Activation.Configuration;
    using System.ServiceModel.Activation.Diagnostics;
    using System.Threading;
    using System.Web;
    using System.Web.Management;
    using System.Web.Routing;
    using TD2 = System.ServiceModel.Diagnostics.Application.TD;

    class HostedHttpRequestAsyncResult : AsyncResult, HttpChannelListener.IHttpAuthenticationContext
    {
        [Fx.Tag.SecurityNote(Critical = "Stores the securitycritical callback values, we need to protect these values")]
        [SecurityCritical]
        static WindowsIdentity anonymousIdentity;
        [SecurityCritical]
        static Action<object> waitOnBeginRequest;
        [SecurityCritical]
        static Action<object> waitOnBeginRequestWithFlow;
        [SecurityCritical]
        static ContextCallback contextOnBeginRequest;
        [SecurityCritical]
        static AsyncCallback processRequestCompleteCallback;


        [ThreadStatic]
        static AutoResetEvent waitObject;

        static Nullable<bool> iisSupportsExtendedProtection;

        [Fx.Tag.SecurityNote(Critical = "Keeps track of impersonated user, caller must use with care")]
        [SecurityCritical]
        HostedImpersonationContext impersonationContext;

        [Fx.Tag.SecurityNote(Critical = "Keeps track of thread static data (HttpContext, CurrentCulture, CurrentUICulture) that is used for AspNetCompatibility mode, caller must use with care")]
        [SecurityCritical]
        HostedThreadData hostedThreadData;

        [Fx.Tag.SecurityNote(Critical =
            "This field is used to manipulate request/responses using APIs protected by LinkDemand." +
            "It is critical because we use it to determine whether we believe we're being hosted in ASP.NET or not." +
            "The field is set in the constructor of this class and we deem it safe because:" +
            "    1) all paths that lead to the .ctor are SecurityCritical and" +
            "    2) those paths have called ServiceHostingEnvironment.EnsureInitialized (which is also critical)" +
            "So if the field is non-null, it's safe to say that we're hosted in ASP.NET, hence all the helper methods in this class that touch this field can be SecurityTreatAsSafe")]
        [SecurityCritical]
        HttpApplication context;

        int state;

        int streamedReadState;

        [Fx.Tag.SecurityNote(Critical = "Determines whether to set the HttpContext on the outgoing thread.")]
        [SecurityCritical]
        bool flowContext;
        bool ensureWFService;
        string configurationBasedServiceVirtualPath;
        EventTraceActivity eventTraceActivity;

        readonly bool isWebSocketRequest;

        [Fx.Tag.SecurityNote(Critical = "Captures HostedImpersonationContext which must be done in the right place, and calls unsafe" +
        "ScheduleCallbackLowPriNoFlow and ScriptTimeout.  Called outside of user security context.")]
        [SecurityCritical]
        public HostedHttpRequestAsyncResult(HttpApplication context, bool flowContext, bool ensureWFService, AsyncCallback callback, object state) :
            this(context, null, flowContext, ensureWFService, callback, state)
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Captures HostedImpersonationContext which must be done in the right place, and calls unsafe" +
            "ScheduleCallbackLowPriNoFlow and ScriptTimeout.  Called outside of user security context.")]
        [SecurityCritical]
        public HostedHttpRequestAsyncResult(HttpApplication context, string aspNetRouteServiceVirtualPath, bool flowContext, bool ensureWFService, AsyncCallback callback, object state) :
            base(callback, state)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            HostedAspNetEnvironment.TrySetWebSocketVersion(context);
            this.context = context;
            // WebSockets require the integrated pipeline mode and the WebSocket IIS module to be loaded. If these conditions 
            // are not met, the HttpContext.IsWebSocketRequest property throws. Also, if these conditions are not met,
            // we do not let WebSocket listeners to be started (we fail the service activation), so setting the 'isWebSocketRequest' flag 
            // to false in this case will not create confusion (or make troubleshooting difficult).
            this.isWebSocketRequest = HttpRuntime.UsingIntegratedPipeline && AspNetEnvironment.Current.IsWebSocketModuleLoaded && this.context.Context.IsWebSocketRequest;
            this.flowContext = flowContext;
            if (ensureWFService)
            {
                // check for CBA scenario. if true, service should be handled by WCF instead of WF, 
                // set this.ensureWFservice to false
                if (ServiceHostingEnvironment.IsConfigurationBasedService(context, out this.configurationBasedServiceVirtualPath))
                {
                    this.ensureWFService = false;
                }
                else
                {
                    this.ensureWFService = true;
                }
            }

            if (!string.IsNullOrEmpty(aspNetRouteServiceVirtualPath))
            {
                // aspnet routing can hijack CBA request as we append {*pathInfo} to urlpattern and there is no real file for CBA 
                // check for CBA scenario. if the request is hijacked. i.e., 
                // 1) route maps to a virtual directory:
                // aspNetRouteServiceVirtualPath <> context.Request.AppRelativeCurrentExecutionFilePath == configurationBasedServiceVirtualPath 
                // if RouteExistingFiles <> true, set aspnetRouteServiceVirtualPath to null so that the request will be treated as CBA
                // if RouteExistingFiles == true, this hijack is by-design, do nothing
                // 2) route maps to a CBA entry:
                // aspNetRouteServiceVirtualPath == context.Request.AppRelativeCurrentExecutionFilePath == configurationBasedServiceVirtualPath
                // we will use RouteExistingFiles to decide which service should be activated. We do it in ServiceHostingEnviroment.HostingManager, 
                // as we cannot pass this info to the latter. 
                if (!RouteTable.Routes.RouteExistingFiles &&
                    ServiceHostingEnvironment.IsConfigurationBasedService(context, out this.configurationBasedServiceVirtualPath))
                {
                    this.AspNetRouteServiceVirtualPath = null;
                }
                else
                {
                    this.AspNetRouteServiceVirtualPath = aspNetRouteServiceVirtualPath;
                }
            }

            // If this is a DEBUG request, complete right away and let ASP.NET handle it.
            string method = context.Request.HttpMethod ?? "";
            char firstMethodChar = method.Length == 5 ? method[0] : '\0';
            if ((firstMethodChar == 'd' || firstMethodChar == 'D') &&
                string.Compare(method, "DEBUG", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.WebHostDebugRequest, SR.TraceCodeWebHostDebugRequest, this);
                }

                this.state = State.Completed;
                Complete(true, null);
                return;
            }

            this.impersonationContext = new HostedImpersonationContext();

            if (flowContext)
            {
                if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
                {
                    // Capture HttpContext/culture context if necessary.  Can be used later by HostedHttpInput to re-apply
                    // the culture during dispatch.  Also flowed here.
                    hostedThreadData = new HostedThreadData();
                }
            }

            // Set this up before calling IncrementRequestCount so if it fails, we don't leak a count.
            Action<object> iotsCallback = (AspNetPartialTrustHelpers.NeedPartialTrustInvoke || flowContext) ?
                WaitOnBeginRequestWithFlow : WaitOnBeginRequest;

            // Tell ASPNET to by-pass all the other events so no other http modules will
            // be invoked, Indigo basically takes over the request completely. This should
            // only be called in non-AspNetCompatibilityEnabled mode.
            if (!ServiceHostingEnvironment.AspNetCompatibilityEnabled && !this.ensureWFService)
            {
                context.CompleteRequest();
            }

            // Prevent ASP.NET from generating thread aborts in relation to this request.
            context.Server.ScriptTimeout = int.MaxValue;

            ServiceHostingEnvironment.IncrementRequestCount(ref this.eventTraceActivity, context.Request.AppRelativeCurrentExecutionFilePath);

            IOThreadScheduler.ScheduleCallbackLowPriNoFlow(iotsCallback, this);
        }

        public static WindowsIdentity AnonymousIdentity
        {
            [Fx.Tag.SecurityNote(Critical = "Access the value of corresponding static field and prevent someone from changing its value")]
            [SecuritySafeCritical]
            get
            {
                if (anonymousIdentity == null)
                {
                    anonymousIdentity = WindowsIdentity.GetAnonymous();
                }
                return anonymousIdentity;
            }
        }

        public static Action<object> WaitOnBeginRequest
        {
            [Fx.Tag.SecurityNote(Critical = "Access the value of corresponding static field and prevent someone from changing its value")]
            [SecuritySafeCritical]
            get
            {
                if (waitOnBeginRequest == null)
                {
                    waitOnBeginRequest = new Action<object>(OnBeginRequest);
                }
                return waitOnBeginRequest;
            }
        }

        public static Action<object> WaitOnBeginRequestWithFlow
        {
            [Fx.Tag.SecurityNote(Critical = "Access the value of corresponding static field and prevent someone from changing its value")]
            [SecuritySafeCritical]
            get
            {
                if (waitOnBeginRequestWithFlow == null)
                {
                    waitOnBeginRequestWithFlow = new Action<object>(OnBeginRequestWithFlow);
                }
                return waitOnBeginRequestWithFlow;
            }
        }

        public static ContextCallback ContextOnBeginRequest
        {
            [Fx.Tag.SecurityNote(Critical = "Access the value of corresponding static field and prevent someone from changing its value")]
            [SecuritySafeCritical]
            get
            {
                if (contextOnBeginRequest == null)
                {
                    contextOnBeginRequest = new ContextCallback(OnBeginRequest);
                }
                return contextOnBeginRequest;
            }
        }

        public static AsyncCallback ProcessRequestCompleteCallback
        {
            [Fx.Tag.SecurityNote(Critical = "Access the value of corresponding static field and prevent someone from changing its value")]
            [SecuritySafeCritical]
            get
            {
                if (processRequestCompleteCallback == null)
                {
                    processRequestCompleteCallback = Fx.ThunkCallback(new AsyncCallback(ProcessRequestComplete));
                }
                return processRequestCompleteCallback;
            }
        }

        public bool IISSupportsExtendedProtection
        {
            get
            {
                if (HostedHttpRequestAsyncResult.iisSupportsExtendedProtection == null)
                {
                    HostedHttpRequestAsyncResult.iisSupportsExtendedProtection = this.IISSupportsExtendedProtectionInternal();
                }
                return HostedHttpRequestAsyncResult.iisSupportsExtendedProtection.Value;
            }
        }

        public bool IsWebSocketRequest
        {
            get { return this.isWebSocketRequest; }
        }

        [Fx.Tag.SecurityNote(Critical = "Touches critical field context.", Safe = "Does not leak control or data, no potential for harm.")]
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [PermissionSetAttribute(SecurityAction.Assert, Unrestricted = true)]
        private bool IISSupportsExtendedProtectionInternal()
        {
            DiagnosticUtility.DebugAssert(ExtendedProtectionPolicy.OSSupportsExtendedProtection, "OS must support ExtendedProtection");

            try
            {
                ChannelBinding cbt = this.context.Request.HttpChannelBinding;
                return true;
            }
            catch (PlatformNotSupportedException)
            {
                // contract with Asp.Net is that they will always throw a PlatformNotSupportedException if IIS is not patched for CBT yet
                return false;
            }
            catch (COMException)
            {
                // If IIS is patched for CBT and an error occurs when trying to retrieve the token a COMException is thrown. Even in this
                // case we know that IIS is patched for CBT.
                return true;
            }
        }


        [Fx.Tag.SecurityNote(Critical = "Captures HostedImpersonationContext which must be done in the right place, and calls unsafe" +
            "ScheduleCallbackLowPriNoFlow and ScriptTimeout.  Called outside of user security context." +
            "Callers of this function must call ServiceHostingEnvironment.EnsureInitialized")]
        [SecurityCritical]
        public static void ExecuteSynchronous(HttpApplication context, bool flowContext, bool ensureWFService)
        {
            ExecuteSynchronous(context, null, flowContext, ensureWFService);
        }

        [Fx.Tag.SecurityNote(Critical = "Captures HostedImpersonationContext which must be done in the right place, and calls unsafe" +
            "ScheduleCallbackLowPriNoFlow and ScriptTimeout.  Called outside of user security context." +
            "Callers of this function must call ServiceHostingEnvironment.EnsureInitialized")]
        [SecurityCritical]
        public static void ExecuteSynchronous(HttpApplication context, string routeServiceVirtualPath, bool flowContext, bool ensureWFService)
        {
            AutoResetEvent wait = HostedHttpRequestAsyncResult.waitObject;
            if (wait == null)
            {
                wait = new AutoResetEvent(false);
                HostedHttpRequestAsyncResult.waitObject = wait;
            }

            HostedHttpRequestAsyncResult result;
            try
            {
                result = new HostedHttpRequestAsyncResult(context, routeServiceVirtualPath, flowContext, ensureWFService, ProcessRequestCompleteCallback, wait);
                if (!result.CompletedSynchronously)
                {
                    wait.WaitOne();
                }
                wait = null;
            }
            finally
            {
                if (wait != null)
                {
                    // Not sure of the state anymore.
                    HostedHttpRequestAsyncResult.waitObject = null;
                    wait.Close();
                }
            }

            HostedHttpRequestAsyncResult.End(result);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Can be called outside of a user context.")]
        static void ProcessRequestComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    ((AutoResetEvent)result.AsyncState).Set();
                }
                catch (ObjectDisposedException exception)
                {
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Can be called outside of user context, accesses hostedThreadData.",
            Safe = "Uses hostedThreadData to set HttpContext.Current, cultures to the one attached to this async-result instance.")]
        [SecuritySafeCritical]
        static void OnBeginRequestWithFlow(object state)
        {
            HostedHttpRequestAsyncResult self = (HostedHttpRequestAsyncResult)state;

            IDisposable hostedThreadContext = null;
            try
            {
                if (self.flowContext)
                {
                    // In AspCompat case, these are the three things that need to be flowed.  See HostedHttpInput.
                    if (self.hostedThreadData != null)
                    {
                        hostedThreadContext = self.hostedThreadData.CreateContext();
                    }
                }

                // In full-trust, this simply calls the delegate.
                AspNetPartialTrustHelpers.PartialTrustInvoke(ContextOnBeginRequest, self);
            }
            finally
            {
                if (hostedThreadContext != null)
                {
                    hostedThreadContext.Dispose();
                }
            }
        }

        static void OnBeginRequest(object state)
        {
            HostedHttpRequestAsyncResult self = (HostedHttpRequestAsyncResult)state;

            Exception completionException = null;
            try
            {
                self.BeginRequest();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                completionException = e;
            }

            if (completionException != null)
            {
                self.CompleteOperation(completionException);
            }
        }

        void BeginRequest()
        {
            try
            {
                HandleRequest();
            }
            catch (EndpointNotFoundException exception)
            {
                // HTTP-GET is special cased to avoid that the ServiceActivation-HTTP-response is treated as service response.
                // For WebSocket requests we treat the ServiceActivation in the same way like for SOAP (HTTP-POST) requests.
                if (string.Compare(GetHttpMethod(), "GET", StringComparison.OrdinalIgnoreCase) == 0 &&
                    !this.isWebSocketRequest)
                {
                    // Wrap the exception into HttpException
                    throw FxTrace.Exception.AsError(new HttpException((int)HttpStatusCode.NotFound, exception.Message, exception));
                }

                SetStatusCode((int)HttpStatusCode.NotFound);
                CompleteOperation(null);
            }
            catch (ServiceActivationException exception)
            {
                // HTTP-GET is special cased to avoid that the ServiceActivation-HTTP-response is treated as service response.
                // For WebSocket requests we treat the ServiceActivation in the same way like for SOAP (HTTP-POST) requests.
                if (string.Compare(GetHttpMethod(), "GET", StringComparison.OrdinalIgnoreCase) == 0 &&
                    !this.isWebSocketRequest)
                {
                    if (exception.InnerException is HttpException)
                    {
                        throw exception.InnerException;
                    }
                    else
                    {
                        throw;
                    }
                }

                SetStatusCode((int)HttpStatusCode.InternalServerError);
                SetStatusDescription(
                    HttpChannelUtilities.StatusDescriptionStrings.HttpStatusServiceActivationException);
                CompleteOperation(null);
            }
            finally
            {
                ReleaseImpersonation();
            }
        }

        public WindowsIdentity LogonUserIdentity
        {
            get
            {
                IPrincipal user = this.Application.User;
                if (user == null)
                {
                    return AnonymousIdentity;
                }

                WindowsIdentity identity = user.Identity as WindowsIdentity;
                if (identity == null)
                {
                    return AnonymousIdentity;
                }

                return identity;
            }
        }

        WindowsIdentity HttpChannelListener.IHttpAuthenticationContext.LogonUserIdentity
        {
            get
            {
                return this.LogonUserIdentity;
            }
        }

        public HostedImpersonationContext ImpersonationContext
        {
            [Fx.Tag.SecurityNote(Critical = "Keeps track of impersonated user, caller must use with care.",
                Safe = "Safe for Get, individual members of HostedImpersonationContext are protected.")]
            [SecuritySafeCritical]
            get
            {
                return this.impersonationContext;
            }
        }

        public HostedThreadData HostedThreadData
        {
            [Fx.Tag.SecurityNote(Critical = "Keeps track of impersonated user, caller must use with care.",
                Safe = "Safe for Get, individual members of HostedThreadData are protected.")]
            [SecuritySafeCritical]
            get
            {
                return this.hostedThreadData;
            }
        }

        public EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.eventTraceActivity;
            }
        }

        public Uri OriginalRequestUri
        {
            get;
            private set;

        }

        public Uri RequestUri
        {
            get;
            private set;
        }

        public HttpApplication Application
        {
            [Fx.Tag.SecurityNote(Critical = "Touches critical field context.", Safe = "Does not leak control or data, no potential for harm.")]
            [SecuritySafeCritical]
            get
            {
                return this.context;
            }
        }

        public string AspNetRouteServiceVirtualPath
        {
            get;
            private set;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.", Safe = "Does not leak control or data, no potential for harm.")]
        [SecuritySafeCritical]
        public Stream GetInputStream()
        {
            try
            {
                // CSDMain #133228: "Consume GetBufferlessInputStream"
                // The ReadEntityBodyMode property on the HttpRequest keeps track of whether the request stream has already been accessed, and if so, what API was used to access the request.
                //     - "None" means that the request stream hasn't been accessed.
                //     - "Bufferless" means that GetBufferlessInputStream() was used to access it.
                //     - "Buffered" means GetBufferedInputStream() was used to access it.
                //     - "Classic" means that either the InputStream, Form, Files, or BinaryRead APIs were invoked already.
                // In general, these values are incompatible with one another, meaning that once the request was accessed in a "Classic" way, only "Classic" APIs can be invoked on the HttpRequest.
                // If incompatible APIs are invoked, an HttpException is thrown.
                // In order to prevent HttpExceptions from being thrown for this reason, we will check the ReadEntityBodyMode, and access the request stream with the corresponding API
                // If the request stream hasn't been accessed yet (eg, by an HttpModule which executed earlier), then we will use GetBufferlessInputStream by default.
                ReadEntityBodyMode mode = this.context.Request.ReadEntityBodyMode;
                Fx.Assert(mode == ReadEntityBodyMode.None || mode == ReadEntityBodyMode.Bufferless || mode == ReadEntityBodyMode.Buffered || mode == ReadEntityBodyMode.Classic,
                    "Unknown value for System.Web.ReadEntityBodyMode enum");

                if (mode == ReadEntityBodyMode.None && ServiceHostingEnvironment.AspNetCompatibilityEnabled && AppSettings.UseClassicReadEntityMode)
                {
                    mode = ReadEntityBodyMode.Classic;
                }

                switch (mode)
                {
                    case ReadEntityBodyMode.None:
                    case ReadEntityBodyMode.Bufferless:
                        return this.context.Request.GetBufferlessInputStream(true);  // ignores system.web/httpRuntime/maxRequestLength
                    case ReadEntityBodyMode.Buffered:
                        return this.context.Request.GetBufferedInputStream();
                    default: 
                        // ReadEntityBodyMode.Classic:
                        return this.context.Request.InputStream;
                }
            }
            catch (HttpException hostedException)
            {
                if (hostedException.WebEventCode == WebEventCodes.RuntimeErrorPostTooLarge)
                {
                    throw FxTrace.Exception.AsError(HttpInput.CreateHttpProtocolException(SR.Hosting_MaxRequestLengthExceeded, HttpStatusCode.RequestEntityTooLarge, null, hostedException));
                }
                else
                {
                    throw FxTrace.Exception.AsError(new CommunicationException(hostedException.Message, hostedException));
                }
            }
        }

        public void OnReplySent()
        {
            CompleteOperation(null);
        }

        internal void CompleteOperation(Exception exception)
        {
            if (this.state == State.Running &&
                Interlocked.CompareExchange(ref this.state, State.Completed, State.Running) == State.Running)
            {
                this.CompleteAsynchronously(exception);
            }
        }

        public void Abort()
        {
            if (this.state == State.Running &&
                Interlocked.CompareExchange(ref this.state, State.Aborted, State.Running) == State.Running)
            {
                int currentStreamedReadState = Interlocked.Exchange(ref this.streamedReadState, StreamedReadState.AbortStarted);
                // Closes the socket connection to the client
                if (HttpRuntime.UsingIntegratedPipeline)
                {
                    Application.Request.Abort();
                }
                else
                {
                    Application.Response.Close();
                }

                if (currentStreamedReadState == StreamedReadState.None)
                {
                    this.CompleteAsynchronously(null);
                }
                else
                {
                    Fx.Assert(currentStreamedReadState == StreamedReadState.ReceiveStarted, string.Format(CultureInfo.InvariantCulture, "currentStramedReadState is not ReceivedStarted: {0}", currentStreamedReadState));
                    if (Interlocked.CompareExchange(ref this.streamedReadState, StreamedReadState.Aborted, StreamedReadState.AbortStarted) == StreamedReadState.AbortStarted)
                    {
                        return;
                    }

                    Fx.Assert(this.streamedReadState == StreamedReadState.ReceiveFinishedAfterAbortStarted, string.Format(CultureInfo.InvariantCulture, "currentStramedReadState is not ReceiveFinished: {0}", this.streamedReadState));
                    this.CompleteAsynchronously(null);
                }
            }
        }

        void CompleteAsynchronously(Exception ex)
        {
            Complete(false, ex);
            ServiceHostingEnvironment.DecrementRequestCount(this.eventTraceActivity);
        }

        internal bool TryStartStreamedRead()
        {
            return Interlocked.CompareExchange(ref this.streamedReadState, StreamedReadState.ReceiveStarted, StreamedReadState.None) == StreamedReadState.None;
        }

        internal void SetStreamedReadFinished()
        {
            if (Interlocked.CompareExchange(ref this.streamedReadState, StreamedReadState.None, StreamedReadState.ReceiveStarted) == StreamedReadState.ReceiveStarted)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref this.streamedReadState, StreamedReadState.ReceiveFinishedAfterAbortStarted, StreamedReadState.AbortStarted) == StreamedReadState.AbortStarted)
            {
                return;
            }

            Fx.Assert(this.streamedReadState == StreamedReadState.Aborted, string.Format(CultureInfo.InvariantCulture, "currentStramedReadState is not Aborted: {0}", this.streamedReadState));

            this.CompleteAsynchronously(null);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Can be called outside of a user context.")]
        public static void End(IAsyncResult result)
        {
            try
            {
                AsyncResult.End<HostedHttpRequestAsyncResult>(result);
            }
            catch (Exception exception)
            {
                if (!Fx.IsFatal(exception))
                {
                    // Log the exception.
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost, 
                        (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostFailedToProcessRequest,
                      TraceUtility.CreateSourceString(result),
                      exception == null ? string.Empty : exception.ToString());
                }
                throw;
            }
        }

        X509Certificate2 HttpChannelListener.IHttpAuthenticationContext.GetClientCertificate(out bool isValidCertificate)
        {
            HttpClientCertificate certificateInfo = this.Application.Request.ClientCertificate;
            isValidCertificate = certificateInfo.IsValid;
            if (certificateInfo.IsPresent)
            {
                return new X509Certificate2(certificateInfo.Certificate);
            }
            else
            {
                return null;
            }
        }

        TraceRecord HttpChannelListener.IHttpAuthenticationContext.CreateTraceRecord()
        {
            return new System.ServiceModel.Diagnostics.HttpRequestTraceRecord(this.Application.Request);
        }

        void HandleRequest()
        {
            this.OriginalRequestUri = GetUrl();
            string relativeVirtualPath;
            if (!string.IsNullOrEmpty(this.AspNetRouteServiceVirtualPath))
            {
                relativeVirtualPath = this.AspNetRouteServiceVirtualPath;
            }
            else if (!string.IsNullOrEmpty(this.configurationBasedServiceVirtualPath))
            {
                relativeVirtualPath = this.configurationBasedServiceVirtualPath;

            }
            else
            {
                relativeVirtualPath = GetAppRelativeCurrentExecutionFilePath();
            }

            if (ensureWFService)
            {
                bool bypass = false;
                try
                {
                    if (!ServiceHostingEnvironment.EnsureWorkflowService(relativeVirtualPath))
                    {
                        CompleteOperation(null);
                        bypass = true;
                        return;
                    }
                }
                finally
                {
                    if (!bypass)
                    {
                        CompleteRequest();
                    }
                }
            }

            // Support for Cassini.
            if (ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                HostedTransportConfigurationManager.EnsureInitializedForSimpleApplicationHost(this);
            }

            HttpHostedTransportConfiguration transportConfiguration = HostedTransportConfigurationManager.GetConfiguration(this.OriginalRequestUri.Scheme)
                as HttpHostedTransportConfiguration;
            HostedHttpTransportManager transportManager = null;

            // There must be a transport binding that matches the request.
            if (transportConfiguration != null)
            {
                transportManager = transportConfiguration.GetHttpTransportManager(this.OriginalRequestUri);
            }

            if (transportManager == null)
            {
                InvalidOperationException invalidOpException = new InvalidOperationException(SR.Hosting_TransportBindingNotFound(OriginalRequestUri.ToString()));

                ServiceActivationException activationException = new ServiceActivationException(invalidOpException.Message, invalidOpException);

                LogServiceActivationException(activationException);

                throw FxTrace.Exception.AsError(activationException);
            }

            this.RequestUri = new Uri(transportManager.ListenUri, this.OriginalRequestUri.PathAndQuery);
            Fx.Assert(
                object.ReferenceEquals(this.RequestUri.Scheme, Uri.UriSchemeHttp) || object.ReferenceEquals(this.RequestUri.Scheme, Uri.UriSchemeHttps),
                "Scheme must be Http or Https.");

            ServiceHostingEnvironment.EnsureServiceAvailableFast(relativeVirtualPath, this.eventTraceActivity);

            transportManager.HttpContextReceived(this);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into an unsafe UnsafeLogEvent method",
            Safe = "Event identities cannot be spoofed as they are constants determined inside the method")]
        [SecuritySafeCritical]
        void LogServiceActivationException(ServiceActivationException activationException)
        {
            if (TD2.ServiceExceptionIsEnabled())
            {
                TD2.ServiceException(this.eventTraceActivity, activationException.ToString(), typeof(ServiceActivationException).FullName);
            }

            if (TD.ServiceActivationExceptionIsEnabled())
            {
                TD.ServiceActivationException(activationException != null ? activationException.ToString() : string.Empty, activationException);
            }
            DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost, 
                (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostFailedToProcessRequest, true,
                    TraceUtility.CreateSourceString(this), activationException.ToString());
        }

        [Fx.Tag.SecurityNote(Critical = "manipulates impersonation object",
           Safe = "Does not leak control or mutable/harmful data, no potential for harm except memory leak.")]
        [SecuritySafeCritical]
        internal void AddRefForImpersonation()
        {
            if (this.impersonationContext != null)
            {
                this.impersonationContext.AddRef();
            }
        }

        [Fx.Tag.SecurityNote(Critical = "manipulates impersonation object",
            Safe = "Releasing the SafeHandle early could only cause a future impersonation attempt to fail. We have to handle impersonation failures well already.")]
        [SecuritySafeCritical]
        internal void ReleaseImpersonation()
        {
            if (this.impersonationContext != null)
            {
                this.impersonationContext.Release();
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, changes properties of the HTTP response.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void SetContentType(string contentType)
        {
            this.context.Response.ContentType = contentType;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, changes properties of the HTTP response.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void SetContentEncoding(string contentEncoding)
        {
            this.context.Response.AddHeader(HttpChannelUtilities.ContentEncodingHeader, contentEncoding);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, completes the request.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void CompleteRequest()
        {
            this.context.CompleteRequest();
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, changes properties of the HTTP response.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void SetTransferModeToStreaming()
        {
            this.context.Response.BufferOutput = false;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, changes properties of the HTTP response.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void AppendHeader(string name, string value)
        {
            this.context.Response.AppendHeader(name, value);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, changes properties of the HTTP response.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void SetStatusCode(int statusCode)
        {
            this.context.Response.TrySkipIisCustomErrors = true;
            this.context.Response.StatusCode = statusCode;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, changes properties of the HTTP response.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void SetStatusDescription(string statusDescription)
        {
            this.context.Response.StatusDescription = statusDescription;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects, changes properties of the HTTP response.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal void SetConnectionClose()
        {
            this.context.Response.AppendHeader("Connection", "close");
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal byte[] GetPrereadBuffer(ref int contentLength)
        {
            byte[] preReadBuffer = new byte[1];
            if (this.GetInputStream().Read(preReadBuffer, 0, 1) > 0)
            {
                contentLength = -1;
                return preReadBuffer;
            }
            return null;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal Stream GetOutputStream()
        {
            return this.context.Response.OutputStream;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal string GetHttpMethod()
        {
            return this.context.Request.HttpMethod;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal string GetContentType()
        {
            const string ContentTypeHeaderName = "Content-Type";
            return this.context.Request.Headers[ContentTypeHeaderName];
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal string GetAcceptEncoding()
        {
            return this.context.Request.Headers[HttpChannelUtilities.AcceptEncodingHeader];
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal string GetContentTypeFast()
        {
            return this.context.Request.ContentType;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal int GetContentLength()
        {
            return this.context.Request.ContentLength;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal string GetSoapAction()
        {
            const string SoapActionHeaderName = "SOAPAction";
            return this.context.Request.Headers[SoapActionHeaderName];
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        internal ChannelBinding GetChannelBinding()
        {
            if (!this.IISSupportsExtendedProtection)
            {
                return null;
            }

            return this.context.Request.HttpChannelBinding;
        }


        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        string GetAppRelativeCurrentExecutionFilePath()
        {
            return this.context.Request.AppRelativeCurrentExecutionFilePath;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        Uri GetUrl()
        {
            return this.context.Request.Url;
        }

        static class State
        {
            internal const int Running = 0;
            internal const int Completed = 1;
            internal const int Aborted = 2;
        }

        static class StreamedReadState
        {
            internal const int None = 0;
            internal const int ReceiveStarted = 1;
            internal const int ReceiveFinishedAfterAbortStarted = 2;
            internal const int AbortStarted = 3;
            internal const int Aborted = 4;
        }
    }
}
