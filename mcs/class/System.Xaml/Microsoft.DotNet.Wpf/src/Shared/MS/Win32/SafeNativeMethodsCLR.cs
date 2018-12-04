//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace MS.Win32
{
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.ComponentModel;


// The SecurityHelper class differs between assemblies and could not actually be
//  shared, so it is duplicated across namespaces to prevent name collision.
#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use a class (duplicated across multiple namespaces) from an unknown assembly.
#endif

    using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

    ///<SecurityNote>
    /// Critical - This entire class is critical as it has SuppressUnmanagedCodeSecurity.
    /// TreatAsSafe - These Native methods have been reviewed as safe to call.
    ///</SecurityNote>
    internal static partial class SafeNativeMethods
    {

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static int GetMessagePos()
        {
            return SafeNativeMethodsPrivate.GetMessagePos();
        }

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static  IntPtr GetKeyboardLayout(int dwLayout)
        {
            return SafeNativeMethodsPrivate.GetKeyboardLayout(dwLayout);
        }

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static  IntPtr ActivateKeyboardLayout(HandleRef hkl, int uFlags)
        {
            return SafeNativeMethodsPrivate.ActivateKeyboardLayout(hkl, uFlags);
        }

#if BASE_NATIVEMETHODS
        /// <SecurityNote>
        /// Critical - access unmanaged code via SetLastError() and IntGetKeyboardLayoutList().
        /// TreatAsSafe - no returns from SetLastError().  Calling IntGetKeyboardLayoutList() is safe.
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static int GetKeyboardLayoutList(int size, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] hkls)
        {
            int result = NativeMethodsSetLastError.GetKeyboardLayoutList(size, hkls);
            if(result == 0)
            {
                int win32Err = Marshal.GetLastWin32Error();
                if (win32Err != 0)
                {
                    throw new Win32Exception(win32Err);
                }
            }

            return result;
        }
#endif


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static void GetMonitorInfo(HandleRef hmonitor, [In, Out]NativeMethods.MONITORINFOEX info)
        {
            if (SafeNativeMethodsPrivate.IntGetMonitorInfo(hmonitor, info) == false)
            {
                throw new Win32Exception();
            }
        }


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static  IntPtr MonitorFromPoint(NativeMethods.POINTSTRUCT pt, int flags)
        {
            return SafeNativeMethodsPrivate.MonitorFromPoint(pt,flags);
        }


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static  IntPtr MonitorFromRect(ref NativeMethods.RECT rect, int flags)
        {
            return  SafeNativeMethodsPrivate.MonitorFromRect(ref rect,flags);
        }


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static  IntPtr MonitorFromWindow(HandleRef handle, int flags)
       {
        return SafeNativeMethodsPrivate.MonitorFromWindow(handle, flags);
        }

#if BASE_NATIVEMETHODS

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static NativeMethods.CursorHandle LoadCursor(HandleRef hInst, IntPtr iconId)
        {
            NativeMethods.CursorHandle cursorHandle = SafeNativeMethodsPrivate.LoadCursor(hInst, iconId);
            if(cursorHandle == null || cursorHandle.IsInvalid)
            {
                throw new Win32Exception();
            }

            return cursorHandle;
        }

#endif

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static IntPtr GetCursor()
        {
            return SafeNativeMethodsPrivate.GetCursor();
        }

        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Hiding cursor is ok
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static int ShowCursor(bool show)
        {
            return SafeNativeMethodsPrivate.ShowCursor(show);
        }

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static bool AdjustWindowRectEx(ref NativeMethods.RECT lpRect, int dwStyle, bool bMenu, int dwExStyle)
        {
            bool returnValue = SafeNativeMethodsPrivate.IntAdjustWindowRectEx(ref lpRect, dwStyle, bMenu, dwExStyle);
            if (returnValue == false)
            {
                throw new Win32Exception();
            }
            return returnValue;
        }


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static void GetClientRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect)
        {
            if(!SafeNativeMethodsPrivate.IntGetClientRect(hWnd, ref rect))
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static void GetWindowRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect)
        {
            if(!SafeNativeMethodsPrivate.IntGetWindowRect(hWnd, ref rect))
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static int GetDoubleClickTime()
        {
            return SafeNativeMethodsPrivate.GetDoubleClickTime();
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static bool IsWindowEnabled(HandleRef hWnd)
        {
            return SafeNativeMethodsPrivate.IsWindowEnabled(hWnd);
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static bool IsWindowVisible(HandleRef hWnd)
        {
            return SafeNativeMethodsPrivate.IsWindowVisible(hWnd);
        }

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static bool ReleaseCapture()
        {
            bool returnValue = SafeNativeMethodsPrivate.IntReleaseCapture();

            if (returnValue == false)
            {
                throw new Win32Exception();
            }
            return returnValue;
        }


#if BASE_NATIVEMETHODS
        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static bool TrackMouseEvent(NativeMethods.TRACKMOUSEEVENT tme)
        {
            bool retVal = SafeNativeMethodsPrivate.TrackMouseEvent(tme);
            int win32Err = Marshal.GetLastWin32Error(); // Dance around FxCop
            if(!retVal && win32Err != 0)
            {
                throw new System.ComponentModel.Win32Exception(win32Err);
            }
            return retVal;
        }


        // Note: this overload has no return value.  If we need an overload that
        // returns the timer ID, then we'll need to add one.
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static void SetTimer(HandleRef hWnd, int nIDEvent, int uElapse)
        {
            if(SafeNativeMethodsPrivate.SetTimer(hWnd, nIDEvent, uElapse, null) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        // Note: this returns true or false for success.  We still don't have an overload
        // that returns the timer ID.
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static bool TrySetTimer(HandleRef hWnd, int nIDEvent, int uElapse)
        {
            if(SafeNativeMethodsPrivate.TrySetTimer(hWnd, nIDEvent, uElapse, null) == IntPtr.Zero)
            {
                return false;
            }

            return true;
        }
#endif

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call as in the worst case it destroys the dispatcher timer.
        ///     it destroys a timer
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static bool KillTimer(HandleRef hwnd, int idEvent)
        {
            return (SafeNativeMethodsPrivate.KillTimer(hwnd,idEvent));
        }


#if FRAMEWORK_NATIVEMETHODS || CORE_NATIVEMETHODS || BASE_NATIVEMETHODS
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static int GetTickCount()
        {
            return SafeNativeMethodsPrivate.GetTickCount();
        }
#endif

#if BASE_NATIVEMETHODS
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: It is considered safe to play sounds.
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static int MessageBeep(int uType)
        {
            return SafeNativeMethodsPrivate.MessageBeep(uType);
        }
#endif

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static bool IsWindowUnicode(HandleRef hWnd)
        {
        return (SafeNativeMethodsPrivate.IsWindowUnicode(hWnd));
        }


#if BASE_NATIVEMETHODS
        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Setting Cursor is ok
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static IntPtr SetCursor(HandleRef hcursor)
        {
            return SafeNativeMethodsPrivate.SetCursor(hcursor);
        }

        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Setting Cursor is ok
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static IntPtr SetCursor(SafeHandle hcursor)
        {
            return SafeNativeMethodsPrivate.SetCursor(hcursor);
        }
#endif

        // not used by compiler - don't include.

        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Screen to Clien is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static void ScreenToClient(HandleRef hWnd, [In, Out] NativeMethods.POINT pt)
        {
            if(SafeNativeMethodsPrivate.IntScreenToClient(hWnd, pt) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Process Id is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static int GetCurrentProcessId()
        {
            return SafeNativeMethodsPrivate.GetCurrentProcessId();
        }


        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Thread ID is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static int GetCurrentThreadId()
        {
            return SafeNativeMethodsPrivate.GetCurrentThreadId();
        }

        /// <summary>
        /// Returns the ID of the session under which the current process is running
        /// </summary>
        /// <securitynote>
        /// safe: exposes non-critical information
        /// critical: This code eleveates to unmanaged code permission
        /// </securitynote>
        /// <returns>
        /// The session id upon success, null on failure
        /// </returns>
        [SecuritySafeCritical]
        public static int? GetCurrentSessionId()
        {
            int? result = null;

            int sessionId;
            if (SafeNativeMethodsPrivate.ProcessIdToSessionId(
                GetCurrentProcessId(), out sessionId))
            {
                result = sessionId;
            }

            return result;
        }

        /// <SecurityNote>
        /// This will return a valid handle only if a window on the current thread has capture
        /// else it will return NULL. (Refer to Platform SDK)
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Getting mouse capture is ok
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static IntPtr GetCapture()
        {
            return SafeNativeMethodsPrivate.GetCapture();

        }
#if BASE_NATIVEMETHODS
        /// <SecurityNote>
        /// This function cannot be used to capture mouse input for another process.
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Setting Capture is ok
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static IntPtr SetCapture(HandleRef hwnd)
        {
            return SafeNativeMethodsPrivate.SetCapture(hwnd);
        }

        /// <SecurityNote>
        /// This can be guessed anyways and does not relay any risky information
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: Getting virtual key mapping is ok
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static int MapVirtualKey(int nVirtKey, int nMapType)
        {
            return SafeNativeMethodsPrivate.MapVirtualKey(nVirtKey,nMapType);
        }
#endif

        /// <summary>
        /// Identifies whether the given workstation session ID has a WTSConnectState value
        /// of WTSActive, or not.
        /// </summary>
        /// <param name="SessionId">
        /// The ID of the workstation session to query. If this is null,
        /// then this will default to WTS_CURRENT_SESSION. Note that the ID of the 
        /// current session will not be queried explicitly. 
        /// </param>
        /// <param name="defaultResult">
        /// The default result to return if this method is unable to identify the connection 
        /// state of the given session ID.
        /// </param>
        /// <returns>
        /// True if the connection state for <paramref name="SessionId"/> is WTSActive; 
        /// false otherwise
        /// <paramref name="defaultResult"/> is returned if WTSQuerySessionInformation 
        /// fails.
        /// </returns>
        /// <securitynote>
        /// critical: This method elevates to unmanaged-code permission
        /// safe: Returns safe information
        /// </securitynote>
        [SecuritySafeCritical]
        public static bool IsCurrentSessionConnectStateWTSActive(int? SessionId = null, bool defaultResult = true)
        {
            IntPtr buffer = IntPtr.Zero;
            int bytesReturned;

            int sessionId = SessionId.HasValue ? SessionId.Value : NativeMethods.WTS_CURRENT_SESSION;
            bool currentSessionConnectState = defaultResult;

            try
            {
                if (SafeNativeMethodsPrivate.WTSQuerySessionInformation(
                    NativeMethods.WTS_CURRENT_SERVER_HANDLE, 
                    sessionId, 
                    NativeMethods.WTS_INFO_CLASS.WTSConnectState, 
                    out buffer, out bytesReturned) && (bytesReturned >= sizeof(int)))
                {
                    var data = Marshal.ReadInt32(buffer);
                    if (Enum.IsDefined(typeof(NativeMethods.WTS_CONNECTSTATE_CLASS), data))
                    {
                        var connectState = (NativeMethods.WTS_CONNECTSTATE_CLASS)data;
                        currentSessionConnectState = (connectState == NativeMethods.WTS_CONNECTSTATE_CLASS.WTSActive);
                    }
                }
            }
            finally
            {
                try
                {
                    if (buffer != IntPtr.Zero)
                    {
                        SafeNativeMethodsPrivate.WTSFreeMemory(buffer);
                    }
                }
                catch (Exception e) when (e is Win32Exception || e is SEHException)
                {
                    // We will do nothing and return defaultResult
                    //
                    // Note that we don't want to catch and ignore SystemException types
                    // like AV, OOM etc. 
                }
            }

            return currentSessionConnectState;
        }

        [SuppressUnmanagedCodeSecurity,SecurityCritical(SecurityCriticalScope.Everything)]
        private partial class SafeNativeMethodsPrivate
        {

            [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int GetCurrentProcessId();

            [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto)]
            [return:MarshalAs(UnmanagedType.Bool)]
            public static extern bool ProcessIdToSessionId([In]int dwProcessId, [Out]out int pSessionId);

            [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int GetCurrentThreadId();

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern IntPtr GetCapture();

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern bool IsWindowVisible(HandleRef hWnd);

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int GetMessagePos();

            [DllImport(ExternDll.User32, EntryPoint = "ReleaseCapture", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
            public static extern bool IntReleaseCapture();

            [DllImport(ExternDll.User32, EntryPoint = "GetWindowRect", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool IntGetWindowRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect);

            [DllImport(ExternDll.User32, EntryPoint = "GetClientRect", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool IntGetClientRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect);

            [DllImport(ExternDll.User32, EntryPoint = "AdjustWindowRectEx", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
            public static extern bool IntAdjustWindowRectEx(ref NativeMethods.RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

            [DllImport(ExternDll.User32, ExactSpelling=true)]
            public static extern IntPtr MonitorFromRect(ref NativeMethods.RECT rect, int flags);

            [DllImport(ExternDll.User32, ExactSpelling = true)]
            public static extern IntPtr MonitorFromPoint(NativeMethods.POINTSTRUCT pt, int flags);

            [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
            public static extern IntPtr ActivateKeyboardLayout(HandleRef hkl, int uFlags);

            [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
            public static extern IntPtr GetKeyboardLayout(int dwLayout);

            [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
            public static extern IntPtr SetTimer(HandleRef hWnd, int nIDEvent, int uElapse, NativeMethods.TimerProc lpTimerFunc);

            [DllImport(ExternDll.User32, EntryPoint="SetTimer", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern IntPtr TrySetTimer(HandleRef hWnd, int nIDEvent, int uElapse, NativeMethods.TimerProc lpTimerFunc);

            [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
            public static extern bool KillTimer(HandleRef hwnd, int idEvent);

            [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
            public static extern bool IsWindowUnicode(HandleRef hWnd);

            [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
            public static extern int GetDoubleClickTime();

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern bool IsWindowEnabled(HandleRef hWnd);

            [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
            public static extern IntPtr GetCursor();

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int ShowCursor(bool show);

            [DllImport(ExternDll.User32, EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool IntGetMonitorInfo(HandleRef hmonitor, [In, Out]NativeMethods.MONITORINFOEX info);

            [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);

#if BASE_NATIVEMETHODS
            [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
            internal static extern int MapVirtualKey(int nVirtKey, int nMapType);

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SetCapture(HandleRef hwnd);

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SetCursor(HandleRef hcursor);

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SetCursor(SafeHandle hcursor);

            [DllImport(ExternDll.User32, ExactSpelling=true, SetLastError=true)]
            public static extern bool TrackMouseEvent(NativeMethods.TRACKMOUSEEVENT tme);

            [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
            public static extern NativeMethods.CursorHandle LoadCursor(HandleRef hInst, IntPtr iconId);

#endif

#if BASE_NATIVEMETHODS || CORE_NATIVEMETHODS || FRAMEWORK_NATIVEMETHODS
            [DllImport(ExternDll.Kernel32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int GetTickCount();

#endif

            [DllImport(ExternDll.User32, EntryPoint="ScreenToClient", SetLastError=true, ExactSpelling=true, CharSet=CharSet.Auto)]
            public static extern int IntScreenToClient(HandleRef hWnd, [In, Out] NativeMethods.POINT pt);

#if BASE_NATIVEMETHODS
            [DllImport(ExternDll.User32)]
            public static extern int MessageBeep(int uType);
#endif
            [DllImport(ExternDll.WtsApi32, SetLastError = true, EntryPoint = "WTSQuerySessionInformation", CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WTSQuerySessionInformation(
                [In]IntPtr hServer, 
                [In] int SessionId, 
                [In]NativeMethods.WTS_INFO_CLASS WTSInfoClass, 
                [Out]out IntPtr ppBuffer, [Out]out int BytesReturned);

            [DllImport(ExternDll.WtsApi32, EntryPoint = "WTSFreeMemory", CharSet = CharSet.Auto)]
            public static extern bool WTSFreeMemory([In]IntPtr pMemory);
        }
    }
}

