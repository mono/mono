//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Claims;

namespace System.ServiceModel.Security
{
    /// <summary>
    /// Wraps a Saml2SecurityTokenHandler. Delegates the token authentication call to
    /// this wrapped tokenAuthenticator. Wraps the returned ClaimsIdentities into
    /// an IAuthorizationPolicy.
    /// </summary>
    internal class WrappedSaml2SecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        Saml2SecurityTokenHandler _wrappedSaml2SecurityTokenHandler;
        ExceptionMapper _exceptionMapper;

        /// <summary>
        /// Initializes an instance of <see cref="WrappedSaml2SecurityTokenAuthenticator"/>
        /// </summary>
        /// <param name="saml2SecurityTokenHandler">The Saml2SecurityTokenHandler to wrap.</param>
        /// <param name="exceptionMapper">Converts token validation exceptions to SOAP faults.</param>
        public WrappedSaml2SecurityTokenAuthenticator( 
            Saml2SecurityTokenHandler saml2SecurityTokenHandler, 
            ExceptionMapper exceptionMapper )
        {
            if ( saml2SecurityTokenHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedSaml2SecurityTokenHandler" );
            }

            if ( exceptionMapper == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "exceptionMapper" );
            }

            _wrappedSaml2SecurityTokenHandler = saml2SecurityTokenHandler;
            _exceptionMapper = exceptionMapper;
        }

        /// <summary>
        /// Checks if the given token can be validated. Returns true if the token is of type
        /// Saml2SecurityToken and if the wrapped SecurityTokenHandler can validate tokens.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <returns>True if the token is of type Saml2SecurityToken and if the wrapped 
        /// SecurityTokenHandler can validate tokens.</returns>
        protected override bool CanValidateTokenCore( SecurityToken token )
        {
            return (token is Saml2SecurityToken) && _wrappedSaml2SecurityTokenHandler.CanValidateToken;
        }

        /// <summary>
        /// Validates the token using the wrapped token handler and generates IAuthorizationPolicy
        /// wrapping the returned ClaimsIdentities.
        /// </summary>
        /// <param name="token">Token to be validated.</param>
        /// <returns>Read-only collection of IAuthorizationPolicy</returns>
        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore( SecurityToken token )
        {
            IEnumerable<ClaimsIdentity> identities = null;
            try
            {
                identities = _wrappedSaml2SecurityTokenHandler.ValidateToken( token );
            }
            catch ( Exception ex )
            {
                if ( !_exceptionMapper.HandleSecurityTokenProcessingException( ex ) )
                {
                    throw;
                }
            }

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new AuthorizationPolicy(identities));

            return policies.AsReadOnly();
        }
    }
}
