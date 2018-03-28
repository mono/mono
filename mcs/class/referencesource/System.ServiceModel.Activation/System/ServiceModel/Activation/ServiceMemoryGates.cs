//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.Channels;
    using System.Threading;

    static class ServiceMemoryGates
    {
        [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical helper methods to check memory status." +
            "Allocates unmanaged resources, can only be called with admin-specified value.")]
        [SecurityCritical]
        internal static bool Check(int minFreeMemoryPercentage, bool throwOnLowMemory, out ulong availableMemoryBytes)
        {
            // Boundary check percentage, if out of bounds Gate is turned off.
            // 0 is defined as disabled. Configuration defines 99 as max allowed so we disable 
            // if we receive something out of range
            availableMemoryBytes = 0;
            if (minFreeMemoryPercentage < 1 || minFreeMemoryPercentage > 99)
            {
                return true;
            }

            UnsafeNativeMethods.MEMORYSTATUSEX memoryStatus = new UnsafeNativeMethods.MEMORYSTATUSEX();
            GetGlobalMemoryStatus(ref memoryStatus);

            if (memoryStatus.ullAvailVirtual < (memoryStatus.ullTotalVirtual / 100 * (ulong)minFreeMemoryPercentage))
            {
                availableMemoryBytes = memoryStatus.ullAvailVirtual;
            }
            else if (memoryStatus.ullAvailPhys < (memoryStatus.ullTotalPhys / 100 * (ulong)minFreeMemoryPercentage))
            {
                availableMemoryBytes = memoryStatus.ullAvailPhys;
            }

            if (TD.ServiceActivationAvailableMemoryIsEnabled())
            {
                TD.ServiceActivationAvailableMemory(availableMemoryBytes);
            }

            if (availableMemoryBytes != 0)
            {
                if (throwOnLowMemory)
                {
                    throw FxTrace.Exception.AsError(new InsufficientMemoryException(
                        SR.Hosting_MemoryGatesCheckFailed(availableMemoryBytes,
                        minFreeMemoryPercentage)));
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses UnsafeNativeMethods to fetch memory status structure, caller must not leak it.")]
        [SecurityCritical]
        static void GetGlobalMemoryStatus(ref UnsafeNativeMethods.MEMORYSTATUSEX memoryStatus)
        {
            memoryStatus.dwLength = (uint)Marshal.SizeOf(typeof(UnsafeNativeMethods.MEMORYSTATUSEX));

            if (!UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatus))
            {
                int error = Marshal.GetLastWin32Error();
                // Treat as the worst case.
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.Hosting_GetGlobalMemoryFailed, new Win32Exception(error)));
            }
        }
    }
}
