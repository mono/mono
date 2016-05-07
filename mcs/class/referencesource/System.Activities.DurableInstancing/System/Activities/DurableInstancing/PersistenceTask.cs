//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.DurableInstancing;

    abstract class PersistenceTask
    {
        bool automaticallyResetTimer;
        AsyncCallback commandCompletedCallback;
        InstancePersistenceCommand instancePersistenceCommand;
        TimeSpan taskInterval;
        IOThreadTimer taskTimer;
        object thisLock;
        bool timerCancelled;
        TimeSpan taskTimeout;

        public PersistenceTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, InstancePersistenceCommand instancePersistenceCommand, TimeSpan taskInterval, TimeSpan taskTimeout, bool automaticallyResetTimer)
        {
            this.automaticallyResetTimer = automaticallyResetTimer;
            this.commandCompletedCallback = Fx.ThunkCallback(CommandCompletedCallback);
            this.instancePersistenceCommand = instancePersistenceCommand;
            this.Store = store;
            this.StoreLock = storeLock;
            this.SurrogateLockOwnerId = this.StoreLock.SurrogateLockOwnerId;
            this.taskInterval = taskInterval;        
            this.thisLock = new object();
            this.taskTimeout = taskTimeout;
        }

        protected SqlWorkflowInstanceStore Store
        {
            get;
            set;
        }

        protected SqlWorkflowInstanceStoreLock StoreLock
        {
            get;
            set;
        }

        protected long SurrogateLockOwnerId
        {
            get;
            set;
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public void CancelTimer()
        {
            lock (ThisLock)
            {
                this.timerCancelled = true;

                if (this.taskTimer != null)
                {
                    this.taskTimer.Cancel();
                    this.taskTimer = null;
                }
            }
        }

        public void ResetTimer(bool fireImmediately)
        {
            this.ResetTimer(fireImmediately, null);
        }

        public virtual void ResetTimer(bool fireImmediately, TimeSpan? taskIntervalOverride)
        {
            TimeSpan timeTillNextPoll = this.taskInterval;

            if (taskIntervalOverride.HasValue)
            {
                if (taskIntervalOverride.Value < this.taskInterval)
                    timeTillNextPoll = taskIntervalOverride.Value;
            }

            lock (ThisLock)
            {
                if (!this.timerCancelled)
                {
                    if (this.taskTimer == null)
                    {
                        this.taskTimer = new IOThreadTimer(new Action<object>(this.OnTimerFired), null, false);
                    }

                    this.taskTimer.Set(fireImmediately ? TimeSpan.Zero : timeTillNextPoll);
                }
            }
        }

        protected abstract void HandleError(Exception exception);

        void CommandCompletedCallback(IAsyncResult result)
        {
            SqlWorkflowInstanceStoreAsyncResult sqlResult = (SqlWorkflowInstanceStoreAsyncResult) result;

            try
            {
                this.Store.EndTryCommand(result);

                if (this.automaticallyResetTimer)
                {
                    this.ResetTimer(false);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                HandleError(exception);
            }
        }

        void OnTimerFired(object state)
        {
            if (this.StoreLock.IsLockOwnerValid(this.SurrogateLockOwnerId))
            {
                try
                {
                    this.Store.BeginTryCommandSkipRetry(null, this.instancePersistenceCommand, this.taskTimeout,
                        this.commandCompletedCallback, null);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    this.HandleError(exception);
                }
            }
        }
    }
}
