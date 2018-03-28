//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ByeMessageCD1
    {
        private ByeMessageCD1()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public DiscoveryMessageSequenceCD1 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ByeElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public EndpointDiscoveryMetadataCD1 Bye
        {
            get;
            private set;
        }

        public static ByeMessageCD1 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ByeMessageCD1()
                {
                    MessageSequence = DiscoveryMessageSequenceCD1.FromDiscoveryMessageSequence(messageSequence),
                    Bye = EndpointDiscoveryMetadataCD1.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }
    }
}
