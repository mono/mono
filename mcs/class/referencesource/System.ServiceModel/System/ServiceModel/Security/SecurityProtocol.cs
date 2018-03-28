//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    // See SecurityProtocolFactory for contracts on subclasses etc

    // SecureOutgoingMessage and VerifyIncomingMessage take message as
    // ref parameters (instead of taking a message and returning a
    // message) to reduce the likelihood that a caller will forget to
    // do the rest of the processing with the modified message object.
    // Especially, on the sender-side, not sending the modified
    // message will result in sending it with an unencrypted body.
    // Correspondingly, the async versions have out parameters instead
    // of simple return values.
    abstract class SecurityProtocol : ISecurityCommunicationObject
    {
        static ReadOnlyCollection<SupportingTokenProviderSpecification> emptyTokenProviders;

        ICollection<SupportingTokenProviderSpecification> channelSupportingTokenProviderSpecification;
        Dictionary<string, ICollection<SupportingTokenProviderSpecification>> scopedSupportingTokenProviderSpecification;
        Dictionary<string, Collection<SupportingTokenProviderSpecification>> mergedSupportingTokenProvidersMap;
        SecurityProtocolFactory factory;
        EndpointAddress target;
        Uri via;
        WrapperSecurityCommunicationObject communicationObject;
        ChannelParameterCollection channelParameters;

        protected SecurityProtocol(SecurityProtocolFactory factory, EndpointAddress target, Uri via)
        {
            this.factory = factory;
            this.target = target;
            this.via = via;
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        protected WrapperSecurityCommunicationObject CommunicationObject
        {
            get { return this.communicationObject; }
        }

        public SecurityProtocolFactory SecurityProtocolFactory
        {
            get { return this.factory; }
        }

        public EndpointAddress Target
        {
            get { return this.target; }
        }

        public Uri Via
        {
            get { return this.via; }
        }

        public ICollection<SupportingTokenProviderSpecification> ChannelSupportingTokenProviderSpecification
        {
            get
            {
                return this.channelSupportingTokenProviderSpecification;
            }
        }

        public Dictionary<string, ICollection<SupportingTokenProviderSpecification>> ScopedSupportingTokenProviderSpecification
        {
            get
            {
                return this.scopedSupportingTokenProviderSpecification;
            }
        }

        static ReadOnlyCollection<SupportingTokenProviderSpecification> EmptyTokenProviders
        {
            get
            {
                if (emptyTokenProviders == null)
                {
                    emptyTokenProviders = new ReadOnlyCollection<SupportingTokenProviderSpecification>(new List<SupportingTokenProviderSpecification>());
                }
                return emptyTokenProviders;
            }
        }

        public ChannelParameterCollection ChannelParameters
        {
            get
            {
                return this.channelParameters;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.channelParameters = value;
            }
        }

        // ISecurityCommunicationObject members
        public TimeSpan DefaultOpenTimeout
        {
            get { return ServiceDefaults.OpenTimeout; }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get { return ServiceDefaults.CloseTimeout; }
        }

        public IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        public IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        public void OnClosed()
        {
        }

        public void OnClosing()
        {
        }

        public void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnFaulted()
        {
        }

        public void OnOpened()
        {
        }

        public void OnOpening()
        {
        }

        internal IList<SupportingTokenProviderSpecification> GetSupportingTokenProviders(string action)
        {
            if (this.mergedSupportingTokenProvidersMap != null && this.mergedSupportingTokenProvidersMap.Count > 0)
            {
                if (action != null && this.mergedSupportingTokenProvidersMap.ContainsKey(action))
                {
                    return this.mergedSupportingTokenProvidersMap[action];
                }
                else if (this.mergedSupportingTokenProvidersMap.ContainsKey(MessageHeaders.WildcardAction))
                {
                    return this.mergedSupportingTokenProvidersMap[MessageHeaders.WildcardAction];
                }
            }
            // return null if the token providers list is empty - this gets a perf benefit since calling Count is expensive for an empty
            // ReadOnlyCollection
            return (this.channelSupportingTokenProviderSpecification == EmptyTokenProviders) ? null : (IList<SupportingTokenProviderSpecification>)this.channelSupportingTokenProviderSpecification;
        }

        protected InitiatorServiceModelSecurityTokenRequirement CreateInitiatorSecurityTokenRequirement()
        {
            InitiatorServiceModelSecurityTokenRequirement requirement = new InitiatorServiceModelSecurityTokenRequirement();
            requirement.TargetAddress = this.Target;
            requirement.Via = this.via;
            requirement.SecurityBindingElement = this.factory.SecurityBindingElement;
            requirement.SecurityAlgorithmSuite = this.factory.OutgoingAlgorithmSuite;
            requirement.MessageSecurityVersion = this.factory.MessageSecurityVersion.SecurityTokenVersion;
            if (this.factory.PrivacyNoticeUri != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty] = this.factory.PrivacyNoticeUri;
            }
            if (this.channelParameters != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = this.channelParameters;
            }

            requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty] = this.factory.PrivacyNoticeVersion;
            
            return requirement;
        }

        InitiatorServiceModelSecurityTokenRequirement CreateInitiatorSecurityTokenRequirement(SecurityTokenParameters parameters, SecurityTokenAttachmentMode attachmentMode)
        {
            InitiatorServiceModelSecurityTokenRequirement requirement = CreateInitiatorSecurityTokenRequirement();
            parameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = SecurityKeyUsage.Signature;
            requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportingTokenAttachmentModeProperty] = attachmentMode;
            return requirement;
        }

        void AddSupportingTokenProviders(SupportingTokenParameters supportingTokenParameters, bool isOptional, IList<SupportingTokenProviderSpecification> providerSpecList)
        {
            for (int i = 0; i < supportingTokenParameters.Endorsing.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.Endorsing[i], SecurityTokenAttachmentMode.Endorsing);
                try
                {
                    if (isOptional)
                    {
                        requirement.IsOptionalToken = true;
                    }
                    System.IdentityModel.Selectors.SecurityTokenProvider provider = this.factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
                    if (provider == null)
                    {
                        continue;
                    }
                    SupportingTokenProviderSpecification providerSpec = new SupportingTokenProviderSpecification(provider, SecurityTokenAttachmentMode.Endorsing, supportingTokenParameters.Endorsing[i]);
                    providerSpecList.Add(providerSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
            for (int i = 0; i < supportingTokenParameters.SignedEndorsing.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.SignedEndorsing[i], SecurityTokenAttachmentMode.SignedEndorsing);
                try
                {
                    if (isOptional)
                    {
                        requirement.IsOptionalToken = true;
                    }
                    System.IdentityModel.Selectors.SecurityTokenProvider provider = this.factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
                    if (provider == null)
                    {
                        continue;
                    }
                    SupportingTokenProviderSpecification providerSpec = new SupportingTokenProviderSpecification(provider, SecurityTokenAttachmentMode.SignedEndorsing, supportingTokenParameters.SignedEndorsing[i]);
                    providerSpecList.Add(providerSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
            for (int i = 0; i < supportingTokenParameters.SignedEncrypted.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.SignedEncrypted[i], SecurityTokenAttachmentMode.SignedEncrypted);
                try
                {
                    if (isOptional)
                    {
                        requirement.IsOptionalToken = true;
                    }
                    System.IdentityModel.Selectors.SecurityTokenProvider provider = this.factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
                    if (provider == null)
                    {
                        continue;
                    }
                    SupportingTokenProviderSpecification providerSpec = new SupportingTokenProviderSpecification(provider, SecurityTokenAttachmentMode.SignedEncrypted, supportingTokenParameters.SignedEncrypted[i]);
                    providerSpecList.Add(providerSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
            for (int i = 0; i < supportingTokenParameters.Signed.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.Signed[i], SecurityTokenAttachmentMode.Signed);
                try
                {
                    if (isOptional)
                    {
                        requirement.IsOptionalToken = true;
                    }
                    System.IdentityModel.Selectors.SecurityTokenProvider provider = this.factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
                    if (provider == null)
                    {
                        continue;
                    }
                    SupportingTokenProviderSpecification providerSpec = new SupportingTokenProviderSpecification(provider, SecurityTokenAttachmentMode.Signed, supportingTokenParameters.Signed[i]);
                    providerSpecList.Add(providerSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
        }

        void MergeSupportingTokenProviders(TimeSpan timeout)
        {
            if (this.ScopedSupportingTokenProviderSpecification.Count == 0)
            {
                this.mergedSupportingTokenProvidersMap = null;
            }
            else
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.factory.ExpectSupportingTokens = true;
                this.mergedSupportingTokenProvidersMap = new Dictionary<string, Collection<SupportingTokenProviderSpecification>>();
                foreach (string action in this.ScopedSupportingTokenProviderSpecification.Keys)
                {
                    ICollection<SupportingTokenProviderSpecification> scopedProviders = this.ScopedSupportingTokenProviderSpecification[action];
                    if (scopedProviders == null || scopedProviders.Count == 0)
                    {
                        continue;
                    }
                    Collection<SupportingTokenProviderSpecification> mergedProviders = new Collection<SupportingTokenProviderSpecification>();
                    foreach (SupportingTokenProviderSpecification spec in this.channelSupportingTokenProviderSpecification)
                    {
                        mergedProviders.Add(spec);
                    }
                    foreach (SupportingTokenProviderSpecification spec in scopedProviders)
                    {
                        SecurityUtils.OpenTokenProviderIfRequired(spec.TokenProvider, timeoutHelper.RemainingTime());
                        if (spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing || spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)
                        {
                            if (spec.TokenParameters.RequireDerivedKeys && !spec.TokenParameters.HasAsymmetricKey)
                            {
                                this.factory.ExpectKeyDerivation = true;
                            }
                        }
                        mergedProviders.Add(spec);
                    }
                    this.mergedSupportingTokenProvidersMap.Add(action, mergedProviders);
                }
            }
        }

        public void Open(TimeSpan timeout)
        {
            this.communicationObject.Open(timeout);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(timeout, callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.communicationObject.EndOpen(result);
        }

        public virtual void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.factory.ActAsInitiator)
            {
                this.channelSupportingTokenProviderSpecification = new Collection<SupportingTokenProviderSpecification>();
                this.scopedSupportingTokenProviderSpecification = new Dictionary<string, ICollection<SupportingTokenProviderSpecification>>();

                AddSupportingTokenProviders(this.factory.SecurityBindingElement.EndpointSupportingTokenParameters, false, (IList<SupportingTokenProviderSpecification>)this.channelSupportingTokenProviderSpecification);
                AddSupportingTokenProviders(this.factory.SecurityBindingElement.OptionalEndpointSupportingTokenParameters, true, (IList<SupportingTokenProviderSpecification>)this.channelSupportingTokenProviderSpecification);
                foreach (string action in this.factory.SecurityBindingElement.OperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenProviderSpecification> providerSpecList = new Collection<SupportingTokenProviderSpecification>();
                    AddSupportingTokenProviders(this.factory.SecurityBindingElement.OperationSupportingTokenParameters[action], false, providerSpecList);
                    this.scopedSupportingTokenProviderSpecification.Add(action, providerSpecList);
                }
                foreach (string action in this.factory.SecurityBindingElement.OptionalOperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenProviderSpecification> providerSpecList;
                    ICollection<SupportingTokenProviderSpecification> existingList;
                    if (this.scopedSupportingTokenProviderSpecification.TryGetValue(action, out existingList))
                    {
                        providerSpecList = ((Collection<SupportingTokenProviderSpecification>)existingList);
                    }
                    else
                    {
                        providerSpecList = new Collection<SupportingTokenProviderSpecification>();
                        this.scopedSupportingTokenProviderSpecification.Add(action, providerSpecList);
                    }
                    this.AddSupportingTokenProviders(this.factory.SecurityBindingElement.OptionalOperationSupportingTokenParameters[action], true, providerSpecList);
                }

                if (!this.channelSupportingTokenProviderSpecification.IsReadOnly)
                {
                    if (this.channelSupportingTokenProviderSpecification.Count == 0)
                    {
                        this.channelSupportingTokenProviderSpecification = EmptyTokenProviders;
                    }
                    else
                    {
                        this.factory.ExpectSupportingTokens = true;
                        foreach (SupportingTokenProviderSpecification tokenProviderSpec in this.channelSupportingTokenProviderSpecification)
                        {
                            SecurityUtils.OpenTokenProviderIfRequired(tokenProviderSpec.TokenProvider, timeoutHelper.RemainingTime());
                            if (tokenProviderSpec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing || tokenProviderSpec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)
                            {
                                if (tokenProviderSpec.TokenParameters.RequireDerivedKeys && !tokenProviderSpec.TokenParameters.HasAsymmetricKey)
                                {
                                    this.factory.ExpectKeyDerivation = true;
                                }
                            }
                        }
                        this.channelSupportingTokenProviderSpecification =
                            new ReadOnlyCollection<SupportingTokenProviderSpecification>((Collection<SupportingTokenProviderSpecification>)this.channelSupportingTokenProviderSpecification);
                    }
                }
                // create a merged map of the per operation supporting tokens
                MergeSupportingTokenProviders(timeoutHelper.RemainingTime());
            }
        }

        public void Close(bool aborted, TimeSpan timeout)
        {
            if (aborted)
            {
                this.communicationObject.Abort();
            }
            else
            {
                this.communicationObject.Close(timeout);
            }
        }

        public virtual void OnAbort()
        {
            if (this.factory.ActAsInitiator)
            {
                foreach (SupportingTokenProviderSpecification spec in this.channelSupportingTokenProviderSpecification)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(spec.TokenProvider);
                }
                foreach (string action in this.scopedSupportingTokenProviderSpecification.Keys)
                {
                    ICollection<SupportingTokenProviderSpecification> supportingProviders = this.scopedSupportingTokenProviderSpecification[action];
                    foreach (SupportingTokenProviderSpecification spec in supportingProviders)
                    {
                        SecurityUtils.AbortTokenProviderIfRequired(spec.TokenProvider);
                    }
                }
            }
        }

        public virtual void OnClose(TimeSpan timeout)
        {
            if (this.factory.ActAsInitiator)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                foreach (SupportingTokenProviderSpecification spec in this.channelSupportingTokenProviderSpecification)
                {
                    SecurityUtils.CloseTokenProviderIfRequired(spec.TokenProvider, timeoutHelper.RemainingTime());
                }
                foreach (string action in this.scopedSupportingTokenProviderSpecification.Keys)
                {
                    ICollection<SupportingTokenProviderSpecification> supportingProviders = this.scopedSupportingTokenProviderSpecification[action];
                    foreach (SupportingTokenProviderSpecification spec in supportingProviders)
                    {
                        SecurityUtils.CloseTokenProviderIfRequired(spec.TokenProvider, timeoutHelper.RemainingTime());
                    }
                }
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        static void SetSecurityHeaderId(SendSecurityHeader securityHeader, Message message)
        {
            SecurityMessageProperty messageProperty = message.Properties.Security;
            if (messageProperty != null)
            {
                securityHeader.IdPrefix = messageProperty.SenderIdPrefix;
            }
        }

        void AddSupportingTokenSpecification(SecurityMessageProperty security, IList<SecurityToken> tokens, SecurityTokenAttachmentMode attachmentMode, IDictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return;
            }
            for (int i = 0; i < tokens.Count; ++i)
            {
                security.IncomingSupportingTokens.Add(new SupportingTokenSpecification(tokens[i], tokenPoliciesMapping[tokens[i]], attachmentMode));
            }
        }

        protected void AddSupportingTokenSpecification(SecurityMessageProperty security, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens, IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, IDictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            AddSupportingTokenSpecification(security, basicTokens, SecurityTokenAttachmentMode.SignedEncrypted, tokenPoliciesMapping);
            AddSupportingTokenSpecification(security, endorsingTokens, SecurityTokenAttachmentMode.Endorsing, tokenPoliciesMapping);
            AddSupportingTokenSpecification(security, signedEndorsingTokens, SecurityTokenAttachmentMode.SignedEndorsing, tokenPoliciesMapping);
            AddSupportingTokenSpecification(security, signedTokens, SecurityTokenAttachmentMode.Signed, tokenPoliciesMapping);
        }

        protected SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, SecurityProtocolFactory factory)
        {
            return CreateSendSecurityHeader(message, actor, factory, true);
        }

        protected SendSecurityHeader CreateSendSecurityHeaderForTransportProtocol(Message message, string actor, SecurityProtocolFactory factory)
        {
            return CreateSendSecurityHeader(message, actor, factory, false);
        }

        SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, SecurityProtocolFactory factory, bool requireMessageProtection)
        {
            MessageDirection transferDirection = factory.ActAsInitiator ? MessageDirection.Input : MessageDirection.Output;
            SendSecurityHeader sendSecurityHeader = factory.StandardsManager.CreateSendSecurityHeader(
                message,
                actor, true, false,
                factory.OutgoingAlgorithmSuite, transferDirection);
            sendSecurityHeader.Layout = factory.SecurityHeaderLayout;
            sendSecurityHeader.RequireMessageProtection = requireMessageProtection;
            SetSecurityHeaderId(sendSecurityHeader, message);
            if (factory.AddTimestamp)
            {
                sendSecurityHeader.AddTimestamp(factory.TimestampValidityDuration);
            }

            sendSecurityHeader.StreamBufferManager = factory.StreamBufferManager;
            return sendSecurityHeader;
        }

        internal void AddMessageSupportingTokens(Message message, ref IList<SupportingTokenSpecification> supportingTokens)
        {
            SecurityMessageProperty supportingTokensProperty = message.Properties.Security;
            if (supportingTokensProperty != null && supportingTokensProperty.HasOutgoingSupportingTokens)
            {
                if (supportingTokens == null)
                {
                    supportingTokens = new Collection<SupportingTokenSpecification>();
                }
                for (int i = 0; i < supportingTokensProperty.OutgoingSupportingTokens.Count; ++i)
                {
                    SupportingTokenSpecification spec = supportingTokensProperty.OutgoingSupportingTokens[i];
                    if (spec.SecurityTokenParameters == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SenderSideSupportingTokensMustSpecifySecurityTokenParameters)));
                    }
                    supportingTokens.Add(spec);
                }
            }
        }

        internal bool TryGetSupportingTokens(SecurityProtocolFactory factory, EndpointAddress target, Uri via, Message message, TimeSpan timeout, bool isBlockingCall, out IList<SupportingTokenSpecification> supportingTokens)
        {
            if (!factory.ActAsInitiator)
            {
                supportingTokens = null;
                return true;
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            supportingTokens = null;
            IList<SupportingTokenProviderSpecification> supportingTokenProviders = this.GetSupportingTokenProviders(message.Headers.Action);
            if (supportingTokenProviders != null && supportingTokenProviders.Count > 0)
            {
                // dont do anything if blocking is not allowed
                if (!isBlockingCall)
                {
                    return false;
                }

                supportingTokens = new Collection<SupportingTokenSpecification>();
                for (int i = 0; i < supportingTokenProviders.Count; ++i)
                {
                    SupportingTokenProviderSpecification spec = supportingTokenProviders[i];
                    SecurityToken supportingToken;
                    // The ProviderBackedSecurityToken was added in Win7 to allow KerberosRequestorSecurity 
                    // to pass a channel binding to InitializeSecurityContext.
                    if ((this is TransportSecurityProtocol) && (spec.TokenParameters is KerberosSecurityTokenParameters))
                    {
                        supportingToken = new ProviderBackedSecurityToken(spec.TokenProvider, timeoutHelper.RemainingTime());
                    }
                    else
                    {
                        supportingToken = spec.TokenProvider.GetToken(timeoutHelper.RemainingTime());
                    }

                    supportingTokens.Add(new SupportingTokenSpecification(supportingToken, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, spec.SecurityTokenAttachmentMode, spec.TokenParameters));
                }
            }
            // add any runtime supporting tokens
            AddMessageSupportingTokens(message, ref supportingTokens);

            return true;
        }

        protected IList<SupportingTokenAuthenticatorSpecification> GetSupportingTokenAuthenticatorsAndSetExpectationFlags(SecurityProtocolFactory factory, Message message,
            ReceiveSecurityHeader securityHeader)
        {
            if (factory.ActAsInitiator)
            {
                return null;
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            bool expectBasicTokens;
            bool expectSignedTokens;
            bool expectEndorsingTokens;
            IList<SupportingTokenAuthenticatorSpecification> authenticators = factory.GetSupportingTokenAuthenticators(message.Headers.Action,
                out expectSignedTokens, out expectBasicTokens, out expectEndorsingTokens);
            securityHeader.ExpectBasicTokens = expectBasicTokens;
            securityHeader.ExpectEndorsingTokens = expectEndorsingTokens;
            securityHeader.ExpectSignedTokens = expectSignedTokens;
            return authenticators;
        }


        protected ReadOnlyCollection<SecurityTokenResolver> MergeOutOfBandResolvers(IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators, ReadOnlyCollection<SecurityTokenResolver> primaryResolvers)
        {
            Collection<SecurityTokenResolver> outOfBandResolvers = null;
            if (supportingAuthenticators != null && supportingAuthenticators.Count > 0)
            {
                for (int i = 0; i < supportingAuthenticators.Count; ++i)
                {
                    if (supportingAuthenticators[i].TokenResolver != null)
                    {
                        outOfBandResolvers = outOfBandResolvers ?? new Collection<SecurityTokenResolver>();
                        outOfBandResolvers.Add(supportingAuthenticators[i].TokenResolver);
                    }
                }
            }
            if (outOfBandResolvers != null)
            {
                if (primaryResolvers != null)
                {
                    for (int i = 0; i < primaryResolvers.Count; ++i)
                    {
                        outOfBandResolvers.Insert(0, primaryResolvers[i]);
                    }
                }
                return new ReadOnlyCollection<SecurityTokenResolver>(outOfBandResolvers);
            }
            else
            {
                return primaryResolvers ?? EmptyReadOnlyCollection<SecurityTokenResolver>.Instance;
            }
        }


        protected void AddSupportingTokens(SendSecurityHeader securityHeader, IList<SupportingTokenSpecification> supportingTokens)
        {
            if (supportingTokens != null)
            {
                for (int i = 0; i < supportingTokens.Count; ++i)
                {
                    SecurityToken token = supportingTokens[i].SecurityToken;
                    SecurityTokenParameters tokenParameters = supportingTokens[i].SecurityTokenParameters;
                    switch (supportingTokens[i].SecurityTokenAttachmentMode)
                    {
                        case SecurityTokenAttachmentMode.Signed:
                            securityHeader.AddSignedSupportingToken(token, tokenParameters);
                            break;
                        case SecurityTokenAttachmentMode.Endorsing:
                            securityHeader.AddEndorsingSupportingToken(token, tokenParameters);
                            break;
                        case SecurityTokenAttachmentMode.SignedEncrypted:
                            securityHeader.AddBasicSupportingToken(token, tokenParameters);
                            break;
                        case SecurityTokenAttachmentMode.SignedEndorsing:
                            securityHeader.AddSignedEndorsingSupportingToken(token, tokenParameters);
                            break;
                        default:
                            Fx.Assert("Unknown token attachment mode " + supportingTokens[i].SecurityTokenAttachmentMode.ToString());
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnknownTokenAttachmentMode, supportingTokens[i].SecurityTokenAttachmentMode.ToString())));
                    }
                }
            }
        }

        public virtual IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            SecureOutgoingMessage(ref message, timeout);
            return new CompletedAsyncResult<Message>(message, callback, state);
        }

        public virtual IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityProtocolCorrelationState newCorrelationState = SecureOutgoingMessage(ref message, timeout, correlationState);
            return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, newCorrelationState, callback, state);
        }

        public virtual IAsyncResult BeginVerifyIncomingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            VerifyIncomingMessage(ref message, timeout);
            return new CompletedAsyncResult<Message>(message, callback, state);
        }

        public virtual IAsyncResult BeginVerifyIncomingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates, AsyncCallback callback, object state)
        {
            SecurityProtocolCorrelationState newCorrelationState = VerifyIncomingMessage(ref message, timeout, correlationStates);
            return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, newCorrelationState, callback, state);
        }

        public virtual void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
        {
            message = CompletedAsyncResult<Message>.End(result);
        }

        public virtual void EndSecureOutgoingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
        }

        public virtual void EndVerifyIncomingMessage(IAsyncResult result, out Message message)
        {
            message = CompletedAsyncResult<Message>.End(result);
        }

        public virtual void EndVerifyIncomingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
        }

        internal static SecurityToken GetToken(SecurityTokenProvider provider, EndpointAddress target, TimeSpan timeout)
        {
            if (provider == null)
            {
                // should this be an ArgumentNullException ?
                // throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("provider"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenProviderCannotGetTokensForTarget, target)));
            }

            SecurityToken token = null;

            try
            {
                token = provider.GetToken(timeout);
            }
            catch (SecurityTokenException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenProviderCannotGetTokensForTarget, target), exception));
            }
            catch (SecurityNegotiationException sne)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.TokenProviderCannotGetTokensForTarget, target), sne));
            }

            return token;
        }

        public abstract void SecureOutgoingMessage(ref Message message, TimeSpan timeout);

        // subclasses that offer correlation should override this version
        public virtual SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecureOutgoingMessage(ref message, timeout);
            return null;
        }

        protected virtual void OnOutgoingMessageSecured(Message securedMessage)
        {
            SecurityTraceRecordHelper.TraceOutgoingMessageSecured(this, securedMessage);
        }

        protected virtual void OnSecureOutgoingMessageFailure(Message message)
        {
            SecurityTraceRecordHelper.TraceSecureOutgoingMessageFailure(this, message);
        }

        public abstract void VerifyIncomingMessage(ref Message message, TimeSpan timeout);

        // subclasses that offer correlation should override this version
        public virtual SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout, params SecurityProtocolCorrelationState[] correlationStates)
        {
            VerifyIncomingMessage(ref message, timeout);         

            return null;
        }

        protected virtual void OnIncomingMessageVerified(Message verifiedMessage)
        {
            SecurityTraceRecordHelper.TraceIncomingMessageVerified(this, verifiedMessage);

            if (AuditLevel.Success == (this.factory.MessageAuthenticationAuditLevel & AuditLevel.Success))
            {
                SecurityAuditHelper.WriteMessageAuthenticationSuccessEvent(this.factory.AuditLogLocation,
                    this.factory.SuppressAuditFailure, verifiedMessage, verifiedMessage.Headers.To, verifiedMessage.Headers.Action,
                    SecurityUtils.GetIdentityNamesFromContext(verifiedMessage.Properties.Security.ServiceSecurityContext.AuthorizationContext));
            }
        }

        protected virtual void OnVerifyIncomingMessageFailure(Message message, Exception exception)
        {
            SecurityTraceRecordHelper.TraceVerifyIncomingMessageFailure(this, message);
            if (PerformanceCounters.PerformanceCountersEnabled && null != this.factory.ListenUri) //service side
            {
                if ((exception.GetType() == typeof(MessageSecurityException) || exception.GetType().IsSubclassOf(typeof(MessageSecurityException)))
                    || (exception.GetType() == typeof(SecurityTokenException) || exception.GetType().IsSubclassOf(typeof(SecurityTokenException))))
                {
                    PerformanceCounters.AuthenticationFailed(message, this.factory.ListenUri);
                }
            }

            if (AuditLevel.Failure == (this.factory.MessageAuthenticationAuditLevel & AuditLevel.Failure))
            {
                try
                {
                    SecurityMessageProperty security = message.Properties.Security;
                    string primaryIdentity;
                    if (security != null && security.ServiceSecurityContext != null)
                        primaryIdentity = SecurityUtils.GetIdentityNamesFromContext(security.ServiceSecurityContext.AuthorizationContext);
                    else
                        primaryIdentity = SecurityUtils.AnonymousIdentity.Name;

                    SecurityAuditHelper.WriteMessageAuthenticationFailureEvent(this.factory.AuditLogLocation,
                        this.factory.SuppressAuditFailure, message, message.Headers.To, message.Headers.Action, primaryIdentity, exception);
                }
#pragma warning suppress 56500
                catch (Exception auditException)
                {
                    if (Fx.IsFatal(auditException))
                        throw;

                    DiagnosticUtility.TraceHandledException(auditException, TraceEventType.Error);
                }
            }
        }

        protected abstract class GetSupportingTokensAsyncResult : AsyncResult
        {
            static AsyncCallback getSupportingTokensCallback = Fx.ThunkCallback(new AsyncCallback(GetSupportingTokenCallback));
            SecurityProtocol binding;
            Message message;
            IList<SupportingTokenSpecification> supportingTokens;
            int currentTokenProviderIndex = 0;
            IList<SupportingTokenProviderSpecification> supportingTokenProviders;
            TimeoutHelper timeoutHelper;

            public GetSupportingTokensAsyncResult(Message m, SecurityProtocol binding, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.message = m;
                this.binding = binding;
                this.timeoutHelper = new TimeoutHelper(timeout);
            }

            protected IList<SupportingTokenSpecification> SupportingTokens
            {
                get { return this.supportingTokens; }
            }

            protected abstract bool OnGetSupportingTokensDone(TimeSpan timeout);

            static void GetSupportingTokenCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                GetSupportingTokensAsyncResult self = (GetSupportingTokensAsyncResult)result.AsyncState;
                bool completeSelf;
                Exception completionException = null;
                try
                {
                    self.AddSupportingToken(result);
                    completeSelf = self.AddSupportingTokens();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            void AddSupportingToken(IAsyncResult result)
            {
                SupportingTokenProviderSpecification spec = supportingTokenProviders[this.currentTokenProviderIndex];
                SecurityTokenProvider.SecurityTokenAsyncResult securityTokenAsyncResult = result as SecurityTokenProvider.SecurityTokenAsyncResult;
                if (securityTokenAsyncResult != null)
                {
                    this.supportingTokens.Add(new SupportingTokenSpecification(SecurityTokenProvider.SecurityTokenAsyncResult.End(result), EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, spec.SecurityTokenAttachmentMode, spec.TokenParameters));
                }
                else
                {
                    this.supportingTokens.Add(new SupportingTokenSpecification(spec.TokenProvider.EndGetToken(result), EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, spec.SecurityTokenAttachmentMode, spec.TokenParameters));
                }

                ++this.currentTokenProviderIndex;
            }

            bool AddSupportingTokens()
            {
                while (this.currentTokenProviderIndex < supportingTokenProviders.Count)
                {
                    SupportingTokenProviderSpecification spec = supportingTokenProviders[this.currentTokenProviderIndex];
                    IAsyncResult result = null;
                    if ((this.binding is TransportSecurityProtocol) && (spec.TokenParameters is KerberosSecurityTokenParameters))
                    {
                        result = new SecurityTokenProvider.SecurityTokenAsyncResult(new ProviderBackedSecurityToken(spec.TokenProvider, timeoutHelper.RemainingTime()), null, this);
                    }
                    else
                    {
                        result = spec.TokenProvider.BeginGetToken(timeoutHelper.RemainingTime(), getSupportingTokensCallback, this);
                    }

                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.AddSupportingToken(result);
                }
                this.binding.AddMessageSupportingTokens(message, ref this.supportingTokens);
                return this.OnGetSupportingTokensDone(timeoutHelper.RemainingTime());
            }

            protected void Start()
            {
                bool completeSelf;
                if (this.binding.TryGetSupportingTokens(this.binding.SecurityProtocolFactory, this.binding.Target, this.binding.Via, this.message, timeoutHelper.RemainingTime(), false, out supportingTokens))
                {
                    completeSelf = this.OnGetSupportingTokensDone(timeoutHelper.RemainingTime());
                }
                else
                {
                    this.supportingTokens = new Collection<SupportingTokenSpecification>();
                    this.supportingTokenProviders = this.binding.GetSupportingTokenProviders(message.Headers.Action);
                    if (!(this.supportingTokenProviders != null && this.supportingTokenProviders.Count > 0))
                    {
                        Fx.Assert("There must be at least 1 supporting token provider");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException("There must be at least 1 supporting token provider"));
                    }
                    completeSelf = this.AddSupportingTokens();
                }
                if (completeSelf)
                {
                    base.Complete(true);
                }
            }
        }
    }
}
