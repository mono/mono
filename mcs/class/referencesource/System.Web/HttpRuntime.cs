//------------------------------------------------------------------------------
// <copyright file="HttpRuntime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * The ASP.NET runtime services
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using Microsoft.Win32;

    /// <devdoc>
    ///    <para>Provides a set of ASP.NET runtime services.</para>
    /// </devdoc>
    public sealed class HttpRuntime {

        internal const string codegenDirName = "Temporary ASP.NET Files";
        internal const string profileFileName = "profileoptimization.prof";

        private static HttpRuntime _theRuntime;   // single instance of the class
        internal static byte[] s_autogenKeys = new byte[1024];

        //
        // Names of special ASP.NET directories
        //

        internal const string BinDirectoryName = "bin";
        internal const string CodeDirectoryName = "App_Code";
        internal const string WebRefDirectoryName = "App_WebReferences";
        internal const string ResourcesDirectoryName = "App_GlobalResources";
        internal const string LocalResourcesDirectoryName = "App_LocalResources";
        internal const string DataDirectoryName = "App_Data";
        internal const string ThemesDirectoryName = "App_Themes";
        internal const string GlobalThemesDirectoryName = "Themes";
        internal const string BrowsersDirectoryName = "App_Browsers";

        private static string DirectorySeparatorString = new string(Path.DirectorySeparatorChar, 1);
        private static string DoubleDirectorySeparatorString = new string(Path.DirectorySeparatorChar, 2);
        private static char[] s_InvalidPhysicalPathChars = { '/', '?', '*', '<', '>', '|', '"' };



#if OLD
        // For s_forbiddenDirs and s_forbiddenDirsConstant, see
        // ndll.h, and RestrictIISFolders in regiis.cxx

        internal static string[]    s_forbiddenDirs =   {
                                        BinDirectoryName,
                                        CodeDirectoryName,
                                        DataDirectoryName,
                                        ResourcesDirectoryName,
                                        WebRefDirectoryName,
                                    };

        internal static Int32[]     s_forbiddenDirsConstant = {
                                        UnsafeNativeMethods.RESTRICT_BIN,
                                        UnsafeNativeMethods.RESTRICT_CODE,
                                        UnsafeNativeMethods.RESTRICT_DATA,
                                        UnsafeNativeMethods.RESTRICT_RESOURCES,
                                        UnsafeNativeMethods.RESTRICT_WEBREFERENCES,
                                    };
#endif

        static HttpRuntime() {
            AddAppDomainTraceMessage("*HttpRuntime::cctor");

            StaticInit();

            _theRuntime = new HttpRuntime();

            _theRuntime.Init();

            AddAppDomainTraceMessage("HttpRuntime::cctor*");
        }

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        public HttpRuntime() {
        }

        //
        // static initialization to get hooked up to the unmanaged code
        // get installation directory, etc.
        //

        private static bool s_initialized = false;
        private static String s_installDirectory;
        private static bool s_isEngineLoaded = false;

        // Force the static initialization of this class.
        internal static void ForceStaticInit() { }

        private static void StaticInit() {
            if (s_initialized) {
                // already initialized
                return;
            }

            bool isEngineLoaded = false;
            bool wasEngineLoadedHere = false;
            String installDir = null;

            // load webengine.dll if it is not loaded already

#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features

            installDir = RuntimeEnvironment.GetRuntimeDirectory();

            if (UnsafeNativeMethods.GetModuleHandle(ModName.ENGINE_FULL_NAME) != IntPtr.Zero) {
                isEngineLoaded = true;
            }

            // Load webengine.dll if not loaded already

            if (!isEngineLoaded) {
                String fullPath = installDir + Path.DirectorySeparatorChar + ModName.ENGINE_FULL_NAME;

                if (UnsafeNativeMethods.LoadLibrary(fullPath) != IntPtr.Zero) {
                    isEngineLoaded = true;
                    wasEngineLoadedHere = true;
                }
            }

            if (isEngineLoaded) {
                UnsafeNativeMethods.InitializeLibrary(false);

                if (wasEngineLoadedHere) {
                    UnsafeNativeMethods.PerfCounterInitialize();
                }
            }

#else // !FEATURE_PAL
            string p = typeof(object).Module.FullyQualifiedName;
            installDir = Path.GetDirectoryName(p);
#endif // !FEATURE_PAL

            s_installDirectory = installDir;
            s_isEngineLoaded = isEngineLoaded;
            s_initialized = true;

            PopulateIISVersionInformation();

            AddAppDomainTraceMessage("Initialize");
        }

        //
        // Runtime services
        //

        private NamedPermissionSet _namedPermissionSet;
        private PolicyLevel _policyLevel;
        private string _hostSecurityPolicyResolverType = null;
        private FileChangesMonitor _fcm;
        private CacheInternal _cacheInternal;
        private Cache _cachePublic;
        private bool _isOnUNCShare;
        private Profiler _profiler;
        private RequestTimeoutManager _timeoutManager;
        private RequestQueue _requestQueue;
        private bool _apartmentThreading;

        private bool _processRequestInApplicationTrust;
        private bool _disableProcessRequestInApplicationTrust;
        private bool _isLegacyCas;
        //
        // Counters
        //

        private bool _beforeFirstRequest = true;
        private DateTime _firstRequestStartTime;
        private bool _firstRequestCompleted;
        private bool _userForcedShutdown;
        private bool _configInited;
        private bool _fusionInited;
        private int _activeRequestCount;
        private volatile bool _disposingHttpRuntime;
        private DateTime _lastShutdownAttemptTime;
        private bool _shutdownInProgress;
        private String _shutDownStack;
        private String _shutDownMessage;
        private ApplicationShutdownReason _shutdownReason = ApplicationShutdownReason.None;
        private string _trustLevel;
        private string _wpUserId;
        private bool _shutdownWebEventRaised;

        //
        // Header Newlines
        //
        private bool _enableHeaderChecking;

        //
        // Callbacks
        //

        private AsyncCallback _requestNotificationCompletionCallback;
        private AsyncCallback _handlerCompletionCallback;
        private HttpWorkerRequest.EndOfSendNotification _asyncEndOfSendCallback;
        private WaitCallback _appDomainUnloadallback;

        //
        // Initialization error (to be reported on subsequent requests)
        //

        private Exception _initializationError;
        private bool _hostingInitFailed; // make such errors non-sticky
        private Timer _appDomainShutdownTimer = null;


        //
        // App domain related
        //

        private String _tempDir;
        private String _codegenDir;
        private String _appDomainAppId;
        private String _appDomainAppPath;
        private VirtualPath _appDomainAppVPath;
        private String _appDomainId;

        //
        // Debugging support
        //

        private bool _debuggingEnabled = false;

        //
        // App_Offline.htm support
        //

        private const string AppOfflineFileName = "App_Offline.htm";
        private const long MaxAppOfflineFileLength = 1024 * 1024;
        private byte[] _appOfflineMessage;

        //
        // Client script support
        //

        private const string AspNetClientFilesSubDirectory = "asp.netclientfiles";
        private const string AspNetClientFilesParentVirtualPath = "/aspnet_client/system_web/";
        private string _clientScriptVirtualPath;
        private string _clientScriptPhysicalPath;

        //
        // IIS version and whether we're using the integrated pipeline
        //
        private static Version _iisVersion;
        private static bool _useIntegratedPipeline;

        //
        // Prefetch
        //
        private static bool _enablePrefetchOptimization;

        /////////////////////////////////////////////////////////////////////////
        // 3 steps of initialization:
        //     Init() is called from HttpRuntime cctor
        //     HostingInit() is called by the Hosting Environment
        //     FirstRequestInit() is called on first HTTP request
        //

        /*
         * Context-less initialization (on app domain creation)
         */
        private void Init() {
            try {
#if !FEATURE_PAL
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                    throw new PlatformNotSupportedException(SR.GetString(SR.RequiresNT));
#else // !FEATURE_PAL
                // ROTORTODO
                // Do nothing: FEATURE_PAL environment will always support ASP.NET hosting
#endif // !FEATURE_PAL

                _profiler = new Profiler();
                _timeoutManager = new RequestTimeoutManager();
                _wpUserId = GetCurrentUserName();

                _requestNotificationCompletionCallback = new AsyncCallback(this.OnRequestNotificationCompletion);
                _handlerCompletionCallback = new AsyncCallback(this.OnHandlerCompletion);
                _asyncEndOfSendCallback = new HttpWorkerRequest.EndOfSendNotification(this.EndOfSendCallback);
                _appDomainUnloadallback = new WaitCallback(this.ReleaseResourcesAndUnloadAppDomain);


                // appdomain values
                if (GetAppDomainString(".appDomain") != null) {

                    Debug.Assert(HostingEnvironment.IsHosted);

                    _appDomainAppId = GetAppDomainString(".appId");
                    _appDomainAppPath = GetAppDomainString(".appPath");
                    _appDomainAppVPath = VirtualPath.CreateNonRelativeTrailingSlash(GetAppDomainString(".appVPath"));
                    _appDomainId = GetAppDomainString(".domainId");

                    _isOnUNCShare = StringUtil.StringStartsWith(_appDomainAppPath, "\\\\");

                    // init perf counters for this appdomain
                    PerfCounters.Open(_appDomainAppId);
                }
                else {
                    Debug.Assert(!HostingEnvironment.IsHosted);
                }

                // _appDomainAppPath should be set before file change notifications are initialized
                // DevDiv 248126: Check httpRuntime fcnMode first before we use the registry key
                _fcm = new FileChangesMonitor(HostingEnvironment.FcnMode);
            }
            catch (Exception e) {
                // remember static initalization error
                InitializationException = e;
            }
        }

        private void SetUpDataDirectory() {

            // Set the DataDirectory (see VSWhidbey 226834) with permission (DevDiv 29614)
            string dataDirectory = Path.Combine(_appDomainAppPath, DataDirectoryName);
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory,
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dataDirectory));
        }

        private void DisposeAppDomainShutdownTimer() {
            Timer timer = _appDomainShutdownTimer;
            if (timer != null && Interlocked.CompareExchange(ref _appDomainShutdownTimer, null, timer) == timer) {
                timer.Dispose();
            }
        }

        private void AppDomainShutdownTimerCallback(Object state) {
            try {
                DisposeAppDomainShutdownTimer();
                ShutdownAppDomain(ApplicationShutdownReason.InitializationError, "Initialization Error");
            }
            catch { } // ignore exceptions
        }

        /*
         * Restart the AppDomain in 10 seconds
         */
        private void StartAppDomainShutdownTimer() {
            if (_appDomainShutdownTimer == null && !_shutdownInProgress) {
                lock (this) {
                    if (_appDomainShutdownTimer == null && !_shutdownInProgress) {
                        _appDomainShutdownTimer = new Timer(
                            new TimerCallback(this.AppDomainShutdownTimerCallback),
                            null,
                            10 * 1000,
                            0);
                    }
                }
            }
        }


        /*
         * Initialization from HostingEnvironment of HTTP independent features
         */
        private void HostingInit(HostingEnvironmentFlags hostingFlags, PolicyLevel policyLevel, Exception appDomainCreationException) {
            using (new ApplicationImpersonationContext()) {
                try {
                    // To ignore FCN during initialization
                    _firstRequestStartTime = DateTime.UtcNow;

                    SetUpDataDirectory();

                    // Throw an exception about lack of access to app directory early on
                    EnsureAccessToApplicationDirectory();

                    // Monitor renames to directories we are watching, and notifications on the bin directory
                    //
                    // Note that this must be the first monitoring that we do of the application directory.
                    // There is a bug in Windows 2000 Server where notifications on UNC shares do not
                    // happen correctly if:
                    //      1. the directory is monitored for regular notifications
                    //      2. the directory is then monitored for directory renames
                    //      3. the directory is monitored again for regular notifications
                    StartMonitoringDirectoryRenamesAndBinDirectory();

                    // Initialize ObjectCacheHost before config is read, since config relies on the cache
                    if (InitializationException == null) {
                        HostingEnvironment.InitializeObjectCacheHost();
                    }

                    //
                    // Get the configuration needed to minimally initialize
                    // the components required for a complete configuration system,
                    // especially SetTrustLevel.
                    //
                    // We want to do this before calling SetUpCodegenDirectory(),
                    // to remove the risk of the config system loading
                    // codegen assemblies in full trust (VSWhidbey 460506)
                    //
                    CacheSection cacheSection;
                    TrustSection trustSection;
                    SecurityPolicySection securityPolicySection;
                    CompilationSection compilationSection;
                    HostingEnvironmentSection hostingEnvironmentSection;
                    Exception configInitException;

                    GetInitConfigSections(
                            out cacheSection,
                            out trustSection,
                            out securityPolicySection,
                            out compilationSection,
                            out hostingEnvironmentSection,
                            out configInitException);

                    // Once the configuration system is initialized, we can read
                    // the cache configuration settings.
                    //
                    // Note that we must do this after we start monitoring directory renames,
                    // as reading config will cause file monitoring on the application directory
                    // to occur.
                    HttpRuntime.CacheInternal.ReadCacheInternalConfig(cacheSection);

                    // Set up the codegen directory for the app.  This needs to be done before we process
                    // the policy file, because it needs to replace the $CodeGen$ token.
                    SetUpCodegenDirectory(compilationSection);

                    if(compilationSection != null) {
                        _enablePrefetchOptimization = compilationSection.EnablePrefetchOptimization;
                        if(_enablePrefetchOptimization) {
                            UnsafeNativeMethods.StartPrefetchActivity((uint)StringUtil.GetStringHashCode(_appDomainAppId));
                        }
                    }

                    // NOTE: after calling SetUpCodegenDirectory(), and until we call SetTrustLevel(), we are at
                    // risk of codegen assemblies being loaded in full trust.  No code that might cause
                    // assembly loading should be added here! This is only valid if the legacyCasModel is set
                    // to true in <trust> section.

                    // Throw the original configuration exception from ApplicationManager if configuration is broken.
                    if (appDomainCreationException != null) {
                        throw appDomainCreationException;
                    }

                    if (trustSection == null || String.IsNullOrEmpty(trustSection.Level)) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_section_not_present, "trust"));
                    }

                    if (trustSection.LegacyCasModel) {
                        try {
                            _disableProcessRequestInApplicationTrust = false;
                            _isLegacyCas = true;
                            // Set code access policy on the app domain
                            SetTrustLevel(trustSection, securityPolicySection);
                        }
                        catch {
                            // throw the original config exception if it exists
                            if (configInitException != null)
                                throw configInitException;
                            throw;
                        }
                    }
                    else if ((hostingFlags & HostingEnvironmentFlags.ClientBuildManager) != 0) {
                        _trustLevel = "Full";
                    }
                    else {
                        _disableProcessRequestInApplicationTrust = true;
                        // Set code access policy properties of the runtime object
                        SetTrustParameters(trustSection, securityPolicySection, policyLevel);
                    }

                    // Configure fusion to use directories set in the app config
                    InitFusion(hostingEnvironmentSection);

                    // set the sliding expiration for URL metadata
                    CachedPathData.InitializeUrlMetadataSlidingExpiration(hostingEnvironmentSection);

                    // Complete initialization of configuration.
                    // Note that this needs to be called after SetTrustLevel,
                    // as it indicates that we have the permission set needed
                    // to correctly run configuration section handlers.
                    // As little config should be read before CompleteInit() as possible.
                    // No section that runs before CompleteInit() should demand permissions,
                    // as the permissions set has not yet determined until SetTrustLevel()
                    // is called.
                    HttpConfigurationSystem.CompleteInit();

                    //
                    // If an exception occurred loading configuration,
                    // we are now ready to handle exception processing
                    // with the correct trust level set.
                    //
                    if (configInitException != null) {
                        throw configInitException;
                    }

                    SetThreadPoolLimits();

                    SetAutogenKeys();

                    // Initialize the build manager
                    BuildManager.InitializeBuildManager();

                    if(compilationSection != null && compilationSection.ProfileGuidedOptimizations == ProfileGuidedOptimizationsFlags.All) {
                        ProfileOptimization.SetProfileRoot(_codegenDir);
                        ProfileOptimization.StartProfile(profileFileName);
                    }

                    // Determine apartment threading setting
                    InitApartmentThreading();

                    // Init debugging
                    InitDebuggingSupport();

                    _processRequestInApplicationTrust = trustSection.ProcessRequestInApplicationTrust;

                    // Init AppDomain Resource Perf Counters
                    AppDomainResourcePerfCounters.Init();


                    RelaxMapPathIfRequired();
                }
                catch (Exception e) {
                    _hostingInitFailed = true;
                    InitializationException = e;

                    Debug.Trace("AppDomainFactory", "HostingInit failed. " + e.ToString());

                    if ((hostingFlags & HostingEnvironmentFlags.ThrowHostingInitErrors) != 0)
                        throw;
                }
            }
        }

        internal static Exception InitializationException {
            get {
                return _theRuntime._initializationError;
            }

            // The exception is "cached" for 10 seconds, then the AppDomain is restarted.
            set {
                _theRuntime._initializationError = value;
                // In v2.0, we shutdown immediately if hostingInitFailed...so we don't need the timer
                if (!HostingInitFailed) {
                    _theRuntime.StartAppDomainShutdownTimer();
                }
            }
        }

        internal static bool HostingInitFailed {
            get {
                return _theRuntime._hostingInitFailed;
            }
        }

        internal static void InitializeHostingFeatures(HostingEnvironmentFlags hostingFlags, PolicyLevel policyLevel, Exception appDomainCreationException) {
            _theRuntime.HostingInit(hostingFlags, policyLevel, appDomainCreationException);
        }

        internal static bool EnableHeaderChecking {
            get {
                return _theRuntime._enableHeaderChecking;
            }
        }

        internal static bool ProcessRequestInApplicationTrust {
            get {
                return _theRuntime._processRequestInApplicationTrust;
            }
        }

        internal static bool DisableProcessRequestInApplicationTrust {
            get {
                return _theRuntime._disableProcessRequestInApplicationTrust;
            }
        }

        internal static bool IsLegacyCas {
            get {
                return _theRuntime._isLegacyCas;
            }
        }

        internal static byte[] AppOfflineMessage {
            get {
                return _theRuntime._appOfflineMessage;
            }
        }

        /*
         * Initialization on first request (context available)
         */
        private void FirstRequestInit(HttpContext context) {
            Exception error = null;

            if (InitializationException == null && _appDomainId != null) {
#if DBG
                HttpContext.SetDebugAssertOnAccessToCurrent(true);
#endif
                try {
                    using (new ApplicationImpersonationContext()) {
                        // Is this necessary?  See InitHttpConfiguration
                        CultureInfo savedCulture = Thread.CurrentThread.CurrentCulture;
                        CultureInfo savedUICulture = Thread.CurrentThread.CurrentUICulture;

                        try {
                            // Ensure config system is initialized
                            InitHttpConfiguration(); // be sure config system is set

                            // Check if applicaton is enabled
                            CheckApplicationEnabled();

                            // Check access to temp compilation directory (under hosting identity)
                            CheckAccessToTempDirectory();

                            // Initialize health monitoring
                            InitializeHealthMonitoring();

                            // Init request queue (after reading config)
                            InitRequestQueue();

                            // configure the profiler according to config
                            InitTrace(context);

                            // Start heatbeat for Web Event Health Monitoring
                            HealthMonitoringManager.StartHealthMonitoringHeartbeat();

                            // Remove read and browse access of the bin directory
                            RestrictIISFolders(context);

                            // Preload all assemblies from bin (only if required).  ASURT 114486
                            PreloadAssembliesFromBin();

                            // Decide whether or not to encode headers.  VsWhidbey 257154
                            InitHeaderEncoding();

                            // Force the current encoder + validator to load so that there's a deterministic
                            // place (here) for an exception to occur if there's a load error
                            HttpEncoder.InitializeOnFirstRequest();
                            RequestValidator.InitializeOnFirstRequest();

                            if (context.WorkerRequest is ISAPIWorkerRequestOutOfProc) {
                                // Make sure that the <processModel> section has no errors
                                ProcessModelSection processModel = RuntimeConfig.GetMachineConfig().ProcessModel;
                            }
                        }
                        finally {
                            Thread.CurrentThread.CurrentUICulture = savedUICulture;
                            SetCurrentThreadCultureWithAssert(savedCulture);
                        }
                    }
                }
                catch (ConfigurationException e) {
                    error = e;
                }
                catch (Exception e) {
                    // remember second-phase initialization error
                    error = new HttpException(SR.GetString(SR.XSP_init_error, e.Message), e);
                }
                finally {
#if DBG
                    HttpContext.SetDebugAssertOnAccessToCurrent(false);
#endif
                }
            }

            if (InitializationException != null) {
                // throw cached exception.  We need to wrap it in a new exception, otherwise
                // we lose the original stack.
                throw new HttpException(InitializationException.Message, InitializationException);
            }
            else if (error != null) {
                InitializationException = error;
                // throw new exception
                throw error;
            }

            AddAppDomainTraceMessage("FirstRequestInit");
        }

        [SecurityPermission(SecurityAction.Assert, ControlThread = true)]
        internal static void SetCurrentThreadCultureWithAssert(CultureInfo cultureInfo) {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        private void EnsureFirstRequestInit(HttpContext context) {
            if (_beforeFirstRequest) {
                lock (this) {
                    if (_beforeFirstRequest) {
                        _firstRequestStartTime = DateTime.UtcNow;
                        FirstRequestInit(context);
                        _beforeFirstRequest = false;
                        context.FirstRequest = true;
                    }
                }
            }
        }

        private void EnsureAccessToApplicationDirectory() {
            if (!FileUtil.DirectoryAccessible(_appDomainAppPath)) {
                // 
                if (_appDomainAppPath.IndexOf('?') >= 0) {
                    // Possible Unicode when not supported
                    throw new HttpException(SR.GetString(SR.Access_denied_to_unicode_app_dir, _appDomainAppPath));
                }
                else {
                    throw new HttpException(SR.GetString(SR.Access_denied_to_app_dir, _appDomainAppPath));
                }
            }
        }

        private void StartMonitoringDirectoryRenamesAndBinDirectory() {
            _fcm.StartMonitoringDirectoryRenamesAndBinDirectory(AppDomainAppPathInternal, new FileChangeEventHandler(this.OnCriticalDirectoryChange));
        }

        //
        // Monitor a local resources subdirectory and unload appdomain when it changes
        //
        internal static void StartListeningToLocalResourcesDirectory(VirtualPath virtualDir) {
#if !FEATURE_PAL // FEATURE_PAL does not enable file change notification
            _theRuntime._fcm.StartListeningToLocalResourcesDirectory(virtualDir);
#endif // !FEATURE_PAL
        }

        //
        // Get the configuration needed to minimally initialize
        // the components required for a complete configuration system,
        //
        // Note that if the application configuration file has an error,
        // AppLKGConfig will still retreive any valid configuration from
        // that file, or from location directives that apply to the
        // application path. This implies that an administrator can
        // lock down an application's trust level in root web.config,
        // and it will still take effect if the application's web.config
        // has errors.
        //
        private void GetInitConfigSections(
                out CacheSection cacheSection,
                out TrustSection trustSection,
                out SecurityPolicySection securityPolicySection,
                out CompilationSection compilationSection,
                out HostingEnvironmentSection hostingEnvironmentSection,
                out Exception initException) {

            cacheSection = null;
            trustSection = null;
            securityPolicySection = null;
            compilationSection = null;
            hostingEnvironmentSection = null;
            initException = null;

            // AppLKGConfig is guaranteed to not throw an exception.
            RuntimeConfig appLKGConfig = RuntimeConfig.GetAppLKGConfig();

            // AppConfig may throw an exception.
            RuntimeConfig appConfig = null;
            try {
                appConfig = RuntimeConfig.GetAppConfig();
            }
            catch (Exception e) {
                initException = e;
            }

            // Cache section
            if (appConfig != null) {
                try {
                    cacheSection = appConfig.Cache;
                }
                catch (Exception e) {
                    if (initException == null) {
                        initException = e;
                    }
                }
            }

            if (cacheSection == null) {
                cacheSection = appLKGConfig.Cache;
            }

            // Trust section
            if (appConfig != null) {
                try {
                    trustSection = appConfig.Trust;
                }
                catch (Exception e) {
                    if (initException == null) {
                        initException = e;
                    }
                }
            }

            if (trustSection == null) {
                trustSection = appLKGConfig.Trust;
            }

            // SecurityPolicy section
            if (appConfig != null) {
                try {
                    securityPolicySection = appConfig.SecurityPolicy;
                }
                catch (Exception e) {
                    if (initException == null) {
                        initException = e;
                    }
                }
            }

            if (securityPolicySection == null) {
                securityPolicySection = appLKGConfig.SecurityPolicy;
            }

            // Compilation section
            if (appConfig != null) {
                try {
                    compilationSection = appConfig.Compilation;
                }
                catch (Exception e) {
                    if (initException == null) {
                        initException = e;
                    }
                }
            }

            if (compilationSection == null) {
                compilationSection = appLKGConfig.Compilation;
            }

            // HostingEnvironment section
            if (appConfig != null) {
                try {
                    hostingEnvironmentSection = appConfig.HostingEnvironment;
                }
                catch (Exception e) {
                    if (initException == null) {
                        initException = e;
                    }
                }
            }

            if (hostingEnvironmentSection == null) {
                hostingEnvironmentSection = appLKGConfig.HostingEnvironment;
            }
        }

        // Set up the codegen directory for the app
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This call site is trusted.")]
        private void SetUpCodegenDirectory(CompilationSection compilationSection) {
            AppDomain appDomain = Thread.GetDomain();

            string codegenBase;

            string simpleAppName = System.Web.Hosting.AppManagerAppDomainFactory.ConstructSimpleAppName(
                AppDomainAppVirtualPath);

            string tempDirectory = null;

            // These variables are used for error handling
            string tempDirAttribName = null;
            string configFileName = null;
            int configLineNumber = 0;

            if (compilationSection != null && !String.IsNullOrEmpty(compilationSection.TempDirectory)) {
                tempDirectory = compilationSection.TempDirectory;

                compilationSection.GetTempDirectoryErrorInfo(out tempDirAttribName,
                    out configFileName, out configLineNumber);
            }

            if (tempDirectory != null) {
                tempDirectory = tempDirectory.Trim();

                if (!Path.IsPathRooted(tempDirectory)) {
                    // Make sure the path is not relative (VSWhidbey 260075)
                    tempDirectory = null;
                }
                else {
                    try {
                        // Canonicalize it to avoid problems with spaces (VSWhidbey 229873)
                        tempDirectory = new DirectoryInfo(tempDirectory).FullName;
                    }
                    catch {
                        tempDirectory = null;
                    }
                }

                if (tempDirectory == null) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_temp_directory, tempDirAttribName),
                        configFileName, configLineNumber);
                }
