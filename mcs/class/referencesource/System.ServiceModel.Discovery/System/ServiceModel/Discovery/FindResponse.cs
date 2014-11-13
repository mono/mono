//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public class FindResponse
    {
        Dictionary<EndpointDiscoveryMetadata, DiscoveryMessageSequence> messageSequenceTable;
        Collection<EndpointDiscoveryMetadata> endpoints;

        internal FindResponse()
        {
            this.endpoints = new Collection<EndpointDiscoveryMetadata>();
            this.messageSequenceTable = new Dictionary<EndpointDiscoveryMetadata, DiscoveryMessageSequence>();
        }

        public Collection<EndpointDiscoveryMetadata> Endpoints
        {
            get
            {
                return this.endpoints;
            }
        }

        public DiscoveryMessageSequence GetMessageSequence(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            if (endpointDiscoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDiscoveryMetadata");
            }

            DiscoveryMessageSequence messageSequence = null;
            if (!this.messageSequenceTable.TryGetValue(endpointDiscoveryMetadata, out messageSequence))
            {
                throw FxTrace.Exception.Argument("endpointDiscoveryMetadata", SR2.DiscoveryFindResponseMessageSequenceNotFound);
            }

            return messageSequence;
        }

        internal void AddDiscoveredEndpoint(EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            DiscoveryMessageSequence messageSequence)
        {
            this.messageSequenceTable.Add(endpointDiscoveryMetadata, messageSequence);
            this.endpoints.Add(endpointDiscoveryMetadata);
        }
    }
}
