// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509Extension.cs
//

namespace System.Security.Cryptography.X509Certificates {
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public class X509Extension : AsnEncodedData {
        private bool m_critical = false;

        internal X509Extension(string oid) : base (new Oid(oid, OidGroup.ExtensionOrAttribute, false)) {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        internal X509Extension(IntPtr pExtension) {
            CAPI.CERT_EXTENSION extension = (CAPI.CERT_EXTENSION) Marshal.PtrToStructure(pExtension, typeof(CAPI.CERT_EXTENSION));
            m_critical = extension.fCritical;
            string oidValue = extension.pszObjId;
            m_oid = new Oid(oidValue, OidGroup.ExtensionOrAttribute, false);
            byte[] rawData = new byte[extension.Value.cbData];
            if (extension.Value.pbData != IntPtr.Zero)
                Marshal.Copy(extension.Value.pbData, rawData, 0, rawData.Length);
            m_rawData = rawData;
        }

        protected X509Extension() : base () {}

        public X509Extension (string oid, byte[] rawData, bool critical) : this (new Oid(oid, OidGroup.ExtensionOrAttribute, true), rawData, critical) {}

        public X509Extension (AsnEncodedData encodedExtension, bool critical) : this (encodedExtension.Oid, encodedExtension.RawData, critical) {}

        public X509Extension (Oid oid, byte[] rawData, bool critical) : base (oid, rawData) {
            if (base.Oid == null || base.Oid.Value == null)
                throw new ArgumentNullException("oid");
            if (base.Oid.Value.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Arg_EmptyOrNullString), "oid.Value");
            m_critical = critical;
        }

        public bool Critical {
            get {
                return m_critical;
            }
            set {
                m_critical = value;
            }
        }

        public override void CopyFrom (AsnEncodedData asnEncodedData) {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            X509Extension extension = asnEncodedData as X509Extension;
            if (extension == null)
                throw new ArgumentException(SR.GetString(SR.Cryptography_X509_ExtensionMismatch));
            base.CopyFrom(asnEncodedData);
            m_critical = extension.Critical;
        }
    }

    //
    // Key Usage flags map the definition in wincrypt.h, so that no mapping will be necessary.
    //

    [Flags]
    public enum X509KeyUsageFlags {
        None             = 0x0000,
        EncipherOnly     = 0x0001,
        CrlSign          = 0x0002,
        KeyCertSign      = 0x0004,
        KeyAgreement     = 0x0008,
        DataEncipherment = 0x0010,
        KeyEncipherment  = 0x0020,
        NonRepudiation   = 0x0040,
        DigitalSignature = 0x0080,
        DecipherOnly     = 0x8000
    }

    public sealed class X509KeyUsageExtension : X509Extension {
        private uint m_keyUsages = 0;
        private bool m_decoded = false;

