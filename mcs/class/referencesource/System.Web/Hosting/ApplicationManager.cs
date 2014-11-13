//------------------------------------------------------------------------------
// <copyright file="ApplicationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Util;

    public enum HostSecurityPolicyResults {
        DefaultPolicy = 0,
        FullTrust = 1,
        AppDomainTrust = 2,
        Nothing = 3
    };

    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
    public class HostSecurityPolicyResolver {
        public virtual HostSecurityPolicyResults ResolvePolicy(Evidence evidence) {
            return HostSecurityPolicyResults.DefaultPolicy;
        }
    }



    internal class LockableAppDomainContext {
        internal HostingEnvironment HostEnv { get; set; }
        internal string PreloadContext { get; set; }
        internal bool RetryingPreload { get; set; }

        internal LockableAppDomainContext() {
        }
    }

    public sealed class ApplicationManager : MarshalByRefObject {

        private const string _regexMatchTimeoutKey = "REGEX_DEFAULT_MATCH_TIMEOUT";
        private static readonly StrongName _mwiV1StrongName = GetMicrosoftWebInfrastructureV1StrongName();

        private static Object _applicationManagerStaticLock = new Object();

        // open count (when last close goes to 0 it shuts down everything)
        int _openCount = 0;
        bool _shutdownInProgress = false;

        // table of app domains (LockableAppDomainContext objects) by app id
        // To simplify per-appdomain synchronization we will never remove LockableAppDomainContext objects from this table even when the AD is unloaded
        // We may need to fix it if profiling shows a noticeable impact on performance
        private Dictionary <string, LockableAppDomainContext> _appDomains = new Dictionary<string, LockableAppDomainContext>(StringComparer.OrdinalIgnoreCase);
        // count of HostingEnvironment instances that is referenced in _appDomains collection
        private int _accessibleHostingEnvCount;

        // could differ from _appDomains or _accessibleHostingEnvCount count (host env is active some time after it is removed)
        private int _activeHostingEnvCount;

        // pending callback to respond to ping (typed as Object to do Interlocked operations)
        private Object _pendingPingCallback;
        // delegate OnRespondToPing
        private WaitCallback _onRespondToPingWaitCallback;

        // single instance of app manager
        private static ApplicationManager _theAppManager;

        // single instance of cache manager
        private static CacheManager _cm;

        // store fatal exception to assist debugging
        private static Exception _fatalException = null;

        internal ApplicationManager() {
            _onRespondToPingWaitCallback = new WaitCallback(this.OnRespondToPingWaitCallback);

            // VSWhidbey 555767: Need better logging for unhandled exceptions (http://support.microsoft.com/?id=911816)
            // We only add a handler in the default domain because it will be notified when an unhandled exception 
            // occurs in ANY domain.  
            // WOS 1983175: (weird) only the handler in the default domain is notified when there is an AV in a native module
            // while we're in a call to MgdIndicateCompletion.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
        }

        private void InitCacheManager(long privateBytesLimit) {
            if (_cm == null) {
                lock (_applicationManagerStaticLock) {
                    if (_cm == null && !_shutdownInProgress) {
                        _cm = new CacheManager(this, privateBytesLimit);
                    }
                }
            }
        }

        private void DisposeCacheManager() {
            if (_cm != null) {
                lock (_applicationManagerStaticLock) {
                    if (_cm != null) {
                        _cm.Dispose();
                        _cm = null;
                    }
                }
            }
        }

        // Each cache must update the total with the difference between it's current size and it's previous size.
        // To reduce cross-domain costs, this also returns the updated total size.
        internal long GetUpdatedTotalCacheSize(long sizeUpdate) {
            CacheManager cm = _cm;
            return (cm != null) ? cm.GetUpdatedTotalCacheSize(sizeUpdate) : 0;
        }

        internal long TrimCaches(int percent) {
            long trimmedOrExpired = 0;
            Dictionary<string, LockableAppDomainContext> apps = CloneAppDomainsCollection();
            foreach (LockableAppDomainContext ac in apps.Values) {
                lock (ac) {
                    HostingEnvironment env = ac.HostEnv;
                    if (_shutdownInProgress) {
                        break;
                    }
                    if (env == null) {
                        continue;
                    }
                    trimmedOrExpired += env.TrimCache(percent);
                }
            }
            return trimmedOrExpired;
        }

        internal bool ShutdownInProgress {
            get {
                return _shutdownInProgress;
            }
        }

        internal static void RecordFatalException(Exception e) {
            RecordFatalException(AppDomain.CurrentDomain, e);
        }

        internal static void RecordFatalException(AppDomain appDomain, Exception e) {
            // store the exception from the first caller to assist debugging
            object originalValue = Interlocked.CompareExchange(ref _fatalException, e, null);

            if (originalValue == null) {
                // create event log entry
                Misc.WriteUnhandledExceptionToEventLog(appDomain, e);
            }
        }

        private static void OnUnhandledException(Object sender, UnhandledExceptionEventArgs eventArgs) {
            // if the CLR is not terminating, ignore the notification
            if (!eventArgs.IsTerminating) {
                return;
            }

            Exception exception = eventArgs.ExceptionObject as Exception;
            if (exception == null) {
                return;
            }

            AppDomain appDomain = sender as AppDomain;
            if (appDomain == null) {
                return;
            }

            RecordFatalException(appDomain, exception);
        }

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        //
        // public ApplicationManager methods
        //


        public static ApplicationManager GetApplicationManager() {
            if (_theAppManager == null) {
                lock (_applicationManagerStaticLock) {
                    if (_theAppManager == null) {
                        if (HostingEnvironment.IsHosted)
                            _theAppManager = HostingEnvironment.GetApplicationManager();

                        if (_theAppManager == null)
                            _theAppManager = new ApplicationManager();
                    }
                }
            }

            return _theAppManager;
        }


        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public void Open() {
            Interlocked.Increment(ref _openCount);
        }


        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void Close() {
            if (Interlocked.Decrement(ref _openCount) > 0)
                return;

            // need to shutdown everything
            ShutdownAll();
        }

        private string CreateSimpleAppID(VirtualPath virtualPath, string physicalPath, string siteName) {
            // Put together some unique app id
            string appId = String.Concat(virtualPath.VirtualPathString, physicalPath);

            if (!String.IsNullOrEmpty(siteName)) {
                appId = String.Concat(appId, siteName);
            }

            return appId.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public IRegisteredObject CreateObject(IApplicationHost appHost, Type type) {
            if (appHost == null) {
                throw new ArgumentNullException("appHost");
            }
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            string appID = CreateSimpleAppID(appHost);
            return CreateObjectInternal(appID, type, appHost, false);            
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public IRegisteredObject CreateObject(String appId, Type type, string virtualPath, string physicalPath, bool failIfExists) {
            return CreateObject(appId, type, virtualPath, physicalPath, failIfExists, false /*throwOnError*/);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public IRegisteredObject CreateObject(String appId, Type type, string virtualPath, string physicalPath,
                                              bool failIfExists, bool throwOnError) {
            // check args
            if (appId == null)
                throw new ArgumentNullException("appId");

            SimpleApplicationHost appHost = new SimpleApplicationHost(VirtualPath.CreateAbsolute(virtualPath), physicalPath);

            // if throw on error flag is set, create hosting parameters accordingly
            HostingEnvironmentParameters hostingParameters = null;

            if (throwOnError) {
                hostingParameters = new HostingEnvironmentParameters();
                hostingParameters.HostingFlags = HostingEnvironmentFlags.ThrowHostingInitErrors;

            }

            // call the internal method
            return CreateObjectInternal(appId, type, appHost, failIfExists, hostingParameters);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        internal IRegisteredObject CreateObjectInternal(String appId, Type type, IApplicationHost appHost, bool failIfExists) {
            // check args
            if (appId == null)
                throw new ArgumentNullException("appId");

            if (type == null)
                throw new ArgumentNullException("type");

            if (appHost == null)
                throw new ArgumentNullException("appHost");

            // call the internal method
            return CreateObjectInternal(appId, type, appHost, failIfExists, null /*hostingParameters*/);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal IRegisteredObject CreateObjectInternal(
                                        String appId,
                                        Type type,
                                        IApplicationHost appHost,
                                        bool failIfExists,
                                        HostingEnvironmentParameters hostingParameters) {

            // check that type is as IRegisteredObject
            if (!typeof(IRegisteredObject).IsAssignableFrom(type))
                throw new ArgumentException(SR.GetString(SR.Not_IRegisteredObject, type.FullName), "type");

            // get hosting environment
            HostingEnvironment env = GetAppDomainWithHostingEnvironment(appId, appHost, hostingParameters);

            // create the managed object in the worker app domain
            // When marshaling Type, the AppDomain must have FileIoPermission to the assembly, which is not
            // always the case, so we marshal the assembly qualified name instead
            ObjectHandle h = env.CreateWellKnownObjectInstance(type.AssemblyQualifiedName, failIfExists);
            return (h != null) ? h.Unwrap() as IRegisteredObject : null;
        }

        internal IRegisteredObject CreateObjectWithDefaultAppHostAndAppId(
                                        String physicalPath,
                                        string virtualPath,
                                        Type type,
                                        out String appId,
                                        out IApplicationHost appHost) {
            return CreateObjectWithDefaultAppHostAndAppId(physicalPath,
                VirtualPath.CreateNonRelative(virtualPath), type, out appId, out appHost);
        }

        internal IRegisteredObject CreateObjectWithDefaultAppHostAndAppId(
                                        String physicalPath,
                                        VirtualPath virtualPath,
                                        Type type,
                                        out String appId,
                                        out IApplicationHost appHost) {

            HostingEnvironmentParameters hostingParameters = new HostingEnvironmentParameters();
            hostingParameters.HostingFlags = HostingEnvironmentFlags.DontCallAppInitialize;

            return CreateObjectWithDefaultAppHostAndAppId(
                        physicalPath,
                        virtualPath,
                        type,
                        false,
                        hostingParameters,
                        out appId,
                        out appHost);
        }

        internal IRegisteredObject CreateObjectWithDefaultAppHostAndAppId(
                                        String physicalPath,
                                        VirtualPath virtualPath,
                                        Type type,
                                        bool failIfExists,
                                        HostingEnvironmentParameters hostingParameters,
                                        out String appId,
                                        out IApplicationHost appHost) {

#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            if (physicalPath == null) { 

                // If the physical path is null, we use an ISAPIApplicationHost based
                // on the virtual path (or metabase id).

                // Make sure the static HttpRuntime is created so isapi assembly can be loaded properly.
                HttpRuntime.ForceStaticInit();

                ISAPIApplicationHost isapiAppHost = new ISAPIApplicationHost(virtualPath.VirtualPathString, null, true, null, hostingParameters.IISExpressVersion);

                appHost = isapiAppHost;
                appId = isapiAppHost.AppId;
                virtualPath = VirtualPath.Create(appHost.GetVirtualPath());
                physicalPath = FileUtil.FixUpPhysicalDirectory(appHost.GetPhysicalPath());
            }
            else {
#endif // !FEATURE_PAL
                // If the physical path was passed in, don't use an Isapi host. Instead,
                // use a simple app host which does simple virtual to physical mappings

                // Put together some unique app id
                appId = CreateSimpleAppID(virtualPath, physicalPath, null);

                appHost = new SimpleApplicationHost(virtualPath, physicalPath);
            }

            string precompTargetPhysicalDir = hostingParameters.PrecompilationTargetPhysicalDirectory;
            if (precompTargetPhysicalDir != null) {
                // Change the appID so we use a different codegendir in precompile for deployment scenario,
                // this ensures we don't use or pollute the regular codegen files.  Also, use different
                // ID's depending on whether the precompilation is Updatable (VSWhidbey 383239)
                if ((hostingParameters.ClientBuildManagerParameter != null) && 
                    (hostingParameters.ClientBuildManagerParameter.PrecompilationFlags & PrecompilationFlags.Updatable) == 0)
                    appId = appId + "_precompile";
                else
                    appId = appId + "_precompile_u";
            }

            return CreateObjectInternal(appId, type, appHost, failIfExists, hostingParameters);
        }


        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public IRegisteredObject GetObject(String appId, Type type) {
            // check args
            if (appId == null)
                throw new ArgumentNullException("appId");
            if (type == null)
                throw new ArgumentNullException("type");

            LockableAppDomainContext ac = GetLockableAppDomainContext(appId);
            lock (ac) {
                HostingEnvironment env = ac.HostEnv;
                if (env == null)
                    return null;

                // find the instance by type
                // When marshaling Type, the AppDomain must have FileIoPermission to the assembly, which is not
                // always the case, so we marshal the assembly qualified name instead
                ObjectHandle h = env.FindWellKnownObject(type.AssemblyQualifiedName);
                return (h != null) ? h.Unwrap() as IRegisteredObject : null;
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public AppDomain GetAppDomain(string appId) {
            if (appId == null) {
                throw new ArgumentNullException("appId");
            }

            LockableAppDomainContext ac = GetLockableAppDomainContext(appId);
            lock (ac) {
                HostingEnvironment env = ac.HostEnv;
                if (env == null) {
                    return null;
                }

                return env.HostedAppDomain;
            }
        }

        public AppDomain GetAppDomain(IApplicationHost appHost) {
            if (appHost == null) {
                throw new ArgumentNullException("appHost");
            }
            string appID = CreateSimpleAppID(appHost);
            return GetAppDomain(appID);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This isn't a dangerous method.")]
        private string CreateSimpleAppID(IApplicationHost appHost) {
            if (appHost == null) {
                throw new ArgumentNullException("appHost");
            }
            return CreateSimpleAppID(VirtualPath.Create(appHost.GetVirtualPath()),
                                     appHost.GetPhysicalPath(), appHost.GetSiteName());
        }
        

        // if a "well-known" object of the specified type already exists in the application,
        // remove the app from the managed application table.  This is
        // used in IIS7 integrated mode when IIS7 determines that it is necessary to create
        // a new application and shutdown the old one.
        internal void RemoveFromTableIfRuntimeExists(String appId, Type runtimeType) {
            // check args
            if (appId == null)
                throw new ArgumentNullException("appId");
            if (runtimeType == null)
                throw new ArgumentNullException("runtimeType");

            LockableAppDomainContext ac = GetLockableAppDomainContext(appId);
            lock (ac) {
                // get hosting environment
                HostingEnvironment env = ac.HostEnv;
                if (env == null)
                    return;

                // find the instance by type
                // When marshaling Type, the AppDomain must have FileIoPermission to the assembly, which is not
                // always the case, so we marshal the assembly qualified name instead
                ObjectHandle h = env.FindWellKnownObject(runtimeType.AssemblyQualifiedName);
                if (h != null)
                {
                    // ensure that it is removed from _appDomains by calling
                    // HostingEnvironmentShutdownInitiated directly.
                    HostingEnvironmentShutdownInitiated(appId, env);
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void StopObject(String appId, Type type) {
            // check args
            if (appId == null)
                throw new ArgumentNullException("appId");
            if (type == null)
                throw new ArgumentNullException("type");

            LockableAppDomainContext ac = GetLockableAppDomainContext(appId);
            lock (ac) {
                HostingEnvironment env = ac.HostEnv;
                if (env != null) {
                    // When marshaling Type, the AppDomain must have FileIoPermission to the assembly, which is not
                    // always the case, so we marshal the assembly qualified name instead
                    env.StopWellKnownObject(type.AssemblyQualifiedName);
                }
            }
        }


        public bool IsIdle() {
            Dictionary<string, LockableAppDomainContext> apps = CloneAppDomainsCollection();

            foreach (LockableAppDomainContext ac in apps.Values) {
                lock (ac) {
                    HostingEnvironment env = ac.HostEnv;
                    bool idle = (null == env) ? true : env.IsIdle();

                    if (!idle)
                        return false;
                }
            }

            return true;
        }


        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void ShutdownApplication(String appId) {
            if (appId == null)
                throw new ArgumentNullException("appId");

            LockableAppDomainContext ac = GetLockableAppDomainContext(appId);
            lock (ac) {
                if (ac.HostEnv != null) {
                    ac.HostEnv.InitiateShutdownInternal();
                }
            }
        }


        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void ShutdownAll() {
            _shutdownInProgress = true;
            Dictionary <string, LockableAppDomainContext> oldTable = null;

            DisposeCacheManager();

            lock (this) {
                oldTable = _appDomains;
                // don't keep references to hosting environments anymore
                _appDomains = new Dictionary<string, LockableAppDomainContext>(StringComparer.OrdinalIgnoreCase);
            }


            foreach (KeyValuePair <string, LockableAppDomainContext> p in oldTable) {
                LockableAppDomainContext ac = p.Value;
                lock (ac) {
                    HostingEnvironment env = ac.HostEnv;
                    if (null != env) {
                        env.InitiateShutdownInternal();
                    }
                }
            }
        
            for (int iter=0; _activeHostingEnvCount > 0 && iter < 3000; iter++) // Wait at most 5 minutes
                Thread.Sleep(100);
        }


        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public ApplicationInfo[] GetRunningApplications() {
            ArrayList appList = new ArrayList();

            Dictionary<string, LockableAppDomainContext> apps = CloneAppDomainsCollection();

            foreach (LockableAppDomainContext ac in apps.Values) {
                lock (ac) {
                    HostingEnvironment env = ac.HostEnv;
                    if (env != null) {
                        appList.Add(env.GetApplicationInfo());
                    }
                }
            }

            int n = appList.Count;
            ApplicationInfo[] result = new ApplicationInfo[n];

            if (n > 0) {
                appList.CopyTo(result);
            }

            return result;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method fails due to serialization issues if not called by ASP.NET.")]
        internal AppDomainInfo [] GetAppDomainInfos()
        {
            ArrayList appList = new ArrayList();
            Dictionary<string, LockableAppDomainContext> apps = CloneAppDomainsCollection();

            foreach (LockableAppDomainContext ac in apps.Values) {
                lock (ac) {
                    HostingEnvironment hostEnv = ac.HostEnv;
                    if (hostEnv == null) {
                        continue;
                    }

                    IApplicationHost appHost = hostEnv.InternalApplicationHost;
                    ApplicationInfo appInfo = hostEnv.GetApplicationInfo();
                    int siteId = 0;

                    if (appHost != null) {
                        try {
                            siteId = Int32.Parse(appHost.GetSiteID(), CultureInfo.InvariantCulture);
                        }
                        catch {
                        }
                    }

                    AppDomainInfo appDomainInfo = new AppDomainInfo(appInfo.ID,
                                                      appInfo.VirtualPath,
                                                      appInfo.PhysicalPath,
                                                      siteId,
                                                      hostEnv.GetIdleValue());

                    appList.Add(appDomainInfo);
                }
            }

            return (AppDomainInfo[]) appList.ToArray(typeof(AppDomainInfo));
        }

        //
        // APIs for the process host to suspend / resume all running applications
        //

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "'this' is never a MBRO proxy object.")]
        internal object SuspendAllApplications() {
            LockableAppDomainContext[] allAppDomainContexts;

            lock (this) {
                allAppDomainContexts = _appDomains.Values.ToArray();
            }

            ApplicationResumeStateContainer[] resumeContainers = Task.WhenAll(allAppDomainContexts.Select(CreateSuspendTask)).Result;
            return resumeContainers;
        }

        private static Task<ApplicationResumeStateContainer> _dummyCompletedSuspendTask = Task.FromResult<ApplicationResumeStateContainer>(null);
        private static Task<ApplicationResumeStateContainer> CreateSuspendTask(LockableAppDomainContext appDomainContext) {
            // dictionary contained a null entry?
            if (appDomainContext == null) {
                return _dummyCompletedSuspendTask;
            }

            HostingEnvironment hostEnv;
            lock (appDomainContext) {
                hostEnv = appDomainContext.HostEnv;
            }

            // Quick check: is this a dummy context that had no associated application?
            if (hostEnv == null) {
                return _dummyCompletedSuspendTask;
            }

            // QUWI since we want to run each application's suspend method in parallel.
            // Unsafe since we don't care about impersonation, identity, etc.
            // Don't use Task.Run since it captures the EC and could execute inline.
            TaskCompletionSource<ApplicationResumeStateContainer> tcs = new TaskCompletionSource<ApplicationResumeStateContainer>();
            ThreadPool.UnsafeQueueUserWorkItem(_ => {

                // We're not locking on the appDomainContext here. The reason for this is two-fold:
                // a) We don't want to cause a potential deadlock issue whereby Suspend could kick
                //    off user code that tries calling InitiateShutdown and thus taking a lock on
                //    appDomainContext.
                // b) It's easier to try calling into the captured HostingEnvironment and just
                //    ---- the "no AD" exception than it is to try to synchronize the Suspend,
                //    Resume, and Stop methods. The CLR protects us from ourselves here.
                //
                // We need to use the captured 'hostEnv' to prevent null refs.

                IntPtr state;
                try {
                    state = hostEnv.SuspendApplication();
                }
                catch (AppDomainUnloadedException) {
                    // AD unloads aren't considered a failure
                    tcs.TrySetResult(null);
                    return;
                }

                tcs.TrySetResult(new ApplicationResumeStateContainer(hostEnv, state));
            }, null);
            return tcs.Task;
        }

        internal void ResumeAllApplications(object state) {
            foreach (var resumeContainer in (ApplicationResumeStateContainer[])state) {
                if (resumeContainer != null) { // could be null if the application went away
                    resumeContainer.Resume();
                }
            }
        }

        //
        // ping implementation
        //

        // called from process host
        internal void Ping(IProcessPingCallback callback) {
            if (callback == null || _pendingPingCallback != null)
                return;

            // remember active callback but only if none is remembered already
            if (Interlocked.CompareExchange(ref _pendingPingCallback, callback, null) == null) {
                // queue a work item to respond to ping
                ThreadPool.QueueUserWorkItem(_onRespondToPingWaitCallback);
            }
        }

        // threadpool callback (also called on some activity from hosting environment)
        internal void OnRespondToPingWaitCallback(Object state) {
            RespondToPingIfNeeded();
        }

        // respond to ping on callback
        internal void RespondToPingIfNeeded() {
            IProcessPingCallback callback = _pendingPingCallback as IProcessPingCallback;

            // make sure we call the callback once
            if (callback != null) {
                if (Interlocked.CompareExchange(ref _pendingPingCallback, null, callback) == callback) {
                    callback.Respond();
                }
            }
        }

        //
        // communication with hosting environments
        //

        internal void HostingEnvironmentActivated(long privateBytesLimit) {
            int count = Interlocked.Increment(ref _activeHostingEnvCount);
            
            // initialize CacheManager once, without blocking
            if (count == 1) {
                InitCacheManager(privateBytesLimit);
            }
        }

        internal void HostingEnvironmentShutdownComplete(String appId, IApplicationHost appHost) {
            try {
                if (appHost != null) {
                    // make sure application host can be GC'd
                    MarshalByRefObject realApplicationHost = appHost as MarshalByRefObject;
                    if (realApplicationHost != null) {
                        RemotingServices.Disconnect(realApplicationHost);
                    }
                }
            }
            finally {
                Interlocked.Decrement(ref _activeHostingEnvCount);
            }
        }

        internal void HostingEnvironmentShutdownInitiated(String appId, HostingEnvironment env) {
            if (!_shutdownInProgress) { // don't bother during shutdown (while enumerating)
                LockableAppDomainContext ac = GetLockableAppDomainContext (appId);

                lock (ac){
                    if (!env.HasBeenRemovedFromAppManagerTable) {
                        env.HasBeenRemovedFromAppManagerTable = true;

                        ac.HostEnv = null;
                        Interlocked.Decrement(ref _accessibleHostingEnvCount);

                        // Autorestart the application right away
                        if (ac.PreloadContext != null && !ac.RetryingPreload) {
                            ProcessHost.PreloadApplicationIfNotShuttingdown(appId, ac);
                        }
                    }
                }
            }
        }

        internal int AppDomainsCount {
            get { return _accessibleHostingEnvCount; }
        }


        internal void ReduceAppDomainsCount(int limit) {
            // 




            Dictionary<string, LockableAppDomainContext> apps = CloneAppDomainsCollection();
            while (_accessibleHostingEnvCount >= limit && !_shutdownInProgress)
            {
                LockableAppDomainContext bestCandidateForShutdown = null;
                int bestCandidateLruScore = 0;
                
                foreach (LockableAppDomainContext ac in apps.Values) {
                    // Don't lock on LockableAppDomainContext before we check that ac.HostEnv != null.
                    // Otherwise we may end up with a deadlock between 2 app domains trying to unload each other
                    HostingEnvironment h = ac.HostEnv;
                    if (h == null) {
                        continue;
                    }
                    lock (ac) {
                        h = ac.HostEnv;

                        // Avoid ---- by checking again under lock
                        if (h == null) {
                            continue;
                        }
                        int newLruScore = h.LruScore;

                        if (bestCandidateForShutdown == null || bestCandidateForShutdown.HostEnv == null || 
                                newLruScore < bestCandidateLruScore) {

                            bestCandidateLruScore = newLruScore;
                            bestCandidateForShutdown = ac;
                        }
                    }
                }

                if (bestCandidateForShutdown == null)
                    break;

                lock (bestCandidateForShutdown) {
                    if (bestCandidateForShutdown.HostEnv != null) {
                        bestCandidateForShutdown.HostEnv.InitiateShutdownInternal();
                    }
                }
            }
        }

        //
        // helper to support legacy APIs (AppHost.CreateAppHost)
        //

        internal ObjectHandle CreateInstanceInNewWorkerAppDomain(
                                Type type,
                                String appId,
                                VirtualPath virtualPath,
                                String physicalPath) {

            Debug.Trace("AppManager", "CreateObjectInNewWorkerAppDomain, type=" + type.FullName);

            IApplicationHost appHost = new SimpleApplicationHost(virtualPath, physicalPath);

            HostingEnvironmentParameters hostingParameters = new HostingEnvironmentParameters();
            hostingParameters.HostingFlags = HostingEnvironmentFlags.HideFromAppManager;

            HostingEnvironment env = CreateAppDomainWithHostingEnvironmentAndReportErrors(appId, appHost, hostingParameters);
            // When marshaling Type, the AppDomain must have FileIoPermission to the assembly, which is not
            // always the case, so we marshal the assembly qualified name instead
            return env.CreateInstance(type.AssemblyQualifiedName);
        }

        //
        // helpers to facilitate app domain creation
        //
        private HostingEnvironment GetAppDomainWithHostingEnvironment(String appId, IApplicationHost appHost, HostingEnvironmentParameters hostingParameters) {
            LockableAppDomainContext ac = GetLockableAppDomainContext (appId);

            lock (ac) {
                HostingEnvironment env = ac.HostEnv;

                if (env != null) {
                    try {
                        env.IsUnloaded();
                    } catch(AppDomainUnloadedException) {
                        env = null;
                    }
                }
                if (env == null) {
                    env = CreateAppDomainWithHostingEnvironmentAndReportErrors(appId, appHost, hostingParameters);
                    ac.HostEnv = env;
                    Interlocked.Increment(ref _accessibleHostingEnvCount);
                }

                return env;
            }
           
        }

        private HostingEnvironment CreateAppDomainWithHostingEnvironmentAndReportErrors(
                                        String appId,
                                        IApplicationHost appHost,
                                        HostingEnvironmentParameters hostingParameters) {
            try {
                return CreateAppDomainWithHostingEnvironment(appId, appHost, hostingParameters);
            }
            catch (Exception e) {
                Misc.ReportUnhandledException(e, new string[] {SR.GetString(SR.Failed_to_initialize_AppDomain), appId});
                throw;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "We carefully control the callers.")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Boolean.TryParse(System.String,System.Boolean@)", Justification = "Sets parameter to default(bool) on conversion failure, which is semantic we need.")]
        private HostingEnvironment CreateAppDomainWithHostingEnvironment(
                                                String appId,
                                                IApplicationHost appHost,
                                                HostingEnvironmentParameters hostingParameters) {

            String physicalPath = appHost.GetPhysicalPath();
            if (!StringUtil.StringEndsWith(physicalPath, Path.DirectorySeparatorChar))
                physicalPath = physicalPath + Path.DirectorySeparatorChar;

            String domainId = ConstructAppDomainId(appId);
            String appName = (StringUtil.GetStringHashCode(String.Concat(appId.ToLower(CultureInfo.InvariantCulture),
                physicalPath.ToLower(CultureInfo.InvariantCulture)))).ToString("x", CultureInfo.InvariantCulture);
            VirtualPath virtualPath = VirtualPath.Create(appHost.GetVirtualPath());

            Debug.Trace("AppManager", "CreateAppDomainWithHostingEnvironment, path=" + physicalPath + "; appId=" + appId + "; domainId=" + domainId);

            IDictionary bindings = new Hashtable(20);
            AppDomainSetup setup = new AppDomainSetup();
            AppDomainSwitches switches = new AppDomainSwitches();
            PopulateDomainBindings(domainId, appId, appName, physicalPath, virtualPath, setup, bindings);

            //  Create the app domain

            AppDomain appDomain = null;
            Dictionary<string, object> appDomainAdditionalData = new Dictionary<string, object>();
            Exception appDomainCreationException = null;

            string siteID = appHost.GetSiteID();
            string appSegment = virtualPath.VirtualPathStringNoTrailingSlash;
            bool inClientBuildManager = false;
            Configuration appConfig = null;
            PolicyLevel policyLevel = null;
            PermissionSet permissionSet = null;
            List<StrongName> fullTrustAssemblies = new List<StrongName>();
            string[] defaultPartialTrustVisibleAssemblies = new[] { "System.Web, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293",
                                                                    "System.Web.Extensions, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                                                    "System.Web.Abstractions, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                                                    "System.Web.Routing, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                                                    "System.Web.DynamicData, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                                                    "System.Web.DataVisualization, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                                                    "System.Web.ApplicationServices, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9" };

            Exception appDomainStartupConfigurationException = null;
            ImpersonationContext ictxConfig = null;
            IntPtr uncTokenConfig = IntPtr.Zero;
            HostingEnvironmentFlags hostingFlags = HostingEnvironmentFlags.Default;
            if (hostingParameters != null) {
                hostingFlags = hostingParameters.HostingFlags;
                if ((hostingFlags & HostingEnvironmentFlags.ClientBuildManager) != 0) {
                    inClientBuildManager = true;
                    // The default hosting policy in VS has changed (from MultiDomainHost to MultiDomain), 
                    // so we need to specify explicitly to allow generated assemblies 
                    // to be unloaded subsequently. (Dev10 bug)
                    setup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                }
            }
            try {
                bool requireHostExecutionContextManager = false;
                bool requireHostSecurityManager = false;

                uncTokenConfig = appHost.GetConfigToken();
                if (uncTokenConfig != IntPtr.Zero) {
                    ictxConfig = new ImpersonationContext(uncTokenConfig);
                }

                try {
                    // Did the custom loader fail to load?
                    ExceptionDispatchInfo customLoaderException = ProcessHost.GetExistingCustomLoaderFailureAndClear(appId);
                    if (customLoaderException != null) {
                        customLoaderException.Throw();
                    }

                    // DevDiv #392603 - disallow running applications when string hash code randomization is enabled
                    if (EnvironmentInfo.IsStringHashCodeRandomizationEnabled) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Require_stable_string_hash_codes));
                    }

                    bool skipAdditionalConfigChecks = false;
                    if (inClientBuildManager && hostingParameters.IISExpressVersion != null) {
                        permissionSet = new PermissionSet(PermissionState.Unrestricted);
                        setup.PartialTrustVisibleAssemblies = defaultPartialTrustVisibleAssemblies;
                        appConfig = GetAppConfigIISExpress(siteID, appSegment, hostingParameters.IISExpressVersion);
                        skipAdditionalConfigChecks = true;
                    }
                    else {
                        //Hosted by IIS, we already have an IISMap.
                        if (appHost is ISAPIApplicationHost) {
                            string cacheKey = System.Web.Caching.CacheInternal.PrefixMapPath + siteID + virtualPath.VirtualPathString;
                            MapPathCacheInfo cacheInfo = (MapPathCacheInfo)HttpRuntime.CacheInternal.Remove(cacheKey);
                            appConfig = WebConfigurationManager.OpenWebConfiguration(appSegment, siteID);
                        }
                        // For non-IIS hosting scenarios, we need to get config map from application host in a generic way.
                        else {
                            appConfig = GetAppConfigGeneric(appHost, siteID, appSegment, virtualPath, physicalPath);
                        }
                    }

                    HttpRuntimeSection httpRuntimeSection = (HttpRuntimeSection)appConfig.GetSection("system.web/httpRuntime");
                    if (httpRuntimeSection == null) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_section_not_present, "httpRuntime"));
                    }

                    // DevDiv #403846 - Change certain config defaults if <httpRuntime targetFramework="4.5" /> exists in config.
                    // We store this information in the AppDomain data because certain configuration sections (like <compilation>)
                    // are loaded before config is "baked" in the child AppDomain, and if we make <compilation> and other sections
                    // dependent on <httpRuntime> which may not have been loaded yet, we risk introducing ----s. Putting this value
                    // in the AppDomain data guarantees that it is available before the first call to the config system.
                    FrameworkName targetFrameworkName = httpRuntimeSection.GetTargetFrameworkName();
                    if (targetFrameworkName != null) {
                        appDomainAdditionalData[BinaryCompatibility.TargetFrameworkKey] = targetFrameworkName;
                    }

                    if (!skipAdditionalConfigChecks) {
                        // DevDiv #71268 - Add <httpRuntime defaultRegexMatchTimeout="HH:MM:SS" /> configuration attribute
                        if (httpRuntimeSection.DefaultRegexMatchTimeout != TimeSpan.Zero) {
                            appDomainAdditionalData[_regexMatchTimeoutKey] = httpRuntimeSection.DefaultRegexMatchTimeout;
                        }

                        // DevDiv #258274 - Add support for CLR quirks mode to ASP.NET
                        if (targetFrameworkName != null) {
                            setup.TargetFrameworkName = targetFrameworkName.ToString();
                        }

                        // DevDiv #286354 - Having a Task-friendly SynchronizationContext requires overriding the AppDomain's HostExecutionContextManager.
                        // DevDiv #403846 - If we can't parse the <appSettings> switch, use the <httpRuntime/targetFramework> setting to determine the default.
                        AppSettingsSection appSettingsSection = appConfig.AppSettings;
                        KeyValueConfigurationElement useTaskFriendlySynchronizationContextElement = appSettingsSection.Settings["aspnet:UseTaskFriendlySynchronizationContext"];
                        if (!(useTaskFriendlySynchronizationContextElement != null && Boolean.TryParse(useTaskFriendlySynchronizationContextElement.Value, out requireHostExecutionContextManager))) {
                            requireHostExecutionContextManager = new BinaryCompatibility(targetFrameworkName).TargetsAtLeastFramework45 ? true : false;
                        }

                        // DevDiv #248126 - Allow configuration of FileChangeMonitor behavior
                        if (httpRuntimeSection.FcnMode != FcnMode.NotSet) {
                            if (hostingParameters == null) {
                                hostingParameters = new HostingEnvironmentParameters();
                            }
                            hostingParameters.FcnMode = httpRuntimeSection.FcnMode;
                        }

                        // DevDiv #322858 - Allow FileChangesMonitor to skip reading DACLs as a perf improvement
                        KeyValueConfigurationElement disableFcnDaclReadElement = appSettingsSection.Settings["aspnet:DisableFcnDaclRead"];
                        if (disableFcnDaclReadElement != null) {
                            bool skipReadingAndCachingDacls;
                            Boolean.TryParse(disableFcnDaclReadElement.Value, out skipReadingAndCachingDacls);
                            if (skipReadingAndCachingDacls) {
                                if (hostingParameters == null) {
                                    hostingParameters = new HostingEnvironmentParameters();
                                }
                                hostingParameters.FcnSkipReadAndCacheDacls = true;
                            }
                        }

                        // If we were launched from a development environment, we might want to enable the application to do things
                        // it otherwise wouldn't normally allow, such as enabling an administrative control panel. For security reasons,
                        // we only do this check if <deployment retail="false" /> [the default value] is specified, since the
                        // <deployment> element can only be set at machine-level in a hosted environment.
                        DeploymentSection deploymentSection = (DeploymentSection)appConfig.GetSection("system.web/deployment");
                        if (deploymentSection != null && !deploymentSection.Retail && EnvironmentInfo.WasLaunchedFromDevelopmentEnvironment) {
                            appDomainAdditionalData[".devEnvironment"] = true;

                            // DevDiv #275724 - Allow LocalDB support in partial trust scenarios
                            // Normally LocalDB requires full trust since it's the equivalent of unmanaged code execution. If this is
                            // a development environment and not a retail deployment, we can assume that the user developing the
                            // application is actually in charge of the host, so we can trust him with LocalDB execution.
                            // Technically this also means that the developer could have set <trust level="Full" /> in his application,
                            // but he might want to deploy his application on a Medium-trust server and thus test how the rest of his
                            // application works in a partial trust environment. (He would use SQL in production, whch is safe in
                            // partial trust.)
                            appDomainAdditionalData["ALLOW_LOCALDB_IN_PARTIAL_TRUST"] = true;
                        }

                        TrustSection trustSection = (TrustSection)appConfig.GetSection("system.web/trust");
                        if (trustSection == null || String.IsNullOrEmpty(trustSection.Level)) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_section_not_present, "trust"));
                        }

                        switches.UseLegacyCas = trustSection.LegacyCasModel;

                        if (inClientBuildManager) {
                            permissionSet = new PermissionSet(PermissionState.Unrestricted);
                            setup.PartialTrustVisibleAssemblies = defaultPartialTrustVisibleAssemblies;
                        }
                        else {
                            if (!switches.UseLegacyCas) {
                                if (trustSection.Level == "Full") {
                                    permissionSet = new PermissionSet(PermissionState.Unrestricted);
                                    setup.PartialTrustVisibleAssemblies = defaultPartialTrustVisibleAssemblies;
                                }
                                else {
                                    SecurityPolicySection securityPolicySection = (SecurityPolicySection)appConfig.GetSection("system.web/securityPolicy");
                                    CompilationSection compilationSection = (CompilationSection)appConfig.GetSection("system.web/compilation");
                                    FullTrustAssembliesSection fullTrustAssembliesSection = (FullTrustAssembliesSection)appConfig.GetSection("system.web/fullTrustAssemblies");
                                    policyLevel = GetPartialTrustPolicyLevel(trustSection, securityPolicySection, compilationSection, physicalPath, virtualPath);
                                    permissionSet = policyLevel.GetNamedPermissionSet(trustSection.PermissionSetName);
                                    if (permissionSet == null) {
                                        throw new ConfigurationErrorsException(SR.GetString(SR.Permission_set_not_found, trustSection.PermissionSetName));
                                    }

                                    // read full trust assemblies and populate the strong name list
                                    if (fullTrustAssembliesSection != null) {
                                        FullTrustAssemblyCollection fullTrustAssembliesCollection = fullTrustAssembliesSection.FullTrustAssemblies;
                                        if (fullTrustAssembliesCollection != null) {
                                            fullTrustAssemblies.AddRange(from FullTrustAssembly fta in fullTrustAssembliesCollection
                                                                         select CreateStrongName(fta.AssemblyName, fta.Version, fta.PublicKey));
                                        }
                                    }

                                    // DevDiv #27645 - We need to add future versions of Microsoft.Web.Infrastructure to <fullTrustAssemblies> so that ASP.NET
                                    // can version out-of-band releases. We should only do this if V1 of M.W.I is listed.
                                    if (fullTrustAssemblies.Contains(_mwiV1StrongName)) {
                                        fullTrustAssemblies.AddRange(CreateFutureMicrosoftWebInfrastructureStrongNames());
                                    }

                                    // Partial-trust AppDomains using a non-legacy CAS model require our special HostSecurityManager
                                    requireHostSecurityManager = true;
                                }
                            }
                            if (trustSection.Level != "Full") {
                                PartialTrustVisibleAssembliesSection partialTrustVisibleAssembliesSection = (PartialTrustVisibleAssembliesSection)appConfig.GetSection("system.web/partialTrustVisibleAssemblies");
                                string[] partialTrustVisibleAssemblies = null;
                                if (partialTrustVisibleAssembliesSection != null) {
                                    PartialTrustVisibleAssemblyCollection partialTrustVisibleAssembliesCollection = partialTrustVisibleAssembliesSection.PartialTrustVisibleAssemblies;
                                    if (partialTrustVisibleAssembliesCollection != null && partialTrustVisibleAssembliesCollection.Count != 0) {
                                        partialTrustVisibleAssemblies = new string[partialTrustVisibleAssembliesCollection.Count + defaultPartialTrustVisibleAssemblies.Length];
                                        for (int i = 0; i < partialTrustVisibleAssembliesCollection.Count; i++) {
                                            partialTrustVisibleAssemblies[i] = partialTrustVisibleAssembliesCollection[i].AssemblyName +
                                                ", PublicKey=" +
                                                NormalizePublicKeyBlob(partialTrustVisibleAssembliesCollection[i].PublicKey);
                                        }
                                        defaultPartialTrustVisibleAssemblies.CopyTo(partialTrustVisibleAssemblies, partialTrustVisibleAssembliesCollection.Count);
                                    }
                                }
                                if (partialTrustVisibleAssemblies == null) {
                                    partialTrustVisibleAssemblies = defaultPartialTrustVisibleAssemblies;
                                }
                                setup.PartialTrustVisibleAssemblies = partialTrustVisibleAssemblies;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    appDomainStartupConfigurationException = e;
                    permissionSet = new PermissionSet(PermissionState.Unrestricted);
                }

                // Set the AppDomainManager if needed
                Type appDomainManagerType = AspNetAppDomainManager.GetAspNetAppDomainManagerType(requireHostExecutionContextManager, requireHostSecurityManager);
                if (appDomainManagerType != null) {
                    setup.AppDomainManagerType = appDomainManagerType.FullName;
                    setup.AppDomainManagerAssembly = appDomainManagerType.Assembly.FullName;
                }

                // Apply compatibility switches
                switches.Apply(setup);

                try {
                    if (switches.UseLegacyCas) {
                        appDomain = AppDomain.CreateDomain(domainId,
#if FEATURE_PAL // FEATURE_PAL: hack to avoid non-supported hosting features
                                                           null,
#else // FEATURE_PAL
GetDefaultDomainIdentity(),
#endif // FEATURE_PAL
setup);
                    }
                    else {
                        appDomain = AppDomain.CreateDomain(domainId,
#if FEATURE_PAL // FEATURE_PAL: hack to avoid non-supported hosting features
                                                           null,
#else // FEATURE_PAL
GetDefaultDomainIdentity(),
#endif // FEATURE_PAL
setup,
                                                           permissionSet,
                                                           fullTrustAssemblies.ToArray() /* fully trusted assemblies list: empty list means only trust GAC assemblies */);
                    }
                    foreach (DictionaryEntry e in bindings)
                        appDomain.SetData((String)e.Key, (String)e.Value);
                    foreach (var entry in appDomainAdditionalData)
                        appDomain.SetData(entry.Key, entry.Value);
                }
                catch (Exception e) {
                    Debug.Trace("AppManager", "AppDomain.CreateDomain failed", e);
                    appDomainCreationException = e;
                }
            }
            finally {
                if (ictxConfig != null) {
                    ictxConfig.Undo();
                    ictxConfig = null;
                }
                if (uncTokenConfig != IntPtr.Zero) {
                    UnsafeNativeMethods.CloseHandle(uncTokenConfig);
                    uncTokenConfig = IntPtr.Zero;
                }
            }

            if (appDomain == null) {
                throw new SystemException(SR.GetString(SR.Cannot_create_AppDomain), appDomainCreationException);
            }

            // Create hosting environment in the new app domain

            Type hostType = typeof(HostingEnvironment);
            String module = hostType.Module.Assembly.FullName;
            String typeName = hostType.FullName;
            ObjectHandle h = null;

            // impersonate UNC identity, if any
            ImpersonationContext ictx = null;
            IntPtr uncToken = IntPtr.Zero;

            //
            // fetching config can fail due to a ---- with the 
            // native config reader
            // if that has happened, force a flush
            //
            int maxRetries = 10;
            int numRetries = 0;

            while (numRetries < maxRetries) {
                try {
                    uncToken = appHost.GetConfigToken();
                    // no throw, so break
                    break;
                }
                catch (InvalidOperationException) {
                    numRetries++;
                    System.Threading.Thread.Sleep(250);
                }
            }


            if (uncToken != IntPtr.Zero) {
                try {
                    ictx = new ImpersonationContext(uncToken);
                }
                catch {
                }
                finally {
                    UnsafeNativeMethods.CloseHandle(uncToken);
                }
            }

            try {

                // Create the hosting environment in the app domain
#if DBG
                try {
                    h = Activator.CreateInstance(appDomain, module, typeName);
                }
                catch (Exception e) {
                    Debug.Trace("AppManager", "appDomain.CreateInstance failed; identity=" + System.Security.Principal.WindowsIdentity.GetCurrent().Name, e);
                    throw;
                }
#else
                h = Activator.CreateInstance(appDomain, module, typeName);
#endif
            }
            finally {
                // revert impersonation
                if (ictx != null)
                    ictx.Undo();

                if (h == null) {
                    AppDomain.Unload(appDomain);
                }
            }

            HostingEnvironment env = (h != null) ? h.Unwrap() as HostingEnvironment : null;

            if (env == null)
                throw new SystemException(SR.GetString(SR.Cannot_create_HostEnv));

            // initialize the hosting environment
            IConfigMapPathFactory configMapPathFactory = appHost.GetConfigMapPathFactory();
            if (appDomainStartupConfigurationException == null) {
                env.Initialize(this, appHost, configMapPathFactory, hostingParameters, policyLevel);
            }
            else {
                env.Initialize(this, appHost, configMapPathFactory, hostingParameters, policyLevel, appDomainStartupConfigurationException);
            }
            return env;
        }

        private static string NormalizePublicKeyBlob(string publicKey) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < publicKey.Length; i++) {
                if (!Char.IsWhiteSpace(publicKey[i])) {
                    sb.Append(publicKey[i]);
                }
            }
            publicKey = sb.ToString();
            return publicKey;
        }

        private static StrongName CreateStrongName(string assemblyName, string version, string publicKeyString) {
            byte[] publicKey = null;
            StrongName strongName = null;
            publicKeyString = NormalizePublicKeyBlob(publicKeyString);
            int publicKeySize = publicKeyString.Length / 2;
            publicKey = new byte[publicKeySize];
            for (int i = 0; i < publicKeySize; i++) {
                publicKey[i] = Byte.Parse(publicKeyString.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            StrongNamePublicKeyBlob keyBlob = new StrongNamePublicKeyBlob(publicKey);
            strongName = new StrongName(keyBlob, assemblyName, new Version(version));
            return strongName;
        }

        // For various reasons, we can't add any entries to the <fullTrustAssemblies> list until .NET 5, which at the time of this writing is a few
        // years out. But since ASP.NET releases projects out-of-band from the .NET Framework as a whole (using Microsoft.Web.Infrasturcture), we
        // need to be sure that future versions of M.W.I have the ability to assert full trust. This code works by seeing if M.W.I v1 is in the
        // <fullTrustAssemblies> list, and if it is then v2 - v10 are implicitly added. If v1 is not present in the <fTA> list, then we'll not
        // treat v2 - v10 as implicitly added. See DevDiv #27645 for more information.

        private static StrongName GetMicrosoftWebInfrastructureV1StrongName() {
            return CreateStrongName(
                    assemblyName: "Microsoft.Web.Infrastructure",
                    version: "1.0.0.0",
                    publicKeyString: "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9");
        }

        private static IEnumerable<StrongName> CreateFutureMicrosoftWebInfrastructureStrongNames() {
            string asmName = _mwiV1StrongName.Name;
            StrongNamePublicKeyBlob publicKey = _mwiV1StrongName.PublicKey;
            for (int i = 2; i <= 10; i++) {
                yield return new StrongName(publicKey, asmName, new Version(i, 0, 0, 0));
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "We carefully control this method's caller.")]
        private static PolicyLevel GetPartialTrustPolicyLevel(
                TrustSection trustSection, SecurityPolicySection securityPolicySection,
                CompilationSection compilationSection, string physicalPath, VirtualPath virtualPath) {
            if (securityPolicySection == null || securityPolicySection.TrustLevels[trustSection.Level] == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Unable_to_get_policy_file, trustSection.Level), String.Empty, 0);
            }
            String configFile = (String)securityPolicySection.TrustLevels[trustSection.Level].PolicyFileExpanded;
            if (configFile == null || !FileUtil.FileExists(configFile)) {
                throw new HttpException(SR.GetString(SR.Unable_to_get_policy_file, trustSection.Level));
            }
            PolicyLevel policyLevel = null;
            String appDir = FileUtil.RemoveTrailingDirectoryBackSlash(physicalPath);
            String appDirUrl = HttpRuntime.MakeFileUrl(appDir);

            // setup $CodeGen$ replacement
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
            }
            else {
                tempDirectory = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), HttpRuntime.codegenDirName);
            }
            // If we don't have write access to the codegen dir, use the TEMP dir instead.
            // This will allow non-admin users to work in hosting scenarios (e.g. Venus, aspnet_compiler)
            if (!System.Web.UI.Util.HasWriteAccessToDirectory(tempDirectory)) {
                // Don't do this if we're in a service (!UserInteractive), as TEMP
                // could point to unwanted places.
                if (!Environment.UserInteractive) {
                    throw new HttpException(SR.GetString(SR.No_codegen_access,
                        System.Web.UI.Util.GetCurrentAccountName(), tempDirectory));
                }
                tempDirectory = Path.GetTempPath();
                Debug.Assert(System.Web.UI.Util.HasWriteAccessToDirectory(tempDirectory));
                tempDirectory = Path.Combine(tempDirectory, HttpRuntime.codegenDirName);
            }
            String simpleAppName = System.Web.Hosting.AppManagerAppDomainFactory.ConstructSimpleAppName(
                VirtualPath.GetVirtualPathStringNoTrailingSlash(virtualPath));
            String binDir = Path.Combine(tempDirectory, simpleAppName);
            binDir = FileUtil.RemoveTrailingDirectoryBackSlash(binDir);
            String binDirUrl = HttpRuntime.MakeFileUrl(binDir);

            String originUrl = trustSection.OriginUrl;
            FileStream file = new FileStream(configFile, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file, Encoding.UTF8);
            String strFileData = reader.ReadToEnd();
            reader.Close();
            strFileData = strFileData.Replace("$AppDir$", appDir);
            strFileData = strFileData.Replace("$AppDirUrl$", appDirUrl);
            strFileData = strFileData.Replace("$CodeGen$", binDirUrl);
            if (originUrl == null)
                originUrl = String.Empty;
            strFileData = strFileData.Replace("$OriginHost$", originUrl);
            String gacLocation = null;
            if (strFileData.IndexOf("$Gac$", StringComparison.Ordinal) != -1) {
                gacLocation = HttpRuntime.GetGacLocation();
                if (gacLocation != null)
                    gacLocation = HttpRuntime.MakeFileUrl(gacLocation);
                if (gacLocation == null)
                    gacLocation = String.Empty;
                strFileData = strFileData.Replace("$Gac$", gacLocation);
            }
#pragma warning disable 618 // ASP is reading their grant set out of legacy policy level files
            policyLevel = SecurityManager.LoadPolicyLevelFromString(strFileData, PolicyLevelType.AppDomain);
            if (policyLevel == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Unable_to_get_policy_file, trustSection.Level));
            }
            // Found GAC Token
            if (gacLocation != null) {
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
                            // now, walk the current groups and insert our new group immediately before the old Gac group
                            // we'll need to use heuristics for this: it will be an UrlMembershipCondition group with full trust
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
                        }
                    }
                }
            }
            return policyLevel;
