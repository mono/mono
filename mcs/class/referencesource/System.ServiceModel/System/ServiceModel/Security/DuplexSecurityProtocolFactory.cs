//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    sealed class DuplexSecurityProtocolFactory : SecurityProtocolFactory
    {
        SecurityProtocolFactory forwardProtocolFactory;
        SecurityProtocolFactory reverseProtocolFactory;
        bool requireSecurityOnBothDuplexDirections = true;

        public DuplexSecurityProtocolFactory()
            : base()
        {
        }

        public DuplexSecurityProtocolFactory(SecurityProtocolFactory forwardProtocolFactory, SecurityProtocolFactory reverseProtocolFactory)
            : this()
        {
            this.forwardProtocolFactory = forwardProtocolFactory;
            this.reverseProtocolFactory = reverseProtocolFactory;
        }

        public SecurityProtocolFactory ForwardProtocolFactory
        {
            get
            {
                return this.forwardProtocolFactory;
            }
            set
            {
                ThrowIfImmutable();
                this.forwardProtocolFactory = value;
            }
        }

        SecurityProtocolFactory ProtocolFactoryForIncomingMessages
        {
            get
            {
                return this.ActAsInitiator ? this.ReverseProtocolFactory : this.ForwardProtocolFactory;
            }
        }

        SecurityProtocolFactory ProtocolFactoryForOutgoingMessages
        {
            get
            {
                return this.ActAsInitiator ? this.ForwardProtocolFactory : this.ReverseProtocolFactory;
            }
        }

        // If RequireSecurityOnBothDuplexDirections is set to false,
        // one or both among ForwardProtocolFactory and
        // ReverseProtocolFactory will be allowed to be null.  The
        // message directions corresponding to the null
        // ProtocolFactory will have no security applied or verified.
        // This mode may be used for GetPolicy message exchanges, for
        // example.
        public bool RequireSecurityOnBothDuplexDirections
        {
            get
            {
                return this.requireSecurityOnBothDuplexDirections;
            }
            set
            {
                ThrowIfImmutable();
                this.requireSecurityOnBothDuplexDirections = value;
            }
        }

        public SecurityProtocolFactory ReverseProtocolFactory
        {
            get
            {
                return this.reverseProtocolFactory;
            }
            set
            {
                ThrowIfImmutable();
                this.reverseProtocolFactory = value;
            }
        }

        public override bool SupportsDuplex
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsReplayDetection
        {
            get
            {
                return this.ForwardProtocolFactory != null && this.ForwardProtocolFactory.SupportsReplayDetection &&
                    this.ReverseProtocolFactory != null && this.ReverseProtocolFactory.SupportsReplayDetection;
            }
        }

        public override bool SupportsRequestReply
        {
            get
            {
                return false;
            }
        }

        public override EndpointIdentity GetIdentityOfSelf()
        {
            SecurityProtocolFactory factory = this.ProtocolFactoryForIncomingMessages;
            if (factory != null)
            {
                return factory.GetIdentityOfSelf();
            }
            else
            {
                return base.GetIdentityOfSelf();
            }
        }

        public override void OnAbort()
        {
            if (this.forwardProtocolFactory != null)
            {
                this.forwardProtocolFactory.Close(true, TimeSpan.Zero);
            }
            if (this.reverseProtocolFactory != null)
            {
                this.reverseProtocolFactory.Close(true, TimeSpan.Zero);
            }
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.forwardProtocolFactory != null)
            {
                this.forwardProtocolFactory.Close(false, timeoutHelper.RemainingTime());
            }
            if (this.reverseProtocolFactory != null)
            {
                this.reverseProtocolFactory.Close(false, timeoutHelper.RemainingTime());
            }
            // no need to the close the base as it has no settings.
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            SecurityProtocolFactory outgoingFactory = this.ProtocolFactoryForOutgoingMessages;
            SecurityProtocolFactory incomingFactory = this.ProtocolFactoryForIncomingMessages;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            SecurityProtocol outgoing = outgoingFactory == null ? null : outgoingFactory.CreateSecurityProtocol(target, via, listenerSecurityState, false, timeoutHelper.RemainingTime());
            SecurityProtocol incoming = incomingFactory == null ? null : incomingFactory.CreateSecurityProtocol(null, null, listenerSecurityState, false, timeoutHelper.RemainingTime());
            return new DuplexSecurityProtocol(outgoing, incoming);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.ForwardProtocolFactory != null && ReferenceEquals(this.ForwardProtocolFactory, this.ReverseProtocolFactory))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ReverseProtocolFactory",
                    SR.GetString(SR.SameProtocolFactoryCannotBeSetForBothDuplexDirections));
            }
            if (this.forwardProtocolFactory != null)
            {
                this.forwardProtocolFactory.ListenUri = this.ListenUri;
            }
            if (this.reverseProtocolFactory != null)
            {
                this.reverseProtocolFactory.ListenUri = this.ListenUri;
            }
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Open(this.ForwardProtocolFactory, this.ActAsInitiator, "ForwardProtocolFactory", timeoutHelper.RemainingTime());
            Open(this.ReverseProtocolFactory, !this.ActAsInitiator, "ReverseProtocolFactory", timeoutHelper.RemainingTime());
            // no need to the open the base as it has no settings.
        }

        void Open(SecurityProtocolFactory factory, bool actAsInitiator, string propertyName, TimeSpan timeout)
        {
            if (factory != null)
            {
                factory.Open(actAsInitiator, timeout);
            }
            else if (this.RequireSecurityOnBothDuplexDirections)
            {
                OnPropertySettingsError(propertyName, true);
            }
        }

        sealed class DuplexSecurityProtocol : SecurityProtocol
        {
            readonly SecurityProtocol outgoingProtocol;
            readonly SecurityProtocol incomingProtocol;

            public DuplexSecurityProtocol(SecurityProtocol outgoingProtocol, SecurityProtocol incomingProtocol)
                : base(incomingProtocol.SecurityProtocolFactory, null, null)
            {
                this.outgoingProtocol = outgoingProtocol;
                this.incomingProtocol = incomingProtocol;
            }

            public override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.outgoingProtocol.Open(timeoutHelper.RemainingTime());
                this.incomingProtocol.Open(timeoutHelper.RemainingTime());
            }

            public override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.outgoingProtocol.Close(false, timeoutHelper.RemainingTime());
                this.incomingProtocol.Close(false, timeoutHelper.RemainingTime());
            }

            public override void OnAbort()
            {
                this.outgoingProtocol.Close(true, TimeSpan.Zero);
                this.incomingProtocol.Close(true, TimeSpan.Zero);
            }

            public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (this.outgoingProtocol != null)
                {
                    return this.outgoingProtocol.BeginSecureOutgoingMessage(message, timeout, callback, state);
                }
                else
                {
                    return new CompletedAsyncResult<Message>(message, callback, state);
                }
            }

            public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState,
                AsyncCallback callback, object state)
            {
                if (this.outgoingProtocol != null)
                {
                    return this.outgoingProtocol.BeginSecureOutgoingMessage(message, timeout, correlationState, callback, state);
                }
                else
                {
                    return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, null, callback, state);
                }
            }

            public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
            {
                if (this.outgoingProtocol != null)
                {
                    this.outgoingProtocol.EndSecureOutgoingMessage(result, out message);
                }
                else
                {
                    message = CompletedAsyncResult<Message>.End(result);
                }
            }

            public override void EndSecureOutgoingMessage(IAsyncResult result,
                out Message message, out SecurityProtocolCorrelationState newCorrelationState)
            {
                if (this.outgoingProtocol != null)
                {
                    this.outgoingProtocol.EndSecureOutgoingMessage(result, out message, out newCorrelationState);
                }
                else
                {
                    message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
                }
            }

            public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
            {
                if (this.outgoingProtocol != null)
                {
                    this.outgoingProtocol.SecureOutgoingMessage(ref message, timeout);
                }
            }

            public override SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                if (this.outgoingProtocol != null)
                {
                    return this.outgoingProtocol.SecureOutgoingMessage(ref message, timeout, correlationState);
                }
                else
                {
                    return null;
                }
            }

            public override void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
            {
                if (this.incomingProtocol != null)
                {
                    this.incomingProtocol.VerifyIncomingMessage(ref message, timeout);
                }
            }

            public override SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout,
                params SecurityProtocolCorrelationState[] correlationStates)
            {
                if (this.incomingProtocol != null)
                {
                    return this.incomingProtocol.VerifyIncomingMessage(ref message, timeout, correlationStates);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
