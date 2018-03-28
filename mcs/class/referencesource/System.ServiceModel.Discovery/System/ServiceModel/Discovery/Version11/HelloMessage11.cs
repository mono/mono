//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class HelloMessage11
    {
        private HelloMessage11()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public DiscoveryMessageSequence11 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.HelloElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public EndpointDiscoveryMetadata11 Hello
        {
            get;
            private set;
        }

        public static HelloMessage11 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new HelloMessage11()
                {
                    MessageSequence = DiscoveryMessageSequence11.FromDiscoveryMessageSequence(messageSequence),
                    Hello = EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }
    }
}
