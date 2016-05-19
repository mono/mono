//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel.PeerResolvers;

namespace System.ServiceModel.PeerResolvers
{

    [ServiceContract(Name = "IPeerResolverContract",
                 Namespace = PeerResolverStrings.Namespace,
                 SessionMode = SessionMode.Allowed)]
    public interface IPeerResolverContract
    {
        [OperationContract(IsOneWay = false, Name = "Register", Action = PeerResolverStrings.RegisterAction, ReplyAction = PeerResolverStrings.RegisterResponseAction)]
        RegisterResponseInfo Register(RegisterInfo registerInfo);

        [OperationContract(IsOneWay = false, Name = "Update", Action = PeerResolverStrings.UpdateAction, ReplyAction = PeerResolverStrings.UpdateResponseAction)]
        RegisterResponseInfo Update(UpdateInfo updateInfo);

        [OperationContract(IsOneWay = false, Name = "Resolve", Action = PeerResolverStrings.ResolveAction, ReplyAction = PeerResolverStrings.ResolveResponseAction)]
        ResolveResponseInfo Resolve(ResolveInfo resolveInfo);

        [OperationContract(IsOneWay = false, Name = "Unregister", Action = PeerResolverStrings.UnregisterAction)]
        void Unregister(UnregisterInfo unregisterInfo);

        [OperationContract(IsOneWay = false, Name = "Refresh", Action = PeerResolverStrings.RefreshAction, ReplyAction = PeerResolverStrings.RefreshResponseAction)]
        RefreshResponseInfo Refresh(RefreshInfo refreshInfo);

        [OperationContract(IsOneWay = false, Name = "GetServiceInfo", Action = PeerResolverStrings.GetServiceSettingsAction, ReplyAction = PeerResolverStrings.GetServiceSettingsResponseAction)]
        ServiceSettingsResponseInfo GetServiceSettings();
    }

    interface IPeerResolverClient : IPeerResolverContract, IClientChannel { }
}
