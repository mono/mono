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
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    sealed class SymmetricSecurityProtocol : MessageSecurityProtocol
    {
        SecurityTokenProvider initiatorSymmetricTokenProvider;
        SecurityTokenProvider initiatorAsymmetricTokenProvider;
        SecurityTokenAuthenticator initiatorTokenAuthenticator;

        public SymmetricSecurityProtocol(SymmetricSecurityProtocolFactory factory,
            EndpointAddress target, Uri via)
            : base(factory, target, via)
        {
        }

        SymmetricSecurityProtocolFactory Factory
        {
            get { return (SymmetricSecurityProtocolFactory)base.MessageSecurityProtocolFactory; }
        }

        public SecurityTokenProvider InitiatorSymmetricTokenProvider
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorSymmetricTokenProvider;
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

        public SecurityTokenAuthenticator InitiatorTokenAuthenticator
        {
            get
            {

                this.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorTokenAuthenticator;
            }
        }

        InitiatorServiceModelSecurityTokenRequirement CreateInitiatorTokenRequirement()
        {
            InitiatorServiceModelSecurityTokenRequirement tokenRequirement = CreateInitiatorSecurityTokenRequirement();
            this.Factory.SecurityTokenParameters.InitializeSecurityTokenRequirement(tokenRequirement);
            tokenRequirement.KeyUsage = this.Factory.SecurityTokenParameters.HasAsymmetricKey ? SecurityKeyUsage.Exchange : SecurityKeyUsage.Signature;
            tokenRequirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
            if (this.Factory.SecurityTokenParameters.HasAsymmetricKey)
            {
                tokenRequirement.IsOutOfBandToken = true;
            }
            return tokenRequirement;
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            if (this.Factory.ActAsInitiator)
            {
                // 1. Create a token requirement for the provider
                InitiatorServiceModelSecurityTokenRequirement tokenProviderRequirement = CreateInitiatorTokenRequirement();

                // 2. Create a provider
                SecurityTokenProvider tokenProvider = this.Factory.SecurityTokenManager.CreateSecurityTokenProvider(tokenProviderRequirement);
                SecurityUtils.OpenTokenProviderIfRequired(tokenProvider, timeoutHelper.RemainingTime());
                if (this.Factory.SecurityTokenParameters.HasAsymmetricKey)
                {
                    this.initiatorAsymmetricTokenProvider = tokenProvider;
                }
                else
                {
                    this.initiatorSymmetricTokenProvider = tokenProvider;
                }

                // 3. Create a token requirement for authenticator
                InitiatorServiceModelSecurityTokenRequirement tokenAuthenticatorRequirement = CreateInitiatorTokenRequirement();

                // 4. Create authenticator (we dont support out of band resolvers on the client side
                SecurityTokenResolver outOfBandTokenResolver;
                this.initiatorTokenAuthenticator = this.Factory.SecurityTokenManager.CreateSecurityTokenAuthenticator(tokenAuthenticatorRequirement, out outOfBandTokenResolver);
                SecurityUtils.OpenTokenAuthenticatorIfRequired(this.initiatorTokenAuthenticator, timeoutHelper.RemainingTime());
            }
        }

        public override void OnAbort()
        {
            if (this.Factory.ActAsInitiator)
            {
                SecurityTokenProvider provider = this.initiatorSymmetricTokenProvider ?? this.initiatorAsymmetricTokenProvider;
                if (provider != null)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(provider);
                }
                if (this.initiatorTokenAuthenticator != null)
                {
                    SecurityUtils.AbortTokenAuthenticatorIfRequired(this.initiatorTokenAuthenticator);
                }
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.Factory.ActAsInitiator)
            {
                SecurityTokenProvider provider = this.initiatorSymmetricTokenProvider ?? this.initiatorAsymmetricTokenProvider;
                if (provider != null)
                {
                    SecurityUtils.CloseTokenProviderIfRequired(provider, timeoutHelper.RemainingTime());
                }
                if (this.initiatorTokenAuthenticator != null)
                {
                    SecurityUtils.CloseTokenAuthenticatorIfRequired(this.initiatorTokenAuthenticator, timeoutHelper.RemainingTime());
                }
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }


        SecurityTokenProvider GetTokenProvider()
        {
            if (this.Factory.ActAsInitiator)
            {
                return (this.initiatorSymmetricTokenProvider ?? this.initiatorAsymmetricTokenProvider);
            }
            else
            {
                return this.Factory.RecipientAsymmetricTokenProvider;
            }
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken token;
            SecurityTokenParameters tokenParameters;
            IList<SupportingTokenSpecification> supportingTokens;
            SecurityToken prerequisiteWrappingToken;
            SecurityProtocolCorrelationState newCorrelationState;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, false, timeoutHelper.RemainingTime(), out token, out tokenParameters, out prerequisiteWrappingToken, out supportingTokens, out newCorrelationState))
            {
                SetUpDelayedSecurityExecution(ref message, prerequisiteWrappingToken, token, tokenParameters, supportingTokens, GetSignatureConfirmationCorrelationState(correlationState, newCorrelationState));
                return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, newCorrelationState, callback, state);
            }
            else
            {
                if (this.Factory.ActAsInitiator != true)
                {
                    Fx.Assert("Unexpected code path for server security application");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ProtocolMustBeInitiator, this.GetType().ToString())));
                }
                SecurityTokenProvider provider = GetTokenProvider();
                return new SecureOutgoingMessageAsyncResult(message, this, provider, this.Factory.ApplyConfidentiality, this.initiatorTokenAuthenticator, correlationState, timeoutHelper.RemainingTime(), callback, state);
            }
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken token;
            SecurityTokenParameters tokenParameters;
            IList<SupportingTokenSpecification> supportingTokens;
            SecurityToken prerequisiteWrappingToken;
            SecurityProtocolCorrelationState newCorrelationState;
            TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, true, timeout, out token, out tokenParameters, out prerequisiteWrappingToken, out supportingTokens, out newCorrelationState);
            SetUpDelayedSecurityExecution(ref message, prerequisiteWrappingToken, token, tokenParameters, supportingTokens, GetSignatureConfirmationCorrelationState(correlationState, newCorrelationState));
            return newCorrelationState;
        }

        void SetUpDelayedSecurityExecution(ref Message message,
            SecurityToken prerequisiteToken,
            SecurityToken primaryToken,
            SecurityTokenParameters primaryTokenParameters,
            IList<SupportingTokenSpecification> supportingTokens,
            SecurityProtocolCorrelationState correlationState
        )
        {
            string actor = string.Empty;
            SendSecurityHeader securityHeader = ConfigureSendSecurityHeader(message, actor, supportingTokens, correlationState);
            if (prerequisiteToken != null)
            {
                securityHeader.AddPrerequisiteToken(prerequisiteToken);
            }
            if (this.Factory.ApplyIntegrity || securityHeader.HasSignedTokens)
            {
                if (!this.Factory.ApplyIntegrity)
                {
                    securityHeader.SignatureParts = MessagePartSpecification.NoParts;
                }
                securityHeader.SetSigningToken(primaryToken, primaryTokenParameters);
            }
            if (Factory.ApplyConfidentiality || securityHeader.HasEncryptedTokens)
            {
                if (!this.Factory.ApplyConfidentiality)
                {
                    securityHeader.EncryptionParts = MessagePartSpecification.NoParts;
                }
                securityHeader.SetEncryptionToken(primaryToken, primaryTokenParameters);
            }
            message = securityHeader.SetupExecution();
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

        WrappedKeySecurityToken CreateWrappedKeyToken(SecurityToken wrappingToken, SecurityTokenParameters wrappingTokenParameters, SecurityTokenReferenceStyle wrappingTokenReferenceStyle)
        {
            int keyLength = Math.Max(128, this.Factory.OutgoingAlgorithmSuite.DefaultSymmetricKeyLength);
            CryptoHelper.ValidateSymmetricKeyLength(keyLength, this.Factory.OutgoingAlgorithmSuite);
            byte[] key = new byte[keyLength / 8];
            CryptoHelper.FillRandomBytes(key);
            string tokenId = SecurityUtils.GenerateId();
            string wrappingAlgorithm = this.Factory.OutgoingAlgorithmSuite.DefaultAsymmetricKeyWrapAlgorithm;
            SecurityKeyIdentifierClause clause = wrappingTokenParameters.CreateKeyIdentifierClause(wrappingToken, wrappingTokenReferenceStyle);
            SecurityKeyIdentifier identifier = new SecurityKeyIdentifier();
            identifier.Add(clause);
            return new WrappedKeySecurityToken(tokenId, key, wrappingAlgorithm, wrappingToken, identifier);
        }

        SecurityToken GetInitiatorToken(SecurityToken providerToken,
            Message message,
            TimeSpan timeout,
            out SecurityTokenParameters tokenParameters,
            out SecurityToken prerequisiteWrappingToken)
        {
            tokenParameters = null;
            prerequisiteWrappingToken = null;
            SecurityToken token;
            if (this.Factory.SecurityTokenParameters.HasAsymmetricKey)
            {
                SecurityToken asymmetricToken = providerToken;
                // non-Indigo server implementations might require the wrapping token to be included in the message
                bool isAsymmetricTokenInMessage = SendSecurityHeader.ShouldSerializeToken(this.Factory.SecurityTokenParameters, MessageDirection.Input);
                if (isAsymmetricTokenInMessage)
                {
                    prerequisiteWrappingToken = asymmetricToken;
                }
                token = CreateWrappedKeyToken(asymmetricToken, this.Factory.SecurityTokenParameters, (isAsymmetricTokenInMessage) ? SecurityTokenReferenceStyle.Internal : SecurityTokenReferenceStyle.External);
            }
            else
            {
                token = providerToken;
            }

            tokenParameters = this.Factory.GetProtectionTokenParameters();
            return token;
        }

        // try to get the token if it can be obtained within the
        // synchronous requirements of the call; return true iff a
        // token not required OR a token is required AND has been
        // obtained within the specified synchronous requirements.
        bool TryGetTokenSynchronouslyForOutgoingSecurity(Message message, SecurityProtocolCorrelationState correlationState, bool isBlockingCall, TimeSpan timeout, out SecurityToken token, out SecurityTokenParameters tokenParameters, out SecurityToken prerequisiteWrappingToken, out IList<SupportingTokenSpecification> supportingTokens, out SecurityProtocolCorrelationState newCorrelationState)
        {
            SymmetricSecurityProtocolFactory factory = this.Factory;
            supportingTokens = null;
            prerequisiteWrappingToken = null;
            token = null;
            tokenParameters = null;
            newCorrelationState = null;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (factory.ApplyIntegrity || factory.ApplyConfidentiality)
            {
                if (factory.ActAsInitiator)
                {
                    if (!isBlockingCall || !TryGetSupportingTokens(factory, this.Target, this.Via, message, timeoutHelper.RemainingTime(), isBlockingCall, out supportingTokens))
                    {
                        return false;
                    }
                    SecurityTokenProvider provider = GetTokenProvider();
                    SecurityToken providerToken = GetTokenAndEnsureOutgoingIdentity(provider, factory.ApplyConfidentiality, timeoutHelper.RemainingTime(), this.initiatorTokenAuthenticator);
                    token = GetInitiatorToken(providerToken, message, timeoutHelper.RemainingTime(), out tokenParameters, out prerequisiteWrappingToken);
                    newCorrelationState = GetCorrelationState(token);
                }
                else
                {
                    token = GetCorrelationToken(correlationState);
                    tokenParameters = this.Factory.GetProtectionTokenParameters();
                }
            }
            return true;
        }

        SecurityToken GetCorrelationToken(SecurityProtocolCorrelationState[] correlationStates, out SecurityTokenParameters correlationTokenParameters)
        {
            SecurityToken token = GetCorrelationToken(correlationStates);
            correlationTokenParameters = this.Factory.GetProtectionTokenParameters();
            return token;
        }

        void EnsureWrappedToken(SecurityToken token, Message message)
        {
            if (!(token is WrappedKeySecurityToken))
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.IncomingSigningTokenMustBeAnEncryptedKey)), message);
            }
        }

        protected override SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            SymmetricSecurityProtocolFactory factory = this.Factory;
            IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            ReceiveSecurityHeader securityHeader = ConfigureReceiveSecurityHeader(message, string.Empty, correlationStates, out supportingAuthenticators);
            SecurityToken requiredReplySigningToken = null;
            if (this.Factory.ActAsInitiator)
            {
                // set the outofband protection token
                SecurityTokenParameters outOfBandTokenParameters;
                SecurityToken outOfBandToken = GetCorrelationToken(correlationStates, out outOfBandTokenParameters);
                securityHeader.ConfigureSymmetricBindingClientReceiveHeader(outOfBandToken, outOfBandTokenParameters);
                requiredReplySigningToken = outOfBandToken;
            }
            else
            {
                if (factory.RecipientSymmetricTokenAuthenticator != null)
                {
                    securityHeader.ConfigureSymmetricBindingServerReceiveHeader(this.Factory.RecipientSymmetricTokenAuthenticator, this.Factory.SecurityTokenParameters, supportingAuthenticators);
                }
                else
                {
                    securityHeader.ConfigureSymmetricBindingServerReceiveHeader(this.Factory.RecipientAsymmetricTokenProvider.GetToken(timeoutHelper.RemainingTime()), this.Factory.SecurityTokenParameters, supportingAuthenticators);
                    securityHeader.WrappedKeySecurityTokenAuthenticator = this.Factory.WrappedKeySecurityTokenAuthenticator;
                }
                securityHeader.ConfigureOutOfBandTokenResolver(MergeOutOfBandResolvers(supportingAuthenticators, this.Factory.RecipientOutOfBandTokenResolverList));
            }

            ProcessSecurityHeader(securityHeader, ref message, requiredReplySigningToken, timeoutHelper.RemainingTime(), correlationStates);
            SecurityToken signingToken = securityHeader.SignatureToken;
            if (factory.RequireIntegrity)
            {
                if (factory.SecurityTokenParameters.HasAsymmetricKey)
                {
                    // enforce that the signing token is a wrapped key token
                    EnsureWrappedToken(signingToken, message);
                }
                else
                {
                    EnsureNonWrappedToken(signingToken, message);
                }

                if (factory.ActAsInitiator)
                {
                    if (!factory.SecurityTokenParameters.HasAsymmetricKey)
                    {
                        ReadOnlyCollection<IAuthorizationPolicy> signingTokenPolicies = this.initiatorTokenAuthenticator.ValidateToken(signingToken);
                        DoIdentityCheckAndAttachInitiatorSecurityProperty(message, signingToken, signingTokenPolicies);
                    }
                    else
                    {
                        SecurityToken wrappingToken = (signingToken as WrappedKeySecurityToken).WrappingToken;
                        ReadOnlyCollection<IAuthorizationPolicy> wrappingTokenPolicies = this.initiatorTokenAuthenticator.ValidateToken(wrappingToken);
                        DoIdentityCheckAndAttachInitiatorSecurityProperty(message, signingToken, wrappingTokenPolicies);
                    }
                }
                else
                {
                    AttachRecipientSecurityProperty(message, signingToken, this.Factory.SecurityTokenParameters.HasAsymmetricKey, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens,
                        securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
                }
            }
            return GetCorrelationState(signingToken, securityHeader);
        }

        sealed class SecureOutgoingMessageAsyncResult : GetOneTokenAndSetUpSecurityAsyncResult
        {
            SymmetricSecurityProtocol symmetricBinding;

            public SecureOutgoingMessageAsyncResult(Message m, SymmetricSecurityProtocol binding, SecurityTokenProvider provider,
                bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator, SecurityProtocolCorrelationState correlationState, TimeSpan timeout, AsyncCallback callback, object state)
                : base(m, binding, provider, doIdentityChecks, identityCheckAuthenticator, correlationState, timeout, callback, state)
            {
                symmetricBinding = binding;
                Start();
            }

            protected override void OnGetTokenDone(ref Message message, SecurityToken providerToken, TimeSpan timeout)
            {
                SecurityTokenParameters tokenParameters;
                SecurityToken prerequisiteWrappingToken;
                SecurityToken token = symmetricBinding.GetInitiatorToken(providerToken, message, timeout, out tokenParameters, out prerequisiteWrappingToken);
                this.SetCorrelationToken(token);
                symmetricBinding.SetUpDelayedSecurityExecution(ref message, prerequisiteWrappingToken, token, tokenParameters, this.SupportingTokens, Binding.GetSignatureConfirmationCorrelationState(OldCorrelationState, NewCorrelationState));
            }
        }
    }
}
