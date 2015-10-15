//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.Channels;
    using System.ServiceModel.ComIntegration;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
    using System.ServiceModel.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurityAttribute()]
    static class ListenerUnsafeNativeMethods
    {
        const string ADVAPI32 = "advapi32.dll";
        const string KERNEL32 = "kernel32.dll";

        internal const int OWNER_SECURITY_INFORMATION = 0x00000001;
        internal const int DACL_SECURITY_INFORMATION = 0x00000004;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_INSUFFICIENT_BUFFER = 122;
        internal const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
        internal const int PROCESS_QUERY_INFORMATION = 0x0400;
        internal const int PROCESS_DUP_HANDLE = 0x0040;
        internal const int READ_CONTROL = 0x00020000;
        internal const int TOKEN_QUERY = 0x0008;
        internal const int WRITE_DAC = 0x00040000;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x0020;

        // for some of these check out: %SDXROOT%\public\sdk\inc\winsvc.h
        internal const int SC_MANAGER_CONNECT = 0x0001;
        internal const int SC_STATUS_PROCESS_INFO = 0;
        internal const int SERVICE_QUERY_CONFIG = 0x0001;
        internal const int SERVICE_QUERY_STATUS = 0x0004;
        internal const int SERVICE_RUNNING = 0x00000004;
        internal const int SERVICE_START = 0x0010;
        internal const int SERVICE_START_PENDING = 0x00000002;

        [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool IsDebuggerPresent();

        [DllImport(KERNEL32, ExactSpelling = true)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern void DebugBreak();

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern bool AdjustTokenPrivileges(SafeCloseHandle tokenHandle, bool disableAllPrivileges, TOKEN_PRIVILEGES* newState, int bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool LookupAccountName(string systemName, string accountName, byte[] sid,
          ref uint cbSid, StringBuilder referencedDomainName, ref uint cchReferencedDomainName, out short peUse);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern bool LookupPrivilegeValue(IntPtr lpSystemName, string lpName, LUID* lpLuid);

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetKernelObjectSecurity(SafeCloseHandle handle, int securityInformation, [Out] byte[] pSecurityDescriptor, int nLength, out int lpnLengthNeeded);

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetTokenInformation(SafeCloseHandle tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, [Out] byte[] pTokenInformation, int tokenInformationLength, out int returnLength);

        [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeCloseHandle OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool OpenProcessToken(SafeCloseHandle processHandle, int desiredAccess, out SafeCloseHandle tokenHandle);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeServiceHandle OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeServiceHandle OpenService(SafeServiceHandle hSCManager, string lpServiceName, int dwDesiredAccess);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool QueryServiceConfig(SafeServiceHandle hService, [Out] byte[] pServiceConfig, int cbBufSize, out int pcbBytesNeeded);

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool QueryServiceStatus(SafeServiceHandle hService, out SERVICE_STATUS_PROCESS status);

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool QueryServiceStatusEx(SafeServiceHandle hService, int InfoLevel, [Out] byte[] pBuffer, int cbBufSize, out int pcbBytesNeeded);

        [DllImport(ADVAPI32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool SetKernelObjectSecurity(SafeCloseHandle handle, int securityInformation, [In] byte[] pSecurityDescriptor);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool StartService(SafeServiceHandle hSCManager, int dwNumServiceArgs, string[] lpServiceArgVectors);

        [Flags]
        internal enum SidAttribute : uint
        {
            SE_GROUP_MANDATORY = 0x1, // The SID cannot have the SE_GROUP_ENABLED attribute cleared by a call to the AdjustTokenGroups function. However, you can use the CreateRestrictedToken function to convert a mandatory SID to a deny-only SID. 
            SE_GROUP_ENABLED_BY_DEFAULT = 0x2, // The SID is enabled by default. 
            SE_GROUP_ENABLED = 0x4, // The SID is enabled for access checks. When the system performs an access check, it checks for access-allowed and access-denied access control entries (ACEs) that apply to the SID. A SID without this attribute is ignored during an access check unless the SE_GROUP_USE_FOR_DENY_ONLY attribute is set.
            SE_GROUP_OWNER = 0x8, // The SID identifies a group account for which the user of the token is the owner of the group, or the SID can be assigned as the owner of the token or objects. 
            SE_GROUP_USE_FOR_DENY_ONLY = 0x10, // 
            SE_GROUP_RESOURCE = 0x20000000, // The SID identifies a domain-local group.Windows NT:  This value is not supported. 
            SE_GROUP_LOGON_ID = 0xC0000000, // The SID is a logon SID that identifies the logon session associated with an access token. 
        }

        internal enum TOKEN_INFORMATION_CLASS : int
        {
            TokenUser = 1, // TOKEN_USER structure that contains the user account of the token. = 1, 
            TokenGroups, // a TOKEN_GROUPS structure that contains the group accounts associated with the token., 
            TokenPrivileges, // a TOKEN_PRIVILEGES structure that contains the privileges of the token., 
            TokenOwner, // a TOKEN_OWNER structure that contains the default owner security identifier (SID) for newly created objects., 
            TokenPrimaryGroup, // a TOKEN_PRIMARY_GROUP structure that contains the default primary group SID for newly created objects., 
            TokenDefaultDacl, // a TOKEN_DEFAULT_DACL structure that contains the default DACL for newly created objects., 
            TokenSource, // a TOKEN_SOURCE structure that contains the source of the token. TOKEN_QUERY_SOURCE access is needed to retrieve this information., 
            TokenType, // a TOKEN_TYPE value that indicates whether the token is a primary or impersonation token., 
            TokenImpersonationLevel, // a SECURITY_IMPERSONATION_LEVEL value that indicates the impersonation level of the token. If the access token is not an impersonation token, the function fails., 
            TokenStatistics, // a TOKEN_STATISTICS structure that contains various token statistics., 
            TokenRestrictedSids, // a TOKEN_GROUPS structure that contains the list of restricting SIDs in a restricted token., 
            TokenSessionId, // a DWORD value that indicates the Terminal Services session identifier that is associated with the token. If the token is associated with the Terminal Server console session, the session identifier is zero. If the token is associated with the Terminal Server client session, the session identifier is nonzero. In a non-Terminal Services environment, the session identifier is zero. If TokenSessionId is set with SetTokenInformation, the application must have the Act As Part Of the Operating System privilege, and the application must be enabled to set the session ID in a token.
            TokenGroupsAndPrivileges, // a TOKEN_GROUPS_AND_PRIVILEGES structure that contains the user SID, the group accounts, the restricted SIDs, and the authentication ID associated with the token., 
            TokenSessionReference, // Reserved,
            TokenSandBoxInert, // a DWORD value that is nonzero if the token includes the SANDBOX_INERT flag., 
            TokenAuditPolicy,
            TokenOrigin, // a TOKEN_ORIGIN value. If the token  resulted from a logon that used explicit credentials, such as passing a name, domain, and password to the  LogonUser function, then the TOKEN_ORIGIN structure will contain the ID of the logon session that created it. If the token resulted from  network authentication, such as a call to AcceptSecurityContext  or a call to LogonUser with dwLogonType set to LOGON32_LOGON_NETWORK or LOGON32_LOGON_NETWORK_CLEARTEXT, then this value will be zero.
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            TokenIsAppContainer,
            TokenCapabilities,
            TokenAppContainerSid,
            TokenAppContainerNumber,
            TokenUserClaimAttributes,
            TokenDeviceClaimAttributes,
            TokenRestrictedUserClaimAttributes,
            TokenRestrictedDeviceClaimAttributes,
            TokenDeviceGroups,
            TokenRestrictedDeviceGroups,
            MaxTokenInfoClass  // MaxTokenInfoClass should always be the last enum  
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct QUERY_SERVICE_CONFIG
        {
            internal int dwServiceType;
            internal int dwStartType;
            internal int dwErrorControl;
            internal string lpBinaryPathName;
            internal string lpLoadOrderGroup;
            internal int dwTagId;
            internal string lpDependencies;
            internal string lpServiceStartName;
            internal string lpDisplayName;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SERVICE_STATUS_PROCESS
        {
            internal int dwServiceType;
            internal int dwCurrentState;
            internal int dwControlsAccepted;
            internal int dwWin32ExitCode;
            internal int dwServiceSpecificExitCode;
            internal int dwCheckPoint;
            internal int dwWaitHint;
            internal int dwProcessId;
            internal int dwServiceFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct SID_AND_ATTRIBUTES
        {
            internal IntPtr Sid;
            internal SidAttribute Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_GROUPS
        {
            internal int GroupCount;
            internal IntPtr Groups; // array of SID_AND_ATTRIBUTES
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_USER
        {
            internal IntPtr User; // a SID_AND_ATTRIBUTES
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct TOKEN_PRIVILEGES
        {
            internal int PrivilegeCount;
            internal LUID_AND_ATTRIBUTES Privileges; // array of LUID_AND_ATTRIBUTES
        }

        [ComImport, Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E"), ComConversionLoss, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICorRuntimeHost
        {
            void Void0();
            void Void1();
            void Void2();
            void Void3();
            void Void4();
            void Void5();
            void Void6();
            void Void7();
            void Void8();
            void Void9();
            void GetDefaultDomain([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
        }
    }

    sealed class SafeCloseHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        const string KERNEL32 = "kernel32.dll";

        SafeCloseHandle() : base(true) { }
        internal SafeCloseHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            DiagnosticUtility.DebugAssert(handle == IntPtr.Zero || !ownsHandle, "Unsafe to create a SafeHandle that owns a pre-existing handle before the SafeHandle was created.");
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp 
#pragma warning suppress 56523 // We are not interested to throw an exception here. We can ignore the Last Error code.
            return CloseHandle(handle);
        }

        [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CloseHandle(IntPtr handle);
    }

    [SuppressUnmanagedCodeSecurityAttribute()]
    sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeServiceHandle()
            : base(true)
        {
        }

        override protected bool ReleaseHandle()
        {
#pragma warning suppress 56523 // Microsoft, should only fail if there is a 
            return ListenerUnsafeNativeMethods.CloseServiceHandle(handle);
        }
    }
}
