//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Description
{
    using System.Runtime;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class ControlOperationBehavior : IOperationBehavior
    {
        bool isWrappedMode;

        //There are two modes of operation.
        // 1) IWorkflowControlServiceOperations :: Implemented completley by the ControlOperationInvoker.
        // 2) Infrastructure endpoints(Delay/Compensation/OCS) where we wrap their invoker over ControlOperationInvoker.
        public ControlOperationBehavior(bool isWrappedMode)
        {
            this.isWrappedMode = isWrappedMode;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {

        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            ServiceHostBase serviceHost = dispatchOperation.Parent.ChannelDispatcher.Host;
            if (!(serviceHost is WorkflowServiceHost))
            {
                throw FxTrace.Exception.AsError(
                   new InvalidOperationException(SR.WorkflowBehaviorWithNonWorkflowHost(typeof(ControlOperationBehavior).Name)));
            }

            ServiceEndpoint endpoint = null;
            foreach (ServiceEndpoint endpointToMatch in serviceHost.Description.Endpoints)
            {
                if (endpointToMatch.Id == dispatchOperation.Parent.EndpointDispatcher.Id)
                {
                    endpoint = endpointToMatch;
                    break;
                }
            }

            if (this.isWrappedMode)
            {
                CorrelationKeyCalculator keyCalculator = null;

                if (endpoint != null)
                {
                    CorrelationQueryBehavior endpointQueryBehavior = endpoint.Behaviors.Find<CorrelationQueryBehavior>();

                    if (endpointQueryBehavior != null)
                    {
                        keyCalculator = endpointQueryBehavior.GetKeyCalculator();
                    }
                }

                //This will be the case for infrastructure endpoints like Compensation/Interop OCS endpoints.
                dispatchOperation.Invoker = new ControlOperationInvoker(
                    operationDescription,
                    endpoint,
                    keyCalculator,
                    dispatchOperation.Invoker,
                    serviceHost);
            }
            else
            {
                //This will be for IWorkflowInstanceManagement endpoint operation.
                dispatchOperation.Invoker = new ControlOperationInvoker(
                    operationDescription,
                    endpoint,
                    null,
                    serviceHost);
            }
        }

        public void Validate(OperationDescription operationDescription)
        {

        }
    }
}
