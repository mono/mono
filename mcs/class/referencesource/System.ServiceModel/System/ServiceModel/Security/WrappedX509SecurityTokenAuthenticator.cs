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
    /// Wraps a X509SecurityTokenHandler. Delegates the token authentication call the inner tokenAuthenticator. 
    /// Wraps the returned ClaimsIdentities into an AuthorizationPolicy that supports IAuthorizationPolicy
    /// </summary>
    internal class WrappedX509SecurityTokenAuthenticator : X509SecurityTokenAuthenticator
    {
        X509SecurityTokenHandler _wrappedX509SecurityTokenHandler;
        ExceptionMapper _exceptionMapper;

        /// <summary>
        /// Initializes an instance of <see cref="WrappedX509SecurityTokenAuthenticator"/>
        /// </summary>
        /// <param name="wrappedX509SecurityTokenHandler">X509SecurityTokenHandler to wrap.</param>
        /// <param name="exceptionMapper">Converts token validation exceptions to SOAP faults.</param>
        public WrappedX509SecurityTokenAuthenticator( 
            X509SecurityTokenHandler wrappedX509SecurityTokenHandler, 
            ExceptionMapper exceptionMapper )
            : base( X509CertificateValidator.None, GetMapToWindowsSetting( wrappedX509SecurityTokenHandler ), true )
        {
            if ( wrappedX509SecurityTokenHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedX509SecurityTokenHandler" );
            }

            if ( exceptionMapper == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "exceptionMapper" );
            }

            _wrappedX509SecurityTokenHandler = wrappedX509SecurityTokenHandler;
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
                identities = _wrappedX509SecurityTokenHandler.ValidateToken(token);
            }
            catch ( Exception ex )
            {
                if ( !_exceptionMapper.HandleSecurityTokenProcessingException( ex ) )
                {
                    throw;
                }
            }

            // tlsnego will dispose of the x509, when we write out the bootstrap we will get a dispose error.

            bool shouldSaveBootstrapContext = SecurityTokenHandlerConfiguration.DefaultSaveBootstrapContext;
            if ( _wrappedX509SecurityTokenHandler.Configuration != null )
            {
                shouldSaveBootstrapContext = _wrappedX509SecurityTokenHandler.Configuration.SaveBootstrapContext;
            }

            if ( shouldSaveBootstrapContext )
            {
                X509SecurityToken x509Token = token as X509SecurityToken;
                SecurityToken tokenToCache;
                if ( x509Token != null )
                {
                    tokenToCache = new X509SecurityToken( x509Token.Certificate );
                }
                else
                {
                    tokenToCache = token;
                }

                BootstrapContext bootstrapContext = new BootstrapContext(tokenToCache, _wrappedX509SecurityTokenHandler);
                foreach (ClaimsIdentity identity in identities)
                {
                    identity.BootstrapContext = bootstrapContext;
                }
            }

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new AuthorizationPolicy(identities));

            return policies.AsReadOnly();
        }

        static bool GetMapToWindowsSetting( X509SecurityTokenHandler securityTokenHandler )
        {
            if ( securityTokenHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "securityTokenHandler" );
            }

            return securityTokenHandler.MapToWindows;
        }
    }
}
