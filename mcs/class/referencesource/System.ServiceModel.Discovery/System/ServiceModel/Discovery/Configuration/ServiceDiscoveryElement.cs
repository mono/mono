//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using SR2 = System.ServiceModel.Discovery.SR;

    public sealed class ServiceDiscoveryElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection properties;

        public ServiceDiscoveryElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.AnnouncementEndpoints)]
        public AnnouncementChannelEndpointElementCollection AnnouncementEndpoints
        {
            get
            {
                return (AnnouncementChannelEndpointElementCollection)base[ConfigurationStrings.AnnouncementEndpoints];
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to determine the type of the behavior.")]
        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceDiscoveryBehavior);
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
                        ConfigurationStrings.AnnouncementEndpoints,
                        typeof(AnnouncementChannelEndpointElementCollection), 
                        null, 
                        null, 
                        null, 
                        ConfigurationPropertyOptions.None));

                    this.properties = properties;
                }
                return this.properties;
            }
        }

        protected internal override object CreateBehavior()
        {
            ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();

            AnnouncementEndpoint announcementEndpoint;
            foreach (ChannelEndpointElement channelEndpointElement in this.AnnouncementEndpoints)
            {
                if (string.IsNullOrEmpty(channelEndpointElement.Kind))
                {
                    throw FxTrace.Exception.AsError(
                        new ConfigurationErrorsException(
                        SR2.DiscoveryConfigAnnouncementEndpointMissingKind(
                        typeof(AnnouncementEndpoint).FullName)));
                }

                ServiceEndpoint serviceEndpoint = ConfigLoader.LookupEndpoint(channelEndpointElement, null);
                if (serviceEndpoint == null)
                {
                    throw FxTrace.Exception.AsError(
                        new ConfigurationErrorsException(
                        SR2.DiscoveryConfigInvalidEndpointConfiguration(
                        channelEndpointElement.Kind)));
                }

                announcementEndpoint = serviceEndpoint as AnnouncementEndpoint;
                if (announcementEndpoint == null)
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(
                        SR2.DiscoveryConfigInvalidAnnouncementEndpoint(
                        channelEndpointElement.Kind,
                        serviceEndpoint.GetType().FullName,
                        typeof(AnnouncementEndpoint).FullName)));
                }

                serviceDiscoveryBehavior.AnnouncementEndpoints.Add(announcementEndpoint);
            }

            return serviceDiscoveryBehavior;
        }
    }
}
