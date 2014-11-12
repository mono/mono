// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509Certificate2Collection.cs
//

namespace System.Security.Cryptography.X509Certificates {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Runtime.Versioning;

    using _FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    public enum X509FindType {
        FindByThumbprint                = 0,
        FindBySubjectName               = 1,
        FindBySubjectDistinguishedName  = 2,
        FindByIssuerName                = 3,
        FindByIssuerDistinguishedName   = 4,
        FindBySerialNumber              = 5,
        FindByTimeValid                 = 6,
        FindByTimeNotYetValid           = 7,
        FindByTimeExpired               = 8,
        FindByTemplateName              = 9,
        FindByApplicationPolicy         = 10,
        FindByCertificatePolicy         = 11,
        FindByExtension                 = 12,
        FindByKeyUsage                  = 13,
        FindBySubjectKeyIdentifier      = 14
    }

    public class X509Certificate2Collection : X509CertificateCollection {
        public X509Certificate2Collection() {}

        public X509Certificate2Collection(X509Certificate2 certificate) {
            this.Add(certificate);
        }

        public X509Certificate2Collection(X509Certificate2Collection certificates) {
            this.AddRange(certificates);
        }

        public X509Certificate2Collection(X509Certificate2[] certificates) {
            this.AddRange(certificates);
        }

        public new X509Certificate2 this[int index] {
            get {
                return (X509Certificate2) List[index];
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                List[index] = value;
            }
        }

        public int Add(X509Certificate2 certificate) {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            return List.Add(certificate);
        }

        public void AddRange(X509Certificate2[] certificates) {
            if (certificates == null)
                throw new ArgumentNullException("certificates");

            int i=0;
            try {
                for (; i<certificates.Length; i++) {
                    Add(certificates[i]);
                }
            } catch {
                for (int j=0; j<i; j++) {
                    Remove(certificates[j]);
                }
                throw;
            }
        }

        public void AddRange(X509Certificate2Collection certificates) {
            if (certificates == null)
                throw new ArgumentNullException("certificates");

            int i = 0;
            try {
                foreach (X509Certificate2 certificate in certificates) {
                    Add(certificate);
                    i++;
                }
            } catch {
                for (int j=0; j<i; j++) {
                    Remove(certificates[j]);
                }
                throw;
            }
        }

        public bool Contains(X509Certificate2 certificate) {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            return List.Contains(certificate);
        }

        public void Insert(int index, X509Certificate2 certificate) {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            List.Insert(index, certificate);
        }

        public new X509Certificate2Enumerator GetEnumerator() {
            return new X509Certificate2Enumerator(this);
        }

        public void Remove(X509Certificate2 certificate) {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            List.Remove(certificate);
        }

        public void RemoveRange(X509Certificate2[] certificates) {
            if (certificates == null)
                throw new ArgumentNullException("certificates");

            int i=0;
            try {
                for (; i<certificates.Length; i++) {
                    Remove(certificates[i]);
                }
            } catch {
                for (int j=0; j<i; j++) {
                    Add(certificates[j]);
                }
                throw;
            }
        }

        public void RemoveRange(X509Certificate2Collection certificates) {
            if (certificates == null)
                throw new ArgumentNullException("certificates");

            int i = 0;
            try {
                foreach (X509Certificate2 certificate in certificates) {
                    Remove(certificate);
                    i++;
                }
            } catch {
                for (int j=0; j<i; j++) {
                    Add(certificates[j]);
                }
                throw;
            }
        }

        public X509Certificate2Collection Find(X509FindType findType, Object findValue, bool validOnly) {
            //
            // We need to Assert all StorePermission flags since this is a memory store and we want 
            // semi-trusted code to be able to find certificates in a memory store.
            //

            StorePermission sp = new StorePermission(StorePermissionFlags.AllFlags);
            sp.Assert();

            SafeCertStoreHandle safeSourceStoreHandle = X509Utils.ExportToMemoryStore(this);

            SafeCertStoreHandle safeTargetStoreHandle = FindCertInStore(safeSourceStoreHandle, findType, findValue, validOnly);
            X509Certificate2Collection collection = X509Utils.GetCertificates(safeTargetStoreHandle);

            safeTargetStoreHandle.Dispose();
            safeSourceStoreHandle.Dispose();

            return collection;
        }

        public void Import(byte[] rawData) {
            Import(rawData, null, X509KeyStorageFlags.DefaultKeySet);
        }

