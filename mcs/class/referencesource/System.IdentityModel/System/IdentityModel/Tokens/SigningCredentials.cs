//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.IdentityModel;

    public class SigningCredentials
    {
        string digestAlgorithm;
        string signatureAlgorithm;
        SecurityKey signingKey;
        SecurityKeyIdentifier signingKeyIdentifier;

        public SigningCredentials(SecurityKey signingKey, string signatureAlgorithm, string digestAlgorithm) :
            this(signingKey, signatureAlgorithm, digestAlgorithm, null)
        { }

        public SigningCredentials(SecurityKey signingKey, string signatureAlgorithm, string digestAlgorithm, SecurityKeyIdentifier signingKeyIdentifier)
        {
            if (signingKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("signingKey"));
            }

            if (signatureAlgorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("signatureAlgorithm"));
            }
            if (digestAlgorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("digestAlgorithm"));
            }
            this.signingKey = signingKey;
            this.signatureAlgorithm = signatureAlgorithm;
            this.digestAlgorithm = digestAlgorithm;
            this.signingKeyIdentifier = signingKeyIdentifier;
        }

        public string DigestAlgorithm
        {
            get { return this.digestAlgorithm; }
        }

        public string SignatureAlgorithm
        {
            get { return this.signatureAlgorithm; }
        }

        public SecurityKey SigningKey
        {
            get { return this.signingKey; }
        }

        public SecurityKeyIdentifier SigningKeyIdentifier
        {
            get { return this.signingKeyIdentifier; }
        }
    }
}
