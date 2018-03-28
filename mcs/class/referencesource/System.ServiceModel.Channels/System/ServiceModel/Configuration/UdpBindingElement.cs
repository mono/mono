//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Text;

    public partial class UdpBindingElement : StandardBindingElement
    {
        public UdpBindingElement(string name)
            : base(name)
        {
        }

        public UdpBindingElement()
            : this(null)
        {
        }
        
        protected override Type BindingElementType
        {
            get { return typeof(UdpBinding); }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.DuplicateMessageHistoryLength, DefaultValue = UdpConstants.Defaults.DuplicateMessageHistoryLength)]
        [IntegerValidator(MinValue = 0)]
        public int DuplicateMessageHistoryLength
        {
            get { return (int)base[UdpTransportConfigurationStrings.DuplicateMessageHistoryLength]; }
            set { base[UdpTransportConfigurationStrings.DuplicateMessageHistoryLength] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxBufferPoolSize, DefaultValue = TransportDefaults.MaxBufferPoolSize)]
        [LongValidator(MinValue = 0)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[UdpTransportConfigurationStrings.MaxBufferPoolSize]; }
            set { base[UdpTransportConfigurationStrings.MaxBufferPoolSize] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxRetransmitCount, DefaultValue = UdpConstants.Defaults.MaxRetransmitCount)]
        [IntegerValidator(MinValue = 0)]
        public int MaxRetransmitCount
        {
            get { return (int)base[UdpTransportConfigurationStrings.MaxRetransmitCount]; }
            set { base[UdpTransportConfigurationStrings.MaxRetransmitCount] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize, DefaultValue = UdpConstants.Defaults.DefaultMaxPendingMessagesTotalSize)]
        [LongValidator(MinValue = UdpConstants.MinPendingMessagesTotalSize)]
        public long MaxPendingMessagesTotalSize
        {
            get { return (long)base[UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize]; }
            set { base[UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize] = value; }
        }

        // Min value has to be 1, because it's 1 in the TransportBindingElement
        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxReceivedMessageSize, DefaultValue = UdpConstants.Defaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = 1)]
        public long MaxReceivedMessageSize
        {
            get { return (long)this[UdpTransportConfigurationStrings.MaxReceivedMessageSize]; }
            set { this[UdpTransportConfigurationStrings.MaxReceivedMessageSize] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MulticastInterfaceId, DefaultValue = UdpConstants.Defaults.MulticastInterfaceId)]
        [StringValidator()]
        public string MulticastInterfaceId
        {
            get { return (string)base[UdpTransportConfigurationStrings.MulticastInterfaceId]; }
            set { base[UdpTransportConfigurationStrings.MulticastInterfaceId] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement)base[UdpTransportConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.TextEncoding, DefaultValue = UdpConstants.Defaults.EncodingString)]
        [TypeConverter(typeof(EncodingConverter))]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "this property not a configuration property")]
        public Encoding TextEncoding
        {
            get { return (Encoding)base[UdpTransportConfigurationStrings.TextEncoding]; }
            set { base[UdpTransportConfigurationStrings.TextEncoding] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.TimeToLive, DefaultValue = UdpConstants.Defaults.TimeToLive)]
        [IntegerValidator(MinValue = UdpConstants.MinTimeToLive, MaxValue = UdpConstants.MaxTimeToLive)]
        public int TimeToLive
        {
            get { return (int)base[UdpTransportConfigurationStrings.TimeToLive]; }
            set { base[UdpTransportConfigurationStrings.TimeToLive] = value; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            UdpBinding udpBinding = (UdpBinding)binding;

            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.DuplicateMessageHistoryLength, udpBinding.DuplicateMessageHistoryLength);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxBufferPoolSize, udpBinding.MaxBufferPoolSize);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxRetransmitCount, udpBinding.MaxRetransmitCount);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxPendingMessagesTotalSize, udpBinding.MaxPendingMessagesTotalSize);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxReceivedMessageSize, udpBinding.MaxReceivedMessageSize);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MulticastInterfaceId, udpBinding.MulticastInterfaceId);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.TimeToLive, udpBinding.TimeToLive);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            UdpBinding udpBinding = (UdpBinding)binding;

            udpBinding.DuplicateMessageHistoryLength = this.DuplicateMessageHistoryLength;
            udpBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            udpBinding.MaxRetransmitCount = this.MaxRetransmitCount;
            udpBinding.MaxPendingMessagesTotalSize = this.MaxPendingMessagesTotalSize;
            udpBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            udpBinding.MulticastInterfaceId = this.MulticastInterfaceId;
            udpBinding.TimeToLive = this.TimeToLive;
        }
    }
}
