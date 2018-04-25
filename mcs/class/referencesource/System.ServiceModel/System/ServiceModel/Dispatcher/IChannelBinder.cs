//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    interface IChannelBinder
    {
        IChannel Channel { get; }
        bool HasSession { get; }
        Uri ListenUri { get; }
        EndpointAddress LocalAddress { get; }
        EndpointAddress RemoteAddress { get; }

        void Abort();
        void CloseAfterFault(TimeSpan timeout);

        bool TryReceive(TimeSpan timeout, out RequestContext requestContext);
        IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndTryReceive(IAsyncResult result, out RequestContext requestContext);

        void Send(Message message, TimeSpan timeout);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        void EndSend(IAsyncResult result);

        Message Request(Message message, TimeSpan timeout);
        IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        Message EndRequest(IAsyncResult result);

        bool WaitForMessage(TimeSpan timeout);
        IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndWaitForMessage(IAsyncResult result);

        RequestContext CreateRequestContext(Message message);
    }
}
