// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: RuntimeEnvironment
**
**
** Purpose: Runtime information
**          
**
=============================================================================*/

using System;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.Versioning;
using StackCrawlMark = System.Threading.StackCrawlMark;

namespace System.Runtime.InteropServices {
[System.Runtime.InteropServices.ComVisible(true)]
#if FEATURE_CORECLR
    static
#endif
    public class RuntimeEnvironment {

#if !FEATURE_CORECLR
        // This should have been a static class, but wasn't as of v3.5.  Clearly, this is
        // broken.  We'll keep this in V4 for binary compat, but marked obsolete as error
        // so migrated source code gets fixed.  On Silverlight, this type exists but is
        // not public.
        [Obsolete("Do not create instances of the RuntimeEnvironment class.  Call the static methods directly on this type instead", true)]
        public RuntimeEnvironment()
        {
            // Should not have been instantiable - here for binary compatibility in V4.
        }
#endif
#if !MONO
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern String GetModuleFileName();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern String GetDeveloperPath();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern String GetHostBindingFile();
#endif
#if !FEATURE_CORECLR && !MONO
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern void _GetSystemVersion(StringHandleOnStack retVer);
#endif //!FEATURE_CORECLR

        public static bool FromGlobalAccessCache(Assembly a)
        {
            return a.GlobalAssemblyCache;
        }
        
#if !FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // public member
#endif
        [MethodImpl (MethodImplOptions.NoInlining)]
        public static String GetSystemVersion()
        {
#if FEATURE_CORECLR || MONO

            return Assembly.GetExecutingAssembly().ImageRuntimeVersion;

#else // FEATURE_CORECLR

            String ver = null;
            _GetSystemVersion(JitHelpers.GetStringHandleOnStack(ref ver));
            return ver;

#endif // FEATURE_CORECLR

        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static String GetRuntimeDirectory()
        {
#if !MOBILE
            //
            // Workaround for csc hardcoded behaviour where executing mscorlib
            // location is always the first path to search for references unless
            // they have full path. Mono build is using simple assembly names for
            // references and -lib for path which is by default csc dehaviour never
            // used
            //
            var sdk = Environment.GetEnvironmentVariable ("CSC_SDK_PATH_DISABLED");
            if (sdk != null)
                return null;
#endif
            String dir = GetRuntimeDirectoryImpl();
#if MONO_FEATURE_CAS
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dir).Demand();
#endif
            return dir;
        }

#if MONO
        static String GetRuntimeDirectoryImpl()
        {
            return Path.GetDirectoryName (typeof (object).Assembly.Location);
        }
#else
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern String GetRuntimeDirectoryImpl();
#endif
        
        // Returns the system ConfigurationFile
        public static String SystemConfigurationFile {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
#if MONO
                String path = Environment.GetMachineConfigPath ();
#else
                StringBuilder sb = new StringBuilder(Path.MAX_PATH);
                sb.Append(GetRuntimeDirectory());
                sb.Append(AppDomainSetup.RuntimeConfigurationFile);
                String path = sb.ToString();
#endif
                
#if MONO_FEATURE_CAS
                // Do security check
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
#endif
                return path;
            }
        }

#if FEATURE_COMINTEROP || MONO
#if MONO
        private static IntPtr GetRuntimeInterfaceImpl(Guid clsid, Guid riid)
        {
            throw new NotSupportedException();
        }
#else
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr GetRuntimeInterfaceImpl(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
#endif

        //
        // This function does the equivalent of calling GetInterface(clsid, riid) on the
        // ICLRRuntimeInfo representing this runtime. See MetaHost.idl for a list of
        // CLSIDs and IIDs supported by this method.
        //
        // Returns unmanaged pointer to requested interface on success. Throws
        // COMException with failed HR if there is a QI failure.
        //
        [System.Security.SecurityCritical]  // do not allow partial trust callers
        [ComVisible(false)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static IntPtr GetRuntimeInterfaceAsIntPtr(Guid clsid, Guid riid)
        {
            return GetRuntimeInterfaceImpl(clsid, riid);
        }

        //
        // This function does the equivalent of calling GetInterface(clsid, riid) on the
        // ICLRRuntimeInfo representing this runtime. See MetaHost.idl for a list of
        // CLSIDs and IIDs supported by this method.
        //
        // Returns an RCW to requested interface on success. Throws
        // COMException with failed HR if there is a QI failure.
        //
        [System.Security.SecurityCritical]  // do not allow partial trust callers
        [ComVisible(false)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static object GetRuntimeInterfaceAsObject(Guid clsid, Guid riid)
        {
            IntPtr p = IntPtr.Zero;
            try {
                p = GetRuntimeInterfaceImpl(clsid, riid);
                return Marshal.GetObjectForIUnknown(p);
            } finally {
                if(p != IntPtr.Zero) {
                    Marshal.Release(p);
                }
            }
        }

#endif // FEATURE_COMINTEROP
    }
}
