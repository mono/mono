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

    class TransportSecurityProtocol : SecurityProtocol
    {
        public TransportSecurityProtocol(TransportSecurityProtocolFactory factory, EndpointAddress target, Uri via)
            : base(factory, target, via)
        {
        }

        public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            string actor = string.Empty; // message.Version.Envelope.UltimateDestinationActor;
            try
            {
                if (this.SecurityProtocolFactory.ActAsInitiator)
                {
                    SecureOutgoingMessageAtInitiator(ref message, actor, timeout);
                }
                else
                {
                    SecureOutgoingMessageAtResponder(ref message, actor);
                }
                base.OnOutgoingMessageSecured(message);
            }
            catch
            {
                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        protected virtual void SecureOutgoingMessageAtInitiator(ref Message message, string actor, TimeSpan timeout)
        {
            IList<SupportingTokenSpecification> supportingTokens;
            TryGetSupportingTokens(this.SecurityProtocolFactory, this.Target, this.Via, message, timeout, true, out supportingTokens);
            SetUpDelayedSecurityExecution(ref message, actor, supportingTokens);
        }

        protected void SecureOutgoingMessageAtResponder(ref Message message, string actor)
        {
            if (this.SecurityProtocolFactory.AddTimestamp && !this.SecurityProtocolFactory.SecurityBindingElement.EnableUnsecuredResponse)
            {
                SendSecurityHeader securityHeader = CreateSendSecurityHeaderForTransportProtocol(message, actor, this.SecurityProtocolFactory);
                message = securityHeader.SetupExecution();
            }
        }

        internal void SetUpDelayedSecurityExecution(ref Message message, string actor,
            IList<SupportingTokenSpecification> supportingTokens)
        {
            SendSecurityHeader securityHeader = CreateSendSecurityHeaderForTransportProtocol(message, actor, this.SecurityProtocolFactory);
            AddSupportingTokens(securityHeader, supportingTokens);
            message = securityHeader.SetupExecution();
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            string actor = string.Empty; // message.Version.Envelope.UltimateDestinationActor;
            try
            {
                if (this.SecurityProtocolFactory.ActAsInitiator)
                {
                    return this.BeginSecureOutgoingMessageAtInitiatorCore(message, actor, timeout, callback, state);
                }
                else
                {
                    SecureOutgoingMessageAtResponder(ref message, actor);
                    return new CompletedAsyncResult<Message>(message, callback, state);
                }
            }
            catch (Exception exception)
            {
                // Always immediately rethrow fatal exceptions.
                if (Fx.IsFatal(exception)) throw;

                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginSecureOutgoingMessage(message, timeout, null, callback, state);
        }

        protected virtual IAsyncResult BeginSecureOutgoingMessageAtInitiatorCore(Message message, string actor, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IList<SupportingTokenSpecification> supportingTokens;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (TryGetSupportingTokens(this.SecurityProtocolFactory, this.Target, this.Via, message, timeoutHelper.RemainingTime(), false, out supportingTokens))
            {
                SetUpDelayedSecurityExecution(ref message, actor, supportingTokens);
                return new CompletedAsyncResult<Message>(message, callback, state);
            }
            else
            {
                return new SecureOutgoingMessageAsyncResult(actor, message, this, timeout, callback, state);
            }
        }

        protected virtual Message EndSecureOutgoingMessageAtInitiatorCore(IAsyncResult result)
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

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
        {
            SecurityProtocolCorrelationState dummyState;
            this.EndSecureOutgoingMessage(result, out message, out dummyState);
        }

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            newCorrelationState = null;
            try
            {
                if (result is CompletedAsyncResult<Message>)
                {
                    message = CompletedAsyncResult<Message>.End(result);
                }
                else
                {
                    message = this.EndSecureOutgoingMessageAtInitiatorCore(result);
                }
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

        public sealed override void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            try
            {
                VerifyIncomingMessageCore(ref message, timeout);
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

        protected void AttachRecipientSecurityProperty(Message message, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens,
           IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            SecurityMessageProperty security = SecurityMessageProperty.GetOrCreate(message);
            AddSupportingTokenSpecification(security, basicTokens, endorsingTokens, signedEndorsingTokens, signedTokens, tokenPoliciesMapping);
            security.ServiceSecurityContext = new ServiceSecurityContext(security.GetInitiatorTokenAuthorizationPolicies());
        }

        protected virtual void VerifyIncomingMessageCore(ref Message message, TimeSpan timeout)
        {
            TransportSecurityProtocolFactory factory = (TransportSecurityProtocolFactory)this.SecurityProtocolFactory;
            string actor = string.Empty; // message.Version.Envelope.UltimateDestinationActor;

            ReceiveSecurityHeader securityHeader = factory.StandardsManager.TryCreateReceiveSecurityHeader(message, actor,
                factory.IncomingAlgorithmSuite, (factory.ActAsInitiator) ? MessageDirection.Output : MessageDirection.Input);
            bool expectBasicTokens;
            bool expectEndorsingTokens;
            bool expectSignedTokens;
            IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators = factory.GetSupportingTokenAuthenticators(message.Headers.Action,
                out expectSignedTokens, out expectBasicTokens, out expectEndorsingTokens);
            if (securityHeader == null)
            {
                bool expectSupportingTokens = expectEndorsingTokens || expectSignedTokens || expectBasicTokens;
                if ((factory.ActAsInitiator && (!factory.AddTimestamp || factory.SecurityBindingElement.EnableUnsecuredResponse))
                    || (!factory.ActAsInitiator && !factory.AddTimestamp && !expectSupportingTokens))
                {
                    return;
                }
                else
                {
                    if (String.IsNullOrEmpty(actor))
                        throw System.ServiceModel.Diagnostics.TraceUtility.ThrowHelperError(new MessageSecurityException(
                            SR.GetString(SR.UnableToFindSecurityHeaderInMessageNoActor)), message);
                    else
                        throw System.ServiceModel.Diagnostics.TraceUtility.ThrowHelperError(new MessageSecurityException(
                            SR.GetString(SR.UnableToFindSecurityHeaderInMessage, actor)), message);
                }
            }

            securityHeader.RequireMessageProtection = false;
            securityHeader.ExpectBasicTokens = expectBasicTokens;
            securityHeader.ExpectSignedTokens = expectSignedTokens;
            securityHeader.ExpectEndorsingTokens = expectEndorsingTokens;
            securityHeader.MaxReceivedMessageSize = factory.SecurityBindingElement.MaxReceivedMessageSize;
            securityHeader.ReaderQuotas = factory.SecurityBindingElement.ReaderQuotas;

            // Due to compatibility, only honor this setting if this app setting is enabled
            if (ServiceModelAppSettings.UseConfiguredTransportSecurityHeaderLayout)
            {
                securityHeader.Layout = factory.SecurityHeaderLayout;
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!factory.ActAsInitiator)
            {
                securityHeader.ConfigureTransportBindingServerReceiveHeader(supportingAuthenticators);
                securityHeader.ConfigureOutOfBandTokenResolver(MergeOutOfBandResolvers(supportingAuthenticators, EmptyReadOnlyCollection<SecurityTokenResolver>.Instance));
                if (factory.ExpectKeyDerivation)
                {
                    securityHeader.DerivedTokenAuthenticator = factory.DerivedKeyTokenAuthenticator;
                }
            }
            securityHeader.ReplayDetectionEnabled = factory.DetectReplays;
            securityHeader.SetTimeParameters(factory.NonceCache, factory.ReplayWindow, factory.MaxClockSkew);
            securityHeader.Process(timeoutHelper.RemainingTime(), SecurityUtils.GetChannelBindingFromMessage(message), factory.ExtendedProtectionPolicy);
            message = securityHeader.ProcessedMessage;
            if (!factory.ActAsInitiator)
            {
                AttachRecipientSecurityProperty(message, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens,
                    securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
            }

            base.OnIncomingMessageVerified(message);
        }

        sealed class SecureOutgoingMessageAsyncResult : GetSupportingTokensAsyncResult
        {
            Message message;
            string actor;
            TransportSecurityProtocol binding;

            public SecureOutgoingMessageAsyncResult(string actor, Message message, TransportSecurityProtocol binding, TimeSpan timeout, AsyncCallback callback, object state)
                : base(message, binding, timeout, callback, state)
            {
                this.actor = actor;
                this.message = message;
                this.binding = binding;
                this.Start();
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.binding.SetUpDelayedSecurityExecution(ref this.message, this.actor, this.SupportingTokens);
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
