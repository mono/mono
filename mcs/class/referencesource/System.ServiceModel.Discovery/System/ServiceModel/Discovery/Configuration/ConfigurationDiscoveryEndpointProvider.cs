//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using SR2 = System.ServiceModel.Discovery.SR;

    class ConfigurationDiscoveryEndpointProvider : DiscoveryEndpointProvider
    {
        readonly ChannelEndpointElement channelEndpointElement;

        public ConfigurationDiscoveryEndpointProvider()            
        {
            this.channelEndpointElement = ConfigurationUtility.GetDefaultDiscoveryEndpointElement();
        }

        public ConfigurationDiscoveryEndpointProvider(ChannelEndpointElement channelEndpointElement)
        {
            Fx.Assert(channelEndpointElement != null, "The channelEndpointElement parameter must be non null.");

            ConfigurationDiscoveryEndpointProvider.ValidateAndGetDiscoveryEndpoint(channelEndpointElement);
            this.channelEndpointElement = channelEndpointElement;
        }

        public override DiscoveryEndpoint GetDiscoveryEndpoint()
        {
            return ConfigurationDiscoveryEndpointProvider.ValidateAndGetDiscoveryEndpoint(this.channelEndpointElement);
        }

        static DiscoveryEndpoint ValidateAndGetDiscoveryEndpoint(ChannelEndpointElement channelEndpointElement)
        {
            if (string.IsNullOrEmpty(channelEndpointElement.Kind))
            {
                throw FxTrace.Exception.AsError(
                    new ConfigurationErrorsException(
                    SR2.DiscoveryConfigDiscoveryEndpointMissingKind(
                    typeof(DiscoveryEndpoint).FullName)));
            }

            ServiceEndpoint serviceEndpoint = ConfigLoader.LookupEndpoint(channelEndpointElement, null);

            if (serviceEndpoint == null)
            {
                throw FxTrace.Exception.AsError(
                    new ConfigurationErrorsException(
                    SR2.DiscoveryConfigInvalidEndpointConfiguration(
                    channelEndpointElement.Kind)));
            }

            DiscoveryEndpoint discoveryEndpoint = serviceEndpoint as DiscoveryEndpoint;
            if (discoveryEndpoint == null)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(
                        SR2.DiscoveryConfigInvalidDiscoveryEndpoint(
                        typeof(DiscoveryEndpoint).FullName,
                        channelEndpointElement.Kind,
                        serviceEndpoint.GetType().FullName)));
            }

            return discoveryEndpoint;
        }
    }
}
