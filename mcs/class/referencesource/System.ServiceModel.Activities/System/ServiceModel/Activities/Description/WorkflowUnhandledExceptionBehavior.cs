//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowUnhandledExceptionBehavior : IServiceBehavior
    {
        internal const WorkflowUnhandledExceptionAction defaultAction = WorkflowUnhandledExceptionAction.AbandonAndSuspend;

        WorkflowUnhandledExceptionAction action;

        public WorkflowUnhandledExceptionBehavior()
        {
            this.action = defaultAction;
        }

        public WorkflowUnhandledExceptionAction Action
        {
            get { return this.action; }
            set
            {
                if (!WorkflowUnhandledExceptionActionHelper.IsDefined(value))
                {
                    throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("value"));
                }
                this.action = value;
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost workflowServiceHost = serviceHostBase as WorkflowServiceHost;
            if (workflowServiceHost != null)
            {
                workflowServiceHost.UnhandledExceptionAction = this.Action;
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
    }
}
