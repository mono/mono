//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class DiscoveryCallbackBehavior : IEndpointBehavior
    {
        CallbackBehaviorAttribute innerCallbackBehavior;

        public DiscoveryCallbackBehavior()
        {
            this.innerCallbackBehavior = new CallbackBehaviorAttribute();

            this.innerCallbackBehavior.ConcurrencyMode = ConcurrencyMode.Multiple;
            this.innerCallbackBehavior.UseSynchronizationContext = false;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            ((IEndpointBehavior)this.innerCallbackBehavior).AddBindingParameters(endpoint, bindingParameters);
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            ((IEndpointBehavior)this.innerCallbackBehavior).ApplyClientBehavior(endpoint, clientRuntime);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // no-op on the service side - the InnerCallbackBehavior throws on the service side.
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
            ((IEndpointBehavior)this.innerCallbackBehavior).Validate(endpoint);
        }
    }
}
