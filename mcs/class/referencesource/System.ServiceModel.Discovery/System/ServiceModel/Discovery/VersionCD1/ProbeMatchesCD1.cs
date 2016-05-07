//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;

    [CollectionDataContract(ItemName = ProtocolStrings.SchemaNames.ProbeMatchElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
    class ProbeMatchesCD1 : Collection<EndpointDiscoveryMetadataCD1>
    {
        private ProbeMatchesCD1()
        {
        }

        public static ProbeMatchesCD1 Create(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ProbeMatchesCD1()
                {
                    EndpointDiscoveryMetadataCD1.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }

        public static ProbeMatchesCD1 Create(Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadatas)
        {
            ProbeMatchesCD1 probeMatches = new ProbeMatchesCD1();
            if (endpointDiscoveryMetadatas != null)
            {
                foreach (EndpointDiscoveryMetadata endpointDiscoveryMetadata in endpointDiscoveryMetadatas)
                {
                    probeMatches.Add(EndpointDiscoveryMetadataCD1.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata));
                }
            };

            return probeMatches;
        }
    }
}
