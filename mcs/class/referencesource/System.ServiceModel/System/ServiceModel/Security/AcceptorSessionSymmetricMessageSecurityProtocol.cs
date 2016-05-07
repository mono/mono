//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    sealed class AcceptorSessionSymmetricMessageSecurityProtocol : MessageSecurityProtocol, IAcceptorSecuritySessionProtocol
    {
        SecurityToken outgoingSessionToken;
        SecurityTokenAuthenticator sessionTokenAuthenticator;
        SecurityTokenResolver sessionTokenResolver;
        ReadOnlyCollection<SecurityTokenResolver> sessionResolverList;
        bool returnCorrelationState = false;
        DerivedKeySecurityToken derivedSignatureToken;
        DerivedKeySecurityToken derivedEncryptionToken;
        UniqueId sessionId;
        SecurityStandardsManager sessionStandardsManager;
        Object thisLock = new Object();
        bool requireDerivedKeys;

        public AcceptorSessionSymmetricMessageSecurityProtocol(SessionSymmetricMessageSecurityProtocolFactory factory,
            EndpointAddress target)
            : base(factory, target, null)
        {
            if (factory.ActAsInitiator == true)
            {
                Fx.Assert("This protocol can only be used at the recipient.");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ProtocolMustBeRecipient, this.GetType().ToString())));
            }
            this.requireDerivedKeys = factory.SecurityTokenParameters.RequireDerivedKeys;
            if (requireDerivedKeys)
            {
                SecurityTokenSerializer innerTokenSerializer = this.Factory.StandardsManager.SecurityTokenSerializer;
                WSSecureConversation secureConversation = (innerTokenSerializer is WSSecurityTokenSerializer) ? ((WSSecurityTokenSerializer)innerTokenSerializer).SecureConversation : new WSSecurityTokenSerializer(this.Factory.MessageSecurityVersion.SecurityVersion).SecureConversation;
                this.sessionStandardsManager = new SecurityStandardsManager(factory.MessageSecurityVersion, new DerivedKeyCachingSecurityTokenSerializer(2, false, secureConversation, innerTokenSerializer));
            }
        }

        Object ThisLock
        {
            get
            {
                return thisLock;
            }
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

        protected override bool PerformIncomingAndOutgoingMessageExpectationChecks
        {
            get { return false; }
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
                        DerivedKeySecurityToken.DefaultNonceLength, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.External), derivationAlgorithm, SecurityUtils.GenerateId());

                    this.derivedEncryptionToken = new DerivedKeySecurityToken(-1, 0,
                        this.Factory.OutgoingAlgorithmSuite.GetEncryptionKeyDerivationLength(token, this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion), null,
                        DerivedKeySecurityToken.DefaultNonceLength, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.External), derivationAlgorithm, SecurityUtils.GenerateId());
                }
            }
        }

        public void SetSessionTokenAuthenticator(UniqueId sessionId, SecurityTokenAuthenticator sessionTokenAuthenticator, SecurityTokenResolver sessionTokenResolver)
        {
            this.CommunicationObject.ThrowIfDisposedOrImmutable();
            lock (ThisLock)
            {
                this.sessionId = sessionId;
                this.sessionTokenAuthenticator = sessionTokenAuthenticator;
                this.sessionTokenResolver = sessionTokenResolver;
                List<SecurityTokenResolver> tmp = new List<SecurityTokenResolver>(1);
                tmp.Add(this.sessionTokenResolver);
                this.sessionResolverList = new ReadOnlyCollection<SecurityTokenResolver>(tmp);
            }
        }

        void GetTokensForOutgoingMessages(out SecurityToken signingToken, out SecurityToken encryptionToken, out SecurityTokenParameters tokenParameters)
        {
            lock (ThisLock)
            {
                if (requireDerivedKeys)
                {
                    signingToken = this.derivedSignatureToken;
                    encryptionToken = this.derivedEncryptionToken;
                }
                else
                {
                    signingToken = encryptionToken = this.outgoingSessionToken;
                }
            }
            tokenParameters = this.Factory.GetTokenParameters();
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken signingToken;
            SecurityToken encryptionToken;
            SecurityTokenParameters tokenParameters;
            this.GetTokensForOutgoingMessages(out signingToken, out encryptionToken, out tokenParameters);
            SetUpDelayedSecurityExecution(ref message, signingToken, encryptionToken, tokenParameters, correlationState);
            return new CompletedAsyncResult<Message>(message, callback, state);
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken signingToken;
            SecurityToken encryptionToken;
            SecurityTokenParameters tokenParameters;
            this.GetTokensForOutgoingMessages(out signingToken, out encryptionToken, out tokenParameters);
            SetUpDelayedSecurityExecution(ref message, signingToken, encryptionToken, tokenParameters, correlationState);
            return null;
        }

        protected override void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            message = CompletedAsyncResult<Message>.End(result);
            newCorrelationState = null;
        }

        void SetUpDelayedSecurityExecution(ref Message message, SecurityToken signingToken, SecurityToken encryptionToken, 
            SecurityTokenParameters tokenParameters, SecurityProtocolCorrelationState correlationState)
        {
            string actor = string.Empty;
            SendSecurityHeader securityHeader = ConfigureSendSecurityHeader(message, actor, null, correlationState);
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
            IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators;
            ReceiveSecurityHeader securityHeader = ConfigureReceiveSecurityHeader(message, string.Empty, correlationStates, (this.requireDerivedKeys) ? this.sessionStandardsManager : null, out supportingAuthenticators);
            securityHeader.ConfigureSymmetricBindingServerReceiveHeader(this.sessionTokenAuthenticator, this.Factory.SecurityTokenParameters, supportingAuthenticators);
            securityHeader.ConfigureOutOfBandTokenResolver(MergeOutOfBandResolvers(supportingAuthenticators, this.sessionResolverList));
            // do not enforce key derivation requirement for Cancel messages due to WSE interop
            securityHeader.EnforceDerivedKeyRequirement = (message.Headers.Action != factory.StandardsManager.SecureConversationDriver.CloseAction.Value);
            ProcessSecurityHeader(securityHeader, ref message, null, timeout, correlationStates);
            SecurityToken signingToken = securityHeader.SignatureToken;
            SecurityContextSecurityToken signingSct = (signingToken as SecurityContextSecurityToken);
            if (signingSct == null || signingSct.ContextId != sessionId)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.NoSessionTokenPresentInMessage)));
            }
            AttachRecipientSecurityProperty(message, signingToken, false, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens,
                securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
            return GetCorrelationState(null, securityHeader);
        }
    }
}
