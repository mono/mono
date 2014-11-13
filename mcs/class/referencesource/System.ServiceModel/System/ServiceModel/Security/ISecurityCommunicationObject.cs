//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    interface ISecurityCommunicationObject
    {
        TimeSpan DefaultOpenTimeout { get; }
        TimeSpan DefaultCloseTimeout { get; }
        void OnAbort();
        IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        void OnClose(TimeSpan timeout);
        void OnClosed();
        void OnClosing();
        void OnEndClose(IAsyncResult result);
        void OnEndOpen(IAsyncResult result);
        void OnFaulted();
        void OnOpen(TimeSpan timeout);
        void OnOpened();
        void OnOpening();
    }
}
