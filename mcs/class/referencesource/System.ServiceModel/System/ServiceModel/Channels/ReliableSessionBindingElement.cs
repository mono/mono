//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class ReliableSessionBindingElement : BindingElement, IPolicyExportExtension
    {
        TimeSpan acknowledgementInterval = ReliableSessionDefaults.AcknowledgementInterval;
        bool flowControlEnabled = ReliableSessionDefaults.FlowControlEnabled;
        TimeSpan inactivityTimeout = ReliableSessionDefaults.InactivityTimeout;
        int maxPendingChannels = ReliableSessionDefaults.MaxPendingChannels;
        int maxRetryCount = ReliableSessionDefaults.MaxRetryCount;
        int maxTransferWindowSize = ReliableSessionDefaults.MaxTransferWindowSize;
        bool ordered = ReliableSessionDefaults.Ordered;
        ReliableMessagingVersion reliableMessagingVersion = ReliableMessagingVersion.Default;
        InternalDuplexBindingElement internalDuplexBindingElement;

        static MessagePartSpecification bodyOnly;

        public ReliableSessionBindingElement()
        {
        }

        internal ReliableSessionBindingElement(ReliableSessionBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.AcknowledgementInterval = elementToBeCloned.AcknowledgementInterval;
            this.FlowControlEnabled = elementToBeCloned.FlowControlEnabled;
            this.InactivityTimeout = elementToBeCloned.InactivityTimeout;
            this.MaxPendingChannels = elementToBeCloned.MaxPendingChannels;
            this.MaxRetryCount = elementToBeCloned.MaxRetryCount;
            this.MaxTransferWindowSize = elementToBeCloned.MaxTransferWindowSize;
            this.Ordered = elementToBeCloned.Ordered;
            this.ReliableMessagingVersion = elementToBeCloned.ReliableMessagingVersion;

            this.internalDuplexBindingElement = elementToBeCloned.internalDuplexBindingElement;
        }

        public ReliableSessionBindingElement(bool ordered)
        {
            this.ordered = ordered;
        }

        [DefaultValue(typeof(TimeSpan), ReliableSessionDefaults.AcknowledgementIntervalString)]
        public TimeSpan AcknowledgementInterval
        {
            get
            {
                return this.acknowledgementInterval;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.acknowledgementInterval = value;
            }
        }

        [DefaultValue(ReliableSessionDefaults.FlowControlEnabled)]
        public bool FlowControlEnabled
        {
            get
            {
                return this.flowControlEnabled;
            }
            set
            {
                this.flowControlEnabled = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), ReliableSessionDefaults.InactivityTimeoutString)]
        public TimeSpan InactivityTimeout
        {
            get
            {
                return this.inactivityTimeout;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.inactivityTimeout = value;
            }
        }

        [DefaultValue(ReliableSessionDefaults.MaxPendingChannels)]
        public int MaxPendingChannels
        {
            get
            {
                return this.maxPendingChannels;
            }
            set
            {
                if (value <= 0 || value > 16384)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeInRange, 0, 16384)));
                this.maxPendingChannels = value;
            }
        }

        [DefaultValue(ReliableSessionDefaults.MaxRetryCount)]
        public int MaxRetryCount
        {
            get
            {
                return this.maxRetryCount;
            }
            set
            {
                if (value <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBePositive)));
                this.maxRetryCount = value;
            }
        }

        [DefaultValue(ReliableSessionDefaults.MaxTransferWindowSize)]
        public int MaxTransferWindowSize
        {
            get
            {
                return this.maxTransferWindowSize;
            }
            set
            {
                if (value <= 0 || value > 4096)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeInRange, 0, 4096)));
                this.maxTransferWindowSize = value;
            }
        }

        [DefaultValue(ReliableSessionDefaults.Ordered)]
        public bool Ordered
        {
            get
            {
                return this.ordered;
            }
            set
            {
                this.ordered = value;
            }
        }

        [DefaultValue(typeof(ReliableMessagingVersion), ReliableSessionDefaults.ReliableMessagingVersionString)]
        public ReliableMessagingVersion ReliableMessagingVersion
        {
            get
            {
                return this.reliableMessagingVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (!ReliableMessagingVersion.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.reliableMessagingVersion = value;
            }
        }

        static MessagePartSpecification BodyOnly
        {
            get
            {
                if (bodyOnly == null)
                {
                    MessagePartSpecification temp = new MessagePartSpecification(true);
                    temp.MakeReadOnly();
                    bodyOnly = temp;
                }

                return bodyOnly;
            }
        }

        public override BindingElement Clone()
        {
            return new ReliableSessionBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                ChannelProtectionRequirements myRequirements = this.GetProtectionRequirements();
                myRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
                return (T)(object)myRequirements;
            }
            else if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T)(object)new BindingDeliveryCapabilitiesHelper(this, context.GetInnerProperty<IBindingDeliveryCapabilities>());
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        ChannelProtectionRequirements GetProtectionRequirements()
        {
            // Listing headers that must be signed.
            ChannelProtectionRequirements result = new ChannelProtectionRequirements();
            MessagePartSpecification signedReliabilityMessageParts = WsrmIndex.GetSignedReliabilityMessageParts(
                this.reliableMessagingVersion);
            result.IncomingSignatureParts.AddParts(signedReliabilityMessageParts);
            result.OutgoingSignatureParts.AddParts(signedReliabilityMessageParts);

            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                // Adding RM protocol message actions so that each RM protocol message's body will be 
                // signed and encrypted.
                // From the Client to the Service
                ScopedMessagePartSpecification signaturePart = result.IncomingSignatureParts;
                ScopedMessagePartSpecification encryptionPart = result.IncomingEncryptionParts;
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.AckRequestedAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.CreateSequenceAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.SequenceAcknowledgementAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.LastMessageAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.TerminateSequenceAction);

                // From the Service to the Client
                signaturePart = result.OutgoingSignatureParts;
                encryptionPart = result.OutgoingEncryptionParts;
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.CreateSequenceResponseAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.SequenceAcknowledgementAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.LastMessageAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, WsrmFeb2005Strings.TerminateSequenceAction);
            }
            else if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                // Adding RM protocol message actions so that each RM protocol message's body will be 
                // signed and encrypted.
                // From the Client to the Service
                ScopedMessagePartSpecification signaturePart = result.IncomingSignatureParts;
                ScopedMessagePartSpecification encryptionPart = result.IncomingEncryptionParts;
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.AckRequestedAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.CloseSequenceAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.CloseSequenceResponseAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.CreateSequenceAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.FaultAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.SequenceAcknowledgementAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.TerminateSequenceAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.TerminateSequenceResponseAction);

                // From the Service to the Client
                signaturePart = result.OutgoingSignatureParts;
                encryptionPart = result.OutgoingEncryptionParts;
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.AckRequestedAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.CloseSequenceAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.CloseSequenceResponseAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.CreateSequenceResponseAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.FaultAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.SequenceAcknowledgementAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.TerminateSequenceAction);
                ProtectProtocolMessage(signaturePart, encryptionPart, Wsrm11Strings.TerminateSequenceResponseAction);
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }

            return result;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            this.VerifyTransportMode(context);
            this.SetSecuritySettings(context);

            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref this.internalDuplexBindingElement);

            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                if (context.CanBuildInnerChannelFactory<IRequestSessionChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IRequestSessionChannel>(
                        this, context.BuildInnerChannelFactory<IRequestSessionChannel>(), context.Binding);
                }
                else if (context.CanBuildInnerChannelFactory<IRequestChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IRequestChannel>(
                        this, context.BuildInnerChannelFactory<IRequestChannel>(), context.Binding);
                }
                else if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IDuplexSessionChannel>(
                        this, context.BuildInnerChannelFactory<IDuplexSessionChannel>(), context.Binding);
                }
                else if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IDuplexChannel>(
                        this, context.BuildInnerChannelFactory<IDuplexChannel>(), context.Binding);
                }
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IDuplexSessionChannel>(
                        this, context.BuildInnerChannelFactory<IDuplexSessionChannel>(), context.Binding);
                }
                else if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IDuplexChannel>(
                        this, context.BuildInnerChannelFactory<IDuplexChannel>(), context.Binding);
                }
            }
            else if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                if (context.CanBuildInnerChannelFactory<IRequestSessionChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IRequestSessionChannel>(
                        this, context.BuildInnerChannelFactory<IRequestSessionChannel>(), context.Binding);
                }
                else if (context.CanBuildInnerChannelFactory<IRequestChannel>())
                {
                    return (IChannelFactory<TChannel>)(object)
                        new ReliableChannelFactory<TChannel, IRequestChannel>(
                        this, context.BuildInnerChannelFactory<IRequestChannel>(), context.Binding);
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref this.internalDuplexBindingElement);

            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                return context.CanBuildInnerChannelFactory<IRequestSessionChannel>()
                    || context.CanBuildInnerChannelFactory<IRequestChannel>()
                    || context.CanBuildInnerChannelFactory<IDuplexSessionChannel>()
                    || context.CanBuildInnerChannelFactory<IDuplexChannel>();
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return context.CanBuildInnerChannelFactory<IDuplexSessionChannel>()
                    || context.CanBuildInnerChannelFactory<IDuplexChannel>();
            }
            else if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return context.CanBuildInnerChannelFactory<IRequestSessionChannel>()
                    || context.CanBuildInnerChannelFactory<IRequestChannel>();
            }

            return false;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            this.VerifyTransportMode(context);
            this.SetSecuritySettings(context);

