//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace MS.Win32
{
    using Accessibility;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.ConstrainedExecution;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Security;
    using System.Diagnostics;
    using System.ComponentModel;
#if !DRT && !UIAUTOMATIONTYPES
    using MS.Internal.Interop;
#endif

 // DRTs cannot access MS.Internal
#if !DRT && !UIAUTOMATIONTYPES
    using HR = MS.Internal.Interop.HRESULT;
#endif

 //The SecurityHelper class differs between assemblies and could not actually be
 // shared, so it is duplicated across namespaces to prevent name collision.
#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif UIAUTOMATIONTYPES
    using MS.Internal.UIAutomationTypes;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use a class (duplicated across multiple namespaces) from an unknown assembly.
#endif

    using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

    public partial class UnsafeNativeMethods {

        private struct POINTSTRUCT {
            public int x;
            public int y;

            public POINTSTRUCT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        // For some reason "PtrToStructure" requires super high permission.
        /// <SecurityNote>
        ///     Critical: The code below has a link demand for unmanaged code permission.This code can be used to
        ///               get to data that a pointer points to which can lead to easier data reading.
        /// </SecurityNote>
        [SecurityCritical]
        public static object PtrToStructure(IntPtr lparam, Type cls) {
            return Marshal.PtrToStructure(lparam, cls);
        }

        // For some reason "StructureToPtr" requires super high permission.
        /// <SecurityNote>
        ///     Critical: The code below has a link demand for unmanaged code permission.This code can be used to
        ///               write data to arbitrary memory.
        /// </SecurityNote>
        [SecurityCritical]
        public static void StructureToPtr(object structure, IntPtr ptr, bool fDeleteOld)
        {
            Marshal.StructureToPtr(structure, ptr, fDeleteOld);
        }

#if BASE_NATIVEMETHODS
        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int OleGetClipboard(ref IComDataObject data);
        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern int OleSetClipboard(IComDataObject pDataObj);
        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern int OleFlushClipboard();
#endif
        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.DwmAPI, BestFitMapping = false)]
        public static extern int DwmIsCompositionEnabled(out Int32 enabled);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr GetCurrentThread();

#if !DRT && !UIAUTOMATIONTYPES
        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        public static extern WindowMessage RegisterWindowMessage(string msg);
#endif

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, EntryPoint = "SetWindowPos", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        ///<SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);

        public enum MonitorOpts : int
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002,
        }

        public enum MonitorDpiType
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
        }

        public enum ProcessDpiAwareness
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Shcore, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern uint GetProcessDpiAwareness(HandleRef hProcess, out IntPtr awareness);

        ///<SecurityNote>
        /// Critical: This code escalates to unmanaged code permission
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Shcore, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern uint GetDpiForMonitor(HandleRef hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, EntryPoint = "IsProcessDPIAware", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool IsProcessDPIAware();

        ///<SecurityNote>
        /// Critical: This code escalates to unmanaged code permission
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool fInherit, int dwProcessId);

        ///<SecurityNote>
        /// Critical: This code escalates to unmanaged code permission
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, EntryPoint = "EnableNonClientDpiScaling", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnableNonClientDpiScaling(HandleRef hWnd);

        ///<SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        public static extern int GetClassName(HandleRef hwnd, StringBuilder lpClassName, int nMaxCount);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        public static extern int MessageBox(HandleRef hWnd, string text, string caption, int type);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "SetWindowTheme")]
        public static extern int CriticalSetWindowTheme(HandleRef hWnd, string subAppName, string subIdList);


        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, EntryPoint = "CreateCompatibleBitmap", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateCompatibleBitmap(HandleRef hDC, int width, int height);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC. Can be used to run arbitrary code.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, EntryPoint = "CreateCompatibleBitmap", CharSet = CharSet.Auto)]
        public static extern IntPtr CriticalCreateCompatibleBitmap(HandleRef hDC, int width, int height);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC. Can be used to run arbitrary code.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, EntryPoint = "GetStockObject", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CriticalGetStockObject(int stockObject);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC. Can be used to run arbitrary code.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint = "FillRect", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int CriticalFillRect(IntPtr hdc, ref NativeMethods.RECT rcFill, IntPtr brush);

        /// <SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetBitmapBits(HandleRef hbmp, int cbBuffer, byte[] lpvBits);

        /// <SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);

        /// <SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        public static void DeleteObject(HandleRef hObject)
        {
            HandleCollector.Remove((IntPtr)hObject, NativeMethods.CommonHandles.GDI);

            if (!IntDeleteObject(hObject))
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission via a call to IntDeleteObject
        /// </SecurityNote>
        [SecurityCritical]
        public static bool DeleteObjectNoThrow(HandleRef hObject)
        {
            HandleCollector.Remove((IntPtr)hObject, NativeMethods.CommonHandles.GDI);

            bool result = IntDeleteObject(hObject);
            int error = Marshal.GetLastWin32Error();

            if(!result)
            {
                Debug.WriteLine("DeleteObject failed.  Error = " + error);
            }

            return result;
        }


        /// <SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Gdi32, SetLastError=true, ExactSpelling = true, EntryPoint="DeleteObject", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool IntDeleteObject(HandleRef hObject);


        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SelectObject(HandleRef hdc, IntPtr obj);

        /// <SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SelectObject(HandleRef hdc, NativeMethods.BitmapHandle obj);

        /// <SecurityNote>
        ///     Critical: This code escalates to unmanaged code permission
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Gdi32, EntryPoint="SelectObject", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CriticalSelectObject(HandleRef hdc, IntPtr obj);

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        public static extern int GetClipboardFormatName(int format, StringBuilder lpString, int cchMax);

        /// <SecurityNote>
        ///     This code elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        public static extern int RegisterClipboardFormat(string format);

        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool BitBlt(HandleRef hDC, int x, int y, int nWidth, int nHeight,
                                         HandleRef hSrcDC, int xSrc, int ySrc, int dwRop);
        /// <SecurityNote>
        ///     This code elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="PrintWindow", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool CriticalPrintWindow(HandleRef hWnd, HandleRef hDC, int flags);

        /// <SecurityNote>
        ///     This code elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="RedrawWindow", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool CriticalRedrawWindow(HandleRef hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);

        [DllImport(ExternDll.Shell32, CharSet=CharSet.Auto, BestFitMapping = false)]
        public static extern int DragQueryFile(HandleRef hDrop, int iFile, StringBuilder lpszFile, int cch);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Shell32, CharSet=CharSet.Auto, BestFitMapping = false)]
        public static extern IntPtr ShellExecute(HandleRef hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

	    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal class ShellExecuteInfo
        {
            public int cbSize;
            public ShellExecuteFlags fMask;
            public IntPtr hwnd;
            public string lpVerb;
            public string lpFile;
            public string lpParameters;
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            public string lpClass;
            public IntPtr hkeyClass;
            public int dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        [Flags]
        internal enum ShellExecuteFlags
        {
            SEE_MASK_CLASSNAME = 0x00000001,
            SEE_MASK_CLASSKEY =  0x00000003,
            SEE_MASK_NOCLOSEPROCESS = 0x00000040,
            SEE_MASK_FLAG_DDEWAIT = 0x00000100,
            SEE_MASK_DOENVSUBST = 0x00000200,
            SEE_MASK_FLAG_NO_UI = 0x00000400,
            SEE_MASK_UNICODE = 0x00004000,
            SEE_MASK_NO_CONSOLE = 0x00008000,
            SEE_MASK_ASYNCOK = 0x00100000,
            SEE_MASK_HMONITOR = 0x00200000,
            SEE_MASK_NOZONECHECKS = 0x00800000,
            SEE_MASK_NOQUERYCLASSSTORE = 0x01000000,
            SEE_MASK_WAITFORINPUTIDLE = 0x02000000
        };

        ///<SecurityNote>
        ///     Critical - elevates via SUC. Starts a new process.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Shell32, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool ShellExecuteEx([In, Out] ShellExecuteInfo lpExecInfo);

        public const int MB_PRECOMPOSED            = 0x00000001;
        public const int MB_COMPOSITE              = 0x00000002;
        public const int MB_USEGLYPHCHARS          = 0x00000004;
        public const int MB_ERR_INVALID_CHARS      = 0x00000008;
        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Kernel32, ExactSpelling=true, CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int MultiByteToWideChar(int CodePage, int dwFlags, byte[] lpMultiByteStr, int cchMultiByte, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpWideCharStr, int cchWideChar);
        ///<SecurityNote>
        ///     Critical - elevates (via SuppressUnmanagedCodeSecurity).
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Kernel32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int WideCharToMultiByte(int codePage, int flags, [MarshalAs(UnmanagedType.LPWStr)]string wideStr, int chars, [In,Out]byte[] pOutBytes, int bufferBytes, IntPtr defaultChar, IntPtr pDefaultUsed);

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling=true, EntryPoint="RtlMoveMemory", CharSet=CharSet.Unicode)]
        public static extern void CopyMemoryW(IntPtr pdst, string psrc, int cb);
        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode)]
        public static extern void CopyMemoryW(IntPtr pdst, char[] psrc, int cb);
        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling=true, EntryPoint="RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr pdst, byte[] psrc, int cb);

#if BASE_NATIVEMETHODS
        ///<SecurityNote>
        /// Critical as this code performs an elevation due to an unmanaged code call. Also this
        /// information can be used to exploit the system.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="GetKeyboardState", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern int IntGetKeyboardState(byte [] keystate);
        [SecurityCritical]
        public static void GetKeyboardState(byte [] keystate)
        {
            if(IntGetKeyboardState(keystate) == 0)
            {
                throw new Win32Exception();
            }
        }
#endif

#if DRT_NATIVEMETHODS
        [DllImport(ExternDll.User32, ExactSpelling=true, EntryPoint="keybd_event", CharSet=CharSet.Auto)]
        public static extern void Keybd_event(byte vk, byte scan, int flags, IntPtr extrainfo);
#endif

#if !DRT && !UIAUTOMATIONTYPES
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, EntryPoint = "GetModuleFileName", CharSet=CharSet.Unicode, SetLastError = true)]
        private static extern int IntGetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission by calling into IntGetModuleFileName
        /// </SecurityNote>
        [SecurityCritical]
        internal static string GetModuleFileName(HandleRef hModule)
        {
            // .Net is currently far behind Windows with regard to supporting paths longer than MAX_PATH.
            // At one point it was tested trying to load UNC paths longer than MAX_PATH and mscorlib threw
            // FileIOExceptions before WPF was even on the stack.
            // All the same, we still want to have this grow-and-retry logic because the CLR can be hosted
            // in a native application.  Callers bothering to use this rather than Assembly based reflection
            // are likely doing so because of (at least the potential for) the returned name referring to a
            // native module.
            StringBuilder buffer = new StringBuilder(Win32Constant.MAX_PATH);
            while (true)
            {
                int size = IntGetModuleFileName(hModule, buffer, buffer.Capacity);
                if (size == 0)
                {
                    throw new Win32Exception();
                }

                // GetModuleFileName returns nSize when it's truncated but does NOT set the last error.
                // MSDN documentation says this has changed in Windows 2000+.
                if (size == buffer.Capacity)
                {
                    // Enlarge the buffer and try again.
                    buffer.EnsureCapacity(buffer.Capacity * 2);
                    continue;
                }

                return buffer.ToString();
            }
        }
#endif


#if BASE_NATIVEMETHODS
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern bool TranslateMessage([In, Out] ref System.Windows.Interop.MSG msg);


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet=CharSet.Auto)]
        public static extern IntPtr DispatchMessage([In] ref System.Windows.Interop.MSG msg);
#endif

#if BASE_NATIVEMETHODS
        ///<SecurityNote>
        ///     Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet=CharSet.Auto, EntryPoint="PostThreadMessage", SetLastError=true)]
        private static extern int IntPostThreadMessage(int id, int msg, IntPtr wparam, IntPtr lparam);
        [SecurityCritical]
        public static void PostThreadMessage(int id, int msg, IntPtr wparam, IntPtr lparam)
        {
            if(IntPostThreadMessage(id, msg, wparam, lparam) == 0)
            {
                throw new Win32Exception();
            }
        }
