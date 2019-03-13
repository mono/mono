//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Runtime.InteropServices;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;

#if (MONO || FEATURE_PAL)
    [System.Runtime.InteropServices.ComVisible(false)]    
#else
    [
    System.Runtime.InteropServices.ComVisible(false), 
    System.Security.SuppressUnmanagedCodeSecurityAttribute()
    ]
#endif
    internal sealed class SafeNativeMethods {
        
        private SafeNativeMethods() {}

#if (!MONO || !FEATURE_PAL)

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

#else
        internal static int GetCurrentProcessId() {
                return Process.GetCurrentProcess().Id;
        }

        internal static int GetCurrentThreadId() {
                return Thread.CurrentThread.ManagedThreadId;
        }

        internal static bool QueryPerformanceCounter(ref long lpPerformanceCount) {
                lpPerformanceCount = -1;
                return true;
        }

        internal static bool QueryPerformanceFrequency(ref long lpFrequency) {
                lpFrequency = -1;
                return true;
        }        
#endif
    }
}
