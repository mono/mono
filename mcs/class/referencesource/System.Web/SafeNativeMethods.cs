//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Runtime.InteropServices;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;

    [
    System.Runtime.InteropServices.ComVisible(false), 
    System.Security.SuppressUnmanagedCodeSecurityAttribute()
    ]
    internal sealed class SafeNativeMethods {
        
        private SafeNativeMethods() {}

        [DllImport(ModName.KERNEL32_FULL_NAME)]
        internal /*public*/ extern static int GetCurrentProcessId();

        [DllImport(ModName.KERNEL32_FULL_NAME)]
        internal /*public*/ extern static int GetCurrentThreadId();

        [DllImport(ModName.KERNEL32_FULL_NAME)]
        internal static extern bool QueryPerformanceCounter( [System.Runtime.InteropServices.Out, In] ref long lpPerformanceCount);

        [DllImport(ModName.KERNEL32_FULL_NAME)]
        internal static extern bool QueryPerformanceFrequency( [System.Runtime.InteropServices.Out, In] ref long lpFrequency);                     

// required for HttpDebugHandlerTimeLog
#if PERF
        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void OutputDebugString(String message);
#endif
    }
}