#if FEATURE_PAL
            } else {
                System.UInt32 length = 0;
                StringBuilder sb = null;
                bool bRet;

                // Get the required length
                bRet = UnsafeNativeMethods.GetUserTempDirectory(
                                    UnsafeNativeMethods.DeploymentDirectoryType.ddtInstallationDependentDirectory,
                                    null, ref length);

                if (true == bRet) {
                    // now, allocate the string
                    sb = new StringBuilder ((int)length);

                    // call again to get the value
                    bRet = UnsafeNativeMethods.GetUserTempDirectory(
                                    UnsafeNativeMethods.DeploymentDirectoryType.ddtInstallationDependentDirectory,
                                    sb, ref length);
                }

                if (false == bRet) {
                    throw new ConfigurationException(
                        HttpRuntime.FormatResourceString(SR.Invalid_temp_directory, tempDirAttribName));
                }

                tempDirectory = Path.Combine(sb.ToString(), codegenDirName);
            }

            // Always try to create the ASP.Net temp directory for FEATURE_PAL
#endif // FEATURE_PAL

                // Create the config-specified directory if needed
                try {
                    Directory.CreateDirectory(tempDirectory);
                }
                catch (Exception e) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_temp_directory, tempDirAttribName),
                        e,
                        configFileName, configLineNumber);
                }
#if !FEATURE_PAL
            }
            else {
                tempDirectory = Path.Combine(s_installDirectory, codegenDirName);
            }
#endif // !FEATURE_PAL

            // If we don't have write access to the codegen dir, use the TEMP dir instead.
            // This will allow non-admin users to work in hosting scenarios (e.g. Venus, aspnet_compiler)
            if (!System.Web.UI.Util.HasWriteAccessToDirectory(tempDirectory)) {

                // Don't do this if we are not in a CBM scenario and we're in a service (!UserInteractive), 
                // as TEMP could point to unwanted places.

#if !FEATURE_PAL // always fail here
                if ((!BuildManagerHost.InClientBuildManager) && (!Environment.UserInteractive))
#endif // !FEATURE_PAL
                {
                    throw new HttpException(SR.GetString(SR.No_codegen_access,
                        System.Web.UI.Util.GetCurrentAccountName(), tempDirectory));
                }

                tempDirectory = Path.GetTempPath();
                Debug.Assert(System.Web.UI.Util.HasWriteAccessToDirectory(tempDirectory));
                tempDirectory = Path.Combine(tempDirectory, codegenDirName);
            }

            _tempDir = tempDirectory;

            codegenBase = Path.Combine(tempDirectory, simpleAppName);

#pragma warning disable 0618    // To avoid deprecation warning
            appDomain.SetDynamicBase(codegenBase);
