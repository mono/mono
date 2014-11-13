//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Administration;
    using System.Diagnostics.PerformanceData;

    abstract class EndpointPerformanceCountersBase : PerformanceCountersBase
    {
        protected string instanceName;

        protected enum PerfCounters : int
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
            RMSessionsFaulted,
            RMSessionsFaultedPerSecond,
            RMMessagesDropped,
            RMMessagesDroppedPerSecond,
            TxFlowed,
            TxFlowedPerSecond,
            TotalCounters = TxFlowedPerSecond + 1
        }

        protected static readonly string[] perfCounterNames = 
        {
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECalls,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallsPerSecond,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallsOutstanding,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallsFailed,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallsFailedPerSecond,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallsFaulted,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallsFaultedPerSecond,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallDuration,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ECallDurationBase,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ESecurityValidationAuthenticationFailures,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ESecurityValidationAuthenticationFailuresPerSecond,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ESecurityCallsNotAuthorized,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ESecurityCallsNotAuthorizedPerSecond,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ERMSessionsFaulted,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ERMSessionsFaultedPerSecond,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ERMMessagesDropped,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ERMMessagesDroppedPerSecond,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ETxFlowed,
            PerformanceCounterStrings.SERVICEMODELENDPOINT.ETxFlowedPerSecond,
        };

        const int maxCounterLength = 64;
        const int hashLength = 2;
        [Flags]
        enum truncOptions : uint
        {
            NoBits = 0,
            service15 = 0x01,
            contract16 = 0x02,
            uri31 = 0x04
        }

        internal EndpointPerformanceCountersBase(string service, string contract, string uri)
        {
            this.instanceName = CreateFriendlyInstanceName(service, contract, uri);
        }

        static internal string CreateFriendlyInstanceName(string service, string contract, string uri)
        {
            // instance name is: serviceName.interfaceName.operationName@uri
            int length = service.Length + contract.Length + uri.Length + 2;

            if (length > maxCounterLength)
            {
                int count = 0;

                truncOptions tasks = EndpointPerformanceCounters.GetCompressionTasks(
                    length, service.Length, contract.Length, uri.Length);

                //if necessary, compress service name to 13 chars with a 2 char hash code
                if ((tasks & truncOptions.service15) > 0)
                {
                    count = 15;
                    service = GetHashedString(service, count - hashLength, service.Length - count + hashLength, true);
                }

                //if necessary, compress contract name to 14 chars with a 2 char hash code
                if ((tasks & truncOptions.contract16) > 0)
                {
                    count = 16;
                    contract = GetHashedString(contract, count - hashLength, contract.Length - count + hashLength, true);
                }

                //if necessary,  compress uri to 29 chars with a 2 char hash code
                if ((tasks & truncOptions.uri31) > 0)
                {
                    count = 31;
                    uri = GetHashedString(uri, 0, uri.Length - count + hashLength, false);
                }
            }

            // replace '/' with '|' because perfmon fails when '/' is in perfcounter instance name
            return service + "." + contract + "@" + uri.Replace('/', '|');
        }

        private static truncOptions GetCompressionTasks(int totalLen, int serviceLen, int contractLen, int uriLen)
        {
            truncOptions bitmask = 0;

            if (totalLen > maxCounterLength)
            {
                int workingLen = totalLen;

                //note: order of if statements important (see spec)!
                if (workingLen > maxCounterLength && serviceLen > 15)
                {
                    bitmask |= truncOptions.service15; //compress service name to 16 chars
                    workingLen -= serviceLen - 15;
                }
                if (workingLen > maxCounterLength && contractLen > 16)
                {
                    bitmask |= truncOptions.contract16; //compress contract name to 8 chars
                    workingLen -= contractLen - 16;
                }
                if (workingLen > maxCounterLength && uriLen > 31)
                {
                    bitmask |= truncOptions.uri31; //compress uri to 31 chars
                }
            }

            return bitmask;
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
        
        internal abstract void MethodCalled();

        internal abstract void MethodReturnedSuccess();

        internal abstract void MethodReturnedError();

        internal abstract void MethodReturnedFault();

        internal abstract void SaveCallDuration(long time);

        internal abstract void AuthenticationFailed();

        internal abstract void AuthorizationFailed();

        internal abstract void SessionFaulted();

        internal abstract void MessageDropped();

        internal abstract void TxFlowed();
    }
}
