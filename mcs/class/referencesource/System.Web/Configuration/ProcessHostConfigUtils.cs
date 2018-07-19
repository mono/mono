//------------------------------------------------------------------------------
// <copyright file="ProcessHostMapPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Web.Configuration {
    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;
    using System.Security;
    using System.Text;
    using System.Web.Util;
    using System.Web.UI;
    using System.IO;
    using System.Web.Hosting;
    using System.Runtime.ConstrainedExecution;
    
    //
    // Uses IIS 7 native config
    //
    internal static class ProcessHostConfigUtils {
        internal const uint DEFAULT_SITE_ID_UINT = 1;
        internal const string DEFAULT_SITE_ID_STRING = "1";
        private static string s_defaultSiteName;
        private static int s_InitedExternalConfig;
        private static object s_InitedExternalConfigLock = new object();
        private static NativeConfigWrapper _configWrapper;

        // static class ctor
        static ProcessHostConfigUtils() {
            HttpRuntime.ForceStaticInit();
        }

        internal static void InitStandaloneConfig() {
            if (!HostingEnvironment.IsUnderIISProcess && !ServerConfig.UseMetabase && s_InitedExternalConfig == 0) {
                lock (s_InitedExternalConfigLock) {
                    if (s_InitedExternalConfig == 0) {
                        try {
                            _configWrapper = new NativeConfigWrapper();
                        } finally {
                            s_InitedExternalConfig = 1;
                        }
                    }
                }
            }
        }

        internal static string MapPathActual(string siteName, VirtualPath path) {
            string physicalPath = null;
            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            try {
                int result = UnsafeIISMethods.MgdMapPathDirect(IntPtr.Zero, siteName, path.VirtualPathString, out pBstr, out cBstr);
                if (result < 0) {
                    throw new InvalidOperationException(SR.GetString(SR.Cannot_map_path, path.VirtualPathString));
                }
                physicalPath = (pBstr != IntPtr.Zero) ? StringUtil.StringFromWCharPtr(pBstr, cBstr) : null;
            }
            finally {
                if (pBstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstr);
                }                 
            }
            return physicalPath;
        }

        internal static string GetSiteNameFromId(uint siteId) {
            if ( siteId == DEFAULT_SITE_ID_UINT && s_defaultSiteName != null) {
                return s_defaultSiteName;
            }
            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            string siteName = null;
            try {
                int result = UnsafeIISMethods.MgdGetSiteNameFromId(IntPtr.Zero, siteId, out pBstr, out cBstr);
                siteName = (result == 0 && pBstr != IntPtr.Zero) ? StringUtil.StringFromWCharPtr(pBstr, cBstr) : String.Empty;
            }
            finally {
                if (pBstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstr);
                }
            }

            if ( siteId == DEFAULT_SITE_ID_UINT) {
                s_defaultSiteName = siteName;
            }            

            return siteName;
        }

        private class NativeConfigWrapper : CriticalFinalizerObject {
            internal NativeConfigWrapper() {
                int result = UnsafeIISMethods.MgdInitNativeConfig();

                if (result < 0) {
                    s_InitedExternalConfig = 0;
                    throw new InvalidOperationException(SR.GetString(SR.Cant_Init_Native_Config, result.ToString("X8", CultureInfo.InvariantCulture)));
                }                
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            ~NativeConfigWrapper() {
                UnsafeIISMethods.MgdTerminateNativeConfig();                    
            }
        }        
    }    
} 



