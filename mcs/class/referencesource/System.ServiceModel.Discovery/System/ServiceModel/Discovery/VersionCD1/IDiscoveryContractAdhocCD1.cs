//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryAdhocContractName,
        Namespace = ProtocolStrings.VersionCD1.Namespace,
        CallbackContract = typeof(IDiscoveryResponseContractCD1))]
    interface IDiscoveryContractAdhocCD1
    {
        [OperationContract(Action = ProtocolStrings.VersionCD1.ProbeAction, IsOneWay = true)]
        void ProbeOperation(ProbeMessageCD1 request);

        [OperationContract(Action = ProtocolStrings.VersionCD1.ProbeAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginProbeOperation(ProbeMessageCD1 request, AsyncCallback callback, object state);

        void EndProbeOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.VersionCD1.ResolveAction, IsOneWay = true)]
        void ResolveOperation(ResolveMessageCD1 request);

        [OperationContract(Action = ProtocolStrings.VersionCD1.ResolveAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginResolveOperation(ResolveMessageCD1 request, AsyncCallback callback, object state);

        void EndResolveOperation(IAsyncResult result);               
    }
}