        public void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags) {
            uint dwFlags = X509Utils.MapKeyStorageFlags(keyStorageFlags);
            SafeCertStoreHandle safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;

            //
            // We need to Assert all StorePermission flags since this is a memory store and we want 
            // semi-trusted code to be able to import certificates to a memory store.
            //

            StorePermission sp = new StorePermission(StorePermissionFlags.AllFlags);
            sp.Assert();

            safeCertStoreHandle = LoadStoreFromBlob(rawData, password, dwFlags, (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != 0);

            X509Certificate2Collection collection = X509Utils.GetCertificates(safeCertStoreHandle);

            safeCertStoreHandle.Dispose();
            X509Certificate2[] x509Certs = new X509Certificate2[collection.Count];
            collection.CopyTo(x509Certs, 0);
            this.AddRange(x509Certs);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void Import(string fileName) {
            Import(fileName, null, X509KeyStorageFlags.DefaultKeySet);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags) {
            uint dwFlags = X509Utils.MapKeyStorageFlags(keyStorageFlags);
            SafeCertStoreHandle safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;

            //
            // We need to Assert all StorePermission flags since this is a memory store and we want 
            // semi-trusted code to be able to import certificates to a memory store.
            //

            StorePermission sp = new StorePermission(StorePermissionFlags.AllFlags);
            sp.Assert();

            safeCertStoreHandle = LoadStoreFromFile(fileName, password, dwFlags, (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != 0);

            X509Certificate2Collection collection = X509Utils.GetCertificates(safeCertStoreHandle);

            safeCertStoreHandle.Dispose();
            X509Certificate2[] x509Certs = new X509Certificate2[collection.Count];
            collection.CopyTo(x509Certs, 0);
            this.AddRange(x509Certs);
        }

        public byte[] Export(X509ContentType contentType) {
            return Export(contentType, null);
        }

        public byte[] Export(X509ContentType contentType, string password) {
            //
            // We need to Assert all StorePermission flags since this is a memory store and we want 
            // semi-trusted code to be able to export certificates to a memory store.
            //

            StorePermission sp = new StorePermission(StorePermissionFlags.AllFlags);
            sp.Assert();

            SafeCertStoreHandle safeCertStoreHandle = X509Utils.ExportToMemoryStore(this);

            byte[] result = ExportCertificatesToBlob(safeCertStoreHandle, contentType, password);
            safeCertStoreHandle.Dispose();
            return result;
        }

        private unsafe static byte[] ExportCertificatesToBlob(SafeCertStoreHandle safeCertStoreHandle, X509ContentType contentType, string password) {
            SafeCertContextHandle safeCertContextHandle = SafeCertContextHandle.InvalidHandle;
            uint dwSaveAs = CAPI.CERT_STORE_SAVE_AS_PKCS7;
            byte[] pbBlob = null;
            CAPI.CRYPTOAPI_BLOB DataBlob = new CAPI.CRYPTOAPI_BLOB();
            SafeLocalAllocHandle pbEncoded = SafeLocalAllocHandle.InvalidHandle;

            switch(contentType) {
            case X509ContentType.Cert:
                safeCertContextHandle = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, safeCertContextHandle);
                if (safeCertContextHandle != null && !safeCertContextHandle.IsInvalid) {
                    CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
                    pbBlob = new byte[pCertContext.cbCertEncoded];
                    Marshal.Copy(pCertContext.pbCertEncoded, pbBlob, 0, pbBlob.Length);
                }
                break;

            case X509ContentType.SerializedCert:
                safeCertContextHandle = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, safeCertContextHandle);
                uint cbEncoded = 0;
                if (safeCertContextHandle != null && !safeCertContextHandle.IsInvalid) {
                    if (!CAPI.CertSerializeCertificateStoreElement(safeCertContextHandle, 
                                                                   0, 
                                                                   pbEncoded, 
                                                                   new IntPtr(&cbEncoded))) 
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    pbEncoded = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbEncoded));
                    if (!CAPI.CertSerializeCertificateStoreElement(safeCertContextHandle, 
                                                                   0, 
                                                                   pbEncoded, 
                                                                   new IntPtr(&cbEncoded)))
                        throw new CryptographicException(Marshal.GetLastWin32Error());

                    pbBlob = new byte[cbEncoded];
                    Marshal.Copy(pbEncoded.DangerousGetHandle(), pbBlob, 0, pbBlob.Length);
                }
                break;

            case X509ContentType.Pkcs12:
                if (!CAPI.PFXExportCertStore(safeCertStoreHandle, 
                                             new IntPtr(&DataBlob), 
                                             password, 
                                             CAPI.EXPORT_PRIVATE_KEYS | CAPI.REPORT_NOT_ABLE_TO_EXPORT_PRIVATE_KEY))
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                pbEncoded = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(DataBlob.cbData));
                DataBlob.pbData = pbEncoded.DangerousGetHandle();
                if (!CAPI.PFXExportCertStore(safeCertStoreHandle, 
                                             new IntPtr(&DataBlob),
                                             password, 
                                             CAPI.EXPORT_PRIVATE_KEYS | CAPI.REPORT_NOT_ABLE_TO_EXPORT_PRIVATE_KEY))
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                pbBlob = new byte[DataBlob.cbData];
                Marshal.Copy(DataBlob.pbData, pbBlob, 0, pbBlob.Length);
                break;

            case X509ContentType.SerializedStore:
                // falling through
            case X509ContentType.Pkcs7:
                if (contentType == X509ContentType.SerializedStore)
                    dwSaveAs = CAPI.CERT_STORE_SAVE_AS_STORE;

