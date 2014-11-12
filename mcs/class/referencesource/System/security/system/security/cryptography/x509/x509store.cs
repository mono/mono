// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509Store.cs
//

namespace System.Security.Cryptography.X509Certificates {
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    public enum StoreLocation {
        CurrentUser     = 0x01,
        LocalMachine    = 0x02
    }

    [Flags]
    // this enum defines the Open modes. Read/ReadWrite/MaxAllowed are mutually exclusive.
    public enum OpenFlags {
        ReadOnly         = 0x00,
        ReadWrite        = 0x01,
        MaxAllowed       = 0x02,
        OpenExistingOnly = 0x04,
        IncludeArchived  = 0x08
    }

    public enum StoreName {
        AddressBook = 1,        // other people.
        AuthRoot,               // third party trusted roots.
        CertificateAuthority,   // intermediate CAs.
        Disallowed,             // revoked certificates.
        My,                     // personal certificates.
        Root,                   // trusted root CAs.
        TrustedPeople,          // trusted people (used in EFS).
        TrustedPublisher,       // trusted publishers (used in Authenticode).
    }

    public sealed class X509Store {
        private string m_storeName;
        private StoreLocation m_location;
        private SafeCertStoreHandle m_safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;

        public X509Store () : this("MY", StoreLocation.CurrentUser) {}

        public X509Store (string storeName) : this (storeName, StoreLocation.CurrentUser) {}

        public X509Store (StoreName storeName) : this(storeName, StoreLocation.CurrentUser) {}

        public X509Store (StoreLocation storeLocation) : this ("MY", storeLocation) {}

        public X509Store (StoreName storeName, StoreLocation storeLocation) {
            if (storeLocation != StoreLocation.CurrentUser && storeLocation != StoreLocation.LocalMachine)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "storeLocation"));

            switch (storeName) {
            case StoreName.AddressBook:
                m_storeName = "AddressBook";
                break;
            case StoreName.AuthRoot:
                m_storeName = "AuthRoot";
                break;
            case StoreName.CertificateAuthority:
                m_storeName = "CA";
                break;
            case StoreName.Disallowed:
                m_storeName = "Disallowed";
                break;
            case StoreName.My:
                m_storeName = "My";
                break;
            case StoreName.Root:
                m_storeName = "Root";
                break;
            case StoreName.TrustedPeople:
                m_storeName = "TrustedPeople";
                break;
            case StoreName.TrustedPublisher:
                m_storeName = "TrustedPublisher";
                break;
            default:
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "storeName"));
            }

            m_location = storeLocation;
        }

        public X509Store (string storeName, StoreLocation storeLocation) {
            if (storeLocation != StoreLocation.CurrentUser && storeLocation != StoreLocation.LocalMachine)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "storeLocation"));

            m_storeName = storeName;
            m_location = storeLocation;
        }

        // Package protected constructor for creating a chain from a HCERTSTORE
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public X509Store (IntPtr storeHandle) {
            if (storeHandle == IntPtr.Zero)
                throw new ArgumentNullException("storeHandle");
            m_safeCertStoreHandle = CAPI.CertDuplicateStore(storeHandle);
            if (m_safeCertStoreHandle == null || m_safeCertStoreHandle.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidStoreHandle), "storeHandle");
        }

        public IntPtr StoreHandle {
            [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get {
                if (m_safeCertStoreHandle == null || m_safeCertStoreHandle.IsInvalid || m_safeCertStoreHandle.IsClosed)
                    throw new CryptographicException(SR.GetString(SR.Cryptography_X509_StoreNotOpen));

                return m_safeCertStoreHandle.DangerousGetHandle();
            }
        }

        public StoreLocation Location { 
            get { return m_location; } 
        }

        public string Name {
            get { return m_storeName; }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void Open(OpenFlags flags) {
            if (m_location != StoreLocation.CurrentUser && m_location != StoreLocation.LocalMachine)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "m_location"));

            uint storeFlags = X509Utils.MapX509StoreFlags(m_location, flags);
            if (!m_safeCertStoreHandle.IsInvalid)
                // Free the current store handle
                m_safeCertStoreHandle.Dispose();

            m_safeCertStoreHandle = CAPI.CertOpenStore(new IntPtr(CAPI.CERT_STORE_PROV_SYSTEM), 
                                                       CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                       IntPtr.Zero,
                                                       storeFlags, 
                                                       m_storeName);

            if (m_safeCertStoreHandle == null || m_safeCertStoreHandle.IsInvalid)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            //
            // We want the store to auto-resync when requesting a snapshot so that
            // updates to the store will be taken into account.
            //

            CAPI.CertControlStore(m_safeCertStoreHandle,
                                  0,
                                  CAPI.CERT_STORE_CTRL_AUTO_RESYNC,
                                  IntPtr.Zero);
        }

        public void Close() {
            if (m_safeCertStoreHandle != null && !m_safeCertStoreHandle.IsClosed)
                m_safeCertStoreHandle.Dispose();
        }

        public void Add(X509Certificate2 certificate) {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            if (m_safeCertStoreHandle == null || m_safeCertStoreHandle.IsInvalid || m_safeCertStoreHandle.IsClosed)
                throw new CryptographicException(SR.GetString(SR.Cryptography_X509_StoreNotOpen));

            if (!CAPI.CertAddCertificateContextToStore(m_safeCertStoreHandle,
                                                       certificate.CertContext,
                                                       CAPI.CERT_STORE_ADD_REPLACE_EXISTING_INHERIT_PROPERTIES,
                                                       SafeCertContextHandle.InvalidHandle))
                throw new CryptographicException(Marshal.GetLastWin32Error());
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

        public void Remove(X509Certificate2 certificate) {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            RemoveCertificateFromStore(m_safeCertStoreHandle, certificate.CertContext);
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

        public X509Certificate2Collection Certificates { 
            get {
                if (m_safeCertStoreHandle.IsInvalid || m_safeCertStoreHandle.IsClosed)
                    return new X509Certificate2Collection();
                return X509Utils.GetCertificates(m_safeCertStoreHandle);
            }
        }

        // 
        // private static methods
        //

        private static void RemoveCertificateFromStore(SafeCertStoreHandle safeCertStoreHandle, SafeCertContextHandle safeCertContext) {
            if (safeCertContext == null || safeCertContext.IsInvalid)
                return;

            if (safeCertStoreHandle == null || safeCertStoreHandle.IsInvalid || safeCertStoreHandle.IsClosed)
                throw new CryptographicException(SR.GetString(SR.Cryptography_X509_StoreNotOpen));

            // Find the certificate in the store.
            SafeCertContextHandle safeCertContext2 = CAPI.CertFindCertificateInStore(safeCertStoreHandle, 
                                                                                     CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                                                     0, 
                                                                                     CAPI.CERT_FIND_EXISTING, 
                                                                                     safeCertContext.DangerousGetHandle(),
                                                                                     SafeCertContextHandle.InvalidHandle);

            // The certificate is not present in the store, simply return.
            if (safeCertContext2 == null || safeCertContext2.IsInvalid)
                return;

            // CertDeleteCertificateFromStore always releases the context regardless of success 
            // or failure so we don't need to manually release it
            GC.SuppressFinalize(safeCertContext2);

            // Remove from the store.
            if (!CAPI.CertDeleteCertificateFromStore(safeCertContext2))
                throw new CryptographicException(Marshal.GetLastWin32Error());
        }
    }
}
