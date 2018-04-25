//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System;
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.AnnouncementContractName,
        Namespace = ProtocolStrings.Version11.Namespace)]
    interface IAnnouncementContract11
    {
        [OperationContract(IsOneWay = true, Action = ProtocolStrings.Version11.HelloAction)]
        void HelloOperation(HelloMessage11 message);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.Version11.HelloAction, AsyncPattern = true)]
        IAsyncResult BeginHelloOperation(HelloMessage11 message, AsyncCallback callback, Object state);

        void EndHelloOperation(IAsyncResult result);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.Version11.ByeAction)]
        void ByeOperation(ByeMessage11 message);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.Version11.ByeAction, AsyncPattern = true)]
        IAsyncResult BeginByeOperation(ByeMessage11 message, AsyncCallback callback, Object state);

        void EndByeOperation(IAsyncResult result);
    }
}
