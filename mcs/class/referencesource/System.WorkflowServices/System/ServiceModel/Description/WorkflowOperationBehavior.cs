//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Administration;

    class WorkflowOperationBehavior : IOperationBehavior, IWmiInstanceProvider
    {
        bool canCreateInstance = true;
        ServiceAuthorizationManager serviceAuthorizationManager;

        public bool CanCreateInstance
        {
            get
            {
                return this.canCreateInstance;
            }
            set
            {
                this.canCreateInstance = value;
            }
        }

        public ServiceAuthorizationManager ServiceAuthorizationManager
        {
            get
            {
                return this.serviceAuthorizationManager;
            }
            set
            {
                this.serviceAuthorizationManager = value;
            }
        }

        public void AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {

        }

        public void ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {

        }

        public void ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (dispatch == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");
            }
            if (dispatch.Parent == null
                || dispatch.Parent.ChannelDispatcher == null
                || dispatch.Parent.ChannelDispatcher.Host == null
                || dispatch.Parent.ChannelDispatcher.Host.Description == null
                || dispatch.Parent.ChannelDispatcher.Host.Description.Behaviors == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.DispatchOperationInInvalidState)));
            }

            WorkflowRuntimeBehavior workflowRuntimeBehavior = dispatch.Parent.ChannelDispatcher.Host.Description.Behaviors.Find<WorkflowRuntimeBehavior>();

            if (workflowRuntimeBehavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.NoWorkflowRuntimeBehavior)));
            }

            dispatch.Invoker = new WorkflowOperationInvoker(description, this, workflowRuntimeBehavior.WorkflowRuntime, dispatch.Parent);
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("CanCreateInstance", this.CanCreateInstance);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "WorkflowOperationBehavior";
        }

        public void Validate(OperationDescription description)
        {

        }
    }
}
