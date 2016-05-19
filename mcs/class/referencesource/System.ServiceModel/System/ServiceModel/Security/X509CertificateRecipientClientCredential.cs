//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public sealed class X509CertificateRecipientClientCredential
    {
        X509ServiceCertificateAuthentication authentication;
        X509ServiceCertificateAuthentication sslCertificateAuthentication;

        internal const StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;
        internal const StoreName DefaultStoreName = StoreName.My;
        internal const X509FindType DefaultFindType = X509FindType.FindBySubjectDistinguishedName;

        X509Certificate2 defaultCertificate;
        Dictionary<Uri, X509Certificate2> scopedCertificates;
        bool isReadOnly;

        internal X509CertificateRecipientClientCredential()
        {
            this.authentication = new X509ServiceCertificateAuthentication();
            this.scopedCertificates = new Dictionary<Uri, X509Certificate2>();
        }

        internal X509CertificateRecipientClientCredential(X509CertificateRecipientClientCredential other)
        {
            this.authentication = new X509ServiceCertificateAuthentication(other.authentication);
            if (other.sslCertificateAuthentication != null)
            {
                this.sslCertificateAuthentication = new X509ServiceCertificateAuthentication(other.sslCertificateAuthentication);
            }

            this.defaultCertificate = other.defaultCertificate;
            this.scopedCertificates = new Dictionary<Uri, X509Certificate2>();
            foreach (Uri uri in other.ScopedCertificates.Keys)
            {
                this.scopedCertificates.Add(uri, other.ScopedCertificates[uri]);
            }
            this.isReadOnly = other.isReadOnly;
        }

        public X509Certificate2 DefaultCertificate
        {
            get
            {
                
                return this.defaultCertificate;
            }
            set
            {
                ThrowIfImmutable();
                this.defaultCertificate = value;
            }
        }

        public Dictionary<Uri, X509Certificate2> ScopedCertificates
        {
            get
            {
                return this.scopedCertificates;
            }
        }

        public X509ServiceCertificateAuthentication Authentication
        {
            get
            {
                
                return this.authentication;
            }
        }

        public X509ServiceCertificateAuthentication SslCertificateAuthentication
        {
            get
            {
                return this.sslCertificateAuthentication;
            }
            set
            {
                ThrowIfImmutable();
                this.sslCertificateAuthentication = value;
            }
        }

        public void SetDefaultCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName)
        {
            if (subjectName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectName");
            }
            SetDefaultCertificate(storeLocation, storeName, DefaultFindType, subjectName);
        }

        public void SetDefaultCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            ThrowIfImmutable();
            this.defaultCertificate = SecurityUtils.GetCertificateFromStore(storeName, storeLocation, findType, findValue, null);
        }

        public void SetScopedCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName, Uri targetService)
        {
            if (subjectName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectName");
            }
            SetScopedCertificate(DefaultStoreLocation, DefaultStoreName, DefaultFindType, subjectName, targetService);
        }

        public void SetScopedCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue, Uri targetService)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            if (targetService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("targetService");
            }
            ThrowIfImmutable();
            X509Certificate2 certificate = SecurityUtils.GetCertificateFromStore(storeName, storeLocation, findType, findValue, null);
            ScopedCertificates[targetService] = certificate;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
            this.Authentication.MakeReadOnly();
            if (this.sslCertificateAuthentication != null)
            {
                this.sslCertificateAuthentication.MakeReadOnly();
            }
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }
    }
}
