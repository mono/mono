//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    //Marker Attribute for StandardEndpoint contract to opt-in for Durable setup.
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class WorkflowContractBehaviorAttribute : Attribute, IContractBehavior
    {

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {

        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            //Only when DurableServiceBehavior is present ensure this endpoint operates in wrapped mode.
            if (dispatchRuntime.ChannelDispatcher.Host.Description.Behaviors.Contains(typeof(WorkflowServiceBehavior)))
            {
                foreach (OperationDescription operation in contractDescription.Operations)
                {
                    if (operation.Behaviors.Find<ControlOperationBehavior>() == null)
                    {
                        operation.Behaviors.Add(new ControlOperationBehavior(true));
                    }
                }
            }
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {

        }     
    }
}
