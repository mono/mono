//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.IO;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;
    using System.Net.Security;

    abstract class MessageSecurityProtocolFactory : SecurityProtocolFactory
    {
        internal const MessageProtectionOrder defaultMessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
        internal const bool defaultDoRequestSignatureConfirmation = false;

        bool applyIntegrity = true;
        bool applyConfidentiality = true;
        bool doRequestSignatureConfirmation = defaultDoRequestSignatureConfirmation;
        IdentityVerifier identityVerifier;
        ChannelProtectionRequirements protectionRequirements = new ChannelProtectionRequirements();
        MessageProtectionOrder messageProtectionOrder = defaultMessageProtectionOrder;
        bool requireIntegrity = true;
        bool requireConfidentiality = true;
        List<SecurityTokenAuthenticator> wrappedKeyTokenAuthenticator;

        protected MessageSecurityProtocolFactory()
        {
        }

        internal MessageSecurityProtocolFactory(MessageSecurityProtocolFactory factory)
            : base(factory)
        {
            if (factory == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("factory");

            this.applyIntegrity = factory.applyIntegrity;
            this.applyConfidentiality = factory.applyConfidentiality;
            this.identityVerifier = factory.identityVerifier;
            this.protectionRequirements = new ChannelProtectionRequirements(factory.protectionRequirements);
            this.messageProtectionOrder = factory.messageProtectionOrder;
            this.requireIntegrity = factory.requireIntegrity;
            this.requireConfidentiality = factory.requireConfidentiality;
            this.doRequestSignatureConfirmation = factory.doRequestSignatureConfirmation;
        }

        public bool ApplyConfidentiality
        {
            get
            {
                return this.applyConfidentiality;
            }
            set
            {
                ThrowIfImmutable();
                this.applyConfidentiality = value;
            }
        }

        public bool ApplyIntegrity
        {
            get
            {
                return this.applyIntegrity;
            }
            set
            {
                ThrowIfImmutable();
                this.applyIntegrity = value;
            }
        }

        public bool DoRequestSignatureConfirmation
        {
            get
            {
                return this.doRequestSignatureConfirmation;
            }
            set
            {
                ThrowIfImmutable();
                this.doRequestSignatureConfirmation = value;
            }
        }

        public IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
            set
            {
                ThrowIfImmutable();
                this.identityVerifier = value;
            }
        }

        public ChannelProtectionRequirements ProtectionRequirements
        {
            get
            {
                return this.protectionRequirements;
            }
        }

        public MessageProtectionOrder MessageProtectionOrder
        {
            get
            {
                return this.messageProtectionOrder;
            }
            set
            {
                ThrowIfImmutable();
                this.messageProtectionOrder = value;
            }
        }

        public bool RequireIntegrity
        {
            get
            {
                return this.requireIntegrity;
            }
            set
            {
                ThrowIfImmutable();
                this.requireIntegrity = value;
            }
        }

        public bool RequireConfidentiality
        {
            get
            {
                return this.requireConfidentiality;
            }
            set
            {
                ThrowIfImmutable();
                this.requireConfidentiality = value;
            }
        }

        internal List<SecurityTokenAuthenticator> WrappedKeySecurityTokenAuthenticator
        {
            get
            {
                return this.wrappedKeyTokenAuthenticator;
            }
        }

        protected virtual void ValidateCorrelationSecuritySettings()
        {
            if (this.ActAsInitiator && this.SupportsRequestReply)
            {
                bool savesCorrelationTokenOnRequest = this.ApplyIntegrity || this.ApplyConfidentiality;
                bool needsCorrelationTokenOnReply = this.RequireIntegrity || this.RequireConfidentiality;
                if (!savesCorrelationTokenOnRequest && needsCorrelationTokenOnReply)
                {
                    OnPropertySettingsError("ApplyIntegrity", false);
                }
            }
        }

        public override void OnOpen(TimeSpan timeout)
        {
            base.OnOpen(timeout);
            this.protectionRequirements.MakeReadOnly();

            if (this.DetectReplays && !this.RequireIntegrity)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("RequireIntegrity", SR.GetString(SR.ForReplayDetectionToBeDoneRequireIntegrityMustBeSet));
            }

            if (this.DoRequestSignatureConfirmation)
            {
                if (!this.SupportsRequestReply)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SignatureConfirmationRequiresRequestReply));
                }
                if (!this.StandardsManager.SecurityVersion.SupportsSignatureConfirmation)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SecurityVersionDoesNotSupportSignatureConfirmation, this.StandardsManager.SecurityVersion));
                }
            }

            this.wrappedKeyTokenAuthenticator = new List<SecurityTokenAuthenticator>(1);
            SecurityTokenAuthenticator authenticator = new NonValidatingSecurityTokenAuthenticator<WrappedKeySecurityToken>();
            this.wrappedKeyTokenAuthenticator.Add(authenticator);

            ValidateCorrelationSecuritySettings();
        }

        static MessagePartSpecification ExtractMessageParts(string action,
            ScopedMessagePartSpecification scopedParts, bool isForSignature)
        {
            MessagePartSpecification parts = null;

            if (scopedParts.TryGetParts(action, out parts))
            {
                return parts;
            }
            else if (scopedParts.TryGetParts(MessageHeaders.WildcardAction, out parts))
            {
                return parts;
            }

            // send back a fault indication that the action is unknown
            SecurityVersion wss = MessageSecurityVersion.Default.SecurityVersion;
            FaultCode subCode = new FaultCode(wss.InvalidSecurityFaultCode.Value, wss.HeaderNamespace.Value);
            FaultCode senderCode = FaultCode.CreateSenderFaultCode(subCode);
            FaultReason reason = new FaultReason(SR.GetString(SR.InvalidOrUnrecognizedAction, action), System.Globalization.CultureInfo.CurrentCulture);
            MessageFault fault = MessageFault.CreateFault(senderCode, reason);
            if (isForSignature)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoSignaturePartsSpecified, action), null, fault));
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoEncryptionPartsSpecified, action), null, fault));
            }
        }

        internal MessagePartSpecification GetIncomingEncryptionParts(string action)
        {
            if (this.RequireConfidentiality)
            {
                //return ExtractMessageParts(action, (this.SecurityTokenManager is ClientCredentialsSecurityTokenManager) ? this.ProtectionRequirements.OutgoingEncryptionParts : this.ProtectionRequirements.IncomingEncryptionParts, false);

                if (this.IsDuplexReply)
                    return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingEncryptionParts, false);
                else
                    return ExtractMessageParts(action, (this.ActAsInitiator) ? this.ProtectionRequirements.OutgoingEncryptionParts : this.ProtectionRequirements.IncomingEncryptionParts, false);
            }
            else
            {
                return MessagePartSpecification.NoParts;
            }
        }

        internal MessagePartSpecification GetIncomingSignatureParts(string action)
        {
            if (this.RequireIntegrity)
            {
                //return ExtractMessageParts(action, (this.SecurityTokenManager is ClientCredentialsSecurityTokenManager) ? this.ProtectionRequirements.OutgoingSignatureParts : this.ProtectionRequirements.IncomingSignatureParts, true);
                if (this.IsDuplexReply)
                    return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingSignatureParts, true);
                else
                    return ExtractMessageParts(action, (this.ActAsInitiator) ? this.ProtectionRequirements.OutgoingSignatureParts : this.ProtectionRequirements.IncomingSignatureParts, true);
            }
            else
            {
                return MessagePartSpecification.NoParts;
            }
        }

        internal MessagePartSpecification GetOutgoingEncryptionParts(string action)
        {
            if (this.ApplyConfidentiality)
            {
                //return ExtractMessageParts(action, (this.SecurityTokenManager is ClientCredentialsSecurityTokenManager) ? this.ProtectionRequirements.IncomingEncryptionParts : this.ProtectionRequirements.OutgoingEncryptionParts, false);
                if (this.IsDuplexReply)
                    return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingEncryptionParts, false);
                else
                    return ExtractMessageParts(action, (this.ActAsInitiator) ? this.ProtectionRequirements.IncomingEncryptionParts : this.ProtectionRequirements.OutgoingEncryptionParts, false);
            }
            else
            {
                return MessagePartSpecification.NoParts;
            }
        }

        internal MessagePartSpecification GetOutgoingSignatureParts(string action)
        {
            if (this.ApplyIntegrity)
            {
                //return ExtractMessageParts(action, (this.SecurityTokenManager is ClientCredentialsSecurityTokenManager) ? this.ProtectionRequirements.IncomingSignatureParts : this.ProtectionRequirements.OutgoingSignatureParts, true);
                if (this.IsDuplexReply)
                    return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingSignatureParts, true);
                else
                    return ExtractMessageParts(action, (this.ActAsInitiator) ? this.ProtectionRequirements.IncomingSignatureParts : this.ProtectionRequirements.OutgoingSignatureParts, true);
            }
            else
            {
                return MessagePartSpecification.NoParts;
            }
        }
    }
}
