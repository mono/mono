// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: UnsafeNativeMethods
**
============================================================*/
namespace Microsoft.Win32 {

    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Configuration.Assemblies;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Principal;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Diagnostics.Eventing;
    using System.Diagnostics.Eventing.Reader;

    [SuppressUnmanagedCodeSecurityAttribute()]
    internal static class UnsafeNativeMethods {

        internal const String KERNEL32 = "kernel32.dll";
        internal const String ADVAPI32 = "advapi32.dll";
        internal const String WEVTAPI  = "wevtapi.dll";
        internal static readonly IntPtr NULL = IntPtr.Zero;

        //
        // Win32 IO
        //
        internal const int CREDUI_MAX_USERNAME_LENGTH = 513;
     
       
        // WinError.h codes:

        internal const int ERROR_SUCCESS                 = 0x0;
        internal const int ERROR_FILE_NOT_FOUND          = 0x2;
        internal const int ERROR_PATH_NOT_FOUND          = 0x3;
        internal const int ERROR_ACCESS_DENIED           = 0x5;
        internal const int ERROR_INVALID_HANDLE          = 0x6;

        // Can occurs when filled buffers are trying to flush to disk, but disk IOs are not fast enough. 
        // This happens when the disk is slow and event traffic is heavy. 
        // Eventually, there are no more free (empty) buffers and the event is dropped.
        internal const int ERROR_NOT_ENOUGH_MEMORY       = 0x8;
        
        internal const int ERROR_INVALID_DRIVE           = 0xF;
        internal const int ERROR_NO_MORE_FILES           = 0x12;
        internal const int ERROR_NOT_READY               = 0x15;
        internal const int ERROR_BAD_LENGTH              = 0x18;
        internal const int ERROR_SHARING_VIOLATION       = 0x20;
        internal const int ERROR_LOCK_VIOLATION          = 0x21;  // 33
        internal const int ERROR_HANDLE_EOF              = 0x26;  // 38
        internal const int ERROR_FILE_EXISTS             = 0x50;
        internal const int ERROR_INVALID_PARAMETER       = 0x57;  // 87
        internal const int ERROR_BROKEN_PIPE             = 0x6D;  // 109
        internal const int ERROR_INSUFFICIENT_BUFFER     = 0x7A;  // 122
        internal const int ERROR_INVALID_NAME            = 0x7B;
        internal const int ERROR_BAD_PATHNAME            = 0xA1;
        internal const int ERROR_ALREADY_EXISTS          = 0xB7;        
        internal const int ERROR_ENVVAR_NOT_FOUND        = 0xCB;
        internal const int ERROR_FILENAME_EXCED_RANGE    = 0xCE;  // filename too long
        internal const int ERROR_PIPE_BUSY               = 0xE7;  // 231
        internal const int ERROR_NO_DATA                 = 0xE8;  // 232
        internal const int ERROR_PIPE_NOT_CONNECTED      = 0xE9;  // 233
        internal const int ERROR_MORE_DATA               = 0xEA;
        internal const int ERROR_NO_MORE_ITEMS           = 0x103;  // 259
        internal const int ERROR_PIPE_CONNECTED          = 0x217;  // 535
        internal const int ERROR_PIPE_LISTENING          = 0x218;  // 536
        internal const int ERROR_OPERATION_ABORTED       = 0x3E3;  // 995; For IO Cancellation
        internal const int ERROR_IO_PENDING              = 0x3E5;  // 997
        internal const int ERROR_NOT_FOUND               = 0x490;  // 1168      
   
        // The event size is larger than the allowed maximum (64k - header).
        internal const int ERROR_ARITHMETIC_OVERFLOW     = 0x216;  // 534

        internal const int ERROR_RESOURCE_LANG_NOT_FOUND = 0x717;  // 1815


        // Event log specific codes:

        internal const int ERROR_EVT_MESSAGE_NOT_FOUND = 15027;
        internal const int ERROR_EVT_MESSAGE_ID_NOT_FOUND = 15028;
        internal const int ERROR_EVT_UNRESOLVED_VALUE_INSERT = 15029;
        internal const int ERROR_EVT_UNRESOLVED_PARAMETER_INSERT = 15030;
        internal const int ERROR_EVT_MAX_INSERTS_REACHED = 15031;
        internal const int ERROR_EVT_MESSAGE_LOCALE_NOT_FOUND = 15033;
        internal const int ERROR_MUI_FILE_NOT_FOUND = 15100;


        internal const int SECURITY_SQOS_PRESENT = 0x00100000;
        internal const int SECURITY_ANONYMOUS = 0 << 16;
        internal const int SECURITY_IDENTIFICATION = 1 << 16;
        internal const int SECURITY_IMPERSONATION = 2 << 16;
        internal const int SECURITY_DELEGATION = 3 << 16;

        internal const int GENERIC_READ = unchecked((int)0x80000000);
        internal const int GENERIC_WRITE = 0x40000000;

        internal const int STD_INPUT_HANDLE = -10;
        internal const int STD_OUTPUT_HANDLE = -11;
        internal const int STD_ERROR_HANDLE = -12;

        internal const int DUPLICATE_SAME_ACCESS = 0x00000002;

        internal const int PIPE_ACCESS_INBOUND = 1;
        internal const int PIPE_ACCESS_OUTBOUND = 2;
        internal const int PIPE_ACCESS_DUPLEX = 3;
        internal const int PIPE_TYPE_BYTE = 0;
        internal const int PIPE_TYPE_MESSAGE = 4;
        internal const int PIPE_READMODE_BYTE = 0;
        internal const int PIPE_READMODE_MESSAGE = 2;
        internal const int PIPE_UNLIMITED_INSTANCES = 255;

        internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
        internal const int FILE_SHARE_READ = 0x00000001;
        internal const int FILE_SHARE_WRITE = 0x00000002;
        internal const int FILE_ATTRIBUTE_NORMAL = 0x00000080;

        internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

        internal const int OPEN_EXISTING = 3;        

        // From WinBase.h
        internal const int FILE_TYPE_DISK = 0x0001;
        internal const int FILE_TYPE_CHAR = 0x0002;
        internal const int FILE_TYPE_PIPE = 0x0003;       

