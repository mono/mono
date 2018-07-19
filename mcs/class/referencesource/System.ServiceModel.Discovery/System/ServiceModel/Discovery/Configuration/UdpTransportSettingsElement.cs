//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;

    [Fx.Tag.XamlVisible(false)]
    public sealed class UdpTransportSettingsElement : ConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        [ConfigurationProperty(ConfigurationStrings.DuplicateMessageHistoryLength, DefaultValue = DiscoveryDefaults.Udp.DuplicateMessageHistoryLength)]
        [IntegerValidator(MinValue = 0, MaxValue = int.MaxValue)]
        public int DuplicateMessageHistoryLength
        {
            get
            {
                return (int)base[ConfigurationStrings.DuplicateMessageHistoryLength];
            }
            set
            {
                base[ConfigurationStrings.DuplicateMessageHistoryLength] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingMessageCount, DefaultValue = UdpConstants.Defaults.MaxPendingMessageCount)]
        [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
        public int MaxPendingMessageCount
        {
            get
            {
                return (int)base[ConfigurationStrings.MaxPendingMessageCount];
            }
            set
            {
                base[ConfigurationStrings.MaxPendingMessageCount] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxMulticastRetransmitCount, DefaultValue = DiscoveryDefaults.Udp.MaxMulticastRetransmitCount)]
        [IntegerValidator(MinValue = 0, MaxValue = int.MaxValue)]
        public int MaxMulticastRetransmitCount
        {
            get
            {
                return (int)base[ConfigurationStrings.MaxMulticastRetransmitCount];
            }
            set
            {
                base[ConfigurationStrings.MaxMulticastRetransmitCount] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxUnicastRetransmitCount, DefaultValue = DiscoveryDefaults.Udp.MaxUnicastRetransmitCount)]
        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "Unicast is a valid name.")]
        [IntegerValidator(MinValue = 0, MaxValue = int.MaxValue)]
        public int MaxUnicastRetransmitCount
        {
            get
            {
                return (int)base[ConfigurationStrings.MaxUnicastRetransmitCount];
            }
            set
            {
                base[ConfigurationStrings.MaxUnicastRetransmitCount] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MulticastInterfaceId)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule)]
        public string MulticastInterfaceId
        {
            get
            {
                return (string)base[ConfigurationStrings.MulticastInterfaceId];
            }
            set
            {
                base[ConfigurationStrings.MulticastInterfaceId] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.SocketReceiveBufferSize, DefaultValue = UdpConstants.Defaults.SocketReceiveBufferSize)]
        [IntegerValidator(MinValue = UdpConstants.MinReceiveBufferSize, MaxValue = int.MaxValue)]
        public int SocketReceiveBufferSize
        {
            get
            {
                return (int)base[ConfigurationStrings.SocketReceiveBufferSize];
            }
            set
            {
                base[ConfigurationStrings.SocketReceiveBufferSize] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.TimeToLive, DefaultValue = UdpConstants.Defaults.TimeToLive)]
        [IntegerValidator(MinValue = UdpConstants.MinTimeToLive, MaxValue = UdpConstants.MaxTimeToLive)]
        public int TimeToLive
        {
            get
            {
                return (int)base[ConfigurationStrings.TimeToLive];
            }

            set
            {
                base[ConfigurationStrings.TimeToLive] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, DefaultValue = UdpConstants.Defaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = 1L, MaxValue = UdpConstants.Defaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return (long)base[ConfigurationStrings.MaxReceivedMessageSize];
            }
            set
            {
                base[ConfigurationStrings.MaxReceivedMessageSize] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBufferPoolSize, DefaultValue = TransportDefaults.MaxBufferPoolSize)]
        [LongValidator(MinValue = 1L, MaxValue = long.MaxValue)]
        public long MaxBufferPoolSize
        {
            get
            {
                return (long)base[ConfigurationStrings.MaxBufferPoolSize];
            }
            set
            {
                base[ConfigurationStrings.MaxBufferPoolSize] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.DuplicateMessageHistoryLength,
                        typeof(int),
                        DiscoveryDefaults.Udp.DuplicateMessageHistoryLength,
                        null,
                        new IntegerValidator(0, int.MaxValue),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxPendingMessageCount,
                        typeof(int),
                        UdpConstants.Defaults.MaxPendingMessageCount,
                        null,
                        new IntegerValidator(1, int.MaxValue),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxMulticastRetransmitCount,
                        typeof(int),
                        DiscoveryDefaults.Udp.MaxMulticastRetransmitCount,
                        null,
                        new IntegerValidator(0, int.MaxValue),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxUnicastRetransmitCount,
                        typeof(int),
                        DiscoveryDefaults.Udp.MaxUnicastRetransmitCount,
                        null,
                        new IntegerValidator(0, int.MaxValue),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MulticastInterfaceId,
                        typeof(string),
                        null,
                        null,
                        null,
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.SocketReceiveBufferSize,
                        typeof(int),
                        UdpConstants.Defaults.SocketReceiveBufferSize,
                        null,
                        new IntegerValidator(UdpConstants.MinReceiveBufferSize, int.MaxValue),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.TimeToLive,
                        typeof(int),
                        UdpConstants.Defaults.TimeToLive,
                        null,
                        new IntegerValidator(UdpConstants.MinTimeToLive, UdpConstants.MaxTimeToLive),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxReceivedMessageSize,
                        typeof(long),
                        UdpConstants.Defaults.MaxReceivedMessageSize,
                        null,
                        new LongValidator(1L, UdpConstants.Defaults.MaxReceivedMessageSize),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxBufferPoolSize,
                        typeof(long),
                        TransportDefaults.MaxBufferPoolSize,
                        null,
                        new LongValidator(1L, long.MaxValue),
                        System.Configuration.ConfigurationPropertyOptions.None));

                    this.properties = properties;
                }
                return this.properties;
            }
        }

        internal void ApplyConfiguration(UdpTransportSettings target)
        {
            target.DuplicateMessageHistoryLength = this.DuplicateMessageHistoryLength;
            target.MaxPendingMessageCount = this.MaxPendingMessageCount;
            target.MaxMulticastRetransmitCount = this.MaxMulticastRetransmitCount;
            target.MaxUnicastRetransmitCount = this.MaxUnicastRetransmitCount;
            target.MulticastInterfaceId = this.MulticastInterfaceId;
            target.SocketReceiveBufferSize = this.SocketReceiveBufferSize;
            target.TimeToLive = this.TimeToLive;
            target.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            target.MaxBufferPoolSize = this.MaxBufferPoolSize;
        }

        internal void InitializeFrom(UdpTransportSettings source)
        {
            this.DuplicateMessageHistoryLength = source.DuplicateMessageHistoryLength;
            this.MaxPendingMessageCount = source.MaxPendingMessageCount;
            this.MaxMulticastRetransmitCount = source.MaxMulticastRetransmitCount;
            this.MaxUnicastRetransmitCount = source.MaxUnicastRetransmitCount;
            this.MulticastInterfaceId = source.MulticastInterfaceId;
            this.SocketReceiveBufferSize = source.SocketReceiveBufferSize;
            this.TimeToLive = source.TimeToLive;
            this.MaxReceivedMessageSize = source.MaxReceivedMessageSize;
            this.MaxBufferPoolSize = source.MaxBufferPoolSize;
        }
    }
}
