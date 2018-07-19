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
    /// Wraps a UserNameSecurityTokenHandler. Delegates the token authentication call to
    /// this wrapped tokenAuthenticator. Wraps the returned ClaimsIdentities into
    /// an IAuthorizationPolicy.
    /// </summary>
    internal class WrappedUserNameSecurityTokenAuthenticator : UserNameSecurityTokenAuthenticator
    {
        UserNameSecurityTokenHandler _wrappedUserNameSecurityTokenHandler;
        ExceptionMapper _exceptionMapper;

        /// <summary>
        /// Initializes an instance of <see cref="WrappedUserNameSecurityTokenAuthenticator"/>
        /// </summary>
        /// <param name="wrappedUserNameSecurityTokenHandler">The UserNameSecurityTokenHandler to wrap.</param>
        /// <param name="exceptionMapper">Converts token validation exceptions to SOAP faults.</param>
        public WrappedUserNameSecurityTokenAuthenticator( 
            UserNameSecurityTokenHandler wrappedUserNameSecurityTokenHandler, 
            ExceptionMapper exceptionMapper )
            : base()
        {
            if ( wrappedUserNameSecurityTokenHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedUserNameSecurityTokenHandler" );
            }

            if ( exceptionMapper == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "exceptionMapper" );
            }

            _wrappedUserNameSecurityTokenHandler = wrappedUserNameSecurityTokenHandler;
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
            ReadOnlyCollection<ClaimsIdentity> identities = null;
            try
            {
                identities = _wrappedUserNameSecurityTokenHandler.ValidateToken( token );
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

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore( string userName, string password )
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new NotImplementedException( SR.GetString( SR.ID4008, "WrappedUserNameSecurityTokenAuthenticator", "ValidateUserNamePasswordCore" ) ) );
        }
    }
}
