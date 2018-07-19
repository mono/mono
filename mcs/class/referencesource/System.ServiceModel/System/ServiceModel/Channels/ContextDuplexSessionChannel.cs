//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextDuplexSessionChannel : ContextOutputChannelBase<IDuplexSessionChannel>, IDuplexSessionChannel
    {
        ContextProtocol contextProtocol;

        public ContextDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel,
            ContextExchangeMechanism contextExchangeMechanism, Uri address, Uri callbackAddress, bool contextManagementEnabled)
            : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ClientContextProtocol(contextExchangeMechanism, address, this, callbackAddress, contextManagementEnabled);
        }

        public ContextDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel,
            ContextExchangeMechanism contextExchangeMechanism)
            : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ServiceContextProtocol(contextExchangeMechanism);
        }

        public EndpointAddress LocalAddress
        {
            get { return this.InnerChannel.LocalAddress; }
        }

        public IDuplexSession Session
        {
            get { return this.InnerChannel.Session; }
        }

        protected override ContextProtocol ContextProtocol
        {
            get { return this.contextProtocol; }
        }

        protected override bool IsClient
        {
            get { return this.ContextProtocol is ClientContextProtocol; }
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
            Message message = this.InnerChannel.EndReceive(result);
            if (message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return message;
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            bool innerResult = this.InnerChannel.EndTryReceive(result, out message);
            if (innerResult && message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return innerResult;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.InnerChannel.EndWaitForMessage(result);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = this.InnerChannel.Receive(timeout);
            if (message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return message;
        }

        public Message Receive()
        {
            Message message = this.InnerChannel.Receive();
            if (message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return message;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            bool result = this.InnerChannel.TryReceive(timeout, out message);
            if (result && message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return result;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.InnerChannel.WaitForMessage(timeout);
        }
    }
}
