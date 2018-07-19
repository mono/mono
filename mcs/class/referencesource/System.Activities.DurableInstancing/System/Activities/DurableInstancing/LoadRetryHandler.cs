//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.DurableInstancing;

    sealed class LoadRetryHandler
    {
        BinaryHeap<DateTime, LoadRetryAsyncResult> retryQueue;
        object syncLock;
        IOThreadTimer retryThreadTimer;

        public LoadRetryHandler()
        {
            this.retryQueue = new BinaryHeap<DateTime, LoadRetryAsyncResult>();
            this.syncLock = new object();
            this.retryThreadTimer = new IOThreadTimer(new Action<object>(this.OnRetryTimer), null, false);
        }

        public bool Enqueue(LoadRetryAsyncResult command)
        {
            bool firstInQueue = false;
            DateTime retryTime = DateTime.UtcNow.Add(command.RetryTimeout);

            lock (this.syncLock)
            {
                firstInQueue = this.retryQueue.Enqueue(retryTime, command);
            }

            if (firstInQueue)
            {
                this.retryThreadTimer.Set(command.RetryTimeout);
            }

            return true;
        }

        public void AbortPendingRetries()
        {
            this.retryThreadTimer.Cancel();

            ICollection<KeyValuePair<DateTime, LoadRetryAsyncResult>> result;

            lock (this.syncLock)
            {
                result = this.retryQueue.RemoveAll(x => x.Value != null);
            }

            foreach (KeyValuePair<DateTime, LoadRetryAsyncResult> value in result)
            {
                ActionItem.Schedule
                (
                    (data) =>
                    {
                        LoadRetryAsyncResult tryCommandResult = data as LoadRetryAsyncResult;
                        tryCommandResult.AbortRetry();
                    },
                    value.Value
                );
            }
        }

        void OnRetryTimer(object state)
        {
            TimeSpan waitTime = TimeSpan.Zero;
            ICollection<KeyValuePair<DateTime, LoadRetryAsyncResult>> retryList;
            bool retriesPending = false;

            lock (this.syncLock)
            {
                if (!this.retryQueue.IsEmpty)
                {
                    DateTime currentTime = DateTime.UtcNow;
                    DateTime expirationTime = retryQueue.Peek().Key;

                    if (currentTime.CompareTo(expirationTime) >= 0)
                    {
                        retriesPending = true;
                    }
                    else
                    {
                        waitTime = expirationTime.Subtract(currentTime);
                    }
                }
            }

            if (retriesPending)
            {
                lock (this.syncLock)
                {
                    DateTime currentTime = DateTime.UtcNow;
                    retryList = retryQueue.TakeWhile(x => currentTime.CompareTo(x) >= 0);

                    if (!this.retryQueue.IsEmpty)
                    {
                        DateTime expirationTime = this.retryQueue.Peek().Key;
                        waitTime = expirationTime.Subtract(currentTime);
                    }
                }

                foreach (KeyValuePair<DateTime, LoadRetryAsyncResult> retry in retryList)
                {
                    retry.Value.Retry();
                }
            }

            if (waitTime != TimeSpan.Zero)
            {
                this.retryThreadTimer.Set(waitTime);
            }
        }
    }
}
