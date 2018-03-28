//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activation.Interop
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Microsoft.Win32.SafeHandles;

#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    sealed class SafeHGlobalHandleCritical : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeHGlobalHandleCritical()
            : base(true)
        {
        }

        // 0 is an Invalid Handle
        SafeHGlobalHandleCritical(IntPtr handle)
            : base(true)
        {
            Fx.Assert(handle == IntPtr.Zero, "SafeHGlobalHandleCritical constructor can only be called with IntPtr.Zero.");
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        public static SafeHGlobalHandleCritical InvalidHandle
        {
            get { return new SafeHGlobalHandleCritical(IntPtr.Zero); }
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(string s)
        {
            byte[] bytes = DiagnosticUtility.Utility.AllocateByteArray(checked((s.Length + 1) * 2));
            Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 0);
            return AllocHGlobal(bytes);
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(byte[] bytes)
        {
            SafeHGlobalHandleCritical result = AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, result.DangerousGetHandle(), bytes.Length);
            return result;
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(uint cb)
        {
            // The cast could overflow to minus.
            // Unfortunately, Marshal.AllocHGlobal only takes int.
            return AllocHGlobal((int)cb);
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(int cb)
        {
            if (cb < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("cb", cb, SR.ValueMustBeNonNegative);
            }

            SafeHGlobalHandleCritical result = new SafeHGlobalHandleCritical();

            // CER 
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                IntPtr ptr = Marshal.AllocHGlobal(cb);
                result.SetHandle(ptr);
            }
            return result;
        }
    }
}
