//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ByeMessageApril2005
    {
        private ByeMessageApril2005()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public DiscoveryMessageSequenceApril2005 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ByeElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public EndpointDiscoveryMetadataApril2005 Bye
        {
            get;
            private set;
        }

        internal static ByeMessageApril2005 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ByeMessageApril2005()
                {
                    MessageSequence = DiscoveryMessageSequenceApril2005.FromDiscoveryMessageSequence(messageSequence),
                    Bye = EndpointDiscoveryMetadataApril2005.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }
    }
}

