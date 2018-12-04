//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace MS.Win32
{
    using Accessibility;
    using SRCS = System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Security;
    using Microsoft.Win32.SafeHandles;
    using MS.Internal;
    using MS.Internal.Interop;
    using MS.Utility;

#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use FriendAccessAllowedAttribute from an unknown assembly.
    using MS.Internal.YourAssemblyName;
#endif

    using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

    //[SuppressUnmanagedCodeSecurity()]
    [FriendAccessAllowed]
    internal partial class UnsafeNativeMethods
    {
        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Unicode, SetLastError=true, EntryPoint="GetTempFileName")]
        internal static extern uint _GetTempFileName(string tmpPath, string prefix, uint uniqueIdOrZero, StringBuilder tmpFileName);

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        internal static uint GetTempFileName(string tmpPath, string prefix, uint uniqueIdOrZero, StringBuilder tmpFileName)
        {
            uint result = _GetTempFileName(tmpPath, prefix, uniqueIdOrZero, tmpFileName);
            if (result == 0)
            {
                throw new Win32Exception();
            }

            return result;
        }

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Shell32, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int ExtractIconEx(
                                        string szExeFileName,
                                        int nIconIndex,
                                        out NativeMethods.IconHandle phiconLarge,
                                        out NativeMethods.IconHandle phiconSmall,
                                        int nIcons);

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        internal static extern NativeMethods.IconHandle CreateIcon(IntPtr hInstance, int nWidth, int nHeight, byte cPlanes, byte cBitsPixel, byte[] lpbANDbits, byte[] lpbXORbits);

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern bool CreateCaret(HandleRef hwnd, NativeMethods.BitmapHandle hbitmap, int width, int height);

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern bool ShowCaret(HandleRef hwnd);

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern bool HideCaret(HandleRef hwnd);

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);

        [DllImport(ExternDll.User32, EntryPoint="LoadImage", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern NativeMethods.IconHandle LoadImageIcon(IntPtr hinst, string stName, int nType, int cxDesired, int cyDesired, int nFlags);

        [DllImport(ExternDll.User32, EntryPoint="LoadImage", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern NativeMethods.CursorHandle LoadImageCursor(IntPtr hinst, string stName, int nType, int cxDesired, int cyDesired, int nFlags);
        // uncomment this if you plan to use LoadImage to load anything other than Icons/Cursors.
        /*
                [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
                internal static extern SafeHandle LoadImage(
                    IntPtr hinst, string stName, int nType, int cxDesired, int cyDesired, int nFlags);
        */
        /*
                [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
                internal static extern NativeMethods.IconHandle LoadImage(
                    IntPtr hinst, string stName, int nType, int cxDesired, int cyDesired, int nFlags);
        */

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///
        ///     Could be a candidate for safe - as the only information disclosed is whether
        ///     a certain security measure is on or off.
        ///     Likely this determination could be made by trying certain actions and failing.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity ]
        [DllImport( ExternDll.Urlmon, ExactSpelling=true)]
        internal static extern int CoInternetIsFeatureEnabled( int featureEntry , int dwFlags );

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity ]
        [DllImport( ExternDll.Urlmon, ExactSpelling=true)]
        internal static extern int CoInternetSetFeatureEnabled( int featureEntry , int dwFlags, bool fEnable );

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///
        ///     Could be a candidate for safe - as the only information disclosed is whether
        ///     a certain security measure is on or off.
        ///     Likely this determination could be made by trying certain actions and failing.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity ]
        [DllImport( ExternDll.Urlmon, ExactSpelling=true)]
        internal static extern int CoInternetIsFeatureZoneElevationEnabled(
                                                            [MarshalAs(UnmanagedType.LPWStr)] string szFromURL,
                                                            [MarshalAs(UnmanagedType.LPWStr)] string szToURL,
                                                            UnsafeNativeMethods.IInternetSecurityManager secMgr,
                                                            int dwFlags
                                                            );


        ///<SecurityNote>
        ///     Critical - call is SUC'ed
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.PresentationHostDll, EntryPoint = "ProcessUnhandledException")]
        internal static extern void ProcessUnhandledException_DLL([MarshalAs(UnmanagedType.BStr)] string errMsg);

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity ]
        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Unicode)]
        internal static extern bool GetVersionEx([In, Out] NativeMethods.OSVERSIONINFOEX ver);
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity ]
        [DllImport( ExternDll.Urlmon, ExactSpelling=true)]
        internal static extern int CoInternetCreateSecurityManager(
                                                                    [MarshalAs(UnmanagedType.Interface)] object pIServiceProvider,
                                                                    [MarshalAs(UnmanagedType.Interface)] out object ppISecurityManager ,
                                                                    int dwReserved ) ;




        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport, ComVisible(false), Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b"), System.Runtime.InteropServices.InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IInternetSecurityManager
        {
            void SetSecuritySite( NativeMethods.IInternetSecurityMgrSite pSite);

            unsafe void GetSecuritySite( /* [out] */ void **ppSite);

            ///<SecurityNote>
            ///     Critical - performs an elevation.
            ///</SecurityNote>
            [SecurityCritical, SuppressUnmanagedCodeSecurity]
            void MapUrlToZone(
                                [In, MarshalAs(UnmanagedType.BStr)]
                                        string pwszUrl,
                                [Out]   out int pdwZone,
                                [In]    int     dwFlags);

            unsafe void GetSecurityId(  /* [in] */ string pwszUrl,
                                /* [size_is][out] */ byte *pbSecurityId,
                                /* [out][in] */ int *pcbSecurityId,
                                /* [in] */ int dwReserved);

            unsafe void ProcessUrlAction(
                                /* [in] */ string pwszUrl,
                                /* [in] */ int dwAction,
                                /* [size_is][out] */ byte *pPolicy,
                                /* [in] */ int cbPolicy,
                                /* [in] */ byte *pContext,
                                /* [in] */ int cbContext,
                                /* [in] */ int dwFlags,
                                /* [in] */ int dwReserved);

            unsafe void QueryCustomPolicy(
                                /* [in] */ string pwszUrl,
                                /* [in] */ /*REFGUID*/ void *guidKey,
                                /* [size_is][size_is][out] */ byte **ppPolicy,
                                /* [out] */ int *pcbPolicy,
                                /* [in] */ byte *pContext,
                                /* [in] */ int cbContext,
                                /* [in] */ int dwReserved);

            unsafe void SetZoneMapping( /* [in] */ int dwZone, /* [in] */ string lpszPattern, /* [in] */ int dwFlags);

            unsafe void GetZoneMappings( /* [in] */ int dwZone, /* [out] */ /*IEnumString*/ void **ppenumString, /* [in] */ int dwFlags);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hMem"></param>
        /// <returns></returns>
        [DllImport(ExternDll.Kernel32, SetLastError = true), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern IntPtr LocalFree(IntPtr hMem);

#if BASE_NATIVEMETHODS
        /// <SecurityNote>
        ///  SecurityCritical: This code returns a critical resource obtained under an elevation.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal unsafe static extern SafeFileHandle CreateFile(
          string lpFileName,
          uint dwDesiredAccess,
          uint dwShareMode,
          [In] NativeMethods.SECURITY_ATTRIBUTES lpSecurityAttributes,
          int dwCreationDisposition,
          int dwFlagsAndAttributes,
          IntPtr hTemplateFile);
#endif


#if BASE_NATIVEMETHODS
        /// <SecurityNote>
        ///     Critical: This code is critical because it can be used to
        ///     pass and force arbitrary data into the tree. We should
        ///     consider yanking it out all the way
        /// </SecurityNote>

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        internal static extern IntPtr GetMessageExtraInfo();
#endif

#if BASE_NATIVEMETHODS
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, EntryPoint="WaitForMultipleObjectsEx", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int IntWaitForMultipleObjectsEx(int nCount, IntPtr[] pHandles, bool bWaitAll, int dwMilliseconds, bool bAlertable);

        public const int WAIT_FAILED = unchecked((int)0xFFFFFFFF);

        ///<SecurityNote>
        /// Critical - calls IntWaitForMultipleObjectsEx (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        internal static int WaitForMultipleObjectsEx(int nCount, IntPtr[] pHandles, bool bWaitAll, int dwMilliseconds, bool bAlertable)
        {
            int result = IntWaitForMultipleObjectsEx(nCount, pHandles, bWaitAll, dwMilliseconds, bAlertable);
            if(result ==  UnsafeNativeMethods.WAIT_FAILED)
            {
                throw new Win32Exception();
            }

            return result;
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="MsgWaitForMultipleObjectsEx", SetLastError=true, ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern int IntMsgWaitForMultipleObjectsEx(int nCount, IntPtr[] pHandles, int dwMilliseconds, int dwWakeMask, int dwFlags);

        ///<SecurityNote>
        /// Critical - calls IntMsgWaitForMultipleObjectsEx (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        internal static int MsgWaitForMultipleObjectsEx(int nCount, IntPtr[] pHandles, int dwMilliseconds, int dwWakeMask, int dwFlags)
        {
            int result = IntMsgWaitForMultipleObjectsEx(nCount, pHandles, dwMilliseconds, dwWakeMask, dwFlags);
            if(result == -1)
            {
                throw new Win32Exception();
            }

            return result;
        }
#endif

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="RegisterClassEx", CharSet=CharSet.Unicode, SetLastError=true, BestFitMapping=false)]
        internal static extern UInt16 IntRegisterClassEx(NativeMethods.WNDCLASSEX_D wc_d);

        ///<SecurityNote>
        /// Critical - calls IntRegisterClassEx (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        internal static UInt16 RegisterClassEx(NativeMethods.WNDCLASSEX_D wc_d)
        {
            UInt16 result = IntRegisterClassEx(wc_d);
            if(result == 0)
            {
                throw new Win32Exception();
            }

            return result;
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="UnregisterClass",CharSet = CharSet.Auto, SetLastError = true, BestFitMapping=false)]
        internal static extern int IntUnregisterClass(IntPtr atomString /*lpClassName*/ , IntPtr hInstance);

        ///<SecurityNote>
        /// Critical - calls IntUnregisterClass (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        internal static void UnregisterClass(IntPtr atomString /*lpClassName*/ , IntPtr hInstance)
        {
            int result = IntUnregisterClass(atomString, hInstance);
            if (result == 0)
            {
                throw new Win32Exception();
            }
        }