        // Memory mapped file constants
        internal const int MEM_COMMIT = 0x1000;
        internal const int MEM_RESERVE = 0x2000;
        internal const int INVALID_FILE_SIZE = -1;
        internal const int PAGE_READWRITE = 0x04;
        internal const int PAGE_READONLY = 0x02;
        internal const int PAGE_WRITECOPY = 0x08;
        internal const int PAGE_EXECUTE_READ = 0x20;
        internal const int PAGE_EXECUTE_READWRITE = 0x40;

        internal const int FILE_MAP_COPY = 0x0001;
        internal const int FILE_MAP_WRITE = 0x0002;
        internal const int FILE_MAP_READ = 0x0004;
        internal const int FILE_MAP_EXECUTE = 0x0020;

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES {
            internal int nLength;
            [SecurityCritical]
            internal unsafe byte* pSecurityDescriptor;
            internal int bInheritHandle;
        }

        [DllImport(KERNEL32)]
        [SecurityCritical]
        internal static extern int GetFileType(SafeFileHandle handle);

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        internal static unsafe extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite,
                                                    out int numBytesWritten, NativeOverlapped* lpOverlapped);

        // Disallow access to all non-file devices from methods that take
        // a String.  This disallows DOS devices like "con:", "com1:", 
        // "lpt1:", etc.  Use this to avoid security problems, like allowing
        // a web client asking a server for "http://server/com1.aspx" and
        // then causing a worker process to hang.
        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        private static extern SafeFileHandle CreateFile(String lpFileName,
            int dwDesiredAccess, System.IO.FileShare dwShareMode,
            SECURITY_ATTRIBUTES securityAttrs, System.IO.FileMode dwCreationDisposition,
            int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [SecurityCritical]
        internal static SafeFileHandle SafeCreateFile(String lpFileName,
                    int dwDesiredAccess, System.IO.FileShare dwShareMode,
                    SECURITY_ATTRIBUTES securityAttrs, System.IO.FileMode dwCreationDisposition,
                    int dwFlagsAndAttributes, IntPtr hTemplateFile) {
            SafeFileHandle handle = CreateFile(lpFileName, dwDesiredAccess, dwShareMode,
                                securityAttrs, dwCreationDisposition,
                                dwFlagsAndAttributes, hTemplateFile);

            if (!handle.IsInvalid) {
                int fileType = UnsafeNativeMethods.GetFileType(handle);
                if (fileType != UnsafeNativeMethods.FILE_TYPE_DISK) {
                    handle.Dispose();
                    throw new NotSupportedException(SR.GetString(SR.NotSupported_IONonFileDevices));
                }
            }
            return handle;
        }


        // From WinBase.h
        internal const int SEM_FAILCRITICALERRORS = 1;

        [DllImport(KERNEL32, SetLastError = false)]
        [ResourceExposure(ResourceScope.Process)]
        [SecurityCritical]
        internal static extern int SetErrorMode(int newMode);

        [DllImport(KERNEL32, SetLastError = true, EntryPoint = "SetFilePointer")]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        private unsafe static extern int SetFilePointerWin32(SafeFileHandle handle, int lo, int* hi, int origin);

        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        internal unsafe static long SetFilePointer(SafeFileHandle handle, long offset, System.IO.SeekOrigin origin, out int hr) {
            hr = 0;
            int lo = (int)offset;
            int hi = (int)(offset >> 32);
            lo = SetFilePointerWin32(handle, lo, &hi, (int)origin);

            if (lo == -1 && ((hr = Marshal.GetLastWin32Error()) != 0))
                return -1;
            return (long)(((ulong)((uint)hi)) << 32) | ((uint)lo);
        }

        //
        // ErrorCode & format 
        //

        // Use this to translate error codes like the above into HRESULTs like

        // 0x80070006 for ERROR_INVALID_HANDLE
        internal static int MakeHRFromErrorCode(int errorCode) {
            return unchecked(((int)0x80070000) | errorCode);
        }

        // for win32 error message formatting
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;


        [DllImport(KERNEL32, CharSet = CharSet.Auto, BestFitMapping = false)]
        [SecurityCritical]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource,
            int dwMessageId, int dwLanguageId, StringBuilder lpBuffer,
            int nSize, IntPtr va_list_arguments);

        // Gets an error message for a Win32 error code.
        [SecurityCritical]
        internal static String GetMessage(int errorCode) {
            StringBuilder sb = new StringBuilder(512);
            int result = UnsafeNativeMethods.FormatMessage(FORMAT_MESSAGE_IGNORE_INSERTS |
                FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY,
                UnsafeNativeMethods.NULL, errorCode, 0, sb, sb.Capacity, UnsafeNativeMethods.NULL);
            if (result != 0) {
                // result is the # of characters copied to the StringBuilder on NT,
                // but on Win9x, it appears to be the number of MBCS buffer.
                // Just give up and return the String as-is...
                String s = sb.ToString();
                return s;
            }
            else {
                return "UnknownError_Num " + errorCode;
            }
        }


        //
        // SafeLibraryHandle
        //

