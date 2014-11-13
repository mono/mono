//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    interface IChannelAcceptor<TChannel> : ICommunicationObject
        where TChannel : class, IChannel
    {
        TChannel AcceptChannel(TimeSpan timeout);
        IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        TChannel EndAcceptChannel(IAsyncResult result);

        bool WaitForChannel(TimeSpan timeout);
        IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndWaitForChannel(IAsyncResult result);
    }
}
