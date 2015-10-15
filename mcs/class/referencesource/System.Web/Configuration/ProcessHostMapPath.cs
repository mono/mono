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
    internal sealed class ProcessHostMapPath  : IConfigMapPath, IConfigMapPath2 {
        IProcessHostSupportFunctions _functions;
        internal static string _DefaultPhysicalPathOnMapPathFailure = null;

        static ProcessHostMapPath() {
            HttpRuntime.ForceStaticInit();
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "We carefully control this method's caller.")]
        internal ProcessHostMapPath(IProcessHostSupportFunctions functions) {
            // if the functions are null and we're
            // not in a worker process, init the mgdeng config system
            if (null == functions) {
                ProcessHostConfigUtils.InitStandaloneConfig();
            }

            // we need to explicit create a COM proxy in this app domain
            // so we don't go back to the default domain or have lifetime issues
            if (null != functions) {
                _functions = Misc.CreateLocalSupportFunctions(functions);
            }

            // proactive set the config functions for
            // webengine in case this is a TCP init path
            if (null != _functions ) {
               IntPtr configSystem = _functions.GetNativeConfigurationSystem();
               Debug.Assert(IntPtr.Zero != configSystem, "null != configSystem");

               if (IntPtr.Zero != configSystem) {
                   // won't fail if valid pointer
                   // no cleanup needed, we don't own instance
                   UnsafeIISMethods.MgdSetNativeConfiguration(configSystem);
                }
            }
        }

        string IConfigMapPath.GetMachineConfigFilename() {
            return HttpConfigurationSystem.MachineConfigurationFilePath;
        }

        string IConfigMapPath.GetRootWebConfigFilename() {
            string rootWeb = null;

            if (null != _functions) {
                rootWeb = _functions.GetRootWebConfigFilename();
            }

            if (String.IsNullOrEmpty(rootWeb)) {
                rootWeb = HttpConfigurationSystem.RootWebConfigurationFilePath;
            }
            Debug.Trace("MapPath", "ProcHostMP.GetRootWebConfigFilename = " +
                    rootWeb);

            Debug.Assert(!String.IsNullOrEmpty(rootWeb), "rootWeb != null or empty");

            return rootWeb;
        }

        private void GetPathConfigFilenameWorker(string siteID, VirtualPath path, out string directory, out string baseName) {
            directory = MapPathCaching(siteID, path);
            if (directory != null) {
                baseName = HttpConfigurationSystem.WebConfigFileName;
            }
            else {
                baseName = null;
            }

            Debug.Trace("MapPath", "ProcHostMP.GetPathConfigFilename(" + siteID + ", " + path + ")\n" +
                    " result = " + directory + " and " + baseName + "\n");
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
            Debug.Trace("MapPath", "ProcHostMP.GetDefaultSiteNameAndID\n");
            siteID = ProcessHostConfigUtils.DEFAULT_SITE_ID_STRING;
            siteName = ProcessHostConfigUtils.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT);
        }


        void IConfigMapPath.ResolveSiteArgument(string siteArgument, out string siteName, out string siteID) {
            Debug.Trace("MapPath", "ProcHostMP.ResolveSiteArgument(" + siteArgument + ")\n");


            if (    String.IsNullOrEmpty(siteArgument) ||
                    StringUtil.EqualsIgnoreCase(siteArgument, ProcessHostConfigUtils.DEFAULT_SITE_ID_STRING) ||
                    StringUtil.EqualsIgnoreCase(siteArgument, ProcessHostConfigUtils.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT))) {

                siteName = ProcessHostConfigUtils.GetSiteNameFromId(ProcessHostConfigUtils.DEFAULT_SITE_ID_UINT);
                siteID = ProcessHostConfigUtils.DEFAULT_SITE_ID_STRING;
            }
            else {
                siteName = String.Empty;
                siteID   = String.Empty;

                string resolvedName = null;
                if (IISMapPath.IsSiteId(siteArgument)) {
                    uint id;

                    if (UInt32.TryParse(siteArgument, out id)) {
                        resolvedName = ProcessHostConfigUtils.GetSiteNameFromId(id);
                    }
                }
                // try to resolve the string
                else {
                    uint id = UnsafeIISMethods.MgdResolveSiteName(IntPtr.Zero, siteArgument);
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
            Debug.Trace("MapPath", "ProcHostMP.MapPath(" + siteID + ", " + path + ")\n");
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
            Debug.Trace("MapPath", "ProcHostMP.GetAppPathForPath(" + siteID + ", " + path.VirtualPathString + ")\n");

            uint siteValue  = 0;
            if (!UInt32.TryParse(siteID, out siteValue)) {
                return VirtualPath.RootVirtualPath;
            }

            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            string appPath;
            try {
                int result = UnsafeIISMethods.MgdGetAppPathForPath(IntPtr.Zero, siteValue, path.VirtualPathString, out pBstr, out cBstr);
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
                    // If it does exist, the existing value will be returned (Dev10 
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
                        try {
                            string physicalPath = null;
                            uint siteIDValue;
                            if (UInt32.TryParse(siteID, out siteIDValue)) {
                                string siteName = ProcessHostConfigUtils.GetSiteNameFromId(siteIDValue);
                                physicalPath = ProcessHostConfigUtils.MapPathActual(siteName, path);
                            }
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

                            cacheInfo.MapPathResult = physicalPath;
                        } catch (Exception e) {
                            if (HttpRuntime.IsMapPathRelaxed) {
                                cacheInfo.MapPathResult = HttpRuntime.GetRelaxedMapPathResult(null);
                            } else {
                                cacheInfo.CachedException = e;
                                cacheInfo.Evaluated=true;
                                throw;
                            }
                        }

                        cacheInfo.Evaluated = true;
                    }
                }
            }

            // Throw an exception if required
            if (cacheInfo.CachedException != null) {
                throw cacheInfo.CachedException;
            }

            return MatchResult(originalPath, cacheInfo.MapPathResult);
        }

        private string MatchResult(VirtualPath path, string result) {
            if (string.IsNullOrEmpty(result)) {
                return result;
            }

            result = result.Replace('/', '\\');

            // ensure extra '\\' in the physical path if the virtual path had extra '/'
            // and the other way -- no extra '\\' in physical if virtual didn't have it.
            if (path.HasTrailingSlash) {
                if (!UrlPath.PathEndsWithExtraSlash(result)) {
                    result = result + "\\";
                }
            }
            else {
                if (UrlPath.PathEndsWithExtraSlash(result)) {
                    result = result.Substring(0, result.Length - 1);
                }
            }

            return result;
        }
    }
}
