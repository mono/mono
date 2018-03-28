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
    /// Wraps a Samll1SecurityTokenHandler. Delegates the token authentication call to
    /// this wrapped tokenAuthenticator.  Wraps the returned ClaimsIdentities into
    /// an IAuthorizationPolicy.
    /// </summary>
    internal class WrappedSaml11SecurityTokenAuthenticator : SamlSecurityTokenAuthenticator
    {
        SamlSecurityTokenHandler _wrappedSaml11SecurityTokenHandler;
        ExceptionMapper _exceptionMapper;

        /// <summary>
        /// Initializes an instance of <see cref="WrappedSaml11SecurityTokenAuthenticator"/>
        /// </summary>
        /// <param name="saml11SecurityTokenHandler">The Saml11SecurityTokenHandler to wrap.</param>
        /// <param name="exceptionMapper">Converts token validation exceptions to SOAP faults.</param>
        public WrappedSaml11SecurityTokenAuthenticator( 
            SamlSecurityTokenHandler saml11SecurityTokenHandler, 
            ExceptionMapper exceptionMapper )
            : base( new List<SecurityTokenAuthenticator>() )
        {
            if ( saml11SecurityTokenHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedSaml11SecurityTokenHandler" );
            }

            if ( exceptionMapper == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "exceptionMapper" );
            }

            _wrappedSaml11SecurityTokenHandler = saml11SecurityTokenHandler;
            _exceptionMapper = exceptionMapper;
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
                identities = _wrappedSaml11SecurityTokenHandler.ValidateToken( token );
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
