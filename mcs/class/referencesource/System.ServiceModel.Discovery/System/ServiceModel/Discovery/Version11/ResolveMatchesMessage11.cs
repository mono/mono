//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ResolveMatchesMessage11
    {
        private ResolveMatchesMessage11()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public DiscoveryMessageSequence11 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ResolveMatchesElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public ResolveMatches11 ResolveMatches
        {
            get;
            private set;
        }

        public static ResolveMatchesMessage11 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ResolveMatchesMessage11()
                {
                    MessageSequence = DiscoveryMessageSequence11.FromDiscoveryMessageSequence(messageSequence),
                    ResolveMatches = ResolveMatches11.Create(endpointDiscoveryMetadata)
                };
        }
    }
}
