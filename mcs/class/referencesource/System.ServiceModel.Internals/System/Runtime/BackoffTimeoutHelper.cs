//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Threading;

    sealed class BackoffTimeoutHelper
    {
        readonly static int maxSkewMilliseconds = (int)(IOThreadTimer.SystemTimeResolutionTicks / TimeSpan.TicksPerMillisecond);
        readonly static long maxDriftTicks = IOThreadTimer.SystemTimeResolutionTicks * 2;
        readonly static TimeSpan defaultInitialWaitTime = TimeSpan.FromMilliseconds(1);
        readonly static TimeSpan defaultMaxWaitTime = TimeSpan.FromMinutes(1);

        DateTime deadline;
        TimeSpan maxWaitTime;
        TimeSpan waitTime;
        IOThreadTimer backoffTimer;
        Action<object> backoffCallback;
        object backoffState;
        Random random;
        TimeSpan originalTimeout;

        internal BackoffTimeoutHelper(TimeSpan timeout)
            : this(timeout, BackoffTimeoutHelper.defaultMaxWaitTime)
        {
        }

        internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime)
            : this(timeout, maxWaitTime, BackoffTimeoutHelper.defaultInitialWaitTime)
        {
        }

        internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime, TimeSpan initialWaitTime)
        {
            this.random = new Random(GetHashCode());
            this.maxWaitTime = maxWaitTime;
            this.originalTimeout = timeout;
            Reset(timeout, initialWaitTime);
        }

        public TimeSpan OriginalTimeout
        {
            get
            {
                return this.originalTimeout;
            }
        }

        void Reset(TimeSpan timeout, TimeSpan initialWaitTime)
        {
            if (timeout == TimeSpan.MaxValue)
            {
                this.deadline = DateTime.MaxValue;
            }
            else
            {
                this.deadline = DateTime.UtcNow + timeout;
            }
            this.waitTime = initialWaitTime;
        }

        public bool IsExpired()
        {
            if (this.deadline == DateTime.MaxValue)
            {
                return false;
            }
            else
            {
                return (DateTime.UtcNow >= this.deadline);
            }
        }

        public void WaitAndBackoff(Action<object> callback, object state)
        {
            if (this.backoffCallback != callback || this.backoffState != state)
            {
                if (this.backoffTimer != null)
                {
                    this.backoffTimer.Cancel();
                }
                this.backoffCallback = callback;
                this.backoffState = state;
                this.backoffTimer = new IOThreadTimer(callback, state, false, BackoffTimeoutHelper.maxSkewMilliseconds);
            }

            TimeSpan backoffTime = WaitTimeWithDrift();
            Backoff();
            this.backoffTimer.Set(backoffTime);
        }

        public void WaitAndBackoff()
        {
            Thread.Sleep(WaitTimeWithDrift());
            Backoff();
        }

        TimeSpan WaitTimeWithDrift()
        {
            return Ticks.ToTimeSpan(Math.Max(
                Ticks.FromTimeSpan(BackoffTimeoutHelper.defaultInitialWaitTime),
                Ticks.Add(Ticks.FromTimeSpan(this.waitTime),
                    (long)(uint)this.random.Next() % (2 * BackoffTimeoutHelper.maxDriftTicks + 1) - BackoffTimeoutHelper.maxDriftTicks)));
        }

        void Backoff()
        {
            if (waitTime.Ticks >= (maxWaitTime.Ticks / 2))
            {
                waitTime = maxWaitTime;
            }
            else
            {
                waitTime = TimeSpan.FromTicks(waitTime.Ticks * 2);
            }

            if (this.deadline != DateTime.MaxValue)
            {
                TimeSpan remainingTime = this.deadline - DateTime.UtcNow;
                if (this.waitTime > remainingTime)
                {
                    this.waitTime = remainingTime;
                    if (this.waitTime < TimeSpan.Zero)
                    {
                        this.waitTime = TimeSpan.Zero;
                    }
                }
            }
        }
    }
}
