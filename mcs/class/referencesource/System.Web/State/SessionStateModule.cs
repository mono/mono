//------------------------------------------------------------------------------
// <copyright file="SessionStateModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * SessionStateModule
 *
 * Copyright (c) 1998-2002, Microsoft Corporation
 *
 */

namespace System.Web.SessionState {

    using System;
    using System.Threading;
    using System.Collections;
    using System.Configuration;
    using System.IO;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Xml;
    using System.Security.Cryptography;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.Web.Management;
    using Microsoft.Win32;

    public delegate void SessionStateItemExpireCallback(
            string id, SessionStateStoreData item);

    class SessionOnEndTargetWorkItem {
        SessionOnEndTarget  _target;
        HttpSessionState    _sessionState;

        internal SessionOnEndTargetWorkItem(SessionOnEndTarget target, HttpSessionState sessionState) {
            _target = target;
            _sessionState = sessionState;
        }

        internal void RaiseOnEndCallback() {
            _target.RaiseOnEnd(_sessionState);
        }
    }

    /*
     * Calls the OnSessionEnd event. We use an object other than the SessionStateModule
     * because the state of the module is unknown - it could have been disposed
     * when a session ends.
     */
    class SessionOnEndTarget {
        internal int _sessionEndEventHandlerCount;

        internal SessionOnEndTarget() {
        }

        internal int SessionEndEventHandlerCount {
            get {
                return _sessionEndEventHandlerCount;
            }
            set {
                _sessionEndEventHandlerCount = value;
            }
        }

        internal void RaiseOnEnd(HttpSessionState sessionState) {
            Debug.Trace("SessionOnEnd", "Firing OnSessionEnd for " + sessionState.SessionID);

            if (_sessionEndEventHandlerCount > 0) {
                HttpApplicationFactory.EndSession(sessionState, this, EventArgs.Empty);
            }
        }

        internal void RaiseSessionOnEnd(String id, SessionStateStoreData item) {
            HttpSessionStateContainer sessionStateContainer = new HttpSessionStateContainer(
                    id,
                    item.Items,
                    item.StaticObjects,
                    item.Timeout,
                    false,
                    SessionStateModule.s_configCookieless,
                    SessionStateModule.s_configMode,
                    true);

            HttpSessionState    sessionState = new HttpSessionState(sessionStateContainer);

            if (HttpRuntime.ShutdownInProgress) {
                // call directly when shutting down
                RaiseOnEnd(sessionState);
            }
            else {
                // post via thread pool
                SessionOnEndTargetWorkItem workItem = new SessionOnEndTargetWorkItem(this, sessionState);
                WorkItem.PostInternal(new WorkItemCallback(workItem.RaiseOnEndCallback));
            }
        }

    }


    /*
     * The sesssion state module provides session state services
     * for an application.
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class SessionStateModule : ISessionStateModule {

        internal const string SQL_CONNECTION_STRING_DEFAULT = "data source=localhost;Integrated Security=SSPI";
        internal const string STATE_CONNECTION_STRING_DEFAULT = "tcpip=loopback:42424";
        internal const int                  TIMEOUT_DEFAULT = 20;
        internal const SessionStateMode     MODE_DEFAULT = SessionStateMode.InProc;

        private static long                 LOCKED_ITEM_POLLING_INTERVAL = 500; // in milliseconds
        static readonly TimeSpan            LOCKED_ITEM_POLLING_DELTA = new TimeSpan(250 * TimeSpan.TicksPerMillisecond);

        static readonly TimeSpan            DEFAULT_DBG_EXECUTION_TIMEOUT = new TimeSpan(0, 0, System.Web.Compilation.PageCodeDomTreeGenerator.DebugScriptTimeout);

        // When we are using Cache to store session state (InProc and StateServer),
        // can't specify a timeout value larger than 1 year because CacheEntry ctor
        // will throw an exception.
        internal const int                  MAX_CACHE_BASED_TIMEOUT_MINUTES = 365 * 24 * 60;

        bool                                s_oneTimeInit;
        static int                          s_timeout;

        #pragma warning disable 0649
        static ReadWriteSpinLock            s_lock;
        #pragma warning restore 0649

        static bool                         s_trustLevelInsufficient;

        static TimeSpan                     s_configExecutionTimeout;

        static bool                         s_configRegenerateExpiredSessionId;
        static bool                         s_useHostingIdentity;
        internal static HttpCookieMode      s_configCookieless;
        internal static SessionStateMode    s_configMode;
        
        // This is used as a perf optimization for IIS7 Integrated Mode.  If session state is released
        // in ReleaseState, we can disable the EndRequest notification if the mode is InProc or StateServer
        // because neither InProcSessionStateStore.EndRequest nor OutOfProcSessionStateStore.EndRequest
        // are implemented.
        static bool                         s_canSkipEndRequestCall;

        private static bool s_PollIntervalRegLookedUp = false;
        private static object s_PollIntervalRegLock = new object();

        //
        // Check if we can optmize for InProc case.
        // Optimization details:
        //
        // If we are in InProc mode, and cookieless=false, in certain scenarios we
        // can avoid reading the session ID from the cookies because that's an expensive operation.
        // To allow that, we use s_sessionEverSet to keep track of whether we've ever created
        // any session state.
        //
        // If no session has ever be created, we can optimize in the following two cases:
        //
        // Case 1: Page has disabled session state
        // In BeginAcquireState, we usually read the session ID, and reset the timeout value
        // of the session state.  However, since no session has ever been created, we can
        // skip both reading the session id and resetting the timeout.
        //
        // Case 2: Page has enabled session state
        // In this case, we will delay reading (and creating it if not found) the session ID
        // until it's really needed. (e.g. from HttpSessionStateContainer.SessionID)
        //
        // Please note that we optimize only if the app is using SessionIDManager
        // as the session ID provider; otherwise, we do not have knowledge about
        // the provider in order to optimize safely.
        //
        // And we will delay reading the id only if we are using cookie to store the session ID.  If we
        // use cookieless, in the delayed session ID creation scenario, cookieless requires a redirect,
        // and it'll be bad to do that in the middle of a page execution.
        //
        static bool                         s_allowInProcOptimization;
        static bool                         s_sessionEverSet;

        //
        // Another optimization is to delay the creation of a new session state store item
        // until it's needed.
        static bool                         s_allowDelayedStateStoreItemCreation;
        static HttpSessionStateContainer    s_delayedSessionState = new HttpSessionStateContainer();

        /* per application vars */
        EventHandler                   _sessionStartEventHandler;
        Timer                          _timer;
        TimerCallback                  _timerCallback;
        volatile int                   _timerId;
        ISessionIDManager              _idManager;
        bool                           _usingAspnetSessionIdManager;
        SessionStateStoreProviderBase  _store;
        bool                           _supportSessionExpiry;
        IPartitionResolver             _partitionResolver;
        bool                           _ignoreImpersonation;
        readonly SessionOnEndTarget    _onEndTarget = new SessionOnEndTarget();

