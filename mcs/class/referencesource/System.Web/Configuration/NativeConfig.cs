//------------------------------------------------------------------------------
// <copyright file="NativeConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web.Hosting;
    using System.Web.Util;
    

    internal sealed class NativeConfig : CriticalFinalizerObject, IDisposable {
        private IntPtr _nativeConfig;

        private NativeConfig() {
            // hidden
        }

        internal NativeConfig(string version) {
#if (!MONO || !FEATURE_PAL)
            if (version == null) {
                throw new ArgumentNullException("version");
            }
            int hresult = 0;
            using (new IISVersionHelper(version)) {
                hresult = UnsafeIISMethods.MgdCreateNativeConfigSystem(out _nativeConfig);
            }
            Misc.ThrowIfFailedHr(hresult);
#endif
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        ~NativeConfig() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
#if (!MONO || !FEATURE_PAL)
            if (disposing) {
                // release managed resources
            }

            // release native resources
            if (_nativeConfig != IntPtr.Zero) {
                IntPtr pConfigSystem = Interlocked.Exchange(ref _nativeConfig, IntPtr.Zero);
                if (pConfigSystem != IntPtr.Zero) {
                    int hresult = UnsafeIISMethods.MgdReleaseNativeConfigSystem(pConfigSystem);
                    Misc.ThrowIfFailedHr(hresult);
                }
            }
#endif
        }

        internal string GetSiteNameFromId(uint siteID) {
#if (!MONO || !FEATURE_PAL)
            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            string siteName = null;
            try {
                int result = UnsafeIISMethods.MgdGetSiteNameFromId(_nativeConfig, siteID, out pBstr, out cBstr);
                siteName = (result == 0 && pBstr != IntPtr.Zero) ? System.Web.Util.StringUtil.StringFromWCharPtr(pBstr, cBstr) : String.Empty;
            }
            finally {
                if (pBstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstr);
                }
            }
            return siteName;
#else
            return null;
#endif
        }

        internal string MapPathDirect(string siteName, VirtualPath path) {
            string physicalPath = null;
#if (!MONO || !FEATURE_PAL)
            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            try {
                int result = UnsafeIISMethods.MgdMapPathDirect(_nativeConfig, siteName, path.VirtualPathString, out pBstr, out cBstr);
                if (result < 0) {
                    throw new InvalidOperationException(System.Web.SR.GetString(System.Web.SR.Cannot_map_path, path.VirtualPathString));
                }
                physicalPath = (pBstr != IntPtr.Zero) ? System.Web.Util.StringUtil.StringFromWCharPtr(pBstr, cBstr) : null;
            }
            finally {
                if (pBstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(pBstr);
                }
            }
#endif
            return physicalPath;
        }

#if (!MONO || !FEATURE_PAL)
        internal int MgdGetAppCollection(string siteName, string virtualPath, out IntPtr pBstr, out int cBstr, out IntPtr pAppCollection, out int count) {
            return UnsafeIISMethods.MgdGetAppCollection(_nativeConfig, siteName, virtualPath, out pBstr, out cBstr, out pAppCollection, out count);
        }

        internal bool MgdIsWithinApp(string siteName, string appPath, string virtualPath) {
            return UnsafeIISMethods.MgdIsWithinApp(_nativeConfig, siteName, appPath, virtualPath);
        }

        internal int MgdGetVrPathCreds(string siteName, string virtualPath, out IntPtr bstrUserName, out int cchUserName, out IntPtr bstrPassword, out int cchPassword) {
            return UnsafeIISMethods.MgdGetVrPathCreds(_nativeConfig, siteName, virtualPath, out bstrUserName, out cchUserName, out bstrPassword, out cchPassword);
        }

        internal uint MgdResolveSiteName(string siteName) {
            return UnsafeIISMethods.MgdResolveSiteName(_nativeConfig, siteName);
        }

        internal int MgdGetAppPathForPath(uint siteId, string virtualPath, out IntPtr bstrPath, out int cchPath) {
            return UnsafeIISMethods.MgdGetAppPathForPath(_nativeConfig, siteId, virtualPath, out bstrPath, out cchPath);
        }
#else
        // For FEATURE_PAL, we just give it dummy info.  We should track where this is used 
        // and see if we can chop it.
        internal int MgdGetAppCollection(string siteName, string virtualPath, out IntPtr pBstr, out int cBstr, out IntPtr pAppCollection, out int count) {
            pBstr = new IntPtr();
            cBstr = -1;
            pAppCollection = new IntPtr();
            count = 0;

            return -1;
        }

        internal bool MgdIsWithinApp(string siteName, string appPath, string virtualPath) {
            return false;
        }

        internal int MgdGetVrPathCreds(string siteName, string virtualPath, out IntPtr bstrUserName, out int cchUserName, out IntPtr bstrPassword, out int cchPassword) {
            bstrUserName = new IntPtr();
            cchUserName = -1;
            bstrPassword = new IntPtr();
            cchPassword = -1;

            return -1;
        }

        internal uint MgdResolveSiteName(string siteName) {
            return 0;
        }

        internal int MgdGetAppPathForPath(uint siteId, string virtualPath, out IntPtr bstrPath, out int cchPath) {
            bstrPath = new IntPtr();
            cchPath = -1;

            return -1;
        }
#endif
    }
}

