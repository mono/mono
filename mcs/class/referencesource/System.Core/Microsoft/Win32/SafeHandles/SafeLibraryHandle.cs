// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: SafeLibraryHandle
**
============================================================*/
namespace Microsoft.Win32 {
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [HostProtectionAttribute(MayLeakOnAbort = true)]
    sealed internal class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid {
        internal SafeLibraryHandle() : base(true) {}
        
        override protected bool ReleaseHandle() {
            return UnsafeNativeMethods.FreeLibrary(handle);
        }
    }
}
