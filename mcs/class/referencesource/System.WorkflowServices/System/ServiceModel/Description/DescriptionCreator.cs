//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    class DescriptionCreator
    {
        WorkflowDefinitionContext workflowDefinitionContext;

        public DescriptionCreator(WorkflowDefinitionContext workflowDefinitionContext)
        {
            if (workflowDefinitionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowDefinitionContext");
            }
            this.workflowDefinitionContext = workflowDefinitionContext;
        }

        public ServiceDescription BuildServiceDescription(out IDictionary<string, ContractDescription> implementedContracts, out IList<Type> reflectedContracts)
        {
            ServiceDescriptionContext context = new ServiceDescriptionContext();

            ServiceDescription description = new ServiceDescription();
            ApplyBehaviors(description);

            context.ServiceDescription = description;

            Walker walker = new Walker(true);
            walker.FoundActivity += delegate(Walker w, WalkerEventArgs args)
            {
                IServiceDescriptionBuilder activity = args.CurrentActivity as IServiceDescriptionBuilder;
                if (activity == null)
                {
                    return;
                }

                activity.BuildServiceDescription(context);
            };

            walker.Walk(this.workflowDefinitionContext.GetWorkflowDefinition());

            if (context.Contracts == null || context.Contracts.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.NoContract)));
            }

            implementedContracts = context.Contracts;
            reflectedContracts = context.ReflectedContracts;
            return description;
        }

        void ApplyBehaviors(ServiceDescription serviceDescription)
        {
            WorkflowServiceBehavior wsb = new WorkflowServiceBehavior(workflowDefinitionContext);
            serviceDescription.Behaviors.Add(wsb);

            if (wsb.Name != null)
            {
                serviceDescription.Name = wsb.Name;
            }
            if (wsb.Namespace != null)
            {
                serviceDescription.Namespace = wsb.Namespace;
            }
            if (wsb.ConfigurationName != null)
            {
                serviceDescription.ConfigurationName = wsb.ConfigurationName;
            }
        }
    }
}
