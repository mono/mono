//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;

    public interface IDuplexContextChannel : IContextChannel
    {
        bool AutomaticInputSessionShutdown { get; set; }
        InstanceContext CallbackInstance { get; set; }

        IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state);
        void EndCloseOutputSession(IAsyncResult result);
        void CloseOutputSession(TimeSpan timeout);
    }
}
