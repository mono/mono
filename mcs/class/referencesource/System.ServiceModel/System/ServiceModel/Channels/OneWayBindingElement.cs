//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class OneWayBindingElement : BindingElement,
        IPolicyExportExtension
    {
        ChannelPoolSettings channelPoolSettings;
        bool packetRoutable;
        int maxAcceptedChannels;

        public OneWayBindingElement()
        {
            this.channelPoolSettings = new ChannelPoolSettings();
            this.packetRoutable = OneWayDefaults.PacketRoutable;
            this.maxAcceptedChannels = OneWayDefaults.MaxAcceptedChannels;
        }

        OneWayBindingElement(OneWayBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.channelPoolSettings = elementToBeCloned.ChannelPoolSettings.Clone();
            this.packetRoutable = elementToBeCloned.PacketRoutable;
            this.maxAcceptedChannels = elementToBeCloned.maxAcceptedChannels;
        }

        public ChannelPoolSettings ChannelPoolSettings
        {
            get
            {
                return this.channelPoolSettings;
            }

            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.channelPoolSettings = value;
            }
        }

        // server
        [DefaultValue(OneWayDefaults.MaxAcceptedChannels)]
        public int MaxAcceptedChannels
        {
            get
            {
                return this.maxAcceptedChannels;
            }
            set
            {
                if (value <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBePositive)));

                this.maxAcceptedChannels = value;
            }
        }

        [DefaultValue(OneWayDefaults.PacketRoutable)]
        public bool PacketRoutable
        {
            get
            {
                return this.packetRoutable;
            }

            set
            {
                this.packetRoutable = value;
            }
        }

        public override BindingElement Clone()
        {
            return new OneWayBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IOutputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel",
                    SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            // Prefer IDuplexChannel
            if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
            {
                return (IChannelFactory<TChannel>)(object)new DuplexOneWayChannelFactory(this, context);
            }

            // Prefer IDuplexSessionChannel
            if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
            {
                return (IChannelFactory<TChannel>)(object)new DuplexSessionOneWayChannelFactory(this, context);
            }

            // Followed by IRequestChannel
            if (context.CanBuildInnerChannelFactory<IRequestChannel>())
            {
                return (IChannelFactory<TChannel>)(object)new RequestOneWayChannelFactory(this, context);
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
#pragma warning suppress 56506 // context.Binding will never be null.
new InvalidOperationException(SR.GetString(SR.OneWayInternalTypeNotSupported, context.Binding.Name)));
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IInputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel",
                    SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            // Prefer IDuplexChannel
            if (context.CanBuildInnerChannelListener<IDuplexChannel>())
            {
                return (IChannelListener<TChannel>)(object)new DuplexOneWayChannelListener(this, context);
            }

            // Prefer IDuplexSessionChannel
            if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
            {
                return (IChannelListener<TChannel>)(object)new DuplexSessionOneWayChannelListener(this, context);
            }

            // Followed by IRequestChannel
            if (context.CanBuildInnerChannelListener<IReplyChannel>())
            {
                return (IChannelListener<TChannel>)(object)new ReplyOneWayChannelListener(this, context);
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
#pragma warning suppress 56506 // context.Binding will never be null.
new InvalidOperationException(SR.GetString(SR.OneWayInternalTypeNotSupported, context.Binding.Name)));
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IOutputChannel))
            {
                return false;
            }

            // we can convert IDuplexChannel
            if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
            {
                return true;
            }

            // we can convert IDuplexSessionChannel
            if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
            {
                return true;
            }

            // and also IRequestChannel
            if (context.CanBuildInnerChannelFactory<IRequestChannel>())
            {
                return true;
            }

            return false;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IInputChannel))
            {
                return false;
            }

            // we can convert IDuplexChannel
            if (context.CanBuildInnerChannelListener<IDuplexChannel>())
            {
                return true;
            }

            // we can convert IDuplexSessionChannel
            if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
            {
                return true;
            }

            // and also IRequestChannel
            if (context.CanBuildInnerChannelListener<IReplyChannel>())
            {
                return true;
            }

            return false;
        }

        static MessagePartSpecification oneWaySignedMessageParts;

        static MessagePartSpecification OneWaySignedMessageParts
        {
            get
            {
                if (oneWaySignedMessageParts == null)
                {
                    MessagePartSpecification tempSignedMessageParts = new MessagePartSpecification(
                        new XmlQualifiedName(DotNetOneWayStrings.HeaderName, DotNetOneWayStrings.Namespace)
                        );
                    tempSignedMessageParts.MakeReadOnly();
                    oneWaySignedMessageParts = tempSignedMessageParts;
                }

                return oneWaySignedMessageParts;
            }
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            // make sure our Datagram header is signed
            if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                ChannelProtectionRequirements myRequirements = new ChannelProtectionRequirements();
                if (PacketRoutable)
                {
                    myRequirements.IncomingSignatureParts.AddParts(OneWaySignedMessageParts);
                    myRequirements.OutgoingSignatureParts.AddParts(OneWaySignedMessageParts);
                }
                ChannelProtectionRequirements innerRequirements = context.GetInnerProperty<ChannelProtectionRequirements>();
                if (innerRequirements != null)
                {
                    myRequirements.Add(innerRequirements);
                }
                return (T)(object)myRequirements;
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }

            OneWayBindingElement oneWayBindingElement = b as OneWayBindingElement;
            if (oneWayBindingElement == null)
            {
                return false;
            }

            if (!this.channelPoolSettings.IsMatch(oneWayBindingElement.ChannelPoolSettings))
            {
                return false;
            }

            if (this.packetRoutable != oneWayBindingElement.PacketRoutable)
            {
                return false;
            }

            if (this.maxAcceptedChannels != oneWayBindingElement.MaxAcceptedChannels)
            {
                return false;
            }

            return true;
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (context.BindingElements != null)
            {
                OneWayBindingElement oneWay = context.BindingElements.Find<OneWayBindingElement>();

                if (oneWay != null)
                {
                    // base assertion
                    XmlDocument doc = new XmlDocument();
                    XmlElement assertion = doc.CreateElement(OneWayPolicyConstants.Prefix,
                        OneWayPolicyConstants.OneWay, OneWayPolicyConstants.Namespace);

                    if (oneWay.PacketRoutable)
                    {
                        // add nested packet routable assertion 
                        XmlElement child = doc.CreateElement(OneWayPolicyConstants.Prefix, OneWayPolicyConstants.PacketRoutable, OneWayPolicyConstants.Namespace);
                        assertion.AppendChild(child);
                    }

                    context.GetBindingAssertions().Add(assertion);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeChannelPoolSettings()
        {
            return this.channelPoolSettings.InternalShouldSerialize();
        }
    }
}
