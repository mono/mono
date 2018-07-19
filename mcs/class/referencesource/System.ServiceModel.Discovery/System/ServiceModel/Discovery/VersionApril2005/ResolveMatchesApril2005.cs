//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = ProtocolStrings.VersionApril2005.Namespace)]
    class ResolveMatchesApril2005
    {
        private ResolveMatchesApril2005()
        {
        }

        [DataMember(EmitDefaultValue = false, Name = ProtocolStrings.SchemaNames.ResolveMatchElement)]
        public EndpointDiscoveryMetadataApril2005 ResolveMatch
        {
            get;
            private set;
        }

        public static ResolveMatchesApril2005 Create(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            ResolveMatchesApril2005 resolveMatches = new ResolveMatchesApril2005();
            if (endpointDiscoveryMetadata != null)
            {
                resolveMatches.ResolveMatch =
                    EndpointDiscoveryMetadataApril2005.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata);
            }

            return resolveMatches;
        }
    }
}
