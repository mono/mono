//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Claims;

namespace System.ServiceModel.Security
{
    /// <summary>
    /// Authenticator that wraps both SAML 1.1 and SAML 2.0 WrapperSecurityTokenAuthenticators.
    /// </summary>
    internal class WrappedSamlSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        WrappedSaml11SecurityTokenAuthenticator _wrappedSaml11SecurityTokenAuthenticator;
        WrappedSaml2SecurityTokenAuthenticator _wrappedSaml2SecurityTokenAuthenticator;

        public WrappedSamlSecurityTokenAuthenticator( WrappedSaml11SecurityTokenAuthenticator wrappedSaml11SecurityTokenAuthenticator, WrappedSaml2SecurityTokenAuthenticator wrappedSaml2SecurityTokenAuthenticator )
        {
            if ( wrappedSaml11SecurityTokenAuthenticator == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedSaml11SecurityTokenAuthenticator" );
            }

            if ( wrappedSaml2SecurityTokenAuthenticator == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedSaml2SecurityTokenAuthenticator" );
            }

            _wrappedSaml11SecurityTokenAuthenticator = wrappedSaml11SecurityTokenAuthenticator;
            _wrappedSaml2SecurityTokenAuthenticator = wrappedSaml2SecurityTokenAuthenticator;
        }

        protected override bool CanValidateTokenCore( SecurityToken token )
        {
            return ( _wrappedSaml11SecurityTokenAuthenticator.CanValidateToken( token ) || _wrappedSaml2SecurityTokenAuthenticator.CanValidateToken( token ) );
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore( SecurityToken token )
        {
            if ( _wrappedSaml11SecurityTokenAuthenticator.CanValidateToken( token ) )
            {
                return _wrappedSaml11SecurityTokenAuthenticator.ValidateToken( token );
            }
            else if ( _wrappedSaml2SecurityTokenAuthenticator.CanValidateToken( token ) )
            {
                return _wrappedSaml2SecurityTokenAuthenticator.ValidateToken( token );
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new ArgumentException( SR.GetString( SR.ID4101, token.GetType().ToString() ) ) );
            }
        }
    }
}