#if !DRT

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", EntryPoint="ChangeWindowMessageFilter", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IntChangeWindowMessageFilter(WindowMessage message, MSGFLT dwFlag);

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", EntryPoint = "ChangeWindowMessageFilterEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IntChangeWindowMessageFilterEx(IntPtr hwnd, WindowMessage message, MSGFLT action, [In, Out, Optional] ref CHANGEFILTERSTRUCT pChangeFilterStruct);

        // Note that processes at or below SECURITY_MANDATORY_LOW_RID are not allowed to change the message filter.
        // If those processes call this function, it will fail and generate the extended error code, ERROR_ACCESS_DENIED.
        ///<SecurityNote>
        ///     Critical - calls SecurityCritical methods IntChangeWindowMessageFilter and IntChangeWindowMessageFilterEx.
        ///</SecurityNote>
        [SecurityCritical]
        internal static MS.Internal.Interop.HRESULT ChangeWindowMessageFilterEx(IntPtr hwnd, WindowMessage message, MSGFLT action, out MSGFLTINFO extStatus)
        {
            extStatus = MSGFLTINFO.NONE;

            // This API were added for Vista.  The Ex version was added for Windows 7.
            // If we're not on either, then this message filter isolation doesn't exist.
            if (!Utilities.IsOSVistaOrNewer)
            {
                return MS.Internal.Interop.HRESULT.S_FALSE;
            }

            // If we're on Vista rather than Win7 then we can't use the Ex version of this function.
            // The Ex version is preferred if possible because this results in process-wide modifications of the filter
            // and is deprecated as of Win7.
            if (!Utilities.IsOSWindows7OrNewer)
            {
                // Note that the Win7 MSGFLT_ALLOW/DISALLOW enum values map to the Vista MSGFLT_ADD/REMOVE
                if (!IntChangeWindowMessageFilter(message, action))
                {
                    return (MS.Internal.Interop.HRESULT)Win32Error.GetLastError();
                }
                return MS.Internal.Interop.HRESULT.S_OK;
            }

            var filterstruct = new CHANGEFILTERSTRUCT { cbSize = (uint)Marshal.SizeOf(typeof(CHANGEFILTERSTRUCT)) };
            if (!IntChangeWindowMessageFilterEx(hwnd, message, action, ref filterstruct))
            {
                return (MS.Internal.Interop.HRESULT)Win32Error.GetLastError();
            }

            extStatus = filterstruct.ExtStatus;
            return MS.Internal.Interop.HRESULT.S_OK;
        }

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Urlmon, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern MS.Internal.Interop.HRESULT ObtainUserAgentString(int dwOption, StringBuilder userAgent, ref int length);

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        internal static string ObtainUserAgentString()
        {
            int length = MS.Win32.NativeMethods.MAX_PATH;
            StringBuilder userAgentBuffer = new StringBuilder(length);
            MS.Internal.Interop.HRESULT hr = ObtainUserAgentString(0 /*reserved. must be 0*/, userAgentBuffer, ref length);

            // Dev10 821573 - Installing .NET 4.0 adds two parts to the user agent string, i.e.
            // .NET4.0C and .NET4.0E, potentially causing the user agent string to overflow its
            // documented maximum length of MAX_PATH. Turns out ObtainUserAgentString can return
            // a longer string if asked to do so. Therefore we grow the string dynamically when
            // needed, accommodating for this failure condition.
            if (hr == MS.Internal.Interop.HRESULT.E_OUTOFMEMORY)
            {
                userAgentBuffer = new StringBuilder(length);
                hr = ObtainUserAgentString(0 /*reserved. must be 0*/, userAgentBuffer, ref length);
            }

            hr.ThrowIfFailed();

            return userAgentBuffer.ToString();
        }

        // note that this method exists in UnsafeNativeMethodsCLR.cs but with a different signature
        // using a HandleRef for the hWnd instead of an IntPtr, and not using an IntPtr for lParam
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);


        // note that this method exists in UnsafeNativeMethodsCLR.cs but with a different signature
        // using a HandleRef for the hWnd instead of an IntPtr, and not using an IntPtr for lParam
        /// <SecurityNote>
        ///     Critical: This code has the ability to send a message to the wndproc. It exists purely for
        ///     the secure close scenario. For any other scenario please use the SendMessage call
        /// </SecurityNote>
        [DllImport(ExternDll.User32,EntryPoint="SendMessage", CharSet = CharSet.Auto)]
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr UnsafeSendMessage(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

        /// <SecurityNote>
        ///     Critical: Registering for system broadcast messages
        /// </SecurityNote>
        [DllImport(ExternDll.User32,EntryPoint="RegisterPowerSettingNotification")]
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        unsafe internal static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, Guid *pGuid, int Flags);

        /// <SecurityNote>
        ///     Critical: Unregistering for system broadcast messages
        /// </SecurityNote>
        [DllImport(ExternDll.User32,EntryPoint="UnregisterPowerSettingNotification")]
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        unsafe internal static extern IntPtr UnregisterPowerSettingNotification(IntPtr hPowerNotify);

