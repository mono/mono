//------------------------------------------------------------------------------
// <copyright file="MetabaseServerConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.IO;
    using System.Web.Util;
    using System.Web.Hosting;
    using System.Web.Caching;
    using Microsoft.Win32;

    class MetabaseServerConfig : IServerConfig, IConfigMapPath, IConfigMapPath2 {
        private const string    DEFAULT_SITEID = "1";
        private const string    DEFAULT_ROOTAPPID = "/LM/W3SVC/1/ROOT";
        private const int       MAX_PATH=260;
        private const int       BUFSIZE = MAX_PATH + 1;
        private const string    LMW3SVC_PREFIX = "/LM/W3SVC/";
        private const string    ROOT_SUFFIX="/ROOT";

        static private MetabaseServerConfig s_instance;
        static private object               s_initLock = new Object();

        string _defaultSiteName;
        string _siteIdForCurrentApplication;

        static internal IServerConfig GetInstance() {
            if (s_instance == null) {
                lock (s_initLock) {
                    if (s_instance == null) {
                        s_instance = new MetabaseServerConfig();
                    }
                }
            }

            return s_instance;
        }

        private MetabaseServerConfig() {
            HttpRuntime.ForceStaticInit(); // force webengine.dll to load

            // Get the default site information
            bool found = MBGetSiteNameFromSiteID(DEFAULT_SITEID, out _defaultSiteName);
            _siteIdForCurrentApplication = HostingEnvironment.SiteID;
            if (_siteIdForCurrentApplication == null) {
                _siteIdForCurrentApplication = DEFAULT_SITEID;
            }
        }

        string IServerConfig.GetSiteNameFromSiteID(string siteID) {
            if (StringUtil.EqualsIgnoreCase(siteID, DEFAULT_SITEID))
                return _defaultSiteName;

            string siteName;
            bool found = MBGetSiteNameFromSiteID(siteID, out siteName);
            return siteName;
        }

        // if appHost is null, we use the site ID for the current application
        string IServerConfig.MapPath(IApplicationHost appHost, VirtualPath path) {
            string siteID = (appHost == null) ? _siteIdForCurrentApplication : appHost.GetSiteID();
            return MapPathCaching(siteID, path);
        }

        string[] IServerConfig.GetVirtualSubdirs(VirtualPath path, bool inApp) {
            string aboPath = GetAboPath(_siteIdForCurrentApplication, path.VirtualPathString);
            return MBGetVirtualSubdirs(aboPath, inApp);
        }

        bool IServerConfig.GetUncUser(IApplicationHost appHost, VirtualPath path, out string username, out string password) {
            string aboPath = GetAboPath(appHost.GetSiteID(), path.VirtualPathString);
            return MBGetUncUser(aboPath, out username, out password);
        }

        long IServerConfig.GetW3WPMemoryLimitInKB() {
            return (long) MBGetW3WPMemoryLimitInKB();
        }

        string IConfigMapPath.GetMachineConfigFilename() {
            return HttpConfigurationSystem.MachineConfigurationFilePath;
        }

        string IConfigMapPath.GetRootWebConfigFilename() {
            return HttpConfigurationSystem.RootWebConfigurationFilePath;
        }

        private void GetPathConfigFilenameWorker(string siteID, VirtualPath path, out string directory, out string baseName) {
            directory = MapPathCaching(siteID, path);
            if (directory != null) {
                baseName = HttpConfigurationSystem.WebConfigFileName;
            }
            else {
                baseName = null;
            }
        }

        // Based on <siteID, path>, return:
        // directory - the physical directory of the path (vpath)
        // baseName - name of the configuration file to look for.
        // E.g. if siteID="1" and path="/", directory="c:\inetpub\wwwroot" and baseName="web.config"
        void IConfigMapPath.GetPathConfigFilename(
                string siteID, string path, out string directory, out string baseName) {
            GetPathConfigFilenameWorker(siteID, VirtualPath.Create(path), out directory, out baseName);
        }

        void IConfigMapPath2.GetPathConfigFilename(
                string siteID, VirtualPath path, out string directory, out string baseName) {
            GetPathConfigFilenameWorker(siteID, path, out directory, out baseName);
        }

        void IConfigMapPath.GetDefaultSiteNameAndID(out string siteName, out string siteID) {
            siteName = _defaultSiteName;
            siteID = DEFAULT_SITEID;
        }

        void IConfigMapPath.ResolveSiteArgument(string siteArgument, out string siteName, out string siteID) {
            if (    String.IsNullOrEmpty(siteArgument) ||
                    StringUtil.EqualsIgnoreCase(siteArgument, DEFAULT_SITEID) ||
                    StringUtil.EqualsIgnoreCase(siteArgument, _defaultSiteName)) {

                siteName = _defaultSiteName;
                siteID = DEFAULT_SITEID;
            }
            else {
                siteName = String.Empty;
                siteID = String.Empty;

                bool found = false;
                if (IISMapPath.IsSiteId(siteArgument)) {
                    found = MBGetSiteNameFromSiteID(siteArgument, out siteName);
                }

                if (found) {
                    siteID = siteArgument;
                }
                else {
                    found = MBGetSiteIDFromSiteName(siteArgument, out siteID);
                    if (found) {
                        siteName = siteArgument;
                    }
                    else {
                        siteName = siteArgument;
                        siteID = String.Empty;
                    }
                }
            }
        }

        // Map from <siteID, path> to a physical file path
        string IConfigMapPath.MapPath(string siteID, string vpath) {
            return MapPathCaching(siteID, VirtualPath.Create(vpath));
        }

        // IConfigMapPath2 VirtualPath fast path
        string IConfigMapPath2.MapPath(string siteID, VirtualPath vpath) {
            return MapPathCaching(siteID, vpath);
        }

        VirtualPath GetAppPathForPathWorker(string siteID, VirtualPath vpath) {
            string aboPath = GetAboPath(siteID, vpath.VirtualPathString);
            string appAboPath = MBGetAppPath(aboPath);
            if (appAboPath == null)
                return VirtualPath.RootVirtualPath;

            string rootAboPath = GetRootAppIDFromSiteID(siteID);
            if (StringUtil.EqualsIgnoreCase(rootAboPath, appAboPath)) {
                return VirtualPath.RootVirtualPath;
            }

            string appPath = appAboPath.Substring(rootAboPath.Length);
            return VirtualPath.CreateAbsolute(appPath);
        }

        string IConfigMapPath.GetAppPathForPath(string siteID, string vpath) {
            VirtualPath resolved = GetAppPathForPathWorker(siteID, VirtualPath.Create(vpath));
            return resolved.VirtualPathString;
        }

        // IConfigMapPath2 VirtualPath fast path
        VirtualPath IConfigMapPath2.GetAppPathForPath(string siteID, VirtualPath vpath) {
            return GetAppPathForPathWorker(siteID, vpath);
        }

        private string MatchResult(VirtualPath path, string result) {
            if (string.IsNullOrEmpty(result)) {
                return result;
            }

            result = result.Replace('/', '\\');

            // ensure extra '\\' in the physical path if the virtual path had extra '/'
            // and the other way -- no extra '\\' in physical if virtual didn't have it.
            if (path.HasTrailingSlash) {
                if (!UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result)) {
                    result = result + "\\";
                }
            }
            else {
                if (UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result)) {
                    result = result.Substring(0, result.Length - 1);
                }
            }

            return result;
        }

        private string MapPathCaching(string siteID, VirtualPath path) {
            // UrlMetaDataSlidingExpiration config setting controls 
            // the duration of all cached items related to url metadata.
            bool doNotCache = CachedPathData.DoNotCacheUrlMetadata;
            TimeSpan slidingExpiration = CachedPathData.UrlMetadataSlidingExpiration; 

            // store a single variation of the path
            VirtualPath originalPath = path;
            MapPathCacheInfo cacheInfo;

            if (doNotCache) {
                cacheInfo = new MapPathCacheInfo();
            }
            else {
                // Check if it's in the cache
                String cacheKey = CacheInternal.PrefixMapPath + siteID + path.VirtualPathString;
                cacheInfo = (MapPathCacheInfo)HttpRuntime.CacheInternal.Get(cacheKey);

                // If not in cache, add it to the cache
                if (cacheInfo == null) {
                    cacheInfo = new MapPathCacheInfo();
                    // Add to the cache.
                    // No need to have a lock here. UtcAdd will add the entry if it doesn't exist. 
                    // If it does exist, the existing value will be returned (Dev10 Bug 755034).
                    object existingEntry = HttpRuntime.CacheInternal.UtcAdd(
                        cacheKey, cacheInfo, null, Cache.NoAbsoluteExpiration, slidingExpiration, CacheItemPriority.Default, null);
                    if (existingEntry != null) {
                        cacheInfo = existingEntry as MapPathCacheInfo;
                    }
                }
            }

            // If not been evaluated, then evaluate it
            if (!cacheInfo.Evaluated) {
                lock(cacheInfo) {

                    if (!cacheInfo.Evaluated && HttpRuntime.IsMapPathRelaxed) {
                        //////////////////////////////////////////////////////////////////
                        // Verify that the parent path is valid. If parent is invalid, then set this to invalid
                        if (path.VirtualPathString.Length > 1) {
                            VirtualPath vParent = path.Parent;
                            if (vParent != null) {
                                string parentPath = vParent.VirtualPathString;
                                if (parentPath.Length > 1 && StringUtil.StringEndsWith(parentPath, '/')) { // Trim the extra trailing / if there is one
                                    vParent = VirtualPath.Create(parentPath.Substring(0, parentPath.Length - 1));
                                }
                                try {
                                    string parentMapPathResult = MapPathCaching(siteID, vParent);
                                    if (parentMapPathResult == HttpRuntime.GetRelaxedMapPathResult(null)) {
                                        // parent is invalid!
                                        cacheInfo.MapPathResult = parentMapPathResult;
                                        cacheInfo.Evaluated = true;
                                    }
                                } catch {
                                    cacheInfo.MapPathResult = HttpRuntime.GetRelaxedMapPathResult(null);
                                    cacheInfo.Evaluated = true;
                                }
                            }
                        }
                    }

                    if (!cacheInfo.Evaluated) {
                        string physicalPath = null;

                        try {
                            physicalPath = MapPathActual(siteID, path);

                            if (HttpRuntime.IsMapPathRelaxed) {
                                physicalPath = HttpRuntime.GetRelaxedMapPathResult(physicalPath);
                            }

                            // Throw if the resulting physical path is not canonical, to prevent potential
                            // security issues (VSWhidbey 418125)
                            if (FileUtil.IsSuspiciousPhysicalPath(physicalPath)) {
                                if (HttpRuntime.IsMapPathRelaxed) {
                                    physicalPath = HttpRuntime.GetRelaxedMapPathResult(null);
                                } else {
                                    throw new HttpException(SR.GetString(SR.Cannot_map_path, path));
                                }
                            }

                        } catch (Exception e) {
                            if (HttpRuntime.IsMapPathRelaxed) {
                                physicalPath = HttpRuntime.GetRelaxedMapPathResult(null);
                            } else {
                                cacheInfo.CachedException = e;
                                cacheInfo.Evaluated=true;
                                throw;
                            }
                        }

                        if ( physicalPath != null ) {
                            // Only cache if we got a good value
                            cacheInfo.MapPathResult = physicalPath;
                            cacheInfo.Evaluated = true;
                        }
                    }
                }
            }

            // Throw an exception if required
            if (cacheInfo.CachedException != null) {
                throw cacheInfo.CachedException;
            }

            return MatchResult(originalPath, cacheInfo.MapPathResult);
        }

        private string MapPathActual(string siteID, VirtualPath path) {
            string appID = GetRootAppIDFromSiteID(siteID);
            string physicalPath = MBMapPath(appID, path.VirtualPathString);
            return physicalPath;
        }

        private string GetRootAppIDFromSiteID(string siteId) {
            return LMW3SVC_PREFIX + siteId + ROOT_SUFFIX;
        }

        private string GetAboPath(string siteID, string path) {
            string rootAppID = GetRootAppIDFromSiteID(siteID);
            string aboPath = rootAppID + FixupPathSlash(path);
            return aboPath;
        }

        private string FixupPathSlash(string path) {
            if (path == null) {
                return null;
            }

            int l = path.Length;
            if (l == 0 || path[l-1] != '/') {
                return path;
            }

            return path.Substring(0, l-1);
        }

        //
        // Metabase access functions
        //
        private bool MBGetSiteNameFromSiteID(string siteID, out string siteName) {
            string appID = GetRootAppIDFromSiteID(siteID);
            StringBuilder sb = new StringBuilder(BUFSIZE);
            int r = UnsafeNativeMethods.IsapiAppHostGetSiteName(appID, sb, sb.Capacity);
            if (r == 1) {
                siteName = sb.ToString();
                return true;
            }
            else {
                siteName = String.Empty;
                return false;
            }
        }

        private bool MBGetSiteIDFromSiteName(string siteName, out string siteID) {
            StringBuilder sb = new StringBuilder(BUFSIZE);
            int r = UnsafeNativeMethods.IsapiAppHostGetSiteId(siteName, sb, sb.Capacity);
            if (r == 1) {
                siteID = sb.ToString();
                return true;
            }
            else {
                siteID = String.Empty;
                return false;
            }
        }


        private string MBMapPath(string appID, string path) {
            // keep growing the buffer to support paths longer than MAX_PATH
            int bufSize = BUFSIZE;
            StringBuilder sb;
            int r;

            for (;;) {
                sb = new StringBuilder(bufSize);
                r = UnsafeNativeMethods.IsapiAppHostMapPath(appID, path, sb, sb.Capacity);
                Debug.Trace("MapPath", "IsapiAppHostMapPath(" + path + ") returns " + r);

                if (r == -2) {
                    // insufficient buffer
                    bufSize *= 2;
                }
                else {
                    break;
                }
            }

            if (r == -1) {
                // special case access denied error
                throw new HostingEnvironmentException(
                    SR.GetString(SR.Cannot_access_mappath_title),
                    SR.GetString(SR.Cannot_access_mappath_details));
            }

            string physicalPath;
            if (r == 1) {
                physicalPath = sb.ToString();
            }
            else {
                physicalPath = null;
            }

            return physicalPath;
        }

        private string[] MBGetVirtualSubdirs(string aboPath, bool inApp) {
            StringBuilder sb = new StringBuilder(BUFSIZE);
            int index = 0;
            ArrayList list = new ArrayList();
            for (;;) {
                sb.Length = 0;
                int r = UnsafeNativeMethods.IsapiAppHostGetNextVirtualSubdir(aboPath, inApp, ref index, sb, sb.Capacity);
                if (r == 0)
                    break;

                string subdir = sb.ToString();
                list.Add(subdir);
            }

            string[] subdirs = new string[list.Count];
            list.CopyTo(subdirs);
            return subdirs;
        }

        private bool MBGetUncUser(string aboPath, out string username, out string password) {
            StringBuilder usr = new StringBuilder(BUFSIZE);
            StringBuilder pwd = new StringBuilder(BUFSIZE);
            int r = UnsafeNativeMethods.IsapiAppHostGetUncUser(aboPath, usr, usr.Capacity, pwd, pwd.Capacity);
            if (r == 1) {
                username = usr.ToString();
                password = pwd.ToString();
                return true;
            }
            else {
                username = null;
                password = null;
                return false;
            }
        }

        private int MBGetW3WPMemoryLimitInKB() {
            return UnsafeNativeMethods.GetW3WPMemoryLimitInKB();
        }

        private string MBGetAppPath(string aboPath) {
            StringBuilder buf = new StringBuilder(aboPath.Length + 1);
            int r = UnsafeNativeMethods.IsapiAppHostGetAppPath(aboPath, buf, buf.Capacity);
            string appAboPath;
            if (r == 1) {
                appAboPath = buf.ToString();
            }
            else {
                appAboPath = null;
            }

            return appAboPath;
        }
    }
}

