//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Runtime;

    sealed class OperationPerformanceCounters : OperationPerformanceCountersBase
    {
        internal PerformanceCounter[] Counters { get; set; }

        internal OperationPerformanceCounters(string service, string contract, string operationName, string uri)
            : base(service, contract, operationName, uri)
        {
            this.Counters = new PerformanceCounter[(int)PerfCounters.TotalCounters];
            for (int i = 0; i < (int)PerfCounters.TotalCounters; i++)
            {
                PerformanceCounter counter = PerformanceCounters.GetOperationPerformanceCounter(perfCounterNames[i], this.instanceName);
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
                            TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.PerformanceCounterFailedToLoad,
                                SR.GetString(SR.TraceCodePerformanceCounterFailedToLoad), null, e);
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

        internal override void TxFlowed()
        {
            Increment((int)PerfCounters.TxFlowed);
            Increment((int)PerfCounters.TxFlowedPerSecond);
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
