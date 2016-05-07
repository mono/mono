//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml;
    // This is to allow easy rollback to CLR implementation by commenting out the below.
    using X509Chain = System.IdentityModel.Selectors.X509CertificateChain;

    public abstract class X509CertificateValidator : ICustomIdentityConfiguration
    {
        static X509CertificateValidator peerTrust;
        static X509CertificateValidator chainTrust;
        static X509CertificateValidator ntAuthChainTrust;
        static X509CertificateValidator peerOrChainTrust;
        static X509CertificateValidator none;

        public static X509CertificateValidator None
        {
            get
            {
                if (none == null)
                    none = new NoneX509CertificateValidator();
                return none;
            }
        }

        public static X509CertificateValidator PeerTrust
        {
            get
            {
                if (peerTrust == null)
                    peerTrust = new PeerTrustValidator();
                return peerTrust;
            }
        }

        public static X509CertificateValidator ChainTrust
        {
            get
            {
                if (chainTrust == null)
                    chainTrust = new ChainTrustValidator();
                return chainTrust;
            }
        }

        internal static X509CertificateValidator NTAuthChainTrust
        {
            get
            {
                if (ntAuthChainTrust == null)
                    ntAuthChainTrust = new ChainTrustValidator(false, null, CAPI.CERT_CHAIN_POLICY_NT_AUTH);
                return ntAuthChainTrust;
            }
        }

        public static X509CertificateValidator PeerOrChainTrust
        {
            get
            {
                if (peerOrChainTrust == null)
                    peerOrChainTrust = new PeerOrChainTrustValidator();
                return peerOrChainTrust;
            }
        }

        public static X509CertificateValidator CreateChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
        {
            if (chainPolicy == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("chainPolicy");
            return new ChainTrustValidator(useMachineContext, chainPolicy, X509CertificateChain.DefaultChainPolicyOID);
        }

        public static X509CertificateValidator CreatePeerOrChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
        {
            if (chainPolicy == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("chainPolicy");
            return new PeerOrChainTrustValidator(useMachineContext, chainPolicy);
        }

        public abstract void Validate(X509Certificate2 certificate);

        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="nodelist">Custom configuration elements</param>
        public virtual void LoadCustomConfiguration(XmlNodeList nodelist)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID0023, this.GetType().AssemblyQualifiedName)));
        }

        class NoneX509CertificateValidator : X509CertificateValidator
        {
            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
        }

        class PeerTrustValidator : X509CertificateValidator
        {
            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

                Exception exception;
                if (!TryValidate(certificate, out exception))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }

            static bool StoreContainsCertificate(StoreName storeName, X509Certificate2 certificate)
            {
                X509CertificateStore store = new X509CertificateStore(storeName, StoreLocation.CurrentUser);
                X509Certificate2Collection certificates = null;
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    certificates = store.Find(X509FindType.FindByThumbprint, certificate.GetCertHash(), false);
                    return certificates.Count > 0;
                }
                finally
                {
                    SecurityUtils.ResetAllCertificates(certificates);
                    store.Close();
                }
            }

            internal bool TryValidate(X509Certificate2 certificate, out Exception exception)
            {
                // Checklist
                // 1) time validity of cert
                // 2) in trusted people store
                // 3) not in disallowed store

                // The following code could be written as:
                // DateTime now = DateTime.UtcNow;
                // if (now > certificate.NotAfter.ToUniversalTime() || now < certificate.NotBefore.ToUniversalTime())
                //
                // this is because X509Certificate2.xxx doesn't return UT.  However this would be a SMALL perf hit.
                // I put a DebugAssert so that this will ensure that the we are compatible with the CLR we shipped with

                DateTime now = DateTime.Now;
                DiagnosticUtility.DebugAssert(now.Kind == certificate.NotAfter.Kind && now.Kind == certificate.NotBefore.Kind, "");

                if (now > certificate.NotAfter || now < certificate.NotBefore)
                {
                    exception = new SecurityTokenValidationException(SR.GetString(SR.X509InvalidUsageTime,
                        SecurityUtils.GetCertificateId(certificate), now, certificate.NotBefore, certificate.NotAfter));
                    return false;
                }

                if (!StoreContainsCertificate(StoreName.TrustedPeople, certificate))
                {
                    exception = new SecurityTokenValidationException(SR.GetString(SR.X509IsNotInTrustedStore,
                        SecurityUtils.GetCertificateId(certificate)));
                    return false;
                }

                if (StoreContainsCertificate(StoreName.Disallowed, certificate))
                {
                    exception = new SecurityTokenValidationException(SR.GetString(SR.X509IsInUntrustedStore,
                        SecurityUtils.GetCertificateId(certificate)));
                    return false;
                }
                exception = null;
                return true;
            }
        }

        class ChainTrustValidator : X509CertificateValidator
        {
            bool useMachineContext;
            X509ChainPolicy chainPolicy;
            uint chainPolicyOID = X509CertificateChain.DefaultChainPolicyOID;

            public ChainTrustValidator()
            {
                this.chainPolicy = null;
            }

            public ChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy, uint chainPolicyOID)
            {
                this.useMachineContext = useMachineContext;
                this.chainPolicy = chainPolicy;
                this.chainPolicyOID = chainPolicyOID;
            }

            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

                X509Chain chain = new X509Chain(this.useMachineContext, this.chainPolicyOID);
                if (this.chainPolicy != null)
                {
                    chain.ChainPolicy = this.chainPolicy;
                }

                if (!chain.Build(certificate))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.X509ChainBuildFail,
                        SecurityUtils.GetCertificateId(certificate), GetChainStatusInformation(chain.ChainStatus))));
                }
            }

            static string GetChainStatusInformation(X509ChainStatus[] chainStatus)
            {
                if (chainStatus != null)
                {
                    StringBuilder error = new StringBuilder(128);
                    for (int i = 0; i < chainStatus.Length; ++i)
                    {
                        error.Append(chainStatus[i].StatusInformation);
                        error.Append(" ");
                    }
                    return error.ToString();
                }
                return String.Empty;
            }
        }

        class PeerOrChainTrustValidator : X509CertificateValidator
        {
            X509CertificateValidator chain;
            PeerTrustValidator peer;

            public PeerOrChainTrustValidator()
            {
                this.chain = X509CertificateValidator.ChainTrust;
                this.peer = (PeerTrustValidator)X509CertificateValidator.PeerTrust;
            }

            public PeerOrChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
            {
                this.chain = X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);
                this.peer = (PeerTrustValidator)X509CertificateValidator.PeerTrust;
            }

            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

                Exception exception;
                if (this.peer.TryValidate(certificate, out exception))
                    return;

                try
                {
                    this.chain.Validate(certificate);
                }
                catch (SecurityTokenValidationException ex)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(exception.Message + " " + ex.Message));
                }
            }
        }
    }
}
