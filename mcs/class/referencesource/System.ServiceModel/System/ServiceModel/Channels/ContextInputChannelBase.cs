//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    abstract class ContextInputChannelBase<TChannel> : LayeredChannel<TChannel> where TChannel : class, IInputChannel
    {
        ContextExchangeMechanism contextExchangeMechanism;
        ServiceContextProtocol contextProtocol;

        protected ContextInputChannelBase(ChannelManagerBase channelManager, TChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism)
            : base(channelManager, innerChannel)
        {
            this.contextExchangeMechanism = contextExchangeMechanism;
            this.contextProtocol = new ServiceContextProtocol(contextExchangeMechanism);
        }

        public EndpointAddress LocalAddress
        {
            get { return this.InnerChannel.LocalAddress; }
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
            ProcessContextHeader(message);
            return message;
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (this.InnerChannel.EndTryReceive(result, out message))
            {
                ProcessContextHeader(message);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.InnerChannel.EndWaitForMessage(result);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = this.InnerChannel.Receive(timeout);
            ProcessContextHeader(message);
            return message;
        }

        public Message Receive()
        {
            Message message = this.InnerChannel.Receive();
            ProcessContextHeader(message);
            return message;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (this.InnerChannel.TryReceive(timeout, out message))
            {
                ProcessContextHeader(message);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.InnerChannel.WaitForMessage(timeout);
        }

        void ProcessContextHeader(Message message)
        {
            if (message != null)
            {
                contextProtocol.OnIncomingMessage(message);
            }
        }
    }
}
