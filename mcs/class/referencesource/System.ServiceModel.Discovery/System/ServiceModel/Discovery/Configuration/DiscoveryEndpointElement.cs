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

    public class DiscoveryEndpointElement : StandardEndpointElement
    {
        ConfigurationPropertyCollection properties;

        public DiscoveryEndpointElement() : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.MaxResponseDelay, DefaultValue = ConfigurationStrings.TimeSpanZero)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaxResponseDelay
        {
            get
            {
                return (TimeSpan)base[ConfigurationStrings.MaxResponseDelay];
            }

            set
            {
                base[ConfigurationStrings.MaxResponseDelay] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.DiscoveryMode, DefaultValue = ServiceDiscoveryMode.Managed)]        
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule)]
        public ServiceDiscoveryMode DiscoveryMode
        {
            get
            {
                return (ServiceDiscoveryMode)base[ConfigurationStrings.DiscoveryMode];
            }

            set
            {
                base[ConfigurationStrings.DiscoveryMode] = value;
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
            get { return typeof(DiscoveryEndpoint); }
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
                        ConfigurationStrings.MaxResponseDelay, 
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

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.DiscoveryMode, 
                        typeof(ServiceDiscoveryMode), 
                        ServiceDiscoveryMode.Managed, 
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
            return new DiscoveryEndpoint(this.DiscoveryVersion, this.DiscoveryMode);
        }

        protected internal override void InitializeFrom(ServiceEndpoint endpoint)
        {
            base.InitializeFrom(endpoint);

            DiscoveryEndpoint source = (DiscoveryEndpoint)endpoint;
            this.MaxResponseDelay = source.MaxResponseDelay;
            this.DiscoveryVersion = source.DiscoveryVersion;
            this.DiscoveryMode = source.DiscoveryMode;
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

            if (PropertyValueOrigin.Default == serviceEndpointElement.ElementInformation.Properties[ConfigurationStrings.IsSystemEndpoint].ValueOrigin)
            {
                serviceEndpointElement.IsSystemEndpoint = true;
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
            DiscoveryEndpoint discoveryEndpoint = (DiscoveryEndpoint)endpoint;
            discoveryEndpoint.MaxResponseDelay = this.MaxResponseDelay;
        }
    }
}