#endif

	    ///<SecurityNote>
        ///     Critical - This code elevates to unmanaged code.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport("oleacc.dll")]
        internal static extern int ObjectFromLresult(IntPtr lResult, ref Guid iid, IntPtr wParam, [In, Out] ref IAccessible ppvObject);

        ///<SecurityNote>
        ///     Critical - This code elevates to unmanaged code.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport("user32.dll")]
        internal static extern bool IsWinEventHookInstalled(int winevent);

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, EntryPoint="OleInitialize")]
        private static extern int IntOleInitialize(IntPtr val);

        [SecurityCritical]
        public static int OleInitialize()
        {
            return IntOleInitialize(IntPtr.Zero);
        }

        /// <SecurityNote>
        ///    Critical: SUC. Inherently unsafe.
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32)]
        public static extern int CoRegisterPSClsid(ref Guid riid, ref Guid rclsid);


        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public extern static bool EnumThreadWindows(int dwThreadId, NativeMethods.EnumThreadWindowsCallback lpfn, HandleRef lParam);

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int OleUninitialize();

        [DllImport(ExternDll.Kernel32, EntryPoint="CloseHandle", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern bool IntCloseHandle(HandleRef handle);

        ///<SecurityNote>
        /// Critical: Closes a passed in handle, LinkDemand on Marshal.GetLastWin32Error
        ///</SecurityNote>
        [SecurityCritical]
        public static bool CloseHandleNoThrow(HandleRef handle)
        {
            HandleCollector.Remove((IntPtr)handle, NativeMethods.CommonHandles.Kernel);

            bool result = IntCloseHandle(handle);
            int error = Marshal.GetLastWin32Error();

            if(!result)
            {
                Debug.WriteLine("CloseHandle failed.  Error = " + error);
            }

            return result;

        }

        ///<SecurityNote>
        ///  Critical as this code performs an UnmanagedCodeSecurity elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, ref System.Runtime.InteropServices.ComTypes.IStream istream);

#if BASE_NATIVEMETHODS
        [DllImport(ExternDll.Gdi32, SetLastError=true, EntryPoint="CreateCompatibleDC", CharSet=CharSet.Auto)]
        private static extern IntPtr IntCreateCompatibleDC(HandleRef hDC);


        ///<SecurityNote>
        ///     Critical - elevates via a SUC. Can be used to run arbitrary code.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError=true, EntryPoint="CreateCompatibleDC", CharSet=CharSet.Auto)]
        public static extern IntPtr CriticalCreateCompatibleDC(HandleRef hDC);

        ///<SecurityNote>
        /// Critical: LinkDemand on Win32Exception constructor
        /// TreatAsSafe: Throwing an exception isn't unsafe
        /// Note: If SupressUnmanagedCodeSecurity attribute is ever added to IntCreateCompatibleDC, we need to be Critical
        ///</SecurityNote>
        [SecuritySafeCritical]
        public static IntPtr CreateCompatibleDC(HandleRef hDC)
        {
            IntPtr h = IntCreateCompatibleDC(hDC);
            if(h == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return HandleCollector.Add(h, NativeMethods.CommonHandles.HDC);
        }
#endif

        [DllImport(ExternDll.Kernel32, EntryPoint="UnmapViewOfFile", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern bool IntUnmapViewOfFile(HandleRef pvBaseAddress);
        /*
        ///<SecurityNote>
        /// Critical: LinkDemand on Win32Exception constructor
        /// TreatAsSafe: Throwing an exception isn't unsafe
        /// Note: If SupressUnmanagedCodeSecurity attribute is ever added to IntUnmapViewOfFile, we need to be Critical
        ///</SecurityNote>
        [SecuritySafeCritical]
        public static void UnmapViewOfFile(HandleRef pvBaseAddress)
        {
            HandleCollector.Remove((IntPtr)pvBaseAddress, NativeMethods.CommonHandles.Kernel);
            if(IntUnmapViewOfFile(pvBaseAddress) == 0)
            {
                throw new Win32Exception();
            }
        }
        */
        ///<SecurityNote>
        /// Critical: Unmaps a file handle, LinkDemand on Marshal.GetLastWin32Error
        ///</SecurityNote>
        [SecurityCritical]
        public static bool UnmapViewOfFileNoThrow(HandleRef pvBaseAddress)
        {
            HandleCollector.Remove((IntPtr)pvBaseAddress, NativeMethods.CommonHandles.Kernel);

            bool result = IntUnmapViewOfFile(pvBaseAddress);
            int error = Marshal.GetLastWin32Error();

            if(!result)
            {
                Debug.WriteLine("UnmapViewOfFile failed.  Error = " + error);
            }

            return result;
        }


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        /// </SecurityNote>
        [SecurityCritical]
        public static bool EnableWindow(HandleRef hWnd, bool enable)
        {
            bool result = NativeMethodsSetLastError.EnableWindow(hWnd, enable);
            if(!result)
            {
                int win32Err = Marshal.GetLastWin32Error();
                if(win32Err != 0)
                {
                    throw new Win32Exception(win32Err);
                }
            }

            return result;
        }

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        /// </SecurityNote>
        [SecurityCritical]
        public static bool EnableWindowNoThrow(HandleRef hWnd, bool enable)
        {
            // This method is not throwing because the caller don't want to fail after calling this.
            // If the window was not previously disabled, the return value is zero, else it is non-zero.
            return NativeMethodsSetLastError.EnableWindow(hWnd, enable);
        }

        // GetObject stuff
        [DllImport(ExternDll.Gdi32, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In, Out] NativeMethods.BITMAP bm);

        /// <SecurityNote>
        ///    Critical: This code returns the window which has focus and elevates to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr GetFocus();

        ///<SecurityNote>
        /// Critical - this code elevates via SUC.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint = "GetCursorPos", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IntGetCursorPos([In, Out] NativeMethods.POINT pt);

        ///<SecurityNote>
        /// Critical - calls a critical function.
        ///</SecurityNote>
        [SecurityCritical]
        internal static bool GetCursorPos([In, Out] NativeMethods.POINT pt)
        {
            bool returnValue = IntGetCursorPos(pt);
            if (returnValue == false)
            {
                throw new Win32Exception();
            }
            return returnValue;
        }

        ///<SecurityNote>
        /// Critical - this code elevates via SUC.
        ///</SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint = "GetCursorPos", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern bool IntTryGetCursorPos([In, Out] NativeMethods.POINT pt);

        ///<SecurityNote>
        /// Critical - calls a critical function.
        ///</SecurityNote>
        [SecurityCritical]
        internal static bool TryGetCursorPos([In, Out] NativeMethods.POINT pt)
        {
            bool returnValue = IntTryGetCursorPos(pt);

            // Sometimes Win32 will fail this call, such as if you are
            // not running in the interactive desktop.  For example,
            // a secure screen saver may be running.
            if (returnValue == false)
            {
                System.Diagnostics.Debug.WriteLine("GetCursorPos failed!");

                pt.x = 0;
                pt.y = 0;
            }
            return returnValue;
        }

#if BASE_NATIVEMETHODS || CORE_NATIVEMETHODS || FRAMEWORK_NATIVEMETHODS
        /// <SecurityNote>
        ///     Critical:Unmanaged code that gets the state of the keyboard keys
        ///     This can be exploited to get keyboard state.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);

        /// <SecurityNote>
        ///     Critical:Unmanaged code that gets the state of the keyboard keys
        ///     This can be exploited to get keyboard state.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern short GetKeyState(int keyCode);

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, PreserveSig = false)]
        public static extern void DoDragDrop(IComDataObject dataObject, UnsafeNativeMethods.IOleDropSource dropSource, int allowedEffects, int[] finalEffect);

        ///<SecurityNote>
        /// Critical - this code elevates via SUC.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling=true, CharSet=CharSet.Auto)]
        internal static extern void ReleaseStgMedium(ref STGMEDIUM medium);

        /// <SecurityNote>
        /// Critical - this code elevates via SUC.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool InvalidateRect(HandleRef hWnd, IntPtr rect, bool erase);


#endif


        /// <SecurityNote>
        /// SecurityCritical due to a call to SetLastError and calls GetWindowText
        /// </SecurityNote>
        [SecurityCritical]
        internal static int GetWindowText(HandleRef hWnd, [Out] StringBuilder lpString, int nMaxCount)
        {
            int returnValue = NativeMethodsSetLastError.GetWindowText(hWnd, lpString, nMaxCount);
            if (returnValue == 0)
            {
                int win32Err = Marshal.GetLastWin32Error();
                if (win32Err != 0)
                {
                    throw new Win32Exception(win32Err);
                }
            }
            return returnValue;
        }

        /// <SecurityNote>
        /// SecurityCritical due to a call to SetLastError
        /// </SecurityNote>
        [SecurityCritical]
        internal static int GetWindowTextLength(HandleRef hWnd)
        {
            int returnValue = NativeMethodsSetLastError.GetWindowTextLength(hWnd);
            if (returnValue == 0)
            {
                int win32Err = Marshal.GetLastWin32Error();
                if (win32Err != 0)
                {
                    throw new Win32Exception(win32Err);
                }
            }
            return returnValue;
        }

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GlobalAlloc(int uFlags, IntPtr dwBytes);

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GlobalReAlloc(HandleRef handle, IntPtr bytes, int flags);

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GlobalLock(HandleRef handle);

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalUnlock(HandleRef handle);

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GlobalFree(HandleRef handle);

        ///<SecurityNote>
        ///  Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GlobalSize(HandleRef handle);

#if BASE_NATIVEMETHODS || CORE_NATIVEMETHODS || FRAMEWORK_NATIVEMETHODS
        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet=CharSet.Auto)]
        public static extern bool ImmSetConversionStatus(HandleRef hIMC, int conversion, int sentence);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet=CharSet.Auto)]
        public static extern bool ImmGetConversionStatus(HandleRef hIMC, ref int conversion, ref int sentence);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern IntPtr ImmGetContext(HandleRef hWnd);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern bool ImmReleaseContext(HandleRef hWnd, HandleRef hIMC);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet=CharSet.Auto)]
        public static extern IntPtr ImmAssociateContext(HandleRef hWnd, HandleRef hIMC);


        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern bool ImmSetOpenStatus(HandleRef hIMC, bool open);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern bool ImmGetOpenStatus(HandleRef hIMC);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern bool ImmNotifyIME(HandleRef hIMC, int dwAction, int dwIndex, int dwValue);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet=CharSet.Auto)]
        public static extern int ImmGetProperty(HandleRef hkl, int flags);

        // ImmGetCompositionString for result and composition strings
        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmGetCompositionString(HandleRef hIMC, int dwIndex, char[] lpBuf, int dwBufLen);

        // ImmGetCompositionString for display attributes
        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmGetCompositionString(HandleRef hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);

        // ImmGetCompositionString for clause information
        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmGetCompositionString(HandleRef hIMC, int dwIndex, int[] lpBuf, int dwBufLen);

        // ImmGetCompositionString for query information
        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmGetCompositionString(HandleRef hIMC, int dwIndex, IntPtr lpBuf, int dwBufLen);

        //[DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        //public static extern int ImmSetCompositionFont(HandleRef hIMC, [In, Out] ref NativeMethods.LOGFONT lf);

        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmConfigureIME(HandleRef hkl, HandleRef hwnd, int dwData, IntPtr pvoid);

        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmConfigureIME(HandleRef hkl, HandleRef hwnd, int dwData, [In] ref NativeMethods.REGISTERWORD registerWord);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmSetCompositionWindow(HandleRef hIMC, [In, Out] ref NativeMethods.COMPOSITIONFORM compform);

        /// <SecurityNote>
        ///     Critical:This code causes an elevation of privilige to unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern int ImmSetCandidateWindow(HandleRef hIMC, [In, Out] ref NativeMethods.CANDIDATEFORM candform);

        [DllImport(ExternDll.Imm32, CharSet = CharSet.Auto)]
        public static extern IntPtr ImmGetDefaultIMEWnd(HandleRef hwnd);
