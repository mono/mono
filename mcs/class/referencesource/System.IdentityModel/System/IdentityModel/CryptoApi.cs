//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using Microsoft.Win32.SafeHandles;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    [SuppressUnmanagedCodeSecurity]
    static class CAPI
    {
        internal const string CRYPT32 = "crypt32.dll";
        internal const string BCRYPT = "bcrypt.dll";
        internal const string SubjectKeyIdentifierOid = "2.5.29.14";

        internal const int S_OK = 0;
        internal const int S_FALSE = 1;

        internal const string szOID_CRL_DIST_POINTS = "2.5.29.31";
        internal const string szOID_AUTHORITY_INFO_ACCESS = "1.3.6.1.5.5.7.1.1";

        //internal const uint CERT_STORE_NO_CRYPT_RELEASE_FLAG = 0x00000001;
        //internal const uint CERT_STORE_SET_LOCALIZED_NAME_FLAG = 0x00000002;
        //internal const uint CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 0x00000004;
        //internal const uint CERT_STORE_DELETE_FLAG = 0x00000010;
        //internal const uint CERT_STORE_SHARE_STORE_FLAG = 0x00000040;
        //internal const uint CERT_STORE_SHARE_CONTEXT_FLAG = 0x00000080;
        //internal const uint CERT_STORE_MANIFOLD_FLAG = 0x00000100;
        internal const uint CERT_STORE_ENUM_ARCHIVED_FLAG = 0x00000200;
        //internal const uint CERT_STORE_UPDATE_KEYID_FLAG = 0x00000400;
        //internal const uint CERT_STORE_BACKUP_RESTORE_FLAG = 0x00000800;
        internal const uint CERT_STORE_READONLY_FLAG = 0x00008000;
        internal const uint CERT_STORE_OPEN_EXISTING_FLAG = 0x00004000;
        internal const uint CERT_STORE_CREATE_NEW_FLAG = 0x00002000;
        internal const uint CERT_STORE_MAXIMUM_ALLOWED_FLAG = 0x00001000;

        internal const uint CERT_STORE_ADD_ALWAYS = 4;
        internal const uint CERT_CHAIN_POLICY_BASE = 1;
        internal const uint CERT_CHAIN_POLICY_NT_AUTH = 6;

        internal const uint X509_ASN_ENCODING = 0x00000001;
        internal const uint PKCS_7_ASN_ENCODING = 0x00010000;
        internal const uint CERT_STORE_PROV_MEMORY = 2;
        internal const uint CERT_STORE_PROV_SYSTEM = 10;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ID = 2;
        internal const uint CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;

        internal const uint CERT_SYSTEM_STORE_CURRENT_USER = ((int)CERT_SYSTEM_STORE_CURRENT_USER_ID << (int)CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE = ((int)CERT_SYSTEM_STORE_LOCAL_MACHINE_ID << (int)CERT_SYSTEM_STORE_LOCATION_SHIFT);

        //internal const uint CERT_INFO_VERSION_FLAG = 1;
        //internal const uint CERT_INFO_SERIAL_NUMBER_FLAG = 2;
        //internal const uint CERT_INFO_SIGNATURE_ALGORITHM_FLAG = 3;
        internal const uint CERT_INFO_ISSUER_FLAG = 4;
        //internal const uint CERT_INFO_NOT_BEFORE_FLAG = 5;
        //internal const uint CERT_INFO_NOT_AFTER_FLAG = 6;
        internal const uint CERT_INFO_SUBJECT_FLAG = 7;
        //internal const uint CERT_INFO_SUBJECT_PUBLIC_KEY_INFO_FLAG = 8;
        //internal const uint CERT_INFO_ISSUER_UNIQUE_ID_FLAG = 9;
        //internal const uint CERT_INFO_SUBJECT_UNIQUE_ID_FLAG = 10;
        //internal const uint CERT_INFO_EXTENSION_FLAG = 11;

        //internal const uint CERT_COMPARE_MASK = 0xFFFF;
        internal const uint CERT_COMPARE_SHIFT = 16;
        internal const uint CERT_COMPARE_ANY = 0;
        internal const uint CERT_COMPARE_SHA1_HASH = 1;
        //internal const uint CERT_COMPARE_NAME = 2;
        //internal const uint CERT_COMPARE_ATTR = 3;
        //internal const uint CERT_COMPARE_MD5_HASH = 4;
        //internal const uint CERT_COMPARE_PROPERTY = 5;
        //internal const uint CERT_COMPARE_PUBLIC_KEY = 6;
        //internal const uint CERT_COMPARE_HASH = CERT_COMPARE_SHA1_HASH;
        internal const uint CERT_COMPARE_NAME_STR_A = 7;
        internal const uint CERT_COMPARE_NAME_STR_W = 8;
        //internal const uint CERT_COMPARE_KEY_SPEC = 9;
        //internal const uint CERT_COMPARE_ENHKEY_USAGE = 10;
        //internal const uint CERT_COMPARE_CTL_USAGE = CERT_COMPARE_ENHKEY_USAGE;
        //internal const uint CERT_COMPARE_SUBJECT_CERT = 11;
        //internal const uint CERT_COMPARE_ISSUER_OF = 12;
        //internal const uint CERT_COMPARE_EXISTING = 13;
        //internal const uint CERT_COMPARE_SIGNATURE_HASH = 14;
        //internal const uint CERT_COMPARE_KEY_IDENTIFIER = 15;
        //internal const uint CERT_COMPARE_CERT_ID = 16;
        //internal const uint CERT_COMPARE_CROSS_CERT_DIST_POINTS = 17;
        //internal const uint CERT_COMPARE_PUBKEY_MD5_HASH = 18;

        internal const uint CERT_FIND_ANY = ((int)CERT_COMPARE_ANY << (int)CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_SHA1_HASH = ((int)CERT_COMPARE_SHA1_HASH << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_MD5_HASH = ((int)CERT_COMPARE_MD5_HASH << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_SIGNATURE_HASH = ((int)CERT_COMPARE_SIGNATURE_HASH << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_KEY_IDENTIFIER = ((int)CERT_COMPARE_KEY_IDENTIFIER << (int)CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_HASH = CERT_FIND_SHA1_HASH;
        //internal const uint CERT_FIND_PROPERTY = ((int)CERT_COMPARE_PROPERTY << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_PUBLIC_KEY = ((int)CERT_COMPARE_PUBLIC_KEY << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_SUBJECT_NAME = ((int)CERT_COMPARE_NAME << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
        //internal const uint CERT_FIND_SUBJECT_ATTR = ((int)CERT_COMPARE_ATTR << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
        //internal const uint CERT_FIND_ISSUER_NAME = ((int)CERT_COMPARE_NAME << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
        //internal const uint CERT_FIND_ISSUER_ATTR = ((int)CERT_COMPARE_ATTR << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
        internal const uint CERT_FIND_SUBJECT_STR_A = ((int)CERT_COMPARE_NAME_STR_A << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
        internal const uint CERT_FIND_SUBJECT_STR_W = ((int)CERT_COMPARE_NAME_STR_W << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
        internal const uint CERT_FIND_SUBJECT_STR = CERT_FIND_SUBJECT_STR_W;
        internal const uint CERT_FIND_ISSUER_STR_A = ((int)CERT_COMPARE_NAME_STR_A << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
        internal const uint CERT_FIND_ISSUER_STR_W = ((int)CERT_COMPARE_NAME_STR_W << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
        internal const uint CERT_FIND_ISSUER_STR = CERT_FIND_ISSUER_STR_W;
        //internal const uint CERT_FIND_KEY_SPEC = ((int)CERT_COMPARE_KEY_SPEC << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_ENHKEY_USAGE = ((int)CERT_COMPARE_ENHKEY_USAGE << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_CTL_USAGE = CERT_FIND_ENHKEY_USAGE;
        //internal const uint CERT_FIND_SUBJECT_CERT = ((int)CERT_COMPARE_SUBJECT_CERT << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_ISSUER_OF = ((int)CERT_COMPARE_ISSUER_OF << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_EXISTING = ((int)CERT_COMPARE_EXISTING << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_CERT_ID = ((int)CERT_COMPARE_CERT_ID << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_CROSS_CERT_DIST_POINTS = ((int)CERT_COMPARE_CROSS_CERT_DIST_POINTS << (int)CERT_COMPARE_SHIFT);
        //internal const uint CERT_FIND_PUBKEY_MD5_HASH = ((int)CERT_COMPARE_PUBKEY_MD5_HASH << (int)CERT_COMPARE_SHIFT);

        // Common chain policy flags.
        internal const uint CERT_CHAIN_REVOCATION_CHECK_END_CERT = 0x10000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN = 0x20000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x40000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY = 0x80000000;
        internal const uint CERT_CHAIN_REVOCATION_ACCUMULATIVE_TIMEOUT = 0x08000000;

        // Chain verification flag (not available in X509VerificationFlags).
        internal const uint CERT_CHAIN_POLICY_IGNORE_PEER_TRUST_FLAG = 0x00001000;


        // Default usage match type is AND with value zero
        internal const uint USAGE_MATCH_TYPE_AND = 0x00000000;
        internal const uint USAGE_MATCH_TYPE_OR = 0x00000001;

        // CertGetCertificateChain chain engine handles.
        internal const uint HCCE_CURRENT_USER = 0x0;
        internal const uint HCCE_LOCAL_MACHINE = 0x1;

        // Vista Peer Trust
        internal const uint CERT_TRUST_IS_PEER_TRUSTED = 0x00000800;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_CONTEXT
        {
            internal uint dwCertEncodingType;
            internal IntPtr pbCertEncoded;
            internal uint cbCertEncoded;
            internal IntPtr pCertInfo;
            internal IntPtr hCertStore;
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPTOAPI_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;

            static internal int Size = Marshal.SizeOf(typeof(CRYPTOAPI_BLOB));
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_ENHKEY_USAGE
        {
            internal uint cUsageIdentifier;
            internal IntPtr rgpszUsageIdentifier; // LPSTR*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_USAGE_MATCH
        {
            internal uint dwType;
            internal CERT_ENHKEY_USAGE Usage;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_CHAIN_PARA
        {
            internal uint cbSize;
            internal CERT_USAGE_MATCH RequestedUsage;
            internal CERT_USAGE_MATCH RequestedIssuancePolicy;
            internal uint dwUrlRetrievalTimeout; // milliseconds
            internal bool fCheckRevocationFreshnessTime;
            internal uint dwRevocationFreshnessTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_PARA
        {
            internal CERT_CHAIN_POLICY_PARA(int size)
            {
                cbSize = (uint)size;
                dwFlags = 0;
                pvExtraPolicyPara = IntPtr.Zero;
            }
            internal uint cbSize;
            internal uint dwFlags;
            internal IntPtr pvExtraPolicyPara;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_STATUS
        {
            internal CERT_CHAIN_POLICY_STATUS(int size)
            {
                cbSize = (uint)size;
                dwError = 0;
                lChainIndex = IntPtr.Zero;
                lElementIndex = IntPtr.Zero;
                pvExtraPolicyStatus = IntPtr.Zero;
            }
            internal uint cbSize;
            internal uint dwError;
            internal IntPtr lChainIndex;
            internal IntPtr lElementIndex;
            internal IntPtr pvExtraPolicyStatus;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_CHAIN_CONTEXT
        {
            internal CERT_CHAIN_CONTEXT(int size)
            {
                cbSize = (uint)size;
                dwErrorStatus = 0;
                dwInfoStatus = 0;
                cChain = 0;
                rgpChain = IntPtr.Zero;
                cLowerQualityChainContext = 0;
                rgpLowerQualityChainContext = IntPtr.Zero;
                fHasRevocationFreshnessTime = 0;
                dwRevocationFreshnessTime = 0;
            }
            internal uint cbSize;
            internal uint dwErrorStatus;   // serialized CERT_TRUST_STATUS
            internal uint dwInfoStatus;    // serialized CERT_TRUST_STATUS
            internal uint cChain;
            internal IntPtr rgpChain;                    // PCERT_SIMPLE_CHAIN*
            internal uint cLowerQualityChainContext;
            internal IntPtr rgpLowerQualityChainContext; // PCCERT_CHAIN_CONTEXT*
            internal uint fHasRevocationFreshnessTime; // Note that we declare the field as a uint here since we are manipulating 
            // the structure manually and a bool is only 1 byte in the managed world.
            internal uint dwRevocationFreshnessTime;   // seconds
        }

        [DllImport(CAPI.CRYPT32, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeCertContextHandle CertCreateCertificateContext(
            [In] uint dwCertEncodingType,
            [In] IntPtr pbCertEncoded,
            [In] uint cbCertEncoded
            );

        // A new store is created if one did not exist. The function fails if the store already exists if dwFlags is set to CERT_STORE_CREATE_NEW_FLAG .
        
        [DllImport(CAPI.CRYPT32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeCertStoreHandle CertOpenStore(
            [In] IntPtr lpszStoreProvider,
            [In] uint dwMsgAndCertEncodingType,
            [In] IntPtr hCryptProv,
            [In] uint dwFlags,
            [In] string pvPara // we want this always as a Unicode string.
            );

        [DllImport(CAPI.CRYPT32, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool CertCloseStore(
            [In] IntPtr hCertStore,
            [In] uint dwFlags
            );

        [DllImport(CAPI.CRYPT32, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure( ResourceScope.None )]
        internal static extern bool CertFreeCertificateContext(
            [In] IntPtr pCertContext
            );

        [DllImport(CAPI.CRYPT32, SetLastError = true)]
        [ResourceExposure( ResourceScope.None )]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern SafeCertContextHandle CertFindCertificateInStore(
            [In] SafeCertStoreHandle hCertStore,
            [In] uint dwCertEncodingType,
            [In] uint dwFindFlags,
            [In] uint dwFindType,
            [In] SafeHGlobalHandle pvFindPara,
            [In] SafeCertContextHandle pPrevCertContext
            );

        [DllImport(CRYPT32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure( ResourceScope.None )]
        internal extern static bool CertAddCertificateLinkToStore(
            [In] SafeCertStoreHandle hCertStore,
            [In] IntPtr pCertContext,
            [In] uint dwAddDisposition,
            [In, Out] SafeCertContextHandle ppStoreContext
            );

        [DllImport(CRYPT32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure( ResourceScope.None )]
        [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
        internal static extern bool CertGetCertificateChain(
            [In] IntPtr hChainEngine,
            [In] IntPtr pCertContext,
            [In] ref FILETIME pTime,
            [In] SafeCertStoreHandle hAdditionalStore,
            [In] ref CERT_CHAIN_PARA pChainPara,
            [In] uint dwFlags,
            [In] IntPtr pvReserved,
            [Out] out SafeCertChainHandle ppChainContext
            );

        [DllImport(CRYPT32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure( ResourceScope.None )]
        internal extern static bool CertVerifyCertificateChainPolicy(
            [In] IntPtr pszPolicyOID,
            [In] SafeCertChainHandle pChainContext,
            [In] ref CERT_CHAIN_POLICY_PARA pPolicyPara,
            [In, Out] ref CERT_CHAIN_POLICY_STATUS pPolicyStatus);

        [DllImport(CRYPT32, SetLastError = true)]
        [ResourceExposure( ResourceScope.None )]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern static void CertFreeCertificateChain(IntPtr handle);

        // On Vista and higher, check the value of the machine FIPS policy
        [DllImport(BCRYPT, SetLastError = true)]
        [ResourceExposure( ResourceScope.None )]
        internal static extern int BCryptGetFipsAlgorithmMode(
            [MarshalAs(UnmanagedType.U1), Out] out bool pfEnabled
            );

    }

#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    class SafeCertStoreHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeCertStoreHandle() : base(true) { }

        // 0 is an Invalid Handle
        SafeCertStoreHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        public static SafeCertStoreHandle InvalidHandle
        {
            get { return new SafeCertStoreHandle(IntPtr.Zero); }
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp 
#pragma warning suppress 56523 // We are not interested in throwing an exception here if CloseHandle fails.
            return CAPI.CertCloseStore(handle, 0);
        }
    }

#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    class SafeCertContextHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeCertContextHandle() : base(true) { }

        // 0 is an Invalid Handle
        SafeCertContextHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        internal static SafeCertContextHandle InvalidHandle
        {
            get { return new SafeCertContextHandle(IntPtr.Zero); }
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp 
#pragma warning suppress 56523 // We are not interested in throwing an exception here if CloseHandle fails.
            return CAPI.CertFreeCertificateContext(handle);
        }
    }

#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    class SafeCertChainHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeCertChainHandle() : base(true) { }

        SafeCertChainHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        internal static SafeCertChainHandle InvalidHandle
        {
            get { return new SafeCertChainHandle(IntPtr.Zero); }
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp 
#pragma warning suppress 56523 // We are not interested in throwing an exception here if CloseHandle fails.
            CAPI.CertFreeCertificateChain(handle);
            return true;
        }
    }
}
