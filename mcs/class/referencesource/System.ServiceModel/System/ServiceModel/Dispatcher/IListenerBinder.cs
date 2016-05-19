//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    interface IListenerBinder
    {
        IChannelListener Listener { get; }
        MessageVersion MessageVersion { get; }

        IChannelBinder Accept(TimeSpan timeout);
        IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state);
        IChannelBinder EndAccept(IAsyncResult result);
    }
}
