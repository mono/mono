//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    using System.Net.Security;
    using System.Text;

    public sealed class AsymmetricSecurityBindingElement : SecurityBindingElement, IPolicyExportExtension
    {
        internal const bool defaultAllowSerializedSigningTokenOnReply = false;

        bool allowSerializedSigningTokenOnReply;
        SecurityTokenParameters initiatorTokenParameters;
        MessageProtectionOrder messageProtectionOrder;
        SecurityTokenParameters recipientTokenParameters;
        bool requireSignatureConfirmation;
        bool isCertificateSignatureBinding;

        AsymmetricSecurityBindingElement(AsymmetricSecurityBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            if (elementToBeCloned.initiatorTokenParameters != null)
                this.initiatorTokenParameters = (SecurityTokenParameters)elementToBeCloned.initiatorTokenParameters.Clone();
            this.messageProtectionOrder = elementToBeCloned.messageProtectionOrder;
            if (elementToBeCloned.recipientTokenParameters != null)
                this.recipientTokenParameters = (SecurityTokenParameters)elementToBeCloned.recipientTokenParameters.Clone();
            this.requireSignatureConfirmation = elementToBeCloned.requireSignatureConfirmation;
            this.allowSerializedSigningTokenOnReply = elementToBeCloned.allowSerializedSigningTokenOnReply;
            this.isCertificateSignatureBinding = elementToBeCloned.isCertificateSignatureBinding;
        }

        public AsymmetricSecurityBindingElement()
            : this(null, null)
        {
            // empty
        }

        public AsymmetricSecurityBindingElement(SecurityTokenParameters recipientTokenParameters)
            : this(recipientTokenParameters, null)
        {
            // empty
        }

        public AsymmetricSecurityBindingElement(SecurityTokenParameters recipientTokenParameters, SecurityTokenParameters initiatorTokenParameters)
            : this(recipientTokenParameters, initiatorTokenParameters, AsymmetricSecurityBindingElement.defaultAllowSerializedSigningTokenOnReply)
        {
            // empty
        }

        internal AsymmetricSecurityBindingElement(
            SecurityTokenParameters recipientTokenParameters,
            SecurityTokenParameters initiatorTokenParameters,
            bool allowSerializedSigningTokenOnReply)
            : base()
        {
            this.messageProtectionOrder = SecurityBindingElement.defaultMessageProtectionOrder;
            this.requireSignatureConfirmation = SecurityBindingElement.defaultRequireSignatureConfirmation;
            this.initiatorTokenParameters = initiatorTokenParameters;
            this.recipientTokenParameters = recipientTokenParameters;
            this.allowSerializedSigningTokenOnReply = allowSerializedSigningTokenOnReply;
            this.isCertificateSignatureBinding = false;
        }

        public bool AllowSerializedSigningTokenOnReply
        {
            get
            {
                return this.allowSerializedSigningTokenOnReply;
            }
            set
            {
                this.allowSerializedSigningTokenOnReply = value;
            }
        }

        public SecurityTokenParameters InitiatorTokenParameters
        {
            get
            {
                return this.initiatorTokenParameters;
            }
            set
            {
                this.initiatorTokenParameters = value;
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
                if (!MessageProtectionOrderHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.messageProtectionOrder = value;
            }
        }

        public SecurityTokenParameters RecipientTokenParameters
        {
            get
            {
                return this.recipientTokenParameters;
            }
            set
            {
                this.recipientTokenParameters = value;
            }
        }

        public bool RequireSignatureConfirmation
        {
            get
            {
                return this.requireSignatureConfirmation;
            }
            set
            {
                this.requireSignatureConfirmation = value;
            }
        }

        internal override ISecurityCapabilities GetIndividualISecurityCapabilities()
        {
            ProtectionLevel requestProtectionLevel = ProtectionLevel.EncryptAndSign;
            ProtectionLevel responseProtectionLevel = ProtectionLevel.EncryptAndSign;
            bool supportsServerAuthentication = false;

            if (IsCertificateSignatureBinding)
            {
                requestProtectionLevel = ProtectionLevel.Sign;
                responseProtectionLevel = ProtectionLevel.None;
            }
            else if (RecipientTokenParameters != null)
            {
                supportsServerAuthentication = RecipientTokenParameters.SupportsServerAuthentication;
            }

            bool supportsClientAuthentication;
            bool supportsClientWindowsIdentity;
            GetSupportingTokensCapabilities(out supportsClientAuthentication, out supportsClientWindowsIdentity);
            if (InitiatorTokenParameters != null)
            {
                supportsClientAuthentication = supportsClientAuthentication || InitiatorTokenParameters.SupportsClientAuthentication;
                supportsClientWindowsIdentity = supportsClientWindowsIdentity || InitiatorTokenParameters.SupportsClientWindowsIdentity;
            }

            return new SecurityCapabilities(supportsClientAuthentication, supportsServerAuthentication, supportsClientWindowsIdentity,
                requestProtectionLevel, responseProtectionLevel);
        }

        internal override bool SupportsDuplex
        {
            get { return !this.isCertificateSignatureBinding; }
        }

        internal override bool SupportsRequestReply
        {
            get
            {
                return !this.isCertificateSignatureBinding;
            }
        }

        internal bool IsCertificateSignatureBinding
        {
            get { return this.isCertificateSignatureBinding; }
            set { this.isCertificateSignatureBinding = value; }
        }

        public override void SetKeyDerivation(bool requireDerivedKeys)
        {
            base.SetKeyDerivation(requireDerivedKeys);
            if (this.initiatorTokenParameters != null)
                this.initiatorTokenParameters.RequireDerivedKeys = requireDerivedKeys;
            if (this.recipientTokenParameters != null)
                this.recipientTokenParameters.RequireDerivedKeys = requireDerivedKeys;
        }

        internal override bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            if (!base.IsSetKeyDerivation(requireDerivedKeys))
                return false;
            if (this.initiatorTokenParameters != null && this.initiatorTokenParameters.RequireDerivedKeys != requireDerivedKeys)
                return false;
            if (this.recipientTokenParameters != null && this.recipientTokenParameters.RequireDerivedKeys != requireDerivedKeys)
                return false;
            return true;
        }

        bool HasProtectionRequirements(ScopedMessagePartSpecification scopedParts)
        {
            foreach (string action in scopedParts.Actions)
            {
                MessagePartSpecification parts;
                if (scopedParts.TryGetParts(action, out parts))
                {
                    if (!parts.IsEmpty())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal override SecurityProtocolFactory CreateSecurityProtocolFactory<TChannel>(BindingContext context, SecurityCredentialsManager credentialsManager, bool isForService, BindingContext issuerBindingContext)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            if (credentialsManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("credentialsManager");

            if (this.InitiatorTokenParameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.AsymmetricSecurityBindingElementNeedsInitiatorTokenParameters, this.ToString())));
            if (this.RecipientTokenParameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.AsymmetricSecurityBindingElementNeedsRecipientTokenParameters, this.ToString())));

            bool isDuplexSecurity = !this.isCertificateSignatureBinding && (typeof(IDuplexChannel) == typeof(TChannel) || typeof(IDuplexSessionChannel) == typeof(TChannel));

            SecurityProtocolFactory protocolFactory;

            AsymmetricSecurityProtocolFactory forward = new AsymmetricSecurityProtocolFactory();
            forward.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, isForService));
            forward.RequireConfidentiality = this.HasProtectionRequirements(forward.ProtectionRequirements.IncomingEncryptionParts);
            forward.RequireIntegrity = this.HasProtectionRequirements(forward.ProtectionRequirements.IncomingSignatureParts);
            if (this.isCertificateSignatureBinding)
            {
                if (isForService)
                {
                    forward.ApplyIntegrity = forward.ApplyConfidentiality = false;
                }
                else
                {
                    forward.ApplyConfidentiality = forward.RequireIntegrity = false;
                }
            }
            else
            {
                forward.ApplyIntegrity = this.HasProtectionRequirements(forward.ProtectionRequirements.OutgoingSignatureParts);
                forward.ApplyConfidentiality = this.HasProtectionRequirements(forward.ProtectionRequirements.OutgoingEncryptionParts);
            }
            if (isForService)
            {
                base.ApplyAuditBehaviorSettings(context, forward);
                if (forward.RequireConfidentiality || (!this.isCertificateSignatureBinding && forward.ApplyIntegrity))
                {
                    forward.AsymmetricTokenParameters = (SecurityTokenParameters)this.RecipientTokenParameters.Clone();
                }
                else
                {
                    forward.AsymmetricTokenParameters = null;
                }
                forward.CryptoTokenParameters = this.InitiatorTokenParameters.Clone();
                SetIssuerBindingContextIfRequired(forward.CryptoTokenParameters, issuerBindingContext);
            }
            else
            {
                if (forward.ApplyConfidentiality || (!this.isCertificateSignatureBinding && forward.RequireIntegrity))
                {
                    forward.AsymmetricTokenParameters = (SecurityTokenParameters)this.RecipientTokenParameters.Clone();
                }
                else
                {
                    forward.AsymmetricTokenParameters = null;
                }
                forward.CryptoTokenParameters = this.InitiatorTokenParameters.Clone();
                SetIssuerBindingContextIfRequired(forward.CryptoTokenParameters, issuerBindingContext);
            }
            if (isDuplexSecurity)
            {
                if (isForService)
                {
                    forward.ApplyConfidentiality = forward.ApplyIntegrity = false;
                }
                else
                {
                    forward.RequireIntegrity = forward.RequireConfidentiality = false;
                }
            }
            else
            {
                if (!isForService)
                {
                    forward.AllowSerializedSigningTokenOnReply = this.AllowSerializedSigningTokenOnReply;
                }
            }

            forward.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
            forward.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
            forward.MessageProtectionOrder = this.MessageProtectionOrder;
            base.ConfigureProtocolFactory(forward, credentialsManager, isForService, issuerBindingContext, context.Binding);
            if (!forward.RequireIntegrity)
                forward.DetectReplays = false;

            if (isDuplexSecurity)
            {
                AsymmetricSecurityProtocolFactory reverse = new AsymmetricSecurityProtocolFactory();
                if (isForService)
                {
                    reverse.AsymmetricTokenParameters = this.InitiatorTokenParameters.Clone();
                    reverse.AsymmetricTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.External;
                    reverse.AsymmetricTokenParameters.InclusionMode = SecurityTokenInclusionMode.Never;
                    reverse.CryptoTokenParameters = (SecurityTokenParameters)this.RecipientTokenParameters.Clone();
                    reverse.CryptoTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.Internal;
                    reverse.CryptoTokenParameters.InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
                    reverse.IdentityVerifier = null;
                }
                else
                {
                    reverse.AsymmetricTokenParameters = this.InitiatorTokenParameters.Clone();
                    reverse.AsymmetricTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.External;
                    reverse.AsymmetricTokenParameters.InclusionMode = SecurityTokenInclusionMode.Never;
                    reverse.CryptoTokenParameters = (SecurityTokenParameters)this.RecipientTokenParameters.Clone();
                    reverse.CryptoTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.Internal;
                    reverse.CryptoTokenParameters.InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
                    reverse.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
                }
                reverse.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                reverse.MessageProtectionOrder = this.MessageProtectionOrder;
                reverse.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, isForService));
                if (isForService)
                {
                    reverse.ApplyConfidentiality = this.HasProtectionRequirements(reverse.ProtectionRequirements.OutgoingEncryptionParts);
                    reverse.ApplyIntegrity = true;
                    reverse.RequireIntegrity = reverse.RequireConfidentiality = false;
                }
                else
                {
                    reverse.RequireConfidentiality = this.HasProtectionRequirements(reverse.ProtectionRequirements.IncomingEncryptionParts);
                    reverse.RequireIntegrity = true;
                    reverse.ApplyIntegrity = reverse.ApplyConfidentiality = false;
                }
                base.ConfigureProtocolFactory(reverse, credentialsManager, !isForService, issuerBindingContext, context.Binding);
                if (!reverse.RequireIntegrity)
                    reverse.DetectReplays = false;

                // setup reverse here
                reverse.IsDuplexReply = true;

                DuplexSecurityProtocolFactory duplex = new DuplexSecurityProtocolFactory();
                duplex.ForwardProtocolFactory = forward;
                duplex.ReverseProtocolFactory = reverse;
                protocolFactory = duplex;
            }
            else
            {
                protocolFactory = forward;
            }

            return protocolFactory;
        }

        internal override bool RequiresChannelDemuxer()
        {
            return (base.RequiresChannelDemuxer() || RequiresChannelDemuxer(this.InitiatorTokenParameters));
        }

        protected override IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context)
        {
            ISecurityCapabilities securityCapabilities = this.GetProperty<ISecurityCapabilities>(context);
            bool requireDemuxer = RequiresChannelDemuxer();
            ChannelBuilder channelBuilder = new ChannelBuilder(context, requireDemuxer);
            if (requireDemuxer)
            {
                ApplyPropertiesOnDemuxer(channelBuilder, context);
            }

            BindingContext issuerBindingContext = context.Clone();
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }

            SecurityProtocolFactory protocolFactory =
                this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, false, issuerBindingContext);

            return new SecurityChannelFactory<TChannel>(securityCapabilities, context, channelBuilder, protocolFactory);
        }

        protected override IChannelListener<TChannel> BuildChannelListenerCore<TChannel>(BindingContext context)
        {
            bool requireDemuxer = RequiresChannelDemuxer();
            ChannelBuilder channelBuilder = new ChannelBuilder(context, requireDemuxer);
            if (requireDemuxer)
            {
                ApplyPropertiesOnDemuxer(channelBuilder, context);
            }
            BindingContext issuerBindingContext = context.Clone();

            SecurityChannelListener<TChannel> channelListener = new SecurityChannelListener<TChannel>(this, context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
                credentialsManager = ServiceCredentials.CreateDefaultCredentials();

            SecurityProtocolFactory protocolFactory = this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, true, issuerBindingContext);
            channelListener.SecurityProtocolFactory = protocolFactory;
            channelListener.InitializeListener(channelBuilder);

            return channelListener;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                AddressingVersion addressing = MessageVersion.Default.Addressing;
#pragma warning suppress 56506
                MessageEncodingBindingElement encoding = context.Binding.Elements.Find<MessageEncodingBindingElement>();
                if (encoding != null)
                {
                    addressing = encoding.MessageVersion.Addressing;
                }

                ChannelProtectionRequirements myRequirements = base.GetProtectionRequirements(addressing, this.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel);
                myRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
                return (T)(object)myRequirements;
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "MessageProtectionOrder: {0}", this.messageProtectionOrder.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "RequireSignatureConfirmation: {0}", this.requireSignatureConfirmation.ToString()));
            sb.Append("InitiatorTokenParameters: ");
            if (this.initiatorTokenParameters != null)
                sb.AppendLine(this.initiatorTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            else
                sb.AppendLine("null");
            sb.Append("RecipientTokenParameters: ");
            if (this.recipientTokenParameters != null)
                sb.AppendLine(this.recipientTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            else
                sb.AppendLine("null");

            return sb.ToString().Trim();
        }

        public override BindingElement Clone()
        {
            return new AsymmetricSecurityBindingElement(this);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            SecurityBindingElement.ExportPolicy(exporter, context);
        }
    }
}
