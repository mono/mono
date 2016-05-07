// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Validation
{
    using System.Activities.Validation;
    using System.Runtime;
    using System.ServiceModel.Activities;

    internal sealed class ValidationRoot
    {
        private WorkflowService workflowService;
        private Activity activity;

        public ValidationRoot(WorkflowService workflowService)
        {
            Fx.Assert(workflowService != null, "workflowService != null");
            this.workflowService = workflowService;
        }

        public ValidationRoot(Activity activity)
        {
            Fx.Assert(activity != null, "activity!=null");
            this.activity = activity;
        }

        public ValidationResults Validate(ValidationSettings settings)
        {
            if (this.workflowService != null)
            {
                return this.workflowService.Validate(settings);
            }
            else
            {
                return ActivityValidationServices.Validate(this.activity, settings);
            }
        }

        public Activity Resolve(string id)
        {
            Fx.Assert(id != null, "id should not be null.");

            Activity activityRoot = null;
            if (this.workflowService != null)
            {
                activityRoot = this.workflowService.GetWorkflowRoot();
            }
            else
            {
                activityRoot = this.activity;
            }

            return ActivityValidationServices.Resolve(activityRoot, id);
        }
    }
}
