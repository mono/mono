// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509Certificate2.cs
//
// 09/22/2002
//

namespace System.Security.Cryptography.X509Certificates {
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Runtime.Versioning;

    using _FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    public enum X509NameType {
        SimpleName = 0,
        EmailName,
        UpnName,
        DnsName,
        DnsFromAlternativeName,
        UrlName
    }

    public enum X509IncludeOption {
        None = 0,
        ExcludeRoot,
        EndCertOnly,
        WholeChain
    }

    public sealed class PublicKey {
        private AsnEncodedData m_encodedKeyValue;
        private AsnEncodedData m_encodedParameters;
        private Oid m_oid;
        private uint m_aiPubKey = 0;
        private byte[] m_cspBlobData = null;
        private AsymmetricAlgorithm m_key = null;

        private PublicKey() {}

        public PublicKey (Oid oid, AsnEncodedData parameters, AsnEncodedData keyValue) {
            m_oid = new Oid(oid);
            m_encodedParameters = new AsnEncodedData(parameters);
            m_encodedKeyValue = new AsnEncodedData(keyValue);
        }

        internal PublicKey (PublicKey publicKey) {
            m_oid = new Oid(publicKey.m_oid);
            m_encodedParameters = new AsnEncodedData(publicKey.m_encodedParameters);
            m_encodedKeyValue = new AsnEncodedData(publicKey.m_encodedKeyValue);
        }

