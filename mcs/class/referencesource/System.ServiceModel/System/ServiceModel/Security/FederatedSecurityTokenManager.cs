//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;

using SR = System.ServiceModel.SR;


namespace System.ServiceModel.Security
{
    /// <summary>
    /// SecurityTokenManager that enables plugging custom tokens easily.
    /// The SecurityTokenManager provides methods to register custom token providers,
    /// serializers and authenticators. It can wrap another Token Managers and
    /// delegate token operation calls to it if required.
    /// </summary>
    /// <remarks>
    /// Framework use only - this is an implementation adapter class that is used to expose
    /// the Framework SecurityTokenHandlers to WCF.
    /// </remarks>
    sealed class FederatedSecurityTokenManager : ServiceCredentialsSecurityTokenManager
    {
        static string ListenUriProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ListenUri";

        ExceptionMapper _exceptionMapper;
        SecurityTokenResolver _defaultTokenResolver;

        SecurityTokenHandlerCollection _securityTokenHandlerCollection;
        object _syncObject = new object();

        ReadOnlyCollection<CookieTransform> _cookieTransforms;
        SessionSecurityTokenCache _tokenCache;

        /// <summary>
        /// Initializes an instance of <see cref="FederatedSecurityTokenManager"/>.
        /// </summary>
        /// <param name="parentCredentials">ServiceCredentials that created this instance of TokenManager.</param>
        /// <exception cref="ArgumentNullException">The argument 'parentCredentials' is null.</exception>
        public FederatedSecurityTokenManager( ServiceCredentials parentCredentials )
            : base( parentCredentials )
        {
            if ( parentCredentials == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "parentCredentials" );
            }

            if ( parentCredentials.IdentityConfiguration == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "parentCredentials.IdentityConfiguration" );
            }

            _exceptionMapper = parentCredentials.ExceptionMapper;

