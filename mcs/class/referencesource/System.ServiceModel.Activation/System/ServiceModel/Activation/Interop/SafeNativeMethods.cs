//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activation.Interop
{
    using System;
    using System.Security;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Runtime.Versioning;
    using System.Runtime;

    [SuppressUnmanagedCodeSecurity]
    static class SafeNativeMethods
    {
        public const int ERROR_NO_TOKEN = 1008;

        const string ADVAPI32 = "advapi32.dll";
        const string KERNEL32 = "kernel32.dll";

        [DllImport(ADVAPI32, SetLastError = true, EntryPoint = "OpenThreadToken")]
        [ResourceExposure(ResourceScope.None)]
        static extern bool OpenThreadTokenCritical(
            [In] IntPtr ThreadHandle,
            [In] TokenAccessLevels DesiredAccess,
            [In] bool OpenAsSelf,
            [Out] out SafeCloseHandleCritical TokenHandle);

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        static extern IntPtr GetCurrentThread();

        [Fx.Tag.SecurityNote(Critical = "Calls two safe native methods: GetCurrentThread and OpenThreadToken." +
            "Marshal.GetLastWin32Error captures current thread token in a SecurityCritical field.")]
        [SecurityCritical]
        internal static bool OpenCurrentThreadTokenCritical(TokenAccessLevels desiredAccess, bool openAsSelf, out SafeCloseHandleCritical tokenHandle, out int error)
        {
            bool result = OpenThreadTokenCritical(GetCurrentThread(), desiredAccess, openAsSelf, out tokenHandle);
            error = Marshal.GetLastWin32Error();
            return result;
        }
    }
}
