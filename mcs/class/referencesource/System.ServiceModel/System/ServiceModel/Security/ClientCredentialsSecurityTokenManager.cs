//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using System.IdentityModel.Protocols.WSTrust;
    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;

    public class ClientCredentialsSecurityTokenManager : SecurityTokenManager
    {
        ClientCredentials parent;

        public ClientCredentialsSecurityTokenManager(ClientCredentials clientCredentials)
        {
            if (clientCredentials == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clientCredentials");
            }
            this.parent = clientCredentials;
        }

        public ClientCredentials ClientCredentials
        {
            get { return this.parent; }
        }

        string GetServicePrincipalName(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            EndpointAddress targetAddress = initiatorRequirement.TargetAddress;
            if (targetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenRequirementDoesNotSpecifyTargetAddress, initiatorRequirement));
            }
            IdentityVerifier identityVerifier;
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement != null)
            {
                identityVerifier = securityBindingElement.LocalClientSettings.IdentityVerifier;
            }
            else
            {
                identityVerifier = IdentityVerifier.CreateDefault();
            }
            EndpointIdentity identity;
            identityVerifier.TryGetIdentity(targetAddress, out identity);
            return SecurityUtils.GetSpnFromIdentity(identity, targetAddress);
        }

        SspiSecurityToken GetSpnegoClientCredential(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            InitiatorServiceModelSecurityTokenRequirement sspiCredentialRequirement = new InitiatorServiceModelSecurityTokenRequirement();
            sspiCredentialRequirement.TargetAddress = initiatorRequirement.TargetAddress;
            sspiCredentialRequirement.TokenType = ServiceModelSecurityTokenTypes.SspiCredential;
            sspiCredentialRequirement.Via = initiatorRequirement.Via;
            sspiCredentialRequirement.RequireCryptographicToken = false;
            sspiCredentialRequirement.SecurityBindingElement = initiatorRequirement.SecurityBindingElement;
            sspiCredentialRequirement.MessageSecurityVersion = initiatorRequirement.MessageSecurityVersion;
            ChannelParameterCollection parameters;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters))
            {
                sspiCredentialRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = parameters;
            }
            SecurityTokenProvider sspiTokenProvider = this.CreateSecurityTokenProvider(sspiCredentialRequirement);
            SecurityUtils.OpenTokenProviderIfRequired(sspiTokenProvider, TimeSpan.Zero);
            SspiSecurityToken sspiToken = (SspiSecurityToken) sspiTokenProvider.GetToken(TimeSpan.Zero);
            SecurityUtils.AbortTokenProviderIfRequired(sspiTokenProvider);
            return sspiToken;
        }

        SecurityTokenProvider CreateSpnegoTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            EndpointAddress targetAddress = initiatorRequirement.TargetAddress;
            if (targetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenRequirementDoesNotSpecifyTargetAddress, initiatorRequirement));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenProviderRequiresSecurityBindingElement, initiatorRequirement));
            }
            SspiIssuanceChannelParameter sspiChannelParameter = GetSspiIssuanceChannelParameter(initiatorRequirement);
            bool negotiateTokenOnOpen = (sspiChannelParameter == null ? true : sspiChannelParameter.GetTokenOnOpen);
            LocalClientSecuritySettings localClientSettings = securityBindingElement.LocalClientSettings;
            BindingContext issuerBindingContext = initiatorRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            SpnegoTokenProvider spnegoTokenProvider = new SpnegoTokenProvider(sspiChannelParameter != null ? sspiChannelParameter.CredentialsHandle : null, securityBindingElement);
            SspiSecurityToken clientSspiToken = GetSpnegoClientCredential(initiatorRequirement);
            spnegoTokenProvider.ClientCredential = clientSspiToken.NetworkCredential;
            spnegoTokenProvider.IssuerAddress = initiatorRequirement.IssuerAddress;
            spnegoTokenProvider.AllowedImpersonationLevel = parent.Windows.AllowedImpersonationLevel;
            spnegoTokenProvider.AllowNtlm = clientSspiToken.AllowNtlm;
            spnegoTokenProvider.IdentityVerifier = localClientSettings.IdentityVerifier;
            spnegoTokenProvider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
            // if this is not a supporting token, authenticate the server
            spnegoTokenProvider.AuthenticateServer = !initiatorRequirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.SupportingTokenAttachmentModeProperty);
            spnegoTokenProvider.NegotiateTokenOnOpen = negotiateTokenOnOpen;
            spnegoTokenProvider.CacheServiceTokens = negotiateTokenOnOpen || localClientSettings.CacheCookies;
            spnegoTokenProvider.IssuerBindingContext = issuerBindingContext;
            spnegoTokenProvider.MaxServiceTokenCachingTime = localClientSettings.MaxCookieCachingTime;
            spnegoTokenProvider.ServiceTokenValidityThresholdPercentage = localClientSettings.CookieRenewalThresholdPercentage;
            spnegoTokenProvider.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this);
            spnegoTokenProvider.TargetAddress = targetAddress;
            spnegoTokenProvider.Via = initiatorRequirement.GetPropertyOrDefault<Uri>(InitiatorServiceModelSecurityTokenRequirement.ViaProperty, null);
            spnegoTokenProvider.ApplicationProtectionRequirements = (issuerBindingContext != null) ? issuerBindingContext.BindingParameters.Find<ChannelProtectionRequirements>() : null;
            spnegoTokenProvider.InteractiveNegoExLogonEnabled = this.ClientCredentials.SupportInteractive;
            
            return spnegoTokenProvider;
        }

        SecurityTokenProvider CreateTlsnegoClientX509TokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            InitiatorServiceModelSecurityTokenRequirement clientX509Requirement = new InitiatorServiceModelSecurityTokenRequirement();
            clientX509Requirement.TokenType = SecurityTokenTypes.X509Certificate;
            clientX509Requirement.TargetAddress = initiatorRequirement.TargetAddress;
            clientX509Requirement.SecurityBindingElement = initiatorRequirement.SecurityBindingElement;
            clientX509Requirement.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
            clientX509Requirement.RequireCryptographicToken = true;
            clientX509Requirement.MessageSecurityVersion = initiatorRequirement.MessageSecurityVersion;
            clientX509Requirement.KeyUsage = SecurityKeyUsage.Signature;
            clientX509Requirement.KeyType = SecurityKeyType.AsymmetricKey;
            clientX509Requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
            ChannelParameterCollection parameters;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters))
            {
                clientX509Requirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = parameters;
            }
            return this.CreateSecurityTokenProvider(clientX509Requirement);
        }

        SecurityTokenAuthenticator CreateTlsnegoServerX509TokenAuthenticator(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            InitiatorServiceModelSecurityTokenRequirement serverX509Requirement = new InitiatorServiceModelSecurityTokenRequirement();
            serverX509Requirement.TokenType = SecurityTokenTypes.X509Certificate;
            serverX509Requirement.RequireCryptographicToken = true;
            serverX509Requirement.SecurityBindingElement = initiatorRequirement.SecurityBindingElement;
            serverX509Requirement.MessageSecurityVersion = initiatorRequirement.MessageSecurityVersion;
            serverX509Requirement.KeyUsage = SecurityKeyUsage.Exchange;
            serverX509Requirement.KeyType = SecurityKeyType.AsymmetricKey;
            serverX509Requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
            ChannelParameterCollection parameters;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters))
            {
                serverX509Requirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = parameters;
            }
            SecurityTokenResolver dummy;
            return this.CreateSecurityTokenAuthenticator(serverX509Requirement, out dummy);
        }

        SspiIssuanceChannelParameter GetSspiIssuanceChannelParameter(SecurityTokenRequirement initiatorRequirement)
        {
            ChannelParameterCollection channelParameters;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out channelParameters))
            {
                if (channelParameters != null)
                {
                    for (int i = 0; i < channelParameters.Count; ++i)
                    {
                        if (channelParameters[i] is SspiIssuanceChannelParameter)
                        {
                            return (SspiIssuanceChannelParameter)channelParameters[i];
                        }
                    }
                }
            }
            return null;
        }

        SecurityTokenProvider CreateTlsnegoTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement, bool requireClientCertificate)
        {
            EndpointAddress targetAddress = initiatorRequirement.TargetAddress;
            if (targetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenRequirementDoesNotSpecifyTargetAddress, initiatorRequirement));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenProviderRequiresSecurityBindingElement, initiatorRequirement));
            }
            SspiIssuanceChannelParameter sspiChannelParameter = GetSspiIssuanceChannelParameter(initiatorRequirement);
            bool negotiateTokenOnOpen = sspiChannelParameter != null && sspiChannelParameter.GetTokenOnOpen;
            LocalClientSecuritySettings localClientSettings = securityBindingElement.LocalClientSettings;
            BindingContext issuerBindingContext = initiatorRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            TlsnegoTokenProvider tlsnegoTokenProvider = new TlsnegoTokenProvider();
            tlsnegoTokenProvider.IssuerAddress = initiatorRequirement.IssuerAddress;
            tlsnegoTokenProvider.NegotiateTokenOnOpen = negotiateTokenOnOpen;
            tlsnegoTokenProvider.CacheServiceTokens = negotiateTokenOnOpen || localClientSettings.CacheCookies;
            if (requireClientCertificate)
            {
                tlsnegoTokenProvider.ClientTokenProvider = this.CreateTlsnegoClientX509TokenProvider(initiatorRequirement);
            }
            tlsnegoTokenProvider.IssuerBindingContext = issuerBindingContext;
            tlsnegoTokenProvider.ApplicationProtectionRequirements = (issuerBindingContext != null) ? issuerBindingContext.BindingParameters.Find<ChannelProtectionRequirements>() : null;
            tlsnegoTokenProvider.MaxServiceTokenCachingTime = localClientSettings.MaxCookieCachingTime;
            tlsnegoTokenProvider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
            tlsnegoTokenProvider.ServerTokenAuthenticator = this.CreateTlsnegoServerX509TokenAuthenticator(initiatorRequirement);
            tlsnegoTokenProvider.ServiceTokenValidityThresholdPercentage = localClientSettings.CookieRenewalThresholdPercentage;
            tlsnegoTokenProvider.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this);
            tlsnegoTokenProvider.TargetAddress = initiatorRequirement.TargetAddress;
            tlsnegoTokenProvider.Via = initiatorRequirement.GetPropertyOrDefault<Uri>(InitiatorServiceModelSecurityTokenRequirement.ViaProperty, null);
            return tlsnegoTokenProvider;
        }

        SecurityTokenProvider CreateSecureConversationSecurityTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            EndpointAddress targetAddress = initiatorRequirement.TargetAddress;
            if (targetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenRequirementDoesNotSpecifyTargetAddress, initiatorRequirement));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenProviderRequiresSecurityBindingElement, initiatorRequirement));
            }
            LocalClientSecuritySettings localClientSettings = securityBindingElement.LocalClientSettings;
            BindingContext issuerBindingContext = initiatorRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            ChannelParameterCollection channelParameters = initiatorRequirement.GetPropertyOrDefault<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, null);
            bool isSessionMode = initiatorRequirement.SupportSecurityContextCancellation;
            if (isSessionMode)
            {
                SecuritySessionSecurityTokenProvider sessionTokenProvider = new SecuritySessionSecurityTokenProvider(GetCredentialsHandle(initiatorRequirement));
                sessionTokenProvider.BootstrapSecurityBindingElement = SecurityUtils.GetIssuerSecurityBindingElement(initiatorRequirement);
                sessionTokenProvider.IssuedSecurityTokenParameters = initiatorRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
                sessionTokenProvider.IssuerBindingContext = issuerBindingContext;
                sessionTokenProvider.KeyEntropyMode = securityBindingElement.KeyEntropyMode;
                sessionTokenProvider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
                sessionTokenProvider.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this);
                sessionTokenProvider.TargetAddress = targetAddress;
                sessionTokenProvider.Via = initiatorRequirement.GetPropertyOrDefault<Uri>(InitiatorServiceModelSecurityTokenRequirement.ViaProperty, null);
                Uri privacyNoticeUri;
                if (initiatorRequirement.TryGetProperty<Uri>(ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty, out privacyNoticeUri))
                {
                    sessionTokenProvider.PrivacyNoticeUri = privacyNoticeUri;
                }
                int privacyNoticeVersion;
                if (initiatorRequirement.TryGetProperty<int>(ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty, out privacyNoticeVersion))
                {
                    sessionTokenProvider.PrivacyNoticeVersion = privacyNoticeVersion;
                }
                EndpointAddress localAddress;
                if (initiatorRequirement.TryGetProperty<EndpointAddress>(ServiceModelSecurityTokenRequirement.DuplexClientLocalAddressProperty, out localAddress))
                {
                    sessionTokenProvider.LocalAddress = localAddress;
                }
                sessionTokenProvider.ChannelParameters = channelParameters;
                sessionTokenProvider.WebHeaders = initiatorRequirement.WebHeaders;

                return sessionTokenProvider;
            }
            else
            {
                AcceleratedTokenProvider acceleratedTokenProvider = new AcceleratedTokenProvider(GetCredentialsHandle(initiatorRequirement));
                acceleratedTokenProvider.IssuerAddress = initiatorRequirement.IssuerAddress;
                acceleratedTokenProvider.BootstrapSecurityBindingElement = SecurityUtils.GetIssuerSecurityBindingElement(initiatorRequirement);
                acceleratedTokenProvider.CacheServiceTokens = localClientSettings.CacheCookies;
                acceleratedTokenProvider.IssuerBindingContext = issuerBindingContext;
                acceleratedTokenProvider.KeyEntropyMode = securityBindingElement.KeyEntropyMode;
                acceleratedTokenProvider.MaxServiceTokenCachingTime = localClientSettings.MaxCookieCachingTime;
                acceleratedTokenProvider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
                acceleratedTokenProvider.ServiceTokenValidityThresholdPercentage = localClientSettings.CookieRenewalThresholdPercentage;
                acceleratedTokenProvider.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this);
                acceleratedTokenProvider.TargetAddress = targetAddress;
                acceleratedTokenProvider.Via = initiatorRequirement.GetPropertyOrDefault<Uri>(InitiatorServiceModelSecurityTokenRequirement.ViaProperty, null);
                Uri privacyNoticeUri;
                if (initiatorRequirement.TryGetProperty<Uri>(ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty, out privacyNoticeUri))
                {
                    acceleratedTokenProvider.PrivacyNoticeUri = privacyNoticeUri;
                }
                acceleratedTokenProvider.ChannelParameters = channelParameters;
                int privacyNoticeVersion;
                if (initiatorRequirement.TryGetProperty<int>(ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty, out privacyNoticeVersion))
                {
                    acceleratedTokenProvider.PrivacyNoticeVersion = privacyNoticeVersion;
                }
                return acceleratedTokenProvider;
            }
        }

        SecurityTokenProvider CreateServerX509TokenProvider(EndpointAddress targetAddress)
        {
            X509Certificate2 targetServerCertificate = null;
            if (targetAddress != null)
            {
                parent.ServiceCertificate.ScopedCertificates.TryGetValue(targetAddress.Uri, out targetServerCertificate);
            }
            if (targetServerCertificate == null)
            {
                targetServerCertificate = parent.ServiceCertificate.DefaultCertificate;
            }
            if ((targetServerCertificate == null) && (targetAddress.Identity != null) && (targetAddress.Identity.GetType() == typeof(X509CertificateEndpointIdentity)))
            {
                targetServerCertificate = ((X509CertificateEndpointIdentity)targetAddress.Identity).Certificates[0];
            }            
            if (targetServerCertificate != null)
            {
                return new X509SecurityTokenProvider(targetServerCertificate);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ServiceCertificateNotProvidedOnClientCredentials, targetAddress.Uri)));
            }
        }

        X509SecurityTokenAuthenticator CreateServerX509TokenAuthenticator()
        {
            return new X509SecurityTokenAuthenticator(parent.ServiceCertificate.Authentication.GetCertificateValidator(), false);
        }

        X509SecurityTokenAuthenticator CreateServerSslX509TokenAuthenticator()
        {
            if (parent.ServiceCertificate.SslCertificateAuthentication != null)
            {
                return new X509SecurityTokenAuthenticator(parent.ServiceCertificate.SslCertificateAuthentication.GetCertificateValidator(), false);
            }

            return CreateServerX509TokenAuthenticator();
        }

        bool IsDigestAuthenticationScheme(SecurityTokenRequirement requirement)
        {
            if (requirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty))
            {
                AuthenticationSchemes authScheme = (AuthenticationSchemes)requirement.Properties[ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty];

                if (!authScheme.IsSingleton())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.HttpRequiresSingleAuthScheme, authScheme));
                }

                return (authScheme == AuthenticationSchemes.Digest);
            }
            else
            {
                return false;
            }
        }

        internal protected bool IsIssuedSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            if (requirement != null && requirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.IssuerAddressProperty))
            {
                // handle all issued token requirements except for spnego, tlsnego and secure conversation
                if (requirement.TokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego || requirement.TokenType == ServiceModelSecurityTokenTypes.MutualSslnego
                    || requirement.TokenType == ServiceModelSecurityTokenTypes.SecureConversation || requirement.TokenType == ServiceModelSecurityTokenTypes.Spnego)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        void CopyIssuerChannelBehaviorsAndAddSecurityCredentials(IssuedSecurityTokenProvider federationTokenProvider, KeyedByTypeCollection<IEndpointBehavior> issuerChannelBehaviors, EndpointAddress issuerAddress)
        {
            if (issuerChannelBehaviors != null)
            {
                foreach (IEndpointBehavior behavior in issuerChannelBehaviors)
                {
                    if (behavior is SecurityCredentialsManager)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuerChannelBehaviorsCannotContainSecurityCredentialsManager, issuerAddress, typeof(SecurityCredentialsManager))));
                    }
                    federationTokenProvider.IssuerChannelBehaviors.Add(behavior);
                }
            }
            federationTokenProvider.IssuerChannelBehaviors.Add(parent);
        }

        SecurityKeyEntropyMode GetIssuerBindingKeyEntropyModeOrDefault(Binding issuerBinding)
        {
            BindingElementCollection bindingElements = issuerBinding.CreateBindingElements();
            SecurityBindingElement securityBindingElement = bindingElements.Find<SecurityBindingElement>();
            if (securityBindingElement != null)
            {
                return securityBindingElement.KeyEntropyMode;
            }
            else
            {
                return parent.IssuedToken.DefaultKeyEntropyMode;
            }
        }

        void GetIssuerBindingSecurityVersion(Binding issuerBinding, MessageSecurityVersion issuedTokenParametersDefaultMessageSecurityVersion, SecurityBindingElement outerSecurityBindingElement, out MessageSecurityVersion messageSecurityVersion, out SecurityTokenSerializer tokenSerializer)
        {
            // Logic for setting version is:
            // 1. use issuer SBE
            // 2. use ITSP
            // 3. use outer SBE
            //

            messageSecurityVersion = null;

            if (issuerBinding != null)
            {
                BindingElementCollection bindingElements = issuerBinding.CreateBindingElements();
                SecurityBindingElement securityBindingElement = bindingElements.Find<SecurityBindingElement>();
                if (securityBindingElement != null)
                {
                    messageSecurityVersion = securityBindingElement.MessageSecurityVersion;
                }
            }

            if (messageSecurityVersion == null)
            {
                if (issuedTokenParametersDefaultMessageSecurityVersion != null)
                {
                    messageSecurityVersion = issuedTokenParametersDefaultMessageSecurityVersion;
                }
                else if (outerSecurityBindingElement != null)
                {
                    messageSecurityVersion = outerSecurityBindingElement.MessageSecurityVersion;
                }
            }

            if (messageSecurityVersion == null)
            {
                messageSecurityVersion = MessageSecurityVersion.Default;
            }

            tokenSerializer = this.CreateSecurityTokenSerializer(messageSecurityVersion.SecurityTokenVersion);
        }

        IssuedSecurityTokenProvider CreateIssuedSecurityTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement, FederatedClientCredentialsParameters actAsOnBehalfOfParameters)
        {
            if (initiatorRequirement.TargetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenRequirementDoesNotSpecifyTargetAddress, initiatorRequirement));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenProviderRequiresSecurityBindingElement, initiatorRequirement));
            }

            EndpointAddress issuerAddress = initiatorRequirement.IssuerAddress;
            Binding issuerBinding = initiatorRequirement.IssuerBinding;

            //
            // If the issuer address is indeed anonymous or null, we will try the local issuer
            //
            bool isLocalIssuer = (issuerAddress == null || issuerAddress.Equals(EndpointAddress.AnonymousAddress));

            if (isLocalIssuer)
            {
                issuerAddress = parent.IssuedToken.LocalIssuerAddress;
                issuerBinding = parent.IssuedToken.LocalIssuerBinding;
            }
            if (issuerAddress == null)
            {
                // if issuer address is still null then the user forgot to specify the local issuer
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StsAddressNotSet, initiatorRequirement.TargetAddress)));
            }
            if (issuerBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StsBindingNotSet, issuerAddress)));
            }

            Uri issuerUri = issuerAddress.Uri;
            KeyedByTypeCollection<IEndpointBehavior> issuerChannelBehaviors;
            if (!parent.IssuedToken.IssuerChannelBehaviors.TryGetValue(issuerAddress.Uri, out issuerChannelBehaviors) && isLocalIssuer)
            {
                issuerChannelBehaviors = parent.IssuedToken.LocalIssuerChannelBehaviors;
            }

            IssuedSecurityTokenProvider federationTokenProvider = new IssuedSecurityTokenProvider(GetCredentialsHandle(initiatorRequirement));
            federationTokenProvider.TokenHandlerCollectionManager = this.parent.SecurityTokenHandlerCollectionManager;
            federationTokenProvider.TargetAddress = initiatorRequirement.TargetAddress;
            CopyIssuerChannelBehaviorsAndAddSecurityCredentials(federationTokenProvider, issuerChannelBehaviors, issuerAddress);
            federationTokenProvider.CacheIssuedTokens = parent.IssuedToken.CacheIssuedTokens;
            federationTokenProvider.IdentityVerifier = securityBindingElement.LocalClientSettings.IdentityVerifier;
            federationTokenProvider.IssuerAddress = issuerAddress;
            federationTokenProvider.IssuerBinding = issuerBinding;
            federationTokenProvider.KeyEntropyMode = GetIssuerBindingKeyEntropyModeOrDefault(issuerBinding);
            federationTokenProvider.MaxIssuedTokenCachingTime = parent.IssuedToken.MaxIssuedTokenCachingTime;
            federationTokenProvider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
            MessageSecurityVersion issuerSecurityVersion;
            SecurityTokenSerializer issuerSecurityTokenSerializer;
            IssuedSecurityTokenParameters issuedTokenParameters = initiatorRequirement.GetProperty<IssuedSecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);

            GetIssuerBindingSecurityVersion(issuerBinding, issuedTokenParameters.DefaultMessageSecurityVersion, initiatorRequirement.SecurityBindingElement, out issuerSecurityVersion, out issuerSecurityTokenSerializer);
            federationTokenProvider.MessageSecurityVersion = issuerSecurityVersion;
            federationTokenProvider.SecurityTokenSerializer = issuerSecurityTokenSerializer;
            federationTokenProvider.IssuedTokenRenewalThresholdPercentage = parent.IssuedToken.IssuedTokenRenewalThresholdPercentage;

            IEnumerable<XmlElement> tokenRequestParameters = issuedTokenParameters.CreateRequestParameters(issuerSecurityVersion, issuerSecurityTokenSerializer);
            if (tokenRequestParameters != null)
            {
                foreach (XmlElement requestParameter in tokenRequestParameters)
                {
                    federationTokenProvider.TokenRequestParameters.Add(requestParameter);
                }
            }
            ChannelParameterCollection channelParameters;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out channelParameters))
            {
                federationTokenProvider.ChannelParameters = channelParameters;
            }

            federationTokenProvider.SetupActAsOnBehalfOfParameters(actAsOnBehalfOfParameters);
            return federationTokenProvider;
        }

        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
        {
            return this.CreateSecurityTokenProvider(tokenRequirement, false);
        }

        internal SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement, bool disableInfoCard)
        {
            if (tokenRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }

            SecurityTokenProvider result = null;
            if (disableInfoCard || !CardSpaceTryCreateSecurityTokenProviderStub(tokenRequirement, this, out result))
            {
                if (tokenRequirement is RecipientServiceModelSecurityTokenRequirement && tokenRequirement.TokenType == SecurityTokenTypes.X509Certificate && tokenRequirement.KeyUsage == SecurityKeyUsage.Exchange)
                {
                    // this is the uncorrelated duplex case
                    if (parent.ClientCertificate.Certificate == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ClientCertificateNotProvidedOnClientCredentials)));
                    }
                    result = new X509SecurityTokenProvider(parent.ClientCertificate.Certificate);
                }
                else if (tokenRequirement is InitiatorServiceModelSecurityTokenRequirement)
                {
                    InitiatorServiceModelSecurityTokenRequirement initiatorRequirement = tokenRequirement as InitiatorServiceModelSecurityTokenRequirement;
#pragma warning suppress 56506 // initiatorRequirement will never be null due to the preceding 'is' validation.
                    string tokenType = initiatorRequirement.TokenType;
                    if (IsIssuedSecurityTokenRequirement(initiatorRequirement))
                    {
                        FederatedClientCredentialsParameters additionalParameters = this.FindFederatedChannelParameters(tokenRequirement);

                        if (additionalParameters != null && additionalParameters.IssuedSecurityToken != null)
                        {
                            return new SimpleSecurityTokenProvider(additionalParameters.IssuedSecurityToken, tokenRequirement);
                        }
                        
                        result = CreateIssuedSecurityTokenProvider(initiatorRequirement, additionalParameters);
                    }
                    else if (tokenType == SecurityTokenTypes.X509Certificate)
                    {
                        if (initiatorRequirement.Properties.ContainsKey(SecurityTokenRequirement.KeyUsageProperty) && initiatorRequirement.KeyUsage == SecurityKeyUsage.Exchange)
                        {
                            result = CreateServerX509TokenProvider(initiatorRequirement.TargetAddress);
                        }
                        else
                        {
                            if (parent.ClientCertificate.Certificate == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ClientCertificateNotProvidedOnClientCredentials)));
                            }
                            result = new X509SecurityTokenProvider(parent.ClientCertificate.Certificate);
                        }
                    }
                    else if (tokenType == SecurityTokenTypes.Kerberos)
                    {
                        string spn = GetServicePrincipalName(initiatorRequirement);
                        result = new KerberosSecurityTokenProviderWrapper(
                            new KerberosSecurityTokenProvider(spn, parent.Windows.AllowedImpersonationLevel, SecurityUtils.GetNetworkCredentialOrDefault(parent.Windows.ClientCredential)), 
                            GetCredentialsHandle(initiatorRequirement));
                    }
                    else if (tokenType == SecurityTokenTypes.UserName)
                    {
                        if (parent.UserName.UserName == null )
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UserNamePasswordNotProvidedOnClientCredentials)));
                        }
                        result = new UserNameSecurityTokenProvider(parent.UserName.UserName, parent.UserName.Password);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.SspiCredential)
                    {
                        if (IsDigestAuthenticationScheme(initiatorRequirement))
                        {
                            result = new SspiSecurityTokenProvider(SecurityUtils.GetNetworkCredentialOrDefault(parent.HttpDigest.ClientCredential), true, parent.HttpDigest.AllowedImpersonationLevel);
                        }
                        else
                        {

 #pragma warning disable 618   // to disable AllowNtlm obsolete wanring.      
  
                            result = new SspiSecurityTokenProvider(SecurityUtils.GetNetworkCredentialOrDefault(parent.Windows.ClientCredential), 
                      
                                parent.Windows.AllowNtlm, 
                                parent.Windows.AllowedImpersonationLevel);
 #pragma warning restore 618 
                   
                        }
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.Spnego)
                    {
                        result = CreateSpnegoTokenProvider(initiatorRequirement);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.MutualSslnego)
                    {
                        result = CreateTlsnegoTokenProvider(initiatorRequirement, true);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego)
                    {
                        result = CreateTlsnegoTokenProvider(initiatorRequirement, false);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.SecureConversation)
                    {
                        result = CreateSecureConversationSecurityTokenProvider(initiatorRequirement);
                    }
                }
            }

            if ((result == null) && !tokenRequirement.IsOptionalToken)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateProviderForRequirement, tokenRequirement)));
            }

            return result;
        }

        bool CardSpaceTryCreateSecurityTokenProviderStub(SecurityTokenRequirement tokenRequirement, ClientCredentialsSecurityTokenManager clientCredentialsTokenManager, out SecurityTokenProvider provider)
        {
            return InfoCardHelper.TryCreateSecurityTokenProvider(tokenRequirement, clientCredentialsTokenManager, out provider);
        }

        protected SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityVersion version)
        {
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            return this.CreateSecurityTokenSerializer(MessageSecurityTokenVersion.GetSecurityTokenVersion(version, true));
        }

        public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
        {
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }

            if (this.parent != null && this.parent.UseIdentityConfiguration)
            {
                return this.WrapTokenHandlersAsSecurityTokenSerializer(version);
            }

            MessageSecurityTokenVersion wsVersion = version as MessageSecurityTokenVersion;
            if (wsVersion != null)
            {
                return new WSSecurityTokenSerializer(wsVersion.SecurityVersion, wsVersion.TrustVersion, wsVersion.SecureConversationVersion, wsVersion.EmitBspRequiredAttributes, null, null, null);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateSerializerForVersion, version)));
            }
        }

        private SecurityTokenSerializer WrapTokenHandlersAsSecurityTokenSerializer(SecurityTokenVersion version)
        {
            TrustVersion trustVersion = TrustVersion.WSTrust13;
            SecureConversationVersion scVersion = SecureConversationVersion.WSSecureConversation13;
            SecurityVersion securityVersion = SecurityVersion.WSSecurity11;
            foreach (string securitySpecification in version.GetSecuritySpecifications())
            {
                if (StringComparer.Ordinal.Equals(securitySpecification, WSTrustFeb2005Constants.NamespaceURI))
                {
                    trustVersion = TrustVersion.WSTrustFeb2005;
                }
                else if (StringComparer.Ordinal.Equals(securitySpecification, WSTrust13Constants.NamespaceURI))
                {
                    trustVersion = TrustVersion.WSTrust13;
                }
                else if (StringComparer.Ordinal.Equals(securitySpecification, System.IdentityModel.WSSecureConversationFeb2005Constants.Namespace))
                {
                    scVersion = SecureConversationVersion.WSSecureConversationFeb2005;
                }
                else if (StringComparer.Ordinal.Equals(securitySpecification, System.IdentityModel.WSSecureConversation13Constants.Namespace))
                {
                    scVersion = SecureConversationVersion.WSSecureConversation13;
                }
            }

            securityVersion = FederatedSecurityTokenManager.GetSecurityVersion(version);

            //
            // 


            SecurityTokenHandlerCollectionManager sthcm = this.parent.SecurityTokenHandlerCollectionManager;
            WsSecurityTokenSerializerAdapter adapter = new WsSecurityTokenSerializerAdapter(sthcm[SecurityTokenHandlerCollectionManager.Usage.Default], securityVersion, trustVersion, scVersion, false, null, null, null);
            return adapter;
        }

        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (tokenRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }

            outOfBandTokenResolver = null;
            SecurityTokenAuthenticator result = null;

            InitiatorServiceModelSecurityTokenRequirement initiatorRequirement = tokenRequirement as InitiatorServiceModelSecurityTokenRequirement;
            if (initiatorRequirement != null)
            {
                string tokenType = initiatorRequirement.TokenType;
                if (IsIssuedSecurityTokenRequirement(initiatorRequirement))
                {
                    return new GenericXmlSecurityTokenAuthenticator();
                }
                else if (tokenType == SecurityTokenTypes.X509Certificate)
                {
                    if (initiatorRequirement.IsOutOfBandToken)
                    {
                        // when the client side soap security asks for a token authenticator, its for doing
                        // identity checks on the out of band server certificate
                        result = new X509SecurityTokenAuthenticator(X509CertificateValidator.None);
                    }
                    else if (initiatorRequirement.PreferSslCertificateAuthenticator)
                    {
                        result = CreateServerSslX509TokenAuthenticator();
                    }
                    else
                    {
                        result = CreateServerX509TokenAuthenticator();
                    }
                }
                else if (tokenType == SecurityTokenTypes.Rsa)
                {
                    result = new RsaSecurityTokenAuthenticator();
                }
                else if (tokenType == SecurityTokenTypes.Kerberos)
                {
                    result = new KerberosRequestorSecurityTokenAuthenticator();
                }
                else if (tokenType == ServiceModelSecurityTokenTypes.SecureConversation
                    || tokenType == ServiceModelSecurityTokenTypes.MutualSslnego
                    || tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego
                    || tokenType == ServiceModelSecurityTokenTypes.Spnego)
                {
                    result = new GenericXmlSecurityTokenAuthenticator();
                }
            }
            else if ((tokenRequirement is RecipientServiceModelSecurityTokenRequirement) && tokenRequirement.TokenType == SecurityTokenTypes.X509Certificate)
            {
                // uncorrelated duplex case
                result = CreateServerX509TokenAuthenticator();
            }

            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateAuthenticatorForRequirement, tokenRequirement)));
            }

            return result;
        }

        SafeFreeCredentials GetCredentialsHandle(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            SspiIssuanceChannelParameter sspiChannelParameter = GetSspiIssuanceChannelParameter(initiatorRequirement);
            return sspiChannelParameter != null ? sspiChannelParameter.CredentialsHandle : null;
        }

        /// <summary>
        /// Looks for the first FederatedClientCredentialsParameters object in the ChannelParameterCollection
        /// property on the tokenRequirement.
        /// </summary>
        internal FederatedClientCredentialsParameters FindFederatedChannelParameters(SecurityTokenRequirement tokenRequirement)
        {
            FederatedClientCredentialsParameters issuedTokenClientCredentialsParameters = null;

            ChannelParameterCollection channelParameterCollection = null;
            if (tokenRequirement.TryGetProperty<ChannelParameterCollection>(
                                     ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty,
                                     out channelParameterCollection))
            {
                if (channelParameterCollection != null)
                {
                    foreach (object obj in channelParameterCollection)
                    {
                        issuedTokenClientCredentialsParameters = obj as FederatedClientCredentialsParameters;
                        if (issuedTokenClientCredentialsParameters != null)
                        {
                            break;
                        }
                    }
                }
            }
            return issuedTokenClientCredentialsParameters;
        }

        internal class KerberosSecurityTokenProviderWrapper : CommunicationObjectSecurityTokenProvider
        {
            KerberosSecurityTokenProvider innerProvider;
            SafeFreeCredentials credentialsHandle;
            bool ownCredentialsHandle = false;
            
            public KerberosSecurityTokenProviderWrapper(KerberosSecurityTokenProvider innerProvider, SafeFreeCredentials credentialsHandle)
            {
                this.innerProvider = innerProvider;
                this.credentialsHandle = credentialsHandle;
            }

            public override void OnOpening()
            {
                base.OnOpening();
                if (this.credentialsHandle == null)
                {
                    this.credentialsHandle = SecurityUtils.GetCredentialsHandle("Kerberos", this.innerProvider.NetworkCredential, false);
                    this.ownCredentialsHandle = true;
                }
            }

            public override void OnClose(TimeSpan timeout)
            {
                base.OnClose(timeout);
                FreeCredentialsHandle();
            }

            public override void OnAbort()
            {
                base.OnAbort();
                FreeCredentialsHandle();
            }

            void FreeCredentialsHandle()
            {
                if (this.credentialsHandle != null)
                {
                    if (this.ownCredentialsHandle)
                    {
                        this.credentialsHandle.Close();
                    }
                    this.credentialsHandle = null;
                }
            }

            internal SecurityToken GetToken(TimeSpan timeout, ChannelBinding channelbinding)
            {
                return new KerberosRequestorSecurityToken(this.innerProvider.ServicePrincipalName, 
                    this.innerProvider.TokenImpersonationLevel, this.innerProvider.NetworkCredential,
                    SecurityUniqueId.Create().Value, this.credentialsHandle, channelbinding);
            }
            protected override SecurityToken GetTokenCore(TimeSpan timeout)
            {
                return this.GetToken(timeout, null);
            }
        }
    }
}
