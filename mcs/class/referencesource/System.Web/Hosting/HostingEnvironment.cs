//------------------------------------------------------------------------------
// <copyright file="HostingEnvironment.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Caching;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Util;
    using System.Web.WebSockets;
    using Microsoft.Win32;
    using System.Collections.Generic;

    [Flags]
    internal enum HostingEnvironmentFlags {
        Default = 0,
        HideFromAppManager = 1,
        ThrowHostingInitErrors = 2,
        DontCallAppInitialize = 4,
        ClientBuildManager = 8,
        SupportsMultiTargeting = 16,
    }

    [Serializable]
    internal class HostingEnvironmentParameters {
        private HostingEnvironmentFlags _hostingFlags;
        private ClientBuildManagerParameter _clientBuildManagerParameter;
        private string _precompTargetPhysicalDir;
        private string _iisExpressVersion;

        public HostingEnvironmentFlags HostingFlags {
            get { return _hostingFlags; }
            set { _hostingFlags = value; }
        }

        // Directory where the precompiled site is placed
        public string PrecompilationTargetPhysicalDirectory {
            get { return _precompTargetPhysicalDir; }
            set {
                _precompTargetPhysicalDir = FileUtil.FixUpPhysicalDirectory(value);
            }
        }

        // Determines the behavior of the precompilation
        public ClientBuildManagerParameter ClientBuildManagerParameter {
            get { return _clientBuildManagerParameter; }
            set { _clientBuildManagerParameter = value; }
        }

        // Determines which config system to load
        public string IISExpressVersion {
            get { return _iisExpressVersion; }
            set { _iisExpressVersion = value; }
        }

        // Determines what FileChangeMonitor mode to use
        public FcnMode FcnMode {
            get;
            set;
        }

        // Should FileChangesMonitor skip reading and caching DACLs?
        public bool FcnSkipReadAndCacheDacls {
            get;
            set;
        }
    }

    public sealed class HostingEnvironment : MarshalByRefObject {

        private static HostingEnvironment _theHostingEnvironment;
        private EventHandler _onAppDomainUnload;
        private ApplicationManager _appManager;
        private HostingEnvironmentParameters _hostingParameters;
        private IApplicationHost _appHost;
        private bool _externalAppHost;
        private IConfigMapPath _configMapPath;
        private IConfigMapPath2 _configMapPath2;
        private IntPtr _configToken;

        private IdentitySection _appIdentity;
        private IntPtr _appIdentityToken;
        private bool _appIdentityTokenSet;

        private String _appId;
        private VirtualPath _appVirtualPath;
        private String _appPhysicalPath;
        private String _siteName;
        private String _siteID;
        private String _appConfigPath;

        private bool   _isBusy;
        private int    _busyCount;

        private volatile static bool _stopListeningWasCalled; // static since it's process-wide
        private bool _removedFromAppManager;
        private bool _appDomainShutdownStarted;
        private bool _shutdownInitiated;
        private bool _shutdownInProgress;
        private String _shutDownStack;

        private int _inTrimCache;
        private ObjectCacheHost _objectCacheHost;

        // table of well know objects keyed by type
        private Hashtable _wellKnownObjects = new Hashtable();

        // list of registered IRegisteredObject instances, suspend listeners, and background work items
        private Hashtable _registeredObjects = new Hashtable();
        private SuspendManager _suspendManager = new SuspendManager();
        private BackgroundWorkScheduler _backgroundWorkScheduler = null; // created on demand
        private static readonly Task<object> _completedTask = Task.FromResult<object>(null);

        // callback to make InitiateShutdown non-blocking
        private WaitCallback _initiateShutdownWorkItemCallback;

        // inside app domain idle shutdown logic
        private IdleTimeoutMonitor _idleTimeoutMonitor;

        private static IProcessHostSupportFunctions _functions;
        private static bool _hasBeenRemovedFromAppManangerTable;

        private const string TemporaryVirtualPathProviderKey = "__TemporaryVirtualPathProvider__";

        // Determines what FileChangeMonitor mode to use
        internal static FcnMode FcnMode {
            get {
                if (_theHostingEnvironment != null && _theHostingEnvironment._hostingParameters != null) {
                    return _theHostingEnvironment._hostingParameters.FcnMode;
                }
                return FcnMode.NotSet;
            }
        }

        internal static bool FcnSkipReadAndCacheDacls {
            get {
                if (_theHostingEnvironment != null && _theHostingEnvironment._hostingParameters != null) {
                    return _theHostingEnvironment._hostingParameters.FcnSkipReadAndCacheDacls;
                }
                return false;
            }
        }

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        /// <internalonly/>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public HostingEnvironment() {
            if (_theHostingEnvironment != null)
                throw new InvalidOperationException(SR.GetString(SR.Only_1_HostEnv));

            // remember singleton HostingEnvironment in a static
            _theHostingEnvironment = this;

            // start watching for app domain unloading
            _onAppDomainUnload = new EventHandler(OnAppDomainUnload);
            Thread.GetDomain().DomainUnload += _onAppDomainUnload;
        }

        internal long TrimCache(int percent) {
            if (Interlocked.Exchange(ref _inTrimCache, 1) != 0)
                return 0;
            try {
                long trimmedOrExpired = 0;
                // do nothing if we're shutting down
                if (!_shutdownInitiated) {
                    trimmedOrExpired = HttpRuntime.CacheInternal.TrimCache(percent);
                    if (_objectCacheHost != null && !_shutdownInitiated) {
                        trimmedOrExpired += _objectCacheHost.TrimCache(percent);
                    }
                }
                return trimmedOrExpired;
            }
            finally {
                Interlocked.Exchange(ref _inTrimCache, 0);
            }
        }

        private void OnAppDomainUnload(Object unusedObject, EventArgs unusedEventArgs) {
            Debug.Trace("PipelineRuntime", "HE.OnAppDomainUnload");

            Thread.GetDomain().DomainUnload -= _onAppDomainUnload;

            // check for unexpected shutdown
            if (!_removedFromAppManager) {
                RemoveThisAppDomainFromAppManagerTableOnce();
            }

            HttpRuntime.RecoverFromUnexceptedAppDomainUnload();

            // call Stop on all registered objects with immediate = true
            StopRegisteredObjects(true);

            // notify app manager
            if (_appManager != null) {
                // disconnect the real app host and substitute it with a bogus one
                // to avoid exceptions later when app host is called (it normally wouldn't)
                IApplicationHost originalAppHost = null;

                if (_externalAppHost) {
                    originalAppHost = _appHost;
                    _appHost = new SimpleApplicationHost(_appVirtualPath, _appPhysicalPath);
                    _externalAppHost = false;
                }

                IDisposable configSystem = _configMapPath2 as IDisposable;
                if (configSystem != null) {
                    configSystem.Dispose();
                }

                _appManager.HostingEnvironmentShutdownComplete(_appId, originalAppHost);
            }

            // free the config access token
            if (_configToken != IntPtr.Zero) {
                UnsafeNativeMethods.CloseHandle(_configToken);
                _configToken = IntPtr.Zero;
            }
        }

        //
        // Initialization
        //

        // called from app manager right after app domain (and hosting env) is created
        internal void Initialize(ApplicationManager appManager, IApplicationHost appHost, IConfigMapPathFactory configMapPathFactory, HostingEnvironmentParameters hostingParameters, PolicyLevel policyLevel) {
            Initialize(appManager, appHost, configMapPathFactory, hostingParameters, policyLevel, null);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "We carefully control this method's callers.")]
        internal void Initialize(ApplicationManager appManager, IApplicationHost appHost, IConfigMapPathFactory configMapPathFactory,
            HostingEnvironmentParameters hostingParameters, PolicyLevel policyLevel,
            Exception appDomainCreationException) {

            _hostingParameters = hostingParameters;

            HostingEnvironmentFlags hostingFlags = HostingEnvironmentFlags.Default;
            if (_hostingParameters != null) {
                hostingFlags = _hostingParameters.HostingFlags;
                if (_hostingParameters.IISExpressVersion != null) {
                    ServerConfig.IISExpressVersion = _hostingParameters.IISExpressVersion;
                }
            }

            // Keep track of the app manager, unless HideFromAppManager flag was passed
            if ((hostingFlags & HostingEnvironmentFlags.HideFromAppManager) == 0)
                _appManager = appManager;

            if ((hostingFlags & HostingEnvironmentFlags.ClientBuildManager) != 0) {
                BuildManagerHost.InClientBuildManager = true;
            }

            if ((hostingFlags & HostingEnvironmentFlags.SupportsMultiTargeting) != 0) {
                BuildManagerHost.SupportsMultiTargeting = true;
            }


            //
            // init config system using private config if applicable
            //
            if (appHost is ISAPIApplicationHost && !ServerConfig.UseMetabase) {
                string rootWebConfigPath = ((ISAPIApplicationHost)appHost).ResolveRootWebConfigPath();
                if (!String.IsNullOrEmpty(rootWebConfigPath)) {
                    Debug.Assert(File.Exists(rootWebConfigPath), "File.Exists(rootWebConfigPath)");
                    HttpConfigurationSystem.RootWebConfigurationFilePath = rootWebConfigPath;
                }

                // we need to explicit create a COM proxy in this app domain
                // so we don't go back to the default domain or have lifetime issues
                // remember support functions
                IProcessHostSupportFunctions proxyFunctions = ((ISAPIApplicationHost)appHost).SupportFunctions;
                if (null != proxyFunctions) {
                    _functions = Misc.CreateLocalSupportFunctions(proxyFunctions);
                }
            }

            _appId = HttpRuntime.AppDomainAppId;
            _appVirtualPath = HttpRuntime.AppDomainAppVirtualPathObject;
            _appPhysicalPath = HttpRuntime.AppDomainAppPathInternal;
            _appHost = appHost;

            _configMapPath = configMapPathFactory.Create(_appVirtualPath.VirtualPathString, _appPhysicalPath);
            HttpConfigurationSystem.EnsureInit(_configMapPath, true, false);

            // attempt to cache and use IConfigMapPath2 provider
            // which supports VirtualPath's to save on conversions
            _configMapPath2 = _configMapPath as IConfigMapPath2;


            _initiateShutdownWorkItemCallback = new WaitCallback(this.InitiateShutdownWorkItemCallback);

            // notify app manager
            if (_appManager != null) {
                _appManager.HostingEnvironmentActivated(CacheMemorySizePressure.EffectiveProcessMemoryLimit);
            }

            // make sure there is always app host
            if (_appHost == null) {
                _appHost = new SimpleApplicationHost(_appVirtualPath, _appPhysicalPath);
            }
            else {
                _externalAppHost = true;
            }

            // remember the token to access config
            _configToken = _appHost.GetConfigToken();

            // Start with a MapPath based virtual path provider
            _mapPathBasedVirtualPathProvider = new MapPathBasedVirtualPathProvider();
            _virtualPathProvider = _mapPathBasedVirtualPathProvider;

            // initiaze HTTP-independent features
            HttpRuntime.InitializeHostingFeatures(hostingFlags, policyLevel, appDomainCreationException);

            // VSWhidbey 393259. Do not monitor idle timeout for CBM since Venus
            // will always restart a new appdomain if old one is shutdown.
            if (!BuildManagerHost.InClientBuildManager) {
                // start monitoring for idle inside app domain
                StartMonitoringForIdleTimeout();
            }

            // notify app manager if the app domain limit is violated
            EnforceAppDomainLimit();

            // get application identity (for explicit impersonation mode)
            GetApplicationIdentity();

            // call AppInitialize, unless the flag says not to do it (e.g. CBM scenario).
            // Also, don't call it if HostingInit failed (VSWhidbey 210495)
            if(!HttpRuntime.HostingInitFailed) {
                try {
                    BuildManager.ExecutePreAppStart();
                    if ((hostingFlags & HostingEnvironmentFlags.DontCallAppInitialize) == 0) {
                        BuildManager.CallAppInitializeMethod();
                    }
                }
                catch (Exception e) {
                    // could throw compilation errors in 'code' - report them with first http request
                    HttpRuntime.InitializationException = e;

                    if ((hostingFlags & HostingEnvironmentFlags.ThrowHostingInitErrors) != 0) {
                        throw;
                    }
                }
            }
        }

        private void InitializeObjectCacheHostPrivate() {
            // set ObjectCacheHost if the Host is not already set
            if (ObjectCache.Host == null) {
                ObjectCacheHost objectCacheHost = new ObjectCacheHost();
                ObjectCache.Host = objectCacheHost;
                _objectCacheHost = objectCacheHost;
            }
        }

        internal static void InitializeObjectCacheHost() {
            if (_theHostingEnvironment != null) {
                _theHostingEnvironment.InitializeObjectCacheHostPrivate();
            }
        }

        private void StartMonitoringForIdleTimeout() {
            HostingEnvironmentSection hostEnvConfig = RuntimeConfig.GetAppLKGConfig().HostingEnvironment;

            TimeSpan idleTimeout = (hostEnvConfig != null) ? hostEnvConfig.IdleTimeout : HostingEnvironmentSection.DefaultIdleTimeout;

            // always create IdleTimeoutMonitor (even if config value is TimeSpan.MaxValue (infinite)
            // IdleTimeoutMonitor is also needed to keep the last event for app domain set trimming
            // and the timer is used to trim the application instances
            _idleTimeoutMonitor = new IdleTimeoutMonitor(idleTimeout);
        }

        // enforce app domain limit
        private void EnforceAppDomainLimit() {
            if (_appManager == null)  /// detached app domain
                return;

            int limit = 0;

            try {
                ProcessModelSection pmConfig = RuntimeConfig.GetMachineConfig().ProcessModel;
                limit = pmConfig.MaxAppDomains;
            }
            catch {
            }

            if (limit > 0 && _appManager.AppDomainsCount >= limit) {
                // current app domain doesn't count yet (not in the table)
                // that's why '>=' above
                _appManager.ReduceAppDomainsCount(limit);
            }
        }

        private void GetApplicationIdentity() {
            // if the explicit impersonation is set, use it instead of UNC identity
            try {
                IdentitySection c = RuntimeConfig.GetAppConfig().Identity;
                if (c.Impersonate && c.ImpersonateToken != IntPtr.Zero) {
                    _appIdentity = c;
                    _appIdentityToken = c.ImpersonateToken;
                }
                else {
                    _appIdentityToken = _configToken;
                }
                _appIdentityTokenSet = true;
            }
            catch {
            }
        }


        // If an exception was thrown during initialization, return it.
        public static Exception InitializationException {
            get {
                return HttpRuntime.InitializationException;
            }
        }

        // called from app manager (from management APIs)
        internal ApplicationInfo GetApplicationInfo() {
            return new ApplicationInfo(_appId, _appVirtualPath, _appPhysicalPath);
        }

        //
        // Shutdown logic
        //
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private void StopRegisteredObjects(bool immediate) {
            if (_registeredObjects.Count > 0) {
                ArrayList list = new ArrayList();

                lock (this) {
                    foreach (DictionaryEntry e in _registeredObjects) {
                        Object x = e.Key;

                        // well-known objects first
                        if (IsWellKnownObject(x)) {
                            list.Insert(0, x);
                        }
                        else {
                            list.Add(x);
                        }
                    }
                }

                foreach (IRegisteredObject obj in list) {
                    try {
                        obj.Stop(immediate);
                    }
                    catch {
                    }
                }
            }
        }

        private void InitiateShutdownWorkItemCallback(Object state /*not used*/) {
            Debug.Trace("HostingEnvironmentShutdown", "Shutting down: appId=" + _appId);

            // no registered objects -- shutdown
            if (_registeredObjects.Count == 0) {
                Debug.Trace("HostingEnvironmentShutdown", "No registered objects");
                ShutdownThisAppDomainOnce();
                return;
            }

            // call Stop on all registered objects with immediate = false
            StopRegisteredObjects(false);

            // no registered objects -- shutdown now
            if (_registeredObjects.Count == 0) {
                Debug.Trace("HostingEnvironmentShutdown", "All registered objects gone after Stop(false)");
                ShutdownThisAppDomainOnce();
                return;
            }

            // if not everything shutdown synchronously give it some time.
            int shutdownTimeoutSeconds = HostingEnvironmentSection.DefaultShutdownTimeout;
            HostingEnvironmentSection hostEnvConfig = RuntimeConfig.GetAppLKGConfig().HostingEnvironment;
            if (hostEnvConfig != null) {
                shutdownTimeoutSeconds = (int) hostEnvConfig.ShutdownTimeout.TotalSeconds;
            }
            Debug.Trace("HostingEnvironmentShutdown", "Waiting for " + shutdownTimeoutSeconds + " sec...");

            DateTime waitUntil = DateTime.UtcNow.AddSeconds(shutdownTimeoutSeconds);
            while (_registeredObjects.Count > 0 && DateTime.UtcNow < waitUntil) {
                Thread.Sleep(100);
            }

            Debug.Trace("HostingEnvironmentShutdown", "Shutdown timeout (" + shutdownTimeoutSeconds + " sec) expired");

            // call Stop on all registered objects with immediate = true
            StopRegisteredObjects(true);

            // no registered objects -- shutdown now
            if (_registeredObjects.Count == 0) {
                Debug.Trace("HostingEnvironmentShutdown", "All registered objects gone after Stop(true)");
                ShutdownThisAppDomainOnce();
                return;
            }

            // shutdown regardless
            Debug.Trace("HostingEnvironmentShutdown", "Forced shutdown: " + _registeredObjects.Count + " registered objects left");
            _registeredObjects = new Hashtable();
            ShutdownThisAppDomainOnce();
        }

        // app domain shutdown logic
        internal void InitiateShutdownInternal() {
#if DBG
            try {
#endif
            Debug.Trace("AppManager", "HostingEnvironment.InitiateShutdownInternal appId=" + _appId);

            bool proceed = false;

            if (!_shutdownInitiated) {
                lock (this) {
                    if (!_shutdownInitiated) {
                        _shutdownInProgress = true;
                        proceed = true;
                        _shutdownInitiated = true;
                    }
                }
            }

            if (!proceed) {
                return;
            }

            HttpRuntime.SetShutdownReason(ApplicationShutdownReason.HostingEnvironment, "HostingEnvironment initiated shutdown");

            // Avoid calling Environment.StackTrace if we are in the ClientBuildManager (Dev10 bug 824659)
            if (!BuildManagerHost.InClientBuildManager) {
                new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                try {
                    _shutDownStack = Environment.StackTrace;
                }
                finally {
                    CodeAccessPermission.RevertAssert();
                }
            }

            // waitChangeNotification need not be honored in ClientBuildManager (Dev11 bug 264894)
            if (!BuildManagerHost.InClientBuildManager) {
                // this should only be called once, before the cache is disposed, and
                // the config records are released.
                HttpRuntime.CoalesceNotifications();
            }

            RemoveThisAppDomainFromAppManagerTableOnce();

            // stop all registered objects without blocking
            ThreadPool.QueueUserWorkItem(this._initiateShutdownWorkItemCallback);
#if DBG
            } catch (Exception ex) {
                HandleExceptionFromInitiateShutdownInternal(ex);
                throw;
            }
#endif
        }

