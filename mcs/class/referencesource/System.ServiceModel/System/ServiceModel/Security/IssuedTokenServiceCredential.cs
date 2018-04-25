//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.Security.Cryptography.X509Certificates;

    public class IssuedTokenServiceCredential
    {
        internal const bool DefaultAllowUntrustedRsaIssuers = false;
        internal const AudienceUriMode DefaultAudienceUriMode = AudienceUriMode.Always;
        internal const X509CertificateValidationMode DefaultCertificateValidationMode = X509CertificateValidationMode.ChainTrust;
        internal const X509RevocationMode DefaultRevocationMode = X509RevocationMode.Online;
        internal const StoreLocation DefaultTrustedStoreLocation = StoreLocation.LocalMachine;

        List<string> allowedAudienceUris;
        AudienceUriMode audienceUriMode = DefaultAudienceUriMode;
        List<X509Certificate2> knownCertificates;
        SamlSerializer samlSerializer;
        X509CertificateValidationMode certificateValidationMode = DefaultCertificateValidationMode;
        X509RevocationMode revocationMode = DefaultRevocationMode;
        StoreLocation trustedStoreLocation = DefaultTrustedStoreLocation;
        X509CertificateValidator customCertificateValidator = null;
        bool allowUntrustedRsaIssuers = DefaultAllowUntrustedRsaIssuers;
        bool isReadOnly;

        internal IssuedTokenServiceCredential()
        {
            this.allowedAudienceUris = new List<string>();
            this.knownCertificates = new List<X509Certificate2>();
        }

        internal IssuedTokenServiceCredential(IssuedTokenServiceCredential other)
        {
            this.audienceUriMode = other.audienceUriMode;
            this.allowedAudienceUris = new List<string>(other.allowedAudienceUris);
            this.samlSerializer = other.samlSerializer;
            this.knownCertificates = new List<X509Certificate2>(other.knownCertificates);
            this.certificateValidationMode = other.certificateValidationMode;
            this.customCertificateValidator = other.customCertificateValidator;
            this.trustedStoreLocation = other.trustedStoreLocation;
            this.revocationMode = other.revocationMode;
            this.allowUntrustedRsaIssuers = other.allowUntrustedRsaIssuers;
            this.isReadOnly = other.isReadOnly;
        }

        public IList<string> AllowedAudienceUris
        {
            get
            {
                if (this.isReadOnly)
                    return this.allowedAudienceUris.AsReadOnly();
                else
                    return this.allowedAudienceUris;
            }
        }

        public AudienceUriMode AudienceUriMode
        {
            get
            {
                return this.audienceUriMode;
            }
            set
            {
                ThrowIfImmutable();
                AudienceUriModeValidationHelper.Validate(audienceUriMode);
                this.audienceUriMode = value;
            }
        }


        public IList<X509Certificate2> KnownCertificates 
        { 
            get 
            {
                if (this.isReadOnly)
                    return this.knownCertificates.AsReadOnly();
                else
                    return this.knownCertificates; 
            }
        }
        
        public SamlSerializer SamlSerializer
        { 
            get 
            { 
                return this.samlSerializer; 
            } 
            set 
            {
                ThrowIfImmutable();
                this.samlSerializer = value;
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

        public bool AllowUntrustedRsaIssuers
        {
            get
            {
                return this.allowUntrustedRsaIssuers;
            }
            set
            {
                ThrowIfImmutable();
                this.allowUntrustedRsaIssuers = value;
            }
        }

        internal X509CertificateValidator GetCertificateValidator()
        {
            if (this.certificateValidationMode == X509CertificateValidationMode.None)
            {
                return X509CertificateValidator.None;
            }
            else if (this.certificateValidationMode == X509CertificateValidationMode.PeerTrust)
            {
                return X509CertificateValidator.PeerTrust;
            }
            else if (this.certificateValidationMode == X509CertificateValidationMode.Custom)
            {
                if (this.customCertificateValidator == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MissingCustomCertificateValidator)));
                }
                return this.customCertificateValidator;
            }
            else
            {
                bool useMachineContext = this.trustedStoreLocation == StoreLocation.LocalMachine;
                X509ChainPolicy chainPolicy = new X509ChainPolicy();
                chainPolicy.RevocationMode = this.revocationMode;
                if (this.certificateValidationMode == X509CertificateValidationMode.ChainTrust)
                {
                    return X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);
                }
                else
                {
                    return X509CertificateValidator.CreatePeerOrChainTrustValidator(useMachineContext, chainPolicy);
                }
            }
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
