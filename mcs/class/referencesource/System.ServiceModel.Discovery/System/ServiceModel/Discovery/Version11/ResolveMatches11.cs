//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = ProtocolStrings.Version11.Namespace)]
    class ResolveMatches11
    {
        private ResolveMatches11()
        {
        }

        [DataMember(EmitDefaultValue = false, Name = ProtocolStrings.SchemaNames.ResolveMatchElement)]
        public EndpointDiscoveryMetadata11 ResolveMatch
        {
            get;
            private set;
        }

        public static ResolveMatches11 Create(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            ResolveMatches11 resolveMatches = new ResolveMatches11();
            if (endpointDiscoveryMetadata != null)
            {
                resolveMatches.ResolveMatch = 
                    EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata);
            }

            return resolveMatches;
        }
    }
}
