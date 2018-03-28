// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeMemoryMappedFileHandle
**
** Purpose: Safe handle wrapping a file mapping object handle
**
** Date:  Febuary 7, 2007
**
===========================================================*/

using System;
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
    public sealed class SafeMemoryMappedFileHandle : SafeHandleZeroOrMinusOneIsInvalid {

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeMemoryMappedFileHandle() : base(true) { }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeMemoryMappedFileHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) {
            SetHandle(handle);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        override protected bool ReleaseHandle() {
            return UnsafeNativeMethods.CloseHandle(handle);
        }
    }
}
