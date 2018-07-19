//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;

    class AcceleratedTokenProvider : 
        NegotiationTokenProvider<AcceleratedTokenProviderState>
    {
        internal const SecurityKeyEntropyMode defaultKeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
        SecurityKeyEntropyMode keyEntropyMode = defaultKeyEntropyMode;
        SecurityBindingElement bootstrapSecurityBindingElement;
        Uri privacyNoticeUri;
        int privacyNoticeVersion;
        ChannelParameterCollection channelParameters;
        SafeFreeCredentials credentialsHandle;
        bool ownCredentialsHandle;

        public AcceleratedTokenProvider(SafeFreeCredentials credentialsHandle)
            : base()
        {
            this.credentialsHandle = credentialsHandle;
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
                this.bootstrapSecurityBindingElement = (SecurityBindingElement) value.Clone();
            }
        }

        public Uri PrivacyNoticeUri
        {
            get { return this.privacyNoticeUri; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.privacyNoticeUri = value;
            }
        }

        public int PrivacyNoticeVersion
        {
            get { return this.privacyNoticeVersion; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.privacyNoticeVersion = value;
            }
        }

        public ChannelParameterCollection ChannelParameters
        {
            get { return this.channelParameters; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.channelParameters = value;
            }
        }



        // SC/Trust workshop change to turn off context
        protected override bool IsMultiLegNegotiation
        {
            get { return false; }
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

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.BootstrapSecurityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BootstrapSecurityBindingElementNotSet, this.GetType())));
            }
            base.OnOpen(timeout);
        }

        public override void OnOpening()
        {
            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                if (this.BootstrapSecurityBindingElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BootstrapSecurityBindingElementNotSet, this.GetType())));
                }
                this.credentialsHandle = SecurityUtils.GetCredentialsHandle(this.BootstrapSecurityBindingElement, this.IssuerBindingContext);
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

        protected override IChannelFactory<IRequestChannel> GetNegotiationChannelFactory(IChannelFactory<IRequestChannel> transportChannelFactory, ChannelBuilder channelBuilder)
        {
            ISecurityCapabilities securityCapabilities = this.bootstrapSecurityBindingElement.GetProperty<ISecurityCapabilities>(this.IssuerBindingContext);
            SecurityCredentialsManager securityCredentials = this.IssuerBindingContext.BindingParameters.Find<SecurityCredentialsManager>();
            if (securityCredentials == null)
            {
                securityCredentials = ClientCredentials.CreateDefaultCredentials();
            }

            this.bootstrapSecurityBindingElement.ReaderQuotas = this.IssuerBindingContext.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.bootstrapSecurityBindingElement.ReaderQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EncodingBindingElementDoesNotHandleReaderQuotas)));
            }

            TransportBindingElement transportBindingElement = this.IssuerBindingContext.RemainingBindingElements.Find<TransportBindingElement>();
            if (transportBindingElement != null)
                this.bootstrapSecurityBindingElement.MaxReceivedMessageSize = transportBindingElement.MaxReceivedMessageSize;

            SecurityProtocolFactory securityProtocolFactory = this.bootstrapSecurityBindingElement.CreateSecurityProtocolFactory<IRequestChannel>(this.IssuerBindingContext.Clone(), securityCredentials, false, this.IssuerBindingContext.Clone());
            MessageSecurityProtocolFactory soapBindingFactory = (securityProtocolFactory as MessageSecurityProtocolFactory);
            if (soapBindingFactory != null)
            {
                soapBindingFactory.ApplyConfidentiality = soapBindingFactory.ApplyIntegrity
                    = soapBindingFactory.RequireConfidentiality = soapBindingFactory.RequireIntegrity = true;

                MessagePartSpecification bodyPart = new MessagePartSpecification(true);
                soapBindingFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, RequestSecurityTokenAction);
                soapBindingFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(bodyPart, RequestSecurityTokenAction);
                soapBindingFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, RequestSecurityTokenResponseAction);
                soapBindingFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, RequestSecurityTokenResponseAction);
            }
            securityProtocolFactory.PrivacyNoticeUri = this.PrivacyNoticeUri;
            securityProtocolFactory.PrivacyNoticeVersion = this.PrivacyNoticeVersion;
            return new SecurityChannelFactory<IRequestChannel>(
                securityCapabilities, this.IssuerBindingContext, channelBuilder, securityProtocolFactory, transportChannelFactory);

        }

        protected override IRequestChannel CreateClientChannel(EndpointAddress target, Uri via)
        {
            IRequestChannel result = base.CreateClientChannel(target, via);
            if (this.channelParameters != null)
            {
                this.channelParameters.PropagateChannelParameters(result);
            }
            if (this.ownCredentialsHandle)
            {
                ChannelParameterCollection newParameters = result.GetProperty<ChannelParameterCollection>();
                if (newParameters != null)
                {
                    newParameters.Add(new SspiIssuanceChannelParameter(true, this.credentialsHandle));
                }
            }
            return result;
        }

        protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
        {
            return true;
        }

        protected override AcceleratedTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
        {
            byte[] keyEntropy;
            if (this.keyEntropyMode == SecurityKeyEntropyMode.ClientEntropy || this.keyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy)
            {
                keyEntropy = new byte[this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength / 8];
                CryptoHelper.FillRandomBytes(keyEntropy);
            }
            else
            {
                keyEntropy = null;
            }
            return new AcceleratedTokenProviderState(keyEntropy);
        }

        protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<AcceleratedTokenProviderState>(CreateNegotiationState(target, via, timeout), callback, state);
        }

        protected override AcceleratedTokenProviderState EndCreateNegotiationState(IAsyncResult result)
        {
            return CompletedAsyncResult<AcceleratedTokenProviderState>.End(result);
        }

        protected override BodyWriter GetFirstOutgoingMessageBody(AcceleratedTokenProviderState negotiationState, out MessageProperties messageProperties)
        {
            messageProperties = null;
            RequestSecurityToken rst = new RequestSecurityToken(this.StandardsManager);
            rst.Context = negotiationState.Context;
            rst.KeySize = this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength;
            rst.TokenType = this.SecurityContextTokenUri;
            byte[] requestorEntropy = negotiationState.GetRequestorEntropy();
            if (requestorEntropy != null)
            {
                rst.SetRequestorEntropy(requestorEntropy);
            }
            rst.MakeReadOnly();
            return rst;
        }

        protected override BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, AcceleratedTokenProviderState negotiationState)
        {
            ThrowIfFault(incomingMessage, this.TargetAddress);
            if (incomingMessage.Headers.Action != RequestSecurityTokenResponseAction.Value)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidActionForNegotiationMessage, incomingMessage.Headers.Action)), incomingMessage);
            }
            // get the claims corresponding to the server
            SecurityMessageProperty serverContextProperty = incomingMessage.Properties.Security;
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
            if (serverContextProperty != null && serverContextProperty.ServiceSecurityContext != null)
            {
                authorizationPolicies = serverContextProperty.ServiceSecurityContext.AuthorizationPolicies;
            }
            else
            {
                authorizationPolicies = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            RequestSecurityTokenResponse rstr = null;
            XmlDictionaryReader bodyReader = incomingMessage.GetReaderAtBodyContents();
            using (bodyReader)
            {
                if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                    rstr = RequestSecurityTokenResponse.CreateFrom(this.StandardsManager, bodyReader);
                else if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    RequestSecurityTokenResponseCollection rstrc = this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(bodyReader);

                    foreach (RequestSecurityTokenResponse rstrItem in rstrc.RstrCollection)
                    {
                        if (rstr != null)
                        {
                            // More than one RSTR is found. So throw an exception.
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MoreThanOneRSTRInRSTRC)));
                        }
                        rstr = rstrItem;
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }

                incomingMessage.ReadFromBodyContentsToEnd(bodyReader);
            }
            if (rstr.Context != negotiationState.Context)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.BadSecurityNegotiationContext)), incomingMessage);
            }
            byte[] keyEntropy = negotiationState.GetRequestorEntropy();
            GenericXmlSecurityToken serviceToken = rstr.GetIssuedToken(null, null, this.keyEntropyMode, keyEntropy, this.SecurityContextTokenUri, authorizationPolicies, this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength, false);
            negotiationState.SetServiceToken(serviceToken);
            return null;
        }
    }
}
