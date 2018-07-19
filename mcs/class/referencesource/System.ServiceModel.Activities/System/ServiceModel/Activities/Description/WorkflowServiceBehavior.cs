//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Activities;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class WorkflowServiceBehavior : IServiceBehavior
    {
        public WorkflowServiceBehavior(WorkflowDefinitionProvider workflowDefinitionProvider)
        {
            this.WorkflowDefinitionProvider = workflowDefinitionProvider;
        }

        public WorkflowDefinitionProvider WorkflowDefinitionProvider
        {
            get;
            private set;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDescription");
            }

            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }

            DurableInstanceContextProvider instanceContextProvider = new DurableInstanceContextProvider(serviceHostBase);
            DurableInstanceProvider instanceProvider = new DurableInstanceProvider(serviceHostBase);

            ServiceDebugBehavior serviceDebugBehavior = serviceDescription.Behaviors.Find<ServiceDebugBehavior>();

            bool includeExceptionDetailInFaults = serviceDebugBehavior != null ?
                serviceDebugBehavior.IncludeExceptionDetailInFaults
                : false;

            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endPointDispatcher in channelDispatcher.Endpoints)
                    {
                        if (IsWorkflowEndpoint(endPointDispatcher))
                        {
                            DispatchRuntime dispatchRuntime = endPointDispatcher.DispatchRuntime;
                            dispatchRuntime.AutomaticInputSessionShutdown = true;
                            dispatchRuntime.ConcurrencyMode = ConcurrencyMode.Multiple;

                            //
                            dispatchRuntime.InstanceContextProvider = instanceContextProvider;
                            dispatchRuntime.InstanceProvider = instanceProvider;

                            if (includeExceptionDetailInFaults)
                            {
                                dispatchRuntime.SetDebugFlagInDispatchOperations(includeExceptionDetailInFaults);
                            }
                        }
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDescription");
            }

            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }            
        }

        internal static bool IsWorkflowEndpoint(EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher.IsSystemEndpoint)
            {
                //Check whether the System Endpoint Opted in for WorkflowDispatch

                ServiceHostBase serviceHost = endpointDispatcher.ChannelDispatcher.Host;
                ServiceEndpoint serviceEndpoint = null;
                foreach (ServiceEndpoint endpointToMatch in serviceHost.Description.Endpoints)
                {
                    if (endpointToMatch.Id == endpointDispatcher.Id)
                    {
                        serviceEndpoint = endpointToMatch;
                        break;
                    }
                }

                if (serviceEndpoint != null)
                {
                    //User defined Std Endpoint with WorkflowContractBehaviorAttribute.
                    return serviceEndpoint is WorkflowHostingEndpoint || serviceEndpoint.Contract.Behaviors.Contains(typeof(WorkflowContractBehaviorAttribute));
                }
                return false; //Some Einstein scenario where EndpointDispatcher is added explicitly without associated ServiceEndpoint.
            }
            return true; //Application Endpoint
        }
    }
}
