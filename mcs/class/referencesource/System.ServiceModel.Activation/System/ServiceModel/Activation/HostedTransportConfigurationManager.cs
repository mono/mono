//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activation.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Diagnostics.CodeAnalysis;    

    class HostedTransportConfigurationManager
    {
        IDictionary<string, HostedTransportConfiguration> configurations = new Dictionary<string, HostedTransportConfiguration>(StringComparer.Ordinal);

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool initialized = false;
        MetabaseSettings metabaseSettings;

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile HostedTransportConfigurationManager singleton;
        static object syncRoot = new object();
  #pragma warning disable 436
        const string WasHostingAssemblyName = "System.ServiceModel.WasHosting, Version=" + ThisAssembly.Version + ", Culture=neutral, PublicKeyToken=" + AssemblyRef.EcmaPublicKey;
        const string MetabaseSettingsIis7FactoryTypeName = "System.ServiceModel.WasHosting.MetabaseSettingsIis7Factory, " + WasHostingAssemblyName;
  #pragma warning restore 436
        const string CreateMetabaseSettingsIis7MethodName = "CreateMetabaseSettings";

        HostedTransportConfigurationManager()
        {
            if (!Iis7Helper.IsIis7)
            {
                metabaseSettings = new MetabaseSettingsIis6();
            }
            else
            {

                metabaseSettings = CreateWasHostingMetabaseSettings();
            }
        }

        HostedTransportConfigurationManager(MetabaseSettings metabaseSettings)
        {
            this.metabaseSettings = metabaseSettings;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical methods CreateMetabaseSettings.",
            Safe = "Ensures that only the correct, well-known method is called to construct the metabase settings. No other " +
            "details are leaked, and no control flow or data is allowed in.")]
        [SecuritySafeCritical]
        static MetabaseSettingsIis CreateWasHostingMetabaseSettings()
        {
            Type type = Type.GetType(MetabaseSettingsIis7FactoryTypeName, false);
            if (type == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_MetabaseSettingsIis7TypeNotFound(MetabaseSettingsIis7FactoryTypeName, WasHostingAssemblyName)));
            }                             
            return CreateMetabaseSettings(type);
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts, Justification = "This is a private SecurityCritical method and its only caller passes in non-user data. Users cannot pass arbitrary data to this code.")]
        [Fx.Tag.SecurityNote(Critical = "Asserts full trust in order to call a well-known internal static in WasHosting.dll." +
            "Caller must ensure that 'type' argument refers to the trusted, well-known Type.")]
        [SecurityCritical]
        static MetabaseSettingsIis CreateMetabaseSettings(Type type)
        {
            object instance = null;
            MethodInfo method = type.GetMethod(CreateMetabaseSettingsIis7MethodName, BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                new PermissionSet(PermissionState.Unrestricted).Assert();

                instance = method.Invoke(null, null);
            }
            finally
            {
                PermissionSet.RevertAssert();
            }

            if (!(instance is MetabaseSettingsIis))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_BadMetabaseSettingsIis7Type(type.AssemblyQualifiedName)));
            }

            return (MetabaseSettingsIis)instance;
        }

        internal static void EnsureInitializedForSimpleApplicationHost(HostedHttpRequestAsyncResult result)
        {
            if (singleton != null)
            {
                return;
            }

            lock (syncRoot)
            {
                if (singleton != null)
                    return;

                singleton = new HostedTransportConfigurationManager(new MetabaseSettingsCassini(result));
            }
        }

        internal static MetabaseSettings MetabaseSettings
        {
            get
            {
                return HostedTransportConfigurationManager.Value.metabaseSettings;
            }
        }

        object ThisLock
        {
            get
            {
                return this;
            }
        }

        static HostedTransportConfigurationManager Value
        {
            get
            {
                if (singleton == null)
                {
                    lock (syncRoot)
                    {
                        if (singleton == null)
                        {
                            //Ensure ETW tracing is initialized
                            System.ServiceModel.Diagnostics.TraceUtility.SetEtwProviderId();

                            if (TD.HostedTransportConfigurationManagerConfigInitStartIsEnabled())
                            {
                                TD.HostedTransportConfigurationManagerConfigInitStart();
                            }
                            ServiceHostingEnvironment.EnsureInitialized();
                            singleton = new HostedTransportConfigurationManager();
                            if (TD.HostedTransportConfigurationManagerConfigInitStopIsEnabled())
                            {
                                TD.HostedTransportConfigurationManagerConfigInitStop();
                            }
                        }
                    }
                }
                return singleton;
            }
        }

        void EnsureInitialized()
        {
            if (!initialized)
            {
                lock (ThisLock)
                {
                    if (!initialized)
                    {
                        // Register protocols.
                        foreach (string protocol in metabaseSettings.GetProtocols())
                        {
                            // special case HTTP, it's a legacy protocol
                            if (string.CompareOrdinal(protocol, Uri.UriSchemeHttp) == 0 ||
                                string.CompareOrdinal(protocol, Uri.UriSchemeHttps) == 0)
                            {
                                HttpHostedTransportConfiguration httpConfiguration = null;
                                if (string.CompareOrdinal(protocol, Uri.UriSchemeHttp) == 0)
                                {
                                    httpConfiguration = new HttpHostedTransportConfiguration();
                                }
                                else
                                {
                                    httpConfiguration = new HttpsHostedTransportConfiguration();
                                }

                                configurations.Add(protocol, httpConfiguration);
                            }
                            else
                            {
                                if (!Iis7Helper.IsIis7)
                                {
                                    throw Fx.AssertAndThrowFatal("HostedTransportConfigurationManager.EnsureInitialized() protocols other than http and https can only be configured in IIS7");
                                }
                                if (AspNetPartialTrustHelpers.NeedPartialTrustInvoke)
                                {
                                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.PartialTrustNonHttpActivation(protocol, HostingEnvironmentWrapper.ApplicationVirtualPath)));
                                }
                                AddHostedTransportConfigurationIis7(protocol);
                            }
                        }

                        initialized = true;
                    }
                }
            }
        }

        void AddHostedTransportConfigurationIis7(string protocol)
        {
            HostedTransportConfiguration configuration = null;
            try
            {
                ServiceHostingEnvironmentSection section = ServiceHostingEnvironmentSection.GetSection();
                if (section.TransportConfigurationTypes.ContainsKey(protocol))
                {
                    TransportConfigurationTypeElement element = section.TransportConfigurationTypes[protocol];
                    Debug.Print("HostedTransportConfigurationManager.AddHostedTransportConfigurationIis7() found TransportConfigurationTypes for protocol: " + protocol + " name: " + element.TransportConfigurationType);

                    Type type = Type.GetType(element.TransportConfigurationType);
                    configuration = Activator.CreateInstance(type) as HostedTransportConfiguration;
                    configurations.Add(protocol, configuration);
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_ProtocolNoConfiguration(protocol)));
                }
            }
            catch (Exception exception)
            {
                if (!Fx.IsFatal(exception))
                {
                    Debug.Print("HostedTransportConfigurationManager.AddHostedTransportConfigurationIis7() caught exception: " + exception);
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.WebHostProtocolMisconfigured, SR.TraceCodeWebHostProtocolMisconfigured,
                            new StringTraceRecord("Protocol", protocol),
                            this, exception);
                    }
                }
                throw;
            }
        }

        internal static Uri[] GetBaseAddresses(string virtualPath)
        {
            return HostedTransportConfigurationManager.Value.InternalGetBaseAddresses(virtualPath);
        }

        internal static HostedTransportConfiguration GetConfiguration(string scheme)
        {
            return HostedTransportConfigurationManager.Value.InternalGetConfiguration(scheme);
        }

        Uri[] InternalGetBaseAddresses(string virtualPath)
        {
            EnsureInitialized();
            List<Uri> baseAddresses = new List<Uri>();
            foreach (HostedTransportConfiguration configuration in configurations.Values)
            {
                baseAddresses.AddRange(configuration.GetBaseAddresses(virtualPath));
            }

            return baseAddresses.ToArray();
        }

        HostedTransportConfiguration InternalGetConfiguration(string scheme)
        {
            EnsureInitialized();
            if (!configurations.ContainsKey(scheme))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_NotSupportedProtocol(scheme)));
            }

            return configurations[scheme];
        }
    }
}
