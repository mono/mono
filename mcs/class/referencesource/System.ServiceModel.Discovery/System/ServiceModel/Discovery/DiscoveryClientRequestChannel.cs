//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class DiscoveryClientRequestChannel<TChannel> : DiscoveryClientChannelBase<TChannel>, IRequestChannel
        where TChannel : class, IRequestChannel
    {
        public DiscoveryClientRequestChannel(
            ChannelManagerBase channelManagerBase,
            IChannelFactory<TChannel> innerChannelFactory,
            FindCriteria findCriteria,
            DiscoveryEndpointProvider discoveryEndpointProvider)
            : base(channelManagerBase, innerChannelFactory, findCriteria, discoveryEndpointProvider)
        {
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                if (this.InnerChannel == null)
                {
                    return DiscoveryClientBindingElement.DiscoveryEndpointAddress;
                }

                return this.InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                if (this.InnerChannel == null)
                {
                    return DiscoveryClientBindingElement.DiscoveryEndpointAddress.Uri;
                }

                return this.InnerChannel.Via;
            }
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginRequest(message, timeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, this.DefaultSendTimeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return this.InnerChannel.EndRequest(result);
        }        

        public Message Request(Message message, TimeSpan timeout)
        {
            return this.InnerChannel.Request(message, timeout);
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.DefaultSendTimeout);
        }        
    }
}