        internal uint AlgorithmId {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_aiPubKey == 0)
                    m_aiPubKey = X509Utils.OidToAlgId(m_oid.Value);
                return m_aiPubKey;
            }
        }

        private byte[] CspBlobData {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_cspBlobData == null)
                    DecodePublicKeyObject(AlgorithmId, m_encodedKeyValue.RawData, m_encodedParameters.RawData, out m_cspBlobData);
                return m_cspBlobData;
            }
        }

        public AsymmetricAlgorithm Key {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_key == null) {
                    switch (AlgorithmId) {
                    case CAPI.CALG_RSA_KEYX:
                    case CAPI.CALG_RSA_SIGN:
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        rsa.ImportCspBlob(CspBlobData);
                        m_key = rsa;
                        break;

#if !FEATURE_CORESYSTEM
                    case CAPI.CALG_DSS_SIGN:
                        DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                        dsa.ImportCspBlob(CspBlobData);
                        m_key = dsa;
                        break;
#endif

                    default:
                        throw new NotSupportedException(SR.GetString(SR.NotSupported_KeyAlgorithm));
                    }
                }
                return m_key;
            }
        }

        public Oid Oid {
            get { return new Oid(m_oid); }
        }

        public AsnEncodedData EncodedKeyValue {
            get { return m_encodedKeyValue; }
        }

        public AsnEncodedData EncodedParameters {
            get { return m_encodedParameters; }
        }

        //
        // private static methods.
        //

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static void DecodePublicKeyObject(uint aiPubKey, byte[] encodedKeyValue, byte[] encodedParameters, out byte[] decodedData) {
            // Initialize the out parameter
            decodedData = null;
            IntPtr pszStructType = IntPtr.Zero;
            switch (aiPubKey) {
            case CAPI.CALG_DSS_SIGN:
                pszStructType = new IntPtr(CAPI.X509_DSS_PUBLICKEY);
                break;

            case CAPI.CALG_RSA_SIGN:
            case CAPI.CALG_RSA_KEYX:
                pszStructType = new IntPtr(CAPI.RSA_CSP_PUBLICKEYBLOB);
                break;

            case CAPI.CALG_DH_SF:
            case CAPI.CALG_DH_EPHEM:
                // We don't support DH for now
                throw new NotSupportedException(SR.GetString(SR.NotSupported_KeyAlgorithm));

            default:
                // We should never get here
                Debug.Assert(false);
                throw new NotSupportedException(SR.GetString(SR.NotSupported_KeyAlgorithm));
            }

            SafeLocalAllocHandle decodedKeyValue = null;
            uint cbDecodedKeyValue = 0;
            bool result = CAPI.DecodeObject(pszStructType,
                                            encodedKeyValue,
                                            out decodedKeyValue,
                                            out cbDecodedKeyValue);
            if (!result)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            if ((uint) pszStructType == CAPI.RSA_CSP_PUBLICKEYBLOB) {
                decodedData = new byte[cbDecodedKeyValue];
                Marshal.Copy(decodedKeyValue.DangerousGetHandle(), decodedData, 0, decodedData.Length);
            } else if ((uint) pszStructType == CAPI.X509_DSS_PUBLICKEY) {
                // We need to decode the parameters as well
                SafeLocalAllocHandle decodedParameters = null;
                uint cbDecodedParameters = 0;
                result = CAPI.DecodeObject(new IntPtr(CAPI.X509_DSS_PARAMETERS), 
                                           encodedParameters,
                                           out decodedParameters,
                                           out cbDecodedParameters); 
                if (!result)
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                decodedData = ConstructDSSPubKeyCspBlob(decodedKeyValue, decodedParameters);
                decodedParameters.Dispose();
            }
            decodedKeyValue.Dispose();
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static byte[] ConstructDSSPubKeyCspBlob (SafeLocalAllocHandle decodedKeyValue,
                                                         SafeLocalAllocHandle decodedParameters) {

            // The CAPI DSS public key representation consists of the following sequence:
            //  - PUBLICKEYSTRUC
            //  - DSSPUBKEY
            //  - rgbP[cbKey]
            //  - rgbQ[20]
            //  - rgbG[cbKey]
            //  - rgbY[cbKey]
            //  - DSSSEED

            CAPI.CRYPTOAPI_BLOB pDssPubKey = (CAPI.CRYPTOAPI_BLOB) Marshal.PtrToStructure(decodedKeyValue.DangerousGetHandle(), typeof(CAPI.CRYPTOAPI_BLOB));
            CAPI.CERT_DSS_PARAMETERS pDssParameters = (CAPI.CERT_DSS_PARAMETERS) Marshal.PtrToStructure(decodedParameters.DangerousGetHandle(), typeof(CAPI.CERT_DSS_PARAMETERS));

            uint cbKey = pDssParameters.p.cbData;
            if (cbKey == 0)
                throw new CryptographicException(CAPI.NTE_BAD_PUBLIC_KEY);

            const uint DSS_Q_LEN = 20;
            uint cbKeyBlob = 8 /* sizeof(CAPI.BLOBHEADER) */ + 8 /* sizeof(CAPI.DSSPUBKEY) */ +
                        cbKey + DSS_Q_LEN + cbKey + cbKey + 24 /* sizeof(CAPI.DSSSEED) */;

            MemoryStream keyBlob = new MemoryStream((int) cbKeyBlob);
            BinaryWriter bw = new BinaryWriter(keyBlob);

            // PUBLICKEYSTRUC
            bw.Write(CAPI.PUBLICKEYBLOB); // pPubKeyStruc->bType = PUBLICKEYBLOB
            bw.Write(CAPI.CUR_BLOB_VERSION); // pPubKeyStruc->bVersion = CUR_BLOB_VERSION
            bw.Write((short) 0); // pPubKeyStruc->reserved = 0;
            bw.Write(CAPI.CALG_DSS_SIGN); // pPubKeyStruc->aiKeyAlg = CALG_DSS_SIGN;

            // DSSPUBKEY
            bw.Write(CAPI.DSS_MAGIC); // pCspPubKey->magic = DSS_MAGIC; We are constructing a DSS1 Csp blob.
            bw.Write(cbKey * 8); // pCspPubKey->bitlen = cbKey * 8;

            // rgbP[cbKey]
            byte[] p = new byte[pDssParameters.p.cbData];
            Marshal.Copy(pDssParameters.p.pbData, p, 0, p.Length);
            bw.Write(p);

            // rgbQ[20]
            uint cb = pDssParameters.q.cbData;
            if (cb == 0 || cb > DSS_Q_LEN)
                throw new CryptographicException(CAPI.NTE_BAD_PUBLIC_KEY);

            byte[] q = new byte[pDssParameters.q.cbData];
            Marshal.Copy(pDssParameters.q.pbData, q, 0, q.Length);
            bw.Write(q);
            if (DSS_Q_LEN > cb)
                bw.Write(new byte[DSS_Q_LEN - cb]);

            // rgbG[cbKey]
            cb = pDssParameters.g.cbData;
            if (cb == 0 || cb > cbKey)
                throw new CryptographicException(CAPI.NTE_BAD_PUBLIC_KEY);

            byte[] g = new byte[pDssParameters.g.cbData];
            Marshal.Copy(pDssParameters.g.pbData, g, 0, g.Length);
            bw.Write(g);
            if (cbKey > cb)
                bw.Write(new byte[cbKey - cb]);

            // rgbY[cbKey]
            cb = pDssPubKey.cbData;
            if (cb == 0 || cb > cbKey)
                throw new CryptographicException(CAPI.NTE_BAD_PUBLIC_KEY);

            byte[] y = new byte[pDssPubKey.cbData];
            Marshal.Copy(pDssPubKey.pbData, y, 0, y.Length);
            bw.Write(y);
            if (cbKey > cb)
                bw.Write(new byte[cbKey - cb]);

            // DSSSEED: set counter to 0xFFFFFFFF to indicate not available
            bw.Write(0xFFFFFFFF);
            bw.Write(new byte[20]);

            return keyBlob.ToArray();
        }
    }

#if !FEATURE_CORESYSTEM
    [Serializable]
#endif
    public class X509Certificate2 : X509Certificate {
        private int m_version; 
        private DateTime m_notBefore;
        private DateTime m_notAfter;
        private AsymmetricAlgorithm m_privateKey;
        private PublicKey m_publicKey;
        private X509ExtensionCollection m_extensions;
        private Oid m_signatureAlgorithm;
        private X500DistinguishedName m_subjectName;
        private X500DistinguishedName m_issuerName;
#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
        private SafeCertContextHandle m_safeCertContext = SafeCertContextHandle.InvalidHandle;
        
        private static int s_publicKeyOffset;

        //
        // public constructors
        //

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509Certificate2 () : base() {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509Certificate2 (byte[] rawData) : base (rawData) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509Certificate2 (byte[] rawData, string password) : base (rawData, password) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if !FEATURE_CORESYSTEM
        public X509Certificate2 (byte[] rawData, SecureString password) : base (rawData, password) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }
#endif

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509Certificate2 (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags) : base (rawData, password, keyStorageFlags) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if !FEATURE_CORESYSTEM
        public X509Certificate2 (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags) : base (rawData, password, keyStorageFlags) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }
