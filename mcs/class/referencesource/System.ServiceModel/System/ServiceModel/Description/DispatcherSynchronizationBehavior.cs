//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;

    public class DispatcherSynchronizationBehavior : IEndpointBehavior
    {
        public DispatcherSynchronizationBehavior() :
            this(false, MultipleReceiveBinder.MultipleReceiveDefaults.MaxPendingReceives)
        {

        }

        public DispatcherSynchronizationBehavior(bool asynchronousSendEnabled, int maxPendingReceives)
        {
            this.AsynchronousSendEnabled = asynchronousSendEnabled;
            this.MaxPendingReceives = maxPendingReceives;
        }

        public bool AsynchronousSendEnabled
        {
            get;
            set;
        }

        public int MaxPendingReceives
        {
            get;
            set;
        }

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

            endpointDispatcher.ChannelDispatcher.SendAsynchronously = this.AsynchronousSendEnabled;
            endpointDispatcher.ChannelDispatcher.MaxPendingReceives = this.MaxPendingReceives;
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }
    }
}