/*
        //
        // SendMessage taking a SafeHandle for wParam. Needed by some Win32 messages. e.g. WM_PRINT
        //
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(HandleRef hWnd, WindowMessage msg, SafeHandle wParam, IntPtr lParam);
*/

        // private  DllImport - that takes an IconHandle.
        /// <SecurityNote>
        ///     Critical: This code causes elevation to unmanaged code
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SendMessage( HandleRef hWnd, WindowMessage msg, IntPtr wParam, NativeMethods.IconHandle iconHandle );
#endif

        /// <SecurityNote>
        ///     Critical: This code causes elevation to unmanaged code
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern void SetLastError(int dwErrorCode);

#if BASE_NATIVEMETHODS || CORE_NATIVEMETHODS || FRAMEWORK_NATIVEMETHODS
        /// <summary>
        /// Win32 GetLayeredWindowAttributes.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="pcrKey"></param>
        /// <param name="pbAlpha"></param>
        /// <param name="pdwFlags"></param>
        /// <returns></returns>
        /// <SecurityNote>
        /// Critical: This code calls into unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern bool GetLayeredWindowAttributes(
                HandleRef hwnd, IntPtr pcrKey, IntPtr pbAlpha, IntPtr pdwFlags);
        internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            /// <SecurityNote>
            ///   Critical: base class enforces link demand and inheritance demand
            /// </SecurityNote>
            [SecurityCritical]
            internal SafeFileMappingHandle(IntPtr handle) : base(false)
            {
                SetHandle(handle);
            }

            /// <SecurityNote>
            ///   Critical: base class enforces link demand and inheritance demand
            ///   TreatAsSafe: Creating this is ok, accessing the pointer is bad
            /// </SecurityNote>
            [SecurityCritical,SecurityTreatAsSafe]
            internal SafeFileMappingHandle() : base(true)
            {
            }

            /// <SecurityNote>
            ///   Critical: base class enforces link demand and inheritance demand
            ///   TreatAsSafe: This call is safe
            /// </SecurityNote>
            public override bool IsInvalid
            {
                [SecurityCritical,SecurityTreatAsSafe]
                get
                {
                    return handle == IntPtr.Zero;
                }
            }

            /// <SecurityNote>
            ///     Critical - as this function does an elevation to close a handle.
            ///     TreatAsSafe - as this can at best be used to destabilize one's own app.
            /// </SecurityNote>
            [SecurityCritical, SecurityTreatAsSafe]
            protected override bool ReleaseHandle()
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    return CloseHandleNoThrow(new HandleRef(null, handle));
                }
                finally
                {
                    SecurityPermission.RevertAssert();
                }
            }
        }
        internal sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            ///<SecurityNote>
            ///    Critical: This code calls into a base class which link demands for unmanaged code
            ///    TreatAsSafe:Creating this is ok it is acessing the pointers in it that can be risky
            /// </SecurityNote>
            [SecurityCritical,SecurityTreatAsSafe]
            internal SafeViewOfFileHandle() : base(true) { }

            /// <SecurityNote>
            /// Critical: This code accesses an unsafe object (pointer) and returns it as a pointer
            /// </SecurityNote>
            internal unsafe void* Memory
            {
                [SecurityCritical]
                get
                {
                    Debug.Assert(handle != IntPtr.Zero);
                    return (void*)handle;
                }
            }

            /// <SecurityNote>
            ///     Critical - as this function does an elevation to close a handle.
            ///     TreatAsSafe - as this can at best be used to destabilize one's own app.
            /// </SecurityNote>
            [SecurityCritical, SecurityTreatAsSafe]
            override protected bool ReleaseHandle()
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    return UnsafeNativeMethods.UnmapViewOfFileNoThrow(new HandleRef(null, handle));
                }
                finally
                {
                    SecurityPermission.RevertAssert();
                }
            }
        }

        /// <SecurityNote>
        ///  SecurityCritical: This code returns critical resource obtained under an elevation.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal unsafe static extern SafeFileMappingHandle CreateFileMapping(SafeFileHandle hFile, NativeMethods.SECURITY_ATTRIBUTES lpFileMappingAttributes, int flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        /// <SecurityNote>
        ///  SecurityCritical: This code returns a critical resource obtained under an elevation.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        internal static extern SafeViewOfFileHandle MapViewOfFileEx(SafeFileMappingHandle hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, IntPtr dwNumberOfBytesToMap, IntPtr lpBaseAddress);