        /* per request data goes in _rq* variables */
        bool                            _acquireCalled;
        bool                            _releaseCalled;
        HttpSessionStateContainer       _rqSessionState;
        String                          _rqId;
        bool                            _rqIdNew;
        ISessionStateItemCollection     _rqSessionItems;
        HttpStaticObjectsCollection     _rqStaticObjects;
        bool                            _rqIsNewSession;
        bool                            _rqSessionStateNotFound;
        bool                            _rqReadonly;
        HttpContext                     _rqContext;
        HttpAsyncResult                 _rqAr;
        SessionStateStoreData           _rqItem;
        object                          _rqLockId;  // The id of its SessionStateItem ownership
                                                    // If the ownership change hands (e.g. this ownership
                                                    // times out), the lockId of the item at the store
                                                    // will change.
        int                             _rqInCallback;
        DateTime                        _rqLastPollCompleted;
        TimeSpan                        _rqExecutionTimeout;
        bool                            _rqAddedCookie;
        SessionStateActions             _rqActionFlags;
        ImpersonationContext            _rqIctx;
        internal int                    _rqChangeImpersonationRefCount;
        ImpersonationContext            _rqTimerThreadImpersonationIctx;
        bool                            _rqSupportSessionIdReissue;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.State.SessionStateModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public SessionStateModule() {
        }

