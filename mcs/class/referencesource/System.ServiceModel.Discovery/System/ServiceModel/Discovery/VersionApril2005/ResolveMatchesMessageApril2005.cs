//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ResolveMatchesMessageApril2005
    {
        private ResolveMatchesMessageApril2005()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public DiscoveryMessageSequenceApril2005 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ResolveMatchesElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public ResolveMatchesApril2005 ResolveMatches
        {
            get;
            private set;
        }

        public static ResolveMatchesMessageApril2005 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ResolveMatchesMessageApril2005()
                {
                    MessageSequence = DiscoveryMessageSequenceApril2005.FromDiscoveryMessageSequence(messageSequence),
                    ResolveMatches = ResolveMatchesApril2005.Create(endpointDiscoveryMetadata)
                };
        }
    }
}
