//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Principal;
    using System.Text;

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

    enum TokenInformationClass : uint
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

    enum Win32Error
    {
        ERROR_SUCCESS = 0,
        ERROR_INSUFFICIENT_BUFFER = 122,
        ERROR_NO_TOKEN = 1008,
        ERROR_NONE_MAPPED = 1332,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CREDUI_INFO
    {
        public int cbSize;
        public IntPtr hwndParent;
        public string pszMessageText;
        public string pszCaptionText;
        public IntPtr hbmBanner;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class SEC_WINNT_AUTH_IDENTITY_EX
    {
        public uint Version;
        public uint Length;
        public string User;
        public uint UserLength;
        public string Domain;
        public uint DomainLength;
        public string Password;
        public uint PasswordLength;
        public uint Flags;
        public string PackageList;
        public uint PackageListLength;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SID_AND_ATTRIBUTES
    {
        internal IntPtr Sid;
        internal uint Attributes;
        internal static readonly long SizeOf = (long)Marshal.SizeOf(typeof(SID_AND_ATTRIBUTES));
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TOKEN_GROUPS
    {
        internal uint GroupCount;
        internal SID_AND_ATTRIBUTES Groups; // SID_AND_ATTRIBUTES Groups[ANYSIZE_ARRAY];
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PLAINTEXTKEYBLOBHEADER
    {
        internal byte bType;
        internal byte bVersion;
        internal short reserved;
        internal int aiKeyAlg;
        internal int keyLength;

        internal static readonly int SizeOf = Marshal.SizeOf(typeof(PLAINTEXTKEYBLOBHEADER));
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID
    {
        internal uint LowPart;
        internal uint HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID_AND_ATTRIBUTES
    {
        internal LUID Luid;
        internal uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TOKEN_PRIVILEGE
    {
        internal uint PrivilegeCount;
        internal LUID_AND_ATTRIBUTES Privilege;

        internal static readonly uint Size = (uint)Marshal.SizeOf(typeof(TOKEN_PRIVILEGE));
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct UNICODE_INTPTR_STRING
    {
        internal UNICODE_INTPTR_STRING(int length, int maximumLength, IntPtr buffer)
        {
            this.Length = (ushort)length;
            this.MaxLength = (ushort)maximumLength;
            this.Buffer = buffer;
        }
        internal ushort Length;
        internal ushort MaxLength;
        internal IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct KERB_CERTIFICATE_S4U_LOGON
    {
        internal KERB_LOGON_SUBMIT_TYPE MessageType;
        internal uint Flags;
        internal UNICODE_INTPTR_STRING UserPrincipalName;
        // OPTIONAL, certificate mapping hints: username or username@domain
        internal UNICODE_INTPTR_STRING DomainName; // used to locate the forest
        // OPTIONAL, certificate mapping hints: if missing, using the local machine's domain
        internal uint CertificateLength;   // for the client certificate 
        internal IntPtr Certificate;        // for the client certificate, BER encoded

        internal static int Size = Marshal.SizeOf(typeof(KERB_CERTIFICATE_S4U_LOGON));
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TOKEN_SOURCE
    {
        private const int TOKEN_SOURCE_LENGTH = 8;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = TOKEN_SOURCE_LENGTH)]
        internal char[] Name;
        internal LUID SourceIdentifier;
    }

    internal enum KERB_LOGON_SUBMIT_TYPE
    {
        KerbInteractiveLogon = 2,
        KerbSmartCardLogon = 6,
        KerbWorkstationUnlockLogon = 7,
        KerbSmartCardUnlockLogon = 8,
        KerbProxyLogon = 9,
        KerbTicketLogon = 10,
        KerbTicketUnlockLogon = 11,
        //#if (_WIN32_WINNT >= 0x0501) -- Disabled until IIS fixes their target version.
        KerbS4ULogon = 12,
        //#endif
        //#if (_WIN32_WINNT >= 0x0600)     
        KerbCertificateLogon = 13,
        KerbCertificateS4ULogon = 14,
        KerbCertificateUnlockLogon = 15,
        //#endif    
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct QUOTA_LIMITS
    {
        internal IntPtr PagedPoolLimit;
        internal IntPtr NonPagedPoolLimit;
        internal IntPtr MinimumWorkingSetSize;
        internal IntPtr MaximumWorkingSetSize;
        internal IntPtr PagefileLimit;
        internal IntPtr TimeLimit;
    }

    internal enum SECURITY_IMPERSONATION_LEVEL
    {
        Anonymous = 0,
        Identification = 1,
        Impersonation = 2,
        Delegation = 3,
    }

    internal enum TokenType : int
    {
        TokenPrimary = 1,
        TokenImpersonation
    }

    internal enum SecurityLogonType : int
    {
        Interactive = 2,
        Network,
        Batch,
        Service,
        Proxy,
        Unlock
    }

    [SuppressUnmanagedCodeSecurity]
    static class NativeMethods
    {
        const string ADVAPI32 = "advapi32.dll";
        const string KERNEL32 = "kernel32.dll";
        const string SECUR32 = "secur32.dll";
        const string CREDUI = "credui.dll";



        // Error codes from ntstatus.h
        //internal const uint STATUS_SOME_NOT_MAPPED = 0x00000107;
        internal const uint STATUS_NO_MEMORY = 0xC0000017;
        //internal const uint STATUS_NONE_MAPPED = 0xC0000073;
        internal const uint STATUS_INSUFFICIENT_RESOURCES = 0xC000009A;
        internal const uint STATUS_ACCESS_DENIED = 0xC0000022;

        // From WinStatus.h
        internal const uint STATUS_ACCOUNT_RESTRICTION = 0xC000006E;

        internal static byte[] LsaSourceName = new byte[] { (byte)'W', (byte)'C', (byte)'F' }; // we set the source name to "WCF".
        internal static byte[] LsaKerberosName = new byte[] { (byte)'K', (byte)'e', (byte)'r', (byte)'b', (byte)'e', (byte)'r', (byte)'o', (byte)'s' };

        internal const uint KERB_CERTIFICATE_S4U_LOGON_FLAG_CHECK_DUPLICATES = 0x1;
        internal const uint KERB_CERTIFICATE_S4U_LOGON_FLAG_CHECK_LOGONHOURS = 0x2;

        // Error codes from WinError.h
        internal const int ERROR_ACCESS_DENIED = 0x5;
        internal const int ERROR_BAD_LENGTH = 0x18;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

        internal const uint SE_GROUP_ENABLED = 0x00000004;
        internal const uint SE_GROUP_USE_FOR_DENY_ONLY = 0x00000010;
        internal const uint SE_GROUP_LOGON_ID = 0xC0000000;

        internal const int PROV_RSA_AES = 24;
        internal const int KP_IV = 1;
        internal const uint CRYPT_DELETEKEYSET = 0x00000010;
        internal const uint CRYPT_VERIFYCONTEXT = 0xF0000000;
        internal const byte PLAINTEXTKEYBLOB = 0x8;
        internal const byte CUR_BLOB_VERSION = 0x2;

        internal const int ALG_CLASS_DATA_ENCRYPT = (3 << 13);
        internal const int ALG_TYPE_BLOCK = (3 << 9);
        internal const int CALG_AES_128 = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | 14);
        internal const int CALG_AES_192 = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | 15);
        internal const int CALG_AES_256 = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | 16);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool LogonUser(
            [In] string lpszUserName,
            [In] string lpszDomain,
            [In] string lpszPassword,
            [In] uint dwLogonType,
            [In] uint dwLogonProvider,
            [Out] out SafeCloseHandle phToken
            );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetTokenInformation(
            [In] IntPtr tokenHandle,
            [In] uint tokenInformationClass,
            [In] SafeHGlobalHandle tokenInformation,
            [In] uint tokenInformationLength,
            [Out] out uint returnLength);

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool CryptAcquireContextW(
            [Out] out SafeProvHandle phProv,
            [In] string pszContainer,
            [In] string pszProvider,
            [In] uint dwProvType,
            [In] uint dwFlags
            );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe static extern bool CryptImportKey(
          [In] SafeProvHandle hProv,
          [In] void* pbData,
          [In] uint dwDataLen,
          [In] IntPtr hPubKey,
          [In] uint dwFlags,
          [Out] out SafeKeyHandle phKey
        );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool CryptGetKeyParam(
          [In] SafeKeyHandle phKey,
          [In] uint dwParam,
          [In] IntPtr pbData,
          [In, Out] ref uint dwDataLen,
          [In] uint dwFlags
        );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal unsafe static extern bool CryptSetKeyParam(
          [In] SafeKeyHandle phKey,
          [In] uint dwParam,
          [In] void* pbData,
          [In] uint dwFlags
        );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern bool CryptEncrypt(
          [In] SafeKeyHandle phKey,
          [In] IntPtr hHash,
          [In] bool final,
          [In] uint dwFlags,
          [In] void* pbData,
          [In, Out] ref int dwDataLen,
          [In] int dwBufLen
        );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern bool CryptDecrypt(
          [In] SafeKeyHandle phKey,
          [In] IntPtr hHash,
          [In] bool final,
          [In] uint dwFlags,
          [In] void* pbData,
          [In, Out] ref int dwDataLen
        );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool CryptDestroyKey(
            [In] IntPtr phKey
            );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool CryptReleaseContext(
            [In] IntPtr hProv,
            [In] uint dwFlags
            );

        [DllImport(ADVAPI32, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool LookupPrivilegeValueW(
            [In] string lpSystemName,
            [In] string lpName,
            [Out] out LUID Luid
            );

        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool AdjustTokenPrivileges(
            [In] SafeCloseHandle tokenHandle,
            [In] bool disableAllPrivileges,
            [In] ref TOKEN_PRIVILEGE newState,
            [In] uint bufferLength,
            [Out] out TOKEN_PRIVILEGE previousState,
            [Out] out uint returnLength
            );

        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool RevertToSelf();

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceConsumption(ResourceScope.Process)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern bool OpenProcessToken(
            [In] IntPtr processToken,
            [In] TokenAccessLevels desiredAccess,
            [Out] out SafeCloseHandle tokenHandle
            );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool OpenThreadToken(
            [In] IntPtr threadHandle,
            [In] TokenAccessLevels desiredAccess,
            [In] bool openAsSelf,
            [Out] out SafeCloseHandle tokenHandle
            );

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr GetCurrentThread();

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool DuplicateTokenEx(
            [In] SafeCloseHandle existingTokenHandle,
            [In] TokenAccessLevels desiredAccess,
            [In] IntPtr tokenAttributes,
            [In] SECURITY_IMPERSONATION_LEVEL impersonationLevel,
            [In] TokenType tokenType,
            [Out] out SafeCloseHandle duplicateTokenHandle
            );

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool SetThreadToken(
            [In] IntPtr threadHandle,
            [In] SafeCloseHandle threadToken
            );


        [DllImport(SECUR32, CharSet = CharSet.Auto, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int LsaRegisterLogonProcess(
            [In] ref UNICODE_INTPTR_STRING logonProcessName,
            [Out] out SafeLsaLogonProcessHandle lsaHandle,
            [Out] out IntPtr securityMode
            );

        [DllImport(SECUR32, CharSet = CharSet.Auto, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int LsaConnectUntrusted(
            [Out] out SafeLsaLogonProcessHandle lsaHandle
            );

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int LsaNtStatusToWinError(
            [In] int status
            );

        [DllImport(SECUR32, CharSet = CharSet.Auto, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int LsaLookupAuthenticationPackage(
            [In] SafeLsaLogonProcessHandle lsaHandle,
            [In] ref UNICODE_INTPTR_STRING packageName,
            [Out] out uint authenticationPackage
            );

        [DllImport(ADVAPI32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool AllocateLocallyUniqueId(
            [Out] out LUID Luid
            );

        [DllImport(SECUR32, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern int LsaFreeReturnBuffer(
            IntPtr handle
            );

        [DllImport(SECUR32, CharSet = CharSet.Auto, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int LsaLogonUser(
            [In] SafeLsaLogonProcessHandle LsaHandle,
            [In] ref UNICODE_INTPTR_STRING OriginName,
            [In] SecurityLogonType LogonType,
            [In] uint AuthenticationPackage,
            [In] IntPtr AuthenticationInformation,
            [In] uint AuthenticationInformationLength,
            [In] IntPtr LocalGroups,
            [In] ref TOKEN_SOURCE SourceContext,
            [Out] out SafeLsaReturnBufferHandle ProfileBuffer,
            [Out] out uint ProfileBufferLength,
            [Out] out LUID LogonId,
            [Out] out SafeCloseHandle Token,
            [Out] out QUOTA_LIMITS Quotas,
            [Out] out int SubStatus
            );

        [DllImport(SECUR32, CharSet = CharSet.Auto, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern int LsaDeregisterLogonProcess(
            [In] IntPtr handle
            );


        [DllImport(CREDUI, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal unsafe static extern uint SspiPromptForCredentials(
           string pszTargetName,
           ref CREDUI_INFO pUiInfo,
           uint dwAuthError,
           string pszPackage,
           IntPtr authIdentity,
           out IntPtr ppAuthIdentity,
           [MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
           uint dwFlags
            );

        [DllImport(CREDUI, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal unsafe static extern bool SspiIsPromptingNeeded(uint ErrorOrNtStatus);

        [DllImport(SECUR32, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal extern static bool TranslateName( 
            string input, 
            EXTENDED_NAME_FORMAT inputFormat, 
            EXTENDED_NAME_FORMAT outputFormat, 
            StringBuilder outputString, 
            out uint size 
            );

    }
}
