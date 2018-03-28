//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    class ServiceInstanceProvider : ProviderBase, IWmiProvider
    {
        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            Fx.Assert(null != instances, "");
            int processId = AppDomainInfo.Current.ProcessId;
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                IWmiInstance instance = instances.NewInstance(null);
                instance.SetProperty(AdministrationStrings.DistinguishedName, info.DistinguishedName);
                instance.SetProperty(AdministrationStrings.ProcessId, processId);

                FillServiceInfo(info, instance);
                instances.AddInstance(instance);
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            bool bFound = false;
            if ((int)instance.GetProperty(AdministrationStrings.ProcessId) == AppDomainInfo.Current.ProcessId)
            {
                foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
                {
                    if (String.Equals((string)instance.GetProperty(AdministrationStrings.DistinguishedName), info.DistinguishedName, StringComparison.OrdinalIgnoreCase))
                    {
                        FillServiceInfo(info, instance);
                        bFound = true;
                        break;
                    }
                }
            }

            return bFound;
        }

        internal static string GetReference(ServiceInfo serviceInfo)
        {
            Fx.Assert(null != serviceInfo, "");
            return String.Format(CultureInfo.InvariantCulture, AdministrationStrings.Service +
                                    "." +
                                    AdministrationStrings.DistinguishedName +
                                    "='{0}'," +
                                    AdministrationStrings.ProcessId +
                                    "={1}",
                                 serviceInfo.DistinguishedName,
                                 AppDomainInfo.Current.ProcessId);
        }

        internal static IWmiInstance GetAppDomainInfo(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            IWmiInstance appDomainInfo = instance.NewInstance(AdministrationStrings.AppDomainInfo);
            if (null != appDomainInfo)
            {
                AppDomainInstanceProvider.FillAppDomainInfo(appDomainInfo);
            }

            return appDomainInfo;
        }

        void FillBehaviorsInfo(ServiceInfo info, IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            List<IWmiInstance> behaviors = new List<IWmiInstance>(info.Behaviors.Count);
            foreach (IServiceBehavior behavior in info.Behaviors)
            {
                IWmiInstance behaviorInstance;
                FillBehaviorInfo(behavior, instance, out behaviorInstance);
                if (null != behaviorInstance)
                {
                    behaviors.Add(behaviorInstance);
                }
            }
            instance.SetProperty(AdministrationStrings.Behaviors, behaviors.ToArray());
        }

        void FillChannelsInfo(ServiceInfo info, IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            Fx.Assert(null != info, "");
            int channelsCount = 0;

            List<IWmiInstance> instances = new List<IWmiInstance>();
            IEnumerable<InstanceContext> contexts = info.Service.GetInstanceContexts();
            foreach (InstanceContext instanceContext in contexts)
            {
                lock (instanceContext.ThisLock)
                {
                    channelsCount += instanceContext.WmiChannels.Count;
                    foreach (IChannel channel in instanceContext.WmiChannels)
                    {
                        IWmiInstance channelInstance = instance.NewInstance(AdministrationStrings.Channel);
                        FillChannelInfo(channel, channelInstance);
                        instances.Add(channelInstance);
                    }
                }
            }

            instance.SetProperty(AdministrationStrings.Channels, instances.ToArray());
        }

        static void FillExtensionsInfo(ServiceInfo info, IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            Fx.Assert(null != info, "");
            ProviderBase.FillCollectionInfo(info.Service.Extensions, instance, AdministrationStrings.Extensions);
        }

        void FillServiceInfo(ServiceInfo info, IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            Fx.Assert(null != info, "");
            ProviderBase.FillCollectionInfo(info.Service.BaseAddresses, instance, AdministrationStrings.BaseAddresses);
            instance.SetProperty(AdministrationStrings.CounterInstanceName, PerformanceCounters.PerformanceCountersEnabled ? ServicePerformanceCounters.GetFriendlyInstanceName(info.Service) : String.Empty);
            instance.SetProperty(AdministrationStrings.ConfigurationName, info.ConfigurationName);
            instance.SetProperty(AdministrationStrings.DistinguishedName, info.DistinguishedName);
            instance.SetProperty(AdministrationStrings.Name, info.Name);
            instance.SetProperty(AdministrationStrings.Namespace, info.Namespace);
            instance.SetProperty(AdministrationStrings.Metadata, info.Metadata);
            instance.SetProperty(AdministrationStrings.Opened, ManagementExtension.GetTimeOpened(info.Service));


            FillBehaviorsInfo(info, instance);
            FillExtensionsInfo(info, instance);
            FillChannelsInfo(info, instance);
        }

        void FillBehaviorInfo(IServiceBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            Fx.Assert(null != existingInstance, "");
            Fx.Assert(null != behavior, "");
            instance = null;
            if (behavior is AspNetCompatibilityRequirementsAttribute)
            {
                instance = existingInstance.NewInstance("AspNetCompatibilityRequirementsAttribute");
                AspNetCompatibilityRequirementsAttribute specificBehavior = (AspNetCompatibilityRequirementsAttribute)behavior;
                instance.SetProperty(AdministrationStrings.RequirementsMode, specificBehavior.RequirementsMode.ToString());
            }
            else if (behavior is ServiceCredentials)
            {
                instance = existingInstance.NewInstance("ServiceCredentials");
                ServiceCredentials specificBehavior = (ServiceCredentials)behavior;
                if (specificBehavior.ClientCertificate != null && specificBehavior.ClientCertificate.Certificate != null)
                {
                    string result = string.Empty;
                    result += String.Format(CultureInfo.InvariantCulture, "Certificate: {0}\n", specificBehavior.ClientCertificate.Certificate);
                    instance.SetProperty(AdministrationStrings.ClientCertificate, result);
                }
                if (specificBehavior.IssuedTokenAuthentication != null && specificBehavior.IssuedTokenAuthentication.KnownCertificates != null)
                {
                    string result = string.Empty;
                    result += String.Format(CultureInfo.InvariantCulture, "AllowUntrustedRsaIssuers: {0}\n", specificBehavior.IssuedTokenAuthentication.AllowUntrustedRsaIssuers);
                    result += String.Format(CultureInfo.InvariantCulture, "CertificateValidationMode: {0}\n", specificBehavior.IssuedTokenAuthentication.CertificateValidationMode);
                    result += String.Format(CultureInfo.InvariantCulture, "RevocationMode: {0}\n", specificBehavior.IssuedTokenAuthentication.RevocationMode);
                    result += String.Format(CultureInfo.InvariantCulture, "TrustedStoreLocation: {0}\n", specificBehavior.IssuedTokenAuthentication.TrustedStoreLocation);
                    foreach (X509Certificate2 certificate in specificBehavior.IssuedTokenAuthentication.KnownCertificates)
                    {
                        if (certificate != null)
                        {
                            result += String.Format(CultureInfo.InvariantCulture, "Known certificate: {0}\n", certificate.FriendlyName);
                        }
                    }
                    result += String.Format(CultureInfo.InvariantCulture, "AudienceUriMode: {0}\n", specificBehavior.IssuedTokenAuthentication.AudienceUriMode);
                    if (specificBehavior.IssuedTokenAuthentication.AllowedAudienceUris != null)
                    {
                        foreach (string str in specificBehavior.IssuedTokenAuthentication.AllowedAudienceUris)
                        {
                            if (str != null)
                            {
                                result += String.Format(CultureInfo.InvariantCulture, "Allowed Uri: {0}\n", str);
                            }
                        }
                    }

                    instance.SetProperty(AdministrationStrings.IssuedTokenAuthentication, result);
                }
                if (specificBehavior.Peer != null && specificBehavior.Peer.Certificate != null)
                {
                    string result = string.Empty;
                    result += String.Format(CultureInfo.InvariantCulture, "Certificate: {0}\n", specificBehavior.Peer.Certificate.ToString(true));
                    instance.SetProperty(AdministrationStrings.Peer, result);
                }
                if (specificBehavior.SecureConversationAuthentication != null && specificBehavior.SecureConversationAuthentication.SecurityContextClaimTypes != null)
                {
                    string result = string.Empty;
                    foreach (Type claimType in specificBehavior.SecureConversationAuthentication.SecurityContextClaimTypes)
                    {
                        if (claimType != null)
                        {
                            result += String.Format(CultureInfo.InvariantCulture, "ClaimType: {0}\n", claimType);
                        }
                    }
                    instance.SetProperty(AdministrationStrings.SecureConversationAuthentication, result);
                }
                if (specificBehavior.ServiceCertificate != null && specificBehavior.ServiceCertificate.Certificate != null)
                {
                    instance.SetProperty(AdministrationStrings.ServiceCertificate, specificBehavior.ServiceCertificate.Certificate.ToString());
                }
                if (specificBehavior.UserNameAuthentication != null)
                {
                    instance.SetProperty(AdministrationStrings.UserNameAuthentication, String.Format(CultureInfo.InvariantCulture, "{0}: {1}", AdministrationStrings.ValidationMode, specificBehavior.UserNameAuthentication.UserNamePasswordValidationMode.ToString()));
                }
                if (specificBehavior.WindowsAuthentication != null)
                {
                    instance.SetProperty(AdministrationStrings.WindowsAuthentication, String.Format(CultureInfo.InvariantCulture, "{0}: {1}", AdministrationStrings.AllowAnonymous, specificBehavior.WindowsAuthentication.AllowAnonymousLogons.ToString()));
                }
            }
            else if (behavior is ServiceAuthorizationBehavior)
            {
                instance = existingInstance.NewInstance("ServiceAuthorizationBehavior");
                ServiceAuthorizationBehavior specificBehavior = (ServiceAuthorizationBehavior)behavior;
                instance.SetProperty(AdministrationStrings.ImpersonateCallerForAllOperations, specificBehavior.ImpersonateCallerForAllOperations);
                instance.SetProperty(AdministrationStrings.ImpersonateOnSerializingReply, specificBehavior.ImpersonateOnSerializingReply);
                if (specificBehavior.RoleProvider != null)
                {
                    instance.SetProperty(AdministrationStrings.RoleProvider, specificBehavior.RoleProvider.ToString());
                }
                if (specificBehavior.ServiceAuthorizationManager != null)
                {
                    instance.SetProperty(AdministrationStrings.ServiceAuthorizationManager, specificBehavior.ServiceAuthorizationManager.ToString());
                }
                instance.SetProperty(AdministrationStrings.PrincipalPermissionMode, specificBehavior.PrincipalPermissionMode.ToString());
            }
            else if (behavior is ServiceSecurityAuditBehavior)
            {
                instance = existingInstance.NewInstance("ServiceSecurityAuditBehavior");
                ServiceSecurityAuditBehavior specificBehavior = (ServiceSecurityAuditBehavior)behavior;
                instance.SetProperty(AdministrationStrings.AuditLogLocation, specificBehavior.AuditLogLocation.ToString());
                instance.SetProperty(AdministrationStrings.SuppressAuditFailure, specificBehavior.SuppressAuditFailure);
                instance.SetProperty(AdministrationStrings.ServiceAuthorizationAuditLevel, specificBehavior.ServiceAuthorizationAuditLevel.ToString());
                instance.SetProperty(AdministrationStrings.MessageAuthenticationAuditLevel, specificBehavior.MessageAuthenticationAuditLevel.ToString());
            }
            else if (behavior is ServiceBehaviorAttribute)
            {
                instance = existingInstance.NewInstance("ServiceBehaviorAttribute");
                ServiceBehaviorAttribute serviceBehavior = (ServiceBehaviorAttribute)behavior;
                instance.SetProperty(AdministrationStrings.AddressFilterMode, serviceBehavior.AddressFilterMode.ToString());
                instance.SetProperty(AdministrationStrings.AutomaticSessionShutdown, serviceBehavior.AutomaticSessionShutdown);
                instance.SetProperty(AdministrationStrings.ConcurrencyMode, serviceBehavior.ConcurrencyMode.ToString());
                instance.SetProperty(AdministrationStrings.ConfigurationName, serviceBehavior.ConfigurationName);
                instance.SetProperty(AdministrationStrings.EnsureOrderedDispatch, serviceBehavior.EnsureOrderedDispatch);
                instance.SetProperty(AdministrationStrings.IgnoreExtensionDataObject, serviceBehavior.IgnoreExtensionDataObject);
                instance.SetProperty(AdministrationStrings.IncludeExceptionDetailInFaults, serviceBehavior.IncludeExceptionDetailInFaults);
                instance.SetProperty(AdministrationStrings.InstanceContextMode, serviceBehavior.InstanceContextMode.ToString());
                instance.SetProperty(AdministrationStrings.MaxItemsInObjectGraph, serviceBehavior.MaxItemsInObjectGraph);
                instance.SetProperty(AdministrationStrings.Name, serviceBehavior.Name);
                instance.SetProperty(AdministrationStrings.Namespace, serviceBehavior.Namespace);
                instance.SetProperty(AdministrationStrings.ReleaseServiceInstanceOnTransactionComplete, serviceBehavior.ReleaseServiceInstanceOnTransactionComplete);
                instance.SetProperty(AdministrationStrings.TransactionAutoCompleteOnSessionClose, serviceBehavior.TransactionAutoCompleteOnSessionClose);
                instance.SetProperty(AdministrationStrings.TransactionIsolationLevel, serviceBehavior.TransactionIsolationLevel.ToString());
                if (serviceBehavior.TransactionTimeoutSet)
                {
                    instance.SetProperty(AdministrationStrings.TransactionTimeout, serviceBehavior.TransactionTimeoutTimespan);
                }
                instance.SetProperty(AdministrationStrings.UseSynchronizationContext, serviceBehavior.UseSynchronizationContext);
                instance.SetProperty(AdministrationStrings.ValidateMustUnderstand, serviceBehavior.ValidateMustUnderstand);
            }
            else if (behavior is ServiceDebugBehavior)
            {
                instance = existingInstance.NewInstance("ServiceDebugBehavior");
                ServiceDebugBehavior specificBehavior = (ServiceDebugBehavior)behavior;
                if (null != specificBehavior.HttpHelpPageUrl)
                {
                    instance.SetProperty(AdministrationStrings.HttpHelpPageUrl, specificBehavior.HttpHelpPageUrl.ToString());
                }
                instance.SetProperty(AdministrationStrings.HttpHelpPageEnabled, specificBehavior.HttpHelpPageEnabled);
                if (null != specificBehavior.HttpsHelpPageUrl)
                {
                    instance.SetProperty(AdministrationStrings.HttpsHelpPageUrl, specificBehavior.HttpsHelpPageUrl.ToString());
                }
                instance.SetProperty(AdministrationStrings.HttpsHelpPageEnabled, specificBehavior.HttpsHelpPageEnabled);
                instance.SetProperty(AdministrationStrings.IncludeExceptionDetailInFaults, specificBehavior.IncludeExceptionDetailInFaults);
            }
            else if (behavior is ServiceMetadataBehavior)
            {
                instance = existingInstance.NewInstance("ServiceMetadataBehavior");
                ServiceMetadataBehavior metadataBehavior = (ServiceMetadataBehavior)behavior;
                if (null != metadataBehavior.ExternalMetadataLocation)
                {
                    instance.SetProperty(AdministrationStrings.ExternalMetadataLocation, metadataBehavior.ExternalMetadataLocation.ToString());
                }
                instance.SetProperty(AdministrationStrings.HttpGetEnabled, metadataBehavior.HttpGetEnabled);
                if (null != metadataBehavior.HttpGetUrl)
                {
                    instance.SetProperty(AdministrationStrings.HttpGetUrl, metadataBehavior.HttpGetUrl.ToString());
                }
                instance.SetProperty(AdministrationStrings.HttpsGetEnabled, metadataBehavior.HttpsGetEnabled);
                if (null != metadataBehavior.HttpsGetUrl)
                {
                    instance.SetProperty(AdministrationStrings.HttpsGetUrl, metadataBehavior.HttpsGetUrl.ToString());
                }
                FillMetadataExporterInfo(instance, metadataBehavior.MetadataExporter);
            }
            else if (behavior is ServiceThrottlingBehavior)
            {
                instance = existingInstance.NewInstance("ServiceThrottlingBehavior");
                ServiceThrottlingBehavior throttlingBehavior = (ServiceThrottlingBehavior)behavior;
                instance.SetProperty(AdministrationStrings.MaxConcurrentCalls, throttlingBehavior.MaxConcurrentCalls);
                instance.SetProperty(AdministrationStrings.MaxConcurrentSessions, throttlingBehavior.MaxConcurrentSessions);
                instance.SetProperty(AdministrationStrings.MaxConcurrentInstances, throttlingBehavior.MaxConcurrentInstances);
            }
            else if (behavior is ServiceTimeoutsBehavior)
            {
                instance = existingInstance.NewInstance("ServiceTimeoutsBehavior");
                ServiceTimeoutsBehavior specificBehavior = (ServiceTimeoutsBehavior)behavior;
                instance.SetProperty(AdministrationStrings.TransactionTimeout, specificBehavior.TransactionTimeout);
            }
            else if (behavior is IWmiInstanceProvider)
            {
                IWmiInstanceProvider instanceProvider = (IWmiInstanceProvider)behavior;
                instance = existingInstance.NewInstance(instanceProvider.GetInstanceType());
                instanceProvider.FillInstance(instance);
            }
            else
            {
                instance = existingInstance.NewInstance("Behavior");
            }
            if (null != instance)
            {
                instance.SetProperty(AdministrationStrings.Type, behavior.GetType().FullName);
            }
        }

        void FillMetadataExporterInfo(IWmiInstance instance, MetadataExporter exporter)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi, (uint)System.Runtime.Diagnostics.EventLogEventId.MessageLoggingOn, true, "metadata exporter called");
            Fx.Assert(null != instance, "");
            Fx.Assert(null != exporter, "");
            IWmiInstance metadataExporterInstance = instance.NewInstance(AdministrationStrings.MetadataExporter);
            metadataExporterInstance.SetProperty(AdministrationStrings.PolicyVersion, exporter.PolicyVersion.ToString());
            instance.SetProperty(AdministrationStrings.MetadataExportInfo, metadataExporterInstance);
        }


        void FillChannelInfo(IChannel channel, IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            Fx.Assert(null != channel, "");
            instance.SetProperty(AdministrationStrings.Type, channel.GetType().ToString());

            ServiceChannel serviceChannel = ServiceChannelFactory.GetServiceChannel(channel);
            if (null != serviceChannel)
            {
                string uri = serviceChannel.RemoteAddress == null ? String.Empty : serviceChannel.RemoteAddress.ToString();
                instance.SetProperty(AdministrationStrings.RemoteAddress, uri);
                string contractName = null != serviceChannel.ClientRuntime ? serviceChannel.ClientRuntime.ContractName : String.Empty;
                string remoteEndpoint = EndpointInstanceProvider.EndpointReference(uri, contractName, false);
                instance.SetProperty(AdministrationStrings.RemoteEndpoint, remoteEndpoint);
                instance.SetProperty(AdministrationStrings.LocalAddress, serviceChannel.LocalAddress == null ? String.Empty : serviceChannel.LocalAddress.ToString());
                instance.SetProperty(AdministrationStrings.SessionId, ((IContextChannel)serviceChannel).SessionId);
            }
        }
    }
}