#endif

        ///<SecurityNote>
        /// Critical - calls SetFocusWrapper (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        internal static IntPtr SetFocus(HandleRef hWnd)
        {
            IntPtr result = IntPtr.Zero;

            if(!TrySetFocus(hWnd, ref result))
            {
                throw new Win32Exception();
            }

            return result;
        }

        ///<SecurityNote>
        /// Critical - calls SetFocusWrapper (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        internal static bool TrySetFocus(HandleRef hWnd)
        {
            IntPtr result = IntPtr.Zero;
            return TrySetFocus(hWnd, ref result);
        }

        ///<SecurityNote>
        /// Critical - calls SetFocusWrapper (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        internal static bool TrySetFocus(HandleRef hWnd, ref IntPtr result)
        {
            result = NativeMethodsSetLastError.SetFocus(hWnd);
            int errorCode = Marshal.GetLastWin32Error();

            if (result == IntPtr.Zero && errorCode != 0)
            {
                return false;
            }

            return true;
        }

        /// <SecurityNote>
        /// Critical - This code returns a critical resource and calls critical code.
        /// </SecurityNote>
        [SecurityCritical]
        internal static IntPtr GetParent(HandleRef hWnd)
        {
            IntPtr retVal = NativeMethodsSetLastError.GetParent(hWnd);
            int errorCode = Marshal.GetLastWin32Error();

            if (retVal == IntPtr.Zero && errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }

            return retVal;
        }

        /// <SecurityNote>
        /// Critical - This code returns a critical resource and causes unmanaged code elevation.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetAncestor(HandleRef hWnd, int flags);

        /// <SecurityNote>
        /// Critical - This code causes unmanaged code elevation.
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern bool IsChild(HandleRef hWndParent, HandleRef hwnd);


        //*****************
        //
        // if you're thinking of enabling either of the functions below.
        // you should first take a look at SafeSecurityHelper.TransformGlobalRectToLocal & TransformLocalRectToScreen
        // they likely do what you typically use the function for - and it's safe to use.
        // if you use the function below - you will get exceptions in partial trust.
        // anyquestions - email avsee.
        //
        //******************


        ///<SecurityNote>
        ///     Critical as this code performs an elevation.
        ///</SecurityNote>
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        [ SecurityCritical, SuppressUnmanagedCodeSecurity]
        public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, EntryPoint = "GetModuleHandle", CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        private static extern IntPtr IntGetModuleHandle(string modName);
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        internal static IntPtr GetModuleHandle(string modName)
        {
            IntPtr retVal = IntGetModuleHandle(modName);

            if (retVal == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return retVal;
        }


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet=CharSet.Auto)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hWnd, int msg,
                                                IntPtr wParam, IntPtr lParam);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, SetLastError=true, EntryPoint="GetProcAddress", CharSet=CharSet.Ansi, BestFitMapping=false)]
        public static extern IntPtr IntGetProcAddress(HandleRef hModule, string lpProcName);

        ///<SecurityNote>
        /// Critical - calls IntGetProcAddress (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        public static IntPtr GetProcAddress(HandleRef hModule, string lpProcName)
        {
            IntPtr result = IntGetProcAddress(hModule, lpProcName);
            if(result == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return result;
        }

     // GetProcAddress Note : The lpProcName parameter can identify the DLL function by specifying an ordinal value associated
     // with the function in the EXPORTS statement. GetProcAddress verifies that the specified ordinal is in
     // the range 1 through the highest ordinal value exported in the .def file. The function then uses the
     // ordinal as an index to read the function's address from a function table. If the .def file does not number
     // the functions consecutively from 1 to N (where N is the number of exported functions), an error can
     // occur where GetProcAddress returns an invalid, non-NULL address, even though there is no function with the specified ordinal.

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, EntryPoint="GetProcAddress", CharSet=CharSet.Ansi, BestFitMapping=false)]
        public static extern IntPtr GetProcAddressNoThrow(HandleRef hModule, string lpProcName);

        /// <SecurityNote>
        ///     Critical: as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [Flags]
        internal enum LoadLibraryFlags : uint
        {
            None = 0x00000000,
            /// <summary>
            /// If this value is used, and the executable module is a DLL, the system does 
            /// not call DllMain for process and thread initialization and termination. 
            /// Also, the system does not load additional executable modules that are 
            /// referenced by the specified module.
            /// </summary>
            /// <remarks>
            /// Do not use this value; it is provided only for backward compatibility. 
            /// If you are planning to access only data or resources in the DLL, use 
            /// <see cref="LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE"/> or 
            /// <see cref="LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE"/> or <see cref="LOAD_LIBRARY_AS_IMAGE_RESOURCE"/>
            /// or both. Otherwise, load the library as a DLL or executable module 
            /// using the <see cref="LoadLibrary(string)"/>function.
            /// </remarks>
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            /// <summary>
            /// If this value is used, the system does not check AppLocker rules or apply 
            /// Software Restriction Policies for the DLL. This action applies only to the 
            /// DLL being loaded and not to its dependencies. This value is recommended 
            /// for use in setup programs that must run extracted DLLs during installation.
            /// </summary>
            /// <remarks>
            /// Windows Server 2008 R2 and Windows 7:  
            ///     On systems with KB2532445 installed, the 
            ///     caller must be running as "LocalSystem" or "TrustedInstaller"; otherwise the 
            ///     system ignores this flag. For more information, see "You can circumvent AppLocker 
            ///     rules by using an Office macro on a computer that is running Windows 7 or 
            ///     Windows Server 2008 R2" in the Help and Support Knowledge Base 
            ///     at <see cref="http://support.microsoft.com/kb/2532445."/>
            /// 
            /// Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP:  
            ///     AppLocker was introduced in Windows 7 and Windows Server 2008 R2.
            /// </remarks>
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            /// <summary>
            /// If this value is used, the system maps the file into the calling process's 
            /// virtual address space as if it were a data file. Nothing is done to execute 
            /// or prepare to execute the mapped file. Therefore, you cannot call functions 
            /// like GetModuleFileName, GetModuleHandle or GetProcAddress with this DLL. 
            /// Using this value causes writes to read-only memory to raise an access violation. 
            /// Use this flag when you want to load a DLL only to extract messages or resources 
            /// from it.This value can be used with <see cref="LOAD_LIBRARY_AS_IMAGE_RESOURCE"/>.
            /// </summary>
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            /// <summary>
            /// Similar to LOAD_LIBRARY_AS_DATAFILE, except that the DLL file is opened with 
            /// exclusive write access for the calling process. Other processes cannot open 
            /// the DLL file for write access while it is in use. However, the DLL can 
            /// still be opened by other processes. This value can be used with 
            /// <see cref="LOAD_LIBRARY_AS_IMAGE_RESOURCE"/>. 
            /// </summary>
            /// <remarks>
            /// Windows Server 2003 and Windows XP:  This value is not supported until 
            /// Windows Vista.
            /// </remarks>
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            /// <summary>
            /// If this value is used, the system maps the file into the process's virtual 
            /// address space as an image file. However, the loader does not load the static 
            /// imports or perform the other usual initialization steps. Use this flag when 
            /// you want to load a DLL only to extract messages or resources from it. Unless 
            /// the application depends on the file having the in-memory layout of an image, 
            /// this value should be used with either <see cref="LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE "/> or 
            /// <see cref="LOAD_LIBRARY_AS_DATAFILE"/>.
            /// </summary>
            /// <remarks>
            /// Windows Server 2003 and Windows XP:  This value is not supported until Windows Vista.
            /// </remarks>
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            /// <summary>
            /// If this value is used, the application's installation directory is searched for the 
            /// DLL and its dependencies. Directories in the standard search path are not searched. 
            /// This value cannot be combined with <see cref="LOAD_WITH_ALTERED_SEARCH_PATH"/>.
            /// </summary>
            /// <remarks>
            /// Windows 7, Windows Server 2008 R2, Windows Vista and Windows Server 2008:  
            ///     This value requires KB2533623 to be installed.
            /// Windows Server 2003 and Windows XP:  
            ///     This value is not supported.
            /// </remarks>
            LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
            /// <summary>
            /// This value is a combination of <see cref="LOAD_LIBRARY_SEARCH_APPLICATION_DIR"/>, 
            /// <see cref="LOAD_LIBRARY_SEARCH_SYSTEM32"/>, and <see cref="LOAD_LIBRARY_SEARCH_USER_DIRS"/>. 
            /// Directories in the standard search path are not searched. This value cannot be combined with 
            /// <see cref="LOAD_WITH_ALTERED_SEARCH_PATH"/>. This value represents the recommended maximum number 
            /// of directories an application should include in its DLL search path.
            /// </summary>
            /// <remarks>
            /// Windows 7, Windows Server 2008 R2, Windows Vista and Windows Server 2008:  
            ///     This value requires KB2533623 to be installed. 
            /// 
            /// Windows Server 2003 and Windows XP:  
            ///     This value is not supported.
            /// </remarks>
            LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
            /// <summary>
            /// If this value is used, the directory that contains the DLL is temporarily added to 
            /// the beginning of the list of directories that are searched for the DLL's dependencies. 
            /// Directories in the standard search path are not searched.
            /// 
            /// The lpFileName parameter must specify a fully qualified path. This value cannot be 
            /// combined with <see cref="LOAD_WITH_ALTERED_SEARCH_PATH"/>. 
            /// 
            /// For example, if Lib2.dll is a dependency of C:\Dir1\Lib1.dll, loading Lib1.dll with 
            /// this value causes the system to search for Lib2.dll only in C:\Dir1. To search for 
            /// Lib2.dll in C:\Dir1 and all of the directories in the DLL search path, combine this 
            /// value with <see cref="LOAD_LIBRARY_DEFAULT_DIRS"/>.
            /// </summary>
            /// <remarks>
            /// Windows 7, Windows Server 2008 R2, Windows Vista and Windows Server 2008:  
            ///     This value requires KB2533623 to be installed.
            /// 
            /// Windows Server 2003 and Windows XP:  
            ///     This value is not supported.
            /// </remarks>
            LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
            /// <summary>
            /// If this value is used, %windows%\system32 is searched for the DLL and its dependencies. 
            /// Directories in the standard search path are not searched. This value cannot be 
            /// combined with <see cref="LOAD_WITH_ALTERED_SEARCH_PATH"/>
            /// </summary>
            /// <remarks>
            /// Windows 7, Windows Server 2008 R2, Windows Vista and Windows Server 2008:  
            ///     This value requires KB2533623 to be installed.
            /// Windows Server 2003 and Windows XP:  
            ///     This value is not supported.
            /// </remarks>
            LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
            /// <summary>
            /// If this value is used, directories added using the AddDllDirectory or the SetDllDirectory 
            /// function are searched for the DLL and its dependencies. If more than one directory has been added, 
            /// the order in which the directories are searched is unspecified. Directories in the 
            /// standard search path are not searched. This value cannot be combined with 
            /// <see cref="LOAD_WITH_ALTERED_SEARCH_PATH"/>
            /// </summary>
            /// <remarks>
            /// Windows 7, Windows Server 2008 R2, Windows Vista and Windows Server 2008:  
            ///     This value requires KB2533623 to be installed.
            /// Windows Server 2003 and Windows XP:  
            ///     This value is not supported.
            /// </remarks>
            LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
            /// <summary>
            /// If this value is used and lpFileName specifies an absolute path, the system uses the alternate 
            /// file search strategy discussed in the Remarks section to find associated executable modules that 
            /// the specified module causes to be loaded. If this value is used and lpFileName specifies a 
            /// relative path, the behavior is undefined. If this value is not used, or if lpFileName does not specify a path, 
            /// the system uses the standard search strategy discussed in the Remarks section to find associated 
            /// executable modules that the specified module causes to be loaded.This value cannot be combined with 
            /// any LOAD_LIBRARY_SEARCH flag.
            /// </summary>
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }

        /// <summary>
        /// Do not use this - instead use <see cref="LoadLibraryHelper.SecureLoadLibrary"/>
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [Obsolete("Use LoadLibraryHelper.SafeLoadLibraryEx instead")]
        [DllImport(ExternDll.Kernel32, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibraryEx([In][MarshalAs(UnmanagedType.LPTStr)]string lpFileName, IntPtr hFile, [In] LoadLibraryFlags dwFlags);

        [Flags]
        internal enum GetModuleHandleFlags : uint
        {
            None = 0x00000000,
            /// <summary>
            /// The lpModuleName parameter in <see cref="GetModuleHandleEx"/> is an address 
            /// in the module.
            /// </summary>
            GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004,
            /// <summary>
            /// The module stays loaded until the process is terminated, no matter how many times 
            /// FreeLibrary is called.
            /// This option cannot be used with <see cref="GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT"/>.
            /// </summary>
            GET_MODULE_HANDLE_EX_FLAG_PIN = 0x00000001,
            /// <summary>
            /// The reference count for the module is not incremented. This option is equivalent to the 
            /// behavior of GetModuleHandle. Do not pass the retrieved module handle to the FreeLibrary 
            /// function; doing so can cause the DLL to be unmapped prematurely.
            /// This option cannot be used with <see cref="GET_MODULE_HANDLE_EX_FLAG_PIN"/>.
            /// </summary>
            GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002
        }

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport(ExternDll.Kernel32, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetModuleHandleEx(
            [In] GetModuleHandleFlags dwFlags,
            [In][Optional][MarshalAs(UnmanagedType.LPTStr)] string lpModuleName,
            [Out] out IntPtr hModule);

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport(ExternDll.Kernel32, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary([In] IntPtr hModule);

#if !DRT && !UIAUTOMATIONTYPES
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern int GetSystemMetrics(SM nIndex);
#endif

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet=CharSet.Auto, BestFitMapping = false)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref NativeMethods.RECT rc, int nUpdate);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref int value, int ignore);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref bool value, int ignore);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref NativeMethods.HIGHCONTRAST_I rc, int nUpdate);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, [In, Out] NativeMethods.NONCLIENTMETRICS metrics, int nUpdate);

        /// <SecurityNote>
        ///  Critical as this code performs an elevation.
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetSystemPowerStatus(ref NativeMethods.SYSTEM_POWER_STATUS systemPowerStatus);

        ///<SecurityNote>
        /// Critical - performs an elevation via SUC.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="ClientToScreen", SetLastError=true, ExactSpelling=true, CharSet=CharSet.Auto)]
        private static extern int IntClientToScreen(HandleRef hWnd, [In, Out] NativeMethods.POINT pt);

        ///<SecurityNote>
        ///     Critical calls critical code - IntClientToScreen
        ///</SecurityNote>
        [SecurityCritical]
        public static void ClientToScreen(HandleRef hWnd, [In, Out] NativeMethods.POINT pt)
        {
            if(IntClientToScreen(hWnd, pt) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr GetDesktopWindow();

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission and can be used to
        ///     change the foreground window.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern int RegisterDragDrop(HandleRef hwnd, UnsafeNativeMethods.IOleDropTarget target);

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Ole32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern int RevokeDragDrop(HandleRef hwnd);

#if !DRT && !UIAUTOMATIONTYPES
        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission and can be used to
        ///     get information of messages in queues.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet=CharSet.Auto)]
        public static extern bool PeekMessage([In, Out] ref System.Windows.Interop.MSG msg, HandleRef hwnd, WindowMessage msgMin, WindowMessage msgMax, int remove);

#if BASE_NATIVEMETHODS
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, BestFitMapping = false, CharSet=CharSet.Auto)]
        public static extern bool SetProp(HandleRef hWnd, string propName, HandleRef data);

#endif

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IntPostMessage(HandleRef hwnd, WindowMessage msg, IntPtr wparam, IntPtr lparam);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        internal static void PostMessage(HandleRef hwnd, WindowMessage msg, IntPtr wparam, IntPtr lparam)
        {
            if (!IntPostMessage(hwnd, msg, wparam, lparam))
            {
                throw new Win32Exception();
            }
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint = "PostMessage", CharSet = CharSet.Auto)]
        internal static extern bool TryPostMessage(HandleRef hwnd, WindowMessage msg, IntPtr wparam, IntPtr lparam);
