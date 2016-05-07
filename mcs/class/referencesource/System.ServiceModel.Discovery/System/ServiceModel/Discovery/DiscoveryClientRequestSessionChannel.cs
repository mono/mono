//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;

    class DiscoveryClientRequestSessionChannel : DiscoveryClientRequestChannel<IRequestSessionChannel>, IRequestSessionChannel
    {
        public DiscoveryClientRequestSessionChannel(
            ChannelManagerBase channelManagerBase,
            IChannelFactory<IRequestSessionChannel> innerChannelFactory,
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
