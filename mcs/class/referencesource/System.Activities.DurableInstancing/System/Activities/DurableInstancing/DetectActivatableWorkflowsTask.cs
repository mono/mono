//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    class DetectActivatableWorkflowsTask : PersistenceTask
    {
        public DetectActivatableWorkflowsTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, TimeSpan taskInterval)
            : base(store, storeLock, new DetectActivatableWorkflowsCommand(), taskInterval, SqlWorkflowInstanceStoreConstants.DefaultTaskTimeout, false)
        {
        }

        public override void ResetTimer(bool fireImmediately, TimeSpan? taskIntervalOverride)
        {
            InstanceOwner instanceOwner;
            if (base.Store.FindEvent(HasActivatableWorkflowEvent.Value, out instanceOwner) != null)
            {
                base.ResetTimer(fireImmediately, taskIntervalOverride);
            }
        }

        protected override void HandleError(Exception exception)
        {
            if (TD.RunnableInstancesDetectionErrorIsEnabled())
            {
                TD.RunnableInstancesDetectionError(exception);
            }

            this.ResetTimer(false);
        }
    }
}
