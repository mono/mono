//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.Security.Cryptography.X509Certificates;

    public class X509ClientCertificateAuthentication
    {
        internal const X509CertificateValidationMode DefaultCertificateValidationMode = X509CertificateValidationMode.ChainTrust;
        internal const X509RevocationMode DefaultRevocationMode = X509RevocationMode.Online;
        internal const StoreLocation DefaultTrustedStoreLocation = StoreLocation.LocalMachine;
        internal const bool DefaultMapCertificateToWindowsAccount = false;
        static X509CertificateValidator defaultCertificateValidator;

        X509CertificateValidationMode certificateValidationMode = DefaultCertificateValidationMode;
        X509RevocationMode revocationMode = DefaultRevocationMode;
        StoreLocation trustedStoreLocation = DefaultTrustedStoreLocation;
        X509CertificateValidator customCertificateValidator = null;
        bool mapClientCertificateToWindowsAccount = DefaultMapCertificateToWindowsAccount;
        bool includeWindowsGroups = SspiSecurityTokenProvider.DefaultExtractWindowsGroupClaims;
        bool isReadOnly;

        internal X509ClientCertificateAuthentication()
        {
        }

        internal X509ClientCertificateAuthentication(X509ClientCertificateAuthentication other)
        {
            this.certificateValidationMode = other.certificateValidationMode;
            this.customCertificateValidator = other.customCertificateValidator;
            this.includeWindowsGroups = other.includeWindowsGroups;
            this.mapClientCertificateToWindowsAccount = other.mapClientCertificateToWindowsAccount;
            this.trustedStoreLocation = other.trustedStoreLocation;
            this.revocationMode = other.revocationMode;
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
                    defaultCertificateValidator = X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);
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

        public bool MapClientCertificateToWindowsAccount
        {
            get
            {
                return this.mapClientCertificateToWindowsAccount;
            }
            set
            {
                ThrowIfImmutable();
                this.mapClientCertificateToWindowsAccount = value;
            }
        }

        public bool IncludeWindowsGroups
        {
            get
            {
                return this.includeWindowsGroups;
            }
            set
            {
                ThrowIfImmutable();
                this.includeWindowsGroups = value;
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
