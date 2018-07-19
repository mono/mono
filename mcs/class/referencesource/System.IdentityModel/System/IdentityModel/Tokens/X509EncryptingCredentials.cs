//-----------------------------------------------------------------------
// <copyright file="X509EncryptingCredentials.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Use x509 token as the encrypting credential. This is usually used as key wrapping credentials.
    /// </summary>
    public class X509EncryptingCredentials : EncryptingCredentials
    {
        private X509Certificate2 certificate;

        /// <summary>
        /// Constructs an encrypting credential based on the x509 certificate.
        /// </summary>
        /// <param name="certificate">The x509 certificate.</param>
        public X509EncryptingCredentials(X509Certificate2 certificate)
            : this(new X509SecurityToken(certificate))
        {
        }

        /// <summary>
        /// Constructs an encrypting credential based on the x509 certificate and key wrapping algorithm.
        /// </summary>
        /// <param name="certificate">The x509 certificate.</param>
        /// <param name="keyWrappingAlgorithm">The key wrapping al----htm.</param>
        public X509EncryptingCredentials(X509Certificate2 certificate, string keyWrappingAlgorithm)
            : this(new X509SecurityToken(certificate), keyWrappingAlgorithm)
        {
        }

        /// <summary>
        /// Constructs an encrypting credential based on the x509 certificate and security key identifier.
        /// </summary>
        /// <param name="certificate">The x509 certificate.</param>
        /// /// <param name="ski">The security key identifier to be used.</param>
        public X509EncryptingCredentials(X509Certificate2 certificate, SecurityKeyIdentifier ski)
            : this(new X509SecurityToken(certificate), ski, SecurityAlgorithms.DefaultAsymmetricKeyWrapAlgorithm)
        {
        }

        /// <summary>
        /// Constructs an encrypting credential based on the x509 certificate, key wrapping algorithm, and security key identifier.
        /// </summary>
        /// <param name="certificate">The x509 certificate.</param>
        /// <param name="ski">The security key identifier to be used.</param>
        /// <param name="keyWrappingAlgorithm">The key wrapping al----htm.</param>
        public X509EncryptingCredentials(X509Certificate2 certificate, SecurityKeyIdentifier ski, string keyWrappingAlgorithm)
            : this(new X509SecurityToken(certificate), ski, keyWrappingAlgorithm)
        {
        }

        /// <summary>
        /// Constructs an encrypting credential based on the x509 token.
        /// </summary>
        /// <param name="token">The x509 security token.</param>
        internal X509EncryptingCredentials(X509SecurityToken token)
            : this(
            token,
            new SecurityKeyIdentifier(token.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause>()),
            SecurityAlgorithms.DefaultAsymmetricKeyWrapAlgorithm)
        {
        }

        /// <summary>
        /// Constructs an encrypting credential based on the x509 token and key wrapping algorithm.
        /// </summary>
        /// <param name="token">The x509 security token.</param>
        /// <param name="keyWrappingAlgorithm">The key wrapping al----htm.</param>
        internal X509EncryptingCredentials(X509SecurityToken token, string keyWrappingAlgorithm)
            : this(token, new SecurityKeyIdentifier(token.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause>()), keyWrappingAlgorithm)
        {
        }

        /// <summary>
        /// Constructs an encrypting credential based on the x509 token, key wrapping algorithm, and security key identifier.
        /// </summary>
        /// <param name="token">The x509 security token.</param>
        /// <param name="ski">The security key identifier to be used.</param>
        /// <param name="keyWrappingAlgorithm">The key wrapping al----htm.</param>
        internal X509EncryptingCredentials(X509SecurityToken token, SecurityKeyIdentifier ski, string keyWrappingAlgorithm)
            : base(token.SecurityKeys[0], ski, keyWrappingAlgorithm)
        {
            this.certificate = token.Certificate;
        }

        /// <summary>
        /// Gets the x509 certificate.
        /// </summary>
        public X509Certificate2 Certificate
        {
            get
            {
                return this.certificate;
            }
        }
    }
}
