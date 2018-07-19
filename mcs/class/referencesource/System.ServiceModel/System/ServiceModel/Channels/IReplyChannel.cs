//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IReplyChannel : IChannel
    {
        EndpointAddress LocalAddress { get; }

        RequestContext ReceiveRequest();
        RequestContext ReceiveRequest(TimeSpan timeout);
        IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state);
        IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state);
        RequestContext EndReceiveRequest(IAsyncResult result);

        bool TryReceiveRequest(TimeSpan timeout, out RequestContext context);
        IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context);

        bool WaitForRequest(TimeSpan timeout);
        IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndWaitForRequest(IAsyncResult result);
    }
}
