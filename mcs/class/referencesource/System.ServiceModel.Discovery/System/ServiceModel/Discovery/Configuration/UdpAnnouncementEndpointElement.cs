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

    public class UdpAnnouncementEndpointElement : AnnouncementEndpointElement
    {
        ConfigurationPropertyCollection properties;

        public UdpAnnouncementEndpointElement()
            : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.MaxAnnouncementDelay, DefaultValue = DiscoveryDefaults.Udp.AppMaxDelayString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public new TimeSpan MaxAnnouncementDelay
        {
            get
            {
                return base.MaxAnnouncementDelay;
            }

            set
            {
                base.MaxAnnouncementDelay = value;
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
            get { return typeof(UdpAnnouncementEndpoint); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;

                    properties.Remove(ConfigurationStrings.MaxAnnouncementDelay);

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxAnnouncementDelay,
                        typeof(TimeSpan),
                        DiscoveryDefaults.Udp.AppMaxDelay,
                        new TimeSpanOrInfiniteConverter(),
                        new TimeSpanOrInfiniteValidator(TimeSpan.Zero, TimeSpan.MaxValue),
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MulticastAddress,
                        typeof(Uri),
                        UdpAnnouncementEndpoint.DefaultIPv4MulticastAddress,
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
            return new UdpAnnouncementEndpoint(this.DiscoveryVersion, this.MulticastAddress);
        }

        protected internal override void InitializeFrom(ServiceEndpoint endpoint)
        {
            base.InitializeFrom(endpoint);

            UdpAnnouncementEndpoint source = (UdpAnnouncementEndpoint)endpoint;
            this.MaxAnnouncementDelay = source.MaxAnnouncementDelay;
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
            UdpAnnouncementEndpoint udpAnnouncementEndpoint = (UdpAnnouncementEndpoint)endpoint;
            udpAnnouncementEndpoint.MulticastAddress = this.MulticastAddress;
#pragma warning disable 0618
            this.TransportSettings.ApplyConfiguration(udpAnnouncementEndpoint.TransportSettings);
#pragma warning restore 0618
        }
    }
}
