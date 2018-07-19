//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;
    using System.Collections.ObjectModel;

    [MessageContract(IsWrapped = false)]
    class ProbeMatchesMessageCD1
    {
        private ProbeMatchesMessageCD1()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public DiscoveryMessageSequenceCD1 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ProbeMatchesElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public ProbeMatchesCD1 ProbeMatches
        {
            get;
            private set;
        }

        public static ProbeMatchesMessageCD1 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ProbeMatchesMessageCD1()
                {
                    MessageSequence = DiscoveryMessageSequenceCD1.FromDiscoveryMessageSequence(messageSequence),
                    ProbeMatches = ProbeMatchesCD1.Create(endpointDiscoveryMetadata)
                };
        }

        public static ProbeMatchesMessageCD1 Create(DiscoveryMessageSequence messageSequence,
            Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadatas)
        {
            return new ProbeMatchesMessageCD1()
                {
                    MessageSequence = DiscoveryMessageSequenceCD1.FromDiscoveryMessageSequence(messageSequence),
                    ProbeMatches = ProbeMatchesCD1.Create(endpointDiscoveryMetadatas)
                };
        }
    }
}
