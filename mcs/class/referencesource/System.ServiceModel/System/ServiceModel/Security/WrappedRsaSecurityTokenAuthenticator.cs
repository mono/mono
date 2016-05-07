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
    /// Wraps a RsaSecurityTokenHandler. Delegates the token authentication call to
    /// this wrapped tokenAuthenticator. Wraps the returned ClaimsIdentities into
    /// an IAuthorizationPolicy.
    /// </summary>
    internal class WrappedRsaSecurityTokenAuthenticator : RsaSecurityTokenAuthenticator
    {
        RsaSecurityTokenHandler _wrappedRsaSecurityTokenHandler;
        ExceptionMapper _exceptionMapper;

        /// <summary>
        /// Initializes an instance of <see cref="WrappedRsaSecurityTokenAuthenticator"/>
        /// </summary>
        /// <param name="wrappedRsaSecurityTokenHandler">The RsaSecurityTokenHandler to wrap.</param>
        /// <param name="exceptionMapper">Converts token validation exceptions to SOAP faults.</param>
        public WrappedRsaSecurityTokenAuthenticator( 
            RsaSecurityTokenHandler wrappedRsaSecurityTokenHandler, 
            ExceptionMapper exceptionMapper )
            : base()
        {
            if ( wrappedRsaSecurityTokenHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedRsaSecurityTokenHandler" );
            }

            if ( exceptionMapper == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "exceptionMapper" );
            }

            _wrappedRsaSecurityTokenHandler = wrappedRsaSecurityTokenHandler;
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
                identities = _wrappedRsaSecurityTokenHandler.ValidateToken( token );
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
