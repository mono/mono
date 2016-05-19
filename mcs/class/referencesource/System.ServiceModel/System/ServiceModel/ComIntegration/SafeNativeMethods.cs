//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using SafeCloseHandle = System.IdentityModel.SafeCloseHandle;
    using SafeHGlobalHandle = System.IdentityModel.SafeHGlobalHandle;

    [Flags]
    enum CLSCTX
    {
        INPROC_SERVER = 0x1,
        INPROC_HANDLER = 0x2,
        LOCAL_SERVER = 0x4,
        INPROC_SERVER16 = 0x8,
        REMOTE_SERVER = 0x10,
        INPROC_HANDLER16 = 0x20,
        RESERVED1 = 0x40,
        RESERVED2 = 0x80,
        RESERVED3 = 0x100,
        RESERVED4 = 0x200,
        NO_CODE_DOWNLOAD = 0x400,
        RESERVED5 = 0x800,
        NO_CUSTOM_MARSHAL = 0x1000,
        ENABLE_CODE_DOWNLOAD = 0x2000,
        NO_FAILURE_LOG = 0x4000,
        DISABLE_AAA = 0x8000,
        ENABLE_AAA = 0x10000,
        FROM_DEFAULT_CONTEXT = 0x20000,
        ACTIVATE_32_BIT_SERVER = 0x40000,
        ACTIVATE_64_BIT_SERVER = 0x80000,
        INPROC = INPROC_SERVER | INPROC_HANDLER,
        SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
        ALL = SERVER | INPROC_HANDLER
    }

    [Flags]
    enum ComRights
    {
        EXECUTE = 0x01,
        EXECUTE_LOCAL = 0x02,
        EXECUTE_REMOTE = 0x04,
        ACTIVATE_LOCAL = 0x08,
        ACTIVATE_REMOTE = 0x10
    }
    
    enum TOKEN_INFORMATION_CLASS
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert
    }

    enum SecurityImpersonationLevel
    {
        Anonymous = 0,
        Identification = 1,
        Impersonation = 2,
        Delegation = 3,
    }

    enum TokenType
    {
        TokenPrimary = 1,
        TokenImpersonation
    }

    enum Win32Error
    {
        ERROR_SUCCESS = 0,
        ERROR_INSUFFICIENT_BUFFER = 122,
        ERROR_NO_TOKEN = 1008,
        ERROR_NONE_MAPPED = 1332,
        ERROR_NO_SUCH_DOMAIN = 1355,
    }

    enum EXTENDED_NAME_FORMAT
    {
        NameUnknown = 0,
        NameFullyQualifiedDN = 1,
        NameSamCompatible = 2,
        NameDisplay = 3,
        NameUniqueId = 6,
        NameCanonical = 7,
        NameUserPrincipalName = 8,
        NameCanonicalEx = 9,
        NameServicePrincipalName = 10,
        NameDnsDomainName = 12
    }

    [Flags]
    enum DSFlags : uint
    {
        DS_FORCE_REDISCOVERY = 0x00000001,
        DS_DIRECTORY_SERVICE_REQUIRED = 0x00000010,
        DS_DIRECTORY_SERVICE_PREFERRED = 0x00000020,
        DS_GC_SERVER_REQUIRED = 0x00000040,
        DS_PDC_REQUIRED = 0x00000080,
        DS_BACKGROUND_ONLY = 0x00000100,
        DS_IP_REQUIRED = 0x00000200,
        DS_KDC_REQUIRED = 0x00000400,
        DS_TIMESERV_REQUIRED = 0x00000800,
        DS_WRITABLE_REQUIRED = 0x00001000,
        DS_GOOD_TIMESERV_PREFERRED = 0x00002000,
        DS_AVOID_SELF = 0x00004000,
        DS_ONLY_LDAP_NEEDED = 0x00008000,
        DS_IS_FLAT_NAME = 0x00010000,
        DS_IS_DNS_NAME = 0x00020000,
        DS_TRY_NEXTCLOSEST_SITE = 0x00040000,
        DS_DIRECTORY_SERVICE_6_REQUIRED = 0x00080000,
        DS_WEB_SERVICE_REQUIRED = 0x00100000,
        DS_DIRECTORY_SERVICE_8_REQUIRED = 0x00200000,
        DS_RETURN_DNS_NAME = 0x40000000,
        DS_RETURN_FLAT_NAME = 0x80000000,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct TagVariant
    {
        public ushort vt;
        public ushort reserved1;
        public ushort reserved2;
        public ushort reserved3;
        public IntPtr ptr;
        public IntPtr pRecInfo;
    }

    static class HR
    {
        internal static readonly int S_OK = 0;
        internal static readonly int S_FALSE = 1;
        internal static readonly int MK_E_SYNTAX = unchecked((int)0x800401e4);
        internal static readonly int E_INVALIDARG = unchecked((int)0x80070057);
        internal static readonly int E_UNEXPECTED = unchecked((int)0x8000ffff);
        internal static readonly int DISP_E_UNKNOWNINTERFACE = unchecked((int)0x80020001);
        internal static readonly int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);
        internal static readonly int DISP_E_PARAMNOTFOUND = unchecked((int)0x80020004);
        internal static readonly int DISP_E_TYPEMISMATCH = unchecked((int)0x80020005);
        internal static readonly int DISP_E_UNKNOWNNAME = unchecked((int)0x80020006);
        internal static readonly int DISP_E_NONAMEDARGS = unchecked((int)0x80020007);
        internal static readonly int DISP_E_BADVARTYPE = unchecked((int)0x80020008);
        internal static readonly int DISP_E_EXCEPTION = unchecked((int)0x80020009);
        internal static readonly int DISP_E_OVERFLOW = unchecked((int)0x8002000A);
        internal static readonly int DISP_E_BADINDEX = unchecked((int)0x8002000B);
        internal static readonly int DISP_E_UNKNOWNLCID = unchecked((int)0x8002000C);
        internal static readonly int DISP_E_ARRAYISLOCKED = unchecked((int)0x8002000D);
        internal static readonly int DISP_E_BADPARAMCOUNT = unchecked((int)0x8002000E);
        internal static readonly int DISP_E_PARAMNOTOPTIONAL = unchecked((int)0x8002000F);
        internal static readonly int DISP_E_BADCALLEE = unchecked((int)0x80020010);
        internal static readonly int DISP_E_NOTACOLLECTION = unchecked((int)0x80020011);
        internal static readonly int DISP_E_DIVBYZERO = unchecked((int)0x80020012);
        internal static readonly int DISP_E_BUFFERTOOSMALL = unchecked((int)0x80020013);
        internal static readonly int RPC_E_TOO_LATE = unchecked((int)0x80010119);
        internal static readonly int RPC_NT_BINDING_HAS_NO_AUTH = unchecked((int)0x800706d2);
        internal static readonly int E_FAIL = unchecked((int)0x80040005);
        internal static readonly int COMADMIN_E_PARTITIONS_DISABLED = unchecked((int)0x80110824);
        internal static readonly int CONTEXT_E_NOTRANSACTION = unchecked((int)0x8004E027);
        internal static readonly int ERROR_BAD_IMPERSONATION_LEVEL = unchecked((int)(0x80070542));
    }

    static class InterfaceID
    {
        public static readonly Guid idISupportErrorInfo = new Guid("{df0b3d60-548f-101b-8e65-08002b2bd119}");
        public static readonly Guid idIDispatch = new Guid("00020400-0000-0000-C000-000000000046");
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LUID
    {
        internal uint LowPart;
        internal int HighPart;
    }


    [StructLayout(LayoutKind.Sequential)]
    struct TOKEN_STATISTICS
    {
        internal LUID TokenId;
        internal LUID AuthenticationId;
        internal Int64 ExpirationTime;
        internal uint TokenType;
        internal SecurityImpersonationLevel ImpersonationLevel;
        internal uint DynamicCharged;
        internal uint DynamicAvailable;
        internal uint GroupCount;
        internal uint PrivilegeCount;
        internal LUID ModifiedId;
    }

    [StructLayout(LayoutKind.Sequential)]
    class GENERIC_MAPPING
    {
        internal uint genericRead = 0;
        internal uint genericWrite = 0;
        internal uint genericExecute = 0;
        internal uint genericAll = 0;
    }

    [Flags]
    internal enum PrivilegeAttribute : uint
    {
        SE_PRIVILEGE_DISABLED = 0x00000000, // note that this is not defined in the header files
        SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001,
        SE_PRIVILEGE_ENABLED = 0x00000002,
        SE_PRIVILEGE_REMOVED = 0X00000004,
        SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LUID_AND_ATTRIBUTES
    {
        internal LUID Luid;
        internal PrivilegeAttribute Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    class PRIVILEGE_SET
    {
        internal uint PrivilegeCount = 1;
        internal uint Control = 0;
        internal LUID_AND_ATTRIBUTES Privilege;
    }

    [SuppressUnmanagedCodeSecurity]
    static class SafeNativeMethods
    {
        internal const String KERNEL32 = "kernel32.dll";
        internal const String ADVAPI32 = "advapi32.dll";
        internal const String OLE32 = "ole32.dll";
        internal const String OLEAUT32 = "oleaut32.dll";
        internal const String COMSVCS = "comsvcs.dll";
        internal const String SECUR32 = "secur32.dll";
        internal const String NETAPI32 = "netapi32.dll";

        internal const int ERROR_MORE_DATA = 0xEA;
        internal const int ERROR_SUCCESS = 0;
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int ERROR_NOT_SUPPORTED = 50;
        internal const int READ_CONTROL = 0x00020000;
        internal const int SYNCHRONIZE = 0x00100000;
        internal const int STANDARD_RIGHTS_READ = READ_CONTROL;
        internal const int STANDARD_RIGHTS_WRITE = READ_CONTROL;
        internal const int KEY_QUERY_VALUE = 0x0001;
        internal const int KEY_SET_VALUE = 0x0002;
        internal const int KEY_CREATE_SUB_KEY = 0x0004;
        internal const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
        internal const int KEY_NOTIFY = 0x0010;
        internal const int KEY_CREATE_LINK = 0x0020;
        internal const int KEY_READ = ((STANDARD_RIGHTS_READ |
                                                           KEY_QUERY_VALUE |
                                                           KEY_ENUMERATE_SUB_KEYS |
                                                           KEY_NOTIFY)
                                                          &
                                                          (~SYNCHRONIZE));

        internal const int KEY_WRITE = STANDARD_RIGHTS_WRITE |
                                                           KEY_SET_VALUE |
                                                           KEY_CREATE_SUB_KEY;

        internal const int REG_NONE = 0;     // No value type
        internal const int REG_SZ = 1;     // Unicode nul terminated string
        internal const int REG_EXPAND_SZ = 2;     // Unicode nul terminated string
        internal const int KEY_WOW64_32KEY = (0x0200);
        internal const int KEY_WOW64_64KEY = (0x0100);


        // (with environment variable references)
        internal const int REG_BINARY = 3;     // Free form binary
        internal const int REG_DWORD = 4;     // 32-bit number
        internal const int REG_DWORD_LITTLE_ENDIAN = 4;     // 32-bit number (same as REG_DWORD)
        internal const int REG_DWORD_BIG_ENDIAN = 5;     // 32-bit number
        internal const int REG_LINK = 6;     // Symbolic Link (unicode)
        internal const int REG_MULTI_SZ = 7;     // Multiple Unicode strings
        internal const int REG_RESOURCE_LIST = 8;     // Resource list in the resource map
        internal const int REG_FULL_RESOURCE_DESCRIPTOR = 9;   // Resource list in the hardware description
        internal const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
        internal const int REG_QWORD = 11;    // 64-bit number

        internal const int HWND_BROADCAST = 0xffff;
        internal const int WM_SETTINGCHANGE = 0x001A;


        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern int RegOpenKeyEx(RegistryHandle hKey, String lpSubKey,
                    int ulOptions, int samDesired, out RegistryHandle hkResult);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int RegSetValueEx(RegistryHandle hKey, String lpValueName,
                    int Reserved, int dwType, String val, int cbData);

        [DllImport(ADVAPI32, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int RegCloseKey(IntPtr handle);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int RegQueryValueEx(RegistryHandle hKey, String lpValueName,
                    int[] lpReserved, ref int lpType, [Out] byte[] lpData,
                    ref int lpcbData);
        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int RegEnumKey(RegistryHandle hKey, int index, StringBuilder lpName, ref int len);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int RegDeleteKey(RegistryHandle hKey, String lpValueName);


        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        DuplicateTokenEx(
            [In] SafeCloseHandle ExistingToken,
            [In] TokenAccessLevels DesiredAccess,
            [In] IntPtr TokenAttributes,
            [In] SecurityImpersonationLevel ImpersonationLevel,
            [In] TokenType TokenType,
            [Out] out SafeCloseHandle NewToken);


        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        AccessCheck(
            [In] byte[] SecurityDescriptor,
            [In] SafeCloseHandle ClientToken,
            [In] int DesiredAccess,
            [In] GENERIC_MAPPING GenericMapping,
            [Out] out PRIVILEGE_SET PrivilegeSet,
            [In, Out] ref uint PrivilegeSetLength,
            [Out] out uint GrantedAccess,
            [Out] out bool AccessStatus);


        [DllImport(ADVAPI32, SetLastError = true, EntryPoint = "ImpersonateAnonymousToken")]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        ImpersonateAnonymousUserOnCurrentThread(
            [In] IntPtr CurrentThread);

        [DllImport(ADVAPI32, SetLastError = true, EntryPoint = "OpenThreadToken")]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        OpenCurrentThreadToken(
            [In] IntPtr ThreadHandle,
            [In] TokenAccessLevels DesiredAccess,
            [In] bool OpenAsSelf,
            [Out] out SafeCloseHandle TokenHandle);

        [DllImport(ADVAPI32, SetLastError = true, EntryPoint = "SetThreadToken")]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        SetCurrentThreadToken(
            [In] IntPtr ThreadHandle,
            [In] SafeCloseHandle TokenHandle);

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr
        GetCurrentThread();

        [DllImport(KERNEL32, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int
        GetCurrentThreadId();

        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        RevertToSelf();

        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        GetTokenInformation(
            [In] SafeCloseHandle TokenHandle,
            [In] TOKEN_INFORMATION_CLASS TokenInformationClass,
            [In] SafeHandle TokenInformation,
            [Out] uint TokenInformationLength,
            [Out] out uint ReturnLength);

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr
        GetCurrentProcess();

        [DllImport(ADVAPI32, SetLastError = true, EntryPoint = "OpenProcessToken")]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
        GetCurrentProcessToken(
            [In]IntPtr ProcessHandle,
            [In]TokenAccessLevels DesiredAccess,
            [Out]out SafeCloseHandle TokenHandle);

        [DllImport(OLE32, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        [ResourceExposure(ResourceScope.None)]
        public static extern object CoCreateInstance(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
            [In] CLSCTX dwClsContext,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        [DllImport(OLE32, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IStream CreateStreamOnHGlobal(
            [In] SafeHGlobalHandle hGlobal,
            [In, MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease);

        [DllImport(OLE32, ExactSpelling = true, PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern SafeHGlobalHandle GetHGlobalFromStream(IStream stream);

        [DllImport(OLE32, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        [ResourceExposure(ResourceScope.None)]
        public static extern object CoGetObjectContext(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        [DllImport(COMSVCS, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        [ResourceExposure(ResourceScope.None)]
        public static extern object CoCreateActivity(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pIUnknown,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        [DllImport(OLE32, ExactSpelling = true, PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr CoSwitchCallContext(IntPtr newSecurityObject);

        [DllImport(KERNEL32, ExactSpelling = true, PreserveSig = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr GlobalLock(SafeHGlobalHandle hGlobal);

        [DllImport(KERNEL32, ExactSpelling = true, PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GlobalUnlock(SafeHGlobalHandle hGlobal);

        [DllImport(KERNEL32, ExactSpelling = true, PreserveSig = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr GlobalSize(SafeHGlobalHandle hGlobal);

        [DllImport(OLEAUT32,
           ExactSpelling = true,
           CharSet = CharSet.Unicode,
           PreserveSig = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int LoadRegTypeLib(ref Guid rguid, ushort major, ushort minor, int lcid,
             [MarshalAs(UnmanagedType.Interface)] out object typeLib);

        [DllImport(OLEAUT32,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            PreserveSig = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int SafeArrayGetDim(IntPtr pSafeArray);

        [DllImport(OLEAUT32,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            PreserveSig = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int SafeArrayGetElemsize(IntPtr pSafeArray);

        [DllImport(OLEAUT32,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int SafeArrayGetLBound(IntPtr pSafeArray, int cDims);
        [DllImport(OLEAUT32,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int SafeArrayGetUBound(IntPtr pSafeArray, int cDims);

        [DllImport(OLEAUT32,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr SafeArrayAccessData(IntPtr pSafeArray);

        [DllImport(OLEAUT32,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern void SafeArrayUnaccessData(IntPtr pSafeArray);

        [DllImport(SECUR32, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool TranslateName(string input, EXTENDED_NAME_FORMAT inputFormat, EXTENDED_NAME_FORMAT outputFormat, StringBuilder outputString, out uint size);

        [DllImport(NETAPI32, ExactSpelling = true, EntryPoint = "DsGetDcNameW", CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int DsGetDcName(
            [In] string computerName,
            [In] string domainName,
            [In] IntPtr domainGuid,
            [In] string siteName,
            [In] uint flags,
            [Out] out IntPtr domainControllerInfo);

        [DllImport(NETAPI32)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int NetApiBufferFree([In] IntPtr buffer);
    }

    internal static class InterfaceHelper
    {
        // only use this helper to get interfaces that are guaranteed to be supported
        internal static IntPtr GetInterfacePtrForObject(Guid iid, object obj)
        {
            IntPtr pUnk = Marshal.GetIUnknownForObject(obj);
            if (IntPtr.Zero == pUnk)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.UnableToRetrievepUnk)));
            }

            IntPtr ppv = IntPtr.Zero;
            int hr = Marshal.QueryInterface(pUnk, ref iid, out ppv);

            Marshal.Release(pUnk);

            if (hr != HR.S_OK)
            {
                throw Fx.AssertAndThrow("QueryInterface should succeed");
            }

            return ppv;
        }
    }

    internal class RegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal static readonly RegistryHandle HKEY_CLASSES_ROOT = new RegistryHandle(new IntPtr(unchecked((int)0x80000000)), false);
        internal static readonly RegistryHandle HKEY_CURRENT_USER = new RegistryHandle(new IntPtr(unchecked((int)0x80000001)), false);
        internal static readonly RegistryHandle HKEY_LOCAL_MACHINE = new RegistryHandle(new IntPtr(unchecked((int)0x80000002)), false);
        internal static readonly RegistryHandle HKEY_USERS = new RegistryHandle(new IntPtr(unchecked((int)0x80000003)), false);
        internal static readonly RegistryHandle HKEY_PERFORMANCE_DATA = new RegistryHandle(new IntPtr(unchecked((int)0x80000004)), false);
        internal static readonly RegistryHandle HKEY_CURRENT_CONFIG = new RegistryHandle(new IntPtr(unchecked((int)0x80000005)), false);
        internal static readonly RegistryHandle HKEY_DYN_DATA = new RegistryHandle(new IntPtr(unchecked((int)0x80000006)), false);

        [ResourceConsumption(ResourceScope.Machine)]
        static RegistryHandle GetHKCR()
        {
            RegistryHandle regHandle = null;
            int status = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"Software\Classes", 0, SafeNativeMethods.KEY_READ, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(status));
            }
            if (null == regHandle || regHandle.IsInvalid)
            {
                Fx.Assert("GetHKCR: RegOpenKeyEx returned null but with an invalid handle.");
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_INVALID_HANDLE));
            }

            return regHandle;
        }


        [ResourceConsumption(ResourceScope.Machine)]
        static RegistryHandle Get64bitHKCR()
        {
            RegistryHandle regHandle = null;
            int status = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"Software\Classes", 0, SafeNativeMethods.KEY_READ | SafeNativeMethods.KEY_WOW64_64KEY, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(status));
            }
            if (null == regHandle || regHandle.IsInvalid)
            {
                Fx.Assert("Get64bitHKCR: RegOpenKeyEx returned null but with an invalid handle.");
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_INVALID_HANDLE));
            }

            return regHandle;
        }

        [ResourceConsumption(ResourceScope.Machine)]
        static RegistryHandle Get32bitHKCR()
        {
            RegistryHandle regHandle = null;
            int status = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"Software\Classes", 0, SafeNativeMethods.KEY_READ | SafeNativeMethods.KEY_WOW64_32KEY, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(status));
            }
            if (null == regHandle || regHandle.IsInvalid)
            {
                Fx.Assert("Get64bitHKCR: RegOpenKeyEx returned null but with an invalid handle.");
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_INVALID_HANDLE));
            }
            return regHandle;
        }

        static RegistryHandle GetCorrectBitnessHive(bool is64bit)
        {
            if (is64bit && IntPtr.Size == 8) // No worries we are trying to open up a 64 bit hive just return 
                return GetHKCR();
            else if (is64bit && IntPtr.Size == 4) // we are running under wow get the 64 bit hive
                return Get64bitHKCR();
            else if (!is64bit && IntPtr.Size == 8) // we are running in 64 bit but need to open a 32 bit hive
                return Get32bitHKCR();
            else if (!is64bit && IntPtr.Size == 4)
                return GetHKCR();

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_NOT_SUPPORTED));
        }

        public static RegistryHandle GetBitnessHKCR(bool is64bit)
        {
            return GetCorrectBitnessHive(is64bit);
        }

        public static RegistryHandle GetCorrectBitnessHKLMSubkey(bool is64bit, string key)
        {
            if (is64bit && IntPtr.Size == 8) // No worries we are trying to open up a 64 bit hive just return 
                return GetHKLMSubkey(key);
            else if (is64bit && IntPtr.Size == 4) // we are running under wow get the 64 bit hive
                return Get64bitHKLMSubkey(key);
            else if (!is64bit && IntPtr.Size == 8) // we are running in 64 bit but need to open a 32 bit hive
                return Get32bitHKLMSubkey(key);
            else if (!is64bit && IntPtr.Size == 4)
                return GetHKLMSubkey(key);

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_NOT_SUPPORTED));
        }


        [ResourceConsumption(ResourceScope.Machine)]
        static RegistryHandle GetHKLMSubkey(string key)
        {
            RegistryHandle regHandle = null;
            int status = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, SafeNativeMethods.KEY_READ, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(status));
            }
            if (null == regHandle || regHandle.IsInvalid)
            {
                Fx.Assert("GetHKLMSubkey: RegOpenKeyEx returned null but with an invalid handle.");
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_INVALID_HANDLE));
            }
            return regHandle;

        }

        [ResourceConsumption(ResourceScope.Machine)]
        static RegistryHandle Get64bitHKLMSubkey(string key)
        {
            RegistryHandle regHandle = null;
            int status = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, SafeNativeMethods.KEY_READ | SafeNativeMethods.KEY_WOW64_64KEY, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(status));
            }
            if (null == regHandle || regHandle.IsInvalid)
            {
                Fx.Assert("Get64bitHKLMSubkey: RegOpenKeyEx returned null but with an invalid handle.");
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_INVALID_HANDLE));
            }
            return regHandle;
        }

        [ResourceConsumption(ResourceScope.Machine)]
        static RegistryHandle Get32bitHKLMSubkey(string key)
        {
            RegistryHandle regHandle = null;
            int status = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, SafeNativeMethods.KEY_READ | SafeNativeMethods.KEY_WOW64_32KEY, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(status));
            }
            if (null == regHandle || regHandle.IsInvalid)
            {
                Fx.Assert("Get32bitHKLMSubkey: RegOpenKeyEx returned null but with an invalid handle.");
                Utility.CloseInvalidOutSafeHandle(regHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(SafeNativeMethods.ERROR_INVALID_HANDLE));
            }
            return regHandle;
        }

        [ResourceConsumption(ResourceScope.Machine)]
        internal static RegistryHandle GetNativeHKLMSubkey(string subKey, bool writeable)
        {
            RegistryHandle regHandle = null;
            int samDesired = SafeNativeMethods.KEY_READ | SafeNativeMethods.KEY_WOW64_64KEY;

            if (writeable)
            {
                samDesired |= SafeNativeMethods.KEY_WRITE;
            }

            int status = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, subKey, 0, samDesired, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS || regHandle == null || regHandle.IsInvalid)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                return null;
            }
            return regHandle;
        }

        public RegistryHandle(IntPtr hKey, bool ownHandle)
            : base(ownHandle)
        {
            handle = hKey;
        }

        public RegistryHandle()
            : base(true)
        {
        }

        public bool DeleteKey(string key)
        {
            int status = SafeNativeMethods.RegDeleteKey(this, key);
            if (status == SafeNativeMethods.ERROR_SUCCESS)
                return true;
            else
                return false;
        }

        public void SetValue(string valName, string value)
        {
            int status = SafeNativeMethods.RegSetValueEx(this, valName, 0, SafeNativeMethods.REG_SZ, value, (value.Length * 2) + 2);
            if (status != SafeNativeMethods.ERROR_SUCCESS)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(status));
        }

        [ResourceConsumption(ResourceScope.Machine)]
        public RegistryHandle OpenSubKey(string subkey)
        {
            RegistryHandle regHandle = null;
            int status = SafeNativeMethods.RegOpenKeyEx(this, subkey, 0, SafeNativeMethods.KEY_READ, out regHandle);
            if (status != SafeNativeMethods.ERROR_SUCCESS || regHandle == null || regHandle.IsInvalid)
            {
                Utility.CloseInvalidOutSafeHandle(regHandle);
                return null;
            }
            return regHandle;
        }

        public string GetStringValue(string valName)
        {
            int type = 0;
            int datasize = 0;
            int ret = SafeNativeMethods.RegQueryValueEx(this, valName, null, ref type, (byte[])null, ref datasize);
            if (ret == SafeNativeMethods.ERROR_SUCCESS)
                if (type == SafeNativeMethods.REG_SZ)
                {
                    byte[] blob = new byte[datasize];
                    ret = SafeNativeMethods.RegQueryValueEx(this, valName, null, ref type, (byte[])blob, ref datasize);
                    UnicodeEncoding unicode = new UnicodeEncoding();
                    return unicode.GetString(blob);
                }
            return null;
        }
        public StringCollection GetSubKeyNames()
        {
            int ret = 0;
            int index = 0;
            StringCollection keyNames = new StringCollection();
            do
            {
                int lengthInChars = 0;
                ret = SafeNativeMethods.RegEnumKey(this, index, null, ref lengthInChars);
                if (ret == SafeNativeMethods.ERROR_MORE_DATA)
                {
                    StringBuilder keyName = new StringBuilder(lengthInChars + 1);
                    ret = SafeNativeMethods.RegEnumKey(this, index, keyName, ref lengthInChars);
                    if (ret == SafeNativeMethods.ERROR_SUCCESS)
                        keyNames.Add(keyName.ToString());
                }
                index++;
            }
            while (ret == SafeNativeMethods.ERROR_SUCCESS);
            return keyNames;

        }
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        internal unsafe object GetValue(string valName)
        {
            object retVal = null;
            int type = 0;
            int datasize = 0;
            int ret = SafeNativeMethods.RegQueryValueEx(this, valName, null, ref type, (byte[])null, ref datasize);
            if (SafeNativeMethods.ERROR_SUCCESS == ret)
            {
                byte[] blob = new byte[datasize];
                ret = SafeNativeMethods.RegQueryValueEx(this, valName, null, ref type, (byte[])blob, ref datasize);

                if (SafeNativeMethods.ERROR_SUCCESS == ret)
                {
                    UnicodeEncoding unicode = new UnicodeEncoding();
                    string stringVal = unicode.GetString(blob);

                    switch (type)
                    {
                        case (SafeNativeMethods.REG_BINARY):
                            retVal = blob;
                            break;

                        case (SafeNativeMethods.REG_DWORD):
                            fixed (byte* pBuffer = blob)
                            {
                                retVal = Marshal.ReadInt32((IntPtr)pBuffer);
                            }
                            break;

                        case (SafeNativeMethods.REG_MULTI_SZ):
                            retVal = stringVal.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                            break;

                        case (SafeNativeMethods.REG_QWORD):
                            fixed (byte* pBuffer = blob)
                            {
                                retVal = Marshal.ReadInt64((IntPtr)pBuffer);
                            }
                            break;

                        case (SafeNativeMethods.REG_EXPAND_SZ):
                        case (SafeNativeMethods.REG_SZ):
                            retVal = stringVal.Trim(new char[] { '\0' });
                            break;

                        default:
                            retVal = blob;
                            break;
                    }
                }
            }

            return retVal;
        }
        protected override bool ReleaseHandle()
        {
            if (SafeNativeMethods.RegCloseKey(handle) == SafeNativeMethods.ERROR_SUCCESS)
                return true;
            else
                return false;
        }
    }
}
