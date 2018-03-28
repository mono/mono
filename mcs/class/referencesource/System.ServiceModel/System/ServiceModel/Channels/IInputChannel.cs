//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Collections;

    public interface IInputChannel : IChannel
    {
        EndpointAddress LocalAddress { get; }

        Message Receive();
        Message Receive(TimeSpan timeout);
        IAsyncResult BeginReceive(AsyncCallback callback, object state);
        IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state);
        Message EndReceive(IAsyncResult result);

        bool TryReceive(TimeSpan timeout, out Message message);
        IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndTryReceive(IAsyncResult result, out Message message);

        bool WaitForMessage(TimeSpan timeout);
        IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndWaitForMessage(IAsyncResult result);
    }
}
