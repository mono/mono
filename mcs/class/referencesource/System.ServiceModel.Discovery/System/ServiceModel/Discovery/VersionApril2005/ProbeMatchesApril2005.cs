//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;

    [CollectionDataContract(ItemName = ProtocolStrings.SchemaNames.ProbeMatchElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
    class ProbeMatchesApril2005 : Collection<EndpointDiscoveryMetadataApril2005>
    {
        private ProbeMatchesApril2005()
        {
        }

        public static ProbeMatchesApril2005 Create(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ProbeMatchesApril2005()
                {
                    EndpointDiscoveryMetadataApril2005.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }

        public static ProbeMatchesApril2005 Create(Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadatas)
        {
            ProbeMatchesApril2005 probeMatches = new ProbeMatchesApril2005();
            if (endpointDiscoveryMetadatas != null)
            {
                foreach (EndpointDiscoveryMetadata endpointDiscoveryMetadata in endpointDiscoveryMetadatas)
                {
                    probeMatches.Add(EndpointDiscoveryMetadataApril2005.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata));
                }
            };

            return probeMatches;
        }
    }
}
