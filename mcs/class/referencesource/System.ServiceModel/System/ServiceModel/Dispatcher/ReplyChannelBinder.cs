//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    class ReplyChannelBinder : IChannelBinder
    {
        IReplyChannel channel;
        Uri listenUri;

        internal ReplyChannelBinder(IReplyChannel channel, Uri listenUri)
        {
            if (channel == null)
            {
                Fx.Assert("ReplyChannelBinder.ReplyChannelBinder: (channel != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }
            this.channel = channel;
            this.listenUri = listenUri;
        }

        public IChannel Channel
        {
            get { return this.channel; }
        }

        public bool HasSession
        {
            get { return this.channel is ISessionChannel<IInputSession>; }
        }

        public Uri ListenUri
        {
            get { return this.listenUri; }
        }

        public EndpointAddress LocalAddress
        {
            get { return this.channel.LocalAddress; }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
#pragma warning suppress 56503 // [....], the property is really not implemented, cannot lie, API not public
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException()); 
            }
        }

        public void Abort()
        {
            this.channel.Abort();
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
            this.channel.Close(timeout);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginTryReceiveRequest(timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            return this.channel.EndTryReceiveRequest(result, out requestContext);
        }

        public RequestContext CreateRequestContext(Message message)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public void EndSend(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void Send(Message message, TimeSpan timeout)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public Message EndRequest(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            return this.channel.TryReceiveRequest(timeout, out requestContext);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }
    }
}
