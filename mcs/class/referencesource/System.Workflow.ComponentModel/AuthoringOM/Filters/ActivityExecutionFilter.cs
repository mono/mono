namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal abstract class ActivityExecutionFilter : ActivityExecutor, ISupportWorkflowChanges
    {
        #region ISupportWorkflowChanges Members

        public virtual void OnActivityAdded(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");

            NextDynamicChangeExecutorInChain(executionContext.Activity).OnActivityAdded(executionContext, addedActivity);
        }
        public virtual void OnActivityRemoved(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (removedActivity == null)
                throw new ArgumentNullException("removedActivity");

            NextDynamicChangeExecutorInChain(executionContext.Activity).OnActivityRemoved(executionContext, removedActivity);
        }
        public virtual void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            NextDynamicChangeExecutorInChain(executionContext.Activity).OnWorkflowChangesCompleted(executionContext);
        }
        #endregion ISupportWorkflowChanges

        #region Execute, Cancel, Compensate and HandleFault

        public override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return NextActivityExecutorInChain(executionContext.Activity).Execute(activity, executionContext);
        }

        public override ActivityExecutionStatus Cancel(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            // If primary activity is Closed, then return, these filters might have acquired locks 
            // on to the primary activity, and in that case even if the activity has closed itself
            // it might get Cancel signal. So we don't want activity to get Cancel signal, when it 
            // already has declared itself Closed.
            ActivityExecutor nextActivityExecutor = NextActivityExecutorInChain(executionContext.Activity);
            if (!(nextActivityExecutor is ActivityExecutionFilter) && executionContext.Activity.HasPrimaryClosed)
                return ActivityExecutionStatus.Closed;

            return nextActivityExecutor.Cancel(activity, executionContext);
        }
        public override ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (exception == null)
                throw new ArgumentNullException("exception");

            // If primary activity is Closed, then return, these filters might have acquired locks 
            // on to the primary activity, and in that case even if the activity has closed itself
            // it might get HandleFault signal. So we don't want activity to get HandleFault signal, when it 
            // already has declared itself Closed.
            ActivityExecutor nextActivityExecutor = NextActivityExecutorInChain(executionContext.Activity);
            if (!(nextActivityExecutor is ActivityExecutionFilter) && executionContext.Activity.HasPrimaryClosed)
                return ActivityExecutionStatus.Closed;

            return nextActivityExecutor.HandleFault(activity, executionContext, exception);
        }
        public override ActivityExecutionStatus Compensate(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return NextActivityExecutorInChain(executionContext.Activity).Compensate(activity, executionContext);
        }

        #endregion

        #region Helper Methods

        protected ActivityExecutor NextActivityExecutorInChain(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            ActivityExecutor nextActivityExecutor = null;
            IList activityExecutors = ActivityExecutors.GetActivityExecutors(activity);
            int thisIndex = activityExecutors.IndexOf(this);
            if (thisIndex < activityExecutors.Count - 1)
                nextActivityExecutor = (ActivityExecutor)activityExecutors[thisIndex + 1];
            return nextActivityExecutor;
        }
        protected ISupportWorkflowChanges NextDynamicChangeExecutorInChain(Activity activity)
        {
            return NextActivityExecutorInChain(activity) as ISupportWorkflowChanges;
        }
        #endregion
    }
}
