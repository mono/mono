//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class DiscoveryClientDuplexChannel<TChannel> : DiscoveryClientOutputChannel<TChannel>, IDuplexChannel
        where TChannel : class, IDuplexChannel 
    {
        public DiscoveryClientDuplexChannel(
            ChannelManagerBase channelManagerBase,
            IChannelFactory<TChannel> innerChannelFactory,
            FindCriteria findCriteria,
            DiscoveryEndpointProvider discoveryEndpointProvider)
            : base(channelManagerBase, innerChannelFactory, findCriteria, discoveryEndpointProvider)
        {
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                if (this.InnerChannel == null)
                {
                    return DiscoveryClientBindingElement.DiscoveryEndpointAddress;
                }
                return this.InnerChannel.LocalAddress;
            }
        }

        public override IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.EnsureReplyTo(message);
            return base.BeginSend(message, timeout, callback, state);
        }

        public override void Send(Message message, TimeSpan timeout)
        {
            this.EnsureReplyTo(message);
            base.Send(message, timeout);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginReceive(timeout, callback, state);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginReceive(callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return this.InnerChannel.EndReceive(result);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return this.InnerChannel.EndTryReceive(result, out message);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.InnerChannel.EndWaitForMessage(result);
        }        

        public Message Receive(TimeSpan timeout)
        {
            return this.InnerChannel.Receive(timeout);
        }

        public Message Receive()
        {
            return this.InnerChannel.Receive();
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            return this.InnerChannel.TryReceive(timeout, out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.InnerChannel.WaitForMessage(timeout);
        }

        void EnsureReplyTo(Message message)
        {
            if (message != null && message.Headers != null)
            {
                if (message.Headers.ReplyTo == DiscoveryClientBindingElement.DiscoveryEndpointAddress)
                {
                    message.Headers.ReplyTo = this.LocalAddress;
                }
            }
        }
    }
}
