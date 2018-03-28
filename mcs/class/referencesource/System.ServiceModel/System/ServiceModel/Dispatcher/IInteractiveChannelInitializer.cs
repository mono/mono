//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;

    public interface IInteractiveChannelInitializer
    {
        IAsyncResult BeginDisplayInitializationUI(IClientChannel channel, AsyncCallback callback, object state);
        void EndDisplayInitializationUI(IAsyncResult result);
    }
}
