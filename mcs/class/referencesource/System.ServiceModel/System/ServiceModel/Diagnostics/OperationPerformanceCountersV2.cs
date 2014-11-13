//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics.PerformanceData;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Administration;

    sealed class OperationPerformanceCountersV2 : OperationPerformanceCountersBase
    {
        static object syncRoot = new object();
        static Guid serviceModelProviderId = new Guid("{890c10c3-8c2a-4fe3-a36a-9eca153d47cb}");
        static Guid operationCounterSetId = new Guid("{8ebb0470-da6d-485b-8441-8e06b049157a}");

        private static readonly CounterSetInstanceCache counterSetInstanceCache = new CounterSetInstanceCache();

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile CounterSet operationCounterSet;         // Defines the counter set
        CounterSetInstance operationCounterSetInstance; // Instance of the counter set
        CounterData[] counters;

        internal OperationPerformanceCountersV2(string service, string contract, string operationName, string uri)
            : base(service, contract, operationName, uri)
        {
            EnsureCounterSet();
            // Create an instance of the counter set (contains the counter data).
            this.operationCounterSetInstance = CreateCounterSetInstance(this.InstanceName);
            this.counters = new CounterData[(int)PerfCounters.TotalCounters]; // Cache to dodge dictionary lookups in ServiceModelInstance
            for (int i = 0; i < (int)PerfCounters.TotalCounters; i++)
            {
                this.counters[i] = this.operationCounterSetInstance.Counters[i];
                this.counters[i].Value = 0;
            }
        }

        internal static void EnsureCounterSet()
        {
            if (operationCounterSet == null)
            {
                lock (syncRoot)
                {
                    if (operationCounterSet == null)
                    {
                        CounterSet localCounterSet = CreateCounterSet();
                        // Add the counters to the counter set definition.
                        localCounterSet.AddCounter((int)PerfCounters.Calls, CounterType.RawData32, perfCounterNames[(int)PerfCounters.Calls]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.CallsPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsOutstanding, CounterType.RawData32, perfCounterNames[(int)PerfCounters.CallsOutstanding]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsFailed, CounterType.RawData32, perfCounterNames[(int)PerfCounters.CallsFailed]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsFailedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.CallsFailedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsFaulted, CounterType.RawData32, perfCounterNames[(int)PerfCounters.CallsFaulted]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsFaultedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.CallsFaultedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.CallDurationBase, CounterType.AverageBase, perfCounterNames[(int)PerfCounters.CallDurationBase]);
                        localCounterSet.AddCounter((int)PerfCounters.CallDuration, CounterType.AverageTimer32, perfCounterNames[(int)PerfCounters.CallDuration]);
                        localCounterSet.AddCounter((int)PerfCounters.SecurityValidationAuthenticationFailures, CounterType.RawData32, perfCounterNames[(int)PerfCounters.SecurityValidationAuthenticationFailures]);
                        localCounterSet.AddCounter((int)PerfCounters.SecurityValidationAuthenticationFailuresPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.SecurityValidationAuthenticationFailuresPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsNotAuthorized, CounterType.RawData32, perfCounterNames[(int)PerfCounters.CallsNotAuthorized]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsNotAuthorizedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.CallsNotAuthorizedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.TxFlowed, CounterType.RawData32, perfCounterNames[(int)PerfCounters.TxFlowed]);
                        localCounterSet.AddCounter((int)PerfCounters.TxFlowedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.TxFlowedPerSecond]);
                        operationCounterSet = localCounterSet;
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into Sys.Diag.PerformanceData.CounterSet..ctor marked as SecurityCritical", Safe = "No user provided data is passed to the call")]
        [SecuritySafeCritical]
        static CounterSet CreateCounterSet()
        {
            return new CounterSet(serviceModelProviderId, operationCounterSetId, CounterSetInstanceType.Multiple);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into Sys.Diag.PerformanceData.CounterSetInstance.CreateCounterSetInstance marked as SecurityCritical", Safe = "No user provided data is passed to the call, instance name parameter is generated by Sys.ServiceModel.Diagnostics code from service description")]
        [SecuritySafeCritical]
        static CounterSetInstance CreateCounterSetInstance(string name)
        {
            return counterSetInstanceCache.Get(name) ?? operationCounterSet.CreateCounterSetInstance(name);
        }

        internal override void MethodCalled()
        {
            this.counters[(int)PerfCounters.Calls].Increment();
            this.counters[(int)PerfCounters.CallsPerSecond].Increment();
            this.counters[(int)PerfCounters.CallsOutstanding].Increment();
        }

        internal override void MethodReturnedSuccess()
        {
            this.counters[(int)PerfCounters.CallsOutstanding].Decrement();
        }

        internal override void MethodReturnedError()
        {
            this.counters[(int)PerfCounters.CallsFailed].Increment();
            this.counters[(int)PerfCounters.CallsFailedPerSecond].Increment();
            this.counters[(int)PerfCounters.CallsOutstanding].Decrement();
        }

        internal override void MethodReturnedFault()
        {
            this.counters[(int)PerfCounters.CallsFaulted].Increment();
            this.counters[(int)PerfCounters.CallsFaultedPerSecond].Increment();
            this.counters[(int)PerfCounters.CallsOutstanding].Decrement();
        }


        internal override void SaveCallDuration(long time)
        {
            this.counters[(int)PerfCounters.CallDuration].IncrementBy(time);
            this.counters[(int)PerfCounters.CallDurationBase].Increment();
        }

        internal override void AuthenticationFailed()
        {
            this.counters[(int)PerfCounters.SecurityValidationAuthenticationFailures].Increment();
            this.counters[(int)PerfCounters.SecurityValidationAuthenticationFailuresPerSecond].Increment();
        }

        internal override void AuthorizationFailed()
        {
            this.counters[(int)PerfCounters.CallsNotAuthorized].Increment();
            this.counters[(int)PerfCounters.CallsNotAuthorizedPerSecond].Increment();
        }

        internal override void TxFlowed()
        {
            this.counters[(int)PerfCounters.TxFlowed].Increment();
            this.counters[(int)PerfCounters.TxFlowedPerSecond].Increment();
        }

        internal override bool Initialized
        {
            get { return this.operationCounterSetInstance != null; }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && PerformanceCounters.PerformanceCountersEnabled && this.operationCounterSetInstance != null)
                {
                    counterSetInstanceCache.Add(this.InstanceName, this.operationCounterSetInstance);
                }
            }
            finally
            {
                // Not really necessary as base.Dispose(bool) does nothing
                // But forced to leave this with try/finally by unability to suspend FxCop 1.35 warning
                base.Dispose(disposing);
            }
        }

        internal static void CleanupCache()
        {
            counterSetInstanceCache.Cleanup();
        }
    }
}
