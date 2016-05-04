//------------------------------------------------------------------------------
// <copyright file="StopListeningWaitHandle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Web.Hosting;

    // This is a ManualResetEvent that corresponds to the OnGlobalStopListening event.
    internal sealed class StopListeningWaitHandle : WaitHandle {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = @"This is a pseudohandle so shouldn't be closed.")]
        private static IntPtr _processHandle = GetCurrentProcess();

        public StopListeningWaitHandle() {
            // This handle is process-wide and not ref counted, so no need to wrap inside a SafeHandle
            IntPtr eventHandle = UnsafeIISMethods.MgdGetStopListeningEventHandle();

            // Per documentation for RegisterWaitForSingleObject, we need to duplicate handles
            // before asynchronously waiting on them.
            SafeWaitHandle safeWaitHandle;
            bool succeeded = DuplicateHandle(
                hSourceProcessHandle: _processHandle,
                hSourceHandle: eventHandle,
                hTargetProcessHandle: _processHandle,
                lpTargetHandle: out safeWaitHandle,
                dwDesiredAccess: 0,
                bInheritHandle: false,
                dwOptions: 2 /* DUPLICATE_SAME_ACCESS */);

            if (!succeeded) {
                int hr = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hr);
            }

            this.SafeWaitHandle = safeWaitHandle;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [SuppressUnmanagedCodeSecurity]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        private static extern bool DuplicateHandle([In] IntPtr hSourceProcessHandle, [In] IntPtr hSourceHandle, [In] IntPtr hTargetProcessHandle, [Out] out SafeWaitHandle lpTargetHandle, [In] uint dwDesiredAccess, [In] bool bInheritHandle, [In] uint dwOptions);
    }
}
