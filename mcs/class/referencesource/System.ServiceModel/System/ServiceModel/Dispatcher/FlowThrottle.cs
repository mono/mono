//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Collections.Generic;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    sealed class FlowThrottle
    {
        int capacity;
        int count;
        bool warningIssued;
        int warningRestoreLimit;
        object mutex;
        WaitCallback release;
        Queue<object> waiters;
        String propertyName;
        String configName;
        Action acquired;
        Action released;
        Action<int> ratio;

        internal FlowThrottle(WaitCallback release, int capacity, String propertyName, String configName)
        {
            if (capacity <= 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxThrottleLimitMustBeGreaterThanZero0)));

            this.count = 0;
            this.capacity = capacity;
            this.mutex = new object();
            this.release = release;
            this.waiters = new Queue<object>();
            this.propertyName = propertyName;
            this.configName = configName;
            this.warningRestoreLimit = (int)Math.Floor(0.7 * (double)capacity);
        }

        internal int Capacity
        {
            get { return this.capacity; }
            set
            {
                if (value <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxThrottleLimitMustBeGreaterThanZero0)));
                this.capacity = value;
            }
        }

        internal bool Acquire(object o)
        {
            bool acquiredThrottle = true;

            lock (this.mutex)
            {
                if (this.count < this.capacity)
                {
                    this.count++;
                }
                else
                {
                    if (this.waiters.Count == 0)
                    {
                        if (TD.MessageThrottleExceededIsEnabled())
                        {
                            if (!this.warningIssued)
                            {
                                TD.MessageThrottleExceeded(this.propertyName, this.capacity);
                                this.warningIssued = true;
                            }
                        }
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            string traceMessage;
                            if (this.propertyName != null)
                            {
                                traceMessage = SR.GetString(SR.TraceCodeServiceThrottleLimitReached,
                                                 this.propertyName, this.capacity, this.configName);
                            }
                            else
                            {
                                traceMessage = SR.GetString(SR.TraceCodeServiceThrottleLimitReachedInternal,
                                                 this.capacity);
                            }

                            TraceUtility.TraceEvent(
                                TraceEventType.Warning, TraceCode.ServiceThrottleLimitReached, traceMessage);

                        }
                    }

                    this.waiters.Enqueue(o);
                    acquiredThrottle = false;
                }

                if (this.acquired != null)
                {
                    this.acquired();
                }
                if (this.ratio != null)
                {
                    this.ratio(this.count);
                }

                return acquiredThrottle;
            }
        }

        internal void Release()
        {
            object next = null;

            lock (this.mutex)
            {
                if (this.waiters.Count > 0)
                {
                    next = this.waiters.Dequeue();
                    if (this.waiters.Count == 0)
                        this.waiters.TrimExcess();
                }
                else
                {
                    this.count--;
                    if (this.count < this.warningRestoreLimit)
                    {
                        if (TD.MessageThrottleAtSeventyPercentIsEnabled() && this.warningIssued)
                        {
                            TD.MessageThrottleAtSeventyPercent(this.propertyName, this.capacity);
                        }
                        this.warningIssued = false;
                    }
                }
            }

            if (next != null)
                this.release(next);

            if (this.released != null)
            {
                this.released();
            }
            if (this.ratio != null)
            {
                this.ratio(this.count);
            }
        }

        internal void SetReleased(Action action)
        {
            this.released = action;
        }

        internal void SetAcquired(Action action)
        {
            this.acquired = action;
        }

        internal void SetRatio(Action<int> action)
        {
            this.ratio = action;
        }
    }
}
