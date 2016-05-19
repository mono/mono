// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// RuntimeHelpers
//    This class defines entry point for multi-core JIT API
//
// Date: Oct 2010
//
namespace System.Runtime {

    using System;
    
    using System.Reflection;

    using System.Security;
    using System.Security.Permissions;
    
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Runtime.CompilerServices;

#if FEATURE_MULTICOREJIT

    public static class ProfileOptimization
    {
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern void InternalSetProfileRoot(string directoryPath);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern void InternalStartProfile(string profile, IntPtr ptrNativeAssemblyLoadContext);

        [SecurityCritical]
        public static void SetProfileRoot(string directoryPath)
        {
            InternalSetProfileRoot(directoryPath);
        }

        [SecurityCritical]
        public static void StartProfile(string profile)
        {
            InternalStartProfile(profile, IntPtr.Zero);
        }
    }

#endif
}

