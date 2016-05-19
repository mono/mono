//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public sealed class X509CertificateInitiatorClientCredential
    {
        internal const StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;
        internal const StoreName DefaultStoreName = StoreName.My;
        internal const X509FindType DefaultFindType = X509FindType.FindBySubjectDistinguishedName;

        X509Certificate2 certificate;
        bool isReadOnly;

        internal X509CertificateInitiatorClientCredential()
        {
            // empty
        }

        internal X509CertificateInitiatorClientCredential(X509CertificateInitiatorClientCredential other)
        {
            this.certificate = other.certificate;
            this.isReadOnly = other.isReadOnly;
        }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.certificate;
            }
            set
            {
                ThrowIfImmutable();
                this.certificate = value;
            }
        }

        public void SetCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName)
        {
            if (subjectName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectName");
            }
            SetCertificate(storeLocation, storeName, DefaultFindType, subjectName);
        }

        public void SetCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            ThrowIfImmutable();
            this.certificate = SecurityUtils.GetCertificateFromStore(storeName, storeLocation, findType, findValue, null);
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
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
