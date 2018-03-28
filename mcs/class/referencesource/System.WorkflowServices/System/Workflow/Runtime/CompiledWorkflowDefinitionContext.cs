//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Runtime
{
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    class CompiledWorkflowDefinitionContext : WorkflowDefinitionContext
    {
        static Type activityType = typeof(Activity);
        Activity rootActivity;

        Type workflowType;

        internal CompiledWorkflowDefinitionContext(Type workflowType)
        {
            if (workflowType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowType");
            }

            if (!activityType.IsAssignableFrom(workflowType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("workflowType", SR2.GetString(SR2.NotAnActivityType));
            }

            this.workflowType = workflowType;
        }

        public override string ConfigurationName
        {
            get
            {
                return this.workflowType.FullName;
            }
        }

        public override string WorkflowName
        {
            get
            {
                return NamingHelper.XmlName(this.workflowType.Name);
            }
        }

        public override WorkflowInstance CreateWorkflow()
        {
            return this.CreateWorkflow(Guid.NewGuid());
        }

        public override WorkflowInstance CreateWorkflow(Guid instanceId)
        {
            return base.WorkflowRuntime.CreateWorkflow(this.workflowType, null, instanceId);
        }

        public override Activity GetWorkflowDefinition()
        {
            if (rootActivity == null)
            {
                rootActivity = (Activity) Activator.CreateInstance(workflowType);
            }

            return rootActivity;
        }

        protected override void OnRegister()
        {

        }

        protected override void OnValidate(ValidationErrorCollection errors)
        {

        }
    }
}