#if DBG
        // InitiateShutdownInternal should never throw an exception, but we have seen cases where
        // CLR bugs can cause it to fail without running to completion. This could cause an ASP.NET
        // AppDomain never to unload. If we detect that an exception is thrown, we should DebugBreak
        // so that the fundamentals team can investigate. Taking the Exception object as a parameter
        // makes it easy to locate when looking at a stack dump.
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void HandleExceptionFromInitiateShutdownInternal(Exception ex) {
            Debug.Break();
        }
#endif

        internal bool HasBeenRemovedFromAppManagerTable {
            get {
                return _hasBeenRemovedFromAppManangerTable;
            }
            set {
                _hasBeenRemovedFromAppManangerTable = value;
            }
        }

        private void RemoveThisAppDomainFromAppManagerTableOnce() {
            bool proceed = false;
            if (!_removedFromAppManager) {
                lock (this) {
                    if (!_removedFromAppManager) {
                        proceed = true;
                        _removedFromAppManager = true;
                    }
                }
            }

            if (!proceed)
                return;

            if (_appManager != null) {
                Debug.Trace("AppManager", "Removing HostingEnvironment from AppManager table, appId=" + _appId);
                _appManager.HostingEnvironmentShutdownInitiated(_appId, this);
            }
#if DBG
            Debug.Trace("FileChangesMonitorIgnoreSubdirChange", 
                        "*** REMOVE APPMANAGER TABLE" + DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) 
                        + ": _appId=" + _appId);
