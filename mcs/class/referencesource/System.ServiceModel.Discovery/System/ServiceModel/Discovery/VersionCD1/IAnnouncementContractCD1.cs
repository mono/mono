//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System;
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.AnnouncementContractName,
        Namespace = ProtocolStrings.VersionCD1.Namespace)]
    interface IAnnouncementContractCD1
    {
        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionCD1.HelloAction)]
        void HelloOperation(HelloMessageCD1 message);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionCD1.HelloAction, AsyncPattern = true)]
        IAsyncResult BeginHelloOperation(HelloMessageCD1 message, AsyncCallback callback, Object state);

        void EndHelloOperation(IAsyncResult result);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionCD1.ByeAction)]
        void ByeOperation(ByeMessageCD1 message);

        [OperationContract(IsOneWay = true, Action = ProtocolStrings.VersionCD1.ByeAction, AsyncPattern = true)]
        IAsyncResult BeginByeOperation(ByeMessageCD1 message, AsyncCallback callback, Object state);

        void EndByeOperation(IAsyncResult result);
    }
}
