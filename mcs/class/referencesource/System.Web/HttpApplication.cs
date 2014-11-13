//------------------------------------------------------------------------------
// <copyright file="HttpApplication.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Configuration.Common;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.Util;
    using IIS = System.Web.Hosting.UnsafeIISMethods;


    //
    // Async EventHandler support
    //


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public delegate IAsyncResult BeginEventHandler(object sender, EventArgs e, AsyncCallback cb, object extraData);

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public delegate void EndEventHandler(IAsyncResult ar);

    // Represents an event handler using TAP (Task Asynchronous Pattern).
    public delegate Task TaskEventHandler(object sender, EventArgs e);


    /// <devdoc>
    ///    <para>
    ///       The  HttpApplication class defines the methods, properties and events common to all
    ///       HttpApplication objects within the ASP.NET Framework.
    ///    </para>
    /// </devdoc>
    [
    ToolboxItem(false)
    ]
    public class HttpApplication : IComponent, IHttpAsyncHandler, IRequestCompletedNotifier, ISyncContext {
        // application state dictionary
        private HttpApplicationState _state;

        // context during init for config lookups
        private HttpContext _initContext;

        // async support
        private HttpAsyncResult _ar; // currently pending async result for call into application

        // list of modules
        private static readonly DynamicModuleRegistry _dynamicModuleRegistry = new DynamicModuleRegistry();
        private HttpModuleCollection  _moduleCollection;

        // event handlers
        private static readonly object EventDisposed = new object();
        private static readonly object EventErrorRecorded = new object();
        private static readonly object EventRequestCompleted = new object();
        private static readonly object EventPreSendRequestHeaders = new object();
        private static readonly object EventPreSendRequestContent = new object();

        private static readonly object EventBeginRequest = new object();
        private static readonly object EventAuthenticateRequest = new object();
        private static readonly object EventDefaultAuthentication = new object();
        private static readonly object EventPostAuthenticateRequest = new object();
        private static readonly object EventAuthorizeRequest = new object();
        private static readonly object EventPostAuthorizeRequest = new object();
        private static readonly object EventResolveRequestCache = new object();
        private static readonly object EventPostResolveRequestCache = new object();
        private static readonly object EventMapRequestHandler = new object();
        private static readonly object EventPostMapRequestHandler = new object();
        private static readonly object EventAcquireRequestState = new object();
        private static readonly object EventPostAcquireRequestState = new object();
        private static readonly object EventPreRequestHandlerExecute = new object();
        private static readonly object EventPostRequestHandlerExecute = new object();
        private static readonly object EventReleaseRequestState = new object();
        private static readonly object EventPostReleaseRequestState = new object();
        private static readonly object EventUpdateRequestCache = new object();
        private static readonly object EventPostUpdateRequestCache = new object();
        private static readonly object EventLogRequest = new object();
        private static readonly object EventPostLogRequest = new object();
        private static readonly object EventEndRequest = new object();
        internal static readonly string AutoCulture = "auto";

        private EventHandlerList _events;
        private AsyncAppEventHandlersTable _asyncEvents;

        // execution steps
        private StepManager _stepManager;

        // callback for Application ResumeSteps
        #pragma warning disable 0649
        private WaitCallback _resumeStepsWaitCallback;
        #pragma warning restore 0649

        // event passed to modules
        private EventArgs _appEvent;

        // list of handler mappings
        private Hashtable _handlerFactories = new Hashtable();

        // list of handler/factory pairs to be recycled
        private ArrayList _handlerRecycleList;

        // flag to hide request and response intrinsics
        private bool _hideRequestResponse;

        // application execution variables
        private HttpContext _context;
        private Exception _lastError;  // placeholder for the error when context not avail
        private bool _timeoutManagerInitialized;

        // session (supplied by session-on-end outside of context)
        private HttpSessionState _session;

        // culture (needs to be set per thread)
        private CultureInfo _appLevelCulture;
        private CultureInfo _appLevelUICulture;
        private CultureInfo _savedAppLevelCulture;
        private CultureInfo _savedAppLevelUICulture;
        private bool _appLevelAutoCulture;
        private bool _appLevelAutoUICulture;

        // pipeline event mappings
        private Dictionary<string, RequestNotification> _pipelineEventMasks;


        // IComponent support
        private ISite _site;

        // IIS7 specific fields
        internal const string MANAGED_PRECONDITION = "managedHandler";
        internal const string IMPLICIT_FILTER_MODULE = "AspNetFilterModule";
        internal const string IMPLICIT_HANDLER = "ManagedPipelineHandler";

        // map modules to their index
        private static Hashtable _moduleIndexMap = new Hashtable();
        private static bool _initSpecialCompleted;

        private bool _initInternalCompleted;
        private RequestNotification _appRequestNotifications;
        private RequestNotification _appPostNotifications;

        // Set the current module init key to the global.asax module to enable
        // the custom global.asax derivation constructor to register event handlers
        private string _currentModuleCollectionKey = HttpApplicationFactory.applicationFileName;

        // module config is read once per app domain and used to initialize the per-instance _moduleContainers array
        private static List<ModuleConfigurationInfo> _moduleConfigInfo;

        // this is the per instance list that contains the events for each module
        private PipelineModuleStepContainer[] _moduleContainers;

        // Byte array to be used by HttpRequest.GetEntireRawContent. Windows OS Bug 1632921
        private byte[] _entityBuffer;

        // Counts the number of code paths consuming this HttpApplication instance. When the counter hits zero,
        // it is safe to release this HttpApplication instance back into the HttpApplication pool.
        // This counter can be null if we're not using the new Task-friendly code paths.
        internal CountdownTask ApplicationInstanceConsumersCounter;

        //
        // Public Application properties
        //


        /// <devdoc>
        ///    <para>
        ///          HTTPRuntime provided context object that provides access to additional
        ///          pipeline-module exposed objects.
        ///       </para>
        ///    </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpContext Context {
            get {
                return(_context != null) ? _context : _initContext;
            }
        }

        private bool IsContainerInitalizationAllowed {
            get {
                if (HttpRuntime.UseIntegratedPipeline && _initSpecialCompleted && !_initInternalCompleted) {
                    // return true if
                    //      i) this is integrated pipeline mode,
                    //     ii) InitSpecial has been called at least once in this AppDomain to register events with IIS,
                    //    iii) InitInternal has not been invoked yet or is currently executing
                    return true;
                }
                return false;
            }
        }

        private void ThrowIfEventBindingDisallowed() {
            if (HttpRuntime.UseIntegratedPipeline && _initSpecialCompleted && _initInternalCompleted) {
                // throw if we're using the integrated pipeline and both InitSpecial and InitInternal have completed.
                throw new InvalidOperationException(SR.GetString(SR.Event_Binding_Disallowed));
            }
        }

        private PipelineModuleStepContainer[] ModuleContainers {
            get {
                if (_moduleContainers == null) {

                    Debug.Assert(_moduleIndexMap != null && _moduleIndexMap.Count > 0, "_moduleIndexMap != null && _moduleIndexMap.Count > 0");

                    // At this point, all modules have been registered with IIS via RegisterIntegratedEvent.
                    // Now we need to create a container for each module and add execution steps.
                    // The number of containers is the same as the number of modules that have been
                    // registered (_moduleIndexMap.Count).

                    _moduleContainers = new PipelineModuleStepContainer[_moduleIndexMap.Count];

                    for (int i = 0; i < _moduleContainers.Length; i++) {
                        _moduleContainers[i] = new PipelineModuleStepContainer();
                    }

                }

                return _moduleContainers;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public event EventHandler Disposed {
            add {
                Events.AddHandler(EventDisposed, value);
            }

            remove {
                Events.RemoveHandler(EventDisposed, value);
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected EventHandlerList Events {
            get {
                if (_events == null) {
                    _events = new EventHandlerList();
                }
                return _events;
            }
        }

        internal IExecutionStep CreateImplicitAsyncPreloadExecutionStep() {
            ImplicitAsyncPreloadModule implicitAsyncPreloadModule = new ImplicitAsyncPreloadModule();
            BeginEventHandler beginHandler = null;
            EndEventHandler endHandler = null;
            implicitAsyncPreloadModule.GetEventHandlers(out beginHandler, out endHandler);
            return new AsyncEventExecutionStep(this, beginHandler, endHandler, null);            
        }

        private AsyncAppEventHandlersTable AsyncEvents {
            get {
                if (_asyncEvents == null)
                    _asyncEvents = new AsyncAppEventHandlersTable();
                return _asyncEvents;
            }
        }

        // Last error during the processing of the current request.
        internal Exception LastError {
            get {
                // only temporaraly public (will be internal and not related context)
                return (_context != null) ? _context.Error : _lastError;
            }

        }

        // Used by HttpRequest.GetEntireRawContent. Windows OS Bug 1632921
        internal byte[] EntityBuffer
        {
            get
            {
                if (_entityBuffer == null)
                {
                    _entityBuffer = new byte[8 * 1024];
                }
                return _entityBuffer;
            }
        }

        internal void ClearError() {
            _lastError = null;
        }

        /// <devdoc>
        ///    <para>HTTPRuntime provided request intrinsic object that provides access to incoming HTTP
        ///       request data.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpRequest Request {
            get {
                HttpRequest request = null;

                if (_context != null && !_hideRequestResponse)
                    request = _context.Request;

                if (request == null)
                    throw new HttpException(SR.GetString(SR.Request_not_available));

                return request;
            }
        }


        /// <devdoc>
        ///    <para>HTTPRuntime provided
        ///       response intrinsic object that allows transmission of HTTP response data to a
        ///       client.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpResponse Response {
            get {
                HttpResponse response = null;

                if (_context != null && !_hideRequestResponse)
                    response = _context.Response;

                if (response == null)
                    throw new HttpException(SR.GetString(SR.Response_not_available));

                return response;
            }
        }


        /// <devdoc>
        ///    <para>
        ///    HTTPRuntime provided session intrinsic.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpSessionState Session {
            get {
                HttpSessionState session = null;

                if (_session != null)
                    session = _session;
                else if (_context != null)
                    session = _context.Session;

                if (session == null)
                    throw new HttpException(SR.GetString(SR.Session_not_available));

                return session;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Returns
        ///          a reference to an HTTPApplication state bag instance.
        ///       </para>
        ///    </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpApplicationState Application {
            get {
                Debug.Assert(_state != null);  // app state always available
                return _state;
            }
        }


        /// <devdoc>
        ///    <para>Provides the web server Intrinsic object.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpServerUtility Server {
            get {
                if (_context != null)
                    return _context.Server;
                else
                    return new HttpServerUtility(this); // special Server for application only
            }
        }


        /// <devdoc>
        ///    <para>Provides the User Intrinsic object.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public IPrincipal User {
            get {
                if (_context == null)
                    throw new HttpException(SR.GetString(SR.User_not_available));

                return _context.User;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Collection
        ///          of all IHTTPModules configured for the current application.
        ///       </para>
        ///    </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpModuleCollection Modules {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get {
                if (_moduleCollection == null)
                    _moduleCollection = new HttpModuleCollection();
                return _moduleCollection;
            }
        }

        // event passed to all modules
        internal EventArgs AppEvent {
            get {
                if (_appEvent == null)
                    _appEvent = EventArgs.Empty;

                return _appEvent;
            }

            set {
                _appEvent = null;
            }
        }

        // DevDiv Bugs 151914: Release session state before executing child request
        internal void EnsureReleaseState() {
            if (_moduleCollection != null) {
                for (int i = 0; i < _moduleCollection.Count; i++) {
                    IHttpModule module = _moduleCollection.Get(i);
                    if (module is SessionStateModule) {
                        ((SessionStateModule) module).EnsureReleaseState(this);
                        break;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CompleteRequest() {
            //
            // Request completion (force skipping all steps until RequestEnd
            //
            _stepManager.CompleteRequest();
        }

        internal bool IsRequestCompleted {
            get {
                if (null == _stepManager) {
                    return false;
                }

                return _stepManager.IsCompleted;
            }
        }

        bool IRequestCompletedNotifier.IsRequestCompleted {
            get {
                return IsRequestCompleted;
            }
        }

        // Dev10 745301: Asynchronous pipeline steps can start a new thread that triggers
        // a SendResponse notification.  E.g., it might call Flush when a module is registered
        // for PreSendRequestHeaders/Content.  If the async pipeline step returns from ExecuteStep
        // while the SendResponse notification is executing, the NotificationContext can 
        // be corrupted.  To fix this, a lock is now taken to prevent multi-threaded access when
        // the async pipeline step sets the NotificationContext.PendingAsyncCompletion field.  
        // The SendResponse notification also acquires the lock when it enters managed code and 
        // releases the lock when it leaves.
        internal void AcquireNotifcationContextLock(ref bool locked) {
            Debug.Assert(HttpRuntime.UseIntegratedPipeline, "HttpRuntime.UseIntegratedPipeline");
            Monitor.Enter(_stepManager, ref locked);
        }

        internal void ReleaseNotifcationContextLock() {
            Debug.Assert(HttpRuntime.UseIntegratedPipeline, "HttpRuntime.UseIntegratedPipeline");
            Monitor.Exit(_stepManager);
        }

        private void RaiseOnError() {
            EventHandler handler = (EventHandler)Events[EventErrorRecorded];
            if (handler != null) {
                try {
                    handler(this, AppEvent);
                }
                catch (Exception e) {
                    if (_context != null) {
                        _context.AddError(e);
                    }
                }
            }
        }

        private void RaiseOnRequestCompleted() {
            EventHandler handler = (EventHandler)Events[EventRequestCompleted];
            if (handler != null) {
                try {
                    handler(this, AppEvent);
                }
                catch (Exception e) {
                    WebBaseEvent.RaiseRuntimeError(e, this);
                }
            }
        }

        internal void RaiseOnPreSendRequestHeaders() {
            EventHandler handler = (EventHandler)Events[EventPreSendRequestHeaders];
            if (handler != null) {
                try {
                    handler(this, AppEvent);
                }
                catch (Exception e) {
                    RecordError(e);
                }
            }
        }

        internal void RaiseOnPreSendRequestContent() {
            EventHandler handler = (EventHandler)Events[EventPreSendRequestContent];
            if (handler != null) {
                try {
                    handler(this, AppEvent);
                }
                catch (Exception e) {
                    RecordError(e);
                }
            }
        }

        internal HttpAsyncResult AsyncResult {
            get {
                if (HttpRuntime.UseIntegratedPipeline) {
                    return (_context.NotificationContext != null) ? _context.NotificationContext.AsyncResult : null;
                }
                else {
                    return _ar;
                }
            }
            set {
                if (HttpRuntime.UseIntegratedPipeline) {
                    _context.NotificationContext.AsyncResult = value;
                }
                else {
                    _ar = value;
                }
            }
        }

        internal void AddSyncEventHookup(object key, Delegate handler, RequestNotification notification) {
            AddSyncEventHookup(key, handler, notification, false);
        }

        private PipelineModuleStepContainer CurrentModuleContainer { get { return ModuleContainers[_context.CurrentModuleIndex]; } }

        private PipelineModuleStepContainer GetModuleContainer(string moduleName) {
            object value = _moduleIndexMap[moduleName];

            if (value == null) {
                return null;
            }

            int moduleIndex = (int)value;

#if DBG
            Debug.Trace("PipelineRuntime", "GetModuleContainer: moduleName=" + moduleName + ", index=" + moduleIndex.ToString(CultureInfo.InvariantCulture) + "\r\n");
            Debug.Assert(moduleIndex >= 0 && moduleIndex < ModuleContainers.Length, "moduleIndex >= 0 && moduleIndex < ModuleContainers.Length");
#endif

            PipelineModuleStepContainer container = ModuleContainers[moduleIndex];

            Debug.Assert(container != null, "container != null");

            return container;
        }

        private void AddSyncEventHookup(object key, Delegate handler, RequestNotification notification, bool isPostNotification) {
            ThrowIfEventBindingDisallowed();

            // add the event to the delegate invocation list
            // this keeps non-pipeline ASP.NET hosts working
            Events.AddHandler(key, handler);

            // For integrated pipeline mode, add events to the IExecutionStep containers only if
            // InitSpecial has completed and InitInternal has not completed.
            if (IsContainerInitalizationAllowed) {
                // lookup the module index and add this notification
                PipelineModuleStepContainer container = GetModuleContainer(CurrentModuleCollectionKey);
                //WOS 1985878: HttpModule unsubscribing an event handler causes AV in Integrated Mode
                if (container != null) {
#if DBG
                    container.DebugModuleName = CurrentModuleCollectionKey;
#endif
                    SyncEventExecutionStep step = new SyncEventExecutionStep(this, (EventHandler)handler);
                    container.AddEvent(notification, isPostNotification, step);
                }
            }
        }

        internal void RemoveSyncEventHookup(object key, Delegate handler, RequestNotification notification) {
            RemoveSyncEventHookup(key, handler, notification, false);
        }

        internal void RemoveSyncEventHookup(object key, Delegate handler, RequestNotification notification, bool isPostNotification) {
            ThrowIfEventBindingDisallowed();

            Events.RemoveHandler(key, handler);

            if (IsContainerInitalizationAllowed) {
                PipelineModuleStepContainer container = GetModuleContainer(CurrentModuleCollectionKey);
                //WOS 1985878: HttpModule unsubscribing an event handler causes AV in Integrated Mode
                if (container != null) {
                    container.RemoveEvent(notification, isPostNotification, handler);
                }
            }
        }

        private void AddSendResponseEventHookup(object key, Delegate handler) {
            ThrowIfEventBindingDisallowed();

            // add the event to the delegate invocation list
            // this keeps non-pipeline ASP.NET hosts working
            Events.AddHandler(key, handler);

            // For integrated pipeline mode, add events to the IExecutionStep containers only if
            // InitSpecial has completed and InitInternal has not completed.
            if (IsContainerInitalizationAllowed) {
                // lookup the module index and add this notification
                PipelineModuleStepContainer container = GetModuleContainer(CurrentModuleCollectionKey);
                //WOS 1985878: HttpModule unsubscribing an event handler causes AV in Integrated Mode
                if (container != null) {
#if DBG
                    container.DebugModuleName = CurrentModuleCollectionKey;
#endif
                    bool isHeaders = (key == EventPreSendRequestHeaders);
                    SendResponseExecutionStep step = new SendResponseExecutionStep(this, (EventHandler)handler, isHeaders);
                    container.AddEvent(RequestNotification.SendResponse, false /*isPostNotification*/, step);
                }
            }
        }

        private void RemoveSendResponseEventHookup(object key, Delegate handler) {
            ThrowIfEventBindingDisallowed();

            Events.RemoveHandler(key, handler);

            if (IsContainerInitalizationAllowed) {
                PipelineModuleStepContainer container = GetModuleContainer(CurrentModuleCollectionKey);
                //WOS 1985878: HttpModule unsubscribing an event handler causes AV in Integrated Mode
                if (container != null) {
                    container.RemoveEvent(RequestNotification.SendResponse, false /*isPostNotification*/, handler);
                }
            }
        }

        //
        // [....] event hookup
        //


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler BeginRequest {
            add { AddSyncEventHookup(EventBeginRequest, value, RequestNotification.BeginRequest); }
            remove { RemoveSyncEventHookup(EventBeginRequest, value, RequestNotification.BeginRequest); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler AuthenticateRequest {
            add { AddSyncEventHookup(EventAuthenticateRequest, value, RequestNotification.AuthenticateRequest); }
            remove { RemoveSyncEventHookup(EventAuthenticateRequest, value, RequestNotification.AuthenticateRequest); }
        }

        // internal - for back-stop module only
        internal event EventHandler DefaultAuthentication {
            add { AddSyncEventHookup(EventDefaultAuthentication, value, RequestNotification.AuthenticateRequest); }
            remove { RemoveSyncEventHookup(EventDefaultAuthentication, value, RequestNotification.AuthenticateRequest); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostAuthenticateRequest {
            add { AddSyncEventHookup(EventPostAuthenticateRequest, value, RequestNotification.AuthenticateRequest, true); }
            remove { RemoveSyncEventHookup(EventPostAuthenticateRequest, value, RequestNotification.AuthenticateRequest, true); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler AuthorizeRequest {
            add { AddSyncEventHookup(EventAuthorizeRequest, value, RequestNotification.AuthorizeRequest); }
            remove { RemoveSyncEventHookup(EventAuthorizeRequest, value, RequestNotification.AuthorizeRequest); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostAuthorizeRequest {
            add { AddSyncEventHookup(EventPostAuthorizeRequest, value, RequestNotification.AuthorizeRequest, true); }
            remove { RemoveSyncEventHookup(EventPostAuthorizeRequest, value, RequestNotification.AuthorizeRequest, true); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler ResolveRequestCache {
            add { AddSyncEventHookup(EventResolveRequestCache, value, RequestNotification.ResolveRequestCache); }
            remove { RemoveSyncEventHookup(EventResolveRequestCache, value, RequestNotification.ResolveRequestCache); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostResolveRequestCache {
            add { AddSyncEventHookup(EventPostResolveRequestCache, value, RequestNotification.ResolveRequestCache, true); }
            remove { RemoveSyncEventHookup(EventPostResolveRequestCache, value, RequestNotification.ResolveRequestCache, true); }
        }

        public event EventHandler MapRequestHandler {
            add {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                AddSyncEventHookup(EventMapRequestHandler, value, RequestNotification.MapRequestHandler);
            }
            remove {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                RemoveSyncEventHookup(EventMapRequestHandler, value, RequestNotification.MapRequestHandler);
            }
        }

        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostMapRequestHandler {
            add { AddSyncEventHookup(EventPostMapRequestHandler, value, RequestNotification.MapRequestHandler, true); }
            remove { RemoveSyncEventHookup(EventPostMapRequestHandler, value, RequestNotification.MapRequestHandler); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler AcquireRequestState {
            add { AddSyncEventHookup(EventAcquireRequestState, value, RequestNotification.AcquireRequestState); }
            remove { RemoveSyncEventHookup(EventAcquireRequestState, value, RequestNotification.AcquireRequestState); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostAcquireRequestState {
            add { AddSyncEventHookup(EventPostAcquireRequestState, value, RequestNotification.AcquireRequestState, true); }
            remove { RemoveSyncEventHookup(EventPostAcquireRequestState, value, RequestNotification.AcquireRequestState, true); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PreRequestHandlerExecute {
            add { AddSyncEventHookup(EventPreRequestHandlerExecute, value, RequestNotification.PreExecuteRequestHandler); }
            remove { RemoveSyncEventHookup(EventPreRequestHandlerExecute, value, RequestNotification.PreExecuteRequestHandler); }
        }

        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostRequestHandlerExecute {
            add { AddSyncEventHookup(EventPostRequestHandlerExecute, value, RequestNotification.ExecuteRequestHandler, true); }
            remove { RemoveSyncEventHookup(EventPostRequestHandlerExecute, value, RequestNotification.ExecuteRequestHandler, true); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler ReleaseRequestState {
            add { AddSyncEventHookup(EventReleaseRequestState, value, RequestNotification.ReleaseRequestState ); }
            remove { RemoveSyncEventHookup(EventReleaseRequestState, value, RequestNotification.ReleaseRequestState); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostReleaseRequestState {
            add { AddSyncEventHookup(EventPostReleaseRequestState, value, RequestNotification.ReleaseRequestState, true); }
            remove { RemoveSyncEventHookup(EventPostReleaseRequestState, value, RequestNotification.ReleaseRequestState, true); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler UpdateRequestCache {
            add { AddSyncEventHookup(EventUpdateRequestCache, value, RequestNotification.UpdateRequestCache); }
            remove { RemoveSyncEventHookup(EventUpdateRequestCache, value, RequestNotification.UpdateRequestCache); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PostUpdateRequestCache {
            add { AddSyncEventHookup(EventPostUpdateRequestCache, value, RequestNotification.UpdateRequestCache, true); }
            remove { RemoveSyncEventHookup(EventPostUpdateRequestCache, value, RequestNotification.UpdateRequestCache, true); }
        }

        public event EventHandler LogRequest {
            add {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                AddSyncEventHookup(EventLogRequest, value, RequestNotification.LogRequest);
            }
            remove {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                RemoveSyncEventHookup(EventLogRequest, value, RequestNotification.LogRequest);
            }
        }

        public event EventHandler PostLogRequest {
            add {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                AddSyncEventHookup(EventPostLogRequest, value, RequestNotification.LogRequest, true);
            }
            remove {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                RemoveSyncEventHookup(EventPostLogRequest, value, RequestNotification.LogRequest, true);
            }
        }

        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler EndRequest {
            add { AddSyncEventHookup(EventEndRequest, value, RequestNotification.EndRequest); }
            remove { RemoveSyncEventHookup(EventEndRequest, value, RequestNotification.EndRequest); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler Error {
            add { Events.AddHandler(EventErrorRecorded, value); }
            remove { Events.RemoveHandler(EventErrorRecorded, value); }
        }


        // Dev10 902404: a new HttpApplication.RequestCompleted event raised when the managed objects associated with 
        // the request are being released.  It allows modules to cleanup resources after all managed modules and handlers
        // have executed.  This may occur before the native processing of the request has completed; for example, before 
        // the final response bytes have been sent to the client.  The HttpContext is not available during this event 
        // because it has already been released.
        public event EventHandler RequestCompleted {
            add { Events.AddHandler(EventRequestCompleted, value); }
            remove { Events.RemoveHandler(EventRequestCompleted, value); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PreSendRequestHeaders {
            add { AddSendResponseEventHookup(EventPreSendRequestHeaders, value); }
            remove { RemoveSendResponseEventHookup(EventPreSendRequestHeaders, value); }
        }


        /// <devdoc><para>[To be supplied.]</para></devdoc>
        public event EventHandler PreSendRequestContent {
            add { AddSendResponseEventHookup(EventPreSendRequestContent, value); }
            remove { RemoveSendResponseEventHookup(EventPreSendRequestContent, value); }
        }

        //
        // Async event hookup
        //

        public void AddOnBeginRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnBeginRequestAsync(bh, eh, null);
        }

        public void AddOnBeginRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventBeginRequest, beginHandler, endHandler, state, RequestNotification.BeginRequest, false, this);
        }

        public void AddOnAuthenticateRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnAuthenticateRequestAsync(bh, eh, null);
        }

        public void AddOnAuthenticateRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventAuthenticateRequest, beginHandler, endHandler, state,
                                   RequestNotification.AuthenticateRequest, false, this);
        }

        public void AddOnPostAuthenticateRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostAuthenticateRequestAsync(bh, eh, null);
        }

        public void AddOnPostAuthenticateRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostAuthenticateRequest, beginHandler, endHandler, state,
                                   RequestNotification.AuthenticateRequest, true, this);
        }

        public void AddOnAuthorizeRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnAuthorizeRequestAsync(bh, eh, null);
        }

        public void AddOnAuthorizeRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventAuthorizeRequest, beginHandler, endHandler, state,
                                   RequestNotification.AuthorizeRequest, false, this);
        }

        public void AddOnPostAuthorizeRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostAuthorizeRequestAsync(bh, eh, null);
        }

        public void AddOnPostAuthorizeRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostAuthorizeRequest, beginHandler, endHandler, state,
                                   RequestNotification.AuthorizeRequest, true, this);
        }

        public void AddOnResolveRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnResolveRequestCacheAsync(bh, eh, null);
        }

        public void AddOnResolveRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventResolveRequestCache, beginHandler, endHandler, state,
                                   RequestNotification.ResolveRequestCache, false, this);
        }

        public void AddOnPostResolveRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostResolveRequestCacheAsync(bh, eh, null);
        }

        public void AddOnPostResolveRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostResolveRequestCache, beginHandler, endHandler, state,
                                   RequestNotification.ResolveRequestCache, true, this);
        }

        public void AddOnMapRequestHandlerAsync(BeginEventHandler bh, EndEventHandler eh) {
            if (!HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
            AddOnMapRequestHandlerAsync(bh, eh, null);
        }

        public void AddOnMapRequestHandlerAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            if (!HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
            AsyncEvents.AddHandler(EventMapRequestHandler, beginHandler, endHandler, state,
                                   RequestNotification.MapRequestHandler, false, this);
        }

        public void AddOnPostMapRequestHandlerAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostMapRequestHandlerAsync(bh, eh, null);
        }

        public void AddOnPostMapRequestHandlerAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostMapRequestHandler, beginHandler, endHandler, state,
                                   RequestNotification.MapRequestHandler, true, this);
        }

        public void AddOnAcquireRequestStateAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnAcquireRequestStateAsync(bh, eh, null);
        }

        public void AddOnAcquireRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventAcquireRequestState, beginHandler, endHandler, state,
                                   RequestNotification.AcquireRequestState, false, this);
        }

        public void AddOnPostAcquireRequestStateAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostAcquireRequestStateAsync(bh, eh, null);
        }

        public void AddOnPostAcquireRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostAcquireRequestState, beginHandler, endHandler, state,
                                   RequestNotification.AcquireRequestState, true, this);
        }

        public void AddOnPreRequestHandlerExecuteAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPreRequestHandlerExecuteAsync(bh, eh, null);
        }

        public void AddOnPreRequestHandlerExecuteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPreRequestHandlerExecute, beginHandler, endHandler, state,
                                   RequestNotification.PreExecuteRequestHandler, false, this);
        }

        public void AddOnPostRequestHandlerExecuteAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostRequestHandlerExecuteAsync(bh, eh, null);
        }

        public void AddOnPostRequestHandlerExecuteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostRequestHandlerExecute, beginHandler, endHandler, state,
                                   RequestNotification.ExecuteRequestHandler, true, this);
        }

        public void AddOnReleaseRequestStateAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnReleaseRequestStateAsync(bh, eh, null);
        }

        public void AddOnReleaseRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventReleaseRequestState, beginHandler, endHandler, state,
                                   RequestNotification.ReleaseRequestState, false, this);
        }

        public void AddOnPostReleaseRequestStateAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostReleaseRequestStateAsync(bh, eh, null);
        }

        public void AddOnPostReleaseRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostReleaseRequestState, beginHandler, endHandler, state,
                                   RequestNotification.ReleaseRequestState, true, this);
        }

        public void AddOnUpdateRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnUpdateRequestCacheAsync(bh, eh, null);
        }

        public void AddOnUpdateRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventUpdateRequestCache, beginHandler, endHandler, state,
                                   RequestNotification.UpdateRequestCache , false, this);
        }

        public void AddOnPostUpdateRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnPostUpdateRequestCacheAsync(bh, eh, null);
        }

        public void AddOnPostUpdateRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventPostUpdateRequestCache, beginHandler, endHandler, state,
                                   RequestNotification.UpdateRequestCache , true, this);
        }

        public void AddOnLogRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            if (!HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
            AddOnLogRequestAsync(bh, eh, null);
        }

        public void AddOnLogRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            if (!HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
            AsyncEvents.AddHandler(EventLogRequest, beginHandler, endHandler, state,
                                   RequestNotification.LogRequest, false, this);
        }

        public void AddOnPostLogRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            if (!HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
            AddOnPostLogRequestAsync(bh, eh, null);
        }

        public void AddOnPostLogRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            if (!HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
            AsyncEvents.AddHandler(EventPostLogRequest, beginHandler, endHandler, state,
                                   RequestNotification.LogRequest, true, this);
        }

        public void AddOnEndRequestAsync(BeginEventHandler bh, EndEventHandler eh) {
            AddOnEndRequestAsync(bh, eh, null);
        }

        public void AddOnEndRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            AsyncEvents.AddHandler(EventEndRequest, beginHandler, endHandler, state,
                                   RequestNotification.EndRequest, false, this);
        }

        //
        // Public Application virtual methods
        //


        /// <devdoc>
        ///    <para>
        ///       Used
        ///          to initialize a HttpModule?s instance variables, and to wireup event handlers to
        ///          the hosting HttpApplication.
        ///       </para>
        ///    </devdoc>
        public virtual void Init() {
            // derived class implements this
        }


        /// <devdoc>
        ///    <para>
        ///       Used
        ///          to clean up an HttpModule?s instance variables
        ///       </para>
        ///    </devdoc>
        public virtual void Dispose() {
            // also part of IComponent
            // derived class implements this
            _site = null;
            if (_events != null) {
                try {
                    EventHandler handler = (EventHandler)_events[EventDisposed];
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
                finally {
                    _events.Dispose();
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
        internal static void SetCurrentPrincipalWithAssert(IPrincipal user) {
            Thread.CurrentPrincipal = user;
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
        internal static WindowsIdentity GetCurrentWindowsIdentityWithAssert() {
            return WindowsIdentity.GetCurrent();
        }

        private HttpHandlerAction GetHandlerMapping(HttpContext context, String requestType, VirtualPath path, bool useAppConfig) {
            CachedPathData pathData = null;
            HandlerMappingMemo memo = null;
            HttpHandlerAction mapping = null;

            // Check if cached handler could be used
            if (!useAppConfig) {
                // Grab mapping from cache - verify that the verb matches exactly
                pathData = context.GetPathData(path);
                memo = pathData.CachedHandler;

                // Invalidate cache on missmatch
                if (memo != null && !memo.IsMatch(requestType, path)) {
                    memo = null;
                }
            }

            // Get new mapping
            if (memo == null) {
                // Load from config
                HttpHandlersSection map = useAppConfig ? RuntimeConfig.GetAppConfig().HttpHandlers
                                                       : RuntimeConfig.GetConfig(context).HttpHandlers;
                mapping = map.FindMapping(requestType, path);

                // Add cache entry
                if (!useAppConfig) {
                    memo = new HandlerMappingMemo(mapping, requestType, path);
                    pathData.CachedHandler = memo;
                }
            }
            else {
                // Get mapping from the cache
                mapping = memo.Mapping;
            }

            return mapping;
        }

        internal IHttpHandler MapIntegratedHttpHandler(HttpContext context, String requestType, VirtualPath path, String pathTranslated, bool useAppConfig, bool convertNativeStaticFileModule) {
            IHttpHandler handler = null;

            using (new ApplicationImpersonationContext()) {
                string type;

                // vpath is a non-relative virtual path
                string vpath = path.VirtualPathString;

                // If we're using app config, modify vpath by appending the path after the last slash
                // to the app's virtual path.  This will force IIS IHttpContext::MapHandler to use app configuration.
                if (useAppConfig) {
                    int index = vpath.LastIndexOf('/');
                    index++;
                    if (index != 0 && index < vpath.Length) {
                        vpath = UrlPath.SimpleCombine(HttpRuntime.AppDomainAppVirtualPathString, vpath.Substring(index));
                    }
                    else {
                        vpath = HttpRuntime.AppDomainAppVirtualPathString;
                    }
                }


                IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;
                type = wr.MapHandlerAndGetHandlerTypeString(method: requestType, path: vpath, convertNativeStaticFileModule: convertNativeStaticFileModule, ignoreWildcardMappings: false);

                // If a page developer has removed the default mappings with <handlers><clear>
                // without replacing them then we need to give a more descriptive error than
                // a null parameter exception.
                if (type == null) {
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_FAILED);
                    throw new HttpException(SR.GetString(SR.Http_handler_not_found_for_request_type, requestType));
                }

                // if it's a native type, don't go any further
                if(String.IsNullOrEmpty(type)) {
                    return handler;
                }

                // Get factory from the mapping
                IHttpHandlerFactory factory = GetFactory(type);

                try {
                    handler = factory.GetHandler(context, requestType, path.VirtualPathString, pathTranslated);
                }
                catch (FileNotFoundException e) {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                        throw new HttpException(404, null, e);
                    else
                        throw new HttpException(404, null);
                }
                catch (DirectoryNotFoundException e) {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                        throw new HttpException(404, null, e);
                    else
                        throw new HttpException(404, null);
                }
                catch (PathTooLongException e) {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                        throw new HttpException(414, null, e);
                    else
                        throw new HttpException(414, null);
                }

                // Remember for recycling
                if (_handlerRecycleList == null)
                    _handlerRecycleList = new ArrayList();
                _handlerRecycleList.Add(new HandlerWithFactory(handler, factory));
            }

            return handler;
        }

        internal IHttpHandler MapHttpHandler(HttpContext context, String requestType, VirtualPath path, String pathTranslated, bool useAppConfig) {
            // Don't use remap handler when HttpServerUtility.Execute called
            IHttpHandler handler = (context.ServerExecuteDepth == 0) ? context.RemapHandlerInstance : null;

            using (new ApplicationImpersonationContext()) {
                // Use remap handler if possible
                if (handler != null){
                    return handler;
                }

                // Map new handler
                HttpHandlerAction mapping = GetHandlerMapping(context, requestType, path, useAppConfig);

                // If a page developer has removed the default mappings with <httpHandlers><clear>
                // without replacing them then we need to give a more descriptive error than
                // a null parameter exception.
                if (mapping == null) {
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_FAILED);
                    throw new HttpException(SR.GetString(SR.Http_handler_not_found_for_request_type, requestType));
                }

                // Get factory from the mapping
                IHttpHandlerFactory factory = GetFactory(mapping);


                // Get factory from the mapping
                try {
                    // Check if it supports the more efficient GetHandler call that can avoid
                    // a VirtualPath object creation.
                    IHttpHandlerFactory2 factory2 = factory as IHttpHandlerFactory2;

                    if (factory2 != null) {
                        handler = factory2.GetHandler(context, requestType, path, pathTranslated);
                    }
                    else {
                        handler = factory.GetHandler(context, requestType, path.VirtualPathString, pathTranslated);
                    }
                }
                catch (FileNotFoundException e) {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                        throw new HttpException(404, null, e);
                    else
                        throw new HttpException(404, null);
                }
                catch (DirectoryNotFoundException e) {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                        throw new HttpException(404, null, e);
                    else
                        throw new HttpException(404, null);
                }
                catch (PathTooLongException e) {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                        throw new HttpException(414, null, e);
                    else
                        throw new HttpException(414, null);
                }

                // Remember for recycling
                if (_handlerRecycleList == null)
                    _handlerRecycleList = new ArrayList();
                _handlerRecycleList.Add(new HandlerWithFactory(handler, factory));
            }

            return handler;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual string GetVaryByCustomString(HttpContext context, string custom) {

            if (StringUtil.EqualsIgnoreCase(custom, "browser")) {
                return context.Request.Browser.Type;
            }

            return null;
        }

        public virtual string GetOutputCacheProviderName(HttpContext context) {
            // default implementation
            return System.Web.Caching.OutputCache.DefaultProviderName;
        }

        //
        // IComponent implementation
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public ISite Site {
            get { return _site;}
            set { _site = value;}
        }

        //
        // IHttpAsyncHandler implementation
        //


        /// <internalonly/>
        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData) {
            HttpAsyncResult result;

            // Setup the asynchronous stuff and application variables
            _context = context;
            _context.ApplicationInstance = this;

            _stepManager.InitRequest();

            // Make sure the context stays rooted (including all async operations)
            _context.Root();

            // Create the async result
            result = new HttpAsyncResult(cb, extraData);

            // Remember the async result for use in async completions
            AsyncResult = result;

            if (_context.TraceIsEnabled)
                HttpRuntime.Profile.StartRequest(_context);

            // Start the application
            ResumeSteps(null);

            // Return the async result
            return result;
        }


        /// <internalonly/>
        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result) {
            // throw error caught during execution
            HttpAsyncResult ar = (HttpAsyncResult)result;
            if (ar.Error != null)
                throw ar.Error;
        }

        //
        // IHttpHandler implementation
        //


        /// <internalonly/>
        void IHttpHandler.ProcessRequest(HttpContext context) {
            throw new HttpException(SR.GetString(SR.Sync_not_supported));
        }


        /// <internalonly/>
        bool IHttpHandler.IsReusable {
            get { return true; }
        }

        //
        // Support for external calls into the application like app_onStart
        //

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.RestrictedMemberAccess)]
        private void InvokeMethodWithAssert(MethodInfo method, int paramCount, object eventSource, EventArgs eventArgs) {
            if (paramCount == 0) {
                method.Invoke(this, new Object[0]);
            }
            else {
                Debug.Assert(paramCount == 2);

                method.Invoke(this, new Object[2] { eventSource, eventArgs });
            }
        }

        internal void ProcessSpecialRequest(HttpContext context,
                                            MethodInfo method,
                                            int paramCount,
                                            Object eventSource,
                                            EventArgs eventArgs,
                                            HttpSessionState session) {
            _context = context;
            if (HttpRuntime.UseIntegratedPipeline && _context != null) {
                _context.HideRequestResponse = true;
            }
            _hideRequestResponse = true;
            _session = session;
            _lastError = null;

            using (new DisposableHttpContextWrapper(context)) {
                using (new ApplicationImpersonationContext()) {
                    try {
                        // set culture on the current thread
                        SetAppLevelCulture();
                        InvokeMethodWithAssert(method, paramCount, eventSource, eventArgs);
                    }
                    catch (Exception e) {
                        // dereference reflection invocation exceptions
                        Exception eActual;
                        if (e is TargetInvocationException)
                            eActual = e.InnerException;
                        else
                            eActual = e;

                        RecordError(eActual);

                        if (context == null) {
                            try {
                                WebBaseEvent.RaiseRuntimeError(eActual, this);
                            }
                            catch {
                            }
                        }

                    }
                    finally {

                        // this thread should not be locking app state
                        if (_state != null)
                            _state.EnsureUnLock();

                        // restore culture
                        RestoreAppLevelCulture();

                        if (HttpRuntime.UseIntegratedPipeline && _context != null) {
                            _context.HideRequestResponse = false;
                        }
                        _hideRequestResponse = false;
                        _context = null;
                        _session = null;
                        _lastError = null;
                        _appEvent = null;
                    }
                }
            }
        }

        //
        // Report context-less error
        //

        internal void RaiseErrorWithoutContext(Exception error) {
            try {
                try {
                    SetAppLevelCulture();
                    _lastError = error;

                    RaiseOnError();
                }
                finally {
                    // this thread should not be locking app state
                    if (_state != null)
                        _state.EnsureUnLock();

                    RestoreAppLevelCulture();
                    _lastError = null;
                    _appEvent = null;
                }
            }
            catch { // Protect against exception filters
                throw;
            }
        }

        //
        //
        //

        internal void InitInternal(HttpContext context, HttpApplicationState state, MethodInfo[] handlers) {
            Debug.Assert(context != null, "context != null");

            // Remember state
            _state = state;

            PerfCounters.IncrementCounter(AppPerfCounter.PIPELINES);

            try {
                try {
                    // Remember context for config lookups
                    _initContext = context;
                    _initContext.ApplicationInstance = this;

                    // Set config path to be application path for the application initialization
                    context.ConfigurationPath = context.Request.ApplicationPathObject;

                    // keep HttpContext.Current working while running user code
                    using (new DisposableHttpContextWrapper(context)) {

                        // Build module list from config
                        if (HttpRuntime.UseIntegratedPipeline) {

                            Debug.Assert(_moduleConfigInfo != null, "_moduleConfigInfo != null");
                            Debug.Assert(_moduleConfigInfo.Count >= 0, "_moduleConfigInfo.Count >= 0");

                            try {
                                context.HideRequestResponse = true;
                                _hideRequestResponse = true;
                                InitIntegratedModules();
                            }
                            finally {
                                context.HideRequestResponse = false;
                                _hideRequestResponse = false;
                            }
                        }
                        else {
                            InitModules();

                            // this is used exclusively for integrated mode
                            Debug.Assert(null == _moduleContainers, "null == _moduleContainers");
                        }

                        // Hookup event handlers via reflection
                        if (handlers != null)
                            HookupEventHandlersForApplicationAndModules(handlers);

                        // Initialization of the derived class
                        _context = context;
                        if (HttpRuntime.UseIntegratedPipeline && _context != null) {
                            _context.HideRequestResponse = true;
                        }
                        _hideRequestResponse = true;

                        try {
                            Init();
                        }
                        catch (Exception e) {
                            RecordError(e);
                        }
                    }

                    if (HttpRuntime.UseIntegratedPipeline && _context != null) {
                        _context.HideRequestResponse = false;
                    }
                    _hideRequestResponse = false;
                    _context = null;
                    _resumeStepsWaitCallback= new WaitCallback(this.ResumeStepsWaitCallback);

                    // Construct the execution steps array
                    if (HttpRuntime.UseIntegratedPipeline) {
                        _stepManager = new PipelineStepManager(this);
                    }
                    else {
                        _stepManager = new ApplicationStepManager(this);
                    }

                    _stepManager.BuildSteps(_resumeStepsWaitCallback);
                }
                finally {
                    _initInternalCompleted = true;

                    // Reset config path
                    context.ConfigurationPath = null;

                    // don't hold on to the context
                    _initContext.ApplicationInstance = null;
                    _initContext = null;
                }
            }
            catch { // Protect against exception filters
                throw;
            }
        }

        // helper to expand an event handler into application steps
        private void CreateEventExecutionSteps(Object eventIndex, ArrayList steps) {
            // async
            AsyncAppEventHandler asyncHandler = AsyncEvents[eventIndex];

            if (asyncHandler != null) {
                asyncHandler.CreateExecutionSteps(this, steps);
            }

            // [....]
            EventHandler handler = (EventHandler)Events[eventIndex];

            if (handler != null) {
                Delegate[] handlers = handler.GetInvocationList();

                for (int i = 0; i < handlers.Length; i++)  {
                    steps.Add(new SyncEventExecutionStep(this, (EventHandler)handlers[i]));
                }
            }
        }

        internal void InitSpecial(HttpApplicationState state, MethodInfo[] handlers, IntPtr appContext, HttpContext context) {
            // Remember state
            _state = state;

            try {
                //  Remember the context for the initialization
                if (context != null) {
                    _initContext = context;
                    _initContext.ApplicationInstance = this;
                }

                // if we're doing integrated pipeline wireup, then appContext is non-null and we need to init modules and register event subscriptions with IIS
                if (appContext != IntPtr.Zero) {
                    // 1694356: app_offline.htm and <httpRuntime enabled=/> require that we make this check here for integrated mode
                    using (new ApplicationImpersonationContext()) {
                        HttpRuntime.CheckApplicationEnabled();
                    }

                    // retrieve app level culture
                    InitAppLevelCulture();

                    Debug.Trace("PipelineRuntime", "InitSpecial for " + appContext.ToString() + "\n");
                    RegisterEventSubscriptionsWithIIS(appContext, context, handlers);
                }
                else {
                    // retrieve app level culture
                    InitAppLevelCulture();

                    // Hookup event handlers via reflection
                    if (handlers != null) {
                        HookupEventHandlersForApplicationAndModules(handlers);
                    }
                }

                // if we're doing integrated pipeline wireup, then appContext is non-null and we need to register the application (global.asax) event handlers
                if (appContext != IntPtr.Zero) {
                    if (_appPostNotifications != 0 || _appRequestNotifications != 0) {
                        RegisterIntegratedEvent(appContext,
                                                HttpApplicationFactory.applicationFileName,
                                                _appRequestNotifications,
                                                _appPostNotifications,
                                                this.GetType().FullName,
                                                MANAGED_PRECONDITION,
                                                false);
                    }
                }
            }
            finally  {
                _initSpecialCompleted = true;

                //  Do not hold on to the context
                if (_initContext != null) {
                    _initContext.ApplicationInstance = null;
                    _initContext = null;
                }
            }
        }

        internal void DisposeInternal() {
            PerfCounters.DecrementCounter(AppPerfCounter.PIPELINES);

            // call derived class

            try {
                Dispose();
            }
            catch (Exception e) {
                RecordError(e);
            }

            // dispose modules

            if (_moduleCollection != null) {
                int numModules = _moduleCollection.Count;

                for (int i = 0; i < numModules; i++) {
                    try {
                        // set the init key during Dispose for modules
                        // that try to unregister events
                        if (HttpRuntime.UseIntegratedPipeline) {
                            _currentModuleCollectionKey = _moduleCollection.GetKey(i);
                        }
                        _moduleCollection[i].Dispose();
                    }
                    catch {
                    }
                }

                _moduleCollection = null;
            }
        }

        private void BuildEventMaskDictionary(Dictionary<string, RequestNotification> eventMask) {
            eventMask["BeginRequest"]              = RequestNotification.BeginRequest;
            eventMask["AuthenticateRequest"]       = RequestNotification.AuthenticateRequest;
            eventMask["PostAuthenticateRequest"]   = RequestNotification.AuthenticateRequest;
            eventMask["AuthorizeRequest"]          = RequestNotification.AuthorizeRequest;
            eventMask["PostAuthorizeRequest"]      = RequestNotification.AuthorizeRequest;
            eventMask["ResolveRequestCache"]       = RequestNotification.ResolveRequestCache;
            eventMask["PostResolveRequestCache"]   = RequestNotification.ResolveRequestCache;
            eventMask["MapRequestHandler"]         = RequestNotification.MapRequestHandler;
            eventMask["PostMapRequestHandler"]     = RequestNotification.MapRequestHandler;
            eventMask["AcquireRequestState"]       = RequestNotification.AcquireRequestState;
            eventMask["PostAcquireRequestState"]   = RequestNotification.AcquireRequestState;
            eventMask["PreRequestHandlerExecute"]  = RequestNotification.PreExecuteRequestHandler;
            eventMask["PostRequestHandlerExecute"] = RequestNotification.ExecuteRequestHandler;
            eventMask["ReleaseRequestState"]       = RequestNotification.ReleaseRequestState;
            eventMask["PostReleaseRequestState"]   = RequestNotification.ReleaseRequestState;
            eventMask["UpdateRequestCache"]        = RequestNotification.UpdateRequestCache;
            eventMask["PostUpdateRequestCache"]    = RequestNotification.UpdateRequestCache;
            eventMask["LogRequest"]                = RequestNotification.LogRequest;
            eventMask["PostLogRequest"]            = RequestNotification.LogRequest;
            eventMask["EndRequest"]                = RequestNotification.EndRequest;
            eventMask["PreSendRequestHeaders"]     = RequestNotification.SendResponse;
            eventMask["PreSendRequestContent"]     = RequestNotification.SendResponse;
        }

        private void HookupEventHandlersForApplicationAndModules(MethodInfo[] handlers) {
            _currentModuleCollectionKey = HttpApplicationFactory.applicationFileName;

            if(null == _pipelineEventMasks) {
                Dictionary<string, RequestNotification> dict = new Dictionary<string, RequestNotification>();
                BuildEventMaskDictionary(dict);
                if(null == _pipelineEventMasks) {
                    _pipelineEventMasks = dict;
                }
            }


            for (int i = 0; i < handlers.Length; i++) {
                MethodInfo appMethod = handlers[i];
                String appMethodName = appMethod.Name;
                int namePosIndex = appMethodName.IndexOf('_');
                String targetName = appMethodName.Substring(0, namePosIndex);

                // Find target for method
                Object target = null;

                if (StringUtil.EqualsIgnoreCase(targetName, "Application"))
                    target = this;
                else if (_moduleCollection != null)
                    target = _moduleCollection[targetName];

                if (target == null)
                    continue;

                // Find event on the module type
                Type targetType = target.GetType();
                EventDescriptorCollection events = TypeDescriptor.GetEvents(targetType);
                string eventName = appMethodName.Substring(namePosIndex+1);

                EventDescriptor foundEvent = events.Find(eventName, true);
                if (foundEvent == null
                    && StringUtil.EqualsIgnoreCase(eventName.Substring(0, 2), "on")) {

                    eventName = eventName.Substring(2);
                    foundEvent = events.Find(eventName, true);
                }

                MethodInfo addMethod = null;
                if (foundEvent != null) {
                    EventInfo reflectionEvent = targetType.GetEvent(foundEvent.Name);
                    Debug.Assert(reflectionEvent != null);
                    if (reflectionEvent != null) {
                        addMethod = reflectionEvent.GetAddMethod();
                    }
                }

                if (addMethod == null)
                    continue;

                ParameterInfo[] addMethodParams = addMethod.GetParameters();

                if (addMethodParams.Length != 1)
                    continue;

                // Create the delegate from app method to pass to AddXXX(handler) method

                Delegate handlerDelegate = null;

                ParameterInfo[] appMethodParams = appMethod.GetParameters();

                if (appMethodParams.Length == 0) {
                    // If the app method doesn't have arguments --
                    // -- hookup via intermidiate handler

                    // only can do it for EventHandler, not strongly typed
                    if (addMethodParams[0].ParameterType != typeof(System.EventHandler))
                        continue;

                    ArglessEventHandlerProxy proxy = new ArglessEventHandlerProxy(this, appMethod);
                    handlerDelegate = proxy.Handler;
                }
                else {
                    // Hookup directly to the app methods hoping all types match

                    try {
                        handlerDelegate = Delegate.CreateDelegate(addMethodParams[0].ParameterType, this, appMethodName);
                    }
                    catch {
                        // some type mismatch
                        continue;
                    }
                }

                // Call the AddXXX() to hook up the delegate

                try {
                    addMethod.Invoke(target, new Object[1]{handlerDelegate});
                }
                catch {
                    if (HttpRuntime.UseIntegratedPipeline) {
                        throw;
                    }
                }

                if (eventName != null) {
                    if (_pipelineEventMasks.ContainsKey(eventName)) {
                        if (!StringUtil.StringStartsWith(eventName, "Post")) {
                            _appRequestNotifications |= _pipelineEventMasks[eventName];
                        }
                        else {
                            _appPostNotifications |= _pipelineEventMasks[eventName];
                        }
                    }
                }
            }
        }

        private void RegisterIntegratedEvent(IntPtr appContext,
                                             string moduleName,
                                             RequestNotification requestNotifications,
                                             RequestNotification postRequestNotifications,
                                             string moduleType,
                                             string modulePrecondition,
                                             bool useHighPriority) {

            // lookup the modules event index, if it already exists
            // use it, otherwise, bump the global count
            // the module is used for event dispatch

            int moduleIndex;
            if (_moduleIndexMap.ContainsKey(moduleName)) {
                moduleIndex = (int) _moduleIndexMap[moduleName];
            }
            else {
                moduleIndex = _moduleIndexMap.Count;
                _moduleIndexMap[moduleName] = moduleIndex;
            }

#if DBG
            Debug.Assert(moduleIndex >= 0, "moduleIndex >= 0");
            Debug.Trace("PipelineRuntime", "RegisterIntegratedEvent:"
                        + " module=" + moduleName
                        + ", index=" + moduleIndex.ToString(CultureInfo.InvariantCulture)
                        + ", rq_notify=" + requestNotifications
                        + ", post_rq_notify=" + postRequestNotifications
                        + ", preconditon=" + modulePrecondition + "\r\n");
#endif

            int result = UnsafeIISMethods.MgdRegisterEventSubscription(appContext,
                                                                       moduleName,
                                                                       requestNotifications,
                                                                       postRequestNotifications,
                                                                       moduleType,
                                                                       modulePrecondition,
                                                                       new IntPtr(moduleIndex),
                                                                       useHighPriority);

            if(result < 0) {
                throw new HttpException(SR.GetString(SR.Failed_Pipeline_Subscription, moduleName));
            }
        }


        private void SetAppLevelCulture() {
            CultureInfo culture = null;
            CultureInfo uiculture = null;
            CultureInfo browserCulture = null;
            //get the language from the browser
            //DevDivBugs 2001091: Request object is not available in integrated mode during Application_Start,
            //so don't try to access it if it is hidden
            if((_appLevelAutoCulture || _appLevelAutoUICulture) && _context != null && _context.HideRequestResponse == false) {
                string[] userLanguages = _context.UserLanguagesFromContext();
                if (userLanguages != null) {
                    try { browserCulture = CultureUtil.CreateReadOnlyCulture(userLanguages, requireSpecific: true); }
                    catch { }
                }
            }

            culture = _appLevelCulture;
            uiculture = _appLevelUICulture;
            if(browserCulture != null) {
                if(_appLevelAutoCulture) {
                    culture = browserCulture;
                }
                if(_appLevelAutoUICulture) {
                    uiculture = browserCulture;
                }
            }

            _savedAppLevelCulture = Thread.CurrentThread.CurrentCulture;
            _savedAppLevelUICulture = Thread.CurrentThread.CurrentUICulture;

            if (culture != null && culture != Thread.CurrentThread.CurrentCulture) {
                HttpRuntime.SetCurrentThreadCultureWithAssert(culture);
            }

            if (uiculture != null && uiculture != Thread.CurrentThread.CurrentUICulture) {
                Thread.CurrentThread.CurrentUICulture = uiculture;
            }
        }

        private void RestoreAppLevelCulture() {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

            if (_savedAppLevelCulture != null) {
                // Avoid the cost of the Demand when setting the culture by comparing the cultures first
                if (currentCulture != _savedAppLevelCulture) {
                    HttpRuntime.SetCurrentThreadCultureWithAssert(_savedAppLevelCulture);
                }

                _savedAppLevelCulture = null;
            }

            if (_savedAppLevelUICulture != null) {
                // Avoid the cost of the Demand when setting the culture by comparing the cultures first
                if (currentUICulture  != _savedAppLevelUICulture) {
                    Thread.CurrentThread.CurrentUICulture = _savedAppLevelUICulture;
                }

                _savedAppLevelUICulture = null;
            }
        }

        // Initializes the thread on entry to the managed pipeline. A ThreadContext is returned, on
        // which the caller must call Leave.  The IIS7 integrated pipeline uses setImpersonationContext
        // to prevent it from being set until after the authentication notification.

        // OnThreadEnterPrivate returns ThreadContext.
        // ThreadContext.Enter sets variables that are stored on the thread,
        // and saves anything currently on the thread so it can be restored
        // during the call to ThreadContext.Leave.  All variables that are
        // modified on the thread should be stored in ThreadContext so they
        // can be restored later.  ThreadContext.Enter should only be called
        // when holding a lock on the HttpApplication instance.
        // ThreadContext.Leave is also normally called under the lock, but
        // the Integrated Pipeline may delay this call until after the call to
        // IndicateCompletion returns.  When IndicateCompletion is called,
        // IIS7 will execute the remaining notifications for the request on
        // the current thread.  As a performance improvement, we do not call
        // Leave before calling IndicateCompletion, and we do not call Enter/Leave
        // for the notifications executed while we are in the call to
        // IndicateCompletion.  But when IndicateCompletion returns, we do not
        // have a lock on the HttpApplication instance and therefore cannot
        // modify request state, such as the HttpContext or HttpApplication.
        // The only thing we can do is restore the state of the thread.
        // There's one problem, the Culture/UICulture may be changed by
        // user code that directly updates the values on the current thread, so
        // before leaving the pipeline we call ThreadContext.Synchronize to
        // synchronize the values that are stored on the HttpContext with what
        // is on the thread.  Because of this, the next notification will end up using
        // the Culture/UICulture set by user-code, just as it did on IIS6.
        private ThreadContext OnThreadEnterPrivate(bool setImpersonationContext) {
            ThreadContext threadContext = new ThreadContext(_context);
            threadContext.AssociateWithCurrentThread(setImpersonationContext);

            // An entry is added to the request timeout manager once per request
            // and removed in ReleaseAppInstance.
            if (!_timeoutManagerInitialized) {
                // ensure Timeout is set (see ASURT 148698)
                // to avoid ---- getting config later (ASURT 127388)
                _context.EnsureTimeout();

                HttpRuntime.RequestTimeoutManager.Add(_context);
                _timeoutManagerInitialized = true;
            }

            return threadContext;
        }

        // consumed by AppVerifier when it is enabled
        HttpContext ISyncContext.HttpContext {
            get {
                return _context;
            }
        }

        // consumed by AspNetSynchronizationContext
        ISyncContextLock ISyncContext.Enter() {
            return OnThreadEnter();
        }

        internal ThreadContext OnThreadEnter() {
            return OnThreadEnterPrivate(true /* setImpersonationContext */);
        }

        internal ThreadContext OnThreadEnter(bool setImpersonationContext) {
            return OnThreadEnterPrivate(setImpersonationContext);
        }

        /*
         * Execute single step catching exceptions in a fancy way (see below)
         */
        internal Exception ExecuteStep(IExecutionStep step, ref bool completedSynchronously) {
            Exception error = null;

            try {
                try {
                    if (step.IsCancellable) {
                        _context.BeginCancellablePeriod();  // request can be cancelled from this point

                        try {
                            step.Execute();
                        }
                        finally {
                            _context.EndCancellablePeriod();  // request can be cancelled until this point
                        }

                        _context.WaitForExceptionIfCancelled();  // wait outside of finally
                    }
                    else {
                        step.Execute();
                    }

                    if (!step.CompletedSynchronously) {
                        completedSynchronously = false;
                        return null;
                    }
                }
                catch (Exception e) {
                    error = e;

                    // Since we will leave the context later, we need to remember if we are impersonating
                    // before we lose that info - VSWhidbey 494476
                    if (ImpersonationContext.CurrentThreadTokenExists) {
                        e.Data[System.Web.Management.WebThreadInformation.IsImpersonatingKey] = String.Empty;
                    }
                    // This might force ThreadAbortException to be thrown
                    // automatically, because we consumed an exception that was
                    // hiding ThreadAbortException behind it

                    if (e is ThreadAbortException &&
                        ((Thread.CurrentThread.ThreadState & ThreadState.AbortRequested) == 0))  {
                        // Response.End from a COM+ component that re-throws ThreadAbortException
                        // It is not a real ThreadAbort
                        // VSWhidbey 178556
                        error = null;
                        _stepManager.CompleteRequest();
                    }
                }
#pragma warning disable 1058
                catch {
                    // ignore non-Exception objects that could be thrown
                }
#pragma warning restore 1058
            }
            catch (ThreadAbortException e) {
                // ThreadAbortException could be masked as another one
                // the try-catch above consumes all exceptions, only
                // ThreadAbortException can filter up here because it gets
                // auto rethrown if no other exception is thrown on catch

                if (e.ExceptionState != null && e.ExceptionState is CancelModuleException) {
                    // one of ours (Response.End or timeout) -- cancel abort

                    CancelModuleException cancelException = (CancelModuleException)e.ExceptionState;

                    if (cancelException.Timeout) {
                        // Timed out
                        error = new HttpException(SR.GetString(SR.Request_timed_out),
                                            null, WebEventCodes.RuntimeErrorRequestAbort);
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TIMED_OUT);
                    }
                    else {
                        // Response.End
                        error = null;
                        _stepManager.CompleteRequest();
                    }

                    Thread.ResetAbort();
                }
            }

            completedSynchronously = true;
            return error;
        }

        /*
         * Resume execution of the app steps
         */

        private void ResumeStepsFromThreadPoolThread(Exception error) {
            if (Thread.CurrentThread.IsThreadPoolThread) {
                // if on thread pool thread, use the current thread
                ResumeSteps(error);
            }
            else {
                // if on a non-threadpool thread, requeue
                ThreadPool.QueueUserWorkItem(_resumeStepsWaitCallback, error);
            }
        }

        private void ResumeStepsWaitCallback(Object error) {
            ResumeSteps(error as Exception);
        }

        private void ResumeSteps(Exception error) {
            _stepManager.ResumeSteps(error);
        }


        /*
         * Add error to the context fire OnError on first error
         */
        private void RecordError(Exception error) {
            bool firstError = true;

            if (_context != null) {
                if (_context.Error != null)
                    firstError = false;

                _context.AddError(error);
            }
            else {
                if (_lastError != null)
                    firstError = false;

                _lastError = error;
            }

            if (firstError)
                RaiseOnError();
        }


        //
        // Init module list
        //

        private void InitModulesCommon() {
            int n = _moduleCollection.Count;

            for (int i = 0; i < n; i++) {
                // remember the module being inited for event subscriptions
                // we'll later use this for routing
                _currentModuleCollectionKey = _moduleCollection.GetKey(i);
                _moduleCollection[i].Init(this);
            }

            _currentModuleCollectionKey = null;
            InitAppLevelCulture();
        }

        private void InitIntegratedModules() {
            Debug.Assert(null != _moduleConfigInfo, "null != _moduleConfigInfo");
            _moduleCollection = BuildIntegratedModuleCollection(_moduleConfigInfo);
            InitModulesCommon();
        }

        private void InitModules() {
            HttpModulesSection pconfig = RuntimeConfig.GetAppConfig().HttpModules;

            // get the static list, then add the dynamic members
            HttpModuleCollection moduleCollection = pconfig.CreateModules();
            HttpModuleCollection dynamicModules = CreateDynamicModules();

            moduleCollection.AppendCollection(dynamicModules);
            _moduleCollection = moduleCollection; // don't assign until all ops have succeeded

            InitModulesCommon();
        }

        // instantiates modules that have been added to the dynamic registry (classic pipeline)
        private HttpModuleCollection CreateDynamicModules() {
            HttpModuleCollection moduleCollection = new HttpModuleCollection();

            foreach (DynamicModuleRegistryEntry entry in _dynamicModuleRegistry.LockAndFetchList()) {
                HttpModuleAction modAction = new HttpModuleAction(entry.Name, entry.Type);
                moduleCollection.AddModule(modAction.Entry.ModuleName, modAction.Entry.Create());
            }

            return moduleCollection;
        }

        internal string CurrentModuleCollectionKey {
            get {
                return (null == _currentModuleCollectionKey) ? "UnknownModule" : _currentModuleCollectionKey;
            }
        }

        internal static void RegisterModuleInternal(Type moduleType) {
            _dynamicModuleRegistry.Add(moduleType);
        }

        public static void RegisterModule(Type moduleType) {
            RuntimeConfig config = RuntimeConfig.GetAppConfig();
            HttpRuntimeSection runtimeSection = config.HttpRuntime;
            if (runtimeSection.AllowDynamicModuleRegistration) {
                RegisterModuleInternal(moduleType);
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.DynamicModuleRegistrationOff));
            }
        }

        private void RegisterEventSubscriptionsWithIIS(IntPtr appContext, HttpContext context, MethodInfo[] handlers) {
            RequestNotification requestNotifications;
            RequestNotification postRequestNotifications;

            // register an implicit filter module
            RegisterIntegratedEvent(appContext,
                                    IMPLICIT_FILTER_MODULE,
                                    RequestNotification.UpdateRequestCache| RequestNotification.LogRequest  /*requestNotifications*/,
                                    0 /*postRequestNotifications*/,
                                    String.Empty /*type*/,
                                    String.Empty /*precondition*/,
                                    true /*useHighPriority*/);

            // integrated pipeline will always use serverModules instead of <httpModules>
            _moduleCollection = GetModuleCollection(appContext);

            if (handlers != null) {
                HookupEventHandlersForApplicationAndModules(handlers);
            }

            // 1643363: Breaking Change: ASP.Net v2.0: Application_OnStart is called after Module.Init (Integarted mode)
            HttpApplicationFactory.EnsureAppStartCalledForIntegratedMode(context, this);

            // Call Init on HttpApplication derived class ("global.asax")
            // and process event subscriptions before processing other modules.
            // Doing this now prevents clearing any events that may
            // have been added to event handlers during instantiation of this instance.
            // NOTE:  If "global.asax" has a constructor which hooks up event handlers,
            // then they were added to the event handler lists but have not been registered with IIS yet,
            // so we MUST call ProcessEventSubscriptions on it first, before the other modules.
            _currentModuleCollectionKey = HttpApplicationFactory.applicationFileName;

            try {
                _hideRequestResponse = true;
                context.HideRequestResponse = true;
                _context = context;
                Init();
            }
            catch (Exception e) {
                RecordError(e);
                Exception error = context.Error;
                if (error != null) {
                    throw error;
                }
            }
            finally {
                _context = null;
                context.HideRequestResponse = false;
                _hideRequestResponse = false;
            }

            ProcessEventSubscriptions(out requestNotifications, out postRequestNotifications);

            // Save the notification subscriptions so we can register them with IIS later, after
            // we call HookupEventHandlersForApplicationAndModules and process global.asax event handlers.
            _appRequestNotifications |= requestNotifications;
            _appPostNotifications    |= postRequestNotifications;

            for (int i = 0; i < _moduleCollection.Count; i++) {
                _currentModuleCollectionKey = _moduleCollection.GetKey(i);
                IHttpModule httpModule = _moduleCollection.Get(i);
                ModuleConfigurationInfo moduleInfo = _moduleConfigInfo[i];

#if DBG
                Debug.Trace("PipelineRuntime", "RegisterEventSubscriptionsWithIIS: name=" + CurrentModuleCollectionKey
                            + ", type=" + httpModule.GetType().FullName + "\n");

                // make sure collections are in [....]
                Debug.Assert(moduleInfo.Name == _currentModuleCollectionKey, "moduleInfo.Name == _currentModuleCollectionKey");
#endif

                httpModule.Init(this);

                ProcessEventSubscriptions(out requestNotifications, out postRequestNotifications);

                // are any events wired up?
                if (requestNotifications != 0 || postRequestNotifications != 0) {

                    RegisterIntegratedEvent(appContext,
                                            moduleInfo.Name,
                                            requestNotifications,
                                            postRequestNotifications,
                                            moduleInfo.Type,
                                            moduleInfo.Precondition,
                                            false /*useHighPriority*/);
                }
            }

            // WOS 1728067: RewritePath does not remap the handler when rewriting from a non-ASP.NET request
            // register a default implicit handler
            RegisterIntegratedEvent(appContext,
                                    IMPLICIT_HANDLER,
                                    RequestNotification.ExecuteRequestHandler | RequestNotification.MapRequestHandler /*requestNotifications*/,
                                    RequestNotification.EndRequest /*postRequestNotifications*/,
                                    String.Empty /*type*/,
                                    String.Empty /*precondition*/,
                                    false /*useHighPriority*/);
        }

        private void ProcessEventSubscriptions(out RequestNotification requestNotifications,
                                               out RequestNotification postRequestNotifications) {
            requestNotifications = 0;
            postRequestNotifications = 0;

            // Begin
            if(HasEventSubscription(EventBeginRequest)) {
                requestNotifications |= RequestNotification.BeginRequest;
            }

            // Authenticate
            if(HasEventSubscription(EventAuthenticateRequest)) {
                requestNotifications |= RequestNotification.AuthenticateRequest;
            }

            if(HasEventSubscription(EventPostAuthenticateRequest)) {
                postRequestNotifications |= RequestNotification.AuthenticateRequest;
            }

            // Authorize
            if(HasEventSubscription(EventAuthorizeRequest)) {
                requestNotifications |= RequestNotification.AuthorizeRequest;
            }
            if(HasEventSubscription(EventPostAuthorizeRequest)) {
                postRequestNotifications |= RequestNotification.AuthorizeRequest;
            }

            // ResolveRequestCache
            if(HasEventSubscription(EventResolveRequestCache)) {
                requestNotifications |= RequestNotification.ResolveRequestCache;
            }
            if(HasEventSubscription(EventPostResolveRequestCache)) {
                postRequestNotifications |= RequestNotification.ResolveRequestCache;
            }

            // MapRequestHandler
            if(HasEventSubscription(EventMapRequestHandler)) {
                requestNotifications |= RequestNotification.MapRequestHandler;
            }
            if(HasEventSubscription(EventPostMapRequestHandler)) {
                postRequestNotifications |= RequestNotification.MapRequestHandler;
            }

            // AcquireRequestState
            if(HasEventSubscription(EventAcquireRequestState)) {
                requestNotifications |= RequestNotification.AcquireRequestState;
            }
            if(HasEventSubscription(EventPostAcquireRequestState)) {
                postRequestNotifications |= RequestNotification.AcquireRequestState;
            }

            // PreExecuteRequestHandler
            if(HasEventSubscription(EventPreRequestHandlerExecute)) {
                requestNotifications |= RequestNotification.PreExecuteRequestHandler;
            }

            // PostRequestHandlerExecute
            if (HasEventSubscription(EventPostRequestHandlerExecute)) {
                postRequestNotifications |= RequestNotification.ExecuteRequestHandler;
            }

            // ReleaseRequestState
            if(HasEventSubscription(EventReleaseRequestState)) {
                requestNotifications |= RequestNotification.ReleaseRequestState;
            }
            if(HasEventSubscription(EventPostReleaseRequestState)) {
                postRequestNotifications |= RequestNotification.ReleaseRequestState;
            }

            // UpdateRequestCache
            if(HasEventSubscription(EventUpdateRequestCache)) {
                requestNotifications |= RequestNotification.UpdateRequestCache;
            }
            if(HasEventSubscription(EventPostUpdateRequestCache)) {
                postRequestNotifications |= RequestNotification.UpdateRequestCache;
            }

            // LogRequest
            if(HasEventSubscription(EventLogRequest)) {
                requestNotifications |= RequestNotification.LogRequest;
            }
            if(HasEventSubscription(EventPostLogRequest)) {
                postRequestNotifications |= RequestNotification.LogRequest;
            }

            // EndRequest
            if(HasEventSubscription(EventEndRequest)) {
                requestNotifications |= RequestNotification.EndRequest;
            }

            // PreSendRequestHeaders
            if(HasEventSubscription(EventPreSendRequestHeaders)) {
                requestNotifications |= RequestNotification.SendResponse;
            }

            // PreSendRequestContent
            if(HasEventSubscription(EventPreSendRequestContent)) {
                requestNotifications |= RequestNotification.SendResponse;
            }
        }

        // check if an event has subscribers
        // and *reset* them if so
        // this is used only for special app instances
        // and not for processing requests
        private bool HasEventSubscription(Object eventIndex) {
            bool hasEvents = false;

            // async
            AsyncAppEventHandler asyncHandler = AsyncEvents[eventIndex];

            if (asyncHandler != null && asyncHandler.Count > 0) {
                asyncHandler.Reset();
                hasEvents = true;
            }

            // [....]
            EventHandler handler = (EventHandler)Events[eventIndex];

            if (handler != null) {
                Delegate[] handlers = handler.GetInvocationList();
                if( handlers.Length > 0 ) {
                    hasEvents = true;
                }

                foreach(Delegate d in handlers) {
                    Events.RemoveHandler(eventIndex, d);
                }
            }

            return hasEvents;
        }


        //
        // Get app-level culture info (needed to context-less 'global' methods)
        //

        private void InitAppLevelCulture() {
            GlobalizationSection globConfig = RuntimeConfig.GetAppConfig().Globalization;
            string culture = globConfig.Culture;
            string uiCulture = globConfig.UICulture;
            if (!String.IsNullOrEmpty(culture)) {
                if (StringUtil.StringStartsWithIgnoreCase(culture, AutoCulture)) {
                    _appLevelAutoCulture = true;
                    string appLevelCulture = GetFallbackCulture(culture);
                    if(appLevelCulture != null) {
                        _appLevelCulture = HttpServerUtility.CreateReadOnlyCultureInfo(culture.Substring(5));
                    }
                }
                else {
                    _appLevelAutoCulture = false;
                    _appLevelCulture = HttpServerUtility.CreateReadOnlyCultureInfo(globConfig.Culture);
                }
            }
            if (!String.IsNullOrEmpty(uiCulture)) {
                if (StringUtil.StringStartsWithIgnoreCase(uiCulture, AutoCulture))
                {
                    _appLevelAutoUICulture = true;
                    string appLevelUICulture = GetFallbackCulture(uiCulture);
                    if(appLevelUICulture != null) {
                        _appLevelUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(uiCulture.Substring(5));
                    }
                }
                else {
                    _appLevelAutoUICulture = false;
                    _appLevelUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(globConfig.UICulture);
                }
            }
        }

        internal static string GetFallbackCulture(string culture) {
            if((culture.Length > 5) && (culture.IndexOf(':') == 4)) {
                return culture.Substring(5);
            }
            return null;
        }

        //
        // Request mappings management functions
        //

        private IHttpHandlerFactory GetFactory(HttpHandlerAction mapping) {
            HandlerFactoryCache entry = (HandlerFactoryCache)_handlerFactories[mapping.Type];
            if (entry == null) {
                entry = new HandlerFactoryCache(mapping);
                _handlerFactories[mapping.Type] = entry;
            }

            return entry.Factory;
        }

        private IHttpHandlerFactory GetFactory(string type) {
            HandlerFactoryCache entry = (HandlerFactoryCache)_handlerFactories[type];
            if (entry == null) {
                entry = new HandlerFactoryCache(type);
                _handlerFactories[type] = entry;
            }

            return entry.Factory;
        }


        /*
         * Recycle all handlers mapped during the request processing
         */
        private void RecycleHandlers() {
            if (_handlerRecycleList != null) {
                int numHandlers = _handlerRecycleList.Count;

                for (int i = 0; i < numHandlers; i++)
                    ((HandlerWithFactory)_handlerRecycleList[i]).Recycle();

                _handlerRecycleList = null;
            }
        }

        /*
         * Special exception to cancel module execution (not really an exception)
         * used in Response.End and when cancelling requests
         */
        internal class CancelModuleException {
            private bool _timeout;

            internal CancelModuleException(bool timeout) {
                _timeout = timeout;
            }

            internal bool Timeout { get { return _timeout;}}
        }

        // Setup the asynchronous stuff and application variables
        // context for the entire deal is already rooted for native handler
        internal void AssignContext(HttpContext context) {
            Debug.Assert(HttpRuntime.UseIntegratedPipeline, "HttpRuntime.UseIntegratedPipeline");

            if (null == _context) {
                _stepManager.InitRequest();

                _context = context;
                _context.ApplicationInstance = this;

                if (_context.TraceIsEnabled)
                    HttpRuntime.Profile.StartRequest(_context);

                // this will throw if config is invalid, so we do it after HttpContext.ApplicationInstance is set
                _context.SetImpersonationEnabled();
            }
        }

        internal IAsyncResult BeginProcessRequestNotification(HttpContext context, AsyncCallback cb) {
            Debug.Trace("PipelineRuntime", "BeginProcessRequestNotification");

            HttpAsyncResult result;

            if (_context == null) {
                // 
                AssignContext(context);
            }

            //
            // everytime initialization
            //

            context.CurrentModuleEventIndex = -1;

            // Create the async result
            result = new HttpAsyncResult(cb, context);
            context.NotificationContext.AsyncResult = result;

            // enter notification execution loop

            ResumeSteps(null);

            return result;
        }

        internal RequestNotificationStatus EndProcessRequestNotification(IAsyncResult result) {
            HttpAsyncResult ar = (HttpAsyncResult)result;
            if (ar.Error != null)
                throw ar.Error;

            return ar.Status;
        }

        internal void ReleaseAppInstance() {
            if (_context != null)
            {
                if (_context.TraceIsEnabled) {
                    HttpRuntime.Profile.EndRequest(_context);
                }
                _context.ClearReferences();
                if (_timeoutManagerInitialized) {
                    HttpRuntime.RequestTimeoutManager.Remove(_context);
                    _timeoutManagerInitialized = false;
                }

                if(HttpRuntime.EnablePrefetchOptimization && 
                   HttpRuntime.InitializationException == null && 
                   _context.FirstRequest && 
                   _context.Error == null) {
                        UnsafeNativeMethods.EndPrefetchActivity((uint)HttpRuntime.AppDomainAppId.GetHashCode());
                }
            }
            RecycleHandlers();
            if (AsyncResult != null) {
                AsyncResult = null;
            }
            _context = null;
            RaiseOnRequestCompleted();
            AppEvent = null;

            if (ApplicationInstanceConsumersCounter != null) {
                ApplicationInstanceConsumersCounter.MarkOperationCompleted(); // ReleaseAppInstance call complete
            }
            else {
                HttpApplicationFactory.RecycleApplicationInstance(this);
            }
        }

        private void AddEventMapping(string moduleName,
                                      RequestNotification requestNotification,
                                      bool isPostNotification,
                                      IExecutionStep step) {

            ThrowIfEventBindingDisallowed();

            // Add events to the IExecutionStep containers only if
            // InitSpecial has completed and InitInternal has not completed.
            if (!IsContainerInitalizationAllowed) {
                return;
            }

            Debug.Assert(!String.IsNullOrEmpty(moduleName), "!String.IsNullOrEmpty(moduleName)");
            Debug.Trace("PipelineRuntime", "AddEventMapping: for " + moduleName +
                        " for " + requestNotification + "\r\n" );


            PipelineModuleStepContainer container = GetModuleContainer(moduleName);
            //WOS 1985878: HttpModule unsubscribing an event handler causes AV in Integrated Mode
            if (container != null) {
#if DBG
                container.DebugModuleName = moduleName;
#endif
                container.AddEvent(requestNotification, isPostNotification, step);
            }
        }

        static internal List<ModuleConfigurationInfo> IntegratedModuleList {
            get {
                return _moduleConfigInfo;
            }
        }

        private HttpModuleCollection GetModuleCollection(IntPtr appContext) {
            if (_moduleConfigInfo != null) {
                return BuildIntegratedModuleCollection(_moduleConfigInfo);
            }

            List<ModuleConfigurationInfo> moduleList = null;

            IntPtr pModuleCollection = IntPtr.Zero;
            IntPtr pBstrModuleName = IntPtr.Zero;
            int cBstrModuleName = 0;
            IntPtr pBstrModuleType = IntPtr.Zero;
            int cBstrModuleType = 0;
            IntPtr pBstrModulePrecondition = IntPtr.Zero;
            int cBstrModulePrecondition = 0;
            try {
                int count = 0;
                int result = UnsafeIISMethods.MgdGetModuleCollection(IntPtr.Zero, appContext, out pModuleCollection, out count);
                if (result < 0) {
                    throw new HttpException(SR.GetString(SR.Cant_Read_Native_Modules, result.ToString("X8", CultureInfo.InvariantCulture)));
                }
                moduleList = new List<ModuleConfigurationInfo>(count);

                for (uint index = 0; index < count; index++) {
                    result = UnsafeIISMethods.MgdGetNextModule(pModuleCollection, ref index,
                                                               out pBstrModuleName, out cBstrModuleName,
                                                               out pBstrModuleType, out cBstrModuleType,
                                                               out pBstrModulePrecondition, out cBstrModulePrecondition);
                    if (result < 0) {
                        throw new HttpException(SR.GetString(SR.Cant_Read_Native_Modules, result.ToString("X8", CultureInfo.InvariantCulture)));
                    }
                    string moduleName = (cBstrModuleName > 0) ? StringUtil.StringFromWCharPtr(pBstrModuleName, cBstrModuleName) : null;
                    string moduleType = (cBstrModuleType > 0) ? StringUtil.StringFromWCharPtr(pBstrModuleType, cBstrModuleType) : null;
                    string modulePrecondition = (cBstrModulePrecondition > 0) ? StringUtil.StringFromWCharPtr(pBstrModulePrecondition, cBstrModulePrecondition) : String.Empty;
                    Marshal.FreeBSTR(pBstrModuleName);
                    pBstrModuleName = IntPtr.Zero;
                    cBstrModuleName = 0;
                    Marshal.FreeBSTR(pBstrModuleType);
                    pBstrModuleType = IntPtr.Zero;
                    cBstrModuleType = 0;
                    Marshal.FreeBSTR(pBstrModulePrecondition);
                    pBstrModulePrecondition = IntPtr.Zero;
                    cBstrModulePrecondition = 0;

                    if (!String.IsNullOrEmpty(moduleName) && !String.IsNullOrEmpty(moduleType)) {
                        moduleList.Add(new ModuleConfigurationInfo(moduleName, moduleType, modulePrecondition));
                    }
                }
            }
            finally {
                if (pModuleCollection != IntPtr.Zero) {
                    Marshal.Release(pModuleCollection);
                    pModuleCollection = IntPtr.Zero;
                }
                if (pBstrModuleName != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstrModuleName);
                    pBstrModuleName = IntPtr.Zero;
                }
                if (pBstrModuleType != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstrModuleType);
                    pBstrModuleType = IntPtr.Zero;
                }
                if (pBstrModulePrecondition != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstrModulePrecondition);
                    pBstrModulePrecondition = IntPtr.Zero;
                }
            }

            // now that the static list has been processed, add in the dynamic module list
            moduleList.AddRange(GetConfigInfoForDynamicModules());
            _moduleConfigInfo = moduleList;

            return BuildIntegratedModuleCollection(_moduleConfigInfo);
        }

        // gets configuration for modules that have been added to the dynamic registry (integrated pipeline)
        private IEnumerable<ModuleConfigurationInfo> GetConfigInfoForDynamicModules() {
            return from entry in _dynamicModuleRegistry.LockAndFetchList()
                   select new ModuleConfigurationInfo(entry.Name, entry.Type, "managedHandler" /* condition */);
        }

        HttpModuleCollection BuildIntegratedModuleCollection(List<ModuleConfigurationInfo> moduleList) {
            HttpModuleCollection modules = new HttpModuleCollection();

            foreach(ModuleConfigurationInfo mod in moduleList) {
#if DBG
                Debug.Trace("NativeConfig", "Runtime module: " + mod.Name + " of type " + mod.Type + "\n");
#endif
                ModulesEntry currentModule = new ModulesEntry(mod.Name, mod.Type, "type", null);

                modules.AddModule(currentModule.ModuleName, currentModule.Create());
            }

            return modules;
        }

        //
        // Internal classes to support [asynchronous] app execution logic
        //

        internal class AsyncAppEventHandler {
            int _count;
            ArrayList _beginHandlers;
            ArrayList _endHandlers;
            ArrayList _stateObjects;

            internal AsyncAppEventHandler() {
                _count = 0;
                _beginHandlers = new ArrayList();
                _endHandlers   = new ArrayList();
                _stateObjects  = new ArrayList();
            }

            internal void Reset() {
                _count = 0;
                _beginHandlers.Clear();
                _endHandlers.Clear();
                _stateObjects.Clear();
            }

            internal int Count {
                get {
                    return _count;
                }
            }

            internal void Add(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
                _beginHandlers.Add(beginHandler);
                _endHandlers.Add(endHandler);
                _stateObjects.Add(state);
                _count++;
            }

            internal void CreateExecutionSteps(HttpApplication app, ArrayList steps) {
                for (int i = 0; i < _count; i++) {
                    steps.Add(new AsyncEventExecutionStep(
                        app,
                        (BeginEventHandler)_beginHandlers[i],
                        (EndEventHandler)_endHandlers[i],
                        _stateObjects[i]));
                }
            }
        }

        internal class AsyncAppEventHandlersTable {
            private Hashtable _table;

            internal void AddHandler(Object eventId, BeginEventHandler beginHandler,
                                     EndEventHandler endHandler, Object state,
                                     RequestNotification requestNotification,
                                     bool isPost, HttpApplication app) {
                if (_table == null)
                    _table = new Hashtable();

                AsyncAppEventHandler asyncHandler = (AsyncAppEventHandler)_table[eventId];

                if (asyncHandler == null) {
                    asyncHandler = new AsyncAppEventHandler();
                    _table[eventId] = asyncHandler;
                }

                asyncHandler.Add(beginHandler, endHandler, state);

                if (HttpRuntime.UseIntegratedPipeline) {
                    AsyncEventExecutionStep step =
                        new AsyncEventExecutionStep(app,
                                                    beginHandler,
                                                    endHandler,
                                                    state);

                    app.AddEventMapping(app.CurrentModuleCollectionKey, requestNotification, isPost, step);
                }
            }

            internal AsyncAppEventHandler this[Object eventId] {
                get {
                    if (_table == null)
                        return null;
                    return (AsyncAppEventHandler)_table[eventId];
                }
            }
        }

        // interface to represent one execution step
        internal interface IExecutionStep {
            void Execute();
            bool CompletedSynchronously { get;}
            bool IsCancellable { get; }
        }

        // execution step -- stub
        internal class NoopExecutionStep : IExecutionStep {
            internal NoopExecutionStep() {
            }

            void IExecutionStep.Execute() {
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true;}
            }

            bool IExecutionStep.IsCancellable {
                get { return false; }
            }
        }

        // execution step -- call synchronous event
        internal class SyncEventExecutionStep : IExecutionStep {
            private HttpApplication _application;
            private EventHandler    _handler;

            internal SyncEventExecutionStep(HttpApplication app, EventHandler handler) {
                _application = app;
                _handler = handler;
            }

            internal EventHandler Handler {
                get {
                    return _handler;
                }
            }

            void IExecutionStep.Execute() {
                string targetTypeStr = null;

                if (_handler != null) {
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Module)) {
                        targetTypeStr = _handler.Method.ReflectedType.ToString();

                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_ENTER, _application.Context.WorkerRequest, targetTypeStr);
                    }
                    _handler(_application, _application.AppEvent);
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Module)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, _application.Context.WorkerRequest, targetTypeStr);
                }
            }

            bool IExecutionStep.CompletedSynchronously {
                [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get { return true;}
            }

            bool IExecutionStep.IsCancellable {
                [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get { return true; }
            }
        }

        // execution step -- call asynchronous event
        internal class AsyncEventExecutionStep : IExecutionStep {
            private HttpApplication     _application;
            private BeginEventHandler   _beginHandler;
            private EndEventHandler     _endHandler;
            private Object              _state;
            private AsyncCallback       _completionCallback;
            private AsyncStepCompletionInfo _asyncStepCompletionInfo; // per call
            private bool                _sync;          // per call
            private string              _targetTypeStr;

            internal AsyncEventExecutionStep(HttpApplication app, BeginEventHandler beginHandler, EndEventHandler endHandler, Object state)
                :this(app, beginHandler, endHandler, state, HttpRuntime.UseIntegratedPipeline)
                {
                }

            internal AsyncEventExecutionStep(HttpApplication app, BeginEventHandler beginHandler, EndEventHandler endHandler, Object state, bool useIntegratedPipeline) {

                _application = app;
                // Instrument the beginHandler method if AppVerifier is enabled.
                // If AppVerifier not enabled, we just get back the original delegate to beginHandler uninstrumented.
                _beginHandler = AppVerifier.WrapBeginMethod(_application, beginHandler);
                _endHandler = endHandler;
                _state = state;
                _completionCallback = new AsyncCallback(this.OnAsyncEventCompletion);
            }

            private void OnAsyncEventCompletion(IAsyncResult ar) {
                if (ar.CompletedSynchronously) {
                    // Synchronous completions will be handled by IExecutionStep.Execute.
                    return;
                }

                // This IAsyncResult may actually have completed synchronously (we might be on the same thread
                // which called IExecutionStep.Execute) even if CompletedSynchronously = false. Regardless,
                // we should invoke the End* method on the same thread that invoked this callback, as some
                // applications use TLS instead of the IAsyncResult object itself to convey state information.

                Debug.Trace("PipelineRuntime", "AsyncStep.OnAsyncEventCompletion");
                HttpContext context = _application.Context;
                Exception error = null;

                // The asynchronous step has completed, so we should disallow further
                // async operations until the next step.
                context.SyncContext.ProhibitVoidAsyncOperations();

                try {
                    _endHandler(ar);
                }
                catch (Exception e) {
                    error = e;
                }

                bool shouldCallResumeSteps = _asyncStepCompletionInfo.RegisterAsyncCompletion(error);
                if (!shouldCallResumeSteps) {
                    return;
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Module)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, context.WorkerRequest, _targetTypeStr);

                // re-set start time after an async completion (see VSWhidbey 231010)
                context.SetStartTime();

                // Assert to disregard the user code up the stack
                if (HttpRuntime.IsLegacyCas) {
                    ResumeStepsWithAssert(error);
                }
                else {
                    ResumeSteps(error);
                }
            }

            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            void ResumeStepsWithAssert(Exception error) {
                ResumeSteps(error);
            }

            void ResumeSteps(Exception error) {
                _application.ResumeStepsFromThreadPoolThread(error);
            }

            void IExecutionStep.Execute() {
                _sync = false;

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Module)) {
                    _targetTypeStr = _beginHandler.Method.ReflectedType.ToString();
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_ENTER, _application.Context.WorkerRequest, _targetTypeStr);
                }

                HttpContext context = _application.Context;

                _asyncStepCompletionInfo.Reset();
                context.SyncContext.AllowVoidAsyncOperations();
                IAsyncResult ar;
                try {
                    ar = _beginHandler(_application, _application.AppEvent, _completionCallback, _state);
                }
                catch {
                    // The asynchronous step has completed, so we should disallow further
                    // async operations until the next step.
                    context.SyncContext.ProhibitVoidAsyncOperations();
                    throw;
                }

                bool operationCompleted;
                bool mustCallEndHandler;
                _asyncStepCompletionInfo.RegisterBeginUnwound(ar, out operationCompleted, out mustCallEndHandler);

                if (operationCompleted) {
                    _sync = true;

                    if (mustCallEndHandler) {
                        // The asynchronous step has completed, so we should disallow further
                        // async operations until the next step.
                        context.SyncContext.ProhibitVoidAsyncOperations();
                        _endHandler(ar);
                    }

                    _asyncStepCompletionInfo.ReportError();

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Module)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, _application.Context.WorkerRequest, _targetTypeStr);
                }
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return _sync;}
            }

            bool IExecutionStep.IsCancellable {
                get { return false; }
            }
        }

        // execution step -- validate the path for canonicalization issues
        internal class ValidatePathExecutionStep : IExecutionStep {
            private HttpApplication _application;

            internal ValidatePathExecutionStep(HttpApplication app) {
                _application = app;
            }

            void IExecutionStep.Execute() {
                _application.Context.ValidatePath();
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true; }
            }

            bool IExecutionStep.IsCancellable {
                get { return false; }
            }
        }

        // execution step -- validate request (virtual path, query string, entity body, etc)
        internal class ValidateRequestExecutionStep : IExecutionStep {
            private HttpApplication _application;

            internal ValidateRequestExecutionStep(HttpApplication app) {
                _application = app;
            }

            void IExecutionStep.Execute() {
                _application.Context.Request.ValidateInputIfRequiredByConfig();
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true; }
            }

            bool IExecutionStep.IsCancellable {
                get { return false; }
            }
        }

        // materialize handler for integrated pipeline
        // this does not map handler, rather that's done by the core
        // this does instantiate the managed type so that things that need to
        // look at it can
        internal class MaterializeHandlerExecutionStep : IExecutionStep {
            private HttpApplication _application;

            internal MaterializeHandlerExecutionStep(HttpApplication app) {
                _application = app;
            }

            void IExecutionStep.Execute() {
                HttpContext context = _application.Context;
                HttpRequest request = context.Request;
                IHttpHandler handler = null;
                string configType = null;

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_ENTER, context.WorkerRequest);

                IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;

                // Get handler
                if (context.RemapHandlerInstance != null){
                    //RemapHandler overrides all
                    wr.SetScriptMapForRemapHandler();
                    context.Handler = context.RemapHandlerInstance;
                }
                else if (request.RewrittenUrl != null) {
                    // RewritePath, we need to re-map the handler
                    bool handlerExists;
                    configType = wr.ReMapHandlerAndGetHandlerTypeString(context, request.Path, out handlerExists);
                    if (!handlerExists) {
                        // WOS 1973590: When RewritePath is used with missing handler in Integrated Mode,an empty response 200 is returned instead of 404
                        throw new HttpException(404, SR.GetString(SR.Http_handler_not_found_for_request_type, request.RequestType));
                    }
                }
                else {
                    configType = wr.GetManagedHandlerType();
                }

                if (!String.IsNullOrEmpty(configType)) {
                    IHttpHandlerFactory factory = _application.GetFactory(configType);
                    string pathTranslated = request.PhysicalPathInternal;

                    try {
                        handler = factory.GetHandler(context, request.RequestType, request.FilePath, pathTranslated);
                    }
                    catch (FileNotFoundException e) {
                        if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                            throw new HttpException(404, null, e);
                        else
                            throw new HttpException(404, null);
                    }
                    catch (DirectoryNotFoundException e) {
                        if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                            throw new HttpException(404, null, e);
                        else
                            throw new HttpException(404, null);
                    }
                    catch (PathTooLongException e) {
                        if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                            throw new HttpException(414, null, e);
                        else
                            throw new HttpException(414, null);
                    }

                    context.Handler = handler;

                    // Remember for recycling
                    if (_application._handlerRecycleList == null)
                        _application._handlerRecycleList = new ArrayList();
                    _application._handlerRecycleList.Add(new HandlerWithFactory(handler, factory));
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_LEAVE, context.WorkerRequest);
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true;}
            }

            bool IExecutionStep.IsCancellable {
                get { return false; }
            }
        }


        // execution step -- map HTTP handler (used to be a separate module)
        internal class MapHandlerExecutionStep : IExecutionStep {
            private HttpApplication _application;

            internal MapHandlerExecutionStep(HttpApplication app) {
                _application = app;
            }

            void IExecutionStep.Execute() {
                HttpContext context = _application.Context;
                HttpRequest request = context.Request;

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_ENTER, context.WorkerRequest);

                context.Handler = _application.MapHttpHandler(
                    context,
                    request.RequestType,
                    request.FilePathObject,
                    request.PhysicalPathInternal,
                    false /*useAppConfig*/);
                Debug.Assert(context.ConfigurationPath == context.Request.FilePathObject, "context.ConfigurationPath (" +
                             context.ConfigurationPath + ") != context.Request.FilePath (" + context.Request.FilePath + ")");

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_LEAVE, context.WorkerRequest);
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true;}
            }

            bool IExecutionStep.IsCancellable {
                get { return false; }
            }
        }

        // execution step -- call HTTP handler (used to be a separate module)
        internal class CallHandlerExecutionStep : IExecutionStep {
            private HttpApplication   _application;
            private AsyncCallback     _completionCallback;
            private IHttpAsyncHandler _handler;       // per call
            private AsyncStepCompletionInfo _asyncStepCompletionInfo; // per call
            private bool              _sync;          // per call

            internal CallHandlerExecutionStep(HttpApplication app) {
                _application = app;
                _completionCallback = new AsyncCallback(this.OnAsyncHandlerCompletion);
            }

            private void OnAsyncHandlerCompletion(IAsyncResult ar) {
                if (ar.CompletedSynchronously) {
                    // Synchronous completions will be handled by IExecutionStep.Execute.
                    return;
                }

                // This IAsyncResult may actually have completed synchronously (we might be on the same thread
                // which called IExecutionStep.Execute) even if CompletedSynchronously = false. Regardless,
                // we should invoke the End* method on the same thread that invoked this callback, as some
                // applications use TLS instead of the IAsyncResult object itself to convey state information.

                HttpContext context = _application.Context;
                Exception error = null;

                // The asynchronous step has completed, so we should disallow further
                // async operations until the next step.
                context.SyncContext.ProhibitVoidAsyncOperations();

                try {
                    try {
                        _handler.EndProcessRequest(ar);
                    }
                    finally {
                        SuppressPostEndRequestIfNecessary(context);

                        // In Integrated mode, generate the necessary response headers
                        // after the ASP.NET handler runs.  If EndProcessRequest throws,
                        // the headers will be generated by ReportRuntimeError
                        context.Response.GenerateResponseHeadersForHandler();
                    }
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e.InnerException != null && e.InnerException is ThreadAbortException) {
                        // Response.End happened during async operation
                        _application.CompleteRequest();
                    }
                    else {
                        error = e;
                    }
                }

                bool shouldCallResumeSteps = _asyncStepCompletionInfo.RegisterAsyncCompletion(error);
                if (!shouldCallResumeSteps) {
                    return;
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_LEAVE, context.WorkerRequest);

                _handler = null; // not to remember

                // re-set start time after an async completion (see VSWhidbey 231010)
                context.SetStartTime();

                // Assert to disregard the user code up the stack
                if (HttpRuntime.IsLegacyCas) {
                    ResumeStepsWithAssert(error);
                }
                else {
                    ResumeSteps(error);
                }
            }

            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            void ResumeStepsWithAssert(Exception error) {
                ResumeSteps(error);
            }

            void ResumeSteps(Exception error) {
                _application.ResumeStepsFromThreadPoolThread(error);
            }

            private static void SuppressPostEndRequestIfNecessary(HttpContext context) {
                // DevDiv #245124 - ASP.NET now hooks PostEndRequest in order to kick off the WebSocket pipeline.
                // If this is not a WebSocket request or the handshake was not completed, then we can suppress
                // this pipeline event. This allows us to send the appropriate cache headers to the client,
                // and it also gives a small perf boost.

                if (!context.IsWebSocketRequestUpgrading) {
                    IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;
                    if (wr != null) {
                        wr.DisableNotifications(notifications: 0, postNotifications: RequestNotification.EndRequest);
                    }
                }
            }

            void IExecutionStep.Execute() {
                HttpContext context = _application.Context;
                IHttpHandler handler = context.Handler;

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_ENTER, context.WorkerRequest);

                if (handler != null && HttpRuntime.UseIntegratedPipeline) {
                    IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;
                    if (wr != null && wr.IsHandlerExecutionDenied()) {
                        _sync = true;
                        HttpException error = new HttpException(403, SR.GetString(SR.Handler_access_denied));
                        error.SetFormatter(new PageForbiddenErrorFormatter(context.Request.Path, SR.GetString(SR.Handler_access_denied)));
                        throw error;
                    }
                }

                if (handler == null) {
                    _sync = true;
                }
                else if (handler is IHttpAsyncHandler) {
                    // asynchronous handler
                    IHttpAsyncHandler asyncHandler = (IHttpAsyncHandler)handler;

                    _sync = false;
                    _handler = asyncHandler;

                    // Instrument the BeginProcessRequest method if AppVerifier is enabled.
                    // If AppVerifier not enabled, we just get back the original delegate to BeginProcessRequest uninstrumented.
                    var beginProcessRequestDelegate = AppVerifier.WrapBeginMethod<HttpContext>(_application, asyncHandler.BeginProcessRequest);

                    _asyncStepCompletionInfo.Reset();
                    context.SyncContext.AllowVoidAsyncOperations();
                    IAsyncResult ar;
                    try {
                        ar = beginProcessRequestDelegate(context, _completionCallback, null);
                    }
                    catch {
                        // The asynchronous step has completed, so we should disallow further
                        // async operations until the next step.
                        context.SyncContext.ProhibitVoidAsyncOperations();
                        throw;
                    }

                    bool operationCompleted;
                    bool mustCallEndHandler;
                    _asyncStepCompletionInfo.RegisterBeginUnwound(ar, out operationCompleted, out mustCallEndHandler);

                    if (operationCompleted) {
                        _sync = true;
                        _handler = null; // not to remember

                        // The asynchronous step has completed, so we should disallow further
                        // async operations until the next step.
                        context.SyncContext.ProhibitVoidAsyncOperations();

                        try {
                            if (mustCallEndHandler) {
                                asyncHandler.EndProcessRequest(ar);
                            }

                            _asyncStepCompletionInfo.ReportError();
                        }
                        finally {
                            SuppressPostEndRequestIfNecessary(context);

                            //  In Integrated mode, generate the necessary response headers
                            //  after the ASP.NET handler runs
                            context.Response.GenerateResponseHeadersForHandler();
                        }

                        if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_LEAVE, context.WorkerRequest);
                    }
                }
                else {
                    // synchronous handler
                    _sync = true;

                    // disable async operations
                    //_application.SyncContext.Disable();

                    // VSWhidbey 268772 - If a synchronous handler internally kicks off an asynchronous operation and waits (blocking) for that
                    // operation to complete, the handler will deadlock since the asynchronous operation can't come back to the appropriate
                    // thread to perform the completion. The solution below was only meant to be temporary but was accidentally left in the product
                    // for v2.0 RTM, so it's now legacy behavior and cannot be changed.
                    context.SyncContext.SetSyncCaller();

                    try {
                        handler.ProcessRequest(context);
                    }
                    finally {
                        context.SyncContext.ResetSyncCaller();
                        if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_LEAVE, context.WorkerRequest);

                        SuppressPostEndRequestIfNecessary(context);

                        // In Integrated mode, generate the necessary response headers
                        // after the ASP.NET handler runs
                        context.Response.GenerateResponseHeadersForHandler();
                    }
                }
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return _sync;}
            }

            bool IExecutionStep.IsCancellable {
                // launching of async handler should not be cancellable
                get { return (_application.Context.Handler is IHttpAsyncHandler) ? false : true; }
            }
        }

        // execution step -- initiate the transition to a WebSocket request
        internal class TransitionToWebSocketsExecutionStep : IExecutionStep {
            private readonly HttpApplication _application;

            internal TransitionToWebSocketsExecutionStep(HttpApplication app) {
                _application = app;
            }

            void IExecutionStep.Execute() {
                HttpContext context = _application.Context;

                if (context.RootedObjects == null
                    || context.RootedObjects.WebSocketPipeline == null
                    || context.Response.StatusCode != (int)HttpStatusCode.SwitchingProtocols) {

                    // If this isn't a WebSocket request or something has caused the status code
                    // not to be HTTP 101 (such as an error, redirect, or something else), no-op.
                    CompletedSynchronously = true;
                }
                else {
                    // DevDiv #273639: Let the HttpRequest instance maintain a reference to the response
                    // cookie collection, as the HttpResponse instance won't be available after the transition.
                    context.Request.StoreReferenceToResponseCookies(context.Response.GetCookiesNoCreate());

                    // If this is a WebSocket request, mark as transitioned so that asynchronous events (like SendRequest)
                    // don't execute. We also need to mark ourselves as not having completed synchronously so that the
                    // pipeline unwinds back to ProcessRequestNotification. That method special-cases WebSocket handlers
                    // and cleans up the HttpContext / HttpApplication eagerly.

                    // transition: AcceptWebSocketRequestCalled -> TransitionStarted
                    context.TransitionToWebSocketState(WebSocketTransitionState.TransitionStarted);
                    CompletedSynchronously = false;
                }
            }

            public bool CompletedSynchronously {
                get;
                private set;
            }

            public bool IsCancellable {
                // launching of async operation should not be cancellable
                get { return false; }
            }
        }

        // execution step -- call response filter
        internal class CallFilterExecutionStep : IExecutionStep {
            private HttpApplication _application;

            internal CallFilterExecutionStep(HttpApplication app) {
                _application = app;
            }

            void IExecutionStep.Execute() {
                try {
                    _application.Context.Response.FilterOutput();
                }
                finally {
                    // if this is the UpdateCache notification, then disable the LogRequest notification (which handles the error case)
                    if (HttpRuntime.UseIntegratedPipeline && (_application.Context.CurrentNotification == RequestNotification.UpdateRequestCache)) {
                        _application.Context.DisableNotifications(RequestNotification.LogRequest, 0 /*postNotifications*/);
                    }
                }
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true;}
            }

            bool IExecutionStep.IsCancellable {
                get { return true; }
            }
        }

        // integrated pipeline execution step for RaiseOnPreSendRequestHeaders and RaiseOnPreSendRequestContent
        internal class SendResponseExecutionStep : IExecutionStep {
            private HttpApplication _application;
            private EventHandler _handler;
            private bool _isHeaders;

            internal SendResponseExecutionStep(HttpApplication app, EventHandler handler, bool isHeaders) {
                _application = app;
                _handler = handler;
                _isHeaders = isHeaders;
            }

            void IExecutionStep.Execute() {

                // IIS only has a SendResponse notification, so we check the flags
                // to determine whether this notification is for headers or content.
                // The step uses _isHeaders to keep track of whether this is for headers or content.
                if (_application.Context.IsSendResponseHeaders && _isHeaders
                    || !_isHeaders) {

                    string targetTypeStr = null;

                    if (_handler != null) {
                        if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Module)) {
                            targetTypeStr = _handler.Method.ReflectedType.ToString();

                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_ENTER, _application.Context.WorkerRequest, targetTypeStr);
                        }
                        _handler(_application, _application.AppEvent);
                        if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Module)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, _application.Context.WorkerRequest, targetTypeStr);
                    }
                }
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true;}
            }

            bool IExecutionStep.IsCancellable {
                get { return true; }
            }
        }

        internal class UrlMappingsExecutionStep : IExecutionStep {
            private HttpApplication _application;


            internal UrlMappingsExecutionStep(HttpApplication app) {
                _application = app;
            }

            void IExecutionStep.Execute() {
                HttpContext context = _application.Context;
                UrlMappingsModule.UrlMappingRewritePath(context);
            }

            bool IExecutionStep.CompletedSynchronously {
                get { return true;}
            }

            bool IExecutionStep.IsCancellable {
                get { return false; }
            }
        }

        internal abstract class StepManager {
            protected HttpApplication _application;
            protected bool _requestCompleted;

            internal StepManager(HttpApplication application) {
                _application = application;
            }

            internal bool IsCompleted { get { return _requestCompleted; } }

            internal abstract void BuildSteps(WaitCallback stepCallback);

            internal void CompleteRequest() {
                _requestCompleted = true;
                if (HttpRuntime.UseIntegratedPipeline) {
                    HttpContext context = _application.Context;
                    if (context != null && context.NotificationContext != null) {
                        context.NotificationContext.RequestCompleted = true;
                    }
                }
            }

            internal abstract void InitRequest();

            internal abstract void ResumeSteps(Exception error);
        }

        internal class ApplicationStepManager : StepManager {
            private IExecutionStep[] _execSteps;
            private WaitCallback _resumeStepsWaitCallback;
            private int _currentStepIndex;
            private int _numStepCalls;
            private int _numSyncStepCalls;
            private int _endRequestStepIndex;

            internal ApplicationStepManager(HttpApplication app): base(app) {
            }

            internal override void BuildSteps(WaitCallback stepCallback ) {
                ArrayList steps = new ArrayList();
                HttpApplication app = _application;

                bool urlMappingsEnabled = false;
                UrlMappingsSection urlMappings = RuntimeConfig.GetConfig().UrlMappings;
                urlMappingsEnabled = urlMappings.IsEnabled && ( urlMappings.UrlMappings.Count > 0 );

                steps.Add(new ValidateRequestExecutionStep(app));
                steps.Add(new ValidatePathExecutionStep(app));

                if (urlMappingsEnabled)
                    steps.Add(new UrlMappingsExecutionStep(app)); // url mappings

                app.CreateEventExecutionSteps(HttpApplication.EventBeginRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventAuthenticateRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventDefaultAuthentication, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostAuthenticateRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventAuthorizeRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostAuthorizeRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventResolveRequestCache, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostResolveRequestCache, steps);
                steps.Add(new MapHandlerExecutionStep(app));     // map handler
                app.CreateEventExecutionSteps(HttpApplication.EventPostMapRequestHandler, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventAcquireRequestState, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostAcquireRequestState, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPreRequestHandlerExecute, steps);
                steps.Add(app.CreateImplicitAsyncPreloadExecutionStep()); // implict async preload step
                steps.Add(new CallHandlerExecutionStep(app));  // execute handler
                app.CreateEventExecutionSteps(HttpApplication.EventPostRequestHandlerExecute, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventReleaseRequestState, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostReleaseRequestState, steps);
                steps.Add(new CallFilterExecutionStep(app));  // filtering
                app.CreateEventExecutionSteps(HttpApplication.EventUpdateRequestCache, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostUpdateRequestCache, steps);
                _endRequestStepIndex = steps.Count;
                app.CreateEventExecutionSteps(HttpApplication.EventEndRequest, steps);
                steps.Add(new NoopExecutionStep()); // the last is always there

                _execSteps = new IExecutionStep[steps.Count];
                steps.CopyTo(_execSteps);

                // callback for async completion when reposting to threadpool thread
                _resumeStepsWaitCallback = stepCallback;
            }

            internal override void InitRequest() {
                _currentStepIndex   = -1;
                _numStepCalls       = 0;
                _numSyncStepCalls   = 0;
                _requestCompleted   = false;
            }

            // This attribute prevents undesirable 'just-my-code' debugging behavior (VSWhidbey 404406/VSWhidbey 609188)
            [System.Diagnostics.DebuggerStepperBoundaryAttribute]
            internal override void ResumeSteps(Exception error) {
                bool appCompleted = false;
                bool stepCompletedSynchronously = true;
                HttpApplication app = _application;
                CountdownTask appInstanceConsumersCounter = app.ApplicationInstanceConsumersCounter;
                HttpContext context = app.Context;
                ThreadContext threadContext = null;
                AspNetSynchronizationContextBase syncContext = context.SyncContext;

                Debug.Trace("Async", "HttpApplication.ResumeSteps");

                try {
                    if (appInstanceConsumersCounter != null) {
                        appInstanceConsumersCounter.MarkOperationPending(); // ResumeSteps call started
                    }

                    using (syncContext.AcquireThreadLock()) {
                        // avoid ---- between the app code and fast async completion from a module


                        try {
                            threadContext = app.OnThreadEnter();
                        }
                        catch (Exception e) {
                            if (error == null)
                                error = e;
                        }

                        try {
                            try {
                                for (; ; ) {
                                    // record error

                                    if (syncContext.Error != null) {
                                        error = syncContext.Error;
                                        syncContext.ClearError();
                                    }

                                    if (error != null) {
                                        app.RecordError(error);
                                        error = null;
                                    }

                                    // check for any outstanding async operations

                                    if (syncContext.PendingCompletion(_resumeStepsWaitCallback)) {
                                        // wait until all pending async operations complete
                                        break;
                                    }

                                    // advance to next step

                                    if (_currentStepIndex < _endRequestStepIndex && (context.Error != null || _requestCompleted)) {
                                        // end request
                                        context.Response.FilterOutput();
                                        _currentStepIndex = _endRequestStepIndex;
                                    }
                                    else {
                                        _currentStepIndex++;
                                    }

                                    if (_currentStepIndex >= _execSteps.Length) {
                                        appCompleted = true;
                                        break;
                                    }

                                    // execute the current step

                                    _numStepCalls++;          // count all calls

                                    // enable launching async operations before each new step
                                    syncContext.Enable();

                                    // call to execute current step catching thread abort exception
                                    error = app.ExecuteStep(_execSteps[_currentStepIndex], ref stepCompletedSynchronously);

                                    // unwind the stack in the async case
                                    if (!stepCompletedSynchronously)
                                        break;

                                    _numSyncStepCalls++;      // count synchronous calls
                                }
                            }
                            finally {
                                if (appCompleted) {
                                    // need to raise OnRequestCompleted while within the ThreadContext so that things like User, CurrentCulture, etc. are available
                                    context.RaiseOnRequestCompleted();
                                }

                                if (threadContext != null) {
                                    try {
                                        threadContext.DisassociateFromCurrentThread();
                                    }
                                    catch {
                                    }
                                }
                            }
                        }
                        catch { // Protect against exception filters
                            throw;
                        }

                    }   // using

                    if (appCompleted) {
                        // need to raise OnPipelineCompleted outside of the ThreadContext so that HttpContext.Current, User, etc. are unavailable
                        context.RaiseOnPipelineCompleted();

                        // unroot context (async app operations ended)
                        context.Unroot();

                        // async completion
                        app.AsyncResult.Complete((_numStepCalls == _numSyncStepCalls), null, null);
                        app.ReleaseAppInstance();
                    }
                }
                finally {
                    if (appInstanceConsumersCounter != null) {
                        appInstanceConsumersCounter.MarkOperationCompleted(); // ResumeSteps call complete
                    }
                }
            }
        }

        internal class PipelineStepManager : StepManager {

            WaitCallback _resumeStepsWaitCallback;
            bool _validatePathCalled;
            bool _validateInputCalled;

            internal PipelineStepManager(HttpApplication app): base(app) {
            }

            internal override void BuildSteps(WaitCallback stepCallback) {
                Debug.Trace("PipelineRuntime", "BuildSteps");
                //ArrayList steps = new ArrayList();
                HttpApplication app = _application;

                // add special steps that don't currently
                // correspond to a configured handler

                IExecutionStep materializeStep = new MaterializeHandlerExecutionStep(app);

                // implicit map step
                app.AddEventMapping(
                    HttpApplication.IMPLICIT_HANDLER,
                    RequestNotification.MapRequestHandler,
                    false, materializeStep);

                // implicit async preload step
                app.AddEventMapping(
                    HttpApplication.IMPLICIT_HANDLER,
                    RequestNotification.ExecuteRequestHandler,
                    false, app.CreateImplicitAsyncPreloadExecutionStep());

                // implicit handler routing step
                IExecutionStep handlerStep = new CallHandlerExecutionStep(app);

                app.AddEventMapping(
                    HttpApplication.IMPLICIT_HANDLER,
                    RequestNotification.ExecuteRequestHandler,
                    false, handlerStep);

                // implicit handler WebSockets step
                IExecutionStep webSocketsStep = new TransitionToWebSocketsExecutionStep(app);

                app.AddEventMapping(
                    HttpApplication.IMPLICIT_HANDLER,
                    RequestNotification.EndRequest,
                    true /* isPostNotification */, webSocketsStep);

                // add implicit request filtering step
                IExecutionStep filterStep = new CallFilterExecutionStep(app);

                // normally, this executes during UpdateRequestCache as a high priority module
                app.AddEventMapping(
                    HttpApplication.IMPLICIT_FILTER_MODULE,
                    RequestNotification.UpdateRequestCache,
                    false, filterStep);

                // for error conditions, this executes during LogRequest as a high priority module
                app.AddEventMapping(
                    HttpApplication.IMPLICIT_FILTER_MODULE,
                    RequestNotification.LogRequest,
                    false, filterStep);

                _resumeStepsWaitCallback = stepCallback;
            }

            internal override void InitRequest() {
                _requestCompleted = false;
                _validatePathCalled = false;
                _validateInputCalled = false;
            }

            // PipelineStepManager::ResumeSteps
            // called from IIS7 (on IIS thread) via BeginProcessRequestNotification
            // or from an async completion (on CLR thread) via HttpApplication::ResumeStepsFromThreadPoolThread
            // This attribute prevents undesirable 'just-my-code' debugging behavior (VSWhidbey 404406/VSWhidbey 609188)
            [System.Diagnostics.DebuggerStepperBoundaryAttribute]
            internal override void ResumeSteps(Exception error) {
                HttpContext context = _application.Context;
                IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;
                AspNetSynchronizationContextBase syncContext = context.SyncContext;

                RequestNotificationStatus status = RequestNotificationStatus.Continue;
                ThreadContext threadContext = null;
                bool needToDisassociateThreadContext = false;
                bool isSynchronousCompletion = false;
                bool needToComplete = false;
                bool stepCompletedSynchronously = false;
                int currentModuleLastEventIndex = _application.CurrentModuleContainer.GetEventCount(context.CurrentNotification, context.IsPostNotification) - 1;
                CountdownTask appInstanceConsumersCounter = _application.ApplicationInstanceConsumersCounter;

                using (context.RootedObjects.WithinTraceBlock()) {
                    // DevDiv Bugs 187441: IIS7 Integrated Mode: Problem flushing Response from background threads in IIS7 integrated mode
                    bool isReEntry = context.NotificationContext.IsReEntry;
                    if (!isReEntry) // currently we only re-enter for SendResponse
                {
                        syncContext.AssociateWithCurrentThread();
                    }
                    try {
                        if (appInstanceConsumersCounter != null) {
                            appInstanceConsumersCounter.MarkOperationPending(); // ResumeSteps call started
                        }

                        bool locked = false;
                        try {
                            // As a performance optimization, ASP.NET uses the IIS IHttpContext::IndicateCompletion function to continue executing notifications
                            // on a thread that is associated with the AppDomain.  This is done by calling IndicateCompletion from within the AppDomain, instead
                            // of returning to native code.  This technique can only be used for notifications that complete synchronously.

                            // There are two cases where notifications happen on a thread that has an initialized ThreadContext, and therefore does not need
                            // to call ThreadContext.OnThreadEnter.  These include SendResponse notifications and notifications that occur within a call to
                            // IndicateCompletion.  Note that SendResponse notifications occur on-demand, i.e., they happen when another notification triggers
                            // a SendResponse, at which point it blocks until the SendResponse notification completes.

                            if (!isReEntry) { // currently we only re-enter for SendResponse
                                // DevDiv 482614 (Sharepoint Bug 3137123)
                                // Async completion or SendResponse can happen on a background thread while the thread that called IndicateCompletion has not unwound yet
                                // Therefore (InIndicateCompletion == true) is not a sufficient evidence that we can use the ThreadContext stored in IndicateCompletionContext
                                // To avoid using other thread's ThreadContext we use IndicateCompletionContext only if ThreadInsideIndicateCompletion is indeed our thread
                                if (context.InIndicateCompletion && context.ThreadInsideIndicateCompletion == Thread.CurrentThread) {
                                    // we already have a ThreadContext
                                    threadContext = context.IndicateCompletionContext;
                                    if (context.UsesImpersonation) {
                                        // UsesImpersonation is set to true after RQ_AUTHENTICATE_REQUEST
                                        threadContext.SetImpersonationContext();
                                    }
                                }
                                else {
                                    // we need to create a new ThreadContext
                                    threadContext = _application.OnThreadEnter(context.UsesImpersonation);
                                    // keep track if we need to disassociate it later
                                    needToDisassociateThreadContext = true;
                                }
                            }

                            for (; ; ) {
#if DBG
                                Debug.Trace("PipelineRuntime", "ResumeSteps: CurrentModuleEventIndex=" + context.CurrentModuleEventIndex);
#endif

                                // check and record errors into the HttpContext
                                if (syncContext.Error != null) {
                                    error = syncContext.Error;
                                    syncContext.ClearError();
                                }
                                if (error != null) {
                                    // the error can be cleared by the user
                                    _application.RecordError(error);
                                    error = null;
                                }

                                if (!_validateInputCalled || !_validatePathCalled) {
                                    error = ValidateHelper(context);
                                    if (error != null) {
                                        continue;
                                    }
                                }

                                // check for any outstanding async operations
                                if (syncContext.PendingCompletion(_resumeStepsWaitCallback)) {
                                    // Since the step completed asynchronously, this thread must return RequestNotificationStatus.Pending to IIS,
                                    // and the async completion of this step must call IIS7WorkerRequest::PostCompletion.  The async completion of
                                    // this step will call ResumeSteps again.
                                    context.NotificationContext.PendingAsyncCompletion = true;
                                    break;
                                }

                                // LogRequest and EndRequest never report errors, and never return a status of FinishRequest.
                                bool needToFinishRequest = (context.NotificationContext.Error != null || context.NotificationContext.RequestCompleted)
                                    && context.CurrentNotification != RequestNotification.LogRequest
                                    && context.CurrentNotification != RequestNotification.EndRequest;

                                if (needToFinishRequest || context.CurrentModuleEventIndex == currentModuleLastEventIndex) {

                                    // if an error occured or someone completed the request, set the status to FinishRequest
                                    status = needToFinishRequest ? RequestNotificationStatus.FinishRequest : RequestNotificationStatus.Continue;

                                    // async case
                                    if (context.NotificationContext.PendingAsyncCompletion) {
                                        context.Response.SyncStatusIntegrated();
                                        context.NotificationContext.PendingAsyncCompletion = false;
                                        isSynchronousCompletion = false;
                                        needToComplete = true;
                                        break;
                                    }

                                    // [....] case (we might be able to stay in managed code and execute another notification)
                                    if (needToFinishRequest || UnsafeIISMethods.MgdGetNextNotification(wr.RequestContext, RequestNotificationStatus.Continue) != 1) {
                                        isSynchronousCompletion = true;
                                        needToComplete = true;
                                        break;
                                    }

                                    int currentModuleIndex = 0;
                                    bool isPostNotification = false;
                                    int currentNotification = 0;

                                    UnsafeIISMethods.MgdGetCurrentNotificationInfo(wr.RequestContext, out currentModuleIndex, out isPostNotification, out currentNotification);

                                    // setup the HttpContext for this event/module combo
                                    context.CurrentModuleIndex = currentModuleIndex;
                                    context.IsPostNotification = isPostNotification;
                                    context.CurrentNotification = (RequestNotification)currentNotification;
                                    context.CurrentModuleEventIndex = -1;
                                    currentModuleLastEventIndex = _application.CurrentModuleContainer.GetEventCount(context.CurrentNotification, context.IsPostNotification) - 1;
                                }

                                context.CurrentModuleEventIndex++;

                                IExecutionStep step = _application.CurrentModuleContainer.GetNextEvent(context.CurrentNotification, context.IsPostNotification,
                                                                                                       context.CurrentModuleEventIndex);

                                // enable launching async operations before each new step
                                context.SyncContext.Enable();

                                stepCompletedSynchronously = false;
                                error = _application.ExecuteStep(step, ref stepCompletedSynchronously);

#if DBG
                                Debug.Trace("PipelineRuntime", "ResumeSteps: notification=" + context.CurrentNotification.ToString()
                                            + ", isPost=" + context.IsPostNotification
                                            + ", step=" + step.GetType().FullName
                                            + ", completedSync=" + stepCompletedSynchronously
                                            + ", moduleName=" + _application.CurrentModuleContainer.DebugModuleName
                                            + ", moduleIndex=" + context.CurrentModuleIndex
                                            + ", eventIndex=" + context.CurrentModuleEventIndex);
#endif


                                if (!stepCompletedSynchronously) {
                                    // Since the step completed asynchronously, this thread must return RequestNotificationStatus.Pending to IIS,
                                    // and the async completion of this step must call IIS7WorkerRequest::PostCompletion.  The async completion of
                                    // this step will call ResumeSteps again.
                                    //context.AcquireNotifcationContextLockBeforeUnwind();
                                    _application.AcquireNotifcationContextLock(ref locked);
                                    context.NotificationContext.PendingAsyncCompletion = true;
                                    break;
                                }
                                else {
                                    context.Response.SyncStatusIntegrated();
                                }
                            }
                        }
                        finally {
                            if (locked) {
                                _application.ReleaseNotifcationContextLock();
                            }
                            if (threadContext != null) {
                                if (context.InIndicateCompletion) {
                                    if (isSynchronousCompletion) {
                                        // this is a [....] completion on an IIS thread
                                        threadContext.Synchronize();
                                        // Note for DevDiv 482614 fix:
                                        // If this threadContext is from IndicateCompletionContext (e.g. this thread called IndicateCompletion)
                                        // then we continue reusing this thread and only undo impersonation before unwinding back to IIS.
                                        //
                                        // If this threadContext was created while another thread was and still is in IndicateCompletion call
                                        // (e.g. [....] or async flush on a background thread from native code, not managed since isReEnty==false)
                                        // then we can not reuse this thread and this threadContext will be cleaned before we leave ResumeSteps
                                        // (because needToDisassociateThreadContext was set to true when we created this threadContext)

                                        //always undo impersonation so that the token is removed before returning to IIS (DDB 156421)
                                        threadContext.UndoImpersonationContext();
                                    }
                                    else {
                                        // We're returning pending on an IIS thread in a call to IndicateCompletion.
                                        // Leave the thread context now while we're still under the lock so that the
                                        // async completion does not corrupt the state of HttpContext or IndicateCompletionContext.
                                        if (!threadContext.HasBeenDisassociatedFromThread) {
                                            lock (threadContext) {
                                                if (!threadContext.HasBeenDisassociatedFromThread) {
                                                    threadContext.DisassociateFromCurrentThread();
                                                    // remember to not disassociate again
                                                    needToDisassociateThreadContext = false;
                                                    // DevDiv 482614:
                                                    // Async steps or completions may happen while another thread is inside IndicateCompletion
                                                    // We do not clear IndicateCompletionContext if it belongs to another thread
                                                    // (otherwise future notifications on the thread that called IndicateCompletion won't have
                                                    // context.IndicateCompletionContext pointing to their not yet disassociated ThreadContext)
                                                    if (context.ThreadInsideIndicateCompletion == Thread.CurrentThread) {
                                                        context.IndicateCompletionContext = null;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (isSynchronousCompletion) {
                                    Debug.Assert(needToDisassociateThreadContext == true, "needToDisassociateThreadContext MUST BE true");
                                    // this is a [....] completion on an IIS thread
                                    threadContext.Synchronize();
                                    // get ready to call IndicateCompletion
                                    context.IndicateCompletionContext = threadContext;
                                    // Note for DevDiv 482614 fix:
                                    // This thread created a new ThreadContext if it did not call IndicateCompletion yet or if there was 
                                    // another thread already in IndicateCompletion (a background flush from native code or a completion 
                                    // on another thread). In either case if currently there is no thread in IndicateCompletion 
                                    // then we can reuse this thread and its threadContext and call IndicateCompletion on the current thread.
                                    // In this case we will not disassociate this threadContext now
                                    needToDisassociateThreadContext = false;
                                    //always undo impersonation so that the token is removed before returning to IIS (DDB 156421)
                                    threadContext.UndoImpersonationContext();
                                }
                                else {
                                    Debug.Assert(needToDisassociateThreadContext == true, "needToDisassociateThreadContext MUST BE true");
                                    // We're not in a call to IndicateCompletion.  We're either returning pending or
                                    // we're in an async completion, and therefore we must clean-up the thread state. Impersonation is reverted
                                    threadContext.DisassociateFromCurrentThread();
                                    // remember to not disassociate again
                                    needToDisassociateThreadContext = false;
                                }

                                // Cleanup the thread state unless we prepared to call IndicateCompletion or already cleaned up
                                if (needToDisassociateThreadContext) {
                                    threadContext.DisassociateFromCurrentThread();
                                }
                            }
                        }

                        // WOS #1703315: we cannot complete until after OnThreadLeave is called.
                        if (needToComplete) {
                            // call HttpRuntime::OnRequestNotificationCompletion
                            _application.AsyncResult.Complete(isSynchronousCompletion, null /*result*/, null /*error*/, status);
                        }
                    } // end of try statement that begins after AssociateWithCurrentThread
                    finally {
                        if (!isReEntry) {
                            syncContext.DisassociateFromCurrentThread();
                        }
                        if (appInstanceConsumersCounter != null) {
                            appInstanceConsumersCounter.MarkOperationCompleted(); // ResumeSteps call completed
                        }
                    }
                }
            }
            
            private Exception ValidateHelper(HttpContext context) {
                if (!_validateInputCalled) {
                    _validateInputCalled = true;
                    try {
                        context.Request.ValidateInputIfRequiredByConfig();
                    }
                    catch(Exception e) {
                        return e;
                    }
                }
                if (!_validatePathCalled) {
                    _validatePathCalled = true;
                    try {
                        context.ValidatePath();
                    }
                    catch(Exception e) {
                        return e;
                    }
                }
                return null;
            }
        }

        // WARNING: Mutable struct for performance reasons; exercise caution when using this type.
        private struct AsyncStepCompletionInfo {
#pragma warning disable 420 // volatile passed by reference; our uses are safe
            // state for async execution steps
            private const int ASYNC_STATE_NONE = 0;
            private const int ASYNC_STATE_BEGIN_UNWOUND = 1;
            private const int ASYNC_STATE_CALLBACK_COMPLETED = 2;

            private volatile int _asyncState;
            private ExceptionDispatchInfo _error;

            // Invoked from the callback to signal that the End* method has run to completion.
            // Returns 'true' if the current thread should call ResumeSteps, 'false' if not.
            public bool RegisterAsyncCompletion(Exception error) {
                // Before the call to Exchange below, the _asyncCompletionInfo field will have the value
                // ASYNC_STATE_NONE or ASYNC_STATE_BEGIN_UNWOUND. If it's the former, then the Begin* method
                // hasn't yet returned control to IExecutionStep.Execute. From this step's point of view,
                // this can be treated as a synchronous completion, which will allow us to call ResumeSteps
                // on the original thread and save the cost of destroying  the existing ThreadContext and
                // creating a new one. If the original value is instead ASYNC_STATE_BEGIN_UNWOUND, then
                // the Begin* method already returned control to IExecutionStep.Execute and this step was
                // marked as having an asynchronous completion. The original thread will tear down the
                // ThreadContext, so the current thread should call back into ResumeSteps to resurrect it.
                //
                // If there was an error, we'll use the _error field to store it so that IExecutionStep.Execute
                // can rethrow it as it's unwinding.

                // Interlocked performs a volatile write; all processors will see the write to _error as being
                // no later than the write to _asyncState.
                _error = (error != null) ? ExceptionDispatchInfo.Capture(error) : null;
                int originalState = Interlocked.Exchange(ref _asyncState, ASYNC_STATE_CALLBACK_COMPLETED);
                if (originalState == ASYNC_STATE_NONE) {
                    return false; // IExecutionStep.Execute should call ResumeSteps
                }

                Debug.Assert(originalState == ASYNC_STATE_BEGIN_UNWOUND, "Unexpected state.");
                _error = null; // to prevent long-lived exception object; write doesn't need to be volatile since nobody reads this field anyway in this case
                return true; // this thread should call ResumeSteps
            }

            public void RegisterBeginUnwound(IAsyncResult asyncResult, out bool operationCompleted, out bool mustCallEndHandler) {
                operationCompleted = false;
                mustCallEndHandler = false;

                int originalState = Interlocked.Exchange(ref _asyncState, ASYNC_STATE_BEGIN_UNWOUND);
                if (originalState == ASYNC_STATE_NONE) {
                    if (asyncResult.CompletedSynchronously) {
                        // Synchronous completion; the callback either wasn't called or was a no-op.
                        // In either case, we should call the End* method from this thread.
                        operationCompleted = true;
                        mustCallEndHandler = true;
                    }

                    // Otherwise, this is an asynchronous completion, and the callback hasn't yet been invoked or hasn't fully completed.
                    // We'll let the thread that invokes the callback call the End* method.
                }
                else {
                    Debug.Assert(originalState == ASYNC_STATE_CALLBACK_COMPLETED, "Unexpected state.");

                    // The operation completed, and the callback already invoked the End* method.
                    // The only thing we need to do is to report to our caller that the operation completed synchronously
                    // (so that ResumeSteps runs on this thread) and to observe any exceptions that occurred.
                    operationCompleted = true;
                }

                // Interlocked performs a volatile read; if RethrowExceptionIfNecessary() is called after RegisterBeginUnwound(),
                // the thread will see the correct value for the _error field.
            }

            public void ReportError() {
                // Using ExceptionDispatchInfo preserves the Exception's stack trace when rethrowing.
                ExceptionDispatchInfo error = _error;
                if (error != null) {
                    _error = null; // prevent long-lived Exception objects on the heap
                    error.Throw();
                }
            }

            public void Reset() {
                // All processors see the _error field write as being no later than the _asyncState field write.
                _error = null;
                _asyncState = ASYNC_STATE_NONE;
            }
#pragma warning restore 420 // volatile passed by reference
        }
    }
}