#pragma warning restore 0618

            _codegenDir = Thread.GetDomain().DynamicDirectory;

            // Create the codegen directory if needed
            Directory.CreateDirectory(_codegenDir);
        }

        private void InitFusion(HostingEnvironmentSection hostingEnvironmentSection) {

            AppDomain appDomain = Thread.GetDomain();

            // If there is a double backslash in the string, get rid of it (ASURT 122191)
            // Make sure to skip the first char, to avoid breaking the UNC case
            string appDomainAppPath = _appDomainAppPath;
            if (appDomainAppPath.IndexOf(DoubleDirectorySeparatorString, 1, StringComparison.Ordinal) >= 1) {
                appDomainAppPath = appDomainAppPath[0] + appDomainAppPath.Substring(1).Replace(DoubleDirectorySeparatorString,
                    DirectorySeparatorString);
            }

#pragma warning disable 0618    // To avoid deprecation warning
            // Allow assemblies from 'bin' to be loaded
            appDomain.AppendPrivatePath(appDomainAppPath + BinDirectoryName);
#pragma warning restore 0618

            // If shadow copying was disabled via config, turn it off (DevDiv 30864)
            if (hostingEnvironmentSection != null && !hostingEnvironmentSection.ShadowCopyBinAssemblies) {
#pragma warning disable 0618    // To avoid deprecation warning
                appDomain.ClearShadowCopyPath();
#pragma warning restore 0618
            }
            else {
                // enable shadow-copying from bin
#pragma warning disable 0618    // To avoid deprecation warning
                appDomain.SetShadowCopyPath(appDomainAppPath + BinDirectoryName);
#pragma warning restore 0618
            }

            // Get rid of the last part of the directory (the app name), since it will
            // be re-appended.
            string parentDir = Directory.GetParent(_codegenDir).FullName;
#pragma warning disable 0618    // To avoid deprecation warning
            appDomain.SetCachePath(parentDir);
#pragma warning restore 0618

            _fusionInited = true;
        }

        private void InitRequestQueue() {
            RuntimeConfig config = RuntimeConfig.GetAppConfig();
            HttpRuntimeSection runtimeConfig = config.HttpRuntime;
            ProcessModelSection processConfig = config.ProcessModel;

            if (processConfig.AutoConfig) {
                _requestQueue = new RequestQueue(
                    88 * processConfig.CpuCount,
                    76 * processConfig.CpuCount,
                    runtimeConfig.AppRequestQueueLimit,
                    processConfig.ClientConnectedCheck);
            }
            else {

                // Configuration section handlers cannot validate values based on values
                // in other configuration sections, so we validate minFreeThreads and
                // minLocalRequestFreeThreads here.
                int maxThreads = (processConfig.MaxWorkerThreadsTimesCpuCount < processConfig.MaxIoThreadsTimesCpuCount) ? processConfig.MaxWorkerThreadsTimesCpuCount : processConfig.MaxIoThreadsTimesCpuCount;
                // validate minFreeThreads
                if (runtimeConfig.MinFreeThreads >= maxThreads) {
                    if (runtimeConfig.ElementInformation.Properties["minFreeThreads"].LineNumber == 0) {
                        if (processConfig.ElementInformation.Properties["maxWorkerThreads"].LineNumber != 0) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Thread_pool_limit_must_be_greater_than_minFreeThreads, runtimeConfig.MinFreeThreads.ToString(CultureInfo.InvariantCulture)),
                                                                   processConfig.ElementInformation.Properties["maxWorkerThreads"].Source,
                                                                   processConfig.ElementInformation.Properties["maxWorkerThreads"].LineNumber);
                        }
                        else {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Thread_pool_limit_must_be_greater_than_minFreeThreads, runtimeConfig.MinFreeThreads.ToString(CultureInfo.InvariantCulture)),
                                                                   processConfig.ElementInformation.Properties["maxIoThreads"].Source,
                                                                   processConfig.ElementInformation.Properties["maxIoThreads"].LineNumber);
                        }
                    }
                    else {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Min_free_threads_must_be_under_thread_pool_limits, maxThreads.ToString(CultureInfo.InvariantCulture)),
                                                               runtimeConfig.ElementInformation.Properties["minFreeThreads"].Source,
                                                               runtimeConfig.ElementInformation.Properties["minFreeThreads"].LineNumber);
                    }
                }
                // validate minLocalRequestFreeThreads
                if (runtimeConfig.MinLocalRequestFreeThreads > runtimeConfig.MinFreeThreads) {
                    if (runtimeConfig.ElementInformation.Properties["minLocalRequestFreeThreads"].LineNumber == 0) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Local_free_threads_cannot_exceed_free_threads),
                                                               processConfig.ElementInformation.Properties["minFreeThreads"].Source,
                                                               processConfig.ElementInformation.Properties["minFreeThreads"].LineNumber);
                    }
                    else {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Local_free_threads_cannot_exceed_free_threads),
                                                               runtimeConfig.ElementInformation.Properties["minLocalRequestFreeThreads"].Source,
                                                               runtimeConfig.ElementInformation.Properties["minLocalRequestFreeThreads"].LineNumber);
                    }
                }

                _requestQueue = new RequestQueue(
                    runtimeConfig.MinFreeThreads,
                    runtimeConfig.MinLocalRequestFreeThreads,
                    runtimeConfig.AppRequestQueueLimit,
                    processConfig.ClientConnectedCheck);
            }
        }

        private void InitApartmentThreading() {
            HttpRuntimeSection runtimeConfig = RuntimeConfig.GetAppConfig().HttpRuntime;

            if (runtimeConfig != null) {
                _apartmentThreading = runtimeConfig.ApartmentThreading;
            }
            else {
                _apartmentThreading = false;
            }
        }

        private void InitTrace(HttpContext context) {
            TraceSection traceConfig = RuntimeConfig.GetAppConfig().Trace;

            Profile.RequestsToProfile = traceConfig.RequestLimit;
            Profile.PageOutput = traceConfig.PageOutput;
            Profile.OutputMode = TraceMode.SortByTime;
            if (traceConfig.TraceMode == TraceDisplayMode.SortByCategory)
                Profile.OutputMode = TraceMode.SortByCategory;

            Profile.LocalOnly = traceConfig.LocalOnly;
            Profile.IsEnabled = traceConfig.Enabled;
            Profile.MostRecent = traceConfig.MostRecent;
            Profile.Reset();

            // the first request's context is created before InitTrace, so
            // we need to set this manually. (ASURT 93730)
            context.TraceIsEnabled = traceConfig.Enabled;
            TraceContext.SetWriteToDiagnosticsTrace(traceConfig.WriteToDiagnosticsTrace);
        }

        private void InitDebuggingSupport() {
            CompilationSection compConfig = RuntimeConfig.GetAppConfig().Compilation;
            _debuggingEnabled = compConfig.Debug;
        }

        /*
         * Pre-load all the bin assemblies if we're impersonated.  This way, if user code
         * calls Assembly.Load while impersonated, the assembly will already be loaded, and
         * we won't fail due to lack of permissions on the codegen dir (see ASURT 114486)
         */
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private void PreloadAssembliesFromBin() {
            bool appClientImpersonationEnabled = false;

            if (!_isOnUNCShare) {
                // if not on UNC share check if config has impersonation enabled (without userName)
                IdentitySection c = RuntimeConfig.GetAppConfig().Identity;
                if (c.Impersonate && c.ImpersonateToken == IntPtr.Zero)
                    appClientImpersonationEnabled = true;
            }

            if (!appClientImpersonationEnabled)
                return;

            // Get the path to the bin directory
            string binPath = HttpRuntime.BinDirectoryInternal;

            DirectoryInfo binPathDirectory = new DirectoryInfo(binPath);

            if (!binPathDirectory.Exists)
                return;

            PreloadAssembliesFromBinRecursive(binPathDirectory);
        }

        private void PreloadAssembliesFromBinRecursive(DirectoryInfo dirInfo) {

            FileInfo[] binDlls = dirInfo.GetFiles("*.dll");

            // Pre-load all the assemblies, ignoring all exceptions
            foreach (FileInfo fi in binDlls) {
                try { Assembly.Load(System.Web.UI.Util.GetAssemblyNameFromFileName(fi.Name)); }
                catch (FileNotFoundException) {
                    // If Load failed, try LoadFrom (VSWhidbey 493725)
                    try { Assembly.LoadFrom(fi.FullName); }
                    catch { }
                }
                catch { }
            }

            // Recurse on the subdirectories
            DirectoryInfo[] subDirs = dirInfo.GetDirectories();
            foreach (DirectoryInfo di in subDirs) {
                PreloadAssembliesFromBinRecursive(di);
            }
        }

        private void SetAutoConfigLimits(ProcessModelSection pmConfig) {
            // check if the current limits are ok
            int workerMax, ioMax;
            ThreadPool.GetMaxThreads(out workerMax, out ioMax);

            // only set if different
            if (pmConfig.DefaultMaxWorkerThreadsForAutoConfig != workerMax || pmConfig.DefaultMaxIoThreadsForAutoConfig != ioMax) {
                Debug.Trace("ThreadPool", "SetThreadLimit: from " + workerMax + "," + ioMax + " to " + pmConfig.DefaultMaxWorkerThreadsForAutoConfig + "," + pmConfig.DefaultMaxIoThreadsForAutoConfig);
                UnsafeNativeMethods.SetClrThreadPoolLimits(pmConfig.DefaultMaxWorkerThreadsForAutoConfig, pmConfig.DefaultMaxIoThreadsForAutoConfig, true);
            }

            // this is the code equivalent of setting maxconnection
            // Dev11 141729: Make autoConfig scale by default
            // Dev11 144842: PERF: Consider removing Max connection limit or changing the default value
            System.Net.ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

            // we call InitRequestQueue later, from FirstRequestInit, and set minFreeThreads and minLocalRequestFreeThreads
        }

        private void SetThreadPoolLimits() {
            try {
                ProcessModelSection pmConfig = RuntimeConfig.GetMachineConfig().ProcessModel;

                if (pmConfig.AutoConfig) {
                    // use recommendation in http://support.microsoft.com/?id=821268
                    SetAutoConfigLimits(pmConfig);
                }
                else if (pmConfig.MaxWorkerThreadsTimesCpuCount > 0 && pmConfig.MaxIoThreadsTimesCpuCount > 0) {
                    // check if the current limits are ok
                    int workerMax, ioMax;
                    ThreadPool.GetMaxThreads(out workerMax, out ioMax);

                    // only set if different
                    if (pmConfig.MaxWorkerThreadsTimesCpuCount != workerMax || pmConfig.MaxIoThreadsTimesCpuCount != ioMax) {
                        Debug.Trace("ThreadPool", "SetThreadLimit: from " + workerMax + "," + ioMax + " to " + pmConfig.MaxWorkerThreadsTimesCpuCount + "," + pmConfig.MaxIoThreadsTimesCpuCount);
                        UnsafeNativeMethods.SetClrThreadPoolLimits(pmConfig.MaxWorkerThreadsTimesCpuCount, pmConfig.MaxIoThreadsTimesCpuCount, false);
                    }
                }

                if (pmConfig.MinWorkerThreadsTimesCpuCount > 0 || pmConfig.MinIoThreadsTimesCpuCount > 0) {
                    int currentMinWorkerThreads, currentMinIoThreads;
                    ThreadPool.GetMinThreads(out currentMinWorkerThreads, out currentMinIoThreads);

                    int newMinWorkerThreads = pmConfig.MinWorkerThreadsTimesCpuCount > 0 ? pmConfig.MinWorkerThreadsTimesCpuCount : currentMinWorkerThreads;
                    int newMinIoThreads = pmConfig.MinIoThreadsTimesCpuCount > 0 ? pmConfig.MinIoThreadsTimesCpuCount : currentMinIoThreads;

                    if (newMinWorkerThreads > 0 && newMinIoThreads > 0
                        && (newMinWorkerThreads != currentMinWorkerThreads || newMinIoThreads != currentMinIoThreads))
                        ThreadPool.SetMinThreads(newMinWorkerThreads, newMinIoThreads);
                }
            }
            catch {
            }
        }

        internal static void CheckApplicationEnabled() {
            // process App_Offline.htm file
            string appOfflineFile = Path.Combine(_theRuntime._appDomainAppPath, AppOfflineFileName);
            bool appOfflineFileFound = false;

            // monitor even if doesn't exist
            _theRuntime._fcm.StartMonitoringFile(appOfflineFile, new FileChangeEventHandler(_theRuntime.OnAppOfflineFileChange));

            // read the file into memory
            try {
                if (File.Exists(appOfflineFile)) {
                    Debug.Trace("AppOffline", "File " + appOfflineFile + " exists. Using it.");

                    using (FileStream fs = new FileStream(appOfflineFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        if (fs.Length <= MaxAppOfflineFileLength) {
                            int length = (int)fs.Length;

                            if (length > 0) {
                                byte[] message = new byte[length];

                                if (fs.Read(message, 0, length) == length) {
                                    // remember the message
                                    _theRuntime._appOfflineMessage = message;
                                    appOfflineFileFound = true;
                                }
                            }
                            else {
                                // empty file
                                appOfflineFileFound = true;
                                _theRuntime._appOfflineMessage = new byte[0];
                            }
                        }
                    }
                }
            }
            catch {
                // ignore any IO errors reading the file
            }

            // throw if there is a valid App_Offline file
            if (appOfflineFileFound) {
                throw new HttpException(503, String.Empty);
            }

            // process the config setting
            HttpRuntimeSection runtimeConfig = RuntimeConfig.GetAppConfig().HttpRuntime;
            if (!runtimeConfig.Enable) {
                // throw 404 on first request init -- this will get cached until config changes
                throw new HttpException(404, String.Empty);
            }
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        private void CheckAccessToTempDirectory() {
            // The original check (in HostingInit) was done under process identity
            // this time we do it under hosting identity
            if (HostingEnvironment.HasHostingIdentity) {
                using (new ApplicationImpersonationContext()) {
                    if (!System.Web.UI.Util.HasWriteAccessToDirectory(_tempDir)) {
                        throw new HttpException(SR.GetString(SR.No_codegen_access,
                            System.Web.UI.Util.GetCurrentAccountName(), _tempDir));
                    }
                }
            }
        }

        private void InitializeHealthMonitoring() {
#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            ProcessModelSection pmConfig = RuntimeConfig.GetMachineConfig().ProcessModel;
            int deadLockInterval = (int)pmConfig.ResponseDeadlockInterval.TotalSeconds;
            int requestQueueLimit = pmConfig.RequestQueueLimit;
            Debug.Trace("HealthMonitor", "Initalizing: ResponseDeadlockInterval=" + deadLockInterval);
            UnsafeNativeMethods.InitializeHealthMonitor(deadLockInterval, requestQueueLimit);
#endif // !FEATURE_PAL
        }

        private static void InitHttpConfiguration() {
            if (!_theRuntime._configInited) {
                _theRuntime._configInited = true;

                HttpConfigurationSystem.EnsureInit(null, true, true);

                // whenever possible report errors in the user's culture (from machine.config)
                // Note: this thread's culture is saved/restored during FirstRequestInit, so this is safe
                // see ASURT 81655

                GlobalizationSection globConfig = RuntimeConfig.GetAppLKGConfig().Globalization;
                if (globConfig != null) {
                    if (!String.IsNullOrEmpty(globConfig.Culture) &&
                        !StringUtil.StringStartsWithIgnoreCase(globConfig.Culture, "auto"))
                        SetCurrentThreadCultureWithAssert(HttpServerUtility.CreateReadOnlyCultureInfo(globConfig.Culture));

                    if (!String.IsNullOrEmpty(globConfig.UICulture) &&
                        !StringUtil.StringStartsWithIgnoreCase(globConfig.UICulture, "auto"))
                        Thread.CurrentThread.CurrentUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(globConfig.UICulture);
                }

                // check for errors in <processModel> section
                RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
                object section = appConfig.ProcessModel;
                // check for errors in <hostingEnvironment> section
                section = appConfig.HostingEnvironment;
            }
        }

        private void InitHeaderEncoding() {
            HttpRuntimeSection runtimeConfig = RuntimeConfig.GetAppConfig().HttpRuntime;
            _enableHeaderChecking = runtimeConfig.EnableHeaderChecking;
        }

        private static void SetAutogenKeys() {
#if !FEATURE_PAL // FEATURE_PAL does not enable cryptography
            byte[] bKeysRandom = new byte[s_autogenKeys.Length];
            byte[] bKeysStored = new byte[s_autogenKeys.Length];
            bool fGetStoredKeys = false;
            RNGCryptoServiceProvider randgen = new RNGCryptoServiceProvider();

            // Gernerate random keys
            randgen.GetBytes(bKeysRandom);

            // If getting stored keys via WorkerRequest object failed, get it directly
            if (!fGetStoredKeys)
                fGetStoredKeys = (UnsafeNativeMethods.EcbCallISAPI(IntPtr.Zero, UnsafeNativeMethods.CallISAPIFunc.GetAutogenKeys,
                                                                   bKeysRandom, bKeysRandom.Length, bKeysStored, bKeysStored.Length) == 1);

            // If we managed to get stored keys, copy them in; else use random keys
            if (fGetStoredKeys)
                Buffer.BlockCopy(bKeysStored, 0, s_autogenKeys, 0, s_autogenKeys.Length);
            else
                Buffer.BlockCopy(bKeysRandom, 0, s_autogenKeys, 0, s_autogenKeys.Length);
#endif // !FEATURE_PAL
        }

        internal static void IncrementActivePipelineCount() {
            Interlocked.Increment(ref _theRuntime._activeRequestCount);
            HostingEnvironment.IncrementBusyCount();
        }

        internal static void DecrementActivePipelineCount() {
            HostingEnvironment.DecrementBusyCount();
            Interlocked.Decrement(ref _theRuntime._activeRequestCount);
        }

        internal static void PopulateIISVersionInformation() {
            if (IsEngineLoaded) {
                uint dwVersion;
                bool fIsIntegratedMode;
                UnsafeIISMethods.MgdGetIISVersionInformation(out dwVersion, out fIsIntegratedMode);

                if (dwVersion != 0) {
                    // High word is the major version; low word is the minor version (this is MAKELONG format)
                    _iisVersion = new Version((int)(dwVersion >> 16), (int)(dwVersion & 0xffff));
                    _useIntegratedPipeline = fIsIntegratedMode;
                }
            }
        }

        // Gets the version of IIS (7.0, 7.5, 8.0, etc.) that is hosting this application, or null if this application isn't IIS-hosted.
        // Should also return the correct version for IIS Express.
        public static Version IISVersion {
            get {
                return _iisVersion;
            }
        }

        // DevDivBugs 190952: public method for querying runtime pipeline mode
        public static bool UsingIntegratedPipeline {
            get {
                return UseIntegratedPipeline;
            }
        }

        internal static bool UseIntegratedPipeline {
             [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get {
                return _useIntegratedPipeline;
            }
        }

        internal static bool EnablePrefetchOptimization {
            get {
                return _enablePrefetchOptimization;
            }
        }

        /*
         * Process one step of the integrated pipeline
         *
         */

        internal static RequestNotificationStatus ProcessRequestNotification(IIS7WorkerRequest wr, HttpContext context)
        {
            return _theRuntime.ProcessRequestNotificationPrivate(wr, context);
        }

        private RequestNotificationStatus ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context) {
            RequestNotificationStatus status = RequestNotificationStatus.Pending;
            try {
                int currentModuleIndex;
                bool isPostNotification;
                int currentNotification;

                // setup the HttpContext for this event/module combo
                UnsafeIISMethods.MgdGetCurrentNotificationInfo(wr.RequestContext, out currentModuleIndex, out isPostNotification, out currentNotification);

                context.CurrentModuleIndex = currentModuleIndex;
                context.IsPostNotification = isPostNotification;
                context.CurrentNotification = (RequestNotification) currentNotification;
#if DBG
                Debug.Trace("PipelineRuntime", "HttpRuntime::ProcessRequestNotificationPrivate: notification=" + context.CurrentNotification.ToString()
                            + ", isPost=" + context.IsPostNotification
                            + ", moduleIndex=" + context.CurrentModuleIndex);
#endif

                IHttpHandler handler = null;
                if (context.NeedToInitializeApp()) {
#if DBG
                    Debug.Trace("FileChangesMonitorIgnoreSubdirChange",
                                "*** FirstNotification " + DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture)
                                + ": _appDomainAppId=" + _appDomainAppId);
#endif
                    // First request initialization
                    try {
                        EnsureFirstRequestInit(context);
                    }
                    catch {
                        // If we are handling a DEBUG request, ignore the FirstRequestInit exception.
                        // This allows the HttpDebugHandler to execute, and lets the debugger attach to
                        // the process (VSWhidbey 358135)
                        if (!context.Request.IsDebuggingRequest) {
                            throw;
                        }
                    }

                    context.Response.InitResponseWriter();
                    handler = HttpApplicationFactory.GetApplicationInstance(context);
                    if (handler == null)
                        throw new HttpException(SR.GetString(SR.Unable_create_app_object));

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_START_HANDLER, context.WorkerRequest, handler.GetType().FullName, "Start");

                    HttpApplication app = handler as HttpApplication;
                    if (app != null) {
                        // associate the context with an application instance
                        app.AssignContext(context);
                    }
                }

                // this may throw, and should be called after app initialization
                wr.SynchronizeVariables(context);

                if (context.ApplicationInstance != null) {
                    // process request
                    IAsyncResult ar = context.ApplicationInstance.BeginProcessRequestNotification(context, _requestNotificationCompletionCallback);

                    if (ar.CompletedSynchronously) {
                        status = RequestNotificationStatus.Continue;
                    }
                }
                else if (handler != null) {
                    // HttpDebugHandler is processed here
                    handler.ProcessRequest(context);
                    status = RequestNotificationStatus.FinishRequest;
                }
                else {
                    status = RequestNotificationStatus.Continue;
                }
            }
            catch (Exception e) {
                status = RequestNotificationStatus.FinishRequest;
                context.Response.InitResponseWriter();
                // errors are handled in HttpRuntime::FinishRequestNotification
                context.AddError(e);
            }

            if (status != RequestNotificationStatus.Pending) {
                // we completed synchronously
                FinishRequestNotification(wr, context, ref status);
            }

#if DBG
            Debug.Trace("PipelineRuntime", "HttpRuntime::ProcessRequestNotificationPrivate: status=" + status.ToString());
#endif

            return status;
        }

        private void FinishRequestNotification(IIS7WorkerRequest wr, HttpContext context, ref RequestNotificationStatus status) {

            Debug.Assert(status != RequestNotificationStatus.Pending, "status != RequestNotificationStatus.Pending");

            HttpApplication app = context.ApplicationInstance;

            if (context.NotificationContext.RequestCompleted) {
                status = RequestNotificationStatus.FinishRequest;
            }

            // check if the app offline or whether an error has occurred, and report the condition
            context.ReportRuntimeErrorIfExists(ref status);

            // we do not return FinishRequest for LogRequest or EndRequest
            if (status == RequestNotificationStatus.FinishRequest
                && (context.CurrentNotification == RequestNotification.LogRequest
                    || context.CurrentNotification == RequestNotification.EndRequest)) {
                status = RequestNotificationStatus.Continue;
            }

            IntPtr requestContext = wr.RequestContext;
            bool sendHeaders = UnsafeIISMethods.MgdIsLastNotification(requestContext, status);
            try {
                context.Response.UpdateNativeResponse(sendHeaders);
            }
            catch(Exception e) {
                // if we catch an exception here then
                // i) clear cached response body bytes on the worker request
                // ii) clear the managed headers, the IIS native headers, the mangaged httpwriter response buffers, and the native IIS response buffers
                // iii) attempt to format the exception and write it to the response
                wr.UnlockCachedResponseBytes();
                context.AddError(e);
                context.ReportRuntimeErrorIfExists(ref status);
                try {
                    context.Response.UpdateNativeResponse(sendHeaders);
                }
                catch {
                }
            }

            if (sendHeaders) {
                context.FinishPipelineRequest();
            }

            // Perf optimization: dispose managed context if possible (no need to try if status is pending)
            if (status != RequestNotificationStatus.Pending) {
                PipelineRuntime.DisposeHandler(context, requestContext, status);
            }
        }

        internal static void FinishPipelineRequest(HttpContext context) {
            // Remember that first request is done
            _theRuntime._firstRequestCompleted = true;

            // need to raise OnRequestCompleted while within the ThreadContext so that things like User, CurrentCulture, etc. are available
            context.RaiseOnRequestCompleted();

            context.Request.Dispose();
            context.Response.Dispose();
            HttpApplication app = context.ApplicationInstance;
            if(null != app) {
                ThreadContext threadContext = context.IndicateCompletionContext;
                if (threadContext != null) {
                    if (!threadContext.HasBeenDisassociatedFromThread) {
                        lock (threadContext) {
                            if (!threadContext.HasBeenDisassociatedFromThread) {
                                threadContext.DisassociateFromCurrentThread();
                                context.IndicateCompletionContext = null;
                                context.InIndicateCompletion = false;
                            }
                        }
                    }
                }
                app.ReleaseAppInstance();
            }

            SetExecutionTimePerformanceCounter(context);
            UpdatePerfCounters(context.Response.StatusCode);
            if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_END_HANDLER, context.WorkerRequest);

            // In case of a HostingInit() error, app domain should not stick around
            if (HostingInitFailed) {
                Debug.Trace("AppDomainFactory", "Shutting down appdomain because of HostingInit error");
                ShutdownAppDomain(ApplicationShutdownReason.HostingEnvironment, "HostingInit error");
            }
        }


        /*
         * Process one request
         */
        private void ProcessRequestInternal(HttpWorkerRequest wr) {
            // Count active requests
            Interlocked.Increment(ref _activeRequestCount);

            if (_disposingHttpRuntime) {
                // Dev11 333176: An appdomain is unloaded before all requests are served, resulting in System.AppDomainUnloadedException during isapi completion callback
                //
                // HttpRuntim.Dispose could have already finished on a different thread when we had no active requests
                // In this case we are about to start or already started unloading the appdomain so we will reject the request the safest way possible
                try {
                    wr.SendStatus(503, "Server Too Busy");
                    wr.SendKnownResponseHeader(HttpWorkerRequest.HeaderContentType, "text/html; charset=utf-8");
                    byte[] body = Encoding.ASCII.GetBytes("<html><body>Server Too Busy</body></html>");
                    wr.SendResponseFromMemory(body, body.Length);
                    // this will flush synchronously because of HttpRuntime.ShutdownInProgress
                    wr.FlushResponse(true);
                    wr.EndOfRequest();
                } finally {
                    Interlocked.Decrement(ref _activeRequestCount);
                }
                return;
            }

            // Construct the Context on HttpWorkerRequest, hook everything together
            HttpContext context;

            try {
                context = new HttpContext(wr, false /* initResponseWriter */);
            } 
            catch {
                try {
                    // If we fail to create the context for any reason, send back a 400 to make sure
                    // the request is correctly closed (relates to VSUQFE3962)
                    wr.SendStatus(400, "Bad Request");
                    wr.SendKnownResponseHeader(HttpWorkerRequest.HeaderContentType, "text/html; charset=utf-8");
                    byte[] body = Encoding.ASCII.GetBytes("<html><body>Bad Request</body></html>");
                    wr.SendResponseFromMemory(body, body.Length);
                    wr.FlushResponse(true);
                    wr.EndOfRequest();
                    return;
                } finally {
                    Interlocked.Decrement(ref _activeRequestCount);
                }
            }

            wr.SetEndOfSendNotification(_asyncEndOfSendCallback, context);

            HostingEnvironment.IncrementBusyCount();

            try {
                // First request initialization
                try {
                    EnsureFirstRequestInit(context);
                }
                catch {
                    // If we are handling a DEBUG request, ignore the FirstRequestInit exception.
                    // This allows the HttpDebugHandler to execute, and lets the debugger attach to
                    // the process (VSWhidbey 358135)
                    if (!context.Request.IsDebuggingRequest) {
                        throw;
                    }
                }

                // Init response writer (after we have config in first request init)
                // no need for impersonation as it is handled in config system
                context.Response.InitResponseWriter();

                // Get application instance
                IHttpHandler app = HttpApplicationFactory.GetApplicationInstance(context);

                if (app == null)
                    throw new HttpException(SR.GetString(SR.Unable_create_app_object));

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_START_HANDLER, context.WorkerRequest, app.GetType().FullName, "Start");

                if (app is IHttpAsyncHandler) {
                    // asynchronous handler
                    IHttpAsyncHandler asyncHandler = (IHttpAsyncHandler)app;
                    context.AsyncAppHandler = asyncHandler;
                    asyncHandler.BeginProcessRequest(context, _handlerCompletionCallback, context);
                }
                else {
                    // synchronous handler
                    app.ProcessRequest(context);
                    FinishRequest(context.WorkerRequest, context, null);
                }
            }
            catch (Exception e) {
                context.Response.InitResponseWriter();
                FinishRequest(wr, context, e);
            }
        }

        private void RejectRequestInternal(HttpWorkerRequest wr, bool silent) {
            // Construct the Context on HttpWorkerRequest, hook everything together
            HttpContext context = new HttpContext(wr, false /* initResponseWriter */);
            wr.SetEndOfSendNotification(_asyncEndOfSendCallback, context);

            // Count active requests
            Interlocked.Increment(ref _activeRequestCount);
            HostingEnvironment.IncrementBusyCount();

            if (silent) {
                context.Response.InitResponseWriter();
                FinishRequest(wr, context, null);
            }
            else {
                PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_REJECTED);
                PerfCounters.IncrementCounter(AppPerfCounter.APP_REQUESTS_REJECTED);
                try {
                    throw new HttpException(503, SR.GetString(SR.Server_too_busy));
                }
                catch (Exception e) {
                    context.Response.InitResponseWriter();
                    FinishRequest(wr, context, e);
                }
            }
        }

        internal static void ReportAppOfflineErrorMessage(HttpResponse response, byte[] appOfflineMessage) {
            response.StatusCode = 503;
            response.ContentType = "text/html";
            response.AddHeader("Retry-After", "3600");
            response.OutputStream.Write(appOfflineMessage, 0, appOfflineMessage.Length);
        }

        /*
         * Finish processing request, [....] or async
         */
        private void FinishRequest(HttpWorkerRequest wr, HttpContext context, Exception e) {
            HttpResponse response = context.Response;

            if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_END_HANDLER, context.WorkerRequest);

            SetExecutionTimePerformanceCounter(context);

            // Flush in case of no error
            if (e == null) {
                // impersonate around PreSendHeaders / PreSendContent
                using (new ClientImpersonationContext(context, false)) {
                    try {
                        // this sends the actual content in most cases
                        response.FinalFlushAtTheEndOfRequestProcessing();
                    }
                    catch (Exception eFlush) {
                        e = eFlush;
                    }
                }
            }

            // Report error if any
            if (e != null) {
                using (new DisposableHttpContextWrapper(context)) {

                    // if the custom encoder throws, it might interfere with returning error information
                    // to the client, so we force use of the default encoder
                    context.DisableCustomHttpEncoder = true;

                    if (_appOfflineMessage != null) {
                        try {
                            ReportAppOfflineErrorMessage(response, _appOfflineMessage);
                            response.FinalFlushAtTheEndOfRequestProcessing();
                        }
                        catch {
                        }
                    }
                    else {
                        // when application is on UNC share the code below must
                        // be run while impersonating the token given by IIS
                        using (new ApplicationImpersonationContext()) {
                            try {
                                try {
                                    // try to report error in a way that could possibly throw (a config exception)
                                    response.ReportRuntimeError(e, true /*canThrow*/, false);
                                }
                                catch (Exception eReport) {
                                    // report the config error in a way that would not throw
                                    response.ReportRuntimeError(eReport, false /*canThrow*/, false);
                                }

                                response.FinalFlushAtTheEndOfRequestProcessing();
                            }
                            catch {
                            }
                        }
                    }
                }
            }

            // Remember that first request is done
            _firstRequestCompleted = true;


            // In case we reporting HostingInit() error, app domain should not stick around
            if (_hostingInitFailed) {
                Debug.Trace("AppDomainFactory", "Shutting down appdomain because of HostingInit error");
                ShutdownAppDomain(ApplicationShutdownReason.HostingEnvironment, "HostingInit error");
            }

            // Check status code and increment proper counter
            // If it's an error status code (i.e. 400 or higher), increment the proper perf counters
            int statusCode = response.StatusCode;
            UpdatePerfCounters(statusCode);

            context.FinishRequestForCachedPathData(statusCode);

            // ---- exceptions from EndOfRequest as they will prevent proper request cleanup
            // Since the exceptions are not expected here we want to log them
            try {
                wr.EndOfRequest();
            }
            catch (Exception ex) {
                WebBaseEvent.RaiseRuntimeError(ex, this);
            }

            // Count active requests
            HostingEnvironment.DecrementBusyCount();
            Interlocked.Decrement(ref _activeRequestCount);

            // Schedule more work if some requests are queued
            if (_requestQueue != null)
                _requestQueue.ScheduleMoreWorkIfNeeded();
        }

        //
        // Make sure shutdown happens only once
        //

        private bool InitiateShutdownOnce() {
            if (_shutdownInProgress)
                return false;

            lock (this) {
                if (_shutdownInProgress)
                    return false;
                _shutdownInProgress = true;
            }

            return true;
        }

        //
        // Shutdown this and restart new app domain
        //
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private void ReleaseResourcesAndUnloadAppDomain(Object state /*not used*/) {
#if DBG
            Debug.Trace("FileChangesMonitorIgnoreSubdirChange",
                        "*** ReleaseResourcesAndUnloadAppDomain " + DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture)
                        + ": _appDomainAppId=" + _appDomainAppId);
