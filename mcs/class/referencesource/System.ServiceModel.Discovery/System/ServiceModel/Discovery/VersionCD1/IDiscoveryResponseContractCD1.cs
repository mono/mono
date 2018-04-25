//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryAdhocResposeContractName,
        Namespace = ProtocolStrings.VersionCD1.Namespace)]
    interface IDiscoveryResponseContractCD1
    {
        [OperationContract(Action = ProtocolStrings.VersionCD1.ProbeMatchesAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginProbeMatchOperation(ProbeMatchesMessageCD1 response, AsyncCallback callback, object state);

        void EndProbeMatchOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.VersionCD1.ResolveMatchesAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginResolveMatchOperation(ResolveMatchesMessageCD1 response, AsyncCallback callback, object state);

        void EndResolveMatchOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.VersionCD1.HelloAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginHelloOperation(HelloMessageCD1 message, AsyncCallback callback, object state);

        void EndHelloOperation(IAsyncResult result);
    }
}
