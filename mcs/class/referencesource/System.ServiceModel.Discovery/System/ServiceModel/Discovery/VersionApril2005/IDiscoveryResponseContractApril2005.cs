//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryAdhocResposeContractName,
        Namespace = ProtocolStrings.VersionApril2005.Namespace)]
    interface IDiscoveryResponseContractApril2005
    {
        [OperationContract(Action = ProtocolStrings.VersionApril2005.ProbeMatchesAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginProbeMatchOperation(ProbeMatchesMessageApril2005 response, AsyncCallback callback, object state);

        void EndProbeMatchOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.VersionApril2005.ResolveMatchesAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginResolveMatchOperation(ResolveMatchesMessageApril2005 response, AsyncCallback callback, object state);

        void EndResolveMatchOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.VersionApril2005.HelloAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginHelloOperation(HelloMessageApril2005 message, AsyncCallback callback, object state);

        void EndHelloOperation(IAsyncResult result);
    }
}
