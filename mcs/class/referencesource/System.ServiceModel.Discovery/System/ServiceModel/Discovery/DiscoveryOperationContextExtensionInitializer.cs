//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class DiscoveryOperationContextExtensionInitializer : IEndpointBehavior, IDispatchMessageInspector
    {
        DiscoveryOperationContextExtension discoveryOperationContextExtension;

        public DiscoveryOperationContextExtensionInitializer(DiscoveryOperationContextExtension discoveryOperationContextExtension)
        {
            this.discoveryOperationContextExtension = discoveryOperationContextExtension;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDispatcher");
            }


            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }

        object IDispatchMessageInspector.AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            OperationContext.Current.Extensions.Add(this.discoveryOperationContextExtension);
            return null;
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}
