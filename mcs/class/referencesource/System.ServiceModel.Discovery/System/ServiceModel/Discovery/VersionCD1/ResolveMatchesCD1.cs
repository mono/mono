//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = ProtocolStrings.VersionCD1.Namespace)]
    class ResolveMatchesCD1
    {
        private ResolveMatchesCD1()
        {
        }

        [DataMember(EmitDefaultValue = false, Name = ProtocolStrings.SchemaNames.ResolveMatchElement)]
        public EndpointDiscoveryMetadataCD1 ResolveMatch
        {
            get;
            private set;
        }

        public static ResolveMatchesCD1 Create(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            ResolveMatchesCD1 resolveMatches = new ResolveMatchesCD1();
            if (endpointDiscoveryMetadata != null)
            {
                resolveMatches.ResolveMatch =
                    EndpointDiscoveryMetadataCD1.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata);
            }

            return resolveMatches;
        }
    }
}
