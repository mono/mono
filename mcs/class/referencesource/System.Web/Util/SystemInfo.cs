//------------------------------------------------------------------------------
// <copyright file="SystemInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {

    internal static class SystemInfo {
        static int  _trueNumberOfProcessors;

        static internal int GetNumProcessCPUs() {
            if (_trueNumberOfProcessors == 0) {
                UnsafeNativeMethods.SYSTEM_INFO si;
                UnsafeNativeMethods.GetSystemInfo(out si);

                if (si.dwNumberOfProcessors == 1) {
                    _trueNumberOfProcessors = 1;
                }
                else {
                    // KERNEL32.DLL:GetCurrentProcess() always returns -1 under NT
                    // Note: not really a handle (no need to CloseHandle())
                    IntPtr processHandle = UnsafeNativeMethods.INVALID_HANDLE_VALUE;
                    IntPtr processAffinityMask;
                    IntPtr systemAffinityMask;
                    int returnCode = UnsafeNativeMethods.GetProcessAffinityMask(
                            processHandle, out processAffinityMask, out systemAffinityMask);

                    if (returnCode == 0) {
                        _trueNumberOfProcessors = 1;
                    }
                    else {
                        // if cpu affinity is set to a single processor busy waiting is a waste of time
                        int numProcessors = 0;
                        if (IntPtr.Size == 4) {
                            uint mask = (uint) processAffinityMask;
                            for (; mask != 0; mask >>= 1) {
                                if ((mask & 1) == 1) {
                                    ++numProcessors;
                                }
                            }
                        }
                        else {
                            ulong mask = (ulong) processAffinityMask;
                            for (; mask != 0; mask >>= 1) {
                                if ((mask & 1) == 1) {
                                    ++numProcessors;
                                }
                            }
                        }

                        _trueNumberOfProcessors = numProcessors;
                    }
                }
            }

            Debug.Assert(_trueNumberOfProcessors > 0);
            return _trueNumberOfProcessors;
        }
    }
}
