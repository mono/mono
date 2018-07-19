//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class HelloMessageCD1
    {
        private HelloMessageCD1()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public DiscoveryMessageSequenceCD1 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.HelloElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public EndpointDiscoveryMetadataCD1 Hello
        {
            get;
            private set;
        }

        public static HelloMessageCD1 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new HelloMessageCD1()
                {
                    MessageSequence = DiscoveryMessageSequenceCD1.FromDiscoveryMessageSequence(messageSequence),
                    Hello = EndpointDiscoveryMetadataCD1.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }
    }
}
