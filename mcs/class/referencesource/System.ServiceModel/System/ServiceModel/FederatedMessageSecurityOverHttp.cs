//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using System.ComponentModel;

    public sealed class FederatedMessageSecurityOverHttp
    {
        internal const bool DefaultNegotiateServiceCredential = true;
        internal const SecurityKeyType DefaultIssuedKeyType = SecurityKeyType.SymmetricKey;
        internal const bool DefaultEstablishSecurityContext = true;

        bool establishSecurityContext;
        bool negotiateServiceCredential;
        SecurityAlgorithmSuite algorithmSuite;
        EndpointAddress issuerAddress;
        EndpointAddress issuerMetadataAddress;
        Binding issuerBinding;
        Collection<ClaimTypeRequirement> claimTypeRequirements;
        string issuedTokenType;
        SecurityKeyType issuedKeyType;
        Collection<XmlElement> tokenRequestParameters;

        public FederatedMessageSecurityOverHttp()
        {
            negotiateServiceCredential = DefaultNegotiateServiceCredential;
            algorithmSuite = SecurityAlgorithmSuite.Default;
            issuedKeyType = DefaultIssuedKeyType;
            claimTypeRequirements = new Collection<ClaimTypeRequirement>();
            tokenRequestParameters = new Collection<XmlElement>();
            establishSecurityContext = DefaultEstablishSecurityContext;
        }

        public bool NegotiateServiceCredential
        {
            get { return this.negotiateServiceCredential; }
            set { this.negotiateServiceCredential = value; }
        }

        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get { return this.algorithmSuite; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.algorithmSuite = value;
            }
        }

        public bool EstablishSecurityContext
        {
            get
            {
                return this.establishSecurityContext;
            }
            set
            {
                this.establishSecurityContext = value;
            }
        }

        [DefaultValue(null)]
        public EndpointAddress IssuerAddress
        {
            get { return this.issuerAddress; }
            set { this.issuerAddress = value; }
        }

        [DefaultValue(null)]
        public EndpointAddress IssuerMetadataAddress
        {
            get { return this.issuerMetadataAddress; }
            set { this.issuerMetadataAddress = value; }
        }

        [DefaultValue(null)]
        public Binding IssuerBinding
        {
            get
            {
                return this.issuerBinding;
            }
            set
            {
                this.issuerBinding = value;
            }
        }

        [DefaultValue(null)]
        public string IssuedTokenType
        {
            get { return this.issuedTokenType; }
            set { this.issuedTokenType = value; }
        }

        public SecurityKeyType IssuedKeyType
        {
            get { return this.issuedKeyType; }
            set
            {
                if (!SecurityKeyTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.issuedKeyType = value;
            }
        }

        public Collection<ClaimTypeRequirement> ClaimTypeRequirements
        {
            get { return this.claimTypeRequirements; }
        }

        public Collection<XmlElement> TokenRequestParameters
        {
            get { return this.tokenRequestParameters; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityBindingElement CreateSecurityBindingElement(bool isSecureTransportMode,
                                                                     bool isReliableSession,
                                                                     MessageSecurityVersion version)
        {
            if ((this.IssuedKeyType == SecurityKeyType.BearerKey) &&
               (version.TrustVersion == TrustVersion.WSTrustFeb2005))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BearerKeyIncompatibleWithWSFederationHttpBinding)));
            }

            if (isReliableSession && !this.EstablishSecurityContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationRequiredByReliableSession)));
            }

            SecurityBindingElement result;
            bool emitBspAttributes = true;
            IssuedSecurityTokenParameters issuedParameters = new IssuedSecurityTokenParameters(this.IssuedTokenType, this.IssuerAddress, this.IssuerBinding);
            issuedParameters.IssuerMetadataAddress = this.issuerMetadataAddress;
            issuedParameters.KeyType = this.IssuedKeyType;
            if (this.IssuedKeyType == SecurityKeyType.SymmetricKey)
            {
                issuedParameters.KeySize = this.AlgorithmSuite.DefaultSymmetricKeyLength;
            }
            else
            {
                issuedParameters.KeySize = 0;
            }
            foreach (ClaimTypeRequirement c in this.claimTypeRequirements)
            {
                issuedParameters.ClaimTypeRequirements.Add(c);
            }
            foreach (XmlElement p in this.TokenRequestParameters)
            {
                issuedParameters.AdditionalRequestParameters.Add(p);
            }
            WSSecurityTokenSerializer versionSpecificSerializer = new WSSecurityTokenSerializer(version.SecurityVersion,
                                                                                                version.TrustVersion,
                                                                                                version.SecureConversationVersion,
                                                                                                emitBspAttributes,
                                                                                                null, null, null);
            SecurityStandardsManager versionSpecificStandardsManager = new SecurityStandardsManager(version, versionSpecificSerializer);
            issuedParameters.AddAlgorithmParameters(this.AlgorithmSuite, versionSpecificStandardsManager, this.issuedKeyType);

            SecurityBindingElement issuedTokenSecurity;
            if (isSecureTransportMode)
            {
                issuedTokenSecurity = SecurityBindingElement.CreateIssuedTokenOverTransportBindingElement(issuedParameters);
            }
            else
            {
                if (negotiateServiceCredential)
                {
                    // We should have passed 'true' as RequireCancelation to be consistent with other standard bindings.
                    // However, to limit the change for Orcas, we scope down to just newer version of WSSecurityPolicy.
                    issuedTokenSecurity = SecurityBindingElement.CreateIssuedTokenForSslBindingElement(issuedParameters, version.SecurityPolicyVersion != SecurityPolicyVersion.WSSecurityPolicy11);
                }
                else
                {
                    issuedTokenSecurity = SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement(issuedParameters);
                }
            }

            issuedTokenSecurity.MessageSecurityVersion = version;
            issuedTokenSecurity.DefaultAlgorithmSuite = this.AlgorithmSuite;

            if (this.EstablishSecurityContext)
            {
                result = SecurityBindingElement.CreateSecureConversationBindingElement(issuedTokenSecurity, true);
            }
            else
            {
                result = issuedTokenSecurity;
            }

            result.MessageSecurityVersion = version;
            result.DefaultAlgorithmSuite = this.AlgorithmSuite;
            result.IncludeTimestamp = true;

            if (!isReliableSession)
            {
                result.LocalServiceSettings.ReconnectTransportOnFailure = false;
                result.LocalClientSettings.ReconnectTransportOnFailure = false;
            }
            else
            {
                result.LocalServiceSettings.ReconnectTransportOnFailure = true;
                result.LocalClientSettings.ReconnectTransportOnFailure = true;
            }

            if (this.establishSecurityContext)
            {
                // issue the transition SCT for a short duration only
                issuedTokenSecurity.LocalServiceSettings.IssuedCookieLifetime = SpnegoTokenAuthenticator.defaultServerIssuedTransitionTokenLifetime;
            }

            return result;
        }

        internal static bool TryCreate(SecurityBindingElement sbe, bool isSecureTransportMode, bool isReliableSession, MessageSecurityVersion version, out FederatedMessageSecurityOverHttp messageSecurity)
        {
            Fx.Assert(null != sbe, string.Empty);

            messageSecurity = null;

            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

            if (!sbe.IncludeTimestamp)
                return false;

            if (sbe.SecurityHeaderLayout != SecurityProtocolFactory.defaultSecurityHeaderLayout)
                return false;

            bool emitBspAttributes = true;

            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            SecurityBindingElement bootstrapSecurity;

            bool establishSecurityContext = SecurityBindingElement.IsSecureConversationBinding(sbe, true, out bootstrapSecurity);
            bootstrapSecurity = establishSecurityContext ? bootstrapSecurity : sbe;

            if (isSecureTransportMode && !(bootstrapSecurity is TransportSecurityBindingElement))
                return false;

            bool negotiateServiceCredential = DefaultNegotiateServiceCredential;
            IssuedSecurityTokenParameters issuedTokenParameters;

            if (isSecureTransportMode)
            {
                if (!SecurityBindingElement.IsIssuedTokenOverTransportBinding(bootstrapSecurity, out issuedTokenParameters))
                    return false;
            }
            else
            {
                // We should have passed 'true' as RequireCancelation to be consistent with other standard bindings.
                // However, to limit the change for Orcas, we scope down to just newer version of WSSecurityPolicy.
                if (SecurityBindingElement.IsIssuedTokenForSslBinding(bootstrapSecurity, version.SecurityPolicyVersion != SecurityPolicyVersion.WSSecurityPolicy11, out issuedTokenParameters))
                    negotiateServiceCredential = true;
                else if (SecurityBindingElement.IsIssuedTokenForCertificateBinding(bootstrapSecurity, out issuedTokenParameters))
                    negotiateServiceCredential = false;
                else
                    return false;
            }

            if ((issuedTokenParameters.KeyType == SecurityKeyType.BearerKey) &&
               (version.TrustVersion == TrustVersion.WSTrustFeb2005))
            {
                return false;
            }

            Collection<XmlElement> nonAlgorithmRequestParameters;
            WSSecurityTokenSerializer versionSpecificSerializer = new WSSecurityTokenSerializer(version.SecurityVersion,
                                                                                                version.TrustVersion,
                                                                                                version.SecureConversationVersion,
                                                                                                emitBspAttributes,
                                                                                                null, null, null);
            SecurityStandardsManager versionSpecificStandardsManager = new SecurityStandardsManager(version, versionSpecificSerializer);

            if (!issuedTokenParameters.DoAlgorithmsMatch(sbe.DefaultAlgorithmSuite,
                                                         versionSpecificStandardsManager,
                                                         out nonAlgorithmRequestParameters))
            {
                return false;
            }
            messageSecurity = new FederatedMessageSecurityOverHttp();

            messageSecurity.AlgorithmSuite = sbe.DefaultAlgorithmSuite;
            messageSecurity.NegotiateServiceCredential = negotiateServiceCredential;
            messageSecurity.EstablishSecurityContext = establishSecurityContext;
            messageSecurity.IssuedTokenType = issuedTokenParameters.TokenType;
            messageSecurity.IssuerAddress = issuedTokenParameters.IssuerAddress;
            messageSecurity.IssuerBinding = issuedTokenParameters.IssuerBinding;
            messageSecurity.IssuerMetadataAddress = issuedTokenParameters.IssuerMetadataAddress;
            messageSecurity.IssuedKeyType = issuedTokenParameters.KeyType;
            foreach (ClaimTypeRequirement c in issuedTokenParameters.ClaimTypeRequirements)
            {
                messageSecurity.ClaimTypeRequirements.Add(c);
            }
            foreach (XmlElement p in nonAlgorithmRequestParameters)
            {
                messageSecurity.TokenRequestParameters.Add(p);
            }
            if (issuedTokenParameters.AlternativeIssuerEndpoints != null && issuedTokenParameters.AlternativeIssuerEndpoints.Count > 0)
            {
                return false;
            }
            return true;
        }

        internal bool InternalShouldSerialize()
        {
            return (this.ShouldSerializeAlgorithmSuite()
                || this.ShouldSerializeClaimTypeRequirements()
                || this.ShouldSerializeNegotiateServiceCredential()
                || this.ShouldSerializeEstablishSecurityContext()
                || this.ShouldSerializeIssuedKeyType()
                || this.ShouldSerializeTokenRequestParameters());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeAlgorithmSuite()
        {
            return (this.AlgorithmSuite != SecurityAlgorithmSuite.Default);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeClaimTypeRequirements()
        {
            return (this.ClaimTypeRequirements.Count > 0);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeNegotiateServiceCredential()
        {
            return (this.NegotiateServiceCredential != DefaultNegotiateServiceCredential);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeEstablishSecurityContext()
        {
            return (this.EstablishSecurityContext != DefaultEstablishSecurityContext);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeIssuedKeyType()
        {
            return (this.IssuedKeyType != DefaultIssuedKeyType);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTokenRequestParameters()
        {
            return (this.TokenRequestParameters.Count > 0);
        }

    }
}
