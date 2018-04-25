//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    public class UdpDiscoveryEndpointElement : DiscoveryEndpointElement
    {
        ConfigurationPropertyCollection properties;

        public UdpDiscoveryEndpointElement()
            : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.MaxResponseDelay, DefaultValue = DiscoveryDefaults.Udp.AppMaxDelayString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public new TimeSpan MaxResponseDelay
        {
            get
            {
                return base.MaxResponseDelay;
            }

            set
            {
                base.MaxResponseDelay = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.DiscoveryMode, DefaultValue = ServiceDiscoveryMode.Adhoc)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule)]
        public new ServiceDiscoveryMode DiscoveryMode
        {
            get
            {
                return base.DiscoveryMode;
            }

            set
            {
                base.DiscoveryMode = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MulticastAddress, DefaultValue = ProtocolStrings.Udp.MulticastIPv4Address)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule)]
        public Uri MulticastAddress
        {
            get
            {
                return (Uri)base[ConfigurationStrings.MulticastAddress];
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }
                base[ConfigurationStrings.MulticastAddress] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.TransportSettings)]
        public UdpTransportSettingsElement TransportSettings
        {
            get
            {
                return (UdpTransportSettingsElement)base[ConfigurationStrings.TransportSettings];
            }
        }

        protected internal override Type EndpointType
        {
            get { return typeof(UdpDiscoveryEndpoint); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;

                    properties.Remove(ConfigurationStrings.DiscoveryMode);
                    properties.Remove(ConfigurationStrings.MaxResponseDelay);

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxResponseDelay,
                        typeof(TimeSpan),
                        DiscoveryDefaults.Udp.AppMaxDelay,
                        new TimeSpanOrInfiniteConverter(),
                        new TimeSpanOrInfiniteValidator(TimeSpan.Zero, TimeSpan.MaxValue),
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.DiscoveryMode,
                        typeof(ServiceDiscoveryMode),
                        ServiceDiscoveryMode.Adhoc,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MulticastAddress,
                        typeof(Uri),
                        UdpDiscoveryEndpoint.DefaultIPv4MulticastAddress,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.TransportSettings,
                        typeof(UdpTransportSettingsElement),
                        null,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));

                    this.properties = properties;
                }
                return this.properties;
            }
        }

        protected internal override ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription)
        {
            return new UdpDiscoveryEndpoint(this.DiscoveryVersion, this.MulticastAddress);
        }

        protected internal override void InitializeFrom(ServiceEndpoint endpoint)
        {
            base.InitializeFrom(endpoint);

            UdpDiscoveryEndpoint source = (UdpDiscoveryEndpoint)endpoint;
            this.MaxResponseDelay = source.MaxResponseDelay;            
            this.DiscoveryMode = source.DiscoveryMode;
            this.MulticastAddress = source.MulticastAddress;
#pragma warning disable 0618
            this.TransportSettings.InitializeFrom(source.TransportSettings);
#pragma warning restore 0618
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            base.OnInitializeAndValidate(channelEndpointElement);

            ConfigurationUtility.InitializeAndValidateUdpChannelEndpointElement(channelEndpointElement);
        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            base.OnInitializeAndValidate(serviceEndpointElement);

            ConfigurationUtility.InitializeAndValidateUdpServiceEndpointElement(serviceEndpointElement);
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
            base.OnApplyConfiguration(endpoint, serviceEndpointElement);
            ApplyConfiguration(endpoint);
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
        {
            base.OnApplyConfiguration(endpoint, serviceEndpointElement);
            ApplyConfiguration(endpoint);
        }

        void ApplyConfiguration(ServiceEndpoint endpoint)
        {
            UdpDiscoveryEndpoint udpDiscoveryEndpoint = (UdpDiscoveryEndpoint)endpoint;
            udpDiscoveryEndpoint.MulticastAddress = this.MulticastAddress;
#pragma warning disable 0618
            this.TransportSettings.ApplyConfiguration(udpDiscoveryEndpoint.TransportSettings);
#pragma warning restore 0618
        }
    }
}
