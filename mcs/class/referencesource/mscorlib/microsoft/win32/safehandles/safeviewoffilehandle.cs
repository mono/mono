// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeViewOfFileHandle 
**
**
** A wrapper for file handles
**
** 
===========================================================*/
using System;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32.SafeHandles
{
    [System.Security.SecurityCritical]  // auto-generated
    internal sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [System.Security.SecurityCritical]  // auto-generated_required
        internal SafeViewOfFileHandle() : base(true) {}

        // 0 is an Invalid Handle
        [System.Security.SecurityCritical]  // auto-generated_required
        internal SafeViewOfFileHandle(IntPtr handle, bool ownsHandle) : base (ownsHandle) {
            SetHandle(handle);
        }

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        override protected bool ReleaseHandle()
        {
            if (Win32Native.UnmapViewOfFile(handle))
            {
                handle = IntPtr.Zero;
                return true;
            }
            
            return false;
        }
    }
}

