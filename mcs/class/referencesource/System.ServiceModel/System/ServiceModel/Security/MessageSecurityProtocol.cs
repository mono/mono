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
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;

    abstract class MessageSecurityProtocol : SecurityProtocol
    {
        readonly MessageSecurityProtocolFactory factory;
        SecurityToken identityVerifiedToken; // verified for the readonly target

        protected MessageSecurityProtocol(MessageSecurityProtocolFactory factory, EndpointAddress target, Uri via)
            : base(factory, target, via)
        {
            this.factory = factory;
        }

        // Protocols that have more than one active, identity checked
        // token at any time should override this property and return
        // false
        protected virtual bool CacheIdentityCheckResultForToken
        {
            get { return true; }
        }

        protected virtual bool DoAutomaticEncryptionMatch
        {
            get { return true; }
        }

        protected virtual bool PerformIncomingAndOutgoingMessageExpectationChecks
        {
            get { return true; }
        }

        protected bool RequiresIncomingSecurityProcessing(Message message)
        {
            // if we are receiveing a response that has no security that we should accept this AND no security header exists
            // then it is OK to skip the header.
            if (this.factory.ActAsInitiator
              && this.factory.SecurityBindingElement.EnableUnsecuredResponse
              && !this.factory.StandardsManager.SecurityVersion.DoesMessageContainSecurityHeader(message))
                return false;

            bool requiresAppSecurity = this.factory.RequireIntegrity || this.factory.RequireConfidentiality || this.factory.DetectReplays;
            return requiresAppSecurity || factory.ExpectSupportingTokens;
        }

        protected bool RequiresOutgoingSecurityProcessing
        {
            get
            {
                // If were are the listener, don't apply security if the flag is set
                if (!this.factory.ActAsInitiator && this.factory.SecurityBindingElement.EnableUnsecuredResponse)
                    return false;

                bool requiresAppSecurity = this.factory.ApplyIntegrity || this.factory.ApplyConfidentiality || this.factory.AddTimestamp;
                return requiresAppSecurity || factory.ExpectSupportingTokens;
            }
        }

        protected MessageSecurityProtocolFactory MessageSecurityProtocolFactory
        {
            get { return this.factory; }
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            try
            {
                this.CommunicationObject.ThrowIfClosedOrNotOpen();
                ValidateOutgoingState(message);
                if (!this.RequiresOutgoingSecurityProcessing && message.Properties.Security == null)
                {
                    return new CompletedAsyncResult<Message>(message, callback, state);
                }
                return BeginSecureOutgoingMessageCore(message, timeout, null, callback, state);
            }
            catch (Exception exception)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(exception)) throw;

                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            try
            {
                this.CommunicationObject.ThrowIfClosedOrNotOpen();
                ValidateOutgoingState(message);
                if (!this.RequiresOutgoingSecurityProcessing && message.Properties.Security == null)
                {
                    return new CompletedAsyncResult<Message>(message, callback, state);
                }
                return BeginSecureOutgoingMessageCore(message, timeout, correlationState, callback, state);
            }
            catch (Exception exception)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(exception)) throw;

                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        protected abstract IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state);

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            try
            {
                SecurityProtocolCorrelationState newCorrelationState;
                EndSecureOutgoingMessageCore(result, out message, out newCorrelationState);
                base.OnOutgoingMessageSecured(message);
            }
            catch (Exception exception)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(exception)) throw;

                base.OnSecureOutgoingMessageFailure(null);
                throw;
            }
        }

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            try
            {
                EndSecureOutgoingMessageCore(result, out message, out newCorrelationState);
                base.OnOutgoingMessageSecured(message);
            }
            catch (Exception exception)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(exception)) throw;

                base.OnSecureOutgoingMessageFailure(null);
                throw;
            }
        }

        protected abstract void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState);

        // helper method for attaching the client claims in a symmetric security protocol
        protected void AttachRecipientSecurityProperty(Message message, SecurityToken protectionToken, bool isWrappedToken, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens,
           IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            ReadOnlyCollection<IAuthorizationPolicy> protectionTokenPolicies;
            if (isWrappedToken)
            {
                protectionTokenPolicies = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            else
            {
                protectionTokenPolicies = tokenPoliciesMapping[protectionToken];
            }
            SecurityMessageProperty security = SecurityMessageProperty.GetOrCreate(message);
            security.ProtectionToken = new SecurityTokenSpecification(protectionToken, protectionTokenPolicies);
            AddSupportingTokenSpecification(security, basicTokens, endorsingTokens, signedEndorsingTokens, signedTokens, tokenPoliciesMapping);
            security.ServiceSecurityContext = new ServiceSecurityContext(security.GetInitiatorTokenAuthorizationPolicies());
        }

        // helper method for attaching the server claims in a symmetric security protocol
        protected void DoIdentityCheckAndAttachInitiatorSecurityProperty(Message message, SecurityToken protectionToken, ReadOnlyCollection<IAuthorizationPolicy> protectionTokenPolicies)
        {
            AuthorizationContext protectionAuthContext = EnsureIncomingIdentity(message, protectionToken, protectionTokenPolicies);
            SecurityMessageProperty security = SecurityMessageProperty.GetOrCreate(message);
            security.ProtectionToken = new SecurityTokenSpecification(protectionToken, protectionTokenPolicies);
            security.ServiceSecurityContext = new ServiceSecurityContext(protectionAuthContext, protectionTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
        }

        protected AuthorizationContext EnsureIncomingIdentity(Message message, SecurityToken token, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (token == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoSigningTokenAvailableToDoIncomingIdentityCheck)), message);
            }
            AuthorizationContext authContext = (authorizationPolicies != null) ? AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies) : null;
            if (this.factory.IdentityVerifier != null)
            {
                if (this.Target == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoOutgoingEndpointAddressAvailableForDoingIdentityCheckOnReply)), message);
                }

                this.factory.IdentityVerifier.EnsureIncomingIdentity(this.Target, authContext);
            }
            return authContext;
        }

        protected void EnsureOutgoingIdentity(SecurityToken token, SecurityTokenAuthenticator authenticator)
        {
            if (object.ReferenceEquals(token, this.identityVerifiedToken))
            {
                return;
            }
            if (this.factory.IdentityVerifier == null)
            {
                return;
            }
            if (this.Target == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoOutgoingEndpointAddressAvailableForDoingIdentityCheck)));
            }
            ReadOnlyCollection<IAuthorizationPolicy> authzPolicies = authenticator.ValidateToken(token);
            this.factory.IdentityVerifier.EnsureOutgoingIdentity(this.Target, authzPolicies);
            if (this.CacheIdentityCheckResultForToken)
            {
                this.identityVerifiedToken = token;
            }
        }

        protected SecurityProtocolCorrelationState GetCorrelationState(SecurityToken correlationToken)
        {
            return new SecurityProtocolCorrelationState(correlationToken);
        }

        protected SecurityProtocolCorrelationState GetCorrelationState(SecurityToken correlationToken, ReceiveSecurityHeader securityHeader)
        {
            SecurityProtocolCorrelationState result = new SecurityProtocolCorrelationState(correlationToken);
            if (securityHeader.MaintainSignatureConfirmationState && !this.factory.ActAsInitiator)
            {
                result.SignatureConfirmations = securityHeader.GetSentSignatureValues();
            }
            return result;
        }

        protected SecurityToken GetCorrelationToken(SecurityProtocolCorrelationState[] correlationStates)
        {
            SecurityToken token = null;
            if (correlationStates != null)
            {
                for (int i = 0; i < correlationStates.Length; ++i)
                {
                    if (correlationStates[i].Token == null)
                        continue;
                    if (token == null)
                    {
                        token = correlationStates[i].Token;
                    }
                    else if (!object.ReferenceEquals(token, correlationStates[i].Token))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MultipleCorrelationTokensFound)));
                    }
                }
            }
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoCorrelationTokenFound)));
            }
            return token;
        }


        protected SecurityToken GetCorrelationToken(SecurityProtocolCorrelationState correlationState)
        {
            if (correlationState == null || correlationState.Token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.CannotFindCorrelationStateForApplyingSecurity)));
            }
            return correlationState.Token;
        }

        protected static void EnsureNonWrappedToken(SecurityToken token, Message message)
        {
            if (token is WrappedKeySecurityToken)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenNotExpectedInSecurityHeader, token)), message);
            }
        }

        protected SecurityToken GetTokenAndEnsureOutgoingIdentity(SecurityTokenProvider provider, bool isEncryptionOn, TimeSpan timeout, SecurityTokenAuthenticator authenticator)
        {
            SecurityToken token = GetToken(provider, this.Target, timeout);
            if (isEncryptionOn)
            {
                EnsureOutgoingIdentity(token, authenticator);
            }
            return token;
        }

        protected SendSecurityHeader ConfigureSendSecurityHeader(Message message, string actor, IList<SupportingTokenSpecification> supportingTokens, SecurityProtocolCorrelationState correlationState)
        {
            MessageSecurityProtocolFactory factory = this.MessageSecurityProtocolFactory;
            SendSecurityHeader securityHeader = CreateSendSecurityHeader(message, actor, factory);
            securityHeader.SignThenEncrypt = factory.MessageProtectionOrder != MessageProtectionOrder.EncryptBeforeSign;
            // If ProtectTokens is enabled then we make sure that both the client side and the service side sign the primary token 
            // ( if it is an issued token, the check exists in sendsecurityheader)in the primary signature while sending a message.
            securityHeader.ShouldProtectTokens = factory.SecurityBindingElement.ProtectTokens;
            securityHeader.EncryptPrimarySignature = factory.MessageProtectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;

            if (factory.DoRequestSignatureConfirmation && correlationState != null)
            {
                if (factory.ActAsInitiator)
                {
                    securityHeader.MaintainSignatureConfirmationState = true;
                    securityHeader.CorrelationState = correlationState;
                }
                else if (correlationState.SignatureConfirmations != null)
                {
                    securityHeader.AddSignatureConfirmations(correlationState.SignatureConfirmations);
                }
            }

            string action = message.Headers.Action;
            if (this.factory.ApplyIntegrity)
            {
                securityHeader.SignatureParts = this.factory.GetOutgoingSignatureParts(action);
            }

            if (factory.ApplyConfidentiality)
            {
                securityHeader.EncryptionParts = this.factory.GetOutgoingEncryptionParts(action);
            }
            AddSupportingTokens(securityHeader, supportingTokens);
            return securityHeader;
        }

        protected ReceiveSecurityHeader CreateSecurityHeader(Message message, string actor, MessageDirection transferDirection, SecurityStandardsManager standardsManager)
        {
            standardsManager = standardsManager ?? this.factory.StandardsManager;
            ReceiveSecurityHeader securityHeader = standardsManager.CreateReceiveSecurityHeader(message, actor,
               this.factory.IncomingAlgorithmSuite, transferDirection);
            securityHeader.Layout = this.factory.SecurityHeaderLayout;
            securityHeader.MaxReceivedMessageSize = factory.SecurityBindingElement.MaxReceivedMessageSize;
            securityHeader.ReaderQuotas = factory.SecurityBindingElement.ReaderQuotas;
            if (this.factory.ExpectKeyDerivation)
            {
                securityHeader.DerivedTokenAuthenticator = this.factory.DerivedKeyTokenAuthenticator;
            }
            return securityHeader;
        }

        bool HasCorrelationState(SecurityProtocolCorrelationState[] correlationState)
        {
            if (correlationState == null || correlationState.Length == 0)
            {
                return false;
            }
            else if (correlationState.Length == 1 && correlationState[0] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected ReceiveSecurityHeader ConfigureReceiveSecurityHeader(Message message, string actor, SecurityProtocolCorrelationState[] correlationStates, out IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators)
        {
            return ConfigureReceiveSecurityHeader(message, actor, correlationStates, null, out supportingAuthenticators);
        }

        protected ReceiveSecurityHeader ConfigureReceiveSecurityHeader(Message message, string actor, SecurityProtocolCorrelationState[] correlationStates, SecurityStandardsManager standardsManager, out IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators)
        {
            MessageSecurityProtocolFactory factory = this.MessageSecurityProtocolFactory;
            MessageDirection direction = factory.ActAsInitiator ? MessageDirection.Output : MessageDirection.Input;
            ReceiveSecurityHeader securityHeader = CreateSecurityHeader(message, actor, direction, standardsManager);

            string action = message.Headers.Action;
            supportingAuthenticators = GetSupportingTokenAuthenticatorsAndSetExpectationFlags(this.factory, message, securityHeader);
            if (factory.RequireIntegrity || securityHeader.ExpectSignedTokens)
            {
                securityHeader.RequiredSignatureParts = factory.GetIncomingSignatureParts(action);
            }
            if (factory.RequireConfidentiality || securityHeader.ExpectBasicTokens)
            {
                securityHeader.RequiredEncryptionParts = factory.GetIncomingEncryptionParts(action);
            }

            securityHeader.ExpectEncryption = factory.RequireConfidentiality || securityHeader.ExpectBasicTokens;
            securityHeader.ExpectSignature = factory.RequireIntegrity || securityHeader.ExpectSignedTokens;
            securityHeader.SetRequiredProtectionOrder(factory.MessageProtectionOrder);

            // On the receiving side if protectTokens is enabled
            // 1. If we are service, we make sure that the client always signs the primary token( can be any token type)else we throw.
            //    But currently the service can sign the primary token in reply only if the primary token is an issued token 
            // 2. If we are client, we do not care if the service signs the primary token or not. Otherwise it will be impossible to have a wcf client /service talk to each other unless we 
            // either use a symmetric binding with issued tokens or asymmetric bindings with both the intiator and recipient parameters being issued tokens( later one is rare).
            securityHeader.RequireSignedPrimaryToken = !factory.ActAsInitiator && factory.SecurityBindingElement.ProtectTokens;

            if (factory.ActAsInitiator && factory.DoRequestSignatureConfirmation && HasCorrelationState(correlationStates))
            {
                securityHeader.MaintainSignatureConfirmationState = true;
                securityHeader.ExpectSignatureConfirmation = true;
            }
            else if (!factory.ActAsInitiator && factory.DoRequestSignatureConfirmation)
            {
                securityHeader.MaintainSignatureConfirmationState = true;
            }
            else
            {
                securityHeader.MaintainSignatureConfirmationState = false;
            }
            return securityHeader;
        }

        protected void ProcessSecurityHeader(ReceiveSecurityHeader securityHeader, ref Message message,
            SecurityToken requiredSigningToken, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            securityHeader.ReplayDetectionEnabled = this.factory.DetectReplays;
            securityHeader.SetTimeParameters(this.factory.NonceCache, this.factory.ReplayWindow, this.factory.MaxClockSkew);

            securityHeader.Process(timeoutHelper.RemainingTime(), SecurityUtils.GetChannelBindingFromMessage(message), this.factory.ExtendedProtectionPolicy);
            if (this.factory.AddTimestamp && securityHeader.Timestamp == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.RequiredTimestampMissingInSecurityHeader)));
            }

            if (requiredSigningToken != null && requiredSigningToken != securityHeader.SignatureToken)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.ReplyWasNotSignedWithRequiredSigningToken)), message);
            }

            if (this.DoAutomaticEncryptionMatch)
            {
                SecurityUtils.EnsureExpectedSymmetricMatch(securityHeader.SignatureToken, securityHeader.EncryptionToken, message);
            }

            if (securityHeader.MaintainSignatureConfirmationState && this.factory.ActAsInitiator)
            {
                CheckSignatureConfirmation(securityHeader, correlationStates);
            }

            message = securityHeader.ProcessedMessage;
        }

        protected void CheckSignatureConfirmation(ReceiveSecurityHeader securityHeader, SecurityProtocolCorrelationState[] correlationStates)
        {
            SignatureConfirmations receivedConfirmations = securityHeader.GetSentSignatureConfirmations();
            SignatureConfirmations sentSignatures = null;
            if (correlationStates != null)
            {
                for (int i = 0; i < correlationStates.Length; ++i)
                {
                    if (correlationStates[i].SignatureConfirmations != null)
                    {
                        sentSignatures = correlationStates[i].SignatureConfirmations;
                        break;
                    }
                }
            }
            if (sentSignatures == null)
            {
                if (receivedConfirmations != null && receivedConfirmations.Count > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.FoundUnexpectedSignatureConfirmations)));
                }
                return;
            }
            bool allSignaturesConfirmed = false;
            if (receivedConfirmations != null && sentSignatures.Count == receivedConfirmations.Count)
            {
                bool[] matchingSigIndexes = new bool[sentSignatures.Count];
                for (int i = 0; i < sentSignatures.Count; ++i)
                {
                    byte[] sentSignature;
                    bool wasSentSigEncrypted;
                    sentSignatures.GetConfirmation(i, out sentSignature, out wasSentSigEncrypted);
                    for (int j = 0; j < receivedConfirmations.Count; ++j)
                    {
                        byte[] receivedSignature;
                        bool wasReceivedSigEncrypted;
                        if (matchingSigIndexes[j])
                        {
                            continue;
                        }
                        receivedConfirmations.GetConfirmation(j, out receivedSignature, out wasReceivedSigEncrypted);
                        if ((wasReceivedSigEncrypted == wasSentSigEncrypted) && CryptoHelper.IsEqual(receivedSignature, sentSignature))
                        {
                            matchingSigIndexes[j] = true;
                            break;
                        }
                    }
                }
                int k;
                for (k = 0; k < matchingSigIndexes.Length; ++k)
                {
                    if (!matchingSigIndexes[k])
                    {
                        break;
                    }
                }
                if (k == matchingSigIndexes.Length)
                {
                    allSignaturesConfirmed = true;
                }
            }
            if (!allSignaturesConfirmed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NotAllSignaturesConfirmed)));
            }
        }

        public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
        {
            try
            {
                this.CommunicationObject.ThrowIfClosedOrNotOpen();
                ValidateOutgoingState(message);
                if (!this.RequiresOutgoingSecurityProcessing && message.Properties.Security == null)
                {
                    return;
                }

                SecureOutgoingMessageCore(ref message, timeout, null);
                base.OnOutgoingMessageSecured(message);
            }
            catch (Exception exception)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(exception)) throw;

                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        public override SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            try
            {
                this.CommunicationObject.ThrowIfClosedOrNotOpen();
                ValidateOutgoingState(message);
                if (!this.RequiresOutgoingSecurityProcessing && message.Properties.Security == null)
                {
                    return null;
                }
                SecurityProtocolCorrelationState newCorrelationState = SecureOutgoingMessageCore(ref message, timeout, correlationState);
                base.OnOutgoingMessageSecured(message);
                return newCorrelationState;
            }
            catch (Exception exception)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(exception)) throw;

                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        protected abstract SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState);

        void ValidateOutgoingState(Message message)
        {
            if (this.PerformIncomingAndOutgoingMessageExpectationChecks && !this.factory.ExpectOutgoingMessages)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityBindingNotSetUpToProcessOutgoingMessages)));
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
        }

        public override void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
        {
            try
            {
                this.CommunicationObject.ThrowIfClosedOrNotOpen();
                if (this.PerformIncomingAndOutgoingMessageExpectationChecks && !factory.ExpectIncomingMessages)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityBindingNotSetUpToProcessIncomingMessages)));
                }
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                if (!this.RequiresIncomingSecurityProcessing(message))
                {
                    return;
                }
                string actor = string.Empty; // message.Version.Envelope.UltimateDestinationActor;
                VerifyIncomingMessageCore(ref message, actor, timeout, null);
                base.OnIncomingMessageVerified(message);
            }
            catch (MessageSecurityException e)
            {
                base.OnVerifyIncomingMessageFailure(message, e);
                throw;
            }
            catch (Exception e)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(e)) throw;

                base.OnVerifyIncomingMessageFailure(message, e);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MessageSecurityVerificationFailed), e));
            }
        }

        public override SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout, params SecurityProtocolCorrelationState[] correlationStates)
        {
            try
            {
                this.CommunicationObject.ThrowIfClosedOrNotOpen();
                if (this.PerformIncomingAndOutgoingMessageExpectationChecks && !factory.ExpectIncomingMessages)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityBindingNotSetUpToProcessIncomingMessages)));
                }
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                if (!this.RequiresIncomingSecurityProcessing(message))
                {
                    return null;
                }
                string actor = string.Empty; // message.Version.Envelope.UltimateDestinationActor;
                SecurityProtocolCorrelationState newCorrelationState = VerifyIncomingMessageCore(ref message, actor, timeout, correlationStates);
                base.OnIncomingMessageVerified(message);
                return newCorrelationState;
            }
            catch (MessageSecurityException e)
            {
                base.OnVerifyIncomingMessageFailure(message, e);
                throw;
            }
            catch (Exception e)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(e)) throw;

                base.OnVerifyIncomingMessageFailure(message, e);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MessageSecurityVerificationFailed), e));
            }
        }

        protected abstract SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates);

        internal SecurityProtocolCorrelationState GetSignatureConfirmationCorrelationState(SecurityProtocolCorrelationState oldCorrelationState, SecurityProtocolCorrelationState newCorrelationState)
        {
            if (this.factory.ActAsInitiator)
            {
                return newCorrelationState;
            }
            else
            {
                return oldCorrelationState;
            }
        }

        protected abstract class GetOneTokenAndSetUpSecurityAsyncResult : GetSupportingTokensAsyncResult
        {
            readonly MessageSecurityProtocol binding;
            readonly SecurityTokenProvider provider;
            Message message;
            readonly bool doIdentityChecks;
            SecurityTokenAuthenticator identityCheckAuthenticator;
            static AsyncCallback getTokenCompleteCallback = Fx.ThunkCallback(new AsyncCallback(GetTokenCompleteCallback));
            SecurityProtocolCorrelationState newCorrelationState;
            SecurityProtocolCorrelationState oldCorrelationState;
            TimeoutHelper timeoutHelper;

            public GetOneTokenAndSetUpSecurityAsyncResult(Message m, MessageSecurityProtocol binding, SecurityTokenProvider provider,
                bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator, SecurityProtocolCorrelationState oldCorrelationState, TimeSpan timeout, AsyncCallback callback, object state)
                : base(m, binding, timeout, callback, state)
            {
                this.message = m;
                this.binding = binding;
                this.provider = provider;
                this.doIdentityChecks = doIdentityChecks;
                this.oldCorrelationState = oldCorrelationState;
                this.identityCheckAuthenticator = identityCheckAuthenticator;
            }

            protected MessageSecurityProtocol Binding
            {
                get { return this.binding; }
            }

            protected SecurityProtocolCorrelationState NewCorrelationState
            {
                get { return this.newCorrelationState; }
            }

            protected SecurityProtocolCorrelationState OldCorrelationState
            {
                get { return this.oldCorrelationState; }
            }

            internal static Message End(IAsyncResult result, out SecurityProtocolCorrelationState newCorrelationState)
            {
                GetOneTokenAndSetUpSecurityAsyncResult self = AsyncResult.End<GetOneTokenAndSetUpSecurityAsyncResult>(result);
                newCorrelationState = self.newCorrelationState;
                return self.message;
            }

            bool OnGetTokenComplete(SecurityToken token)
            {
                if (token == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenProviderCannotGetTokensForTarget, this.binding.Target)));
                }
                if (this.doIdentityChecks)
                {
                    this.binding.EnsureOutgoingIdentity(token, this.identityCheckAuthenticator);
                }
                OnGetTokenDone(ref this.message, token, timeoutHelper.RemainingTime());
                return true;
            }

            protected abstract void OnGetTokenDone(ref Message message, SecurityToken token, TimeSpan timeout);

            static void GetTokenCompleteCallback(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }
                if (result.CompletedSynchronously)
                {
                    return;
                }
                GetOneTokenAndSetUpSecurityAsyncResult self = result.AsyncState as GetOneTokenAndSetUpSecurityAsyncResult;
                if (self == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.InvalidAsyncResult));
                }
                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    SecurityToken token = self.provider.EndGetToken(result);
                    completeSelf = self.OnGetTokenComplete(token);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    // Always immediately rethrow fatal exceptions.
                    if (Fx.IsFatal(e)) throw;

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            protected void SetCorrelationToken(SecurityToken token)
            {
                newCorrelationState = new SecurityProtocolCorrelationState(token);
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                IAsyncResult result = this.provider.BeginGetToken(timeoutHelper.RemainingTime(), getTokenCompleteCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;

                }
                SecurityToken token = this.provider.EndGetToken(result);
                return this.OnGetTokenComplete(token);
            }
        }

        // note: identity check done only on token obtained from first
        // token provider; either or both token providers may be null;
        // get token calls are skipped for null providers.
        protected abstract class GetTwoTokensAndSetUpSecurityAsyncResult : GetSupportingTokensAsyncResult
        {
            readonly MessageSecurityProtocol binding;
            readonly SecurityTokenProvider primaryProvider;
            readonly SecurityTokenProvider secondaryProvider;
            Message message;
            readonly bool doIdentityChecks;
            SecurityTokenAuthenticator identityCheckAuthenticator;
            SecurityToken primaryToken;
            static readonly AsyncCallback getPrimaryTokenCompleteCallback = Fx.ThunkCallback(new AsyncCallback(GetPrimaryTokenCompleteCallback));
            static readonly AsyncCallback getSecondaryTokenCompleteCallback = Fx.ThunkCallback(new AsyncCallback(GetSecondaryTokenCompleteCallback));
            SecurityProtocolCorrelationState newCorrelationState;
            SecurityProtocolCorrelationState oldCorrelationState;
            TimeoutHelper timeoutHelper;

            public GetTwoTokensAndSetUpSecurityAsyncResult(Message m, MessageSecurityProtocol binding,
                SecurityTokenProvider primaryProvider, SecurityTokenProvider secondaryProvider, bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator,
                SecurityProtocolCorrelationState oldCorrelationState,
                TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(m, binding, timeout, callback, state)
            {
                this.message = m;
                this.binding = binding;
                this.primaryProvider = primaryProvider;
                this.secondaryProvider = secondaryProvider;
                this.doIdentityChecks = doIdentityChecks;
                this.identityCheckAuthenticator = identityCheckAuthenticator;
                this.oldCorrelationState = oldCorrelationState;
            }

            protected MessageSecurityProtocol Binding
            {
                get { return this.binding; }
            }

            protected SecurityProtocolCorrelationState NewCorrelationState
            {
                get { return this.newCorrelationState; }
            }

            protected SecurityProtocolCorrelationState OldCorrelationState
            {
                get { return this.oldCorrelationState; }
            }

            internal static Message End(IAsyncResult result, out SecurityProtocolCorrelationState newCorrelationState)
            {
                GetTwoTokensAndSetUpSecurityAsyncResult self = AsyncResult.End<GetTwoTokensAndSetUpSecurityAsyncResult>(result);
                newCorrelationState = self.newCorrelationState;
                return self.message;
            }

            bool OnGetPrimaryTokenComplete(SecurityToken token)
            {
                return OnGetPrimaryTokenComplete(token, false);
            }

            bool OnGetPrimaryTokenComplete(SecurityToken token, bool primaryCallSkipped)
            {
                if (!primaryCallSkipped)
                {
                    if (token == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenProviderCannotGetTokensForTarget, this.binding.Target)), this.message);
                    }
                    if (this.doIdentityChecks)
                    {
                        this.binding.EnsureOutgoingIdentity(token, this.identityCheckAuthenticator);
                    }
                }
                this.primaryToken = token;

                if (this.secondaryProvider == null)
                {
                    return this.OnGetSecondaryTokenComplete(null, true);
                }
                else
                {
                    IAsyncResult result = this.secondaryProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), getSecondaryTokenCompleteCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    SecurityToken token2 = this.secondaryProvider.EndGetToken(result);
                    return this.OnGetSecondaryTokenComplete(token2);
                }
            }

            bool OnGetSecondaryTokenComplete(SecurityToken token)
            {
                return OnGetSecondaryTokenComplete(token, false);
            }

            bool OnGetSecondaryTokenComplete(SecurityToken token, bool secondaryCallSkipped)
            {
                if (!secondaryCallSkipped && token == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenProviderCannotGetTokensForTarget, this.binding.Target)), this.message);
                }
                OnBothGetTokenCallsDone(ref this.message, this.primaryToken, token, timeoutHelper.RemainingTime());
                return true;
            }

            protected abstract void OnBothGetTokenCallsDone(ref Message message, SecurityToken primaryToken, SecurityToken secondaryToken, TimeSpan timeout);

            static void GetPrimaryTokenCompleteCallback(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }
                if (result.CompletedSynchronously)
                {
                    return;
                }
                GetTwoTokensAndSetUpSecurityAsyncResult self = result.AsyncState as GetTwoTokensAndSetUpSecurityAsyncResult;
                if (self == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.InvalidAsyncResult));
                }
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    SecurityToken token = self.primaryProvider.EndGetToken(result);
                    completeSelf = self.OnGetPrimaryTokenComplete(token);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    // Always immediately rethrow fatal exceptions.
                    if (Fx.IsFatal(e)) throw;

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            static void GetSecondaryTokenCompleteCallback(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }
                if (result.CompletedSynchronously)
                {
                    return;
                }
                GetTwoTokensAndSetUpSecurityAsyncResult self = result.AsyncState as GetTwoTokensAndSetUpSecurityAsyncResult;
                if (self == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.InvalidAsyncResult));
                }
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    SecurityToken token = self.secondaryProvider.EndGetToken(result);
                    completeSelf = self.OnGetSecondaryTokenComplete(token);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    // Always immediately rethrow fatal exceptions.
                    if (Fx.IsFatal(e)) throw;

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            protected void SetCorrelationToken(SecurityToken token)
            {
                newCorrelationState = new SecurityProtocolCorrelationState(token);
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool completeSelf = false;
                if (this.primaryProvider == null)
                {
                    completeSelf = this.OnGetPrimaryTokenComplete(null);
                }
                else
                {
                    IAsyncResult result = this.primaryProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), getPrimaryTokenCompleteCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        SecurityToken token = this.primaryProvider.EndGetToken(result);
                        completeSelf = this.OnGetPrimaryTokenComplete(token);
                    }
                }
                return completeSelf;
            }
        }
    }
}
