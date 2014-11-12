//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace Microsoft.Win32 {

    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System;
    using System.Security;
#if !SILVERLIGHT || FEATURE_NETCORE
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.ConstrainedExecution;
    using System.Diagnostics;
#endif // !SILVERLIGHT || FEATURE_NETCORE

#if !SILVERLIGHT
    [HostProtection(MayLeakOnAbort = true)]
    [SuppressUnmanagedCodeSecurity]
#endif // !SILVERLIGHT

    internal static class SafeNativeMethods {

#if FEATURE_PAL && !FEATURE_NETCORE
        [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CFStringCreateWithCharacters(
            IntPtr alloc,                // CFAllocatorRef
            [MarshalAs(UnmanagedType.LPWStr)] 
            string chars,
            int numChars);

        [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CFRelease(IntPtr cf);

        [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CFUserNotificationDisplayAlert(double timeout, uint flags, IntPtr iconUrl, IntPtr soundUrl, IntPtr localizationUrl, IntPtr alertHeader, IntPtr alertMessage, IntPtr defaultButtonTitle, IntPtr alternateButtonTitle, IntPtr otherButtonTitle, ref uint responseFlags);
#endif // SILVERLIGHT && !FEATURE_NETCORE

        public const int
            MB_RIGHT = 0x00080000,
            MB_RTLREADING = 0x00100000;

#if !FEATURE_PAL && !FEATURE_CORESYSTEM
        [DllImport(ExternDll.Gdi32, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetTextMetrics(IntPtr hDC, [In, Out] NativeMethods.TEXTMETRIC tm);
        
        [DllImport(ExternDll.Gdi32, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetStockObject(int nIndex);
#endif

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern void OutputDebugString(String message);

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Unicode, EntryPoint="MessageBoxW", ExactSpelling=true)]
        private static extern int MessageBoxSystem(IntPtr hWnd, string text, string caption, int type);
        
        [SecurityCritical]
        public static int MessageBox(IntPtr hWnd, string text, string caption, int type) {
            try {
                return MessageBoxSystem(hWnd, text, caption, type);
            } catch (DllNotFoundException) {
                return 0;
            } catch (EntryPointNotFoundException) {
                return 0;
            }
        }

#if !SILVERLIGHT || FEATURE_NETCORE
        public const int
            FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
            FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
            FORMAT_MESSAGE_FROM_STRING = 0x00000400,
            FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
            FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
			
#if FEATURE_NETCORE
        [SecurityCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=true)]            
        [SuppressMessage("Microsoft.Security", "CA2101:SpecifyMarshalingForPInvokeStringArguments")]
        [ResourceExposure(ResourceScope.None)]
        public static unsafe extern int FormatMessage(int dwFlags, IntPtr lpSource_mustBeNull, uint dwMessageId,
            int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);

#if FEATURE_NETCORE
        [SecurityCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = true)]
        [SuppressMessage("Microsoft.Security", "CA2101:SpecifyMarshalingForPInvokeStringArguments")]
        [ResourceExposure(ResourceScope.None)]
        public static unsafe extern int FormatMessage(int dwFlags, SafeLibraryHandle lpSource, uint dwMessageId,
            int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);

#if FEATURE_NETCORE
        [SecurityCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(ExternDll.Kernel32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool CloseHandle(IntPtr handle);
#endif // !SILVERLIGHT || FEATURE_NETCORE

#if !SILVERLIGHT || FEATURE_NETCORE

#if FEATURE_NETCORE
        [SecurityCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(ExternDll.Kernel32)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool QueryPerformanceCounter(out long value);

#if FEATURE_NETCORE
        [SecurityCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(ExternDll.Kernel32)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool QueryPerformanceFrequency(out long value);
#endif // !SILVERLIGHT || FEATURE_NETCORE

#if !SILVERLIGHT
#if !FEATURE_PAL
        public const int
            FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF,
            FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int RegisterWindowMessage(string msg);

#if DEBUG
        // Used only from debug code to assert we're on the right thread
        // for calling certain Windows methods.
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern int GetCurrentThreadId();

        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);
#endif

        // file src\Services\Monitoring\system\Diagnosticts\SafeNativeMethods.cs
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr LoadLibrary(string libFilename);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool FreeLibrary(HandleRef hModule);
        
        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetComputerName(StringBuilder lpBuffer, int[] nSize);                                           
        
        public static unsafe int InterlockedCompareExchange(IntPtr pDestination, int exchange, int compare)
        {
            return Interlocked.CompareExchange(ref *(int *)pDestination.ToPointer(), exchange, compare);
        }

        [DllImport(ExternDll.PerfCounter, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static unsafe extern int FormatFromRawValue(
          uint dwCounterType,
          uint dwFormat,
          ref long pTimeBase,
          NativeMethods.PDH_RAW_COUNTER pRawValue1,
          NativeMethods.PDH_RAW_COUNTER pRawValue2,
          NativeMethods.PDH_FMT_COUNTERVALUE pFmtValue
        );
#endif // !FEATURE_PAL

        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool IsWow64Process(SafeProcessHandle hProcess, ref bool Wow64Process);

        [StructLayout(LayoutKind.Sequential)]
        internal class PROCESS_INFORMATION {
            // The handles in PROCESS_INFORMATION are initialized in unmanaged functions.
            // We can't use SafeHandle here because Interop doesn't support [out] SafeHandles in structures/classes yet.            
            public IntPtr               hProcess = IntPtr.Zero;
            public IntPtr               hThread = IntPtr.Zero;
            public int                  dwProcessId = 0;
            public int                  dwThreadId = 0;

            // Note this class makes no attempt to free the handles
            // Use InitialSetHandle to copy to handles into SafeHandles

        }

        /* The following code has been removed to prevent FXCOP violations.
           The code is left here incase it needs to be resurrected.

        // From file src\services\timers\system\timers\safenativemethods.cs
        public delegate void TimerAPCProc(IntPtr argToCompletionRoutine, int timerLowValue, int timerHighValue);
        */
#endif // !SILVERLIGHT

#if !SILVERLIGHT || FEATURE_NETCORE
#if FEATURE_NETCORE
        [SecurityCritical]
#endif
        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeWaitHandle CreateSemaphore(NativeMethods.SECURITY_ATTRIBUTES lpSecurityAttributes, int initialCount, int maximumCount, String name);

#if FEATURE_NETCORE
        [SecurityCritical]
#endif
        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeWaitHandle OpenSemaphore(/* DWORD */ int desiredAccess, bool inheritHandle, String name);

#if FEATURE_NETCORE
        [SecurityCritical]
#endif
        [DllImport(ExternDll.Kernel32, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool ReleaseSemaphore(SafeWaitHandle handle, int releaseCount, out int previousCount);
#endif  //!SILVERLIGHT || FEATURE_NETCORE
    }
}
