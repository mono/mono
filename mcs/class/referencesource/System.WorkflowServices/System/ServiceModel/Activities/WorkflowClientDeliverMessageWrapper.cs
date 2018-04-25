//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.Workflow.Activities;
    using System.Workflow.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    class WorkflowClientDeliverMessageWrapper : IDeliverMessage
    {
        string baseUri;
        public WorkflowClientDeliverMessageWrapper(string baseUri)
        {
            this.baseUri = baseUri;
        }

        public object[] PrepareEventArgsArray(object sender, ExternalDataEventArgs eventArgs, out object workItem, out IPendingWork workHandler)
        {
            workItem = null;
            workHandler = null;
            return new object[] { sender, eventArgs };
        }
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods,
            Justification = "Calling into already shipped assembly; can't apply APTCA")]
        public void DeliverMessage(ExternalDataEventArgs eventArgs, IComparable queueName, object message, object workItem, IPendingWork workHandler)
        {
            if (eventArgs == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("eventArgs");
            }
            if (queueName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("queueName");
            }

            using (ExternalDataExchangeClient desClient = new ExternalDataExchangeClient(WorkflowRuntimeEndpoint.netNamedPipeContextBinding,
                new EndpointAddress(this.baseUri)))
            {
                using (OperationContextScope scope = new OperationContextScope((IContextChannel)desClient.InnerChannel))
                {
                    IContextManager contextManager = desClient.InnerChannel.GetProperty<IContextManager>();
                    Fx.Assert(contextManager != null, "IContextManager must not be null.");
                    if (contextManager != null)
                    {
                        IDictionary<string, string> context = new Dictionary<string, string>();
                        context["instanceId"] = eventArgs.InstanceId.ToString();
                        contextManager.SetContext(context);
                    }

                    desClient.RaiseEvent(eventArgs, queueName, message);
                }
            }
  
        }
    }
}
