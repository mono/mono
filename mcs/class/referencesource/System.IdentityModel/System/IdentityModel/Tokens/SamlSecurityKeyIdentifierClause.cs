//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class is used when a SamlAssertion is received without a KeyInfo inside the signature element.
    /// The KeyInfo describes the key required to check the signature.  When the key is needed this clause 
    /// will be presented to the current SecurityTokenResolver. It will contain the 
    /// SamlAssertion fully read which can be querried to determine the key required.
    /// </summary>
    public class SamlSecurityKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        SamlAssertion assertion;

        /// <summary>
        /// Creates an instance of <see cref="SamlSecurityKeyIdentifierClause"/>
        /// </summary>
        /// <param name="assertion">The assertion can be queried to obtain information about 
        /// the issuer when resolving the key needed to check the signature. The assertion will
        /// be read completely when this clause is passed to the SecurityTokenResolver.</param>
        public SamlSecurityKeyIdentifierClause(SamlAssertion assertion)
            : base(typeof(SamlSecurityKeyIdentifierClause).ToString())
        {
            this.assertion = assertion;
        }

        /// <summary>
        /// When Saml11 assertions are being process and have signatures without KeyInfo, 
        /// this property will contain the assertion that is currently being processed.
        /// </summary>
        /// <remarks>The Assertion may be null.</remarks>
        public SamlAssertion Assertion
        {
            get { return this.assertion; }
        }
    }
}
