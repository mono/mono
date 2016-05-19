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
#if MONO
                // In Mono, Marshal.GetHRForLastWin32Error is not implemented;
                // and also DuplicateHandle throws its own exception rather
                // than returning false on error, so this code is unreachable.
                throw new SystemException("Unknown error in DuplicateHandle");
#else
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
#endif
            }

            this.SafeWaitHandle = waitHandle;         
        }
    }
}
