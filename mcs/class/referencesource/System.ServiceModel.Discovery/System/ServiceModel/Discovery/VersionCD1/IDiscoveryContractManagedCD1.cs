//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryManagedContractName,
        Namespace = ProtocolStrings.VersionCD1.Namespace)]
    interface IDiscoveryContractManagedCD1
    {
        [OperationContract(Action = ProtocolStrings.VersionCD1.ProbeAction, ReplyAction = ProtocolStrings.VersionCD1.ProbeMatchesAction)]
        ProbeMatchesMessageCD1 ProbeOperation(ProbeMessageCD1 request);

        [OperationContract(Action = ProtocolStrings.VersionCD1.ProbeAction, ReplyAction = ProtocolStrings.VersionCD1.ProbeMatchesAction, AsyncPattern = true)]
        IAsyncResult BeginProbeOperation(ProbeMessageCD1 request, AsyncCallback callback, object state);

        ProbeMatchesMessageCD1 EndProbeOperation(IAsyncResult result);

        [OperationContract(Action = ProtocolStrings.VersionCD1.ResolveAction, ReplyAction = ProtocolStrings.VersionCD1.ResolveMatchesAction)]
        ResolveMatchesMessageCD1 ResolveOperation(ResolveMessageCD1 request);

        [OperationContract(Action = ProtocolStrings.VersionCD1.ResolveAction, ReplyAction = ProtocolStrings.VersionCD1.ResolveMatchesAction, AsyncPattern = true)]
        IAsyncResult BeginResolveOperation(ResolveMessageCD1 request, AsyncCallback callback, object state);        

        ResolveMatchesMessageCD1 EndResolveOperation(IAsyncResult result);
    }
}
