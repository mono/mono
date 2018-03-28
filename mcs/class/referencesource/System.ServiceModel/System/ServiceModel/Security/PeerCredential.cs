//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public class PeerCredential
    {
        internal const StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;
        internal const StoreName DefaultStoreName = StoreName.My;
        internal const X509FindType DefaultFindType = X509FindType.FindBySubjectDistinguishedName;

        X509Certificate2 certificate;
        string meshPassword;
        X509PeerCertificateAuthentication peerAuthentication;
        X509PeerCertificateAuthentication messageSenderAuthentication;
        bool isReadOnly;

        internal PeerCredential()
        {
            peerAuthentication = new X509PeerCertificateAuthentication();
            messageSenderAuthentication = new X509PeerCertificateAuthentication();
        }

        internal PeerCredential(PeerCredential other)
        {
            this.certificate = other.certificate;
            this.meshPassword = other.meshPassword;
            this.peerAuthentication = new X509PeerCertificateAuthentication(other.peerAuthentication);
            this.messageSenderAuthentication = new X509PeerCertificateAuthentication(other.messageSenderAuthentication);
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

        public string MeshPassword
        {
            get
            {
                return this.meshPassword;
            }
            set
            {
                ThrowIfImmutable();
                this.meshPassword = value;
            }
        }

        public X509PeerCertificateAuthentication PeerAuthentication
        {
            get
            {
                return this.peerAuthentication;
            }
            set
            {
                ThrowIfImmutable();
                this.peerAuthentication = value;
            }
        }

        public X509PeerCertificateAuthentication MessageSenderAuthentication
        {
            get
            {
                return this.messageSenderAuthentication;
            }
            set
            {
                ThrowIfImmutable();
                this.messageSenderAuthentication = value;
            }
        }

        public void SetCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName)
        {
            if (subjectName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectName");
            }
            this.SetCertificate(storeLocation, storeName, DefaultFindType, subjectName);
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
            this.peerAuthentication.MakeReadOnly();
            this.messageSenderAuthentication.MakeReadOnly();
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }

        bool SameAuthenticators(X509PeerCertificateAuthentication one, X509PeerCertificateAuthentication two)
        {
            if (one.CertificateValidationMode != two.CertificateValidationMode)
                return false;
            if (one.CertificateValidationMode != X509CertificateValidationMode.Custom)
            {
                return (one.GetType().Equals(two.GetType()));
            }
            else
            {
                System.IdentityModel.Selectors.X509CertificateValidator first = null, second = null;
                one.TryGetCertificateValidator(out first);
                two.TryGetCertificateValidator(out second);
                return (first != null && second != null && first.Equals(second));
            }
        }

        internal bool Equals(PeerCredential that, PeerAuthenticationMode mode, bool messageAuthentication)
        {
            if (messageAuthentication)
            {
                if (!SameAuthenticators(this.MessageSenderAuthentication, that.messageSenderAuthentication))
                    return false;
                if (this.Certificate != null && that.Certificate != null && !this.Certificate.Equals(that.Certificate))
                    return false;
            }
            switch (mode)
            {
                case PeerAuthenticationMode.None:
                    return true;
                case PeerAuthenticationMode.Password:
                    if (!this.MeshPassword.Equals(that.MeshPassword))
                        return false;
                    if (this.Certificate == null && that.Certificate == null)
                        return true;
                    if ((this.Certificate == null) || !this.Certificate.Equals(that.Certificate))
                        return false;
                    break;
                case PeerAuthenticationMode.MutualCertificate:
                    if (!this.Certificate.Equals(that.Certificate))
                        return false;
                    if (!SameAuthenticators(this.PeerAuthentication, that.PeerAuthentication))
                        return false;
                    break;
            }
            return true;
        }
    }
}
