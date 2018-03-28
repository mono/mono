//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;

    sealed class InitiatorSessionSymmetricTransportSecurityProtocol : TransportSecurityProtocol, IInitiatorSecuritySessionProtocol
    {
        SecurityToken outgoingSessionToken;
        List<SecurityToken> incomingSessionTokens;
        Object thisLock = new Object();
        DerivedKeySecurityToken derivedSignatureToken;
        bool requireDerivedKeys;

        public InitiatorSessionSymmetricTransportSecurityProtocol(SessionSymmetricTransportSecurityProtocolFactory factory,
            EndpointAddress target, Uri via) : base(factory, target, via)
        {
            if (factory.ActAsInitiator != true)
            {
                Fx.Assert("This protocol can only be used at the initiator.");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ProtocolMustBeInitiator, "InitiatorSessionSymmetricTransportSecurityProtocol")));
            }
            this.requireDerivedKeys = factory.SecurityTokenParameters.RequireDerivedKeys;
        }

        SessionSymmetricTransportSecurityProtocolFactory Factory
        {
            get { return (SessionSymmetricTransportSecurityProtocolFactory)this.SecurityProtocolFactory; }
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
                return false;
            }
            set
            {
            }
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
                    string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(this.Factory.MessageSecurityVersion.SecureConversationVersion);

                    this.derivedSignatureToken = new DerivedKeySecurityToken(-1, 0,
                        this.Factory.OutgoingAlgorithmSuite.GetSignatureKeyDerivationLength(token, this.Factory.MessageSecurityVersion.SecureConversationVersion), null, DerivedKeySecurityToken.DefaultNonceLength, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal), derivationAlgorithm, SecurityUtils.GenerateId());
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

        void GetTokensForOutgoingMessages(out SecurityToken signingToken, out SecurityToken sourceToken, out SecurityTokenParameters tokenParameters)
        {
            lock (ThisLock)
            {
                if (this.requireDerivedKeys)
                {
                    signingToken = this.derivedSignatureToken;
                    sourceToken = this.outgoingSessionToken;
                }
                else
                {
                    signingToken = this.outgoingSessionToken;
                    sourceToken = null;
                }
            }
            tokenParameters = this.Factory.GetTokenParameters();
        }

        internal void SetupDelayedSecurityExecution(string actor, ref Message message, SecurityToken signingToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters,
            IList<SupportingTokenSpecification> supportingTokens)
        {
            SendSecurityHeader securityHeader = CreateSendSecurityHeaderForTransportProtocol(message, actor, this.Factory);
            securityHeader.RequireMessageProtection = false;
            if (sourceToken != null)
            {
                securityHeader.AddPrerequisiteToken(sourceToken);
            }
            AddSupportingTokens(securityHeader, supportingTokens);
            securityHeader.AddEndorsingSupportingToken(signingToken, tokenParameters);
            message = securityHeader.SetupExecution();
        }

        protected override void SecureOutgoingMessageAtInitiator(ref Message message, string actor, TimeSpan timeout)
        {
            SecurityToken signingToken;
            SecurityToken sourceToken;
            SecurityTokenParameters tokenParameters;
            this.GetTokensForOutgoingMessages(out signingToken, out sourceToken, out tokenParameters);
            IList<SupportingTokenSpecification> supportingTokens;
            this.TryGetSupportingTokens(this.SecurityProtocolFactory, this.Target, this.Via, message, timeout, true, out supportingTokens);
            SetupDelayedSecurityExecution(actor, ref message, signingToken, sourceToken, tokenParameters, supportingTokens);
        }

        protected override IAsyncResult BeginSecureOutgoingMessageAtInitiatorCore(Message message, string actor, TimeSpan timeout, AsyncCallback callback, object state)
        {
            SecurityToken signingToken;
            SecurityToken sourceToken;
            SecurityTokenParameters tokenParameters;
            this.GetTokensForOutgoingMessages(out signingToken, out sourceToken, out tokenParameters);
            IList<SupportingTokenSpecification> supportingTokens;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!TryGetSupportingTokens(this.SecurityProtocolFactory, this.Target, this.Via, message, timeoutHelper.RemainingTime(), false, out supportingTokens))
            {
                return new SecureOutgoingMessageAsyncResult(actor, message, this, signingToken, sourceToken, tokenParameters, timeoutHelper.RemainingTime(), callback, state);
            }
            else
            {
                SetupDelayedSecurityExecution(actor, ref message, signingToken, sourceToken, tokenParameters, supportingTokens);
                return new CompletedAsyncResult<Message>(message, callback, state);
            }
        }

        protected override Message EndSecureOutgoingMessageAtInitiatorCore(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<Message>)
            {
                return CompletedAsyncResult<Message>.End(result);
            }
            else
            {
                return SecureOutgoingMessageAsyncResult.End(result);
            }
        }

        sealed class SecureOutgoingMessageAsyncResult : GetSupportingTokensAsyncResult
        {
            Message message;
            string actor;
            SecurityToken signingToken;
            SecurityToken sourceToken;
            SecurityTokenParameters tokenParameters;
            InitiatorSessionSymmetricTransportSecurityProtocol binding;

            public SecureOutgoingMessageAsyncResult(string actor, Message message, InitiatorSessionSymmetricTransportSecurityProtocol binding, SecurityToken signingToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters, TimeSpan timeout, AsyncCallback callback, object state)
                : base(message, binding, timeout, callback, state)
            {
                this.actor = actor;
                this.message = message;
                this.binding = binding;
                this.signingToken = signingToken;
                this.sourceToken = sourceToken;
                this.tokenParameters = tokenParameters;
                this.Start();
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.binding.SetupDelayedSecurityExecution(actor, ref message, signingToken, sourceToken, tokenParameters, this.SupportingTokens);
                return true;
            }

            internal static Message End(IAsyncResult result)
            {
                SecureOutgoingMessageAsyncResult self = AsyncResult.End<SecureOutgoingMessageAsyncResult>(result);
                return self.message;
            }
        }
    }
}
