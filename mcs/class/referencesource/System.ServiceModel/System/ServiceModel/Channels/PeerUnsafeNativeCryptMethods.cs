//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    struct SystemTime
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;

        public SystemTime(DateTime date)
        {
            wYear = (short)date.Year;
            wMonth = (short)date.Month;
            wDayOfWeek = (short)date.DayOfWeek;
            wDay = (short)date.Day;
            wHour = (short)date.Hour;
            wMinute = (short)date.Minute;
            wSecond = (short)date.Second;
            wMilliseconds = (short)date.Millisecond;
        }
    }

    [SuppressUnmanagedCodeSecurity]
    class CertificateHandle : SafeHandle
    {
        #region PInvoke declarations
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CertFreeCertificateContext(IntPtr pCertContext);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CertDeleteCertificateFromStore(IntPtr pCertContext);

        #endregion
        protected bool delete = false;
        protected CertificateHandle()
            : base(IntPtr.Zero, true)
        {
            return;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            if (delete)
                return CertDeleteCertificateFromStore(handle);
            else
                return CertFreeCertificateContext(handle);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    sealed class StoreCertificateHandle : CertificateHandle
    {
        StoreCertificateHandle() : base() { base.delete = true; }
    }

    [SuppressUnmanagedCodeSecurity]
    sealed class CertificateStoreHandle : SafeHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall)]
        [ResourceExposure(ResourceScope.None)]
        static extern bool CertCloseStore(IntPtr hCertStore, int dwFlags);

        CertificateStoreHandle()
            : base(IntPtr.Zero, true)
        {
            return;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            return CertCloseStore(handle, 0);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    sealed class KeyContainerHandle : SafeHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall)]
        [ResourceExposure(ResourceScope.None)]
        static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);

        KeyContainerHandle()
            : base(IntPtr.Zero, true)
        {
            return;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            return CryptReleaseContext(handle, 0);
        }
    }

    sealed class KeyHandle : SafeHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall)]
        [ResourceExposure(ResourceScope.None)]
        static extern bool CryptDestroyKey(IntPtr hKey);

        KeyHandle()
            : base(IntPtr.Zero, true)
        {
            return;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            return CryptDestroyKey(handle);
        }
    }

    sealed class CryptoApiBlob : IDisposable
    {
        int cbData;
        CriticalAllocHandle data;

        public CryptoApiBlob()
        {
        }

        public CryptoApiBlob(byte[] bytes)
        {
            Fx.Assert(bytes != null, "Cannot set null data");
            AllocateBlob(bytes.Length);
            Marshal.Copy(bytes, 0, (IntPtr)data, bytes.Length);
            cbData = bytes.Length;
            return;
        }

        public int DataSize
        {
            get
            {
                Fx.Assert(cbData >= 0, "Size must be greater than or equal to zero");
                return cbData;
            }
        }

        public void AllocateBlob(int size)
        {
            data = CriticalAllocHandle.FromSize(size);
            cbData = size;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class InteropHelper
        {
            public int size;
            public IntPtr data;
            public InteropHelper(int size, IntPtr data)
            {
                this.size = size;
                this.data = data;
            }
        }
        public InteropHelper GetMemoryForPinning()
        {
            return new InteropHelper(cbData, (IntPtr)data);
        }

        public byte[] GetBytes()
        {
            if (cbData == 0)
                return null;

            byte[] bytes = DiagnosticUtility.Utility.AllocateByteArray(cbData);
            Marshal.Copy((IntPtr)data, bytes, 0, cbData);
            return bytes;
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            return;
        }
    }

    sealed class CertificateName
    {
        #region PInvoke Declarations
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CertStrToName(CertEncodingType dwCertEncodingType,
                                                    [MarshalAs(UnmanagedType.LPTStr)]string pszX500,
                                                    StringType dwStrType,
                                                    IntPtr pvReserved,
                                                    [In, Out]byte[] pbEncoded,
                                                    [In, Out]ref int pcbEncoded,
                                                    [MarshalAs(UnmanagedType.LPTStr)]ref StringBuilder ppszError);


        [Flags]
        enum CertEncodingType : int
        {
            X509AsnEncoding = 0x00000001,
            PKCS7AsnEncoding = 0x00010000
        }

        [Flags]
        enum StringType : int
        {
            SimpleNameString = 1,
            OIDNameString = 2,
            X500NameString = 3,

            CommaFlag = 0x04000000,
            SemicolonFlag = 0x40000000,
            CRLFFlag = 0x08000000,
            NoPlusFlag = 0x20000000,
            NoQuotingFlag = 0x10000000,
            ReverseFlag = 0x02000000,
            DisableIE4UTF8Flag = 0x00010000,
            EnableT61UnicodeFlag = 0x00020000,
            EnableUTF8UnicodeFlag = 0x00040000
        }
        #endregion

        string dn;

        public CertificateName(string dn)
        {
            Fx.Assert(!String.IsNullOrEmpty(dn), "Empty subject name for certificate!");
            this.dn = dn;
        }

        public string DistinguishedName
        {
            get { return dn; }
        }

        public CryptoApiBlob GetCryptoApiBlob()
        {
            byte[] encodedName = GetEncodedName();
            return new CryptoApiBlob(encodedName);
        }

        byte[] GetEncodedName()
        {
            int encodingSize = 0;
            StringBuilder errorString = null;

            CertStrToName(CertEncodingType.X509AsnEncoding | CertEncodingType.PKCS7AsnEncoding,
                            DistinguishedName,
                            StringType.OIDNameString | StringType.ReverseFlag,
                            IntPtr.Zero,
                            null,
                            ref encodingSize,
                            ref errorString);

            byte[] encodedBytes = new byte[encodingSize];
            bool ok = CertStrToName(CertEncodingType.X509AsnEncoding | CertEncodingType.PKCS7AsnEncoding,
                                        DistinguishedName,
                                        StringType.OIDNameString | StringType.ReverseFlag,
                                        IntPtr.Zero,
                                        encodedBytes,
                                        ref encodingSize,
                                        ref errorString);

            if (!ok)
            {
                PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
            }

            return encodedBytes;
        }
    }


    sealed partial class SelfSignedCertificate : IDisposable
    {
        #region PInvoke declarations
        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static CertificateHandle CertCreateSelfSignCertificate(
                                                    KeyContainerHandle hProv,
                                                    CryptoApiBlob.InteropHelper pSubjectIssuerBlob,
                                                    SelfSignFlags dwFlags,
                                                    IntPtr pKeyProvInfo,
                                                    IntPtr pSignatureAlgorithm,
                                                    [In] ref SystemTime pStartTime,
                                                    [In] ref SystemTime pEndTime,
                                                    IntPtr pExtensions);

        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static CertificateStoreHandle CertOpenStore(
                                                    IntPtr lpszStoreProvider,
                                                    int dwMsgAndCertEncodingType,
                                                    IntPtr hCryptProv,
                                                    int dwFlags,
                                                    IntPtr pvPara);

        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CertAddCertificateContextToStore(
                                                    CertificateStoreHandle hCertStore,
                                                    CertificateHandle pCertContext,
                                                    AddDisposition dwAddDisposition,
                                                    [Out]out StoreCertificateHandle ppStoreContext);

        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CryptAcquireContext(
                                                    [Out]out KeyContainerHandle phProv,
                                                    string pszContainer,
                                                    string pszProvider,
                                                    ProviderType dwProvType,
                                                    ContextFlags dwFlags);

        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CryptGenKey(
                                                    KeyContainerHandle hProv,
                                                    AlgorithmType algId,
                                                    KeyFlags dwFlags,
                                                    [Out]out KeyHandle phKey);

        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool PFXExportCertStoreEx(
                                                    CertificateStoreHandle hStore,
                                                    IntPtr pPFX,
            //IntPtr szPassword,
                                                    string password,
                                                    IntPtr pvReserved,
                                                    PfxExportFlags dwFlags);

        [DllImport("Crypt32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CertSetCertificateContextProperty(
                                                    CertificateHandle context,
                                                    int propId,
                                                    int flags,
                                                    KeyHandle pv);

        [Flags]
        enum SelfSignFlags : int
        {
            None = 0,
            NoSign = 1,
            NoKeyInfo = 2,
        }

        enum AddDisposition : int
        {
            New = 1,
            UseExisting = 2,
            ReplaceExisting = 3,
            Always = 4,
            ReplaceExistingInheritProperties = 5
        }

        [Flags]
        enum PfxExportFlags : int
        {
            ReportNoPrivateKey = 0x00000001,
            ReportNotAbleToExportPrivateKey = 0x00000002,
            ExportPrivateKeys = 0x00000004
        }

        enum ProviderType : int
        {
            RsaFull = 1,
            RsaSignature = 2,
            Dss = 3,
            Fortezza = 4,
            MsExchange = 5,
            Ssl = 6,
            RsaSecureChannel = 12,
            DssDiffieHellman = 13,
            EcDsaSignature = 14,
            EcNraSignature = 15,
            EcDsaFull = 16,
            EcNraFull = 17,
            DiffieHellmanSecureChannel = 18,
            SpyrusLynks = 20,
            RandomNumberGenerator = 21,
            IntelSec = 22,
            ReplaceOwf = 23,
            RsaAes = 24
        }

        [Flags]
        enum ContextFlags : uint
        {
            VerifyContext = 0xF0000000,
            NewKeySet = 0x00000008,
            DeleteKeySet = 0x00000010,
            MachineKeySet = 0x00000020,
            Silent = 0x00000040
        }

        enum AlgorithmType : int
        {
            KeyExchange = 1,
            Signature = 2
        }

        enum KeyFlags : int
        {
            Exportable = 0x00000001,
            UserProtected = 0x00000002,
            CreateSalt = 0x00000004,
            UpdateKey = 0x00000008,
            NoSalt = 0x00000010,
            PreGenerate = 0x00000040,
            Online = 0x00000080,
            Sf = 0x00000100,
            CreateIv = 0x00000200,
            KeyExchangeKey = 0x00000400,
            DataKey = 0x00000800,
            Volatile = 0x00001000,
            SgcKey = 0x00002000,
            Archivable = 0x00004000,
            Exportable2k = 0x08000001,
        }

        const int CERT_KEY_SPEC_PROP_ID = 1;
        const int CERT_KEY_PROV_INFO_PROP_ID = 2;

        #endregion

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class CRYPT_KEY_PROV_INFO
        {
            public string container;
            public string provName;
            public int providerType;
            public int flags;
            public int paramsCount;
            public IntPtr param;
            public int keySpec;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_OBJID_BLOB
        {
            public int count;
            public IntPtr parameters;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class CRYPT_ALGORITHM_IDENTIFIER
        {
            public CRYPT_ALGORITHM_IDENTIFIER(string id)
            {
                this.pszObjId = id;
            }
            public string pszObjId;
            public CRYPT_OBJID_BLOB Parameters;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class Sha1AlgorithmId : CRYPT_ALGORITHM_IDENTIFIER
        {
            const string AlgId = "1.2.840.113549.1.1.5";
            public Sha1AlgorithmId() : base(AlgId) { }
        }

        CriticalAllocHandle GetProviderInfo()
        {
            CRYPT_KEY_PROV_INFO provInfo = new CRYPT_KEY_PROV_INFO();
            provInfo.container = this.keyContainerName;
            provInfo.providerType = (int)ProviderType.RsaSecureChannel;
            provInfo.paramsCount = 0;
            provInfo.keySpec = (int)AlgorithmType.KeyExchange;
            return CriticalAllocHandleBlob.FromBlob<CRYPT_KEY_PROV_INFO>(provInfo);
        }

        static CriticalAllocHandle GetSha1AlgorithmId()
        {
            Sha1AlgorithmId sha1Id = new Sha1AlgorithmId();
            return CriticalAllocHandleBlob.FromBlob<CRYPT_ALGORITHM_IDENTIFIER>(sha1Id);
        }
    }
}


