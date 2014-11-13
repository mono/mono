//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    static class PerformanceCounters
    {
        static PerformanceCounterScope scope;
        static object perfCounterDictionarySyncObject = new object();
        internal const int MaxInstanceNameLength = 127;
        static bool serviceOOM = false;
        static bool endpointOOM = false;
        static bool operationOOM = false;

        //we need a couple of ways of accessing the same performance counters. Normally, we know which endpoint
        //for which we need to update a perf counter. In some cases (e.g. RM), we only have a base uri. In those
        //cases, we update all the perf counters associated with the base uri. These two dictionaries point to 
        //the same underlying perf counters, but in different ways.
        static Dictionary<string, ServiceModelPerformanceCounters> performanceCounters = null;
        static Dictionary<string, ServiceModelPerformanceCountersEntry> performanceCountersBaseUri = null;
        static List<ServiceModelPerformanceCounters> performanceCountersList = null;

        static internal PerformanceCounterScope Scope
        {
            get
            {
                return PerformanceCounters.scope;
            }
            set
            {
                PerformanceCounters.scope = value;
            }
        }

        static internal bool PerformanceCountersEnabled
        {
            get
            {
                return (PerformanceCounters.scope != PerformanceCounterScope.Off) &&
                    (PerformanceCounters.scope != PerformanceCounterScope.Default);
            }
        }

        static internal bool MinimalPerformanceCountersEnabled
        {
            get
            {
                return (PerformanceCounters.scope == PerformanceCounterScope.Default);
            }
        }

        static PerformanceCounters()
        {
            PerformanceCounterScope scope = GetPerformanceCountersFromConfig();
            if (PerformanceCounterScope.Off != scope)
            {
                try
                {
                    if (scope == PerformanceCounterScope.Default)
                    {
                        scope = OSEnvironmentHelper.IsVistaOrGreater ? PerformanceCounterScope.ServiceOnly : PerformanceCounterScope.Off;
                    }
                    PerformanceCounters.scope = scope;
                }
                catch (SecurityException securityException)
                {
                    //switch off the counters - not supported in PT
                    PerformanceCounters.scope = PerformanceCounterScope.Off;

                    // not re-throwing on purpose
                    DiagnosticUtility.TraceHandledException(securityException, TraceEventType.Warning);
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Warning,
                                                    TraceCode.PerformanceCounterFailedToLoad,
                                                    SR.GetString(SR.PartialTrustPerformanceCountersNotEnabled));                        
                    }
                }
            }
            else
            {
                PerformanceCounters.scope = PerformanceCounterScope.Off;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method UnsafeGetSection which elevates in order to load config.",
            Safe = "Does not leak any config objects.")]
        [SecuritySafeCritical]
        static PerformanceCounterScope GetPerformanceCountersFromConfig()
        {
            return DiagnosticSection.UnsafeGetSection().PerformanceCounters;
        }

        static internal PerformanceCounter GetOperationPerformanceCounter(string perfCounterName, string instanceName)
        {
            return PerformanceCounters.GetPerformanceCounter(
                PerformanceCounterStrings.SERVICEMODELOPERATION.OperationPerfCounters,
                perfCounterName,
                instanceName,
                PerformanceCounterInstanceLifetime.Process);
        }

        static internal PerformanceCounter GetEndpointPerformanceCounter(string perfCounterName, string instanceName)
        {
            return PerformanceCounters.GetPerformanceCounter(
                PerformanceCounterStrings.SERVICEMODELENDPOINT.EndpointPerfCounters,
                perfCounterName,
                instanceName,
                PerformanceCounterInstanceLifetime.Process);
        }

        static internal PerformanceCounter GetServicePerformanceCounter(string perfCounterName, string instanceName)
        {
            return PerformanceCounters.GetPerformanceCounter(
                PerformanceCounterStrings.SERVICEMODELSERVICE.ServicePerfCounters,
                perfCounterName,
                instanceName,
                PerformanceCounterInstanceLifetime.Process);
        }

        static internal PerformanceCounter GetDefaultPerformanceCounter(string perfCounterName, string instanceName)
        {
            return PerformanceCounters.GetPerformanceCounter(
                PerformanceCounterStrings.SERVICEMODELSERVICE.ServicePerfCounters,
                perfCounterName,
                instanceName,
                PerformanceCounterInstanceLifetime.Global);
        }

        static internal PerformanceCounter GetPerformanceCounter(string categoryName, string perfCounterName, string instanceName, PerformanceCounterInstanceLifetime instanceLifetime)
        {
            PerformanceCounter counter = null;
            if (PerformanceCounters.PerformanceCountersEnabled || PerformanceCounters.MinimalPerformanceCountersEnabled)
            {
                counter = PerformanceCounters.GetPerformanceCounterInternal(categoryName, perfCounterName, instanceName, instanceLifetime);
            }

            return counter;
        }

        static internal PerformanceCounter GetPerformanceCounterInternal(string categoryName, string perfCounterName, string instanceName, PerformanceCounterInstanceLifetime instanceLifetime)
        {
            PerformanceCounter counter = null;
            try
            {
                counter = new PerformanceCounter();
                counter.CategoryName = categoryName;
                counter.CounterName = perfCounterName;
                counter.InstanceName = instanceName;
                counter.ReadOnly = false;
                counter.InstanceLifetime = instanceLifetime;

                // We now need to access the counter raw data to
                // force the counter object to be initialized.  This
                // will force any exceptions due to mis-installation
                // of counters to occur here and be traced appropriately.
                try
                {
                    long rawValue = counter.RawValue;
                }
                catch (InvalidOperationException)
                {
                    counter = null;
                    throw;
                }
                catch (SecurityException securityException)
                {
                    // Cannot access performance counter due to partial trust scenarios
                    // Disable the default performance counters' access otherwise
                    // in PT the service will be broken
                    PerformanceCounters.scope = PerformanceCounterScope.Off;

                    DiagnosticUtility.TraceHandledException(new SecurityException(SR.GetString(
                                SR.PartialTrustPerformanceCountersNotEnabled), securityException), TraceEventType.Warning);
                    
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustPerformanceCountersNotEnabled)));
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (null != counter)
                {
                    if (!counter.ReadOnly)
                    {
                        try
                        {
                            counter.RemoveInstance();
                        }
                        // Already inside a catch block for a failure case
                        // ok to ---- any exceptions here and trace the
                        // original failure.
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e1)
                        {
                            if (Fx.IsFatal(e1))
                            {
                                throw;
                            }
                        }
                    }

                    counter = null;
                }
                bool logEvent = true;
                if (categoryName == PerformanceCounterStrings.SERVICEMODELSERVICE.ServicePerfCounters)
                {
                    if (serviceOOM == false)
                    {
                        serviceOOM = true;
                    }
                    else
                    {
                        logEvent = false;
                    }
                }
                else if (categoryName == PerformanceCounterStrings.SERVICEMODELOPERATION.OperationPerfCounters)
                {
                    if (operationOOM == false)
                    {
                        operationOOM = true;
                    }
                    else
                    {
                        logEvent = false;
                    }
                }
                else if (categoryName == PerformanceCounterStrings.SERVICEMODELENDPOINT.EndpointPerfCounters)
                {
                    if (endpointOOM == false)
                    {
                        endpointOOM = true;
                    }
                    else
                    {
                        logEvent = false;
                    }
                }

                if (logEvent)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                                             (ushort)System.Runtime.Diagnostics.EventLogCategory.PerformanceCounter,
                                             (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToLoadPerformanceCounter,
                                             categoryName,
                                             perfCounterName,
                                             e.ToString());
                }
            }
            return counter;
        }

        internal static Dictionary<string, ServiceModelPerformanceCounters> PerformanceCountersForEndpoint
        {
            get
            {
                if (PerformanceCounters.performanceCounters == null)
                {
                    lock (PerformanceCounters.perfCounterDictionarySyncObject)
                    {
                        if (PerformanceCounters.performanceCounters == null)
                        {
                            PerformanceCounters.performanceCounters = new Dictionary<string, ServiceModelPerformanceCounters>();
                        }
                    }
                }
                return PerformanceCounters.performanceCounters;
            }
        }

        internal static List<ServiceModelPerformanceCounters> PerformanceCountersForEndpointList
        {
            get
            {
                if (PerformanceCounters.performanceCountersList == null)
                {
                    lock (PerformanceCounters.perfCounterDictionarySyncObject)
                    {
                        if (PerformanceCounters.performanceCountersList == null)
                        {
                            PerformanceCounters.performanceCountersList = new List<ServiceModelPerformanceCounters>();
                        }
                    }
                }
                return PerformanceCounters.performanceCountersList;
            }
        }

        internal static Dictionary<string, ServiceModelPerformanceCountersEntry> PerformanceCountersForBaseUri
        {
            get
            {
                if (PerformanceCounters.performanceCountersBaseUri == null)
                {
                    lock (PerformanceCounters.perfCounterDictionarySyncObject)
                    {
                        if (PerformanceCounters.performanceCountersBaseUri == null)
                        {
                            PerformanceCounters.performanceCountersBaseUri = new Dictionary<string, ServiceModelPerformanceCountersEntry>();
                        }
                    }
                }
                return PerformanceCounters.performanceCountersBaseUri;
            }
        }


        internal static void AddPerformanceCountersForEndpoint(
            ServiceHostBase serviceHost,
            ContractDescription contractDescription,
            EndpointDispatcher endpointDispatcher)
        {
            Fx.Assert(serviceHost != null, "The 'serviceHost' argument must not be null.");
            Fx.Assert(contractDescription != null, "The 'contractDescription' argument must not be null.");
            Fx.Assert(endpointDispatcher != null, "The 'endpointDispatcher' argument must not be null.");
            
            bool performanceCountersEnabled = PerformanceCounters.PerformanceCountersEnabled;
            bool minimalPerformanceCountersEnabled = PerformanceCounters.MinimalPerformanceCountersEnabled;

            if (performanceCountersEnabled || minimalPerformanceCountersEnabled)
            {
                if (endpointDispatcher.SetPerfCounterId())
                {
                    ServiceModelPerformanceCounters counters;
                    lock (PerformanceCounters.perfCounterDictionarySyncObject)
                    {
                        if (!PerformanceCounters.PerformanceCountersForEndpoint.TryGetValue(endpointDispatcher.PerfCounterId, out counters))
                        {
                            counters = new ServiceModelPerformanceCounters(serviceHost, contractDescription, endpointDispatcher);
                            if (counters.Initialized)
                            {
                                PerformanceCounters.PerformanceCountersForEndpoint.Add(endpointDispatcher.PerfCounterId, counters);

                                int index = PerformanceCounters.PerformanceCountersForEndpointList.FindIndex(c => c == null);
                                if (index >= 0)
                                {
                                    PerformanceCounters.PerformanceCountersForEndpointList[index] = counters;
                                }
                                else
                                {
                                    PerformanceCounters.PerformanceCountersForEndpointList.Add(counters);
                                    index = PerformanceCounters.PerformanceCountersForEndpointList.Count - 1;
                                }
                                endpointDispatcher.PerfCounterInstanceId = index;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    ServiceModelPerformanceCountersEntry countersEntry;
                    lock (PerformanceCounters.perfCounterDictionarySyncObject)
                    {
                        if (!PerformanceCounters.PerformanceCountersForBaseUri.TryGetValue(endpointDispatcher.PerfCounterBaseId, out countersEntry))
                        {
                            if (performanceCountersEnabled)
                            {
                                countersEntry = new ServiceModelPerformanceCountersEntry(serviceHost.Counters);
                            }
                            else if (minimalPerformanceCountersEnabled)
                            {
                                countersEntry = new ServiceModelPerformanceCountersEntry(serviceHost.DefaultCounters);
                            }
                            PerformanceCounters.PerformanceCountersForBaseUri.Add(endpointDispatcher.PerfCounterBaseId, countersEntry);
                        }
                        countersEntry.Add(counters);
                    }
                }
            }
        }

        internal static void ReleasePerformanceCountersForEndpoint(string id, string baseId)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                lock (PerformanceCounters.perfCounterDictionarySyncObject)
                {
                    if (!String.IsNullOrEmpty(id))
                    {
                        ServiceModelPerformanceCounters counters;
                        if (PerformanceCounters.PerformanceCountersForEndpoint.TryGetValue(id, out counters))
                        {
                            PerformanceCounters.PerformanceCountersForEndpoint.Remove(id);
                            int index = PerformanceCounters.PerformanceCountersForEndpointList.IndexOf(counters);
                            PerformanceCounters.PerformanceCountersForEndpointList[index] = null;
                        }
                    }
                    if (!String.IsNullOrEmpty(baseId))
                    {
                        PerformanceCounters.PerformanceCountersForBaseUri.Remove(baseId);
                    }
                }
            }
        }

        internal static void ReleasePerformanceCounter(ref PerformanceCounter counter)
        {
            if (counter != null)
            {
                try
                {
                    counter.RemoveInstance();
                    counter = null;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
        }

        internal static void TxFlowed(EndpointDispatcher el, string operation)
        {
            if (null != el)
            {
                ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (null != sCounters)
                {
                    sCounters.TxFlowed();
                }

                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    OperationPerformanceCountersBase oCounters = PerformanceCounters.GetOperationPerformanceCounters(el.PerfCounterInstanceId, operation);
                    if (null != oCounters)
                    {
                        oCounters.TxFlowed();
                    }

                    EndpointPerformanceCountersBase eCounters = PerformanceCounters.GetEndpointPerformanceCounters(el.PerfCounterInstanceId);
                    if (null != sCounters)
                    {
                        eCounters.TxFlowed();
                    }
                }
            }
        }

        internal static void TxAborted(EndpointDispatcher el, long count)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                if (null != el)
                {
                    ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                    if (null != sCounters)
                    {
                        sCounters.TxAborted(count);
                    }
                }
            }
        }

        internal static void TxCommitted(EndpointDispatcher el, long count)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                if (null != el)
                {
                    ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                    if (null != sCounters)
                    {
                        sCounters.TxCommitted(count);
                    }
                }
            }
        }


        internal static void TxInDoubt(EndpointDispatcher el, long count)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                if (null != el)
                {
                    ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                    if (null != sCounters)
                    {
                        sCounters.TxInDoubt(count);
                    }
                }
            }
        }


        internal static void MethodCalled(string operationName)
        {
            EndpointDispatcher el = GetEndpointDispatcher();
            if (null != el)
            {
                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    string uri = el.PerfCounterId;
                    OperationPerformanceCountersBase opCounters = PerformanceCounters.GetOperationPerformanceCounters(el.PerfCounterInstanceId, operationName);
                    if (null != opCounters)
                    {
                        opCounters.MethodCalled();
                    }
                    EndpointPerformanceCountersBase eCounters = PerformanceCounters.GetEndpointPerformanceCounters(el.PerfCounterInstanceId);
                    if (null != eCounters)
                    {
                        eCounters.MethodCalled();
                    }
                }
                ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (null != sCounters)
                {
                    sCounters.MethodCalled();
                }
            }
        }

        internal static void MethodReturnedSuccess(string operationName)
        {
            PerformanceCounters.MethodReturnedSuccess(operationName, -1);
        }

        internal static void MethodReturnedSuccess(string operationName, long time)
        {
            EndpointDispatcher el = GetEndpointDispatcher();
            if (null != el)
            {
                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    string uri = el.PerfCounterId;
                    OperationPerformanceCountersBase counters = PerformanceCounters.GetOperationPerformanceCounters(el.PerfCounterInstanceId, operationName);
                    if (null != counters)
                    {
                        counters.MethodReturnedSuccess();
                        if (time > 0)
                        {
                            counters.SaveCallDuration(time);
                        }
                    }
                    EndpointPerformanceCountersBase eCounters = PerformanceCounters.GetEndpointPerformanceCounters(el.PerfCounterInstanceId);
                    if (null != eCounters)
                    {
                        eCounters.MethodReturnedSuccess();
                        if (time > 0)
                        {
                            eCounters.SaveCallDuration(time);
                        }
                    }
                }
                ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (null != sCounters)
                {
                    sCounters.MethodReturnedSuccess();
                    if (time > 0)
                    {
                        sCounters.SaveCallDuration(time);
                    }
                }
            }
        }

        internal static void MethodReturnedFault(string operationName)
        {
            PerformanceCounters.MethodReturnedFault(operationName, -1);
        }

        internal static void MethodReturnedFault(string operationName, long time)
        {
            EndpointDispatcher el = GetEndpointDispatcher();
            if (null != el)
            {
                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    string uri = el.PerfCounterId;
                    OperationPerformanceCountersBase counters = PerformanceCounters.GetOperationPerformanceCounters(el.PerfCounterInstanceId, operationName);
                    if (null != counters)
                    {
                        counters.MethodReturnedFault();
                        if (time > 0)
                        {
                            counters.SaveCallDuration(time);
                        }
                    }
                    EndpointPerformanceCountersBase eCounters = PerformanceCounters.GetEndpointPerformanceCounters(el.PerfCounterInstanceId);
                    if (null != eCounters)
                    {
                        eCounters.MethodReturnedFault();
                        if (time > 0)
                        {
                            eCounters.SaveCallDuration(time);
                        }
                    }
                }
                ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (null != sCounters)
                {
                    sCounters.MethodReturnedFault();
                    if (time > 0)
                    {
                        sCounters.SaveCallDuration(time);
                    }
                }
            }
        }

        internal static void MethodReturnedError(string operationName)
        {
            PerformanceCounters.MethodReturnedError(operationName, -1);
        }

        internal static void MethodReturnedError(string operationName, long time)
        {
            EndpointDispatcher el = GetEndpointDispatcher();
            if (null != el)
            {
                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    string uri = el.PerfCounterId;
                    OperationPerformanceCountersBase counters = PerformanceCounters.GetOperationPerformanceCounters(el.PerfCounterInstanceId, operationName);
                    if (null != counters)
                    {
                        counters.MethodReturnedError();
                        if (time > 0)
                        {
                            counters.SaveCallDuration(time);
                        }
                    }
                    EndpointPerformanceCountersBase eCounters = PerformanceCounters.GetEndpointPerformanceCounters(el.PerfCounterInstanceId);
                    if (null != eCounters)
                    {
                        eCounters.MethodReturnedError();
                        if (time > 0)
                        {
                            eCounters.SaveCallDuration(time);
                        }
                    }
                }
                ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (null != sCounters)
                {
                    sCounters.MethodReturnedError();
                    if (time > 0)
                    {
                        sCounters.SaveCallDuration(time);
                    }
                }
            }
        }

        static void InvokeMethod(object o, string methodName)
        {
            Fx.Assert(null != o, "object must not be null");
            MethodInfo method = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Fx.Assert(null != method, o.GetType().ToString() + " must have method " + methodName);
            method.Invoke(o, null);
        }

        static void CallOnAllCounters(string methodName, Message message, Uri listenUri, bool includeOperations)
        {
            Fx.Assert(null != message, "message must not be null");
            Fx.Assert(null != listenUri, "listenUri must not be null");
            if (null != message && null != message.Headers && null != message.Headers.To && null != listenUri)
            {
                string uri = listenUri.AbsoluteUri.ToUpperInvariant();

                ServiceModelPerformanceCountersEntry counters = PerformanceCounters.GetServiceModelPerformanceCountersBaseUri(uri);
                if (null != counters)
                {
                    Fx.Assert(null != counters.ServicePerformanceCounters, "counters.ServicePerformanceCounters must not be null");
                    PerformanceCounters.InvokeMethod(counters.ServicePerformanceCounters, methodName);

                    if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                    {
                        List<ServiceModelPerformanceCounters> counters2 = counters.CounterList;
                        foreach (ServiceModelPerformanceCounters sCounters in counters2)
                        {
                            if (sCounters.EndpointPerformanceCounters != null)
                            {
                                PerformanceCounters.InvokeMethod(sCounters.EndpointPerformanceCounters, methodName);
                            }

                            if (includeOperations)
                            {
                                OperationPerformanceCountersBase oCounters = sCounters.GetOperationPerformanceCountersFromMessage(message);
                                if (oCounters != null)
                                {
                                    PerformanceCounters.InvokeMethod(oCounters, methodName);
                                }
                            }
                        }
                    }
                }
            }
        }

        static internal void AuthenticationFailed(Message message, Uri listenUri)
        {
            PerformanceCounters.CallOnAllCounters("AuthenticationFailed", message, listenUri, true);
        }

        static internal void AuthorizationFailed(string operationName)
        {
            EndpointDispatcher el = GetEndpointDispatcher();
            if (null != el)
            {
                string uri = el.PerfCounterId;
                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    OperationPerformanceCountersBase counters = PerformanceCounters.GetOperationPerformanceCounters(el.PerfCounterInstanceId, operationName);
                    if (null != counters)
                    {
                        counters.AuthorizationFailed();
                    }

                    EndpointPerformanceCountersBase eCounters = PerformanceCounters.GetEndpointPerformanceCounters(el.PerfCounterInstanceId);
                    if (null != eCounters)
                    {
                        eCounters.AuthorizationFailed();
                    }
                }

                ServicePerformanceCountersBase sCounters = PerformanceCounters.GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (null != sCounters)
                {
                    sCounters.AuthorizationFailed();
                }
            }
        }

        internal static void SessionFaulted(string uri)
        {
            ServiceModelPerformanceCountersEntry counters = PerformanceCounters.GetServiceModelPerformanceCountersBaseUri(uri);
            if (null != counters)
            {
                counters.ServicePerformanceCounters.SessionFaulted();
                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    List<ServiceModelPerformanceCounters> counters2 = counters.CounterList;
                    foreach (ServiceModelPerformanceCounters sCounters in counters2)
                    {
                        if (sCounters.EndpointPerformanceCounters != null)
                        {
                            sCounters.EndpointPerformanceCounters.SessionFaulted();
                        }
                    }
                }
            }
        }

        internal static void MessageDropped(string uri)
        {
            ServiceModelPerformanceCountersEntry counters = PerformanceCounters.GetServiceModelPerformanceCountersBaseUri(uri);
            if (null != counters)
            {
                counters.ServicePerformanceCounters.MessageDropped();
                if (PerformanceCounters.Scope == PerformanceCounterScope.All)
                {
                    List<ServiceModelPerformanceCounters> counters2 = counters.CounterList;
                    foreach (ServiceModelPerformanceCounters sCounters in counters2)
                    {
                        if (sCounters.EndpointPerformanceCounters != null)
                        {
                            sCounters.EndpointPerformanceCounters.MessageDropped();
                        }
                    }
                }
            }
        }

        internal static void MsmqDroppedMessage(string uri)
        {
            if (PerformanceCounters.Scope == PerformanceCounterScope.All)
            {
                ServiceModelPerformanceCountersEntry counters = PerformanceCounters.GetServiceModelPerformanceCountersBaseUri(uri);
                if (null != counters)
                {
                    counters.ServicePerformanceCounters.MsmqDroppedMessage();
                }
            }
        }

        internal static void MsmqPoisonMessage(string uri)
        {
            if (PerformanceCounters.Scope == PerformanceCounterScope.All)
            {
                ServiceModelPerformanceCountersEntry counters = PerformanceCounters.GetServiceModelPerformanceCountersBaseUri(uri);
                if (null != counters)
                {
                    counters.ServicePerformanceCounters.MsmqPoisonMessage();
                }
            }
        }

        internal static void MsmqRejectedMessage(string uri)
        {
            if (PerformanceCounters.Scope == PerformanceCounterScope.All)
            {
                ServiceModelPerformanceCountersEntry counters = PerformanceCounters.GetServiceModelPerformanceCountersBaseUri(uri);
                if (null != counters)
                {
                    counters.ServicePerformanceCounters.MsmqRejectedMessage();
                }
            }
        }

        static internal EndpointDispatcher GetEndpointDispatcher()
        {
            EndpointDispatcher endpointDispatcher = null;
            OperationContext currentContext = OperationContext.Current;
            if (null != currentContext && currentContext.InternalServiceChannel != null)
            {
                endpointDispatcher = currentContext.EndpointDispatcher;
            }

            return endpointDispatcher;
        }

        static ServiceModelPerformanceCounters GetServiceModelPerformanceCounters(int perfCounterInstanceId)
        {
            if (PerformanceCounters.PerformanceCountersForEndpointList.Count == 0)
            {
                return null;
            }
            return PerformanceCounters.PerformanceCountersForEndpointList[perfCounterInstanceId];
        }

        static ServiceModelPerformanceCountersEntry GetServiceModelPerformanceCountersBaseUri(string uri)
        {
            ServiceModelPerformanceCountersEntry counters = null;
            if (!String.IsNullOrEmpty(uri))
            {
                PerformanceCounters.PerformanceCountersForBaseUri.TryGetValue(uri, out counters);
            }
            return counters;
        }

        static OperationPerformanceCountersBase GetOperationPerformanceCounters(int perfCounterInstanceId, string operation)
        {
            ServiceModelPerformanceCounters counters = PerformanceCounters.GetServiceModelPerformanceCounters(perfCounterInstanceId);
            if (counters != null)
            {
                return counters.GetOperationPerformanceCounters(operation);
            }
            return null;
        }

        static EndpointPerformanceCountersBase GetEndpointPerformanceCounters(int perfCounterInstanceId)
        {
            ServiceModelPerformanceCounters counters = PerformanceCounters.GetServiceModelPerformanceCounters(perfCounterInstanceId);
            if (counters != null)
            {
                return counters.EndpointPerformanceCounters;
            }
            return null;
        }

        static ServicePerformanceCountersBase GetServicePerformanceCounters(int perfCounterInstanceId)
        {
            ServiceModelPerformanceCounters counters = PerformanceCounters.GetServiceModelPerformanceCounters(perfCounterInstanceId);
            if (counters != null)
            {
                return counters.ServicePerformanceCounters;
            }
            return null;
        }

        static internal void TracePerformanceCounterUpdateFailure(string instanceName, string perfCounterName)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                TraceUtility.TraceEvent(
                    System.Diagnostics.TraceEventType.Error,
                    TraceCode.PerformanceCountersFailedDuringUpdate,
                    SR.GetString(SR.TraceCodePerformanceCountersFailedDuringUpdate, perfCounterName + "::" + instanceName));
            }
        }
    }
}