#endif

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        [ResourceExposure(ResourceScope.Machine)]
#if !FEATURE_CORESYSTEM
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public X509Certificate2 (string fileName) : base (fileName) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        [ResourceExposure(ResourceScope.Machine)]
#if !FEATURE_CORESYSTEM
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public X509Certificate2 (string fileName, string password) : base (fileName, password) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if !FEATURE_CORESYSTEM
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public X509Certificate2 (string fileName, SecureString password) : base (fileName, password) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }
#endif

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        [ResourceExposure(ResourceScope.Machine)]
#if !FEATURE_CORESYSTEM
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public X509Certificate2 (string fileName, string password, X509KeyStorageFlags keyStorageFlags) : base (fileName, password, keyStorageFlags) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if !FEATURE_CORESYSTEM
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public X509Certificate2 (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) : base (fileName, password, keyStorageFlags) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }
#endif

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        // Package protected constructor for creating a certificate from a PCCERT_CONTEXT
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public X509Certificate2 (IntPtr handle) : base (handle) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509Certificate2 (X509Certificate certificate) : base(certificate) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

#if !FEATURE_CORESYSTEM
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected X509Certificate2(SerializationInfo info, StreamingContext context) : base(info, context) {
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }
#endif

        public override string ToString() {
            return base.ToString(true);
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public override string ToString(bool verbose) {
            if (verbose == false || m_safeCertContext.IsInvalid)
                return ToString();

            StringBuilder sb = new StringBuilder();
            string newLine = Environment.NewLine;
            string newLine2 = newLine + newLine;
            string newLinesp2 = newLine + "  ";

            // Version
            sb.Append("[Version]");
            sb.Append(newLinesp2);
            sb.Append("V" + this.Version);

            // Subject
            sb.Append(newLine2);
            sb.Append("[Subject]");
            sb.Append(newLinesp2);
            sb.Append(this.SubjectName.Name);
            string simpleName = GetNameInfo(X509NameType.SimpleName, false);
            if (simpleName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("Simple Name: ");
                sb.Append(simpleName);
            }
            string emailName = GetNameInfo(X509NameType.EmailName, false);
            if (emailName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("Email Name: ");
                sb.Append(emailName);
            }
            string upnName = GetNameInfo(X509NameType.UpnName, false);
            if (upnName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("UPN Name: ");
                sb.Append(upnName);
            }
            string dnsName = GetNameInfo(X509NameType.DnsName, false);
            if (dnsName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("DNS Name: ");
                sb.Append(dnsName);
            }

            // Issuer
            sb.Append(newLine2);
            sb.Append("[Issuer]");
            sb.Append(newLinesp2);
            sb.Append(this.IssuerName.Name);
            simpleName = GetNameInfo(X509NameType.SimpleName, true);
            if (simpleName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("Simple Name: ");
                sb.Append(simpleName);
            }
            emailName = GetNameInfo(X509NameType.EmailName, true);
            if (emailName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("Email Name: ");
                sb.Append(emailName);
            }
            upnName = GetNameInfo(X509NameType.UpnName, true);
            if (upnName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("UPN Name: ");
                sb.Append(upnName);
            }
            dnsName = GetNameInfo(X509NameType.DnsName, true);
            if (dnsName.Length > 0) {
                sb.Append(newLinesp2);
                sb.Append("DNS Name: ");
                sb.Append(dnsName);
            }

            // Serial Number
            sb.Append(newLine2);
            sb.Append("[Serial Number]");
            sb.Append(newLinesp2);
            sb.Append(this.SerialNumber);

            // NotBefore
            sb.Append(newLine2);
            sb.Append("[Not Before]");
            sb.Append(newLinesp2);
            sb.Append(FormatDate(this.NotBefore));

            // NotAfter
            sb.Append(newLine2);
            sb.Append("[Not After]");
            sb.Append(newLinesp2);
            sb.Append(FormatDate(this.NotAfter));

            // Thumbprint
            sb.Append(newLine2);
            sb.Append("[Thumbprint]");
            sb.Append(newLinesp2);
            sb.Append(this.Thumbprint);

            // Signature Algorithm
            sb.Append(newLine2);
            sb.Append("[Signature Algorithm]");
            sb.Append(newLinesp2);
            sb.Append(this.SignatureAlgorithm.FriendlyName + "(" + this.SignatureAlgorithm.Value + ")");

            // Public Key
            sb.Append(newLine2);
            sb.Append("[Public Key]");
            // It could throw if it's some user-defined CryptoServiceProvider
            try {
                PublicKey pubKey = this.PublicKey;

                string temp = pubKey.Oid.FriendlyName;
                sb.Append(newLinesp2);
                sb.Append("Algorithm: ");
                sb.Append(temp);
                // So far, we only support RSACryptoServiceProvider & DSACryptoServiceProvider Keys
                try {
                    temp = pubKey.Key.KeySize.ToString();
                    sb.Append(newLinesp2);
                    sb.Append("Length: ");
                    sb.Append(temp);
                }
                catch (NotSupportedException) {
                }

                temp = pubKey.EncodedKeyValue.Format(true);
                sb.Append(newLinesp2);
                sb.Append("Key Blob: ");
                sb.Append(temp);

                temp = pubKey.EncodedParameters.Format(true);
                sb.Append(newLinesp2);
                sb.Append("Parameters: ");
                sb.Append(temp);
            }
            catch (CryptographicException) {
            }

            // Private key
            AppendPrivateKeyInfo(sb);

            // Extensions
            X509ExtensionCollection extensions = this.Extensions;
            if (extensions.Count > 0) {
                sb.Append(newLine2);
                sb.Append("[Extensions]");
                string temp;
                foreach (X509Extension extension in extensions) {
                    try {
                        temp = extension.Oid.FriendlyName;
                        sb.Append(newLine);
                        sb.Append("* " + temp);
                        sb.Append("(" + extension.Oid.Value + "):");

                        temp = extension.Format(true);
                        sb.Append(newLinesp2);
                        sb.Append(temp);
                    }
                    catch (CryptographicException) {
                    }
                }
            }

            sb.Append(newLine);
            return sb.ToString();
        }

        public bool Archived {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                uint cbData = 0;
                return CAPI.CertGetCertificateContextProperty(m_safeCertContext, 
                                                              CAPI.CERT_ARCHIVED_PROP_ID, 
                                                              SafeLocalAllocHandle.InvalidHandle, 
                                                              ref cbData);
            }
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            set {
                SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
                if (value == true) 
                    ptr = CAPI.LocalAlloc(CAPI.LPTR, new IntPtr(Marshal.SizeOf(typeof(CAPI.CRYPTOAPI_BLOB))));

                if (!CAPI.CertSetCertificateContextProperty(m_safeCertContext,
                                                            CAPI.CERT_ARCHIVED_PROP_ID,
                                                            0,
                                                            ptr))
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                ptr.Dispose();
            }
        }

        public X509ExtensionCollection Extensions {
#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_extensions == null) 
                    m_extensions = new X509ExtensionCollection(m_safeCertContext);

                return m_extensions;
            }
        }

        public string FriendlyName {
#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
                uint cbData = 0;
                if (!CAPI.CertGetCertificateContextProperty(m_safeCertContext, 
                                                            CAPI.CERT_FRIENDLY_NAME_PROP_ID, 
                                                            ptr, 
                                                            ref cbData))
                    return String.Empty;

                ptr = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbData));
                if (!CAPI.CertGetCertificateContextProperty(m_safeCertContext, 
                                                            CAPI.CERT_FRIENDLY_NAME_PROP_ID, 
                                                            ptr, 
                                                            ref cbData))
                    return String.Empty;

                string friendlyName = Marshal.PtrToStringUni(ptr.DangerousGetHandle());
                ptr.Dispose();
                return friendlyName;
            }
