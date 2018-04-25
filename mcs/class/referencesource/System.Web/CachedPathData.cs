//------------------------------------------------------------------------------
// <copyright file="CachedPathData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Globalization;
    using System.Security.Principal;
    using System.Threading;
    using System.Web.Security;
    using System.Web.SessionState;
    using System.Web.Configuration;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.UI;
    using System.Security.Permissions;

    // Data about a path that is cached across requests
    class CachedPathData {
        internal const int FInited                  = 0x0001;
        internal const int FCompletedFirstRequest   = 0x0002;
        internal const int FExists                  = 0x0004; 
        internal const int FOwnsConfigRecord        = 0x0010;   // is this the highest ancestor pointing to the config record? 
        internal const int FClosed                  = 0x0020;   // Has item been closed already?
        internal const int FCloseNeeded             = 0x0040;   // Should we close?
        internal const int FAnonymousAccessChecked  = 0x0100;
        internal const int FAnonymousAccessAllowed  = 0x0200;

        static CacheItemRemovedCallback s_callback = new CacheItemRemovedCallback(CachedPathData.OnCacheItemRemoved);
        // initialize the URL metadata cache expiration here, just in case there's an issue with HttpRuntime.HostingInit
        private static TimeSpan s_urlMetadataSlidingExpiration = HostingEnvironmentSection.DefaultUrlMetadataSlidingExpiration;
        private static bool s_doNotCacheUrlMetadata = false;
        private static int s_appConfigPathLength = 0;

        #pragma warning disable 0649
        SafeBitVector32         _flags;
        #pragma warning restore 0649
        string                  _configPath;
        VirtualPath             _virtualPath;
        string                  _physicalPath;
        RuntimeConfig           _runtimeConfig;
        HandlerMappingMemo      _handlerMemo;


        //
        // Constructor
        //
        internal CachedPathData(string configPath, VirtualPath virtualPath, string physicalPath, bool exists) {
            // Guarantee that we return a non-null config record
            // if an error occurs during initialization.
            _runtimeConfig = RuntimeConfig.GetErrorRuntimeConfig();
            _configPath = configPath;
            _virtualPath = virtualPath;
            _physicalPath = physicalPath;
            _flags[FExists] = exists;

            // VSWhidbey 607683: Config loading for web app has a dependency on CachedPathData.
            // On the other hand, Config also has a dependency on Uri class which has
            // a new static constructor that calls config, and eventually to CachedPathData again.
            // We need a dummy reference to Uri class so the static constructor would be involved
            // first to initialize config.
            string dummy = System.Uri.SchemeDelimiter;

        }

        //
        // Called by HttpRuntime.HostingInit to initialize UrlMetadataSlidingExpiration
        //
        static internal void InitializeUrlMetadataSlidingExpiration(HostingEnvironmentSection section) {
            TimeSpan slidingExp = section.UrlMetadataSlidingExpiration;
            if (slidingExp == TimeSpan.Zero) {
                // a value of TimeSpan.Zero means don't cache
                // this "feature" was added for Bing, because they
                // have scenarios where the same URL is never seen twice
                s_doNotCacheUrlMetadata = true;
            }
            else if (slidingExp == TimeSpan.MaxValue) {
                // a value of TimeSpan.MaxValue means use Cache.NoSlidingExpiration,
                // which is how CachedPathData used to be cached, so this effectively
                // reverts to v2.0 behavior for caching CachedPathData
                s_urlMetadataSlidingExpiration = Cache.NoSlidingExpiration;
                s_doNotCacheUrlMetadata = false;
            }
            else {
                // anything in between means cache with that sliding expiration
                s_urlMetadataSlidingExpiration = slidingExp;
                s_doNotCacheUrlMetadata = false;
            }
        }

        //
        // Get CachedPathData for the machine.config level
        //
        static internal CachedPathData GetMachinePathData() {
            return GetConfigPathData(WebConfigurationHost.MachineConfigPath);
        }

        //
        // Get CachedPathData for the root web.config path
        //
        static internal CachedPathData GetRootWebPathData() {
            return GetConfigPathData(WebConfigurationHost.RootWebConfigPath);
        }

        //
        // Get CachedPathData for the application.
        //
        static internal CachedPathData GetApplicationPathData() {
            if (!HostingEnvironment.IsHosted) {
                return GetRootWebPathData();
            }

            return GetConfigPathData(HostingEnvironment.AppConfigPath);
        }

        //
        // Get CachedPathData for a virtual path.
        // The path may be supplied by user code, so check that it is valid.
        //
        static internal CachedPathData GetVirtualPathData(VirtualPath virtualPath, bool permitPathsOutsideApp) {
            if (!HostingEnvironment.IsHosted) {
                return GetRootWebPathData();
            }

            // Make sure it's not relative
            if (virtualPath != null) {
                virtualPath.FailIfRelativePath();
            }

            // Check if the path is within the application.
            if (virtualPath == null || !virtualPath.IsWithinAppRoot) {
                if (permitPathsOutsideApp) {
                    return GetApplicationPathData();
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.Cross_app_not_allowed,
                        (virtualPath != null) ? virtualPath.VirtualPathString : "null"));
                }
            }

            // Construct a configPath based on the unvalidated virtualPath.
            string configPath = WebConfigurationHost.GetConfigPathFromSiteIDAndVPath(HostingEnvironment.SiteID, virtualPath);

            // Pass the virtualPath to GetConfigPathData to validate in the case where the
            // CachedPathData for the unsafeConfigPath is not found.
            return GetConfigPathData(configPath);
        }

        // Dev10 862204: AppDomain does not restart when the application's web.config is touched 2 minutes after the last request
        static private bool IsCachedPathDataRemovable(string configPath) {
            // have we initialized yet?
            if (s_appConfigPathLength == 0) {
                // when hosted use AppConfigPath, otherwise use RootWebConfigPath
                s_appConfigPathLength = (HostingEnvironment.IsHosted) ? HostingEnvironment.AppConfigPath.Length : WebConfigurationHost.RootWebConfigPath.Length;
            }            
            // Only config paths beneath the application config path can be removed from the cache.
            return (configPath.Length > s_appConfigPathLength);
        }

        // Example of configPath = "machine/webroot/1/fxtest/sub/foo.aspx"
        // The configPath parameter must be lower case.
        static private CachedPathData GetConfigPathData(string configPath) {
            Debug.Assert(ConfigPathUtility.IsValid(configPath), "ConfigPathUtility.IsValid(configPath)");
            Debug.Assert(configPath == configPath.ToLower(CultureInfo.InvariantCulture), "configPath == configPath.ToLower(CultureInfo.InvariantCulture)");
            bool exists = false;
            bool isDirectory = false;
            bool isRemovable = IsCachedPathDataRemovable(configPath);
            // if the sliding expiration is zero, we won't cache it unless it is a configPath for root web.config or above
            if (isRemovable && DoNotCacheUrlMetadata) {
                string pathSiteID = null;
                VirtualPath virtualFilePath = null;
                string physicalFilePath = null;
                WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(configPath, out pathSiteID, out virtualFilePath);
                physicalFilePath = GetPhysicalPath(virtualFilePath);

                string parentConfigPath = ConfigPathUtility.GetParent(configPath);
                CachedPathData pathParentData = GetConfigPathData(parentConfigPath);
                if (!String.IsNullOrEmpty(physicalFilePath)) {
                    FileUtil.PhysicalPathStatus(physicalFilePath, false, false, out exists, out isDirectory);
                }
                CachedPathData pathData = new CachedPathData(configPath, virtualFilePath, physicalFilePath, exists);
                pathData.Init(pathParentData);

                return pathData;
            }

            //
            // First, see if the CachedPathData is in the cache.
            // we don't use Add for this lookup, as doing so requires
            // creating a CacheDependency, which can be slow as it may hit
            // the filesystem.
            //
            string key = CreateKey(configPath);
            CacheStoreProvider cacheInternal = HttpRuntime.Cache.InternalCache;
            CachedPathData data = (CachedPathData) cacheInternal.Get(key);

            // if found, return the data
            if (data != null) {
                data.WaitForInit();
                return data;
            }
            
            // WOS 


            bool cacheEntryIsNotRemovable = false;

            // if not found, try to add it
            string siteID = null;
            VirtualPath virtualPath = null;
            CachedPathData parentData = null;
            CacheDependency dependency = null;
            string physicalPath = null;
            string[] fileDependencies = null;
            string[] cacheItemDependencies = null;

            if (WebConfigurationHost.IsMachineConfigPath(configPath)) {
                cacheEntryIsNotRemovable = true;
            }
            else {
                // Make sure we have the parent data so we can create a dependency on the parent.
                // The parent dependency will ensure that configuration data in the parent 
                // will be referenced by a cache hit on the child. (see UtcUpdateUsageRecursive in Cache.cs)
                string parentConfigPath = ConfigPathUtility.GetParent(configPath);
                parentData = GetConfigPathData(parentConfigPath);
                string parentKey = CreateKey(parentConfigPath);
                cacheItemDependencies = new string[1] {parentKey};

                if (!WebConfigurationHost.IsVirtualPathConfigPath(configPath)) {
                    // assume hardcoded levels above the path, such as root web.config, exist
                    cacheEntryIsNotRemovable = true;
                }
                else {
                    cacheEntryIsNotRemovable = !isRemovable;
                    WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out virtualPath);
                    physicalPath = GetPhysicalPath(virtualPath);

                    // Add a dependency on the path itself, if it is a file,
                    // to handle the case where a file is deleted and replaced
                    // with a directory of the same name.
                    if (!String.IsNullOrEmpty(physicalPath)) {
                        FileUtil.PhysicalPathStatus(physicalPath, false, false, out exists, out isDirectory);
                        if (exists && !isDirectory) {
                            fileDependencies = new string[1] {physicalPath};
                        }
                    }
                }

                try {
                    dependency = new CacheDependency(0, fileDependencies, cacheItemDependencies);
                }
                catch {
                    // CacheDependency ctor could fail because of bogus file path
                    // and it is ok not to watch those
                }
            }

            // Try to add the CachedPathData to the cache.
            CachedPathData    dataAdd = null;
            bool              isDataCreator = false;
            bool              initCompleted = false;
            CacheItemPriority priority = cacheEntryIsNotRemovable ? CacheItemPriority.NotRemovable : CacheItemPriority.Normal;
            TimeSpan          slidingExpiration = cacheEntryIsNotRemovable ? Cache.NoSlidingExpiration : UrlMetadataSlidingExpiration;
            try {
                using (dependency) {
                    dataAdd = new CachedPathData(configPath, virtualPath, physicalPath, exists);
                    try {
                    }
                    finally {
                        data = (CachedPathData)cacheInternal.Add(key, dataAdd, new CacheInsertOptions() {
                            Dependencies = dependency,
                            SlidingExpiration = slidingExpiration,
                            Priority = priority,
                            OnRemovedCallback = s_callback
                        });
                        
                        if (data == null) {
                            isDataCreator = true;
                        }
                    }
                }

                // If another thread added it first, return the data
                if (!isDataCreator) {
                    data.WaitForInit();
                    return data;
                }

                // This thread is the creator of the CachedPathData, initialize it
                lock (dataAdd) {
                    try {
                        dataAdd.Init(parentData);
                        initCompleted = true;
                    }
                    finally {
                        // free waiters
                        dataAdd._flags[FInited] = true;
                
                        // Wake up waiters.
                        Monitor.PulseAll(dataAdd);
                
                        if (dataAdd._flags[FCloseNeeded]) {
                            // If we have received a call back to close, then lets 
                            // make sure that our config object is cleaned up
                            dataAdd.Close();
                        }
                    }
                }
            }
            finally {
                // All the work in this finally block is for the case where we're the
                // creator of the CachedPathData.
                if (isDataCreator) {

                    // 




                    if (!dataAdd._flags[FInited]) {
                        lock (dataAdd) {
                            // free waiters
                            dataAdd._flags[FInited] = true;
                    
                            // Wake up waiters.
                            Monitor.PulseAll(dataAdd);
                            
                            if (dataAdd._flags[FCloseNeeded]) {
                                // If we have received a call back to close, then lets 
                                // make sure that our config object is cleaned up
                                dataAdd.Close();
                            }
                        }
                    }
                    
                    //
                    // Even though there is a try/catch handler surrounding the call to Init,
                    // a ThreadAbortException can still cause the handler to be bypassed.
                    //
                    // If there is an error, either a thread abort or an error in the config
                    // file itself, we do want to leave the item cached for a short period
                    // so that we do not revisit the error and potentially reparse the config file
                    // on every request.
                    // 
                    // The reason we simply do not leave the item in the cache forever is that the 
                    // problem that caused the configuration exception may be fixed without touching
                    // the config file in a way that causes a file change notification (for example, an 
                    // acl change in a parent directory, or a change of path mapping in the metabase).
                    //
                    // NOTE: It is important to reinsert the item into the cache AFTER dropping
                    // the lock on dataAdd, in order to prevent the possibility of deadlock.
                    //
                    Debug.Assert(dataAdd._flags[FInited], "_flags[FInited]");
                    if (!initCompleted || (dataAdd.ConfigRecord != null && dataAdd.ConfigRecord.HasInitErrors)) {
                        //
                        // Create a new dependency object as the old one cannot be reused.
                        // Do not include a file dependency if initialization could not be completed,
                        // as invoking the file system could lead to further errors during a thread abort.
                        //
                        if (dependency != null) {
                            if (!initCompleted) {
                                dependency = new CacheDependency(0, null, cacheItemDependencies);
                            }
                            else {
                                dependency = new CacheDependency(0, fileDependencies, cacheItemDependencies);
                            }
                        }
                    
                        using (dependency) {
                            cacheInternal.Insert(key, dataAdd, new CacheInsertOptions() {
                                                                    Dependencies = dependency,
                                                                    AbsoluteExpiration = DateTime.UtcNow.AddSeconds(5),
                                                                    OnRemovedCallback = s_callback
                                                                });
                        }
                    }
                    
                }
            }

            return dataAdd;
        }

        // Ensure that the physical path does not look suspicious (MSRC 5556).
        static private string GetPhysicalPath(VirtualPath virtualPath) {
            string physicalPath = null;
            try {
                physicalPath = virtualPath.MapPathInternal(true);
            }
            catch (HttpException e) {
                //
                // Treat exceptions that are thrown because the path is suspicious
                // as "404 Not Found" exceptions. Implementations of MapPath
                // will throw HttpException with no error code if the path is
                // suspicious.
                //
                if (e.GetHttpCode() == 500) {
                    throw new HttpException(404, String.Empty);
                }
                else {
                    throw;
                }
            }

            //
            // Throw "404 Not Found" if the path is suspicious and 
            // the implementation of MapPath has not already done so.
            //
            FileUtil.CheckSuspiciousPhysicalPath(physicalPath);

            return physicalPath;
        }

        // Remove CachedPathData when the first request for the path results in a 
        // 400 range error. We need to remove all data up the path to account for
        // virtual files.
        // An example of a 400 range error is "path not found". 
        static internal void RemoveBadPathData(CachedPathData pathData) {
            CacheStoreProvider cacheInternal = HttpRuntime.Cache.InternalCache;

            string configPath = pathData._configPath;
            string key = CreateKey(configPath);
            while (pathData != null && !pathData.CompletedFirstRequest && !pathData.Exists) {

                cacheInternal.Remove(key);

                configPath = ConfigPathUtility.GetParent(configPath);
                if (configPath == null)
                    break;

                key = CreateKey(configPath);
                pathData = (CachedPathData) cacheInternal.Get(key);
            }
        }

        // Mark CachedPathData as completed when the first request for the path results in a 
        // status outside the 400 range. We need to mark all data up the path to account for
        // virtual files.
        static internal void MarkCompleted(CachedPathData pathData) {
            CacheStoreProvider cacheInternal = HttpRuntime.Cache.InternalCache;

            string configPath = pathData._configPath;
            do {
                pathData.CompletedFirstRequest = true;

                configPath = ConfigPathUtility.GetParent(configPath);
                if (configPath == null)
                    break;

                string key = CreateKey(configPath);
                pathData = (CachedPathData) cacheInternal.Get(key);
            } while (pathData != null && !pathData.CompletedFirstRequest);
        }

        // Close
        //
        // Close the object.  This does not mean it can not be used anymore, 
        // it just means that the cleanup as been done, so we don't have
        // to worry about closing it anymore
        //
        void Close() {
            // Only close if we are propertly initialized
            if (_flags[FInited]) {

                // Only close if we haven't already closed
                if (_flags.ChangeValue(FClosed, true)) {

                    // Remove the config record if we own it
                    // N.B. ConfigRecord.Remove is safe to call more than once.
                    if (_flags[FOwnsConfigRecord]) {
                        ConfigRecord.Remove();
                    }
                }
            }
        }

        // OnCacheItemRemoved
        //
        // Notification the items has been removed from the cache.  Flag
        // the item to be cleaned up, and then try cleanup
        //
        static void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason) {
            CachedPathData data = (CachedPathData) value;
            
            data._flags[FCloseNeeded] = true;
            data.Close();
        }

        static string CreateKey(string configPath) {
            Debug.Assert(configPath == configPath.ToLower(CultureInfo.InvariantCulture), "configPath == configPath.ToLower(CultureInfo.InvariantCulture)");
            return CacheInternal.PrefixPathData + configPath;
        }

        // Initialize the data
        void Init(CachedPathData parentData) {
            // Note that _runtimeConfig will be set to the singleton instance of ErrorRuntimeConfig
            // if a ThreadAbortException is thrown during this method.
            Debug.Assert(_runtimeConfig == RuntimeConfig.GetErrorRuntimeConfig(), "_runtimeConfig == RuntimeConfig.GetErrorRuntimeConfig()");

            if (!HttpConfigurationSystem.UseHttpConfigurationSystem) {
                // 
                // configRecord may legitimately be null if we are not using the HttpConfigurationSystem.
                //
                _runtimeConfig = null;
            }
            else {
                IInternalConfigRecord configRecord = HttpConfigurationSystem.GetUniqueConfigRecord(_configPath);
                Debug.Assert(configRecord != null, "configRecord != null");

                if (configRecord.ConfigPath.Length == _configPath.Length) {
                    //
                    // The config is unique to this path, so this make this record the owner of the config.
                    //
                    _flags[FOwnsConfigRecord] = true;
                    _runtimeConfig = new RuntimeConfig(configRecord);
                }
                else {
                    //
                    // The config record is the same as an ancestor's, so use the parent's RuntimeConfig.
                    //
                    Debug.Assert(parentData != null, "parentData != null");
                    _runtimeConfig = parentData._runtimeConfig;
                }
            }
        }

        void WaitForInit() {
            // Wait for the data to be initialized.
            if (!_flags[FInited]) {
                lock (this) {
                    if (!_flags[FInited]) {
                        Monitor.Wait(this);
                    }
                }
            }
        }

        // Ensure that Request.PhysicalPath is valid (canonical, not too long, and contains valid characters).
        // The work is done by CheckSuspiciousPhysicalPath, but as a perf optimization, we can compare 
        // Request.PhysicalPath with the cached path result.  The cached path result is validated before 
        // it is cached.  As long as the cached path result is identical to Request.PhysicalPath, we don't 
        // have to call CheckSuspiciousPhysicalPath again.
        internal void ValidatePath(String physicalPath) {
            if (String.IsNullOrEmpty(_physicalPath) && String.IsNullOrEmpty(physicalPath)) {
                return;
            }
            if (!String.IsNullOrEmpty(_physicalPath) && !String.IsNullOrEmpty(physicalPath)) {
                if (_physicalPath.Length == physicalPath.Length) {
                    // if identical, we don't have to call CheckSuspiciousPhysicalPath
                    if (0 == String.Compare(_physicalPath, 0, physicalPath, 0, physicalPath.Length, StringComparison.OrdinalIgnoreCase)) {
                        return;
                    }
                }
                else if (_physicalPath.Length - physicalPath.Length == 1) {
                    // if they differ by a trailing slash, we shouldn't call CheckSuspiciousPhysicalPath again
                    if (_physicalPath[_physicalPath.Length-1] == System.IO.Path.DirectorySeparatorChar
                        && (0 == String.Compare(_physicalPath, 0, physicalPath, 0, physicalPath.Length, StringComparison.OrdinalIgnoreCase))) {
                        return;
                    }
                }
                else if (physicalPath.Length - _physicalPath.Length == 1) {
                    // if they differ by a trailing slash, we shouldn't call CheckSuspiciousPhysicalPath again
                    if (physicalPath[physicalPath.Length-1] == System.IO.Path.DirectorySeparatorChar
                        && (0 == String.Compare(_physicalPath, 0, physicalPath, 0, _physicalPath.Length, StringComparison.OrdinalIgnoreCase))) {
                        return;
                    }
                }
            }

            // If we're here, the paths were different, which normally should not happen.
            Debug.Assert(false, "ValidatePath optimization failed: Request.PhysicalPath=" 
                         + physicalPath + "; _physicalPath=" + _physicalPath);            
            FileUtil.CheckSuspiciousPhysicalPath(physicalPath);
        }

        internal bool CompletedFirstRequest {
            get {return _flags[FCompletedFirstRequest];}
            set {
                _flags[FCompletedFirstRequest] = value;
            }
        }

        internal VirtualPath Path {
            get {return _virtualPath;}
        }

        internal string PhysicalPath {
            get { return _physicalPath; }
        }

        internal bool AnonymousAccessChecked {
            get { return _flags[FAnonymousAccessChecked]; }
            set { _flags[FAnonymousAccessChecked] = value; }
        }

        internal bool AnonymousAccessAllowed {
            get { return _flags[FAnonymousAccessAllowed]; }
            set { _flags[FAnonymousAccessAllowed] = value; }
        }

        internal bool Exists {
            get {return _flags[FExists];}
        }

        internal HandlerMappingMemo CachedHandler {
            get {return _handlerMemo;}
            set {_handlerMemo = value;}
        }


        internal IInternalConfigRecord ConfigRecord {
            get {
                // _runtimeConfig may be null if we are not using the HttpConfigurationSystem.
                return (_runtimeConfig != null) ? _runtimeConfig.ConfigRecord : null;
            }
        }

        internal RuntimeConfig RuntimeConfig {
            get {
                return _runtimeConfig;
            }
        }

        // Any time we cache metadata for the URL, we should use this
        // sliding expiration, unless DoNotCacheUrlMetadata is true.
        // This is currently used by CachedPathData, MapPathBasedVirtualPathProvider,
        // FileAuthorizationModule, ProcessHostMapPath and MetabaseServerConfig.
        internal static TimeSpan UrlMetadataSlidingExpiration { 
            get { 
                return s_urlMetadataSlidingExpiration; 
            } 
        }

        // if true, do not cache at all.  
        internal static bool DoNotCacheUrlMetadata { 
            get { return s_doNotCacheUrlMetadata; }
        }
    }
}

