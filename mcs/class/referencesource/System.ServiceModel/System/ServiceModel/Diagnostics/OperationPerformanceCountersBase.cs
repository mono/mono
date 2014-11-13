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

    abstract class OperationPerformanceCountersBase : PerformanceCountersBase
    {
        protected string instanceName;
        protected string operationName;

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
            TxFlowed,
            TxFlowedPerSecond,
            TotalCounters = TxFlowedPerSecond + 1
        }

        protected static readonly string[] perfCounterNames = 
        {
            PerformanceCounterStrings.SERVICEMODELOPERATION.Calls,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallsPerSecond,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallsOutstanding,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallsFailed,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallsFailedPerSecond,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallsFaulted,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallsFaultedPerSecond,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallDuration,
            PerformanceCounterStrings.SERVICEMODELOPERATION.CallDurationBase,
            PerformanceCounterStrings.SERVICEMODELOPERATION.SecurityValidationAuthenticationFailures,
            PerformanceCounterStrings.SERVICEMODELOPERATION.SecurityValidationAuthenticationFailuresPerSecond,
            PerformanceCounterStrings.SERVICEMODELOPERATION.SecurityCallsNotAuthorized,
            PerformanceCounterStrings.SERVICEMODELOPERATION.SecurityCallsNotAuthorizedPerSecond,
            PerformanceCounterStrings.SERVICEMODELOPERATION.TxFlowed,
            PerformanceCounterStrings.SERVICEMODELOPERATION.TxFlowedPerSecond,
        };

        const int maxCounterLength = 64;
        const int hashLength = 2;
        [Flags]
        enum truncOptions : uint
        {
            NoBits = 0,
            service7 = 0x01,
            contract7 = 0x02,
            operation15 = 0x04,
            uri32 = 0x08
        }

        internal OperationPerformanceCountersBase(string service, string contract, string operationName, string uri)
        {
            this.operationName = operationName;
            this.instanceName = CreateFriendlyInstanceName(service, contract, operationName, uri);
        }

        static internal string CreateFriendlyInstanceName(string service, string contract, string operation, string uri)
        {
            // instance name is: serviceName.interfaceName.operationName@uri
            int length = service.Length + contract.Length + operation.Length + uri.Length + 3;

            if (length > maxCounterLength)
            {
                int count = 0;

                truncOptions tasks = OperationPerformanceCounters.GetCompressionTasks(
                    length, service.Length, contract.Length, operation.Length, uri.Length);

                //if necessary, compress service name to 5 chars with a 2 char hash code
                if ((tasks & truncOptions.service7) > 0)
                {
                    count = 7;
                    service = GetHashedString(service, count - hashLength, service.Length - count + hashLength, true);
                }

                //if necessary, compress contract name to 5 chars with a 2 char hash code
                if ((tasks & truncOptions.contract7) > 0)
                {
                    count = 7;
                    contract = GetHashedString(contract, count - hashLength, contract.Length - count + hashLength, true);
                }

                //if necessary, compress operation name to 13 chars with a 2 char hash code
                if ((tasks & truncOptions.operation15) > 0)
                {
                    count = 15;
                    operation = GetHashedString(operation, count - hashLength, operation.Length - count + hashLength, true);
                }

                //if necessary,  compress uri to 30 chars with a 2 char hash code
                if ((tasks & truncOptions.uri32) > 0)
                {
                    count = 32;
                    uri = GetHashedString(uri, 0, uri.Length - count + hashLength, false);
                }
            }

            // replace '/' with '|' because perfmon fails when '/' is in perfcounter instance name
            return service + "." + contract + "." + operation + "@" + uri.Replace('/', '|');
        }

        static truncOptions GetCompressionTasks(int totalLen, int serviceLen, int contractLen, int operationLen, int uriLen)
        {
            truncOptions bitmask = 0;

            if (totalLen > maxCounterLength)
            {
                int workingLen = totalLen;

                //note: order of if statements important (see spec)!
                if (workingLen > maxCounterLength && serviceLen > 8)
                {
                    bitmask |= truncOptions.service7; //compress service name to 8 chars
                    workingLen -= serviceLen - 7;
                }
                if (workingLen > maxCounterLength && contractLen > 7)
                {
                    bitmask |= truncOptions.contract7; //compress contract name to 8 chars
                    workingLen -= contractLen - 7;
                }
                if (workingLen > maxCounterLength && operationLen > 15)
                {
                    bitmask |= truncOptions.operation15; //compress operation name to 16 chars
                    workingLen -= operationLen - 15;
                }
                if (workingLen > maxCounterLength && uriLen > 32)
                {
                    bitmask |= truncOptions.uri32; //compress uri to 32 chars
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

        internal string OperationName
        {
            get { return this.operationName; }
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

        internal abstract void TxFlowed();
    }
}
