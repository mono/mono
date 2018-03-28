//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing.Configuration
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    sealed class ClientEndpointLoader : ChannelFactory
    {
        ClientEndpointLoader(string configurationName)
        {
            base.InitializeEndpoint(configurationName, null);
            base.Endpoint.Name = configurationName;
        }

        public static ServiceEndpoint LoadEndpoint(string configurationName)
        {
            using (ClientEndpointLoader loader = new ClientEndpointLoader(configurationName))
            {
                return loader.Endpoint;
            }
        }

        protected override ServiceEndpoint CreateDescription()
        {
            ServiceEndpoint ep = new ServiceEndpoint(new ContractDescription("contract"));
            ep.Contract.ConfigurationName = "*";
            return ep;
        }
    }
}
