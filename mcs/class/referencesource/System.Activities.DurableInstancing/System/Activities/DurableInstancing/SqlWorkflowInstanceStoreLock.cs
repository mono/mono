//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Threading;    

    class SqlWorkflowInstanceStoreLock
    {
        TimeSpan hostLockRenewalPulseInterval = TimeSpan.Zero;
        bool isBeingModified;
        Guid lockOwnerId;
        SqlWorkflowInstanceStore sqlWorkflowInstanceStore;
        WeakReference lockOwnerInstanceHandle;
        object thisLock;

        public SqlWorkflowInstanceStoreLock(SqlWorkflowInstanceStore sqlWorkflowInstanceStore)
        {
            this.sqlWorkflowInstanceStore = sqlWorkflowInstanceStore;
            this.thisLock = new object();
            this.SurrogateLockOwnerId = -1;
        }

        public PersistenceTask InstanceDetectionTask
        {
            get;
            set;
        }

        public bool IsValid
        {
            get
            {
                return IsLockOwnerValid(this.SurrogateLockOwnerId);
            }
        }

        public bool IsLockOwnerValid(long surrogateLockOwnerId)
        {
            return (this.SurrogateLockOwnerId != -1) 
                && (surrogateLockOwnerId == this.SurrogateLockOwnerId)
                    && (this.sqlWorkflowInstanceStore.InstanceOwnersExist);
        }

        public Guid LockOwnerId
        {
            get
            {
                return this.lockOwnerId;
            }
        }

        public PersistenceTask LockRecoveryTask
        {
            get;
            set;
        }

        public PersistenceTask LockRenewalTask
        {
            get;
            set;
        }

        public long SurrogateLockOwnerId
        {
            get;            
            private set;
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        TimeSpan HostLockRenewalPulseInterval
        {
            get
            {
                if (this.hostLockRenewalPulseInterval == TimeSpan.Zero)
                {
                    // if user configured HostLockRenewalPeriod is less than constant MaxHostLockRenewalPulseInterval,
                    // then HostLockRenewalPeriod is how frequently SWIS will connect to SQL store to renew lock expiration.
                    // Otherwise, SWIS will connect to SQL store to renew lock expiration every MaxHostLockRenewalPulseInterval timespan. 

                    if (SqlWorkflowInstanceStoreConstants.MaxHostLockRenewalPulseInterval < this.sqlWorkflowInstanceStore.HostLockRenewalPeriod)
                    {
                        this.hostLockRenewalPulseInterval = SqlWorkflowInstanceStoreConstants.MaxHostLockRenewalPulseInterval;
                    }
                    else
                    {
                        this.hostLockRenewalPulseInterval = this.sqlWorkflowInstanceStore.HostLockRenewalPeriod;
                    }
                }

                return this.hostLockRenewalPulseInterval;
            }
        }

        public void MarkInstanceOwnerCreated(Guid lockOwnerId, long surrogateLockOwnerId, InstanceHandle lockOwnerInstanceHandle, bool detectRunnableInstances, bool detectActivatableInstances)
        {
            Fx.Assert(this.isBeingModified, "Must have modification lock to mark owner as created");
            this.lockOwnerId = lockOwnerId;
            this.SurrogateLockOwnerId = surrogateLockOwnerId;
            this.lockOwnerInstanceHandle = new WeakReference(lockOwnerInstanceHandle);

            TimeSpan runnableInstancesDetectionPeriod = this.sqlWorkflowInstanceStore.RunnableInstancesDetectionPeriod;

            if (detectActivatableInstances)
            {
                this.InstanceDetectionTask = new DetectActivatableWorkflowsTask(this.sqlWorkflowInstanceStore, this, runnableInstancesDetectionPeriod);
            }
            else if (detectRunnableInstances)
            {
                this.InstanceDetectionTask = new DetectRunnableInstancesTask(this.sqlWorkflowInstanceStore, this, runnableInstancesDetectionPeriod);
            }

            // By setting taskTimeout value with BufferedHostLockRenewalPeriod, 
            //  BufferedHostLockRenewalPeriod becomes max sql retry duration for ExtendLock command and RecoveryIntanceLock command.            
            this.LockRenewalTask = new LockRenewalTask(this.sqlWorkflowInstanceStore, this, this.HostLockRenewalPulseInterval, this.sqlWorkflowInstanceStore.BufferedHostLockRenewalPeriod);
            this.LockRecoveryTask = new LockRecoveryTask(this.sqlWorkflowInstanceStore, this, this.HostLockRenewalPulseInterval, this.sqlWorkflowInstanceStore.BufferedHostLockRenewalPeriod);

            if (this.InstanceDetectionTask != null)
            {
                this.InstanceDetectionTask.ResetTimer(true);
            }
            this.LockRenewalTask.ResetTimer(true);
            this.LockRecoveryTask.ResetTimer(true);
        }

        public void MarkInstanceOwnerLost(long surrogateLockOwnerId, bool hasModificationLock)
        {
            if (hasModificationLock)
            {
                this.MarkInstanceOwnerLost(surrogateLockOwnerId);
            }
            else
            {
                this.TakeModificationLock();
                this.MarkInstanceOwnerLost(surrogateLockOwnerId);
                this.ReturnModificationLock();
            }
        }

        public void ReturnModificationLock()
        {
            Fx.Assert(this.isBeingModified, "Must have modification lock to release it!");
            bool lockTaken = false;

            while (true)
            {
                Monitor.Enter(ThisLock, ref lockTaken);

                if (lockTaken)
                {
                    this.isBeingModified = false;
                    Monitor.Pulse(ThisLock);
                    Monitor.Exit(ThisLock);
                    return;
                }
            }
        }

        public void TakeModificationLock()
        {
            bool lockTaken = false;

            while (true)
            {
                Monitor.Enter(ThisLock, ref lockTaken);

                if (lockTaken)
                {
                    while (this.isBeingModified)
                    {
                        Monitor.Wait(ThisLock);
                    }

                    this.isBeingModified = true;
                    Monitor.Exit(ThisLock);
                    return;
                }
            }
        }

        void MarkInstanceOwnerLost(long surrogateLockOwnerId)
        {
            Fx.Assert(this.isBeingModified, "Must have modification lock to mark owner as lost");

            if (this.SurrogateLockOwnerId == surrogateLockOwnerId)
            {
                this.SurrogateLockOwnerId = -1;
                InstanceHandle instanceHandle = this.lockOwnerInstanceHandle.Target as InstanceHandle;
                if (instanceHandle != null)
                {
                    instanceHandle.Free();
                }

                if (this.sqlWorkflowInstanceStore.IsLockRetryEnabled())
                {
                    this.sqlWorkflowInstanceStore.LoadRetryHandler.AbortPendingRetries();
                }

                if (this.LockRenewalTask != null)
                {
                    this.LockRenewalTask.CancelTimer();
                }

                if (this.LockRecoveryTask != null)
                {
                    this.LockRecoveryTask.CancelTimer();
                }

                if (this.InstanceDetectionTask != null)
                {
                    this.InstanceDetectionTask.CancelTimer();
                }
            }
        }
    }
}
