//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryAdhocResposeContractName,
        Namespace = ProtocolStrings.Version11.Namespace)]
    interface IDiscoveryResponseContract11
    {
        [OperationContract(Action = ProtocolStrings.Version11.ProbeMatchesAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginProbeMatchOperation(ProbeMatchesMessage11 response, AsyncCallback callback, object state);

        void EndProbeMatchOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.Version11.ResolveMatchesAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginResolveMatchOperation(ResolveMatchesMessage11 response, AsyncCallback callback, object state);

        void EndResolveMatchOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.Version11.HelloAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginHelloOperation(HelloMessage11 message, AsyncCallback callback, object state);

        void EndHelloOperation(IAsyncResult result);
    }
}
