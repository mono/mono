//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.ServiceModel.Channels;
    using System.Transactions;

    interface IRoutingClient
    {
        IAsyncResult BeginOperation(Message message, Transaction transaction, AsyncCallback callback, object state);
        Message EndOperation(IAsyncResult result);
        event EventHandler Faulted;
        RoutingEndpointTrait Key { get; }
        CommunicationState State { get; }
        void Open();
    }
}
