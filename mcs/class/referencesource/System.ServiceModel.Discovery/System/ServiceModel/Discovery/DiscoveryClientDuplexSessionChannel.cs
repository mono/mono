//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;

    class DiscoveryClientDuplexSessionChannel : DiscoveryClientDuplexChannel<IDuplexSessionChannel>, IDuplexSessionChannel
    {
        public DiscoveryClientDuplexSessionChannel(
            ChannelManagerBase channelManagerBase,
            IChannelFactory<IDuplexSessionChannel> innerChannelFactory,
            FindCriteria findCriteria,
            DiscoveryEndpointProvider discoveryEndpointProvider)
            : base(channelManagerBase, innerChannelFactory, findCriteria, discoveryEndpointProvider)
        {
        }

        public IDuplexSession Session
        {
            get 
            {
                if (this.InnerChannel == null)
                {
                    return null;    
                }

                return this.InnerChannel.Session;
            }
        }
    }
}
