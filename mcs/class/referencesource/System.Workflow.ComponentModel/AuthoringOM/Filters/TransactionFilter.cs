// ****************************************************************************
// Copyright (C) 2000-2001 Microsoft Corporation.  All rights reserved.
//
// CONTENTS
//     Scope like TransactionModel Interceptor/Filter Executor
// 
// DESCRIPTION
//
// ****************************************************************************
namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Reflection;

    // TransactedFilter
    // 
    // This interceptor executor deals with the transaction aspects (as
    // defined by the current scope activity) of an activity.
    //
    // The activity must be attributed with SupportsTransactionAttribute.
    // The activity executor need not worry about any of the scope like 
    // transaction behavior management.
    internal sealed class TransactedContextFilter : ActivityExecutionFilter, IActivityEventListener<EventArgs>, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        #region ActivityExecutor Members

        public override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (!activity.SupportsTransaction)
                throw new ArgumentException("activity");

            // subscribe to the main activity
            activity.RegisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
            activity.HoldLockOnStatusChange(this);
            return TransactedContextFilter.ExecuteActivity(activity, executionContext, false);
        }
        #endregion

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException("sender");

            if (context.Activity.HasPrimaryClosed && context.Activity.LockCountOnStatusChange == 1)
            {
                // get exception
                Exception exception = (Exception)context.Activity.GetValue(ActivityExecutionContext.CurrentExceptionProperty);
                if (exception != null)
                {
                    WorkflowTransactionOptions transactionOptions = TransactedContextFilter.GetTransactionOptions(context.Activity);
                    if (transactionOptions != null)
                    {
                        // request revert to checkpoint state
                        context.RequestRevertToCheckpointState(this.OnRevertInstanceState, new StateRevertedEventArgs(exception), false, null);
                    }
                    else
                    {
                        // release locks 
                        context.ReleaseLocks(false);
                        context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                        context.Activity.ReleaseLockOnStatusChange(this);
                    }
                }
                else
                {
                    try
                    {
                        // 1st param is for transactional, means if the release lock on status change will try to persist the workflow instace
                        // if that fails, then locks will be reacquired, otherwise they will be released.
                        context.ReleaseLocks(true);
                        context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                        context.Activity.ReleaseLockOnStatusChange(this);
                        context.DisposeCheckpointState();
                    }
                    catch
                    {
                        // re-subscribe
                        context.Activity.RegisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                        throw;
                    }
                }
            }
        }
        #endregion

        #region OnRevertInstanceData Member

        private void OnRevertInstanceState(object sender, EventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityExecutionContext context = sender as ActivityExecutionContext;
            StateRevertedEventArgs args = e as StateRevertedEventArgs;

            // stash exception
            context.Activity.SetValueCommon(ActivityExecutionContext.CurrentExceptionProperty, args.Exception, ActivityExecutionContext.CurrentExceptionProperty.DefaultMetadata, false);

            // cancel the activity
            context.ReleaseLocks(false);
            context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
            context.Activity.ReleaseLockOnStatusChange(this);
        }

        #endregion

        #region IActivityEventListener<EventArgs> Members

        void IActivityEventListener<EventArgs>.OnEvent(object sender, EventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext)sender;

            // only if activity is still executing, then run it
            if (context.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                ActivityExecutionStatus newStatus = TransactedContextFilter.ExecuteActivity(context.Activity, context, true);
                if (newStatus == ActivityExecutionStatus.Closed)
                    context.CloseActivity();
            }
        }
        #endregion

        #region Helper Methods
        private static ActivityExecutionStatus ExecuteActivity(Activity activity, ActivityExecutionContext context, bool locksAcquired)
        {
            // acquire needed synchronization
            TransactedContextFilter executor = (TransactedContextFilter)ActivityExecutors.GetActivityExecutorFromType(typeof(TransactedContextFilter));
            if (!locksAcquired && !context.AcquireLocks(executor))
                return activity.ExecutionStatus;

            // checkpoint for instance state
            //
            WorkflowTransactionOptions transaction = TransactedContextFilter.GetTransactionOptions(activity);
            if (transaction != null)
                context.CheckpointInstanceState();

            // delegate to the next executor for the activity
            return executor.NextActivityExecutorInChain(activity).Execute(activity, context);
        }

        internal static WorkflowTransactionOptions GetTransactionOptions(Activity activity)
        {
            return activity.GetValue(activity is TransactionScopeActivity ? TransactionScopeActivity.TransactionOptionsProperty : CompensatableTransactionScopeActivity.TransactionOptionsProperty) as WorkflowTransactionOptions;
        }
        #endregion
    }

    #region StateRevertedEventArgs Class
    [Serializable]
    internal class StateRevertedEventArgs : EventArgs
    {
        public Exception Exception;
        public StateRevertedEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
    #endregion
}
