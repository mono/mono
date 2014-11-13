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

    class ConfigurationUtility
    {
        public static ChannelEndpointElement GetDefaultDiscoveryEndpointElement()
        {
            return new ChannelEndpointElement() { Kind = ConfigurationStrings.UdpDiscoveryEndpoint };
        }   

        public static T LookupEndpoint<T>(ChannelEndpointElement channelEndpointElement) where T : ServiceEndpoint
        {
            Fx.Assert(channelEndpointElement != null, "The parameter channelEndpointElement must be non null.");
            Fx.Assert(!string.IsNullOrEmpty(channelEndpointElement.Kind), "The Kind property of the specified channelEndpointElement parameter cannot be null or empty.");

            return ConfigLoader.LookupEndpoint(channelEndpointElement, null) as T;
        }
        internal static void InitializeAndValidateUdpChannelEndpointElement(ChannelEndpointElement channelEndpointElement)
        {
            if (!(channelEndpointElement.Address == null || String.IsNullOrEmpty(channelEndpointElement.Address.ToString())))
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.DiscoveryConfigAddressSpecifiedForUdpDiscoveryEndpoint(channelEndpointElement.Kind)));
            }
            channelEndpointElement.Address = null;
        }

        internal static void InitializeAndValidateUdpServiceEndpointElement(ServiceEndpointElement serviceEndpointElement)
        {
            if (!(serviceEndpointElement.Address == null || String.IsNullOrEmpty(serviceEndpointElement.Address.ToString())))
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.DiscoveryConfigAddressSpecifiedForUdpDiscoveryEndpoint(serviceEndpointElement.Kind)));
            }
            serviceEndpointElement.Address = null;

            if (serviceEndpointElement.ListenUri != null)
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.DiscoveryConfigListenUriSpecifiedForUdpDiscoveryEndpoint(serviceEndpointElement.Kind)));
            }
        }

        internal static TEndpoint LookupEndpointFromClientSection<TEndpoint>(string endpointConfigurationName) where TEndpoint : ServiceEndpoint
        {
            Fx.Assert(endpointConfigurationName != null, "The endpointConfigurationName parameter must be non null.");

            TEndpoint retval = default(TEndpoint);

            bool wildcard = string.Equals(endpointConfigurationName, "*", StringComparison.Ordinal);

            ClientSection clientSection = ClientSection.GetSection();
            foreach (ChannelEndpointElement channelEndpointElement in clientSection.Endpoints)
            {
                if (string.IsNullOrEmpty(channelEndpointElement.Kind))
                {
                    continue;
                }

                if (endpointConfigurationName == channelEndpointElement.Name || wildcard)
                {
                    TEndpoint endpoint = LookupEndpoint<TEndpoint>(channelEndpointElement);
                    if (endpoint != null)
                    {
                        if (retval != null)
                        {
                            if (wildcard)
                            {
                                throw FxTrace.Exception.AsError(
                                    new InvalidOperationException(
                                    SR2.DiscoveryConfigMultipleEndpointsMatchWildcard(
                                    typeof(TEndpoint).FullName,
                                    clientSection.SectionInformation.SectionName)));
                            }
                            else
                            {
                                throw FxTrace.Exception.AsError(
                                    new InvalidOperationException(
                                    SR2.DiscoveryConfigMultipleEndpointsMatch(
                                    typeof(TEndpoint).FullName,
                                    endpointConfigurationName,
                                    clientSection.SectionInformation.SectionName)));
                            }
                        }
                        else
                        {
                            retval = endpoint;
                        }
                    }
                }
            }

            if (retval == null)
            {
                if (wildcard)
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(
                        SR2.DiscoveryConfigNoEndpointsMatchWildcard(
                        typeof(TEndpoint).FullName,
                        clientSection.SectionInformation.SectionName)));
                }
                else
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(
                        SR2.DiscoveryConfigNoEndpointsMatch(
                        typeof(TEndpoint).FullName,
                        endpointConfigurationName,
                        clientSection.SectionInformation.SectionName)));
                }
            }

            return retval;
        }
    }
}

