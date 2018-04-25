//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class HelloMessageApril2005
    {
        private HelloMessageApril2005()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public DiscoveryMessageSequenceApril2005 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.HelloElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public EndpointDiscoveryMetadataApril2005 Hello
        {
            get;
            private set;
        }

        public static HelloMessageApril2005 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new HelloMessageApril2005()
                {
                    MessageSequence = DiscoveryMessageSequenceApril2005.FromDiscoveryMessageSequence(messageSequence),
                    Hello = EndpointDiscoveryMetadataApril2005.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }
    }
}

