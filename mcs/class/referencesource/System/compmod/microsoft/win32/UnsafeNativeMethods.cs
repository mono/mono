//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Win32 {
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if !SILVERLIGHT
    using System.Threading;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.Configuration;	

    [HostProtectionAttribute(MayLeakOnAbort = true)]
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif // !SILVERLIGHT
    internal static class UnsafeNativeMethods {

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern IntPtr GetStdHandle(int type);

#if !FEATURE_PAL && !FEATURE_CORESYSTEM
        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "reviewed")]
        [DllImport(ExternDll.Gdi32, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetSystemMetrics(int nIndex);
#endif // !FEATURE_PAL && !FEATURE_CORESYSTEM

#if !SILVERLIGHT       
        [DllImport(ExternDll.User32, ExactSpelling=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetProcessWindowStation();
        [DllImport(ExternDll.User32, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetUserObjectInformation(HandleRef hObj, int nIndex, [MarshalAs(UnmanagedType.LPStruct)] NativeMethods.USEROBJECTFLAGS pvBuffer, int nLength, ref int lpnLengthNeeded);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern IntPtr GetModuleHandle(string modName);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetClassInfo(HandleRef hInst, string lpszClass, [In, Out] NativeMethods.WNDCLASS_I wc);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool IsWindow(HandleRef hWnd);

        //SetClassLong won't work correctly for 64-bit: we should use SetClassLongPtr instead.  On
        //32-bit, SetClassLongPtr is just #defined as SetClassLong.  SetClassLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr SetClassLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong) {
            if (IntPtr.Size == 4) {
                return SetClassLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
        }
        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, EntryPoint = "SetClassLong")]
        [ResourceExposure(ResourceScope.None)]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        public static extern IntPtr SetClassLongPtr32(HandleRef hwnd, int nIndex, IntPtr dwNewLong);
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, EntryPoint = "SetClassLongPtr")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetClassLongPtr64(HandleRef hwnd, int nIndex, IntPtr dwNewLong);

        //SetWindowLong won't work correctly for 64-bit: we should use SetWindowLongPtr instead.  On
        //32-bit, SetWindowLongPtr is just #defined as SetWindowLong.  SetWindowLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, HandleRef dwNewLong) 
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, HandleRef dwNewLong);


        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern short RegisterClass(NativeMethods.WNDCLASS wc);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern short UnregisterClass(string lpClassName, HandleRef hInstance);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr CreateWindowEx(int exStyle, string lpszClassName, string lpszWindowName, int style, int x, int y, int width,
                                              int height, HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool SetConsoleCtrlHandler(NativeMethods.ConHndlr handler, int add);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool DestroyWindow(HandleRef hWnd);

        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int MsgWaitForMultipleObjectsEx(int nCount, IntPtr pHandles, int dwMilliseconds, int dwWakeMask, int dwFlags);

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int DispatchMessage([In] ref NativeMethods.MSG msg);

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool PeekMessage([In, Out] ref NativeMethods.MSG msg, HandleRef hwnd, int msgMin, int msgMax, int remove);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetTimer(HandleRef hWnd, HandleRef nIDEvent, int uElapse, HandleRef lpTimerProc);

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool KillTimer(HandleRef hwnd, HandleRef idEvent);

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool TranslateMessage([In, Out] ref NativeMethods.MSG msg);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Ansi, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetProcAddress(HandleRef hModule, string lpProcName);

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport(ExternDll.Wtsapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool WTSRegisterSessionNotification(HandleRef hWnd, int dwFlags);

        [DllImport(ExternDll.Wtsapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool WTSUnRegisterSessionNotification(HandleRef hWnd);

        private const int ERROR_INSUFFICIENT_BUFFER = 0x007A;
        private const int ERROR_NO_PACKAGE_IDENTITY = 0x3d54;

        // AppModel.h functions (Win8+)
        [DllImport(ExternDll.Kernel32, CharSet = CharSet.None, EntryPoint = "GetCurrentPackageId")]
        [System.Security.SecuritySafeCritical]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern Int32 _GetCurrentPackageId(ref Int32 pBufferLength, Byte[] pBuffer);

        // Copied from Win32Native.cs
        // Note - do NOT use this to call methods.  Use P/Invoke, which will
        // do much better things w.r.t. marshaling, pinning memory, security 
        // stuff, better interactions with thread aborts, etc.  This is used
        // solely by DoesWin32MethodExist for avoiding try/catch EntryPointNotFoundException
        // in scenarios where an OS Version check is insufficient
        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Ansi, BestFitMapping=false, SetLastError=true, ExactSpelling=true)]
        [ResourceExposure(ResourceScope.None)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, String methodName);

        [System.Security.SecurityCritical]  // auto-generated
        private static bool DoesWin32MethodExist(String moduleName, String methodName)
        {
            // GetModuleHandle does not increment the module's ref count, so we don't need to call FreeLibrary.
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule == IntPtr.Zero) {
                Debug.Assert(hModule != IntPtr.Zero, "GetModuleHandle failed.  Dll isn't loaded?");
                return false;
            }
            IntPtr functionPointer = GetProcAddress(hModule, methodName);
            return (functionPointer != IntPtr.Zero);       
        }
        
        [System.Security.SecuritySafeCritical]
        private static bool _IsPackagedProcess()
        {
            OperatingSystem os = Environment.OSVersion;
            if(os.Platform == PlatformID.Win32NT && os.Version >= new Version(6,2,0,0) && DoesWin32MethodExist(ExternDll.Kernel32, "GetCurrentPackageId"))
            {
                Int32 bufLen = 0;
                // Will return ERROR_INSUFFICIENT_BUFFER when running within a packaged application,
                // and will return ERROR_NO_PACKAGE_IDENTITY otherwise.
                return _GetCurrentPackageId(ref bufLen, null) == ERROR_INSUFFICIENT_BUFFER;
            }
            else
            {   // We must be running on a downlevel OS.
                return false;
            }
        }

        [System.Security.SecuritySafeCritical]
        internal static Lazy<bool> IsPackagedProcess = new Lazy<bool>(() => _IsPackagedProcess());


        // File src\services\system\io\unsafenativemethods.cs

        public const int FILE_READ_DATA = (0x0001),
        FILE_LIST_DIRECTORY = (0x0001),
        FILE_WRITE_DATA = (0x0002),
        FILE_ADD_FILE = (0x0002),
        FILE_APPEND_DATA = (0x0004),
        FILE_ADD_SUBDIRECTORY = (0x0004),
        FILE_CREATE_PIPE_INSTANCE = (0x0004),
        FILE_READ_EA = (0x0008),
        FILE_WRITE_EA = (0x0010),
        FILE_EXECUTE = (0x0020),
        FILE_TRAVERSE = (0x0020),
        FILE_DELETE_CHILD = (0x0040),
        FILE_READ_ATTRIBUTES = (0x0080),
        FILE_WRITE_ATTRIBUTES = (0x0100),
        FILE_SHARE_READ = 0x00000001,
        FILE_SHARE_WRITE = 0x00000002,
        FILE_SHARE_DELETE = 0x00000004,
        FILE_ATTRIBUTE_READONLY = 0x00000001,
        FILE_ATTRIBUTE_HIDDEN = 0x00000002,
        FILE_ATTRIBUTE_SYSTEM = 0x00000004,
        FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
        FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
        FILE_ATTRIBUTE_NORMAL = 0x00000080,
        FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
        FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
        FILE_ATTRIBUTE_OFFLINE = 0x00001000,
        FILE_NOTIFY_CHANGE_FILE_NAME = 0x00000001,
        FILE_NOTIFY_CHANGE_DIR_NAME = 0x00000002,
        FILE_NOTIFY_CHANGE_ATTRIBUTES = 0x00000004,
        FILE_NOTIFY_CHANGE_SIZE = 0x00000008,
        FILE_NOTIFY_CHANGE_LAST_WRITE = 0x00000010,
        FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x00000020,
        FILE_NOTIFY_CHANGE_CREATION = 0x00000040,
        FILE_NOTIFY_CHANGE_SECURITY = 0x00000100,
        FILE_ACTION_ADDED = 0x00000001,
        FILE_ACTION_REMOVED = 0x00000002,
        FILE_ACTION_MODIFIED = 0x00000003,
        FILE_ACTION_RENAMED_OLD_NAME = 0x00000004,
        FILE_ACTION_RENAMED_NEW_NAME = 0x00000005,
        FILE_CASE_SENSITIVE_SEARCH = 0x00000001,
        FILE_CASE_PRESERVED_NAMES = 0x00000002,
        FILE_UNICODE_ON_DISK = 0x00000004,
        FILE_PERSISTENT_ACLS = 0x00000008,
        FILE_FILE_COMPRESSION = 0x00000010,
        OPEN_EXISTING = 3,
        OPEN_ALWAYS = 4,
        FILE_FLAG_WRITE_THROUGH = unchecked((int)0x80000000),
        FILE_FLAG_OVERLAPPED = 0x40000000,
        FILE_FLAG_NO_BUFFERING = 0x20000000,
        FILE_FLAG_RANDOM_ACCESS = 0x10000000,
        FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
        FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
        FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
        FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
        FILE_TYPE_UNKNOWN = 0x0000,
        FILE_TYPE_DISK = 0x0001,
        FILE_TYPE_CHAR = 0x0002,
        FILE_TYPE_PIPE = 0x0003,
        FILE_TYPE_REMOTE = unchecked((int)0x8000),
        FILE_VOLUME_IS_COMPRESSED = 0x00008000;

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "reviewed")]
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public extern static int LookupAccountSid(string systemName, byte[] pSid, StringBuilder szUserName, ref int userNameSize, StringBuilder szDomainName, ref int domainNameSize, ref int eUse);

        public const int GetFileExInfoStandard = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FILE_ATTRIBUTE_DATA {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint fileSizeHigh;
            internal uint fileSizeLow;
        }

        // file Windows Forms
        [DllImport(ExternDll.Version, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int GetFileVersionInfoSize(string lptstrFilename, out int handle);
        [DllImport(ExternDll.Version, CharSet=CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool GetFileVersionInfo(string lptstrFilename, int dwHandle, int dwLen, HandleRef lpData);
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
        [SuppressMessage("Microsoft.Security", "CA2101:SpecifyMarshalingForPInvokeStringArguments")]
        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        [DllImport(ExternDll.Version, CharSet=CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Process)]   // Review usages to see if versioning problems exist in distant callers
        public static extern bool VerQueryValue(HandleRef pBlock, string lpSubBlock, [In, Out] ref IntPtr lplpBuffer, out int len);
        [DllImport(ExternDll.Version, CharSet=CharSet.Auto, BestFitMapping=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int VerLanguageName( int langID, StringBuilder lpBuffer, int nSize);

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool ReportEvent(SafeHandle hEventLog, short type, ushort category,
                                                uint eventID, byte[] userSID, short numStrings, int dataLen, HandleRef strings,
                                                byte[] rawData);
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool ClearEventLog(SafeHandle hEventLog, HandleRef lpctstrBackupFileName);
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetNumberOfEventLogRecords(SafeHandle hEventLog, out int count);
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "[....]: EventLog is protected by EventLogPermission")]
        public static extern bool GetOldestEventLogRecord(SafeHandle hEventLog, out int number);
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "[....]: EventLog is protected by EventLogPermission")]
        public static extern bool ReadEventLog(SafeHandle hEventLog, int dwReadFlags,
                                                 int dwRecordOffset, byte[] buffer, int numberOfBytesToRead, out int bytesRead,
                                                 out int minNumOfBytesNeeded);
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "[....]: EventLog is protected by EventLogPermission")]
        public static extern bool NotifyChangeEventLog(SafeHandle hEventLog, SafeWaitHandle hEvent);

        [DllImport(ExternDll.Kernel32, EntryPoint="ReadDirectoryChangesW", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern bool ReadDirectoryChangesW(SafeFileHandle hDirectory, HandleRef lpBuffer,
                                                                int nBufferLength, int bWatchSubtree, int dwNotifyFilter, out int lpBytesReturned,
                                                                NativeOverlapped* overlappedPointer, HandleRef lpCompletionRoutine);

        //////////////////// Serial Port structs ////////////////////
        // Declaration for C# representation of Win32 Device Control Block (DCB)
        // structure.  Note that all flag properties are encapsulated in the Flags field here,
        // and accessed/set through SerialStream's GetDcbFlag(...) and SetDcbFlag(...) methods.
        internal struct DCB
        {

            public uint DCBlength;
            public uint BaudRate;
            public uint Flags;
            public ushort wReserved;
            public ushort XonLim;
            public ushort XoffLim;
            public byte ByteSize;
            public byte Parity;
            public byte StopBits;
            public byte XonChar;
            public byte XoffChar;
            public byte ErrorChar;
            public byte EofChar;
            public byte EvtChar;
            public ushort wReserved1;
        }

        // Declaration for C# representation of Win32 COMSTAT structure associated with
        // a file handle to a serial communications resource.  SerialStream's
        // InBufferBytes and OutBufferBytes directly expose cbInQue and cbOutQue to reading, respectively.
        internal struct COMSTAT
        {
            public uint Flags;
            public uint cbInQue;
            public uint cbOutQue;
        }

        // Declaration for C# representation of Win32 COMMTIMEOUTS
        // structure associated with a file handle to a serial communications resource.
        ///Currently the only set fields are ReadTotalTimeoutConstant
        // and WriteTotalTimeoutConstant.
        internal struct COMMTIMEOUTS
        {
            public int ReadIntervalTimeout;
            public int ReadTotalTimeoutMultiplier;
            public int ReadTotalTimeoutConstant;
            public int WriteTotalTimeoutMultiplier;
            public int WriteTotalTimeoutConstant;
        }

        // Declaration for C# representation of Win32 COMMPROP
        // structure associated with a file handle to a serial communications resource.
        // Currently the only fields used are dwMaxTxQueue, dwMaxRxQueue, and dwMaxBaud
        // to ensure that users provide appropriate settings to the SerialStream constructor.
        internal struct COMMPROP
        {
            public ushort  wPacketLength;
            public ushort  wPacketVersion;
            public int dwServiceMask;
            public int dwReserved1;
            public int dwMaxTxQueue;
            public int dwMaxRxQueue;
            public int dwMaxBaud;
            public int dwProvSubType;
            public int dwProvCapabilities;
            public int dwSettableParams;
            public int dwSettableBaud;
            public ushort wSettableData;
            public ushort  wSettableStopParity;
            public int dwCurrentTxQueue;
            public int dwCurrentRxQueue;
            public int dwProvSpec1;
            public int dwProvSpec2;
            public char wcProvChar;
        }
        //////////////////// end Serial Port structs ////////////////////

        //////////////////// Serial Port methods ////////////////////

        

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeFileHandle CreateFile(String lpFileName,
            int dwDesiredAccess, int dwShareMode,
            IntPtr securityAttrs, int dwCreationDisposition,
            int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetCommState(
            SafeFileHandle hFile,  // handle to communications device
            ref DCB lpDCB    // device-control block
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool SetCommState(
            SafeFileHandle hFile,  // handle to communications device
            ref DCB lpDCB    // device-control block
            );


        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetCommModemStatus(
            SafeFileHandle hFile,        // handle to communications device
            ref int lpModemStat  // control-register values
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool SetupComm(
            SafeFileHandle hFile,     // handle to communications device
            int dwInQueue,  // size of input buffer
            int dwOutQueue  // size of output buffer
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool SetCommTimeouts(
            SafeFileHandle hFile,                  // handle to comm device
            ref COMMTIMEOUTS lpCommTimeouts  // time-out values
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool SetCommBreak(
            SafeFileHandle hFile                 // handle to comm device
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool ClearCommBreak(
            SafeFileHandle hFile                 // handle to comm device
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool ClearCommError(
            SafeFileHandle hFile,                 // handle to comm device
            ref int lpErrors,
            ref COMSTAT lpStat
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool ClearCommError(
            SafeFileHandle hFile,                 // handle to comm device
            ref int lpErrors,
            IntPtr lpStat
            );
        
        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool PurgeComm(
            SafeFileHandle hFile,  // handle to communications resource
            uint dwFlags  // action to perform
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hFile);
    
        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetCommProperties(
            SafeFileHandle hFile,           // handle to comm device
            ref COMMPROP lpCommProp   // communications properties
            );

        // All actual file Read/Write methods, which are declared to be unsafe.
        [DllImport(ExternDll.Kernel32, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead, NativeOverlapped* overlapped);

        [DllImport(ExternDll.Kernel32, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr overlapped);

        [DllImport(ExternDll.Kernel32 , SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten, NativeOverlapped* lpOverlapped);

        [DllImport(ExternDll.Kernel32 , SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr lpOverlapped);

        [DllImport(ExternDll.Kernel32 , SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int GetFileType(
            SafeFileHandle hFile   // handle to file
            );
        [DllImport(ExternDll.Kernel32 , SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool EscapeCommFunction(
            SafeFileHandle hFile, // handle to communications device
            int dwFunc      // extended function to perform
            );

        [DllImport(ExternDll.Kernel32 , SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern bool WaitCommEvent(
            SafeFileHandle hFile,                // handle to comm device
            int* lpEvtMask,                      // event type
            NativeOverlapped* lpOverlapped       // overlapped structure
            );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern bool SetCommMask(
            SafeFileHandle hFile,
            int dwEvtMask
        );

        [DllImport(ExternDll.Kernel32, SetLastError=true, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern bool GetOverlappedResult(
            SafeFileHandle hFile,
            NativeOverlapped* lpOverlapped,
            ref int lpNumberOfBytesTransferred,
            bool bWait
        );


      //////////////////// end Serial Port methods ////////////////////

        [DllImport(ExternDll.Advapi32, CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern 
        bool GetTokenInformation (
            [In]  IntPtr                TokenHandle,
            [In]  uint                  TokenInformationClass,
            [In]  IntPtr                TokenInformation,
            [In]  uint                  TokenInformationLength,
            [Out] out uint              ReturnLength);

        internal const int TokenIsAppContainer = 29;

        
        [ComImport, Guid("00000003-0000-0000-C000-000000000046"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMarshal 
        {
            [PreserveSig]
            int GetUnmarshalClass( 
                ref Guid riid,
                IntPtr pv,
                int dwDestContext,
                IntPtr pvDestContext,
                int mshlflags,
                out Guid pCid);
        
            [PreserveSig]
            int GetMarshalSizeMax( 
                ref Guid riid,
                IntPtr pv,
                int dwDestContext,
                IntPtr pvDestContext,
                int mshlflags,
                out int pSize);
       
            [PreserveSig]
            int MarshalInterface( 
                IntPtr pStm,
                ref Guid riid,
                IntPtr pv,
                int dwDestContext,
                IntPtr pvDestContext,
                int mshlflags);
       
            [PreserveSig]
            int UnmarshalInterface( 
                IntPtr pStm,
                ref Guid riid,
                out IntPtr ppv);
       
            [PreserveSig]
            int ReleaseMarshalData(IntPtr pStm);
        
            [PreserveSig]
            int DisconnectObject(int dwReserved);
        }


        [DllImport(ExternDll.Ole32)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int CoGetStandardMarshal(
                ref Guid riid,
                IntPtr pv,
                int dwDestContext,
                IntPtr pvDestContext,
                int mshlflags,
                out IntPtr ppMarshal
        );

#endif // !SILVERLIGHT

    }
}
