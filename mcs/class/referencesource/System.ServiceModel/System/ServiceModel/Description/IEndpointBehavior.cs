//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public interface IEndpointBehavior
    {
        void Validate(ServiceEndpoint endpoint);
        void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters);
        void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher);
        void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime);
    }
}