        static bool CheckTrustLevel(SessionStateSection config) {
            switch (config.Mode) {
                case SessionStateMode.SQLServer:
                case SessionStateMode.StateServer:
                    return HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium);

                default:
                case SessionStateMode.Off:
                case SessionStateMode.InProc: // In-proc session doesn't require any trust level (part of ASURT 124513)
                    return true;
            }
        }

        [AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.Low)]
        private SessionStateStoreProviderBase SecureInstantiateProvider(ProviderSettings settings) {
            return (SessionStateStoreProviderBase)ProvidersHelper.InstantiateProvider(settings, typeof(SessionStateStoreProviderBase));
        }

        // Create an instance of the custom store as specified in the config file
        SessionStateStoreProviderBase InitCustomStore(SessionStateSection config) {
            string          providerName = config.CustomProvider;
            ProviderSettings  ps;

            if (String.IsNullOrEmpty(providerName)) {
                throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_session_custom_provider, providerName),
                        config.ElementInformation.Properties["customProvider"].Source, config.ElementInformation.Properties["customProvider"].LineNumber);
            }

            ps = config.Providers[providerName];
            if (ps == null) {
                throw new ConfigurationErrorsException(
                        SR.GetString(SR.Missing_session_custom_provider, providerName),
                        config.ElementInformation.Properties["customProvider"].Source, config.ElementInformation.Properties["customProvider"].LineNumber);
            }

            return SecureInstantiateProvider(ps);
        }

        IPartitionResolver InitPartitionResolver(SessionStateSection config) {
            string  partitionResolverType = config.PartitionResolverType;
            Type    resolverType;
            IPartitionResolver  iResolver;

            if (String.IsNullOrEmpty(partitionResolverType)) {
                return null;
            }

            if (config.Mode != SessionStateMode.StateServer &&
                config.Mode != SessionStateMode.SQLServer) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Cant_use_partition_resolve),
                        config.ElementInformation.Properties["partitionResolverType"].Source, config.ElementInformation.Properties["partitionResolverType"].LineNumber);
            }


            resolverType = ConfigUtil.GetType(partitionResolverType, "partitionResolverType", config);
            ConfigUtil.CheckAssignableType(typeof(IPartitionResolver), resolverType, config, "partitionResolverType");

            iResolver = (IPartitionResolver)HttpRuntime.CreatePublicInstance(resolverType);
            iResolver.Initialize();

            return iResolver;
        }

        ISessionIDManager InitSessionIDManager(SessionStateSection config) {
            string  sessionIDManagerType = config.SessionIDManagerType;
            ISessionIDManager  iManager;

            if (String.IsNullOrEmpty(sessionIDManagerType)) {
                iManager = new SessionIDManager();
                _usingAspnetSessionIdManager = true;
            }
            else {
                Type    managerType;

                managerType = ConfigUtil.GetType(sessionIDManagerType, "sessionIDManagerType", config);
                ConfigUtil.CheckAssignableType(typeof(ISessionIDManager), managerType, config, "sessionIDManagerType");

                iManager = (ISessionIDManager)HttpRuntime.CreatePublicInstance(managerType);
            }

            iManager.Initialize();

            return iManager;
        }

        void InitModuleFromConfig(HttpApplication app, SessionStateSection config) {
            if (config.Mode == SessionStateMode.Off) {
                return;
            }

            app.AddOnAcquireRequestStateAsync(
                    new BeginEventHandler(this.BeginAcquireState),
                    new EndEventHandler(this.EndAcquireState));

            app.ReleaseRequestState += new EventHandler(this.OnReleaseState);
            app.EndRequest += new EventHandler(this.OnEndRequest);

            _partitionResolver = InitPartitionResolver(config);

            switch (config.Mode) {
                case SessionStateMode.InProc:
                    if (HttpRuntime.UseIntegratedPipeline) {
                        s_canSkipEndRequestCall = true;
                    }
                    _store = new InProcSessionStateStore();
                    _store.Initialize(null, null);
                    break;

#if !FEATURE_PAL // FEATURE_PAL does not enable out of proc session state
                case SessionStateMode.StateServer:
                    if (HttpRuntime.UseIntegratedPipeline) {
                        s_canSkipEndRequestCall = true;
                    }
                    _store = new OutOfProcSessionStateStore();
                    ((OutOfProcSessionStateStore)_store).Initialize(null, null, _partitionResolver);
                    break;

                case SessionStateMode.SQLServer:
                    _store = new SqlSessionStateStore();
                    ((SqlSessionStateStore)_store).Initialize(null, null, _partitionResolver);
#if DBG
                    ((SqlSessionStateStore)_store).SetModule(this);
#endif
                    break;
#else // !FEATURE_PAL
                case SessionStateMode.StateServer:
                    throw new NotImplementedException("ROTORTODO");
                    break;

                case SessionStateMode.SQLServer:
                    throw new NotImplementedException("ROTORTODO");
                    break;
#endif // !FEATURE_PAL

                case SessionStateMode.Custom:
                    _store = InitCustomStore(config);
                    break;

                default:
                    break;
            }

            // We depend on SessionIDManager to manage session id
            _idManager = InitSessionIDManager(config);

            if ((config.Mode == SessionStateMode.InProc || config.Mode == SessionStateMode.StateServer) &&
                _usingAspnetSessionIdManager) {
                // If we're using InProc mode or StateServer mode, and also using our own session id module,
                // we know we don't care about impersonation in our all session state store read/write
                // and session id read/write.
                _ignoreImpersonation = true;
            }

        }

        public void Init(HttpApplication app) {
            bool initModuleCalled = false;
            SessionStateSection config = RuntimeConfig.GetAppConfig().SessionState;

            if (!s_oneTimeInit) {
                s_lock.AcquireWriterLock();
                try {
                    if (!s_oneTimeInit) {
                        InitModuleFromConfig(app, config);
                        initModuleCalled = true;

                        if (!CheckTrustLevel(config))
                            s_trustLevelInsufficient = true;

                        s_timeout = (int)config.Timeout.TotalMinutes;

                        s_useHostingIdentity = config.UseHostingIdentity;

                        // See if we can try InProc optimization. See inline doc of s_allowInProcOptimization
                        // for details.
                        if (config.Mode == SessionStateMode.InProc &&
                            _usingAspnetSessionIdManager) {
                            s_allowInProcOptimization = true;
                        }

                        if (config.Mode != SessionStateMode.Custom &&
                            config.Mode != SessionStateMode.Off &&
                            !config.RegenerateExpiredSessionId) {
                            s_allowDelayedStateStoreItemCreation = true;
                        }

                        s_configExecutionTimeout = RuntimeConfig.GetConfig().HttpRuntime.ExecutionTimeout;

                        s_configRegenerateExpiredSessionId = config.RegenerateExpiredSessionId;
                        s_configCookieless = config.Cookieless;
                        s_configMode = config.Mode;

                        // The last thing to set in this if-block.
                        s_oneTimeInit = true;

                        Debug.Trace("SessionStateModuleInit",
                                    "Configuration: _mode=" + config.Mode +
                                    ";Timeout=" + config.Timeout +
                                    ";CookieMode=" + config.Cookieless +
                                    ";SqlConnectionString=" + config.SqlConnectionString +
                                    ";StateConnectionString=" + config.StateConnectionString +
                                    ";s_allowInProcOptimization=" + s_allowInProcOptimization +
                                    ";s_allowDelayedStateStoreItemCreation=" + s_allowDelayedStateStoreItemCreation);

                    }
                }
                finally {
                    s_lock.ReleaseWriterLock();
                }
            }

            if (!initModuleCalled) {
                InitModuleFromConfig(app, config);
            }

            if (s_trustLevelInsufficient) {
                throw new HttpException(SR.GetString(SR.Session_state_need_higher_trust));
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Dispose() {
            if (_timer != null) {
                ((IDisposable)_timer).Dispose();
            }

            if (_store != null) {
                _store.Dispose();
            }
        }

        void ResetPerRequestFields() {
            Debug.Assert(_rqIctx == null, "_rqIctx == null");
            Debug.Assert(_rqChangeImpersonationRefCount == 0, "_rqChangeImpersonationRefCount == 0");

            _rqSessionState = null;
            _rqId = null;
            _rqSessionItems = null;
            _rqStaticObjects = null;
            _rqIsNewSession = false;
            _rqSessionStateNotFound = true;
            _rqReadonly = false;
            _rqItem = null;
            _rqContext = null;
            _rqAr = null;
            _rqLockId = null;
            _rqInCallback = 0;
            _rqLastPollCompleted = DateTime.MinValue;
            _rqExecutionTimeout = TimeSpan.Zero;
            _rqAddedCookie = false;
            _rqIdNew = false;
            _rqActionFlags = 0;
            _rqIctx = null;
            _rqChangeImpersonationRefCount = 0;
            _rqTimerThreadImpersonationIctx = null;
            _rqSupportSessionIdReissue = false;
        }

        /*
         * Add a OnStart event handler.
         *
         * @param sessionEventHandler
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public event EventHandler Start {
            add {
                _sessionStartEventHandler += value;
            }
            remove {
                _sessionStartEventHandler -= value;
            }
        }

        void RaiseOnStart(EventArgs e) {
            if (_sessionStartEventHandler == null)
                return;

            Debug.Trace("SessionStateModuleRaiseOnStart",
                "Session_Start called for session id:" + _rqId);

            // Session_OnStart for ASPCOMPAT pages has to be raised from an STA thread
            // 
            if (HttpRuntime.ApartmentThreading || _rqContext.InAspCompatMode) {
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                AspCompatApplicationStep.RaiseAspCompatEvent(
                    _rqContext,
                    _rqContext.ApplicationInstance,
                    null,
                    _sessionStartEventHandler,
                    this,
                    e);
#else // !FEATURE_PAL
                throw new NotImplementedException ("ROTORTODO");
#endif // !FEATURE_PAL

            }
            else {
                if (HttpContext.Current == null) {
                    // This can happen if it's called by a timer thread
                    DisposableHttpContextWrapper.SwitchContext(_rqContext);
                }

                _sessionStartEventHandler(this, e);
            }
        }

        /*
         * Fire the OnStart event.
         *
         * @param e
         */
        void OnStart(EventArgs e) {
            RaiseOnStart(e);
        }

        /*
         * Add a OnEnd event handler.
         *
         * @param sessionEventHandler
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public event EventHandler End {
            add {
                lock(_onEndTarget) {
                    if (_store != null && _onEndTarget.SessionEndEventHandlerCount == 0) {
                        _supportSessionExpiry = _store.SetItemExpireCallback(
                                new SessionStateItemExpireCallback(_onEndTarget.RaiseSessionOnEnd));
                    }
                    ++_onEndTarget.SessionEndEventHandlerCount;
                }
            }
            remove {
                lock(_onEndTarget) {
                    --_onEndTarget.SessionEndEventHandlerCount;
                    // 
                    if (_store != null && _onEndTarget.SessionEndEventHandlerCount == 0) {
                        _store.SetItemExpireCallback(null);
                        _supportSessionExpiry = false;
                    }
                }
            }
        }

        /*
         * Acquire session state
         */
        IAsyncResult BeginAcquireState(Object source, EventArgs e, AsyncCallback cb, Object extraData) {
            bool                requiresState;
            bool                isCompleted = true;
            bool                skipReadingId = false;

            Debug.Trace("SessionStateModuleOnAcquireState", "Beginning SessionStateModule::OnAcquireState");

            _acquireCalled = true;
            _releaseCalled = false;
            ResetPerRequestFields();

            _rqContext = ((HttpApplication)source).Context;
            _rqAr = new HttpAsyncResult(cb, extraData);

            ChangeImpersonation(_rqContext, false);

            try {
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_BEGIN, _rqContext.WorkerRequest);

                /* Notify the store we are beginning to get process request */
                _store.InitializeRequest(_rqContext);

                /* determine if the request requires state at all */
                requiresState = _rqContext.RequiresSessionState;

                // SessionIDManager may need to do a redirect if cookieless setting is AutoDetect
                if (_idManager.InitializeRequest(_rqContext, false, out _rqSupportSessionIdReissue)) {
                    _rqAr.Complete(true, null, null);
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, _rqContext.WorkerRequest);
                    return _rqAr;
                }

                // See if we can skip reading the session id.  See inline doc of s_allowInProcOptimization
                // for details.
                if (s_allowInProcOptimization &&
                    !s_sessionEverSet &&
                     (!requiresState ||             // Case 1
                      !((SessionIDManager)_idManager).UseCookieless(_rqContext)) ) {  // Case 2

                    skipReadingId = true;

#if DBG
                    if (!requiresState) {
                        // Case 1
                        Debug.Trace("SessionStateModuleOnAcquireState", "Skip reading id because page has disabled session state");
                    }
                    else {
                        // Case 2
                        Debug.Trace("SessionStateModuleOnAcquireState", "Delay reading id because we're using InProc optimization, and we are not using cookieless");
                    }
#endif
                }
                else {
                    /* Get sessionid */
                    _rqId = _idManager.GetSessionID(_rqContext);
                    Debug.Trace("SessionStateModuleOnAcquireState", "Current request id=" + _rqId);
                }

                if (!requiresState) {
                    if (_rqId == null) {
                        Debug.Trace("SessionStateModuleOnAcquireState",
                                    "Handler does not require state, " +
                                    "session id skipped or no id found, " +
                                    "skipReadingId=" + skipReadingId +
                                    "\nReturning from SessionStateModule::OnAcquireState");
                    }
                    else {
                        Debug.Trace("SessionStateModuleOnAcquireState",
                                    "Handler does not require state, " +
                                    "resetting timeout for SessionId=" + _rqId +
                                    "\nReturning from SessionStateModule::OnAcquireState");

                        // Still need to update the sliding timeout to keep session alive.
                        // There is a plan to skip this for perf reason.  But it was postponed to
                        // after Whidbey.
                        _store.ResetItemTimeout(_rqContext, _rqId);
                    }

                    _rqAr.Complete(true, null, null);

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, _rqContext.WorkerRequest);
                    return _rqAr;
                }

                _rqExecutionTimeout = _rqContext.Timeout;

                // If the page is marked as DEBUG, HttpContext.Timeout will return a very large value (~1 year)
                // In this case, we want to use the executionTimeout value specified in the config to avoid
                // PollLockedSession to run forever.
                if (_rqExecutionTimeout == DEFAULT_DBG_EXECUTION_TIMEOUT) {
                    _rqExecutionTimeout = s_configExecutionTimeout;
                }

                /* determine if we need just read-only access */
                _rqReadonly = _rqContext.ReadOnlySessionState;

                if (_rqId != null) {
                    /* get the session state corresponding to this session id */
                    isCompleted = GetSessionStateItem();
                }
                else if (!skipReadingId) {
                    /* if there's no id yet, create it */
                    bool    redirected = CreateSessionId();

                    _rqIdNew = true;

                    if (redirected) {
                        if (s_configRegenerateExpiredSessionId) {
                            // See inline comments in CreateUninitializedSessionState()
                            CreateUninitializedSessionState();
                        }

                        _rqAr.Complete(true, null, null);

                        if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, _rqContext.WorkerRequest);
                        return _rqAr;
                    }
                }

                if (isCompleted) {
                    CompleteAcquireState();
                    _rqAr.Complete(true, null, null);
                }

                return _rqAr;
            }
            finally {
                RestoreImpersonation();
            }
        }

        internal bool CreateSessionId() {
            // CreateSessionId should be called only if:
            Debug.Assert(_rqId == null ||               // Session id isn't found in the request, OR

                (_rqSessionStateNotFound &&             // The session state isn't found, AND
                 s_configRegenerateExpiredSessionId &&  // We are regenerating expired session id, AND
                 _rqSupportSessionIdReissue &&          // This request supports session id re-issue, AND
                 !_rqIdNew),                            // The above three condition should imply the session id
                                                        // isn't just created, but is sent by the request.
                "CreateSessionId should be called only if we're generating new id, or re-generating expired one");
            Debug.Assert(_rqChangeImpersonationRefCount > 0, "Must call ChangeImpersonation first");

            bool redirected;
            _rqId = _idManager.CreateSessionID(_rqContext);
            _idManager.SaveSessionID(_rqContext, _rqId, out redirected, out _rqAddedCookie);

            return redirected;
        }

        internal void EnsureStateStoreItemLocked() {
            // DevDiv 665141: 
            // Ensure ownership of the session state item here as the session ID now can be put on the wire (by Response.Flush)
            // and the client can initiate a request before this one reaches OnReleaseState and thus causing a race condition.
            // Note: It changes when we call into the Session Store provider. Now it may happen at BeginAcquireState instead of OnReleaseState.

            // Item is locked yet here only if this is a new session
            if (!_rqSessionStateNotFound) {
                return;
            }

            Debug.Assert(_rqId != null, "Session State ID must exist");
            Debug.Assert(_rqItem != null, "Session State item must exist");

            ChangeImpersonation(_rqContext, false);

            try {
                // Store the item if already have been created
                _store.SetAndReleaseItemExclusive(_rqContext, _rqId, _rqItem, _rqLockId, true /*_rqSessionStateNotFound*/);

                // Lock Session State Item in Session State Store
                LockSessionStateItem();
            }
            catch {
                throw;
            }
            finally {
                RestoreImpersonation();
            }

            // Mark as old session here. The SessionState is fully initialized, the item is locked
            _rqSessionStateNotFound = false;
            s_sessionEverSet = true;
        }
 
        // Called when AcquireState is done.  This function will add the returned
        // SessionStateStore item to the request context.
        void CompleteAcquireState() {
            Debug.Trace("SessionStateModuleOnAcquireState", "Item retrieved=" + (_rqItem != null).ToString(CultureInfo.InvariantCulture));
            bool delayInitStateStoreItem = false;

            Debug.Assert(!(s_allowDelayedStateStoreItemCreation && s_configRegenerateExpiredSessionId),
                "!(s_allowDelayedStateStoreItemCreation && s_configRegenerateExpiredSessionId)");

            try {
                if (_rqItem != null) {
                    _rqSessionStateNotFound = false;

                    if ((_rqActionFlags & SessionStateActions.InitializeItem) != 0) {
                        Debug.Trace("SessionStateModuleOnAcquireState", "Initialize an uninit item");
                        _rqIsNewSession = true;
                    }
                    else {
                        _rqIsNewSession = false;
                    }
                }
                else {
                    _rqIsNewSession = true;
                    _rqSessionStateNotFound = true;

                    if (s_allowDelayedStateStoreItemCreation) {
                        Debug.Trace("SessionStateModuleOnAcquireState", "Delay creating new session state");
                        delayInitStateStoreItem = true;
                    }

                    // We couldn't find the session state.
                    if (!_rqIdNew &&                            // If the request has a session id, that means the session state has expired
                        s_configRegenerateExpiredSessionId &&   // And we're asked to regenerate expired session
                        _rqSupportSessionIdReissue) {           // And this request support session id reissue

                        // We will generate a new session id for this expired session state
                        bool redirected = CreateSessionId();

                        Debug.Trace("SessionStateModuleOnAcquireState", "Complete re-creating new id; redirected=" + redirected);

                        if (redirected) {
                            Debug.Trace("SessionStateModuleOnAcquireState", "Will redirect because we've reissued a new id and it's cookieless");
                            CreateUninitializedSessionState();
                            return;
                        }
                    }
                }

                if (delayInitStateStoreItem) {
                    _rqSessionState = s_delayedSessionState;
                }
                else {
                    InitStateStoreItem(true);
                }

                // Set session state module
                SessionStateUtility.AddHttpSessionStateModuleToContext(_rqContext, this, delayInitStateStoreItem);

                if (_rqIsNewSession) {
                    Debug.Trace("SessionStateModuleOnAcquireState", "Calling OnStart");
                    OnStart(EventArgs.Empty);
                }
            }
            finally {
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, _rqContext.WorkerRequest);
            }

