// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SafeFindHandle 
**
**
** A wrapper for find handles
**
** 
===========================================================*/

using System;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32;

namespace Microsoft.Win32.SafeHandles {
    [System.Security.SecurityCritical]  // auto-generated
    internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [System.Security.SecurityCritical]  // auto-generated_required
        internal SafeFindHandle() : base(true) {}

#if MONO
        internal SafeFindHandle(IntPtr preexistingHandle) : base(true)
        {
            SetHandle (preexistingHandle);
        }
#endif

        [System.Security.SecurityCritical]
        override protected bool ReleaseHandle()
        {
#if MONO
            return System.IO.MonoIO.FindCloseFile (handle);
#else
            return Win32Native.FindClose(handle);
#endif
        }
    }
}
