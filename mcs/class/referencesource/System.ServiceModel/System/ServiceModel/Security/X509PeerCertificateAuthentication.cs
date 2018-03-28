//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public class X509PeerCertificateAuthentication
    {
        internal const X509CertificateValidationMode DefaultCertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
        internal const X509RevocationMode DefaultRevocationMode = X509RevocationMode.Online;
        internal const StoreLocation DefaultTrustedStoreLocation = StoreLocation.CurrentUser;
        static X509CertificateValidator defaultCertificateValidator;

        X509CertificateValidationMode certificateValidationMode = DefaultCertificateValidationMode;
        X509RevocationMode revocationMode = DefaultRevocationMode;
        StoreLocation trustedStoreLocation = DefaultTrustedStoreLocation;
        X509CertificateValidator customCertificateValidator = null;
        bool isReadOnly;

        internal X509PeerCertificateAuthentication()
        {
        }

        internal X509PeerCertificateAuthentication(X509PeerCertificateAuthentication other)
        {
            this.certificateValidationMode = other.certificateValidationMode;
            this.customCertificateValidator = other.customCertificateValidator;
            this.revocationMode = other.revocationMode;
            this.trustedStoreLocation = other.trustedStoreLocation;
            this.isReadOnly = other.isReadOnly;
        }

        internal static X509CertificateValidator DefaultCertificateValidator
        {
            get
            {
                if (defaultCertificateValidator == null)
                {
                    bool useMachineContext = DefaultTrustedStoreLocation == StoreLocation.LocalMachine;
                    X509ChainPolicy chainPolicy = new X509ChainPolicy();
                    chainPolicy.RevocationMode = DefaultRevocationMode;
                    defaultCertificateValidator = X509CertificateValidator.CreatePeerOrChainTrustValidator(useMachineContext, chainPolicy);
                }
                return defaultCertificateValidator;
            }
        }

        public X509CertificateValidationMode CertificateValidationMode
        {
            get
            {
                return this.certificateValidationMode;
            }
            set
            {
                X509CertificateValidationModeHelper.Validate(value);
                ThrowIfImmutable();
                this.certificateValidationMode = value;
            }
        }

        public X509RevocationMode RevocationMode
        {
            get
            {
                return this.revocationMode;
            }
            set
            {
                ThrowIfImmutable();
                this.revocationMode = value;
            }
        }

        public StoreLocation TrustedStoreLocation
        {
            get
            {
                return this.trustedStoreLocation;
            }
            set
            {
                ThrowIfImmutable();
                this.trustedStoreLocation = value;
            }
        }

        public X509CertificateValidator CustomCertificateValidator
        {
            get
            {
                return this.customCertificateValidator;
            }
            set
            {
                ThrowIfImmutable();
                this.customCertificateValidator = value;
            }
        }

        internal bool TryGetCertificateValidator(out X509CertificateValidator validator)
        {
            validator = null;
            if (this.certificateValidationMode == X509CertificateValidationMode.None)
            {
                validator = X509CertificateValidator.None;
            }
            else if (this.certificateValidationMode == X509CertificateValidationMode.PeerTrust)
            {
                validator = X509CertificateValidator.PeerTrust;
            }
            else if (this.certificateValidationMode == X509CertificateValidationMode.Custom)
            {
                validator = this.customCertificateValidator;
            }
            else
            {
                bool useMachineContext = this.trustedStoreLocation == StoreLocation.LocalMachine;
                X509ChainPolicy chainPolicy = new X509ChainPolicy();
                chainPolicy.RevocationMode = this.revocationMode;
                if (this.certificateValidationMode == X509CertificateValidationMode.ChainTrust)
                {
                    validator = X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);
                }
                else
                {
                    validator = X509CertificateValidator.CreatePeerOrChainTrustValidator(useMachineContext, chainPolicy);
                }
            }
            return (validator != null);
        }

        internal X509CertificateValidator GetCertificateValidator()
        {
            X509CertificateValidator result;
            if (!TryGetCertificateValidator(out result))
            {
                Fx.Assert(this.customCertificateValidator == null, "");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MissingCustomCertificateValidator)));
            }
            return result;
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
