namespace System.Workflow.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityExecutionStatusChangedEventArgs : EventArgs
    {
        private ActivityExecutionStatus status = ActivityExecutionStatus.Initialized;
        private ActivityExecutionResult activityExecutionResult = ActivityExecutionResult.None;
        private string activityQualifiedName = null;
        private int stateId = -1;

        [NonSerialized]
        private IWorkflowCoreRuntime workflowCoreRuntime = null;

        internal ActivityExecutionStatusChangedEventArgs(ActivityExecutionStatus executionStatus, ActivityExecutionResult executionResult, Activity activity)
        {
            this.status = executionStatus;
            this.activityExecutionResult = executionResult;
            this.activityQualifiedName = activity.QualifiedName;
            this.stateId = activity.ContextActivity.ContextId;
        }

        public ActivityExecutionStatus ExecutionStatus
        {
            get
            {
                return this.status;
            }
        }
        public ActivityExecutionResult ExecutionResult
        {
            get
            {
                return this.activityExecutionResult;
            }
        }
        public Activity Activity
        {
            get
            {
                Activity activity = null;
                if (this.workflowCoreRuntime != null)
                {
                    Activity contextActivity = this.workflowCoreRuntime.GetContextActivityForId(this.stateId);
                    if (contextActivity != null)
                        activity = contextActivity.GetActivityByName(this.activityQualifiedName);
                }
                return activity;
            }
        }

        // 
        internal IWorkflowCoreRuntime BaseExecutor
        {
            set
            {
                this.workflowCoreRuntime = value;
            }
        }
        public override string ToString()
        {
            return "ActivityStatusChange('" + "(" + this.stateId.ToString(CultureInfo.CurrentCulture) + ")" + this.activityQualifiedName + "', " + Activity.ActivityExecutionStatusEnumToString(this.ExecutionStatus) + ", " + Activity.ActivityExecutionResultEnumToString(this.ExecutionResult) + ")";
        }
    }
}
