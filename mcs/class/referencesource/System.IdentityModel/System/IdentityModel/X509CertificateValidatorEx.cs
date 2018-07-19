//-----------------------------------------------------------------------
// <copyright file="X509CertificateValidatorEx.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;

    /// <summary>
    /// This class wraps the four WCF validator types (Peer, Chain, PeerOrChain, and None).
    /// This class also resets the validation time each time a certificate is validated, to fix a .NET issue
    /// where certificates created after the validator is created will not chain.
    /// </summary>
    internal class X509CertificateValidatorEx : X509CertificateValidator
    {
        private X509CertificateValidationMode certificateValidationMode;
        private X509ChainPolicy chainPolicy;
        private X509CertificateValidator validator;

        public X509CertificateValidatorEx(
            X509CertificateValidationMode certificateValidationMode,
            X509RevocationMode revocationMode,
            StoreLocation trustedStoreLocation)
        {
            this.certificateValidationMode = certificateValidationMode;

            switch (this.certificateValidationMode)
            {
                case X509CertificateValidationMode.None:
                    {
                        this.validator = X509CertificateValidator.None;
                        break;
                    }

                case X509CertificateValidationMode.PeerTrust:
                    {
                        this.validator = X509CertificateValidator.PeerTrust;
                        break;
                    }

                case X509CertificateValidationMode.ChainTrust:
                    {
                        bool useMachineContext = trustedStoreLocation == StoreLocation.LocalMachine;
                        this.chainPolicy = new X509ChainPolicy();
                        this.chainPolicy.RevocationMode = revocationMode;

                        this.validator = X509CertificateValidator.CreateChainTrustValidator(useMachineContext, this.chainPolicy);
                        break;
                    }

                case X509CertificateValidationMode.PeerOrChainTrust:
                    {
                        bool useMachineContext = trustedStoreLocation == StoreLocation.LocalMachine;
                        this.chainPolicy = new X509ChainPolicy();
                        this.chainPolicy.RevocationMode = revocationMode;

                        this.validator = X509CertificateValidator.CreatePeerOrChainTrustValidator(useMachineContext, this.chainPolicy);
                        break;
                    }

                case X509CertificateValidationMode.Custom:
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4256)));
            }
        }

        public override void Validate(X509Certificate2 certificate)
        {
            if (this.certificateValidationMode == X509CertificateValidationMode.ChainTrust ||
                 this.certificateValidationMode == X509CertificateValidationMode.PeerOrChainTrust)
            {
                // This is needed due to a .NET issue where the validation time is not properly set, 
                // causing certificates created after the creation of the validator to fail chain trust.
                this.chainPolicy.VerificationTime = DateTime.Now;
            }

            this.validator.Validate(certificate);
        }
    }
}
