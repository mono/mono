//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextReplyChannel : LayeredChannel<IReplyChannel>, IReplyChannel
    {
        ContextExchangeMechanism contextExchangeMechanism;

        public ContextReplyChannel(ChannelManagerBase channelManager, IReplyChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism)
            : base(channelManager, innerChannel)
        {
            this.contextExchangeMechanism = contextExchangeMechanism;
        }

        public EndpointAddress LocalAddress
        {
            get { return this.InnerChannel.LocalAddress; }
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginReceiveRequest(callback, state);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginTryReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginWaitForRequest(timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            RequestContext innerContext = this.InnerChannel.EndReceiveRequest(result);
            if (innerContext == null)
            {
                return null;
            }
            else
            {
                return this.CreateContextChannelRequestContext(innerContext);
            }
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            context = null;
            RequestContext innerContext;
            if (this.InnerChannel.EndTryReceiveRequest(result, out innerContext))
            {
                if (innerContext != null)
                {
                    context = this.CreateContextChannelRequestContext(innerContext);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return this.InnerChannel.EndWaitForRequest(result);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            RequestContext innerContext = this.InnerChannel.ReceiveRequest(timeout);
            if (innerContext == null)
            {
                return null;
            }
            else
            {
                return this.CreateContextChannelRequestContext(innerContext);
            }
        }

        public RequestContext ReceiveRequest()
        {
            RequestContext innerContext = this.InnerChannel.ReceiveRequest();
            if (innerContext == null)
            {
                return null;
            }
            else
            {
                return this.CreateContextChannelRequestContext(innerContext);
            }
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            RequestContext innerContext;
            if (this.InnerChannel.TryReceiveRequest(timeout, out innerContext))
            {
                context = this.CreateContextChannelRequestContext(innerContext);
                return true;
            }
            else
            {
                context = null;
                return false;
            }
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return this.InnerChannel.WaitForRequest(timeout);
        }

        ContextChannelRequestContext CreateContextChannelRequestContext(RequestContext innerContext)
        {
            ServiceContextProtocol contextProtocol = new ServiceContextProtocol(this.contextExchangeMechanism);
            contextProtocol.OnIncomingMessage(innerContext.RequestMessage);
            return new ContextChannelRequestContext(innerContext, contextProtocol, this.DefaultSendTimeout);
        }
    }
}
