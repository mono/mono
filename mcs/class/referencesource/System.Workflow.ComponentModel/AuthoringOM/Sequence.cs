namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;

    #endregion

    internal static class SequenceHelper
    {
        private static DependencyProperty ActiveChildQualifiedNameProperty = DependencyProperty.RegisterAttached("ActiveChildQualifiedName", typeof(String), typeof(SequenceHelper));
        private static DependencyProperty ActiveChildRemovedProperty = DependencyProperty.RegisterAttached("ActiveChildRemoved", typeof(bool), typeof(SequenceHelper), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        public static ActivityExecutionStatus Execute(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            if (activity.EnabledActivities.Count == 0)
                return ActivityExecutionStatus.Closed;
            else
            {
                activity.EnabledActivities[0].RegisterForStatusChange(Activity.ClosedEvent, (IActivityEventListener<ActivityExecutionStatusChangedEventArgs>)activity);
                executionContext.ExecuteActivity(activity.EnabledActivities[0]);
                activity.SetValue(ActiveChildQualifiedNameProperty, activity.EnabledActivities[0].QualifiedName);
                return ActivityExecutionStatus.Executing;
            }
        }

        public static ActivityExecutionStatus Cancel(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            for (int i = (activity.EnabledActivities.Count - 1); i >= 0; i--)
            {
                Activity childActivity = activity.EnabledActivities[i];

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    executionContext.CancelActivity(childActivity);
                    return activity.ExecutionStatus;
                }

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Canceling ||
                    childActivity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                {
                    return activity.ExecutionStatus;
                }

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    activity.RemoveProperty(ActiveChildQualifiedNameProperty);
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        public static void OnEvent(CompositeActivity activity, Object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, (IActivityEventListener<ActivityExecutionStatusChangedEventArgs>)activity);

            if (activity.ExecutionStatus == ActivityExecutionStatus.Canceling ||
                activity.ExecutionStatus == ActivityExecutionStatus.Faulting ||
                activity.ExecutionStatus == ActivityExecutionStatus.Executing && !TryScheduleNextChild(activity, context))
            {
                activity.RemoveProperty(ActiveChildQualifiedNameProperty);
                context.CloseActivity();
            }
        }

        private static bool TryScheduleNextChild(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            IList<Activity> children = activity.EnabledActivities;

            // Find index of next activity to run.
            int indexOfNextActivity = 0;
            for (int i = (children.Count - 1); i >= 0; i--)
            {
                if (children[i].ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    // Check whether this is last child?
                    if (i == (children.Count - 1))
                        return false;

                    indexOfNextActivity = i + 1;
                    break;
                }
            }

            children[indexOfNextActivity].RegisterForStatusChange(Activity.ClosedEvent, (IActivityEventListener<ActivityExecutionStatusChangedEventArgs>)activity);
            executionContext.ExecuteActivity(children[indexOfNextActivity]);
            activity.SetValue(ActiveChildQualifiedNameProperty, children[indexOfNextActivity].QualifiedName);
            return true;
        }

        public static void OnActivityChangeRemove(CompositeActivity activity, ActivityExecutionContext executionContext, Activity removedActivity)
        {
            String activeChildQualifiedName = activity.GetValue(ActiveChildQualifiedNameProperty) as String;

            if (removedActivity.QualifiedName.Equals(activeChildQualifiedName))
                activity.SetValue(ActiveChildRemovedProperty, true);
        }

        public static void OnWorkflowChangesCompleted(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            String activeChildQualifiedName = activity.GetValue(ActiveChildQualifiedNameProperty) as String;
            bool activeChildRemovedInDynamicUpdate = (bool)activity.GetValue(ActiveChildRemovedProperty);

            if (activeChildQualifiedName != null && activeChildRemovedInDynamicUpdate)
            {   //We have our active child removed.    
                if (activity.ExecutionStatus == ActivityExecutionStatus.Canceling ||
                activity.ExecutionStatus == ActivityExecutionStatus.Faulting ||
                activity.ExecutionStatus == ActivityExecutionStatus.Executing && !TryScheduleNextChild(activity, executionContext))
                {
                    activity.RemoveProperty(ActiveChildQualifiedNameProperty);
                    executionContext.CloseActivity();
                }
            }
            activity.RemoveProperty(ActiveChildRemovedProperty);
        }
    }
}
