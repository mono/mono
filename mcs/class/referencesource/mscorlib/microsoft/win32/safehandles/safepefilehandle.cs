// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.SafeHandles
{
    /// <summary>
    ///     Handle to a VM PEFile *
    /// </summary>
    [SecurityCritical] 
    internal sealed class SafePEFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafePEFileHandle() : base(true)
        {
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void ReleaseSafePEFileHandle(IntPtr peFile);

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            ReleaseSafePEFileHandle(handle);
            return true;
        }
    }
}
