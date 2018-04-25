// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeMemoryMappedViewHandle
**
** Purpose: Safe handle wrapping a MMF view pointer
**
** Date:  February 7, 2007
**
===========================================================*/

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;


namespace Microsoft.Win32.SafeHandles {

    // Reliability notes:
    // ReleaseHandle has reliability guarantee of Cer.Success, as defined by SafeHandle.
    // It gets prepared as a CER at instance construction time. This safe handle doesn't
    // need to override IsInvalid because the one it inherits from 
    // SafeHandleZeroOrMinusOneIsInvalid is correct.

#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    public sealed class SafeMemoryMappedViewHandle : SafeBuffer {

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeMemoryMappedViewHandle() : base(true) { }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeMemoryMappedViewHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) {
            base.SetHandle(handle);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        override protected bool ReleaseHandle() {
            if (UnsafeNativeMethods.UnmapViewOfFile(handle)) {
                handle = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }

}
