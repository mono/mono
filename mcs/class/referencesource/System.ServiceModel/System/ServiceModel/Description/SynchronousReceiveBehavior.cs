//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;

    public class SynchronousReceiveBehavior : IEndpointBehavior
    {
        public SynchronousReceiveBehavior() { }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        { 
            if (endpointDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointDispatcher");
            }

            endpointDispatcher.ChannelDispatcher.ReceiveSynchronously = true;
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }
    }
}
