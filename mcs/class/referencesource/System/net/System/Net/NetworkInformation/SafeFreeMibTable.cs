using System;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.Net.NetworkInformation
{
    [SuppressUnmanagedCodeSecurity]
    internal class SafeFreeMibTable : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeFreeMibTable() : base(true) { }

        protected override bool ReleaseHandle()
        {
            UnsafeNetInfoNativeMethods.FreeMibTable(base.handle);
            base.handle = IntPtr.Zero;
            return true;
        }
    }
}
