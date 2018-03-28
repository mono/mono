//-----------------------------------------------------------------------
// <copyright file="X509NTAuthChainTrustValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    /// <summary>
    /// can be mapped to Windows account and if the Certificatez chain is trusted.
    /// </summary>
    public class X509NTAuthChainTrustValidator : X509CertificateValidator
    {
        private bool useMachineContext;
        private X509ChainPolicy chainPolicy;
        private uint chainPolicyOID = CAPI.CERT_CHAIN_POLICY_NT_AUTH;

        /// <summary>
        /// Creates an instance of <see cref="X509NTAuthChainTrustValidator"/>
        /// </summary>
        public X509NTAuthChainTrustValidator()
            : this(false, null)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="X509NTAuthChainTrustValidator"/>
        /// </summary>
        /// <param name="useMachineContext">True to use local machine context to build the cert chain.</param>
        /// <param name="chainPolicy">X509Chain policy to use.</param>
        public X509NTAuthChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
        {
            this.useMachineContext = useMachineContext;
            this.chainPolicy = chainPolicy;
        }

        /// <summary>
        /// Validates the given certificate.
        /// </summary>
        /// <param name="certificate">X.509 Certificate to validate.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'certificate' is null.</exception>
        /// <exception cref="SecurityTokenValidationException">X.509 Certificate validation failed.</exception>
        public override void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }

            X509CertificateChain chain = new X509CertificateChain(this.useMachineContext, (uint)this.chainPolicyOID);
            if (this.chainPolicy != null)
            {
                chain.ChainPolicy = this.chainPolicy;
            }

            if (!chain.Build(certificate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SecurityTokenValidationException(
                        SR.GetString(
                            SR.ID4070,
                            X509Util.GetCertificateId(certificate),
                            GetChainStatusInformation(chain.ChainStatus))));
            }
        }

        private static string GetChainStatusInformation(X509ChainStatus[] chainStatus)
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

            return string.Empty;
        }
    }
}