#if DBG
            if (_rqIsNewSession) {
                if (_rqId == null) {
                    Debug.Assert(s_allowInProcOptimization, "s_allowInProcOptimization");
                    Debug.Trace("SessionStateModuleOnAcquireState", "New session: session id reading is delayed"+
                                "\nReturning from SessionStateModule::OnAcquireState");
                }
                else {
                    Debug.Trace("SessionStateModuleOnAcquireState", "New session: SessionId= " + _rqId +
                                "\nReturning from SessionStateModule::OnAcquireState");
                }

            }
            else {
                Debug.Trace("SessionStateModuleOnAcquireState", "Retrieved old session, SessionId= " + _rqId +
                            "\nReturning from SessionStateModule::OnAcquireState");

            }
#endif
        }

        void CreateUninitializedSessionState() {
            Debug.Assert(_rqChangeImpersonationRefCount > 0, "Must call ChangeImpersonation first");

            // When we generate a new session id in cookieless case, and if "reissueExpiredSession" is
            // true, we need to generate a new temporary empty session and save it
            // under the new session id, otherwise when the next request (i.e. when the browser is
            // redirected back to the web server) comes in, we will think it's accessing an expired session.
            _store.CreateUninitializedItem(_rqContext, _rqId, s_timeout);
        }

        internal void InitStateStoreItem(bool addToContext) {
            Debug.Assert(_rqId != null || s_allowInProcOptimization, "_rqId != null || s_allowInProcOptimization");

            ChangeImpersonation(_rqContext, false);
            try {

                if (_rqItem == null) {
                    Debug.Trace("InitStateStoreItem", "Creating new session state");
                    _rqItem = _store.CreateNewStoreData(_rqContext, s_timeout);
                }

                _rqSessionItems = _rqItem.Items;
                if (_rqSessionItems == null) {
                    throw new HttpException(SR.GetString(SR.Null_value_for_SessionStateItemCollection));
                }

                // No check for null because we allow our custom provider to return a null StaticObjects.
                _rqStaticObjects = _rqItem.StaticObjects;

                _rqSessionItems.Dirty = false;

                _rqSessionState = new HttpSessionStateContainer(
                        this,
                        _rqId,            // could be null if we're using InProc optimization
                        _rqSessionItems,
                        _rqStaticObjects,
                        _rqItem.Timeout,
                        _rqIsNewSession,
                        s_configCookieless,
                        s_configMode,
                        _rqReadonly);

                if (addToContext) {
                    SessionStateUtility.AddHttpSessionStateToContext(_rqContext, _rqSessionState);
                }
            }
            finally {
                RestoreImpersonation();
            }
        }

        // Used for InProc session id optimization
        internal string DelayedGetSessionId() {
            Debug.Assert(s_allowInProcOptimization, "Shouldn't be called if we don't allow InProc optimization");
            Debug.Assert(_rqId == null, "Shouldn't be called if we already have the id");
            Debug.Assert(!((SessionIDManager)_idManager).UseCookieless(_rqContext), "We can delay session id only if we are not using cookieless");

            Debug.Trace("DelayedOperation", "Delayed getting session id");

            bool    redirected;

            ChangeImpersonation(_rqContext, false);
            try {
                _rqId = _idManager.GetSessionID(_rqContext);

                if (_rqId == null) {
                    Debug.Trace("DelayedOperation", "Delayed creating session id");

                    redirected = CreateSessionId();
                    Debug.Assert(!redirected, "DelayedGetSessionId shouldn't redirect us here.");
                }
            }
            finally {
                RestoreImpersonation();
            }

            return _rqId;
        }

        void LockSessionStateItem() {
            bool locked;
            TimeSpan lockAge;

            Debug.Assert(_rqId != null, "_rqId != null");
            Debug.Assert(_rqChangeImpersonationRefCount > 0, "Must call ChangeImpersonation first");

            if (!_rqReadonly) {
                SessionStateStoreData storedItem = _store.GetItemExclusive(_rqContext, _rqId, out locked, out lockAge, out _rqLockId, out _rqActionFlags);
                Debug.Assert(storedItem != null, "Must succeed in locking session state item.");
            }
        }

        bool GetSessionStateItem() {
            bool            isCompleted = true;
            bool            locked;
            TimeSpan        lockAge;

            Debug.Assert(_rqId != null, "_rqId != null");
            Debug.Assert(_rqChangeImpersonationRefCount > 0, "Must call ChangeImpersonation first");

            if (_rqReadonly) {
                _rqItem = _store.GetItem(_rqContext, _rqId, out locked, out lockAge, out _rqLockId, out _rqActionFlags);
            }
            else {
                _rqItem = _store.GetItemExclusive(_rqContext, _rqId, out locked, out lockAge, out _rqLockId, out _rqActionFlags);
                
                // DevDiv Bugs 146875: WebForm and WebService Session Access Concurrency Issue
                // If we have an expired session, we need to insert the state in the store here to
                // ensure serialized access in case more than one entity requests it simultaneously.
                // If the state has already been created before, CreateUninitializedSessionState is a no-op.
                if (_rqItem == null && locked == false && _rqId != null) {
                    if (!(s_configCookieless == HttpCookieMode.UseUri && s_configRegenerateExpiredSessionId == true)) {
                        CreateUninitializedSessionState();
                        _rqItem = _store.GetItemExclusive(_rqContext, _rqId, out locked, out lockAge, out _rqLockId, out _rqActionFlags);
                    }
                }
            }

            // We didn't get it because it's locked....
            if (_rqItem == null && locked) {
                // 
                if (lockAge >= _rqExecutionTimeout) {
                    /* Release the lock on the item, which is held by another thread*/
                    Debug.Trace("SessionStateModuleOnAcquireState",
                                "Lock timed out, lockAge=" + lockAge +
                                ", id=" + _rqId);

                    _store.ReleaseItemExclusive(_rqContext, _rqId, _rqLockId);
                }

                Debug.Trace("SessionStateModuleOnAcquireState",
                            "Item is locked, will poll, id=" + _rqId);

                isCompleted = false;
                PollLockedSession();
            }

            return isCompleted;
        }

        void PollLockedSession() {
            if (_timerCallback == null) {
                _timerCallback = new TimerCallback(this.PollLockedSessionCallback);
            }

            if (_timer == null) {
                _timerId++;

#if DBG
                if (!Debug.IsTagPresent("Timer") || Debug.IsTagEnabled("Timer"))
#endif
                {
                    if (!s_PollIntervalRegLookedUp)
                        LookUpRegForPollInterval();
                    _timer = new Timer(_timerCallback, _timerId, LOCKED_ITEM_POLLING_INTERVAL, LOCKED_ITEM_POLLING_INTERVAL);
                }
            }
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
        private static void LookUpRegForPollInterval() {
            lock (s_PollIntervalRegLock) {
                if (s_PollIntervalRegLookedUp)
                    return;
                try {
                    object o = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET", "SessionStateLockedItemPollInterval", 0);
                    if (o != null && (o is int || o is uint) && ((int)o) > 0)
                        LOCKED_ITEM_POLLING_INTERVAL = (int) o;
                    s_PollIntervalRegLookedUp = true;
                }
                catch { // ignore exceptions
                }
            }
        }


        void ResetPollTimer() {
            _timerId++;
            if (_timer != null) {
                ((IDisposable)_timer).Dispose();
                _timer = null;
            }
        }

        void ChangeImpersonation(HttpContext context, bool timerThread) {
#if !FEATURE_PAL // FEATURE_PAL doesn't enable impersonation
            _rqChangeImpersonationRefCount++;

            if (_ignoreImpersonation) {
                return;
            }

            // If SQL store isn't using integrated security, and we're using our own session id module,
            // we know we don't care about impersonation in our all session state store read/write
            // and session id read/write.
            if (s_configMode == SessionStateMode.SQLServer &&
                ((SqlSessionStateStore)_store).KnowForSureNotUsingIntegratedSecurity &&
                _usingAspnetSessionIdManager) {
                return;
            }

            // Please note that there are two types of calls coming in.  One is from a request thread,
            // where timerThread==false; the other is from PollLockedSessionCallback, where
            // timerThread==true.

            if (s_useHostingIdentity) {
                // If we're told to use Application Identity, in each case we should impersonate,
                // if not called yet.
                if (_rqIctx == null) {
                    _rqIctx = new ApplicationImpersonationContext();
                }
            }
            else {
                if (timerThread) {
                    // For the timer thread, we should explicity impersonate back to what the HttpContext was
                    // orginally impersonating.
                    _rqTimerThreadImpersonationIctx = new ClientImpersonationContext(context, false);
                }
                else {
                    // For a request thread, if we're told to not use hosting id, there's no need
                    // to do anything special.
                    Debug.Assert(_rqIctx == null, "_rqIctx == null");
                    return;
                }
            }
#endif // !FEATURE_PAL
        }

        void RestoreImpersonation() {
            Debug.Assert(_rqChangeImpersonationRefCount != 0, "_rqChangeImpersonationRefCount != 0");

            _rqChangeImpersonationRefCount--;

            if (_rqChangeImpersonationRefCount == 0) {
                Debug.Assert(!(_rqIctx != null && _rqTimerThreadImpersonationIctx != null), "Should not have mixed mode of impersonation");

                if (_rqIctx != null) {
                    _rqIctx.Undo();
                    _rqIctx = null;
                }

                if (_rqTimerThreadImpersonationIctx != null) {
                    Debug.Assert(_rqContext != null, "_rqContext != null");
                    _rqTimerThreadImpersonationIctx.Undo();
                    _rqTimerThreadImpersonationIctx = null;
                }
            }
        }

        void PollLockedSessionCallback(object state) {
            Debug.Assert(_rqId != null, "_rqId != null");
            Debug.Trace("SessionStateModuleOnAcquireState",
                        "Polling callback called from timer, id=" + _rqId);

            bool isCompleted = false;
            Exception error = null;

            /* check whether we are currently in a callback */
            if (Interlocked.CompareExchange(ref _rqInCallback, 1, 0) != 0)
                return;

            try {
                /*
                 * check whether this callback is for the current request,
                 * and whether sufficient time has passed since the last poll
                 * to try again.
                 */
                int timerId = (int) state;
                if (    (timerId == _timerId) &&
                        (DateTime.UtcNow - _rqLastPollCompleted >= LOCKED_ITEM_POLLING_DELTA)) {

                    ChangeImpersonation(_rqContext, true);

                    try {
                        isCompleted = GetSessionStateItem();
                        _rqLastPollCompleted = DateTime.UtcNow;
                        if (isCompleted) {
                            Debug.Assert(_timer != null, "_timer != null");
                            ResetPollTimer();
                            CompleteAcquireState();
                        }
                    }
                    finally {
                        RestoreImpersonation();
                    }
                }
            }
            catch (Exception e) {
                ResetPollTimer();
                error = e;
            }
            finally {
                Interlocked.Exchange(ref _rqInCallback, 0);
            }

            if (isCompleted || error != null) {
                _rqAr.Complete(false, null, error);
            }
        }


        void EndAcquireState(IAsyncResult ar) {
            ((HttpAsyncResult)ar).End();
        }

        // Called by OnReleaseState to get the session id.
        string ReleaseStateGetSessionID() {
            if (_rqId == null) {
                Debug.Assert(s_allowInProcOptimization, "s_allowInProcOptimization");
                DelayedGetSessionId();
            }

            Debug.Assert(_rqId != null, "_rqId != null");
            return _rqId;
        }

        /*
         * Release session state
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void OnReleaseState(Object source, EventArgs eventArgs) {
            HttpApplication             app;
            HttpContext                 context;
            bool                        setItemCalled = false;

            Debug.Trace("SessionStateOnReleaseState", "Beginning SessionStateModule::OnReleaseState");

            Debug.Assert(!(_rqAddedCookie && !_rqIsNewSession),
                "If session id was added to the cookie, it must be a new session.");

            // !!!
            // Please note that due to InProc session id optimization, this function should not
            // use _rqId directly because it can still be null.  Instead, use DelayedGetSessionId().

            _releaseCalled = true;

            app = (HttpApplication)source;
            context = app.Context;

            ChangeImpersonation(context, false);

            try {
                if (_rqSessionState != null) {
                    bool delayedSessionState = (_rqSessionState == s_delayedSessionState);

                    Debug.Trace("SessionStateOnReleaseState", "Remove session state from context");
                    SessionStateUtility.RemoveHttpSessionStateFromContext(_rqContext, delayedSessionState);

                    /*
                     * Don't store untouched new sessions.
                     */

                    if (
                                // The store doesn't have the session state.
                                // ( Please note we aren't checking _rqIsNewSession because _rqIsNewSession
                                // is lalso true if the item is converted from temp to perm in a GetItemXXX() call.)
                                _rqSessionStateNotFound

                                // OnStart is not defined
                               && _sessionStartEventHandler == null

                               // Nothing has been stored in session state
                               && (delayedSessionState || !_rqSessionItems.Dirty)
                               && (delayedSessionState || _rqStaticObjects == null || _rqStaticObjects.NeverAccessed)
                        ) {

                        Debug.Trace("SessionStateOnReleaseState", "Not storing unused new session.");
                    }
                    else if (_rqSessionState.IsAbandoned) {
                        Debug.Trace("SessionStateOnReleaseState", "Removing session due to abandonment, SessionId=" + _rqId);

                        if (_rqSessionStateNotFound) {
                            // The store provider doesn't have it, and so we don't need to remove it from the store.

                            // However, if the store provider supports session expiry, and we have a Session_End in global.asax,
                            // we need to explicitly call Session_End.
                            if (_supportSessionExpiry) {
                                if (delayedSessionState) {
                                    Debug.Assert(s_allowDelayedStateStoreItemCreation, "s_allowDelayedStateStoreItemCreation");
                                    Debug.Assert(_rqItem == null, "_rqItem == null");

                                    InitStateStoreItem(false /*addToContext*/);
                                }

                                _onEndTarget.RaiseSessionOnEnd(ReleaseStateGetSessionID(), _rqItem);
                            }
                        }
                        else {
                            Debug.Assert(_rqItem != null, "_rqItem cannot null if it's not a new session");

                            // Remove it from the store because the session is abandoned.
                            _store.RemoveItem(_rqContext, ReleaseStateGetSessionID(), _rqLockId, _rqItem);
                        }
                    }
                    else if (!_rqReadonly ||
                             (_rqReadonly &&
                              _rqIsNewSession &&
                              _sessionStartEventHandler != null &&
                              !SessionIDManagerUseCookieless)) {
                        // We need to save it since it isn't read-only
                        // See Dev10 588711: Issuing a redirect from inside of Session_Start event 
                        // triggers an infinite loop when using pages with read-only session state

                        // We save it only if there is no error, and if something has changed (unless it's a new session)
                        if (    context.Error == null   // no error
                                && (    _rqSessionStateNotFound
                                    || _rqSessionItems.Dirty    // SessionItems has changed.
                                    || (_rqStaticObjects != null && !_rqStaticObjects.NeverAccessed) // Static objects have been accessed
                                    || _rqItem.Timeout != _rqSessionState.Timeout   // Timeout value has changed
                                    )
                            ) {

                            if (delayedSessionState) {
                                Debug.Assert(_rqIsNewSession, "Saving a session and delayedSessionState is true: _rqIsNewSession must be true");
                                Debug.Assert(s_allowDelayedStateStoreItemCreation, "Saving a session and delayedSessionState is true: s_allowDelayedStateStoreItemCreation");
                                Debug.Assert(_rqItem == null, "Saving a session and delayedSessionState is true: _rqItem == null");

                                InitStateStoreItem(false /*addToContext*/);
                            }

#if DBG
                            if (_rqSessionItems.Dirty) {
                                Debug.Trace("SessionStateOnReleaseState", "Setting new session due to dirty SessionItems, SessionId=" + _rqId);
                            }
                            else if (_rqStaticObjects != null && !_rqStaticObjects.NeverAccessed) {
                                Debug.Trace("SessionStateOnReleaseState", "Setting new session due to accessed Static Objects, SessionId=" + _rqId);
                            }
                            else if (_rqSessionStateNotFound) {
                                Debug.Trace("SessionStateOnReleaseState", "Setting new session because it's not found, SessionId=" + _rqId);
                            }
                            else {
                                Debug.Trace("SessionStateOnReleaseState", "Setting new session due to options change, SessionId=" + _rqId +
                                            "\n\t_rq.timeout=" + _rqItem.Timeout.ToString(CultureInfo.InvariantCulture) +
                                            ", _rqSessionState.timeout=" + _rqSessionState.Timeout.ToString(CultureInfo.InvariantCulture));
                            }
#endif
                            if (_rqItem.Timeout != _rqSessionState.Timeout) {
                                _rqItem.Timeout = _rqSessionState.Timeout;
                            }

                            s_sessionEverSet = true;
                            setItemCalled = true;
                            _store.SetAndReleaseItemExclusive(_rqContext, ReleaseStateGetSessionID(), _rqItem, _rqLockId, _rqSessionStateNotFound);
                        }
                        else {
                            // Can't save it because of various reason.  Just release our exclusive lock on it.
                            Debug.Trace("SessionStateOnReleaseState", "Release exclusive lock on session, SessionId=" + _rqId);

                            if (!_rqSessionStateNotFound) {
                                Debug.Assert(_rqItem != null, "_rqItem cannot null if it's not a new session");
                                _store.ReleaseItemExclusive(_rqContext, ReleaseStateGetSessionID(), _rqLockId);
                            }
                        }
                    }
#if DBG
                    else {
                        Debug.Trace("SessionStateOnReleaseState", "Session is read-only, ignoring SessionId=" + _rqId);
                    }
#endif

                    Debug.Trace("SessionStateOnReleaseState", "Returning from SessionStateModule::OnReleaseState");
                }

                if (_rqAddedCookie && !setItemCalled && context.Response.IsBuffered()) {
                    _idManager.RemoveSessionID(_rqContext);
                }

            }
            finally {
                RestoreImpersonation();
            }

            // WOS 1679798: PERF: Session State Module should disable EndRequest on successful cleanup
            bool implementsIRequiresSessionState = context.RequiresSessionState;
            if (HttpRuntime.UseIntegratedPipeline 
                && (context.NotificationContext.CurrentNotification == RequestNotification.ReleaseRequestState) 
                && (s_canSkipEndRequestCall || !implementsIRequiresSessionState)) {
                context.DisableNotifications(RequestNotification.EndRequest, 0 /*postNotifications*/);
                _acquireCalled = false;
                _releaseCalled = false;
                ResetPerRequestFields();
            }
        }

        /*
         * End of request processing. Possibly does release if skipped due to errors
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void OnEndRequest(Object source, EventArgs eventArgs) {
            HttpApplication app;
            HttpContext context;
            String id;

            Debug.Trace("SessionStateOnEndRequest", "Beginning SessionStateModule::OnEndRequest");

            app = (HttpApplication)source;
            context = app.Context;

            /* determine if the request requires state at all */
            if (!context.RequiresSessionState) {
                return;
            }

            ChangeImpersonation(context, false);

            try {
                if (!_releaseCalled) {
                    if (_acquireCalled) {
                        /*
                         * need to do release here if the request short-circuited due to an error
                         */
                        OnReleaseState(source, eventArgs);
                    }
                    else {
                        /*
                         * 'advise' -- update session timeout
                         */

                        if (_rqContext == null) {
                            _rqContext = context;
                        }

                        // We haven't called BeginAcquireState.  So we have to call these InitializeRequest
                        // methods here.
                        bool    dummy;
                        _store.InitializeRequest(_rqContext);
                        _idManager.InitializeRequest(_rqContext, true, out dummy);

                        id = _idManager.GetSessionID(context);
                        if (id != null) {
                            Debug.Trace("SessionStateOnEndRequest", "Resetting timeout for SessionId=" + id);
                            _store.ResetItemTimeout(context, id);
                        }
#if DBG
                        else {
                            Debug.Trace("SessionStateOnEndRequest", "No session id found.");
                        }
#endif
                    }
                }

                /* Notify the store we are finishing a request */
                _store.EndRequest(_rqContext);
            }
            finally {
                _acquireCalled = false;
                _releaseCalled = false;
                RestoreImpersonation();
                ResetPerRequestFields();
            }

            Debug.Trace("SessionStateOnEndRequest", "Returning from SessionStateModule::OnEndRequest");
        }

        internal static void ReadConnectionString(SessionStateSection config, ref string cntString, string propName) {
            ConfigsHelper.GetRegistryStringAttribute(ref cntString, config, propName);
            HandlerBase.CheckAndReadConnectionString(ref cntString, true);
        }

        internal bool SessionIDManagerUseCookieless {
            get {
                // See VSWhidbey 399907
                if (!_usingAspnetSessionIdManager) {
                    return s_configCookieless == HttpCookieMode.UseUri;
                }
                else {
                    return ((SessionIDManager)_idManager).UseCookieless(_rqContext);
                }
            }
        }
        
        public void ReleaseSessionState(HttpContext context) {
            if (HttpRuntime.UseIntegratedPipeline && _acquireCalled && !_releaseCalled) {
                try {
                    OnReleaseState(context.ApplicationInstance, null);
                }
                catch { }
            }
        }

        public Task ReleaseSessionStateAsync(HttpContext context) {
            ReleaseSessionState(context);
            return TaskAsyncHelper.CompletedTask;
        }
    }
}
