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
    using SR2 = System.ServiceModel.Discovery.SR;

    public class AnnouncementEndpointElement : StandardEndpointElement
    {
        ConfigurationPropertyCollection properties;

        public AnnouncementEndpointElement()
            : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.MaxAnnouncementDelay, DefaultValue = ConfigurationStrings.TimeSpanZero)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaxAnnouncementDelay
        {
            get
            {
                return (TimeSpan)base[ConfigurationStrings.MaxAnnouncementDelay];
            }

            set
            {
                base[ConfigurationStrings.MaxAnnouncementDelay] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.DiscoveryVersion, DefaultValue = ProtocolStrings.VersionNameDefault)]
        [TypeConverter(typeof(DiscoveryVersionConverter))]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule)]
        public DiscoveryVersion DiscoveryVersion
        {
            get
            {
                return (DiscoveryVersion)base[ConfigurationStrings.DiscoveryVersion];
            }

            set
            {
                base[ConfigurationStrings.DiscoveryVersion] = value;
            }
        }

        protected internal override Type EndpointType
        {
            get { return typeof(AnnouncementEndpoint); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxAnnouncementDelay,
                        typeof(TimeSpan),
                        TimeSpan.Zero,
                        new TimeSpanOrInfiniteConverter(),
                        new TimeSpanOrInfiniteValidator(TimeSpan.Zero, TimeSpan.MaxValue),
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.DiscoveryVersion,
                        typeof(DiscoveryVersion),
                        DiscoveryVersion.DefaultDiscoveryVersion,
                        new DiscoveryVersionConverter(),
                        null,
                        ConfigurationPropertyOptions.None));

                    this.properties = properties;
                }
                return this.properties;
            }
        }

        protected internal override ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription)
        {
            return new AnnouncementEndpoint(this.DiscoveryVersion);
        }

        protected internal override void InitializeFrom(ServiceEndpoint endpoint)
        {
            base.InitializeFrom(endpoint);

            AnnouncementEndpoint source = (AnnouncementEndpoint)endpoint;
            this.MaxAnnouncementDelay = source.MaxAnnouncementDelay;
            this.DiscoveryVersion = source.DiscoveryVersion;
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            if (!String.IsNullOrEmpty(channelEndpointElement.Contract))
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.DiscoveryConfigContractSpecified(channelEndpointElement.Kind)));
            }
        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            if (!String.IsNullOrEmpty(serviceEndpointElement.Contract))
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.DiscoveryConfigContractSpecified(serviceEndpointElement.Kind)));
            }
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
            ApplyConfiguration(endpoint);
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
        {
            ApplyConfiguration(endpoint);
        }

        void ApplyConfiguration(ServiceEndpoint endpoint)
        {
            AnnouncementEndpoint announcementEndpoint = (AnnouncementEndpoint)endpoint;
            announcementEndpoint.MaxAnnouncementDelay = this.MaxAnnouncementDelay;
        }
    }
}
