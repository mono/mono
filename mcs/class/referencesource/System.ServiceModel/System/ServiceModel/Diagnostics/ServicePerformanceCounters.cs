//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Administration;

    sealed class ServicePerformanceCounters : ServicePerformanceCountersBase
    {
        internal PerformanceCounter[] Counters { get; set; }

        internal ServicePerformanceCounters(ServiceHostBase serviceHost)
            : base(serviceHost)
        {
            this.Counters = new PerformanceCounter[(int)PerfCounters.TotalCounters];
            for (int i = 0; i < (int)PerfCounters.TotalCounters; i++)
            {
                PerformanceCounter counter = PerformanceCounters.GetServicePerformanceCounter(perfCounterNames[i], this.InstanceName);
                if (counter != null)
                {
                    try
                    {
                        counter.RawValue = 0;
                        this.Counters[i] = counter;
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        if (DiagnosticUtility.ShouldTraceError)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.PerformanceCountersFailedForService,
                                SR.GetString(SR.TraceCodePerformanceCountersFailedForService), null, e);
                        }
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        void Increment(int counter)
        {
            this.Increment(this.Counters, counter);
        }

        void IncrementBy(int counter, long time)
        {
            this.IncrementBy(this.Counters, counter, time);
        }

        void Decrement(int counter)
        {
            this.Decrement(this.Counters, counter);
        }

        void Set(int counter, long denominator)
        {
            this.Set(this.Counters, counter, denominator);
        }

        internal override void MethodCalled()
        {
            Increment((int)PerfCounters.Calls);
            Increment((int)PerfCounters.CallsPerSecond);
            Increment((int)PerfCounters.CallsOutstanding);
        }

        internal override void MethodReturnedSuccess()
        {
            Decrement((int)PerfCounters.CallsOutstanding);
        }

        internal override void MethodReturnedError()
        {
            Increment((int)PerfCounters.CallsFailed);
            Increment((int)PerfCounters.CallsFailedPerSecond);
            Decrement((int)PerfCounters.CallsOutstanding);
        }

        internal override void MethodReturnedFault()
        {
            Increment((int)PerfCounters.CallsFaulted);
            Increment((int)PerfCounters.CallsFaultedPerSecond);
            Decrement((int)PerfCounters.CallsOutstanding);
        }


        internal override void SaveCallDuration(long time)
        {
            IncrementBy((int)PerfCounters.CallDuration, time);
            Increment((int)PerfCounters.CallDurationBase);
        }

        internal override void AuthenticationFailed()
        {
            Increment((int)PerfCounters.SecurityValidationAuthenticationFailures);
            Increment((int)PerfCounters.SecurityValidationAuthenticationFailuresPerSecond);
        }

        internal override void AuthorizationFailed()
        {
            Increment((int)PerfCounters.CallsNotAuthorized);
            Increment((int)PerfCounters.CallsNotAuthorizedPerSecond);
        }

        internal override void ServiceInstanceCreated()
        {
            Increment((int)PerfCounters.Instances);
            Increment((int)PerfCounters.InstancesRate);
        }

        internal override void ServiceInstanceRemoved()
        {
            Decrement((int)PerfCounters.Instances);
        }

        internal override void SessionFaulted()
        {
            Increment((int)PerfCounters.RMSessionsFaulted);
            Increment((int)PerfCounters.RMSessionsFaultedPerSecond);
        }

        internal override void MessageDropped()
        {
            Increment((int)PerfCounters.RMMessagesDropped);
            Increment((int)PerfCounters.RMMessagesDroppedPerSecond);
        }

        internal override void TxCommitted(long count)
        {
            IncrementBy((int)PerfCounters.TxCommitted, count);
            IncrementBy((int)PerfCounters.TxCommittedPerSecond, count);
        }

        internal override void TxInDoubt(long count)
        {
            IncrementBy((int)PerfCounters.TxInDoubt, count);
            IncrementBy((int)PerfCounters.TxInDoubtPerSecond, count);
        }

        internal override void TxAborted(long count)
        {
            IncrementBy((int)PerfCounters.TxAborted, count);
            IncrementBy((int)PerfCounters.TxAbortedPerSecond, count);
        }

        internal override void TxFlowed()
        {
            Increment((int)PerfCounters.TxFlowed);
            Increment((int)PerfCounters.TxFlowedPerSecond);
        }

        internal override void MsmqDroppedMessage()
        {
            Increment((int)PerfCounters.MsmqDroppedMessages);
            Increment((int)PerfCounters.MsmqDroppedMessagesPerSecond);
        }

        internal override void MsmqPoisonMessage()
        {
            Increment((int)PerfCounters.MsmqPoisonMessages);
            Increment((int)PerfCounters.MsmqPoisonMessagesPerSecond);
        }

        internal override void MsmqRejectedMessage()
        {
            Increment((int)PerfCounters.MsmqRejectedMessages);
            Increment((int)PerfCounters.MsmqRejectedMessagesPerSecond);
        }

        internal override void IncrementThrottlePercent(int counterIndex)
        {
            Increment(counterIndex);
        }

        internal override void SetThrottleBase(int counterIndex, long denominator)
        {
            Set(counterIndex, denominator);
        }

        internal override void DecrementThrottlePercent(int counterIndex)
        {
            Decrement(counterIndex);
        }

        internal override bool Initialized
        {
            get { return this.Counters != null; }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (PerformanceCounters.PerformanceCountersEnabled)
                    {
                        if (null != this.Counters)
                        {
                            for (int ctr = this.PerfCounterStart; ctr < this.PerfCounterEnd; ++ctr)
                            {
                                PerformanceCounter counter = this.Counters[ctr];
                                if (counter != null)
                                {
                                    PerformanceCounters.ReleasePerformanceCounter(ref counter);
                                }
                                this.Counters[ctr] = null;
                            }
                            this.Counters = null;
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
