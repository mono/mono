//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IConnectionDuplicator))]
    interface IConnectionRegister
    {
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        ListenerExceptionStatus Register(Version version, int pid, BaseUriWithWildcard path, int queueId, Guid token, string eventName);

        [OperationContract]
        bool ValidateUriRoute(Uri uri, IPAddress address, int port);

        [OperationContract]
        void Unregister();
    }

    //Used on the client side (e.g. inside WebHost) to add async support to validate the Uri without blocking IO threads to improve scalability.
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IConnectionDuplicator))]
    interface IConnectionRegisterAsync : IConnectionRegister
    {
        [OperationContract(AsyncPattern = true, Action = "http://tempuri.org/IConnectionRegister/ValidateUriRoute", ReplyAction = "http://tempuri.org/IConnectionRegister/ValidateUriRouteResponse")]
        IAsyncResult BeginValidateUriRoute(System.Uri uri, IPAddress address, int port, AsyncCallback callback, object asyncState);
        bool EndValidateUriRoute(System.IAsyncResult result);
    }
}
