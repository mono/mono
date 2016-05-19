//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.ServiceModel;
    using System.Workflow.ComponentModel;

    [ProvideProperty("WorkflowServiceAttributes", typeof(Activity))]
    internal sealed class WorkflowServiceAttributesPropertyProviderExtender : IExtenderProvider
    {
        internal WorkflowServiceAttributesPropertyProviderExtender()
        {
        }

        public bool CanExtend(object extendee)
        {
            return ((extendee is Activity) && (((Activity) extendee).Parent == null));
        }

        public WorkflowServiceAttributes GetWorkflowServiceAttributes(Activity activity)
        {
            return activity.GetValue(ReceiveActivity.WorkflowServiceAttributesProperty) as WorkflowServiceAttributes;
        }

        public void SetWorkflowServiceAttributes(Activity activity, WorkflowServiceAttributes value)
        {
            activity.SetValue(ReceiveActivity.WorkflowServiceAttributesProperty, value);
        }
    }
}
