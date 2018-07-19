//------------------------------------------------------------------------------
// <copyright file="ExpressServerConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Web.Configuration {
    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Web.Hosting;

    //
    // Uses IIS Express native config
    //
    internal sealed class ExpressServerConfig : IServerConfig, IServerConfig2, IConfigMapPath, IConfigMapPath2, IDisposable {
        static object                     s_initLock = new Object();
        static ExpressServerConfig        s_instance;

        NativeConfig                      _nativeConfig;
        string                            _currentAppSiteName;

        // called by HostingEnvironment to initiliaze the singleton config
        // instance for the domain
        static internal IServerConfig GetInstance(string version) {
            if (s_instance == null) {
                lock (s_initLock) {
                    if (s_instance == null) {
                        if (Thread.GetDomain().IsDefaultAppDomain()) {
                            throw new InvalidOperationException();
                        }
                        s_instance = new ExpressServerConfig(version);
                    }
                }
            }
            return s_instance;
        }

        static ExpressServerConfig() {
            HttpRuntime.ForceStaticInit();
        }

        private ExpressServerConfig() {
            // hidden
        }

        internal ExpressServerConfig(string version) {
            if (version == null) {
                throw new ArgumentNullException("version");
            }
            _nativeConfig = new NativeConfig(version);
        }

        string CurrentAppSiteName {
            get {
                string name = _currentAppSiteName;
                if (name == null) {
                    name = HostingEnvironment.SiteNameNoDemand;
                    if (name == null) {
                        name = _nativeConfig.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT);
                    }
                    _currentAppSiteName = name;
                }
                return name;
            }
        }

        void IDisposable.Dispose() {
            NativeConfig nativeConfig = _nativeConfig;
            _nativeConfig = null;
            if (nativeConfig != null) {
                nativeConfig.Dispose();
            }
        }

        string IServerConfig.GetSiteNameFromSiteID(string siteID) {
            uint siteIDValue;

            if (!UInt32.TryParse(siteID, out siteIDValue)) {
                Debug.Assert(false, "siteID is not numeric");
                return String.Empty;
            }

            return _nativeConfig.GetSiteNameFromId(siteIDValue);
        }

        // if appHost is null, we use the site name for the current application
        string IServerConfig.MapPath(IApplicationHost appHost, VirtualPath path) {
            string siteName = (appHost == null) ? CurrentAppSiteName : appHost.GetSiteName();
            string physicalPath = _nativeConfig.MapPathDirect(siteName, path);
            if (FileUtil.IsSuspiciousPhysicalPath(physicalPath)) {
                throw new InvalidOperationException(SR.GetString(SR.Cannot_map_path, path.VirtualPathString));
            }
            return physicalPath;
        }

        string[] IServerConfig.GetVirtualSubdirs(VirtualPath path, bool inApp) {
            // WOS 1956227: PERF: inactive applications on the web server degrade Working Set by 10%
            // It is very expensive to get a list of subdirs not in the application if there are a lot of applications,
            // so instead, use ProcessHostServerConfig.IsWithinApp to check if a particular path is in the app.
            if (inApp == false) {
                throw new NotSupportedException();
            }
            
            string vpath = path.VirtualPathString;
            string [] dirList = null;
            int dirListCount = 0;
            
            IntPtr pAppCollection = IntPtr.Zero;
            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            try {
                int count = 0;
                int result = _nativeConfig.MgdGetAppCollection(CurrentAppSiteName, vpath, out pBstr, out cBstr, out pAppCollection, out count);
                if (result < 0 || pBstr == IntPtr.Zero) {
                    throw new InvalidOperationException(SR.GetString(SR.Cant_Enumerate_NativeDirs, result));
                }
                string appRoot = StringUtil.StringFromWCharPtr(pBstr, cBstr);
                Marshal.FreeBSTR(pBstr);
                pBstr = IntPtr.Zero;
                cBstr = 0;
                dirList = new string[count];

                int lenNoTrailingSlash = vpath.Length;
                if (vpath[lenNoTrailingSlash - 1] == '/') {
                    lenNoTrailingSlash--;
                }
                int lenAppRoot = appRoot.Length;
                string appRootRelativePath = (lenNoTrailingSlash > lenAppRoot) ? vpath.Substring(lenAppRoot, lenNoTrailingSlash - lenAppRoot) : String.Empty;

                for (uint index = 0; index < count; index++) {
                    result = UnsafeIISMethods.MgdGetNextVPath(pAppCollection, index, out pBstr, out cBstr);
                    if (result < 0 || pBstr == IntPtr.Zero) {
                        throw new InvalidOperationException(SR.GetString(SR.Cant_Enumerate_NativeDirs, result));
                    }
                    // if cBstr = 1, then pBstr = "/" and can be ignored
                    string subVdir = (cBstr > 1) ? StringUtil.StringFromWCharPtr(pBstr, cBstr) : null;
                    Marshal.FreeBSTR(pBstr);
                    pBstr = IntPtr.Zero;
                    cBstr = 0;

                    // only put the subVdir in our list if it is a subdirectory of the specified vpath
                    if (subVdir != null && subVdir.Length > appRootRelativePath.Length) {
                        if (appRootRelativePath.Length == 0) {
                            if (subVdir.IndexOf('/', 1) == -1) {
                                dirList[dirListCount++] = subVdir.Substring(1);
                            }
                        }
                        else if (StringUtil.EqualsIgnoreCase(appRootRelativePath, 0, subVdir, 0, appRootRelativePath.Length)) {
                            int nextSlashIndex = subVdir.IndexOf('/', 1 + appRootRelativePath.Length);
                            if (nextSlashIndex > -1) {
                                dirList[dirListCount++] = subVdir.Substring(appRootRelativePath.Length + 1, nextSlashIndex - appRootRelativePath.Length);
                            }
                            else {
                                dirList[dirListCount++] = subVdir.Substring(appRootRelativePath.Length + 1);
                            }
                        }
                    }
                }
            }
            finally {
                if (pAppCollection != IntPtr.Zero) {
                    Marshal.Release(pAppCollection);
                    pAppCollection = IntPtr.Zero;
                }
                if (pBstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstr);
                    pBstr = IntPtr.Zero;
                }
            }

            string[] subdirs = null;
            if (dirListCount > 0) {
                subdirs = new string[dirListCount];
                for (int i = 0; i < subdirs.Length; i++) {
                    subdirs[i] = dirList[i];
                }
            }
            return subdirs;
        }

        bool IServerConfig2.IsWithinApp(string virtualPath) {
            return _nativeConfig.MgdIsWithinApp(CurrentAppSiteName, HttpRuntime.AppDomainAppVirtualPathString, virtualPath);
        }

        bool IServerConfig.GetUncUser(IApplicationHost appHost, VirtualPath path, out string username, out string password) {
            bool foundCreds = false;
            username = null;
            password = null;

            IntPtr pBstrUserName = IntPtr.Zero;
            int cBstrUserName = 0;
            IntPtr pBstrPassword = IntPtr.Zero;
            int cBstrPassword = 0;

            try {
                int result = _nativeConfig.MgdGetVrPathCreds( appHost.GetSiteName(),
                                                              path.VirtualPathString,
                                                              out pBstrUserName,
                                                              out cBstrUserName,
                                                              out pBstrPassword,
                                                              out cBstrPassword);
                if (result == 0) {
                    username = (cBstrUserName > 0) ? StringUtil.StringFromWCharPtr(pBstrUserName, cBstrUserName) : null;
                    password = (cBstrPassword > 0) ? StringUtil.StringFromWCharPtr(pBstrPassword, cBstrPassword) : null;
                    foundCreds = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));
                }
            }
            finally {
                if (pBstrUserName != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstrUserName);
                }
                if (pBstrPassword != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstrPassword);
                }
            }

            return foundCreds;
        }

        long IServerConfig.GetW3WPMemoryLimitInKB() {
            long limit = 0;

            int result = UnsafeIISMethods.MgdGetMemoryLimitKB( out limit );
            if (result < 0)
                return 0;

            return limit;
        }        


        //
        // IConfigMapPath 
        //

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

        void IConfigMapPath.GetPathConfigFilename(
                string siteID, string path, out string directory, out string baseName) {
            GetPathConfigFilenameWorker(siteID, VirtualPath.Create(path), out directory, out baseName);
        }

        // IConfigMapPath2 VirtualPath variant
        void IConfigMapPath2.GetPathConfigFilename(
                    string siteID,
                    VirtualPath path,
                    out string directory,
                    out string baseName) {
            GetPathConfigFilenameWorker(siteID, path, out directory, out baseName);
        }

        void IConfigMapPath.GetDefaultSiteNameAndID(out string siteName, out string siteID) {
            siteID = ProcessHostConfigUtils.DEFAULT_SITE_ID_STRING;
            siteName = _nativeConfig.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT);
        }

        void IConfigMapPath.ResolveSiteArgument(string siteArgument, out string siteName, out string siteID) {
            if (    String.IsNullOrEmpty(siteArgument) ||
                    StringUtil.EqualsIgnoreCase(siteArgument, ProcessHostConfigUtils.DEFAULT_SITE_ID_STRING) ||
                    StringUtil.EqualsIgnoreCase(siteArgument, _nativeConfig.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT))) {

                siteName = _nativeConfig.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT);
                siteID = ProcessHostConfigUtils.DEFAULT_SITE_ID_STRING;
            }
            else {
                siteName = String.Empty;
                siteID   = String.Empty;

                string resolvedName = null;
                if (IISMapPath.IsSiteId(siteArgument)) {
                    uint id;

                    if (UInt32.TryParse(siteArgument, out id)) {
                        resolvedName = _nativeConfig.GetSiteNameFromId(id);
                    }
                }
                // try to resolve the string
                else {
                    uint id = _nativeConfig.MgdResolveSiteName(siteArgument);
                    if (id != 0) {
                        siteID = id.ToString(CultureInfo.InvariantCulture);
                        siteName = siteArgument;
                        return;
                    }
                }

                if (!String.IsNullOrEmpty(resolvedName)) {
                    siteName = resolvedName;
                    siteID = siteArgument;
                }
                else {
                    siteName = siteArgument;
                    siteID = String.Empty;
                }
            }

            Debug.Assert(!String.IsNullOrEmpty(siteName), "!String.IsNullOrEmpty(siteName), siteArg=" + siteArgument);
        }

        private string MapPathWorker(string siteID, VirtualPath path) {
            return MapPathCaching(siteID, path);
        }

        // IConfigMapPath2 variant with VirtualPath
        string IConfigMapPath2.MapPath(string siteID, VirtualPath path) {
            return MapPathWorker(siteID, path);
        }

        string IConfigMapPath.MapPath(string siteID,  string path) {
            return MapPathWorker(siteID, VirtualPath.Create(path));
        }

        string IConfigMapPath.GetAppPathForPath(string siteID, string path) {
            VirtualPath resolved = GetAppPathForPathWorker(siteID, VirtualPath.Create(path));
            return resolved.VirtualPathString;
        }

        // IConfigMapPath2 variant with VirtualPath
        VirtualPath IConfigMapPath2.GetAppPathForPath(string siteID, VirtualPath path) {
            return GetAppPathForPathWorker(siteID, path);
        }

        VirtualPath GetAppPathForPathWorker(string siteID, VirtualPath path) {
            uint siteValue  = 0;
            if (!UInt32.TryParse(siteID, out siteValue)) {
                return VirtualPath.RootVirtualPath;
            }

            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            string appPath;
            try {
                int result = _nativeConfig.MgdGetAppPathForPath(siteValue, path.VirtualPathString, out pBstr, out cBstr);
                appPath = (result == 0 && cBstr > 0) ? StringUtil.StringFromWCharPtr(pBstr, cBstr) : null;
            }
            finally {
                if (pBstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstr);
                }
            }

            return (appPath != null) ? VirtualPath.Create(appPath) : VirtualPath.RootVirtualPath;
        }

        private string MapPathCaching(string siteID, VirtualPath path) {
            // do we need caching for the designer?
            string physicalPath = _nativeConfig.MapPathDirect(((IServerConfig)this).GetSiteNameFromSiteID(siteID), path);

            if (physicalPath != null && physicalPath.Length == 2 && physicalPath[1] == ':')
                physicalPath += "\\";

            // Throw if the resulting physical path is not canonical, to prevent potential
            // security issues (VSWhidbey 418125)

            if (HttpRuntime.IsMapPathRelaxed) {
                physicalPath = HttpRuntime.GetRelaxedMapPathResult(physicalPath);
            }
            
            if (FileUtil.IsSuspiciousPhysicalPath(physicalPath)) {
                if (HttpRuntime.IsMapPathRelaxed) {
                    physicalPath = HttpRuntime.GetRelaxedMapPathResult(null);
                } else {
                    throw new HttpException(SR.GetString(SR.Cannot_map_path, path));
                }
            }
            return physicalPath;
        }
    }
}