            _securityTokenHandlerCollection = parentCredentials.IdentityConfiguration.SecurityTokenHandlers;                        
            _tokenCache = _securityTokenHandlerCollection.Configuration.Caches.SessionSecurityTokenCache;                        
            _cookieTransforms = SessionSecurityTokenHandler.DefaultCookieTransforms;
        }

        /// <summary>
        /// Returns the list of SecurityTokenHandlers.
        /// </summary>
        public SecurityTokenHandlerCollection SecurityTokenHandlers
        {
            //
            // 


            get { return _securityTokenHandlerCollection; }
        }

        /// <summary>
        /// Gets or sets the ExceptionMapper to be used when throwing exceptions.
        /// </summary>
        public ExceptionMapper ExceptionMapper
        {
            get
            {
                return _exceptionMapper;
            }
            set
            {
                if ( value == null )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "value" );
                }
                _exceptionMapper = value;
            }
        }

        #region SecurityTokenManager Implementation

        /// <summary>
        /// Overriden from the base class. Creates the requested Token Authenticator.
        /// Looks up the list of Token Handlers registered with the token Manager
        /// based on the TokenType Uri in the SecurityTokenRequirement. If none is found,
        /// then the call is delegated to the inner Token Manager.
        /// </summary>
        /// <param name="tokenRequirement">Security Token Requirement for which the Authenticator should be created.</param>
        /// <param name="outOfBandTokenResolver">Token resolver that resolves any out-of-band tokens.</param>
        /// <returns>Instance of Security Token Authenticator.</returns>
        /// <exception cref="ArgumentNullException">'tokenRequirement' parameter is null.</exception>
        /// <exception cref="NotSupportedException">No Authenticator is registered for the given token type.</exception>
        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator( SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver )
        {
            if ( tokenRequirement == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "tokenRequirement" );
            }

            outOfBandTokenResolver = null;

            // Check for a registered authenticator
            SecurityTokenAuthenticator securityTokenAuthenticator = null;

            string tokenType = tokenRequirement.TokenType;

            //
            // When the TokenRequirement.TokenType is null, we treat this as a SAML issued token case. It may be SAML 1.1 or SAML 2.0.
            //
            if ( String.IsNullOrEmpty( tokenType ) )
            {
                return CreateSamlSecurityTokenAuthenticator( tokenRequirement, out outOfBandTokenResolver );
            }

            //
            // When the TokenType is set, build a token authenticator for the specified token type.
            //
            SecurityTokenHandler securityTokenHandler = _securityTokenHandlerCollection[tokenType];
            if ( ( securityTokenHandler != null ) && ( securityTokenHandler.CanValidateToken ) )
            {
                outOfBandTokenResolver = GetDefaultOutOfBandTokenResolver();

                if ( StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.UserName ) )
                {
                    UserNameSecurityTokenHandler upSecurityTokenHandler = securityTokenHandler as UserNameSecurityTokenHandler;
                    if ( upSecurityTokenHandler == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException( SR.GetString( SR.ID4072, securityTokenHandler.GetType(), tokenType, typeof( UserNameSecurityTokenHandler ) ) ) );
                    }
                    securityTokenAuthenticator = new WrappedUserNameSecurityTokenAuthenticator( upSecurityTokenHandler, _exceptionMapper );
                }
                else if ( StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.Kerberos ) )
                {
                    securityTokenAuthenticator = CreateInnerSecurityTokenAuthenticator( tokenRequirement, out outOfBandTokenResolver );
                }
                else if ( StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.Rsa ) )
                {
                    RsaSecurityTokenHandler rsaSecurityTokenHandler = securityTokenHandler as RsaSecurityTokenHandler;
                    if ( rsaSecurityTokenHandler == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException( SR.GetString( SR.ID4072, securityTokenHandler.GetType(), tokenType, typeof( RsaSecurityTokenHandler ) ) ) );
                    }
                    securityTokenAuthenticator = new WrappedRsaSecurityTokenAuthenticator( rsaSecurityTokenHandler, _exceptionMapper );
                }
                else if ( StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.X509Certificate ) )
                {
                    X509SecurityTokenHandler x509SecurityTokenHandler = securityTokenHandler as X509SecurityTokenHandler;
                    if ( x509SecurityTokenHandler == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException( SR.GetString( SR.ID4072, securityTokenHandler.GetType(), tokenType, typeof( X509SecurityTokenHandler ) ) ) );
                    }
                    securityTokenAuthenticator = new WrappedX509SecurityTokenAuthenticator( x509SecurityTokenHandler, _exceptionMapper );
                }
                else if ( StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.SamlTokenProfile11 ) ||
                          StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.OasisWssSamlTokenProfile11 ) )
                {
                    SamlSecurityTokenHandler saml11SecurityTokenHandler = securityTokenHandler as SamlSecurityTokenHandler;
                    if ( saml11SecurityTokenHandler == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException( SR.GetString( SR.ID4072, securityTokenHandler.GetType(), tokenType, typeof( SamlSecurityTokenHandler ) ) ) );
                    }

                    if ( saml11SecurityTokenHandler.Configuration == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4274 ) );
                    }

                    securityTokenAuthenticator = new WrappedSaml11SecurityTokenAuthenticator( saml11SecurityTokenHandler, _exceptionMapper );
                    // The out-of-band token resolver will be used by WCF to decrypt any encrypted SAML tokens.
                    outOfBandTokenResolver = saml11SecurityTokenHandler.Configuration.ServiceTokenResolver;
                }
                else if ( StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.Saml2TokenProfile11 ) ||
                          StringComparer.Ordinal.Equals( tokenType, SecurityTokenTypes.OasisWssSaml2TokenProfile11 ) )
                {
                    Saml2SecurityTokenHandler saml2SecurityTokenHandler = securityTokenHandler as Saml2SecurityTokenHandler;
                    if ( saml2SecurityTokenHandler == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException( SR.GetString( SR.ID4072, securityTokenHandler.GetType(), tokenType, typeof( Saml2SecurityTokenHandler ) ) ) );
                    }

                    if ( saml2SecurityTokenHandler.Configuration == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4274 ) );
                    }

                    securityTokenAuthenticator = new WrappedSaml2SecurityTokenAuthenticator( saml2SecurityTokenHandler, _exceptionMapper );
                    // The out-of-band token resolver will be used by WCF to decrypt any encrypted SAML tokens.
                    outOfBandTokenResolver = saml2SecurityTokenHandler.Configuration.ServiceTokenResolver;
                }
                else if ( StringComparer.Ordinal.Equals( tokenType, ServiceModelSecurityTokenTypes.SecureConversation ) )
                {
                    RecipientServiceModelSecurityTokenRequirement tr = tokenRequirement as RecipientServiceModelSecurityTokenRequirement;
                    if ( tr == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4240, tokenRequirement.GetType().ToString() ) );
                    }

                    securityTokenAuthenticator = SetupSecureConversationWrapper( tr, securityTokenHandler as SessionSecurityTokenHandler, out outOfBandTokenResolver );
                }
                else
                {
                    securityTokenAuthenticator = new SecurityTokenAuthenticatorAdapter( securityTokenHandler, _exceptionMapper );
                }
            }
            else
            {
                if ( tokenType == ServiceModelSecurityTokenTypes.SecureConversation
                    || tokenType == ServiceModelSecurityTokenTypes.MutualSslnego
                    || tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego
                    || tokenType == ServiceModelSecurityTokenTypes.SecurityContext
                    || tokenType == ServiceModelSecurityTokenTypes.Spnego )
                {
                    RecipientServiceModelSecurityTokenRequirement tr = tokenRequirement as RecipientServiceModelSecurityTokenRequirement;
                    if ( tr == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4240, tokenRequirement.GetType().ToString() ) );
                    }

                    securityTokenAuthenticator = SetupSecureConversationWrapper( tr, null, out outOfBandTokenResolver );
                }
                else
                {
                    securityTokenAuthenticator = CreateInnerSecurityTokenAuthenticator( tokenRequirement, out outOfBandTokenResolver );
                }
            }

            return securityTokenAuthenticator;
        }

        /// <summary>
        /// Helper method to setup the WrappedSecureConversttion
        /// </summary>
        SecurityTokenAuthenticator SetupSecureConversationWrapper( RecipientServiceModelSecurityTokenRequirement tokenRequirement, SessionSecurityTokenHandler tokenHandler, out SecurityTokenResolver outOfBandTokenResolver )
        {
            // This code requires Orcas SP1 to compile.
            // WCF expects this securityTokenAuthenticator to support:
            // 1. IIssuanceSecurityTokenAuthenticator
            // 2. ICommunicationObject is needed for this to work right.
            // WCF opens a listener in this STA that handles the nego and uses an internal class for negotiating the 
            // the bootstrap tokens.  We want to handle ValidateToken to return our authorization policies and surface the bootstrap tokens.

            // when sp1 is installed, use this one.
            //SecurityTokenAuthenticator sta = base.CreateSecureConversationTokenAuthenticator( tokenRequirement as RecipientServiceModelSecurityTokenRequirement, _saveBootstrapTokensInSession, out outOfBandTokenResolver );

            // use this code if SP1 is not installed
            SecurityTokenAuthenticator sta = base.CreateSecurityTokenAuthenticator( tokenRequirement, out outOfBandTokenResolver );
            SessionSecurityTokenHandler sessionTokenHandler = tokenHandler;

            //
            // If there is no SCT handler here, create one.
            //
            if ( tokenHandler == null )
            {
                sessionTokenHandler = new SessionSecurityTokenHandler( _cookieTransforms, SessionSecurityTokenHandler.DefaultTokenLifetime );
                sessionTokenHandler.ContainingCollection = _securityTokenHandlerCollection;
                sessionTokenHandler.Configuration = _securityTokenHandlerCollection.Configuration;
            }

            if ( ServiceCredentials != null )
            {
                sessionTokenHandler.Configuration.MaxClockSkew = ServiceCredentials.IdentityConfiguration.MaxClockSkew;
            }

            SctClaimsHandler claimsHandler = new SctClaimsHandler(
                                                    _securityTokenHandlerCollection,
                                                    GetNormalizedEndpointId( tokenRequirement ) );

            WrappedSessionSecurityTokenAuthenticator wssta = new WrappedSessionSecurityTokenAuthenticator( sessionTokenHandler, sta,
                                                                                                           claimsHandler, _exceptionMapper );
            WrappedTokenCache wrappedTokenCache = new WrappedTokenCache( _tokenCache, claimsHandler);
            SetWrappedTokenCache( wrappedTokenCache, sta, wssta, claimsHandler );
            outOfBandTokenResolver = wrappedTokenCache;

            return wssta;
        }

        /// <summary>
        /// The purpose of this method is to set our WrappedTokenCache as the token cache for SCT's.
        /// And to set our OnIssuedToken callback when in cookie mode.
        /// We have to use reflection here as this is a private method.
        /// </summary>
        static void SetWrappedTokenCache(
            WrappedTokenCache wrappedTokenCache,
            SecurityTokenAuthenticator sta,
            WrappedSessionSecurityTokenAuthenticator wssta,
            SctClaimsHandler claimsHandler )
        {
            if ( sta is SecuritySessionSecurityTokenAuthenticator )
            {
                ( sta as SecuritySessionSecurityTokenAuthenticator ).IssuedTokenCache = wrappedTokenCache;
            }
            else if ( sta is AcceleratedTokenAuthenticator )
            {
                ( sta as AcceleratedTokenAuthenticator ).IssuedTokenCache = wrappedTokenCache;
            }
            else if ( sta is SpnegoTokenAuthenticator )
            {
                ( sta as SpnegoTokenAuthenticator ).IssuedTokenCache = wrappedTokenCache;
            }
            else if ( sta is TlsnegoTokenAuthenticator )
            {
                ( sta as TlsnegoTokenAuthenticator ).IssuedTokenCache = wrappedTokenCache;
            }

            // we need to special case this as the OnTokenIssued callback is not hooked up in the cookie mode case.
            IIssuanceSecurityTokenAuthenticator issuanceTokenAuthenticator = sta as IIssuanceSecurityTokenAuthenticator;
            if ( issuanceTokenAuthenticator != null )
            {
                issuanceTokenAuthenticator.IssuedSecurityTokenHandler = claimsHandler.OnTokenIssued;
                issuanceTokenAuthenticator.RenewedSecurityTokenHandler = claimsHandler.OnTokenRenewed;
            }
        }

        /// <summary>
        /// Overriden from the base class. Creates the requested Token Serializer.
        /// Returns a Security Token Serializer that is wraps the list of token
        /// hanlders registerd and also the serializers from the inner token manager.
        /// </summary>
        /// <param name="version">SecurityTokenVersion of the serializer to be created.</param>
        /// <returns>Instance of SecurityTokenSerializer.</returns>
        /// <exception cref="ArgumentNullException">Input parameter is null.</exception>
        public override SecurityTokenSerializer CreateSecurityTokenSerializer( SecurityTokenVersion version )
        {
            if ( version == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "version" );
            }

            TrustVersion trustVersion = null;
            SecureConversationVersion scVersion = null;

            foreach ( string securitySpecification in version.GetSecuritySpecifications() )
            {
                if ( StringComparer.Ordinal.Equals( securitySpecification, WSTrustFeb2005Constants.NamespaceURI ) )
                {
                    trustVersion = TrustVersion.WSTrustFeb2005;
                }
                else if ( StringComparer.Ordinal.Equals( securitySpecification, WSTrust13Constants.NamespaceURI ) )
                {
                    trustVersion = TrustVersion.WSTrust13;
                }
                else if ( StringComparer.Ordinal.Equals( securitySpecification, WSSecureConversationFeb2005Constants.Namespace ) )
                {
                    scVersion = SecureConversationVersion.WSSecureConversationFeb2005;
                }
                else if ( StringComparer.Ordinal.Equals( securitySpecification, WSSecureConversation13Constants.Namespace ) )
                {
                    scVersion = SecureConversationVersion.WSSecureConversation13;
                }

                if ( trustVersion != null && scVersion != null )
                {
                    break;
                }
            }

            if ( trustVersion == null )
            {
                trustVersion = TrustVersion.WSTrust13;
            }

            if ( scVersion == null )
            {
                scVersion = SecureConversationVersion.WSSecureConversation13;
            }

            WsSecurityTokenSerializerAdapter adapter = new WsSecurityTokenSerializerAdapter( _securityTokenHandlerCollection,
                GetSecurityVersion( version ), trustVersion, scVersion, false, this.ServiceCredentials.IssuedTokenAuthentication.SamlSerializer,
                this.ServiceCredentials.SecureConversationAuthentication.SecurityStateEncoder,
                this.ServiceCredentials.SecureConversationAuthentication.SecurityContextClaimTypes );

            adapter.MapExceptionsToSoapFaults = true;
            adapter.ExceptionMapper = _exceptionMapper;

            return adapter;
        }

        /// <summary>
        /// The out-of-band token resolver to be used if the authenticator does
        /// not provide another.
        /// </summary>
        /// <remarks>By default this will create the resolver with the service certificate and 
        /// know certificates collections specified in the service credentials when the STS is 
        /// hosted inside WCF.</remarks>
        SecurityTokenResolver GetDefaultOutOfBandTokenResolver()
        {
            if ( _defaultTokenResolver == null )
            {
                lock ( _syncObject )
                {
                    if ( _defaultTokenResolver == null )
                    {
                        // 
                        // Create default Out-Of-Band SecurityResolver.
                        //
                        List<SecurityToken> outOfBandTokens = new List<SecurityToken>();
                        if ( base.ServiceCredentials.ServiceCertificate.Certificate != null )
                        {
                            outOfBandTokens.Add( new X509SecurityToken( base.ServiceCredentials.ServiceCertificate.Certificate ) );
                        }

                        if ( ( base.ServiceCredentials.IssuedTokenAuthentication.KnownCertificates != null ) && ( base.ServiceCredentials.IssuedTokenAuthentication.KnownCertificates.Count > 0 ) )
                        {
                            for ( int i = 0; i < base.ServiceCredentials.IssuedTokenAuthentication.KnownCertificates.Count; ++i )
                            {
                                outOfBandTokens.Add( new X509SecurityToken( base.ServiceCredentials.IssuedTokenAuthentication.KnownCertificates[i]));
                            }
                        }

                        _defaultTokenResolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver( outOfBandTokens.AsReadOnly(), false );
                    }
                }
            }

            return _defaultTokenResolver;
        }
        /// <summary>
        /// There is a bug in WCF where the version obtained from the public SecurityTokenVersion strings is wrong.
        /// The internal MessageSecurityTokenVersion has the right version.
        /// </summary>
        internal static SecurityVersion GetSecurityVersion( SecurityTokenVersion tokenVersion )
        {
            if ( tokenVersion == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "tokenVersion" );
            }

            //
            // Workaround for WCF bug.
            // In .NET 3.5 WCF returns the wrong Token Specification. We need to reflect on the
            // internal code so we can access the SecurityVersion directly instead of depending
            // on the security specification.
            //
            if ( tokenVersion is MessageSecurityTokenVersion )
            {
                SecurityVersion sv = ( tokenVersion as MessageSecurityTokenVersion ).SecurityVersion;

                if ( sv != null )
                {
                    return sv;
                }
            }
            else
            {
                if ( tokenVersion.GetSecuritySpecifications().Contains( WSSecurity11Constants.Namespace ) )
                {
                    return SecurityVersion.WSSecurity11;
                }
                else if ( tokenVersion.GetSecuritySpecifications().Contains( WSSecurity10Constants.Namespace ) )
                {
                    return SecurityVersion.WSSecurity10;
                }
            }

            return SecurityVersion.WSSecurity11;
        }

        #endregion // SecurityTokenManager Implementation

        /// <summary>
        /// This method creates the inner security token authenticator from the base class.
        /// The wrapped token cache is initialized with this authenticator.
        /// </summary>
        SecurityTokenAuthenticator CreateInnerSecurityTokenAuthenticator( SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver )
        {
            SecurityTokenAuthenticator securityTokenAuthenticator = base.CreateSecurityTokenAuthenticator( tokenRequirement, out outOfBandTokenResolver );
            SctClaimsHandler claimsHandler = new SctClaimsHandler(
                                        _securityTokenHandlerCollection,
                                        GetNormalizedEndpointId( tokenRequirement ) );
            
            SetWrappedTokenCache( new WrappedTokenCache( _tokenCache, claimsHandler ), securityTokenAuthenticator, null, claimsHandler );
            return securityTokenAuthenticator;
        }

        /// <summary>
        /// This method creates a SAML security token authenticator when token type is null.
        /// It wraps the SAML 1.1 and the SAML 2.0 token handlers that are configured.
        /// If no token handler was found, then the inner token manager is created.
        /// </summary>
        SecurityTokenAuthenticator CreateSamlSecurityTokenAuthenticator( SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver )
        {
            outOfBandTokenResolver = null;

            SecurityTokenAuthenticator securityTokenAuthenticator = null;

            SamlSecurityTokenHandler saml11SecurityTokenHandler = _securityTokenHandlerCollection[SecurityTokenTypes.SamlTokenProfile11] as SamlSecurityTokenHandler;
            Saml2SecurityTokenHandler saml2SecurityTokenHandler = _securityTokenHandlerCollection[SecurityTokenTypes.Saml2TokenProfile11] as Saml2SecurityTokenHandler;

            if ( saml11SecurityTokenHandler != null && saml11SecurityTokenHandler.Configuration == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4274 ) );
            }

            if ( saml2SecurityTokenHandler != null && saml2SecurityTokenHandler.Configuration == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4274 ) );
            }

            if ( saml11SecurityTokenHandler != null && saml2SecurityTokenHandler != null )
            {
                //
                // Both SAML 1.1 and SAML 2.0 token handlers have been configured.
                //

                WrappedSaml11SecurityTokenAuthenticator wrappedSaml11SecurityTokenAuthenticator = new WrappedSaml11SecurityTokenAuthenticator( saml11SecurityTokenHandler, _exceptionMapper );
                WrappedSaml2SecurityTokenAuthenticator wrappedSaml2SecurityTokenAuthenticator = new WrappedSaml2SecurityTokenAuthenticator( saml2SecurityTokenHandler, _exceptionMapper );

                securityTokenAuthenticator = new WrappedSamlSecurityTokenAuthenticator( wrappedSaml11SecurityTokenAuthenticator, wrappedSaml2SecurityTokenAuthenticator );

                // The out-of-band token resolver will be used by WCF to decrypt any encrypted SAML tokens.
                List<SecurityTokenResolver> resolvers = new List<SecurityTokenResolver>();
                resolvers.Add( saml11SecurityTokenHandler.Configuration.ServiceTokenResolver );
                resolvers.Add( saml2SecurityTokenHandler.Configuration.ServiceTokenResolver );
                outOfBandTokenResolver = new AggregateTokenResolver( resolvers );
            }
            else if ( saml11SecurityTokenHandler == null && saml2SecurityTokenHandler != null )
            {
                //
                // SAML 1.1 token handler is not present but SAML 2.0 is. Set the token type to SAML 2.0
                //

                securityTokenAuthenticator = new WrappedSaml2SecurityTokenAuthenticator( saml2SecurityTokenHandler, _exceptionMapper );

                // The out-of-band token resolver will be used by WCF to decrypt any encrypted SAML tokens.
                outOfBandTokenResolver = saml2SecurityTokenHandler.Configuration.ServiceTokenResolver;
            }
            else if ( saml11SecurityTokenHandler != null && saml2SecurityTokenHandler == null )
            {
                //
                // SAML 1.1 token handler is present but SAML 2.0 is not. Set the token type to SAML 1.1
                //

                securityTokenAuthenticator = new WrappedSaml11SecurityTokenAuthenticator( saml11SecurityTokenHandler, _exceptionMapper );

                // The out-of-band token resolver will be used by WCF to decrypt any encrypted SAML tokens.
                outOfBandTokenResolver = saml11SecurityTokenHandler.Configuration.ServiceTokenResolver;
            }
            else
            {
                securityTokenAuthenticator = CreateInnerSecurityTokenAuthenticator( tokenRequirement, out outOfBandTokenResolver );
            }

            return securityTokenAuthenticator;
        }

        /// <summary>
        /// Converts the ListenUri in the <see cref="SecurityTokenRequirement"/> to a normalized string.
        /// The method preserves the Uri scheme, port and absolute path and replaces the host name 
        /// with the string 'NormalizedHostName'.
        /// </summary>
        /// <param name="tokenRequirement">The <see cref="SecurityTokenRequirement"/> which contains the 'ListenUri' property.</param>
        /// <returns>A string representing the Normalized URI string.</returns>
        public static string GetNormalizedEndpointId( SecurityTokenRequirement tokenRequirement )
        {
            if ( tokenRequirement == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "tokenRequirement" );
            }

            Uri listenUri = null;
            if ( tokenRequirement.Properties.ContainsKey( ListenUriProperty ) )
            {
                listenUri = tokenRequirement.Properties[ListenUriProperty] as Uri;
            }

            if ( listenUri == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4287, tokenRequirement ) );
            }

            if ( listenUri.IsDefaultPort )
            {
                return String.Format( CultureInfo.InvariantCulture, "{0}://NormalizedHostName{1}", listenUri.Scheme, listenUri.AbsolutePath );
            }
            else
            {
                return String.Format( CultureInfo.InvariantCulture, "{0}://NormalizedHostName:{1}{2}", listenUri.Scheme, listenUri.Port, listenUri.AbsolutePath );
            }
        }
    }

}
