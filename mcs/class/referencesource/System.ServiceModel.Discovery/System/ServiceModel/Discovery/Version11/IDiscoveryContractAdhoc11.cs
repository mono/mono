//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryAdhocContractName,
        Namespace = ProtocolStrings.Version11.Namespace,
        CallbackContract = typeof(IDiscoveryResponseContract11))]
    interface IDiscoveryContractAdhoc11
    {
        [OperationContract(Action = ProtocolStrings.Version11.ProbeAction, IsOneWay = true)]
        void ProbeOperation(ProbeMessage11 request);

        [OperationContract(Action = ProtocolStrings.Version11.ProbeAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginProbeOperation(ProbeMessage11 request, AsyncCallback callback, object state);

        void EndProbeOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.Version11.ResolveAction, IsOneWay = true)]
        void ResolveOperation(ResolveMessage11 request);

        [OperationContract(Action = ProtocolStrings.Version11.ResolveAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginResolveOperation(ResolveMessage11 request, AsyncCallback callback, object state);

        void EndResolveOperation(IAsyncResult result);               
    }
}
