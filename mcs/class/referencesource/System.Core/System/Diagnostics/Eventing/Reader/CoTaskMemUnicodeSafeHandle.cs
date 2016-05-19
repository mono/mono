// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: CoTaskMemUnicodeSafeHandle
**
** Purpose: 
** This internal class is a SafeHandle implementation over a 
** native CoTaskMem allocated via SecureStringToCoTaskMemUnicode.
**
============================================================*/
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Diagnostics.Eventing.Reader {

    //
    // Marked as SecurityCritical due to link demands from inherited
    // SafeHandle members.
    //
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class CoTaskMemUnicodeSafeHandle : SafeHandle {

        internal CoTaskMemUnicodeSafeHandle()
            : base(IntPtr.Zero, true) {
        }

        internal CoTaskMemUnicodeSafeHandle(IntPtr handle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle) {
            SetHandle(handle);
        }

        internal void SetMemory(IntPtr handle) {
            SetHandle(handle);
        }

        internal IntPtr GetMemory() {
            return handle;
        }

        
        public override bool IsInvalid {
            get {
                return IsClosed || handle == IntPtr.Zero;
            }
        }

        protected override bool ReleaseHandle() {
            Marshal.ZeroFreeCoTaskMemUnicode(handle);
            handle = IntPtr.Zero;
            return true;
        }

        // DONT compare CoTaskMemUnicodeSafeHandle with CoTaskMemUnicodeSafeHandle.Zero
        // use IsInvalid instead. Zero is provided where a NULL handle needed
        public static CoTaskMemUnicodeSafeHandle Zero {
            get {
                return new CoTaskMemUnicodeSafeHandle();
            }
        }
    }
}
