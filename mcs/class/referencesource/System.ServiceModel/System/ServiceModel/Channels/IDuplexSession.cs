//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;

    public interface IDuplexSession : IInputSession, IOutputSession
    {
        void CloseOutputSession();
        void CloseOutputSession(TimeSpan timeout);
        IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state);
        IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state);
        void EndCloseOutputSession(IAsyncResult result);
    }
}
