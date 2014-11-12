using System;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System.Diagnostics {
    internal class ProcessWaitHandle : WaitHandle {

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal ProcessWaitHandle( SafeProcessHandle processHandle): base() {
            SafeWaitHandle waitHandle = null;
            bool succeeded = NativeMethods.DuplicateHandle(
                new HandleRef(this, NativeMethods.GetCurrentProcess()),
                processHandle,
                new HandleRef(this, NativeMethods.GetCurrentProcess()),
                out waitHandle,
                0,
                false,
                NativeMethods.DUPLICATE_SAME_ACCESS);
                    
            if (!succeeded) {                    
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            this.SafeWaitHandle = waitHandle;         
        }
    }
}
