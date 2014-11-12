// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
//
// File: SafeRegistryHandle.cs
//
// <OWNER>[....]</OWNER>
//
// Implements Microsoft.Win32.SafeHandles.SafeRegistryHandle
//
// ======================================================================================
#if !FEATURE_PAL
namespace Microsoft.Win32.SafeHandles {
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;

    [System.Security.SecurityCritical]
    public sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid {
        [System.Security.SecurityCritical]
        internal SafeRegistryHandle() : base(true) {}

        [System.Security.SecurityCritical]
        public SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle) {
            SetHandle(preexistingHandle);
        }

        [System.Security.SecurityCritical]
        override protected bool ReleaseHandle() {
            return (RegCloseKey(handle) == Win32Native.ERROR_SUCCESS);
        }

        [DllImport(Win32Native.ADVAPI32),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern int RegCloseKey(IntPtr hKey);
    }
}
#endif // !FEATURE_PAL