#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
            set {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (value == null)
                    value = String.Empty;

                SetFriendlyNameExtendedProperty(m_safeCertContext, value);
            }
        }

        public X500DistinguishedName IssuerName {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_issuerName == null) {
                    unsafe {
                        CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) m_safeCertContext.DangerousGetHandle());
                        CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));
                        m_issuerName = new X500DistinguishedName(pCertInfo.Issuer);
                    }
                }

                return m_issuerName;
            }
        }

        public DateTime NotAfter {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_notAfter == DateTime.MinValue) {
                    unsafe {
                        CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) m_safeCertContext.DangerousGetHandle());
                        CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));
                        long dt = (((long)(uint)pCertInfo.NotAfter.dwHighDateTime) << 32) | ((long)(uint)pCertInfo.NotAfter.dwLowDateTime);
                        m_notAfter = DateTime.FromFileTime(dt);
                    }
                }

                return m_notAfter;
            }
        }

        public DateTime NotBefore {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_notBefore == DateTime.MinValue) {
                    unsafe {
                        CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) m_safeCertContext.DangerousGetHandle());
                        CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));
                        long dt = (((long)(uint)pCertInfo.NotBefore.dwHighDateTime) << 32) | ((long)(uint)pCertInfo.NotBefore.dwLowDateTime);
                        m_notBefore = DateTime.FromFileTime(dt);
                    }
                }

                return m_notBefore;
            }
        }

        public bool HasPrivateKey {
#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                uint cbData = 0;
                return CAPI.CertGetCertificateContextProperty(m_safeCertContext,
                                                              CAPI.CERT_KEY_PROV_INFO_PROP_ID,
                                                              SafeLocalAllocHandle.InvalidHandle,
                                                              ref cbData);
            }
        }

        public AsymmetricAlgorithm PrivateKey {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (!this.HasPrivateKey)
                    return null;

                if (m_privateKey == null) {
                    CspParameters parameters = new CspParameters();
                    if (!GetPrivateKeyInfo(m_safeCertContext, ref parameters))
                        return null;

                    // We never want to stomp over certificate private keys.
                    parameters.Flags |= CspProviderFlags.UseExistingKey;
                    switch (this.PublicKey.AlgorithmId) {
                    case CAPI.CALG_RSA_KEYX:
                    case CAPI.CALG_RSA_SIGN:
                        m_privateKey = new RSACryptoServiceProvider(parameters);
                        break;

#if !FEATURE_CORESYSTEM
                    case CAPI.CALG_DSS_SIGN:
                        m_privateKey = new DSACryptoServiceProvider(parameters);
                        break;
#endif

                    default:
                        throw new NotSupportedException(SR.GetString(SR.NotSupported_KeyAlgorithm));
                    }
                }

                return m_privateKey;
            }
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety")]
            set {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                // we do not support keys in non-CAPI storage for now.
                ICspAsymmetricAlgorithm asymmetricAlgorithm = value as ICspAsymmetricAlgorithm;
                if (value != null && asymmetricAlgorithm == null)
                    throw new NotSupportedException(SR.GetString(SR.NotSupported_InvalidKeyImpl));

                // A null value can be passed in to remove the link to the private key from the certificate.
                if (asymmetricAlgorithm != null) {
                    if (asymmetricAlgorithm.CspKeyContainerInfo == null)
                        throw new ArgumentException("CspKeyContainerInfo");
                    
                    // check that the public key in the certificate corresponds to the private key passed in.
                    // 
                    // note that it should be legal to set a key which matches in every aspect but the usage
                    // i.e. to use a CALG_RSA_KEYX private key to match a CALG_RSA_SIGN public key. A
                    // PUBLICKEYBLOB is defined as:
                    //
                    //  BLOBHEADER publickeystruc
                    //  RSAPUBKEY rsapubkey
                    //  BYTE modulus[rsapubkey.bitlen/8]
                    //  
                    // To allow keys which differ by key usage only, we skip over the BLOBHEADER of the key,
                    // and start comparing bytes at the RSAPUBKEY structure.
                    if(s_publicKeyOffset == 0)
                        s_publicKeyOffset = Marshal.SizeOf(typeof(CAPIBase.BLOBHEADER));
                    
                    ICspAsymmetricAlgorithm publicKey = this.PublicKey.Key as ICspAsymmetricAlgorithm;
                    byte[] array1 = publicKey.ExportCspBlob(false);
                    byte[] array2 = asymmetricAlgorithm.ExportCspBlob(false);
                    if (array1 == null || array2 == null || array1.Length != array2.Length || array1.Length <= s_publicKeyOffset)
                        throw new CryptographicUnexpectedOperationException(SR.GetString(SR.Cryptography_X509_KeyMismatch));
                    for (int index = s_publicKeyOffset; index < array1.Length; index++) {
                        if (array1[index] != array2[index])
                            throw new CryptographicUnexpectedOperationException(SR.GetString(SR.Cryptography_X509_KeyMismatch));
                    }
                }

                // Establish the link between the certificate and the key container.
                SetPrivateKeyProperty(m_safeCertContext, asymmetricAlgorithm);

                m_privateKey = value;
            }
        }

        public PublicKey PublicKey {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_publicKey == null) {
                    string friendlyName = this.GetKeyAlgorithm();
                    byte[] parameters = this.GetKeyAlgorithmParameters();
                    byte[] keyValue = this.GetPublicKey();
                    Oid oid = new Oid(friendlyName, OidGroup.PublicKeyAlgorithm, true);
                    m_publicKey = new PublicKey(oid, new AsnEncodedData(oid, parameters), new AsnEncodedData(oid, keyValue));
                }

                return m_publicKey;
            }
        }

        public byte[] RawData {
            get {
                return GetRawCertData();
            }
        }

        public string SerialNumber {
            get {
                return GetSerialNumberString();
            }
        }

        public X500DistinguishedName SubjectName {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_subjectName == null) {
                    unsafe {
                        CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) m_safeCertContext.DangerousGetHandle());
                        CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));
                        m_subjectName = new X500DistinguishedName(pCertInfo.Subject);
                    }
                }

                return m_subjectName;
            }
        }

        public Oid SignatureAlgorithm {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_signatureAlgorithm == null)
                    m_signatureAlgorithm = GetSignatureAlgorithm(m_safeCertContext);

                return m_signatureAlgorithm;
            }
        }

        public string Thumbprint {
            get {
                return GetCertHashString();
            }
        }

        public int Version {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (m_safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

                if (m_version == 0) 
                    m_version = (int) GetVersion(m_safeCertContext);

                return m_version; 
            }
        }

