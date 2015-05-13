namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    //[SecurityCritical(SecurityCriticalScope.Everything)]
	[SecurityCritical]
    internal sealed class CoTaskMemUnicodeSafeHandle : SafeHandle
    {
        internal CoTaskMemUnicodeSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        internal CoTaskMemUnicodeSafeHandle(IntPtr handle, bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
        {
            base.SetHandle(handle);
        }

        internal IntPtr GetMemory()
        {
            return base.handle;
        }

        protected override bool ReleaseHandle()
        {
            Marshal.ZeroFreeCoTaskMemUnicode(base.handle);
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

        public static CoTaskMemUnicodeSafeHandle Zero
        {
            get
            {
                return new CoTaskMemUnicodeSafeHandle();
            }
        }
    }
}