#endif // BASE_NATIVEMETHODS


        ///<SecurityNote>
        /// Critical: LinkDemand on Marshal.GetLastWin32Error
        /// TreatAsSafe: Getting an error code isn't unsafe
        /// Note: If a SupressUnmanagedCodeSecurity attribute is ever added to IntsetWindowLong(Ptr), we'd need to be Critical
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            IntPtr result = IntPtr.Zero;

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = NativeMethodsSetLastError.SetWindowLong(hWnd, nIndex, NativeMethods.IntPtrToInt32(dwNewLong));
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = NativeMethodsSetLastError.SetWindowLongPtr(hWnd, nIndex, dwNewLong);
            }

            return result;
        }

        /// <SecurityNote>
        /// Critical - it calls IntCriticalSetWindowLongPtr() / IntCriticalSetWindowLong(), which are Critical
        /// </SecurityNote>
        [SecurityCritical]
        internal static IntPtr CriticalSetWindowLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            IntPtr result = IntPtr.Zero;

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = NativeMethodsSetLastError.SetWindowLong(hWnd, nIndex, NativeMethods.IntPtrToInt32(dwNewLong));
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = NativeMethodsSetLastError.SetWindowLongPtr(hWnd, nIndex, dwNewLong);
            }

            return result;
        }

        /// <SecurityNote>
        /// Critical - This calls SetLatError() and IntCriticalSetWindowLongPtr() / IntCriticalSetWindowLong(), which are Critical
        /// </SecurityNote>
        [SecurityCritical]
        internal static IntPtr CriticalSetWindowLong(HandleRef hWnd, int nIndex, NativeMethods.WndProc dwNewLong)
        {
            int errorCode;
            IntPtr retVal;

            if (IntPtr.Size == 4)
            {
                Int32 tempRetVal = NativeMethodsSetLastError.SetWindowLongWndProc(hWnd, nIndex, dwNewLong);
                errorCode = Marshal.GetLastWin32Error();
                retVal = new IntPtr(tempRetVal);
            }
            else
            {
                retVal = NativeMethodsSetLastError.SetWindowLongPtrWndProc(hWnd, nIndex, dwNewLong);
                errorCode = Marshal.GetLastWin32Error();
            }

            if (retVal == IntPtr.Zero)
            {
                if (errorCode != 0)
                {
                    throw new System.ComponentModel.Win32Exception(errorCode);
                }
            }

            return retVal;
        }

        /// <SecurityNote>
        ///  SecurityCritical: This code happens to return a critical resource and causes unmanaged code elevation
        /// </SecurityNote>
        [SecurityCritical]
        internal static IntPtr GetWindowLongPtr(HandleRef hWnd, int nIndex )
        {
            IntPtr result = IntPtr.Zero;
            int error = 0;

            if (IntPtr.Size == 4)
            {
                // use getWindowLong
                Int32 tempResult = NativeMethodsSetLastError.GetWindowLong(hWnd, nIndex);
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use GetWindowLongPtr
                result = NativeMethodsSetLastError.GetWindowLongPtr(hWnd, nIndex);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                // To be consistent with out other PInvoke wrappers
                // we should "throw" here.  But we don't want to
                // introduce new "throws" w/o time to follow up on any
                // new problems that causes.
                Debug.WriteLine("GetWindowLongPtr failed.  Error = " + error);
                // throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        /// <SecurityNote>
        ///  SecurityCritical: This code happens to return a critical resource and causes unmanaged code elevation
        /// </SecurityNote>
        [SecurityCritical]
        internal static Int32 GetWindowLong(HandleRef hWnd, int nIndex )
        {
            int iResult = 0;
            IntPtr result = IntPtr.Zero;
            int error = 0;

            if (IntPtr.Size == 4)
            {
                // use GetWindowLong
                iResult = NativeMethodsSetLastError.GetWindowLong(hWnd, nIndex);
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(iResult);
            }
            else
            {
                // use GetWindowLongPtr
                result = NativeMethodsSetLastError.GetWindowLongPtr(hWnd, nIndex);
                error = Marshal.GetLastWin32Error();
                iResult = NativeMethods.IntPtrToInt32(result);
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                // To be consistent with out other PInvoke wrappers
                // we should "throw" here.  But we don't want to
                // introduce new "throws" w/o time to follow up on any
                // new problems that causes.
                Debug.WriteLine("GetWindowLong failed.  Error = " + error);
                // throw new System.ComponentModel.Win32Exception(error);
            }

            return iResult;
        }

        ///<SecurityNote>
        /// Critical: Call critical method IntGetWindowLongWndProc and IntGetWindowLongWndProcPtr that causes unmanaged code elevation.
        /// LinkDemand on Win32Exception constructor but throwing an exception isn't unsafe
        ///</SecurityNote>
        [SecurityCritical]
        internal static NativeMethods.WndProc GetWindowLongWndProc(HandleRef hWnd)
        {
            NativeMethods.WndProc returnValue = null;
            int error = 0;

            if (IntPtr.Size == 4)
            {
                // use getWindowLong
                returnValue = NativeMethodsSetLastError.GetWindowLongWndProc(hWnd, NativeMethods.GWL_WNDPROC);
                error = Marshal.GetLastWin32Error();
            }
            else
            {
                // use GetWindowLongPtr
                returnValue = NativeMethodsSetLastError.GetWindowLongPtrWndProc(hWnd, NativeMethods.GWL_WNDPROC);
                error = Marshal.GetLastWin32Error();
            }

            if (null == returnValue)
            {
                throw new Win32Exception(error);
            }

            return returnValue;
        }

        ///<SecurityNote>
        /// Critical - Unmanaged code permission is supressed.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        internal static extern bool PlaySound([In]string soundName, IntPtr hmod, SafeNativeMethods.PlaySoundFlags soundFlags);

        internal const uint
            INTERNET_COOKIE_THIRD_PARTY     = 0x10,
            INTERNET_COOKIE_EVALUATE_P3P    = 0x40,
            INTERNET_COOKIE_IS_RESTRICTED   = 0x200,
            COOKIE_STATE_REJECT             = 5;

        //!!! CAUTION
        // PresentationHost intercepts calls to InternetGetCookieEx & InternetSetCookieEx and delegates them
        // to the browser. It doesn't do this for InternetGetCookie & InternetSetCookie.
        // See also Application.Get/SetCookie().
        //!!!

        /// <SecurityNote>
        /// SecurityCritical - calls unmanaged code.
        /// </SecurityNote>
        [DllImport(ExternDll.Wininet, SetLastError=true, ExactSpelling=true, EntryPoint="InternetGetCookieExW", CharSet=CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        internal static extern bool InternetGetCookieEx([In]string Url, [In]string cookieName,
            [Out] StringBuilder cookieData, [In, Out] ref UInt32 pchCookieData, uint flags, IntPtr reserved);

        /// <SecurityNote>
        /// SecurityCritical - calls unmanaged code.
        /// </SecurityNote>
        [DllImport(ExternDll.Wininet, SetLastError = true, ExactSpelling = true, EntryPoint = "InternetSetCookieExW", CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        internal static extern uint InternetSetCookieEx([In]string Url, [In]string CookieName, [In]string cookieData, uint flags, [In] string p3pHeader);

#if DRT_NATIVEMETHODS

        [DllImport(ExternDll.User32, ExactSpelling = true, EntryPoint = "mouse_event", CharSet = CharSet.Auto)]
        internal static extern void Mouse_event(int flags, int dx, int dy, int dwData, IntPtr extrainfo);

#endif
        /////////////////////////////
        // needed by Framework

        /// <SecurityNote>
        /// Critical - calls unmanaged code
        /// </SecurityNote>
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        internal static extern int GetLocaleInfoW(int locale, int type, string data, int dataSize);

        /// <SecurityNote>
        /// Critical - calls unmanaged code
        /// </SecurityNote>
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        internal static extern int FindNLSString(int locale, uint flags, [MarshalAs(UnmanagedType.LPWStr)]string sourceString, int sourceCount, [MarshalAs(UnmanagedType.LPWStr)]string findString, int findCount, out int found);


        //[DllImport(ExternDll.Psapi, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        //public static extern int GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder buffer, int length);

        //
        // OpenProcess
        //
        public const int PROCESS_VM_READ = 0x0010;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;

        //[DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto)]
        //public static extern IntPtr OpenProcess(int dwDesiredAccess, bool fInherit, int dwProcessId);

        [DllImport(ExternDll.User32, EntryPoint = "SetWindowText", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern bool IntSetWindowText(HandleRef hWnd, string text);

        ///<SecurityNote>
        /// Critical: LinkDemand on Win32Exception constructor
        /// TreatAsSafe: Throwing an exception isn't unsafe
        /// Note: If a SupressUnmanagedCodeSecurity attribute is ever added to IntSetWindowText, we'd need to be Critical
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static void SetWindowText(HandleRef hWnd, string text)
        {
            if (IntSetWindowText(hWnd, text) == false)
            {
                throw new Win32Exception();
            }
        }
        /// <SecurityNote>
        ///     Critical: This code calls into unmanaged code
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint = "GetIconInfo", CharSet = CharSet.Auto, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static extern bool GetIconInfoImpl(HandleRef hIcon, [Out] ICONINFO_IMPL piconinfo);

        [StructLayout(LayoutKind.Sequential)]
        internal class ICONINFO_IMPL
        {
            public bool fIcon = false;
            public int xHotspot = 0;
            public int yHotspot = 0;
            public IntPtr hbmMask = IntPtr.Zero;
            public IntPtr hbmColor = IntPtr.Zero;
        }

        // FOR REVIEW
        // note that a different-signature version of this method is defined in SafeNativeMethodsCLR.cs, but
        // this appears to be an intentional override of the functionality.  Seems odd if the real method
        // is really safe to reimplement it in an unsafe manner.  Need to review this.
        /// <SecurityNote>
        ///     Critical: This code calls into unmanaged code GetIconInfoImpl
        /// </SecurityNote>
        [SecurityCritical]
        internal static void GetIconInfo(HandleRef hIcon, out NativeMethods.ICONINFO piconinfo)
        {
            bool success = false;
            int error = 0;
            piconinfo = new NativeMethods.ICONINFO();
            ICONINFO_IMPL iconInfoImpl = new ICONINFO_IMPL();

            SRCS.RuntimeHelpers.PrepareConstrainedRegions(); // Mark the following as special
            try
            {
                // Intentionally empty
            }
            finally
            {
                // This block won't be interrupted by certain runtime induced failures or thread abort
                success = GetIconInfoImpl(hIcon, iconInfoImpl);
                error = Marshal.GetLastWin32Error();

                if (success)
                {
                    piconinfo.hbmMask = NativeMethods.BitmapHandle.CreateFromHandle(iconInfoImpl.hbmMask);
                    piconinfo.hbmColor = NativeMethods.BitmapHandle.CreateFromHandle(iconInfoImpl.hbmColor);
                    piconinfo.fIcon = iconInfoImpl.fIcon;
                    piconinfo.xHotspot = iconInfoImpl.xHotspot;
                    piconinfo.yHotspot = iconInfoImpl.yHotspot;
                }
            }

            if(!success)
            {
                Debug.WriteLine("GetIconInfo failed.  Error = " + error);

                throw new Win32Exception();
            }
        }

#if never

        [DllImport(ExternDll.User32,
#if WIN64
         EntryPoint="GetClassLongPtr",
#endif
 CharSet = CharSet.Auto, SetLastError = true)
        ]
        internal static extern UInt32 GetClassLong(IntPtr hwnd, int nIndex);
#endif

        [DllImport(ExternDll.User32, EntryPoint = "GetWindowPlacement", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IntGetWindowPlacement(HandleRef hWnd, ref NativeMethods.WINDOWPLACEMENT placement);

        // note:  this method exists in UnsafeNativeMethodsCLR.cs, but that method does not have the if/throw implemntation
        ///<SecurityNote>
        /// Critical: LinkDemand on Win32Exception constructor
        /// TreatAsSafe: Throwing an exception isn't unsafe
        /// Note: If a SupressUnmanagedCodeSecurity attribute is ever added to IntGetWindowPlacement, we'd need to be Critical
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static void GetWindowPlacement(HandleRef hWnd, ref NativeMethods.WINDOWPLACEMENT placement)
        {
            if (IntGetWindowPlacement(hWnd, ref placement) == false)
            {
                throw new Win32Exception();
            }
        }


        [DllImport(ExternDll.User32, EntryPoint = "SetWindowPlacement", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IntSetWindowPlacement(HandleRef hWnd, [In] ref NativeMethods.WINDOWPLACEMENT placement);

        // note: this method appears in UnsafeNativeMethodsCLR.cs but does not have the if/throw block
        ///<SecurityNote>
        /// Critical: LinkDemand on Win32Exception constructor
        /// Note: If a SupressUnmanagedCodeSecurity attribute is ever added to IntSetWindowPlacement, we'd need to be Critical
        /// TreatAsSafe: Throwing an exception isn't unsafe
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static void SetWindowPlacement(HandleRef hWnd, [In] ref NativeMethods.WINDOWPLACEMENT placement)
        {
            if (IntSetWindowPlacement(hWnd, ref placement) == false)
            {
                throw new Win32Exception();
            }
        }

        //[DllImport("secur32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        //internal static extern bool GetUserNameExW(
        //    [In] EXTENDED_NAME_FORMAT nameFormat,
        //    [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpNameBuffer,
        //    [In, Out] ref ulong nSize);


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, BestFitMapping = false)]
        internal static extern bool SystemParametersInfo(int nAction, int nParam, [In, Out] NativeMethods.ANIMATIONINFO anim, int nUpdate);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern bool SystemParametersInfo(int nAction, int nParam, [In, Out] NativeMethods.ICONMETRICS metrics, int nUpdate);


        //---------------------------------------------------------------------------
        //  SetWindowThemeAttribute()
        //                      - set attributes to control how themes are applied to
        //                        windows.
        //
        //  hwnd                - the handle of the window (cannot be NULL)
        //
        //  eAttribute           - one of the following:
        //
        //              WTA_NONCLIENT:
        //                      pvAttribute must be a WINDOWTHEMEATTRIBUTE pointer with a valid WTNCA flag
        //                      the default is all flags set to 0
        //
        //  pvAttribute             - pointer to data relevant to property being set size
        //                          is cbAttribute see each property for details.
        //
        //  cbAttribute             - size in bytes of the data pointed to by pvAttribute
        //
        //---------------------------------------------------------------------------
#if WCP_SYSTEM_THEMES_ENABLED
        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]
        public static extern uint SetWindowThemeAttribute(HandleRef hwnd, NativeMethods.WINDOWTHEMEATTRIBUTETYPE eAttribute, [In, MarshalAs(UnmanagedType.LPStruct)] NativeMethods.WTA_OPTIONS pvAttribute, int cbAttribute);
        public static uint SetWindowThemeAttribute(HandleRef hwnd, NativeMethods.WINDOWTHEMEATTRIBUTETYPE eAttribute, NativeMethods.WTA_OPTIONS pvAttribute)
        {
            return SetWindowThemeAttribute(hwnd, eAttribute, pvAttribute, Marshal.SizeOf(typeof(NativeMethods.WTA_OPTIONS)));
        }
#endif


        //---------------------------------------------------------------------------
        // BeginPanningFeedback - Visual feedback init function related to pan gesture
        //
        //  HWND hwnd - The handle to the Target window that will receive feedback
        //
        //---------------------------------------------------------------------------
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]
        public static extern bool BeginPanningFeedback(HandleRef hwnd);

        //---------------------------------------------------------------------------
        // UpdatePanningFeedback : Visual feedback function related to pan gesture
        // Can Be called only after a BeginPanningFeedback call
        //
        // HWND hwnd                 - The handle to the Target window that will receive feedback
        //                             For the method to succeed this must be the same hwnd as provided in
        //                             BeginPanningFeedback
        //
        // LONG lTotalOverpanOffsetX - The Total displacement that the window has moved in the horizontal direction
        //                             since the end of scrollable region was reached. The API would move the window by the distance specified
        //                             A maximum displacement of 30 pixels is allowed
        //
        // LONG lTotalOverpanOffsetY - The Total displacement that the window has moved in the horizontal direction
        //                             since the end of scrollable
        //                             region was reached. The API would move the window by the distance specified
        //                             A maximum displacement of 30 pixels is allowed
        //
        // BOOL fInInertia           - Flag dictating whether the Application is handling a WM_GESTURE message with the
        //                             GF_INERTIA FLAG set
        //
        //   Incremental calls to UpdatePanningFeedback should make sure they always pass
        //   the sum of the increments and not just the increment themselves
        //   Eg : If the initial displacement is 10 pixels and the next displacement 10 pixels
        //        the second call would be with the parameter as 20 pixels as opposed to 10
        //   Eg : UpdatePanningFeedback(hwnd, 10, 10, TRUE)
        //
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]
        public static extern bool UpdatePanningFeedback(
            HandleRef hwnd,
            int lTotalOverpanOffsetX,
            int lTotalOverpanOffsetY,
            bool fInInertia);

        //---------------------------------------------------------------------------
        //
        // EndPanningFeedback :Visual feedback reset function related to pan gesture
        //   Terminates any existing animation that was in process or set up by BeginPanningFeedback and UpdatePanningFeedback
        //   The EndPanningFeedBack needs to be called Prior to calling any BeginPanningFeedBack if we have already
        //   called a BeginPanningFeedBack followed by one/ more UpdatePanningFeedback calls
        //
        //  HWND hwnd         - The handle to the Target window that will receive feedback
        //
        //  BOOL fAnimateBack - Flag to indicate whether you wish the displaced window to move back
        //                      to the original position via animation or a direct jump.
        //                      Either way, the method will try to restore the moved window.
        //                      The latter case exists for compatibility with legacy apps.
        //
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]
        public static extern bool EndPanningFeedback(
            HandleRef hwnd,
            bool fAnimateBack);

        /// <summary>
        ///
        /// </summary>
        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetEvent(IntPtr hEvent);

        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int SetEvent([In] SafeWaitHandle hHandle);

        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int WaitForSingleObject([In] SafeWaitHandle hHandle, [In] int dwMilliseconds);


        //[DllImport(ExternDll.Kernel32, SetLastError = true)]
        //internal static extern int GetFileSize(SafeFileHandle hFile, ref int lpFileSizeHigh);


        //////////////////////////////////////
        // Needed by BASE
#if BASE_NATIVEMETHODS


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetMouseMovePointsEx(
                                        uint  cbSize,
                                        [In] ref NativeMethods.MOUSEMOVEPOINT pointsIn,
                                        [Out] NativeMethods.MOUSEMOVEPOINT[] pointsBufferOut,
                                        int nBufPoints,
                                        uint resolution
                                   );

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct ULARGE_INTEGER
        {
            [FieldOffset(0)]
            internal uint LowPart;

            [FieldOffset(4)]
            internal uint HighPart;

            [FieldOffset(0)]
            internal ulong QuadPart;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct LARGE_INTEGER
        {
            [FieldOffset(0)]
            internal int LowPart;

            [FieldOffset(4)]
            internal int HighPart;

            [FieldOffset(0)]
            internal long QuadPart;
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        internal static extern bool GetFileSizeEx(
            SafeFileHandle hFile,
            ref LARGE_INTEGER lpFileSize
            );


        /// <summary>Win32 constants</summary>
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /// <summary>Win32 constants</summary>
        internal const int PAGE_NOACCESS = 0x01;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_READONLY = 0x02;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_READWRITE = 0x04;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_WRITECOPY = 0x08;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_EXECUTE = 0x10;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_EXECUTE_READ = 0x20;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_EXECUTE_READWRITE = 0x40;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_EXECUTE_WRITECOPY = 0x80;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_GUARD = 0x100;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_NOCACHE = 0x200;
        /// <summary>Win32 constants</summary>
        internal const int PAGE_WRITECOMBINE = 0x400;
        /// <summary>Win32 constants</summary>
        internal const int MEM_COMMIT = 0x1000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_RESERVE = 0x2000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_DECOMMIT = 0x4000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_RELEASE = 0x8000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_FREE = 0x10000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_PRIVATE = 0x20000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_MAPPED = 0x40000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_RESET = 0x80000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_TOP_DOWN = 0x100000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_WRITE_WATCH = 0x200000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_PHYSICAL = 0x400000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_4MB_PAGES = unchecked((int)0x80000000);
        /// <summary>Win32 constants</summary>
        internal const int SEC_FILE = 0x800000;
        /// <summary>Win32 constants</summary>
        internal const int SEC_IMAGE = 0x1000000;
        /// <summary>Win32 constants</summary>
        internal const int SEC_RESERVE = 0x4000000;
        /// <summary>Win32 constants</summary>
        internal const int SEC_COMMIT = 0x8000000;
        /// <summary>Win32 constants</summary>
        internal const int SEC_NOCACHE = 0x10000000;
        /// <summary>Win32 constants</summary>
        internal const int MEM_IMAGE = SEC_IMAGE;
        /// <summary>Win32 constants</summary>
        internal const int WRITE_WATCH_FLAG_RESET = 0x01;

        /// <summary>Win32 constants</summary>
        internal const int SECTION_ALL_ACCESS =
            STANDARD_RIGHTS_REQUIRED |
            SECTION_QUERY |
            SECTION_MAP_WRITE |
            SECTION_MAP_READ |
            SECTION_MAP_EXECUTE |
            SECTION_EXTEND_SIZE;

        /// <summary>Win32 constants</summary>
        internal const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;

        /// <summary>Win32 constants</summary>
        internal const int SECTION_QUERY = 0x0001;
        /// <summary>Win32 constants</summary>
        internal const int SECTION_MAP_WRITE = 0x0002;
        /// <summary>Win32 constants</summary>
        internal const int SECTION_MAP_READ = 0x0004;
        /// <summary>Win32 constants</summary>
        internal const int SECTION_MAP_EXECUTE = 0x0008;
        /// <summary>Win32 constants</summary>
        internal const int SECTION_EXTEND_SIZE = 0x0010;

        /// <summary>Win32 constants</summary>
        internal const int FILE_MAP_COPY = SECTION_QUERY;
        /// <summary>Win32 constants</summary>
        internal const int FILE_MAP_WRITE = SECTION_MAP_WRITE;
        /// <summary>Win32 constants</summary>
        internal const int FILE_MAP_READ = SECTION_MAP_READ;
        /// <summary>Win32 constants</summary>
        internal const int FILE_MAP_ALL_ACCESS = SECTION_ALL_ACCESS;

        /// <summary>
        ///
        /// </summary>
        /// <param name="stringSecurityDescriptor"></param>
        /// <param name="stringSDRevision"></param>
        /// <param name="securityDescriptor"></param>
        /// <param name="securityDescriptorSize"></param>
        /// <returns></returns>
        [DllImport(ExternDll.Advapi32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            string stringSecurityDescriptor,    // security descriptor string
            int stringSDRevision,                  // revision level
            ref IntPtr securityDescriptor,       // SD
            IntPtr securityDescriptorSize       // SD size
            );

        /// <summary>Win32 constants</summary>
        internal const int SDDL_REVISION_1 = 1;
        /// <summary>Win32 constants</summary>
        internal const int SDDL_REVISION = SDDL_REVISION_1;


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern SafeFileMappingHandle OpenFileMapping(
            int dwDesiredAccess,
            bool bInheritHandle,
            string lpName
            );

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            UIntPtr dwSize,
            int flAllocationType,
            int flProtect
            );


        //
        // RIT WM_MOUSEQUERY structure for DWM WM_MOUSEQUERY (see HwndMouseInputSource.cs)
        //
        [StructLayout(LayoutKind.Sequential, Pack = 1)] // For DWM WM_MOUSEQUERY
        internal unsafe struct MOUSEQUERY
        {
            internal UInt32 uMsg;
            internal IntPtr wParam;
            internal IntPtr lParam;
            internal Int32 ptX;
            internal Int32 ptY;
            internal IntPtr hwnd;
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation (via SuppressUnmanagedCodeSecurity)
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int OleIsCurrentClipboard(IComDataObject pDataObj);

        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern int GetOEMCP();

#if never
        [DllImport("user32.dll", CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int ToUnicode(int nVirtKey, int nScanCode, byte[] keystate, StringBuilder text, int cch, int flags);
#endif

        // WinEvent fired when new Avalon UI is created
        public const int EventObjectUIFragmentCreate = 0x6FFFFFFF;


        //////////////////////////////////
        // Needed by FontCache

        [DllImport("ntdll.dll")]
        internal static extern int RtlNtStatusToDosError(int Status);

        internal static bool NtSuccess(int err)
        {
            return err >= STATUS_SUCCESS;
        }

        ///<SecurityNote>
        /// Critical: LinkDemand on Win32Exception constructor
        /// TreatAsSafe: Throwing an exception isn't unsafe
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static void NtCheck(int err)
        {
            if (!NtSuccess(err))
            {
                int win32error = RtlNtStatusToDosError(err);
                throw new System.ComponentModel.Win32Exception(win32error);
            }
        }

        internal const int STATUS_SUCCESS = 0;
        internal const int STATUS_TIMEOUT = 0x00000102;
        internal const int STATUS_BUFFER_TOO_SMALL = unchecked((int)0xC0000023);

#endif // BASE_NATIVEMETHODS

        //
        // COM Helper Methods
        //

        ///<SecurityNote>
        ///     Critical:    Satisfies a LinkDemand on releasecom call.
        ///</SecurityNote>
        [SecurityCritical]
        internal static int SafeReleaseComObject(object o)
        {
            int refCount = 0;

            // Validate
            if (o != null)
            {
                if (Marshal.IsComObject(o))
                {
                    refCount = Marshal.ReleaseComObject(o);
                }
            }

            return refCount;
        }

#if WINDOWS_BASE
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(DllImport.Wininet, EntryPoint = "GetUrlCacheConfigInfoW", SetLastError=true)]
        internal static extern bool GetUrlCacheConfigInfo(
            ref NativeMethods.InternetCacheConfigInfo pInternetCacheConfigInfo,
            ref UInt32 cbCacheConfigInfo,
            UInt32 /* DWORD */ fieldControl
            );
#endif

        /// <SecurityNote>
        ///     Critical: takes an hwnd, calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport("WtsApi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WTSRegisterSessionNotification(IntPtr hwnd, uint dwFlags);

        /// <SecurityNote>
        ///     Critical: takes an hwnd, calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport("WtsApi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WTSUnRegisterSessionNotification(IntPtr hwnd);

        /// <SecurityNote>
        ///     Critical: Calls unmanaged code.  Returns native process handle.
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        public const int DUPLICATE_CLOSE_SOURCE = 1;
        public const int DUPLICATE_SAME_ACCESS = 2;

        /// <SecurityNote>
        ///     Critical: Calls unmanaged code.  Returns native process handle.
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        public static extern bool DuplicateHandle(
            IntPtr hSourceProcess,
            SafeWaitHandle hSourceHandle,
            IntPtr hTargetProcessHandle,
            out IntPtr hTargetHandle,
            uint dwDesiredAccess,
            bool fInheritHandle,
            uint dwOptions
            );

        //
        // <Windows Color System (WCS) types>
        //

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct PROFILEHEADER
        {
            public uint phSize;                  // profile size in bytes
            public uint phCMMType;               // CMM for this profile
            public uint phVersion;               // profile format version number
            public uint phClass;                 // type of profile
            public NativeMethods.ColorSpace phDataColorSpace;  // color space of data
            public uint phConnectionSpace;       // PCS
            public uint phDateTime_0;            // date profile was created
            public uint phDateTime_1;            // date profile was created
            public uint phDateTime_2;            // date profile was created
            public uint phSignature;             // magic number ("Reserved for internal use.")
            public uint phPlatform;              // primary platform
            public uint phProfileFlags;          // various bit settings
            public uint phManufacturer;          // device manufacturer
            public uint phModel;                 // device model number
            public uint phAttributes_0;          // device attributes
            public uint phAttributes_1;          // device attributes
            public uint phRenderingIntent;       // rendering intent
            public uint phIlluminant_0;          // profile illuminant
            public uint phIlluminant_1;          // profile illuminant
            public uint phIlluminant_2;          // profile illuminant
            public uint phCreator;               // profile creator
            public fixed byte phReserved[44];
        };

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct PROFILE
        {
            public NativeMethods.ProfileType dwType; // profile type

            /// <SecurityNote>
            ///     Critical: Pointer field.
            /// </SecurityNote>
            [SecurityCritical]
            public void*       pProfileData;         // either the filename of the profile or buffer containing profile depending upon dwtype
            public uint        cbDataSize;           // size in bytes of pProfileData
        };

        /// <summary>The IsIconic function determines whether the specified window is minimized (iconic).</summary>
        /// <SecurityNote>
        ///     Critical: Calls unmanaged code.
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEHOOKSTRUCT
        {
            public NativeMethods.POINT pt;
            public IntPtr hwnd;
            public uint   wHitTestCode;
            public IntPtr dwExtraInfo;
        }

        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        /// <SecurityNote>
        ///     Critical: Calls unmanaged code.
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        public static HandleRef SetWindowsHookEx(HookType idHook, HookProc lpfn, IntPtr hMod, int dwThreadId)
        {
            IntPtr result = IntSetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
            if (result == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return new HandleRef(lpfn, result);
        }

        /// <SecurityNote>
        ///     Critical: Calls unmanaged code.
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint = "SetWindowsHookExW", SetLastError = true)]
        private static extern IntPtr IntSetWindowsHookEx(HookType idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        /// <SecurityNote>
        ///     Critical: Calls unmanaged code.
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(HandleRef hhk);

        /// <SecurityNote>
        ///     Critical: Calls unmanaged code.
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(HandleRef hhk, int nCode, IntPtr wParam, IntPtr lParam);
    }
}
