//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    sealed class InstanceLockTracking
    {
        object synchLock;
        SqlWorkflowInstanceStore store;

        public InstanceLockTracking(SqlWorkflowInstanceStore store)
        {
            this.InstanceId = Guid.Empty;
            this.store = store;
            this.synchLock = new object();
        }

        public Guid InstanceId { get; set; }
        public bool BoundToLock { get; set; }
        public long InstanceVersion { get; set; }
        public bool IsHandleFreed { get; set; }
        public bool IsSafeToUnlock { get; set; }

        public void HandleFreed()
        {
            lock (this.synchLock)
            {
                if (this.BoundToLock && this.IsSafeToUnlock)
                {
                    this.store.GenerateUnlockCommand(this);
                }

                this.IsHandleFreed = true;
            }
        }

        public void TrackStoreLock(Guid instanceId, long instanceVersion, DependentTransaction dependentTransaction)
        {
            this.BoundToLock = true;
            this.InstanceId = instanceId;
            this.InstanceVersion = instanceVersion;

            if (dependentTransaction != null)
            {
                dependentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(TransactionCompleted);
            }
            else
            {
                this.IsSafeToUnlock = true;
            }
        }

        public void TrackStoreUnlock(DependentTransaction dependentTransaction)
        {
            this.BoundToLock = false;
            this.IsHandleFreed = true;

            if (dependentTransaction != null)
            {
                dependentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(TransactedUnlockCompleted);
            }
        }

        void TransactionCompleted(object sender, TransactionEventArgs e)
        {
            lock (this.synchLock)
            {
                if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
                {
                    if (this.IsHandleFreed)
                    {
                        this.store.GenerateUnlockCommand(this);
                    }
                    else
                    {
                        this.IsSafeToUnlock = true;
                    }
                }
                else
                {
                    this.BoundToLock = false;
                }
            }
        }

        void TransactedUnlockCompleted(object sender, TransactionEventArgs e)
        {
            lock (this.synchLock)
            {
                if (e.Transaction.TransactionInformation.Status != TransactionStatus.Committed && this.IsSafeToUnlock)
                {
                    this.store.GenerateUnlockCommand(this);
                }
            }
        }
    }
}
