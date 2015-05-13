namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    //[SecurityCritical(SecurityCriticalScope.Everything)]
	[SecurityCritical]
    internal sealed class CoTaskMemSafeHandle : SafeHandle
    {
        internal CoTaskMemSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        internal IntPtr GetMemory()
        {
            return base.handle;
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeCoTaskMem(base.handle);
            base.handle = IntPtr.Zero;
            return true;
        }

        internal void SetMemory(IntPtr handle)
        {
            base.SetHandle(handle);
        }

        public override bool IsInvalid
        {
            get
            {
                if (!base.IsClosed)
                {
                    return (base.handle == IntPtr.Zero);
                }
                return true;
            }
        }

        public static CoTaskMemSafeHandle Zero
        {
            get
            {
                return new CoTaskMemSafeHandle();
            }
        }
    }
}

