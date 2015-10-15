// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeProcessHandle 
**
** A wrapper for a process handle
**
** 
===========================================================*/

using System;
using System.Security;
using System.Diagnostics;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;

namespace Microsoft.Win32.SafeHandles {
    [SuppressUnmanagedCodeSecurityAttribute]
    public sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    { 
        internal static SafeProcessHandle InvalidHandle = new SafeProcessHandle(IntPtr.Zero); 
    
        // Note that OpenProcess returns 0 on failure

        internal SafeProcessHandle() : base(true) {}

        internal SafeProcessHandle(IntPtr handle) : base(true) {
            SetHandle(handle);
        }
        
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public SafeProcessHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle) {
            SetHandle(existingHandle);
        }

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);

        
        internal void InitialSetHandle(IntPtr h){
            Debug.Assert(base.IsInvalid, "Safe handle should only be set once");
            base.handle = h;
        }
        
        override protected bool ReleaseHandle()
        {
            return SafeNativeMethods.CloseHandle(handle);
        }

    }
}




