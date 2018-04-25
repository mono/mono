//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class DiscoveryViaBehavior : IEndpointBehavior
    {
        Uri via;

        public DiscoveryViaBehavior(Uri via)
        {
            if (via == null)
            {
                throw FxTrace.Exception.ArgumentNull("via");
            }

            this.via = via;
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.via = value;
            }
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (clientRuntime == null)
            {
                throw FxTrace.Exception.ArgumentNull("clientRuntime");
            }

            clientRuntime.Via = Via;
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // no op on the service side. 
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }
}

