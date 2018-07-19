//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System;
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.AnnouncementContractName,
        Namespace = ProtocolStrings.VersionApril2005.Namespace)]
    interface IAnnouncementContractApril2005
    {
        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionApril2005.HelloAction)]
        void HelloOperation(HelloMessageApril2005 message);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionApril2005.HelloAction, AsyncPattern = true)]
        IAsyncResult BeginHelloOperation(HelloMessageApril2005 message, AsyncCallback callback, Object state);

        void EndHelloOperation(IAsyncResult result);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionApril2005.ByeAction)]
        void ByeOperation(ByeMessageApril2005 message);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionApril2005.ByeAction, AsyncPattern = true)]
        IAsyncResult BeginByeOperation(ByeMessageApril2005 message, AsyncCallback callback, Object state);

        void EndByeOperation(IAsyncResult result);
    }
}
