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
    using System.ServiceModel.Dispatcher;
    using System.Net.Security;
    using System.Text;

    public sealed class SymmetricSecurityBindingElement : SecurityBindingElement, IPolicyExportExtension
    {
        MessageProtectionOrder messageProtectionOrder;
        SecurityTokenParameters protectionTokenParameters;
        bool requireSignatureConfirmation;

        SymmetricSecurityBindingElement(SymmetricSecurityBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.messageProtectionOrder = elementToBeCloned.messageProtectionOrder;
            if (elementToBeCloned.protectionTokenParameters != null)
                this.protectionTokenParameters = (SecurityTokenParameters)elementToBeCloned.protectionTokenParameters.Clone();
            this.requireSignatureConfirmation = elementToBeCloned.requireSignatureConfirmation;
        }

        public SymmetricSecurityBindingElement()
            : this((SecurityTokenParameters)null)
        {
            // empty
        }

        public SymmetricSecurityBindingElement(SecurityTokenParameters protectionTokenParameters)
            : base()
        {
            this.messageProtectionOrder = SecurityBindingElement.defaultMessageProtectionOrder;
            this.requireSignatureConfirmation = SecurityBindingElement.defaultRequireSignatureConfirmation;
            this.protectionTokenParameters = protectionTokenParameters;
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

        public SecurityTokenParameters ProtectionTokenParameters
        {
            get
            {
                return this.protectionTokenParameters;
            }
            set
            {
                this.protectionTokenParameters = value;
            }
        }

        internal override ISecurityCapabilities GetIndividualISecurityCapabilities()
        {
            bool supportsServerAuthentication = false;
            bool supportsClientAuthentication;
            bool supportsClientWindowsIdentity;
            GetSupportingTokensCapabilities(out supportsClientAuthentication, out supportsClientWindowsIdentity);
            if (ProtectionTokenParameters != null)
            {
                supportsClientAuthentication = supportsClientAuthentication || ProtectionTokenParameters.SupportsClientAuthentication;
                supportsClientWindowsIdentity = supportsClientWindowsIdentity || ProtectionTokenParameters.SupportsClientWindowsIdentity;

                if (ProtectionTokenParameters.HasAsymmetricKey)
                {
                    supportsServerAuthentication = ProtectionTokenParameters.SupportsClientAuthentication;
                }
                else
                {
                    supportsServerAuthentication = ProtectionTokenParameters.SupportsServerAuthentication;
                }
            }

            return new SecurityCapabilities(supportsClientAuthentication, supportsServerAuthentication, supportsClientWindowsIdentity,
                ProtectionLevel.EncryptAndSign, ProtectionLevel.EncryptAndSign);
        }

        internal override bool SessionMode
        {
            get
            {
                SecureConversationSecurityTokenParameters secureConversationTokenParameters = this.ProtectionTokenParameters as SecureConversationSecurityTokenParameters;
                if (secureConversationTokenParameters != null)
                    return secureConversationTokenParameters.RequireCancellation;
                else
                    return false;
            }
        }

        internal override bool SupportsDuplex
        {
            get { return this.SessionMode; }
        }

        internal override bool SupportsRequestReply
        {
            get { return true; }
        }

        public override void SetKeyDerivation(bool requireDerivedKeys)
        {
            base.SetKeyDerivation(requireDerivedKeys);
            if (this.protectionTokenParameters != null)
                this.protectionTokenParameters.RequireDerivedKeys = requireDerivedKeys;
        }

        internal override bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            if (!base.IsSetKeyDerivation(requireDerivedKeys))
                return false;

            if (this.protectionTokenParameters != null && this.protectionTokenParameters.RequireDerivedKeys != requireDerivedKeys)
                return false;

            return true;
        }

        internal override SecurityProtocolFactory CreateSecurityProtocolFactory<TChannel>(BindingContext context, SecurityCredentialsManager credentialsManager, bool isForService, BindingContext issuerBindingContext)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            if (credentialsManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("credentialsManager");

            if (this.ProtectionTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SymmetricSecurityBindingElementNeedsProtectionTokenParameters, this.ToString())));
            }

            SymmetricSecurityProtocolFactory protocolFactory = new SymmetricSecurityProtocolFactory();
            if (isForService)
            {
                base.ApplyAuditBehaviorSettings(context, protocolFactory);
            }
            protocolFactory.SecurityTokenParameters = (SecurityTokenParameters)this.ProtectionTokenParameters.Clone();
            SetIssuerBindingContextIfRequired(protocolFactory.SecurityTokenParameters, issuerBindingContext);
            protocolFactory.ApplyConfidentiality = true;
            protocolFactory.RequireConfidentiality = true;
            protocolFactory.ApplyIntegrity = true;
            protocolFactory.RequireIntegrity = true;
            protocolFactory.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
            protocolFactory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
            protocolFactory.MessageProtectionOrder = this.MessageProtectionOrder;
            protocolFactory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, isForService));
            base.ConfigureProtocolFactory(protocolFactory, credentialsManager, isForService, issuerBindingContext, context.Binding);

            return protocolFactory;
        }

        internal override bool RequiresChannelDemuxer()
        {
            return (base.RequiresChannelDemuxer() || RequiresChannelDemuxer(this.ProtectionTokenParameters));
        }

        protected override IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context)
        {
            ISecurityCapabilities securityCapabilities = this.GetProperty<ISecurityCapabilities>(context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }

            // This adds the demuxer element to the context. We add a demuxer element only if the binding is configured to do
            // secure conversation or negotiation
            bool requireDemuxer = RequiresChannelDemuxer();
            ChannelBuilder channelBuilder = new ChannelBuilder(context, requireDemuxer);
            if (requireDemuxer)
            {
                ApplyPropertiesOnDemuxer(channelBuilder, context);
            }
            BindingContext issuerBindingContext = context.Clone();

            SecurityChannelFactory<TChannel> channelFactory;
            if (this.ProtectionTokenParameters is SecureConversationSecurityTokenParameters)
            {
                SecureConversationSecurityTokenParameters scParameters = (SecureConversationSecurityTokenParameters)this.ProtectionTokenParameters;
                if (scParameters.BootstrapSecurityBindingElement == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationSecurityTokenParametersRequireBootstrapBinding)));

                BindingContext scIssuerBindingContext = issuerBindingContext.Clone();
                scIssuerBindingContext.BindingParameters.Remove<ChannelProtectionRequirements>();
                scIssuerBindingContext.BindingParameters.Add(scParameters.BootstrapProtectionRequirements);

                if (scParameters.RequireCancellation)
                {
                    SessionSymmetricMessageSecurityProtocolFactory sessionFactory = new SessionSymmetricMessageSecurityProtocolFactory();
                    sessionFactory.SecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)sessionFactory.SecurityTokenParameters).IssuerBindingContext = scIssuerBindingContext;
                    sessionFactory.ApplyConfidentiality = true;
                    sessionFactory.RequireConfidentiality = true;
                    sessionFactory.ApplyIntegrity = true;
                    sessionFactory.RequireIntegrity = true;
                    sessionFactory.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
                    sessionFactory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                    sessionFactory.MessageProtectionOrder = this.MessageProtectionOrder;
                    sessionFactory.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
                    sessionFactory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, false));
                    base.ConfigureProtocolFactory(sessionFactory, credentialsManager, false, issuerBindingContext, context.Binding);

                    SecuritySessionClientSettings<TChannel> sessionClientSettings = new SecuritySessionClientSettings<TChannel>();
                    sessionClientSettings.ChannelBuilder = channelBuilder;
                    sessionClientSettings.KeyRenewalInterval = this.LocalClientSettings.SessionKeyRenewalInterval;
                    sessionClientSettings.CanRenewSession = scParameters.CanRenewSession;
                    sessionClientSettings.KeyRolloverInterval = this.LocalClientSettings.SessionKeyRolloverInterval;
                    sessionClientSettings.TolerateTransportFailures = this.LocalClientSettings.ReconnectTransportOnFailure;
                    sessionClientSettings.IssuedSecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)sessionClientSettings.IssuedSecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    sessionClientSettings.SecurityStandardsManager = sessionFactory.StandardsManager;
                    sessionClientSettings.SessionProtocolFactory = sessionFactory;
                    channelFactory = new SecurityChannelFactory<TChannel>(securityCapabilities, context, sessionClientSettings);
                }
                else
                {
                    SymmetricSecurityProtocolFactory protocolFactory = new SymmetricSecurityProtocolFactory();

                    protocolFactory.SecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)protocolFactory.SecurityTokenParameters).IssuerBindingContext = scIssuerBindingContext;
                    protocolFactory.ApplyConfidentiality = true;
                    protocolFactory.RequireConfidentiality = true;
                    protocolFactory.ApplyIntegrity = true;
                    protocolFactory.RequireIntegrity = true;
                    protocolFactory.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
                    protocolFactory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                    protocolFactory.MessageProtectionOrder = this.MessageProtectionOrder;
                    protocolFactory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, false));
                    base.ConfigureProtocolFactory(protocolFactory, credentialsManager, false, issuerBindingContext, context.Binding);

                    channelFactory = new SecurityChannelFactory<TChannel>(securityCapabilities, context, channelBuilder, protocolFactory);
                }
            }
            else
            {
                SecurityProtocolFactory protocolFactory = this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, false, issuerBindingContext);
                channelFactory = new SecurityChannelFactory<TChannel>(securityCapabilities, context, channelBuilder, protocolFactory);
            }

            return channelFactory;
        }

        protected override IChannelListener<TChannel> BuildChannelListenerCore<TChannel>(BindingContext context)
        {
            SecurityChannelListener<TChannel> channelListener = new SecurityChannelListener<TChannel>(this, context);

            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
                credentialsManager = ServiceCredentials.CreateDefaultCredentials();

            // This adds the demuxer element to the context. We add a demuxer element only if the binding is configured to do
            // secure conversation or negotiation

            bool requireDemuxer = RequiresChannelDemuxer();
            ChannelBuilder channelBuilder = new ChannelBuilder(context, requireDemuxer);
            if (requireDemuxer)
            {
                ApplyPropertiesOnDemuxer(channelBuilder, context);
            }

            BindingContext issuerBindingContext = context.Clone();

            if (this.ProtectionTokenParameters is SecureConversationSecurityTokenParameters)
            {
                SecureConversationSecurityTokenParameters scParameters = (SecureConversationSecurityTokenParameters)this.ProtectionTokenParameters;
                if (scParameters.BootstrapSecurityBindingElement == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationSecurityTokenParametersRequireBootstrapBinding)));

                BindingContext scIssuerBindingContext = issuerBindingContext.Clone();
                scIssuerBindingContext.BindingParameters.Remove<ChannelProtectionRequirements>();
                scIssuerBindingContext.BindingParameters.Add(scParameters.BootstrapProtectionRequirements);
                IMessageFilterTable<EndpointAddress> endpointFilterTable = context.BindingParameters.Find<IMessageFilterTable<EndpointAddress>>();

                AddDemuxerForSecureConversation(channelBuilder, scIssuerBindingContext);

                if (scParameters.RequireCancellation)
                {
                    SessionSymmetricMessageSecurityProtocolFactory sessionFactory = new SessionSymmetricMessageSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, sessionFactory);
                    sessionFactory.SecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)sessionFactory.SecurityTokenParameters).IssuerBindingContext = scIssuerBindingContext;
                    sessionFactory.ApplyConfidentiality = true;
                    sessionFactory.RequireConfidentiality = true;
                    sessionFactory.ApplyIntegrity = true;
                    sessionFactory.RequireIntegrity = true;
                    sessionFactory.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
                    sessionFactory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                    sessionFactory.MessageProtectionOrder = this.MessageProtectionOrder;
                    sessionFactory.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
                    sessionFactory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, true));
                    base.ConfigureProtocolFactory(sessionFactory, credentialsManager, true, issuerBindingContext, context.Binding);

                    channelListener.SessionMode = true;
                    channelListener.SessionServerSettings.InactivityTimeout = this.LocalServiceSettings.InactivityTimeout;
                    channelListener.SessionServerSettings.KeyRolloverInterval = this.LocalServiceSettings.SessionKeyRolloverInterval;
                    channelListener.SessionServerSettings.MaximumPendingSessions = this.LocalServiceSettings.MaxPendingSessions;
                    channelListener.SessionServerSettings.MaximumKeyRenewalInterval = this.LocalServiceSettings.SessionKeyRenewalInterval;
                    channelListener.SessionServerSettings.TolerateTransportFailures = this.LocalServiceSettings.ReconnectTransportOnFailure;
                    channelListener.SessionServerSettings.CanRenewSession = scParameters.CanRenewSession;
                    channelListener.SessionServerSettings.IssuedSecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)channelListener.SessionServerSettings.IssuedSecurityTokenParameters).IssuerBindingContext = scIssuerBindingContext;
                    channelListener.SessionServerSettings.SecurityStandardsManager = sessionFactory.StandardsManager;
                    channelListener.SessionServerSettings.SessionProtocolFactory = sessionFactory;
                    channelListener.SessionServerSettings.SessionProtocolFactory.EndpointFilterTable = endpointFilterTable;

                    // pass in the error handler for handling unknown security sessions - dont do this if the underlying channel is duplex since sending 
                    // back faults in response to badly secured requests over duplex can result in DoS.
                    if (context.BindingParameters != null && context.BindingParameters.Find<IChannelDemuxFailureHandler>() == null
                        && !IsUnderlyingListenerDuplex<TChannel>(context))
                    {
                        context.BindingParameters.Add(new SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler(sessionFactory.StandardsManager));
                    }
                }
                else
                {
                    SymmetricSecurityProtocolFactory protocolFactory = new SymmetricSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, protocolFactory);
                    protocolFactory.SecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)protocolFactory.SecurityTokenParameters).IssuerBindingContext = scIssuerBindingContext;
                    protocolFactory.ApplyConfidentiality = true;
                    protocolFactory.RequireConfidentiality = true;
                    protocolFactory.ApplyIntegrity = true;
                    protocolFactory.RequireIntegrity = true;
                    protocolFactory.IdentityVerifier = this.LocalClientSettings.IdentityVerifier;
                    protocolFactory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                    protocolFactory.MessageProtectionOrder = this.MessageProtectionOrder;
                    protocolFactory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, true));
                    protocolFactory.EndpointFilterTable = endpointFilterTable;
                    base.ConfigureProtocolFactory(protocolFactory, credentialsManager, true, issuerBindingContext, context.Binding);

                    channelListener.SecurityProtocolFactory = protocolFactory;
                }

            }
            else
            {
                SecurityProtocolFactory protocolFactory = this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, true, issuerBindingContext);
                channelListener.SecurityProtocolFactory = protocolFactory;
            }

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
                ChannelProtectionRequirements myRequirements = base.GetProtectionRequirements(addressing, ProtectionLevel.EncryptAndSign);
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
            sb.Append("ProtectionTokenParameters: ");
            if (this.protectionTokenParameters != null)
                sb.AppendLine(this.protectionTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            else
                sb.AppendLine("null");

            return sb.ToString().Trim();
        }

        public override BindingElement Clone()
        {
            return new SymmetricSecurityBindingElement(this);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            SecurityBindingElement.ExportPolicy(exporter, context);
        }
    }
}
