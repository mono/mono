//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [ServiceContract(
        Namespace = ProtocolStrings.VersionApril2005.Namespace,
        CallbackContract = typeof(IDiscoveryResponseContractApril2005))]
    interface IDiscoveryContractApril2005
    {
        [OperationContract(Action = ProtocolStrings.VersionApril2005.ProbeAction, IsOneWay = true)]
        void ProbeOperation(ProbeMessageApril2005 request);

        [OperationContract(Action = ProtocolStrings.VersionApril2005.ProbeAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginProbeOperation(ProbeMessageApril2005 request, AsyncCallback callback, object state);

        void EndProbeOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.VersionApril2005.ResolveAction, IsOneWay = true)]
        void ResolveOperation(ResolveMessageApril2005 request);

        [OperationContract(Action = ProtocolStrings.VersionApril2005.ResolveAction, IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginResolveOperation(ResolveMessageApril2005 request, AsyncCallback callback, object state);

        void EndResolveOperation(IAsyncResult result);                
    }
}
