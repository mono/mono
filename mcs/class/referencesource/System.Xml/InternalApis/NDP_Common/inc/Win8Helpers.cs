//------------------------------------------------------------------------------
// <copyright file="Compilation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace Microsoft.Win32 {
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        internal const String KERNEL32 = "kernel32.dll";

        // WinError.h codes:
        internal const int ERROR_INSUFFICIENT_BUFFER     = 0x007A;
        internal const int ERROR_NO_PACKAGE_IDENTITY     = 0x3d54;

        // AppModel.h functions (Win8+)
        [DllImport(KERNEL32, CharSet = CharSet.None, EntryPoint = "GetCurrentPackageId")]
        [System.Security.SecurityCritical]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern Int32 _GetCurrentPackageId(
            ref Int32 pBufferLength,
            Byte[] pBuffer);

        // Copied from Win32Native.cs
        // Note - do NOT use this to call methods.  Use P/Invoke, which will
        // do much better things w.r.t. marshaling, pinning memory, security 
        // stuff, better interactions with thread aborts, etc.  This is used
        // solely by DoesWin32MethodExist for avoiding try/catch EntryPointNotFoundException
        // in scenarios where an OS Version check is insufficient
        [DllImport(KERNEL32, CharSet=CharSet.Ansi, BestFitMapping=false, SetLastError=true, ExactSpelling=true)]
        [ResourceExposure(ResourceScope.None)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, String methodName);

        [DllImport(KERNEL32, CharSet=CharSet.Auto, BestFitMapping=false, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.Process)]  // Is your module side-by-side?
        private static extern IntPtr GetModuleHandle(String moduleName);

        [System.Security.SecurityCritical]  // auto-generated
        private static bool DoesWin32MethodExist(String moduleName, String methodName)
        {
            // GetModuleHandle does not increment the module's ref count, so we don't need to call FreeLibrary.
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule == IntPtr.Zero) {
                Debug.Assert(hModule != IntPtr.Zero, "GetModuleHandle failed.  Dll isn't loaded?");
                return false;
            }
            IntPtr functionPointer = GetProcAddress(hModule, methodName);
            return (functionPointer != IntPtr.Zero);       
        }

        [System.Security.SecuritySafeCritical]
        private static bool _IsPackagedProcess()
        {
            OperatingSystem os = Environment.OSVersion;
            if(os.Platform == PlatformID.Win32NT && os.Version >= new Version(6,2,0,0) && DoesWin32MethodExist(KERNEL32, "GetCurrentPackageId"))
            {
                Int32 bufLen = 0;
                // Will return ERROR_INSUFFICIENT_BUFFER when running within a packaged application,
                // and will return ERROR_NO_PACKAGE_IDENTITY otherwise.
                return _GetCurrentPackageId(ref bufLen, null) == ERROR_INSUFFICIENT_BUFFER;
            }
            else
            {   // We must be running on a downlevel OS.
                return false;
            }
        }

        [System.Security.SecuritySafeCritical]
        internal static Lazy<bool> IsPackagedProcess = new Lazy<bool>(() => _IsPackagedProcess());
    }
}

