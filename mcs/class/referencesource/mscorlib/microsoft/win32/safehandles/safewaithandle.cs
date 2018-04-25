// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeWaitHandle
**
**
** A wrapper for Win32 events (mutexes, auto reset events, and
** manual reset events).  Used by WaitHandle.
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
using System.Threading;

namespace Microsoft.Win32.SafeHandles {
 
    [System.Security.SecurityCritical]  // auto-generated_required
    public sealed class SafeWaitHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Called by P/Invoke marshaler
        private SafeWaitHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public SafeWaitHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        override protected bool ReleaseHandle()
        {
#if MONO
            NativeEventCalls.CloseEvent_internal (handle);
            return true;
#else
            return Win32Native.CloseHandle(handle);
#endif
        }
    }
}
