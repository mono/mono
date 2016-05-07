//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Runtime;

    class DiscoveryEndpointValidator : IEndpointBehavior
    {

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpoint");
            }
            if (endpointDispatcher == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDispatcher");
            }
            if (endpoint.IsSystemEndpoint &&
                endpointDispatcher.ChannelDispatcher.Host.Description.Behaviors.Find<ServiceDiscoveryBehavior>() == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DiscoveryEndpointWithoutBehavior(endpoint.Name)));
            }
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
