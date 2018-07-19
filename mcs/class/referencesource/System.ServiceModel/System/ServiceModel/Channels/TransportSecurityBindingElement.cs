//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    using System.Net.Security;

    public sealed class TransportSecurityBindingElement : SecurityBindingElement, IPolicyExportExtension
    {
        public TransportSecurityBindingElement()
            : base()
        {
            this.LocalClientSettings.DetectReplays = this.LocalServiceSettings.DetectReplays = false;
        }

        TransportSecurityBindingElement(TransportSecurityBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            // empty
        }

        internal override ISecurityCapabilities GetIndividualISecurityCapabilities()
        {
            bool supportsClientAuthentication;
            bool supportsClientWindowsIdentity;
            GetSupportingTokensCapabilities(out supportsClientAuthentication, out supportsClientWindowsIdentity);
            return new SecurityCapabilities(supportsClientAuthentication, false, supportsClientWindowsIdentity,
                ProtectionLevel.None, ProtectionLevel.None);
        }

        internal override bool SessionMode
        {
            get
            {
                SecureConversationSecurityTokenParameters scParameters = null;
                if (this.EndpointSupportingTokenParameters.Endorsing.Count > 0)
                    scParameters = this.EndpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
                if (scParameters != null)
                    return scParameters.RequireCancellation;
                else
                    return false;
            }
        }

        internal override bool SupportsDuplex
        {
            get { return true; }
        }

        internal override bool SupportsRequestReply
        {
            get { return true; }
        }

        internal override SecurityProtocolFactory CreateSecurityProtocolFactory<TChannel>(BindingContext context, SecurityCredentialsManager credentialsManager, bool isForService, BindingContext issuerBindingContext)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            if (credentialsManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("credentialsManager");

            TransportSecurityProtocolFactory protocolFactory = new TransportSecurityProtocolFactory();
            if (isForService)
                base.ApplyAuditBehaviorSettings(context, protocolFactory);
            base.ConfigureProtocolFactory(protocolFactory, credentialsManager, isForService, issuerBindingContext, context.Binding);
            protocolFactory.DetectReplays = false;

            return protocolFactory;
        }

        protected override IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context)
        {
            ISecurityCapabilities securityCapabilities = this.GetProperty<ISecurityCapabilities>(context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }

            SecureConversationSecurityTokenParameters scParameters = null;
            if (this.EndpointSupportingTokenParameters.Endorsing.Count > 0)
            {
                scParameters = this.EndpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
            }

            // This adds the demuxer element to the context

            bool requireDemuxer = RequiresChannelDemuxer();
            ChannelBuilder channelBuilder = new ChannelBuilder(context, requireDemuxer);

            if (requireDemuxer)
            {
                ApplyPropertiesOnDemuxer(channelBuilder, context);
            }
            BindingContext issuerBindingContext = context.Clone();

            SecurityChannelFactory<TChannel> channelFactory;
            if (scParameters != null)
            {
                if (scParameters.BootstrapSecurityBindingElement == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationSecurityTokenParametersRequireBootstrapBinding)));

                scParameters.IssuerBindingContext = issuerBindingContext;
                if (scParameters.RequireCancellation)
                {
                    SessionSymmetricTransportSecurityProtocolFactory sessionFactory = new SessionSymmetricTransportSecurityProtocolFactory();
                    sessionFactory.SecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)sessionFactory.SecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    this.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                    try
                    {
                        base.ConfigureProtocolFactory(sessionFactory, credentialsManager, false, issuerBindingContext, context.Binding);
                    }
                    finally
                    {
                        this.EndpointSupportingTokenParameters.Endorsing.Insert(0, scParameters);
                    }

                    SecuritySessionClientSettings<TChannel> sessionClientSettings = new SecuritySessionClientSettings<TChannel>();
                    sessionClientSettings.ChannelBuilder = channelBuilder;
                    sessionClientSettings.KeyRenewalInterval = this.LocalClientSettings.SessionKeyRenewalInterval;
                    sessionClientSettings.KeyRolloverInterval = this.LocalClientSettings.SessionKeyRolloverInterval;
                    sessionClientSettings.TolerateTransportFailures = this.LocalClientSettings.ReconnectTransportOnFailure;
                    sessionClientSettings.CanRenewSession = scParameters.CanRenewSession;
                    sessionClientSettings.IssuedSecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)sessionClientSettings.IssuedSecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    sessionClientSettings.SecurityStandardsManager = sessionFactory.StandardsManager;
                    sessionClientSettings.SessionProtocolFactory = sessionFactory;
                    channelFactory = new SecurityChannelFactory<TChannel>(securityCapabilities, context, sessionClientSettings);
                }
                else
                {
                    TransportSecurityProtocolFactory protocolFactory = new TransportSecurityProtocolFactory();
                    this.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                    try
                    {
                        base.ConfigureProtocolFactory(protocolFactory, credentialsManager, false, issuerBindingContext, context.Binding);
                        SecureConversationSecurityTokenParameters acceleratedTokenParameters = (SecureConversationSecurityTokenParameters)scParameters.Clone();
                        acceleratedTokenParameters.IssuerBindingContext = issuerBindingContext;
                        protocolFactory.SecurityBindingElement.EndpointSupportingTokenParameters.Endorsing.Insert(0, acceleratedTokenParameters);
                    }
                    finally
                    {
                        this.EndpointSupportingTokenParameters.Endorsing.Insert(0, scParameters);
                    }

                    channelFactory = new SecurityChannelFactory<TChannel>(securityCapabilities, context, channelBuilder, protocolFactory);
                }
            }
            else
            {
                SecurityProtocolFactory protocolFactory = this.CreateSecurityProtocolFactory<TChannel>(
                    context, credentialsManager, false, issuerBindingContext);
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

            SecureConversationSecurityTokenParameters scParameters;
            if (this.EndpointSupportingTokenParameters.Endorsing.Count > 0)
                scParameters = this.EndpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
            else
                scParameters = null;

            bool requireDemuxer = RequiresChannelDemuxer();
            ChannelBuilder channelBuilder = new ChannelBuilder(context, requireDemuxer);

            if (requireDemuxer)
            {
                ApplyPropertiesOnDemuxer(channelBuilder, context);
            }

            BindingContext issuerBindingContext = context.Clone();
            if (scParameters != null)
            {
                if (scParameters.BootstrapSecurityBindingElement == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationSecurityTokenParametersRequireBootstrapBinding)));

                AddDemuxerForSecureConversation(channelBuilder, issuerBindingContext);

                if (scParameters.RequireCancellation)
                {
                    SessionSymmetricTransportSecurityProtocolFactory sessionFactory = new SessionSymmetricTransportSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, sessionFactory);
                    sessionFactory.SecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)sessionFactory.SecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    this.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                    try
                    {
                        base.ConfigureProtocolFactory(sessionFactory, credentialsManager, true, issuerBindingContext, context.Binding);
                    }
                    finally
                    {
                        this.EndpointSupportingTokenParameters.Endorsing.Insert(0, scParameters);
                    }

                    channelListener.SessionMode = true;
                    channelListener.SessionServerSettings.InactivityTimeout = this.LocalServiceSettings.InactivityTimeout;
                    channelListener.SessionServerSettings.KeyRolloverInterval = this.LocalServiceSettings.SessionKeyRolloverInterval;
                    channelListener.SessionServerSettings.MaximumPendingSessions = this.LocalServiceSettings.MaxPendingSessions;
                    channelListener.SessionServerSettings.MaximumKeyRenewalInterval = this.LocalServiceSettings.SessionKeyRenewalInterval;
                    channelListener.SessionServerSettings.TolerateTransportFailures = this.LocalServiceSettings.ReconnectTransportOnFailure;
                    channelListener.SessionServerSettings.CanRenewSession = scParameters.CanRenewSession;
                    channelListener.SessionServerSettings.IssuedSecurityTokenParameters = scParameters.Clone();
                    ((SecureConversationSecurityTokenParameters)channelListener.SessionServerSettings.IssuedSecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    channelListener.SessionServerSettings.SecurityStandardsManager = sessionFactory.StandardsManager;
                    channelListener.SessionServerSettings.SessionProtocolFactory = sessionFactory;

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
                    TransportSecurityProtocolFactory protocolFactory = new TransportSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, protocolFactory);
                    this.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                    try
                    {
                        base.ConfigureProtocolFactory(protocolFactory, credentialsManager, true, issuerBindingContext, context.Binding);
                        SecureConversationSecurityTokenParameters acceleratedTokenParameters = (SecureConversationSecurityTokenParameters)scParameters.Clone();
                        acceleratedTokenParameters.IssuerBindingContext = issuerBindingContext;
                        protocolFactory.SecurityBindingElement.EndpointSupportingTokenParameters.Endorsing.Insert(0, acceleratedTokenParameters);
                    }
                    finally
                    {
                        this.EndpointSupportingTokenParameters.Endorsing.Insert(0, scParameters);
                    }

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

        public override BindingElement Clone()
        {
            return new TransportSecurityBindingElement(this);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            if (exporter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            if (policyContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");

            if (policyContext.BindingElements.Find<ITransportTokenAssertionProvider>() == null)
            {
                if (!this.AllowInsecureTransport)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ExportOfBindingWithTransportSecurityBindingElementAndNoTransportSecurityNotSupported)));
                }

                // In AllowInsecureTransport mode there is no assertion provider to export the endpoint supporting tokens. Hence we explicitly call into ExportPolicyForTransportTokenAssertionProviders.
                SecurityBindingElement.ExportPolicyForTransportTokenAssertionProviders(exporter, policyContext);
            }

            // the ITransportTokenAssertionProvider will perform the acutal export steps.
        }
    }
}
