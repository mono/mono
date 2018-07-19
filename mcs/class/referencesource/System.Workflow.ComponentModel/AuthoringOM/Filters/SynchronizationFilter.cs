// ****************************************************************************
// Copyright (C) 2000-2001 Microsoft Corporation.  All rights reserved.
//
// CONTENTS
//     Synchronization Interceptor/Filter Executor
// 
// DESCRIPTION
//
// ****************************************************************************
namespace System.Workflow.ComponentModel
{
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;

    internal sealed class SynchronizationFilter : ActivityExecutionFilter, IActivityEventListener<EventArgs>, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (activity == null)
                throw new ArgumentNullException("activity");

            //Make sure first thing we do is hold lock on StatusChange.
            activity.RegisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
            activity.HoldLockOnStatusChange(this);

            if (executionContext.AcquireLocks(this))
                return ExecuteActivityNow(executionContext);

            return activity.ExecutionStatus;
        }

        private ActivityExecutionStatus ExecuteActivityNow(ActivityExecutionContext context)
        {
            return ((ActivityExecutor)NextActivityExecutorInChain(context.Activity)).Execute(context.Activity, context);
        }

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members

        public void OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context.Activity.HasPrimaryClosed && context.Activity.LockCountOnStatusChange == 1)
            {
                // release locks and status change locks
                context.ReleaseLocks(false);
                context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                context.Activity.ReleaseLockOnStatusChange(this);
            }
        }

        #endregion

        #region IActivityEventListener<EventArgs> Members

        public void OnEvent(object sender, EventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext)sender;

            // only if activity is still executing, then run it
            if (context.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                ActivityExecutionStatus newStatus = ExecuteActivityNow(context);
                if (newStatus == ActivityExecutionStatus.Closed)
                    context.CloseActivity();
            }
        }

        #endregion
    }
}
