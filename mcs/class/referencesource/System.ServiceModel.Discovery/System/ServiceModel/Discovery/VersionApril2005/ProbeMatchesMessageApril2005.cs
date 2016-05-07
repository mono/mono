//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;
    using System.Collections.ObjectModel;

    [MessageContract(IsWrapped = false)]
    class ProbeMatchesMessageApril2005
    {
        private ProbeMatchesMessageApril2005()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public DiscoveryMessageSequenceApril2005 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ProbeMatchesElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public ProbeMatchesApril2005 ProbeMatches
        {
            get;
            private set;
        }

        public static ProbeMatchesMessageApril2005 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ProbeMatchesMessageApril2005()
                {
                    MessageSequence = DiscoveryMessageSequenceApril2005.FromDiscoveryMessageSequence(messageSequence),
                    ProbeMatches = ProbeMatchesApril2005.Create(endpointDiscoveryMetadata)
                };
        }
    }
}
