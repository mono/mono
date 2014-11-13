//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class RequestChannelBinder : IChannelBinder
    {
        IRequestChannel channel;        

        internal RequestChannelBinder(IRequestChannel channel)
        {
            if (channel == null)
            {
                Fx.Assert("RequestChannelBinder.RequestChannelBinder: (channel != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }
            this.channel = channel;            
        }

        public IChannel Channel
        {
            get { return this.channel; }
        }

        public bool HasSession
        {
            get { return this.channel is ISessionChannel<IOutputSession>; }
        }

        public Uri ListenUri
        {
            get { return null; }
        }

        public EndpointAddress LocalAddress
        {
            get { return EndpointAddress.AnonymousAddress; }
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.channel.RemoteAddress; }
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
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public RequestContext CreateRequestContext(Message message)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginRequest(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            ValidateNullReply(this.channel.EndRequest(result));
        }

        public void Send(Message message, TimeSpan timeout)
        {
            ValidateNullReply(this.channel.Request(message, timeout));
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginRequest(message, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return this.channel.EndRequest(result);
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            return this.channel.Request(message, timeout);
        }

        void ValidateNullReply(Message message)
        {
            if (message != null && !(message is NullMessage))
            {
                ProtocolException error = ProtocolException.OneWayOperationReturnedNonNull(message);
                throw System.ServiceModel.Diagnostics.TraceUtility.ThrowHelperError(error, message);
            }
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