#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
        public unsafe string GetNameInfo(X509NameType nameType, bool forIssuer) {
            uint issuerFlag = forIssuer ? CAPI.CERT_NAME_ISSUER_FLAG : 0;
            uint type = X509Utils.MapNameType(nameType);

            switch(type) {
            case CAPI.CERT_NAME_SIMPLE_DISPLAY_TYPE:
                return CAPI.GetCertNameInfo(m_safeCertContext, issuerFlag, type);

            case CAPI.CERT_NAME_EMAIL_TYPE:
                return CAPI.GetCertNameInfo(m_safeCertContext, issuerFlag, type);
            }

            string name = String.Empty;
            // If the type requested is not supported in downlevel platforms; we try to decode the alt name extension by hand.
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) m_safeCertContext.DangerousGetHandle());
            CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));

            IntPtr[] pAltName = new IntPtr[2];
            pAltName[0] = CAPI.CertFindExtension(forIssuer ? CAPI.szOID_ISSUER_ALT_NAME : CAPI.szOID_SUBJECT_ALT_NAME,
                                                 pCertInfo.cExtension,
                                                 pCertInfo.rgExtension);
            pAltName[1] = CAPI.CertFindExtension(forIssuer ? CAPI.szOID_ISSUER_ALT_NAME2 : CAPI.szOID_SUBJECT_ALT_NAME2,
                                                 pCertInfo.cExtension,
                                                 pCertInfo.rgExtension);
            for (int i = 0; i < pAltName.Length; i++) {
                if (pAltName[i] != IntPtr.Zero) {
                    CAPI.CERT_EXTENSION extension = (CAPI.CERT_EXTENSION) Marshal.PtrToStructure(pAltName[i], typeof(CAPI.CERT_EXTENSION));
                    byte[] rawData = new byte[extension.Value.cbData];
                    Marshal.Copy(extension.Value.pbData, rawData, 0, rawData.Length);

                    uint cbDecoded = 0;
                    SafeLocalAllocHandle decoded = null;
                    // Decode the extension.
                    SafeLocalAllocHandle ptr = X509Utils.StringToAnsiPtr(extension.pszObjId);
                    bool result = CAPI.DecodeObject(ptr.DangerousGetHandle(), 
                                                    rawData,
                                                    out decoded,
                                                    out cbDecoded);
                    ptr.Dispose();
                    if (result) {
                        CAPI.CERT_ALT_NAME_INFO altNameInfo = (CAPI.CERT_ALT_NAME_INFO) Marshal.PtrToStructure(decoded.DangerousGetHandle(), typeof(CAPI.CERT_ALT_NAME_INFO));

                        for (int index = 0; index < altNameInfo.cAltEntry; index++) {
                            IntPtr pAltInfoPtr = new IntPtr((long) altNameInfo.rgAltEntry + index * Marshal.SizeOf(typeof(CAPI.CERT_ALT_NAME_ENTRY)));
                            CAPI.CERT_ALT_NAME_ENTRY altNameEntry = (CAPI.CERT_ALT_NAME_ENTRY) Marshal.PtrToStructure(pAltInfoPtr, typeof(CAPI.CERT_ALT_NAME_ENTRY));

                            switch(type) {
                            case CAPI.CERT_NAME_UPN_TYPE:
                                if (altNameEntry.dwAltNameChoice == CAPI.CERT_ALT_NAME_OTHER_NAME) {
                                    CAPI.CERT_OTHER_NAME otherName = (CAPI.CERT_OTHER_NAME) Marshal.PtrToStructure(altNameEntry.Value.pOtherName, typeof(CAPI.CERT_OTHER_NAME));
                                    if (otherName.pszObjId == CAPI.szOID_NT_PRINCIPAL_NAME) {
                                        uint cbUpnName = 0;
                                        SafeLocalAllocHandle pUpnName = null;
                                        result = CAPI.DecodeObject(new IntPtr(CAPI.X509_UNICODE_ANY_STRING), 
                                                                   X509Utils.PtrToByte(otherName.Value.pbData, otherName.Value.cbData),
                                                                   out pUpnName,
                                                                   out cbUpnName);
                                        if (result) {
                                            CAPI.CERT_NAME_VALUE nameValue = (CAPI.CERT_NAME_VALUE) Marshal.PtrToStructure(pUpnName.DangerousGetHandle(), typeof(CAPI.CERT_NAME_VALUE));
                                            if (X509Utils.IsCertRdnCharString(nameValue.dwValueType))
                                                name = Marshal.PtrToStringUni(nameValue.Value.pbData);
                                            pUpnName.Dispose();
                                        }
                                    }
                                }
                                break;

                            case CAPI.CERT_NAME_DNS_TYPE:
                                if (altNameEntry.dwAltNameChoice == CAPI.CERT_ALT_NAME_DNS_NAME)
                                    name = Marshal.PtrToStringUni(altNameEntry.Value.pwszDNSName);

                                break;

                            case CAPI.CERT_NAME_URL_TYPE:
                                if (altNameEntry.dwAltNameChoice == CAPI.CERT_ALT_NAME_URL)
                                    name = Marshal.PtrToStringUni(altNameEntry.Value.pwszURL);

                                break;
                            }
                        }
                        decoded.Dispose();
                    }
                }
            }

            if (nameType == X509NameType.DnsName) {
                // If no DNS name is found in the CERT_ALT_NAME extension, return the CommonName.
                // Commercial CAs such as Verisign don't include a SubjectAltName extension in the certificates they use for SSL server authentication.
                // Instead they use the CommonName in the subject RDN as the server's DNS name.

                if (name == null || name.Length == 0)
                    name = CAPI.GetCertNameInfo(m_safeCertContext, issuerFlag, CAPI.CERT_NAME_ATTR_TYPE);
            }

            return name;
        }

