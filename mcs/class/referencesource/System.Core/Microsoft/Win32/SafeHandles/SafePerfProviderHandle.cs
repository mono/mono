//------------------------------------------------------------------------------
// <copyright file="SafePerfProviderHandle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Win32.SafeHandles {
    using System;
    using System.Threading;
    using System.Diagnostics;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;

#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class SafePerfProviderHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafePerfProviderHandle() : base(true) {}

        protected override bool ReleaseHandle() {
            IntPtr tempProviderHandle = handle;

            if (Interlocked.Exchange(ref handle, IntPtr.Zero) != IntPtr.Zero) {
                uint Status = UnsafeNativeMethods.PerfStopProvider(tempProviderHandle);
                Debug.Assert(Status == (uint)UnsafeNativeMethods.ERROR_SUCCESS, "PerfStopProvider() fails");
                // ERROR_INVALID_PARAMETER
            }
            return true;
        }
    }
}
