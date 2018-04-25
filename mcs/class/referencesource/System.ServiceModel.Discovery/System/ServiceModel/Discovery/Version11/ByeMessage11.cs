//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ByeMessage11
    {
        private ByeMessage11()
        {
        }

        [MessageHeader(Name = ProtocolStrings.SchemaNames.AppSequenceElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public DiscoveryMessageSequence11 MessageSequence
        {
            get;
            private set;
        }

        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ByeElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public EndpointDiscoveryMetadata11 Bye
        {
            get;
            private set;
        }

        public static ByeMessage11 Create(DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            return new ByeMessage11()
                {
                    MessageSequence = DiscoveryMessageSequence11.FromDiscoveryMessageSequence(messageSequence),
                    Bye = EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata(endpointDiscoveryMetadata)
                };
        }
    }
}
