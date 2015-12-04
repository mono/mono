// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeUserTokenHandle 
**
** <EMAIL>Author: David Gutierrez ([....]) </EMAIL>
**
** A wrapper for a user token handle
**
** Date:  July 8, 2002
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
    [HostProtectionAttribute(MayLeakOnAbort = true)]
    [SuppressUnmanagedCodeSecurityAttribute]
    internal sealed class SafeUserTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Note that OpenProcess returns 0 on failure.
        internal SafeUserTokenHandle() : base (true) {}

        internal SafeUserTokenHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle) {
            SetHandle(existingHandle);
        }
        
#if !FEATURE_PAL

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static bool DuplicateTokenEx(SafeHandle hToken, int access, NativeMethods.SECURITY_ATTRIBUTES tokenAttributes, int impersonationLevel, int tokenType, out SafeUserTokenHandle hNewToken);

#endif // !FEATURE_PAL

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





