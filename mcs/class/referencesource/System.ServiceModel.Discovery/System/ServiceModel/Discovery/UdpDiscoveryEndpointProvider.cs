//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    class UdpDiscoveryEndpointProvider : DiscoveryEndpointProvider
    {
        public override DiscoveryEndpoint GetDiscoveryEndpoint()
        {
            return new UdpDiscoveryEndpoint();
        }
    }
}
