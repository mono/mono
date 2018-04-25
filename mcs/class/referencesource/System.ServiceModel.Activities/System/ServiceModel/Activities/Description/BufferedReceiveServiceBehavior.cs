//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Description
{
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using SR = System.ServiceModel.Activities.SR;

    public sealed class BufferedReceiveServiceBehavior : IServiceBehavior
    {
        internal const int DefaultMaxPendingMessagesPerChannel = 512;

        int maxPendingMessagesPerChannel = DefaultMaxPendingMessagesPerChannel;

        public int MaxPendingMessagesPerChannel
        {
            get
            {
                return this.maxPendingMessagesPerChannel;
            }
            set
            {
                if (value <= 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.MaxPendingMessagesPerChannelMustBeGreaterThanZero);
                }
                this.maxPendingMessagesPerChannel = value;
            }
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase is WorkflowServiceHost)
            {
                foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
                {
                    ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                    if (channelDispatcher != null)
                    {
                        foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                        {
                            if (WorkflowServiceBehavior.IsWorkflowEndpoint(endpointDispatcher))
                            {
                                // We need all incoming messages to be copyable
                                endpointDispatcher.DispatchRuntime.PreserveMessage = true;

                                // Enable BufferedReceive processing for each operation
                                foreach (DispatchOperation dispatchOperation in endpointDispatcher.DispatchRuntime.Operations)
                                {
                                    dispatchOperation.BufferedReceiveEnabled = true;
                                }
                            }
                        }
                    }
                }

                serviceHostBase.Extensions.Add(new BufferedReceiveManager(this.MaxPendingMessagesPerChannel));
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Validate that ReceiveContext.ManualControl is set for each operation
            foreach (ServiceEndpoint serviceEndpoint in serviceDescription.Endpoints)
            {
                if (BufferedReceiveServiceBehavior.IsWorkflowEndpoint(serviceEndpoint))
                {
                    foreach (OperationDescription operation in serviceEndpoint.Contract.Operations)
                    {
                        ReceiveContextEnabledAttribute receiveContextEnabled = operation.Behaviors.Find<ReceiveContextEnabledAttribute>();
                        if (receiveContextEnabled == null || !receiveContextEnabled.ManualControl)
                        {
                            throw FxTrace.Exception.AsError(
                                new InvalidOperationException(SR.BufferedReceiveRequiresReceiveContext(operation.Name)));
                        }
                    }
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        // See WorkflowServiceBehavior.cs for another implementation of IsWorkflowEndpoint which
        // operates against EndpointDispatchers instead of ServiceEndpoints
        internal static bool IsWorkflowEndpoint(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint.IsSystemEndpoint)
            {
                return false;
            }

            foreach (OperationDescription operation in serviceEndpoint.Contract.Operations)
            {
                if (operation.Behaviors.Find<WorkflowOperationBehavior>() == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
