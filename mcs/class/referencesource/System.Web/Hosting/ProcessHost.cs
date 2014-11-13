//------------------------------------------------------------------------------
// <copyright file="ProcessHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;


    [ComImport, Guid("0ccd465e-3114-4ca3-ad50-cea561307e93"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProcessHost {

        void StartApplication(
                [In, MarshalAs(UnmanagedType.LPWStr)]
                String appId,
                [In, MarshalAs(UnmanagedType.LPWStr)]
                String appPath,
                [MarshalAs(UnmanagedType.Interface)] out Object runtimeInterface);

        void ShutdownApplication([In, MarshalAs(UnmanagedType.LPWStr)] String appId);

        void Shutdown();

        void EnumerateAppDomains( [MarshalAs(UnmanagedType.Interface)] out IAppDomainInfoEnum appDomainInfoEnum);

    }

    // Used by webengine4.dll for launching Helios applications via ProcessHost.

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("E2A1F244-70EB-483A-ACC8-DE6ACE5BF8B1")]
    internal interface IProcessHostLite {
        [return: MarshalAs(UnmanagedType.Interface)]
        IObjectHandle GetCustomLoader(
            [In, MarshalAs(UnmanagedType.LPWStr)] string appId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string appConfigPath,
            [Out, MarshalAs(UnmanagedType.Interface)] out IProcessHostSupportFunctions supportFunctions,
            [Out, MarshalAs(UnmanagedType.Interface)] out AppDomain newlyCreatedAppDomain);

        void ReportCustomLoaderError(
            [In, MarshalAs(UnmanagedType.LPWStr)] string appId,
            [In] int hr,
            [In, MarshalAs(UnmanagedType.Interface)] AppDomain newlyCreatedAppDomain);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFullExceptionMessage(
            [In] int hr,
            [In] IntPtr pErrorInfo);
    }

    //
    // App domain protocol manager
    // Note that this doesn't provide COM interop
    //

    public interface IAdphManager {

        void StartAppDomainProtocolListenerChannel(
            [In, MarshalAs(UnmanagedType.LPWStr)] String appId,
            [In, MarshalAs(UnmanagedType.LPWStr)] String protocolId,
            IListenerChannelCallback listenerChannelCallback);

        void StopAppDomainProtocolListenerChannel(
            [In, MarshalAs(UnmanagedType.LPWStr)] String appId,
            [In, MarshalAs(UnmanagedType.LPWStr)] String protocolId,
            int listenerChannelId,
            bool immediate);

        void StopAppDomainProtocol(
            [In, MarshalAs(UnmanagedType.LPWStr)] String appId,
            [In, MarshalAs(UnmanagedType.LPWStr)] String protocolId,
            bool immediate);
    }

    [ComImport, Guid("1cc9099d-0a8d-41cb-87d6-845e4f8c4e91"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPphManager {

        void StartProcessProtocolListenerChannel(
            [In, MarshalAs(UnmanagedType.LPWStr)] String protocolId,
            IListenerChannelCallback listenerChannelCallback);

        void StopProcessProtocolListenerChannel(
            [In, MarshalAs(UnmanagedType.LPWStr)] String protocolId,
            int listenerChannelId,
            bool immediate);

        void StopProcessProtocol(
            [In, MarshalAs(UnmanagedType.LPWStr)] String protocolId,
            bool immediate);
    }


    [ComImport, Guid("9d98b251-453e-44f6-9cec-8b5aed970129"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProcessHostIdleAndHealthCheck {

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsIdle();

        void Ping(IProcessPingCallback callback);
    }


    [ComImport, Guid("5BC9C234-6CD7-49bf-A07A-6FDB7F22DFFF"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppDomainInfo {
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetId();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetVirtualPath();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPhysicalPath();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetSiteId();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsIdle();
    }

    [ComImport, Guid("F79648FB-558B-4a09-88F1-1E3BCB30E34F"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppDomainInfoEnum {
        [return: MarshalAs(UnmanagedType.Interface)]
        IAppDomainInfo GetData();

        [return: MarshalAs(UnmanagedType.I4)]
        int Count();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool MoveNext();

        void Reset();
    }

    public class AppDomainInfoEnum : IAppDomainInfoEnum
    {
        private AppDomainInfo[] _appDomainInfos;
        private int _curPos;

        internal AppDomainInfoEnum(AppDomainInfo[] appDomainInfos)
        {
            _appDomainInfos = appDomainInfos;
            _curPos = -1;
        }

        public int Count()
        {
            return _appDomainInfos.Length;
        }

        public IAppDomainInfo GetData()
        {
            return _appDomainInfos[_curPos];
        }

        public bool MoveNext()
        {
            _curPos++;

            if (_curPos >= _appDomainInfos.Length)
            {
                return false;
            }

            return true;
        }

        public void Reset()
        {
            _curPos = -1;
        }
    }

    public class AppDomainInfo : IAppDomainInfo
    {
        private string _id;
        private string _virtualPath;
        private string _physicalPath;
        private int _siteId;
        private bool _isIdle;

        internal AppDomainInfo(string id, string vpath, string physPath, int siteId, bool isIdle)
        {
            _id = id;
            _virtualPath = vpath;
            _physicalPath = physPath;
            _siteId = siteId;
            _isIdle = isIdle;
        }

        public string GetId()
        {
            return _id;
        }

        public string GetVirtualPath()
        {
            return _virtualPath;
        }

        public string GetPhysicalPath()
        {
            return _physicalPath;
        }

        public int GetSiteId()
        {
            return _siteId;
        }

        public bool IsIdle()
        {
            return _isIdle;
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // New for Dev10
    [ComImport, Guid("AE54F424-71BC-4da5-AA2F-8C0CD53496FC"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IApplicationPreloadManager {
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Util",
                         Justification="Name must match IIS COM interface.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="0#Util",
                         Justification="Name must match IIS COM interface.")]
        void SetApplicationPreloadUtil(
            [In, MarshalAs(UnmanagedType.Interface)] IApplicationPreloadUtil preloadUtil);

        void SetApplicationPreloadState(
            [In, MarshalAs(UnmanagedType.LPWStr)] string context,
            [In, MarshalAs(UnmanagedType.LPWStr)] string appId,
            [In, MarshalAs(UnmanagedType.Bool)]  bool enabled);
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Util",
                     Justification="Name must match IIS COM interface.")]
    [ComImport, Guid("940D8ADD-9E40-4475-9A67-2CDCDF57995C"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IApplicationPreloadUtil {

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId="1#",
                         Justification="Parameter kind must match IIS COM interface.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId="2#",
                         Justification="Parameter kind must match IIS COM interface.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId="3#",
                         Justification="Parameter kind must match IIS COM interface.")]
        void GetApplicationPreloadInfo(
            [In, MarshalAs(UnmanagedType.LPWStr)] string context,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool enabled,
            [Out, MarshalAs(UnmanagedType.BStr)] out string startupObjType,
            [Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] parametersForStartupObj);

        void ReportApplicationPreloadFailure(
            [In, MarshalAs(UnmanagedType.LPWStr)] string context,
            [In, MarshalAs(UnmanagedType.U4)] int errorCode,
            [In, MarshalAs(UnmanagedType.LPWStr)] string errorMessage);
    }


    /// <include file='doc\ProcessHost.uex' path='docs/doc[@for="ProcessHost"]/*' />
    public sealed class ProcessHost : MarshalByRefObject,
                                      IProcessHost,
                                      IProcessHostLite,
                                      ICustomRuntimeManager,
                                      IAdphManager, // process protocol handlers manager
                                      IPphManager,  // appdomain protocol handlers manager
                                      IProcessHostIdleAndHealthCheck,
                                      IProcessSuspendListener,
                                      IApplicationPreloadManager {
        private static Object _processHostStaticLock = new Object();
        private static ProcessHost _theProcessHost;

        [ThreadStatic]
        private static KeyValuePair<string, ExceptionDispatchInfo> _customLoaderStartupError;

        private readonly CustomRuntimeManager _customRuntimeManager = new CustomRuntimeManager();

        private IProcessHostSupportFunctions _functions;
        private ApplicationManager _appManager;
        private ProtocolsSection _protocolsConfig;

        // process protocol handlers by prot id
        private Hashtable _protocolHandlers = new Hashtable();
        private IApplicationPreloadUtil _preloadUtil = null;

        private System.Threading.Semaphore _preloadingThrottle = null;

        private ProtocolsSection ProtocolsConfig {
            get {
                if (_protocolsConfig == null) {
                    lock (this) {
                        if (_protocolsConfig == null) {

                            if (HttpConfigurationSystem.IsSet) {
                                _protocolsConfig = RuntimeConfig.GetRootWebConfig().Protocols;
                            } else {
                                Configuration c = WebConfigurationManager.OpenWebConfiguration(null);
                                _protocolsConfig = (ProtocolsSection) c.GetSection("system.web/protocols");
                            }

                        }
                    }
                }
                return _protocolsConfig;
            }
        }

        // ctor only called via GetProcessHost
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reading this particular registry value is safe.")]
        private ProcessHost(IProcessHostSupportFunctions functions) {
            try {
                // remember support functions
                _functions = functions;

                // pass them along to the HostingEnvironment in the default domain
                HostingEnvironment.SupportFunctions = functions;

                // create singleton app manager
                _appManager = ApplicationManager.GetApplicationManager();

                // For M3 we get the throttling limit from the registry.
                // Dev10\Beta1 work item 543420 is to investigate whether we need to get rid of the throttling
                int maxPreloadConcurrency = (int)Misc.GetAspNetRegValue(null, "MaxPreloadConcurrency", 0);
                if (maxPreloadConcurrency > 0) {
                    _preloadingThrottle = new System.Threading.Semaphore(maxPreloadConcurrency, maxPreloadConcurrency);
                }


            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[]
                                                  { SR.GetString(SR.Cant_Create_Process_Host)});
                    Debug.Trace("internal", "ProcessHost::ctor failed with " + e.GetType().FullName + ": " + e.Message + "\r\n" + e.StackTrace);
                }
                throw;
            }
        }

        // ValidateType
        //
        // Validate and Get the Type that is sent in
        //
        // Note: Because ProtocolElement is outside of our assembly we need to do
        //       that here, and because of that we need to hardcode the property
        //       names!!
        //
        private Type ValidateAndGetType( ProtocolElement element,
                                         string          typeName,
                                         Type            assignableType,
                                         string          elementPropertyName ) {
            Type handlerType;

            try {
                 handlerType = Type.GetType(typeName, true /*throwOnError*/);
            }
            catch (Exception e) {

                PropertyInformation propInfo = null;
                string source = String.Empty;
                int lineNum = 0;

                if (element != null  && null != element.ElementInformation) {
                    propInfo = element.ElementInformation.Properties[elementPropertyName];

                    if (null != propInfo) {
                        source = propInfo.Source;
                        lineNum = propInfo.LineNumber;
                    }

                }

                throw new ConfigurationErrorsException(
                            e.Message,
                            e,
                            source,
                            lineNum);
            }

            ConfigUtil.CheckAssignableType( assignableType, handlerType, element, elementPropertyName);

            return handlerType;
        }

        private Type GetAppDomainProtocolHandlerType(String protocolId) {
            Type t = null;

            try {
                // get app domaoin protocol handler type from config
                ProtocolElement configEntry = ProtocolsConfig.Protocols[protocolId];
                if (configEntry == null)
                    throw new ArgumentException(SR.GetString(SR.Unknown_protocol_id, protocolId));

                    t = ValidateAndGetType( configEntry,
                                       configEntry.AppDomainHandlerType,
                                       typeof(AppDomainProtocolHandler),
                                       "AppDomainHandlerType" );
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Invalid_AppDomain_Prot_Type)} );
                }
            }

            return t;
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        // called from ProcessHostFactoryHelper to get ProcessHost
        internal static ProcessHost GetProcessHost(IProcessHostSupportFunctions functions) {
            if (_theProcessHost == null) {
                lock (_processHostStaticLock) {
                    if (_theProcessHost == null) {
                        _theProcessHost = new ProcessHost(functions);
                    }
                }
            }

            return _theProcessHost;
        }

        internal static ProcessHost DefaultHost {
            get {
                return _theProcessHost; // may be null
            }
        }

        internal IProcessHostSupportFunctions SupportFunctions {
            get {
                return _functions;
            }
        }

        //
        // IProcessHostProcessProtocolManager interface implementation
        //

        // starts process protocol handler on demand
        public void StartProcessProtocolListenerChannel(String protocolId, IListenerChannelCallback listenerChannelCallback) {
            try {
                if (protocolId == null)
                    throw new ArgumentNullException("protocolId");

                // validate protocol id
                ProtocolElement configEntry = ProtocolsConfig.Protocols[protocolId];
                if (configEntry == null)
                    throw new ArgumentException(SR.GetString(SR.Unknown_protocol_id, protocolId));

                ProcessProtocolHandler protocolHandler = null;
                Type                   protocolHandlerType = null;

                protocolHandlerType = ValidateAndGetType( configEntry,
                                                          configEntry.ProcessHandlerType,
                                                          typeof(ProcessProtocolHandler),
                                                          "ProcessHandlerType" );

                lock (this) {
                    // lookup or create protocol handler
                    protocolHandler = _protocolHandlers[protocolId] as ProcessProtocolHandler;

                    if (protocolHandler == null) {
                        protocolHandler = (ProcessProtocolHandler)Activator.CreateInstance(protocolHandlerType);
                        _protocolHandlers[protocolId] = protocolHandler;
                    }

                }

                // call the handler to start listenerChannel
                if (protocolHandler != null) {
                    protocolHandler.StartListenerChannel(listenerChannelCallback, this);
                }
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Invalid_Process_Prot_Type)} );
                }
                throw;
            }
        }

        public void StopProcessProtocolListenerChannel(String protocolId, int listenerChannelId, bool immediate) {
            try {
                if (protocolId == null)
                    throw new ArgumentNullException("protocolId");

                ProcessProtocolHandler protocolHandler = null;

                lock (this) {
                    // lookup protocol handler
                    protocolHandler = _protocolHandlers[protocolId] as ProcessProtocolHandler;
                }

                // call the handler to stop listenerChannel
                if (protocolHandler != null) {
                    protocolHandler.StopListenerChannel(listenerChannelId, immediate);
                }
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Stop_Listener_Channel)} );
                }
                throw;
            }
        }


        public void StopProcessProtocol(String protocolId, bool immediate) {
            try {
                if (protocolId == null)
                    throw new ArgumentNullException("protocolId");

                ProcessProtocolHandler protocolHandler = null;

                lock (this) {
                    // lookup and remove protocol handler
                    protocolHandler = _protocolHandlers[protocolId] as ProcessProtocolHandler;

                    if (protocolHandler != null) {
                        _protocolHandlers.Remove(protocolId);
                    }
                }

                if (protocolHandler != null) {
                    protocolHandler.StopProtocol(immediate);
                }
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Stop_Process_Prot)} );
                }
                throw;
            }
        }

        //
        // IAppDomainProtocolManager
        //

        // starts app domain protocol handler on demand (called by process protocol handler

        public void StartAppDomainProtocolListenerChannel(String appId, String protocolId, IListenerChannelCallback listenerChannelCallback) {
            try {
                if (appId == null)
                    throw new ArgumentNullException("appId");
                if (protocolId == null)
                    throw new ArgumentNullException("protocolId");

                ISAPIApplicationHost appHost = CreateAppHost(appId, null);

                // get app domaoin protocol handler type from config
                Type handlerType = GetAppDomainProtocolHandlerType(protocolId);

                AppDomainProtocolHandler handler = null;

                LockableAppDomainContext ac = _appManager.GetLockableAppDomainContext(appId);

                lock (ac) {
                    HostingEnvironmentParameters hostingParameters = new HostingEnvironmentParameters();
                    hostingParameters.HostingFlags = HostingEnvironmentFlags.ThrowHostingInitErrors;
                    
                    PreloadApplicationIfRequired(appId, appHost, hostingParameters, ac);

                    // call app manager to create the handler
                    handler = (AppDomainProtocolHandler)_appManager.CreateObjectInternal(
                        appId, handlerType, appHost, false /*failIfExists*/,
                        hostingParameters);

                    // create a shim object that we can use for proxy unwrapping
                    ListenerAdapterDispatchShim shim = (ListenerAdapterDispatchShim)
                        _appManager.CreateObjectInternal(
                            appId, typeof(ListenerAdapterDispatchShim), appHost, false /*failIfExists*/,
                            hostingParameters);

                    if (null != shim) {
                        shim.StartListenerChannel(handler, listenerChannelCallback);

                        // remove the shim
                        ((IRegisteredObject)shim).Stop(true);
                    }
                    else {
                        throw new HttpException(SR.GetString(SR.Failure_Create_Listener_Shim));
                    }
                }
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Start_AppDomain_Listener)} );
                }
                throw;
            }
        }


        public void StopAppDomainProtocolListenerChannel(String appId, String protocolId, int listenerChannelId, bool immediate) {
            try {
                if (appId == null)
                    throw new ArgumentNullException("appId");
                if (protocolId == null)
                    throw new ArgumentNullException("protocolId");

                // get app domaoin protocol handler type from config
                Type handlerType = GetAppDomainProtocolHandlerType(protocolId);

                AppDomainProtocolHandler handler = null;

                LockableAppDomainContext ac = _appManager.GetLockableAppDomainContext(appId);
                lock (ac) {
                    // call app manager to create the handler
                    handler = (AppDomainProtocolHandler)_appManager.GetObject(appId, handlerType);
                }

                // stop the listenerChannel
                if (handler != null) {
                    handler.StopListenerChannel(listenerChannelId, immediate);
                }
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Stop_AppDomain_Listener)} );
                }
                throw;
            }
        }


        public void StopAppDomainProtocol(String appId, String protocolId, bool immediate) {
            try {
                if (appId == null)
                    throw new ArgumentNullException("appId");
                if (protocolId == null)
                    throw new ArgumentNullException("protocolId");

                // get app domaoin protocol handler type from config
                Type handlerType = GetAppDomainProtocolHandlerType(protocolId);

                AppDomainProtocolHandler handler = null;

                LockableAppDomainContext ac = _appManager.GetLockableAppDomainContext(appId);
                lock (ac) {
                    // call app manager to create the handler
                    handler = (AppDomainProtocolHandler)_appManager.GetObject(appId, handlerType);
                }

                // stop protocol
                if (handler != null) {
                    handler.StopProtocol(immediate);
                }
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Stop_AppDomain_Protocol)} );
                }
                throw;
            }
        }

        public void StartApplication(String appId, String appPath, out Object runtimeInterface)
        {
            try {
                if (appId == null)
                    throw new ArgumentNullException("appId");
                if (appPath == null)
                    throw new ArgumentNullException("appPath");

                Debug.Assert(_functions != null, "_functions != null");

                runtimeInterface = null;

                PipelineRuntime runtime = null;

                //
                //  Fill app a Dictionary with 'binding rules' -- name value string pairs
                //  for app domain creation
                //

                // 


                if (appPath[0] == '.') {
                    System.IO.FileInfo file = new System.IO.FileInfo(appPath);
                    appPath = file.FullName;
                }

                if (!StringUtil.StringEndsWith(appPath, '\\')) {
                    appPath = appPath + "\\";
                }

                // Create new app host of a consistent type
                IApplicationHost appHost = CreateAppHost(appId, appPath);


                //
                // Create the AppDomain and a registered object in it
                //
                LockableAppDomainContext ac = _appManager.GetLockableAppDomainContext(appId);

                lock (ac) {
                    // #1 WOS 1690249: ASP.Net v2.0: ASP.NET stress: 2nd chance exception: Attempted to access an unloaded AppDomain.
                    // if an old AppDomain exists with a PipelineRuntime, remove it from
                    // AppManager._appDomains so that a new AppDomain will be created
                    // #2 WOS 1977425: ASP.NET apps continue recycling after touching machine.config once - this used to initiate shutdown,
                    // but that can cause us to recycle the app repeatedly if we initiate shutdown before IIS initiates shutdown of the
                    // previous app.

                    _appManager.RemoveFromTableIfRuntimeExists(appId, typeof(PipelineRuntime));


                    // Preload (if required) the App Domain before letting the first request to be processed
                    PreloadApplicationIfRequired(appId, appHost, null, ac);

                    try {
                        runtime = (PipelineRuntime)_appManager.CreateObjectInternal(
                            appId,
                            typeof(PipelineRuntime),
                            appHost,
                            true /* failIfExists */,
                            null /* default */ );
                    }
                    catch (AppDomainUnloadedException) {
                        // munch it so we can retry again
                    }

                    if (null != runtime) {
                        runtime.SetThisAppDomainsIsapiAppId(appId);
                        runtime.StartProcessing();
                        runtimeInterface = new ObjectHandle(runtime);
                    }
                }
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Start_Integrated_App)} );
                }
                throw;
            }
        }


        public void ShutdownApplication(String appId) {
            try {
                // call into app manager
                _appManager.ShutdownApplication(appId);
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Stop_Integrated_App)} );
                }
                throw;
            }
        }

        public void Shutdown() {
            try {
                // collect all protocols under lock
                ArrayList protocolList = new ArrayList();
                int       refCount = 0;

                lock (this) {
                    // lookup protocol handler
                    foreach (DictionaryEntry e in _protocolHandlers) {
                        protocolList.Add(e.Value);
                    }

                    _protocolHandlers = new Hashtable();
                }

                // stop all process protocols outside of lock
                foreach (ProcessProtocolHandler p in protocolList) {
                    p.StopProtocol(true);
                }

                // call into app manager to shutdown
                _appManager.ShutdownAll();


                // SupportFunctions interface provided by native layer
                // must be released now.
                // Otherwise the release of the COM object will have
                // to wait for GC. Native layer assumes that after
                // returning from Shutdown there is no reference
                // to the native objects from ProcessHost.
                //
                do {
                    refCount = Marshal.ReleaseComObject( _functions );
                } while( refCount != 0 );

            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_Shutdown_ProcessHost), e.ToString()} );
                }
                throw;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "See comment for why we're calling GC.Collect.")]
        IProcessResumeCallback IProcessSuspendListener.Suspend() {
            object resumeState = _appManager.SuspendAllApplications();
            Action customRuntimeResumeCallback = _customRuntimeManager.SuspendAllCustomRuntimes();

            IProcessResumeCallback callback = new SimpleProcessResumeCallbackDispatcher(() => {
                _appManager.ResumeAllApplications(resumeState);
                if (customRuntimeResumeCallback != null) {
                    customRuntimeResumeCallback();
                }
            });

            // Per CLR team's suggestion, we perform one final GC to try to free
            // any pages that can be reclaimed. Ideally we would do this in the
            // unmanaged layer, but the ICLRGCManager is unavailable to us at the
            // time we would need to perform a collection. So we'll do it here
            // instead.
            GC.Collect();

            return callback;
        }

        ICustomRuntimeRegistrationToken ICustomRuntimeManager.Register(ICustomRuntime customRuntime) {
            Debug.Assert(customRuntime != null);
            return _customRuntimeManager.Register(customRuntime);
        }

        public void EnumerateAppDomains( out IAppDomainInfoEnum appDomainInfoEnum )
        {
            try {
                ApplicationManager appManager = ApplicationManager.GetApplicationManager();
                AppDomainInfo [] infos;

                infos = appManager.GetAppDomainInfos();

                appDomainInfoEnum = new AppDomainInfoEnum(infos);
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_AppDomain_Enum)} );
                }
                throw;
            }
        }

        // IProcessHostIdleAndHealthCheck interface implementation
        public bool IsIdle() {
            bool result = false;

            try {
                result = _appManager.IsIdle();
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_PMH_Idle)} );
                }
                throw;
            }

            return result;
        }


        public void Ping(IProcessPingCallback callback) {
            try {
                if (callback != null)
                    _appManager.Ping(callback);
            }
            catch (Exception e) {
                using (new ProcessImpersonationContext()) {
                    Misc.ReportUnhandledException(e, new string[] {
                                              SR.GetString(SR.Failure_PMH_Ping)} );
                }
                throw;
            }
        }

        // Users cannot provide any call stack that eventually leads to this method, as it will fail at some
        // point with a NullReferenceException. The ASP.NET runtime is the only entity that can call this
        // without something failing, and those call sites are safe.
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "See comment above.")]
        private ISAPIApplicationHost CreateAppHost(string appId, string appPath) {

            //
            // if we have a null physical path, we need
            // to use the PMH to resolve it
            //
            if (String.IsNullOrEmpty(appPath)) {
                string virtualPath;
                string physicalPath;
                string siteName;
                string siteID;

                _functions.GetApplicationProperties(
                        appId,
                        out virtualPath,
                        out physicalPath,
                        out siteName,
                        out siteID);

                //
                // make sure physical app path ends with '\\' and virtual does not
                //
                if (!StringUtil.StringEndsWith(physicalPath, '\\')) {
                    physicalPath = physicalPath + "\\";
                }

                Debug.Assert( !String.IsNullOrEmpty(physicalPath), "!String.IsNullOrEmpty(physicalPath)");
                appPath = physicalPath;
            }

            //
            // Create a new application host
            // This needs to be a coherent type across all
            // protocol types so that we get a consistent
            // environment regardless of which protocol initializes first
            //
            ISAPIApplicationHost appHost = new
                ISAPIApplicationHost(
                        appId,
                        appPath,
                        false, /* validatePhysicalPath */
                        _functions
                        );


            return appHost;
        }

        public void SetApplicationPreloadUtil(IApplicationPreloadUtil applicationPreloadUtil) {

            // Do not allow setting PreloadUtil again if it has already has been set
            if (_preloadUtil != null) {
                throw new InvalidOperationException(SR.GetString(SR.Failure_ApplicationPreloadUtil_Already_Set));
            }

            _preloadUtil = applicationPreloadUtil;
        }

        public void SetApplicationPreloadState(string context, string appId, bool enabled) {
            // Check params
            if (String.IsNullOrEmpty(context)) {
                throw ExceptionUtil.ParameterNullOrEmpty("context");
            }
            if (String.IsNullOrEmpty(appId)) {
                throw ExceptionUtil.ParameterNullOrEmpty("appId");
            }

            // _preloadUtil must be not null if we have an application preload enabled
            if (enabled && _preloadUtil == null) {
                throw new ArgumentException(SR.GetString(SR.Invalid_Enabled_Preload_Parameter), "enabled");
            }

            LockableAppDomainContext ac = _appManager.GetLockableAppDomainContext(appId);

            lock (ac) {
                ac.PreloadContext = context;
                if (enabled) {
                    PreloadApplicationIfRequired(appId, null, null, ac);
                }
            }
        }

        internal static void PreloadApplicationIfNotShuttingdown (string appId, LockableAppDomainContext ac) {
            // If GL_STOP_LISTENING wasn't triggered, the reset is likely due to a configuration change.
            if (ProcessHost.DefaultHost != null && !HostingEnvironment.StopListeningWasCalled) {
                // Start the new app on another thread instead of hijacking the current app unloading thread
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object o) {
                    lock (ac) {
                        try {
                            // NOTE: we don't know what HostingEnvironmentParameters were passed to our previous application instance
                            // so we pass null (default for HTTP activation). We could have cached it in ApplicationContext if needed
                            ProcessHost.DefaultHost.PreloadApplicationIfRequired(appId, null, null, ac);
                        }
                        catch (Exception e) {
                            ProcessHost.DefaultHost.ReportApplicationPreloadFailureWithAssert(
                                ac.PreloadContext,
                                HResults.E_FAIL,
                                Misc.FormatExceptionMessage(
                                    e, new string[] { 
                                        SR.GetString(SR.Failure_Preload_Application_Initialization)}));
                        }
                    }
                }));
            }
        }
        
        // New for Dev10. 
        // creates a new AppDomain, preloads and calls user code in it
        internal void PreloadApplicationIfRequired(
                string appId, 
                IApplicationHost appHostParameter,
                HostingEnvironmentParameters hostingParameters, 
                LockableAppDomainContext ac) {

            // NOTE1:  Must never be called under lock (_appManager)
            // NOTE2:  Must always be called under lock (ac)
    
            // We only need to preload if auto start is enabled and we have not already preloaded (i.e. HostingEnvironment doesn't exist)
            if (_preloadUtil == null || ac.PreloadContext == null || ac.HostEnv != null) {
                return;
            }

            // Get and verify the preload parameters 
            string preloadObjTypeName;
            string[] paramsForStartupObj;
            bool stillEnabled;

            GetApplicationPreloadInfoWithAssert(ac.PreloadContext, out stillEnabled, out preloadObjTypeName, out paramsForStartupObj);
            
            // Dev10: 782385	ASP.NET autostart implementation should be tolerant of empty string for the provider type
            if (!stillEnabled || String.IsNullOrEmpty(preloadObjTypeName)) {
                return;
            }

            // Ready to load the App Domain
            if (_preloadingThrottle != null) {
                // Throttle the number of simultaneously created appdomains
                _preloadingThrottle.WaitOne();
            }

            try {
                
                // Create the app-host and start a new App Domain
                IApplicationHost appHost = (appHostParameter == null) ? CreateAppHost(appId, null) : appHostParameter;
                
                // call app manager to create the PreloadHost
                PreloadHost preloadHostObj = (PreloadHost)_appManager.CreateObjectInternal(
                    appId, 
                    typeof(PreloadHost), 
                    appHost, 
                    true /*failIfExists*/, 
                    hostingParameters);
                
                // Dev10 858421:  File sharing violations on config files cause unnecessary process shutdown in autostart mode
                // 
                // There are race conditions between whoever modifies the config files 
                // and the application config system that reads from the config files
                // These file sharing violation lead to random application initialization failures
                // Service auto-start mode is more vulnerable to these sharing violations because 
                // it starts a new app domain as soon as the file change notification is received
                // and when an error occurs IIS recycles the whole app pool rather than a particular app
                // 
                // In most cases that we see in stress the inner most exception is System.IO.IOException 
                // so if we see this exception we will give it another try before
                // reporting the errors to IIS and recycling the process
                // 

                // Check for I/O exceptions during initialization and retry one time
                Exception appInitEx = preloadHostObj.InitializationException;
                if (GetInnerMostException(appInitEx) is IOException) {
                    try {
                        // prevent ApplicationManager.HostingEnvironmentShutdownInitiated from attempting to preload again
                        ac.RetryingPreload = true;
                        // shutdown old hosting environment
                        ac.HostEnv.InitiateShutdownInternal();
                    }
                    finally {
                        ac.RetryingPreload = false;
                    }

                    // Create the app-host and start a new App Domain
                    appHost = (appHostParameter == null) ? CreateAppHost(appId, null) : appHostParameter;
                
                    // call app manager to create the PreloadHost
                    preloadHostObj = (PreloadHost)_appManager.CreateObjectInternal(
                        appId, 
                        typeof(PreloadHost), 
                        appHost, 
                        true /*failIfExists*/, 
                        hostingParameters);

                    appInitEx = preloadHostObj.InitializationException;
                }

                // Check again for initialization exception and tell IIS to recycle the process
                if (appInitEx != null) {
                    ReportApplicationPreloadFailureWithAssert(
                        ac.PreloadContext,
                        HResults.E_FAIL,
                        Misc.FormatExceptionMessage(
                            appInitEx, new string[] { 
                                SR.GetString(SR.Failure_Preload_Application_Initialization)} ));

                    // we must throw if preload fails because we cannot allow the normal
                    // startup path to continue and attempt to create a HostingEnvironment
                    throw appInitEx;
                }

                // Call preload code in the App Domain
                try {
                    preloadHostObj.CreateIProcessHostPreloadClientInstanceAndCallPreload(preloadObjTypeName, paramsForStartupObj);
                }
                catch (Exception e) {
                    // report errors
                    ReportApplicationPreloadFailureWithAssert(
                        ac.PreloadContext,
                        HResults.E_FAIL,
                        Misc.FormatExceptionMessage(
                            e, new string[] {
                                SR.GetString(SR.Failure_Calling_Preload_Provider)} ).ToString());

                    throw;
                }
            }
            finally {
                if (_preloadingThrottle != null) {
                    _preloadingThrottle.Release();
                }
            }

        }

        private static Exception GetInnerMostException(Exception e) {
            if (e == null) {
                return null;
            }
            while (e.InnerException != null) {
                e = e.InnerException;
            }
            return e;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private void GetApplicationPreloadInfoWithAssert(
                string context, out bool enabled, out string startupObjType, out string[] parametersForStartupObj) {
            _preloadUtil.GetApplicationPreloadInfo(context, out enabled, out startupObjType, out parametersForStartupObj);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private void ReportApplicationPreloadFailureWithAssert(string context, int errorCode, string errorMessage) {
            _preloadUtil.ReportApplicationPreloadFailure(context, errorCode, errorMessage);
        }

        private sealed class SimpleProcessResumeCallbackDispatcher : IProcessResumeCallback {
            private readonly Action _callback;
            public SimpleProcessResumeCallbackDispatcher(Action callback) {
                Debug.Assert(callback != null);
                _callback = callback;
            }

            public void Resume() {
                _callback();
            }
        }

        // The methods below are for propagating error information to ApplicationManager
        // so that we can display a YSOD when the application starts. We rely on the
        // fact that webengine4.dll will immediately call IProcessHost.StartApplication
        // if GetCustomLoader returns null, so TLS is appropriate.

        internal static ExceptionDispatchInfo GetExistingCustomLoaderFailureAndClear(string appId) {
            var copiedError = _customLoaderStartupError;
            if (String.Equals(copiedError.Key, appId, StringComparison.OrdinalIgnoreCase)) {
                _customLoaderStartupError = default(KeyValuePair<string, ExceptionDispatchInfo>);
                return copiedError.Value;
            }
            else {
                return null;
            }
        }

        private static void SetCustomLoaderFailure(string appId, ExceptionDispatchInfo error) {
            _customLoaderStartupError = new KeyValuePair<string, ExceptionDispatchInfo>(appId, error);
        }

        IObjectHandle IProcessHostLite.GetCustomLoader(string appId, string appConfigPath, out IProcessHostSupportFunctions supportFunctions, out AppDomain newlyCreatedAppDomain) {
            supportFunctions = null;
            newlyCreatedAppDomain = null;
            CustomLoaderHelperFunctions helperFunctions = new CustomLoaderHelperFunctions(_functions, appId);
            string appVirtualPath = helperFunctions.AppVirtualPath;

            try {
                string customLoaderAssemblyPhysicalPath = helperFunctions.MapPath("bin/AspNet.Loader.dll");

                if (!File.Exists(customLoaderAssemblyPhysicalPath)) {
                    return null; // no custom loader is in use; fall back to legacy hosting logic
                }

                // Technically there is a race condition between the file existence check above
                // and the assembly load that will take place shortly. We won't worry too much
                // about this since the window is very short and the application shouldn't be
                // modified during this process anyway.

                string appRootPhysicalPath = helperFunctions.AppPhysicalPath;
                string webConfigPhysicalPath = helperFunctions.MapPath("Web.config");
                bool webConfigFileExists = File.Exists(webConfigPhysicalPath);

                // The CustomLoaderHelper class is defined in System.Web.ApplicationServices.dll
                // so that OOB frameworks don't need to take a hardcoded System.Web.dll dependency.
                // There might be weird issues if we try to load a GACed System.Web.dll into the
                // same AppDomain as a bin-deployed System.Web.dll.
                supportFunctions = _functions;
                return CustomLoaderHelper.GetCustomLoader(
                    helperFunctions: helperFunctions,
                    appConfigMetabasePath: appConfigPath,
                    configFilePath: (webConfigFileExists) ? webConfigPhysicalPath : null,
                    customLoaderPhysicalPath: customLoaderAssemblyPhysicalPath,
                    newlyCreatedAppDomain: out newlyCreatedAppDomain);
            }
            catch (Exception ex) {
                SetCustomLoaderFailure(appId, ExceptionDispatchInfo.Capture(ex));
                return null;
            }
        }

        void IProcessHostLite.ReportCustomLoaderError(string appId, int hr, AppDomain newlyCreatedAppDomain) {
            try {
                try {
                    // If the failure originated in managed code (GetCustomLoader), this will actually
                    // result in the original managed exception being rethrown, which is convenient
                    // for us.
                    Marshal.ThrowExceptionForHR(hr);
                }
                finally {
                    // AD wasn't unloaded by CustomLoaderHelper, so kill it here.
                    AppDomain.Unload(newlyCreatedAppDomain);
                }
            }
            catch (Exception ex) {
                SetCustomLoaderFailure(appId, ExceptionDispatchInfo.Capture(ex));
            }
        }

        // Used to extract the full message of an exception, including class name and stack trace.
        string IProcessHostLite.GetFullExceptionMessage(int hr, IntPtr pErrorInfo) {
            // If no IErrorInfo is explicitly specified, provide -1 to suppress the automated
            // GetErrorInfo lookup, as it may have been overwritten and might be irrelevant.
            Exception ex = Marshal.GetExceptionForHR(hr, (pErrorInfo != IntPtr.Zero) ? pErrorInfo : (IntPtr)(-1));

            if (ex != null) {
                return ex.ToString();
            } else {
                // Should never hit this case, but just in case, return a dummy value.
                Debug.Fail("The provided HRESULT should've represented a failure.");
                return String.Empty;
            }
        }

        private sealed class CustomLoaderHelperFunctions : ICustomLoaderHelperFunctions {
            private static readonly bool? _isEnabled = GetIsEnabledValueFromRegistry();

            private readonly IProcessHostSupportFunctions _supportFunctions;

            internal CustomLoaderHelperFunctions(IProcessHostSupportFunctions supportFunctions, string appId) {
                _supportFunctions = supportFunctions;

                string appVirtualPath, appPhysicalPath, siteName, siteId;
                _supportFunctions.GetApplicationProperties(appId, out appVirtualPath, out appPhysicalPath, out siteName, out siteId);

                AppId = appId;
                AppVirtualPath = appVirtualPath;
                AppPhysicalPath = appPhysicalPath;
            }

            public string AppId { get; private set; }
            public string AppPhysicalPath { get; private set; }
            public string AppVirtualPath { get; private set; }
            public bool? CustomLoaderIsEnabled { get { return _isEnabled; } } // true = always enabled, false = always disabled, null = check trust level

            private static bool? GetIsEnabledValueFromRegistry() {
                bool? isEnabled = null;
                try {
                    int valueInRegistry = (int)Misc.GetAspNetRegValue(null, "CustomLoaderEnabled", -1);
                    if (valueInRegistry == 1) {
                        // explicitly enabled
                        isEnabled = true;
                    }
                    else if (valueInRegistry == 0) {
                        // explicitly disabled
                        isEnabled = false;
                    }
                }
                catch {
                    // We don't care about errors, as we'll just query fallback logic.
                }

                return isEnabled;
            }

            public string GetTrustLevel(string appConfigMetabasePath) {
                object retVal;
                int hr = UnsafeIISMethods.MgdGetConfigProperty(appConfigMetabasePath, "system.web/trust", "level", out retVal);
                Marshal.ThrowExceptionForHR(hr);
                return (string)retVal;
            }

            public string MapPath(string relativePath) {
                return _supportFunctions.MapPathInternal(AppId, AppVirtualPath, relativePath);
            }
        }
    }

    internal sealed class ListenerAdapterDispatchShim : MarshalByRefObject, IRegisteredObject {

        void IRegisteredObject.Stop(bool immediate) {
            HostingEnvironment.UnregisterObject(this);
        }

        // this should run in an Hosted app domain (not in the default domain)
        internal void StartListenerChannel( AppDomainProtocolHandler handler, IListenerChannelCallback listenerCallback ) {
            Debug.Assert( HostingEnvironment.IsHosted, "HostingEnvironment.IsHosted" );
            Debug.Assert( null != handler, "null != handler" );

            IListenerChannelCallback unwrappedProxy = MarshalComProxy(listenerCallback);

            Debug.Assert(null != unwrappedProxy, "null != unwrappedProxy");
            if (null != unwrappedProxy && null != handler) {
                handler.StartListenerChannel(unwrappedProxy);
            }
        }

        internal IListenerChannelCallback MarshalComProxy(IListenerChannelCallback defaultDomainCallback) {
            IListenerChannelCallback localProxy = null;

            // get the underlying COM object
            IntPtr pUnk = Marshal.GetIUnknownForObject(defaultDomainCallback);

            // this object isn't a COM object
            if (IntPtr.Zero == pUnk) {
                return null;
            }

            IntPtr ppv = IntPtr.Zero;
            try {
                // QI it for the interface
                Guid g = typeof(IListenerChannelCallback).GUID;

                int hresult = Marshal.QueryInterface(pUnk, ref g, out ppv);
                if (hresult < 0)  {
                    Marshal.ThrowExceptionForHR(hresult);
                }

                // create a RCW we can hold onto in this domain
                // this bumps the ref count so we can drop our refs on the raw interfaces
                localProxy = (IListenerChannelCallback)Marshal.GetObjectForIUnknown(ppv);
            }
            finally {
                // drop our explicit refs and keep the managed instance
                if (IntPtr.Zero != ppv) {
                    Marshal.Release(ppv);
                }
                if (IntPtr.Zero != pUnk) {
                    Marshal.Release(pUnk);
                }
            }

            return localProxy;
        }

    }
}

