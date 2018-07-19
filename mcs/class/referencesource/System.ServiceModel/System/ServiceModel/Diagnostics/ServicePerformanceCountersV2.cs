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

    sealed class ServicePerformanceCountersV2 : ServicePerformanceCountersBase
    {
        static object syncRoot = new object();
        static Guid serviceModelProviderId = new Guid("{890c10c3-8c2a-4fe3-a36a-9eca153d47cb}");
        static Guid serviceCounterSetId = new Guid("{e829b6db-21ab-453b-83c9-d980ec708edd}");

        private static readonly CounterSetInstanceCache counterSetInstanceCache = new CounterSetInstanceCache();

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile CounterSet serviceCounterSet;         // Defines the counter set
        CounterSetInstance serviceCounterSetInstance; // Instance of the counter set
        CounterData[] counters;

        internal ServicePerformanceCountersV2(ServiceHostBase serviceHost)
            : base(serviceHost)
        {
            if (serviceCounterSet == null)
            {
                lock (syncRoot)
                {
                    if (serviceCounterSet == null)
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
                        localCounterSet.AddCounter((int)PerfCounters.Instances, CounterType.RawData32, perfCounterNames[(int)PerfCounters.Instances]);
                        localCounterSet.AddCounter((int)PerfCounters.InstancesRate, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.InstancesRate]);
                        localCounterSet.AddCounter((int)PerfCounters.RMSessionsFaulted, CounterType.RawData32, perfCounterNames[(int)PerfCounters.RMSessionsFaulted]);
                        localCounterSet.AddCounter((int)PerfCounters.RMSessionsFaultedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.RMSessionsFaultedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.RMMessagesDropped, CounterType.RawData32, perfCounterNames[(int)PerfCounters.RMMessagesDropped]);
                        localCounterSet.AddCounter((int)PerfCounters.RMMessagesDroppedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.RMMessagesDroppedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.TxFlowed, CounterType.RawData32, perfCounterNames[(int)PerfCounters.TxFlowed]);
                        localCounterSet.AddCounter((int)PerfCounters.TxFlowedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.TxFlowedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.TxCommitted, CounterType.RawData32, perfCounterNames[(int)PerfCounters.TxCommitted]);
                        localCounterSet.AddCounter((int)PerfCounters.TxCommittedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.TxCommittedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.TxAborted, CounterType.RawData32, perfCounterNames[(int)PerfCounters.TxAborted]);
                        localCounterSet.AddCounter((int)PerfCounters.TxAbortedPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.TxAbortedPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.TxInDoubt, CounterType.RawData32, perfCounterNames[(int)PerfCounters.TxInDoubt]);
                        localCounterSet.AddCounter((int)PerfCounters.TxInDoubtPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.TxInDoubtPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.MsmqPoisonMessages, CounterType.RawData32, perfCounterNames[(int)PerfCounters.MsmqPoisonMessages]);
                        localCounterSet.AddCounter((int)PerfCounters.MsmqPoisonMessagesPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.MsmqPoisonMessagesPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.MsmqRejectedMessages, CounterType.RawData32, perfCounterNames[(int)PerfCounters.MsmqRejectedMessages]);
                        localCounterSet.AddCounter((int)PerfCounters.MsmqRejectedMessagesPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.MsmqRejectedMessagesPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.MsmqDroppedMessages, CounterType.RawData32, perfCounterNames[(int)PerfCounters.MsmqDroppedMessages]);
                        localCounterSet.AddCounter((int)PerfCounters.MsmqDroppedMessagesPerSecond, CounterType.RateOfCountPerSecond32, perfCounterNames[(int)PerfCounters.MsmqDroppedMessagesPerSecond]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsPercentMaxCalls, CounterType.RawFraction32, perfCounterNames[(int)PerfCounters.CallsPercentMaxCalls]);
                        localCounterSet.AddCounter((int)PerfCounters.CallsPercentMaxCallsBase, CounterType.RawBase32, perfCounterNames[(int)PerfCounters.CallsPercentMaxCallsBase]);
                        localCounterSet.AddCounter((int)PerfCounters.InstancesPercentMaxInstances, CounterType.RawFraction32, perfCounterNames[(int)PerfCounters.InstancesPercentMaxInstances]);
                        localCounterSet.AddCounter((int)PerfCounters.InstancesPercentMaxInstancesBase, CounterType.RawBase32, perfCounterNames[(int)PerfCounters.InstancesPercentMaxInstancesBase]);
                        localCounterSet.AddCounter((int)PerfCounters.SessionsPercentMaxSessions, CounterType.RawFraction32, perfCounterNames[(int)PerfCounters.SessionsPercentMaxSessions]);
                        localCounterSet.AddCounter((int)PerfCounters.SessionsPercentMaxSessionsBase, CounterType.RawBase32, perfCounterNames[(int)PerfCounters.SessionsPercentMaxSessionsBase]);
                        serviceCounterSet = localCounterSet;
                    }
                }
            }
            // Create an instance of the counter set (contains the counter data).
            this.serviceCounterSetInstance = CreateCounterSetInstance(this.InstanceName);
            this.counters = new CounterData[(int)PerfCounters.TotalCounters]; // Cache to dodge dictionary lookups in ServiceModelInstance
            for (int i = 0; i < (int)PerfCounters.TotalCounters; i++)
            {
                this.counters[i] = this.serviceCounterSetInstance.Counters[i];
                this.counters[i].Value = 0;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into Sys.Diag.PerformanceData.CounterSet..ctor marked as SecurityCritical", Safe = "No user provided data is passed to the call")]
        [SecuritySafeCritical]
        static CounterSet CreateCounterSet()
        {
            return new CounterSet(serviceModelProviderId, serviceCounterSetId, CounterSetInstanceType.Multiple);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into Sys.Diag.PerformanceData.CounterSetInstance.CreateCounterSetInstance marked as SecurityCritical", Safe = "No user provided data is passed to the call, instance name parameter is generated by Sys.ServiceModel.Diagnostics code from service description")]
        [SecuritySafeCritical]
        static CounterSetInstance CreateCounterSetInstance(string name)
        {
            return counterSetInstanceCache.Get(name) ?? serviceCounterSet.CreateCounterSetInstance(name);
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

        internal override void ServiceInstanceCreated()
        {
            this.counters[(int)PerfCounters.Instances].Increment();
            this.counters[(int)PerfCounters.InstancesRate].Increment();
        }

        internal override void ServiceInstanceRemoved()
        {
            this.counters[(int)PerfCounters.Instances].Decrement();
        }

        internal override void SessionFaulted()
        {
            this.counters[(int)PerfCounters.RMSessionsFaulted].Increment();
            this.counters[(int)PerfCounters.RMSessionsFaultedPerSecond].Increment();
        }

        internal override void MessageDropped()
        {
            this.counters[(int)PerfCounters.RMMessagesDropped].Increment();
            this.counters[(int)PerfCounters.RMMessagesDroppedPerSecond].Increment();
        }

        internal override void TxCommitted(long count)
        {
            this.counters[(int)PerfCounters.TxCommitted].Increment();
            this.counters[(int)PerfCounters.TxCommittedPerSecond].Increment();
        }

        internal override void TxInDoubt(long count)
        {
            this.counters[(int)PerfCounters.TxInDoubt].Increment();
            this.counters[(int)PerfCounters.TxInDoubtPerSecond].Increment();
        }

        internal override void TxAborted(long count)
        {
            this.counters[(int)PerfCounters.TxAborted].Increment();
            this.counters[(int)PerfCounters.TxAbortedPerSecond].Increment();
        }

        internal override void TxFlowed()
        {
            this.counters[(int)PerfCounters.TxFlowed].Increment();
            this.counters[(int)PerfCounters.TxFlowedPerSecond].Increment();
        }

        internal override void MsmqDroppedMessage()
        {
            this.counters[(int)PerfCounters.MsmqDroppedMessages].Increment();
            this.counters[(int)PerfCounters.MsmqDroppedMessagesPerSecond].Increment();
        }

        internal override void MsmqPoisonMessage()
        {
            this.counters[(int)PerfCounters.MsmqPoisonMessages].Increment();
            this.counters[(int)PerfCounters.MsmqPoisonMessagesPerSecond].Increment();
        }

        internal override void MsmqRejectedMessage()
        {
            this.counters[(int)PerfCounters.MsmqRejectedMessages].Increment();
            this.counters[(int)PerfCounters.MsmqRejectedMessagesPerSecond].Increment();
        }

        internal override void IncrementThrottlePercent(int counterIndex)
        {
            this.counters[counterIndex].Increment();
        }

        internal override void SetThrottleBase(int counterIndex, long denominator)
        {
            this.counters[counterIndex].Value = denominator;
        }

        internal override void DecrementThrottlePercent(int counterIndex)
        {
            this.counters[counterIndex].Decrement();
        }

        internal override bool Initialized
        {
            get { return this.serviceCounterSetInstance != null; }
        }

        // Immediately disposes and nulls the CounterSetInstance. This differs from Dispose because Dispose is "lazy" in that
        // it holds weak references to the instances so we don't get corrupted state if the values are updated later. This
        // method is used in situations when we need to delete the instance immediately and know the values won't be updated.
        internal void DeleteInstance()
        {
            if (this.serviceCounterSetInstance != null)
            {
                this.serviceCounterSetInstance.Dispose();
                this.serviceCounterSetInstance = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && PerformanceCounters.PerformanceCountersEnabled && this.serviceCounterSetInstance != null)
                {
                    counterSetInstanceCache.Cleanup();
                    OperationPerformanceCountersV2.CleanupCache();
                    EndpointPerformanceCountersV2.CleanupCache();
                    counterSetInstanceCache.Add(this.InstanceName, this.serviceCounterSetInstance);
                }
            }
            finally
            {
                // Not really necessary as base.Dispose() does nothing
                // But forced to leave this with try/finally by unability to suspend FxCop 1.35 warning
                base.Dispose(disposing);
            }
        }
    }
}