#endif
            Debug.Trace("AppDomainFactory", "ReleaseResourcesAndUnloadAppDomain, Id=" + _appDomainAppId
                        + " DomainId = " + _appDomainId
                        + " Stack = " + Environment.StackTrace );

            try {
                PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.APPLICATION_RESTARTS);
            }
            catch {
            }

            // Release all resources
            try {
                Dispose();
            }
            catch {
            }

            Thread.Sleep(250);

            AddAppDomainTraceMessage("before Unload");

            for (; ; ) {
                try {
                    AppDomain.Unload(Thread.GetDomain());
                }
                catch (CannotUnloadAppDomainException) {
                    Debug.Assert(false);
                }
                catch (Exception e) {
                    Debug.Trace("AppDomainFactory", "AppDomain.Unload exception: " + e + "; Id=" + _appDomainAppId);
                    if (!BuildManagerHost.InClientBuildManager) {
                        // Avoid calling Exception.ToString if we are in the ClientBuildManager (Dev10 bug 824659)
                        AddAppDomainTraceMessage("Unload Exception: " + e);
                    }
                    throw;
                }
            }
        }

        private static void SetExecutionTimePerformanceCounter(HttpContext context) {
            // Set the Request Execution time perf counter
            TimeSpan elapsed = DateTime.UtcNow.Subtract(context.WorkerRequest.GetStartTime());
            long milli = elapsed.Ticks / TimeSpan.TicksPerMillisecond;

            if (milli > Int32.MaxValue)
                milli = Int32.MaxValue;

            PerfCounters.SetGlobalCounter(GlobalPerfCounter.REQUEST_EXECUTION_TIME, (int)milli);
            PerfCounters.SetCounter(AppPerfCounter.APP_REQUEST_EXEC_TIME, (int)milli);
        }

        private static void UpdatePerfCounters(int statusCode) {
            if (400 <= statusCode) {
                PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_FAILED);
                switch (statusCode) {
                    case 401: // Not authorized
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_AUTHORIZED);
                        break;
                    case 404: // Not found
                    case 414: // Not found
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
                        break;
                }
            }
            else {
                // If status code is not in the 400-599 range (i.e. 200-299 success or 300-399 redirection),
                // count it as a successful request.
                PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_SUCCEDED);
            }
        }

        private void WaitForRequestsToFinish(int waitTimeoutMs) {
            DateTime waitLimit = DateTime.UtcNow.AddMilliseconds(waitTimeoutMs);

            for (; ; ) {
                if (_activeRequestCount == 0 && (_requestQueue == null || _requestQueue.IsEmpty))
                    break;

                Thread.Sleep(250);

                // only apply timeout if a managed debugger is not attached
                if (!System.Diagnostics.Debugger.IsAttached && DateTime.UtcNow > waitLimit) {
                    break; // give it up
                }
            }
        }

        /*
         * Cleanup of all unmananged state
         */
        private void Dispose() {
            // get shutdown timeout from config
            int drainTimeoutSec = HttpRuntimeSection.DefaultShutdownTimeout;
            try {
                HttpRuntimeSection runtimeConfig = RuntimeConfig.GetAppLKGConfig().HttpRuntime;
                if (runtimeConfig != null) {
                    drainTimeoutSec = (int)runtimeConfig.ShutdownTimeout.TotalSeconds;
                }

                // before aborting compilation give time to drain (new requests are no longer coming at this point)
                WaitForRequestsToFinish(drainTimeoutSec * 1000);

                // reject remaining queued requests
                if (_requestQueue != null)
                    _requestQueue.Drain();
            } finally {
                // By this time all new requests should be directed to a newly created app domain
                // But there might be requests that got dispatched to this old app domain but have not reached ProcessRequestInternal yet
                // Signal ProcessRequestInternal to reject them immediately without initiating async operations
                _disposingHttpRuntime = true;
            }

            // give it a little more time to drain
            WaitForRequestsToFinish((drainTimeoutSec * 1000) / 6);


            // wait for pending async io to complete,  prior to aborting requests
            // this isn't necessary for IIS 7, where the async sends are always done
            // from native code with native buffers
            System.Web.Hosting.ISAPIWorkerRequestInProcForIIS6.WaitForPendingAsyncIo();

            // For IIS7 integrated pipeline, wait until GL_APPLICATION_STOP fires and
            // there are no active calls to IndicateCompletion before unloading the AppDomain
            if (HttpRuntime.UseIntegratedPipeline) {
                PipelineRuntime.WaitForRequestsToDrain();
            }
            else {
                // wait for all active requests to complete
                while (_activeRequestCount != 0) {
                    Thread.Sleep(250);
                }
            }


            // Dispose AppDomainShutdownTimer
            DisposeAppDomainShutdownTimer();

            // kill all remaining requests (and the timeout timer)
            _timeoutManager.Stop();
            AppDomainResourcePerfCounters.Stop();                 

#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            // double check for pending async io
            System.Web.Hosting.ISAPIWorkerRequestInProcForIIS6.WaitForPendingAsyncIo();

            // stop sqlcachedependency polling
            SqlCacheDependencyManager.Dispose((drainTimeoutSec * 1000) / 2);
#endif // !FEATURE_PAL
            // cleanup cache (this ends all sessions)
            if (_cacheInternal != null) {
                _cacheInternal.Dispose();
            }

            // app on end, cleanup app instances
            HttpApplicationFactory.EndApplication();  // call app_onEnd

            // stop file changes monitor
            _fcm.Stop();

            // stop health monitoring timer
            HealthMonitoringManager.Shutdown();
        }

        /*
         * Async completion of IIS7 pipeline (unlike OnHandlerCompletion, this may fire more than once).
         */
        private void OnRequestNotificationCompletion(IAsyncResult ar) {
            try {
                OnRequestNotificationCompletionHelper(ar);
            }
            catch(Exception e) {
                ApplicationManager.RecordFatalException(e);
                throw;
            }
        }

        private void OnRequestNotificationCompletionHelper(IAsyncResult ar) {
            if (ar.CompletedSynchronously) {
                Debug.Trace("PipelineRuntime", "OnRequestNotificationCompletion: completed synchronously");
                return;
            }

            Debug.Trace("PipelineRuntime", "OnRequestNotificationCompletion: completed asynchronously");

            RequestNotificationStatus status = RequestNotificationStatus.Continue;
            HttpContext context = (HttpContext) ar.AsyncState;
            IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;

            try {
                context.ApplicationInstance.EndProcessRequestNotification(ar);
            }
            catch (Exception e) {
                status = RequestNotificationStatus.FinishRequest;
                context.AddError(e);
            }

            // RequestContext is set to null if this is the last notification, so we need to save it
            // for the call to PostCompletion
            IntPtr requestContext = wr.RequestContext;

            FinishRequestNotification(wr, context, ref status);

            // set the notification context to null since we are exiting this notification
            context.NotificationContext = null;

            // Indicate completion to IIS, so that it can resume
            // request processing on an IIS thread
            Debug.Trace("PipelineRuntime", "OnRequestNotificationCompletion(" + status + ")");
            int result = UnsafeIISMethods.MgdPostCompletion(requestContext, status);
            Misc.ThrowIfFailedHr(result);
        }

        /*
         * Async completion of managed pipeline (called at most one time).
         */
        private void OnHandlerCompletion(IAsyncResult ar) {
            HttpContext context = (HttpContext)ar.AsyncState;

            try {
                context.AsyncAppHandler.EndProcessRequest(ar);
            }
            catch (Exception e) {
                context.AddError(e);
            }
            finally {
                // no longer keep AsyncAppHandler poiting to the application
                // is only needed to call EndProcessRequest
                context.AsyncAppHandler = null;
            }

            FinishRequest(context.WorkerRequest, context, context.Error);
        }

        /*
         * Notification from worker request that it is done writing from buffer
         * so that the buffers can be recycled
         */
        private void EndOfSendCallback(HttpWorkerRequest wr, Object arg) {
            Debug.Trace("PipelineRuntime", "HttpRuntime.EndOfSendCallback");
            HttpContext context = (HttpContext)arg;
            context.Request.Dispose();
            context.Response.Dispose();
        }

        /*
         * Notification when something in the bin directory changed
         */
        private void OnCriticalDirectoryChange(Object sender, FileChangeEvent e) {
            // shutdown the app domain
            Debug.Trace("AppDomainFactory", "Shutting down appdomain because of bin dir change or directory rename." +
                " FileName=" + e.FileName + " Action=" + e.Action);

            ApplicationShutdownReason reason = ApplicationShutdownReason.None;
            string directoryName = new DirectoryInfo(e.FileName).Name;

            string message = FileChangesMonitor.GenerateErrorMessage(e.Action);
            message = (message != null) ? message + directoryName : directoryName + " dir change or directory rename";

            if (StringUtil.EqualsIgnoreCase(directoryName, CodeDirectoryName)) {
                reason = ApplicationShutdownReason.CodeDirChangeOrDirectoryRename;
            }
            else if (StringUtil.EqualsIgnoreCase(directoryName, ResourcesDirectoryName)) {
                reason = ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename;
            }
            else if (StringUtil.EqualsIgnoreCase(directoryName, BrowsersDirectoryName)) {
                reason = ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename;
            }
            else if (StringUtil.EqualsIgnoreCase(directoryName, BinDirectoryName)) {
                reason = ApplicationShutdownReason.BinDirChangeOrDirectoryRename;
            }

            if (e.Action == FileAction.Added) {
                // Make sure HttpRuntime does not ignore the appdomain shutdown if a file is added (VSWhidbey 363481)
                HttpRuntime.SetUserForcedShutdown();

                Debug.Trace("AppDomainFactorySpecial", "Call SetUserForcedShutdown: FileName=" + e.FileName + "; now=" + DateTime.Now);
            }

            ShutdownAppDomain(reason, message);
        }

        /**
         * Coalesce file change notifications to minimize sharing violations and AppDomain restarts (ASURT 147492)
         */
        internal static void CoalesceNotifications() {
            int waitChangeNotification = HttpRuntimeSection.DefaultWaitChangeNotification;
            int maxWaitChangeNotification = HttpRuntimeSection.DefaultMaxWaitChangeNotification;
            try {
                HttpRuntimeSection config = RuntimeConfig.GetAppLKGConfig().HttpRuntime;
                if (config != null) {
                    waitChangeNotification = config.WaitChangeNotification;
                    maxWaitChangeNotification = config.MaxWaitChangeNotification;
                }
            }
            catch {
            }

            if (waitChangeNotification == 0 || maxWaitChangeNotification == 0)
                return;

            DateTime maxWait = DateTime.UtcNow.AddSeconds(maxWaitChangeNotification);
            // Coalesce file change notifications
            try {
                while (DateTime.UtcNow < maxWait) {
                    if (DateTime.UtcNow > _theRuntime.LastShutdownAttemptTime.AddSeconds(waitChangeNotification))
                        break;

                    Thread.Sleep(250);
                }
            }
            catch {
            }
        }

        // appdomain shutdown eventhandler
        internal static event BuildManagerHostUnloadEventHandler AppDomainShutdown;

        internal static void OnAppDomainShutdown(BuildManagerHostUnloadEventArgs e) {
            if (AppDomainShutdown != null) {
                AppDomainShutdown(_theRuntime, e);
            }
        }

        internal static void SetUserForcedShutdown() {
            _theRuntime._userForcedShutdown = true;
        }

        /*
         * Shutdown the current app domain
         */
        internal static bool ShutdownAppDomain(ApplicationShutdownReason reason, string message) {
            return ShutdownAppDomainWithStackTrace(reason, message, null /*stackTrace*/);
        }

        /*
         * Shutdown the current app domain with a stack trace.  This is useful for callers that are running
         * on a QUWI callback, and wouldn't provide a meaningful stack trace by default.
         */
        internal static bool ShutdownAppDomainWithStackTrace(ApplicationShutdownReason reason, string message, string stackTrace) {
            SetShutdownReason(reason, message);
            return ShutdownAppDomain(stackTrace);
        }

        private static bool ShutdownAppDomain(string stackTrace) {
#if DBG
            Debug.Trace("FileChangesMonitorIgnoreSubdirChange",
                        "*** ShutdownAppDomain " + DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture)
                        + ": _appDomainAppId=" + HttpRuntime.AppDomainAppId);
