//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IRequestChannel : IChannel
    {
        EndpointAddress RemoteAddress { get; }
        Uri Via { get; }

        Message Request(Message message);
        Message Request(Message message, TimeSpan timeout);
        IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state);
        IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        Message EndRequest(IAsyncResult result);
    }
}
