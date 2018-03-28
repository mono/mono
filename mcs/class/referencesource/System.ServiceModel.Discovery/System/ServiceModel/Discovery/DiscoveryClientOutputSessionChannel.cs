//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;

    class DiscoveryClientOutputSessionChannel : DiscoveryClientOutputChannel<IOutputSessionChannel>, IOutputSessionChannel
    {
        public DiscoveryClientOutputSessionChannel(
            ChannelManagerBase channelManagerBase,
            IChannelFactory<IOutputSessionChannel> innerChannelFactory,
            FindCriteria findCriteria,
            DiscoveryEndpointProvider discoveryEndpointProvider)
            : base(channelManagerBase, innerChannelFactory, findCriteria, discoveryEndpointProvider)
        {
        }

        public IOutputSession Session
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
