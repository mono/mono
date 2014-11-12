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


    // Special case flags for Mutexes enables workaround for known OS bug at 
        // http://support.microsoft.com/default.aspx?scid=kb;en-us;889318        
        // One machine-wide mutex serializes all OpenMutex and CloseHandle operations.

        // bIsMutex: if true, we need to grab machine-wide mutex before doing any Close ops.
        // Initialized to false by the runtime.
        private bool bIsMutex;

        // bIsMutex: if true, we need to avoid grabbing the machine-wide mutex before Close ops, 
        // since that mutex is, of course, this very handle.
        // Initialized to false by the runtime.
        private bool bIsReservedMutex;

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
#if !FEATURE_CORECLR
            if (!bIsMutex || Environment.HasShutdownStarted)
                return Win32Native.CloseHandle(handle);                
            
            bool bReturn = false;                
            bool bMutexObtained = false;                
            try
            {
               if (!bIsReservedMutex)
               {
                   Mutex.AcquireReservedMutex(ref bMutexObtained);    
               }
               bReturn = Win32Native.CloseHandle(handle);
            }
            finally
            {
                if (bMutexObtained)
                    Mutex.ReleaseReservedMutex();
            }
            return bReturn;
#else
            return Win32Native.CloseHandle(handle);
#endif
        }

        internal void SetAsMutex()
        {
            bIsMutex = true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]        
        internal void SetAsReservedMutex()
        {
            bIsReservedMutex = true;
        }    
    }
}
