//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Security.Cryptography;
using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class can be used for issuing the asymmetric key based token.
    /// </summary>
    public class AsymmetricProofDescriptor : ProofDescriptor
    {
        SecurityKeyIdentifier _keyIdentifier;

        /// <summary>
        /// Constructor for extensibility 
        /// </summary>
        public AsymmetricProofDescriptor()
        {   
        }

        /// <summary>
        /// Constructs a proof token based on RSA key.
        /// </summary>
        /// <param name="rsaAlgorithm"></param>
        public AsymmetricProofDescriptor( RSA rsaAlgorithm )
        {
            if ( rsaAlgorithm == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "rsaAlgorithm" );
            }

            _keyIdentifier = new SecurityKeyIdentifier(new RsaKeyIdentifierClause(rsaAlgorithm));
        }

        /// <summary>
        /// Constructs a proof token based on key identifier.
        /// </summary>
        /// <param name="keyIdentifier"></param>
        public AsymmetricProofDescriptor( SecurityKeyIdentifier keyIdentifier )
        {
            if ( keyIdentifier == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifier" );
            }
            //
            // This is a key identifier for an asymmetric key
            //
            _keyIdentifier = keyIdentifier;
        }

        #region ProofDescriptor Overrides

        /// <summary>
        /// Basically nothing to write into the RSTR's requested proof token.
        /// </summary>
        /// <param name="response"></param>
        public override void ApplyTo( RSTR response )
        {
            if ( response == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "response" );
            }
            //
            // Nothing else to do for an asymmetric key
            //
        }

        /// <summary>
        /// This is the key identifier that the requestor has provided from the use key.
        /// This can be echo back inside the saml token if needed. This would be either 
        /// </summary>
        public override SecurityKeyIdentifier KeyIdentifier
        {
            get { return _keyIdentifier; }
        }

        #endregion
    }
}
