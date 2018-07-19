namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;

    internal class CompensationHandlingFilter : ActivityExecutionFilter, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public static DependencyProperty CompensateProcessedProperty = DependencyProperty.RegisterAttached("CompensateProcessed", typeof(bool), typeof(CompensationHandlingFilter), new PropertyMetadata(false));
        internal static DependencyProperty LastCompensatedOrderIdProperty = DependencyProperty.RegisterAttached("LastCompensatedOrderId", typeof(int), typeof(CompensationHandlingFilter), new PropertyMetadata(false));

        #region Compensate Signal

        public override ActivityExecutionStatus Compensate(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            executionContext.Activity.HoldLockOnStatusChange(this);
            return NextActivityExecutorInChain(activity).Compensate(activity, executionContext);
        }
        #endregion

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException("sender");

            if (e.Activity == context.Activity)
            {
                if (context.Activity.HasPrimaryClosed && !(bool)context.Activity.GetValue(CompensateProcessedProperty))
                {
                    context.Activity.SetValue(CompensateProcessedProperty, true);
                    if (context.Activity.ExecutionResult == ActivityExecutionResult.Compensated)
                    {
                        // run compensation handler or do default compensation handling
                        Activity compensationHandler = GetCompensationHandler(context.Activity);
                        if (compensationHandler != null)
                        {
                            // subscribe for status change on compensation handler
                            compensationHandler.RegisterForStatusChange(Activity.ClosedEvent, this);

                            // execute compensation handler
                            context.ExecuteActivity(compensationHandler);
                        }
                        else
                        {
                            // do default compensation
                            if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                            {
                                // let activity get into closed state
                                context.Activity.ReleaseLockOnStatusChange(this);
                            }
                        }
                    }
                    else
                    {
                        // let activity get into closed state
                        context.Activity.ReleaseLockOnStatusChange(this);
                    }
                }
            }
            else if (e.Activity is CompensationHandlerActivity && e.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                // remove subscriber for status change on compensation handler
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

                // release lock on the primary activity
                context.Activity.ReleaseLockOnStatusChange(this);
            }
            else if (e.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                // remove subscriber for status change on compensated activity
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

                if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                {
                    // release lock on the primary activity
                    context.Activity.ReleaseLockOnStatusChange(this);
                }
            }
        }
        #endregion

        #region Helper Methods

        internal static Activity GetCompensationHandler(Activity activityWithCompensation)
        {
            Activity compensationHandler = null;
            CompositeActivity compositeActivity = activityWithCompensation as CompositeActivity;
            if (compositeActivity != null)
            {
                foreach (Activity activity in ((ISupportAlternateFlow)compositeActivity).AlternateFlowActivities)
                {
                    if (activity is CompensationHandlerActivity)
                    {
                        compensationHandler = activity;
                        break;
                    }
                }
            }
            return compensationHandler;
        }
        #endregion

    }
    #region CompensationUtils

    internal static class CompensationUtils
    {
        internal static bool TryCompensateLastCompletedChildActivity(ActivityExecutionContext context, Activity targetActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> statusChangeHandler)
        {
            try
            {
                return TryCompensateLastCompletedChildActivity(context, targetActivity, statusChangeHandler, true);
            }
            catch (Exception)
            {
                //If root compensation failed. then flush Execution Contexts, which we opened 
                //up now.
                if (targetActivity.Parent == null)
                    CompleteRevokedExecutionContext(targetActivity, context);
                throw;
            }
        }

        private static bool TryCompensateLastCompletedChildActivity(ActivityExecutionContext context, Activity targetActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> statusChangeHandler, bool isimmediateCompensation)
        {
            SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets = new SortedDictionary<int, CompensationInfo>();

            if (!(targetActivity is CompositeActivity))
                return false;

            //Walk through all of the direct children which are compensatable and add them in the sorted order of their completion
            //bail out if any of the compensatable children is currently compensating/faulting or canceling
            if (CollectCompensatableTargetActivities(targetActivity as CompositeActivity, sortedListOfCompensatableTargets, isimmediateCompensation))
                return true;

            // walk through active contexts that contain compensatable child, add them in the sorted order of the completion
            // this also, walks through the completed contexts which are compensatable and are nested directly within the active contexts and adds them in the order of their completion
            // bail out if any activity is currently compensating/faulting or cancelling
            if (CollectCompensatableActiveContexts(context, targetActivity, sortedListOfCompensatableTargets, isimmediateCompensation))
                return true;

            // walk through all completed execution contexts which are compensatable and are directly nested under the target activity, 
            //and add them to our sorted list
            CollectCompensatableCompletedContexts(context, targetActivity, sortedListOfCompensatableTargets, isimmediateCompensation);

            //if there were no compensatable targets found, bail out
            if (sortedListOfCompensatableTargets.Count == 0)
            {
                CompleteRevokedExecutionContext(targetActivity, context);
                return false;
            }

            int? lastCompletedOrderId = targetActivity.GetValue(CompensationHandlingFilter.LastCompensatedOrderIdProperty) as Nullable<int>;
            int nextLastCompletedOrderId = -1;
            //get the last compensatable target - this could be an activity, contextInfo or a Context
            CompensationInfo lastCompensatableTarget = null;
            foreach (int completedOrderId in sortedListOfCompensatableTargets.Keys)
            {
                if (lastCompletedOrderId.HasValue && lastCompletedOrderId < completedOrderId)
                    break;

                lastCompensatableTarget = sortedListOfCompensatableTargets[completedOrderId];
                nextLastCompletedOrderId = completedOrderId;
            }

            //We are done with compensation on entire branch, now complete execution contexts
            //recursilvely which we might have opened up.
            if (lastCompensatableTarget == null)
            {
                CompleteRevokedExecutionContext(targetActivity, context);
                return false;
            }

            targetActivity.SetValue(CompensationHandlingFilter.LastCompensatedOrderIdProperty, nextLastCompletedOrderId);

            //the last compensatable target could be an activity
            if (lastCompensatableTarget.TargetActivity != null && lastCompensatableTarget.TargetActivity is ICompensatableActivity)
            {
                lastCompensatableTarget.TargetActivity.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                context.CompensateActivity(lastCompensatableTarget.TargetActivity);
                return true;
            } //or get the last compensatable "completed" context
            else if (lastCompensatableTarget.TargetExecutionInfo != null && lastCompensatableTarget.TargetExecutionContextManager != null)
            {
                ActivityExecutionContext revokedExecutionContext = lastCompensatableTarget.TargetExecutionContextManager.DiscardPersistedExecutionContext(lastCompensatableTarget.TargetExecutionInfo);

                //get the "first" compensatable child and compensate it
                if (revokedExecutionContext.Activity is ICompensatableActivity)
                {
                    revokedExecutionContext.Activity.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                    revokedExecutionContext.CompensateActivity(revokedExecutionContext.Activity);
                    return true;
                }
                else if (revokedExecutionContext.Activity is CompositeActivity)
                {
                    //get the last compensatable child of the revoked context
                    Activity compensatableChild = GetLastCompensatableChild(revokedExecutionContext.Activity as CompositeActivity);
                    if (compensatableChild != null)
                    {
                        compensatableChild.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                        revokedExecutionContext.CompensateActivity(compensatableChild);
                        return true;
                    }
                    else// recursively, walk the context tree and keep revoking the compensatable contexts
                        return TryCompensateLastCompletedChildActivity(revokedExecutionContext, revokedExecutionContext.Activity, statusChangeHandler, false);
                }
            }
            else if (lastCompensatableTarget.TargetExecutionContext != null) //or get the last compensatable "active" context
            {
                if (lastCompensatableTarget.TargetExecutionContext.Activity is CompositeActivity)
                {
                    //get the last compensatable child of the active context
                    Activity compensatableChild = GetLastCompensatableChild(lastCompensatableTarget.TargetExecutionContext.Activity as CompositeActivity);
                    if (compensatableChild != null)
                    {
                        compensatableChild.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                        lastCompensatableTarget.TargetExecutionContext.CompensateActivity(compensatableChild);
                        return true;
                    }
                    else // recursively, walk the context tree and keep revoking the compensatable contexts
                        return TryCompensateLastCompletedChildActivity(lastCompensatableTarget.TargetExecutionContext, lastCompensatableTarget.TargetExecutionContext.Activity, statusChangeHandler, false);
                }
            }
            return false;
        }

        private static void CompleteRevokedExecutionContext(Activity targetActivity, ActivityExecutionContext context)
        {
            ActivityExecutionContext[] activeContextsClone = new ActivityExecutionContext[context.ExecutionContextManager.ExecutionContexts.Count];
            context.ExecutionContextManager.ExecutionContexts.CopyTo(activeContextsClone, 0);

            foreach (ActivityExecutionContext childContext in activeContextsClone)
            {
                if (targetActivity.GetActivityByName(childContext.Activity.QualifiedName, true) != null)
                {
                    if (childContext.Activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                        CompleteRevokedExecutionContext(childContext.Activity, childContext);

                    context.ExecutionContextManager.CompleteExecutionContext(childContext);
                }
            }
        }


        #region helpers

        private sealed class CompensationInfo
        {
            private Activity targetActivity = null;
            private ActivityExecutionContext targetExecutionContext = null;
            private ActivityExecutionContextInfo targetExecutionInfo = null;
            private ActivityExecutionContextManager targetExecutionContextManager = null;

            internal CompensationInfo(ActivityExecutionContextInfo targetExecutionInfo, ActivityExecutionContextManager targetExecutionContextManager)
            {
                this.targetExecutionInfo = targetExecutionInfo;
                this.targetExecutionContextManager = targetExecutionContextManager;
            }
            internal CompensationInfo(Activity targetActivity)
            {
                this.targetActivity = targetActivity;
            }
            internal CompensationInfo(ActivityExecutionContext targetExecutionContext)
            {
                this.targetExecutionContext = targetExecutionContext;
            }

            internal Activity TargetActivity
            {
                get { return targetActivity; }
            }
            internal ActivityExecutionContext TargetExecutionContext
            {
                get { return targetExecutionContext; }
            }
            internal ActivityExecutionContextInfo TargetExecutionInfo
            {
                get { return targetExecutionInfo; }
            }
            internal ActivityExecutionContextManager TargetExecutionContextManager
            {
                get { return targetExecutionContextManager; }
            }
        }

        //Walk through all of the direct children which are compensatable and add them in the sorted order of their completion
        //bail out if any of the compensatable children is currently compensating/faulting or canceling
        private static bool CollectCompensatableTargetActivities(CompositeActivity compositeActivity, SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets, bool immediateCompensation)
        {
            // walk through all compensatable children and compensate them
            Queue<Activity> completedActivities = new Queue<Activity>(Helpers.GetAllEnabledActivities(compositeActivity));
            while (completedActivities.Count > 0)
            {
                Activity completedChild = completedActivities.Dequeue();
                if (completedChild.ExecutionStatus == ActivityExecutionStatus.Compensating || completedChild.ExecutionStatus == ActivityExecutionStatus.Faulting || completedChild.ExecutionStatus == ActivityExecutionStatus.Canceling)
                    return true;

                //Don't walk activities which are part of reverse work of target activity.
                if (immediateCompensation && IsActivityInBackWorkBranch(compositeActivity, completedChild))
                    continue;

                if (completedChild is ICompensatableActivity && completedChild.ExecutionStatus == ActivityExecutionStatus.Closed && completedChild.ExecutionResult == ActivityExecutionResult.Succeeded)
                    sortedListOfCompensatableTargets.Add((int)completedChild.GetValue(Activity.CompletedOrderIdProperty), new CompensationInfo(completedChild));
                else if (completedChild is CompositeActivity)
                {
                    foreach (Activity nestedCompletedActivity in Helpers.GetAllEnabledActivities((CompositeActivity)completedChild))
                        completedActivities.Enqueue(nestedCompletedActivity);
                }
            }
            return false;
        }

        // walk through active contexts that contain compensatable child, add them in the sorted order of the completion
        // this also, walks through the completed contexts which are compensatable and are nested directly within the active contexts and adds them in the order of their completion
        // bail out if any activity is currently compensating/faulting or cancelling
        private static bool CollectCompensatableActiveContexts(ActivityExecutionContext context, Activity targetActivity, SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets, bool immediateCompensation)
        {
            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;

            foreach (ActivityExecutionContext activeContext in contextManager.ExecutionContexts)
            {
                if (targetActivity.GetActivityByName(activeContext.Activity.QualifiedName, true) != null)
                {
                    //Dont walk context which are part of reverse work.
                    if (immediateCompensation && IsActivityInBackWorkBranch(targetActivity, activeContext.Activity))
                        continue;

                    if (activeContext.Activity is ICompensatableActivity && (activeContext.Activity.ExecutionStatus == ActivityExecutionStatus.Compensating || activeContext.Activity.ExecutionStatus == ActivityExecutionStatus.Faulting || activeContext.Activity.ExecutionStatus == ActivityExecutionStatus.Canceling))
                        return true;
                    else if (activeContext.Activity is CompositeActivity)
                    {
                        Activity[] activities = GetCompensatableChildren(activeContext.Activity as CompositeActivity);
                        if (activities != null)
                        {
                            int lastcompletedContextOrderId = 0;
                            foreach (Activity childActivity in activities)
                            {
                                int completedOrderId = (int)childActivity.GetValue(Activity.CompletedOrderIdProperty);
                                if (lastcompletedContextOrderId < completedOrderId)
                                    lastcompletedContextOrderId = completedOrderId;

                            }
                            if (lastcompletedContextOrderId != 0)
                                sortedListOfCompensatableTargets.Add(lastcompletedContextOrderId, new CompensationInfo(activeContext));
                        }
                        CollectCompensatableActiveContexts(activeContext, targetActivity, sortedListOfCompensatableTargets, immediateCompensation);
                        CollectCompensatableCompletedContexts(activeContext, targetActivity, sortedListOfCompensatableTargets, immediateCompensation);
                    }
                }
            }
            return false;
        }

        private static bool IsActivityInBackWorkBranch(Activity targetParent, Activity childActivity)
        {
            //Find immediate child in targetParent, which is in path to childActivity.
            Activity immediateChild = childActivity;

            while (immediateChild.Parent != targetParent)
                immediateChild = immediateChild.Parent;

            return Helpers.IsFrameworkActivity(immediateChild);
        }

        // walk through all completed execution contexts which are compensatable and are directly nested under the target activity, 
        //and add them to our sorted list
        private static void CollectCompensatableCompletedContexts(ActivityExecutionContext context, Activity targetActivity, SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets, bool immediateCompensation)
        {
            // walk through all completed execution contexts, add them to our sorted list
            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            for (int index = contextManager.CompletedExecutionContexts.Count - 1; index >= 0; index--)
            {
                //if the context does not have any compensatable children, continue
                ActivityExecutionContextInfo completedActivityInfo = contextManager.CompletedExecutionContexts[index];
                if ((completedActivityInfo.Flags & PersistFlags.NeedsCompensation) == 0)
                    continue;

                //ok, found a compensatable child.
                Activity completedActivity = targetActivity.GetActivityByName(completedActivityInfo.ActivityQualifiedName, true);

                if (completedActivity != null && !(immediateCompensation && IsActivityInBackWorkBranch(targetActivity, completedActivity)))
                    sortedListOfCompensatableTargets.Add(completedActivityInfo.CompletedOrderId, new CompensationInfo(completedActivityInfo, contextManager));
            }
        }

        internal static Activity[] GetCompensatableChildren(CompositeActivity compositeActivity)
        {
            SortedDictionary<int, Activity> sortedListOfCompensatableTargets = new SortedDictionary<int, Activity>();
            Queue<Activity> completedActivities = new Queue<Activity>(Helpers.GetAllEnabledActivities(compositeActivity));
            while (completedActivities.Count > 0)
            {
                Activity completedChild = completedActivities.Dequeue();
                if (completedChild is ICompensatableActivity && completedChild.ExecutionStatus == ActivityExecutionStatus.Closed && completedChild.ExecutionResult == ActivityExecutionResult.Succeeded)
                    sortedListOfCompensatableTargets.Add((int)completedChild.GetValue(Activity.CompletedOrderIdProperty), completedChild);

                else if (completedChild is CompositeActivity)
                {
                    foreach (Activity nestedCompletedActivity in Helpers.GetAllEnabledActivities((CompositeActivity)completedChild))
                        completedActivities.Enqueue(nestedCompletedActivity);
                }
            }
            Activity[] ar = new Activity[sortedListOfCompensatableTargets.Count];
            sortedListOfCompensatableTargets.Values.CopyTo(ar, 0);
            return ar;
        }
        internal static Activity GetLastCompensatableChild(CompositeActivity compositeActivity)
        {
            Activity[] activities = CompensationUtils.GetCompensatableChildren(compositeActivity);
            if (activities != null && activities.Length > 0 && activities[activities.Length - 1] != null)
                return activities[activities.Length - 1];

            return null;
        }
        #endregion helpers

    }
    #endregion CompensationUtils

}
