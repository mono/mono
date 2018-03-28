//------------------------------------------------------------------------------
// <copyright file="ProcessHostMapPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Web.Configuration {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;

    
    //
    // Uses IIS 7 native config
    //
    internal sealed class ProcessHostServerConfig : IServerConfig, IServerConfig2 {
        static object                     s_initLock = new Object();
        static ProcessHostServerConfig    s_instance;

        string _siteNameForCurrentApplication;

        static internal IServerConfig GetInstance() {
            if (s_instance == null) {
                lock (s_initLock) {
                    if (s_instance == null) {
                        s_instance = new ProcessHostServerConfig();
                    }
                }
            }
            return s_instance;
        }

        static ProcessHostServerConfig() {
            HttpRuntime.ForceStaticInit();
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Private constructor used to make a singleton instance.")]
        ProcessHostServerConfig() {
            if (null == HostingEnvironment.SupportFunctions) {
                ProcessHostConfigUtils.InitStandaloneConfig();
            }
            else {
                IProcessHostSupportFunctions fns = HostingEnvironment.SupportFunctions;
                if (null != fns ) {
                    IntPtr configSystem = fns.GetNativeConfigurationSystem();
                    Debug.Assert(IntPtr.Zero != configSystem, "null != configSystem");

                    if (IntPtr.Zero != configSystem) {
                        // won't fail if valid pointer
                        // no cleanup needed, we don't own instance
                        UnsafeIISMethods.MgdSetNativeConfiguration(configSystem);
                    }
                }
            }
            _siteNameForCurrentApplication = HostingEnvironment.SiteNameNoDemand;
            if (_siteNameForCurrentApplication == null) {
                _siteNameForCurrentApplication = ProcessHostConfigUtils.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT);
            }
        }
        
        string IServerConfig.GetSiteNameFromSiteID(string siteID) {
            uint siteIDValue;

            if (!UInt32.TryParse(siteID, out siteIDValue)) {
                Debug.Assert(false, "siteID is not numeric");
                return String.Empty;
            }

            return ProcessHostConfigUtils.GetSiteNameFromId(siteIDValue);
        }

        // if appHost is null, we use the site name for the current application
        string IServerConfig.MapPath(IApplicationHost appHost, VirtualPath path) {
            string siteName = (appHost == null) ? _siteNameForCurrentApplication : appHost.GetSiteName();
            string physicalPath = ProcessHostConfigUtils.MapPathActual(siteName, path);
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
                int result = UnsafeIISMethods.MgdGetAppCollection(IntPtr.Zero, _siteNameForCurrentApplication, vpath, out pBstr, out cBstr, out pAppCollection, out count);
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
            return UnsafeIISMethods.MgdIsWithinApp(IntPtr.Zero, _siteNameForCurrentApplication, HttpRuntime.AppDomainAppVirtualPathString, virtualPath);
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
                int result = UnsafeIISMethods.MgdGetVrPathCreds( IntPtr.Zero,
                                                                 appHost.GetSiteName(),
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
    }
}

