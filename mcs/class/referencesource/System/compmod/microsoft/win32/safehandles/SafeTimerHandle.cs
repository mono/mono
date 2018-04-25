// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeTimerHandle
**
** <EMAIL>Author: David Gutierrez (Microsoft) </EMAIL>
**
** A wrapper for a timer handle
**
** Date:  July 23, 2002
** 
===========================================================*/

using System;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;

namespace Microsoft.Win32.SafeHandles {
    [HostProtectionAttribute(MayLeakOnAbort=true)]
    [SuppressUnmanagedCodeSecurityAttribute]
    internal sealed class SafeTimerHandle : SafeHandleZeroOrMinusOneIsInvalid
    { 
        // Note that CreateWaitableTimer returns 0 on failure
        
        internal SafeTimerHandle() : base (true) {}
        
        // Not currently used
        //[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        //internal SafeTimerHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle) {
        //    SetHandle(existingHandle);
        //}
        
        [DllImport(ExternDll.Kernel32, ExactSpelling=true, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool CloseHandle(IntPtr handle);

        override protected bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
}






