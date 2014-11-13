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
    using System.ServiceModel.Security.Tokens;

    sealed class InitiatorSessionSymmetricMessageSecurityProtocol : MessageSecurityProtocol, IInitiatorSecuritySessionProtocol
    {
        SecurityToken outgoingSessionToken;
        SecurityTokenAuthenticator sessionTokenAuthenticator;
        List<SecurityToken> incomingSessionTokens;
        DerivedKeySecurityToken derivedSignatureToken;
        DerivedKeySecurityToken derivedEncryptionToken;
        SecurityStandardsManager sessionStandardsManager;
        bool requireDerivedKeys;
        Object thisLock = new Object();
        bool returnCorrelationState = false;

        public InitiatorSessionSymmetricMessageSecurityProtocol(SessionSymmetricMessageSecurityProtocolFactory factory,
            EndpointAddress target, Uri via)
            : base(factory, target, via)
        {
            if (factory.ActAsInitiator != true)
            {
                Fx.Assert("This protocol can only be used at the initiator.");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ProtocolMustBeInitiator, "InitiatorSessionSymmetricMessageSecurityProtocol")));
            }
            this.requireDerivedKeys = factory.SecurityTokenParameters.RequireDerivedKeys;
            if (requireDerivedKeys)
            {
                SecurityTokenSerializer innerTokenSerializer = this.Factory.StandardsManager.SecurityTokenSerializer;
                WSSecureConversation secureConversation = (innerTokenSerializer is WSSecurityTokenSerializer) ? ((WSSecurityTokenSerializer)innerTokenSerializer).SecureConversation : new WSSecurityTokenSerializer(this.Factory.MessageSecurityVersion.SecurityVersion).SecureConversation;
                this.sessionStandardsManager = new SecurityStandardsManager(factory.MessageSecurityVersion, new DerivedKeyCachingSecurityTokenSerializer(2, true, secureConversation, innerTokenSerializer));
            }
        }

        Object ThisLock
        {
            get
            {
                return thisLock;
            }
        }

        protected override bool PerformIncomingAndOutgoingMessageExpectationChecks
        {
            get { return false; }
        }


        public bool ReturnCorrelationState
        {
            get
            {
                return this.returnCorrelationState;
            }
            set
            {
                this.returnCorrelationState = value;
            }
        }

        SessionSymmetricMessageSecurityProtocolFactory Factory
        {
            get { return (SessionSymmetricMessageSecurityProtocolFactory)base.MessageSecurityProtocolFactory; }
        }

        public SecurityToken GetOutgoingSessionToken()
        {
            lock (ThisLock)
            {
                return this.outgoingSessionToken;
            }
        }

        public void SetIdentityCheckAuthenticator(SecurityTokenAuthenticator authenticator)
        {
            this.sessionTokenAuthenticator = authenticator;
        }

        public void SetOutgoingSessionToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            lock (ThisLock)
            {
                this.outgoingSessionToken = token;
                if (this.requireDerivedKeys)
                {
                    string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion);

                    this.derivedSignatureToken = new DerivedKeySecurityToken(-1, 0, 
                        this.Factory.OutgoingAlgorithmSuite.GetSignatureKeyDerivationLength(token, this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion), null,
                        DerivedKeySecurityToken.DefaultNonceLength, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal), derivationAlgorithm, SecurityUtils.GenerateId());
                    this.derivedEncryptionToken = new DerivedKeySecurityToken(-1, 0, 
                        this.Factory.OutgoingAlgorithmSuite.GetEncryptionKeyDerivationLength(token, this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion),
                        null, DerivedKeySecurityToken.DefaultNonceLength, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal), derivationAlgorithm, SecurityUtils.GenerateId());
                }
            }
        }

        public List<SecurityToken> GetIncomingSessionTokens()
        {
            lock (ThisLock)
            {
                return this.incomingSessionTokens;
            }
        }

        public void SetIncomingSessionTokens(List<SecurityToken> tokens)
        {
            if (tokens == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokens");
            }
            lock (ThisLock)
            {
                this.incomingSessionTokens = new List<SecurityToken>(tokens);
            }
        }

        void GetTokensForOutgoingMessages(out SecurityToken signingToken, out SecurityToken encryptionToken, out SecurityToken sourceToken, out SecurityTokenParameters tokenParameters)
        {
            lock (ThisLock)
            {
                if (this.requireDerivedKeys)
                {
                    signingToken = this.derivedSignatureToken;
                    encryptionToken = this.derivedEncryptionToken;
                    sourceToken = this.outgoingSessionToken;
                }
                else
                {
                    signingToken = encryptionToken = this.outgoingSessionToken;
                    sourceToken = null;
                }
            }
            if (this.Factory.ApplyConfidentiality)
            {
                EnsureOutgoingIdentity(sourceToken ?? encryptionToken, this.sessionTokenAuthenticator);
            }
            tokenParameters = this.Factory.GetTokenParameters();
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken signingToken;
            SecurityToken encryptionToken;
            SecurityToken sourceToken;
            SecurityTokenParameters tokenParameters;
            this.GetTokensForOutgoingMessages(out signingToken, out encryptionToken, out sourceToken, out tokenParameters);
            IList<SupportingTokenSpecification> supportingTokens;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (TryGetSupportingTokens(this.Factory, this.Target, this.Via, message, timeoutHelper.RemainingTime(), false, out supportingTokens))
            {
                SecurityProtocolCorrelationState newCorrelationState = CreateCorrelationStateIfRequired();
                SetUpDelayedSecurityExecution(ref message, signingToken, encryptionToken, sourceToken, tokenParameters, supportingTokens, newCorrelationState);
                return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, newCorrelationState, callback, state);
            }
            else
            {
                return new SecureOutgoingMessageAsyncResult(message, this, signingToken, encryptionToken, sourceToken, tokenParameters, timeoutHelper.RemainingTime(), callback, state);
            }
        }

        internal SecurityProtocolCorrelationState CreateCorrelationStateIfRequired()
        {
            if (this.ReturnCorrelationState)
            {
                return new SecurityProtocolCorrelationState(null);
            }
            else
            {
                return null;
            }
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken signingToken;
            SecurityToken encryptionToken;
            SecurityToken sourceToken;
            SecurityTokenParameters tokenParameters;
            this.GetTokensForOutgoingMessages(out signingToken, out encryptionToken, out sourceToken, out tokenParameters);
            SecurityProtocolCorrelationState newCorrelationState = CreateCorrelationStateIfRequired();
            IList<SupportingTokenSpecification> supportingTokens;
            this.TryGetSupportingTokens(this.SecurityProtocolFactory, this.Target, this.Via, message, timeout, true, out supportingTokens);
            SetUpDelayedSecurityExecution(ref message, signingToken, encryptionToken, sourceToken, tokenParameters, supportingTokens, newCorrelationState);
            return newCorrelationState;
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

        internal void SetUpDelayedSecurityExecution(ref Message message, SecurityToken signingToken, SecurityToken encryptionToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters, 
            IList<SupportingTokenSpecification> supportingTokens,  SecurityProtocolCorrelationState correlationState)
        {
            SessionSymmetricMessageSecurityProtocolFactory factory = this.Factory;
            string actor = string.Empty;
            SendSecurityHeader securityHeader = ConfigureSendSecurityHeader(message, actor, supportingTokens, correlationState);
            if (sourceToken != null)
            {
                securityHeader.AddPrerequisiteToken(sourceToken);
            }
            if (this.Factory.ApplyIntegrity)
            {
                securityHeader.SetSigningToken(signingToken, tokenParameters);
            }
            if (Factory.ApplyConfidentiality)
            {
                securityHeader.SetEncryptionToken(encryptionToken, tokenParameters);
            }
            message = securityHeader.SetupExecution();
        }

        protected override SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            SessionSymmetricMessageSecurityProtocolFactory factory = this.Factory;
            IList<SupportingTokenAuthenticatorSpecification> dummyAuthenticators;
            ReceiveSecurityHeader securityHeader = ConfigureReceiveSecurityHeader(message, string.Empty, correlationStates, this.requireDerivedKeys ? this.sessionStandardsManager : null, out dummyAuthenticators);
            
            List<SecurityToken> sessionTokens = GetIncomingSessionTokens();
            securityHeader.ConfigureSymmetricBindingClientReceiveHeader(sessionTokens, this.Factory.SecurityTokenParameters);
            // do not enforce the key derivation requirement for CancelResponse due to WSE interop
            securityHeader.EnforceDerivedKeyRequirement = (message.Headers.Action != factory.StandardsManager.SecureConversationDriver.CloseResponseAction.Value);
            ProcessSecurityHeader(securityHeader, ref message, null, timeout, correlationStates);
            SecurityToken signingToken = securityHeader.SignatureToken;
            // verify that the signing token was one of the session tokens
            bool isSessionToken = false;
            for (int i = 0; i < sessionTokens.Count; ++i)
            {
                if (Object.ReferenceEquals(signingToken, sessionTokens[i]))
                {
                    isSessionToken = true;
                    break;
                }
            }
            if (!isSessionToken)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.NoSessionTokenPresentInMessage)));
            }
            if (factory.RequireIntegrity)
            {
                ReadOnlyCollection<IAuthorizationPolicy> signingTokenPolicies = this.sessionTokenAuthenticator.ValidateToken(signingToken);
                DoIdentityCheckAndAttachInitiatorSecurityProperty(message, signingToken, signingTokenPolicies);
            }
            return null;
        }

        sealed class SecureOutgoingMessageAsyncResult : GetSupportingTokensAsyncResult
        {
            Message message;
            InitiatorSessionSymmetricMessageSecurityProtocol binding;
            SecurityToken signingToken;
            SecurityToken encryptionToken;
            SecurityToken sourceToken;
            SecurityTokenParameters tokenParameters;
            SecurityProtocolCorrelationState newCorrelationState;

            public SecureOutgoingMessageAsyncResult(Message message, InitiatorSessionSymmetricMessageSecurityProtocol binding, SecurityToken signingToken, SecurityToken encryptionToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters, TimeSpan timeout, AsyncCallback callback, object state)
                : base(message, binding, timeout, callback, state)
            {
                this.message = message;
                this.binding = binding;
                this.signingToken = signingToken;
                this.encryptionToken = encryptionToken;
                this.sourceToken = sourceToken;
                this.tokenParameters = tokenParameters;
                this.Start();
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                newCorrelationState = binding.CreateCorrelationStateIfRequired();
                binding.SetUpDelayedSecurityExecution(ref message, signingToken, encryptionToken, sourceToken, tokenParameters, this.SupportingTokens, newCorrelationState);
                return true;
            }

            internal static Message End(IAsyncResult result, out SecurityProtocolCorrelationState newCorrelationState)
            {
                SecureOutgoingMessageAsyncResult self = AsyncResult.End<SecureOutgoingMessageAsyncResult>(result);
                newCorrelationState = self.newCorrelationState;
                return self.message;
            }
        }
    }
}
