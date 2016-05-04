//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;

    public sealed partial class UdpTransportElement : TransportElement
    {
        public UdpTransportElement() : base()
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            UdpTransportBindingElement udpTransportBindingElement = (UdpTransportBindingElement)bindingElement;
            
            udpTransportBindingElement.DuplicateMessageHistoryLength = this.DuplicateMessageHistoryLength;
            udpTransportBindingElement.MaxPendingMessagesTotalSize = this.MaxPendingMessagesTotalSize;
            udpTransportBindingElement.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            udpTransportBindingElement.MulticastInterfaceId = this.MulticastInterfaceId;
            this.RetransmissionSettings.ApplyConfiguration(udpTransportBindingElement.RetransmissionSettings);
            udpTransportBindingElement.SocketReceiveBufferSize = this.SocketReceiveBufferSize;
            udpTransportBindingElement.TimeToLive = this.TimeToLive;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            UdpTransportBindingElement udpTransportBindingElement = (UdpTransportBindingElement)bindingElement;

            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.DuplicateMessageHistoryLength, udpTransportBindingElement.DuplicateMessageHistoryLength);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize, udpTransportBindingElement.MaxPendingMessagesTotalSize);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxReceivedMessageSize, udpTransportBindingElement.MaxReceivedMessageSize);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MulticastInterfaceId, udpTransportBindingElement.MulticastInterfaceId);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.SocketReceiveBufferSize, udpTransportBindingElement.SocketReceiveBufferSize);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.TimeToLive, udpTransportBindingElement.TimeToLive);
            
            this.RetransmissionSettings.InitializeFrom(udpTransportBindingElement.RetransmissionSettings);
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule, Justification = "this property not a configuration property")]
        public override Type BindingElementType
        {
            get { return typeof(UdpTransportBindingElement); }
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new UdpTransportBindingElement();
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.DuplicateMessageHistoryLength, DefaultValue = UdpConstants.Defaults.DuplicateMessageHistoryLength)]
        [IntegerValidator(MinValue = 0)]
        public int DuplicateMessageHistoryLength
        {
            get { return (int)base[UdpTransportConfigurationStrings.DuplicateMessageHistoryLength]; }
            set { base[UdpTransportConfigurationStrings.DuplicateMessageHistoryLength] = value; }
        }     

        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize, DefaultValue = UdpConstants.Defaults.DefaultMaxPendingMessagesTotalSize)]
        [LongValidator(MinValue = UdpConstants.MinPendingMessagesTotalSize)]
        public long MaxPendingMessagesTotalSize
        {
            get { return (long)base[UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize]; }
            set { base[UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MulticastInterfaceId, DefaultValue = UdpConstants.Defaults.MulticastInterfaceId)]
        [StringValidator()]
        public string MulticastInterfaceId
        {
            get { return (string)base[UdpTransportConfigurationStrings.MulticastInterfaceId]; }
            set { base[UdpTransportConfigurationStrings.MulticastInterfaceId] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.SocketReceiveBufferSize, DefaultValue = UdpConstants.Defaults.SocketReceiveBufferSize)]
        [IntegerValidator(MinValue = UdpConstants.MinReceiveBufferSize)]
        public int SocketReceiveBufferSize
        {
            get { return (int)base[UdpTransportConfigurationStrings.SocketReceiveBufferSize]; }
            set { base[UdpTransportConfigurationStrings.SocketReceiveBufferSize] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.TimeToLive, DefaultValue = UdpConstants.Defaults.TimeToLive)]
        [IntegerValidator(MinValue = UdpConstants.MinTimeToLive, MaxValue = UdpConstants.MaxTimeToLive)]
        public int TimeToLive
        {
            get { return (int)base[UdpTransportConfigurationStrings.TimeToLive]; }
            set { base[UdpTransportConfigurationStrings.TimeToLive] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.RetransmissionSettings)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "there's no validator for UdpRetransmissionSettingsElement")]
        public UdpRetransmissionSettingsElement RetransmissionSettings
        {
            get { return (UdpRetransmissionSettingsElement)base[UdpTransportConfigurationStrings.RetransmissionSettings]; }
            set { base[UdpTransportConfigurationStrings.RetransmissionSettings] = value; }
        }    
    }
}