                // determine the required length
                if (!CAPI.CertSaveStore(safeCertStoreHandle, 
                                        CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING, 
                                        dwSaveAs, 
                                        CAPI.CERT_STORE_SAVE_TO_MEMORY, 
                                        new IntPtr(&DataBlob), 
                                        0)) 
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                pbEncoded = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(DataBlob.cbData));
                DataBlob.pbData = pbEncoded.DangerousGetHandle();
                // now save the store to a memory blob
                if (!CAPI.CertSaveStore(safeCertStoreHandle, 
                                        CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING, 
                                        dwSaveAs, 
                                        CAPI.CERT_STORE_SAVE_TO_MEMORY, 
                                        new IntPtr(&DataBlob), 
                                        0)) 
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                pbBlob = new byte[DataBlob.cbData];
                Marshal.Copy(DataBlob.pbData, pbBlob, 0, pbBlob.Length);
                break;

            default:
                throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidContentType));
            }

            pbEncoded.Dispose();
            safeCertContextHandle.Dispose();

            return pbBlob;
        }

        internal delegate int FindProcDelegate (SafeCertContextHandle safeCertContextHandle, object pvCallbackData);
        private unsafe static SafeCertStoreHandle FindCertInStore(SafeCertStoreHandle safeSourceStoreHandle, X509FindType findType, Object findValue, bool validOnly) {
            if (findValue == null)
                throw new ArgumentNullException("findValue");

            IntPtr pvFindPara = IntPtr.Zero;
            object pvCallbackData1 = null;
            object pvCallbackData2 = null;
            FindProcDelegate pfnCertCallback1 = null;
            FindProcDelegate pfnCertCallback2 = null;
            uint dwFindType = CAPI.CERT_FIND_ANY;
            string subject, issuer;

            CAPI.CRYPTOAPI_BLOB HashBlob = new CAPI.CRYPTOAPI_BLOB();
            SafeLocalAllocHandle pb = SafeLocalAllocHandle.InvalidHandle;
            _FILETIME ft = new _FILETIME();
            string oidValue = null;

            switch(findType) {
            case X509FindType.FindByThumbprint:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                byte[] hex = X509Utils.DecodeHexString((string) findValue);
                pb = X509Utils.ByteToPtr(hex);
                HashBlob.pbData = pb.DangerousGetHandle(); 
                HashBlob.cbData = (uint) hex.Length;
                dwFindType = CAPI.CERT_FIND_HASH;
                pvFindPara = new IntPtr(&HashBlob);
                break;

            case X509FindType.FindBySubjectName:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                subject = (string) findValue;
                dwFindType = CAPI.CERT_FIND_SUBJECT_STR;
                pb = X509Utils.StringToUniPtr(subject);
                pvFindPara = pb.DangerousGetHandle();
                break;

            case X509FindType.FindBySubjectDistinguishedName:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                subject = (string) findValue;
                pfnCertCallback1 = new FindProcDelegate(FindSubjectDistinguishedNameCallback);
                pvCallbackData1 = subject;
                break;

            case X509FindType.FindByIssuerName:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                issuer = (string) findValue;
                dwFindType = CAPI.CERT_FIND_ISSUER_STR;
                pb = X509Utils.StringToUniPtr(issuer);
                pvFindPara = pb.DangerousGetHandle();
                break;

            case X509FindType.FindByIssuerDistinguishedName:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                issuer = (string) findValue;
                pfnCertCallback1 = new FindProcDelegate(FindIssuerDistinguishedNameCallback);
                pvCallbackData1 = issuer;
                break;

            case X509FindType.FindBySerialNumber:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                pfnCertCallback1 = new FindProcDelegate(FindSerialNumberCallback);
                pfnCertCallback2 = new FindProcDelegate(FindSerialNumberCallback);
                BigInt h = new BigInt();
                h.FromHexadecimal((string) findValue);
                pvCallbackData1 = (byte[]) h.ToByteArray();
                h.FromDecimal((string) findValue);
                pvCallbackData2 = (byte[]) h.ToByteArray();
                break;

            case X509FindType.FindByTimeValid:
                if (findValue.GetType() != typeof(DateTime))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                *((long*) &ft) = ((DateTime) findValue).ToFileTime();
                pfnCertCallback1 = new FindProcDelegate(FindTimeValidCallback);
                pvCallbackData1 = ft; 
                break;

            case X509FindType.FindByTimeNotYetValid:
                if (findValue.GetType() != typeof(DateTime))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                *((long*) &ft) = ((DateTime) findValue).ToFileTime();
                pfnCertCallback1 = new FindProcDelegate(FindTimeNotBeforeCallback);
                pvCallbackData1 = ft; 
                break;

            case X509FindType.FindByTimeExpired:
                if (findValue.GetType() != typeof(DateTime))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                *((long*) &ft) = ((DateTime) findValue).ToFileTime();
                pfnCertCallback1 = new FindProcDelegate(FindTimeNotAfterCallback);
                pvCallbackData1 = ft; 
                break;

            case X509FindType.FindByTemplateName:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                pvCallbackData1 = (string) findValue; 
                pfnCertCallback1 = new FindProcDelegate(FindTemplateNameCallback);
                break;

            case X509FindType.FindByApplicationPolicy:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                // If we were passed the friendly name, retrieve the value string.
                oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, (string) findValue, OidGroup.Policy);
                if (oidValue == null) {
                    oidValue = (string) findValue;
                    X509Utils.ValidateOidValue(oidValue);
                }
                pvCallbackData1 = oidValue;
                pfnCertCallback1 = new FindProcDelegate(FindApplicationPolicyCallback);
                break;

            case X509FindType.FindByCertificatePolicy:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                // If we were passed the friendly name, retrieve the value string.
                oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, (string)findValue, OidGroup.Policy);
                if (oidValue == null) {
                    oidValue = (string) findValue;
                    X509Utils.ValidateOidValue(oidValue);
                }
                pvCallbackData1 = oidValue;
                pfnCertCallback1 = new FindProcDelegate(FindCertificatePolicyCallback);
                break;

            case X509FindType.FindByExtension:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                // If we were passed the friendly name, retrieve the value string.
                oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, (string)findValue, OidGroup.ExtensionOrAttribute);
                if (oidValue == null) {
                    oidValue = (string) findValue;
                    X509Utils.ValidateOidValue(oidValue);
                }
                pvCallbackData1 = oidValue;
                pfnCertCallback1 = new FindProcDelegate(FindExtensionCallback);
                break;

            case X509FindType.FindByKeyUsage:
                // The findValue object can be either a friendly name, a X509KeyUsageFlags enum or an integer.
                if (findValue.GetType() == typeof(string)) {
                    CAPI.KEY_USAGE_STRUCT[] KeyUsages = new CAPI.KEY_USAGE_STRUCT[] { 
                        new CAPI.KEY_USAGE_STRUCT("DigitalSignature", CAPI.CERT_DIGITAL_SIGNATURE_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("NonRepudiation",   CAPI.CERT_NON_REPUDIATION_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("KeyEncipherment",  CAPI.CERT_KEY_ENCIPHERMENT_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("DataEncipherment", CAPI.CERT_DATA_ENCIPHERMENT_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("KeyAgreement",     CAPI.CERT_KEY_AGREEMENT_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("KeyCertSign",      CAPI.CERT_KEY_CERT_SIGN_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("CrlSign",          CAPI.CERT_CRL_SIGN_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("EncipherOnly",     CAPI.CERT_ENCIPHER_ONLY_KEY_USAGE),
                        new CAPI.KEY_USAGE_STRUCT("DecipherOnly",     CAPI.CERT_DECIPHER_ONLY_KEY_USAGE)
                    };

                    for (uint index = 0; index < KeyUsages.Length; index++) {
                        if (String.Compare(KeyUsages[index].pwszKeyUsage, (string) findValue, StringComparison.OrdinalIgnoreCase) == 0) {
                            pvCallbackData1 = KeyUsages[index].dwKeyUsageBit;
                            break;
                        }
                    }
                    if (pvCallbackData1 == null)
                        throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindType));
                } else if (findValue.GetType() == typeof(X509KeyUsageFlags)) {
                    pvCallbackData1 = findValue;
                } else if (findValue.GetType() == typeof(uint) || findValue.GetType() == typeof(int)) {
                    // We got the actual DWORD
                    pvCallbackData1 = findValue;
                } else 
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindType));

                pfnCertCallback1 = new FindProcDelegate(FindKeyUsageCallback);
                break;

            case X509FindType.FindBySubjectKeyIdentifier:
                if (findValue.GetType() != typeof(string))
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindValue));
                pvCallbackData1 = (byte[]) X509Utils.DecodeHexString((string) findValue);
                pfnCertCallback1 = new FindProcDelegate(FindSubjectKeyIdentifierCallback);
                break;

            default:
                throw new CryptographicException(SR.GetString(SR.Cryptography_X509_InvalidFindType));
            }

            // First, create a memory store
            SafeCertStoreHandle safeTargetStoreHandle = CAPI.CertOpenStore(new IntPtr(CAPI.CERT_STORE_PROV_MEMORY), 
                                                                           CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING, 
                                                                           IntPtr.Zero, 
                                                                           CAPI.CERT_STORE_ENUM_ARCHIVED_FLAG | CAPI.CERT_STORE_CREATE_NEW_FLAG, 
                                                                           null);
            if (safeTargetStoreHandle == null || safeTargetStoreHandle.IsInvalid)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            // FindByCert will throw an exception in case of failures.
            FindByCert(safeSourceStoreHandle, 
                       dwFindType,
                       pvFindPara, 
                       validOnly, 
                       pfnCertCallback1,
                       pfnCertCallback2, 
                       pvCallbackData1,
                       pvCallbackData2, 
                       safeTargetStoreHandle);

            pb.Dispose();
            return safeTargetStoreHandle;
        }

        private static void FindByCert(SafeCertStoreHandle safeSourceStoreHandle, 
                                        uint dwFindType, 
                                        IntPtr pvFindPara, 
                                        bool validOnly, 
                                        FindProcDelegate pfnCertCallback1, 
                                        FindProcDelegate pfnCertCallback2, 
                                        object pvCallbackData1, 
                                        object pvCallbackData2, 
                                        SafeCertStoreHandle safeTargetStoreHandle) {

            int hr = CAPI.S_OK;

            SafeCertContextHandle pEnumContext = SafeCertContextHandle.InvalidHandle;
            pEnumContext = CAPI.CertFindCertificateInStore(safeSourceStoreHandle, 
                                                           CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                           0, 
                                                           dwFindType,
                                                           pvFindPara,
                                                           pEnumContext);

            while (pEnumContext != null && !pEnumContext.IsInvalid) {
                if (pfnCertCallback1 != null) {
                    hr = pfnCertCallback1(pEnumContext, pvCallbackData1);
                    if (hr == CAPI.S_FALSE) {
                        if (pfnCertCallback2 != null) 
                            hr = pfnCertCallback2(pEnumContext, pvCallbackData2);

                        if (hr == CAPI.S_FALSE) // skip this certificate
                            goto skip;
                    }

                    if (hr != CAPI.S_OK)
                        break;
                }

                if (validOnly) {
                    hr = X509Utils.VerifyCertificate(pEnumContext, 
                                           null,
                                           null,
                                           X509RevocationMode.NoCheck,
                                           X509RevocationFlag.ExcludeRoot,
                                           DateTime.Now,
                                           new TimeSpan(0, 0, 0), // default
                                           null,
                                           new IntPtr(CAPI.CERT_CHAIN_POLICY_BASE), 
                                           IntPtr.Zero);
                    if (hr == CAPI.S_FALSE) // skip this certificate
                        goto skip;

                    if (hr != CAPI.S_OK)
                        break;
                }

                //
                // We use CertAddCertificateLinkToStore to keep a link to the original store, so any property changes get
                // applied to the original store. This has a limit of 99 links per cert context however.
                //

                if (!CAPI.CertAddCertificateLinkToStore(safeTargetStoreHandle, 
                                                        pEnumContext, 
                                                        CAPI.CERT_STORE_ADD_ALWAYS, 
                                                        SafeCertContextHandle.InvalidHandle)) {
                    hr = Marshal.GetHRForLastWin32Error();
                    break;
                }

skip:
                // CertFindCertificateInStore always releases the context regardless of success 
                // or failure so we don't need to manually release it
                GC.SuppressFinalize(pEnumContext);

                pEnumContext = CAPI.CertFindCertificateInStore(safeSourceStoreHandle, 
                                                               CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                               0, 
                                                               dwFindType, 
                                                               pvFindPara,
                                                               pEnumContext);
            }

            if (pEnumContext != null && !pEnumContext.IsInvalid)
                pEnumContext.Dispose();

            if (hr != CAPI.S_FALSE && hr != CAPI.S_OK)
                throw new CryptographicException(hr);
        }

        //
        // Callback method to find certificates by subject DN.
        //

        private static unsafe int FindSubjectDistinguishedNameCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            string rdn = CAPI.GetCertNameInfo(safeCertContextHandle, 0, CAPI.CERT_NAME_RDN_TYPE);
            if (String.Compare(rdn, (string) pvCallbackData, StringComparison.OrdinalIgnoreCase) != 0)
                return CAPI.S_FALSE;
            return CAPI.S_OK;
        }

        //
        // Callback method to find certificates by issuer DN.
        //

        private static unsafe int FindIssuerDistinguishedNameCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            string rdn = CAPI.GetCertNameInfo(safeCertContextHandle, CAPI.CERT_NAME_ISSUER_FLAG, CAPI.CERT_NAME_RDN_TYPE);
            if (String.Compare(rdn, (string) pvCallbackData, StringComparison.OrdinalIgnoreCase) != 0)
                return CAPI.S_FALSE;
            return CAPI.S_OK;
        }

        //
        // Callback method to find certificates by serial number.
        // This can be useful when using XML Digital Signature and X509Data.
        //

        private static unsafe int FindSerialNumberCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));

            byte[] hex = new byte[pCertInfo.SerialNumber.cbData];
            Marshal.Copy(pCertInfo.SerialNumber.pbData, hex, 0, hex.Length);

            int size = X509Utils.GetHexArraySize(hex);
            byte[] serialNumber = (byte[]) pvCallbackData;
            if (serialNumber.Length != size)
                return CAPI.S_FALSE;

            for (int index = 0; index < serialNumber.Length; index++) {
                if (serialNumber[index] != hex[index])
                    return CAPI.S_FALSE;
            }

            return CAPI.S_OK;
        }

        //
        // Callback method to find certificates by validity time.
        // The callback data has to be a UTC FILETEME.
        //

        private static unsafe int FindTimeValidCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            _FILETIME ft = (_FILETIME) pvCallbackData;
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            if (CAPI.CertVerifyTimeValidity(ref ft, pCertContext.pCertInfo) == 0)
                return CAPI.S_OK;

            return CAPI.S_FALSE;
        }

        //
        // Callback method to find certificates expired at a certain DateTime.
        // The callback data has to be a UTC FILETEME.
        //

        private static unsafe int FindTimeNotAfterCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            _FILETIME ft = (_FILETIME) pvCallbackData;
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            if (CAPI.CertVerifyTimeValidity(ref ft, pCertContext.pCertInfo) == 1)
                return CAPI.S_OK;

            return CAPI.S_FALSE;
        }

        //
        // Callback method to find certificates effective after a certain DateTime.
        // The callback data has to be a UTC FILETEME.
        //

        private static unsafe int FindTimeNotBeforeCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            _FILETIME ft = (_FILETIME) pvCallbackData;
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            if (CAPI.CertVerifyTimeValidity(ref ft, pCertContext.pCertInfo) == -1)
                return CAPI.S_OK;

            return CAPI.S_FALSE;
        }

        //
        // Callback method to find certificates by template name.
        // The template name can have 2 different formats: V1 format (<= Win2K) is just a string
        // V2 format (XP only) can be a friendly name or an OID.
        // An example of Template Name can be "ClientAuth".
        //

        private static unsafe int FindTemplateNameCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            IntPtr pV1Template = IntPtr.Zero;
            IntPtr pV2Template = IntPtr.Zero;

            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));

            pV1Template = CAPI.CertFindExtension(CAPI.szOID_ENROLL_CERTTYPE_EXTENSION,
                                                 pCertInfo.cExtension,
                                                 pCertInfo.rgExtension);
            pV2Template = CAPI.CertFindExtension(CAPI.szOID_CERTIFICATE_TEMPLATE,
                                                 pCertInfo.cExtension,
                                                 pCertInfo.rgExtension);

            if (pV1Template == IntPtr.Zero && pV2Template == IntPtr.Zero)
                return CAPI.S_FALSE;

            if (pV1Template != IntPtr.Zero) {
                CAPI.CERT_EXTENSION extension = (CAPI.CERT_EXTENSION) Marshal.PtrToStructure(pV1Template, typeof(CAPI.CERT_EXTENSION));
                byte[] rawData = new byte[extension.Value.cbData];
                Marshal.Copy(extension.Value.pbData, rawData, 0, rawData.Length);

                uint cbDecoded = 0;
                SafeLocalAllocHandle decoded = null;
                // Decode the extension.
                bool result = CAPI.DecodeObject(new IntPtr(CAPI.X509_UNICODE_ANY_STRING), 
                                                rawData,
                                                out decoded,
                                                out cbDecoded);
                if (result) {
                    CAPI.CERT_NAME_VALUE pNameValue = (CAPI.CERT_NAME_VALUE) Marshal.PtrToStructure(decoded.DangerousGetHandle(), typeof(CAPI.CERT_NAME_VALUE));
                    string s = Marshal.PtrToStringUni(pNameValue.Value.pbData);
                    if (String.Compare(s, (string) pvCallbackData, StringComparison.OrdinalIgnoreCase) == 0)
                        return CAPI.S_OK;
                }
            }

            if (pV2Template != IntPtr.Zero) {
                CAPI.CERT_EXTENSION extension = (CAPI.CERT_EXTENSION) Marshal.PtrToStructure(pV2Template, typeof(CAPI.CERT_EXTENSION));
                byte[] rawData = new byte[extension.Value.cbData];
                Marshal.Copy(extension.Value.pbData, rawData, 0, rawData.Length);

                uint cbDecoded = 0;
                SafeLocalAllocHandle decoded = null;
                // Decode the extension.
                bool result = CAPI.DecodeObject(new IntPtr(CAPI.X509_CERTIFICATE_TEMPLATE), 
                                                rawData,
                                                out decoded,
                                                out cbDecoded);
                if (result) {
                    CAPI.CERT_TEMPLATE_EXT pTemplate = (CAPI.CERT_TEMPLATE_EXT) Marshal.PtrToStructure(decoded.DangerousGetHandle(), typeof(CAPI.CERT_TEMPLATE_EXT));
                    // If we were passed the friendly name, retrieve the value string.
                    string oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, (string)pvCallbackData, OidGroup.Template);
                    if (oidValue == null)
                        oidValue = (string) pvCallbackData;
                    if (String.Compare(pTemplate.pszObjId, oidValue, StringComparison.OrdinalIgnoreCase) == 0)
                        return CAPI.S_OK;
                }
            }

            return CAPI.S_FALSE;
        }

        //
        // Callback method to find certificates by application policy (also known as EKU)
        // An example of application policy can be: "Encrypting File System"
        //

        private static unsafe int FindApplicationPolicyCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            string eku = (string) pvCallbackData;
            if (eku.Length == 0)
                return CAPI.S_FALSE;
            IntPtr pCertContext = safeCertContextHandle.DangerousGetHandle();
            int cNumOIDs = 0;
            uint cbOIDs = 0;
            SafeLocalAllocHandle rghOIDs = SafeLocalAllocHandle.InvalidHandle;
            if (!CAPI.CertGetValidUsages(1, new IntPtr(&pCertContext), new IntPtr(&cNumOIDs), rghOIDs, new IntPtr(&cbOIDs))) 
                return CAPI.S_FALSE;

            rghOIDs = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbOIDs));
            if (!CAPI.CertGetValidUsages(1, new IntPtr(&pCertContext), new IntPtr(&cNumOIDs), rghOIDs, new IntPtr(&cbOIDs))) 
                return CAPI.S_FALSE;

            // -1 means the certificate is good for all usages.
            if (cNumOIDs == -1)
                return CAPI.S_OK;

            for (int index = 0; index < cNumOIDs; index++) {
                IntPtr pszOid = Marshal.ReadIntPtr(new IntPtr((long) rghOIDs.DangerousGetHandle() + index * Marshal.SizeOf(typeof(IntPtr))));
                string oidValue = Marshal.PtrToStringAnsi(pszOid);
                if (String.Compare(eku, oidValue, StringComparison.OrdinalIgnoreCase) == 0)
                    return CAPI.S_OK;
            }

            return CAPI.S_FALSE;
        }

        //
        // Callback method to find certificates by certificate policy.
        // This is only recognized in XP platforms. However, passing in an OID value should work on downlevel platforms as well.
        //

        private static unsafe int FindCertificatePolicyCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            string certPolicy = (string) pvCallbackData;
            if (certPolicy.Length == 0)
                return CAPI.S_FALSE;
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));

            IntPtr pExtension = CAPI.CertFindExtension(CAPI.szOID_CERT_POLICIES,
                                                       pCertInfo.cExtension,
                                                       pCertInfo.rgExtension);
            if (pExtension == IntPtr.Zero)
                return CAPI.S_FALSE;

            CAPI.CERT_EXTENSION extension = (CAPI.CERT_EXTENSION) Marshal.PtrToStructure(pExtension, typeof(CAPI.CERT_EXTENSION));
            byte[] rawData = new byte[extension.Value.cbData];
            Marshal.Copy(extension.Value.pbData, rawData, 0, rawData.Length);

            uint cbDecoded = 0;
            SafeLocalAllocHandle decoded = null;
            // Decode the extension.
            bool result = CAPI.DecodeObject(new IntPtr(CAPI.X509_CERT_POLICIES), 
                                            rawData,
                                            out decoded,
                                            out cbDecoded);
            if (result) {
                CAPI.CERT_POLICIES_INFO pInfo = (CAPI.CERT_POLICIES_INFO) Marshal.PtrToStructure(decoded.DangerousGetHandle(), typeof(CAPI.CERT_POLICIES_INFO));
                for (int index = 0; index < pInfo.cPolicyInfo; index++) {
                    IntPtr pPolicyInfoPtr = new IntPtr((long) pInfo.rgPolicyInfo + index * Marshal.SizeOf(typeof(CAPI.CERT_POLICY_INFO)));
                    CAPI.CERT_POLICY_INFO pPolicyInfo = (CAPI.CERT_POLICY_INFO) Marshal.PtrToStructure(pPolicyInfoPtr, typeof(CAPI.CERT_POLICY_INFO));
                    if (String.Compare(certPolicy, pPolicyInfo.pszPolicyIdentifier, StringComparison.OrdinalIgnoreCase) == 0)
                        return CAPI.S_OK;
                }
            }

            return CAPI.S_FALSE;
        }

        //
        // Callback method to find certificates that have a particular extension.
        // The callback data can be either an OID friendly name or value (all should be ANSI strings).
        //

        private static unsafe int FindExtensionCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPI.CERT_INFO pCertInfo = (CAPI.CERT_INFO) Marshal.PtrToStructure(pCertContext.pCertInfo, typeof(CAPI.CERT_INFO));

            IntPtr pExtension = CAPI.CertFindExtension((string) pvCallbackData,
                                                       pCertInfo.cExtension,
                                                       pCertInfo.rgExtension);
            if (pExtension == IntPtr.Zero)
                return CAPI.S_FALSE;

            return CAPI.S_OK;
        }

        //
        // Callback method to find certificates that have a particular Key Usage.
        // The callback data can be either a string (example: "KeyEncipherment") or a DWORD which can have multiple bits set in it.
        // If the callback data is a string, we can achieve the effect of a bit union by calling it multiple times, each time 
        // further restricting the set of selected certificates.
        //

        private static unsafe int FindKeyUsageCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            CAPI.CERT_CONTEXT pCertContext = *((CAPI.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            uint dwUsages = 0;
            if (!CAPI.CertGetIntendedKeyUsage(CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING, 
                                              pCertContext.pCertInfo, 
                                              new IntPtr(&dwUsages), 
                                              4 /* sizeof(DWORD) */)) 
                return CAPI.S_OK; // no key usage means it is valid for all key usages.

            uint dwCheckUsage = Convert.ToUInt32(pvCallbackData, null);
            if ((dwUsages & dwCheckUsage) == dwCheckUsage)
                return CAPI.S_OK;

            return CAPI.S_FALSE;
        }

        //
        // Callback method to find certificates by subject key identifier. 
        // This can be useful when using XML Digital Signature and X509Data.
        //

        private static unsafe int FindSubjectKeyIdentifierCallback(SafeCertContextHandle safeCertContextHandle, object pvCallbackData) {
            SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
            // We look for the Key Id extended property 
            // this will first look if there is a V3 SKI extension
            // and then if that fails, It will return the Key Id extended property.
            uint cbData = 0;
            if (!CAPI.CertGetCertificateContextProperty(safeCertContextHandle, 
                                                        CAPI.CERT_KEY_IDENTIFIER_PROP_ID, 
                                                        ptr, 
                                                        ref cbData))
                return CAPI.S_FALSE;

            ptr = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbData));
            if (!CAPI.CertGetCertificateContextProperty(safeCertContextHandle, 
                                                        CAPI.CERT_KEY_IDENTIFIER_PROP_ID, 
                                                        ptr, 
                                                        ref cbData))
                return CAPI.S_FALSE;

            byte[] subjectKeyIdentifier = (byte[]) pvCallbackData;
            if (subjectKeyIdentifier.Length != cbData)
                return CAPI.S_FALSE;

            byte[] hex = new byte[cbData];
            Marshal.Copy(ptr.DangerousGetHandle(), hex, 0, hex.Length);
            ptr.Dispose();

            for (uint index = 0; index < cbData; index++) {
                if (subjectKeyIdentifier[index] != hex[index])
                    return CAPI.S_FALSE;
            }

            return CAPI.S_OK;
        }

        private const uint X509_STORE_CONTENT_FLAGS                         =
                                       (CAPI.CERT_QUERY_CONTENT_FLAG_CERT | 
                                        CAPI.CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT | 
                                        CAPI.CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED | 
                                        CAPI.CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED | 
                                        CAPI.CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED | 
                                        CAPI.CERT_QUERY_CONTENT_FLAG_PFX |
                                        CAPI.CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE);

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private unsafe static SafeCertStoreHandle LoadStoreFromBlob(byte[] rawData, string password, uint dwFlags, bool persistKeyContainers) {
            uint contentType = 0;
            SafeCertStoreHandle safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;
            if (!CAPI.CryptQueryObject(CAPI.CERT_QUERY_OBJECT_BLOB,
                                       rawData,
                                       X509_STORE_CONTENT_FLAGS,
                                       CAPI.CERT_QUERY_FORMAT_FLAG_ALL,
                                       0,
                                       IntPtr.Zero,
                                       new IntPtr(&contentType),
                                       IntPtr.Zero,
                                       ref safeCertStoreHandle,
                                       IntPtr.Zero,
                                       IntPtr.Zero))
                throw new CryptographicException(Marshal.GetLastWin32Error());

            if (contentType == CAPI.CERT_QUERY_CONTENT_PFX) {
                safeCertStoreHandle.Dispose();
                safeCertStoreHandle = CAPI.PFXImportCertStore(CAPI.CERT_QUERY_OBJECT_BLOB,
                                                              rawData, 
                                                              password, 
                                                              dwFlags,
                                                              persistKeyContainers);
            }

            if (safeCertStoreHandle == null || safeCertStoreHandle.IsInvalid)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            return safeCertStoreHandle;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private unsafe static SafeCertStoreHandle LoadStoreFromFile(string fileName, string password, uint dwFlags, bool persistKeyContainers) {
            uint contentType = 0;
            SafeCertStoreHandle safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;
            if (!CAPI.CryptQueryObject(CAPI.CERT_QUERY_OBJECT_FILE,
                                       fileName,
                                       X509_STORE_CONTENT_FLAGS,
                                       CAPI.CERT_QUERY_FORMAT_FLAG_ALL,
                                       0,
                                       IntPtr.Zero,
                                       new IntPtr(&contentType),
                                       IntPtr.Zero,
                                       ref safeCertStoreHandle,
                                       IntPtr.Zero,
                                       IntPtr.Zero))
                throw new CryptographicException(Marshal.GetLastWin32Error());

            if (contentType == CAPI.CERT_QUERY_CONTENT_PFX) {
                safeCertStoreHandle.Dispose();
                safeCertStoreHandle = CAPI.PFXImportCertStore(CAPI.CERT_QUERY_OBJECT_FILE,
                                                              fileName, 
                                                              password, 
                                                              dwFlags,
                                                              persistKeyContainers);
            }

            if (safeCertStoreHandle == null || safeCertStoreHandle.IsInvalid)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            return safeCertStoreHandle;
        }
    }

    public sealed class X509Certificate2Enumerator : IEnumerator {
        private IEnumerator baseEnumerator;

        private X509Certificate2Enumerator() {}
        internal X509Certificate2Enumerator(X509Certificate2Collection mappings) {
            this.baseEnumerator = ((IEnumerable) mappings).GetEnumerator();
        }

        public X509Certificate2 Current {
            get {
                return ((X509Certificate2)(baseEnumerator.Current));
            }
        }

        /// <internalonly/>
        object IEnumerator.Current {
            get {
                return baseEnumerator.Current;
            }
        }

        public bool MoveNext() {
            return baseEnumerator.MoveNext();
        }

        /// <internalonly/>
        bool IEnumerator.MoveNext() {
            return baseEnumerator.MoveNext();
        }

        public void Reset() {
            baseEnumerator.Reset();
        }

        /// <internalonly/>
        void IEnumerator.Reset() {
            baseEnumerator.Reset();
        }
    }
}
