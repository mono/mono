//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class ResolveResponse
    {
        EndpointDiscoveryMetadata endpointDiscoveryMetadata;
        DiscoveryMessageSequence messageSequence;

        internal ResolveResponse()
        {
        }

        public EndpointDiscoveryMetadata EndpointDiscoveryMetadata
        {
            get
            {
                return this.endpointDiscoveryMetadata;
            }
            internal set
            {
                this.endpointDiscoveryMetadata = value;
            }
        }

        public DiscoveryMessageSequence MessageSequence
        {
            get
            {
                return this.messageSequence;
            }
            internal set
            {
                this.messageSequence = value;
            }
        }
    }
}
