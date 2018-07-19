//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;

    class LockRenewalTask : PersistenceTask
    {
        public LockRenewalTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, TimeSpan taskInterval, TimeSpan taskTimeout)
            : base(store, storeLock, new ExtendLockCommand(), taskInterval, taskTimeout, true)
        {
        }

        protected override void HandleError(Exception exception)
        {
            if (TD.RenewLockSystemErrorIsEnabled())
            {
                TD.RenewLockSystemError();
            }

            base.StoreLock.MarkInstanceOwnerLost(base.SurrogateLockOwnerId, false);
        }
    }
}
