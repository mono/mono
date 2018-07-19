//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    sealed class AcceleratedTokenAuthenticator : NegotiationTokenAuthenticator<NegotiationTokenAuthenticatorState>
    {
        SecurityBindingElement bootstrapSecurityBindingElement;
        SecurityKeyEntropyMode keyEntropyMode;
        bool shouldMatchRstWithEndpointFilter;
        bool preserveBootstrapTokens;

        public AcceleratedTokenAuthenticator()
            : base()
        {
            keyEntropyMode = AcceleratedTokenProvider.defaultKeyEntropyMode;
        }

        public bool PreserveBootstrapTokens
        {
            get
            {
                return this.preserveBootstrapTokens;
            }
            set
            {
                this.preserveBootstrapTokens = value;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenAction
        {
            get 
            {
                return this.StandardsManager.SecureConversationDriver.IssueAction;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenResponseAction
        {
            get 
            {
                return this.StandardsManager.SecureConversationDriver.IssueResponseAction;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenResponseFinalAction
        {
            get
            {
                return this.StandardsManager.SecureConversationDriver.IssueResponseAction;
            }
        }

        public SecurityBindingElement BootstrapSecurityBindingElement
        {
            get { return this.bootstrapSecurityBindingElement; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.bootstrapSecurityBindingElement = (SecurityBindingElement)value.Clone();
            }
        }

        public SecurityKeyEntropyMode KeyEntropyMode
        {
            get
            {
                return this.keyEntropyMode;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                SecurityKeyEntropyModeHelper.Validate(value);
                this.keyEntropyMode = value;
            }
        }

        protected override bool IsMultiLegNegotiation
        {
            get 
            {
                return false;
            }
        }

        protected override MessageFilter GetListenerFilter()
        {
            return new RstDirectFilter(this.StandardsManager, this);
        }

        protected override Binding GetNegotiationBinding(Binding binding)
        {
            CustomBinding customBinding = new CustomBinding(binding);
            customBinding.Elements.Insert(0, new AcceleratedTokenAuthenticatorBindingElement(this));
            return customBinding;
        }

        internal IChannelListener<TChannel> BuildNegotiationChannelListener<TChannel>(BindingContext context)
            where TChannel : class, IChannel
        {
            SecurityCredentialsManager securityCredentials = this.IssuerBindingContext.BindingParameters.Find<SecurityCredentialsManager>();
            if (securityCredentials == null)
            {
                securityCredentials = ServiceCredentials.CreateDefaultCredentials();
            }

            this.bootstrapSecurityBindingElement.ReaderQuotas = context.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.bootstrapSecurityBindingElement.ReaderQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EncodingBindingElementDoesNotHandleReaderQuotas)));
            }

            TransportBindingElement transportBindingElement = context.RemainingBindingElements.Find<TransportBindingElement>();
            if (transportBindingElement != null)
                this.bootstrapSecurityBindingElement.MaxReceivedMessageSize = transportBindingElement.MaxReceivedMessageSize;

            SecurityProtocolFactory securityProtocolFactory = this.bootstrapSecurityBindingElement.CreateSecurityProtocolFactory<TChannel>(this.IssuerBindingContext.Clone(), securityCredentials, true, this.IssuerBindingContext.Clone());
            MessageSecurityProtocolFactory soapBindingFactory = securityProtocolFactory as MessageSecurityProtocolFactory;
            if (soapBindingFactory != null)
            {
                soapBindingFactory.ApplyConfidentiality = soapBindingFactory.ApplyIntegrity
                    = soapBindingFactory.RequireConfidentiality = soapBindingFactory.RequireIntegrity = true;
                MessagePartSpecification bodyPart = new MessagePartSpecification(true);
                soapBindingFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, RequestSecurityTokenResponseAction);
                soapBindingFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, RequestSecurityTokenResponseAction);
                soapBindingFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, RequestSecurityTokenAction);
                soapBindingFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(bodyPart, RequestSecurityTokenAction);
            }

            SecurityChannelListener<TChannel> securityChannelListener = 
                new SecurityChannelListener<TChannel>(this.bootstrapSecurityBindingElement, context);
            securityChannelListener.SecurityProtocolFactory = securityProtocolFactory;
            // do not send back unsecured faults over composite duplex
            securityChannelListener.SendUnsecuredFaults = !SecurityUtils.IsCompositeDuplexBinding(context);

            ChannelBuilder channelBuilder = new ChannelBuilder(context, true);
            securityChannelListener.InitializeListener(channelBuilder);
            this.shouldMatchRstWithEndpointFilter = SecurityUtils.ShouldMatchRstWithEndpointFilter(this.bootstrapSecurityBindingElement);
            return securityChannelListener;
        }
        
        protected override BodyWriter ProcessRequestSecurityToken(Message request, RequestSecurityToken requestSecurityToken, out NegotiationTokenAuthenticatorState negotiationState)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            if (requestSecurityToken == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("requestSecurityToken", request);
            }
            try
            {
                if (requestSecurityToken.RequestType != null && requestSecurityToken.RequestType != this.StandardsManager.TrustDriver.RequestTypeIssue)
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.InvalidRstRequestType, requestSecurityToken.RequestType)), request);
                }
                if (requestSecurityToken.TokenType != null && requestSecurityToken.TokenType != this.SecurityContextTokenUri)
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.CannotIssueRstTokenType, requestSecurityToken.TokenType)), request);
                }
                
                EndpointAddress appliesTo;
                DataContractSerializer appliesToSerializer;
                string appliesToName;
                string appliesToNamespace;
                requestSecurityToken.GetAppliesToQName(out appliesToName, out appliesToNamespace);
                if (appliesToName == AddressingStrings.EndpointReference && appliesToNamespace == request.Version.Addressing.Namespace)
                {
                    if (request.Version.Addressing == AddressingVersion.WSAddressing10)
                    {
                        appliesToSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddress10), DataContractSerializerDefaults.MaxItemsInObjectGraph);
                        appliesTo = requestSecurityToken.GetAppliesTo<EndpointAddress10>(appliesToSerializer).ToEndpointAddress();
                    }
                    else if (request.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        appliesToSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddressAugust2004), DataContractSerializerDefaults.MaxItemsInObjectGraph);
                        appliesTo = requestSecurityToken.GetAppliesTo<EndpointAddressAugust2004>(appliesToSerializer).ToEndpointAddress();
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, request.Version.Addressing)));
                    }
                }
                else
                {
                    appliesTo = null;
                    appliesToSerializer = null;
                }
                if (this.shouldMatchRstWithEndpointFilter)
                {
                    SecurityUtils.MatchRstWithEndpointFilter(request, this.EndpointFilterTable, this.ListenUri);
                }
                int issuedKeySize;
                byte[] issuerEntropy;
                byte[] proofKey;
                SecurityToken proofToken;
                WSTrust.Driver.ProcessRstAndIssueKey(requestSecurityToken, null, this.KeyEntropyMode, this.SecurityAlgorithmSuite,
                    out issuedKeySize, out issuerEntropy, out proofKey, out proofToken);
                UniqueId contextId = SecurityUtils.GenerateUniqueId();
                string id = SecurityUtils.GenerateId();
                DateTime effectiveTime = DateTime.UtcNow;
                DateTime expirationTime = TimeoutHelper.Add(effectiveTime, this.ServiceTokenLifetime);
                // ensure that a SecurityContext is present in the message
                SecurityMessageProperty securityProperty = request.Properties.Security;
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
                if (securityProperty != null)
                    authorizationPolicies = SecuritySessionSecurityTokenAuthenticator.CreateSecureConversationPolicies(securityProperty, expirationTime);
                else
                    authorizationPolicies = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
                SecurityContextSecurityToken serviceToken = this.IssueSecurityContextToken(contextId, id, proofKey, effectiveTime, expirationTime, authorizationPolicies, 
                    this.EncryptStateInServiceToken);
                if (this.preserveBootstrapTokens)
                {
                    serviceToken.BootstrapMessageProperty = (securityProperty == null) ? null : (SecurityMessageProperty)securityProperty.CreateCopy();
                    SecurityUtils.ErasePasswordInUsernameTokenIfPresent(serviceToken.BootstrapMessageProperty);
                }
                RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse(this.StandardsManager);
                rstr.Context = requestSecurityToken.Context;
                rstr.KeySize = issuedKeySize;
                rstr.RequestedUnattachedReference = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(serviceToken, SecurityTokenReferenceStyle.External);
                rstr.RequestedAttachedReference = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(serviceToken, SecurityTokenReferenceStyle.Internal);
                rstr.TokenType = this.SecurityContextTokenUri;
                rstr.RequestedSecurityToken = serviceToken;
                if (issuerEntropy != null)
                {
                    rstr.SetIssuerEntropy(issuerEntropy);
                    rstr.ComputeKey = true;
                }
                if (proofToken != null)
                {
                    rstr.RequestedProofToken = proofToken;
                }
                if (appliesTo != null)
                {
                    if (request.Version.Addressing == AddressingVersion.WSAddressing10)
                    {
                        rstr.SetAppliesTo<EndpointAddress10>(EndpointAddress10.FromEndpointAddress(appliesTo), appliesToSerializer);
                    }
                    else if (request.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        rstr.SetAppliesTo<EndpointAddressAugust2004>(EndpointAddressAugust2004.FromEndpointAddress(appliesTo), appliesToSerializer);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, request.Version.Addressing)));
                    }
                }
                rstr.MakeReadOnly();
                negotiationState = new NegotiationTokenAuthenticatorState();
                negotiationState.SetServiceToken(serviceToken);

                if (this.StandardsManager.MessageSecurityVersion.SecureConversationVersion == SecureConversationVersion.WSSecureConversationFeb2005)
                    return rstr;
                else if (this.StandardsManager.MessageSecurityVersion.SecureConversationVersion == SecureConversationVersion.WSSecureConversation13)
                {
                    List<RequestSecurityTokenResponse> rstrList = new List<RequestSecurityTokenResponse>(1);
                    rstrList.Add(rstr);
                    RequestSecurityTokenResponseCollection rstrCollection = new RequestSecurityTokenResponseCollection(rstrList, this.StandardsManager);
                    return rstrCollection;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
            }
            finally
            {
                SecuritySessionSecurityTokenAuthenticator.RemoveCachedTokensIfRequired(request.Properties.Security);
            }
        }

        protected override BodyWriter ProcessRequestSecurityTokenResponse(NegotiationTokenAuthenticatorState negotiationState, Message request, RequestSecurityTokenResponse requestSecurityTokenResponse)
        {
            throw TraceUtility.ThrowHelperWarning(new NotSupportedException(SR.GetString(SR.RstDirectDoesNotExpectRstr)), request);
        }

        class RstDirectFilter : HeaderFilter
        {
            SecurityStandardsManager standardsManager;
            AcceleratedTokenAuthenticator authenticator;

            public RstDirectFilter(SecurityStandardsManager standardsManager, AcceleratedTokenAuthenticator authenticator)
            {
                this.standardsManager = standardsManager;
                this.authenticator = authenticator;
            }

            public override bool Match(Message message)
            {
                if (message.Headers.Action == this.authenticator.RequestSecurityTokenAction.Value)
                {
                    return this.standardsManager.DoesMessageContainSecurityHeader(message);
                }
                else
                {
                    return false;
                }
            }
        }
    }

    class AcceleratedTokenAuthenticatorBindingElement : BindingElement
    {
        AcceleratedTokenAuthenticator authenticator;

        public AcceleratedTokenAuthenticatorBindingElement(AcceleratedTokenAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            return authenticator.BuildNegotiationChannelListener<TChannel>(context);
        }

        public override BindingElement Clone()
        {
            return new AcceleratedTokenAuthenticatorBindingElement(this.authenticator);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)authenticator.BootstrapSecurityBindingElement.GetProperty<ISecurityCapabilities>(context);
            }

            return context.GetInnerProperty<T>();
        }
    }
}
