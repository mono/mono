//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class DiscoveryClientOutputChannel<TChannel> : DiscoveryClientChannelBase<TChannel>, IOutputChannel
        where TChannel : class, IOutputChannel
    {
        public DiscoveryClientOutputChannel(
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

        public virtual IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginSend(message, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.InnerChannel.EndSend(result);
        }

        public virtual void Send(Message message, TimeSpan timeout)
        {
            this.InnerChannel.Send(message, timeout);
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }        
    }
}