#endif
#if BASE_NATIVEMETHODS || CORE_NATIVEMETHODS
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern void NotifyWinEvent(int winEvent, HandleRef hwnd, int objType, int objID);
#endif
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, EntryPoint = "BeginPaint", CharSet = CharSet.Auto)]
        private static extern IntPtr IntBeginPaint(HandleRef hWnd, [In, Out] ref NativeMethods.PAINTSTRUCT lpPaint);

        ///<SecurityNote>
        /// Critical as this code performs an elevation. via the call to IntBeginPaint
        ///</SecurityNote>
        [SecurityCritical]
        public static IntPtr BeginPaint(HandleRef hWnd, [In, Out, MarshalAs(UnmanagedType.LPStruct)] ref NativeMethods.PAINTSTRUCT lpPaint) {
            return HandleCollector.Add(IntBeginPaint(hWnd, ref lpPaint), NativeMethods.CommonHandles.HDC);
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, EntryPoint = "EndPaint", CharSet = CharSet.Auto)]
        private static extern bool IntEndPaint(HandleRef hWnd, ref NativeMethods.PAINTSTRUCT lpPaint);
        ///<SecurityNote>
        /// Critical as this code performs an elevation via the call to IntEndPaint.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        public static bool EndPaint(HandleRef hWnd, [In, MarshalAs(UnmanagedType.LPStruct)] ref NativeMethods.PAINTSTRUCT lpPaint) {
            HandleCollector.Remove(lpPaint.hdc, NativeMethods.CommonHandles.HDC);
            return IntEndPaint(hWnd, ref lpPaint);
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true, EntryPoint = "GetDC", CharSet = CharSet.Auto)]
        private static extern IntPtr IntGetDC(HandleRef hWnd);
        ///<SecurityNote>
        /// Critical as this code performs an elevation. The call to handle collector is
        /// by itself not dangerous because handle collector simply
        /// stores a count of the number of instances of a given
        /// handle and not the handle itself.
        ///</SecurityNote>
        [SecurityCritical]
        public static IntPtr GetDC(HandleRef hWnd)
        {
            IntPtr hDc = IntGetDC(hWnd);
            if(hDc == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return HandleCollector.Add(hDc, NativeMethods.CommonHandles.HDC);
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.The call to handle collector
        /// is by itself not dangerous because handle collector simply
        /// stores a count of the number of instances of a given handle and not the handle itself.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, EntryPoint = "ReleaseDC", CharSet = CharSet.Auto)]
        private static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC) {
            HandleCollector.Remove((IntPtr)hDC, NativeMethods.CommonHandles.HDC);
            return IntReleaseDC(hWnd, hDC);
        }


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);

        ///<SecurityNote>
        /// Critical as this code performs an elevation to unmanaged code
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr GetActiveWindow();

        ///<SecurityNote>
        /// Critical as this code performs an elevation to unmanaged code
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern bool SetForegroundWindow(HandleRef hWnd);

        // Begin API Additions to support common dialog controls
        ///<SecurityNote>
        /// Critical as this code performs an elevation to unmanaged code
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Comdlg32, SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern int CommDlgExtendedError();

        ///<SecurityNote>
        /// Critical as this code performs an elevation to unmanaged code
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Comdlg32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetOpenFileName([In, Out] NativeMethods.OPENFILENAME_I ofn);

        ///<SecurityNote>
        /// Critical as this code performs an elevation to unmanaged code
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Comdlg32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetSaveFileName([In, Out] NativeMethods.OPENFILENAME_I ofn);
        // End Common Dialog API Additions

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [return:MarshalAs(UnmanagedType.Bool)]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetLayeredWindowAttributes(HandleRef hwnd, int crKey, byte bAlpha, int dwFlags);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, NativeMethods.POINT pptDst, NativeMethods.POINT pSizeDst, IntPtr hdcSrc, NativeMethods.POINT pptSrc, int crKey, ref NativeMethods.BLENDFUNCTION pBlend, int dwFlags);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern IntPtr SetActiveWindow(HandleRef hWnd);

        //TODO: Refactor shared native methods so that parser dependency
        // is in separate file. PS # 30845.
#if PBTCOMPILER
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr SetCursor(HandleRef hcursor);
#endif

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]

        [DllImport(ExternDll.User32, ExactSpelling=true, EntryPoint="DestroyCursor", CharSet=CharSet.Auto)]
        private static extern bool IntDestroyCursor(IntPtr hCurs);

        ///<SecurityNote>
        /// Critical calls IntDestroyCursor
        ///</SecurityNote>
        [SecurityCritical]
        public static bool DestroyCursor(IntPtr hCurs) {
            return IntDestroyCursor(hCurs);
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="DestroyIcon", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        private static extern bool IntDestroyIcon(IntPtr hIcon);

        ///<SecurityNote>
        /// Critical: calls a critical method (IntDestroyIcon)
        ///</SecurityNote>
        [SecurityCritical]
        public static bool DestroyIcon(IntPtr hIcon)
        {
            bool result = IntDestroyIcon(hIcon);
            int error = Marshal.GetLastWin32Error();

            if(!result)
            {
                // To be consistent with out other PInvoke wrappers
                // we should "throw" here.  But we don't want to
                // introduce new "throws" w/o time to follow up on any
                // new problems that causes.
                Debug.WriteLine("DestroyIcon failed.  Error = " + error);
                //throw new Win32Exception();
            }

            return result;
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, EntryPoint="DeleteObject", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        private static extern bool IntDeleteObject(IntPtr hObject);

        ///<SecurityNote>
        /// Critical: calls a critical method (IntDeleteObject)
        ///</SecurityNote>
        [SecurityCritical]
        public static bool DeleteObject(IntPtr hObject)
        {
            bool result = IntDeleteObject(hObject);
            int error = Marshal.GetLastWin32Error();

            if(!result)
            {
                // To be consistent with out other PInvoke wrappers
                // we should "throw" here.  But we don't want to
                // introduce new "throws" w/o time to follow up on any
                // new problems that causes.
                Debug.WriteLine("DeleteObject failed.  Error = " + error);
                //throw new Win32Exception();
            }

            return result;
        }

#if BASE_NATIVEMETHODS || CORE_NATIVEMETHODS || FRAMEWORK_NATIVEMETHODS
        /// <SecurityNote>
        /// Critical as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "CreateDIBSection")]
        private static extern NativeMethods.BitmapHandle PrivateCreateDIBSection(HandleRef hdc, ref NativeMethods.BITMAPINFO bitmapInfo, int iUsage, ref IntPtr ppvBits, SafeFileMappingHandle hSection, int dwOffset);
        /// <SecurityNote>
        /// Critical - The method invokes PrivateCreateDIBSection.
        /// </SecurityNote>
        [SecurityCritical]
        internal static NativeMethods.BitmapHandle CreateDIBSection(HandleRef hdc, ref NativeMethods.BITMAPINFO bitmapInfo, int iUsage, ref IntPtr ppvBits, SafeFileMappingHandle hSection, int dwOffset)
        {
            if (hSection == null)
            {
                // PInvoke marshalling does not handle null SafeHandle, we must pass an IntPtr.Zero backed SafeHandle
                hSection = new SafeFileMappingHandle(IntPtr.Zero);
            }

            NativeMethods.BitmapHandle hBitmap = PrivateCreateDIBSection(hdc, ref bitmapInfo, iUsage, ref ppvBits, hSection, dwOffset);
            int error = Marshal.GetLastWin32Error();

            if ( hBitmap.IsInvalid )
            {
                Debug.WriteLine("CreateDIBSection failed. Error = " + error);
            }

            return hBitmap;
        }
#endif

        /// <SecurityNote>
        /// Critical as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "CreateBitmap")]
        private static extern NativeMethods.BitmapHandle PrivateCreateBitmap(int width, int height, int planes, int bitsPerPixel, byte[] lpvBits);
        /// <SecurityNote>
        /// Critical - The method invokes PrivateCreateBitmap.
        /// </SecurityNote>
        [SecurityCritical]
        internal static NativeMethods.BitmapHandle CreateBitmap(int width, int height, int planes, int bitsPerPixel, byte[] lpvBits)
        {
            NativeMethods.BitmapHandle hBitmap = PrivateCreateBitmap(width, height, planes, bitsPerPixel, lpvBits);
            int error = Marshal.GetLastWin32Error();

            if ( hBitmap.IsInvalid )
            {
                Debug.WriteLine("CreateBitmap failed. Error = " + error);
            }

            return hBitmap;
        }

        /// <SecurityNote>
        /// Critical as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "DestroyIcon")]
        private static extern bool PrivateDestroyIcon(HandleRef handle);
        /// <SecurityNote>
        /// Critical - The method invokes PrivateDestroyIcon.
        /// </SecurityNote>
        [SecurityCritical]
        internal static bool DestroyIcon(HandleRef handle)
        {
            HandleCollector.Remove((IntPtr)handle, NativeMethods.CommonHandles.Icon);

            bool result = PrivateDestroyIcon(handle);
            int error = Marshal.GetLastWin32Error();

            if ( !result )
            {
                Debug.WriteLine("DestroyIcon failed. Error = " + error);
            }

            return result;
        }

        /// <SecurityNote>
        /// Critical as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "CreateIconIndirect")]
        private static extern NativeMethods.IconHandle PrivateCreateIconIndirect([In, MarshalAs(UnmanagedType.LPStruct)]NativeMethods.ICONINFO iconInfo);
        /// <SecurityNote>
        /// Critical - The method invokes PrivateCreateIconIndirect.
        /// </SecurityNote>
        [SecurityCritical]
        internal static NativeMethods.IconHandle CreateIconIndirect([In, MarshalAs(UnmanagedType.LPStruct)]NativeMethods.ICONINFO iconInfo)
        {
            NativeMethods.IconHandle hIcon = PrivateCreateIconIndirect(iconInfo);
            int error = Marshal.GetLastWin32Error();

            if ( hIcon.IsInvalid )
            {
                Debug.WriteLine("CreateIconIndirect failed. Error = " + error);
            }

            return hIcon;
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern bool IsWindow(HandleRef hWnd);

#if BASE_NATIVEMETHODS
        [DllImport(ExternDll.Gdi32, SetLastError=true, ExactSpelling=true, EntryPoint="DeleteDC", CharSet=CharSet.Auto)]
        private static extern bool IntDeleteDC(HandleRef hDC);
        ///<SecurityNote>
        /// Critical: LinkDemand on Win32Exception constructor
        /// TreatAsSafe: Throwing an exception isn't unsafe
        /// Note: If SupressUnmanagedCodeSecurity attribute is ever added to IntDeleteDC, we need to be Critical
        ///</SecurityNote>
        [SecuritySafeCritical]
        public static void DeleteDC(HandleRef hDC)
        {
            HandleCollector.Remove((IntPtr)hDC, NativeMethods.CommonHandles.HDC);
            if(!IntDeleteDC(hDC))
            {
                throw new Win32Exception();
            }
        }


        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError=true, ExactSpelling=true, EntryPoint="DeleteDC", CharSet=CharSet.Auto)]
        private static extern bool IntCriticalDeleteDC(HandleRef hDC);

        ///<SecurityNote>
        ///     Critical: This code elevates to unmanaged code
        ///</SecurityNote>
        [SecurityCritical]
        public static void CriticalDeleteDC(HandleRef hDC)
        {
            HandleCollector.Remove((IntPtr)hDC, NativeMethods.CommonHandles.HDC);
            if(!IntCriticalDeleteDC(hDC))
            {
                throw new Win32Exception();
            }
        }
#endif


#if BASE_NATIVEMETHODS

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError=true, EntryPoint="GetMessageW", ExactSpelling=true, CharSet=CharSet.Unicode)]
        private static extern int IntGetMessageW([In, Out] ref System.Windows.Interop.MSG msg, HandleRef hWnd, int uMsgFilterMin, int uMsgFilterMax);
        ///<SecurityNote>
        /// Critical - calls IntGetMessageW (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        public static bool GetMessageW([In, Out] ref System.Windows.Interop.MSG msg, HandleRef hWnd, int uMsgFilterMin, int uMsgFilterMax)
        {
            bool boolResult = false;

            int result = IntGetMessageW(ref msg, hWnd, uMsgFilterMin, uMsgFilterMax);
            if(result == -1)
            {
                throw new Win32Exception();
            }
            else if(result == 0)
            {
                boolResult = false;
            }
            else
            {
                boolResult = true;
            }

            return boolResult;
        }

#endif

#if BASE_NATIVEMETHODS

        /// <SecurityNote>
        ///     Critical: This code elevates via a SUC to call into unmanaged Code and can get the HWND of windows at any arbitrary point on the screen
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="WindowFromPoint", ExactSpelling=true, CharSet=CharSet.Auto)]
        private static extern IntPtr IntWindowFromPoint(POINTSTRUCT pt);

        /// <SecurityNote>
        ///     Critical: This calls WindowFromPoint(POINTSTRUCT) which is marked SecurityCritical
        /// </SecurityNote>
        [SecurityCritical]
        public static IntPtr WindowFromPoint(int x, int y) {
            POINTSTRUCT ps = new POINTSTRUCT(x, y);
            return IntWindowFromPoint(ps);
        }
