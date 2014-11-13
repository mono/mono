//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    [
    System.Runtime.InteropServices.ComVisible(false),
    System.Security.SuppressUnmanagedCodeSecurityAttribute()
    ]
    internal static class UnsafeNativeMethods {
        static internal readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /*
         * ADVAPI32.dll
         */
        [DllImport(ModName.ADVAPI32_FULL_NAME)]
        internal static extern int SetThreadToken(IntPtr threadref, IntPtr token);

        [DllImport(ModName.ADVAPI32_FULL_NAME)]
        internal static extern int RevertToSelf();

        public const int TOKEN_ALL_ACCESS   = 0x000f01ff;
        public const int TOKEN_EXECUTE      = 0x00020000;
        public const int TOKEN_READ         = 0x00020008;
        public const int TOKEN_IMPERSONATE  = 0x00000004;

        public const int ERROR_NO_TOKEN = 1008;

        [DllImport(ModName.ADVAPI32_FULL_NAME, SetLastError=true)]
        internal static extern int OpenThreadToken(IntPtr thread, int access, bool openAsSelf, ref IntPtr hToken);


        public const int OWNER_SECURITY_INFORMATION             = 0x00000001;
        public const int GROUP_SECURITY_INFORMATION             = 0x00000002;
        public const int DACL_SECURITY_INFORMATION              = 0x00000004;
        public const int SACL_SECURITY_INFORMATION              = 0x00000008;

        [DllImport(ModName.ADVAPI32_FULL_NAME, SetLastError=true, CharSet=CharSet.Unicode)]
        internal static extern int GetFileSecurity(string filename, int requestedInformation, byte[] securityDescriptor, int length, ref int lengthNeeded);

        [DllImport(ModName.ADVAPI32_FULL_NAME, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int LogonUser(String username, String domain, String password, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport(ModName.ADVAPI32_FULL_NAME, SetLastError = true, CharSet = CharSet.Unicode)]
        public extern static int ConvertStringSidToSid(string stringSid, out IntPtr pSid);

        [DllImport(ModName.ADVAPI32_FULL_NAME, SetLastError = true, CharSet = CharSet.Unicode)]
        public extern static int LookupAccountSid(string systemName, IntPtr pSid, StringBuilder szName, ref int nameSize, StringBuilder szDomain, ref int domainSize, ref int eUse);

        /*
         * ASPNET_STATE.EXE
         */

        [DllImport(ModName.STATE_FULL_NAME)]
        internal static extern void STWNDCloseConnection(IntPtr tracker);

        [DllImport(ModName.STATE_FULL_NAME)]
        internal static extern void STWNDDeleteStateItem(IntPtr stateItem);

        [DllImport(ModName.STATE_FULL_NAME)]
        internal static extern void STWNDEndOfRequest(IntPtr tracker);

        [DllImport(ModName.STATE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern void STWNDGetLocalAddress(IntPtr tracker, StringBuilder buf);

        [DllImport(ModName.STATE_FULL_NAME)]
        internal static extern int STWNDGetLocalPort(IntPtr tracker);

        [DllImport(ModName.STATE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern void STWNDGetRemoteAddress(IntPtr tracker, StringBuilder buf);

        [DllImport(ModName.STATE_FULL_NAME)]
        internal static extern int STWNDGetRemotePort(IntPtr tracker);


        [DllImport(ModName.STATE_FULL_NAME)]
        internal static extern bool STWNDIsClientConnected(IntPtr tracker);

        [DllImport(ModName.STATE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void STWNDSendResponse(IntPtr tracker, StringBuilder status, int statusLength,
                                                    StringBuilder headers, int headersLength, IntPtr unmanagedState);

        /*
         * KERNEL32.DLL
         */
        internal const int FILE_ATTRIBUTE_READONLY             = 0x00000001;
        internal const int FILE_ATTRIBUTE_HIDDEN               = 0x00000002;
        internal const int FILE_ATTRIBUTE_SYSTEM               = 0x00000004;
        internal const int FILE_ATTRIBUTE_DIRECTORY            = 0x00000010;
        internal const int FILE_ATTRIBUTE_ARCHIVE              = 0x00000020;
        internal const int FILE_ATTRIBUTE_DEVICE               = 0x00000040;
        internal const int FILE_ATTRIBUTE_NORMAL               = 0x00000080;
        internal const int FILE_ATTRIBUTE_TEMPORARY            = 0x00000100;
        internal const int FILE_ATTRIBUTE_SPARSE_FILE          = 0x00000200;
        internal const int FILE_ATTRIBUTE_REPARSE_POINT        = 0x00000400;
        internal const int FILE_ATTRIBUTE_COMPRESSED           = 0x00000800;
        internal const int FILE_ATTRIBUTE_OFFLINE              = 0x00001000;
        internal const int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED  = 0x00002000;
        internal const int FILE_ATTRIBUTE_ENCRYPTED            = 0x00004000;

        internal const int DELETE                           = 0x00010000;
        internal const int READ_CONTROL                     = 0x00020000;
        internal const int WRITE_DAC                        = 0x00040000;
        internal const int WRITE_OWNER                      = 0x00080000;
        internal const int SYNCHRONIZE                      = 0x00100000;

        internal const int STANDARD_RIGHTS_REQUIRED         = 0x000F0000;

        internal const int STANDARD_RIGHTS_READ             = READ_CONTROL;
        internal const int STANDARD_RIGHTS_WRITE            = READ_CONTROL;
        internal const int STANDARD_RIGHTS_EXECUTE          = READ_CONTROL;

        internal const int GENERIC_READ = unchecked(((int)0x80000000));

        internal const int STANDARD_RIGHTS_ALL              = 0x001F0000;
        internal const int SPECIFIC_RIGHTS_ALL              = 0x0000FFFF;

        internal const int FILE_SHARE_READ = 0x00000001;
        internal const int FILE_SHARE_WRITE = 0x00000002;
        internal const int FILE_SHARE_DELETE = 0x00000004;

        internal const int OPEN_EXISTING = 3;
        internal const int OPEN_ALWAYS = 4;

        internal const int FILE_FLAG_WRITE_THROUGH = unchecked((int)0x80000000);
        internal const int FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const int FILE_FLAG_NO_BUFFERING = 0x20000000;
        internal const int FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        internal const int FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
        internal const int FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
        internal const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        internal const int FILE_FLAG_POSIX_SEMANTICS = 0x01000000;

        // Win32 Structs in N/Direct style
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct WIN32_FIND_DATA {
            internal uint dwFileAttributes;
            // ftCreationTime was a by-value FILETIME structure
            internal uint ftCreationTime_dwLowDateTime ;
            internal uint ftCreationTime_dwHighDateTime;
            // ftLastAccessTime was a by-value FILETIME structure
            internal uint ftLastAccessTime_dwLowDateTime;
            internal uint ftLastAccessTime_dwHighDateTime;
            // ftLastWriteTime was a by-value FILETIME structure
            internal uint ftLastWriteTime_dwLowDateTime;
            internal uint ftLastWriteTime_dwHighDateTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            internal string   cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
            internal string   cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_FILE_ATTRIBUTE_DATA {
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

        [StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_BY_HANDLE_FILE_INFORMATION  {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint volumeSerialNumber;
            internal uint fileSizeHigh;
            internal uint fileSizeLow;
            internal uint numberOfLinks;
            internal uint fileIndexHigh;
            internal uint fileIndexLow;
        }

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int lstrlenW(IntPtr ptr);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Ansi)]
        internal static extern int lstrlenA(IntPtr ptr);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool MoveFileEx(string oldFilename, string newFilename, UInt32 flags);

        [DllImport(ModName.KERNEL32_FULL_NAME, SetLastError=true)]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport(ModName.KERNEL32_FULL_NAME, SetLastError=true)]
        internal static extern bool FindClose(IntPtr hndFindFile);

        [DllImport(ModName.KERNEL32_FULL_NAME, SetLastError=true, CharSet=CharSet.Unicode)]
        internal static extern IntPtr FindFirstFile(
                    string pFileName, out WIN32_FIND_DATA pFindFileData);

        [DllImport(ModName.KERNEL32_FULL_NAME, SetLastError=true, CharSet=CharSet.Unicode)]
        internal static extern bool FindNextFile(
                    IntPtr hndFindFile, out WIN32_FIND_DATA pFindFileData);

        internal const int GetFileExInfoStandard = 0;

        [DllImport(ModName.KERNEL32_FULL_NAME, SetLastError=true, CharSet=CharSet.Unicode)]
        internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, out WIN32_FILE_ATTRIBUTE_DATA data);

#if !FEATURE_PAL // FEATURE_PAL native imports
        [DllImport(ModName.KERNEL32_FULL_NAME)]
        internal  extern static int GetProcessAffinityMask(
                IntPtr handle,
                out IntPtr processAffinityMask,
                out IntPtr systemAffinityMask);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        internal  extern static int GetComputerName(StringBuilder nameBuffer, ref int bufferSize);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        internal /*public*/ extern static int GetModuleFileName(IntPtr module, StringBuilder filename, int size);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        internal /*public*/ extern static IntPtr GetModuleHandle(string moduleName);

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct SYSTEM_INFO {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };


        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void GetSystemInfo(out SYSTEM_INFO si);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LoadLibrary(string libFilename);

        [DllImport(ModName.KERNEL32_FULL_NAME, SetLastError=true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LockResource(IntPtr hResData);

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        public extern static IntPtr LocalFree(IntPtr pMem);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct MEMORYSTATUSEX {
            internal int dwLength;
            internal int dwMemoryLoad;
            internal long ullTotalPhys;
            internal long ullAvailPhys;
            internal long ullTotalPageFile;
            internal long ullAvailPageFile;
            internal long ullTotalVirtual;
            internal long ullAvailVirtual;
            internal long ullAvailExtendedVirtual;

            internal  void Init() {
                dwLength = Marshal.SizeOf(typeof(UnsafeNativeMethods.MEMORYSTATUSEX));
            }
        }

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode)]
        internal  extern static int GlobalMemoryStatusEx(ref MEMORYSTATUSEX memoryStatusEx);

#else // !FEATURE_PAL
        internal static int GetProcessAffinityMask(
            IntPtr handle,
            out IntPtr processAffinityMask,
            out IntPtr systemAffinityMask)
        {
            // ROTORTODO - PAL should supply GetProcessAffinityMask

            // The only code that calls here is in SystemInfo::GetNumProcessCPUs and
            // it fails graciously if we return 0
            processAffinityMask = IntPtr.Zero;
            systemAffinityMask  = IntPtr.Zero;
            return 0; // fail
        }

        internal static IntPtr GetModuleHandle(string moduleName)
        {
            // ROTORTODO
            // So we never find any modules, so what?  :-)
            return IntPtr.Zero;
        }

        internal static int GlobalMemoryStatusEx(ref MEMORYSTATUSEX memoryStatusEx)
        {
            // ROTORTODO
            // This API is called from two places in CacheMemoryTotalMemoryPressure
            // Does it fail gracefully if the API fails?
            return 0;
        }

        internal static void AppDomainRestart(string appId)
        {
            // ROTORTODO
            // Do Nothing
        }

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true, EntryPoint="PAL_GetUserTempDirectoryW")]
        internal extern static bool GetUserTempDirectory(DeploymentDirectoryType ddt, StringBuilder sb, ref UInt32 length);

        // The order should be the same as in rotor_pal.h
        internal enum DeploymentDirectoryType
        {
            ddtInstallationDependentDirectory = 0,
            ddtInstallationIndependentDirectory
        }

        [DllImport(ModName.KERNEL32_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true, EntryPoint="PAL_GetMachineConfigurationDirectoryW")]
        internal extern static bool GetMachineConfigurationDirectory(StringBuilder sb, ref UInt32 length);

#endif // !FEATURE_PAL


        [DllImport(ModName.KERNEL32_FULL_NAME)]
        internal static extern IntPtr GetCurrentThread();

        /*
         * webengine.dll
         */

#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern void AppDomainRestart(string appId);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int AspCompatProcessRequest(AspCompatCallback callback, [MarshalAs(UnmanagedType.Interface)] Object context, bool sharedActivity, int activityHash);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int AspCompatOnPageStart([MarshalAs(UnmanagedType.Interface)] Object obj);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int AspCompatOnPageEnd();

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int AspCompatIsApartmentComponent([MarshalAs(UnmanagedType.Interface)] Object obj);

#endif // !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int AttachDebugger(string clsId, string sessId, IntPtr userToken);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int ChangeAccessToKeyContainer(string containerName, string accountName, string csp, int options);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int CookieAuthParseTicket (byte []         pData,
                                                        int             iDataLen,
                                                        StringBuilder   szName,
                                                        int             iNameLen,
                                                        StringBuilder   szData,
                                                        int             iUserDataLen,
                                                        StringBuilder   szPath,
                                                        int             iPathLen,
                                                        byte []         pBytes,
                                                        long []         pDates);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int CookieAuthConstructTicket (byte []         pData,
                                                            int             iDataLen,
                                                            string          szName,
                                                            string          szData,
                                                            string          szPath,
                                                            byte []         pBytes,
                                                            long []         pDates);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern IntPtr CreateUserToken(string name, string password, int fImpersonationToken, StringBuilder strError, int iErrorSize);

        internal const uint FILE_NOTIFY_CHANGE_FILE_NAME    = 0x00000001;
        internal const uint FILE_NOTIFY_CHANGE_DIR_NAME     = 0x00000002;
        internal const uint FILE_NOTIFY_CHANGE_ATTRIBUTES   = 0x00000004;
        internal const uint FILE_NOTIFY_CHANGE_SIZE         = 0x00000008;
        internal const uint FILE_NOTIFY_CHANGE_LAST_WRITE   = 0x00000010;
        internal const uint FILE_NOTIFY_CHANGE_LAST_ACCESS  = 0x00000020;
        internal const uint FILE_NOTIFY_CHANGE_CREATION     = 0x00000040;
        internal const uint FILE_NOTIFY_CHANGE_SECURITY     = 0x00000100;

        internal const uint RDCW_FILTER_FILE_AND_DIR_CHANGES =
             FILE_NOTIFY_CHANGE_FILE_NAME |
             FILE_NOTIFY_CHANGE_DIR_NAME |
             FILE_NOTIFY_CHANGE_CREATION |
             FILE_NOTIFY_CHANGE_SIZE |
             FILE_NOTIFY_CHANGE_LAST_WRITE |
             FILE_NOTIFY_CHANGE_SECURITY;


        internal const uint RDCW_FILTER_FILE_CHANGES =
             FILE_NOTIFY_CHANGE_FILE_NAME |
             FILE_NOTIFY_CHANGE_CREATION |
             FILE_NOTIFY_CHANGE_SIZE |
             FILE_NOTIFY_CHANGE_LAST_WRITE |
             FILE_NOTIFY_CHANGE_SECURITY;

        internal const uint RDCW_FILTER_DIR_RENAMES = FILE_NOTIFY_CHANGE_DIR_NAME;

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void GetDirMonConfiguration(out int FCNMode);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void DirMonClose(HandleRef dirMon, bool fNeedToDispose);

#if !FEATURE_PAL // FEATURE_PAL does not enable file change notification
        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int DirMonOpen(string dir, string appId, bool watchSubtree, uint notifyFilter, int fcnMode, NativeFileChangeNotification callback, out IntPtr pCompletion);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int GrowFileNotificationBuffer( string appId, bool fWatchSubtree );
#endif // !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void EcbFreeExecUrlEntityInfo(IntPtr pEntity);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetBasics(IntPtr pECB, byte[] buffer, int size, int[] contentInfo);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetBasicsContentInfo(IntPtr pECB, int[] contentInfo);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetTraceFlags(IntPtr pECB, int[] contentInfo);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet = CharSet.Unicode)]
        internal static extern int EcbEmitSimpleTrace(IntPtr pECB, int type, string eventData);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet = CharSet.Unicode)]
        internal static extern int EcbEmitWebEventTrace(
            IntPtr pECB,
            int webEventType,
            int fieldCount,
            string[] fieldNames,
            int[] fieldTypes,
            string[] fieldData);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetClientCertificate(IntPtr pECB, byte[] buffer, int size, int [] pInts, long [] pDates);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetExecUrlEntityInfo(int entityLength, byte[] entity, out IntPtr ppEntity);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetTraceContextId(IntPtr pECB, out Guid traceContextId);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int EcbGetServerVariable(IntPtr pECB, string name, byte[] buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetServerVariableByIndex(IntPtr pECB, int nameIndex, byte[] buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int EcbGetQueryString(IntPtr pECB, int encode, StringBuilder buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int EcbGetUnicodeServerVariable(IntPtr pECB, string name, IntPtr buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetUnicodeServerVariableByIndex(IntPtr pECB, int nameIndex, IntPtr buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetUnicodeServerVariables(IntPtr pECB, IntPtr buffer, int bufferSizeInChars, int[] serverVarLengths, int serverVarCount, int startIndex, ref int requiredSize);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetVersion(IntPtr pECB);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetQueryStringRawBytes(IntPtr pECB, byte[] buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetPreloadedPostedContent(IntPtr pECB, byte[] bytes, int offset, int bufferSize);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbGetAdditionalPostedContent(IntPtr pECB, byte[] bytes, int offset, int bufferSize);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbReadClientAsync(IntPtr pECB, int dwBytesToRead, AsyncCompletionCallback pfnCallback);

#if !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbFlushCore(IntPtr    pECB,
                                              byte[]    status,
                                              byte[]    header,
                                              int       keepConnected,
                                              int       totalBodySize,
                                              int       numBodyFragments,
                                              IntPtr[]  bodyFragments,
                                              int[]     bodyFragmentLengths,
                                              int       doneWithSession,
                                              int       finalStatus,
                                              int       kernelCache,
                                              int       async,
                                              ISAPIAsyncCompletionCallback asyncCompletionCallback);

#endif // !FEATURE_PAL
        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbIsClientConnected(IntPtr pECB);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbCloseConnection(IntPtr pECB);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int EcbMapUrlToPath(IntPtr pECB, string url, byte[] buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern IntPtr EcbGetImpersonationToken(IntPtr pECB, IntPtr processHandle);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern IntPtr EcbGetVirtualPathToken(IntPtr pECB, IntPtr processHandle);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int EcbAppendLogParameter(IntPtr pECB, string logParam);

#if !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int EcbExecuteUrlUnicode(IntPtr pECB,
                                                string url,
                                                string method,
                                                string childHeaders,
                                                bool   sendHeaders,
                                                bool   addUserIndo,
                                                IntPtr token,
                                                string name,
                                                string authType,
                                                IntPtr pEntity,
                                                ISAPIAsyncCompletionCallback asyncCompletionCallback);

#endif // !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void InvalidateKernelCache(string key);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void FreeFileSecurityDescriptor(IntPtr securityDesciptor);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr GetFileHandleForTransmitFile(string strFile);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern IntPtr GetFileSecurityDescriptor(string strFile);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int GetGroupsForUser(IntPtr token, StringBuilder allGroups, int allGrpSize, StringBuilder error, int errorSize);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int GetHMACSHA1Hash(byte[] data1, int dataOffset1, int dataSize1, byte[] data2, int dataSize2,
                                                   byte[] innerKey, int innerKeySize, byte[] outerKey, int outerKeySize,
                                                   byte[] hash, int hashSize);

#if !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int GetPrivateBytesIIS6(out long privatePageCount, bool nocache);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int GetProcessMemoryInformation(uint pid, out uint privatePageCount, out uint peakPagefileUsage, bool nocache);

#else // !FEATURE_PAL
        internal static int GetProcessMemoryInformation(uint pid, out uint privatePageCount, out uint peakPagefileUsage, bool nocache)
        {
            // ROTORTODO
            // called from CacheMemoryPrivateBytesPressure.GetCurrentPressure;
            // returning 0 causes it to ignore memory pressure
            privatePageCount = 0;
            peakPagefileUsage = 0;
            return 0;
        }
#endif // !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int GetSHA1Hash(byte[] data, int dataSize,
                                               byte[] hash, int hashSize);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int GetW3WPMemoryLimitInKB();

        [DllImport(ModName.ENGINE_FULL_NAME)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "This isn't a dangerous method.")]
        internal static extern void SetClrThreadPoolLimits(int maxWorkerThreads, int maxIoThreads, bool autoConfig);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void SetMinRequestsExecutingToDetectDeadlock(int minRequestsExecutingToDetectDeadlock);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void InitializeLibrary(bool reduceMaxThreads);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void PerfCounterInitialize();

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void InitializeHealthMonitor(int deadlockIntervalSeconds, int requestQueueLimit);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int IsAccessToFileAllowed(IntPtr securityDesciptor, IntPtr iThreadToken, int iAccess);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int IsUserInRole(IntPtr token, string rolename, StringBuilder error, int errorSize);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void UpdateLastActivityTimeForHealthMonitor();

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern int GetCredentialFromRegistry(String strRegKey, StringBuilder buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, BestFitMapping=false)]
        internal static extern int EcbGetChannelBindingToken(IntPtr pECB, out IntPtr token, out int tokenSize);

        /////////////////////////////////////////////////////////////////////////////
        // List of functions supported by PMCallISAPI
        //
        // ATTENTION!!
        // If you change this list, make sure it is in [....] with the
        // CallISAPIFunc enum in ecbdirect.h
        //
        internal enum CallISAPIFunc : int {
            GetSiteServerComment = 1,
            RestrictIISFolders = 2,
            CreateTempDir = 3,
            GetAutogenKeys = 4,
            GenerateToken  = 5
        };

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EcbCallISAPI(IntPtr pECB, UnsafeNativeMethods.CallISAPIFunc iFunction, byte[] bufferIn, int sizeIn, byte[] bufferOut, int sizeOut);

        // Constants as defined in ndll.h
        public const int RESTRICT_BIN          =0x00000001;

        /////////////////////////////////////////////////////////////////////////////
        // Passport Auth
        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern  int PassportVersion();

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int PassportCreateHttpRaw(
                string      szRequestLine,
                string      szHeaders,
                int         fSecure,
                StringBuilder szBufOut,
                int         dwRetBufSize,
                ref IntPtr  passportManager);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern  int    PassportTicket(
                IntPtr pManager,
                string     szAttr,
                out object  pReturn);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern  int    PassportGetCurrentConfig(
                IntPtr pManager,
                string     szAttr,
                out object   pReturn);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern  int    PassportLogoutURL(
            IntPtr pManager,
            string     szReturnURL,
            string     szCOBrandArgs,
            int         iLangID,
            string     strDomain,
            int         iUseSecureAuth,
            StringBuilder      szAuthVal,
            int         iAuthValSize);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern  int       PassportGetOption(
            IntPtr pManager,
            string     szOption,
            out Object   vOut);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern  int    PassportSetOption(
            IntPtr pManager,
            string     szOption,
            Object     vOut);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern  int    PassportGetLoginChallenge(
                IntPtr pManager,
                string     szRetURL,
                int         iTimeWindow,
                int        fForceLogin,
                string     szCOBrandArgs,
                int         iLangID,
                string     strNameSpace,
                int         iKPP,
                int         iUseSecureAuth,
                object     vExtraParams,
                StringBuilder      szOut,
                int         iOutSize);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern  int    PassportHexPUID(
                IntPtr pManager,
                StringBuilder      szOut,
                int         iOutSize);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int PassportCreate     (string              szQueryStrT,
                                                       string              szQueryStrP,
                                                       string              szAuthCookie,
                                                       string              szProfCookie,
                                                       string              szProfCCookie,
                                                       StringBuilder       szAuthCookieRet,
                                                       StringBuilder       szProfCookieRet,
                                                       int                 iRetBufSize,
                                                        ref IntPtr passportManager);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int PassportAuthURL (
                IntPtr              iPassport,
                string              szReturnURL,
                int                 iTimeWindow,
                int                 fForceLogin,
                string              szCOBrandArgs,
                int                 iLangID,
                string              strNameSpace,
                int                 iKPP,
                int                 iUseSecureAuth,
                StringBuilder       szAuthVal,
                int                 iAuthValSize);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int PassportAuthURL2 (
                IntPtr              iPassport,
                string              szReturnURL,
                int                 iTimeWindow,
                int                 fForceLogin,
                string              szCOBrandArgs,
                int                 iLangID,
                string              strNameSpace,
                int                 iKPP,
                int                 iUseSecureAuth,
                StringBuilder       szAuthVal,
                int                 iAuthValSize);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportGetError(IntPtr iPassport);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportDomainFromMemberName (
                IntPtr             iPassport,
                string             szDomain,
                StringBuilder      szMember,
                int                iMemberSize);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int   PassportGetFromNetworkServer (IntPtr iPassport);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportGetDomainAttribute   (
                IntPtr        iPassport,
                string        szAttributeName,
                int           iLCID,
                string        szDomain,
                StringBuilder szValue,
                int           iValueSize);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportHasProfile            (
                IntPtr      iPassport,
                string      szProfile);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportHasFlag            (
                IntPtr      iPassport,
                int         iFlagMask);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportHasConsent            (
                IntPtr      iPassport,
                int         iFullConsent,
                int         iNeedBirthdate);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportGetHasSavedPassword   (IntPtr      iPassport);


        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportHasTicket             (IntPtr      iPassport);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportIsAuthenticated       (
            IntPtr      iPassport,
            int         iTimeWindow,
            int         fForceLogin,
            int         iUseSecureAuth);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportLogoTag               (
                IntPtr        iPassport,
                string        szRetURL,
                int           iTimeWindow,
                int           fForceLogin,
                string        szCOBrandArgs,
                int           iLangID,
                int           fSecure,
                string        strNameSpace,
                int           iKPP,
                int           iUseSecureAuth,
                StringBuilder szValue,
                int           iValueSize);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportLogoTag2              (
                IntPtr        iPassport,
                string        szRetURL,
                int           iTimeWindow,
                int           fForceLogin,
                string        szCOBrandArgs,
                int           iLangID,
                int           fSecure,
                string        strNameSpace,
                int           iKPP,
                int           iUseSecureAuth,
                StringBuilder szValue,
                int           iValueSize);



        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportGetProfile            (
                IntPtr     iPassport,
                string     szProfile,
                out Object rOut);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportGetTicketAge(IntPtr   iPassport);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportGetTimeSinceSignIn(IntPtr iPassport);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void   PassportDestroy(IntPtr iPassport);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportCrypt(
                int            iFunctionID,
                string         szSrc,
                StringBuilder  szDest,
                int            iDestLength);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int   PassportCryptPut(
                int            iFunctionID,
                string         szSrc);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int   PassportCryptIsValid();

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int PostThreadPoolWorkItem(WorkItemCallback callback);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern IntPtr InstrumentedMutexCreate(string name);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void InstrumentedMutexDelete(HandleRef mutex);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int InstrumentedMutexGetLock(HandleRef mutex, int timeout);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int InstrumentedMutexReleaseLock(HandleRef mutex);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void InstrumentedMutexSetState(HandleRef mutex, int state);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern int IsapiAppHostMapPath(String appId, String virtualPath, StringBuilder buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern int IsapiAppHostGetAppPath(String aboPath, StringBuilder buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern int IsapiAppHostGetUncUser(String appId, StringBuilder usernameBuffer, int usernameSize, StringBuilder passwordBuffer, int passwordSize);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern int IsapiAppHostGetSiteName(String appId, StringBuilder buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern int IsapiAppHostGetSiteId(String site, StringBuilder buffer, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode, BestFitMapping=false)]
        internal static extern int IsapiAppHostGetNextVirtualSubdir(String aboPath, bool inApp, ref int index, StringBuilder sb, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, BestFitMapping=false)]
        internal static extern IntPtr BufferPoolGetPool(int bufferSize, int maxFreeListCount);

        [DllImport(ModName.ENGINE_FULL_NAME, BestFitMapping=false)]
        internal static extern IntPtr BufferPoolGetBuffer(IntPtr pool);

        [DllImport(ModName.ENGINE_FULL_NAME, BestFitMapping=false)]
        internal static extern void BufferPoolReleaseBuffer(IntPtr buffer);


        /*
         * ASPNET_WP.EXE
         */


        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetTraceContextId")]
        internal static extern int PMGetTraceContextId(IntPtr pMsg, out Guid traceContextId);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetHistoryTable")]
        internal static extern int PMGetHistoryTable (int       iRows,
                                                    int []   dwPIDArr,
                                                    int []   dwReqExecuted,
                                                    int []   dwReqPending,
                                                    int []   dwReqExecuting,
                                                    int []   dwReasonForDeath,
                                                    int []   dwPeakMemoryUsed,
                                                    long [] tmCreateTime,
                                                    long [] tmDeathTime);


        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetCurrentProcessInfo")]
        internal static extern int PMGetCurrentProcessInfo (ref int dwReqExecuted,
                                                          ref int dwReqExecuting,
                                                          ref int dwPeakMemoryUsed,
                                                          ref long tmCreateTime,
                                                          ref int pid);


        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetMemoryLimitInMB")]
        internal static extern int PMGetMemoryLimitInMB ();

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetBasics")]
        internal static extern int PMGetBasics(IntPtr pMsg, byte[] buffer, int size, int[] contentInfo);

        [DllImport(ModName.WP_FULL_NAME)]
        internal static extern int PMGetClientCertificate(IntPtr pMsg, byte[] buffer, int size, int [] pInts, long [] pDates);

        [DllImport(ModName.WP_FULL_NAME)]
        internal static extern long PMGetStartTimeStamp(IntPtr pMsg);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetAllServerVariables")]
        internal static extern int PMGetAllServerVariables(IntPtr pMsg, byte[] buffer, int size);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetQueryString", CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int PMGetQueryString(IntPtr pMsg, int encode, StringBuilder buffer, int size);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetQueryStringRawBytes")]
        internal static extern int PMGetQueryStringRawBytes(IntPtr pMsg, byte[] buffer, int size);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetPreloadedPostedContent")]
        internal static extern int PMGetPreloadedPostedContent(IntPtr pMsg, byte[] bytes, int offset, int bufferSize);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetAdditionalPostedContent")]
        internal static extern int PMGetAdditionalPostedContent(IntPtr pMsg, byte[] bytes, int offset, int bufferSize);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMEmptyResponse")]
        internal static extern int PMEmptyResponse(IntPtr pMsg);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMIsClientConnected")]
        internal static extern int PMIsClientConnected(IntPtr pMsg);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMCloseConnection")]
        internal static extern int PMCloseConnection(IntPtr pMsg);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMMapUrlToPath", CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int PMMapUrlToPath(IntPtr pMsg, string url, byte[] buffer, int size);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetImpersonationToken")]
        internal static extern IntPtr PMGetImpersonationToken(IntPtr pMsg);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMGetVirtualPathToken")]
        internal static extern IntPtr PMGetVirtualPathToken(IntPtr pMsg);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMAppendLogParameter", CharSet=CharSet.Ansi, BestFitMapping=false)]
        internal static extern int PMAppendLogParameter(IntPtr pMsg, string logParam);

        [DllImport(ModName.WP_FULL_NAME, EntryPoint="PMFlushCore")]
        internal static extern int PMFlushCore(IntPtr  pMsg,
                                             byte[]     status,
                                             byte[]     header,
                                             int        keepConnected,
                                             int        totalBodySize,
                                             int        bodyFragmentsOffset,
                                             int        numBodyFragments,
                                             IntPtr[]   bodyFragments,
                                             int[]      bodyFragmentLengths,
                                             int        doneWithSession,
                                             int        finalStatus);

        [DllImport(ModName.WP_FULL_NAME)]
        internal static extern int PMCallISAPI(IntPtr pECB, UnsafeNativeMethods.CallISAPIFunc iFunction, byte[] bufferIn, int sizeIn, byte[] bufferOut, int sizeOut);

        // perf counters support

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern IntPtr PerfOpenGlobalCounters();

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern IntPtr PerfOpenStateCounters();

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern PerfInstanceDataHandle PerfOpenAppCounters(string AppName);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void PerfCloseAppCounters(IntPtr pCounters);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void PerfIncrementCounter(IntPtr pCounters, int number);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void PerfDecrementCounter(IntPtr pCounters, int number);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void PerfIncrementCounterEx(IntPtr pCounters, int number, int increment);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void PerfSetCounter(IntPtr pCounters, int number, int increment);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int PerfGetCounter(IntPtr pCounters, int number);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void GetEtwValues(out int level, out int flags);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void TraceRaiseEventMgdHandler(int eventType, IntPtr pRequestContext, string data1, string data2, string data3, string data4);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void TraceRaiseEventWithEcb(int eventType, IntPtr ecb, string data1, string data2, string data3, string data4);

        [DllImport(ModName.WP_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void PMTraceRaiseEvent(int eventType, IntPtr pMsg, string data1, string data2, string data3, string data4);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int SessionNDConnectToService(string server);

        [StructLayout(LayoutKind.Sequential)]
        internal struct SessionNDMakeRequestResults {
            internal IntPtr         socket;
            internal int            httpStatus;
            internal int            timeout;
            internal int            contentLength;
            internal IntPtr         content;
            internal int            lockCookie;
            internal long           lockDate;
            internal int            lockAge;
            internal int            stateServerMajVer;
            internal int            actionFlags;
            internal int            lastPhase;
        };

        internal enum SessionNDMakeRequestPhase {
            Initialization = 0,
            Connecting,
            SendingRequest,
            ReadingResponse
        };


        internal enum StateProtocolVerb {
            GET = 1,
            PUT = 2,
            DELETE = 3,
            HEAD = 4,
        };

        internal enum StateProtocolExclusive {
            NONE = 0,
            ACQUIRE = 1,
            RELEASE = 2,
        };

        internal const int  StateProtocolFlagUninitialized = 0x00000001;

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Ansi, BestFitMapping=false, ThrowOnUnmappableChar=true)]
        internal static extern int SessionNDMakeRequest(
                HandleRef               socket,
                string                  server,
                int                     port,
                bool                    forceIPv6,
                int                     networkTimeout,
                StateProtocolVerb       verb,
                string                  uri,
                StateProtocolExclusive  exclusive,
                int                     extraFlags,
                int                     timeout,
                int                     lockCookie,
                byte[]                  body,
                int                     cb,
                bool                    checkVersion,
                out SessionNDMakeRequestResults results);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void SessionNDFreeBody(HandleRef body);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void SessionNDCloseConnection(HandleRef socket);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int TransactManagedCallback(TransactedExecCallback callback, int mode);

        [DllImport(ModName.ENGINE_FULL_NAME, SetLastError=true)]
        internal static extern bool IsValidResource(IntPtr hModule, IntPtr ip, int size);

        /*
         * Fusion API's (now coming from mscorwks.dll)
         */
        [DllImport(ModName.MSCORWKS_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int GetCachePath(int dwCacheFlags, StringBuilder pwzCachePath, ref int pcchPath);

#if !FEATURE_PAL
        [DllImport(ModName.MSCORWKS_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int DeleteShadowCache(string pwzCachePath, string pwzAppName);
#else // !FEATURE_PAL
        internal static int DeleteShadowCache(string pwzCachePath, string pwzAppName)
        {
            // ROTORTODO
            return 0;
        }
#endif // !FEATURE_PAL

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int InitializeWmiManager();

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet = CharSet.Unicode)]
        internal static extern int DoesKeyContainerExist(string containerName, string provider, int useMachineContainer);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct WmiData {
            internal int     eventType;

            // WebBaseEvent + WebProcessInformation + WebApplicationInformation
            internal int     eventCode;
            internal int     eventDetailCode;
            internal string  eventTime;
            internal string  eventMessage;
            internal string  eventId;
            internal string  sequenceNumber;
            internal string  occurrence;
            internal int     processId;
            internal string  processName;
            internal string  accountName;
            internal string  machineName;
            internal string  appDomain;
            internal string  trustLevel;
            internal string  appVirtualPath;
            internal string  appPath;

            internal string  details;

            // WebRequestInformation
            internal string  requestUrl;
            internal string  requestPath;
            internal string  userHostAddress;
            internal string  userName;
            internal bool    userAuthenticated;
            internal string  userAuthenticationType;
            internal string  requestThreadAccountName;

            // WebProcessStatistics
            internal string  processStartTime;
            internal int     threadCount;
            internal string  workingSet;
            internal string  peakWorkingSet;
            internal string  managedHeapSize;
            internal int     appdomainCount;
            internal int     requestsExecuting;
            internal int     requestsQueued;
            internal int     requestsRejected;

            // WebThreadInformation
            internal int     threadId;
            internal string  threadAccountName;
            internal string  stackTrace;
            internal bool    isImpersonating;

            // Exception
            internal string  exceptionType;
            internal string  exceptionMessage;

            internal string  nameToAuthenticate;

            // ViewStateException
            internal string  remoteAddress;
            internal string  remotePort;
            internal string  userAgent;
            internal string  persistedState;
            internal string  referer;
            internal string  path;
        };

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int RaiseWmiEvent(
                ref WmiData pWmiData,
                bool IsInAspCompatMode
            );

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern int RaiseEventlogEvent(
                int eventType, string[] dataFields, int size);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void LogWebeventProviderFailure(
                string  appUrl,
                string  providerName,
                string  exception);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern IntPtr GetEcb(
                IntPtr pHttpCompletion);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern void SetDoneWithSessionCalled(
                IntPtr pHttpCompletion);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void ReportUnhandledException(
                string  eventInfo);

        [DllImport(ModName.ENGINE_FULL_NAME, CharSet=CharSet.Unicode)]
        internal static extern void RaiseFileMonitoringEventlogEvent(
                string  eventInfo,
                string path,
                string appVirtualPath,
                int hr);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int StartPrefetchActivity(
            uint           ulActivityId);

        [DllImport(ModName.ENGINE_FULL_NAME)]
        internal static extern int EndPrefetchActivity(
            uint           ulActivityId);

        [DllImport(ModName.FILTER_FULL_NAME)]
        internal static extern IntPtr GetExtensionlessUrlAppendage();

        [DllImport(ModName.OLE32_FULL_NAME, CharSet = CharSet.Unicode)]
        internal static extern int CoCreateInstanceEx(ref Guid clsid, IntPtr pUnkOuter,
                                                         int dwClsContext, [In, Out] COSERVERINFO srv,
                                                         int num, [In, Out] MULTI_QI[] amqi);

        [DllImport(ModName.OLE32_FULL_NAME, CharSet = CharSet.Unicode)]
        internal static extern int CoCreateInstanceEx(ref Guid clsid, IntPtr pUnkOuter,
                                                         int dwClsContext, [In, Out] COSERVERINFO_X64 srv,
                                                         int num, [In, Out] MULTI_QI_X64[] amqi);
        [DllImport(ModName.OLE32_FULL_NAME, CharSet = CharSet.Unicode)]
        internal static extern int CoSetProxyBlanket(IntPtr pProxy, RpcAuthent authent, RpcAuthor author,
                                                        string serverprinc, RpcLevel level, RpcImpers
                                                        impers,
                                                        IntPtr ciptr, int dwCapabilities);

#if FEATURE_PAL // FEATURE_PAL-specific perf counter constants
        // PerfCounters support
        internal static int FILE_MAP_READ = 0x00000004;
        internal static int FILE_MAP_WRITE = 0x00000002; // same as FILE_MAP_ALL_ACCESS
        internal static uint PAGE_READONLY = 0x00000002;
        internal static uint PAGE_READWRITE = 0x00000004;
        internal static int ERROR_FILE_NOT_FOUND = 0x00000002;
#endif // FEATURE_pAL

    }
}

