namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;

    internal sealed class FaultAndCancellationHandlingFilter : ActivityExecutionFilter, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public static DependencyProperty FaultProcessedProperty = DependencyProperty.RegisterAttached("FaultProcessed", typeof(bool), typeof(FaultAndCancellationHandlingFilter), new PropertyMetadata(false));

        #region Execute Signal

        public override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (!(activity is CompositeActivity))
                throw new InvalidOperationException("activity");

            executionContext.Activity.HoldLockOnStatusChange(this);
            return base.Execute(activity, executionContext);
        }

        public override ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception)
        {
            if (activity.HasPrimaryClosed != true)
                return base.HandleFault(activity, executionContext, exception);

            //We are handed fault again. Quiten Fault & Cancellation Handlers if any running.
            Activity handlersActivity = FaultAndCancellationHandlingFilter.GetFaultHandlers(executionContext.Activity);
            if (handlersActivity != null && (handlersActivity.ExecutionStatus != ActivityExecutionStatus.Closed && handlersActivity.ExecutionStatus != ActivityExecutionStatus.Initialized))
            {
                if (handlersActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                    executionContext.CancelActivity(handlersActivity);

                return ActivityExecutionStatus.Faulting;
            }
            else
            {
                handlersActivity = FaultAndCancellationHandlingFilter.GetCancellationHandler(executionContext.Activity);

                if (handlersActivity != null && (handlersActivity.ExecutionStatus != ActivityExecutionStatus.Closed && handlersActivity.ExecutionStatus != ActivityExecutionStatus.Initialized))
                {
                    if (handlersActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                        executionContext.CancelActivity(handlersActivity);
                    return ActivityExecutionStatus.Faulting;
                }
            }

            if ((bool)activity.GetValue(FaultAndCancellationHandlingFilter.FaultProcessedProperty))
                SafeReleaseLockOnStatusChange(executionContext);

            return base.HandleFault(activity, executionContext, exception);
        }

        #endregion

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members

        public void OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException("sender");

            // waiting for primary activity to close
            if (e.Activity == context.Activity)
            {
                if (context.Activity.HasPrimaryClosed &&
                    !(bool)context.Activity.GetValue(FaultAndCancellationHandlingFilter.FaultProcessedProperty))
                {
                    context.Activity.SetValue(FaultAndCancellationHandlingFilter.FaultProcessedProperty, true);

                    if (context.Activity.WasExecuting &&
                        context.Activity.ExecutionResult == ActivityExecutionResult.Faulted &&
                        context.Activity.GetValue(ActivityExecutionContext.CurrentExceptionProperty) != null)
                    {
                        // execute exceptionHandlers, iff activity has transitioned from Executing to Faulting.
                        CompositeActivity exceptionHandlersActivity = FaultAndCancellationHandlingFilter.GetFaultHandlers(context.Activity);
                        if (exceptionHandlersActivity != null)
                        {
                            // listen for FaultHandler status change events
                            exceptionHandlersActivity.RegisterForStatusChange(Activity.ClosedEvent, this);

                            // execute exception handlers
                            context.ExecuteActivity(exceptionHandlersActivity);
                        }
                        else
                        {
                            // compensate completed children
                            if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                                SafeReleaseLockOnStatusChange(context); // no children to compensate...release lock on to the close status of the activity
                        }
                    }
                    else if (context.Activity.ExecutionResult == ActivityExecutionResult.Canceled)
                    {
                        // if primary activity is closed and outcome is canceled, then run the cancel handler
                        Activity cancelHandler = FaultAndCancellationHandlingFilter.GetCancellationHandler(context.Activity);
                        if (cancelHandler != null)
                        {
                            // execute the cancel handler
                            cancelHandler.RegisterForStatusChange(Activity.ClosedEvent, this);
                            context.ExecuteActivity(cancelHandler);
                        }
                        else
                        {
                            // run default compensation
                            if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                                SafeReleaseLockOnStatusChange(context); // release lock on to the close status of the activity
                        }
                    }
                    else // release lock on to the close status of the activity
                        SafeReleaseLockOnStatusChange(context);
                }
            }
            else if ((e.Activity is FaultHandlersActivity || e.Activity is CancellationHandlerActivity)
                            &&
                        (e.ExecutionStatus == ActivityExecutionStatus.Closed)
                    )
            {
                // remove subscriber
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

                // fetch the exception , it would be null if it was handled
                if (context.Activity.GetValue(ActivityExecutionContext.CurrentExceptionProperty) != null)
                {
                    // the exception was not handled by exceptionHandlers.... do default exceptionHandling
                    // compesate completed children
                    if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                        SafeReleaseLockOnStatusChange(context); // no children to compensate.Release lock on to the close status of the activity
                }
                else// the exception was handled by the exceptionHandlers. Release lock on to the close status of the parent activity
                    SafeReleaseLockOnStatusChange(context);
            }
            else if (e.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                // compensation of a child was in progress. // remove subscriber for this
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

                // see if there are other children to be compensated
                if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                    SafeReleaseLockOnStatusChange(context); // release lock on to the close status of the parent activity
            }
        }

        void SafeReleaseLockOnStatusChange(ActivityExecutionContext context)
        {
            try
            {
                context.Activity.ReleaseLockOnStatusChange(this);
            }
            catch (Exception)
            {
                context.Activity.RemoveProperty(FaultAndCancellationHandlingFilter.FaultProcessedProperty);
                throw;
            }
        }
        #endregion

        #region Helper Methods

        internal static CompositeActivity GetFaultHandlers(Activity activityWithExceptionHandlers)
        {
            CompositeActivity exceptionHandlers = null;
            CompositeActivity compositeActivity = activityWithExceptionHandlers as CompositeActivity;
            if (compositeActivity != null)
            {
                foreach (Activity activity in ((ISupportAlternateFlow)compositeActivity).AlternateFlowActivities)
                {
                    if (activity is FaultHandlersActivity)
                    {
                        exceptionHandlers = activity as CompositeActivity;
                        break;
                    }
                }
            }
            return exceptionHandlers;
        }
        internal static Activity GetCancellationHandler(Activity activityWithCancelHandler)
        {
            Activity cancelHandler = null;
            CompositeActivity compositeActivity = activityWithCancelHandler as CompositeActivity;
            if (compositeActivity != null)
            {
                foreach (Activity activity in ((ISupportAlternateFlow)compositeActivity).AlternateFlowActivities)
                {
                    if (activity is CancellationHandlerActivity)
                    {
                        cancelHandler = activity;
                        break;
                    }
                }
            }
            return cancelHandler;
        }
        #endregion
    }
}