#pragma warning restore 618
        }

        private sealed class ApplicationResumeStateContainer {
            private static readonly WaitCallback _tpCallback = ResumeCallback;

            private readonly HostingEnvironment _hostEnv;
            private readonly IntPtr _resumeState;

            internal ApplicationResumeStateContainer(HostingEnvironment hostEnv, IntPtr resumeState) {
                _hostEnv = hostEnv;
                _resumeState = resumeState;
            }

            // schedules resume for execution on a new thread
            // unsafe since we don't care about impersonation, identity, etc.
            internal void Resume() {
                ThreadPool.UnsafeQueueUserWorkItem(_tpCallback, this);
            }

            private static void ResumeCallback(object state) {
                ApplicationResumeStateContainer container = (ApplicationResumeStateContainer)state;
                try {
                    container._hostEnv.ResumeApplication(container._resumeState);
                }
                catch (AppDomainUnloadedException) {
                    // AD unloads aren't considered a failure, ----
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        private static class AspNetAppDomainManager {
            internal static Type GetAspNetAppDomainManagerType(bool overrideHostExecutionContextManager, bool overrideHostSecurityManager) {
                if (!overrideHostExecutionContextManager && !overrideHostSecurityManager) {
                    // A custom AppDomainManager isn't necessary for this AppDomain.
                    return null;
                }
                else {
                    // A custom AppDomainManager is necessary for this AppDomain.
                    // See comment on the generic type for further information.
                    Type openGenericType = typeof(AspNetAppDomainManagerImpl<,>);
                    Type closedGenericType = openGenericType.MakeGenericType(
                        (overrideHostExecutionContextManager) ? typeof(AspNetHostExecutionContextManager) : typeof(object),
                        (overrideHostSecurityManager) ? typeof(AspNetHostSecurityManager) : typeof(object)
                        );
                    return closedGenericType;
                }
            }

            // This AppDomainManager may have been set because we need it for Task support, because this is a partial-trust
            // AppDomain, or both. Normally we would store this data in the AppDomain's ambient data store (AppDomain.SetData),
            // but the AppDomainManager instance is initialized in the new AppDomain before the original AppDomain has a chance to
            // call SetData, so in this AppDomain the call to GetData is useless. However, we can use the AppDomainManager type
            // itself to carry the necessary information in the form of a generic type parameter. If a custom
            // HostExecutionContextManager or HostSecurityManager is necessary, the generic type parameters can tell us that.
            // A generic type parameter of "object" means that this particular sub-manager isn't necessary.
            private sealed class AspNetAppDomainManagerImpl<THostExecutionContextManager, THostSecurityManager> : AppDomainManager
                where THostExecutionContextManager : class, new()
                where THostSecurityManager : class, new() {

                private readonly HostExecutionContextManager _hostExecutionContextManager = CreateHostExecutionContextManager();
                private readonly HostSecurityManager _hostSecurityManager = CreateHostSecurityManager();

                public override HostExecutionContextManager HostExecutionContextManager {
                    get {
                        return _hostExecutionContextManager ?? base.HostExecutionContextManager;
                    }
                }

                public override HostSecurityManager HostSecurityManager {
                    get {
                        return _hostSecurityManager ?? base.HostSecurityManager;
                    }
                }

                private static HostExecutionContextManager CreateHostExecutionContextManager() {
                    object hostExecutionContextManager = new THostExecutionContextManager();
                    Debug.Assert(hostExecutionContextManager is HostExecutionContextManager || hostExecutionContextManager.GetType() == typeof(object), "THostExecutionContextManager was an unexpected type!");
                    return hostExecutionContextManager as HostExecutionContextManager;
                }

                private static HostSecurityManager CreateHostSecurityManager() {
                    object hostSecurityManager = new THostSecurityManager();
                    Debug.Assert(hostSecurityManager is HostSecurityManager || hostSecurityManager.GetType() == typeof(object), "THostSecurityManager was an unexpected type!");
                    return hostSecurityManager as HostSecurityManager;
                }
            }

            private sealed class AspNetHostSecurityManager : HostSecurityManager {
                private PermissionSet Nothing = new PermissionSet(PermissionState.None);
                private PermissionSet FullTrust = new PermissionSet(PermissionState.Unrestricted);
                private HostSecurityPolicyResolver hostSecurityPolicyResolver = null;

                public override HostSecurityManagerOptions Flags {
                    get {
                        return HostSecurityManagerOptions.HostResolvePolicy;
                    }
                }

                [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
                public override PermissionSet ResolvePolicy(Evidence evidence) {
                    if (base.ResolvePolicy(evidence).IsUnrestricted()) {
                        return FullTrust;
                    }

                    if (!String.IsNullOrEmpty(HttpRuntime.HostSecurityPolicyResolverType) && hostSecurityPolicyResolver == null) {
                        hostSecurityPolicyResolver = Activator.CreateInstance(
                            Type.GetType(HttpRuntime.HostSecurityPolicyResolverType)) as HostSecurityPolicyResolver;
                    }

                    if (hostSecurityPolicyResolver != null) {
                        switch (hostSecurityPolicyResolver.ResolvePolicy(evidence)) {
                            case HostSecurityPolicyResults.FullTrust:
                                return FullTrust;
                            case HostSecurityPolicyResults.AppDomainTrust:
                                return HttpRuntime.NamedPermissionSet;
                            case HostSecurityPolicyResults.Nothing:
                                return Nothing;
                            case HostSecurityPolicyResults.DefaultPolicy:
                                break;
                        }
                    }

                    if (HttpRuntime.PolicyLevel == null || HttpRuntime.PolicyLevel.Resolve(evidence).PermissionSet.IsUnrestricted())
                        return FullTrust;
                    else if (HttpRuntime.PolicyLevel.Resolve(evidence).PermissionSet.Equals(Nothing))
                        return Nothing;
                    else
                        return HttpRuntime.NamedPermissionSet;
                }
            }
        }

        private static void PopulateDomainBindings(String domainId, String appId, String appName,
                                                    String appPath, VirtualPath appVPath,
                                                    AppDomainSetup setup, IDictionary dict) {
            // assembly loading settings

            // We put both the old and new bin dir names on the private bin path
            setup.PrivateBinPathProbe   = "*";  // disable loading from app base
            setup.ShadowCopyFiles       = "true";
            setup.ApplicationBase       = appPath;
            setup.ApplicationName       = appName;
            setup.ConfigurationFile     = HttpConfigurationSystem.WebConfigFileName;

            // Disallow code download, since it's unreliable in services (ASURT 123836/127606)
            setup.DisallowCodeDownload  = true;

            // internal settings
            dict.Add(".appDomain",     "*");
            dict.Add(".appId",         appId);
            dict.Add(".appPath",       appPath);
            dict.Add(".appVPath",      appVPath.VirtualPathString);
            dict.Add(".domainId",      domainId);
        }

        private static Evidence GetDefaultDomainIdentity() {
            Evidence evidence = AppDomain.CurrentDomain.Evidence; // CurrentDomain.Evidence returns a clone so we can modify it if we need
            bool hasZone = evidence.GetHostEvidence<Zone>() != null;
            bool hasUrl = evidence.GetHostEvidence<Url>() != null;

            if (!hasZone)
                evidence.AddHostEvidence(new Zone(SecurityZone.MyComputer));

            if (!hasUrl)
                evidence.AddHostEvidence(new Url("ms-internal-microsoft-asp-net-webhost-20"));

            return evidence;
        }

        private static int s_domainCount = 0;
        private static Object s_domainCountLock = new Object();

        private static String ConstructAppDomainId(String id) {
            int domainCount = 0;
            lock (s_domainCountLock) {
                domainCount = ++s_domainCount;
            }
            return id + "-" + domainCount.ToString(NumberFormatInfo.InvariantInfo) + "-" + DateTime.UtcNow.ToFileTime().ToString();
        }

        internal LockableAppDomainContext GetLockableAppDomainContext (string appId) {
            lock (this) {
                LockableAppDomainContext ac;
                if (!_appDomains.TryGetValue(appId, out ac)) {
                    ac = new LockableAppDomainContext();
                    _appDomains.Add (appId, ac);
                }

                return ac;
            }
        }

        // take a copy of _appDomains collection so that it can be used without locking on ApplicationManager
        private Dictionary<string, LockableAppDomainContext> CloneAppDomainsCollection() {
            lock (this) {
                return new Dictionary<string, LockableAppDomainContext>(_appDomains, StringComparer.OrdinalIgnoreCase);
            }
        }

        private static Configuration GetAppConfigCommon(IConfigMapPath configMapPath, string siteID, string appSegment) {
            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
            string dir = null;
            string fileName = null;
            string subDir = "/";
            // add root mapping
            configMapPath.GetPathConfigFilename(siteID, subDir, out dir, out fileName);
            if (dir != null) {
                fileMap.VirtualDirectories.Add(subDir, new VirtualDirectoryMapping(Path.GetFullPath(dir), true));
            }
            // add subdir mappings
            string[] subDirs = appSegment.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in subDirs) {
                subDir = subDir + s;
                configMapPath.GetPathConfigFilename(siteID, subDir, out dir, out fileName);
                if (dir != null) {
                    fileMap.VirtualDirectories.Add(subDir, new VirtualDirectoryMapping(Path.GetFullPath(dir), true));
                }
                subDir = subDir + "/";
            }
            // open mapped web config for application
            return WebConfigurationManager.OpenMappedWebConfiguration(fileMap, appSegment, siteID);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Only ever called by the full-trust parent AppDomain.")]
        private static Configuration GetAppConfigGeneric(IApplicationHost appHost, string siteID, string appSegment, VirtualPath virtualPath, string physicalPath) {
            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
            IConfigMapPathFactory configMapPathFactory2 = appHost.GetConfigMapPathFactory();
            IConfigMapPath configMapPath = configMapPathFactory2.Create(virtualPath.VirtualPathString, physicalPath);
            return GetAppConfigCommon(configMapPath, siteID, appSegment);
        }

        private static Configuration GetAppConfigIISExpress(string siteID, string appSegment, string iisExpressVersion) {
            ExpressServerConfig serverConfig = (ExpressServerConfig)ServerConfig.GetDefaultDomainInstance(iisExpressVersion);
            return GetAppConfigCommon(serverConfig, siteID, appSegment);
        }

        private sealed class AppDomainSwitches {
            public bool UseLegacyCas;

            public void Apply(AppDomainSetup setup) {
                List<string> switches = new List<string>();

                if (UseLegacyCas) {
                    // Enables the AppDomain to use the legacy CAS model for compatibility <trust/legacyCasModel>
                    switches.Add("NetFx40_LegacySecurityPolicy");
                }

                if (switches.Count > 0) {
                    setup.SetCompatibilitySwitches(switches);
                }
            }
        }

        // This class holds information about the environment that is hosting ASP.NET. The particular design of this class
        // is that the information is computed once and stored, and the methods which compute the information are private.
        // This prevents accidental misuse of this type via querying the environment after user code has had a chance to
        // run, which could potentially affect the environment itself.
        private static class EnvironmentInfo {
            public static readonly bool IsStringHashCodeRandomizationEnabled = GetIsStringHashCodeRandomizationEnabled();
            public static readonly bool WasLaunchedFromDevelopmentEnvironment = GetWasLaunchedFromDevelopmentEnvironmentValue();

            private static bool GetIsStringHashCodeRandomizationEnabled() {
                // known test vector
                return (StringComparer.InvariantCultureIgnoreCase.GetHashCode("The quick brown fox jumps over the lazy dog.") != 0x703e662e);
            }

            // Visual Studio / WebMatrix will set DEV_ENVIRONMENT=1 when launching an ASP.NET host in a development environment.
            private static bool GetWasLaunchedFromDevelopmentEnvironmentValue() {
                try {
                    string envVar = Environment.GetEnvironmentVariable("DEV_ENVIRONMENT", EnvironmentVariableTarget.Process);
                    return String.Equals(envVar, "1", StringComparison.Ordinal);
                }
                catch {
                    // We don't care if we can't read the environment variable; just treat it as not present.
                    return false;
                }
            }
        }

    }
}
