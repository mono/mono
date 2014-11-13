//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ResolveMatchesMessageCD1
    {
        private ResolveMatchesMessageCD1()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public DiscoveryMessageSequenceCD1 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ResolveMatchesElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public ResolveMatchesCD1 ResolveMatches
        {
            get;
            private set;
        }

        public static ResolveMatchesMessageCD1 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ResolveMatchesMessageCD1()
                {
                    MessageSequence = DiscoveryMessageSequenceCD1.FromDiscoveryMessageSequence(messageSequence),
                    ResolveMatches = ResolveMatchesCD1.Create(endpointDiscoveryMetadata)
                };
        }
    }
}