#if !FEATURE_CORESYSTEM
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Import(byte[] rawData) {
            Reset();
            base.Import(rawData);
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags) {
            Reset();
            base.Import(rawData, password, keyStorageFlags);
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags) {
            Reset();
            base.Import(rawData, password, keyStorageFlags);
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public override void Import(string fileName) {
            Reset();
            base.Import(fileName);
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public override void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags) {
            Reset();
            base.Import(fileName, password, keyStorageFlags);
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public override void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) {
            Reset();
            base.Import(fileName, password, keyStorageFlags);
            m_safeCertContext = CAPI.CertDuplicateCertificateContext(this.Handle);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Reset () {
            m_version = 0; 
            m_notBefore = DateTime.MinValue;
            m_notAfter = DateTime.MinValue;
            m_privateKey = null;
            m_publicKey = null;
            m_extensions = null;
            m_signatureAlgorithm = null;
            m_subjectName = null;
            m_issuerName = null;
            if (!m_safeCertContext.IsInvalid) {
                // Free the current certificate handle
                m_safeCertContext.Dispose();
                m_safeCertContext = SafeCertContextHandle.InvalidHandle;
            }
            base.Reset();
        }
#endif // !FEATURE_CORESYSTEM

        public bool Verify () {
            if (m_safeCertContext.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "m_safeCertContext");

            int hr = X509Utils.VerifyCertificate(this.CertContext, 
                                   null,
                                   null,
                                   X509RevocationMode.Online, // We default to online revocation check.
                                   X509RevocationFlag.ExcludeRoot,
                                   DateTime.Now,
                                   new TimeSpan(0, 0, 0), // default
                                   null,
                                   new IntPtr(CAPI.CERT_CHAIN_POLICY_BASE), 
                                   IntPtr.Zero);
            return (hr == CAPI.S_OK);
        }

        // 
        // public static methods
        //

        public static X509ContentType GetCertContentType (byte[] rawData) {
            if (rawData == null || rawData.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Arg_EmptyOrNullArray), "rawData");

            uint contentType = QueryCertBlobType(rawData);
            return X509Utils.MapContentType(contentType);
        }

        [ResourceExposure(ResourceScope.Machine)]
#if !FEATURE_CORESYSTEM
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public static X509ContentType GetCertContentType (string fileName) {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            string fullPath = Path.GetFullPath(fileName);
#if !FEATURE_CORESYSTEM
            new FileIOPermission (FileIOPermissionAccess.Read, fullPath).Demand();
#endif
            uint contentType = QueryCertFileType(fileName);
            return X509Utils.MapContentType(contentType);
        }

        //
        // Internal
        //

        internal SafeCertContextHandle CertContext {
#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
            get {
                return m_safeCertContext;
            }
        }

#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
        internal static bool GetPrivateKeyInfo (SafeCertContextHandle safeCertContext, ref CspParameters parameters) {
            SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
            uint cbData = 0;
            if (!CAPI.CertGetCertificateContextProperty(safeCertContext,
                                                        CAPI.CERT_KEY_PROV_INFO_PROP_ID,
                                                        ptr,
                                                        ref cbData)) {
                int dwErrorCode = Marshal.GetLastWin32Error();
                if (dwErrorCode == CAPI.CRYPT_E_NOT_FOUND)
                    return false;
                else
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            ptr = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbData));
            if (!CAPI.CertGetCertificateContextProperty(safeCertContext,
                                                        CAPI.CERT_KEY_PROV_INFO_PROP_ID,
                                                        ptr,
                                                        ref cbData)) {
                int dwErrorCode = Marshal.GetLastWin32Error();
                if (dwErrorCode == CAPI.CRYPT_E_NOT_FOUND)
                    return false;
                else
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            CAPI.CRYPT_KEY_PROV_INFO pKeyProvInfo = (CAPI.CRYPT_KEY_PROV_INFO) Marshal.PtrToStructure(ptr.DangerousGetHandle(), typeof(CAPI.CRYPT_KEY_PROV_INFO));
            parameters.ProviderName = pKeyProvInfo.pwszProvName;
            parameters.KeyContainerName = pKeyProvInfo.pwszContainerName;
            parameters.ProviderType = (int) pKeyProvInfo.dwProvType;
            parameters.KeyNumber = (int) pKeyProvInfo.dwKeySpec;
            parameters.Flags = (CspProviderFlags) ((pKeyProvInfo.dwFlags & CAPI.CRYPT_MACHINE_KEYSET) == CAPI.CRYPT_MACHINE_KEYSET ? CspProviderFlags.UseMachineKeyStore : 0);

            ptr.Dispose();
            return true;
        }

        //
        // Private
        //

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private void AppendPrivateKeyInfo (StringBuilder sb) {
            CspKeyContainerInfo cspKeyContainerInfo = null;
            try {
                if (this.HasPrivateKey) {
                    CspParameters parameters = new CspParameters();
                    if (GetPrivateKeyInfo(m_safeCertContext, ref parameters))
                        cspKeyContainerInfo = new CspKeyContainerInfo(parameters);
                }
            }
            // We don't have the permission to access the key container. Just return.
            catch (SecurityException) {}
            // We could not access the key container. Just return.
            catch (CryptographicException) {}

            if (cspKeyContainerInfo == null)
                return;

            sb.Append(Environment.NewLine + Environment.NewLine + "[Private Key]");
            sb.Append(Environment.NewLine + "  Key Store: ");
            sb.Append(cspKeyContainerInfo.MachineKeyStore ? "Machine" : "User");
            sb.Append(Environment.NewLine + "  Provider Name: ");
            sb.Append(cspKeyContainerInfo.ProviderName);
            sb.Append(Environment.NewLine + "  Provider type: ");
            sb.Append(cspKeyContainerInfo.ProviderType);
            sb.Append(Environment.NewLine + "  Key Spec: ");
            sb.Append(cspKeyContainerInfo.KeyNumber);
            sb.Append(Environment.NewLine + "  Key Container Name: ");
            sb.Append(cspKeyContainerInfo.KeyContainerName);

            try {
                string uniqueKeyContainer = cspKeyContainerInfo.UniqueKeyContainerName;
                sb.Append(Environment.NewLine + "  Unique Key Container Name: ");
                sb.Append(uniqueKeyContainer);
            }
            catch (CryptographicException) {}
            catch (NotSupportedException) {}

            bool b = false;
            try {
                b = cspKeyContainerInfo.HardwareDevice;
                sb.Append(Environment.NewLine + "  Hardware Device: ");
                sb.Append(b);
            }
            catch (CryptographicException) {}

            try {
                b = cspKeyContainerInfo.Removable;
                sb.Append(Environment.NewLine + "  Removable: ");
                sb.Append(b);
            }
            catch (CryptographicException) {}

            try {
                b = cspKeyContainerInfo.Protected;
                sb.Append(Environment.NewLine + "  Protected: ");
                sb.Append(b);
            }
            catch (CryptographicException) {}
            catch (NotSupportedException) {}
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe Oid GetSignatureAlgorithm (SafeCertContextHandle safeCertContextHandle) {
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));
            return new Oid(pCertInfo.SignatureAlgorithm.pszObjId, OidGroup.SignatureAlgorithm, false);
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe uint GetVersion (SafeCertContextHandle safeCertContextHandle) {
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));
            return (pCertInfo.dwVersion + 1);
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        [ResourceExposure(ResourceScope.None)]
#if !FEATURE_CORESYSTEM
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
#endif
        private static unsafe uint QueryCertBlobType(byte[] rawData) {
            uint contentType = 0;
            if (!CAPI.CryptQueryObject(CAPI.CERT_QUERY_OBJECT_BLOB,
                                       rawData,
                                       CAPI.CERT_QUERY_CONTENT_FLAG_ALL,
                                       CAPI.CERT_QUERY_FORMAT_FLAG_ALL,
                                       0,
                                       IntPtr.Zero,
                                       new IntPtr(&contentType),
                                       IntPtr.Zero,
                                       IntPtr.Zero,
                                       IntPtr.Zero,
                                       IntPtr.Zero))
                throw new CryptographicException(Marshal.GetLastWin32Error());

            return contentType;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        [ResourceExposure(ResourceScope.Machine)]
#if !FEATURE_CORESYSTEM
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        private static unsafe uint QueryCertFileType(string fileName) {
            uint contentType = 0;
            if (!CAPI.CryptQueryObject(CAPI.CERT_QUERY_OBJECT_FILE,
                                       fileName,
                                       CAPI.CERT_QUERY_CONTENT_FLAG_ALL,
                                       CAPI.CERT_QUERY_FORMAT_FLAG_ALL,
                                       0,
                                       IntPtr.Zero,
                                       new IntPtr(&contentType),
                                       IntPtr.Zero,
                                       IntPtr.Zero,
                                       IntPtr.Zero,
                                       IntPtr.Zero))
                throw new CryptographicException(Marshal.GetLastWin32Error());

            return contentType;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe void SetFriendlyNameExtendedProperty (SafeCertContextHandle safeCertContextHandle, string name) {
            SafeLocalAllocHandle ptr = X509Utils.StringToUniPtr(name);
            using (ptr) {
                CAPI.CRYPTOAPI_BLOB DataBlob = new CAPI.CRYPTOAPI_BLOB();
                DataBlob.cbData = 2 * ((uint) name.Length + 1);
                DataBlob.pbData = ptr.DangerousGetHandle();

                if (!CAPI.CertSetCertificateContextProperty(safeCertContextHandle,
                                                            CAPI.CERT_FRIENDLY_NAME_PROP_ID,
                                                            0,
                                                            new IntPtr(&DataBlob)))
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe void SetPrivateKeyProperty (SafeCertContextHandle safeCertContextHandle, ICspAsymmetricAlgorithm asymmetricAlgorithm) {
            SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
            if (asymmetricAlgorithm != null) {
                CAPI.CRYPT_KEY_PROV_INFO keyProvInfo = new CAPI.CRYPT_KEY_PROV_INFO();
                keyProvInfo.pwszContainerName = asymmetricAlgorithm.CspKeyContainerInfo.KeyContainerName;
                keyProvInfo.pwszProvName = asymmetricAlgorithm.CspKeyContainerInfo.ProviderName;
                keyProvInfo.dwProvType = (uint) asymmetricAlgorithm.CspKeyContainerInfo.ProviderType;
                keyProvInfo.dwFlags = asymmetricAlgorithm.CspKeyContainerInfo.MachineKeyStore ? CAPI.CRYPT_MACHINE_KEYSET : 0;
                keyProvInfo.cProvParam = 0;
                keyProvInfo.rgProvParam = IntPtr.Zero;
                keyProvInfo.dwKeySpec = (uint) asymmetricAlgorithm.CspKeyContainerInfo.KeyNumber;

                ptr = CAPI.LocalAlloc(CAPI.LPTR, new IntPtr(Marshal.SizeOf(typeof(CAPI.CRYPT_KEY_PROV_INFO))));
                Marshal.StructureToPtr(keyProvInfo, ptr.DangerousGetHandle(), false);
            }

            try {
                if (!CAPI.CertSetCertificateContextProperty(safeCertContextHandle,
                                                            CAPI.CERT_KEY_PROV_INFO_PROP_ID,
                                                            0,
                                                            ptr))
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            } finally {
                if (!ptr.IsInvalid) {
                    Marshal.DestroyStructure(ptr.DangerousGetHandle(), typeof(CAPI.CRYPT_KEY_PROV_INFO));
                    ptr.Dispose();
                }
            }
        }
    }
}
