//-----------------------------------------------------------------------
// <copyright file="X509SigningCredentials.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Use x509 token as the encrypting credential
    /// </summary>
    public class X509SigningCredentials : SigningCredentials
    {
        private X509Certificate2 certificate;

        /// <summary>
        /// Constructor for X509SigningCredentials based on an x509 certificate.
        /// By default, it uses the X509RawDataKeyIdentifierClause to generate the security key identifier.
        /// </summary>
        /// <param name="certificate">x509 certificate</param>
        public X509SigningCredentials(X509Certificate2 certificate)
            : this(
                certificate,
                new SecurityKeyIdentifier((new X509SecurityToken(certificate)).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>()))
        {
        }

#if INCLUDE_CERT_CHAIN        
        /// <summary>
        /// Constructor for X509SigningCredentials based on an x509 certificate.
        /// By default, it uses the X509RawDataKeyIdentifierClause to generate the security key identifier.
        /// </summary>
        /// <param name="certificates">x509 certificate chain</param>
        public X509SigningCredentials( IEnumerable<X509Certificate2> certificates )
            : this ( GetLeafCertificate( certificates ),
                    new SecurityKeyIdentifier( new X509ChainRawDataKeyIdentifierClause( certificates ) ) )
        {
        }
#endif

        /// <summary>
        /// Constructor for X509SigningCredentials based on an x509 certificate.
        /// By default, it uses the X509RawDataKeyIdentifierClause to generate the security key identifier.
        /// </summary>
        /// <param name="certificate">x509 certificate</param>
        /// <param name="signatureAlgorithm">signature algorithm</param>
        /// <param name="digestAlgorithm">digest algorithm</param>
        public X509SigningCredentials(X509Certificate2 certificate, string signatureAlgorithm, string digestAlgorithm)
            : this(
                new X509SecurityToken(certificate),
                new SecurityKeyIdentifier((new X509SecurityToken(certificate)).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>()),
                signatureAlgorithm,
                digestAlgorithm)
        {
        }

#if INCLUDE_CERT_CHAIN
        /// <summary>
        /// Constructor for X509SigningCredentials based on an x509 certificate.
        /// By default, it uses the X509RawDataKeyIdentifierClause to generate the security key identifier.
        /// </summary>
        /// <param name="certificates">x509 certificate chain</param>
        /// <param name="signatureAlgorithm">signature algorithm</param>
        /// <param name="digestAlgorithm">digest algorithm</param>
        public X509SigningCredentials( IEnumerable<X509Certificate2> certificates, string signatureAlgorithm, string digestAlgorithm )
            : this ( new X509SecurityToken( GetLeafCertificate( certificates ) ),
                    new SecurityKeyIdentifier( new X509ChainRawDataKeyIdentifierClause( certificates ) ),
                    signatureAlgorithm, 
                    digestAlgorithm )
        {
        }
#endif

        /// <summary>
        /// Constructor for X509SigningCredentials based on an x509 certificate and the security key identifier to be used.
        /// <remarks>
        /// Note that the key identifier clause types supported by Windows Communication Foundation for generating a security key identifier that 
        /// references an X509SecurityToken are X509SubjectKeyIdentifierClause, X509ThumbprintKeyIdentifierClause, X509IssuerSerialKeyIdentifierClause, 
        /// and X509RawDataKeyIdentifierClause. However, in order to enable custom scenarios, this constructor does not perform any validation on 
        /// the clause types that were used to generate the security key identifier supplied in the <paramref name="ski"/> parameter. 
        /// </remarks>
        /// </summary>
        /// <param name="certificate">The x509 certificate.</param>
        /// <param name="ski">The security key identifier to be used.</param>
        public X509SigningCredentials(X509Certificate2 certificate, SecurityKeyIdentifier ski)
            : this(new X509SecurityToken(certificate), ski, SecurityAlgorithms.DefaultAsymmetricSignatureAlgorithm, SecurityAlgorithms.DefaultDigestAlgorithm)
        {
        }

        /// <summary>
        /// Constructor for X509SigningCredentials based on an x509 certificate and the security key identifier to be used.
        /// <remarks>
        /// Note that the key identifier clause types supported by Windows Communication Foundation for generating a security key identifier that 
        /// references an X509SecurityToken are X509SubjectKeyIdentifierClause, X509ThumbprintKeyIdentifierClause, X509IssuerSerialKeyIdentifierClause, 
        /// and X509RawDataKeyIdentifierClause. However, in order to enable custom scenarios, this constructor does not perform any validation on 
        /// the clause types that were used to generate the security key identifier supplied in the <paramref name="ski"/> parameter. 
        /// </remarks>
        /// </summary>
        /// <param name="certificate">The x509 certificate.</param>
        /// <param name="ski">The security key identifier to be used.</param>
        /// <param name="signatureAlgorithm">signature algorithm</param>
        /// <param name="digestAlgorithm">digest algorithm</param>        
        public X509SigningCredentials(X509Certificate2 certificate, SecurityKeyIdentifier ski, string signatureAlgorithm, string digestAlgorithm)
            : this(new X509SecurityToken(certificate), ski, signatureAlgorithm, digestAlgorithm)
        {
        }

        /// <summary>
        /// Constructor for X509SigningCredentials based on an x509 token and the security key identifier to be used. 
        /// It uses the token's public key, its default asymmetric signature algorithm and the specified security key identifier 
        /// </summary>
        /// <param name="token">The x509 security token.</param>
        /// <param name="ski">The security key identifier to be used.</param>
        /// <param name="signatureAlgorithm">signature algorithm</param>
        /// <param name="digestAlgorithm">digest algorithm</param>                
        internal X509SigningCredentials(X509SecurityToken token, SecurityKeyIdentifier ski, string signatureAlgorithm, string digestAlgorithm)
            : base(token.SecurityKeys[0], signatureAlgorithm, digestAlgorithm, ski)
        {
            this.certificate = token.Certificate;

            if (!this.certificate.HasPrivateKey)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID2057));
            }
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
#if INCLUDE_CERT_CHAIN
        internal static X509Certificate2 GetLeafCertificate( IEnumerable<X509Certificate2> certificates )
        {
            if ( null == certificates )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "certificates" );
            }
            X509Certificate2 cert = certificates.FirstOrDefault();
            if( null == cert )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "certificates", SR.GetString( SR.ID2100 ) );
            }
            return cert;
        }      
#endif
    }
}
