namespace System.Workflow.ComponentModel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Collections.Generic;

    [Serializable]
    internal abstract class SchedulableItem
    {
        private int contextId = -1;
        string activityId = null;
        protected SchedulableItem(int contextId, string activityId)
        {
            this.contextId = contextId;
            this.activityId = activityId;
        }

        public int ContextId
        {
            get
            {
                return this.contextId;
            }
        }

        public string ActivityId
        {
            get
            {
                return this.activityId;
            }
        }

        public abstract bool Run(IWorkflowCoreRuntime workflowCoreRuntime);
    }

    internal enum ActivityOperationType : byte
    {
        Execute = 0,
        Cancel = 1,
        Compensate = 2,
        HandleFault = 3
    }

    [Serializable]
    internal sealed class ActivityExecutorOperation : SchedulableItem
    {
        private string activityName;
        private ActivityOperationType operation;
        private Exception exceptionToDeliver;

        public ActivityExecutorOperation(Activity activity, ActivityOperationType opt, int contextId)
            : base(contextId, activity.QualifiedName)
        {
            this.activityName = activity.QualifiedName;
            this.operation = opt;
        }
        public ActivityExecutorOperation(Activity activity, ActivityOperationType opt, int contextId, Exception e)
            : this(activity, opt, contextId)
        {
            this.exceptionToDeliver = e;
        }
        public override bool Run(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            // get state reader
            Activity contextActivity = workflowCoreRuntime.GetContextActivityForId(this.ContextId);
            Activity activity = contextActivity.GetActivityByName(this.activityName);

            using (workflowCoreRuntime.SetCurrentActivity(activity))
            {
                using (ActivityExecutionContext activityExecutionContext = new ActivityExecutionContext(activity))
                {
                    ActivityExecutor activityExecutor = ActivityExecutors.GetActivityExecutor(activity);
                    switch (this.operation)
                    {
                        case ActivityOperationType.Execute:
                            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                            {
                                try
                                {
                                    workflowCoreRuntime.RaiseActivityExecuting(activity);

                                    ActivityExecutionStatus newStatus = activityExecutor.Execute(activity, activityExecutionContext);
                                    if (newStatus == ActivityExecutionStatus.Closed)
                                        activityExecutionContext.CloseActivity();
                                    else if (newStatus != ActivityExecutionStatus.Executing)
                                        throw new InvalidOperationException(SR.GetString(SR.InvalidExecutionStatus, activity.QualifiedName, newStatus.ToString(), ActivityExecutionStatus.Executing.ToString()));
                                }
                                catch (Exception e)
                                {
                                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Execute of Activity {0} threw {1}", activity.QualifiedName, e.ToString());
                                    throw;
                                }
                            }
                            break;
                        case ActivityOperationType.Cancel:
                            if (activity.ExecutionStatus == ActivityExecutionStatus.Canceling)
                            {
                                try
                                {
                                    ActivityExecutionStatus newStatus = activityExecutor.Cancel(activity, activityExecutionContext);
                                    if (newStatus == ActivityExecutionStatus.Closed)
                                        activityExecutionContext.CloseActivity();
                                    else if (newStatus != ActivityExecutionStatus.Canceling)
                                        throw new InvalidOperationException(SR.GetString(SR.InvalidExecutionStatus, activity.QualifiedName, newStatus.ToString(), ActivityExecutionStatus.Canceling.ToString()));

                                }
                                catch (Exception e)
                                {
                                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Cancel of Activity {0} threw {1}", activity.QualifiedName, e.ToString());
                                    throw;
                                }
                            }
                            break;
                        case ActivityOperationType.Compensate:
                            if (activity.ExecutionStatus == ActivityExecutionStatus.Compensating)
                            {
                                try
                                {
                                    ActivityExecutionStatus newStatus = activityExecutor.Compensate(activity, activityExecutionContext);
                                    if (newStatus == ActivityExecutionStatus.Closed)
                                        activityExecutionContext.CloseActivity();
                                    else if (newStatus != ActivityExecutionStatus.Compensating)
                                        throw new InvalidOperationException(SR.GetString(SR.InvalidExecutionStatus, activity.QualifiedName, newStatus.ToString(), ActivityExecutionStatus.Compensating.ToString()));
                                }
                                catch (Exception e)
                                {
                                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Compensate of Activity {0} threw {1}", activity.QualifiedName, e.ToString());
                                    throw;
                                }
                            }
                            break;
                        case ActivityOperationType.HandleFault:
                            if (activity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                            {
                                try
                                {
                                    ActivityExecutionStatus newStatus = activityExecutor.HandleFault(activity, activityExecutionContext, this.exceptionToDeliver);
                                    if (newStatus == ActivityExecutionStatus.Closed)
                                        activityExecutionContext.CloseActivity();
                                    else if (newStatus != ActivityExecutionStatus.Faulting)
                                        throw new InvalidOperationException(SR.GetString(SR.InvalidExecutionStatus, activity.QualifiedName, newStatus.ToString(), ActivityExecutionStatus.Faulting.ToString()));
                                }
                                catch (Exception e)
                                {
                                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Compensate of Activity {0} threw {1}", activity.QualifiedName, e.ToString());
                                    throw;
                                }
                            }
                            break;
                    }
                }
            }
            return true;
        }
        public override string ToString()
        {
            return "ActivityOperation(" + "(" + this.ContextId.ToString(CultureInfo.CurrentCulture) + ")" + this.activityName + ", " + ActivityOperationToString(this.operation) + ")";
        }
        private string ActivityOperationToString(ActivityOperationType operationType)
        {
            string retVal = string.Empty;
            switch (operationType)
            {
                case ActivityOperationType.Execute:
                    retVal = "Execute";
                    break;
                case ActivityOperationType.Cancel:
                    retVal = "Cancel";
                    break;
                case ActivityOperationType.HandleFault:
                    retVal = "HandleFault";
                    break;
                case ActivityOperationType.Compensate:
                    retVal = "Compensate";
                    break;
            }
            return retVal;
        }
    }
}
