//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;

    [CollectionDataContract(ItemName = ProtocolStrings.SchemaNames.ProbeMatchElement, Namespace = ProtocolStrings.Version11.Namespace)]
    class ProbeMatches11 : Collection<EndpointDiscoveryMetadata11>
    {
        private ProbeMatches11()
        {
        }

        public static ProbeMatches11 Create(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ProbeMatches11()
                {
                    EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }

        public static ProbeMatches11 Create(Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadatas)
        {
            ProbeMatches11 probeMatches = new ProbeMatches11();
            if (endpointDiscoveryMetadatas != null)
            {
                foreach (EndpointDiscoveryMetadata endpointDiscoveryMetadata in endpointDiscoveryMetadatas)
                {
                    probeMatches.Add(EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata));
                }
            };

            return probeMatches;
        }
    }
}
