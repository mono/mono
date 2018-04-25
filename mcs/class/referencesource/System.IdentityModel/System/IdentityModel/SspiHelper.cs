//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;

    internal enum CredentialUse
    {
        Inbound = 0x1,
        Outbound = 0x2,
        Both = 0x3,
    }

    internal enum Endianness
    {
        Network = 0x00,
        Native = 0x10,
    }

    internal enum CertificateEncoding
    {
        Zero = 0,
        X509AsnEncoding = unchecked((int)0x00000001),
        X509NdrEncoding = unchecked((int)0x00000002),
        Pkcs7AsnEncoding = unchecked((int)0x00010000),
        Pkcs7NdrEncoding = unchecked((int)0x00020000),
        AnyAsnEncoding = X509AsnEncoding | Pkcs7AsnEncoding
    }

    internal enum BufferType
    {
        Empty = 0x00,
        Data = 0x01,
        Token = 0x02,
        Parameters = 0x03,
        Missing = 0x04,
        Extra = 0x05,
        Trailer = 0x06,
        Header = 0x07,
        Padding = 0x09,
        Stream = 0x0A,
        ChannelBindings = 0x0E,
    }

    internal enum SecurityStatus
    {
        OK = 0x00000000,
        OutOfMemory = unchecked((int)0x80090300),
        InvalidHandle = unchecked((int)0x80090301),
        Unsupported = unchecked((int)0x80090302),
        TargetUnknown = unchecked((int)0x80090303),
        InternalError = unchecked((int)0x80090304),
        PackageNotFound = unchecked((int)0x80090305),
        NotOwner = unchecked((int)0x80090306),
        CannotInstall = unchecked((int)0x80090307),
        InvalidToken = unchecked((int)0x80090308),
        LogonDenied = unchecked((int)0x8009030C),
        UnknownCredential = unchecked((int)0x8009030D),
        NoCredentials = unchecked((int)0x8009030E),
        MessageAltered = unchecked((int)0x8009030F),

        ContinueNeeded = unchecked((int)0x00090312),
        CompleteNeeded = unchecked((int)0x00090313),
        CompAndContinue = unchecked((int)0x00090314),
        ContextExpired = unchecked((int)0x00090317),
        IncompleteMessage = unchecked((int)0x80090318),
        IncompleteCred = unchecked((int)0x80090320),
        BufferNotEnough = unchecked((int)0x80090321),
        WrongPrincipal = unchecked((int)0x80090322),
        UntrustedRoot = unchecked((int)0x80090325),
        UnknownCertificate = unchecked((int)0x80090327),

        CredentialsNeeded = unchecked((int)0x00090320),
        Renegotiate = unchecked((int)0x00090321),
    }

    internal enum ContextAttribute
    {
        //
        // look into <sspi.h> and <schannel.h>
        //
        Sizes = 0x00,
        Names = 0x01,
        Lifespan = 0x02,
        DceInfo = 0x03,
        StreamSizes = 0x04,
        //KeyInfo             = 0x05, must not be used, see ConnectionInfo instead
        Authority = 0x06,
        // SECPKG_ATTR_PROTO_INFO          = 7,
        // SECPKG_ATTR_PASSWORD_EXPIRY     = 8,
        SessionKey = 0x09,
        PackageInfo = 0x0A,
        // SECPKG_ATTR_USER_FLAGS          = 11,
        NegotiationInfo = 0x0C,
        // SECPKG_ATTR_NATIVE_NAMES        = 13,
        // SECPKG_ATTR_FLAGS               = 14,
        Flags = 0x0E,
        // SECPKG_ATTR_USE_VALIDATED       = 15,
        // SECPKG_ATTR_CREDENTIAL_NAME     = 16,
        // SECPKG_ATTR_TARGET_INFORMATION  = 17,
        // SECPKG_ATTR_ACCESS_TOKEN        = 18,
        // SECPKG_ATTR_TARGET              = 19,
        // SECPKG_ATTR_AUTHENTICATION_ID   = 20,
        // SECPKG_ATTR_CLIENT_SPECIFIED_TARGET 27
        SpecifiedTarget = 0x1B,
        RemoteCertificate = 0x53,
        LocalCertificate = 0x54,
        RootStore = 0x55,
        IssuerListInfoEx = 0x59,
        ConnectionInfo = 0x5A,
        EapKey = 0x5B
    }

    internal enum CredentialStatus
    {
        InValidParameter = unchecked((int)0x57),
        NoSuchPackage = unchecked((int)0x554),
        NotSupported = unchecked((int)0x32),
        Cancelled = unchecked((int)0x4C7),
        Success = unchecked((int)0x0),
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SecurityBufferStruct
    {
        public int count;
        public BufferType type;
        public IntPtr token;

        public static readonly int Size = sizeof(SecurityBufferStruct);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct AuthIdentityEx
    {
        // see SEC_WINNT_AUTH_IDENTITY_EX
        internal int Version;

        internal int Length;

        internal string UserName;

        internal int UserNameLength;

        internal string Domain;

        internal int DomainLength;

        internal string Password;

        internal int PasswordLength;

        internal int Flags;

        internal string PackageList;

        internal int PackageListLength;

        // sspi.h: #define SEC_WINNT_AUTH_IDENTITY_VERSION 0x200
        static readonly int WinNTAuthIdentityVersion = 0x200;

        internal AuthIdentityEx(string userName, string password, string domain, params string[] additionalPackages)
        {
            Version = WinNTAuthIdentityVersion;
            Length = Marshal.SizeOf(typeof(AuthIdentityEx));
            UserName = userName;
            UserNameLength = userName == null ? 0 : userName.Length;
            Password = password;
            PasswordLength = password == null ? 0 : password.Length;
            Domain = domain;
            DomainLength = domain == null ? 0 : domain.Length;

            // Flags are 2 for Unicode and 1 for ANSI. We use 2 on NT 
            Flags = 2;

            if (null == additionalPackages)
            {
                PackageList = null;
                PackageListLength = 0;
            }
            else
            {
                PackageList = String.Join(",", additionalPackages);
                PackageListLength = PackageList.Length;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecureCredential
    {
        /*
        typedef struct _SCHANNEL_CRED
        {
            DWORD           dwVersion;      // always SCHANNEL_CRED_VERSION
            DWORD           cCreds;
            PCCERT_CONTEXT *paCred;
            HCERTSTORE      hRootStore;

            DWORD           cMappers;
            struct _HMAPPER **aphMappers;

            DWORD           cSupportedAlgs;
            ALG_ID *        palgSupportedAlgs;

            DWORD           grbitEnabledProtocols;
            DWORD           dwMinimumCipherStrength;
            DWORD           dwMaximumCipherStrength;
            DWORD           dwSessionLifespan;
            DWORD           dwFlags;
            DWORD           reserved;
        } SCHANNEL_CRED, *PSCHANNEL_CRED;
        */
        public const int CurrentVersion = 0x4;

        public int version;
        public int cCreds;

        // ptr to an array of pointers
        // Cannot convert it to SafeHandle because it gets wrapped with another pointer
        // right before native AcquireCredentialsHandle call
        public IntPtr certContextArray;

        private IntPtr rootStore; // == always null, OTHERWISE NOT RELIABLE
        public int cMappers;
        private IntPtr phMappers; // == always null, OTHERWISE NOT RELIABLE
        public int cSupportedAlgs;
        private IntPtr palgSupportedAlgs; // == always null, OTHERWISE NOT RELIABLE
        public SchProtocols grbitEnabledProtocols;
        public int dwMinimumCipherStrength;
        public int dwMaximumCipherStrength;
        public int dwSessionLifespan;
        public SecureCredential.Flags dwFlags;
        public int reserved;

        [Flags]
        public enum Flags
        {
            Zero = 0,
            NoSystemMapper = 0x02,
            NoNameCheck = 0x04,
            ValidateManual = 0x08,
            NoDefaultCred = 0x10,
            ValidateAuto = 0x20
        }

        public SecureCredential(int version, X509Certificate2 certificate,
            SecureCredential.Flags flags, SchProtocols protocols)
        {
            //Setting default values
            rootStore = phMappers = palgSupportedAlgs = certContextArray = IntPtr.Zero;
            cCreds = cMappers = cSupportedAlgs = 0;
            dwMinimumCipherStrength = dwMaximumCipherStrength = 0;
            dwSessionLifespan = reserved = 0;

            this.version = version;
            dwFlags = flags;
            grbitEnabledProtocols = protocols;
            if (certificate != null)
            {
                certContextArray = certificate.Handle;
                cCreds = 1;
            }
        }
    } // SecureCredential

    static class IntPtrHelper
    {
        private const string KERNEL32 = "kernel32.dll";

        //internal static bool IsZero(IntPtr a) 
        //{
        //    return ((long) a)==0;
        //}

        //internal static IntPtr Add(IntPtr a, IntPtr b) 
        //{
        //    return (IntPtr) ((long) a + (long) b);
        //}

        //internal static IntPtr Add(IntPtr a, long b) 
        //{
        //    return (IntPtr) ((long) a + b);
        //}

        //internal static IntPtr Add(long a, IntPtr b) 
        //{
        //    return (IntPtr) (a + (long) b);
        //}

        internal static IntPtr Add(IntPtr a, int b)
        {
            return (IntPtr)((long)a + (long)b);
        }

        //internal static IntPtr Add(int a, IntPtr b) 
        //{
        //    return (IntPtr) ((long) a + (long) b);
        //}
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe class SecurityBufferDescriptor
    {
        /*
        typedef struct _SecBufferDesc {
            ULONG        ulVersion;
            ULONG        cBuffers;
            PSecBuffer   pBuffers;
        } SecBufferDesc, * PSecBufferDesc;
        */
        public readonly int Version;
        public readonly int Count;
        public void* UnmanagedPointer;

        public SecurityBufferDescriptor(int count)
        {
            Version = 0;
            Count = count;
            UnmanagedPointer = null;
        }
    } // SecurityBufferDescriptor

    internal class SecurityBuffer
    {
        /*
        typedef struct _SecBuffer {
            ULONG        cbBuffer;
            ULONG        BufferType;
            PVOID        pvBuffer;
        } SecBuffer, *PSecBuffer;
        */
        public int size;
        public BufferType type;
        public byte[] token;
        public int offset;
        public SafeHandle unmanagedToken;

        public SecurityBuffer(byte[] data, int offset, int size, BufferType tokentype)
        {
            this.offset = offset;
            this.size = (data == null) ? 0 : size;
            this.type = tokentype;
            this.token = data;
        }

        public SecurityBuffer(byte[] data, BufferType tokentype)
        {
            this.size = (data == null) ? 0 : data.Length;
            this.type = tokentype;
            this.token = data;
        }

        public SecurityBuffer(int size, BufferType tokentype)
        {
            this.size = size;
            this.type = tokentype;
            this.token = size == 0 ? null : DiagnosticUtility.Utility.AllocateByteArray(size);
        }

        public SecurityBuffer(ChannelBinding channelBinding)
        {
            this.size = channelBinding.Size;
            this.type = BufferType.ChannelBindings;
            this.unmanagedToken = channelBinding;
        }
    }
}