#pragma warning suppress 56506 // BindingContext guarantees BindingParameters is never null.
            IMessageFilterTable<EndpointAddress> table = context.BindingParameters.Find<IMessageFilterTable<EndpointAddress>>();

            InternalDuplexBindingElement.AddDuplexListenerSupport(context, ref this.internalDuplexBindingElement);

            if (typeof(TChannel) == typeof(IInputSessionChannel))
            {
                ReliableChannelListenerBase<IInputSessionChannel> listener = null;

                if (context.CanBuildInnerChannelListener<IReplySessionChannel>())
                {
                    listener = new ReliableInputListenerOverReplySession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IReplyChannel>())
                {
                    listener = new ReliableInputListenerOverReply(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
                {
                    listener = new ReliableInputListenerOverDuplexSession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IDuplexChannel>())
                {
                    listener = new ReliableInputListenerOverDuplex(this, context);
                }

                if (listener != null)
                {
                    listener.LocalAddresses = table;
                    return (IChannelListener<TChannel>)(object)listener;
                }
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                ReliableChannelListenerBase<IDuplexSessionChannel> listener = null;

                if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
                {
                    listener = new ReliableDuplexListenerOverDuplexSession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IDuplexChannel>())
                {
                    listener = new ReliableDuplexListenerOverDuplex(this, context);
                }

                if (listener != null)
                {
                    listener.LocalAddresses = table;
                    return (IChannelListener<TChannel>)(object)listener;
                }
            }
            else if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                ReliableChannelListenerBase<IReplySessionChannel> listener = null;

                if (context.CanBuildInnerChannelListener<IReplySessionChannel>())
                {
                    listener = new ReliableReplyListenerOverReplySession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IReplyChannel>())
                {
                    listener = new ReliableReplyListenerOverReply(this, context);
                }

                if (listener != null)
                {
                    listener.LocalAddresses = table;
                    return (IChannelListener<TChannel>)(object)listener;
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            InternalDuplexBindingElement.AddDuplexListenerSupport(context, ref this.internalDuplexBindingElement);

            if (typeof(TChannel) == typeof(IInputSessionChannel))
            {
                return context.CanBuildInnerChannelListener<IReplySessionChannel>()
                    || context.CanBuildInnerChannelListener<IReplyChannel>()
                    || context.CanBuildInnerChannelListener<IDuplexSessionChannel>()
                    || context.CanBuildInnerChannelListener<IDuplexChannel>();
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return context.CanBuildInnerChannelListener<IDuplexSessionChannel>()
                    || context.CanBuildInnerChannelListener<IDuplexChannel>();
            }
            else if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                return context.CanBuildInnerChannelListener<IReplySessionChannel>()
                    || context.CanBuildInnerChannelListener<IReplyChannel>();
            }

            return false;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
                return false;
            ReliableSessionBindingElement session = b as ReliableSessionBindingElement;
            if (session == null)
                return false;
            if (this.acknowledgementInterval != session.acknowledgementInterval)
                return false;
            if (this.flowControlEnabled != session.flowControlEnabled)
                return false;
            if (this.inactivityTimeout != session.inactivityTimeout)
                return false;
            if (this.maxPendingChannels != session.maxPendingChannels)
                return false;
            if (this.maxRetryCount != session.maxRetryCount)
                return false;
            if (this.maxTransferWindowSize != session.maxTransferWindowSize)
                return false;
            if (this.ordered != session.ordered)
                return false;
            if (this.reliableMessagingVersion != session.reliableMessagingVersion)
                return false;

            return true;
        }

        static void ProtectProtocolMessage(
            ScopedMessagePartSpecification signaturePart,
            ScopedMessagePartSpecification encryptionPart,
            string action)
        {
            signaturePart.AddParts(BodyOnly, action);
            encryptionPart.AddParts(MessagePartSpecification.NoParts, action);
            //encryptionPart.AddParts(BodyOnly, action);
        }

        void SetSecuritySettings(BindingContext context)
        {
            SecurityBindingElement element = context.RemainingBindingElements.Find<SecurityBindingElement>();

            if (element != null)
            {
                element.LocalServiceSettings.ReconnectTransportOnFailure = true;
            }
        }

        void VerifyTransportMode(BindingContext context)
        {
            TransportBindingElement transportElement = context.RemainingBindingElements.Find<TransportBindingElement>();

            // Verify ManualAdderssing is turned off.
            if ((transportElement != null) && (transportElement.ManualAddressing))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ManualAddressingNotSupported)));
            }

            ConnectionOrientedTransportBindingElement connectionElement = transportElement as ConnectionOrientedTransportBindingElement;
            HttpTransportBindingElement httpElement = transportElement as HttpTransportBindingElement;

            // Verify TransportMode is Buffered.
            TransferMode transportTransferMode;

            if (connectionElement != null)
            {
                transportTransferMode = connectionElement.TransferMode;
            }
            else if (httpElement != null)
            {
                transportTransferMode = httpElement.TransferMode;
            }
            else
            {
                // Not one of the elements we can check, we have to assume TransferMode.Buffered.
                transportTransferMode = TransferMode.Buffered;
            }

            if (transportTransferMode != TransferMode.Buffered)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.TransferModeNotSupported,
                    transportTransferMode, this.GetType().Name)));
            }
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            if (context.BindingElements != null)
            {
                BindingElementCollection bindingElements = context.BindingElements;
                ReliableSessionBindingElement settings = bindingElements.Find<ReliableSessionBindingElement>();

                if (settings != null)
                {
                    // ReliableSession assertion
                    XmlElement assertion = settings.CreateReliabilityAssertion(exporter.PolicyVersion, bindingElements);
                    context.GetBindingAssertions().Add(assertion);
                }
            }
        }

        static XmlElement CreatePolicyElement(PolicyVersion policyVersion, XmlDocument doc)
        {
            string policy = MetadataStrings.WSPolicy.Elements.Policy;
            string policyNs = policyVersion.Namespace;
            string policyPrefix = MetadataStrings.WSPolicy.Prefix;

            return doc.CreateElement(policyPrefix, policy, policyNs);
        }

        XmlElement CreateReliabilityAssertion(PolicyVersion policyVersion, BindingElementCollection bindingElements)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement child = null;
            string reliableSessionPrefix;
            string reliableSessionNs;
            string assertionPrefix;
            string assertionNs;

            if (this.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                reliableSessionPrefix = ReliableSessionPolicyStrings.ReliableSessionFebruary2005Prefix;
                reliableSessionNs = ReliableSessionPolicyStrings.ReliableSessionFebruary2005Namespace;
                assertionPrefix = reliableSessionPrefix;
                assertionNs = reliableSessionNs;
            }
            else
            {
                reliableSessionPrefix = ReliableSessionPolicyStrings.ReliableSession11Prefix;
                reliableSessionNs = ReliableSessionPolicyStrings.ReliableSession11Namespace;
                assertionPrefix = ReliableSessionPolicyStrings.NET11Prefix;
                assertionNs = ReliableSessionPolicyStrings.NET11Namespace;
            }

            // ReliableSession assertion
            XmlElement assertion = doc.CreateElement(reliableSessionPrefix, ReliableSessionPolicyStrings.ReliableSessionName, reliableSessionNs);

            if (this.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                // Policy
                XmlElement policy = CreatePolicyElement(policyVersion, doc);

                // SequenceSTR
                if (IsSecureConversationEnabled(bindingElements))
                {
                    XmlElement sequenceSTR = doc.CreateElement(reliableSessionPrefix, ReliableSessionPolicyStrings.SequenceSTR, reliableSessionNs);
                    policy.AppendChild(sequenceSTR);
                }

                // DeliveryAssurance
                XmlElement deliveryAssurance = doc.CreateElement(reliableSessionPrefix, ReliableSessionPolicyStrings.DeliveryAssurance, reliableSessionNs);

                // Policy
                XmlElement nestedPolicy = CreatePolicyElement(policyVersion, doc);

                // ExactlyOnce
                XmlElement exactlyOnce = doc.CreateElement(reliableSessionPrefix, ReliableSessionPolicyStrings.ExactlyOnce, reliableSessionNs);
                nestedPolicy.AppendChild(exactlyOnce);

                if (this.ordered)
                {
                    // InOrder
                    XmlElement inOrder = doc.CreateElement(reliableSessionPrefix, ReliableSessionPolicyStrings.InOrder, reliableSessionNs);
                    nestedPolicy.AppendChild(inOrder);
                }

                deliveryAssurance.AppendChild(nestedPolicy);
                policy.AppendChild(deliveryAssurance);
                assertion.AppendChild(policy);
            }

            // Nested InactivityTimeout assertion
            child = doc.CreateElement(assertionPrefix, ReliableSessionPolicyStrings.InactivityTimeout, assertionNs);
            WriteMillisecondsAttribute(child, this.InactivityTimeout);
            assertion.AppendChild(child);

            // Nested AcknowledgementInterval assertion
            child = doc.CreateElement(assertionPrefix, ReliableSessionPolicyStrings.AcknowledgementInterval, assertionNs);
            WriteMillisecondsAttribute(child, this.AcknowledgementInterval);
            assertion.AppendChild(child);

            return assertion;
        }

        static bool IsSecureConversationEnabled(BindingElementCollection bindingElements)
        {
            bool foundRM = false;

            for (int i = 0; i < bindingElements.Count; i++)
            {
                if (!foundRM)
                {
                    ReliableSessionBindingElement bindingElement = bindingElements[i] as ReliableSessionBindingElement;
                    foundRM = (bindingElement != null);
                }
                else
                {
                    SecurityBindingElement securityBindingElement = bindingElements[i] as SecurityBindingElement;

                    if (securityBindingElement != null)
                    {
                        SecurityBindingElement bootstrapSecurity;

                        // The difference in bool (requireCancellation) does not affect whether the binding is valid,
                        // but the method does match on the value so we need to pass both true and false.
                        return SecurityBindingElement.IsSecureConversationBinding(securityBindingElement, true, out bootstrapSecurity)
                            || SecurityBindingElement.IsSecureConversationBinding(securityBindingElement, false, out bootstrapSecurity);
                    }

                    break;
                }
            }

            return false;
        }

        static void WriteMillisecondsAttribute(XmlElement childElement, TimeSpan timeSpan)
        {
            UInt64 milliseconds = Convert.ToUInt64(timeSpan.TotalMilliseconds);
            childElement.SetAttribute(ReliableSessionPolicyStrings.Milliseconds, XmlConvert.ToString(milliseconds));
        }

        class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            ReliableSessionBindingElement element;
            IBindingDeliveryCapabilities inner;

            internal BindingDeliveryCapabilitiesHelper(ReliableSessionBindingElement element, IBindingDeliveryCapabilities inner)
            {
                this.element = element;
                this.inner = inner;
            }
            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get { return element.Ordered; }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get { return inner != null ? inner.QueuedDelivery : false; }
            }
        }
    }
}
