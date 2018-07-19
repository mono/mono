//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Namespace = RoutingUtilities.RoutingNamespace, SessionMode = SessionMode.Allowed)]
    public interface ISimplexDatagramRouter
    {
        [OperationContract(AsyncPattern = true, IsOneWay = true, Action = "*")]
        IAsyncResult BeginProcessMessage(Message message, AsyncCallback callback, object state);

        void EndProcessMessage(IAsyncResult result);
    }
}
