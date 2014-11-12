// <OWNER>clrbclc</OWNER>
namespace Microsoft.Win32.SafeHandles {
#if !FEATURE_PAL
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;

    [System.Security.SecurityCritical]  // auto-generated
    internal sealed class SafeLocalAllocHandle : SafeBuffer {
        private SafeLocalAllocHandle () : base(true) {}

        // 0 is an Invalid Handle
        internal SafeLocalAllocHandle (IntPtr handle) : base (true) {
            SetHandle(handle);
        }

        internal static SafeLocalAllocHandle InvalidHandle {
            get { return new SafeLocalAllocHandle(IntPtr.Zero); }
        }

        [System.Security.SecurityCritical]
        override protected bool ReleaseHandle()
        {
            return Win32Native.LocalFree(handle) == IntPtr.Zero;
        }
    }
#endif
}