        public X509KeyUsageExtension() : base (CAPI.szOID_KEY_USAGE) {
            m_decoded = true;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509KeyUsageExtension (X509KeyUsageFlags keyUsages, bool critical) :
            base (CAPI.szOID_KEY_USAGE, EncodeExtension(keyUsages), critical) {}

        public X509KeyUsageExtension (AsnEncodedData encodedKeyUsage, bool critical) :
            base (CAPI.szOID_KEY_USAGE, encodedKeyUsage.RawData, critical) {}

        public X509KeyUsageFlags KeyUsages {
            get {
                if (!m_decoded)
                    DecodeExtension();
                return (X509KeyUsageFlags) m_keyUsages;
            }
        }

        public override void CopyFrom (AsnEncodedData asnEncodedData) {
            base.CopyFrom(asnEncodedData);
            m_decoded = false;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private void DecodeExtension () {
            uint cbDecoded = 0;
            SafeLocalAllocHandle decoded = null;

            bool result = CAPI.DecodeObject(new IntPtr(CAPI.X509_KEY_USAGE), 
                                            m_rawData,
                                            out decoded,
                                            out cbDecoded);
            if (result == false) 
                throw new CryptographicException(Marshal.GetLastWin32Error());

            CAPI.CRYPTOAPI_BLOB pKeyUsage = (CAPI.CRYPTOAPI_BLOB) Marshal.PtrToStructure(decoded.DangerousGetHandle(), typeof(CAPI.CRYPTOAPI_BLOB));
            if (pKeyUsage.cbData > 4)
                pKeyUsage.cbData = 4;
            byte[] keyUsage = new byte[4];
            if (pKeyUsage.pbData != IntPtr.Zero)
                Marshal.Copy(pKeyUsage.pbData, keyUsage, 0, (int) pKeyUsage.cbData);
            m_keyUsages = BitConverter.ToUInt32(keyUsage, 0);
            m_decoded = true;

            decoded.Dispose();
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe byte[] EncodeExtension (X509KeyUsageFlags keyUsages) {
            CAPI.CRYPT_BIT_BLOB blob = new CAPI.CRYPT_BIT_BLOB();
            blob.cbData = 2;
            blob.pbData = new IntPtr(&keyUsages);
            blob.cUnusedBits = 0;

            byte[] encodedKeyUsages = null;
            if (!CAPI.EncodeObject(CAPI.szOID_KEY_USAGE, new IntPtr(&blob), out encodedKeyUsages))
                throw new CryptographicException(Marshal.GetLastWin32Error());

            return encodedKeyUsages;
        }
    }

    public sealed class X509BasicConstraintsExtension : X509Extension {
        private bool m_isCA = false;
        private bool m_hasPathLenConstraint = false;
        private int m_pathLenConstraint = 0;
        private bool m_decoded = false;

        public X509BasicConstraintsExtension() : base (CAPI.szOID_BASIC_CONSTRAINTS2) {
            m_decoded = true;
        }

        public X509BasicConstraintsExtension (bool certificateAuthority, bool hasPathLengthConstraint, int pathLengthConstraint, bool critical) :
            base (CAPI.szOID_BASIC_CONSTRAINTS2, EncodeExtension(certificateAuthority, hasPathLengthConstraint, pathLengthConstraint), critical) {}

        public X509BasicConstraintsExtension (AsnEncodedData encodedBasicConstraints, bool critical) :
            base (CAPI.szOID_BASIC_CONSTRAINTS2, encodedBasicConstraints.RawData, critical) {}

        public bool CertificateAuthority {
            get {
                if (!m_decoded)
                    DecodeExtension();
                return m_isCA;
            }
        }

        public bool HasPathLengthConstraint {
            get {
                if (!m_decoded)
                    DecodeExtension();
                return m_hasPathLenConstraint;
            }
        }

        public int PathLengthConstraint {
            get {
                if (!m_decoded)
                    DecodeExtension();
                return m_pathLenConstraint;
            }
        }

        public override void CopyFrom (AsnEncodedData asnEncodedData) {
            base.CopyFrom(asnEncodedData);
            m_decoded = false;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private void DecodeExtension () {
            uint cbDecoded = 0;
            SafeLocalAllocHandle decoded = null;

            if (Oid.Value == CAPI.szOID_BASIC_CONSTRAINTS) {
                bool result = CAPI.DecodeObject(new IntPtr(CAPI.X509_BASIC_CONSTRAINTS), 
                                                m_rawData,
                                                out decoded,
                                                out cbDecoded);
                if (result == false) 
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                CAPI.CERT_BASIC_CONSTRAINTS_INFO pBasicConstraints = (CAPI.CERT_BASIC_CONSTRAINTS_INFO) Marshal.PtrToStructure(decoded.DangerousGetHandle(), 
                                                                                typeof(CAPI.CERT_BASIC_CONSTRAINTS_INFO));

                // take the first byte.
                byte[] isCA = new byte[1];
                Marshal.Copy(pBasicConstraints.SubjectType.pbData, isCA, 0, 1);

                m_isCA = (isCA[0] & CAPI.CERT_CA_SUBJECT_FLAG) != 0 ? true : false;
                m_hasPathLenConstraint = pBasicConstraints.fPathLenConstraint;
                m_pathLenConstraint = (int) pBasicConstraints.dwPathLenConstraint;
            } else {
                bool result = CAPI.DecodeObject(new IntPtr(CAPI.X509_BASIC_CONSTRAINTS2), 
                                                m_rawData,
                                                out decoded,
                                                out cbDecoded);
                if (result == false) 
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                CAPI.CERT_BASIC_CONSTRAINTS2_INFO pBasicConstraints2 = (CAPI.CERT_BASIC_CONSTRAINTS2_INFO) Marshal.PtrToStructure(decoded.DangerousGetHandle(), 
                                                                                typeof(CAPI.CERT_BASIC_CONSTRAINTS2_INFO));

                m_isCA = pBasicConstraints2.fCA == 0 ? false : true;
                m_hasPathLenConstraint = pBasicConstraints2.fPathLenConstraint == 0 ? false : true;
                m_pathLenConstraint = (int) pBasicConstraints2.dwPathLenConstraint;
            }

            m_decoded = true;
            decoded.Dispose();
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe byte[] EncodeExtension (bool certificateAuthority, bool hasPathLengthConstraint, int pathLengthConstraint) {
            CAPI.CERT_BASIC_CONSTRAINTS2_INFO pBasicConstraints2 = new CAPI.CERT_BASIC_CONSTRAINTS2_INFO();
            pBasicConstraints2.fCA = certificateAuthority ? 1 : 0;
            pBasicConstraints2.fPathLenConstraint = hasPathLengthConstraint ? 1 : 0;
            if (hasPathLengthConstraint) {
                if (pathLengthConstraint < 0)
                    throw new ArgumentOutOfRangeException("pathLengthConstraint", SR.GetString(SR.Arg_OutOfRange_NeedNonNegNum));
                pBasicConstraints2.dwPathLenConstraint = (uint) pathLengthConstraint;
            }

            byte[] encodedBasicConstraints = null;
            if (!CAPI.EncodeObject(CAPI.szOID_BASIC_CONSTRAINTS2, new IntPtr(&pBasicConstraints2), out encodedBasicConstraints))
                throw new CryptographicException(Marshal.GetLastWin32Error());

            return encodedBasicConstraints;
        }
    }

    public sealed class X509EnhancedKeyUsageExtension : X509Extension {
        private OidCollection m_enhancedKeyUsages;
        private bool m_decoded = false;

        public X509EnhancedKeyUsageExtension() : base (CAPI.szOID_ENHANCED_KEY_USAGE) {
            m_enhancedKeyUsages = new OidCollection();
            m_decoded = true;
        }

        public X509EnhancedKeyUsageExtension(OidCollection enhancedKeyUsages, bool critical) :
            base (CAPI.szOID_ENHANCED_KEY_USAGE, EncodeExtension(enhancedKeyUsages), critical) {}

        public X509EnhancedKeyUsageExtension(AsnEncodedData encodedEnhancedKeyUsages, bool critical) :
            base (CAPI.szOID_ENHANCED_KEY_USAGE, encodedEnhancedKeyUsages.RawData, critical) {}

        public OidCollection EnhancedKeyUsages {
#if FEATURE_CORESYSTEM
            [SecuritySafeCritical]
#endif
            get {
                if (!m_decoded)
                    DecodeExtension();
                OidCollection oids = new OidCollection();
                foreach(Oid oid in m_enhancedKeyUsages) {
                    oids.Add(oid);
                }
                return oids;
            }
        }

        public override void CopyFrom (AsnEncodedData asnEncodedData) {
            base.CopyFrom(asnEncodedData);
            m_decoded = false;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private void DecodeExtension () {
            uint cbDecoded = 0;
            SafeLocalAllocHandle decoded = null;

            bool result = CAPI.DecodeObject(new IntPtr(CAPI.X509_ENHANCED_KEY_USAGE),
                                            m_rawData,
                                            out decoded,
                                            out cbDecoded);
            if (result == false) 
                throw new CryptographicException(Marshal.GetLastWin32Error());

            CAPI.CERT_ENHKEY_USAGE pEnhKeyUsage = (CAPI.CERT_ENHKEY_USAGE) Marshal.PtrToStructure(decoded.DangerousGetHandle(), typeof(CAPI.CERT_ENHKEY_USAGE));

            m_enhancedKeyUsages = new OidCollection();
            for (int index = 0; index < pEnhKeyUsage.cUsageIdentifier; index++) {
                IntPtr pszOid = Marshal.ReadIntPtr(new IntPtr((long) pEnhKeyUsage.rgpszUsageIdentifier + index * Marshal.SizeOf(typeof(IntPtr))));
                string oidValue = Marshal.PtrToStringAnsi(pszOid);
                Oid oid = new Oid(oidValue, OidGroup.ExtensionOrAttribute, false);
                m_enhancedKeyUsages.Add(oid);
            }

            m_decoded = true;
            decoded.Dispose();
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe byte[] EncodeExtension (OidCollection enhancedKeyUsages) {
            if (enhancedKeyUsages == null)
                throw new ArgumentNullException("enhancedKeyUsages");

            SafeLocalAllocHandle safeLocalAllocHandle = X509Utils.CopyOidsToUnmanagedMemory(enhancedKeyUsages);
            byte[] encodedEnhancedKeyUsages = null;
            using (safeLocalAllocHandle) {
                CAPI.CERT_ENHKEY_USAGE pEnhKeyUsage = new CAPI.CERT_ENHKEY_USAGE();
                pEnhKeyUsage.cUsageIdentifier = (uint) enhancedKeyUsages.Count;
                pEnhKeyUsage.rgpszUsageIdentifier = safeLocalAllocHandle.DangerousGetHandle();
                if (!CAPI.EncodeObject(CAPI.szOID_ENHANCED_KEY_USAGE, new IntPtr(&pEnhKeyUsage), out encodedEnhancedKeyUsages))
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            return encodedEnhancedKeyUsages;
        }
    }

    public enum X509SubjectKeyIdentifierHashAlgorithm {
        Sha1        = 0,
        ShortSha1   = 1,
        CapiSha1    = 2,
    }

    public sealed class X509SubjectKeyIdentifierExtension : X509Extension {
        private string m_subjectKeyIdentifier;
        private bool m_decoded = false;

        public X509SubjectKeyIdentifierExtension() : base (CAPI.szOID_SUBJECT_KEY_IDENTIFIER) {
            m_subjectKeyIdentifier = null;
            m_decoded = true;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509SubjectKeyIdentifierExtension (string subjectKeyIdentifier, bool critical) :
            base (CAPI.szOID_SUBJECT_KEY_IDENTIFIER, EncodeExtension(subjectKeyIdentifier), critical) {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509SubjectKeyIdentifierExtension (byte[] subjectKeyIdentifier, bool critical) : 
            base (CAPI.szOID_SUBJECT_KEY_IDENTIFIER, EncodeExtension(subjectKeyIdentifier), critical) {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509SubjectKeyIdentifierExtension (AsnEncodedData encodedSubjectKeyIdentifier, bool critical) :
            base (CAPI.szOID_SUBJECT_KEY_IDENTIFIER, encodedSubjectKeyIdentifier.RawData, critical) {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509SubjectKeyIdentifierExtension (PublicKey key, bool critical) :
            base (CAPI.szOID_SUBJECT_KEY_IDENTIFIER, EncodePublicKey(key, X509SubjectKeyIdentifierHashAlgorithm.Sha1), critical) {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        public X509SubjectKeyIdentifierExtension (PublicKey key, X509SubjectKeyIdentifierHashAlgorithm algorithm, bool critical) :
            base (CAPI.szOID_SUBJECT_KEY_IDENTIFIER, EncodePublicKey(key, algorithm), critical) {}

        public string SubjectKeyIdentifier {
#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
            get {
                if (!m_decoded)
                    DecodeExtension();
                return m_subjectKeyIdentifier;
            }
        }

        public override void CopyFrom (AsnEncodedData asnEncodedData) {
            base.CopyFrom(asnEncodedData);
            m_decoded = false;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private void DecodeExtension () {
            uint cbDecoded = 0;
            SafeLocalAllocHandle decoded = null;

            SafeLocalAllocHandle pb = X509Utils.StringToAnsiPtr(CAPI.szOID_SUBJECT_KEY_IDENTIFIER);
            bool result = CAPI.DecodeObject(pb.DangerousGetHandle(), 
                                            m_rawData,
                                            out decoded,
                                            out cbDecoded);
            if (!result) 
                throw new CryptographicException(Marshal.GetLastWin32Error());

            CAPI.CRYPTOAPI_BLOB pSubjectKeyIdentifier = (CAPI.CRYPTOAPI_BLOB) Marshal.PtrToStructure(decoded.DangerousGetHandle(), typeof(CAPI.CRYPTOAPI_BLOB));
            byte[] hexArray = CAPI.BlobToByteArray(pSubjectKeyIdentifier);
            m_subjectKeyIdentifier = X509Utils.EncodeHexString(hexArray);

            m_decoded = true;
            decoded.Dispose();
            pb.Dispose();
        }

        private static unsafe byte[] EncodeExtension (string subjectKeyIdentifier) {
            if (subjectKeyIdentifier == null)
                throw new ArgumentNullException("subjectKeyIdentifier");

            return EncodeExtension(X509Utils.DecodeHexString(subjectKeyIdentifier));
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe byte[] EncodeExtension (byte[] subjectKeyIdentifier) {
            if (subjectKeyIdentifier == null)
                throw new ArgumentNullException("subjectKeyIdentifier");
            if (subjectKeyIdentifier.Length == 0)
                throw new ArgumentException("subjectKeyIdentifier");

            byte[] encodedSubjectKeyIdentifier = null;
            fixed (byte* pb = subjectKeyIdentifier) {
                CAPI.CRYPTOAPI_BLOB pSubjectKeyIdentifier = new CAPI.CRYPTOAPI_BLOB();
                pSubjectKeyIdentifier.pbData = new IntPtr(pb);
                pSubjectKeyIdentifier.cbData = (uint) subjectKeyIdentifier.Length;

                if (!CAPI.EncodeObject(CAPI.szOID_SUBJECT_KEY_IDENTIFIER, new IntPtr(&pSubjectKeyIdentifier), out encodedSubjectKeyIdentifier))
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            return encodedSubjectKeyIdentifier;
        }

        // Construct CERT_PUBLIC_KEY_INFO2 in unmanged memory from given encoded blobs.
#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe SafeLocalAllocHandle EncodePublicKey (PublicKey key) {
            SafeLocalAllocHandle publicKeyInfo = SafeLocalAllocHandle.InvalidHandle;
            CAPI.CERT_PUBLIC_KEY_INFO2 * pPublicKeyInfo = null;
            string objId = key.Oid.Value;
            byte[] encodedParameters = key.EncodedParameters.RawData;
            byte[] encodedKeyValue = key.EncodedKeyValue.RawData;

            uint cbPublicKeyInfo = (uint) (Marshal.SizeOf(typeof(CAPI.CERT_PUBLIC_KEY_INFO2)) + 
                                                          X509Utils.AlignedLength((uint) (objId.Length + 1)) + 
                                                          X509Utils.AlignedLength((uint) encodedParameters.Length) +
                                                          encodedKeyValue.Length);

            publicKeyInfo = CAPI.LocalAlloc(CAPI.LPTR, new IntPtr(cbPublicKeyInfo));
            pPublicKeyInfo = (CAPI.CERT_PUBLIC_KEY_INFO2 *) publicKeyInfo.DangerousGetHandle();
            IntPtr pszObjId =  new IntPtr((long) pPublicKeyInfo + Marshal.SizeOf(typeof(CAPI.CERT_PUBLIC_KEY_INFO2)));
            IntPtr pbParameters = new IntPtr((long) pszObjId + X509Utils.AlignedLength(((uint) (objId.Length + 1))));
            IntPtr pbPublicKey = new IntPtr((long) pbParameters + X509Utils.AlignedLength((uint) encodedParameters.Length));

            pPublicKeyInfo->Algorithm.pszObjId = pszObjId;
            byte[] szObjId = new byte[objId.Length + 1];
            Encoding.ASCII.GetBytes(objId, 0, objId.Length, szObjId, 0);
            Marshal.Copy(szObjId, 0, pszObjId, szObjId.Length);
            if (encodedParameters.Length > 0) {
                pPublicKeyInfo->Algorithm.Parameters.cbData = (uint) encodedParameters.Length;
                pPublicKeyInfo->Algorithm.Parameters.pbData = pbParameters;
                Marshal.Copy(encodedParameters, 0, pbParameters, encodedParameters.Length);
            }
            pPublicKeyInfo->PublicKey.cbData = (uint) encodedKeyValue.Length;
            pPublicKeyInfo->PublicKey.pbData = pbPublicKey;
            Marshal.Copy(encodedKeyValue, 0, pbPublicKey, encodedKeyValue.Length);
            return publicKeyInfo;
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        private static unsafe byte[] EncodePublicKey (PublicKey key, X509SubjectKeyIdentifierHashAlgorithm algorithm) {
            if (key == null)
                throw new ArgumentNullException("key");

            // Construct CERT_PUBLIC_KEY_INFO2 in unmanged memory from given encoded blobs.
            SafeLocalAllocHandle publicKeyInfo = EncodePublicKey(key);
            CAPI.CERT_PUBLIC_KEY_INFO2 * pPublicKeyInfo = (CAPI.CERT_PUBLIC_KEY_INFO2 *) publicKeyInfo.DangerousGetHandle();

            byte [] buffer = new byte[20];
            byte [] identifier = null;

            fixed (byte * pBuffer = buffer) {
                uint cbData = (uint)buffer.Length;
                IntPtr pbData = new IntPtr(pBuffer);

                try {
                    if ((X509SubjectKeyIdentifierHashAlgorithm.Sha1 == algorithm) 
                        || (X509SubjectKeyIdentifierHashAlgorithm.ShortSha1 == algorithm)) {
                    //+=================================================================
                    // (1) The keyIdentifier is composed of the 160-bit SHA-1 hash of 
                    // the value of the BIT STRING subjectPublicKey (excluding the tag,
                    // length, and number of unused bits).
                        if (!CAPI.CryptHashCertificate(
                                    IntPtr.Zero,        // hCryptProv
                                    CAPI.CALG_SHA1,
                                    0,                  // dwFlags,
                                    pPublicKeyInfo->PublicKey.pbData,
                                    pPublicKeyInfo->PublicKey.cbData,
                                    pbData,
                                    new IntPtr(&cbData)))
                            throw new CryptographicException(Marshal.GetHRForLastWin32Error());
                    }
                    //+=================================================================
                    // Microsoft convention: The keyIdentifier is composed of the 
                    // 160-bit SHA-1 hash of the encoded subjectPublicKey BITSTRING 
                    // (including the tag, length, and number of unused bits).
                    else if (X509SubjectKeyIdentifierHashAlgorithm.CapiSha1 == algorithm) {
                        if (!CAPI.CryptHashPublicKeyInfo(
                                    IntPtr.Zero,        // hCryptProv
                                    CAPI.CALG_SHA1,
                                    0,                  // dwFlags,
                                    CAPI.X509_ASN_ENCODING,
                                    new IntPtr(pPublicKeyInfo),
                                    pbData,
                                    new IntPtr(&cbData))) {
                            throw new CryptographicException(Marshal.GetHRForLastWin32Error());
                        }
                    } else {
                        throw new ArgumentException("algorithm");
                    }

                    //+=================================================================
                    // (2) The keyIdentifier is composed of a four bit type field with
                    //  the value 0100 followed by the least significant 60 bits of the
                    //  SHA-1 hash of the value of the BIT STRING subjectPublicKey 
                    // (excluding the tag, length, and number of unused bit string bits)
                    if (X509SubjectKeyIdentifierHashAlgorithm.ShortSha1 == algorithm) {
                        identifier = new byte[8];
                        Array.Copy(buffer, buffer.Length - 8, identifier, 0, identifier.Length);
                        identifier[0] &= 0x0f;
                        identifier[0] |= 0x40;
                    } else {
                        identifier = buffer;
                        // return the meaningful part only
                        if (buffer.Length > (int)cbData) {
                            identifier = new byte[cbData];
                            Array.Copy(buffer, 0, identifier, 0, identifier.Length);
                        }
                    }
                } finally {
                    publicKeyInfo.Dispose();
                }
            }

            return EncodeExtension(identifier);
        }
    }

    public sealed class X509ExtensionCollection : ICollection {
        private ArrayList m_list = new ArrayList();

        //
        // Constructors.
        //

        public X509ExtensionCollection() {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        internal unsafe X509ExtensionCollection(SafeCertContextHandle safeCertContextHandle) {
            using (SafeCertContextHandle certContext = CAPI.CertDuplicateCertificateContext(safeCertContextHandle)) {
                CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) certContext.DangerousGetHandle());
                CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));
                uint cExtensions = pCertInfo.cExtension;
                IntPtr rgExtensions = pCertInfo.rgExtension;

                for (uint index = 0; index < cExtensions; index++) {
                    X509Extension extension = new X509Extension(new IntPtr((long)rgExtensions + (index * Marshal.SizeOf(typeof(CAPI.CERT_EXTENSION)))));
                    X509Extension customExtension = CryptoConfig.CreateFromName(extension.Oid.Value) as X509Extension;
                    if (customExtension != null) {
                        customExtension.CopyFrom(extension);
                        extension = customExtension;
                    }
                    Add(extension);
                }
            }
        }

        public X509Extension this[int index] {
            get {
                if (index < 0)
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumNotStarted));
                if (index >= m_list.Count)
                    throw new ArgumentOutOfRangeException("index", SR.GetString(SR.ArgumentOutOfRange_Index));

                return (X509Extension) m_list[index];
            }
        }

        // Indexer using an OID friendly name or value.
        public X509Extension this[string oid] {
            get {
                // If we were passed the friendly name, retrieve the value string.
                string oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, oid, OidGroup.ExtensionOrAttribute);
                if (oidValue == null)
                    oidValue = oid;

                foreach (X509Extension extension in m_list) {
                    if (String.Compare(extension.Oid.Value, oidValue, StringComparison.OrdinalIgnoreCase) == 0)
                        return extension;
                }
                return null;
            }
        }

        public int Count {
            get {
                return m_list.Count;
            }
        }

        public int Add (X509Extension extension) {
            if (extension == null)
                throw new ArgumentNullException("extension");
            return m_list.Add(extension);
        }

        public X509ExtensionEnumerator GetEnumerator() {
            return new X509ExtensionEnumerator(this);
        }

        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return new X509ExtensionEnumerator(this);
        }

        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Rank != 1)
                throw new ArgumentException(SR.GetString(SR.Arg_RankMultiDimNotSupported));
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException("index", SR.GetString(SR.ArgumentOutOfRange_Index));
            if (index + this.Count > array.Length)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));

            for (int i=0; i < this.Count; i++) {
                array.SetValue(this[i], index);
                index++;
            }
        }

        public void CopyTo(X509Extension[] array, int index) {
            ((ICollection)this).CopyTo(array, index);
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public Object SyncRoot {
            get {
                return this;
            }
        }
    }

    public sealed class X509ExtensionEnumerator : IEnumerator {
        private X509ExtensionCollection m_extensions;
        private int m_current;

        private X509ExtensionEnumerator() {}
        internal X509ExtensionEnumerator(X509ExtensionCollection extensions) {
            m_extensions = extensions;
            m_current = -1;
        }

        public X509Extension Current {
            get {
                return m_extensions[m_current];
            }
        }

        /// <internalonly/>
        Object IEnumerator.Current {
            get {
                return (Object) m_extensions[m_current];
            }
        }

        public bool MoveNext() {
            if (m_current == ((int) m_extensions.Count - 1))
                return false;
            m_current++;
            return true;
        }

        public void Reset() {
            m_current = -1;
        }
    }
}
