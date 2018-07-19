//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Activation;
    using System.Security;
    using System.Security.Permissions;

    class AppDomainInstanceProvider : ProviderBase, IWmiProvider
    {
        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            Fx.Assert(null != instances, "");
            IWmiInstance instance = instances.NewInstance(null);
            FillAppDomainInfo(instance);
            instances.AddInstance(instance);
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            bool bFound = false;
            if ((int)instance.GetProperty(AdministrationStrings.ProcessId) == AppDomainInfo.Current.ProcessId
                && String.Equals((string)instance.GetProperty(AdministrationStrings.Name), AppDomainInfo.Current.Name, StringComparison.Ordinal))
            {
                FillAppDomainInfo(instance);
                bFound = true;
            }

            return bFound;
        }

        internal static string GetReference()
        {
            return String.Format(CultureInfo.InvariantCulture, AdministrationStrings.AppDomainInfo +
                                    "." +
                                    AdministrationStrings.AppDomainId +
                                    "={0}," +
                                    AdministrationStrings.Name +
                                    "='{1}'," +
                                    AdministrationStrings.ProcessId +
                                    "={2}",
                               AppDomainInfo.Current.Id,
                               AppDomainInfo.Current.Name,
                               AppDomainInfo.Current.ProcessId);
        }

        internal static void FillAppDomainInfo(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            AppDomainInfo domainInfo = AppDomainInfo.Current;
            instance.SetProperty(AdministrationStrings.Name, domainInfo.Name);
            instance.SetProperty(AdministrationStrings.AppDomainId, domainInfo.Id);
            instance.SetProperty(AdministrationStrings.PerformanceCounters, PerformanceCounters.Scope.ToString());
            instance.SetProperty(AdministrationStrings.IsDefault, domainInfo.IsDefaultAppDomain);
            instance.SetProperty(AdministrationStrings.ProcessId, domainInfo.ProcessId);
            instance.SetProperty(AdministrationStrings.TraceLevel, DiagnosticUtility.Level.ToString());
            instance.SetProperty(AdministrationStrings.LogMalformedMessages, MessageLogger.LogMalformedMessages);
            instance.SetProperty(AdministrationStrings.LogMessagesAtServiceLevel, MessageLogger.LogMessagesAtServiceLevel);
            instance.SetProperty(AdministrationStrings.LogMessagesAtTransportLevel, MessageLogger.LogMessagesAtTransportLevel);
            instance.SetProperty(AdministrationStrings.ServiceConfigPath, AspNetEnvironment.Current.ConfigurationPath);
            FillListenersInfo(instance);
        }

        static IWmiInstance[] CreateListenersInfo(TraceSource traceSource, IWmiInstance instance)
        {
            Fx.Assert(null != traceSource, "");
            Fx.Assert(null != instance, "");

            IWmiInstance[] traceListeners = new IWmiInstance[traceSource.Listeners.Count];

            for (int i = 0; i < traceSource.Listeners.Count; i++)
            {
                TraceListener traceListener = traceSource.Listeners[i];
                IWmiInstance traceListenerWmiInstance = instance.NewInstance(AdministrationStrings.TraceListener);
                traceListenerWmiInstance.SetProperty(AdministrationStrings.Name, traceListener.Name);
                List<IWmiInstance> traceListenerArguments = new List<IWmiInstance>(1);

                Type type = traceListener.GetType();
                string initializeData = (string)type.InvokeMember(AdministrationStrings.InitializeData, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, traceListener, null, CultureInfo.InvariantCulture);
                string[] supportedAttributes = (string[])type.InvokeMember(AdministrationStrings.GetSupportedAttributes, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, traceListener, null, CultureInfo.InvariantCulture);

                IWmiInstance argumentWmiInstance = instance.NewInstance(AdministrationStrings.TraceListenerArgument);
                argumentWmiInstance.SetProperty(AdministrationStrings.Name, AdministrationStrings.InitializeData);
                argumentWmiInstance.SetProperty(AdministrationStrings.Value, initializeData);
                traceListenerArguments.Add(argumentWmiInstance);

                if (null != supportedAttributes)
                {
                    foreach (string attribute in supportedAttributes)
                    {
                        argumentWmiInstance = instance.NewInstance(AdministrationStrings.TraceListenerArgument);
                        argumentWmiInstance.SetProperty(AdministrationStrings.Name, attribute);
                        argumentWmiInstance.SetProperty(AdministrationStrings.Value, traceListener.Attributes[attribute]);
                        traceListenerArguments.Add(argumentWmiInstance);
                    }
                }
                traceListenerWmiInstance.SetProperty(AdministrationStrings.TraceListenerArguments, traceListenerArguments.ToArray());
                traceListeners[i] = traceListenerWmiInstance;
            }

            return traceListeners;
        }

        static void FillListenersInfo(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            TraceSource traceSource = DiagnosticUtility.DiagnosticTrace == null ? null : DiagnosticUtility.DiagnosticTrace.TraceSource;
            if (null != traceSource)
            {
                instance.SetProperty(AdministrationStrings.ServiceModelTraceListeners, CreateListenersInfo(traceSource, instance));
            }
            traceSource = MessageLogger.MessageTraceSource;
            if (null != traceSource)
            {
                instance.SetProperty(AdministrationStrings.MessageLoggingTraceListeners, CreateListenersInfo(traceSource, instance));
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are setting DiagnosticUtility.Level.",
            Safe = "Demands UnmanagedCode permission to set the Trace level")]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecuritySafeCritical]
        bool IWmiProvider.PutInstance(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            bool bFound = false;
            if ((int)instance.GetProperty(AdministrationStrings.ProcessId) == AppDomainInfo.Current.ProcessId
                && String.Equals((string)instance.GetProperty(AdministrationStrings.Name), AppDomainInfo.Current.Name, StringComparison.Ordinal))
            {
                try
                {
                    SourceLevels newLevel = (SourceLevels)Enum.Parse(typeof(SourceLevels), (string)instance.GetProperty(AdministrationStrings.TraceLevel));
                    if (DiagnosticUtility.Level != newLevel)
                    {
                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.WmiPut, SR.GetString(SR.TraceCodeWmiPut),
                                new WmiPutTraceRecord("DiagnosticTrace.Level",
                                DiagnosticUtility.Level,
                                newLevel), instance, null);
                        }
                        DiagnosticUtility.Level = newLevel;
                    }

                    bool logMalformedMessages = (bool)instance.GetProperty(AdministrationStrings.LogMalformedMessages);
                    if (MessageLogger.LogMalformedMessages != logMalformedMessages)
                    {
                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.WmiPut, SR.GetString(SR.TraceCodeWmiPut),
                                new WmiPutTraceRecord("MessageLogger.LogMalformedMessages",
                                MessageLogger.LogMalformedMessages,
                                logMalformedMessages), instance, null);
                        }
                        MessageLogger.LogMalformedMessages = logMalformedMessages;
                    }

                    bool logMessagesAtServiceLevel = (bool)instance.GetProperty(AdministrationStrings.LogMessagesAtServiceLevel);
                    if (MessageLogger.LogMessagesAtServiceLevel != logMessagesAtServiceLevel)
                    {
                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.WmiPut, SR.GetString(SR.TraceCodeWmiPut),
                                new WmiPutTraceRecord("MessageLogger.LogMessagesAtServiceLevel",
                                MessageLogger.LogMessagesAtServiceLevel,
                                logMessagesAtServiceLevel), instance, null);
                        }
                        MessageLogger.LogMessagesAtServiceLevel = logMessagesAtServiceLevel;
                    }

                    bool logMessagesAtTransportLevel = (bool)instance.GetProperty(AdministrationStrings.LogMessagesAtTransportLevel);
                    if (MessageLogger.LogMessagesAtTransportLevel != logMessagesAtTransportLevel)
                    {
                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.WmiPut, SR.GetString(SR.TraceCodeWmiPut),
                                new WmiPutTraceRecord("MessageLogger.LogMessagesAtTransportLevel",
                                MessageLogger.LogMessagesAtTransportLevel,
                                logMessagesAtTransportLevel), instance, null);
                        }
                        MessageLogger.LogMessagesAtTransportLevel = logMessagesAtTransportLevel;
                    }
                }
                catch (ArgumentException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidParameterException());
                }
                bFound = true;
            }

            return bFound;
        }
    }
}
