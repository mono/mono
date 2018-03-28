//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    public interface ICommunicationObject
    {
        CommunicationState State { get; }

        event EventHandler Closed;
        event EventHandler Closing;
        event EventHandler Faulted;
        event EventHandler Opened;
        event EventHandler Opening;

        void Abort();

        void Close();
        void Close(TimeSpan timeout);
        IAsyncResult BeginClose(AsyncCallback callback, object state);
        IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        void EndClose(IAsyncResult result);

        void Open();
        void Open(TimeSpan timeout);
        IAsyncResult BeginOpen(AsyncCallback callback, object state);
        IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        void EndOpen(IAsyncResult result);
    }
}
