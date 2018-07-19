//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Namespace = RoutingUtilities.RoutingNamespace, SessionMode = SessionMode.Required, CallbackContract = typeof(IDuplexRouterCallback))]
    public interface IDuplexSessionRouter
    {
        [OperationContract(AsyncPattern = true, IsOneWay = true, Action = "*")]
        [GenericTransactionFlow(TransactionFlowOption.Allowed)]
        IAsyncResult BeginProcessMessage(Message message, AsyncCallback callback, object state);

        void EndProcessMessage(IAsyncResult result);
    }
}