        [DllImport(KERNEL32, CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        [SecurityCritical]
        internal static extern SafeLibraryHandle LoadLibraryEx(string libFilename, IntPtr reserved, int flags);

        [DllImport(KERNEL32, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecurityCritical]
        internal static extern bool FreeLibrary(IntPtr hModule);
        

        // 
        // Pipe
        //

        [DllImport(KERNEL32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
            SafePipeHandle hSourceHandle, IntPtr hTargetProcessHandle, out SafePipeHandle lpTargetHandle,
            uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        [DllImport(KERNEL32)]
        [SecurityCritical]
        internal static extern int GetFileType(SafePipeHandle handle);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CreatePipe(out SafePipeHandle hReadPipe,
            out SafePipeHandle hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);


        [DllImport(KERNEL32, EntryPoint="CreateFile", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        [SecurityCritical]
        internal static extern SafePipeHandle CreateNamedPipeClient(String lpFileName,
            int dwDesiredAccess, System.IO.FileShare dwShareMode,
            UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttrs, System.IO.FileMode dwCreationDisposition,
            int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        unsafe internal static extern bool ConnectNamedPipe(SafePipeHandle handle, NativeOverlapped* overlapped);

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ConnectNamedPipe(SafePipeHandle handle, IntPtr overlapped);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WaitNamedPipe(String name, int timeout);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetNamedPipeHandleState(SafePipeHandle hNamedPipe, out int lpState,
            IntPtr lpCurInstances, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout,
            IntPtr lpUserName, int nMaxUserNameSize);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetNamedPipeHandleState(SafePipeHandle hNamedPipe, IntPtr lpState,
            out int lpCurInstances, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout,
            IntPtr lpUserName, int nMaxUserNameSize);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetNamedPipeHandleState(SafePipeHandle hNamedPipe, IntPtr lpState,
            IntPtr lpCurInstances, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout,
            StringBuilder lpUserName, int nMaxUserNameSize);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetNamedPipeInfo(SafePipeHandle hNamedPipe,
          out int lpFlags,
          IntPtr lpOutBufferSize,
          IntPtr lpInBufferSize,
          IntPtr lpMaxInstances
        );

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetNamedPipeInfo(SafePipeHandle hNamedPipe,
          IntPtr lpFlags,
          out int lpOutBufferSize,
          IntPtr lpInBufferSize,
          IntPtr lpMaxInstances
        );

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetNamedPipeInfo(SafePipeHandle hNamedPipe,
          IntPtr lpFlags,
          IntPtr lpOutBufferSize,
          out int lpInBufferSize,
          IntPtr lpMaxInstances
        );

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe extern bool SetNamedPipeHandleState(
          SafePipeHandle hNamedPipe,
          int* lpMode,
          IntPtr lpMaxCollectionCount,
          IntPtr lpCollectDataTimeout
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DisconnectNamedPipe(SafePipeHandle hNamedPipe);

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FlushFileBuffers(SafePipeHandle hNamedPipe);

        [DllImport(ADVAPI32, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static extern bool RevertToSelf();

        [DllImport(ADVAPI32, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static extern bool ImpersonateNamedPipeClient(SafePipeHandle hNamedPipe);

        [DllImport(KERNEL32, SetLastError = true, BestFitMapping = false, CharSet = CharSet.Auto)]
        [SecurityCritical]
        internal static extern SafePipeHandle CreateNamedPipe(string pipeName,
            int openMode, int pipeMode, int maxInstances,
            int outBufferSize, int inBufferSize, int defaultTimeout,
            SECURITY_ATTRIBUTES securityAttributes);

        // Note there are two different ReadFile prototypes - this is to use 
        // the type system to force you to not trip across a "feature" in 
        // Win32's async IO support.  You can't do the following three things
        // simultaneously: overlapped IO, free the memory for the overlapped 
        // struct in a callback (or an EndRead method called by that callback), 
        // and pass in an address for the numBytesRead parameter.  
        // <

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        unsafe internal static extern int ReadFile(SafePipeHandle handle, byte* bytes, int numBytesToRead,
            IntPtr numBytesRead_mustBeZero, NativeOverlapped* overlapped);

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        unsafe internal static extern int ReadFile(SafePipeHandle handle, byte* bytes, int numBytesToRead,
            out int numBytesRead, IntPtr mustBeZero);

        // Note there are two different WriteFile prototypes - this is to use 
        // the type system to force you to not trip across a "feature" in 
        // Win32's async IO support.  You can't do the following three things
        // simultaneously: overlapped IO, free the memory for the overlapped 
        // struct in a callback (or an EndWrite method called by that callback),
        // and pass in an address for the numBytesRead parameter.  
        // <

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        internal static unsafe extern int WriteFile(SafePipeHandle handle, byte* bytes, int numBytesToWrite,
            IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        internal static unsafe extern int WriteFile(SafePipeHandle handle, byte* bytes, int numBytesToWrite,
            out int numBytesWritten, IntPtr mustBeZero);

        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        internal static extern bool SetEndOfFile(IntPtr hNamedPipe);

        //
        // ETW Methods
        //
        //
        // Callback
        //
#pragma warning disable 618 // Ssytem.Core still uses SecurityRuleSet.Level1
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        internal unsafe delegate void EtwEnableCallback(
            [In] ref Guid sourceId,
            [In] int isEnabled,
            [In] byte level,
            [In] long matchAnyKeywords,
            [In] long matchAllKeywords,
            [In] void* filterData,
            [In] void* callbackContext
            );

        //
        // Registration APIs
        //
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventRegister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventRegister(
                    [In] ref Guid providerId,
                    [In]EtwEnableCallback enableCallback,
                    [In]void* callbackContext,
                    [In][Out]ref long registrationHandle
                    );

        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventUnregister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern int EventUnregister([In] long registrationHandle);


        //
        // Control (Is Enabled) APIs
        //
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventEnabled", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern int EventEnabled([In] long registrationHandle, [In] ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor);

        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventProviderEnabled", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern int EventProviderEnabled([In] long registrationHandle, [In] byte level, [In] long keywords);

        //
        // Writing (Publishing/Logging) APIs
        //
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventWrite", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventWrite(
                [In] long registrationHandle,
                [In] ref EventDescriptor eventDescriptor,
                [In] uint userDataCount,
                [In] void* userData
                );

        //
        // Writing (Publishing/Logging) APIs
        //
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventWrite", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventWrite(
                [In] long registrationHandle,
                [In] EventDescriptor* eventDescriptor,
                [In] uint userDataCount,
                [In] void* userData
                );

        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventWriteTransfer", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventWriteTransfer(
                [In] long registrationHandle,
                [In] ref EventDescriptor eventDescriptor,
                [In] Guid* activityId,
                [In] Guid* relatedActivityId,
                [In] uint userDataCount,
                [In] void* userData
                );

        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventWriteString", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventWriteString(
                [In] long registrationHandle,
                [In] byte level,
                [In] long keywords,
                [In] char* message
                );
        //
        // ActivityId Control APIs
        //
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventActivityIdControl", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventActivityIdControl([In] int ControlCode, [In][Out] ref Guid ActivityId);

        // Native PERFLIB V2 Provider APIs.
        //
        [StructLayout(LayoutKind.Explicit, Size = 40)]
        internal struct PerfCounterSetInfoStruct { // PERF_COUNTERSET_INFO structure defined in perflib.h
            [FieldOffset(0)]  internal Guid CounterSetGuid;
            [FieldOffset(16)] internal Guid ProviderGuid;
            [FieldOffset(32)] internal uint NumCounters;
            [FieldOffset(36)] internal uint InstanceType;
        }
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        internal struct PerfCounterInfoStruct { // PERF_COUNTER_INFO structure defined in perflib.h
            [FieldOffset(0)]  internal uint  CounterId;
            [FieldOffset(4)]  internal uint  CounterType;
            [FieldOffset(8)]  internal Int64 Attrib;
            [FieldOffset(16)] internal uint  Size;
            [FieldOffset(20)] internal uint  DetailLevel;
            [FieldOffset(24)] internal uint  Scale;
            [FieldOffset(28)] internal uint  Offset;
        }
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        internal struct PerfCounterSetInstanceStruct { // PERF_COUNTERSET_INSTANCE structure defined in perflib.h
            [FieldOffset(0)]  internal Guid CounterSetGuid;
            [FieldOffset(16)] internal uint dwSize;
            [FieldOffset(20)] internal uint InstanceId;
            [FieldOffset(24)] internal uint InstanceNameOffset;
            [FieldOffset(28)] internal uint InstanceNameSize;
        }

#pragma warning disable 618 // Ssytem.Core still uses SecurityRuleSet.Level1
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        internal unsafe delegate uint PERFLIBREQUEST(
            [In] uint   RequestCode,
            [In] void * Buffer,
            [In] uint   BufferSize
        );

        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "PerfStartProvider", CharSet = CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint PerfStartProvider(
            [In]  ref Guid                   ProviderGuid,
            [In]  PERFLIBREQUEST             ControlCallback,
            [Out] out SafePerfProviderHandle phProvider
        );
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "PerfStopProvider", CharSet = CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint PerfStopProvider(
            [In] IntPtr hProvider
        );
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "PerfSetCounterSetInfo", CharSet = CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint PerfSetCounterSetInfo(
            [In]      SafePerfProviderHandle     hProvider,
            [In][Out] PerfCounterSetInfoStruct * pTemplate,
            [In]      uint                       dwTemplateSize
        );
        [DllImport(ADVAPI32, SetLastError = true, ExactSpelling = true, EntryPoint = "PerfCreateInstance", CharSet = CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe PerfCounterSetInstanceStruct* PerfCreateInstance(
            [In] SafePerfProviderHandle hProvider,
            [In] ref Guid               CounterSetGuid,
            [In] String                 szInstanceName,
            [In] uint                   dwInstance
        );
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "PerfDeleteInstance", CharSet = CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint PerfDeleteInstance(
            [In] SafePerfProviderHandle         hProvider,
            [In] PerfCounterSetInstanceStruct * InstanceBlock
        );
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "PerfSetCounterRefValue", CharSet = CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint PerfSetCounterRefValue(
            [In] SafePerfProviderHandle         hProvider,
            [In] PerfCounterSetInstanceStruct * pInstance,
            [In] uint                           CounterId,
            [In] void *                         lpAddr
        );

        //
        // EventLog
        // 
        [Flags]
        internal enum EvtQueryFlags {
            EvtQueryChannelPath = 0x1,
            EvtQueryFilePath = 0x2,
            EvtQueryForwardDirection = 0x100,
            EvtQueryReverseDirection = 0x200,
            EvtQueryTolerateQueryErrors = 0x1000
        }

        [Flags]
        internal enum EvtSubscribeFlags {
            EvtSubscribeToFutureEvents = 1,
            EvtSubscribeStartAtOldestRecord = 2,
            EvtSubscribeStartAfterBookmark = 3,
            EvtSubscribeTolerateQueryErrors = 0x1000,
            EvtSubscribeStrict = 0x10000
        }
        
        /// <summary>
        /// Evt Variant types
        /// </summary>
        internal enum EvtVariantType {
            EvtVarTypeNull = 0,
            EvtVarTypeString = 1,
            EvtVarTypeAnsiString = 2,
            EvtVarTypeSByte = 3,
            EvtVarTypeByte = 4,
            EvtVarTypeInt16 = 5,
            EvtVarTypeUInt16 = 6,
            EvtVarTypeInt32 = 7,
            EvtVarTypeUInt32 = 8,
            EvtVarTypeInt64 = 9,
            EvtVarTypeUInt64 = 10,
            EvtVarTypeSingle = 11,
            EvtVarTypeDouble = 12,
            EvtVarTypeBoolean = 13,
            EvtVarTypeBinary = 14,
            EvtVarTypeGuid = 15,
            EvtVarTypeSizeT = 16,
            EvtVarTypeFileTime = 17,
            EvtVarTypeSysTime = 18,
            EvtVarTypeSid = 19,
            EvtVarTypeHexInt32 = 20,
            EvtVarTypeHexInt64 = 21,
            // these types used internally
            EvtVarTypeEvtHandle = 32,
            EvtVarTypeEvtXml = 35,
            //Array = 128
            EvtVarTypeStringArray = 129,
            EvtVarTypeUInt32Array = 136
        }

        internal enum EvtMasks {
            EVT_VARIANT_TYPE_MASK = 0x7f,
            EVT_VARIANT_TYPE_ARRAY = 128
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SystemTime {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
#pragma warning disable 618 // Ssytem.Core still uses SecurityRuleSet.Level1
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        internal struct EvtVariant {
            [FieldOffset(0)]
            public UInt32 UInteger;
            [FieldOffset(0)]
            public Int32 Integer;
            [FieldOffset(0)]
            public byte UInt8;
            [FieldOffset(0)]
            public short Short;
            [FieldOffset(0)]
            public ushort UShort;
            [FieldOffset(0)]
            public UInt32 Bool;
            [FieldOffset(0)]
            public Byte ByteVal;
            [FieldOffset(0)]
            public byte SByte;
            [FieldOffset(0)]
            public UInt64 ULong;
            [FieldOffset(0)]
            public Int64 Long;
            [FieldOffset(0)]
            public Single Single;            
            [FieldOffset(0)]
            public Double Double;
            [FieldOffset(0)]
            public IntPtr StringVal;
            [FieldOffset(0)]
            public IntPtr AnsiString;
            [FieldOffset(0)]
            public IntPtr SidVal;
            [FieldOffset(0)]
            public IntPtr Binary;
            [FieldOffset(0)]
            public IntPtr Reference;
            [FieldOffset(0)]
            public IntPtr Handle;
            [FieldOffset(0)]
            public IntPtr GuidReference;
            [FieldOffset(0)]
            public UInt64 FileTime;
            [FieldOffset(0)]
            public IntPtr SystemTime;
            [FieldOffset(0)]
            public IntPtr SizeT;
            [FieldOffset(8)]
            public UInt32 Count;   // number of elements (not length) in bytes.
            [FieldOffset(12)]
            public UInt32 Type;
        }

        internal enum EvtEventPropertyId {
            EvtEventQueryIDs = 0,
            EvtEventPath = 1
        }

        /// <summary>
        /// The query flags to get information about query
        /// </summary>
        internal enum EvtQueryPropertyId {
            EvtQueryNames = 0,   //String;   //Variant will be array of EvtVarTypeString
            EvtQueryStatuses = 1 //UInt32;   //Variant will be Array of EvtVarTypeUInt32
        }

        /// <summary>
        /// Publisher Metadata properties
        /// </summary>
        internal enum EvtPublisherMetadataPropertyId {
            EvtPublisherMetadataPublisherGuid = 0,      // EvtVarTypeGuid
            EvtPublisherMetadataResourceFilePath = 1,       // EvtVarTypeString
            EvtPublisherMetadataParameterFilePath = 2,      // EvtVarTypeString
            EvtPublisherMetadataMessageFilePath = 3,        // EvtVarTypeString
            EvtPublisherMetadataHelpLink = 4,               // EvtVarTypeString
            EvtPublisherMetadataPublisherMessageID = 5,     // EvtVarTypeUInt32

            EvtPublisherMetadataChannelReferences = 6,      // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataChannelReferencePath = 7,   // EvtVarTypeString
            EvtPublisherMetadataChannelReferenceIndex = 8,  // EvtVarTypeUInt32
            EvtPublisherMetadataChannelReferenceID = 9,     // EvtVarTypeUInt32
            EvtPublisherMetadataChannelReferenceFlags = 10,  // EvtVarTypeUInt32
            EvtPublisherMetadataChannelReferenceMessageID = 11, // EvtVarTypeUInt32

            EvtPublisherMetadataLevels = 12,                 // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataLevelName = 13,              // EvtVarTypeString
            EvtPublisherMetadataLevelValue = 14,             // EvtVarTypeUInt32
            EvtPublisherMetadataLevelMessageID = 15,         // EvtVarTypeUInt32

            EvtPublisherMetadataTasks = 16,                  // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataTaskName = 17,               // EvtVarTypeString
            EvtPublisherMetadataTaskEventGuid = 18,          // EvtVarTypeGuid
            EvtPublisherMetadataTaskValue = 19,              // EvtVarTypeUInt32
            EvtPublisherMetadataTaskMessageID = 20,          // EvtVarTypeUInt32

            EvtPublisherMetadataOpcodes = 21,                // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataOpcodeName = 22,             // EvtVarTypeString
            EvtPublisherMetadataOpcodeValue = 23,            // EvtVarTypeUInt32
            EvtPublisherMetadataOpcodeMessageID = 24,        // EvtVarTypeUInt32

            EvtPublisherMetadataKeywords = 25,               // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataKeywordName = 26,            // EvtVarTypeString
            EvtPublisherMetadataKeywordValue = 27,           // EvtVarTypeUInt64
            EvtPublisherMetadataKeywordMessageID = 28//,       // EvtVarTypeUInt32
            //EvtPublisherMetadataPropertyIdEND
        }

        internal enum EvtChannelReferenceFlags {
            EvtChannelReferenceImported = 1
        }

        internal enum EvtEventMetadataPropertyId {
            EventMetadataEventID,       // EvtVarTypeUInt32
            EventMetadataEventVersion,  // EvtVarTypeUInt32
            EventMetadataEventChannel,  // EvtVarTypeUInt32
            EventMetadataEventLevel,    // EvtVarTypeUInt32
            EventMetadataEventOpcode,   // EvtVarTypeUInt32
            EventMetadataEventTask,     // EvtVarTypeUInt32
            EventMetadataEventKeyword,  // EvtVarTypeUInt64
            EventMetadataEventMessageID,// EvtVarTypeUInt32
            EventMetadataEventTemplate // EvtVarTypeString
            //EvtEventMetadataPropertyIdEND
        }

        //CHANNEL CONFIGURATION 
        internal enum EvtChannelConfigPropertyId {
            EvtChannelConfigEnabled = 0,            // EvtVarTypeBoolean
            EvtChannelConfigIsolation,              // EvtVarTypeUInt32, EVT_CHANNEL_ISOLATION_TYPE
            EvtChannelConfigType,                   // EvtVarTypeUInt32, EVT_CHANNEL_TYPE
            EvtChannelConfigOwningPublisher,        // EvtVarTypeString
            EvtChannelConfigClassicEventlog,        // EvtVarTypeBoolean
            EvtChannelConfigAccess,                 // EvtVarTypeString
            EvtChannelLoggingConfigRetention,       // EvtVarTypeBoolean
            EvtChannelLoggingConfigAutoBackup,      // EvtVarTypeBoolean
            EvtChannelLoggingConfigMaxSize,         // EvtVarTypeUInt64
            EvtChannelLoggingConfigLogFilePath,     // EvtVarTypeString
            EvtChannelPublishingConfigLevel,        // EvtVarTypeUInt32
            EvtChannelPublishingConfigKeywords,     // EvtVarTypeUInt64
            EvtChannelPublishingConfigControlGuid,  // EvtVarTypeGuid
            EvtChannelPublishingConfigBufferSize,   // EvtVarTypeUInt32
            EvtChannelPublishingConfigMinBuffers,   // EvtVarTypeUInt32
            EvtChannelPublishingConfigMaxBuffers,   // EvtVarTypeUInt32
            EvtChannelPublishingConfigLatency,      // EvtVarTypeUInt32
            EvtChannelPublishingConfigClockType,    // EvtVarTypeUInt32, EVT_CHANNEL_CLOCK_TYPE
            EvtChannelPublishingConfigSidType,      // EvtVarTypeUInt32, EVT_CHANNEL_SID_TYPE
            EvtChannelPublisherList,                // EvtVarTypeString | EVT_VARIANT_TYPE_ARRAY
            EvtChannelConfigPropertyIdEND
        }

        //LOG INFORMATION
        internal enum EvtLogPropertyId {
            EvtLogCreationTime = 0,             // EvtVarTypeFileTime
            EvtLogLastAccessTime,               // EvtVarTypeFileTime
            EvtLogLastWriteTime,                // EvtVarTypeFileTime
            EvtLogFileSize,                     // EvtVarTypeUInt64
            EvtLogAttributes,                   // EvtVarTypeUInt32
            EvtLogNumberOfLogRecords,           // EvtVarTypeUInt64
            EvtLogOldestRecordNumber,           // EvtVarTypeUInt64
            EvtLogFull,                         // EvtVarTypeBoolean
        }

        internal enum EvtExportLogFlags {
            EvtExportLogChannelPath = 1,
            EvtExportLogFilePath = 2,
            EvtExportLogTolerateQueryErrors = 0x1000
        }

        //RENDERING    
        internal enum EvtRenderContextFlags {
            EvtRenderContextValues = 0,      // Render specific properties
            EvtRenderContextSystem = 1,      // Render all system properties (System)
            EvtRenderContextUser = 2         // Render all user properties (User/EventData)
        }

        internal enum EvtRenderFlags {
            EvtRenderEventValues = 0,       // Variants
            EvtRenderEventXml = 1,          // XML
            EvtRenderBookmark = 2           // Bookmark
        }

        internal enum EvtFormatMessageFlags {
            EvtFormatMessageEvent = 1,
            EvtFormatMessageLevel = 2,
            EvtFormatMessageTask = 3,
            EvtFormatMessageOpcode = 4,
            EvtFormatMessageKeyword = 5,
            EvtFormatMessageChannel = 6,
            EvtFormatMessageProvider = 7,
            EvtFormatMessageId = 8,
            EvtFormatMessageXml = 9
        }

        internal enum EvtSystemPropertyId {
            EvtSystemProviderName = 0,          // EvtVarTypeString             
            EvtSystemProviderGuid,              // EvtVarTypeGuid  
            EvtSystemEventID,                   // EvtVarTypeUInt16  
            EvtSystemQualifiers,                // EvtVarTypeUInt16
            EvtSystemLevel,                     // EvtVarTypeUInt8
            EvtSystemTask,                      // EvtVarTypeUInt16
            EvtSystemOpcode,                    // EvtVarTypeUInt8
            EvtSystemKeywords,                  // EvtVarTypeHexInt64
            EvtSystemTimeCreated,               // EvtVarTypeFileTime
            EvtSystemEventRecordId,             // EvtVarTypeUInt64
            EvtSystemActivityID,                // EvtVarTypeGuid
            EvtSystemRelatedActivityID,         // EvtVarTypeGuid
            EvtSystemProcessID,                 // EvtVarTypeUInt32
            EvtSystemThreadID,                  // EvtVarTypeUInt32
            EvtSystemChannel,                   // EvtVarTypeString 
            EvtSystemComputer,                  // EvtVarTypeString 
            EvtSystemUserID,                    // EvtVarTypeSid
            EvtSystemVersion,                   // EvtVarTypeUInt8
            EvtSystemPropertyIdEND
        }

        //SESSION
        internal enum EvtLoginClass {
            EvtRpcLogin = 1
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct EvtRpcLogin {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Server;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string User;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Domain;
            [SecurityCritical]
            public CoTaskMemUnicodeSafeHandle Password;
            public int Flags;
            }


            //SEEK
            [Flags]
            internal enum EvtSeekFlags {
                EvtSeekRelativeToFirst = 1,
                EvtSeekRelativeToLast = 2,
                EvtSeekRelativeToCurrent = 3,
                EvtSeekRelativeToBookmark = 4,
                EvtSeekOriginMask = 7,
                EvtSeekStrict = 0x10000
            }

            [DllImport(WEVTAPI, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtQuery(
                                EventLogHandle session,
                                [MarshalAs(UnmanagedType.LPWStr)]string path,
                                [MarshalAs(UnmanagedType.LPWStr)]string query,
                                int flags);

            //SEEK
            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtSeek(
                                EventLogHandle resultSet,
                                long position,
                                EventLogHandle bookmark,
                                int timeout,
                                [MarshalAs(UnmanagedType.I4)]EvtSeekFlags flags
                                            );

            [DllImport(WEVTAPI, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtSubscribe(
                                EventLogHandle session,
                                SafeWaitHandle signalEvent,
                                [MarshalAs(UnmanagedType.LPWStr)]string path,
                                [MarshalAs(UnmanagedType.LPWStr)]string query,
                                EventLogHandle bookmark,
                                IntPtr context,
                                IntPtr callback,
                                int flags);

            [DllImport(WEVTAPI, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtNext(
                                EventLogHandle queryHandle,
                                int eventSize,
                                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] events,
                                int timeout,
                                int flags,
                                ref int returned);

            [DllImport(WEVTAPI, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtCancel(EventLogHandle handle);

            [DllImport(WEVTAPI)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtClose(IntPtr handle);

            /*
            [DllImport(WEVTAPI, EntryPoint = "EvtClose", SetLastError = true)]
            public static extern bool EvtClose(
                                IntPtr eventHandle
                                               );
             */

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetEventInfo(
                                EventLogHandle eventHandle,
                //int propertyId
                                [MarshalAs(UnmanagedType.I4)]EvtEventPropertyId propertyId,
                                int bufferSize,
                                IntPtr bufferPtr,
                                out int bufferUsed
                                                );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetQueryInfo(
                                EventLogHandle queryHandle,
                                [MarshalAs(UnmanagedType.I4)]EvtQueryPropertyId propertyId,
                                int bufferSize,
                                IntPtr buffer,
                                ref int bufferRequired
                                                );

            //PUBLISHER METADATA
            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtOpenPublisherMetadata(
                                EventLogHandle session,
                                [MarshalAs(UnmanagedType.LPWStr)] string publisherId,
                                [MarshalAs(UnmanagedType.LPWStr)] string logFilePath,
                                int locale,
                                int flags
                                        );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetPublisherMetadataProperty(
                                EventLogHandle publisherMetadataHandle,
                                [MarshalAs(UnmanagedType.I4)] EvtPublisherMetadataPropertyId propertyId,
                                int flags,
                                int publisherMetadataPropertyBufferSize,
                                IntPtr publisherMetadataPropertyBuffer,
                                out int publisherMetadataPropertyBufferUsed
                                        );

            //NEW

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetObjectArraySize(
                                EventLogHandle objectArray,
                                out int objectArraySize
                                            );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetObjectArrayProperty(
                                EventLogHandle objectArray,
                                int propertyId,
                                int arrayIndex,
                                int flags,
                                int propertyValueBufferSize,
                                IntPtr propertyValueBuffer,
                                out int propertyValueBufferUsed
                                                );

            //NEW 2
            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtOpenEventMetadataEnum(
                                EventLogHandle publisherMetadata,
                                int flags
                                        );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            //public static extern IntPtr EvtNextEventMetadata(
            internal static extern EventLogHandle EvtNextEventMetadata(
                                EventLogHandle eventMetadataEnum,
                                int flags
                                        );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetEventMetadataProperty(
                                EventLogHandle eventMetadata,
                                [MarshalAs(UnmanagedType.I4)]  EvtEventMetadataPropertyId propertyId,
                                int flags,
                                int eventMetadataPropertyBufferSize,
                                IntPtr eventMetadataPropertyBuffer,
                                out int eventMetadataPropertyBufferUsed
                                       );


            //Channel Configuration Native Api

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtOpenChannelEnum(
                                EventLogHandle session,
                                int flags
                                        );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtNextChannelPath(
                                EventLogHandle channelEnum,
                                int channelPathBufferSize,
                //StringBuilder channelPathBuffer,
                                [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder channelPathBuffer,
                                out int channelPathBufferUsed
                                        );


            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtOpenPublisherEnum(
                                EventLogHandle session,
                                int flags
                                        );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtNextPublisherId(
                                EventLogHandle publisherEnum,
                                int publisherIdBufferSize,
                                [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder publisherIdBuffer,
                                out int publisherIdBufferUsed
                                        );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtOpenChannelConfig(
                                EventLogHandle session,
                                [MarshalAs(UnmanagedType.LPWStr)]String channelPath,
                                int flags
                                        );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtSaveChannelConfig(
                                EventLogHandle channelConfig,
                                int flags
                                        );


            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtSetChannelConfigProperty(
                                EventLogHandle channelConfig,
                                [MarshalAs(UnmanagedType.I4)]EvtChannelConfigPropertyId propertyId,
                                int flags,
                                ref EvtVariant propertyValue
                                        );


            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetChannelConfigProperty(
                                EventLogHandle channelConfig,
                                [MarshalAs(UnmanagedType.I4)]EvtChannelConfigPropertyId propertyId,
                                int flags,
                                int propertyValueBufferSize,
                                IntPtr propertyValueBuffer,
                                out int propertyValueBufferUsed
                                       );

            //Log Information Native Api

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtOpenLog(
                                EventLogHandle session,
                                [MarshalAs(UnmanagedType.LPWStr)] string path,
                                [MarshalAs(UnmanagedType.I4)]PathType flags
                                        );


            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtGetLogInfo(
                                EventLogHandle log,
                                [MarshalAs(UnmanagedType.I4)]EvtLogPropertyId propertyId,
                                int propertyValueBufferSize,
                                IntPtr propertyValueBuffer,
                                out int propertyValueBufferUsed
                                        );

            //LOG MANIPULATION

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtExportLog(
                                EventLogHandle session,
                                [MarshalAs(UnmanagedType.LPWStr)]string channelPath,
                                [MarshalAs(UnmanagedType.LPWStr)]string query,
                                [MarshalAs(UnmanagedType.LPWStr)]string targetFilePath,
                                int flags
                                            );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtArchiveExportedLog(
                                EventLogHandle session,
                                [MarshalAs(UnmanagedType.LPWStr)]string logFilePath,
                                int locale,
                                int flags
                                            );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtClearLog(
                                EventLogHandle session,
                                [MarshalAs(UnmanagedType.LPWStr)]string channelPath,
                                [MarshalAs(UnmanagedType.LPWStr)]string targetFilePath,
                                int flags
                                            );

            //RENDERING
            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtCreateRenderContext(
                                Int32 valuePathsCount,
                                [MarshalAs(UnmanagedType.LPArray,ArraySubType = UnmanagedType.LPWStr)]
                                String[] valuePaths,
                                [MarshalAs(UnmanagedType.I4)]EvtRenderContextFlags flags
                                        );

            [DllImport(WEVTAPI, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtRender(
                                EventLogHandle context,
                                EventLogHandle eventHandle,
                                EvtRenderFlags flags,
                                int buffSize,
                                [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder buffer,
                                out int buffUsed,
                                out int propCount
                                            );


            [DllImport(WEVTAPI, EntryPoint = "EvtRender", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtRender(
                                EventLogHandle context,
                                EventLogHandle eventHandle,
                                EvtRenderFlags flags,
                                int buffSize,
                                IntPtr buffer,
                                out int buffUsed,
                                out int propCount
                                            );


            [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
            internal struct EvtStringVariant {
                [MarshalAs(UnmanagedType.LPWStr),FieldOffset(0)]
                public string StringVal;
                [FieldOffset(8)]
                public UInt32 Count;
                [FieldOffset(12)]
                public UInt32 Type;
            };

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtFormatMessage(
                                 EventLogHandle publisherMetadataHandle,
                                 EventLogHandle eventHandle,
                                 uint messageId,
                                 int valueCount,
                                 EvtStringVariant [] values,
                                 [MarshalAs(UnmanagedType.I4)]EvtFormatMessageFlags flags,
                                 int bufferSize,
                                 [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder buffer,
                                 out int bufferUsed
                                            );

            [DllImport(WEVTAPI, EntryPoint = "EvtFormatMessage", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtFormatMessageBuffer(
                                 EventLogHandle publisherMetadataHandle,
                                 EventLogHandle eventHandle,
                                 uint messageId,
                                 int valueCount,
                                 IntPtr values,
                                 [MarshalAs(UnmanagedType.I4)]EvtFormatMessageFlags flags,
                                 int bufferSize,
                                 IntPtr buffer,
                                 out int bufferUsed
                                            );

            //SESSION
            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtOpenSession(
                                [MarshalAs(UnmanagedType.I4)]EvtLoginClass loginClass,
                                ref EvtRpcLogin login,
                                int timeout,
                                int flags
                                            );

            //BOOKMARK
            [DllImport(WEVTAPI, EntryPoint = "EvtCreateBookmark", CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            internal static extern EventLogHandle EvtCreateBookmark(
                                [MarshalAs(UnmanagedType.LPWStr)] string bookmarkXml
                                            );

            [DllImport(WEVTAPI, CharSet = CharSet.Auto, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EvtUpdateBookmark(
                                EventLogHandle bookmark,
                                EventLogHandle eventHandle
                                            );
            //
            // EventLog
            // 

            //
            // Memory Mapped File
            //
            [StructLayout(LayoutKind.Sequential)]
#pragma warning disable 618 // Ssytem.Core still uses SecurityRuleSet.Level1
            [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
            internal unsafe struct MEMORY_BASIC_INFORMATION {
                internal void* BaseAddress;
                internal void* AllocationBase;
                internal uint AllocationProtect;
                internal UIntPtr RegionSize;
                internal uint State;
                internal uint Protect;
                internal uint Type;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SYSTEM_INFO {
                internal int dwOemId;    // This is a union of a DWORD and a struct containing 2 WORDs.
                internal int dwPageSize;
                internal IntPtr lpMinimumApplicationAddress;
                internal IntPtr lpMaximumApplicationAddress;
                internal IntPtr dwActiveProcessorMask;
                internal int dwNumberOfProcessors;
                internal int dwProcessorType;
                internal int dwAllocationGranularity;
                internal short wProcessorLevel;
                internal short wProcessorRevision;
            }

            [DllImport(KERNEL32, SetLastError = true)]
            [SecurityCritical]
            internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

            [DllImport(KERNEL32, ExactSpelling = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

            [DllImport(KERNEL32, SetLastError = true)]
            [SecurityCritical]
            internal static extern int GetFileSize(
                                SafeMemoryMappedFileHandle hFile, 
                                out int highSize
                                );
    
            [DllImport(KERNEL32, SetLastError = true)]
            [SecurityCritical]
            unsafe internal static extern IntPtr VirtualQuery(
                                SafeMemoryMappedViewHandle address, 
                                ref MEMORY_BASIC_INFORMATION buffer, 
                                IntPtr sizeOfBuffer
                                );

            [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
            [SecurityCritical]
            internal static extern SafeMemoryMappedFileHandle CreateFileMapping(
                                SafeFileHandle hFile, 
                                SECURITY_ATTRIBUTES lpAttributes, 
                                int fProtect, 
                                int dwMaximumSizeHigh, 
                                int dwMaximumSizeLow, 
                                String lpName
                                );

            [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            unsafe internal static extern bool FlushViewOfFile(
                                byte* lpBaseAddress, 
                                IntPtr dwNumberOfBytesToFlush
                                );

            [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
            [SecurityCritical]
            internal static extern SafeMemoryMappedFileHandle OpenFileMapping(
                                int dwDesiredAccess, 
                                [MarshalAs(UnmanagedType.Bool)] 
                                bool bInheritHandle, 
                                string lpName
                                );

            [DllImport(KERNEL32, SetLastError = true, ExactSpelling = true)]
            [SecurityCritical]
            internal static extern SafeMemoryMappedViewHandle MapViewOfFile(
                                SafeMemoryMappedFileHandle handle,
                                int dwDesiredAccess, 
                                uint dwFileOffsetHigh, 
                                uint dwFileOffsetLow, 
                                UIntPtr dwNumberOfBytesToMap
                                );

            [DllImport(KERNEL32, SetLastError = true)]
            [SecurityCritical]
            unsafe internal static extern IntPtr VirtualAlloc(
                                SafeMemoryMappedViewHandle address, 
                                UIntPtr numBytes, 
                                int commitOrReserve, 
                                int pageProtectionMode
                                );

            [SecurityCritical]
            internal static bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer)
            {
                lpBuffer.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                return GlobalMemoryStatusExNative(ref lpBuffer);
            }
                                
            [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "GlobalMemoryStatusEx")]
            [SecurityCritical]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GlobalMemoryStatusExNative([In, Out] ref MEMORYSTATUSEX lpBuffer);

            [DllImport(KERNEL32, SetLastError = true)]
            [SecurityCritical]
            internal static unsafe extern bool CancelIoEx(SafeHandle handle, NativeOverlapped* lpOverlapped);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            internal struct MEMORYSTATUSEX {
                internal uint dwLength;
                internal uint dwMemoryLoad;
                internal ulong ullTotalPhys;
                internal ulong ullAvailPhys;
                internal ulong ullTotalPageFile;
                internal ulong ullAvailPageFile;
                internal ulong ullTotalVirtual;
                internal ulong ullAvailVirtual;
                internal ulong ullAvailExtendedVirtual;
            }
    }
}
