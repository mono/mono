//------------------------------------------------------------------------------
// <copyright file="HttpContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HttpContext class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Instrumentation;
    using System.Web.Management;
    using System.Web.Profile;
    using System.Web.Security;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.Util;
    using System.Web.WebSockets;


    /// <devdoc>
    ///    <para>Encapsulates
    ///       all HTTP-specific
    ///       context used by the HTTP server to process Web requests.</para>
    /// <para>System.Web.IHttpModules and System.Web.IHttpHandler instances are provided a
    ///    reference to an appropriate HttpContext object. For example
    ///    the Request and Response
    ///    objects.</para>
    /// </devdoc>
    [SuppressMessage("Microsoft.Usage", "CA2302:FlagServiceProviders", Justification = "The service provider implementation is only for specific types which are not com interop types.")]
    public sealed class HttpContext : IServiceProvider, IPrincipalContainer
    {

        internal static readonly Assembly SystemWebAssembly = typeof(HttpContext).Assembly;
        private static volatile bool s_eurlSet;
        private static string s_eurl;

        private IHttpAsyncHandler  _asyncAppHandler;   // application as handler (not always HttpApplication)
        private AsyncPreloadModeFlags _asyncPreloadModeFlags;
        private bool               _asyncPreloadModeFlagsSet;
        private HttpApplication    _appInstance;
        private IHttpHandler       _handler;
        [DoNotReset]
        private HttpRequest        _request;
        private HttpResponse       _response;
        private HttpServerUtility  _server;
        private Stack              _traceContextStack;
        private TraceContext       _topTraceContext;
        [DoNotReset]
        private Hashtable          _items;
        private ArrayList          _errors;
        private Exception          _tempError;
        private bool               _errorCleared;
        [DoNotReset]
        private IPrincipalContainer _principalContainer;
        [DoNotReset]
        internal ProfileBase       _Profile;
        [DoNotReset]
        private DateTime           _utcTimestamp;
        [DoNotReset]
        private HttpWorkerRequest  _wr;
        private VirtualPath        _configurationPath;
        internal bool              _skipAuthorization;
        [DoNotReset]
        private CultureInfo        _dynamicCulture;
        [DoNotReset]
        private CultureInfo        _dynamicUICulture;
        private int                _serverExecuteDepth;
        private Stack              _handlerStack;
        private bool               _preventPostback;
        private bool               _runtimeErrorReported;
        private PageInstrumentationService _pageInstrumentationService = null;
        private ReadOnlyCollection<string> _webSocketRequestedProtocols;

        // timeout support
        [DoNotReset]
        private CancellationTokenHelper _timeoutCancellationTokenHelper; // used for TimedOutToken

        private long       _timeoutStartTimeUtcTicks = -1; // should always be accessed atomically; -1 means uninitialized
        private long       _timeoutTicks = -1; // should always be accessed atomically; -1 means uninitialized
        private int        _timeoutState;   // 0=non-cancelable, 1=cancelable, -1=canceled
        private DoubleLink _timeoutLink;    // link in the timeout's manager list
        private bool       _threadAbortOnTimeout = true; // whether we should Thread.Abort() this thread when it times out
        private Thread     _thread;

        // cached configuration
        private CachedPathData _configurationPathData; // Cached data if _configurationPath != null
        private CachedPathData _filePathData;   // Cached data of the file being requested

        // Sql Cache Dependency
        private string _sqlDependencyCookie;

        // Session State
        volatile SessionStateModule _sessionStateModule;
        volatile bool               _delayedSessionState;   // Delayed session state item

        // non-compiled pages
        private TemplateControl _templateControl;

        // integrated pipeline state

        // For the virtual Disposing / Disposed events
        private SubscriptionQueue<Action<HttpContext>> _requestCompletedQueue;
        [DoNotReset]
        private SubscriptionQueue<IDisposable> _pipelineCompletedQueue;

        // keep synchronized with mgdhandler.hxx
        private const int FLAG_NONE                          =   0x0;
        private const int FLAG_CHANGE_IN_SERVER_VARIABLES    =   0x1;
        private const int FLAG_CHANGE_IN_REQUEST_HEADERS     =   0x2;
        private const int FLAG_CHANGE_IN_RESPONSE_HEADERS    =   0x4;
        private const int FLAG_CHANGE_IN_USER_OBJECT         =   0x8;
        private const int FLAG_SEND_RESPONSE_HEADERS         =  0x10;
        private const int FLAG_RESPONSE_HEADERS_SENT         =  0x20;
        internal const int FLAG_ETW_PROVIDER_ENABLED         =  0x40;
        private const int FLAG_CHANGE_IN_RESPONSE_STATUS     =  0x80;

        private volatile NotificationContext _notificationContext;
        private bool _isAppInitialized;
        [DoNotReset]
        private bool _isIntegratedPipeline;
        private bool _finishPipelineRequestCalled;
        [DoNotReset]
        private bool _impersonationEnabled;

        internal bool HideRequestResponse;
        internal volatile bool InIndicateCompletion;
        internal volatile ThreadContext IndicateCompletionContext = null;
        internal volatile Thread ThreadInsideIndicateCompletion = null;


        // This field is a surrogate for the HttpContext object itself. Our HostExecutionContextManager
        // shouldn't capture a reference to the HttpContext itself since these references could be long-lived,
        // e.g. if they're captured by a call to ThreadPool.QueueUserWorkItem or a Timer. This would cause the
        // associated HttpContext object graph to be long-lived, which would negatively affect performance.
        // Instead we capture a reference to this 'Id' object, which allows the HostExecutionContextManager
        // to compare the original captured HttpContext with the current HttpContext without actually
        // holding on to the original HttpContext instance.
        [DoNotReset]
        internal readonly object ThreadContextId = new object();

        // synchronization context (for EAP / TAP models)
        private AspNetSynchronizationContextBase _syncContext;

        // This field doesn't need to be volatile since it will only ever be written to by a single thread, and when that thread
        // later reads the field it will be guaranteed non-null. We don't care what other threads see, since it will never be
        // equal to Thread.CurrentThread for them regardless of whether those threads are seeing the latest value of this field.
        // This field should not be marked [DoNotReset] since we want it to be cleared when WebSocket processing begins.
        internal Thread _threadWhichStartedWebSocketTransition;

        // WebSocket state
        [DoNotReset]
        private WebSocketTransitionState _webSocketTransitionState; // see comments in WebSocketTransitionState.cs for detailed info on this enum
        [DoNotReset]
        private string _webSocketNegotiatedProtocol;

        // see comments on WebSocketInitStatus for what all of these codes mean
        private WebSocketInitStatus GetWebSocketInitStatus() {
            IIS7WorkerRequest iis7wr =_wr as IIS7WorkerRequest;
            if (iis7wr == null) {
                return WebSocketInitStatus.RequiresIntegratedMode;
            }

            if (CurrentNotification <= RequestNotification.BeginRequest) {
                return WebSocketInitStatus.CannotCallFromBeginRequest;
            }

            if (!iis7wr.IsWebSocketRequest()) {
                if (iis7wr.IsWebSocketModuleActive()) {
                    return WebSocketInitStatus.NotAWebSocketRequest;
                }
                else {
                    return WebSocketInitStatus.NativeModuleNotEnabled;
                }
            }

            if (iis7wr.GetIsChildRequest()) {
                return WebSocketInitStatus.CurrentRequestIsChildRequest;
            }

            return WebSocketInitStatus.Success;
        }
      
        // Returns true if the request contained the initial WebSocket handshake
        // and IIS's WebSocket module is active.
        public bool IsWebSocketRequest {
            get {
                // If AcceptWebSocketRequest has already been called and run to completion, then this
                // is obviously a WebSocket request and we can skip further checks (which might throw).
                if (IsWebSocketRequestUpgrading) {
                    return true;
                }

                switch (GetWebSocketInitStatus()) {
                    case WebSocketInitStatus.RequiresIntegratedMode:
                        throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));

                    case WebSocketInitStatus.CannotCallFromBeginRequest:
                        throw new InvalidOperationException(SR.GetString(SR.WebSockets_CannotBeCalledDuringBeginRequest));

                    case WebSocketInitStatus.Success:
                        return true;

                    default:
                        return false;
                }
            }
        }

        // While unwinding an HTTP request this indicates if the developer 
        // told ASP.NET that they wanted to transition to a websocket request
        public bool IsWebSocketRequestUpgrading {
            get { return (WebSocketTransitionState >= WebSocketTransitionState.AcceptWebSocketRequestCalled); }
        }

        internal bool HasWebSocketRequestTransitionStarted {
            get { return WebSocketTransitionState >= WebSocketTransitionState.TransitionStarted; }
        }

        internal bool HasWebSocketRequestTransitionCompleted {
            get { return WebSocketTransitionState >= WebSocketTransitionState.TransitionCompleted; }
        }

        internal WebSocketTransitionState WebSocketTransitionState {
            get { return _webSocketTransitionState; }
            private set { _webSocketTransitionState = value; }
        }

        // Returns the ordered list of protocols requested by the client,
        // or an empty collection if this wasn't a WebSocket request or there was no list present.
        public IList<string> WebSocketRequestedProtocols {
            get {
                if (IsWebSocketRequest) {
                    if (_webSocketRequestedProtocols == null) {
                        string rawHeaderValue = _wr.GetUnknownRequestHeader("Sec-WebSocket-Protocol");
                        IList<string> requestedProtocols = SubProtocolUtil.ParseHeader(rawHeaderValue); // checks for invalid values
                        _webSocketRequestedProtocols = new ReadOnlyCollection<string>(requestedProtocols ?? new string[0]);
                    }
                    return _webSocketRequestedProtocols;
                }
                else {
                    // not a WebSocket request
                    return null;
                }
            }
        }

        // Returns the negotiated protocol (sent from the server to the client) for a
        // WebSocket request.
        public string WebSocketNegotiatedProtocol {
            get { return _webSocketNegotiatedProtocol; }
        }

        public void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc) {
            AcceptWebSocketRequest(userFunc, null);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This is a safe critical method.")]
        public void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc, AspNetWebSocketOptions options) {
            // Begin argument & state checking

            // We throw different error codes depending on the check that failed. Things that are
            // server configuration errors (WebSockets not enabled) or developer errors (called this
            // method with bad parameters) result in an appropriate exception type. Things that are
            // remote errors (e.g. bad parameters from the client) result in an HTTP 4xx.

            if (userFunc == null) {
                throw new ArgumentNullException("userFunc");
            }

            if (IsWebSocketRequestUpgrading) {
                // this method cannot be called multiple times
                throw new InvalidOperationException(SR.GetString(SR.WebSockets_AcceptWebSocketRequestCanOnlyBeCalledOnce));
            }

            // DevDiv #384514: Task<T> doesn't work correctly using the legacy SynchronizationContext setting. Since
            // WebSockets operation requires correct Task<T> behavior, we should forbid using the feature when legacy
            // mode is enabled.
            SynchronizationContextUtil.ValidateModeForWebSockets();

            switch (GetWebSocketInitStatus()) {
                case WebSocketInitStatus.RequiresIntegratedMode:
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));

                case WebSocketInitStatus.CannotCallFromBeginRequest:
                    throw new InvalidOperationException(SR.GetString(SR.WebSockets_CannotBeCalledDuringBeginRequest));

                case WebSocketInitStatus.NativeModuleNotEnabled:
                    throw new PlatformNotSupportedException(SR.GetString(SR.WebSockets_WebSocketModuleNotEnabled));

                case WebSocketInitStatus.NotAWebSocketRequest:
                    throw new HttpException((int)HttpStatusCode.BadRequest, SR.GetString(SR.WebSockets_NotAWebSocketRequest));

                case WebSocketInitStatus.CurrentRequestIsChildRequest:
                    throw new InvalidOperationException(SR.GetString(SR.WebSockets_CannotBeCalledDuringChildExecute));

                case WebSocketInitStatus.Success:
                    break;

                default:
                    // fallback error message - not a WebSocket request
                    throw new HttpException(SR.GetString(SR.WebSockets_UnknownErrorWhileAccepting));
            }

            if (CurrentNotification > RequestNotification.ExecuteRequestHandler) {
                // it is too late to call this method
                throw new InvalidOperationException(SR.GetString(SR.WebSockets_CannotBeCalledAfterHandlerExecute));
            }
            // End argument & state checking

            IIS7WorkerRequest wr = (IIS7WorkerRequest)_wr;

            // Begin options checking and parsing
            if (options != null && options.RequireSameOrigin) {
                if (!WebSocketUtil.IsSameOriginRequest(wr)) {
                    // use Forbidden (HTTP 403) since it's not an authentication error; it's a usage error
                    throw new HttpException((int)HttpStatusCode.Forbidden, SR.GetString(SR.WebSockets_OriginCheckFailed));
                }
            }

            string subprotocol = null;
            if (options != null && !String.IsNullOrEmpty(options.SubProtocol)) {
                // AspNetWebSocketOptions.set_SubProtocol() already checked that the provided value is valid
                subprotocol = options.SubProtocol;
            }

            if (subprotocol != null) {
                IList<string> incomingProtocols = WebSocketRequestedProtocols;
                if (incomingProtocols == null || !incomingProtocols.Contains(subprotocol, StringComparer.Ordinal)) {
                    // The caller requested a subprotocol that wasn't in the list of accepted protocols coming from the client.
                    // This is disallowed by the WebSockets protocol spec, Sec. 5.2.2 (#2).
                    throw new ArgumentException(SR.GetString(SR.WebSockets_SubProtocolCannotBeNegotiated, subprotocol), "options");
                }
            }
            // End options checking and parsing

            wr.AcceptWebSocket();

            // transition: Inactive -> AcceptWebSocketRequestCalled
            TransitionToWebSocketState(WebSocketTransitionState.AcceptWebSocketRequestCalled);

            Response.StatusCode = (int)HttpStatusCode.SwitchingProtocols; // 101
            if (subprotocol != null) {
                Response.AppendHeader("Sec-WebSocket-Protocol", subprotocol);
                _webSocketNegotiatedProtocol = subprotocol;
            }
            RootedObjects.WebSocketPipeline = new WebSocketPipeline(RootedObjects, this, userFunc, subprotocol);
        }

        internal void TransitionToWebSocketState(WebSocketTransitionState newState) {
            // Make sure the state transition is happening in the correct order
#if DBG
            WebSocketTransitionState expectedOldState = checked(newState - 1);
            Debug.Assert(WebSocketTransitionState == expectedOldState, String.Format(CultureInfo.InvariantCulture, "Expected WebSocketTransitionState to be '{0}', but it was '{1}'.", expectedOldState, WebSocketTransitionState));
#endif

            WebSocketTransitionState = newState;
            if (newState == Web.WebSocketTransitionState.TransitionStarted) {
                _threadWhichStartedWebSocketTransition = Thread.CurrentThread;
            }
        }

        internal bool DidCurrentThreadStartWebSocketTransition {
            get {
                return _threadWhichStartedWebSocketTransition == Thread.CurrentThread;
            }
        }

        // helper that throws an exception if we have transitioned the current request to a WebSocket request
        internal void EnsureHasNotTransitionedToWebSocket() {
            if (HasWebSocketRequestTransitionCompleted) {
                throw new NotSupportedException(SR.GetString(SR.WebSockets_MethodNotAvailableDuringWebSocketProcessing));
            }
        }

        internal bool FirstRequest {get; set;}

        // session state support
        private bool _requiresSessionStateFromHandler;
        internal bool RequiresSessionState {
            get {
                switch (SessionStateBehavior) {
                    case SessionStateBehavior.Required:
                    case SessionStateBehavior.ReadOnly:
                        return true;
                    case SessionStateBehavior.Disabled:
                        return false;
                    case SessionStateBehavior.Default:
                    default:
                        return _requiresSessionStateFromHandler;
                }
            }
        }

        private bool _readOnlySessionStateFromHandler;
        internal bool ReadOnlySessionState {
            get {
                switch (SessionStateBehavior) {
                    case SessionStateBehavior.ReadOnly:
                        return true;
                    case SessionStateBehavior.Required:
                    case SessionStateBehavior.Disabled:
                        return false;
                    case SessionStateBehavior.Default:
                    default:
                        return _readOnlySessionStateFromHandler;
                }
            }
        }
        internal bool InAspCompatMode;

        private IHttpHandler _remapHandler = null;

        /// <include file='doc\HttpContext.uex' path='docs/doc[@for="HttpContext.HttpContext"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the HttpContext class.
        ///    </para>
        /// </devdoc>
        public HttpContext(HttpRequest request, HttpResponse response) {
            Init(request, response);
            request.Context = this;
            response.Context = this;
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the HttpContext class.
        ///    </para>
        /// </devdoc>
        public HttpContext(HttpWorkerRequest wr) {
            _wr = wr;
            Init(new HttpRequest(wr, this), new HttpResponse(wr, this));
            _response.InitResponseWriter();
        }

        // ctor used in HttpRuntime
        internal HttpContext(HttpWorkerRequest wr, bool initResponseWriter) {
            _wr = wr;
            Init(new HttpRequest(wr, this), new HttpResponse(wr, this));

            if (initResponseWriter)
                _response.InitResponseWriter();

            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
        }

        private void Init(HttpRequest request, HttpResponse response) {
            _request = request;
            _response = response;
            _utcTimestamp = DateTime.UtcNow;
            _principalContainer = this;

            if (_wr is IIS7WorkerRequest) {
                _isIntegratedPipeline = true;
            }

            if (!(_wr is System.Web.SessionState.StateHttpWorkerRequest))
                CookielessHelper.RemoveCookielessValuesFromPath(); // This ensures that the cookieless-helper is initialized and
            // rewrites the path if the URI contains cookieless form-auth ticket, session-id, etc.

            Profiler p = HttpRuntime.Profile;
            if (p != null && p.IsEnabled)
                _topTraceContext = new TraceContext(this);

            // rewrite path in order to remove "/eurl.axd/guid", if it was
            // added to the URL by aspnet_filter.dll.
            string eurl = GetEurl();
            if (!String.IsNullOrEmpty(eurl)) {
                string path = request.Path;
                int idxStartEurl = path.Length - eurl.Length;
                bool hasTrailingSlash = (path[path.Length - 1] == '/');
                if (hasTrailingSlash) {
                    idxStartEurl--;
                }
                if (idxStartEurl >= 0
                    && StringUtil.Equals(path, idxStartEurl, eurl, 0, eurl.Length)) {                    
                    // restore original URL
                    int originalUrlLen = idxStartEurl;
                    if (hasTrailingSlash) {
                        originalUrlLen++;
                    }
                    string originalUrl = path.Substring(0, originalUrlLen);
                    // Dev10 835901: We don't call HttpContext.RewritePath(path) because the 
                    // original path may contain '?' encoded as %3F, and RewritePath
                    // would interpret what follows as the query string.  So instead, we
                    // clear ConfigurationPath and call InternalRewritePath directly.
                    ConfigurationPath = null;
                    Request.InternalRewritePath(VirtualPath.Create(originalUrl), null, true);
                }
            }
        }

        // We have a feature that directs extensionless URLs
        // into managed code by appending "/eurl.axd/guid" to the path.  On IIS 6.0,
        // we restore the URL as soon as we get into managed code.  Here we  get the
        // actual value of "/eurl.axd/guid" and remember it.
        private string GetEurl() {
            // only used on IIS 6.0
            if (!(_wr is ISAPIWorkerRequestInProcForIIS6)
                || (_wr is ISAPIWorkerRequestInProcForIIS7)) {
                return null;
            }

            string eurl = s_eurl;
            if (eurl == null && !s_eurlSet) {
                try {
                    IntPtr pBuffer = UnsafeNativeMethods.GetExtensionlessUrlAppendage();
                    if (pBuffer != IntPtr.Zero) {
                        eurl = StringUtil.StringFromWCharPtr(pBuffer, UnsafeNativeMethods.lstrlenW(pBuffer));
                    }
                }
                catch {} // ignore all exceptions
                s_eurl = eurl;
                s_eurlSet = true;
            }
            return eurl;
        }

        // Current HttpContext off the call context
#if DBG
        internal static void SetDebugAssertOnAccessToCurrent(bool doAssert) {
            if (doAssert) {
                CallContext.SetData("__ContextAssert", String.Empty);
            }
            else {
                CallContext.SetData("__ContextAssert", null);
            }
        }

        private static bool NeedDebugAssertOnAccessToCurrent {
            get {
                return (CallContext.GetData("__ContextAssert") != null);
            }
        }
#endif

        /// <devdoc>
        ///    <para>Returns the current HttpContext object.</para>
        /// </devdoc>
        public static HttpContext Current {
            get {
#if DBG
                if (NeedDebugAssertOnAccessToCurrent) {
                    Debug.Assert(ContextBase.Current != null);
                }
#endif
                return ContextBase.Current as HttpContext;
            }

            set {
                ContextBase.Current = value;
            }
        }

        //
        //  Root / unroot for the duration of async operation
        //  These are only used for the classic pipeline. The integrated pipeline uses a different rooting mechanism.
        //

        private IntPtr _rootedPtr;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This is a safe critical method.")]
        internal void Root() {
            _rootedPtr = GCUtil.RootObject(this);
        }

        internal void Unroot() {
            GCUtil.UnrootObject(_rootedPtr);
            _rootedPtr = IntPtr.Zero;
        }

        internal void FinishPipelineRequest() {
            if (!_finishPipelineRequestCalled) {
                _finishPipelineRequestCalled = true;
                HttpRuntime.FinishPipelineRequest(this);
            }
        }

        // This is a virtual event which occurs when the HTTP part of this request is winding down, e.g. after EndRequest
        // but before the WebSockets pipeline kicks in. The HttpContext is still available for inspection and is provided
        // as a parameter to the supplied callback.
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = @"The normal event pattern doesn't work between HttpContext and HttpContextBase since the signatures differ.")]
        public ISubscriptionToken AddOnRequestCompleted(Action<HttpContext> callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }

            return _requestCompletedQueue.Enqueue(callback);
        }

        internal void RaiseOnRequestCompleted() {
            // The callbacks really shouldn't throw exceptions, but we have a catch block just in case.
            // Since there's nobody else that can listen for these errors (the request is unwinding and
            // user code will no longer run), we'll just log the error.
            try {
                _requestCompletedQueue.FireAndComplete(action => action(this));
            }
            catch (Exception e) {
                WebBaseEvent.RaiseRuntimeError(e, this);
            }
            finally {
                // Dispose of TimedOutToken so that nobody tries using it after this point.
                DisposeTimedOutToken();
            }
        }

        // Allows an object's Dispose() method to be called when the pipeline part of this request is completed, e.g.
        // after both the HTTP part and the WebSockets loop have completed. The HttpContext is not available for
        // inspection, and HttpContext.Current will be null.
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This is a safe critical method.")]
        public ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target) {
            if (target == null) {
                throw new ArgumentNullException("target");
            }

            if (RootedObjects != null) {
                // integrated pipeline
                return RootedObjects.DisposeOnPipelineCompleted(target);
            }
            else {
                // classic pipeline
                return _pipelineCompletedQueue.Enqueue(target);
            }
        }

        internal void RaiseOnPipelineCompleted() {
            // The callbacks really shouldn't throw exceptions, but we have a catch block just in case.
            // Since there's nobody else that can listen for these errors (the request is unwinding and
            // user code will no longer run), we'll just log the error.
            try {
                _pipelineCompletedQueue.FireAndComplete(disposable => disposable.Dispose());
            }
            catch (Exception e) {
                WebBaseEvent.RaiseRuntimeError(e, null);
            }
        }

        internal void ValidatePath() {
            CachedPathData pathData = GetConfigurationPathData();
            pathData.ValidatePath(_request.PhysicalPathInternal);
        }


        // IServiceProvider implementation

        /// <internalonly/>
        Object IServiceProvider.GetService(Type service) {
            Object obj;

            if (service == typeof(HttpWorkerRequest)) {
                InternalSecurityPermissions.UnmanagedCode.Demand();
                obj = _wr;
            }
            else if (service == typeof(HttpRequest))
                obj = Request;
            else if (service == typeof(HttpResponse))
                obj = Response;
            else if (service == typeof(HttpApplication))
                obj = ApplicationInstance;
            else if (service == typeof(HttpApplicationState))
                obj = Application;
            else if (service == typeof(HttpSessionState))
                obj = Session;
            else if (service == typeof(HttpServerUtility))
                obj = Server;
            else
                obj = null;

            return obj;
        }

        //
        // Async app handler is remembered for the duration of execution of the
        // request when application happens to be IHttpAsyncHandler. It is needed
        // for HttpRuntime to remember the object on which to call OnEndRequest.
        //
        // The assumption is that application is a IHttpAsyncHandler, not always
        // HttpApplication.
        //
        internal IHttpAsyncHandler AsyncAppHandler {
            get { return _asyncAppHandler; }
            set { _asyncAppHandler = value; }
        }

        public AsyncPreloadModeFlags AsyncPreloadMode {
            get {
                if (!_asyncPreloadModeFlagsSet) {
                    _asyncPreloadModeFlags = RuntimeConfig.GetConfig(this).HttpRuntime.AsyncPreloadMode;
                    _asyncPreloadModeFlagsSet = true;
                }
                return _asyncPreloadModeFlags;
            }
            set {
                _asyncPreloadModeFlags = value; 
                _asyncPreloadModeFlagsSet = true;
            }
        }

        // If this flag is not set, the AspNetSynchronizationContext associated with this request will throw
        // exceptions when it detects the application misusing the async API. This can occur if somebody
        // tries to call SynchronizationContext.Post / OperationStarted / etc. during a part of the
        // pipeline where we weren't expecting asynchronous work to take place, if there is still
        // outstanding asynchronous work when an asynchronous module or handler signals completion, etc.
        // It is meant as a safety net to let developers know early on when they're writing async code
        // which doesn't fit our expected patterns and where that code likely has negative side effects.
        // 
        // This flag is respected only by AspNetSynchronizationContext; it has no effect when the
        // legacy sync context is in use.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool AllowAsyncDuringSyncStages {
            get {
                return SyncContext.AllowAsyncDuringSyncStages;
            }
            set {
                SyncContext.AllowAsyncDuringSyncStages = value;
            }
        }

        /// <devdoc>
        ///    <para>Retrieves a reference to the application object for the current Http request.</para>
        /// </devdoc>
        public HttpApplication ApplicationInstance {
            get {
                return _appInstance;
            }
            set {
                // For integrated pipeline, once this is set to a non-null value, it can only be set to null.
                // The setter should never have been made public.  It probably happened in 1.0, before it was possible
                // to have getter and setter with different accessibility.
                if (_isIntegratedPipeline && _appInstance != null && value != null) {
                    throw new InvalidOperationException(SR.GetString(SR.Application_instance_cannot_be_changed));
                }
                else {
                    _appInstance = value;

                    // Use HttpApplication instance custom allocator provider
                    if (_isIntegratedPipeline) {
                        // The provider allows null - everyone should fallback to default implementation
                        IAllocatorProvider allocator = _appInstance != null ? _appInstance.AllocatorProvider : null;

                        _response.SetAllocatorProvider(allocator);
                        ((IIS7WorkerRequest)_wr).AllocatorProvider = allocator;
                    }
                }
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Retrieves a reference to the application object for the current
        ///       Http request.
        ///    </para>
        /// </devdoc>
        public HttpApplicationState Application {
            get { return HttpApplicationFactory.ApplicationState; }
        }


        // flag to suppress use of custom HttpEncoder registered in web.config
        // for example, yellow error pages should use the default encoder rather than a custom encoder
        internal bool DisableCustomHttpEncoder {
            get;
            set;
        }


        /// <devdoc>
        ///    <para>
        ///       Retrieves or assigns a reference to the <see cref='System.Web.IHttpHandler'/>
        ///       object for the current request.
        ///    </para>
        /// </devdoc>
        public IHttpHandler Handler {
            get { return _handler;}
            set {
                _handler = value;
                _requiresSessionStateFromHandler = false;
                _readOnlySessionStateFromHandler = false;
                InAspCompatMode = false;
                if (_handler != null) {
                    if (_handler is IRequiresSessionState) {
                        _requiresSessionStateFromHandler = true;
                    }
                    if (_handler is IReadOnlySessionState) {
                        _readOnlySessionStateFromHandler = true;
                    }
                    Page page = _handler as Page;
                    if (page != null && page.IsInAspCompatMode) {
                        InAspCompatMode = true;
                    }
                }
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Retrieves or assigns a reference to the <see cref='System.Web.IHttpHandler'/>
        ///       object for the previous handler;
        ///    </para>
        /// </devdoc>

        public IHttpHandler PreviousHandler {
            get {
                if (_handlerStack == null || _handlerStack.Count == 0)
                    return null;

                return (IHttpHandler)_handlerStack.Peek();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Retrieves or assigns a reference to the <see cref='System.Web.IHttpHandler'/>
        ///       object for the current executing handler;
        ///    </para>
        /// </devdoc>
        private IHttpHandler _currentHandler = null;

        public IHttpHandler CurrentHandler {
            get {
                if (_currentHandler == null)
                    _currentHandler = _handler;

                return _currentHandler;
            }
        }

        internal void RestoreCurrentHandler() {
            _currentHandler = (IHttpHandler)_handlerStack.Pop();
        }

        internal void SetCurrentHandler(IHttpHandler newtHandler) {
            if (_handlerStack == null) {
                _handlerStack = new Stack();
            }
            _handlerStack.Push(CurrentHandler);

            _currentHandler = newtHandler;
        }

        /// <devdoc>
        ///    <para>
        ///       Set custom mapping handler processing the request <see cref='System.Web.IHttpHandler'/>
        ///    </para>
        /// </devdoc>
        public void RemapHandler(IHttpHandler handler) {
            EnsureHasNotTransitionedToWebSocket();

            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;

            if (wr != null) {
                // Remap handler not allowed after ResolveRequestCache notification
                if (_notificationContext.CurrentNotification >= RequestNotification.MapRequestHandler) {
                    throw new InvalidOperationException(SR.GetString(SR.Invoke_before_pipeline_event, "HttpContext.RemapHandler", "HttpApplication.MapRequestHandler"));
                }

                string handlerTypeName = null;
                string handlerName = null;

                if (handler != null) {
                    Type handlerType = handler.GetType();

                    handlerTypeName = handlerType.AssemblyQualifiedName;
                    handlerName = handlerType.FullName;
                }

                wr.SetRemapHandler(handlerTypeName, handlerName);
            }

            _remapHandler = handler;
        }

        internal IHttpHandler RemapHandlerInstance {
            get {
                return _remapHandler;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Retrieves a reference to the target <see cref='System.Web.HttpRequest'/>
        ///       object for the current request.
        ///    </para>
        /// </devdoc>
        public HttpRequest Request {
            get {
                 if (HideRequestResponse)
                    throw new HttpException(SR.GetString(SR.Request_not_available));
                return _request;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Retrieves a reference to the <see cref='System.Web.HttpResponse'/>
        ///       object for the current response.
        ///    </para>
        /// </devdoc>
        public HttpResponse Response {
            get {
                if (HideRequestResponse || HasWebSocketRequestTransitionCompleted)
                    throw new HttpException(SR.GetString(SR.Response_not_available));
                return _response;
            }
        }


        internal IHttpHandler TopHandler {
            get {
                if (_handlerStack == null) {
                    return _handler;
                }
                object[] handlers = _handlerStack.ToArray();
                if (handlers == null || handlers.Length == 0) {
                    return _handler;
                }
                return (IHttpHandler)handlers[handlers.Length - 1];
            }
        }


        /// <devdoc>
        /// <para>Retrieves a reference to the <see cref='System.Web.TraceContext'/> object for the current
        ///    response.</para>
        /// </devdoc>
        public TraceContext Trace {
            get {
                if (_topTraceContext == null)
                    _topTraceContext = new TraceContext(this);
                return _topTraceContext;
            }
        }

        internal bool TraceIsEnabled {
            get {
                if (_topTraceContext == null)
                    return false;

                return _topTraceContext.IsEnabled;
            }
            set {
                if (value)
                    _topTraceContext = new TraceContext(this);
            }

        }



        /// <devdoc>
        ///    <para>
        ///       Retrieves a key-value collection that can be used to
        ///       build up and share data between an <see cref='System.Web.IHttpModule'/> and an <see cref='System.Web.IHttpHandler'/>
        ///       during a
        ///       request.
        ///    </para>
        /// </devdoc>
        public IDictionary Items {
            get {
                if (_items == null)
                    _items = new Hashtable();

                return _items;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a reference to the <see cref='System.Web.SessionState'/> instance for the current request.
        ///    </para>
        /// </devdoc>
        public HttpSessionState Session {
            get {
                if (HasWebSocketRequestTransitionCompleted) {
                    // Session is unavailable at this point
                    return null;
                }

                if (_delayedSessionState) {
                    lock (this) {
                        if (_delayedSessionState) {
                            Debug.Assert(_sessionStateModule != null, "_sessionStateModule != null");

                            // If it's not null, it means we have a delayed session state item
                            _sessionStateModule.InitStateStoreItem(true);
                            _delayedSessionState = false;
                        }
                    }
                }

                return(HttpSessionState)Items[SessionStateUtility.SESSION_KEY];
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void EnsureSessionStateIfNecessary() {
            if (_sessionStateModule == null)
            {
                // If _sessionStateModule is null, we wouldn't be able to call 
                // _sessionStateModule.EnsureStateStoreItemLocked(), so we return here.
                // _sessionStateModule could be null in the following cases,
                // 1. No session state acquired.
                // 2. HttpResponse.Flush() happens after session state being released.
                // 3. The session state module in use is not System.Web.SessionState.SessionStateModule.
                //
                // This method is for the in-framework SessionStateModule only.
                //  OOB SessionStateModule can achieve this by using HttpResponse.AddOnSendingHeaders. 
                return;
            }

            HttpSessionState session = (HttpSessionState)Items[SessionStateUtility.SESSION_KEY];

            if (session != null &&                                 // The session has been initiated
                session.Count > 0 &&                               // The session state is used
                !string.IsNullOrEmpty(session.SessionID)) {        // Ensure the session Id is valid - it will force to create new if didn't exist
                _sessionStateModule.EnsureStateStoreItemLocked();  // Lock the item if in use
            }
        }


        internal void AddHttpSessionStateModule(SessionStateModule module, bool delayed) {
            if (_sessionStateModule != null && _sessionStateModule != module) {
                throw new HttpException(SR.GetString(SR.Cant_have_multiple_session_module));
            }
            _sessionStateModule = module;
            _delayedSessionState = delayed;
        }

        internal void RemoveHttpSessionStateModule() {
            _delayedSessionState = false;
            _sessionStateModule = null;
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a reference to the <see cref='System.Web.HttpServerUtility'/>
        ///       for the current
        ///       request.
        ///    </para>
        /// </devdoc>
        public HttpServerUtility Server {
            get {
                // create only on demand
                if (_server == null)
                    _server = new HttpServerUtility(this);
                return _server;
            }
        }

        // if the context has an error, report it, but only one time
        internal void ReportRuntimeErrorIfExists(ref RequestNotificationStatus status) {
            Exception e = Error;

            if (e == null || _runtimeErrorReported) {
                return;
            }

            // WOS 1921799: custom errors don't work in integrated mode if there's an initialization exception
            if (_notificationContext != null && CurrentModuleIndex == -1) {
                try {
                    IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
                    if (Request.QueryString["aspxerrorpath"] != null
                        && wr != null
                        && String.IsNullOrEmpty(wr.GetManagedHandlerType())
                        && wr.GetCurrentModuleName() == PipelineRuntime.InitExceptionModuleName) {
                        status = RequestNotificationStatus.Continue;   // allow non-managed handler to execute request
                        return;
                    }
                }
                catch {
                }
            }

            _runtimeErrorReported = true;

            if (HttpRuntime.AppOfflineMessage != null) {
                try {
                    // report app offline error
                    Response.TrySkipIisCustomErrors = true;
                    HttpRuntime.ReportAppOfflineErrorMessage(Response, HttpRuntime.AppOfflineMessage);

                }
                catch {
                }
            }
            else {
                // report error exception
                using (new DisposableHttpContextWrapper(this)) {

                    // if the custom encoder throws, it might interfere with returning error information
                    // to the client, so we force use of the default encoder
                    DisableCustomHttpEncoder = true;

                    // when application is on UNC share the code below must
                    // be run while impersonating the token given by IIS
                    using (new ApplicationImpersonationContext()) {

                        try {
                            try {
                                // try to report error in a way that could possibly throw (a config exception)
                                Response.ReportRuntimeError(e, true /*canThrow*/, false);
                            }
                            catch (Exception eReport) {
                                // report the config error in a way that would not throw
                                Response.ReportRuntimeError(eReport, false /*canThrow*/, false);
                            }
                        }
                        catch (Exception) {
                        }
                    }
                }
            }

            status = RequestNotificationStatus.FinishRequest;
            return;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the
        ///       first error (if any) accumulated during request processing.
        ///    </para>
        /// </devdoc>
        public Exception Error {
            get {
                if (_tempError != null)
                    return _tempError;
                if (_errors == null || _errors.Count == 0 || _errorCleared)
                    return null;
                return (Exception)_errors[0];
            }
        }

        //
        // Temp error (yet to be caught on app level)
        // to be reported as Server.GetLastError() but could be cleared later
        //
        internal Exception TempError {
            get { return _tempError; }
            set { _tempError = value; }
        }


        /// <devdoc>
        ///    <para>
        ///       An array (collection) of errors accumulated while processing a
        ///       request.
        ///    </para>
        /// </devdoc>
        public Exception[] AllErrors {
            get {
                int n = (_errors != null) ? _errors.Count : 0;

                if (n == 0)
                    return null;

                Exception[] errors = new Exception[n];
                _errors.CopyTo(0, errors, 0, n);
                return errors;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Registers an error for the current request.
        ///    </para>
        /// </devdoc>
        public void AddError(Exception errorInfo) {
            if (_errors == null)
                _errors = new ArrayList();

            _errors.Add(errorInfo);

            if (_isIntegratedPipeline && _notificationContext != null) {
                // set the error on the current notification context
                _notificationContext.Error = errorInfo;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Clears all errors for the current request.
        ///    </para>
        /// </devdoc>
        public void ClearError() {
            if (_tempError != null)
                _tempError = null;
            else
                _errorCleared = true;

            if (_isIntegratedPipeline && _notificationContext != null) {
                // clear the error on the current notification context
                _notificationContext.Error = null;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       IPrincipal security information.
        ///    </para>
        /// </devdoc>
        public IPrincipal User {
            get { return _principalContainer.Principal; }

            [SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
            set {
                SetPrincipalNoDemand(value);
            }
        }

        IPrincipal IPrincipalContainer.Principal {
            get;
            set;
        }

        // route all internals call to the principal (that don't have luring attacks)
        // through this method so we can centralize reporting
        // Before this, some auth modules were assigning directly to _user
        internal void SetPrincipalNoDemand(IPrincipal principal, bool needToSetNativePrincipal) {
            _principalContainer.Principal = principal;

            // push changes through to native side
            if (needToSetNativePrincipal
                && _isIntegratedPipeline
                && _notificationContext.CurrentNotification == RequestNotification.AuthenticateRequest) {

                IntPtr pManagedPrincipal = IntPtr.Zero;
                IIS7WorkerRequest wr = (IIS7WorkerRequest)_wr;
                wr.SetPrincipal(principal);
            }
        }

        internal void SetPrincipalNoDemand(IPrincipal principal) {
            SetPrincipalNoDemand(principal, true /*needToSetNativePrincipal*/);
        }

        [DoNotReset]
        internal bool _ProfileDelayLoad = false;

        public ProfileBase  Profile {
            get {
                if (_Profile == null && _ProfileDelayLoad)
                    _Profile = ProfileBase.Create(Request.IsAuthenticated ? User.Identity.Name : Request.AnonymousID, Request.IsAuthenticated);
                return _Profile;
            }
        }

        internal SessionStateBehavior SessionStateBehavior { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
          Justification = "An internal property already exists. This method does additional work.")]
        public void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior) {
            if (_notificationContext != null && _notificationContext.CurrentNotification >= RequestNotification.AcquireRequestState) {
                throw new InvalidOperationException(SR.GetString(SR.Invoke_before_pipeline_event, "HttpContext.SetSessionStateBehavior", "HttpApplication.AcquireRequestState"));
            }

            SessionStateBehavior = sessionStateBehavior;
        }


        public bool SkipAuthorization {
            get { return _skipAuthorization;}

            [SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
            set {
                SetSkipAuthorizationNoDemand(value, false);
            }
        }

        internal void SetSkipAuthorizationNoDemand(bool value, bool managedOnly)
        {
            if (HttpRuntime.UseIntegratedPipeline
                && !managedOnly
                && value != _skipAuthorization) {

                // For integrated mode, persist changes to SkipAuthorization
                // in the IS_LOGIN_PAGE server variable.  When this server variable exists
                // and the value is not "0", IIS skips authorization.

                _request.SetSkipAuthorization(value);
            }

            _skipAuthorization = value;
        }

        // Pointer to the RootedObjects element, which contains information that needs to be flowed
        // between the HttpContext and the WebSocket, such as the current principal.
        [DoNotReset]
        private RootedObjects _rootedObjects;
        
        internal RootedObjects RootedObjects {
            get {
                return _rootedObjects;
            }
            set {
                // Sync the Principal between the containers
                SwitchPrincipalContainer(value);
                _rootedObjects = value;
            }
        }

        private void SwitchPrincipalContainer(IPrincipalContainer newPrincipalContainer) {
            if (newPrincipalContainer == null) {
                newPrincipalContainer = this;
            }

            // Ensure new container contains the current principal
            IPrincipal currentPrincipal = _principalContainer.Principal;
            newPrincipalContainer.Principal = currentPrincipal;
            _principalContainer = newPrincipalContainer;
        }

        /// <devdoc>
        ///    <para>
        ///       Is this request in debug mode?
        ///    </para>
        /// </devdoc>
        public bool IsDebuggingEnabled {
            get {
                try {
                    return CompilationUtil.IsDebuggingEnabled(this);
                }
                catch {
                    // in case of config errors don't throw
                    return false;
                }
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Is this custom error enabled for this request?
        ///    </para>
        /// </devdoc>
        public bool IsCustomErrorEnabled {
            get {
                return CustomErrorsSection.GetSettings(this).CustomErrorsEnabled(_request);
            }
        }

        internal TemplateControl TemplateControl {
            get {
                return _templateControl;
            }
            set {
                _templateControl = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets the initial timestamp of the current request.</para>
        /// </devdoc>
        public DateTime Timestamp {
            get { return _utcTimestamp.ToLocalTime();}
        }

        internal DateTime UtcTimestamp {
            get { return _utcTimestamp;}
        }

        internal HttpWorkerRequest WorkerRequest {
            get { return _wr;}
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a reference to the System.Web.Cache.Cache object for the current request.
        ///    </para>
        /// </devdoc>
        public Cache Cache {
            get { return HttpRuntime.Cache;}
        }

        /// <summary>
        /// Gets a reference to the System.Web.Instrumentation.PageInstrumentationService instance for this request. Guaranteed not to be null (barring private reflection magic).
        /// </summary>
        public PageInstrumentationService PageInstrumentation {
            get { 
                if(_pageInstrumentationService == null) {
                    _pageInstrumentationService = new PageInstrumentationService();
                }
                return _pageInstrumentationService;
            }
        }

        /*
         * The virtual path used to get config settings.  This allows the user
         * to specify a non default config path, without having to pass it to every
         * configuration call.
         */
        internal VirtualPath ConfigurationPath {
            get {
                if (_configurationPath == null)
                    _configurationPath = _request.FilePathObject;

                return _configurationPath;
            }

            set {
                _configurationPath = value;
                _configurationPathData = null;
                _filePathData = null;
            }
        }

        internal CachedPathData GetFilePathData() {
            if (_filePathData == null) {
                _filePathData = CachedPathData.GetVirtualPathData(_request.FilePathObject, false);
            }

            return _filePathData;
        }

        internal CachedPathData GetConfigurationPathData() {
            if (_configurationPath == null) {
                return GetFilePathData();
            }

            // 
            if (_configurationPathData == null) {
                _configurationPathData = CachedPathData.GetVirtualPathData(_configurationPath, true);
            }

            return _configurationPathData;
        }

        internal CachedPathData GetPathData(VirtualPath path) {
            if (path != null) {
                if (path.Equals(_request.FilePathObject)) {
                    return GetFilePathData();
                }

                if (_configurationPath != null && path.Equals(_configurationPath)) {
                    return GetConfigurationPathData();
                }
            }

            return CachedPathData.GetVirtualPathData(path, false);
        }

        internal void FinishRequestForCachedPathData(int statusCode) {
            // Remove the cached path data for a file path if the first request for it
            // does not succeed due to a bad request. Otherwise we could be vulnerable
            // to a DOS attack.
            if (_filePathData != null && !_filePathData.CompletedFirstRequest) {
                if (400 <= statusCode && statusCode < 500) {
                    CachedPathData.RemoveBadPathData(_filePathData);
                }
                else {
                    CachedPathData.MarkCompleted(_filePathData);
                }
            }
        }

        /*
         * Uses the Config system to get the specified configuraiton
         */
        [Obsolete("The recommended alternative is System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection in System.Web.dll. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static object GetAppConfig(String name) {
            return WebConfigurationManager.GetWebApplicationSection(name);
        }

        [Obsolete("The recommended alternative is System.Web.HttpContext.GetSection in System.Web.dll. http://go.microsoft.com/fwlink/?linkid=14202")]
        public object GetConfig(String name) {
            return GetSection(name);
        }

        public object GetSection(String sectionName) {
            if (HttpConfigurationSystem.UseHttpConfigurationSystem) {
                return GetConfigurationPathData().ConfigRecord.GetSection(sectionName);
            }
            else {
                return ConfigurationManager.GetSection(sectionName);
            }
        }

        internal RuntimeConfig GetRuntimeConfig() {
            return GetConfigurationPathData().RuntimeConfig;
        }

        internal RuntimeConfig GetRuntimeConfig(VirtualPath path) {
            return GetPathData(path).RuntimeConfig;
        }

        public void RewritePath(String path) {
            RewritePath(path, true);
        }

        /*
         * Called by the URL rewrite module to modify the path for downstream modules
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void RewritePath(String path, bool rebaseClientPath) {
            if (path == null)
                throw new ArgumentNullException("path");

            // extract query string
            String qs = null;
            int iqs = path.IndexOf('?');
            if (iqs >= 0) {
                qs = (iqs < path.Length-1) ? path.Substring(iqs+1) : String.Empty;
                path = path.Substring(0, iqs);
            }

            // resolve relative path
            VirtualPath virtualPath = VirtualPath.Create(path);
            virtualPath = Request.FilePathObject.Combine(virtualPath);

            // disallow paths outside of app
            virtualPath.FailIfNotWithinAppRoot();

            // clear things that depend on path
            ConfigurationPath = null;

            // rewrite path on request
            Request.InternalRewritePath(virtualPath, qs, rebaseClientPath);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void RewritePath(String filePath, String pathInfo, String queryString) {
            RewritePath(VirtualPath.CreateAllowNull(filePath), VirtualPath.CreateAllowNull(pathInfo),
                queryString, false /*setClientFilePath*/);
        }
        public void RewritePath(string filePath, string pathInfo, String queryString, bool setClientFilePath)
        {
            RewritePath(VirtualPath.CreateAllowNull(filePath), VirtualPath.CreateAllowNull(pathInfo), queryString, setClientFilePath);
        }
        internal void RewritePath(VirtualPath filePath, VirtualPath pathInfo, String queryString, bool setClientFilePath) {
            EnsureHasNotTransitionedToWebSocket();

            if (filePath == null)
                throw new ArgumentNullException("filePath");

            // resolve relative path
            filePath = Request.FilePathObject.Combine(filePath);

            // disallow paths outside of app
            filePath.FailIfNotWithinAppRoot();

            // clear things that depend on path
            ConfigurationPath = null;

            // rewrite path on request
            Request.InternalRewritePath(filePath, pathInfo, queryString, setClientFilePath);
        }

        internal CultureInfo DynamicCulture {
            get { return _dynamicCulture; }
            set { _dynamicCulture = value; }
        }

        internal CultureInfo DynamicUICulture {
            get { return _dynamicUICulture; }
            set { _dynamicUICulture = value; }
        }

        public static object GetGlobalResourceObject(string classKey, string resourceKey) {
            return GetGlobalResourceObject(classKey, resourceKey, null);
        }

        public static object GetGlobalResourceObject(string classKey, string resourceKey, CultureInfo culture) {
            return ResourceExpressionBuilder.GetGlobalResourceObject(classKey, resourceKey, null, null, culture);
        }

        public static object GetLocalResourceObject(string virtualPath, string resourceKey) {
            return GetLocalResourceObject(virtualPath, resourceKey, null);
        }

        public static object GetLocalResourceObject(string virtualPath, string resourceKey, CultureInfo culture) {
            IResourceProvider pageProvider = ResourceExpressionBuilder.GetLocalResourceProvider(
                VirtualPath.Create(virtualPath));
            return ResourceExpressionBuilder.GetResourceObject(pageProvider, resourceKey, culture);
        }

        internal int ServerExecuteDepth {
            get { return _serverExecuteDepth; }
            set { _serverExecuteDepth = value; }
        }

        internal bool PreventPostback {
            get { return _preventPostback; }
            set { _preventPostback = value; }
        }

        //
        // Timeout support
        //

        internal Thread CurrentThread {
            get {
                return _thread;
            }
            set {
                _thread = value;
            }
        }

        // Property is thread-safe since needs to be accessed by RequestTimeoutManager in addition to
        // normal request threads.
        internal TimeSpan Timeout {
            get {
                long ticks = EnsureTimeout();
                return TimeSpan.FromTicks(ticks);
            }

            set {
                Interlocked.Exchange(ref _timeoutTicks, value.Ticks);
            }
        }

        // Access via HttpRequest.TimedOutToken instead.
        internal CancellationToken TimedOutToken {
            get {
                // If we are the first call site to observe the token, then create it in the non-canceled state.
                CancellationTokenHelper helper = LazyInitializer.EnsureInitialized(ref _timeoutCancellationTokenHelper, () => new CancellationTokenHelper(canceled: false));
                return helper.Token;
            }
        }

        /// <summary>
        /// Determines whether the ASP.NET runtime calls Thread.Abort() on the thread servicing this request when
        /// the request times out. Default value is 'true'.
        /// </summary>
        /// <remarks>
        /// Handlers and modules that are using Request.TimedOutToken to implement cooperative cancellation may
        /// wish to disable the rude Thread.Abort behavior that ASP.NET has historically performed when a request
        /// times out. This can help developers make sure that their g----ful cancellation + cleanup routines
        /// will run without interruption by ASP.NET.
        /// 
        /// The rules for determining when a thread is aborted are somewhat complicated, so applications shouldn't
        /// try to depend on them. Currently, the behavior is:
        /// 
        /// - The thread will be aborted at some point after Request.TimedOutToken is canceled. The abort might not
        ///   occur immediately afterward, as the "should Thread.Abort" timer is separate from the "should signal
        ///   the CancellationToken" timer.
        /// 
        /// - We generally don't abort threads that are processing async modules or handlers. There are some
        ///   exceptions. E.g., during certain parts of the lifecycle for async WebForms pages, the thread can be
        ///   a candidate to be aborted when a timeout occurs.
        ///   
        /// If a developer sets this property to 'false', ASP.NET will not automatically display a "Request timed
        /// out" YSOD when a timeout occurs. If this happens the application is responsible for setting the response
        /// content appropriately.
        /// </remarks>
        public bool ThreadAbortOnTimeout {
            get { return Volatile.Read(ref _threadAbortOnTimeout); }
            set { Volatile.Write(ref _threadAbortOnTimeout, value); }
        }

        private void DisposeTimedOutToken() {
            // If we are the first call site to observe the token, then create it in the disposed state.
            CancellationTokenHelper helper = LazyInitializer.EnsureInitialized(ref _timeoutCancellationTokenHelper, () => CancellationTokenHelper.StaticDisposed);
            helper.Dispose();
        }

        internal long EnsureTimeout() {
            // Calls to Volatile.* are atomic, even for 64-bit fields.
            long ticks = Volatile.Read(ref _timeoutTicks);
            if (ticks == -1) {
                // Only go to config if the value hasn't yet been initialized.
                HttpRuntimeSection cfg = RuntimeConfig.GetConfig(this).HttpRuntime;
                ticks = cfg.ExecutionTimeout.Ticks;

                // If another thread already came in and initialized _timeoutTicks,
                // return that value instead of the value we just read from config.
                long originalTicks = Interlocked.CompareExchange(ref _timeoutTicks, ticks, -1);
                if (originalTicks != -1) {
                    ticks = originalTicks;
                }
            }

            return ticks;
        }

        internal DoubleLink TimeoutLink {
            get { return _timeoutLink;}
            set { _timeoutLink = value;}
        }

        /*

        Notes on the following 5 functions:

        Execution can be cancelled only during certain periods, when inside the catch
        block for ThreadAbortException.  These periods are marked with the value of
        _timeoutState of 1.

        There is potential [rare] race condition when the timeout thread would call
        thread.abort but the execution logic in the meantime escapes the catch block.
        To avoid such race conditions _timeoutState of -1 (cancelled) is introduced.
        The timeout thread sets _timeoutState to -1 before thread abort and the
        unwinding logic just waits for the exception in this case. The wait cannot
        be done in EndCancellablePeriod because the function is call from inside of
        a finally block and thus would wait indefinetely. That's why another function
        WaitForExceptionIfCancelled had been added.

        Originally _timeoutStartTime was set in BeginCancellablePeriod. However, that means
        we'll call UtcNow everytime we call ExecuteStep, which is too expensive. So to save
        CPU time we created a new method SetStartTime() which is called by the caller of
        ExecuteStep.

        */

        internal void BeginCancellablePeriod() {
            // It could be caused by an exception in OnThreadStart
            if (Volatile.Read(ref _timeoutStartTimeUtcTicks) == -1) {
                SetStartTime();
            }

            Volatile.Write(ref _timeoutState, 1);
        }

        internal void SetStartTime() {
            Interlocked.Exchange(ref _timeoutStartTimeUtcTicks, DateTime.UtcNow.Ticks);
        }

        internal void EndCancellablePeriod() {
            Interlocked.CompareExchange(ref _timeoutState, 0, 1);
        }

        internal void WaitForExceptionIfCancelled() {
            while (Volatile.Read(ref _timeoutState) == -1)
                Thread.Sleep(100);
        }

        internal bool IsInCancellablePeriod {
            get { return (Volatile.Read(ref _timeoutState) == 1); }
        }

        internal Thread MustTimeout(DateTime utcNow) {
            // Note: The TimedOutToken is keyed off of the HttpContext creation time, not the most recent async
            // completion time (like the Thread.Abort logic later in this method).

            if (_utcTimestamp + Timeout < utcNow) {
                // If we are the first call site to observe the token, then create it in the canceled state.
                CancellationTokenHelper helper = LazyInitializer.EnsureInitialized(ref _timeoutCancellationTokenHelper, () => new CancellationTokenHelper(canceled: true));
                helper.Cancel();
            }

            if (Volatile.Read(ref _timeoutState) == 1 && ThreadAbortOnTimeout) {  // fast check
                long expirationUtcTicks = Volatile.Read(ref _timeoutStartTimeUtcTicks) + Timeout.Ticks; // don't care about overflow
                if (expirationUtcTicks < utcNow.Ticks) {
                    // don't abort in debug mode
                    try {
                        if (CompilationUtil.IsDebuggingEnabled(this) || System.Diagnostics.Debugger.IsAttached)
                            return null;
                    }
                    catch {
                        // ignore config errors
                        return null;
                    }

                    // abort the thread only if in cancelable state, avoiding race conditions
                    // the caller MUST timeout if the return is true
                    if (Interlocked.CompareExchange(ref _timeoutState, -1, 1) == 1) {
                        if (_wr.IsInReadEntitySync) {
                            AbortConnection();
                        }
                        return _thread;
                    }
                }
            }

            return null;
        }

        internal bool HasTimeoutExpired {
            get {
                // Check if it is allowed to timeout
                if (Volatile.Read(ref _timeoutState) != 1 || !ThreadAbortOnTimeout) {
                    return false;
                }

                // Check if the timeout has expired
                long expirationUtcTicks = Volatile.Read(ref _timeoutStartTimeUtcTicks) + Timeout.Ticks; // don't care about overflow
                if (expirationUtcTicks >= DateTime.UtcNow.Ticks) {
                    return false;
                }

                // Dont't timeout when in debug
                try {
                    if (CompilationUtil.IsDebuggingEnabled(this) || System.Diagnostics.Debugger.IsAttached) {
                        return false;
                    }
                }
                catch {
                    // ignore config errors
                    return false;
                }

                return true;
            }
        }

        // call a delegate within cancellable period (possibly throwing timeout exception)
        internal void InvokeCancellableCallback(WaitCallback callback, Object state) {
            if (IsInCancellablePeriod) {
                // call directly
                callback(state);
                return;
            }

            try {
                BeginCancellablePeriod();  // request can be cancelled from this point

                try {
                    callback(state);
                }
                finally {
                    EndCancellablePeriod();  // request can be cancelled until this point
                }

                WaitForExceptionIfCancelled();  // wait outside of finally
            }
            catch (ThreadAbortException e) {
                if (e.ExceptionState != null &&
                    e.ExceptionState is HttpApplication.CancelModuleException &&
                    ((HttpApplication.CancelModuleException)e.ExceptionState).Timeout) {

                    Thread.ResetAbort();
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TIMED_OUT);

                    throw new HttpException(SR.GetString(SR.Request_timed_out),
                                        null, WebEventCodes.RuntimeErrorRequestAbort);
                }
            }
        }

        internal void PushTraceContext() {
            if (_traceContextStack == null) {
                _traceContextStack = new Stack();
            }

            // push current TraceContext on stack
            _traceContextStack.Push(_topTraceContext);

            // now make a new one for the top if necessary
            if (_topTraceContext != null) {
                TraceContext tc = new TraceContext(this);
                _topTraceContext.CopySettingsTo(tc);
                _topTraceContext = tc;
            }
        }

        internal void PopTraceContext() {
            Debug.Assert(_traceContextStack != null);
            _topTraceContext = (TraceContext) _traceContextStack.Pop();
        }

        internal bool RequestRequiresAuthorization()  {
#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            // if current user is anonymous, then trivially, this page does not require authorization
            if (!User.Identity.IsAuthenticated)
                return false;

            // Ask each of the authorization modules
            return
                ( FileAuthorizationModule.RequestRequiresAuthorization(this) ||
                  UrlAuthorizationModule.RequestRequiresAuthorization(this)   );
#else // !FEATURE_PAL
                return false; // ROTORTODO
#endif // !FEATURE_PAL
        }

        internal int CallISAPI(UnsafeNativeMethods.CallISAPIFunc iFunction, byte [] bufIn, byte [] bufOut) {

            if (_wr == null || !(_wr is System.Web.Hosting.ISAPIWorkerRequest))
                throw new HttpException(SR.GetString(SR.Cannot_call_ISAPI_functions));
#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            return ((System.Web.Hosting.ISAPIWorkerRequest) _wr).CallISAPI(iFunction, bufIn, bufOut);
#else // !FEATURE_PAL
                throw new NotImplementedException ("ROTORTODO");
#endif // !FEATURE_PAL
        }

        internal void SendEmptyResponse() {
#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            if (_wr != null  && (_wr is System.Web.Hosting.ISAPIWorkerRequest))
                ((System.Web.Hosting.ISAPIWorkerRequest) _wr).SendEmptyResponse();
#endif // !FEATURE_PAL
        }

        private  CookielessHelperClass _CookielessHelper;
        internal CookielessHelperClass  CookielessHelper {
            get {
                if (_CookielessHelper == null)
                    _CookielessHelper = new CookielessHelperClass(this);
                return _CookielessHelper;
            }
        }


        // When a thread enters the pipeline, we may need to set the cookie in the CallContext.
        internal void ResetSqlDependencyCookie() {
            if (_sqlDependencyCookie != null) {
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(SqlCacheDependency.SQL9_OUTPUT_CACHE_DEPENDENCY_COOKIE, _sqlDependencyCookie);
            }
        }

        // When a thread leaves the pipeline, we may need to remove the cookie from the CallContext.
        internal void RemoveSqlDependencyCookie() {
            if (_sqlDependencyCookie != null) {
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(SqlCacheDependency.SQL9_OUTPUT_CACHE_DEPENDENCY_COOKIE, null);
            }
        }

        internal string SqlDependencyCookie {
            get {
                return _sqlDependencyCookie;
            }

            set {
                _sqlDependencyCookie = value;
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(SqlCacheDependency.SQL9_OUTPUT_CACHE_DEPENDENCY_COOKIE, value);
            }
        }

        //
        // integrated pipeline related
        //
        internal NotificationContext NotificationContext {
            get { return _notificationContext; }
            set { _notificationContext = value; }
        }

        public RequestNotification CurrentNotification {
            get {
                EnsureHasNotTransitionedToWebSocket();

                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }

                return _notificationContext.CurrentNotification;
            }
            internal set {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }

                _notificationContext.CurrentNotification = value;
            }
        }

        internal bool IsChangeInServerVars {
            get { return (_notificationContext.CurrentNotificationFlags & FLAG_CHANGE_IN_SERVER_VARIABLES) == FLAG_CHANGE_IN_SERVER_VARIABLES; }
        }

        internal bool IsChangeInRequestHeaders {
            get { return (_notificationContext.CurrentNotificationFlags & FLAG_CHANGE_IN_REQUEST_HEADERS) == FLAG_CHANGE_IN_REQUEST_HEADERS; }
        }

        internal bool IsChangeInResponseHeaders {
            get { return (_notificationContext.CurrentNotificationFlags & FLAG_CHANGE_IN_RESPONSE_HEADERS) == FLAG_CHANGE_IN_RESPONSE_HEADERS; }
        }

        internal bool IsChangeInResponseStatus {
            get { return (_notificationContext.CurrentNotificationFlags & FLAG_CHANGE_IN_RESPONSE_STATUS) == FLAG_CHANGE_IN_RESPONSE_STATUS; }
        }

        internal bool IsChangeInUserPrincipal {
            get { return (_notificationContext.CurrentNotificationFlags & FLAG_CHANGE_IN_USER_OBJECT) == FLAG_CHANGE_IN_USER_OBJECT; }
        }

        internal bool IsSendResponseHeaders {
            get { return (_notificationContext.CurrentNotificationFlags & FLAG_SEND_RESPONSE_HEADERS) == FLAG_SEND_RESPONSE_HEADERS; }
        }

        internal void SetImpersonationEnabled() {
            IdentitySection c = RuntimeConfig.GetConfig(this).Identity;
            _impersonationEnabled = (c != null && c.Impersonate);
        }

        internal bool UsesImpersonation {
            get {
                // if we're on a UNC share and we have a UNC token, then use impersonation for all notifications
                if (HttpRuntime.IsOnUNCShareInternal && HostingEnvironment.ApplicationIdentityToken != IntPtr.Zero) {
                    return true;
                }
                // if <identity impersonate=/> is false, then don't use impersonation
                if (!_impersonationEnabled) {
                    return false;
                }
                // the notification context won't be available after we have completed the transition
                if (HasWebSocketRequestTransitionCompleted) {
                    return true;
                }

                // if this notification is after AuthenticateRequest and not a SendResponse notification, use impersonation
                return (((_notificationContext.CurrentNotification == RequestNotification.AuthenticateRequest && _notificationContext.IsPostNotification)
                        || _notificationContext.CurrentNotification > RequestNotification.AuthenticateRequest)
                        && _notificationContext.CurrentNotification != RequestNotification.SendResponse);
            }
        }

        internal bool AreResponseHeadersSent {
            get { return (_notificationContext.CurrentNotificationFlags & FLAG_RESPONSE_HEADERS_SENT) == FLAG_RESPONSE_HEADERS_SENT; }
        }

        internal bool NeedToInitializeApp() {
            bool needToInit = !_isAppInitialized;
            if (needToInit) {
                _isAppInitialized = true;
            }
            return needToInit;
        }

        // flags passed in on the call to PipelineRuntime::ProcessRequestNotification
        internal int CurrentNotificationFlags {
            get {
                return _notificationContext.CurrentNotificationFlags;
            }
            set {
                _notificationContext.CurrentNotificationFlags = value;
            }
        }

        // index of the current "module" running the request
        // into the application module array
        internal int CurrentModuleIndex {
            get {
                return _notificationContext.CurrentModuleIndex;
            }
            set {
                _notificationContext.CurrentModuleIndex = value;
            }
        }

        // Each module has a PipelineModuleStepContainer
        // which stores/manages a list of event handlers
        // that correspond to each RequestNotification.
        // CurrentModuleEventIndex is the index (for the current
        // module) of the current event handler.
        // This will be greater than one when a single
        // module registers multiple delegates for a single event.
        // e.g.
        // app.BeginRequest += Foo;
        // app.BeginRequest += Bar;
        internal int CurrentModuleEventIndex {
            get {
                return _notificationContext.CurrentModuleEventIndex;
            }
            set {
                _notificationContext.CurrentModuleEventIndex = value;
            }
        }

        internal void DisableNotifications(RequestNotification notifications, RequestNotification postNotifications) {
            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
            if (null != wr) {
                wr.DisableNotifications(notifications, postNotifications);
            }
        }

        public bool IsPostNotification {
            get {
                EnsureHasNotTransitionedToWebSocket();

                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                return _notificationContext.IsPostNotification;
            }
            internal set {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                _notificationContext.IsPostNotification = value;
            }

        }

        // user token for the request
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This is a safe critical method.")]
        internal IntPtr ClientIdentityToken {
            get {
                if (_wr != null) {
                    return _wr.GetUserToken();
                }
                else {
                    return IntPtr.Zero;
                }
            }
        }

        // is configured to impersonate client?
        internal bool IsClientImpersonationConfigured {
            get {
                try {
                    IdentitySection c = RuntimeConfig.GetConfig(this).Identity;
                    return (c != null && c.Impersonate && c.ImpersonateToken == IntPtr.Zero);
                }
                catch {
                    // this property should not throw as it is used in the error reporting pass
                    // config errors will be reported elsewhere
                    return false;
                }
            }
        }

        internal IntPtr ImpersonationToken {
            get {
                // by default use app identity
                IntPtr token = HostingEnvironment.ApplicationIdentityToken;
                IdentitySection c = RuntimeConfig.GetConfig(this).Identity;
                if (c != null) {
                    if (c.Impersonate) {
                        token = (c.ImpersonateToken != IntPtr.Zero) ? c.ImpersonateToken : ClientIdentityToken;
                    }
                    else {
                        // for non-UNC case impersonate="false" means "don't impersonate",
                        // but there is a special case for UNC shares - even if
                        // impersonate="false" we still impersonate the UNC identity
                        // (hosting identity). and this is how v1.x works as well
                        if (!HttpRuntime.IsOnUNCShareInternal) {
                            token = IntPtr.Zero;
                        }
                    }
                }
                return token;
            }
        }

        internal AspNetSynchronizationContextBase SyncContext {
            get {
                if (_syncContext == null) {
                    _syncContext = CreateNewAspNetSynchronizationContext();
                }

                return _syncContext;
            }
            set {
                _syncContext = value;
            }
        }

        internal AspNetSynchronizationContextBase InstallNewAspNetSynchronizationContext() {
            AspNetSynchronizationContextBase syncContext = _syncContext;

            if (syncContext != null && syncContext == AsyncOperationManager.SynchronizationContext) {
                // using current ASP.NET synchronization context - switch it
                _syncContext = CreateNewAspNetSynchronizationContext();
                AsyncOperationManager.SynchronizationContext = _syncContext;
                return syncContext;
            }

            return null;
        }

        private AspNetSynchronizationContextBase CreateNewAspNetSynchronizationContext() {
            if (!AppSettings.UseTaskFriendlySynchronizationContext) {
                return new LegacyAspNetSynchronizationContext(ApplicationInstance);
            }
            else {
                return new AspNetSynchronizationContext(ApplicationInstance);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This is a safe critical method.")]
        internal void RestoreSavedAspNetSynchronizationContext(AspNetSynchronizationContextBase syncContext) {
            AsyncOperationManager.SynchronizationContext = syncContext;
            _syncContext = syncContext;
        }

        internal string[] UserLanguagesFromContext() {
            return (Request != null) ? Request.UserLanguages : null;
        }

        // References should be nulled a.s.a.p. to reduce working set
        internal void ClearReferences() {
            _appInstance = null;
            _handler = null;
            _handlerStack = null;
            _currentHandler = null;
            _remapHandler = null;
            if (_isIntegratedPipeline) {
                if (!HasWebSocketRequestTransitionStarted) {
                    // Items is also used by AspNetWebSocketContext and should only be cleared if we're not transitioning to WebSockets
                    _items = null;
                }
                _syncContext = null;
            }
        }

        internal void CompleteTransitionToWebSocket() {
            ClearReferencesForWebSocketProcessing();

            // transition: TransitionStarted -> TransitionCompleted
            TransitionToWebSocketState(WebSocketTransitionState.TransitionCompleted);
        }

        // This is much stronger than just ClearReferences; it tries to free absolutely as much memory as possible.
        // Some necessary items (like _wr, etc.) are preserved. The reason we want to modify this particular instance
        // in-place rather than create a new instance is that it is likely that references to this object still exist,
        // and we don't want the existence of those references to cause memory leaks.
        private void ClearReferencesForWebSocketProcessing() {
            HttpResponse response = _response;

            // everything not marked [DoNotReset] should be eligible for garbage collection
            ReflectionUtil.Reset(this);

            // Miscellaneous steps:
            _request.ClearReferencesForWebSocketProcessing(); // also clean up the HttpRequest instance
            if (response != null) {
                // HttpResponse is off-limits, but it is possible that the developer accidentally maintained a reference
                // to it, e.g. via a closure. We'll release the HttpResponse's references to all its data to prevent
                // this from causing memory problems.
                ReflectionUtil.Reset(response);
            }
        }

        internal CultureInfo CultureFromConfig(string configString, bool requireSpecific) {
            //auto
            if(StringUtil.EqualsIgnoreCase(configString, HttpApplication.AutoCulture)) {
                string[] userLanguages = UserLanguagesFromContext();
                if (userLanguages != null) {
                    try {
                        return CultureUtil.CreateReadOnlyCulture(userLanguages, requireSpecific);
                    }
                    catch {
                        return null;
                    }
                }
                else {
                    return null;
                }
            }
            else if(StringUtil.StringStartsWithIgnoreCase(configString, "auto:")) {
                string[] userLanguages = UserLanguagesFromContext();
                if (userLanguages != null) {
                    try {
                        return CultureUtil.CreateReadOnlyCulture(userLanguages, requireSpecific);
                    }
                    catch {
                        return CultureUtil.CreateReadOnlyCulture(configString.Substring(5 /* "auto:".Length */), requireSpecific);
                    }
                }
                else {
                    return CultureUtil.CreateReadOnlyCulture(configString.Substring(5 /* "auto:".Length */), requireSpecific);
                }
            }

            return CultureUtil.CreateReadOnlyCulture(configString, requireSpecific);
        }

        private enum WebSocketInitStatus {
            Success, // iiswsock.dll is active and has told us that the current request is a WebSocket request
            RequiresIntegratedMode, // WebSockets requires integrated mode, and the current server is not Integrated mode
            CannotCallFromBeginRequest, // We need to wait for BeginRequest to complete before the module has set the server variables
            NativeModuleNotEnabled, // iiswsock.dll isn't active in the pipeline
            NotAWebSocketRequest, // iiswsock.dll is active, but the current request is not a WebSocket request
            CurrentRequestIsChildRequest, // We are currently inside of a child request (IHttpContext::ExecuteRequest)
        }

        private void AbortConnection() {
            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;

            if (wr != null) { 
                // Direct API Abort is suported in integrated mode only
                wr.AbortConnection();
            }
            else {
                // Close in classic mode acts as Abort (see HSE_REQ_CLOSE_CONNECTION) 
                // It closes the underlined connection
                _wr.CloseConnection();
            }
        }
    }

    //
    // Helper class to add/remove HttpContext to/from CallContext
    //
    // using (new DisposableHttpContextWrapper(context)) {
    //     // this code will have HttpContext.Current working
    // }
    //

    internal class DisposableHttpContextWrapper : IDisposable {
        private bool _needToUndo;
        private HttpContext _savedContext;

        internal static HttpContext SwitchContext(HttpContext context) {
            return ContextBase.SwitchContext(context) as HttpContext;
        }

        internal DisposableHttpContextWrapper(HttpContext context) {
            if (context != null) {
                _savedContext = SwitchContext(context);
                _needToUndo = (_savedContext != context);
            }
        }

        void IDisposable.Dispose() {
            if (_needToUndo) {
                SwitchContext(_savedContext);
                _savedContext = null;
                _needToUndo = false;
            }
        }
    }
}