#endif
            // Ignore notifications during the processing of the first request (ASURT 100335)
            // skip this if LastShutdownAttemptTime has been set
            if (_theRuntime.LastShutdownAttemptTime == DateTime.MinValue && !_theRuntime._firstRequestCompleted && !_theRuntime._userForcedShutdown) {
                // check the timeout (don't disable notifications forever
                int delayTimeoutSec = HttpRuntimeSection.DefaultDelayNotificationTimeout;

                try {
                    RuntimeConfig runtimeConfig = RuntimeConfig.GetAppLKGConfig();
                    if (runtimeConfig != null) {
                        HttpRuntimeSection runtimeSection = runtimeConfig.HttpRuntime;
                        if (runtimeSection != null) {
                            delayTimeoutSec = (int)runtimeSection.DelayNotificationTimeout.TotalSeconds;

                            if (DateTime.UtcNow < _theRuntime._firstRequestStartTime.AddSeconds(delayTimeoutSec)) {
                                Debug.Trace("AppDomainFactory", "ShutdownAppDomain IGNORED (1st request is not done yet), Id = " + AppDomainAppId);
                                return false;
                            }
                        }
                    }
                }
                catch {
                }
            }

            try {
                _theRuntime.RaiseShutdownWebEventOnce();
            }
            catch {
                // VSWhidbey 444472: if an exception is thrown, we consume it and continue executing the following code.
            }

            // Update last time ShutdownAppDomain was called
            _theRuntime.LastShutdownAttemptTime = DateTime.UtcNow;

            if (!HostingEnvironment.ShutdownInitiated) {
                // This shutdown is not triggered by hosting environment - let it do the job
                HostingEnvironment.InitiateShutdownWithoutDemand();
                return true;
            }

            //WOS 1400290: CantUnloadAppDomainException in ISAPI mode, wait until HostingEnvironment.ShutdownThisAppDomainOnce completes
            if (HostingEnvironment.ShutdownInProgress) {
                return false;
            }

            // Make sure we don't go through shutdown logic many times
            if (!_theRuntime.InitiateShutdownOnce())
                return false;

            Debug.Trace("AppDomainFactory", "ShutdownAppDomain, Id = " + AppDomainAppId + ", ShutdownInProgress=" + ShutdownInProgress
                        + ", ShutdownMessage=" + _theRuntime._shutDownMessage);

            if (String.IsNullOrEmpty(stackTrace) && !BuildManagerHost.InClientBuildManager) {
                // Avoid calling Environment.StackTrace if we are in the ClientBuildManager (Dev10 bug 824659)

                // Instrument to be able to see what's causing a shutdown
                new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                try {
                    _theRuntime._shutDownStack = Environment.StackTrace;
                }
                finally {
                    CodeAccessPermission.RevertAssert();
                }
            }
            else {
                _theRuntime._shutDownStack = stackTrace;
            }

            // Notify when appdomain is about to shutdown.
            OnAppDomainShutdown(new BuildManagerHostUnloadEventArgs(_theRuntime._shutdownReason));

            // unload app domain from another CLR thread
            ThreadPool.QueueUserWorkItem(_theRuntime._appDomainUnloadallback);

            return true;
        }

        internal static void RecoverFromUnexceptedAppDomainUnload() {
            if (_theRuntime._shutdownInProgress)
                return;

            // someone unloaded app domain directly - tell unmanaged code
            Debug.Trace("AppDomainFactory", "Unexpected AppDomainUnload");
            _theRuntime._shutdownInProgress = true;

            // tell unmanaged code not to dispatch requests to this app domain
            try {
                ISAPIRuntime.RemoveThisAppDomainFromUnmanagedTable();
                PipelineRuntime.RemoveThisAppDomainFromUnmanagedTable();
                AddAppDomainTraceMessage("AppDomainRestart");
            }
            finally {
                // release all resources
                _theRuntime.Dispose();
            }
        }

        /*
         * Notification when app-level Config changed
         */
         internal static void OnConfigChange(String message) {
            Debug.Trace("AppDomainFactory", "Shutting down appdomain because of config change");
            ShutdownAppDomain(ApplicationShutdownReason.ConfigurationChange, (message != null) ? message : "CONFIG change");
        }

        // Intrumentation to remember the overwhelming file change
        internal static void SetShutdownReason(ApplicationShutdownReason reason, String message) {
            if (_theRuntime._shutdownReason == ApplicationShutdownReason.None) {
                _theRuntime._shutdownReason = reason;
            }

            SetShutdownMessage(message);
        }

        internal static void SetShutdownMessage(String message) {
            if (message != null) {
                if (_theRuntime._shutDownMessage == null)
                    _theRuntime._shutDownMessage = message;
                else
                    _theRuntime._shutDownMessage += "\r\n" + message;
            }
        }


        // public method is on HostingEnvironment
        internal static ApplicationShutdownReason ShutdownReason {
            get { return _theRuntime._shutdownReason; }
        }

        //
        // public static APIs
        //

        /*
         * Process one request
         */

        /// <devdoc>
        ///    <para><SPAN>The method that drives
        ///       all ASP.NET web processing execution.</SPAN></para>
        /// </devdoc>
        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
        public static void ProcessRequest(HttpWorkerRequest wr) {
            if (wr == null)
                throw new ArgumentNullException("wr");

            if (HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Method_Not_Supported_By_Iis_Integrated_Mode, "HttpRuntime.ProcessRequest"));
            }

            ProcessRequestNoDemand(wr);
        }


        internal static void ProcessRequestNoDemand(HttpWorkerRequest wr) {
            RequestQueue rq = _theRuntime._requestQueue;

            wr.UpdateInitialCounters();

            if (rq != null)  // could be null before first request
                wr = rq.GetRequestToExecute(wr);

            if (wr != null) {
                CalculateWaitTimeAndUpdatePerfCounter(wr);
                wr.ResetStartTime();
                ProcessRequestNow(wr);
            }
        }


        private static void CalculateWaitTimeAndUpdatePerfCounter(HttpWorkerRequest wr) {
            DateTime begin = wr.GetStartTime();

            TimeSpan elapsed = DateTime.UtcNow.Subtract(begin);
            long milli = elapsed.Ticks / TimeSpan.TicksPerMillisecond;

            if (milli > Int32.MaxValue)
                milli = Int32.MaxValue;

            PerfCounters.SetGlobalCounter(GlobalPerfCounter.REQUEST_WAIT_TIME, (int)milli);
            PerfCounters.SetCounter(AppPerfCounter.APP_REQUEST_WAIT_TIME, (int)milli);
        }

        internal static void ProcessRequestNow(HttpWorkerRequest wr) {
            _theRuntime.ProcessRequestInternal(wr);
        }

        internal static void RejectRequestNow(HttpWorkerRequest wr, bool silent) {
            _theRuntime.RejectRequestInternal(wr, silent);
        }


        /// <devdoc>
        ///       <para>Removes all items from the cache and shuts down the runtime.</para>
        ///    </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static void Close() {
            Debug.Trace("AppDomainFactory", "HttpRuntime.Close, ShutdownInProgress=" + ShutdownInProgress);
            if (_theRuntime.InitiateShutdownOnce()) {
                SetShutdownReason(ApplicationShutdownReason.HttpRuntimeClose, "HttpRuntime.Close is called");

                if (HostingEnvironment.IsHosted) {
                    // go throw initiate shutdown for hosted scenarios
                    HostingEnvironment.InitiateShutdownWithoutDemand();
                }
                else {
                    _theRuntime.Dispose();
                }
            }
        }


        /// <devdoc>
        ///       <para>Unloads the current app domain.</para>
        ///    </devdoc>
        public static void UnloadAppDomain() {
            _theRuntime._userForcedShutdown = true;
            ShutdownAppDomain(ApplicationShutdownReason.UnloadAppDomainCalled, "User code called UnloadAppDomain");
        }

        private DateTime LastShutdownAttemptTime {
            get {
                DateTime dt;
                lock (this) {
                    dt = _lastShutdownAttemptTime;
                }
                return dt;
            }
            set {
                lock (this) {
                    _lastShutdownAttemptTime = value;
                }
            }
        }

        internal static Profiler Profile {
            get {
                return _theRuntime._profiler;
            }
        }

        internal static bool IsTrustLevelInitialized {
            get {
                return !HostingEnvironment.IsHosted || TrustLevel != null;
            }
        }

        internal static NamedPermissionSet NamedPermissionSet {
            get {
                // Make sure we have already initialized the trust level
                // 


                return _theRuntime._namedPermissionSet;
            }
        }

        internal static PolicyLevel PolicyLevel {
            get {
                return _theRuntime._policyLevel;
            }
        }

        internal static string HostSecurityPolicyResolverType {
            get {
                return _theRuntime._hostSecurityPolicyResolverType;
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Unrestricted)]
        public static NamedPermissionSet GetNamedPermissionSet() {
            NamedPermissionSet namedPermissionSet = _theRuntime._namedPermissionSet;
            if (namedPermissionSet == null) {
                return null;
            }
            else {
                return new NamedPermissionSet(namedPermissionSet);
            }
        }

        internal static bool IsFullTrust {
            get {
                // Make sure we have already initialized the trust level
                Debug.Assert(IsTrustLevelInitialized);

                return (_theRuntime._namedPermissionSet == null);
            }
        }

        /*
         * Check that the current trust level allows access to a virtual path.  Throw if it doesn't,
         */
        internal static void CheckVirtualFilePermission(string virtualPath) {
            string physicalPath = HostingEnvironment.MapPath(virtualPath);
            CheckFilePermission(physicalPath);
        }

        /*
         * Check that the current trust level allows access to a path.  Throw if it doesn't,
         */
        internal static void CheckFilePermission(string path) {
            CheckFilePermission(path, false);
        }

        internal static void CheckFilePermission(string path, bool writePermissions) {
            if (!HasFilePermission(path, writePermissions)) {
                throw new HttpException(SR.GetString(SR.Access_denied_to_path, GetSafePath(path)));
            }
        }

        internal static bool HasFilePermission(string path) {
            return HasFilePermission(path, false);
        }

        internal static bool HasFilePermission(string path, bool writePermissions) {
            // WOS #1523618: need to skip this check for HttpResponse.ReportRuntimeError when reporting an
            // InitializationException (e.g., necessary to display line info for ConfigurationException).

            if (TrustLevel == null && InitializationException != null) {
                return true;
            }

            // Make sure we have already initialized the trust level
            Debug.Assert(TrustLevel != null || !HostingEnvironment.IsHosted, "TrustLevel != null || !HostingEnvironment.IsHosted");

            // If we don't have a NamedPermissionSet, we're in full trust
            if (NamedPermissionSet == null)
                return true;

            bool fAccess = false;

            // Check that the user has permission to the path
            IPermission allowedPermission = NamedPermissionSet.GetPermission(typeof(FileIOPermission));
            if (allowedPermission != null) {
                IPermission askedPermission = null;
                try {
                    if (!writePermissions)
                        askedPermission = new FileIOPermission(FileIOPermissionAccess.Read, path);
                    else
                        askedPermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, path);
                }
                catch {
                    // This could happen if the path is not absolute
                    return false;
                }
                fAccess = askedPermission.IsSubsetOf(allowedPermission);
            }

            return fAccess;
        }

        internal static bool HasWebPermission(Uri uri) {

            // Make sure we have already initialized the trust level
            Debug.Assert(TrustLevel != null || !HostingEnvironment.IsHosted);

            // If we don't have a NamedPermissionSet, we're in full trust
            if (NamedPermissionSet == null)
                return true;

            bool fAccess = false;

            // Check that the user has permission to the URI
            IPermission allowedPermission = NamedPermissionSet.GetPermission(typeof(WebPermission));
            if (allowedPermission != null) {
                IPermission askedPermission = null;
                try {
                    askedPermission = new WebPermission(NetworkAccess.Connect, uri.ToString());
                }
                catch {
                    return false;
                }
                fAccess = askedPermission.IsSubsetOf(allowedPermission);
            }

            return fAccess;
        }

        internal static bool HasDbPermission(DbProviderFactory factory) {

            // Make sure we have already initialized the trust level
            Debug.Assert(TrustLevel != null || !HostingEnvironment.IsHosted);

            // If we don't have a NamedPermissionSet, we're in full trust
            if (NamedPermissionSet == null)
                return true;

            bool fAccess = false;

            // Check that the user has permission to the provider
            CodeAccessPermission askedPermission = factory.CreatePermission(PermissionState.Unrestricted);
            if (askedPermission != null) {
                IPermission allowedPermission = NamedPermissionSet.GetPermission(askedPermission.GetType());
                if (allowedPermission != null) {
                    fAccess = askedPermission.IsSubsetOf(allowedPermission);
                }
            }

            return fAccess;
        }

        internal static bool HasPathDiscoveryPermission(string path) {
            // WOS #1523618: need to skip this check for HttpResponse.ReportRuntimeError when reporting an
            // InitializationException (e.g., necessary to display line info for ConfigurationException).

            if (TrustLevel == null && InitializationException != null) {
                return true;
            }

            // Make sure we have already initialized the trust level
            Debug.Assert(TrustLevel != null || !HostingEnvironment.IsHosted);

            // If we don't have a NamedPermissionSet, we're in full trust
            if (NamedPermissionSet == null)
                return true;

            bool fAccess = false;

            // Check that the user has permission to the path
            IPermission allowedPermission = NamedPermissionSet.GetPermission(typeof(FileIOPermission));
            if (allowedPermission != null) {
                IPermission askedPermission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path);
                fAccess = askedPermission.IsSubsetOf(allowedPermission);
            }

            return fAccess;

        }

        internal static bool HasAppPathDiscoveryPermission() {
            return HasPathDiscoveryPermission(HttpRuntime.AppDomainAppPathInternal);
        }

        internal static string GetSafePath(string path) {
            if (String.IsNullOrEmpty(path))
                return path;

            try {
                if (HasPathDiscoveryPermission(path)) // could throw on bad filenames
                    return path;
            }
            catch {
            }

            return Path.GetFileName(path);
        }

        /*
         * Check that the current trust level allows Unmanaged access
         */
        internal static bool HasUnmanagedPermission() {

            // Make sure we have already initialized the trust level
            Debug.Assert(TrustLevel != null || !HostingEnvironment.IsHosted);

            // If we don't have a NamedPermissionSet, we're in full trust
            if (NamedPermissionSet == null)
                return true;

            SecurityPermission securityPermission = (SecurityPermission)NamedPermissionSet.GetPermission(
                typeof(SecurityPermission));
            if (securityPermission == null)
                return false;

            return (securityPermission.Flags & SecurityPermissionFlag.UnmanagedCode) != 0;
        }

        internal static bool HasAspNetHostingPermission(AspNetHostingPermissionLevel level) {

            // Make sure we have already initialized the trust level
            // 



            // If we don't have a NamedPermissionSet, we're in full trust
            if (NamedPermissionSet == null)
                return true;

            AspNetHostingPermission permission = (AspNetHostingPermission)NamedPermissionSet.GetPermission(
                typeof(AspNetHostingPermission));
            if (permission == null)
                return false;

            return (permission.Level >= level);
        }

        internal static void CheckAspNetHostingPermission(AspNetHostingPermissionLevel level, String errorMessageId) {
            if (!HasAspNetHostingPermission(level)) {
                throw new HttpException(SR.GetString(errorMessageId));
            }
        }

        // If we're not in full trust, fail if the passed in type doesn't have the APTCA bit
        internal static void FailIfNoAPTCABit(Type t, ElementInformation elemInfo, string propertyName) {

            if (!IsTypeAllowedInConfig(t)) {
                if (null != elemInfo) {
                    PropertyInformation propInfo = elemInfo.Properties[propertyName];

                    throw new ConfigurationErrorsException(SR.GetString(SR.Type_from_untrusted_assembly, t.FullName),
                    propInfo.Source, propInfo.LineNumber);
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Type_from_untrusted_assembly, t.FullName));
                }
            }
        }

        // If we're not in full trust, fail if the passed in type doesn't have the APTCA bit
        internal static void FailIfNoAPTCABit(Type t, XmlNode node) {

            if (!IsTypeAllowedInConfig(t)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Type_from_untrusted_assembly, t.FullName),
                    node);
            }
        }

        private static bool HasAPTCABit(Assembly assembly) {
            return assembly.IsDefined(typeof(AllowPartiallyTrustedCallersAttribute), inherit: false);
        }

        // Check if the type is allowed to be used in config by checking the APTCA bit
        internal static bool IsTypeAllowedInConfig(Type t) {

            // Allow everything in full trust
            if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted))
                return true;

            return IsTypeAccessibleFromPartialTrust(t);
        }

        internal static bool IsTypeAccessibleFromPartialTrust(Type t) {
            Assembly assembly = t.Assembly;
            
            if (assembly.SecurityRuleSet == SecurityRuleSet.Level1) {
                // Level 1 CAS uses transparency as an auditing mechanism rather than an enforcement mechanism, so we can't
                // perform a transparency check. Instead, allow the call to go through if:
                // (a) the referenced assembly is partially trusted, hence it cannot do anything dangerous; or
                // (b) the assembly is fully trusted and has APTCA.
                return (!assembly.IsFullyTrusted || HasAPTCABit(assembly));
            }
            else {
                // ** TEMPORARY **
                // Some GACed assemblies register critical modules / handlers. We can't break these scenarios for .NET 4.5, but we should
                // remove this APTCA check when we fix DevDiv #85358 and use only the transparency check defined below.
                if (HasAPTCABit(assembly)) {
                    return true;
                }
                // ** END TEMPORARY **

                // Level 2 CAS uses transparency as an enforcement mechanism, so we can perform a transparency check.
                // Transparent and SafeCritical types are safe to use from partial trust code.
                return (t.IsSecurityTransparent || t.IsSecuritySafeCritical);
            }
        }

        internal static FileChangesMonitor FileChangesMonitor {
            get { return _theRuntime._fcm; }
        }

        internal static RequestTimeoutManager RequestTimeoutManager {
            get { return _theRuntime._timeoutManager; }
        }


        /// <devdoc>
        ///    <para>Provides access to the cache.</para>
        /// </devdoc>
        public static Cache Cache {
            get {

                if (HttpRuntime.AspInstallDirectoryInternal == null) {
                    throw new HttpException(SR.GetString(SR.Aspnet_not_installed, VersionInfo.SystemWebVersion));
                }

                // In a web app, ReadCacheInternalConfig() is called from HttpRuntime.HostingInit.
                // However, if the cache is used by a non-http app, HttpRuntime.HostingInit won't
                // be called and we need to find a way to call ReadCacheInternalConfig().
                // The safe and inexpensive place to call it is when the non-http app accesses the
                // Cache thru HttpRuntime.Cache.
                //
                // ReadCacheInternalConfig() protects itself from being read multiple times.
                //
                Cache cachePublic = _theRuntime._cachePublic;
                if (cachePublic == null) {
                    CacheInternal cacheInternal = CacheInternal;
                    CacheSection cacheSection = RuntimeConfig.GetAppConfig().Cache;
                    cacheInternal.ReadCacheInternalConfig(cacheSection);
                    _theRuntime._cachePublic = cacheInternal.CachePublic;
                    cachePublic = _theRuntime._cachePublic;
                }

                return cachePublic;
            }
        }

        private void CreateCache() {
            lock (this) {
                if (_cacheInternal == null) {
                    _cacheInternal = CacheInternal.Create();
                }
            }
        }

        internal static CacheInternal GetCacheInternal(bool createIfDoesNotExist) {
            // Note that we only create the cache on first access,
            // not in HttpRuntime initialization.
            // This prevents cache timers from running when
            // the cache is not used.
            CacheInternal cacheInternal = _theRuntime._cacheInternal;
            if (cacheInternal == null && createIfDoesNotExist) {
                _theRuntime.CreateCache();
                cacheInternal = _theRuntime._cacheInternal;
            }

            return cacheInternal;
        }

        internal static CacheInternal CacheInternal {
            get { return GetCacheInternal(createIfDoesNotExist: true); }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string AspInstallDirectory {
            get {
                String path = AspInstallDirectoryInternal;

                if (path == null) {
                    throw new HttpException(SR.GetString(SR.Aspnet_not_installed, VersionInfo.SystemWebVersion));
                }

                InternalSecurityPermissions.PathDiscovery(path).Demand();
                return path;
            }
        }

        internal static string AspInstallDirectoryInternal {
            get { return s_installDirectory; }
        }

        //
        // Return the client script virtual path, e.g. "/aspnet_client/system_web/2_0_50217"
        //
        public static string AspClientScriptVirtualPath {
            get {
                if (_theRuntime._clientScriptVirtualPath == null) {
                    string aspNetVersion = VersionInfo.SystemWebVersion;
                    string clientScriptVirtualPath = AspNetClientFilesParentVirtualPath + aspNetVersion.Substring(0, aspNetVersion.LastIndexOf('.')).Replace('.', '_');

                    _theRuntime._clientScriptVirtualPath = clientScriptVirtualPath;
                }

                return _theRuntime._clientScriptVirtualPath;
            }
        }

        public static string AspClientScriptPhysicalPath {
            get {
                String path = AspClientScriptPhysicalPathInternal;

                if (path == null) {
                    throw new HttpException(SR.GetString(SR.Aspnet_not_installed, VersionInfo.SystemWebVersion));
                }

                return path;
            }
        }

        //
        // Return the client script physical path, e.g. @"c:\windows\microsoft.net\framework\v2.0.50217.0\asp.netclientfiles"
        //
        internal static string AspClientScriptPhysicalPathInternal {
            get {
                if (_theRuntime._clientScriptPhysicalPath == null) {
                    string clientScriptPhysicalPath = System.IO.Path.Combine(AspInstallDirectoryInternal, AspNetClientFilesSubDirectory);

                    _theRuntime._clientScriptPhysicalPath = clientScriptPhysicalPath;
                }

                return _theRuntime._clientScriptPhysicalPath;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string ClrInstallDirectory {
            get {
                String path = ClrInstallDirectoryInternal;
                InternalSecurityPermissions.PathDiscovery(path).Demand();
                return path;
            }
        }

        internal static string ClrInstallDirectoryInternal {
            get { return HttpConfigurationSystem.MsCorLibDirectory; }
        }



        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string MachineConfigurationDirectory {
            get {
                String path = MachineConfigurationDirectoryInternal;
                InternalSecurityPermissions.PathDiscovery(path).Demand();
                return path;
            }
        }

        internal static string MachineConfigurationDirectoryInternal {
            get { return HttpConfigurationSystem.MachineConfigurationDirectory; }
        }

        internal static bool IsEngineLoaded {
            get { return s_isEngineLoaded; }
        }


        //
        //  Static app domain related properties
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String CodegenDir {
            get {
                String path = CodegenDirInternal;
                InternalSecurityPermissions.PathDiscovery(path).Demand();
                return path;
            }
        }

        internal static string CodegenDirInternal {
            get { return _theRuntime._codegenDir; }
        }

        internal static string TempDirInternal {
            get { return _theRuntime._tempDir; }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String AppDomainAppId {
            get {
                return _theRuntime._appDomainAppId;
            }
        }

        internal static bool IsAspNetAppDomain {
            get { return AppDomainAppId != null; }
        }



        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String AppDomainAppPath {
            get {
                InternalSecurityPermissions.AppPathDiscovery.Demand();
                return AppDomainAppPathInternal;
            }
        }

        internal static string AppDomainAppPathInternal {
            get { return _theRuntime._appDomainAppPath; }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String AppDomainAppVirtualPath {
            get {
                return VirtualPath.GetVirtualPathStringNoTrailingSlash(_theRuntime._appDomainAppVPath);
            }
        }

        // Save as AppDomainAppVirtualPath, but includes the trailng slash.  We can't change
        // AppDomainAppVirtualPath since it's public.
        internal static String AppDomainAppVirtualPathString {
            get {
                return VirtualPath.GetVirtualPathString(_theRuntime._appDomainAppVPath);
            }
        }

        internal static VirtualPath AppDomainAppVirtualPathObject {
            get {
                return _theRuntime._appDomainAppVPath;
            }
        }

        internal static bool IsPathWithinAppRoot(String path) {
            if (AppDomainIdInternal == null)
                return true;    // app domain not initialized

            return UrlPath.IsEqualOrSubpath(AppDomainAppVirtualPathString, path);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String AppDomainId {
            [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
            get {
                return AppDomainIdInternal;
            }
        }

        internal static string AppDomainIdInternal {
            get { return _theRuntime._appDomainId; }
        }



        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String BinDirectory {
            get {
                String path = BinDirectoryInternal;
                InternalSecurityPermissions.PathDiscovery(path).Demand();
                return path;
            }
        }

        internal static string BinDirectoryInternal {
            get { return Path.Combine(_theRuntime._appDomainAppPath, BinDirectoryName) + Path.DirectorySeparatorChar; }

        }

        internal static VirtualPath CodeDirectoryVirtualPath {
            get { return _theRuntime._appDomainAppVPath.SimpleCombineWithDir(CodeDirectoryName); }
        }

        internal static VirtualPath ResourcesDirectoryVirtualPath {
            get { return _theRuntime._appDomainAppVPath.SimpleCombineWithDir(ResourcesDirectoryName); }
        }

        internal static VirtualPath WebRefDirectoryVirtualPath {
            get { return _theRuntime._appDomainAppVPath.SimpleCombineWithDir(WebRefDirectoryName); }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool IsOnUNCShare {
            [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
            get {
                return IsOnUNCShareInternal;
            }
        }

        internal static bool IsOnUNCShareInternal {
            get { return _theRuntime._isOnUNCShare; }
        }


        //
        //  Static helper to retrieve app domain values
        //

        private static String GetAppDomainString(String key) {
            Object x = Thread.GetDomain().GetData(key);

            return x as String;
        }

        internal static void AddAppDomainTraceMessage(String message) {
            const String appDomainTraceKey = "ASP.NET Domain Trace";
            AppDomain d = Thread.GetDomain();
            String m = d.GetData(appDomainTraceKey) as String;
            d.SetData(appDomainTraceKey, (m != null) ? m + " ... " + message : message);
        }

        // Gets the version of the ASP.NET framework the current web applications is targeting.
        // This property is normally set via the <httpRuntime> element's "targetFramework"
        // attribute. The property is not guaranteed to return a correct value if the current
        // AppDomain is not an ASP.NET web application AppDomain.
        public static Version TargetFramework {
            get {
                return BinaryCompatibility.Current.TargetFramework;
            }
        }


        //
        //  Flags
        //

        internal static bool DebuggingEnabled {
            get { return _theRuntime._debuggingEnabled; }
        }

        internal static bool ConfigInited {
            get { return _theRuntime._configInited; }
        }

        internal static bool FusionInited {
            get { return _theRuntime._fusionInited; }
        }

        internal static bool ApartmentThreading {
            get { return _theRuntime._apartmentThreading; }
        }

        internal static bool ShutdownInProgress {
            get { return _theRuntime._shutdownInProgress; }
        }

        internal static string TrustLevel {
            get { return _theRuntime._trustLevel; }
        }

        internal static string WpUserId {
            get { return _theRuntime._wpUserId; }
        }


        private void SetTrustLevel(TrustSection trustSection, SecurityPolicySection securityPolicySection) {
            // Use a temporary variable, since we use the field as a signal that the trust has really
            // been set, which is not the case until later in this method.
            string trustLevel = trustSection.Level;

            if (trustSection.Level == "Full") {
                _trustLevel = trustLevel;
                return;
            }

            if (securityPolicySection == null || securityPolicySection.TrustLevels[trustSection.Level] == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Unable_to_get_policy_file, trustSection.Level), String.Empty, 0);
                //             Do not give out configuration information since we don't know what trust level we are
                //             supposed to be running at.  If the information below is added to the error it might expose
                //             part of the config file that the users does not have permissions to see. VS261145
                //            ,trustSection.ElementInformation.Properties["level"].Source,
                //            trustSection.ElementInformation.Properties["level"].LineNumber);
            }
            String file = null;
            if (trustSection.Level == "Minimal" || trustSection.Level == "Low" ||
                trustSection.Level == "Medium" || trustSection.Level == "High") {
                file = (String)securityPolicySection.TrustLevels[trustSection.Level].LegacyPolicyFileExpanded;
            }
            else {
                file = (String)securityPolicySection.TrustLevels[trustSection.Level].PolicyFileExpanded;
            }
            if (file == null || !FileUtil.FileExists(file)) {
                //if HttpContext.Current.IsCustomErrorEnabled
                throw new HttpException(SR.GetString(SR.Unable_to_get_policy_file, trustSection.Level));
                //else
                //    throw new ConfigurationErrorsException(SR.GetString(SR.Unable_to_get_policy_file, trustSection.Level),
                //        trustSection.Filename, trustSection.LineNumber);
            }

            bool foundGacToken = false;
#pragma warning disable 618
            PolicyLevel policyLevel = CreatePolicyLevel(file, AppDomainAppPathInternal, CodegenDirInternal, trustSection.OriginUrl, out foundGacToken);

            // see if the policy file contained a v1.x UrlMembershipCondition containing
            // a GAC token.  If so, let's upgrade it by adding a code group granting
            // full trust to code from the GAC
            if (foundGacToken) {
                // walk the code groups at the app domain level and look for one that grants
                // access to the GAC with an UrlMembershipCondition.
                CodeGroup rootGroup = policyLevel.RootCodeGroup;
                bool foundGacCondition = false;
                foreach (CodeGroup childGroup in rootGroup.Children) {
                    if (childGroup.MembershipCondition is GacMembershipCondition) {
                        foundGacCondition = true;

                        // if we found the GAC token and also have the GacMembershipCondition
                        // the policy file needs to be upgraded to just include the GacMembershipCondition
                        Debug.Assert(!foundGacCondition);
                        break;
                    }
                }

                // add one as a child of the toplevel group after
                // some sanity checking to make sure it's an ASP.NET policy file
                // which always begins with a FirstMatchCodeGroup granting nothing
                // this might not upgrade some custom policy files
                if (!foundGacCondition) {
                    if (rootGroup is FirstMatchCodeGroup) {
                        FirstMatchCodeGroup firstMatch = (FirstMatchCodeGroup)rootGroup;
                        if (firstMatch.MembershipCondition is AllMembershipCondition &&
                           firstMatch.PermissionSetName == "Nothing") {
                            PermissionSet fullTrust = new PermissionSet(PermissionState.Unrestricted);

                            CodeGroup gacGroup = new UnionCodeGroup(new GacMembershipCondition(),
                                                                    new PolicyStatement(fullTrust));


                            // now, walk the current groups and insert our new group
                            // immediately before the old Gac group
                            // we'll need to use heuristics for this:
                            // it will be an UrlMembershipCondition group with full trust
                            CodeGroup newRoot = new FirstMatchCodeGroup(rootGroup.MembershipCondition, rootGroup.PolicyStatement);
                            foreach (CodeGroup childGroup in rootGroup.Children) {

                                // is this the target old $Gac$ group?
                                // insert our new GacMembershipCondition group ahead of it
                                if ((childGroup is UnionCodeGroup) &&
                                   (childGroup.MembershipCondition is UrlMembershipCondition) &&
                                   childGroup.PolicyStatement.PermissionSet.IsUnrestricted()) {
                                    if (null != gacGroup) {
                                        newRoot.AddChild(gacGroup);
                                        gacGroup = null;
                                    }
                                }

                                // append this group to the root group
                                // AddChild itself does a deep Copy to get any
                                // child groups so we don't need one here
                                newRoot.AddChild(childGroup);
                            }

                            policyLevel.RootCodeGroup = newRoot;
                            //Debug.Trace("internal", "PolicyLevel: " + policyLevel.ToXml());
                        }
                    }
                }
#pragma warning restore 618
            }


#pragma warning disable 618
            AppDomain.CurrentDomain.SetAppDomainPolicy(policyLevel);
            _namedPermissionSet = policyLevel.GetNamedPermissionSet(trustSection.PermissionSetName);
#pragma warning restore 618

            _trustLevel = trustLevel;

            _fcm.StartMonitoringFile(file, new FileChangeEventHandler(this.OnSecurityPolicyFileChange));
        }

#pragma warning disable 618
        private static PolicyLevel CreatePolicyLevel(String configFile, String appDir, String binDir, String strOriginUrl, out bool foundGacToken) {
            // Read in the config file to a string.
            FileStream file = new FileStream(configFile, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file, Encoding.UTF8);
            String strFileData = reader.ReadToEnd();

            reader.Close();

            appDir = FileUtil.RemoveTrailingDirectoryBackSlash(appDir);
            binDir = FileUtil.RemoveTrailingDirectoryBackSlash(binDir);

            strFileData = strFileData.Replace("$AppDir$", appDir);
            strFileData = strFileData.Replace("$AppDirUrl$", MakeFileUrl(appDir));
            strFileData = strFileData.Replace("$CodeGen$", MakeFileUrl(binDir));
            if (strOriginUrl == null)
                strOriginUrl = String.Empty;
            strFileData = strFileData.Replace("$OriginHost$", strOriginUrl);

            // see if the file contains a GAC token
            // if so, do the replacement and record the
            // fact so that we later add a GacMembershipCondition
            // codegroup to the PolicyLevel
            int ndx = strFileData.IndexOf("$Gac$", StringComparison.Ordinal);
            if (ndx != -1) {
                string gacLocation = GetGacLocation();
                if (gacLocation != null)
                    gacLocation = MakeFileUrl(gacLocation);
                if (gacLocation == null)
                    gacLocation = String.Empty;

                strFileData = strFileData.Replace("$Gac$", gacLocation);
                foundGacToken = true;
            }
            else {
                foundGacToken = false;
            }

            return SecurityManager.LoadPolicyLevelFromString(strFileData, PolicyLevelType.AppDomain);
        }
#pragma warning restore 618

        private void SetTrustParameters(TrustSection trustSection, SecurityPolicySection securityPolicySection, PolicyLevel policyLevel) {
            _trustLevel = trustSection.Level;
            if (_trustLevel != "Full") {
                // if we are in partial trust, HostingEnvironment should init HttpRuntime with a non-null PolicyLevel object
                Debug.Assert(policyLevel != null);

                _namedPermissionSet = policyLevel.GetNamedPermissionSet(trustSection.PermissionSetName);
                _policyLevel = policyLevel;
                _hostSecurityPolicyResolverType = trustSection.HostSecurityPolicyResolverType;
                String file = (String)securityPolicySection.TrustLevels[trustSection.Level].PolicyFileExpanded;
                _fcm.StartMonitoringFile(file, new FileChangeEventHandler(this.OnSecurityPolicyFileChange));
            }
        }

        /*
         * Notification when something in the code-access security policy file changed
         */
        private void OnSecurityPolicyFileChange(Object sender, FileChangeEvent e) {
            // shutdown the app domain
            Debug.Trace("AppDomainFactory", "Shutting down appdomain because code-access security policy file changed");
            string message = FileChangesMonitor.GenerateErrorMessage(e.Action, e.FileName);
            if (message == null) {
                message = "Change in code-access security policy file";
            }
            ShutdownAppDomain(ApplicationShutdownReason.ChangeInSecurityPolicyFile,
                              message);
        }


        // notification when app_offline.htm file changed or created
        private void OnAppOfflineFileChange(Object sender, FileChangeEvent e) {
            // shutdown the app domain
            Debug.Trace("AppOffline", AppOfflineFileName + " changed - shutting down the app domain");
            Debug.Trace("AppDomainFactory", "Shutting down appdomain because " + AppOfflineFileName + " file changed");
            // WOS 1948399: set _userForcedShutdown to avoid DelayNotificationTimeout, since first request has not completed yet in integrated mode;
            SetUserForcedShutdown();
            string message = FileChangesMonitor.GenerateErrorMessage(e.Action, AppOfflineFileName);
            if (message == null) {
                message = "Change in " + AppOfflineFileName;
            }
            ShutdownAppDomain(ApplicationShutdownReason.ConfigurationChange, message);
        }

        internal static String MakeFileUrl(String path) {
            Uri uri = new Uri(path);
            return uri.ToString();
        }

        internal static String GetGacLocation() {

            StringBuilder buf = new StringBuilder(262);
            int iSize = 260;

            // 

            if (UnsafeNativeMethods.GetCachePath(2, buf, ref iSize) >= 0)
                return buf.ToString();
            throw new HttpException(SR.GetString(SR.GetGacLocaltion_failed));
        }


        /*
         * Remove from metabase all read/write/browse permission from certain subdirs
         *
         */
        internal static void RestrictIISFolders(HttpContext context) {
            int ret;

            HttpWorkerRequest wr = context.WorkerRequest;

            Debug.Assert(AppDomainAppId != null);

            // Don't do it if we are not running on IIS
            if (wr == null || !(wr is System.Web.Hosting.ISAPIWorkerRequest)) {
                return;
            }

            // Do it only for IIS 5
#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            if (!(wr is System.Web.Hosting.ISAPIWorkerRequestInProcForIIS6))
#endif // !FEATURE_PAL
 {
                byte[] bufin;
                byte[] bufout = new byte[1];   // Just to keep EcbCallISAPI happy

                bufin = BitConverter.GetBytes(UnsafeNativeMethods.RESTRICT_BIN);
                ret = context.CallISAPI(UnsafeNativeMethods.CallISAPIFunc.RestrictIISFolders, bufin, bufout);
                if (ret != 1) {
                    // Cannot pass back any HR from inetinfo.exe because CSyncPipeManager::GetDataFromIIS
                    // does not support passing back any value when there is an error.
                    Debug.Trace("RestrictIISFolders", "Cannot restrict folder access for '" + AppDomainAppId + "'.");
                }
            }
        }

        //
        // Helper to create instances (public vs. internal/private ctors, see 89781)
        //

        internal static Object CreateNonPublicInstance(Type type) {
            return CreateNonPublicInstance(type, null);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal static Object CreateNonPublicInstance(Type type, Object[] args) {
            return Activator.CreateInstance(
                type,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                args,
                null);
        }

        internal static Object CreatePublicInstance(Type type) {
            return Activator.CreateInstance(type);
        }

#if !DONTUSEFACTORYGENERATOR
        // Cache instances of IWebObjectFactory for each Type, which allow us
        // to instantiate the objects very efficiently, compared to calling
        // Activator.CreateInstance on every call.
        private static FactoryGenerator s_factoryGenerator;
        private static Hashtable s_factoryCache;
        private static bool s_initializedFactory;
        private static object s_factoryLock = new Object();

#endif // DONTUSEFACTORYGENERATOR

        /*
         * Faster implementation of CreatePublicInstance.  It generates bits of IL
         * on the fly to achieve the improve performance.  this should only be used
         * in cases where the number of different types to be created is well bounded.
         * Otherwise, we would create too much IL, which can bloat the process.
         */
        internal static Object FastCreatePublicInstance(Type type) {

#if DONTUSEFACTORYGENERATOR
            return CreatePublicInstance(type);
#else

            // Only use the factory logic if the assembly is in the GAC, to avoid getting
            // assembly conflicts (VSWhidbey 405086)
            if (!type.Assembly.GlobalAssemblyCache) {
                return CreatePublicInstance(type);
            }

            // Create the factory generator on demand
            if (!s_initializedFactory) {

                // Devdiv 90810 - Synchronize to avoid race condition
                lock (s_factoryLock) {
                    if (!s_initializedFactory) {
                        s_factoryGenerator = new FactoryGenerator();

                        // Create the factory cache
                        s_factoryCache = Hashtable.Synchronized(new Hashtable());

                        s_initializedFactory = true;
                    }
                }
            }

            // First, check if it's cached
            IWebObjectFactory factory = (IWebObjectFactory)s_factoryCache[type];

            if (factory == null) {

                Debug.Trace("FastCreatePublicInstance", "Creating generator for type " + type.FullName);

                // Create the object factory
                factory = s_factoryGenerator.CreateFactory(type);

                // Cache the factory
                s_factoryCache[type] = factory;
            }

            return factory.CreateInstance();
#endif // DONTUSEFACTORYGENERATOR
        }

        internal static Object CreatePublicInstance(Type type, Object[] args) {
            if (args == null)
                return Activator.CreateInstance(type);

            return Activator.CreateInstance(type, args);
        }

        static string GetCurrentUserName() {
            try {
                return WindowsIdentity.GetCurrent().Name;
            }
            catch {
                return null;
            }
        }

        void RaiseShutdownWebEventOnce() {
            if (!_shutdownWebEventRaised) {
                lock (this) {
                    if (!_shutdownWebEventRaised) {
                        // Raise Web Event
                        WebBaseEvent.RaiseSystemEvent(this, WebEventCodes.ApplicationShutdown,
                                WebApplicationLifetimeEvent.DetailCodeFromShutdownReason(ShutdownReason));

                        _shutdownWebEventRaised = true;
                    }
                }
            }
        }

        private static string _DefaultPhysicalPathOnMapPathFailure;
        private void RelaxMapPathIfRequired() {
            try {
                RuntimeConfig config = RuntimeConfig.GetAppConfig();
                if (config != null && config.HttpRuntime != null && config.HttpRuntime.RelaxedUrlToFileSystemMapping) {
                    _DefaultPhysicalPathOnMapPathFailure = Path.Combine(_appDomainAppPath, "NOT_A_VALID_FILESYSTEM_PATH");
                }
            } catch {}
        }
        internal static bool IsMapPathRelaxed {
            get {
                return _DefaultPhysicalPathOnMapPathFailure != null;
            }
        }
        internal static string GetRelaxedMapPathResult(string originalResult) {
            if (!IsMapPathRelaxed) // Feature not enabled?
                return originalResult;

            if (originalResult == null) // null is never valid: Return the hard coded default physical path
                return _DefaultPhysicalPathOnMapPathFailure;

            // Does it contain an invalid file-path char?
            if (originalResult.IndexOfAny(s_InvalidPhysicalPathChars) >= 0)
                return _DefaultPhysicalPathOnMapPathFailure;

            // Final check: do the full check to ensure it is valid
            try {
                bool pathTooLong;
                if (FileUtil.IsSuspiciousPhysicalPath(originalResult, out pathTooLong) || pathTooLong)
                    return _DefaultPhysicalPathOnMapPathFailure;
            } catch {
                return _DefaultPhysicalPathOnMapPathFailure;
            }

            // it is valid
            return originalResult;
        }
    }


    public enum ApplicationShutdownReason {

        None = 0,

        HostingEnvironment = 1,

        ChangeInGlobalAsax = 2,

        ConfigurationChange = 3,

        UnloadAppDomainCalled = 4,

        ChangeInSecurityPolicyFile = 5,

        BinDirChangeOrDirectoryRename = 6,

        BrowsersDirChangeOrDirectoryRename = 7,

        CodeDirChangeOrDirectoryRename = 8,

        ResourcesDirChangeOrDirectoryRename = 9,

        IdleTimeout = 10,

        PhysicalApplicationPathChanged = 11,

        HttpRuntimeClose = 12,

        InitializationError = 13,

        MaxRecompilationsReached = 14,

        BuildManagerChange = 15,
    };


}
