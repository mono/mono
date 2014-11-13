//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;

    static class PerformanceCountersFactory
    {
        static internal ServicePerformanceCountersBase CreateServiceCounters(ServiceHostBase serviceHost)
        {
            if (!CheckPermissions())
            {
                return null;
            }

            if (OSEnvironmentHelper.IsVistaOrGreater)
            {
                try
                {
                    var counters = new ServicePerformanceCountersV2(serviceHost);
                    // Workaround Sys.Diag.PerformanceData problem:
                    // Ensure that all three categories are initialized so other processes can still 
                    // expose endpoint/operation perf counters event if this one doesn't
                    EndpointPerformanceCountersV2.EnsureCounterSet(); 
                    OperationPerformanceCountersV2.EnsureCounterSet();
                    return counters;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    PerformanceCounters.Scope = PerformanceCounterScope.Off;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error,
                                                TraceCode.PerformanceCountersFailedForService,
                                                SR.GetString(SR.TraceCodePerformanceCountersFailedForService),
                                                null, e);
                    }
                    return null;
                }
            }
            return new ServicePerformanceCounters(serviceHost);
        }

        static internal EndpointPerformanceCountersBase CreateEndpointCounters(string service, string contract, string uri)
        {
            if (!CheckPermissions())
            {
                return null;
            }

            if (OSEnvironmentHelper.IsVistaOrGreater)
            {
                try
                {
                    return new EndpointPerformanceCountersV2(service, contract, uri);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    PerformanceCounters.Scope = PerformanceCounterScope.Off;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error,
                                                TraceCode.PerformanceCountersFailedForService,
                                                SR.GetString(SR.TraceCodePerformanceCountersFailedForService),
                                                null, e);
                    }
                    return null;
                }
            }
            return new EndpointPerformanceCounters(service, contract, uri);
        }

        static internal OperationPerformanceCountersBase CreateOperationCounters(string service, string contract, string operationName, string uri)
        {
            if (!CheckPermissions())
            {
                return null;
            }

            if (OSEnvironmentHelper.IsVistaOrGreater)
            {
                try
                {
                    return new OperationPerformanceCountersV2(service, contract, operationName, uri);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    PerformanceCounters.Scope = PerformanceCounterScope.Off;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error,
                                                TraceCode.PerformanceCountersFailedForService,
                                                SR.GetString(SR.TraceCodePerformanceCountersFailedForService),
                                                null, e);
                    }
                    return null;
                }
            }
            return new OperationPerformanceCounters(service, contract, operationName, uri);
        }

        /// <summary>
        /// Returns true if we're running in full trust. Otherwise turns off performance counters and returns false.
        /// </summary>
        private static bool CheckPermissions()
        {
            // At this time (.net 4.5), performance counters require Unrestricted permissions to be created.
            if (PartialTrustHelpers.AppDomainFullyTrusted)
            {
                return true;
            }

            PerformanceCounters.Scope = PerformanceCounterScope.Off;

            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning,
                                        TraceCode.PerformanceCountersFailedForService,
                                        SR.GetString(SR.PartialTrustPerformanceCountersNotEnabled));
            }

            return false;
        }
    }
}
