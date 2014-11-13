//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Administration;
    using System.Diagnostics.PerformanceData;

    abstract class ServicePerformanceCountersBase : PerformanceCountersBase
    {
        string instanceName;

        internal enum PerfCounters : int
        {
            Calls = 0,
            CallsPerSecond,
            CallsOutstanding,
            CallsFailed,
            CallsFailedPerSecond,
            CallsFaulted,
            CallsFaultedPerSecond,
            CallDuration,
            CallDurationBase,
            SecurityValidationAuthenticationFailures,
            SecurityValidationAuthenticationFailuresPerSecond,
            CallsNotAuthorized,
            CallsNotAuthorizedPerSecond,
            Instances,
            InstancesRate,
            RMSessionsFaulted,
            RMSessionsFaultedPerSecond,
            RMMessagesDropped,
            RMMessagesDroppedPerSecond,
            TxFlowed,
            TxFlowedPerSecond,
            TxCommitted,
            TxCommittedPerSecond,
            TxAborted,
            TxAbortedPerSecond,
            TxInDoubt,
            TxInDoubtPerSecond,
            MsmqPoisonMessages,
            MsmqPoisonMessagesPerSecond,
            MsmqRejectedMessages,
            MsmqRejectedMessagesPerSecond,
            MsmqDroppedMessages,
            MsmqDroppedMessagesPerSecond,
            CallsPercentMaxCalls,
            CallsPercentMaxCallsBase,
            InstancesPercentMaxInstances,
            InstancesPercentMaxInstancesBase,
            SessionsPercentMaxSessions,
            SessionsPercentMaxSessionsBase,
            TotalCounters = SessionsPercentMaxSessionsBase + 1
        }

        protected static readonly string[] perfCounterNames = 
        {
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCalls,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallsPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallsOutstanding,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallsFailed,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallsFailedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallsFaulted,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallsFaultedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallDuration,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SCallDurationBase,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SSecurityValidationAuthenticationFailures,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SSecurityValidationAuthenticationFailuresPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SSecurityCallsNotAuthorized,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SSecurityCallsNotAuthorizedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SInstances,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SInstancesPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SRMSessionsFaulted,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SRMSessionsFaultedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SRMMessagesDropped,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SRMMessagesDroppedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxFlowed,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxFlowedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxCommitted,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxCommittedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxAborted,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxAbortedPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxInDoubt,
            PerformanceCounterStrings.SERVICEMODELSERVICE.STxInDoubtPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.MsmqPoisonMessages,
            PerformanceCounterStrings.SERVICEMODELSERVICE.MsmqPoisonMessagesPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.MsmqRejectedMessages,
            PerformanceCounterStrings.SERVICEMODELSERVICE.MsmqRejectedMessagesPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.MsmqDroppedMessages,
            PerformanceCounterStrings.SERVICEMODELSERVICE.MsmqDroppedMessagesPerSecond,
            PerformanceCounterStrings.SERVICEMODELSERVICE.CallsPercentMaxConcurrentCalls,
            PerformanceCounterStrings.SERVICEMODELSERVICE.CallsPercentMaxConcurrentCallsBase,
            PerformanceCounterStrings.SERVICEMODELSERVICE.InstancesPercentMaxConcurrentInstances,
            PerformanceCounterStrings.SERVICEMODELSERVICE.InstancesPercentMaxConcurrentInstancesBase,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SessionsPercentMaxConcurrentSessions,
            PerformanceCounterStrings.SERVICEMODELSERVICE.SessionsPercentMaxConcurrentSessionsBase,
        };

        const int maxCounterLength = 64;
        const int hashLength = 2;
        [Flags]
        enum truncOptions : uint
        {
            NoBits = 0,
            service32 = 0x01,
            uri31 = 0x04
        }

        internal ServicePerformanceCountersBase(ServiceHostBase serviceHost)
        {
            this.instanceName = CreateFriendlyInstanceName(serviceHost);
        }

        internal override string InstanceName
        {
            get
            {
                return this.instanceName;
            }
        }

        internal override string[] CounterNames
        {
            get
            {
                return perfCounterNames;
            }
        }

        internal override int PerfCounterStart
        {
            get { return (int)PerfCounters.Calls; }
        }

        internal override int PerfCounterEnd
        {
            get { return (int)PerfCounters.TotalCounters; }
        }

        static internal string CreateFriendlyInstanceName(ServiceHostBase serviceHost)
        {
            // instance name is: serviceName@uri
            ServiceInfo serviceInfo = new ServiceInfo(serviceHost);
            string serviceName = serviceInfo.ServiceName;
            string uri;
            if (!TryGetFullVirtualPath(serviceHost, out uri))
            {
                uri = serviceInfo.FirstAddress;
            }
            int length = serviceName.Length + uri.Length + 2;

            if (length > maxCounterLength)
            {
                int count = 0;

                truncOptions tasks = ServicePerformanceCountersBase.GetCompressionTasks(
                    length, serviceName.Length, uri.Length);

                //if necessary, compress service name to 8 chars with a 2 char hash code
                if ((tasks & truncOptions.service32) > 0)
                {
                    count = 32;
                    serviceName = GetHashedString(serviceName, count - hashLength, serviceName.Length - count + hashLength, true);
                }

                //if necessary,  compress uri to 36 chars with a 2 char hash code
                if ((tasks & truncOptions.uri31) > 0)
                {
                    count = 31;
                    uri = GetHashedString(uri, 0, uri.Length - count + hashLength, false);
                }
            }

            // replace '/' with '|' because perfmon fails when '/' is in perfcounter instance name
            return serviceName + "@" + uri.Replace('/', '|');
        }

        static bool TryGetFullVirtualPath(ServiceHostBase serviceHost, out string uri)
        {
            VirtualPathExtension pathExtension = serviceHost.Extensions.Find<VirtualPathExtension>();
            if (pathExtension == null)
            {
                uri = null;
                return false;
            }
            uri = pathExtension.ApplicationVirtualPath + pathExtension.VirtualPath.ToString().Replace("~", "");
            return uri != null;
        }

        static truncOptions GetCompressionTasks(int totalLen, int serviceLen, int uriLen)
        {
            truncOptions bitmask = 0;

            if (totalLen > maxCounterLength)
            {
                int workingLen = totalLen;

                //note: order of if statements important (see spec)!
                if (workingLen > maxCounterLength && serviceLen > 32)
                {
                    bitmask |= truncOptions.service32; //compress service name to 16 chars
                    workingLen -= serviceLen - 32;
                }
                if (workingLen > maxCounterLength && uriLen > 31)
                {
                    bitmask |= truncOptions.uri31; //compress uri to 31 chars
                }
            }

            return bitmask;
        }

        internal abstract void MethodCalled();

        internal abstract void MethodReturnedSuccess();

        internal abstract void MethodReturnedError();

        internal abstract void MethodReturnedFault();

        internal abstract void SaveCallDuration(long time);

        internal abstract void AuthenticationFailed();

        internal abstract void AuthorizationFailed();

        internal abstract void ServiceInstanceCreated();

        internal abstract void ServiceInstanceRemoved();

        internal abstract void SessionFaulted();

        internal abstract void MessageDropped();

        internal abstract void TxCommitted(long count);

        internal abstract void TxInDoubt(long count);

        internal abstract void TxAborted(long count);

        internal abstract void TxFlowed();

        internal abstract void MsmqDroppedMessage();

        internal abstract void MsmqPoisonMessage();

        internal abstract void MsmqRejectedMessage();

        internal abstract void IncrementThrottlePercent(int counterIndex);

        internal abstract void SetThrottleBase(int counterIndex, long denominator);

        internal abstract void DecrementThrottlePercent(int counterIndex);
    }
}
