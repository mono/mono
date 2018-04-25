//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryManagedContractName,
        Namespace = ProtocolStrings.Version11.Namespace)]
    interface IDiscoveryContractManaged11
    {
        [OperationContract(Action = ProtocolStrings.Version11.ProbeAction, ReplyAction = ProtocolStrings.Version11.ProbeMatchesAction)]
        ProbeMatchesMessage11 ProbeOperation(ProbeMessage11 request);

        [OperationContract(Action = ProtocolStrings.Version11.ProbeAction, ReplyAction = ProtocolStrings.Version11.ProbeMatchesAction, AsyncPattern = true)]
        IAsyncResult BeginProbeOperation(ProbeMessage11 request, AsyncCallback callback, object state);

        ProbeMatchesMessage11 EndProbeOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.Version11.ResolveAction, ReplyAction = ProtocolStrings.Version11.ResolveMatchesAction)]
        ResolveMatchesMessage11 ResolveOperation(ResolveMessage11 request);

        [OperationContract(Action = ProtocolStrings.Version11.ResolveAction, ReplyAction = ProtocolStrings.Version11.ResolveMatchesAction, AsyncPattern = true)]
        IAsyncResult BeginResolveOperation(ResolveMessage11 request, AsyncCallback callback, object state);        

        ResolveMatchesMessage11 EndResolveOperation(IAsyncResult result);
    }
}
