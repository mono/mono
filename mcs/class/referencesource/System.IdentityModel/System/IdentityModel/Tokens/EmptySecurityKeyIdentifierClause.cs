//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Represents an empty SecurityKeyClause.  This class is used when an 'encrypted data element' or ' signature element' does
    /// not contain a 'key info element' that is used to describe the key required to decrypt the data or check the signature.
    /// </summary>
    public class EmptySecurityKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        object _context;

        /// <summary>
        /// Creates an instance of <see cref="EmptySecurityKeyIdentifierClause"/>
        /// </summary>
        /// <remarks>This constructor assumes that the user knows how to resolve the key required without any context.</remarks>
        public EmptySecurityKeyIdentifierClause()
            : this( null )
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="EmptySecurityKeyIdentifierClause"/>
        /// </summary>
        /// <param name="context">Used to provide a hint when there is a need resolve an empty clause to a particular key.
        /// In the case of Saml11 and Saml2 tokens that have signatures without KeyInfo, 
        /// this clause will contain the assertion that is currently being processed.</param>
        public EmptySecurityKeyIdentifierClause( object context )
            : base( typeof( EmptySecurityKeyIdentifierClause ).ToString() )
        {
            _context = context;
        }

        /// <summary>
        /// Used to provide a hint when there is a need to resolve to a particular key.
        /// In the case of Saml11 and Saml2 tokens that have signatures without KeyInfo, 
        /// this will contain the assertion that is currently being processed.
        /// </summary>
        public object Context
        {
            get { return _context; }
        }
    }
}
