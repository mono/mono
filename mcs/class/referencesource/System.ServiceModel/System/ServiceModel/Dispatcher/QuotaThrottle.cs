//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    sealed class QuotaThrottle
    {
        int limit;
        object mutex;
        WaitCallback release;
        Queue<object> waiters;
        bool didTraceThrottleLimit;
        string propertyName = "ManualFlowControlLimit";
        string owner;

        internal QuotaThrottle(WaitCallback release, object mutex)
        {
            this.limit = Int32.MaxValue;
            this.mutex = mutex;
            this.release = release;
            this.waiters = new Queue<object>();
        }

        bool IsEnabled
        {
            get { return this.limit != Int32.MaxValue; }
        }

        internal String Owner
        {
            set { this.owner = value; }
        }

        internal int Limit
        {
            get { return this.limit; }
        }

        internal bool Acquire(object o)
        {
            lock (this.mutex)
            {
                if (this.IsEnabled)
                {
                    if (this.limit > 0)
                    {
                        this.limit--;

                        if (this.limit == 0)
                        {
                            if (DiagnosticUtility.ShouldTraceWarning && !this.didTraceThrottleLimit)
                            {
                                this.didTraceThrottleLimit = true;

                                TraceUtility.TraceEvent(
                                    TraceEventType.Warning,
                                    TraceCode.ManualFlowThrottleLimitReached,
                                    SR.GetString(SR.TraceCodeManualFlowThrottleLimitReached,
                                                 this.propertyName, this.owner));
                            }
                        }

                        return true;
                    }
                    else
                    {
                        this.waiters.Enqueue(o);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        internal int IncrementLimit(int incrementBy)
        {
            if (incrementBy < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("incrementBy", incrementBy,
                                                     SR.GetString(SR.ValueMustBeNonNegative)));
            int newLimit;
            object[] released = null;

            lock (this.mutex)
            {
                if (this.IsEnabled)
                {
                    checked { this.limit += incrementBy; }
                    released = this.LimitChanged();
                }
                newLimit = this.limit;
            }

            if (released != null)
                this.Release(released);

            return newLimit;
        }

        object[] LimitChanged()
        {
            object[] released = null;

            if (this.IsEnabled)
            {
                if ((this.waiters.Count > 0) && (this.limit > 0))
                {
                    if (this.limit < this.waiters.Count)
                    {
                        released = new object[this.limit];
                        for (int i = 0; i < this.limit; i++)
                            released[i] = this.waiters.Dequeue();

                        this.limit = 0;
                    }
                    else
                    {
                        released = this.waiters.ToArray();
                        this.waiters.Clear();
                        this.waiters.TrimExcess();

                        this.limit -= released.Length;
                    }
                }
                this.didTraceThrottleLimit = false;
            }
            else
            {
                released = this.waiters.ToArray();
                this.waiters.Clear();
                this.waiters.TrimExcess();
            }

            return released;
        }

        internal void SetLimit(int messageLimit)
        {
            if (messageLimit < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageLimit", messageLimit,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));

            object[] released = null;

            lock (this.mutex)
            {
                this.limit = messageLimit;
                released = this.LimitChanged();
            }

            if (released != null)
                this.Release(released);
        }

        void ReleaseAsync(object state)
        {
            this.release(state);
        }

        internal void Release(object[] released)
        {
            for (int i = 0; i < released.Length; i++)
                ActionItem.Schedule(this.ReleaseAsync, released[i]);
        }
    }
}