#endif

        /// <SecurityNote>
        ///     Critical: This code elevates to call into unmanaged Code
        /// </SecurityNote>
        [SecurityCritical,SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, EntryPoint="CreateWindowEx", CharSet=CharSet.Auto, BestFitMapping = false, SetLastError=true)]
        public static extern IntPtr IntCreateWindowEx(int  dwExStyle, string lpszClassName,
                                                   string lpszWindowName, int style, int x, int y, int width, int height,
                                                   HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        /// <SecurityNote>
        ///     Critical: This code elevates to call into unmanaged Code by calling IntCreateWindowEx
        /// </SecurityNote>
        [SecurityCritical]
        public static IntPtr CreateWindowEx(int  dwExStyle, string lpszClassName,
                                         string lpszWindowName, int style, int x, int y, int width, int height,
                                         HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)]object pvParam) {
            IntPtr retVal = IntCreateWindowEx(dwExStyle, lpszClassName,
                                         lpszWindowName, style, x, y, width, height, hWndParent, hMenu,
                                         hInst, pvParam);
            if(retVal == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            return retVal;
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, EntryPoint="DestroyWindow", CharSet=CharSet.Auto)]
        public static extern bool IntDestroyWindow(HandleRef hWnd);

        ///<SecurityNote>
        /// Critical - calls Security Critical method
        ///</SecurityNote>
        [SecurityCritical]
        public static void DestroyWindow(HandleRef hWnd)
        {
            if(!IntDestroyWindow(hWnd))
            {
                throw new Win32Exception();
            }
        }
        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32)]
        internal static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, NativeMethods.WinEventProcDef WinEventReentrancyFilter, uint idProcess, uint idThread, int dwFlags);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32)]
        internal static extern bool UnhookWinEvent(IntPtr winEventHook);

        ///<SecurityNote>
        ///     Critical - Delegate invoked by elevated (via a SUC) pinvoke.
        ///</SecurityNote>
        [SecurityCritical]
        public delegate bool EnumChildrenCallback(IntPtr hwnd, IntPtr lParam);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        public static void EnumChildWindows(HandleRef hwndParent, EnumChildrenCallback lpEnumFunc, HandleRef lParam)
        {
            // http://msdn.microsoft.com/en-us/library/ms633494(VS.85).aspx
            // Return value is not used
            IntEnumChildWindows(hwndParent, lpEnumFunc, lParam);
        }

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.User32, EntryPoint = "EnumChildWindows", ExactSpelling = true)]
        private static extern bool IntEnumChildWindows(HandleRef hwndParent, EnumChildrenCallback lpEnumFunc, HandleRef lParam);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowRgn(HandleRef hWnd, HandleRef hRgn);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PtInRegion(HandleRef hRgn, int X, int Y);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

        // for GetUserNameEx
        public enum EXTENDED_NAME_FORMAT {
            NameUnknown = 0,
            NameFullyQualifiedDN = 1,
            NameSamCompatible = 2,
            NameDisplay = 3,
            NameUniqueId = 6,
            NameCanonical = 7,
            NameUserPrincipal = 8,
            NameCanonicalEx = 9,
            NameServicePrincipal = 10
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000122-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleDropTarget {

            [PreserveSig]
            int OleDragEnter(
                [In, MarshalAs(UnmanagedType.Interface)]
                object pDataObj,
                [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
                [In, MarshalAs(UnmanagedType.U8)]
                long pt,
                [In, Out]
                ref int pdwEffect);

            [PreserveSig]
            int OleDragOver(
                [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
                [In, MarshalAs(UnmanagedType.U8)]
                long pt,
                [In, Out]
                ref int pdwEffect);

            [PreserveSig]
            int OleDragLeave();

            [PreserveSig]
            int OleDrop(
                [In, MarshalAs(UnmanagedType.Interface)]
                object pDataObj,
                [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
                [In, MarshalAs(UnmanagedType.U8)]
                long pt,
                [In, Out]
                ref int pdwEffect);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000121-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleDropSource {

            [PreserveSig]
            int OleQueryContinueDrag(
                int fEscapePressed,
                [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState);

            [PreserveSig]
            int OleGiveFeedback(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwEffect);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [
        ComImport(),
        Guid("B196B289-BAB4-101A-B69C-00AA00341D07"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)
        ]
        public interface IOleControlSite {

            [PreserveSig]
            int OnControlInfoChanged();

            [PreserveSig]
            int LockInPlaceActive(int fLock);

            [PreserveSig]
            int GetExtendedControl(
                [Out, MarshalAs(UnmanagedType.IDispatch)]
                out object ppDisp);

            [PreserveSig]
            int TransformCoords(
                [In, Out]
                NativeMethods.POINT pPtlHimetric,
                [In, Out]
                NativeMethods.POINTF pPtfContainer,
                [In, MarshalAs(UnmanagedType.U4)]
                int dwFlags);

            [PreserveSig]
            int TranslateAccelerator(
                [In]
                ref System.Windows.Interop.MSG pMsg,
                [In, MarshalAs(UnmanagedType.U4)]
                int grfModifiers);

            [PreserveSig]
            int OnFocus(int fGotFocus);

            [PreserveSig]
            int ShowPropertyFrame();

        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000118-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleClientSite {

            [PreserveSig]
            int SaveObject();

            [PreserveSig]
            int GetMoniker(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwAssign,
                [In, MarshalAs(UnmanagedType.U4)]
                int dwWhichMoniker,
                [Out, MarshalAs(UnmanagedType.Interface)]
                out object moniker);

            [PreserveSig]
            int GetContainer(out IOleContainer container);

            [PreserveSig]
            int ShowObject();

            [PreserveSig]
            int OnShowWindow(int fShow);

            [PreserveSig]
            int RequestNewObjectLayout();
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000119-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceSite {

            IntPtr GetWindow();

            [PreserveSig]
            int ContextSensitiveHelp(int fEnterMode);

            [PreserveSig]
            int CanInPlaceActivate();

            [PreserveSig]
            int OnInPlaceActivate();

            [PreserveSig]
            int OnUIActivate();

            [PreserveSig]
            int GetWindowContext(
                [Out, MarshalAs(UnmanagedType.Interface)]
                out UnsafeNativeMethods.IOleInPlaceFrame ppFrame,
                [Out, MarshalAs(UnmanagedType.Interface)]
                out UnsafeNativeMethods.IOleInPlaceUIWindow ppDoc,
                [Out]
                NativeMethods.COMRECT lprcPosRect,
                [Out]
                NativeMethods.COMRECT lprcClipRect,
                [In, Out]
                NativeMethods.OLEINPLACEFRAMEINFO lpFrameInfo);

            [PreserveSig]
            int Scroll(
                NativeMethods.SIZE scrollExtant);

            [PreserveSig]
            int OnUIDeactivate(
                int fUndoable);

            [PreserveSig]
            int OnInPlaceDeactivate();

            [PreserveSig]
            int DiscardUndoState();

            [PreserveSig]
            int DeactivateAndUndo();

            [PreserveSig]
            int OnPosRectChange(
                [In]
                NativeMethods.COMRECT lprcPosRect);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("9BFBBC02-EFF1-101A-84ED-00AA00341D07"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyNotifySink {
            void OnChanged(int dispID);

            [PreserveSig]
            int OnRequestEdit(int dispID);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000100-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IEnumUnknown {

            [PreserveSig]
            int Next(
                [In, MarshalAs(UnmanagedType.U4)]
                int celt,
                [Out]
                IntPtr rgelt,
                IntPtr pceltFetched);

            [PreserveSig]
                int Skip(
                [In, MarshalAs(UnmanagedType.U4)]
                int celt);

            void Reset();

            void Clone(
                [Out]
                out IEnumUnknown ppenum);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("0000011B-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleContainer {

            [PreserveSig]
            int ParseDisplayName(
                [In, MarshalAs(UnmanagedType.Interface)]
                object pbc,
                [In, MarshalAs(UnmanagedType.BStr)]
                string pszDisplayName,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                int[] pchEaten,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                object[] ppmkOut);

            [PreserveSig]
            int EnumObjects(
                [In, MarshalAs(UnmanagedType.U4)]
                int grfFlags,
                [Out]
                out IEnumUnknown ppenum);

            [PreserveSig]
            int LockContainer(
                bool fLock);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000116-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceFrame {

            IntPtr GetWindow();

            [PreserveSig]
            int ContextSensitiveHelp(int fEnterMode);

            [PreserveSig]
            int GetBorder(
                [Out]
                NativeMethods.COMRECT lprectBorder);

            [PreserveSig]
            int RequestBorderSpace(
                [In]
                NativeMethods.COMRECT pborderwidths);

            [PreserveSig]
            int SetBorderSpace(
                [In]
                NativeMethods.COMRECT pborderwidths);

            [PreserveSig]
            int  SetActiveObject(
                [In, MarshalAs(UnmanagedType.Interface)]
                UnsafeNativeMethods.IOleInPlaceActiveObject pActiveObject,
                [In, MarshalAs(UnmanagedType.LPWStr)]
                string pszObjName);

            [PreserveSig]
            int InsertMenus(
                [In]
                IntPtr hmenuShared,
                [In, Out]
                NativeMethods.tagOleMenuGroupWidths lpMenuWidths);

            [PreserveSig]
            int SetMenu(
                [In]
                IntPtr hmenuShared,
                [In]
                IntPtr holemenu,
                [In]
                IntPtr hwndActiveObject);

            [PreserveSig]
            int RemoveMenus(
                [In]
                IntPtr hmenuShared);

            [PreserveSig]
            int SetStatusText(
                [In, MarshalAs(UnmanagedType.LPWStr)]
                string pszStatusText);

            [PreserveSig]
            int EnableModeless(
                bool fEnable);

            [PreserveSig]
                int TranslateAccelerator(
                [In]
                ref System.Windows.Interop.MSG lpmsg,
                [In, MarshalAs(UnmanagedType.U2)]
                short wID);
            }

        //IMPORTANT: Do not try to optimize perf here by changing the enum size to byte
        //instead of int since this is used in COM Interop for browser hosting scenarios
        // Enum for OLECMDIDs used by IOleCommandTarget in browser hosted scenarios
        // Imported from the published header - docobj.h, If you need to support more
        // than these OLECMDS, add it from that header file
        public enum OLECMDID {
            OLECMDID_SAVE                   = 3,
            OLECMDID_SAVEAS                 = 4,
            OLECMDID_PRINT                  = 6,
            OLECMDID_PRINTPREVIEW           = 7,
            OLECMDID_PAGESETUP              = 8,
            OLECMDID_PROPERTIES             = 10,
            OLECMDID_CUT                    = 11,
            OLECMDID_COPY                   = 12,
            OLECMDID_PASTE                  = 13,
            OLECMDID_SELECTALL              = 17,
            OLECMDID_REFRESH                = 22,
            OLECMDID_STOP                   = 23,
        }

        public enum OLECMDEXECOPT {
            OLECMDEXECOPT_DODEFAULT         = 0,
            OLECMDEXECOPT_PROMPTUSER        = 1,
            OLECMDEXECOPT_DONTPROMPTUSER    = 2,
            OLECMDEXECOPT_SHOWHELP          = 3
        }

        // OLECMDID Flags used by IOleCommandTarget to specify status of commands in browser hosted scenarios
        // Imported from the published header - docobj.h
        public enum OLECMDF {
            /// <summary>
            /// The command is supported by this object
            /// </summary>
            OLECMDF_SUPPORTED = 0x1,
            /// <summary>
            /// The command is available and enabled
            /// </summary>
            OLECMDF_ENABLED = 0x2,
            /// <summary>
            /// The command is an on-off toggle and is currently on
            /// </summary>
            OLECMDF_LATCHED = 0x4,
            /// <summary>
            /// Reserved for future use
            /// </summary>
            OLECMDF_NINCHED = 0x8,
            /// <summary>
            /// Command is invisible
            /// </summary>
            OLECMDF_INVISIBLE = 0x10,
            /// <summary>
            /// Command should not be displayed in the context menu
            /// </summary>
            OLECMDF_DEFHIDEONCTXTMENU = 0x20
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000115-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceUIWindow {
             IntPtr GetWindow();

             [PreserveSig]
             int ContextSensitiveHelp(
                    int fEnterMode);

             [PreserveSig]
             int GetBorder(
                    [Out]
                    NativeMethods.RECT lprectBorder);

             [PreserveSig]
             int RequestBorderSpace(
                    [In]
                    NativeMethods.RECT pborderwidths);

             [PreserveSig]
             int SetBorderSpace(
                    [In]
                    NativeMethods.RECT pborderwidths);

             void SetActiveObject(
                    [In, MarshalAs(UnmanagedType.Interface)]
                    UnsafeNativeMethods.IOleInPlaceActiveObject pActiveObject,
                    [In, MarshalAs(UnmanagedType.LPWStr)]
                    string pszObjName);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(),
        Guid("00000117-0000-0000-C000-000000000046"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceActiveObject {
             /// <SecurityNote>
             /// Critical: SUC. Exposes a native window handle.
             /// </SecurityNote>
             [SuppressUnmanagedCodeSecurity, SecurityCritical]
             [PreserveSig]
             int GetWindow(out IntPtr hwnd);

             void ContextSensitiveHelp(
                     int fEnterMode);

             /// <SecurityNote>
             ///     Critical: This code escalates to unmanaged code permission
             /// </SecurityNote>
             [SuppressUnmanagedCodeSecurity, SecurityCritical]
             [PreserveSig]
             int TranslateAccelerator(
                    [In]
                    ref System.Windows.Interop.MSG lpmsg);

             void OnFrameWindowActivate(
                    int fActivate);

             void OnDocWindowActivate(
                    int fActivate);

             void ResizeBorder(
                    [In]
                    NativeMethods.RECT prcBorder,
                    [In]
                    UnsafeNativeMethods.IOleInPlaceUIWindow pUIWindow,
                    bool fFrameWindow);

             void EnableModeless(
                    int fEnable);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("00000114-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleWindow {

             [PreserveSig]
             int GetWindow( [Out]out IntPtr hwnd );


             void ContextSensitiveHelp(

                     int fEnterMode);
        }

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [ SecurityCritical( SecurityCriticalScope.Everything ) , SuppressUnmanagedCodeSecurity ]
        [ComImport(),
        Guid("00000113-0000-0000-C000-000000000046"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceObject {

             [PreserveSig]
             int GetWindow( [Out]out IntPtr hwnd );


             void ContextSensitiveHelp(

                     int fEnterMode);


             void InPlaceDeactivate();


             [PreserveSig]
             int UIDeactivate();


             void SetObjectRects(
                    [In]
                      NativeMethods.COMRECT lprcPosRect,
                    [In]
                      NativeMethods.COMRECT lprcClipRect);

             void ReactivateAndUndo();


        }

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SecurityCritical( SecurityCriticalScope.Everything ) , SuppressUnmanagedCodeSecurity ]
        [ComImport(),
        Guid("00000112-0000-0000-C000-000000000046"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleObject {

             [PreserveSig]
             int SetClientSite(
                    [In, MarshalAs(UnmanagedType.Interface)]
                      UnsafeNativeMethods.IOleClientSite pClientSite);


             UnsafeNativeMethods.IOleClientSite GetClientSite();

             [PreserveSig]
             int SetHostNames(
                    [In, MarshalAs(UnmanagedType.LPWStr)]
                      string szContainerApp,
                    [In, MarshalAs(UnmanagedType.LPWStr)]
                      string szContainerObj);

             [PreserveSig]
             int Close(

                     int dwSaveOption);

             [PreserveSig]
             int SetMoniker(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwWhichMoniker,
                    [In, MarshalAs(UnmanagedType.Interface)]
                     object pmk);

              [PreserveSig]
              int GetMoniker(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwAssign,
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwWhichMoniker,
                    [Out, MarshalAs(UnmanagedType.Interface)]
                     out object moniker);

             [PreserveSig]
             int InitFromData(
                    [In, MarshalAs(UnmanagedType.Interface)]
                     IComDataObject pDataObject,

                     int fCreation,
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwReserved);

             [PreserveSig]
             int GetClipboardData(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwReserved,
                     out IComDataObject data);

             [PreserveSig]
             int DoVerb(

                     int iVerb,
                    [In]
                     IntPtr lpmsg,
                    [In, MarshalAs(UnmanagedType.Interface)]
                      UnsafeNativeMethods.IOleClientSite pActiveSite,

                     int lindex,

                     IntPtr hwndParent,
                    [In]
                     NativeMethods.COMRECT lprcPosRect);

             [PreserveSig]
             int EnumVerbs(out UnsafeNativeMethods.IEnumOLEVERB e);

             [PreserveSig]
             int OleUpdate();

             [PreserveSig]
             int IsUpToDate();

             [PreserveSig]
             int GetUserClassID(
                    [In, Out]
                      ref Guid pClsid);

             [PreserveSig]
             int GetUserType(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwFormOfType,
                    [Out, MarshalAs(UnmanagedType.LPWStr)]
                     out string userType);

             [PreserveSig]
             int SetExtent(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwDrawAspect,
                    [In]
                     NativeMethods.SIZE pSizel);

             [PreserveSig]
             int GetExtent(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwDrawAspect,
                    [Out]
                     NativeMethods.SIZE pSizel);

             [PreserveSig]
             int Advise(
                     IAdviseSink pAdvSink,
                     out int cookie);

             [PreserveSig]
             int Unadvise(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwConnection);

              [PreserveSig]
              int EnumAdvise(out IEnumSTATDATA e);

             [PreserveSig]
             int GetMiscStatus(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwAspect,
                     out int misc);

             [PreserveSig]
             int SetColorScheme(
                    [In]
                      NativeMethods.tagLOGPALETTE pLogpal);
        }

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("1C2056CC-5EF4-101B-8BC8-00AA003E3B29"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceObjectWindowless {

             [PreserveSig]
             int SetClientSite(
                    [In, MarshalAs(UnmanagedType.Interface)]
                      UnsafeNativeMethods.IOleClientSite pClientSite);

             [PreserveSig]
             int GetClientSite(out UnsafeNativeMethods.IOleClientSite site);

             [PreserveSig]
             int SetHostNames(
                    [In, MarshalAs(UnmanagedType.LPWStr)]
                      string szContainerApp,
                    [In, MarshalAs(UnmanagedType.LPWStr)]
                      string szContainerObj);

             [PreserveSig]
             int Close(

                     int dwSaveOption);

             [PreserveSig]
             int SetMoniker(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwWhichMoniker,
                    [In, MarshalAs(UnmanagedType.Interface)]
                     object pmk);

              [PreserveSig]
              int GetMoniker(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwAssign,
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwWhichMoniker,
                    [Out, MarshalAs(UnmanagedType.Interface)]
                     out object moniker);

             [PreserveSig]
             int InitFromData(
                    [In, MarshalAs(UnmanagedType.Interface)]
                     IComDataObject pDataObject,

                     int fCreation,
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwReserved);

             [PreserveSig]
             int GetClipboardData(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwReserved,
                     out IComDataObject data);

             [PreserveSig]
             int DoVerb(

                     int iVerb,
                    [In]
                     IntPtr lpmsg,
                    [In, MarshalAs(UnmanagedType.Interface)]
                      UnsafeNativeMethods.IOleClientSite pActiveSite,

                     int lindex,

                     IntPtr hwndParent,
                    [In]
                     NativeMethods.RECT lprcPosRect);

             [PreserveSig]
             int EnumVerbs(out UnsafeNativeMethods.IEnumOLEVERB e);

             [PreserveSig]
             int OleUpdate();

             [PreserveSig]
             int IsUpToDate();

             [PreserveSig]
             int GetUserClassID(
                    [In, Out]
                      ref Guid pClsid);

             [PreserveSig]
             int GetUserType(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwFormOfType,
                    [Out, MarshalAs(UnmanagedType.LPWStr)]
                     out string userType);

             [PreserveSig]
             int SetExtent(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwDrawAspect,
                    [In]
                     NativeMethods.SIZE pSizel);

             [PreserveSig]
             int GetExtent(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwDrawAspect,
                    [Out]
                     NativeMethods.SIZE pSizel);

             [PreserveSig]
             int Advise(
                    [In, MarshalAs(UnmanagedType.Interface)]
                     IAdviseSink pAdvSink,
                     out int cookie);

             [PreserveSig]
             int Unadvise(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwConnection);

              [PreserveSig]
                  int EnumAdvise(out IEnumSTATDATA e);

             [PreserveSig]
             int GetMiscStatus(
                    [In, MarshalAs(UnmanagedType.U4)]
                     int dwAspect,
                     out int misc);

             [PreserveSig]
             int SetColorScheme(
                    [In]
                      NativeMethods.tagLOGPALETTE pLogpal);

             [PreserveSig]
             int OnWindowMessage(
                [In, MarshalAs(UnmanagedType.U4)]  int msg,
                [In, MarshalAs(UnmanagedType.U4)]  int wParam,
                [In, MarshalAs(UnmanagedType.U4)]  int lParam,
                [Out, MarshalAs(UnmanagedType.U4)] int plResult);

             [PreserveSig]
             int GetDropTarget(
                [Out, MarshalAs(UnmanagedType.Interface)] object ppDropTarget);

        };

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [SecurityCritical( SecurityCriticalScope.Everything ) , SuppressUnmanagedCodeSecurity ]
        [ComImport(),
        Guid("B196B288-BAB4-101A-B69C-00AA00341D07"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleControl {


             [PreserveSig]
             int GetControlInfo(
                    [Out]
                      NativeMethods.tagCONTROLINFO pCI);

             [PreserveSig]
             int OnMnemonic(
                    [In]
                      ref System.Windows.Interop.MSG pMsg);

             [PreserveSig]
             int OnAmbientPropertyChange(

                     int dispID);

             [PreserveSig]
             int FreezeEvents(

                     int bFreeze);

        }

    ///<SecurityNote>
    ///     Critical - elevates via a SUC.
    ///</SecurityNote>
    [SecurityCritical( SecurityCriticalScope.Everything ) , SuppressUnmanagedCodeSecurity ]
    [ComImport(),
    Guid("B196B286-BAB4-101A-B69C-00AA00341D07"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IConnectionPoint {

        [PreserveSig]
        int GetConnectionInterface(out Guid iid);


        [PreserveSig]
        int GetConnectionPointContainer(
            [MarshalAs(UnmanagedType.Interface)]
            ref IConnectionPointContainer pContainer);


         [PreserveSig]
         int Advise(
                [In, MarshalAs(UnmanagedType.Interface)]
                  object pUnkSink,
              ref int cookie);


        [PreserveSig]
        int Unadvise(

                 int cookie);

        [PreserveSig]
        int EnumConnections(out object pEnum);

    }

     /// <SecurityNote>
     ///     Critical:Elevates to Unmanaged code permission
     /// </SecurityNote>
    
    [SuppressUnmanagedCodeSecurity]
    [ComImport(), Guid("00020404-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumVariant {
        /// <SecurityNote>
        ///    Critical: This code elevates to call unmanaged code
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [PreserveSig]
        int Next(
                [In, MarshalAs(UnmanagedType.U4)]
                 int celt,
                [In, Out]
                 IntPtr rgvar,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                 int[] pceltFetched);

         void Skip(
                [In, MarshalAs(UnmanagedType.U4)]
                 int celt);

         /// <SecurityNote>
         ///    Critical: This code elevates to call unmanaged code
         /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
         void Reset();

         void Clone(
                [Out, MarshalAs(UnmanagedType.LPArray)]
                   UnsafeNativeMethods.IEnumVariant[] ppenum);
    }

     /// <SecurityNote>
     ///     Critical:Elevates to Unmanaged code permission
     /// </SecurityNote>
    
    [SuppressUnmanagedCodeSecurity]
    [ComImport(), Guid("00000104-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumOLEVERB {


         [PreserveSig]
         int Next(
                [MarshalAs(UnmanagedType.U4)]
                int celt,
                [Out]
                NativeMethods.tagOLEVERB rgelt,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                int[] pceltFetched);

         [PreserveSig]
         int Skip(
                [In, MarshalAs(UnmanagedType.U4)]
                 int celt);


         void Reset();


         void Clone(
            out IEnumOLEVERB ppenum);


     }

     /// <SecurityNote>
     ///     Critical:Elevates to Unmanaged code permission
     /// </SecurityNote>
     
    [SuppressUnmanagedCodeSecurity]
     // This interface has different parameter marshaling from System.Runtime.InteropServices.ComTypes.IStream.
     // They are incompatable. But type cast will succeed because they have the same guid.
    [ComImport(), Guid("0000000C-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStream {

         int Read(

                 IntPtr buf,

                 int len);


         int Write(

                 IntPtr buf,

                 int len);

        [return: MarshalAs(UnmanagedType.I8)]
         long Seek(
                [In, MarshalAs(UnmanagedType.I8)]
                 long dlibMove,

                 int dwOrigin);


         void SetSize(
                [In, MarshalAs(UnmanagedType.I8)]
                 long libNewSize);

        [return: MarshalAs(UnmanagedType.I8)]
         long CopyTo(
                [In, MarshalAs(UnmanagedType.Interface)]
                  UnsafeNativeMethods.IStream pstm,
                [In, MarshalAs(UnmanagedType.I8)]
                 long cb,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                 long[] pcbRead);


         void Commit(

                 int grfCommitFlags);


         void Revert();


         void LockRegion(
                [In, MarshalAs(UnmanagedType.I8)]
                 long libOffset,
                [In, MarshalAs(UnmanagedType.I8)]
                 long cb,

                 int dwLockType);


         void UnlockRegion(
                [In, MarshalAs(UnmanagedType.I8)]
                 long libOffset,
                [In, MarshalAs(UnmanagedType.I8)]
                 long cb,

                 int dwLockType);


         void Stat(
                 [Out]
                 NativeMethods.STATSTG pStatstg,
                 int grfStatFlag);

        [return: MarshalAs(UnmanagedType.Interface)]
          UnsafeNativeMethods.IStream Clone();
    }


    ///<SecurityNote>
    ///     Critical - elevates via a SUC.
    ///</SecurityNote>
    [SecurityCritical( SecurityCriticalScope.Everything ) , SuppressUnmanagedCodeSecurity ]
    [ComImport(),
    Guid("B196B284-BAB4-101A-B69C-00AA00341D07"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IConnectionPointContainer
    {

        [return: MarshalAs(UnmanagedType.Interface)]
        object EnumConnectionPoints();

        [PreserveSig]
        int FindConnectionPoint([In] ref Guid guid, [Out, MarshalAs(UnmanagedType.Interface)]out IConnectionPoint ppCP);

    }

     /// <SecurityNote>
     ///     Critical:Elevates to Unmanaged code permission
     /// </SecurityNote>
    
    [SuppressUnmanagedCodeSecurity]
    [ComImport(), Guid("B196B285-BAB4-101A-B69C-00AA00341D07"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumConnectionPoints {
        [PreserveSig]
        int Next(int cConnections, out IConnectionPoint pCp, out int pcFetched);

        [PreserveSig]
        int Skip(int cSkip);

        void Reset();

        IEnumConnectionPoints Clone();
    }

#if !DRT && !UIAUTOMATIONTYPES
    /// <SecurityNote>
     ///     Critical:Elevates to Unmanaged code permission
     /// </SecurityNote>
    
    [SuppressUnmanagedCodeSecurity]
    [ComImport(), Guid("00020400-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDispatch {

    #region <KeepInSync With="IDispatchEx">

         int GetTypeInfoCount();

        [return: MarshalAs(UnmanagedType.Interface)]
         ITypeInfo GetTypeInfo(
                [In, MarshalAs(UnmanagedType.U4)]
                 int iTInfo,
                [In, MarshalAs(UnmanagedType.U4)]
                 int lcid);

         ///<SecurityNote>
         /// Critical elevates via a SUC.
         ///</SecurityNote>
         [SuppressUnmanagedCodeSecurity, SecurityCritical]
         [PreserveSig]
         HR GetIDsOfNames(
                [In]
                 ref Guid riid,
                [In, MarshalAs(UnmanagedType.LPArray)]
                 string[] rgszNames,
                [In, MarshalAs(UnmanagedType.U4)]
                 int cNames,
                [In, MarshalAs(UnmanagedType.U4)]
                 int lcid,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                 int[] rgDispId);


         ///<SecurityNote>
         /// Critical elevates via a SUC.
         ///</SecurityNote>
         [SuppressUnmanagedCodeSecurity, SecurityCritical]
         [PreserveSig]
         HR Invoke(

                 int dispIdMember,
                [In]
                 ref Guid riid,
                [In, MarshalAs(UnmanagedType.U4)]
                 int lcid,
                [In, MarshalAs(UnmanagedType.U4)]
                 int dwFlags,
                [Out, In]
                  NativeMethods.DISPPARAMS pDispParams,
                [Out]
                  out object pVarResult,
                [Out, In]
                  NativeMethods.EXCEPINFO pExcepInfo,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                  IntPtr [] pArgErr);

    #endregion

    }

     /// <SecurityNote>
     ///     Critical:Elevates to Unmanaged code permission
     /// </SecurityNote>
    
    [SuppressUnmanagedCodeSecurity]
    [ComImport(), Guid("A6EF9860-C720-11D0-9337-00A0C90DCAA9"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDispatchEx : IDispatch {

    #region <KeepInSync With="IDispatch">

         new int GetTypeInfoCount();

        [return: MarshalAs(UnmanagedType.Interface)]
         new ITypeInfo GetTypeInfo(
                [In, MarshalAs(UnmanagedType.U4)]
                 int iTInfo,
                [In, MarshalAs(UnmanagedType.U4)]
                 int lcid);

         ///<SecurityNote>
         /// Critical elevates via a SUC.
         ///</SecurityNote>
         [SuppressUnmanagedCodeSecurity, SecurityCritical]
         [PreserveSig]
         new HR GetIDsOfNames(
                [In]
                 ref Guid riid,
                [In, MarshalAs(UnmanagedType.LPArray)]
                 string[] rgszNames,
                [In, MarshalAs(UnmanagedType.U4)]
                 int cNames,
                [In, MarshalAs(UnmanagedType.U4)]
                 int lcid,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                 int[] rgDispId);


         ///<SecurityNote>
         /// Critical elevates via a SUC.
         ///</SecurityNote>
         [SuppressUnmanagedCodeSecurity, SecurityCritical]
         [PreserveSig]
         new HR Invoke(
                 int dispIdMember,
                [In]
                 ref Guid riid,
                [In, MarshalAs(UnmanagedType.U4)]
                 int lcid,
                [In, MarshalAs(UnmanagedType.U4)]
                 int dwFlags,
                [Out, In]
                  NativeMethods.DISPPARAMS pDispParams,
                [Out]
                  out object pVarResult,
                [Out, In]
                  NativeMethods.EXCEPINFO pExcepInfo,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                  IntPtr [] pArgErr);

    #endregion

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [PreserveSig]
        HR GetDispID(
            string name,
            int nameProperties,
            [Out] out int dispId);

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [PreserveSig]
        HR InvokeEx(
            int dispId,
            [MarshalAs(UnmanagedType.U4)] int lcid,
            [MarshalAs(UnmanagedType.U4)] int flags,
            [In, Out] NativeMethods.DISPPARAMS dispParams,
            [Out] out object result,
            /* COM interop caveat: Declaring the following just as Out seems to cause
               garbage being handed out for the native buffer (it's out anyway). Upon
               returning from the COM call, CLR copies back to the managed object but
               chokes on the garbage string pointers trying to do memcpy, causing AV.
               See also Dev10 work item 730339 to fix this in the CLR, by zeroing out
               the memory that's handed over to native code in this circumstance.  */
            [In, Out] NativeMethods.EXCEPINFO exceptionInfo,
            IServiceProvider serviceProvider);

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        void DeleteMemberByName(string name, int flags);

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        void DeleteMemberByDispID(int dispId);

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        int GetMemberProperties(int dispId, int propFlags);

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        string GetMemberName(int dispId);

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        int GetNextDispID(int enumFlags, int dispId);

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object GetNameSpaceParent();

    }

     /// <SecurityNote>
     ///     Critical:Elevates to Unmanaged code permission
     /// </SecurityNote>
    
    [SuppressUnmanagedCodeSecurity]
    [ComImport(), Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider {

        ///<SecurityNote>
        /// Critical elevates via a SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object QueryService(ref Guid service, ref Guid riid);

    }

#endif

    #region WebBrowser Related Definitions
        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E"),
        TypeLibType(TypeLibTypeFlags.FHidden | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FOleAutomation)]
        public interface IWebBrowser2
        {
            //
            // IWebBrowser members

            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [DispId(100)]
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void GoBack();

            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [DispId(101)]
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void GoForward();

            [DispId(102)]
            void GoHome();
            [DispId(103)]
            void GoSearch();
            [DispId(104)]
            void Navigate([In] string Url, [In] ref object flags,
              [In] ref object targetFrameName, [In] ref object postData,
              [In] ref object headers);

            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [DispId(-550)]
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void Refresh();

            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [DispId(105)]
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void Refresh2([In] ref object level);

            [DispId(106)]
            void Stop();
            [DispId(200)]
            object Application { [return: MarshalAs(UnmanagedType.IDispatch)]get;}
            [DispId(201)]
            object Parent { [return: MarshalAs(UnmanagedType.IDispatch)]get;}
            [DispId(202)]
            object Container { [return: MarshalAs(UnmanagedType.IDispatch)]get;}

            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [DispId(203)]
            object Document { [return: MarshalAs(UnmanagedType.IDispatch)]
                [SuppressUnmanagedCodeSecurity, SecurityCritical]
                get;}

            [DispId(204)]
            bool TopLevelContainer { get;}
            [DispId(205)]
            string Type { get;}
            [DispId(206)]
            int Left { get; set;}
            [DispId(207)]
            int Top { get; set;}
            [DispId(208)]
            int Width { get; set;}
            [DispId(209)]
            int Height { get; set;}
            [DispId(210)]
            string LocationName { get;}

            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [DispId(211)]
            string LocationURL {
                [SuppressUnmanagedCodeSecurity, SecurityCritical]
                get;}

            [DispId(212)]
            bool Busy { get;}
            //
            // IWebBrowserApp members
            [DispId(300)]
            void Quit();
            [DispId(301)]
            void ClientToWindow([Out]out int pcx, [Out]out int pcy);
            [DispId(302)]
            void PutProperty([In] string property, [In] object vtValue);
            [DispId(303)]
            object GetProperty([In] string property);
            [DispId(0)]
            string Name { get;}
            [DispId(-515)]
            int HWND { get;}
            [DispId(400)]
            string FullName { get;}
            [DispId(401)]
            string Path { get;}
            [DispId(402)]
            bool Visible { get; set;}
            [DispId(403)]
            bool StatusBar { get; set;}
            [DispId(404)]
            string StatusText { get; set;}
            [DispId(405)]
            int ToolBar { get; set;}
            [DispId(406)]
            bool MenuBar { get; set;}
            [DispId(407)]
            bool FullScreen { get; set;}

            //
            // IWebBrowser2 members
            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [DispId(500)]
            [SuppressUnmanagedCodeSecurity, SecurityCritical ]
            void Navigate2([In] ref object URL, [In] ref object flags,
              [In] ref object targetFrameName, [In] ref object postData,
              [In] ref object headers);

            [DispId(501)]
            UnsafeNativeMethods.OLECMDF QueryStatusWB([In] UnsafeNativeMethods.OLECMDID cmdID);
            [DispId(502)]
            void ExecWB([In] UnsafeNativeMethods.OLECMDID cmdID,
      [In] UnsafeNativeMethods.OLECMDEXECOPT cmdexecopt,
      ref object pvaIn,
      IntPtr pvaOut);
            [DispId(503)]
            void ShowBrowserBar([In] ref object pvaClsid, [In] ref object pvarShow,
      [In] ref object pvarSize);
            [DispId(-525)]
            NativeMethods.WebBrowserReadyState ReadyState { get;}
            [DispId(550)]
            bool Offline { get; set;}
            [DispId(551)]
            bool Silent { get; set;}
            [DispId(552)]
            bool RegisterAsBrowser { get; set;}
            [DispId(553)]
            bool RegisterAsDropTarget { get; set;}
            [DispId(554)]
            bool TheaterMode { get; set;}
            [DispId(555)]
            bool AddressBar { get; set;}
            [DispId(556)]
            bool Resizable { get; set;}
        }

        [ComImport(), Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch),
        TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DWebBrowserEvents2
        {
            [DispId(102)]
            void StatusTextChange([In] string text);
            [DispId(108)]
            void ProgressChange([In] int progress, [In] int progressMax);
            [DispId(105)]
            void CommandStateChange([In] long command, [In] bool enable);
            [DispId(106)]
            void DownloadBegin();
            [DispId(104)]
            void DownloadComplete();
            [DispId(113)]
            void TitleChange([In] string text);
            [DispId(112)]
            void PropertyChange([In] string szProperty);
            [DispId(225)]
            void PrintTemplateInstantiation([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp);
            [DispId(226)]
            void PrintTemplateTeardown([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp);
            [DispId(227)]
            void UpdatePageStatus([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp,
                [In] ref object nPage, [In] ref object fDone);
            [DispId(250)]
            void BeforeNavigate2([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp,
                   [In] ref object URL, [In] ref object flags,
                   [In] ref object targetFrameName, [In] ref object postData,
                   [In] ref object headers, [In, Out] ref bool cancel);
            [DispId(251)]
            void NewWindow2([In, Out, MarshalAs(UnmanagedType.IDispatch)] ref object pDisp,
                  [In, Out] ref bool cancel);
            [DispId(252)]
            void NavigateComplete2([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp,
                  [In] ref object URL);
            [DispId(259)]
            void DocumentComplete([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp,
                  [In] ref object URL);
            [DispId(253)]
            void OnQuit();
            [DispId(254)]
            void OnVisible([In] bool visible);
            [DispId(255)]
            void OnToolBar([In] bool toolBar);
            [DispId(256)]
            void OnMenuBar([In] bool menuBar);
            [DispId(257)]
            void OnStatusBar([In] bool statusBar);
            [DispId(258)]
            void OnFullScreen([In] bool fullScreen);
            [DispId(260)]
            void OnTheaterMode([In] bool theaterMode);
            [DispId(262)]
            void WindowSetResizable([In] bool resizable);
            [DispId(264)]
            void WindowSetLeft([In] int left);
            [DispId(265)]
            void WindowSetTop([In] int top);
            [DispId(266)]
            void WindowSetWidth([In] int width);
            [DispId(267)]
            void WindowSetHeight([In] int height);
            [DispId(263)]
            void WindowClosing([In] bool isChildWindow, [In, Out] ref bool cancel);
            [DispId(268)]
            void ClientToHostWindow([In, Out] ref long cx, [In, Out] ref long cy);
            [DispId(269)]
            void SetSecureLockIcon([In] int secureLockIcon);
            [DispId(270)]
            void FileDownload([In, Out] ref bool ActiveDocument, [In, Out] ref bool cancel);
            [DispId(271)]
            void NavigateError([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp,
                [In] ref object URL, [In] ref object frame, [In] ref object statusCode, [In, Out] ref bool cancel);
            [DispId(272)]
            void PrivacyImpactedStateChange([In] bool bImpacted);
            [DispId(282)] // IE 7+
            void SetPhishingFilterStatus(uint phishingFilterStatus);
            [DispId(283)] // IE 7+
            void WindowStateChanged(uint dwFlags, uint dwValidFlagsMask);
        }


        // Used to control the webbrowser appearance and provide DTE to script via window.external
        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ ComImport(), Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDocHostUIHandler
        {

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowContextMenu(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwID,
                [In]
                NativeMethods.POINT pt,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pcmdtReserved,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pdispReserved);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetHostInfo(
                [In, Out]
                NativeMethods.DOCHOSTUIINFO info);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowUI(
                [In, MarshalAs(UnmanagedType.I4)]
                int dwID,
                [In]
                UnsafeNativeMethods.IOleInPlaceActiveObject activeObject,
                [In]
                NativeMethods.IOleCommandTarget commandTarget,
                [In]
                UnsafeNativeMethods.IOleInPlaceFrame frame,
                [In]
                UnsafeNativeMethods.IOleInPlaceUIWindow doc);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int HideUI();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int UpdateUI();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int EnableModeless(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool fEnable);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnDocWindowActivate(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool fActivate);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnFrameWindowActivate(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool fActivate);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ResizeBorder(
                [In]
                NativeMethods.COMRECT rect,
                [In]
                UnsafeNativeMethods.IOleInPlaceUIWindow doc,
                bool fFrameWindow);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator(
                [In]
                ref System.Windows.Interop.MSG msg,
                [In]
                ref Guid group,
                [In, MarshalAs(UnmanagedType.I4)]
                int nCmdID);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetOptionKeyPath(
                [Out, MarshalAs(UnmanagedType.LPArray)]
                String[] pbstrKey,
                [In, MarshalAs(UnmanagedType.U4)]
                int dw);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetDropTarget(
                [In, MarshalAs(UnmanagedType.Interface)]
                UnsafeNativeMethods.IOleDropTarget pDropTarget,
                [Out, MarshalAs(UnmanagedType.Interface)]
                out UnsafeNativeMethods.IOleDropTarget ppDropTarget);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetExternal(
                [Out, MarshalAs(UnmanagedType.IDispatch)]
                out object ppDispatch);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateUrl(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwTranslate,
                [In, MarshalAs(UnmanagedType.LPWStr)]
                string strURLIn,
                [Out, MarshalAs(UnmanagedType.LPWStr)]
                out string pstrURLOut);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int FilterDataObject(
                IComDataObject pDO,
                out IComDataObject ppDORet);


        }

        ///<SecurityNote>
        /// Critical: elevates via SUC.
        ///</SecurityNote>
        [ComImport, Guid("3050F21F-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        
        [SuppressUnmanagedCodeSecurity]
        internal interface IHTMLElementCollection
        {
            string toString();
            void SetLength(int p);
            int GetLength();
            [return: MarshalAs(UnmanagedType.Interface)]
            object Get_newEnum();
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object Item(object idOrName, object index);
            [return: MarshalAs(UnmanagedType.Interface)]
            object Tags(object tagName);
        };

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport, Guid("626FC520-A41E-11CF-A731-00A0C9082637"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLDocument
        {
            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetScript();

        }

        ///<SecurityNote>
        /// Critical: elevates via SUC.
        ///     If the document is not cross-domain relative to the host application, all methods on this interface
        ///     can be considered 'safe for scripting'.
        ///</SecurityNote>
        [ComImport, Guid("332C4425-26CB-11D0-B483-00C04FD90119"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical(SecurityCriticalScope.Everything)]
        internal interface IHTMLDocument2: IHTMLDocument
        {
            #region IHTMLDocument - base interface
            [return: MarshalAs(UnmanagedType.Interface)]
            new object GetScript();
            #endregion
            IHTMLElementCollection GetAll();
            [return: MarshalAs(UnmanagedType.Interface)]
            /*IHTMLElement*/object GetBody();
            [return: MarshalAs(UnmanagedType.Interface)]
            /*IHTMLElement*/object GetActiveElement();
            IHTMLElementCollection GetImages();
            IHTMLElementCollection GetApplets();
            IHTMLElementCollection GetLinks();
            IHTMLElementCollection GetForms();
            IHTMLElementCollection GetAnchors();
            void SetTitle(string p);
            string GetTitle();
            IHTMLElementCollection GetScripts();
            void SetDesignMode(string p);
            string GetDesignMode();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetSelection();
            string GetReadyState();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetFrames();
            IHTMLElementCollection GetEmbeds();
            IHTMLElementCollection GetPlugins();
            void SetAlinkColor(object c);
            object GetAlinkColor();
            void SetBgColor(object c);
            object GetBgColor();
            void SetFgColor(object c);
            object GetFgColor();
            void SetLinkColor(object c);
            object GetLinkColor();
            void SetVlinkColor(object c);
            object GetVlinkColor();
            string GetReferrer();
            IHTMLLocation GetLocation();
            string GetLastModified();
            void SetUrl(string p);
            string GetUrl();
            void SetDomain(string p);
            string GetDomain();
            void SetCookie(string p);
            string GetCookie();
            void SetExpando(bool p);
            bool GetExpando();
            void SetCharset(string p);
            string GetCharset();
            void SetDefaultCharset(string p);
            string GetDefaultCharset();
            string GetMimeType();
            string GetFileSize();
            string GetFileCreatedDate();
            string GetFileModifiedDate();
            string GetFileUpdatedDate();
            string GetSecurity();
            string GetProtocol();
            string GetNameProp();
            int Write([In, MarshalAs(UnmanagedType.SafeArray)] object[] psarray);
            int WriteLine([In, MarshalAs(UnmanagedType.SafeArray)] object[] psarray);
            [return: MarshalAs(UnmanagedType.Interface)]
            object Open(string mimeExtension, object name, object features, object replace);
            void Close();
            void Clear();
            bool QueryCommandSupported(string cmdID);
            bool QueryCommandEnabled(string cmdID);
            bool QueryCommandState(string cmdID);
            bool QueryCommandIndeterm(string cmdID);
            string QueryCommandText(string cmdID);
            object QueryCommandValue(string cmdID);
            bool ExecCommand(string cmdID, bool showUI, object value);
            bool ExecCommandShowHelp(string cmdID);
            [return: MarshalAs(UnmanagedType.Interface)]
            /*IHTMLElement*/object CreateElement(string eTag);
            void SetOnhelp(object p);
            object GetOnhelp();
            void SetOnclick(object p);
            object GetOnclick();
            void SetOndblclick(object p);
            object GetOndblclick();
            void SetOnkeyup(object p);
            object GetOnkeyup();
            void SetOnkeydown(object p);
            object GetOnkeydown();
            void SetOnkeypress(object p);
            object GetOnkeypress();
            void SetOnmouseup(object p);
            object GetOnmouseup();
            void SetOnmousedown(object p);
            object GetOnmousedown();
            void SetOnmousemove(object p);
            object GetOnmousemove();
            void SetOnmouseout(object p);
            object GetOnmouseout();
            void SetOnmouseover(object p);
            object GetOnmouseover();
            void SetOnreadystatechange(object p);
            object GetOnreadystatechange();
            void SetOnafterupdate(object p);
            object GetOnafterupdate();
            void SetOnrowexit(object p);
            object GetOnrowexit();
            void SetOnrowenter(object p);
            object GetOnrowenter();
            void SetOndragstart(object p);
            object GetOndragstart();
            void SetOnselectstart(object p);
            object GetOnselectstart();
            [return: MarshalAs(UnmanagedType.Interface)]
            /*IHTMLElement*/object ElementFromPoint(int x, int y);
            [return: MarshalAs(UnmanagedType.Interface)]
            /*IHTMLWindow2*/object GetParentWindow();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetStyleSheets();
            void SetOnbeforeupdate(object p);
            object GetOnbeforeupdate();
            void SetOnerrorupdate(object p);
            object GetOnerrorupdate();
            string toString();
            [return: MarshalAs(UnmanagedType.Interface)]
            object CreateStyleSheet(string bstrHref, int lIndex);
        };

        ///<SecurityNote>
        /// Critical: elevates via SUC.
        ///</SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("163BB1E0-6E00-11CF-837A-48DC04C10000")]
        internal interface IHTMLLocation
        {
            void SetHref(string p);
            string GetHref();
            void SetProtocol(string p);
            string GetProtocol();
            void SetHost(string p);
            string GetHost();
            void SetHostname(string p);
            string GetHostname();
            void SetPort(string p);
            string GetPort();
            void SetPathname(string p);
            string GetPathname();
            void SetSearch(string p);
            string GetSearch();
            void SetHash(string p);
            string GetHash();
            void Reload(bool flag);
            void Replace(string bstr);
            void Assign(string bstr);
        };

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport, Guid("3050f6cf-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLWindow4
        {
            [return: MarshalAs(UnmanagedType.IDispatch)] object CreatePopup([In] ref object reserved);
            [return: MarshalAs(UnmanagedType.Interface)] object frameElement();
        }

        internal static class ArrayToVARIANTHelper
        {
            ///<SecurityNote>
            /// Critical - Calls Marshal.OffsetOf(), which has a LinkDemand for unmanaged code.
            /// TreatAsSafe - This is not exploitable.
            ///</SecurityNote>
            [SecuritySafeCritical]
            static ArrayToVARIANTHelper()
            {
                VariantSize = (int)Marshal.OffsetOf(typeof(FindSizeOfVariant), "b");
            }

            // Convert a object[] into an array of VARIANT, allocated with CoTask allocators.
            /// <SecurityNote>
            /// Critical: Calls Marshal.GetNativeVariantForObject(), which has a LinkDemand for unmanaged code.
            /// </SecurityNote>
            [SecurityCritical]
            public unsafe static IntPtr ArrayToVARIANTVector(object[] args)
            {
                IntPtr mem = IntPtr.Zero;
                int i = 0;
                try
                {
                    checked
                    {
                        int len = args.Length;
                        mem = Marshal.AllocCoTaskMem(len * VariantSize);
                        byte* a = (byte*)(void*)mem;
                        for (i = 0; i < len; ++i)
                        {
                            Marshal.GetNativeVariantForObject(args[i], (IntPtr)(a + VariantSize * i));
                        }
                    }
                }
                catch
                {
                    if (mem != IntPtr.Zero)
                    {
                        FreeVARIANTVector(mem, i);
                    }
                    throw;
                }
                return mem;
            }

            // Free a Variant array created with the above function
            /// <SecurityNote>
            /// Critical: Calls Marshal.FreeCoTaskMem(), which has a LinkDemand for unmanaged code.
            /// </SecurityNote>
            /// <param name="mem">The allocated memory to be freed.</param>
            /// <param name="len">The length of the Variant vector to be cleared.</param>
            [SecurityCritical]
            public unsafe static void FreeVARIANTVector(IntPtr mem, int len)
            {
                int hr = NativeMethods.S_OK;
                byte* a = (byte*)(void*)mem;

                for (int i = 0; i < len; ++i)
                {
                    int hrcurrent = NativeMethods.S_OK;
                    checked
                    {
                        hrcurrent = UnsafeNativeMethods.VariantClear((IntPtr)(a + VariantSize * i));
                    }

                    // save the first error and throw after we finish all VariantClear.
                    if (NativeMethods.Succeeded(hr) && NativeMethods.Failed(hrcurrent))
                    {
                        hr = hrcurrent;
                    }
                }
                Marshal.FreeCoTaskMem(mem);

                if (NativeMethods.Failed(hr))
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct FindSizeOfVariant
            {
                [MarshalAs(UnmanagedType.Struct)]
                public object var;
                public byte b;
            }

            private static readonly int VariantSize;
        }

        /// <SecurityNote>
        /// Critical - This code causes unmanaged code elevation.
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport(ExternDll.Oleaut32, PreserveSig=true)]
        private static extern int VariantClear(IntPtr pObject);

        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComImport(), Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPersistStreamInit
        {
            void GetClassID(
                   [Out]
                  out Guid pClassID);

            [PreserveSig]
            int IsDirty();

            ///<SecurityNote>
            /// Critical elevates via a SUC.
            ///</SecurityNote>
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void Load(
                   [In, MarshalAs(UnmanagedType.Interface)]
                  System.Runtime.InteropServices.ComTypes.IStream pstm);

            void Save(
                   [In, MarshalAs(UnmanagedType.Interface)]
                      IStream pstm,
                   [In, MarshalAs(UnmanagedType.Bool)]
                     bool fClearDirty);

            void GetSizeMax(
                   [Out, MarshalAs(UnmanagedType.LPArray)]
                 long pcbSize);

            void InitNew();
        }

        [Flags]
        internal enum BrowserNavConstants : uint
        {
            OpenInNewWindow = 0x00000001,
            NoHistory = 0x00000002,
            NoReadFromCache = 0x00000004,
            NoWriteToCache = 0x00000008,
            AllowAutosearch = 0x00000010,
            BrowserBar = 0x00000020,
            Hyperlink = 0x00000040,
            EnforceRestricted = 0x00000080,
            NewWindowsManaged = 0x00000100,
            UntrustedForDownload = 0x00000200,
            TrustedForActiveX = 0x00000400,
            OpenInNewTab = 0x00000800,
            OpenInBackgroundTab = 0x00001000,
            KeepWordWheelText = 0x00002000
        }
#if never
        //
        // Used to control the webbrowser security
        /// <SecurityNote>
        ///     Critical:Elevates to Unmanaged code permission
        /// </SecurityNote>
        
        [SuppressUnmanagedCodeSecurity]
        [ComVisible(true), ComImport(), Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false)]
        public interface IInternetSecurityManager {
            [PreserveSig] int SetSecuritySite();
            [PreserveSig] int GetSecuritySite();
            [PreserveSig] int MapUrlToZone();
            [PreserveSig] int GetSecurityId();
            [PreserveSig] int ProcessUrlAction(string url, int action,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] policy,
                    int cbPolicy, ref byte context, int cbContext,
                    int flags, int reserved);
            [PreserveSig] int QueryCustomPolicy();
            [PreserveSig] int SetZoneMapping();
            [PreserveSig] int GetZoneMappings();
        }
#endif
    #endregion WebBrowser Related Definitions

        /// <SecurityNote>
        ///     Critical: as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern uint GetRawInputDeviceList(
                                                [In, Out] NativeMethods.RAWINPUTDEVICELIST[] ridl,
                                                [In, Out] ref uint numDevices,
                                                uint sizeInBytes);

        /// <SecurityNote>
        ///     Critical: as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern uint GetRawInputDeviceInfo(
                                                IntPtr hDevice,
                                                uint command,
                                                [In] ref NativeMethods.RID_DEVICE_INFO ridInfo,
                                                ref uint sizeInBytes);


    }
}
