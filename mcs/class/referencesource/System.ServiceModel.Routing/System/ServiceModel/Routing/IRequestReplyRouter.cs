//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Namespace = RoutingUtilities.RoutingNamespace, SessionMode = SessionMode.Allowed)]
    public interface IRequestReplyRouter
    {
        [OperationContract(AsyncPattern = true, IsOneWay = false, Action = "*", ReplyAction = "*")]
        [GenericTransactionFlow(TransactionFlowOption.Allowed)]
        IAsyncResult BeginProcessRequest(Message message, AsyncCallback callback, object state);

        Message EndProcessRequest(IAsyncResult result);
    }
}
