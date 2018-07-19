//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;
    using System.Collections.ObjectModel;

    [MessageContract(IsWrapped = false)]
    class ProbeMatchesMessage11
    {
        private ProbeMatchesMessage11()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public DiscoveryMessageSequence11 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ProbeMatchesElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public ProbeMatches11 ProbeMatches
        {
            get;
            private set;
        }

        public static ProbeMatchesMessage11 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ProbeMatchesMessage11()
                {
                    MessageSequence = DiscoveryMessageSequence11.FromDiscoveryMessageSequence(messageSequence),
                    ProbeMatches = ProbeMatches11.Create(endpointDiscoveryMetadata)
                };
        }

        public static ProbeMatchesMessage11 Create(DiscoveryMessageSequence messageSequence, 
            Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadatas)
        {
            return new ProbeMatchesMessage11()
                {
                    MessageSequence = DiscoveryMessageSequence11.FromDiscoveryMessageSequence(messageSequence),
                    ProbeMatches = ProbeMatches11.Create(endpointDiscoveryMetadatas)
                };
        }
    }
}
