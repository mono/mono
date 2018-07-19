//-----------------------------------------------------------------------
// <copyright file="Saml2SecurityKeyIdentifierClause.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class is used when a Saml2Assertion is received without a KeyInfo inside the signature element.
    /// The KeyInfo describes the key required to check the signature.  When the key is needed this clause 
    /// will be presented to the current SecurityTokenResolver. It will contain the 
    /// Saml2Assertion fully read which can be querried to determine the key required.
    /// </summary>
    public class Saml2SecurityKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private Saml2Assertion assertion;

        /// <summary>
        /// Creates an instance of <see cref="Saml2SecurityKeyIdentifierClause"/>
        /// </summary>
        /// <param name="assertion">The assertion can be queried to obtain information about 
        /// the issuer when resolving the key needed to check the signature.</param>
        public Saml2SecurityKeyIdentifierClause(Saml2Assertion assertion)
            : base(typeof(Saml2SecurityKeyIdentifierClause).ToString())
        {
            this.assertion = assertion;
        }

        /// <summary>
        /// Gets the <see cref="Saml2Assertion"/> that is currently associated with this instance.
        /// </summary>
        /// <remarks>The assertion returned may be null.</remarks>
        public Saml2Assertion Assertion
        {
            get { return this.assertion; }
        }
    }
}
