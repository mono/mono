//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;

    sealed class AsymmetricSecurityProtocol : MessageSecurityProtocol
    {
        SecurityTokenAuthenticator initiatorAsymmetricTokenAuthenticator;
        SecurityTokenProvider initiatorAsymmetricTokenProvider;
        SecurityTokenProvider initiatorCryptoTokenProvider;

        public AsymmetricSecurityProtocol(AsymmetricSecurityProtocolFactory factory,
           EndpointAddress target, Uri via)
            : base(factory, target, via)
        {
        }

        protected override bool DoAutomaticEncryptionMatch
        {
            get { return false; }
        }

        AsymmetricSecurityProtocolFactory Factory
        {
            get { return (AsymmetricSecurityProtocolFactory)base.MessageSecurityProtocolFactory; }
        }

        public SecurityTokenProvider InitiatorCryptoTokenProvider
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorCryptoTokenProvider;
            }
        }

        public SecurityTokenAuthenticator InitiatorAsymmetricTokenAuthenticator
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorAsymmetricTokenAuthenticator;
            }
        }

        public SecurityTokenProvider InitiatorAsymmetricTokenProvider
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorAsymmetricTokenProvider;
            }
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            if (this.Factory.ActAsInitiator)
            {
                if (this.Factory.ApplyIntegrity)
                {
                    InitiatorServiceModelSecurityTokenRequirement requirement = CreateInitiatorSecurityTokenRequirement();
                    this.Factory.CryptoTokenParameters.InitializeSecurityTokenRequirement(requirement);
                    requirement.KeyUsage = SecurityKeyUsage.Signature;
                    requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
                    this.initiatorCryptoTokenProvider = this.Factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
                    SecurityUtils.OpenTokenProviderIfRequired(this.initiatorCryptoTokenProvider, timeoutHelper.RemainingTime());
                }
                if (this.Factory.RequireIntegrity || this.Factory.ApplyConfidentiality)
                {
                    InitiatorServiceModelSecurityTokenRequirement providerRequirement = CreateInitiatorSecurityTokenRequirement();
                    this.Factory.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(providerRequirement);
                    providerRequirement.KeyUsage = SecurityKeyUsage.Exchange;
                    providerRequirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = (this.Factory.ApplyConfidentiality) ? MessageDirection.Output : MessageDirection.Input;
                    this.initiatorAsymmetricTokenProvider = this.Factory.SecurityTokenManager.CreateSecurityTokenProvider(providerRequirement);
                    SecurityUtils.OpenTokenProviderIfRequired(this.initiatorAsymmetricTokenProvider, timeoutHelper.RemainingTime());

                    InitiatorServiceModelSecurityTokenRequirement authenticatorRequirement = CreateInitiatorSecurityTokenRequirement();
                    this.Factory.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(authenticatorRequirement);
                    authenticatorRequirement.IsOutOfBandToken = !this.Factory.AllowSerializedSigningTokenOnReply;
                    authenticatorRequirement.KeyUsage = SecurityKeyUsage.Exchange;
                    authenticatorRequirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = (this.Factory.ApplyConfidentiality) ? MessageDirection.Output : MessageDirection.Input;
                    // Create authenticator (we dont support out of band resolvers on the client side
                    SecurityTokenResolver outOfBandTokenResolver;
                    this.initiatorAsymmetricTokenAuthenticator = this.Factory.SecurityTokenManager.CreateSecurityTokenAuthenticator(authenticatorRequirement, out outOfBandTokenResolver);
                    SecurityUtils.OpenTokenAuthenticatorIfRequired(this.initiatorAsymmetricTokenAuthenticator, timeoutHelper.RemainingTime());
                }
            }
        }

        public override void OnAbort()
        {
            if (this.Factory.ActAsInitiator)
            {
                if (this.initiatorCryptoTokenProvider != null)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(this.initiatorCryptoTokenProvider);
                }
                if (this.initiatorAsymmetricTokenProvider != null)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(this.initiatorAsymmetricTokenProvider);
                }
                if (this.initiatorAsymmetricTokenAuthenticator != null)
                {
                    SecurityUtils.AbortTokenAuthenticatorIfRequired(this.initiatorAsymmetricTokenAuthenticator);
                }
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.Factory.ActAsInitiator)
            {
                if (this.initiatorCryptoTokenProvider != null)
                {
                    SecurityUtils.CloseTokenProviderIfRequired(this.initiatorCryptoTokenProvider, timeoutHelper.RemainingTime());
                }
                if (this.initiatorAsymmetricTokenProvider != null)
                {
                    SecurityUtils.CloseTokenProviderIfRequired(this.initiatorAsymmetricTokenProvider, timeoutHelper.RemainingTime());
                }
                if (this.initiatorAsymmetricTokenAuthenticator != null)
                {
                    SecurityUtils.CloseTokenAuthenticatorIfRequired(this.initiatorAsymmetricTokenAuthenticator, timeoutHelper.RemainingTime());
                }
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken encryptingToken;
            SecurityToken signingToken;
            SecurityProtocolCorrelationState newCorrelationState;
            IList<SupportingTokenSpecification> supportingTokens;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, false, timeoutHelper.RemainingTime(), out encryptingToken, out signingToken, out supportingTokens, out newCorrelationState))
            {
                SetUpDelayedSecurityExecution(ref message, encryptingToken, signingToken, supportingTokens, GetSignatureConfirmationCorrelationState(correlationState, newCorrelationState));
                return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, newCorrelationState, callback, state);
            }
            else
            {
                if (this.Factory.ActAsInitiator == false)
                {
                    Fx.Assert("Unexpected code path for server security application");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SendingOutgoingmessageOnRecipient)));
                }
                AsymmetricSecurityProtocolFactory factory = this.Factory;
                SecurityTokenProvider encProvider = factory.ApplyConfidentiality ? this.initiatorAsymmetricTokenProvider : null;
                SecurityTokenProvider sigProvider = factory.ApplyIntegrity ? this.initiatorCryptoTokenProvider : null;
                return new SecureOutgoingMessageAsyncResult(message, this,
                   encProvider, sigProvider, factory.ApplyConfidentiality, this.initiatorAsymmetricTokenAuthenticator, correlationState, timeoutHelper.RemainingTime(), callback, state);
            }
        }

        protected override void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result is CompletedAsyncResult<Message, SecurityProtocolCorrelationState>)
            {
                message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
            }
            else
            {
                message = SecureOutgoingMessageAsyncResult.End(result, out newCorrelationState);
            }
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken encryptingToken;
            SecurityToken signingToken;
            SecurityProtocolCorrelationState newCorrelationState;
            IList<SupportingTokenSpecification> supportingTokens;
            TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, true, timeout, out encryptingToken, out signingToken, out supportingTokens, out newCorrelationState);
            SetUpDelayedSecurityExecution(ref message, encryptingToken, signingToken, supportingTokens, GetSignatureConfirmationCorrelationState(correlationState, newCorrelationState));
            return newCorrelationState;
        }

        void SetUpDelayedSecurityExecution(ref Message message, SecurityToken encryptingToken, SecurityToken signingToken,
            IList<SupportingTokenSpecification> supportingTokens, SecurityProtocolCorrelationState correlationState)
        {
            AsymmetricSecurityProtocolFactory factory = this.Factory;
            string actor = string.Empty;
            SendSecurityHeader securityHeader = ConfigureSendSecurityHeader(message, actor, supportingTokens, correlationState);
            SecurityTokenParameters signingTokenParameters = (this.Factory.ActAsInitiator) ? this.Factory.CryptoTokenParameters : this.Factory.AsymmetricTokenParameters;
            SecurityTokenParameters encryptionTokenParameters = (this.Factory.ActAsInitiator) ? this.Factory.AsymmetricTokenParameters : this.Factory.CryptoTokenParameters;
            if (this.Factory.ApplyIntegrity || securityHeader.HasSignedTokens)
            {
                if (!this.Factory.ApplyIntegrity)
                {
                    securityHeader.SignatureParts = MessagePartSpecification.NoParts;
                }
                securityHeader.SetSigningToken(signingToken, signingTokenParameters);
            }
            if (Factory.ApplyConfidentiality || securityHeader.HasEncryptedTokens)
            {
                if (!this.Factory.ApplyConfidentiality)
                {
                    securityHeader.EncryptionParts = MessagePartSpecification.NoParts;
                }
                securityHeader.SetEncryptionToken(encryptingToken, encryptionTokenParameters);
            }
            message = securityHeader.SetupExecution();
        }

        void AttachRecipientSecurityProperty(Message message, SecurityToken initiatorToken, SecurityToken recipientToken, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens,
           IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            SecurityMessageProperty security = SecurityMessageProperty.GetOrCreate(message);
            security.InitiatorToken = (initiatorToken != null) ? new SecurityTokenSpecification(initiatorToken, tokenPoliciesMapping[initiatorToken]) : null;
            security.RecipientToken = (recipientToken != null) ? new SecurityTokenSpecification(recipientToken, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance) : null;
            AddSupportingTokenSpecification(security, basicTokens, endorsingTokens, signedEndorsingTokens, signedTokens, tokenPoliciesMapping);
            security.ServiceSecurityContext = new ServiceSecurityContext(security.GetInitiatorTokenAuthorizationPolicies());
        }

        void DoIdentityCheckAndAttachInitiatorSecurityProperty(Message message, SecurityToken initiatorToken, SecurityToken recipientToken, ReadOnlyCollection<IAuthorizationPolicy> recipientTokenPolicies)
        {
            AuthorizationContext recipientAuthorizationContext = base.EnsureIncomingIdentity(message, recipientToken, recipientTokenPolicies);
            SecurityMessageProperty security = SecurityMessageProperty.GetOrCreate(message);
            security.InitiatorToken = (initiatorToken != null) ? new SecurityTokenSpecification(initiatorToken, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance) : null;
            security.RecipientToken = new SecurityTokenSpecification(recipientToken, recipientTokenPolicies);
            security.ServiceSecurityContext = new ServiceSecurityContext(recipientAuthorizationContext, recipientTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
        }

        protected override SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            AsymmetricSecurityProtocolFactory factory = this.Factory;
            IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            ReceiveSecurityHeader securityHeader = ConfigureReceiveSecurityHeader(message, string.Empty, correlationStates, out supportingAuthenticators);
            SecurityToken requiredReplySigningToken = null;
            if (factory.ActAsInitiator)
            {
                SecurityToken encryptionToken = null;
                SecurityToken receiverToken = null;
                if (factory.RequireIntegrity)
                {
                    receiverToken = GetToken(this.initiatorAsymmetricTokenProvider, null, timeoutHelper.RemainingTime());
                    requiredReplySigningToken = receiverToken;
                }
                if (factory.RequireConfidentiality)
                {
                    encryptionToken = GetCorrelationToken(correlationStates);
                    if (!SecurityUtils.HasSymmetricSecurityKey(encryptionToken))
                    {
                        securityHeader.WrappedKeySecurityTokenAuthenticator = this.Factory.WrappedKeySecurityTokenAuthenticator;
                    }
                }
                SecurityTokenAuthenticator primaryTokenAuthenticator;
                if (factory.AllowSerializedSigningTokenOnReply)
                {
                    primaryTokenAuthenticator = this.initiatorAsymmetricTokenAuthenticator;
                    requiredReplySigningToken = null;
                }
                else
                {
                    primaryTokenAuthenticator = null;
                }

                securityHeader.ConfigureAsymmetricBindingClientReceiveHeader(receiverToken,
                    factory.AsymmetricTokenParameters, encryptionToken, factory.CryptoTokenParameters,
                    primaryTokenAuthenticator);
            }
            else
            {
                SecurityToken wrappingToken;
                if (this.Factory.RecipientAsymmetricTokenProvider != null && this.Factory.RequireConfidentiality)
                {
                    wrappingToken = GetToken(factory.RecipientAsymmetricTokenProvider, null, timeoutHelper.RemainingTime());
                }
                else
                {
                    wrappingToken = null;
                }
                securityHeader.ConfigureAsymmetricBindingServerReceiveHeader(this.Factory.RecipientCryptoTokenAuthenticator,
                    this.Factory.CryptoTokenParameters, wrappingToken, this.Factory.AsymmetricTokenParameters, supportingAuthenticators);
                securityHeader.WrappedKeySecurityTokenAuthenticator = this.Factory.WrappedKeySecurityTokenAuthenticator;

                securityHeader.ConfigureOutOfBandTokenResolver(MergeOutOfBandResolvers(supportingAuthenticators, this.Factory.RecipientOutOfBandTokenResolverList));
            }

            ProcessSecurityHeader(securityHeader, ref message, requiredReplySigningToken, timeoutHelper.RemainingTime(), correlationStates);
            SecurityToken signingToken = securityHeader.SignatureToken;
            SecurityToken encryptingToken = securityHeader.EncryptionToken;
            if (factory.RequireIntegrity)
            {
                if (factory.ActAsInitiator)
                {
                    ReadOnlyCollection<IAuthorizationPolicy> signingTokenPolicies = this.initiatorAsymmetricTokenAuthenticator.ValidateToken(signingToken);
                    EnsureNonWrappedToken(signingToken, message);
                    DoIdentityCheckAndAttachInitiatorSecurityProperty(message, encryptingToken, signingToken, signingTokenPolicies);
                }
                else
                {
                    EnsureNonWrappedToken(signingToken, message);
                    AttachRecipientSecurityProperty(message, signingToken, encryptingToken, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens,
                        securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
                }
            }

            return GetCorrelationState(signingToken, securityHeader);
        }

        bool TryGetTokenSynchronouslyForOutgoingSecurity(Message message, SecurityProtocolCorrelationState correlationState, bool isBlockingCall, TimeSpan timeout,
            out SecurityToken encryptingToken, out SecurityToken signingToken, out IList<SupportingTokenSpecification> supportingTokens, out SecurityProtocolCorrelationState newCorrelationState)
        {
            AsymmetricSecurityProtocolFactory factory = this.Factory;
            encryptingToken = null;
            signingToken = null;
            newCorrelationState = null;
            supportingTokens = null;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (factory.ActAsInitiator)
            {
                if (!isBlockingCall || !TryGetSupportingTokens(this.Factory, this.Target, this.Via, message, timeoutHelper.RemainingTime(), isBlockingCall, out supportingTokens))
                {
                    return false;
                }
                if (factory.ApplyConfidentiality)
                {
                    encryptingToken = GetTokenAndEnsureOutgoingIdentity(this.initiatorAsymmetricTokenProvider, true, timeoutHelper.RemainingTime(), this.initiatorAsymmetricTokenAuthenticator);
                }
                if (factory.ApplyIntegrity)
                {
                    signingToken = GetToken(this.initiatorCryptoTokenProvider, this.Target, timeoutHelper.RemainingTime());
                    newCorrelationState = GetCorrelationState(signingToken);
                }
            }
            else
            {
                if (factory.ApplyConfidentiality)
                {
                    encryptingToken = GetCorrelationToken(correlationState);
                }
                if (factory.ApplyIntegrity)
                {
                    signingToken = GetToken(factory.RecipientAsymmetricTokenProvider, null, timeoutHelper.RemainingTime());
                }
            }
            return true;
        }

        sealed class SecureOutgoingMessageAsyncResult : GetTwoTokensAndSetUpSecurityAsyncResult
        {
            public SecureOutgoingMessageAsyncResult(Message m, AsymmetricSecurityProtocol binding,
                SecurityTokenProvider primaryProvider, SecurityTokenProvider secondaryProvider, bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator,
                SecurityProtocolCorrelationState correlationState, TimeSpan timeout, AsyncCallback callback, object state)
                : base(m, binding, primaryProvider, secondaryProvider, doIdentityChecks, identityCheckAuthenticator, correlationState, timeout, callback, state)
            {
                Start();
            }

            protected override void OnBothGetTokenCallsDone(ref Message message, SecurityToken primaryToken, SecurityToken secondaryToken, TimeSpan timeout)
            {
                AsymmetricSecurityProtocol binding = (AsymmetricSecurityProtocol)this.Binding;
                if (secondaryToken != null)
                    this.SetCorrelationToken(secondaryToken);
                binding.SetUpDelayedSecurityExecution(ref message, primaryToken, secondaryToken, this.SupportingTokens, binding.GetSignatureConfirmationCorrelationState(OldCorrelationState, NewCorrelationState));
            }
        }
    }
}
