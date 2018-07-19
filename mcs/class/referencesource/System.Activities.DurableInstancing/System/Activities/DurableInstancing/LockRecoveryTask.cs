//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;

    class LockRecoveryTask : PersistenceTask
    {
        public LockRecoveryTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, TimeSpan taskInterval, TimeSpan taskTimeout)
            : base(store, storeLock, new RecoverInstanceLocksCommand(), taskInterval, taskTimeout, true)
        {
        }

        protected override void HandleError(Exception exception)
        {
            if (TD.InstanceLocksRecoveryErrorIsEnabled())
            {
                TD.InstanceLocksRecoveryError(exception);
            }

            base.ResetTimer(false);
        }
    }
}