#endif
        }

        private void ShutdownThisAppDomainOnce() {
            bool proceed = false;

            if (!_appDomainShutdownStarted) {
                lock (this) {
                    if (!_appDomainShutdownStarted) {
                        proceed = true;
                        _appDomainShutdownStarted = true;
                    }
                }
            }

            if (!proceed)
                return;

            Debug.Trace("AppManager", "HostingEnvironment - shutting down AppDomain, appId=" + _appId);

            // stop the timer used for idle timeout
            if (_idleTimeoutMonitor != null) {
                _idleTimeoutMonitor.Stop();
                _idleTimeoutMonitor = null;
            }

            while (_inTrimCache == 1) {
                Thread.Sleep(100);
            }

            // close all outstanding WebSocket connections and begin winding down code that consumes them
            AspNetWebSocketManager.Current.AbortAllAndWait();

            // 
            HttpRuntime.SetUserForcedShutdown();

            //WOS 1400290: CantUnloadAppDomainException in ISAPI mode, wait until HostingEnvironment.ShutdownThisAppDomainOnce completes
            _shutdownInProgress = false;

            HttpRuntime.ShutdownAppDomainWithStackTrace(ApplicationShutdownReason.HostingEnvironment,
                                                        SR.GetString(SR.Hosting_Env_Restart),
                                                        _shutDownStack);
        }

        //
        // internal methods called by app manager
        //

        // helper for app manager to implement AppHost.CreateAppHost
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal ObjectHandle CreateInstance(String assemblyQualifiedName) {
            Type type = Type.GetType(assemblyQualifiedName, true);
            return new ObjectHandle(Activator.CreateInstance(type));
        }

        // start well known object
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal ObjectHandle CreateWellKnownObjectInstance(String assemblyQualifiedName, bool failIfExists) {
            Type type = Type.GetType(assemblyQualifiedName, true);
            IRegisteredObject obj = null;
            String key = type.FullName;
            bool exists = false;

            lock (this) {
                obj = _wellKnownObjects[key] as IRegisteredObject;

                if (obj == null) {
                    obj = (IRegisteredObject)Activator.CreateInstance(type);
                    _wellKnownObjects[key] = obj;
                }
                else {
                    exists = true;
                }
            }

            if (exists && failIfExists) {
                throw new InvalidOperationException(SR.GetString(SR.Wellknown_object_already_exists, key));
            }

            return new ObjectHandle(obj);
        }

        // check if well known object
        private bool IsWellKnownObject(Object obj) {
            bool found = false;
            String key = obj.GetType().FullName;

            lock (this) {
                if (_wellKnownObjects[key] == obj) {
                    found = true;
                }
            }

            return found;
        }

        // find well known object by type
        internal ObjectHandle FindWellKnownObject(String assemblyQualifiedName) {
            Type type = Type.GetType(assemblyQualifiedName, true);
            IRegisteredObject obj = null;
            String key = type.FullName;

            lock (this) {
                obj = _wellKnownObjects[key] as IRegisteredObject;
            }

            return (obj != null) ? new ObjectHandle(obj) : null;
        }

        // stop well known object by type
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void StopWellKnownObject(String assemblyQualifiedName) {
            Type type = Type.GetType(assemblyQualifiedName, true);
            IRegisteredObject obj = null;
            String key = type.FullName;

            lock (this) {
                obj = _wellKnownObjects[key] as IRegisteredObject;
                if (obj != null) {
                    _wellKnownObjects.Remove(key);
                    obj.Stop(false);
                }
            }
        }

        internal bool IsIdle() {
            bool isBusy = _isBusy;
            _isBusy = false;
            return (!isBusy && _busyCount == 0);
        }

        internal bool GetIdleValue() {
            return (!_isBusy && _busyCount == 0);
        }

        internal void IncrementBusyCountInternal() {
            _isBusy = true;
            Interlocked.Increment(ref _busyCount);
        }

        internal void DecrementBusyCountInternal() {
            _isBusy = true;
            Interlocked.Decrement(ref _busyCount);

            // Notify idle timeout monitor
            IdleTimeoutMonitor itm = _idleTimeoutMonitor;
            if (itm != null) {
                itm.LastEvent = DateTime.UtcNow;
            }
        }
        internal void IsUnloaded()
        {
            return;
        }

        private void MessageReceivedInternal() {
            _isBusy = true;

            IdleTimeoutMonitor itm = _idleTimeoutMonitor;
            if (itm != null) {
                itm.LastEvent = DateTime.UtcNow;
            }
        }

        // the busier the app domain the higher the score
        internal int LruScore {
            get {
                if (_busyCount > 0)
                    return _busyCount;

                IdleTimeoutMonitor itm = _idleTimeoutMonitor;

                if (itm == null)
                    return 0;

                // return negative number of seconds since last activity
                return -(int)(DateTime.UtcNow - itm.LastEvent).TotalSeconds;
            }
        }

        internal static ApplicationManager GetApplicationManager() {
            if (_theHostingEnvironment == null)
                return null;

            return _theHostingEnvironment._appManager;
        }

        //
        // private helpers
        //

        // register protocol handler with hosting environment
        private void RegisterRunningObjectInternal(IRegisteredObject obj) {
            lock (this) {
                _registeredObjects[obj] = obj;

                ISuspendibleRegisteredObject suspendibleObject = obj as ISuspendibleRegisteredObject;
                if (suspendibleObject != null) {
                    _suspendManager.RegisterObject(suspendibleObject);
                }
            }
        }

        // unregister protocol handler from hosting environment
        private void UnregisterRunningObjectInternal(IRegisteredObject obj) {
            bool lastOne = false;

            lock (this) {
                // if it is a well known object, remove it from that table as well
                String key = obj.GetType().FullName;
                if (_wellKnownObjects[key] == obj) {
                    _wellKnownObjects.Remove(key);
                }

                // remove from running objects list
                _registeredObjects.Remove(obj);

                ISuspendibleRegisteredObject suspendibleObject = obj as ISuspendibleRegisteredObject;
                if (suspendibleObject != null) {
                    _suspendManager.UnregisterObject(suspendibleObject);
                }

                if (_registeredObjects.Count == 0)
                    lastOne = true;
            }

            if (!lastOne)
                return;

            // shutdown app domain after last protocol handler is gone

            InitiateShutdownInternal();
        }

        // site name
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method is not dangerous.")]
        private String GetSiteName() {
            if (_siteName == null) {
                lock (this) {
                    if (_siteName == null) {
                        String s = null;

                        if (_appHost != null) {
                            // 
                            InternalSecurityPermissions.Unrestricted.Assert();
                            try {
                                s = _appHost.GetSiteName();
                            }
                            finally {
                                CodeAccessPermission.RevertAssert();
                            }
                        }

                        if (s == null)
                            s = WebConfigurationHost.DefaultSiteName;

                        _siteName = s;
                    }
                }
            }

            return _siteName;
        }

        // site ID
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method is not dangerous.")]
        private String GetSiteID() {
            if (_siteID == null) {
                lock (this) {
                    if (_siteID == null) {
                        String s = null;

                        if (_appHost != null) {
                            // 
                            InternalSecurityPermissions.Unrestricted.Assert();
                            try {
                                s = _appHost.GetSiteID();
                            }
                            finally {
                                CodeAccessPermission.RevertAssert();
                            }
                        }

                        if (s == null)
                            s = WebConfigurationHost.DefaultSiteID;

                        _siteID = s.ToLower(CultureInfo.InvariantCulture);
                    }
                }
            }

            return _siteID;
        }

        // Return the configPath for the app, e.g. "machine/webroot/1/myapp"
        private String GetAppConfigPath() {
            if (_appConfigPath == null) {
                _appConfigPath = WebConfigurationHost.GetConfigPathFromSiteIDAndVPath(SiteID, ApplicationVirtualPathObject);
            }

            return _appConfigPath;
        }

        // Return the call context slot name to use for a virtual path
        private static string GetFixedMappingSlotName(VirtualPath virtualPath) {
            return "MapPath_" + virtualPath.VirtualPathString.ToLowerInvariant().GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        /*
         * Map a virtual path to a physical path.  i.e. the physicalPath will be returned
         * when MapPath is called on the virtual path, bypassing the IApplicationHost
         */
        private static string GetVirtualPathToFileMapping(VirtualPath virtualPath) {
            return CallContext.GetData(GetFixedMappingSlotName(virtualPath)) as string;
        }

        /*
         * Map a virtual path to a physical path.  i.e. the physicalPath will be returned
         * when MapPath is called on the virtual path, bypassing the IApplicationHost
         */
        internal static object AddVirtualPathToFileMapping(
            VirtualPath virtualPath, string physicalPath) {

            // Save the mapping in the call context, using a key derived from the
            // virtual path.  The mapping is only valid for the duration of the request.
            CallContext.SetData(GetFixedMappingSlotName(virtualPath), physicalPath);

            // Return a mapping object to keep track of the virtual path, and of the current
            // virtualPathProvider.
            VirtualPathToFileMappingState state = new VirtualPathToFileMappingState();
            state.VirtualPath = virtualPath;
            state.VirtualPathProvider = _theHostingEnvironment._virtualPathProvider;

            // Always use the MapPathBasedVirtualPathProvider, otherwise the mapping mechanism
            // doesn't work (VSWhidbey 420702)
            // Set/Get the VPP on the call context so as not to affect other concurrent requests  (Dev10 852255)
            CallContext.SetData(TemporaryVirtualPathProviderKey, _theHostingEnvironment._mapPathBasedVirtualPathProvider);

            return state;
        }

        internal static void ClearVirtualPathToFileMapping(object state) {

            VirtualPathToFileMappingState mapping = (VirtualPathToFileMappingState)state;

            // Clear the mapping from the call context
            CallContext.SetData(GetFixedMappingSlotName(mapping.VirtualPath), null);

            // Restore the previous VirtualPathProvider
            // Set/Get the VPP on the call context so as not to affect other concurrent requests  (Dev10 852255)
            CallContext.SetData(TemporaryVirtualPathProviderKey, null);
        }

        private string MapPathActual(VirtualPath virtualPath, bool permitNull)
        {
            string result = null;

            Debug.Assert(virtualPath != null);

            virtualPath.FailIfRelativePath();

            VirtualPath reqpath = virtualPath;

            if (String.CompareOrdinal(reqpath.VirtualPathString, _appVirtualPath.VirtualPathString) == 0) {
                // for application path don't need to call app host
                Debug.Trace("MapPath", reqpath  +" is the app path");
                result = _appPhysicalPath;
            }
            else {
                using (new ProcessImpersonationContext()) {
                    // If there is a mapping for this virtual path in the call context, use it
                    result = GetVirtualPathToFileMapping(reqpath);

                    if (result == null) {
                        // call host's mappath
                        if (_configMapPath == null) {
                            Debug.Trace("MapPath", "Missing _configMapPath");
                            throw new InvalidOperationException(SR.GetString(SR.Cannot_map_path, reqpath));
                        }
                        Debug.Trace("MapPath", "call ConfigMapPath (" + reqpath + ")");

                        // see if the IConfigMapPath provider implements the interface
                        // with VirtualPath
                        try {
                            if (null != _configMapPath2) {
                                result = _configMapPath2.MapPath(GetSiteID(), reqpath);
                            }
                            else {
                                result = _configMapPath.MapPath(GetSiteID(), reqpath.VirtualPathString);
                            }
                            if (HttpRuntime.IsMapPathRelaxed)
                                result = HttpRuntime.GetRelaxedMapPathResult(result);
                        } catch {
                            if (HttpRuntime.IsMapPathRelaxed)
                                result = HttpRuntime.GetRelaxedMapPathResult(null);
                            else
                                throw;
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(result)) {
                Debug.Trace("MapPath", "null Result");
                if (!permitNull) {
                    if (HttpRuntime.IsMapPathRelaxed)
                        result = HttpRuntime.GetRelaxedMapPathResult(null);
                    else
                        throw new InvalidOperationException(SR.GetString(SR.Cannot_map_path, reqpath));
                }
            }
            else {
                // ensure extra '\\' in the physical path if the virtual path had extra '/'
                // and the other way -- no extra '\\' in physical if virtual didn't have it.
                if (virtualPath.HasTrailingSlash) {
                    if (!UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result))
                        result = result + "\\";
                }
                else {
                    if (UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result))
                        result = result.Substring(0, result.Length - 1);
                }

                Debug.Trace("MapPath", "    result=" + result);
            }

            return result;
        }

        //
        // public static methods
        //


        // register protocol handler with hosting environment
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static void RegisterObject(IRegisteredObject obj) {
            if (_theHostingEnvironment != null)
                _theHostingEnvironment.RegisterRunningObjectInternal(obj);
        }


        // unregister protocol handler from hosting environment
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static void UnregisterObject(IRegisteredObject obj) {
            if (_theHostingEnvironment != null)
                _theHostingEnvironment.UnregisterRunningObjectInternal(obj);
        }

        // Schedules a task which can run in the background, independent of any request.
        // This differs from a normal ThreadPool work item in that ASP.NET can keep track
        // of how many work items registered through this API are currently running, and
        // the ASP.NET runtime will try not to delay AppDomain shutdown until these work
        // items have finished executing.
        //
        // Usage notes:
        // - This API cannot be called outside of an ASP.NET-managed AppDomain.
        // - The caller's ExecutionContext is not flowed to the work item.
        // - Scheduled work items are not guaranteed to ever execute, e.g., when AppDomain
        //   shutdown has already started by the time this API was called.
        // - The provided CancellationToken will be signaled when the application is
        //   shutting down. The work item should make every effort to honor this token.
        //   If a work item does not honor this token and continues executing it will
        //   eventually be considered rogue, and the ASP.NET runtime will rudely unload
        //   the AppDomain without waiting for the work item to finish.
        //
        // This overload of QueueBackgroundWorkItem takes a void-returning callback; the
        // work item will be considered finished when the callback returns.
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void QueueBackgroundWorkItem(Action<CancellationToken> workItem) {
            if (workItem == null) {
                throw new ArgumentNullException("workItem");
            }

            QueueBackgroundWorkItem(ct => { workItem(ct); return _completedTask; });
        }

        // See documentation on the other overload for a general API overview.
        //
        // This overload of QueueBackgroundWorkItem takes a Task-returning callback; the
        // work item will be considered finished when the returned Task transitions to a
        // terminal state.
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem) {
            if (workItem == null) {
                throw new ArgumentNullException("workItem");
            }
            if (_theHostingEnvironment == null) {
                throw new InvalidOperationException(); // can only be called within an ASP.NET AppDomain
            }

            _theHostingEnvironment.QueueBackgroundWorkItemInternal(workItem);
        }

        private void QueueBackgroundWorkItemInternal(Func<CancellationToken, Task> workItem) {
            Debug.Assert(workItem != null);

            BackgroundWorkScheduler scheduler = Volatile.Read(ref _backgroundWorkScheduler);

            // If the scheduler doesn't exist, lazily create it, but only allow one instance to ever be published to the backing field
            if (scheduler == null) {
                BackgroundWorkScheduler newlyCreatedScheduler = new BackgroundWorkScheduler(UnregisterObject, Misc.WriteUnhandledExceptionToEventLog);
                scheduler = Interlocked.CompareExchange(ref _backgroundWorkScheduler, newlyCreatedScheduler, null) ?? newlyCreatedScheduler;
                if (scheduler == newlyCreatedScheduler) {
                    RegisterObject(scheduler); // Only call RegisterObject if we just created the "winning" one
                }
            }

            scheduler.ScheduleWorkItem(workItem);
        }

        // This event is a simple way to hook IStopListeningRegisteredObject.StopListening
        // without needing to call RegisterObject. The same restrictions which apply to
        // that method apply to this event.
        public static event EventHandler StopListening;

        //
        // public static methods for the user code to call
        //


        public static void IncrementBusyCount() {
            if (_theHostingEnvironment != null)
                _theHostingEnvironment.IncrementBusyCountInternal();
        }


        public static void DecrementBusyCount() {
            if (_theHostingEnvironment != null)
                _theHostingEnvironment.DecrementBusyCountInternal();
        }


        public static void MessageReceived() {
            if (_theHostingEnvironment != null)
                _theHostingEnvironment.MessageReceivedInternal();
        }

        public static bool InClientBuildManager {
            get {
                return BuildManagerHost.InClientBuildManager;
            }
        }

        public static bool IsHosted {
            get {
                return (_theHostingEnvironment != null);
            }
        }

        internal static bool IsUnderIISProcess {
            get {
                String process = VersionInfo.ExeName;

                return process == "aspnet_wp" ||
                       process == "w3wp" ||
                       process == "inetinfo";
            }
        }

        internal static bool IsUnderIIS6Process {
            get {
                return VersionInfo.ExeName == "w3wp";
            }
        }

        public static IApplicationHost ApplicationHost {
            //DevDivBugs 109864: ASP.NET: path discovery issue - In low trust, it is possible to get the physical path of any virtual path on the machine
            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            get {
                if (_theHostingEnvironment == null)
                    return null;

                return _theHostingEnvironment._appHost;
            }
        }

       internal static IApplicationHost ApplicationHostInternal {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                return _theHostingEnvironment._appHost;
            }
        }

        internal IApplicationHost InternalApplicationHost {
            get {
                return _appHost;
            }
        }

        internal static int BusyCount {
            get {
                if (_theHostingEnvironment == null)
                    return 0;

                return _theHostingEnvironment._busyCount;
            }
        }

        internal static bool ShutdownInitiated {
            get {
                if (_theHostingEnvironment == null)
                    return false;

                return _theHostingEnvironment._shutdownInitiated;
            }
        }


        internal static bool ShutdownInProgress {
            get {
                if (_theHostingEnvironment == null)
                    return false;

                return _theHostingEnvironment._shutdownInProgress;
            }
        }


        /// <devdoc>
        ///    <para>The application ID (metabase path in IIS hosting).</para>
        /// </devdoc>
        public static String ApplicationID {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                InternalSecurityPermissions.AspNetHostingPermissionLevelHigh.Demand();
                return _theHostingEnvironment._appId;
            }
        }

        internal static String ApplicationIDNoDemand {
            get {
                if (_theHostingEnvironment == null) {
                    return null;
                }

                return _theHostingEnvironment._appId;
            }
        }


        /// <devdoc>
        ///    <para>Physical path to the application root.</para>
        /// </devdoc>
        public static String ApplicationPhysicalPath {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                InternalSecurityPermissions.AppPathDiscovery.Demand();
                return _theHostingEnvironment._appPhysicalPath;
            }
        }


        /// <devdoc>
        ///    <para>Virtual path to the application root.</para>
        /// </devdoc>
        public static String ApplicationVirtualPath {
            get {
                return VirtualPath.GetVirtualPathStringNoTrailingSlash(ApplicationVirtualPathObject);
            }
        }

        internal static VirtualPath ApplicationVirtualPathObject {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                return _theHostingEnvironment._appVirtualPath;
            }
        }


        /// <devdoc>
        ///    <para>Site name.</para>
        /// </devdoc>
        public static String SiteName {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                InternalSecurityPermissions.AspNetHostingPermissionLevelMedium.Demand();
                return _theHostingEnvironment.GetSiteName();
            }
        }

        internal static String SiteNameNoDemand {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                return _theHostingEnvironment.GetSiteName();
            }
        }

        internal static String SiteID {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                return _theHostingEnvironment.GetSiteID();
            }
        }

        internal static IConfigMapPath ConfigMapPath {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                return _theHostingEnvironment._configMapPath;
            }
        }

        internal static String AppConfigPath {
            get {
                if (_theHostingEnvironment == null) {
                    return null;
                }

                return _theHostingEnvironment.GetAppConfigPath();
            }
        }

        // See comments in ApplicationManager.CreateAppDomainWithHostingEnvironment. This is the public API to access the
        // information we determined in that method. Defaults to 'false' if our AppDomain data isn't present.
        public static bool IsDevelopmentEnvironment {
            get {
                return (AppDomain.CurrentDomain.GetData(".devEnvironment") as bool?) == true;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a reference to the System.Web.Cache.Cache object for the current request.
        ///    </para>
        /// </devdoc>
        public static Cache Cache {
            get { return HttpRuntime.Cache; }
        }

        // count of all app domain from app manager
        internal static int AppDomainsCount {
            get {
                ApplicationManager appManager = GetApplicationManager();
                return (appManager != null) ? appManager.AppDomainsCount : 0;
            }
        }

        internal static HostingEnvironmentParameters HostingParameters {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                return _theHostingEnvironment._hostingParameters;
            }
        }

        // Return an integer that is unique for each appdomain.  This can be used
        // to create things like once-per-appdomain temp files without having different
        // processes/appdomains step on each other
        private static int s_appDomainUniqueInteger;
        internal static int AppDomainUniqueInteger {
            get {
                if (s_appDomainUniqueInteger == 0) {
                    s_appDomainUniqueInteger = Guid.NewGuid().GetHashCode();
                }

                return s_appDomainUniqueInteger;
            }
        }

        public static ApplicationShutdownReason ShutdownReason {
            get { return HttpRuntime.ShutdownReason; }
        }

        // Was CGlobalModule::OnGlobalStopListening called?
        internal static bool StopListeningWasCalled {
            get {
                return _stopListeningWasCalled;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive", Justification = "See comment in function.")]
        internal static void SetupStopListeningHandler() {
            StopListeningWaitHandle waitHandle = new StopListeningWaitHandle();

            RegisteredWaitHandle registeredWaitHandle = null;
            registeredWaitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle, (_, __) => {
                // Referencing the field from within the callback should be sufficient to keep the GC
                // from reclaiming the RegisteredWaitHandle; the race condition is fine.
                GC.KeepAlive(registeredWaitHandle);
                OnGlobalStopListening();
            }, null, Timeout.Infinite, executeOnlyOnce: true);
        }

        private static void OnGlobalStopListening() {
            _stopListeningWasCalled = true;

            EventHandler eventHandler = StopListening;
            if (eventHandler != null) {
                eventHandler(null /* static means no sender */, EventArgs.Empty);
            }

            if (_theHostingEnvironment != null) {
                _theHostingEnvironment.FireStopListeningHandlers();
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "'this' always has strong identity.")]
        private void FireStopListeningHandlers() {
            List<IStopListeningRegisteredObject> listeners = new List<IStopListeningRegisteredObject>();
            lock (this) {
                foreach (DictionaryEntry e in _registeredObjects) {
                    IStopListeningRegisteredObject listener = e.Key as IStopListeningRegisteredObject;
                    if (listener != null) {
                        listeners.Add(listener);
                    }
                }
            }

            foreach (var listener in listeners) {
                listener.StopListening();
            }
        }

        /// <devdoc>
        ///    <para>Initiate app domain unloading for the current app.</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static void InitiateShutdown() {
            if (_theHostingEnvironment != null)
                _theHostingEnvironment.InitiateShutdownInternal();
        }

        internal static void InitiateShutdownWithoutDemand() {
            if (_theHostingEnvironment != null)
                _theHostingEnvironment.InitiateShutdownInternal();
        }

        //
        // Internal methods for the ApplicationManager to suspend / resume this application.
        // Using GCHandle instead of ObjectHandle means we don't need to worry about lease lifetimes.
        //

        internal IntPtr SuspendApplication() {
            var state = _suspendManager.Suspend();
            return GCUtil.RootObject(state);
        }

        internal void ResumeApplication(IntPtr state) {
            var unwrappedState = GCUtil.UnrootObject(state);
            _suspendManager.Resume(unwrappedState);
        }

        /// <devdoc>
        ///    <para>Maps a virtual path to a physical path.</para>
        /// </devdoc>
        public static string MapPath(string virtualPath) {
            return MapPath(VirtualPath.Create(virtualPath));
        }

        internal static string MapPath(VirtualPath virtualPath) {
            if (_theHostingEnvironment == null)
                return null;

            String path = MapPathInternal(virtualPath);

            if (path != null)
                InternalSecurityPermissions.PathDiscovery(path).Demand();

            return path;
        }

        internal static String MapPathInternal(string virtualPath) {
            return MapPathInternal(VirtualPath.Create(virtualPath));
        }

        internal static String MapPathInternal(VirtualPath virtualPath) {
            if (_theHostingEnvironment == null) {
                return null;
            }

            return _theHostingEnvironment.MapPathActual(virtualPath, false);
        }

        internal static String MapPathInternal(string virtualPath, bool permitNull) {
            return MapPathInternal(VirtualPath.Create(virtualPath), permitNull);
        }

        internal static String MapPathInternal(VirtualPath virtualPath, bool permitNull) {
            if (_theHostingEnvironment == null) {
                return null;
            }

            return _theHostingEnvironment.MapPathActual(virtualPath, permitNull);
        }

        internal static string MapPathInternal(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) {
            return MapPathInternal(VirtualPath.Create(virtualPath),
                VirtualPath.CreateNonRelative(baseVirtualDir), allowCrossAppMapping);
        }

        internal static string MapPathInternal(VirtualPath virtualPath, VirtualPath baseVirtualDir, bool allowCrossAppMapping) {
            Debug.Assert(baseVirtualDir != null, "baseVirtualDir != null");

            // Combine it with the base and reduce
            virtualPath = baseVirtualDir.Combine(virtualPath);

            if (!allowCrossAppMapping && !virtualPath.IsWithinAppRoot)
                throw new ArgumentException(SR.GetString(SR.Cross_app_not_allowed, virtualPath));

            return MapPathInternal(virtualPath);
        }

        internal static WebApplicationLevel GetPathLevel(String path) {
            WebApplicationLevel pathLevel = WebApplicationLevel.AboveApplication;

            if (_theHostingEnvironment != null && !String.IsNullOrEmpty(path)) {
                String appPath = ApplicationVirtualPath;

                if (appPath == "/") {
                    if (path == "/") {
                        pathLevel = WebApplicationLevel.AtApplication;
                    }
                    else if (path[0] == '/') {
                        pathLevel = WebApplicationLevel.BelowApplication;
                    }
                }
                else {
                    if (StringUtil.EqualsIgnoreCase(appPath, path)) {
                        pathLevel = WebApplicationLevel.AtApplication;
                    }
                    else if (path.Length > appPath.Length && path[appPath.Length] == '/' &&
                        StringUtil.StringStartsWithIgnoreCase(path, appPath)) {

                        pathLevel = WebApplicationLevel.BelowApplication;
                    }
                }
            }

            return pathLevel;
        }


        //
        // Impersonation helpers
        //
        // user token for the app (hosting / unc)
        internal static IntPtr ApplicationIdentityToken {
            get {
                if (_theHostingEnvironment == null) {
                    return IntPtr.Zero;
                }
                else {
                    if (_theHostingEnvironment._appIdentityTokenSet)
                        return _theHostingEnvironment._appIdentityToken;
                    else
                        return _theHostingEnvironment._configToken;
                }
            }
        }


        // check if application impersonation != process impersonation
        internal static bool HasHostingIdentity {
            get {
                return (ApplicationIdentityToken != IntPtr.Zero);
            }
        }

        // impersonate application identity
        [SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
        public static IDisposable Impersonate() {
            return new ApplicationImpersonationContext();
        }

        // impersonate the given user identity
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static IDisposable Impersonate(IntPtr token) {
            if (token == IntPtr.Zero) {
                return new ProcessImpersonationContext();
            }
            else {
                return new ImpersonationContext(token);
            }
        }

        // impersonate as configured for a given path
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static IDisposable Impersonate(IntPtr userToken, String virtualPath) {
            virtualPath = UrlPath.MakeVirtualPathAppAbsoluteReduceAndCheck(virtualPath);

            if (_theHostingEnvironment == null) {
                return Impersonate(userToken);
            }

            IdentitySection c = RuntimeConfig.GetConfig(virtualPath).Identity;
            if (c.Impersonate) {
                if (c.ImpersonateToken != IntPtr.Zero) {
                    return new ImpersonationContext(c.ImpersonateToken);
                }
                else {
                    return new ImpersonationContext(userToken);
                }
            }
            else {
                return new ApplicationImpersonationContext();
            }
        }

        //
        //  Culture helpers
        //

        public static IDisposable SetCultures() {
            return SetCultures(RuntimeConfig.GetAppLKGConfig().Globalization);
        }

        public static IDisposable SetCultures(string virtualPath) {
            virtualPath = UrlPath.MakeVirtualPathAppAbsoluteReduceAndCheck(virtualPath);
            return SetCultures(RuntimeConfig.GetConfig(virtualPath).Globalization);
        }

        private static IDisposable SetCultures(GlobalizationSection gs) {
            CultureContext c = new CultureContext();

            if (gs != null) {
                CultureInfo culture = null;
                CultureInfo uiCulture = null;

                if (gs.Culture != null && gs.Culture.Length > 0) {
                    try {
                        culture = HttpServerUtility.CreateReadOnlyCultureInfo(gs.Culture);
                    }
                    catch {
                    }
                }

                if (gs.UICulture != null && gs.UICulture.Length > 0) {
                    try {
                        uiCulture = HttpServerUtility.CreateReadOnlyCultureInfo(gs.UICulture);
                    }
                    catch {
                    }
                }

                c.SetCultures(culture, uiCulture);
            }

            return c;
        }


        class CultureContext : IDisposable {
            CultureInfo _savedCulture;
            CultureInfo _savedUICulture;

            internal CultureContext() {
            }

            void IDisposable.Dispose() {
                RestoreCultures();
            }

            internal void SetCultures(CultureInfo culture, CultureInfo uiCulture) {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

                if (culture != null && culture != currentCulture) {
                    Thread.CurrentThread.CurrentCulture = culture;
                    _savedCulture = currentCulture;
                }

                if (uiCulture != null && uiCulture != currentCulture) {
                    Thread.CurrentThread.CurrentUICulture = uiCulture;
                    _savedUICulture = currentUICulture;
                }
            }

            internal void RestoreCultures() {
                if (_savedCulture != null && _savedCulture != Thread.CurrentThread.CurrentCulture) {
                    Thread.CurrentThread.CurrentCulture = _savedCulture;
                    _savedCulture = null;
                }

                if (_savedUICulture != null && _savedUICulture != Thread.CurrentThread.CurrentUICulture) {
                    Thread.CurrentThread.CurrentUICulture = _savedUICulture;
                    _savedUICulture = null;
                }
            }
        }

        //
        // VirtualPathProvider related code
        //

        private VirtualPathProvider _virtualPathProvider;
        private VirtualPathProvider _mapPathBasedVirtualPathProvider;


        public static VirtualPathProvider VirtualPathProvider {
            get {
                if (_theHostingEnvironment == null)
                    return null;

                // Set/Get the VPP on the call context so as not to affect other concurrent requests  (Dev10 852255)
                var tempVPP = CallContext.GetData(TemporaryVirtualPathProviderKey);
                if (tempVPP != null) {
                    return tempVPP as VirtualPathProvider;
                }

                return _theHostingEnvironment._virtualPathProvider;
            }
        }

        internal static bool UsingMapPathBasedVirtualPathProvider {
            get {
                if (_theHostingEnvironment == null)
                    return true;

                return (_theHostingEnvironment._virtualPathProvider ==
                    _theHostingEnvironment._mapPathBasedVirtualPathProvider);
            }
        }

        // [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        // Removed the above LinkDemand for AspNetHostingPermissionLevel.High. If we decide to add VPP
        // support for config in the future, we should have a separate API with a demand for registering
        // VPPs supporting configuration.
        public static void RegisterVirtualPathProvider(VirtualPathProvider virtualPathProvider) {

            if (_theHostingEnvironment == null)
                throw new InvalidOperationException();

            // Ignore the VirtualPathProvider on precompiled sites (VSWhidbey 368169,404844)
            if (BuildManager.IsPrecompiledApp)
                return;

            RegisterVirtualPathProviderInternal(virtualPathProvider);
        }

        internal static void RegisterVirtualPathProviderInternal(VirtualPathProvider virtualPathProvider) {
            VirtualPathProvider previous = _theHostingEnvironment._virtualPathProvider;
            _theHostingEnvironment._virtualPathProvider = virtualPathProvider;

            // Give it the previous provider so it can delegate if needed
            virtualPathProvider.Initialize(previous);
        }

        // Helper class used to keep track of state when using
        // AddVirtualPathToFileMapping & ClearVirtualPathToFileMapping
        internal class VirtualPathToFileMappingState {
            internal VirtualPath VirtualPath;
            internal VirtualPathProvider VirtualPathProvider;
        }

        internal static IProcessHostSupportFunctions SupportFunctions {
            get {
                return _functions;
            }
            set {
                _functions = value;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", 
                         Justification="matches casing of config attribute")]
        public static int MaxConcurrentRequestsPerCPU {
            get {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                return UnsafeIISMethods.MgdGetMaxConcurrentRequestsPerCPU();
            }
            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            set {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                int hr = UnsafeIISMethods.MgdSetMaxConcurrentRequestsPerCPU(value);
                switch (hr) {
                    case HResults.S_FALSE:
                        // Because "maxConcurrentRequestsPerCPU" is currently zero, we cannot set the value, since that would
                        // enable the feature, which can only be done via configuration.
                        throw new InvalidOperationException(SR.GetString(SR.Queue_limit_is_zero, "maxConcurrentRequestsPerCPU"));
                    case HResults.E_INVALIDARG:
                        // The value must be greater than zero.  A value of zero would disable the feature, but this can only be done via configuration.
                        throw new ArgumentException(SR.GetString(SR.Invalid_queue_limit));
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased",
                         Justification="matches casing of config attribute")]
        public static int MaxConcurrentThreadsPerCPU {
            get {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                return UnsafeIISMethods.MgdGetMaxConcurrentThreadsPerCPU();
            }
            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            set {
                if (!HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
                }
                int hr = UnsafeIISMethods.MgdSetMaxConcurrentThreadsPerCPU(value);
                switch (hr) {
                    case HResults.S_FALSE:
                        // Because "maxConcurrentThreadsPerCPU" is currently zero, we cannot set the value, since that would
                        // enable the feature, which can only be done via configuration.
                        throw new InvalidOperationException(SR.GetString(SR.Queue_limit_is_zero, "maxConcurrentThreadsPerCPU"));
                    case HResults.E_INVALIDARG:
                        // The value must be greater than zero.  A value of zero would disable the feature, but this can only be done via configuration.
                        throw new ArgumentException(SR.GetString(SR.Invalid_queue_limit));
                }
            }
        }

        /// <summary>
        /// Returns the ASP.NET hosted domain.
        /// </summary>
        internal AppDomain HostedAppDomain {
            get {
                return AppDomain.CurrentDomain;
            }
        }

    }
}

