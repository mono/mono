//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;

    public class ServiceCredentialsSecurityTokenManager : SecurityTokenManager, IEndpointIdentityProvider
    {
        ServiceCredentials parent;

        public ServiceCredentialsSecurityTokenManager(ServiceCredentials parent)
        {
            if (parent == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            }
            this.parent = parent;
        }

        public ServiceCredentials ServiceCredentials
        {
            get { return parent; }
        }

        public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
        {
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            MessageSecurityTokenVersion wsVersion = version as MessageSecurityTokenVersion;
            if (wsVersion != null)
            {
                SamlSerializer samlSerializer = null;
                if (parent.IssuedTokenAuthentication != null)
                    samlSerializer = parent.IssuedTokenAuthentication.SamlSerializer;
                else
                    samlSerializer = new SamlSerializer();

                return new WSSecurityTokenSerializer(wsVersion.SecurityVersion, wsVersion.TrustVersion, wsVersion.SecureConversationVersion, wsVersion.EmitBspRequiredAttributes, samlSerializer, parent.SecureConversationAuthentication.SecurityStateEncoder, parent.SecureConversationAuthentication.SecurityContextClaimTypes);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateSerializerForVersion, version)));
            }
        }

        protected SecurityTokenAuthenticator CreateSecureConversationTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, bool preserveBootstrapTokens, out SecurityTokenResolver sctResolver)
        {
            SecurityBindingElement securityBindingElement = recipientRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenAuthenticatorRequiresSecurityBindingElement, recipientRequirement));
            }
            bool isCookieMode = !recipientRequirement.SupportSecurityContextCancellation;
            LocalServiceSecuritySettings localServiceSettings = securityBindingElement.LocalServiceSettings;
            IMessageFilterTable<EndpointAddress> endpointFilterTable = recipientRequirement.GetPropertyOrDefault<IMessageFilterTable<EndpointAddress>>(ServiceModelSecurityTokenRequirement.EndpointFilterTableProperty, null);

            if (!isCookieMode)
            {
                sctResolver = new SecurityContextSecurityTokenResolver(Int32.MaxValue, false);

                // remember this authenticator for future reference
                SecuritySessionSecurityTokenAuthenticator authenticator = new SecuritySessionSecurityTokenAuthenticator();
                authenticator.BootstrapSecurityBindingElement = SecurityUtils.GetIssuerSecurityBindingElement(recipientRequirement);
                authenticator.IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
                authenticator.IssuedTokenCache = (ISecurityContextSecurityTokenCache)sctResolver;
                authenticator.IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
                authenticator.KeyEntropyMode = securityBindingElement.KeyEntropyMode;
                authenticator.ListenUri = recipientRequirement.ListenUri;
                authenticator.SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite;
                authenticator.SessionTokenLifetime = TimeSpan.MaxValue;
                authenticator.KeyRenewalInterval = securityBindingElement.LocalServiceSettings.SessionKeyRenewalInterval;
                authenticator.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this);
                authenticator.EndpointFilterTable = endpointFilterTable;
                authenticator.MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations;
                authenticator.NegotiationTimeout = localServiceSettings.NegotiationTimeout;
                authenticator.PreserveBootstrapTokens = preserveBootstrapTokens;
                return authenticator;
            }
            else
            {
                sctResolver = new SecurityContextSecurityTokenResolver(localServiceSettings.MaxCachedCookies, true, localServiceSettings.MaxClockSkew);

                AcceleratedTokenAuthenticator authenticator = new AcceleratedTokenAuthenticator();
                authenticator.BootstrapSecurityBindingElement = SecurityUtils.GetIssuerSecurityBindingElement(recipientRequirement);
                authenticator.KeyEntropyMode = securityBindingElement.KeyEntropyMode;
                authenticator.EncryptStateInServiceToken = true;
                authenticator.IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
                authenticator.IssuedTokenCache = (ISecurityContextSecurityTokenCache)sctResolver;
                authenticator.IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
                authenticator.ListenUri = recipientRequirement.ListenUri;
                authenticator.SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite;
                authenticator.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this);
                authenticator.SecurityStateEncoder = parent.SecureConversationAuthentication.SecurityStateEncoder;
                authenticator.KnownTypes = parent.SecureConversationAuthentication.SecurityContextClaimTypes;
                authenticator.PreserveBootstrapTokens = preserveBootstrapTokens;

                // local security quotas
                authenticator.MaximumCachedNegotiationState = localServiceSettings.MaxStatefulNegotiations;
                authenticator.NegotiationTimeout = localServiceSettings.NegotiationTimeout;
                authenticator.ServiceTokenLifetime = localServiceSettings.IssuedCookieLifetime;
                authenticator.MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations;

                // audit settings
                authenticator.AuditLogLocation = recipientRequirement.AuditLogLocation;
                authenticator.SuppressAuditFailure = recipientRequirement.SuppressAuditFailure;
                authenticator.MessageAuthenticationAuditLevel = recipientRequirement.MessageAuthenticationAuditLevel;
                authenticator.EndpointFilterTable = endpointFilterTable;
                return authenticator;
            }
        }

        SecurityTokenAuthenticator CreateSpnegoSecurityTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, out SecurityTokenResolver sctResolver)
        {
            SecurityBindingElement securityBindingElement = recipientRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenAuthenticatorRequiresSecurityBindingElement, recipientRequirement));
            }
            bool isCookieMode = !recipientRequirement.SupportSecurityContextCancellation;
            LocalServiceSecuritySettings localServiceSettings = securityBindingElement.LocalServiceSettings;
            sctResolver = new SecurityContextSecurityTokenResolver(localServiceSettings.MaxCachedCookies, true);
            ExtendedProtectionPolicy extendedProtectionPolicy = null;
            recipientRequirement.TryGetProperty<ExtendedProtectionPolicy>(ServiceModelSecurityTokenRequirement.ExtendedProtectionPolicy, out extendedProtectionPolicy);

            SpnegoTokenAuthenticator authenticator = new SpnegoTokenAuthenticator();
            authenticator.ExtendedProtectionPolicy = extendedProtectionPolicy;
            authenticator.AllowUnauthenticatedCallers = parent.WindowsAuthentication.AllowAnonymousLogons;
            authenticator.ExtractGroupsForWindowsAccounts = parent.WindowsAuthentication.IncludeWindowsGroups;
            authenticator.IsClientAnonymous = false;
            authenticator.EncryptStateInServiceToken = isCookieMode;
            authenticator.IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
            authenticator.IssuedTokenCache = (ISecurityContextSecurityTokenCache)sctResolver;
            authenticator.IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            authenticator.ListenUri = recipientRequirement.ListenUri;
            authenticator.SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite;
            authenticator.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this);
            authenticator.SecurityStateEncoder = parent.SecureConversationAuthentication.SecurityStateEncoder;
            authenticator.KnownTypes = parent.SecureConversationAuthentication.SecurityContextClaimTypes;
            // if the SPNEGO is being done in mixed-mode, the nego blobs are from an anonymous client and so there size bound needs to be enforced.
            if (securityBindingElement is TransportSecurityBindingElement)
            {
                authenticator.MaxMessageSize = SecurityUtils.GetMaxNegotiationBufferSize(authenticator.IssuerBindingContext);
            }

            // local security quotas
            authenticator.MaximumCachedNegotiationState = localServiceSettings.MaxStatefulNegotiations;
            authenticator.NegotiationTimeout = localServiceSettings.NegotiationTimeout;
            authenticator.ServiceTokenLifetime = localServiceSettings.IssuedCookieLifetime;
            authenticator.MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations;

            // audit settings
            authenticator.AuditLogLocation = recipientRequirement.AuditLogLocation;
            authenticator.SuppressAuditFailure = recipientRequirement.SuppressAuditFailure;
            authenticator.MessageAuthenticationAuditLevel = recipientRequirement.MessageAuthenticationAuditLevel;
            return authenticator;
        }

        SecurityTokenAuthenticator CreateTlsnegoClientX509TokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement)
        {
            RecipientServiceModelSecurityTokenRequirement clientX509Requirement = new RecipientServiceModelSecurityTokenRequirement();
            clientX509Requirement.TokenType = SecurityTokenTypes.X509Certificate;
            clientX509Requirement.KeyUsage = SecurityKeyUsage.Signature;
            clientX509Requirement.ListenUri = recipientRequirement.ListenUri;
            clientX509Requirement.KeyType = SecurityKeyType.AsymmetricKey;
            clientX509Requirement.SecurityBindingElement = recipientRequirement.SecurityBindingElement;
            SecurityTokenResolver dummy;
            return this.CreateSecurityTokenAuthenticator(clientX509Requirement, out dummy);
        }

        SecurityTokenProvider CreateTlsnegoServerX509TokenProvider(RecipientServiceModelSecurityTokenRequirement recipientRequirement)
        {
            RecipientServiceModelSecurityTokenRequirement serverX509Requirement = new RecipientServiceModelSecurityTokenRequirement();
            serverX509Requirement.TokenType = SecurityTokenTypes.X509Certificate;
            serverX509Requirement.KeyUsage = SecurityKeyUsage.Exchange;
            serverX509Requirement.ListenUri = recipientRequirement.ListenUri;
            serverX509Requirement.KeyType = SecurityKeyType.AsymmetricKey;
            serverX509Requirement.SecurityBindingElement = recipientRequirement.SecurityBindingElement;
            return this.CreateSecurityTokenProvider(serverX509Requirement);
        }

        SecurityTokenAuthenticator CreateTlsnegoSecurityTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, bool requireClientCertificate, out SecurityTokenResolver sctResolver)
        {
            SecurityBindingElement securityBindingElement = recipientRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.TokenAuthenticatorRequiresSecurityBindingElement, recipientRequirement));
            }
            bool isCookieMode = !recipientRequirement.SupportSecurityContextCancellation;
            LocalServiceSecuritySettings localServiceSettings = securityBindingElement.LocalServiceSettings;
            sctResolver = new SecurityContextSecurityTokenResolver(localServiceSettings.MaxCachedCookies, true);

            TlsnegoTokenAuthenticator authenticator = new TlsnegoTokenAuthenticator();
            authenticator.IsClientAnonymous = !requireClientCertificate;
            if (requireClientCertificate)
            {
                authenticator.ClientTokenAuthenticator = this.CreateTlsnegoClientX509TokenAuthenticator(recipientRequirement);
                authenticator.MapCertificateToWindowsAccount = this.ServiceCredentials.ClientCertificate.Authentication.MapClientCertificateToWindowsAccount;
            }
            authenticator.EncryptStateInServiceToken = isCookieMode;
            authenticator.IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
            authenticator.IssuedTokenCache = (ISecurityContextSecurityTokenCache)sctResolver;
            authenticator.IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            authenticator.ListenUri = recipientRequirement.ListenUri;
            authenticator.SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite;
            authenticator.StandardsManager = SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this);
            authenticator.SecurityStateEncoder = parent.SecureConversationAuthentication.SecurityStateEncoder;
            authenticator.KnownTypes = parent.SecureConversationAuthentication.SecurityContextClaimTypes;
            authenticator.ServerTokenProvider = CreateTlsnegoServerX509TokenProvider(recipientRequirement);
            // local security quotas
            authenticator.MaximumCachedNegotiationState = localServiceSettings.MaxStatefulNegotiations;
            authenticator.NegotiationTimeout = localServiceSettings.NegotiationTimeout;
            authenticator.ServiceTokenLifetime = localServiceSettings.IssuedCookieLifetime;
            authenticator.MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations;
            // if the TLSNEGO is being done in mixed-mode, the nego blobs are from an anonymous client and so there size bound needs to be enforced.
            if (securityBindingElement is TransportSecurityBindingElement)
            {
                authenticator.MaxMessageSize = SecurityUtils.GetMaxNegotiationBufferSize(authenticator.IssuerBindingContext);
            }
            // audit settings
            authenticator.AuditLogLocation = recipientRequirement.AuditLogLocation;
            authenticator.SuppressAuditFailure = recipientRequirement.SuppressAuditFailure;
            authenticator.MessageAuthenticationAuditLevel = recipientRequirement.MessageAuthenticationAuditLevel;
            return authenticator;
        }

        X509SecurityTokenAuthenticator CreateClientX509TokenAuthenticator()
        {
            X509ClientCertificateAuthentication authentication = parent.ClientCertificate.Authentication;
            return new X509SecurityTokenAuthenticator(authentication.GetCertificateValidator(), authentication.MapClientCertificateToWindowsAccount, authentication.IncludeWindowsGroups);
        }

        SamlSecurityTokenAuthenticator CreateSamlTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (recipientRequirement == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("recipientRequirement");

            Collection<SecurityToken> outOfBandTokens = new Collection<SecurityToken>();
            if (parent.ServiceCertificate.Certificate != null)
            {
                outOfBandTokens.Add(new X509SecurityToken(parent.ServiceCertificate.Certificate));
            }
            List<SecurityTokenAuthenticator> supportingAuthenticators = new List<SecurityTokenAuthenticator>();
            if ((parent.IssuedTokenAuthentication.KnownCertificates != null) && (parent.IssuedTokenAuthentication.KnownCertificates.Count > 0))
            {
                for (int i = 0; i < parent.IssuedTokenAuthentication.KnownCertificates.Count; ++i)
                {
                    outOfBandTokens.Add(new X509SecurityToken(parent.IssuedTokenAuthentication.KnownCertificates[i]));
                }
            }

            X509CertificateValidator validator = parent.IssuedTokenAuthentication.GetCertificateValidator();
            supportingAuthenticators.Add(new X509SecurityTokenAuthenticator(validator));

            if (parent.IssuedTokenAuthentication.AllowUntrustedRsaIssuers)
            {
                supportingAuthenticators.Add(new RsaSecurityTokenAuthenticator());
            }

            outOfBandTokenResolver = (outOfBandTokens.Count > 0) ? SecurityTokenResolver.CreateDefaultSecurityTokenResolver(new ReadOnlyCollection<SecurityToken>(outOfBandTokens), false) : null;

            SamlSecurityTokenAuthenticator ssta;

            if ((recipientRequirement.SecurityBindingElement == null) || (recipientRequirement.SecurityBindingElement.LocalServiceSettings == null))
            {
                ssta = new SamlSecurityTokenAuthenticator(supportingAuthenticators);
            }
            else
            {
                ssta = new SamlSecurityTokenAuthenticator(supportingAuthenticators, recipientRequirement.SecurityBindingElement.LocalServiceSettings.MaxClockSkew);
            }

            // set audience uri restrictions
            ssta.AudienceUriMode = parent.IssuedTokenAuthentication.AudienceUriMode;
            IList<string> allowedAudienceUris = ssta.AllowedAudienceUris;
            if (parent.IssuedTokenAuthentication.AllowedAudienceUris != null)
            {
                for (int i = 0; i < parent.IssuedTokenAuthentication.AllowedAudienceUris.Count; i++)
                    allowedAudienceUris.Add(parent.IssuedTokenAuthentication.AllowedAudienceUris[i]);
            }

            if (recipientRequirement.ListenUri != null)
            {
                allowedAudienceUris.Add(recipientRequirement.ListenUri.AbsoluteUri);
            }

            return ssta;
        }

        X509SecurityTokenProvider CreateServerX509TokenProvider()
        {
            if (parent.ServiceCertificate.Certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ServiceCertificateNotProvidedOnServiceCredentials)));
            }
            SecurityUtils.EnsureCertificateCanDoKeyExchange(parent.ServiceCertificate.Certificate);
            return new ServiceX509SecurityTokenProvider(parent.ServiceCertificate.Certificate);
        }

        protected bool IsIssuedSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            return (requirement != null && requirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.IssuerAddressProperty));
        }

        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (tokenRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }
            string tokenType = tokenRequirement.TokenType;
            outOfBandTokenResolver = null;
            SecurityTokenAuthenticator result = null;
            if (tokenRequirement is InitiatorServiceModelSecurityTokenRequirement)
            {
                // this is the uncorrelated duplex case in which the server is asking for
                // an authenticator to validate its provisioned client certificate
                if (tokenType == SecurityTokenTypes.X509Certificate && tokenRequirement.KeyUsage == SecurityKeyUsage.Exchange)
                {
                    return new X509SecurityTokenAuthenticator(X509CertificateValidator.None, false);
                }
            }

            RecipientServiceModelSecurityTokenRequirement recipientRequirement = tokenRequirement as RecipientServiceModelSecurityTokenRequirement;
            if (recipientRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateAuthenticatorForRequirement, tokenRequirement)));
            }
            if (tokenType == SecurityTokenTypes.X509Certificate)
            {
                result = CreateClientX509TokenAuthenticator();
            }
            else if (tokenType == SecurityTokenTypes.Kerberos)
            {
                result = new KerberosSecurityTokenAuthenticatorWrapper(
                    new KerberosSecurityTokenAuthenticator(parent.WindowsAuthentication.IncludeWindowsGroups));
            }
            else if (tokenType == SecurityTokenTypes.UserName)
            {
                if (parent.UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.Windows)
                {
                    if (parent.UserNameAuthentication.CacheLogonTokens)
                    {
                        result = new WindowsUserNameCachingSecurityTokenAuthenticator(parent.UserNameAuthentication.IncludeWindowsGroups,
                            parent.UserNameAuthentication.MaxCachedLogonTokens, parent.UserNameAuthentication.CachedLogonTokenLifetime);
                    }
                    else
                    {
                        result = new WindowsUserNameSecurityTokenAuthenticator(parent.UserNameAuthentication.IncludeWindowsGroups);
                    }
                }
                else
                {
                    result = new CustomUserNameSecurityTokenAuthenticator(parent.UserNameAuthentication.GetUserNamePasswordValidator());
                }
            }
            else if (tokenType == SecurityTokenTypes.Rsa)
            {
                result = new RsaSecurityTokenAuthenticator();
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego)
            {
                result = CreateTlsnegoSecurityTokenAuthenticator(recipientRequirement, false, out outOfBandTokenResolver);
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.MutualSslnego)
            {
                result = CreateTlsnegoSecurityTokenAuthenticator(recipientRequirement, true, out outOfBandTokenResolver);
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.Spnego)
            {
                result = CreateSpnegoSecurityTokenAuthenticator(recipientRequirement, out outOfBandTokenResolver);
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.SecureConversation)
            {
                result = CreateSecureConversationTokenAuthenticator(recipientRequirement, false, out outOfBandTokenResolver);
            }
            else if ((tokenType == SecurityTokenTypes.Saml)
                || (tokenType == SecurityXXX2005Strings.SamlTokenType)
                || (tokenType == SecurityJan2004Strings.SamlUri)
                || (tokenType == null && IsIssuedSecurityTokenRequirement(recipientRequirement)))
            {
                result = CreateSamlTokenAuthenticator(recipientRequirement, out outOfBandTokenResolver);
            }

            if (result == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateAuthenticatorForRequirement, tokenRequirement)));

            return result;
        }

        SecurityTokenProvider CreateLocalSecurityTokenProvider(RecipientServiceModelSecurityTokenRequirement recipientRequirement)
        {
            string tokenType = recipientRequirement.TokenType;
            SecurityTokenProvider result = null;
            if (tokenType == SecurityTokenTypes.X509Certificate)
            {
                result = CreateServerX509TokenProvider();
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.SspiCredential)
            {
                // if Transport Security, AuthenicationSchemes.Basic will look at parent.UserNameAuthentication settings.
                AuthenticationSchemes authenticationScheme;
                bool authenticationSchemeIdentified = recipientRequirement.TryGetProperty<AuthenticationSchemes>(ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty, out authenticationScheme);
                if (authenticationSchemeIdentified &&
                    authenticationScheme.IsSet(AuthenticationSchemes.Basic) &&
                    authenticationScheme.IsNotSet(AuthenticationSchemes.Digest | AuthenticationSchemes.Ntlm | AuthenticationSchemes.Negotiate))
                {
                    // create security token provider even when basic and Anonymous are enabled.
                    result = new SspiSecurityTokenProvider(null, parent.UserNameAuthentication.IncludeWindowsGroups, false);
                }
                else
                {
                    if (authenticationSchemeIdentified &&
                       authenticationScheme.IsSet(AuthenticationSchemes.Basic) &&
                       parent.WindowsAuthentication.IncludeWindowsGroups != parent.UserNameAuthentication.IncludeWindowsGroups)
                    {
                        // Ensure there are no inconsistencies when Basic and (Digest and/or Ntlm and/or Negotiate) are both enabled
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenProviderIncludeWindowsGroupsInconsistent,
                            (AuthenticationSchemes)authenticationScheme - AuthenticationSchemes.Basic,
                            parent.UserNameAuthentication.IncludeWindowsGroups,
                            parent.WindowsAuthentication.IncludeWindowsGroups)));
                    }

                    result = new SspiSecurityTokenProvider(null, parent.WindowsAuthentication.IncludeWindowsGroups, parent.WindowsAuthentication.AllowAnonymousLogons);
                }
            }
            return result;
        }

        SecurityTokenProvider CreateUncorrelatedDuplexSecurityTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            string tokenType = initiatorRequirement.TokenType;
            SecurityTokenProvider result = null;
            if (tokenType == SecurityTokenTypes.X509Certificate)
            {
                SecurityKeyUsage keyUsage = initiatorRequirement.KeyUsage;
                if (keyUsage == SecurityKeyUsage.Exchange)
                {
                    if (parent.ClientCertificate.Certificate == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ClientCertificateNotProvidedOnServiceCredentials)));
                    }

                    result = new X509SecurityTokenProvider(parent.ClientCertificate.Certificate);
                }
                else
                {
                    // this is a request for the server's own cert for signing
                    result = CreateServerX509TokenProvider();
                }
            }
            return result;
        }

        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement requirement)
        {
            if (requirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requirement");
            }

            RecipientServiceModelSecurityTokenRequirement recipientRequirement = requirement as RecipientServiceModelSecurityTokenRequirement;
            SecurityTokenProvider result = null;
            if (recipientRequirement != null)
            {
                result = CreateLocalSecurityTokenProvider(recipientRequirement);
            }
            else if (requirement is InitiatorServiceModelSecurityTokenRequirement)
            {
                result = CreateUncorrelatedDuplexSecurityTokenProvider((InitiatorServiceModelSecurityTokenRequirement)requirement);
            }

            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateProviderForRequirement, requirement)));
            }
            return result;
        }

        public virtual EndpointIdentity GetIdentityOfSelf(SecurityTokenRequirement tokenRequirement)
        {
            if (tokenRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }
            if (tokenRequirement is RecipientServiceModelSecurityTokenRequirement)
            {
                string tokenType = tokenRequirement.TokenType;
                if (tokenType == SecurityTokenTypes.X509Certificate
                    || tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego
                    || tokenType == ServiceModelSecurityTokenTypes.MutualSslnego)
                {
                    if (parent.ServiceCertificate.Certificate != null)
                    {
                        return EndpointIdentity.CreateX509CertificateIdentity(parent.ServiceCertificate.Certificate);
                    }
                }
                else if (tokenType == SecurityTokenTypes.Kerberos || tokenType == ServiceModelSecurityTokenTypes.Spnego)
                {
                    return SecurityUtils.CreateWindowsIdentity();
                }
                else if (tokenType == ServiceModelSecurityTokenTypes.SecureConversation)
                {
                    SecurityBindingElement securityBindingElement = ((RecipientServiceModelSecurityTokenRequirement)tokenRequirement).SecureConversationSecurityBindingElement;
                    if (securityBindingElement != null)
                    {
                        if (securityBindingElement == null || securityBindingElement is TransportSecurityBindingElement)
                        {
                            return null;
                        }
                        SecurityTokenParameters bootstrapProtectionParameters = (securityBindingElement is SymmetricSecurityBindingElement) ? ((SymmetricSecurityBindingElement)securityBindingElement).ProtectionTokenParameters : ((AsymmetricSecurityBindingElement)securityBindingElement).RecipientTokenParameters;
                        SecurityTokenRequirement bootstrapRequirement = new RecipientServiceModelSecurityTokenRequirement();
                        bootstrapProtectionParameters.InitializeSecurityTokenRequirement(bootstrapRequirement);
                        return GetIdentityOfSelf(bootstrapRequirement);
                    }
                }
            }
            return null;
        }

        internal class KerberosSecurityTokenAuthenticatorWrapper : CommunicationObjectSecurityTokenAuthenticator
        {
            KerberosSecurityTokenAuthenticator innerAuthenticator;
            System.IdentityModel.SafeFreeCredentials credentialsHandle = null;

            public KerberosSecurityTokenAuthenticatorWrapper(KerberosSecurityTokenAuthenticator innerAuthenticator)
            {
                this.innerAuthenticator = innerAuthenticator;
            }

            public override void OnOpening()
            {
                base.OnOpening();
                if (this.credentialsHandle == null)
                {
                    this.credentialsHandle = SecurityUtils.GetCredentialsHandle("Kerberos", null, true);
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
                    this.credentialsHandle.Close();
                    this.credentialsHandle = null;
                }
            }

            protected override bool CanValidateTokenCore(SecurityToken token)
            {
                return this.innerAuthenticator.CanValidateToken(token);
            }

            internal ReadOnlyCollection<IAuthorizationPolicy> ValidateToken(SecurityToken token, ChannelBinding channelBinding, ExtendedProtectionPolicy protectionPolicy)
            {
                KerberosReceiverSecurityToken kerberosToken = (KerberosReceiverSecurityToken)token;
                kerberosToken.Initialize(this.credentialsHandle, channelBinding, protectionPolicy);
                return this.innerAuthenticator.ValidateToken(kerberosToken);
            }

            protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
            {
                return ValidateToken(token, null, null);
            }
        }
    }
}
