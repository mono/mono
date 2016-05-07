// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: SafePipeHandle
**
============================================================*/
namespace Microsoft.Win32.SafeHandles {

using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class SafePipeHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafePipeHandle()
            : base(true) { }

        public SafePipeHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(ownsHandle) {
            SetHandle(preexistingHandle);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        protected override bool ReleaseHandle() {
            return UnsafeNativeMethods.CloseHandle(handle);
        }
    }

}
