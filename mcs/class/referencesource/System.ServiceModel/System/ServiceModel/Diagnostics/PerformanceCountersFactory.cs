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
        private static bool categoriesExist = false;

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
                    EnsureCategoriesExistIfNeeded();
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
                    EnsureCategoriesExistIfNeeded();
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
                    EnsureCategoriesExistIfNeeded();
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

        // If EnsureUniquePerformanceCounterInstanceName is enabled, PerformanceCountersBase.cs will be checking if instances
        // exist in each of these categories, so we need to ensure the categories all exist. This works around System.Diagnostics 
        // calls using PerformanceCounterLib to cache which categories do/don't exist.
        private static void EnsureCategoriesExistIfNeeded()
        {
            if (categoriesExist || !ServiceModelAppSettings.EnsureUniquePerformanceCounterInstanceNames)
            {
                return;
            }

            OperationPerformanceCountersV2 operationCounter = null;
            EndpointPerformanceCountersV2 endpointCounter = null;
            ServicePerformanceCountersV2 serviceCounter = null;

            try
            {
                if (PerformanceCounterCategory.Exists(PerformanceCounterStrings.SERVICEMODELOPERATION.OperationPerfCounters) &&
                    PerformanceCounterCategory.Exists(PerformanceCounterStrings.SERVICEMODELENDPOINT.EndpointPerfCounters) &&
                    PerformanceCounterCategory.Exists(PerformanceCounterStrings.SERVICEMODELSERVICE.ServicePerfCounters))
                {
                    categoriesExist = true;
                    return;
                }

                // Categories do not exist. Update PerformanceCounterLib's cache using dummy counters.
                const string dummyValue = "_WCF_Admin";

            
                // Older operating systems (such as windows 7) report the category as not existing unless a counter instance
                // has been created in it. Create one instance in each of the categories to ensure they will exist in the cache 
                // that System.Diagnostics calls use. 
                ServiceHost dummyServiceHost = new ServiceHost(typeof(object), new Uri("http://" + dummyValue));
                operationCounter = new OperationPerformanceCountersV2(dummyValue, dummyValue, dummyValue, dummyValue);
                endpointCounter = new EndpointPerformanceCountersV2(dummyValue, dummyValue, dummyValue);
                serviceCounter = new ServicePerformanceCountersV2(dummyServiceHost);

                // Throw away cached categories, then read from the categories to cause the cache to be repopulated.
                PerformanceCounter.CloseSharedResources();
                PerformanceCounterCategory.Exists(dummyValue);
            }
            catch (UnauthorizedAccessException)
            {
                // Don't have permission to read performance counters. Trace a warning.
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                                            TraceCode.PerformanceCountersFailedForService,
                                            SR.GetString(SR.EnsureCategoriesExistFailedPermission));
                }
            }
            catch
            {
                // Failed to ensure all of the categories exist. Catch the exception and try to create the counter anyway.
            }
            finally
            {
                // Delete the dummy counters, we don't need them anymore.
                if (operationCounter != null)
                {
                    operationCounter.DeleteInstance();
                }

                if (endpointCounter != null)
                {
                    endpointCounter.DeleteInstance();
                }

                if (serviceCounter != null)
                {
                    serviceCounter.DeleteInstance();
                }
                
                categoriesExist = true;
            }
        }
    }
}
