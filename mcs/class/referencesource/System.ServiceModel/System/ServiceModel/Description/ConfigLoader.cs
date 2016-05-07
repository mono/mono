//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Runtime.CompilerServices;

    class ConfigLoader
    {
        //resolvedBindings will be initialized to null on all threads
        //ThreadStatic gives each thread own copy of object
        [ThreadStatic]
        static List<string> resolvedBindings;

        //resolvedEndpoints will be initialized to null on all threads
        //ThreadStatic gives each thread own copy of object
        [ThreadStatic]
        static List<string> resolvedEndpoints;

        static readonly object[] emptyObjectArray = new object[] { };
        static readonly Type[] emptyTypeArray = new Type[] { };

        Dictionary<string, Binding> bindingTable;
        IContractResolver contractResolver;
        ContextInformation configurationContext;

        public ConfigLoader()
            : this((IContractResolver)null)
        {
        }

        public ConfigLoader(ContextInformation configurationContext)
            : this((IContractResolver)null)
        {
            this.configurationContext = configurationContext;
        }

        public ConfigLoader(IContractResolver contractResolver)
        {
            this.contractResolver = contractResolver;
            this.bindingTable = new Dictionary<string, Binding>();
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static EndpointIdentity LoadIdentity(IdentityElement element)
        {
            EndpointIdentity identity = null;

            PropertyInformationCollection properties = element.ElementInformation.Properties;
            if (properties[ConfigurationStrings.UserPrincipalName].ValueOrigin != PropertyValueOrigin.Default)
            {
                identity = EndpointIdentity.CreateUpnIdentity(element.UserPrincipalName.Value);
            }
            else if (properties[ConfigurationStrings.ServicePrincipalName].ValueOrigin != PropertyValueOrigin.Default)
            {
                identity = EndpointIdentity.CreateSpnIdentity(element.ServicePrincipalName.Value);
            }
            else if (properties[ConfigurationStrings.Dns].ValueOrigin != PropertyValueOrigin.Default)
            {
                identity = EndpointIdentity.CreateDnsIdentity(element.Dns.Value);
            }
            else if (properties[ConfigurationStrings.Rsa].ValueOrigin != PropertyValueOrigin.Default)
            {
                identity = EndpointIdentity.CreateRsaIdentity(element.Rsa.Value);
            }
            else if (properties[ConfigurationStrings.Certificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                X509Certificate2Collection collection = new X509Certificate2Collection();
                collection.Import(Convert.FromBase64String(element.Certificate.EncodedValue));

                if (collection.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnableToLoadCertificateIdentity)));
                }

                // We assume the first certificate in the list is the primary 
                // certificate.
                X509Certificate2 primaryCert = collection[0];
                collection.RemoveAt(0);
                identity = EndpointIdentity.CreateX509CertificateIdentity(primaryCert, collection);
            }
            else if (properties[ConfigurationStrings.CertificateReference].ValueOrigin != PropertyValueOrigin.Default)
            {
                X509CertificateStore store = new X509CertificateStore(element.CertificateReference.StoreName, element.CertificateReference.StoreLocation);
                X509Certificate2Collection collection = null;
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    collection = store.Find(element.CertificateReference.X509FindType, element.CertificateReference.FindValue, false);

                    if (collection.Count == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnableToLoadCertificateIdentity)));
                    }

                    // Just select the first certificate.
                    X509Certificate2 primaryCert = new X509Certificate2(collection[0]);
                    if (element.CertificateReference.IsChainIncluded)
                    {
                        // Build the chain.
                        X509Chain chain = new X509Chain();
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                        chain.Build(primaryCert);

                        identity = EndpointIdentity.CreateX509CertificateIdentity(chain);
                    }
                    else
                    {
                        identity = EndpointIdentity.CreateX509CertificateIdentity(primaryCert);
                    }
                }
                finally
                {
                    SecurityUtils.ResetAllCertificates(collection);
                    store.Close();
                }
            }

            return identity;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal void LoadChannelBehaviors(ServiceEndpoint serviceEndpoint, string configurationName)
        {
            ServiceEndpoint standardEndpoint;
            bool wildcard = IsWildcardMatch(configurationName);
            ChannelEndpointElement channelElement = LookupChannel(this.configurationContext, configurationName, serviceEndpoint.Contract, null, wildcard, false, out standardEndpoint);
            if (channelElement == null)
            {
                if (wildcard)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxConfigContractNotFound, serviceEndpoint.Contract.ConfigurationName)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxConfigChannelConfigurationNotFound, configurationName, serviceEndpoint.Contract.ConfigurationName)));
                }
            }
            if (serviceEndpoint.Binding == null && !string.IsNullOrEmpty(channelElement.Binding))
            {
                serviceEndpoint.Binding = ConfigLoader.LookupBinding(channelElement.Binding, channelElement.BindingConfiguration, ConfigurationHelpers.GetEvaluationContext(channelElement));
            }

            if (serviceEndpoint.Address == null && channelElement.Address != null && channelElement.Address.OriginalString.Length > 0)
            {
                serviceEndpoint.Address = new EndpointAddress(channelElement.Address, LoadIdentity(channelElement.Identity), channelElement.Headers.Headers);
            }

            CommonBehaviorsSection commonBehaviors = ConfigLoader.LookupCommonBehaviors(ConfigurationHelpers.GetEvaluationContext(channelElement));
            if (commonBehaviors != null && commonBehaviors.EndpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(commonBehaviors.EndpointBehaviors, serviceEndpoint.Behaviors, true/*commonBehaviors*/);
            }

            EndpointBehaviorElement behaviorElement = ConfigLoader.LookupEndpointBehaviors(channelElement.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(channelElement));
            if (behaviorElement != null)
            {
                LoadBehaviors<IEndpointBehavior>(behaviorElement, serviceEndpoint.Behaviors, false/*commonBehaviors*/);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal void LoadCommonClientBehaviors(ServiceEndpoint serviceEndpoint)
        {
            // just load commonBehaviors
            CommonBehaviorsSection commonBehaviors = ConfigLoader.LookupCommonBehaviors(this.configurationContext);
            if (commonBehaviors != null && commonBehaviors.EndpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(commonBehaviors.EndpointBehaviors, serviceEndpoint.Behaviors, true/*commonBehaviors*/);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        static void LoadBehaviors<T>(ServiceModelExtensionCollectionElement<BehaviorExtensionElement> behaviorElement, KeyedByTypeCollection<T> behaviors, bool commonBehaviors)
        {
            Nullable<bool> isPT = new Nullable<bool>();

            KeyedByTypeCollection<T> tempBehaviors = new KeyedByTypeCollection<T>();
            for (int i = 0; i < behaviorElement.Count; i++)
            {
                BehaviorExtensionElement behaviorExtension = behaviorElement[i];

                object behaviorObject = behaviorExtension.CreateBehavior();
                if (behaviorObject == null)
                {
                    continue;
                }

                Type type = behaviorObject.GetType();
                if (!typeof(T).IsAssignableFrom(type))
                {
                    TraceBehaviorWarning(behaviorExtension, TraceCode.SkipBehavior, SR.GetString(SR.TraceCodeSkipBehavior), type, typeof(T));
                    continue;
                }

                if (commonBehaviors)
                {
                    if (ShouldSkipCommonBehavior(type, ref isPT))
                    {
                        TraceBehaviorWarning(behaviorExtension, TraceCode.SkipBehavior, SR.GetString(SR.TraceCodeSkipBehavior), type, typeof(T));
                        continue;
                    }
                }

                // if, at this scope, we try to add same type of behavior twice, throw
                tempBehaviors.Add((T)behaviorObject);
                // but if the same type of behavior was present from an old scope, just remove the old one
                if (behaviors.Contains(type))
                {
                    TraceBehaviorWarning(behaviorExtension, TraceCode.RemoveBehavior, SR.GetString(SR.TraceCodeRemoveBehavior), type, typeof(T));
                    behaviors.Remove(type);
                }
                behaviors.Add((T)behaviorObject);
            }
        }

        // special processing for common behaviors:
        // if:
        //   1. the behavior type (returned from the config element) is in a signed, non-APTCA assembly
        //   2. the caller stack does not have ConfigurationPermission(Unrestricted)
        // .. exclude the behavior from the collection and trace a warning
        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical helpers, makes a security decision.")]
        [SecurityCritical]
        static bool ShouldSkipCommonBehavior(Type behaviorType, ref Nullable<bool> isPT)
        {
            bool skip = false;

            if (!isPT.HasValue)
            {
                if (!PartialTrustHelpers.IsTypeAptca(behaviorType))
                {
                    isPT = !ThreadHasConfigurationPermission();
                    skip = isPT.Value;
                }
            }
            else if (isPT.Value)
            {
                skip = !PartialTrustHelpers.IsTypeAptca(behaviorType);
            }

            return skip;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        static void TraceBehaviorWarning(BehaviorExtensionElement behaviorExtension, int traceCode, string traceDescription, Type type, Type behaviorType)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Hashtable h = new Hashtable(3)
                {
                    { "ConfigurationElementName", behaviorExtension.ConfigurationElementName },
                    { "ConfigurationType", type.AssemblyQualifiedName },
                    { "BehaviorType", behaviorType.AssemblyQualifiedName }
                };
                TraceUtility.TraceEvent(TraceEventType.Warning, traceCode, traceDescription,
                    new DictionaryTraceRecord(h), null, null);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        static void LoadChannelBehaviors(EndpointBehaviorElement behaviorElement, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
        {
            if (behaviorElement != null)
            {
                LoadBehaviors<IEndpointBehavior>(behaviorElement, channelBehaviors, false/*commonBehaviors*/
                                                                                                                               );
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static void LoadChannelBehaviors(string behaviorName, ContextInformation context, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
        {
            LoadChannelBehaviors(
                LookupEndpointBehaviors(behaviorName, context),
                channelBehaviors
            );
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static Collection<IWsdlImportExtension> LoadWsdlImporters(WsdlImporterElementCollection wsdlImporterElements, ContextInformation context)
        {
            Collection<IWsdlImportExtension> wsdlImporters = new Collection<IWsdlImportExtension>();

            foreach (WsdlImporterElement wsdlImporterElement in wsdlImporterElements)
            {
                // Verify that the type implements IWsdlImporter
                Type wsdlImporterType = Type.GetType(wsdlImporterElement.Type, true, true);
                if (!typeof(IWsdlImportExtension).IsAssignableFrom(wsdlImporterType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidWsdlExtensionTypeInConfig, wsdlImporterType.AssemblyQualifiedName)));
                }

                // Verify that the type has a default constructor
                ConstructorInfo constructorInfo = wsdlImporterType.GetConstructor(emptyTypeArray);
                if (constructorInfo == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.WsdlExtensionTypeRequiresDefaultConstructor, wsdlImporterType.AssemblyQualifiedName)));
                }

                wsdlImporters.Add((IWsdlImportExtension)constructorInfo.Invoke(emptyObjectArray));
            }

            return wsdlImporters;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static Collection<IPolicyImportExtension> LoadPolicyImporters(PolicyImporterElementCollection policyImporterElements, ContextInformation context)
        {
            Collection<IPolicyImportExtension> policyImporters = new Collection<IPolicyImportExtension>();

            foreach (PolicyImporterElement policyImporterElement in policyImporterElements)
            {
                // Verify that the type implements IPolicyImporter
                Type policyImporterType = Type.GetType(policyImporterElement.Type, true, true);
                if (!typeof(IPolicyImportExtension).IsAssignableFrom(policyImporterType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidPolicyExtensionTypeInConfig, policyImporterType.AssemblyQualifiedName)));
                }

                // Verify that the type has a default constructor
                ConstructorInfo constructorInfo = policyImporterType.GetConstructor(emptyTypeArray);
                if (constructorInfo == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PolicyExtensionTypeRequiresDefaultConstructor, policyImporterType.AssemblyQualifiedName)));
                }

                policyImporters.Add((IPolicyImportExtension)constructorInfo.Invoke(emptyObjectArray));
            }

            return policyImporters;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static EndpointAddress LoadEndpointAddress(EndpointAddressElementBase element)
        {
            return new EndpointAddress(element.Address, LoadIdentity(element.Identity), element.Headers.Headers);
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        public void LoadHostConfig(ServiceElement serviceElement, ServiceHostBase host, System.Action<Uri> addBaseAddress)
        {
            HostElement hostElement = serviceElement.Host;
            if (hostElement != null)
            {
                if (!AspNetEnvironment.Enabled)
                {
                    foreach (BaseAddressElement bae in hostElement.BaseAddresses)
                    {
                        string cookedAddress = null;
                        string rawAddress = bae.BaseAddress;
                        int colonIndex = rawAddress.IndexOf(':');
                        if (colonIndex != -1 && rawAddress.Length >= colonIndex + 4)
                        {
                            if (rawAddress[colonIndex + 1] == '/' &&
                                rawAddress[colonIndex + 2] == '/' &&
                                rawAddress[colonIndex + 3] == '*')
                            {
                                string beforeAsterisk = rawAddress.Substring(0, colonIndex + 3);
                                string rest = rawAddress.Substring(colonIndex + 4);
                                StringBuilder sb = new StringBuilder(beforeAsterisk);
                                sb.Append(System.Net.Dns.GetHostName());
                                sb.Append(rest);
                                cookedAddress = sb.ToString();
                            }
                        }
                        if (cookedAddress == null)
                        {
                            cookedAddress = rawAddress;
                        }
                        Uri uri;
                        if (!Uri.TryCreate(cookedAddress, UriKind.Absolute, out uri))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.BaseAddressMustBeAbsolute)));
                        }
                        addBaseAddress(uri);
                    }
                }
                HostTimeoutsElement hte = hostElement.Timeouts;
                if (hte != null)
                {
                    if (hte.OpenTimeout != TimeSpan.Zero)
                    {
                        host.OpenTimeout = hte.OpenTimeout;
                    }
                    if (hte.CloseTimeout != TimeSpan.Zero)
                    {
                        host.CloseTimeout = hte.CloseTimeout;
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        public void LoadServiceDescription(ServiceHostBase host, ServiceDescription description, ServiceElement serviceElement, System.Action<Uri> addBaseAddress, bool skipHost = false)
        {
            CommonBehaviorsSection commonBehaviors = ConfigLoader.LookupCommonBehaviors(
                serviceElement == null ? null : ConfigurationHelpers.GetEvaluationContext(serviceElement));
            if (commonBehaviors != null && commonBehaviors.ServiceBehaviors != null)
            {
                LoadBehaviors<IServiceBehavior>(commonBehaviors.ServiceBehaviors, description.Behaviors, true/*commonBehaviors*/);
            }

            string behaviorConfigurationName = ConfigurationStrings.DefaultName;
            if (serviceElement != null)
            {
                if (!skipHost)
                {
                    this.LoadHostConfig(serviceElement, host, addBaseAddress);
                }
                behaviorConfigurationName = serviceElement.BehaviorConfiguration;
            }
            ServiceBehaviorElement behaviorElement = ConfigLoader.LookupServiceBehaviors(behaviorConfigurationName, ConfigurationHelpers.GetEvaluationContext(serviceElement));
            if (behaviorElement != null)
            {
                LoadBehaviors<IServiceBehavior>(behaviorElement, description.Behaviors, false/*commonBehaviors*/);
            }

            ServiceHostBase.ServiceAndBehaviorsContractResolver resolver = this.contractResolver as ServiceHostBase.ServiceAndBehaviorsContractResolver;
            if (resolver != null)
            {
                resolver.AddBehaviorContractsToResolver(description.Behaviors);
            }

            if (serviceElement != null)
            {
                foreach (ServiceEndpointElement endpointElement in serviceElement.Endpoints)
                {
                    if (String.IsNullOrEmpty(endpointElement.Kind))
                    {
                        ContractDescription contract = LookupContract(endpointElement.Contract, description.Name);

                        // binding
                        Binding binding;
                        string bindingKey = endpointElement.Binding + ":" + endpointElement.BindingConfiguration;
                        if (bindingTable.TryGetValue(bindingKey, out binding) == false)
                        {
                            binding = ConfigLoader.LookupBinding(endpointElement.Binding, endpointElement.BindingConfiguration, ConfigurationHelpers.GetEvaluationContext(serviceElement));
                            bindingTable.Add(bindingKey, binding);
                        }

                        if (!string.IsNullOrEmpty(endpointElement.BindingName))
                        {
                            binding.Name = endpointElement.BindingName;
                        }
                        if (!string.IsNullOrEmpty(endpointElement.BindingNamespace))
                        {
                            binding.Namespace = endpointElement.BindingNamespace;
                        }

                        // address
                        Uri address = endpointElement.Address;

                        ServiceEndpoint serviceEndpoint;
                        if (null == address)
                        {
                            serviceEndpoint = new ServiceEndpoint(contract);
                            serviceEndpoint.Binding = binding;
                        }
                        else
                        {
                            Uri via = ServiceHost.MakeAbsoluteUri(address, binding, host.InternalBaseAddresses);
                            serviceEndpoint = new ServiceEndpoint(contract, binding, new EndpointAddress(via, LoadIdentity(endpointElement.Identity), endpointElement.Headers.Headers));
                            serviceEndpoint.UnresolvedAddress = endpointElement.Address;
                        }
                        if (endpointElement.ListenUri != null)
                        {
                            serviceEndpoint.ListenUri = ServiceHost.MakeAbsoluteUri(endpointElement.ListenUri, binding, host.InternalBaseAddresses);
                            serviceEndpoint.UnresolvedListenUri = endpointElement.ListenUri;
                        }
                        serviceEndpoint.ListenUriMode = endpointElement.ListenUriMode;

                        if (!string.IsNullOrEmpty(endpointElement.Name))
                        {
                            serviceEndpoint.Name = endpointElement.Name;
                        }

                        KeyedByTypeCollection<IEndpointBehavior> behaviors = serviceEndpoint.Behaviors;

                        EndpointBehaviorElement behaviorEndpointElement = ConfigLoader.LookupEndpointBehaviors(endpointElement.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(endpointElement));
                        if (behaviorEndpointElement != null)
                        {
                            LoadBehaviors<IEndpointBehavior>(behaviorEndpointElement, behaviors, false/*commonBehaviors*/);
                        }

                        if (endpointElement.ElementInformation.Properties[ConfigurationStrings.IsSystemEndpoint].ValueOrigin != PropertyValueOrigin.Default)
                        {
                            serviceEndpoint.IsSystemEndpoint = endpointElement.IsSystemEndpoint;
                        }

                        description.Endpoints.Add(serviceEndpoint);
                    }
                    else
                    {
                        ServiceEndpoint endpoint = LookupEndpoint(endpointElement, ConfigurationHelpers.GetEvaluationContext(serviceElement), host, description);
                        description.Endpoints.Add(endpoint);
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        public static void LoadDefaultEndpointBehaviors(ServiceEndpoint endpoint)
        {
            EndpointBehaviorElement behaviorEndpointElement = ConfigLoader.LookupEndpointBehaviors(ConfigurationStrings.DefaultName, ConfigurationHelpers.GetEvaluationContext(null));
            if (behaviorEndpointElement != null)
            {
                LoadBehaviors<IEndpointBehavior>(behaviorEndpointElement, endpoint.Behaviors, false);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.")]
        [SecurityCritical]
        static EndpointCollectionElement LookupEndpointCollectionElement(string endpointSectionName, ContextInformation context)
        {
            if (string.IsNullOrEmpty(endpointSectionName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigEndpointTypeCannotBeNullOrEmpty)));
            }
            EndpointCollectionElement endpointCollectionElement = null;
            if (context == null)
            {
                // If no context is passed in, assume that the caller can consume the AppDomain's 
                // current configuration file.
                endpointCollectionElement = (EndpointCollectionElement)ConfigurationHelpers.UnsafeGetEndpointCollectionElement(endpointSectionName);
            }
            else
            {
                // Use the configuration file associated with the passed in context.
                // This may or may not be the same as the file for the current AppDomain.
                endpointCollectionElement = (EndpointCollectionElement)ConfigurationHelpers.UnsafeGetAssociatedEndpointCollectionElement(context, endpointSectionName);
            }

            return endpointCollectionElement;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static ServiceEndpoint LookupEndpoint(string configurationName, EndpointAddress address, ContractDescription contract)
        {
            return LookupEndpoint(configurationName, address, contract, null);
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static ServiceEndpoint LookupEndpoint(string configurationName, EndpointAddress address, ContractDescription contract, ContextInformation configurationContext)
        {
            bool wildcard = IsWildcardMatch(configurationName);
            ServiceEndpoint serviceEndpoint;
            LookupChannel(configurationContext, configurationName, contract, address, wildcard, true, out serviceEndpoint);
            return serviceEndpoint;
        }

        internal static ServiceEndpoint LookupEndpoint(ChannelEndpointElement channelEndpointElement, ContextInformation context)
        {
            return LookupEndpoint(channelEndpointElement, context, null /*address*/, null /*contractDescription*/);
        }

        // This method should only return null when endpointConfiguration is specified on the ChannelEndpointElement and no ChannelEndpointElement matching the 
        // endpointConfiguration name is found.  All other error conditions should throw.
        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        static ServiceEndpoint LookupEndpoint(ChannelEndpointElement channelEndpointElement, ContextInformation context, EndpointAddress address, ContractDescription contract)
        {
            EndpointCollectionElement endpointCollectionElement = LookupEndpointCollectionElement(channelEndpointElement.Kind, context);
            ServiceEndpoint retval = null;

            string endpointConfiguration = channelEndpointElement.EndpointConfiguration ?? String.Empty;

            // We are looking for a specific instance, not the default. 
            bool configuredEndpointFound = false;
            // The Endpoints property is always public
            foreach (StandardEndpointElement standardEndpointElement in endpointCollectionElement.ConfiguredEndpoints)
            {
                if (standardEndpointElement.Name.Equals(endpointConfiguration, StringComparison.Ordinal))
                {
                    if (null == ConfigLoader.resolvedEndpoints)
                    {
                        ConfigLoader.resolvedEndpoints = new List<string>();
                    }

                    string resolvedEndpointID = channelEndpointElement.Kind + "/" + endpointConfiguration;
                    if (ConfigLoader.resolvedEndpoints.Contains(resolvedEndpointID))
                    {
                        ConfigurationElement configErrorElement = (ConfigurationElement)standardEndpointElement;
                        System.Text.StringBuilder detectedCycle = new System.Text.StringBuilder();
                        foreach (string resolvedEndpoint in ConfigLoader.resolvedEndpoints)
                        {
                            detectedCycle = detectedCycle.AppendFormat("{0}, ", resolvedEndpoint);
                        }

                        detectedCycle = detectedCycle.Append(resolvedEndpointID);

                        // Clear list in case application is written to handle exception
                        // by not starting up channel, etc...
                        ConfigLoader.resolvedEndpoints = null;

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigEndpointReferenceCycleDetected, detectedCycle.ToString()),
                            configErrorElement.ElementInformation.Source,
                            configErrorElement.ElementInformation.LineNumber));
                    }

                    try
                    {
                        CheckAccess(standardEndpointElement as IConfigurationContextProviderInternal);
                        ConfigLoader.resolvedEndpoints.Add(resolvedEndpointID);
                        ConfigureEndpoint(standardEndpointElement, channelEndpointElement, address, context, contract, out retval);
                        ConfigLoader.resolvedEndpoints.Remove(resolvedEndpointID);
                    }
                    catch
                    {
                        // Clear list in case application is written to handle exception
                        // by not starting up channel, etc...
                        if (null != ConfigLoader.resolvedEndpoints)
                        {
                            ConfigLoader.resolvedBindings = null;
                        }
                        throw;
                    }

                    if (null != ConfigLoader.resolvedEndpoints &&
                        0 == ConfigLoader.resolvedEndpoints.Count)
                    {
                        ConfigLoader.resolvedEndpoints = null;
                    }

                    configuredEndpointFound = true;
                }
            }
            if (!configuredEndpointFound)
            {
                // We expected to find an instance, but didn't.
                // Return null. 
                retval = null;
            }

            if (retval == null && String.IsNullOrEmpty(endpointConfiguration))
            {
                StandardEndpointElement standardEndpointElement = endpointCollectionElement.GetDefaultStandardEndpointElement();
                ConfigureEndpoint(standardEndpointElement, channelEndpointElement, address, context, contract, out retval);
            }


            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, object> values = new Dictionary<string, object>(3);
                values["FoundEndpoint"] = retval != null;
                bool usingDefault = string.IsNullOrEmpty(endpointConfiguration);
                int traceCode;
                string traceDescription;
                if (usingDefault)
                {
                    traceCode = TraceCode.GetDefaultConfiguredEndpoint;
                    traceDescription = SR.GetString(SR.TraceCodeGetDefaultConfiguredEndpoint);
                }
                else
                {
                    traceCode = TraceCode.GetConfiguredEndpoint;
                    traceDescription = SR.GetString(SR.TraceCodeGetConfiguredEndpoint);
                    values["Name"] = endpointConfiguration;
                }
                values["Endpoint"] = channelEndpointElement.Kind;
                TraceUtility.TraceEvent(TraceEventType.Verbose, traceCode, traceDescription,
                    new DictionaryTraceRecord(values), null, null);
            }

            if (retval != null)
            {
                retval.IsFullyConfigured = true;
            }
            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        static void ConfigureEndpoint(StandardEndpointElement standardEndpointElement, ChannelEndpointElement channelEndpointElement,
            EndpointAddress address, ContextInformation context, ContractDescription contract, out ServiceEndpoint endpoint)
        {
            // copy channelEndpointElement so that it can potentially be modified by the StandardEndpointElement
            // the properties collection of the instance seviceEndpointElement created by System.Configuration is read-only.
            // keeping original serviceEndpointElement so that its context can be used for the lookups.
            ChannelEndpointElement channelEndpointElementCopy = new ChannelEndpointElement();
            channelEndpointElementCopy.Copy(channelEndpointElement);

            standardEndpointElement.InitializeAndValidate(channelEndpointElementCopy);
            endpoint = standardEndpointElement.CreateServiceEndpoint(contract);
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ConfigNoEndpointCreated, standardEndpointElement.GetType().AssemblyQualifiedName,
                    (standardEndpointElement.EndpointType == null) ? string.Empty : standardEndpointElement.EndpointType.AssemblyQualifiedName)));
            }

            //binding
            if (!string.IsNullOrEmpty(channelEndpointElementCopy.Binding))
            {
                endpoint.Binding = ConfigLoader.LookupBinding(channelEndpointElementCopy.Binding, channelEndpointElementCopy.BindingConfiguration, ConfigurationHelpers.GetEvaluationContext(channelEndpointElement));
            }

            //name
            if (!string.IsNullOrEmpty(channelEndpointElementCopy.Name))
            {
                endpoint.Name = channelEndpointElementCopy.Name;
            }

            //address
            if (address != null)
            {
                endpoint.Address = address;
            }
            if (endpoint.Address == null && channelEndpointElementCopy.Address != null && channelEndpointElementCopy.Address.OriginalString.Length > 0)
            {
                endpoint.Address = new EndpointAddress(channelEndpointElementCopy.Address, LoadIdentity(channelEndpointElementCopy.Identity), channelEndpointElementCopy.Headers.Headers);
            }

            //behaviors
            CommonBehaviorsSection commonBehaviors = ConfigLoader.LookupCommonBehaviors(ConfigurationHelpers.GetEvaluationContext(channelEndpointElement));
            if (commonBehaviors != null && commonBehaviors.EndpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(commonBehaviors.EndpointBehaviors, endpoint.Behaviors, true/*commonBehaviors*/);
            }

            EndpointBehaviorElement behaviorElement = ConfigLoader.LookupEndpointBehaviors(channelEndpointElementCopy.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(channelEndpointElement));
            if (behaviorElement != null)
            {
                LoadBehaviors<IEndpointBehavior>(behaviorElement, endpoint.Behaviors, false/*commonBehaviors*/);
            }

            standardEndpointElement.ApplyConfiguration(endpoint, channelEndpointElementCopy);
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal ServiceEndpoint LookupEndpoint(ServiceEndpointElement serviceEndpointElement, ContextInformation context,
            ServiceHostBase host, ServiceDescription description, bool omitSettingEndpointAddress = false)
        {
            EndpointCollectionElement endpointCollectionElement = LookupEndpointCollectionElement(serviceEndpointElement.Kind, context);
            ServiceEndpoint retval = null;

            string endpointConfiguration = serviceEndpointElement.EndpointConfiguration ?? String.Empty;

            // We are looking for a specific instance, not the default. 
            bool configuredEndpointFound = false;
            // The Endpoints property is always public
            foreach (StandardEndpointElement standardEndpointElement in endpointCollectionElement.ConfiguredEndpoints)
            {
                if (standardEndpointElement.Name.Equals(endpointConfiguration, StringComparison.Ordinal))
                {
                    if (null == ConfigLoader.resolvedEndpoints)
                    {
                        ConfigLoader.resolvedEndpoints = new List<string>();
                    }

                    string resolvedEndpointID = serviceEndpointElement.Kind + "/" + endpointConfiguration;
                    if (ConfigLoader.resolvedEndpoints.Contains(resolvedEndpointID))
                    {
                        ConfigurationElement configErrorElement = (ConfigurationElement)standardEndpointElement;
                        System.Text.StringBuilder detectedCycle = new System.Text.StringBuilder();
                        foreach (string resolvedEndpoint in ConfigLoader.resolvedEndpoints)
                        {
                            detectedCycle = detectedCycle.AppendFormat("{0}, ", resolvedEndpoint);
                        }

                        detectedCycle = detectedCycle.Append(resolvedEndpointID);

                        // Clear list in case application is written to handle exception
                        // by not starting up channel, etc...
                        ConfigLoader.resolvedEndpoints = null;

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigEndpointReferenceCycleDetected, detectedCycle.ToString()),
                            configErrorElement.ElementInformation.Source,
                            configErrorElement.ElementInformation.LineNumber));
                    }

                    try
                    {
                        CheckAccess(standardEndpointElement as IConfigurationContextProviderInternal);
                        ConfigLoader.resolvedEndpoints.Add(resolvedEndpointID);
                        ConfigureEndpoint(standardEndpointElement, serviceEndpointElement, context, host, description, out retval);
                        ConfigLoader.resolvedEndpoints.Remove(resolvedEndpointID);
                    }
                    catch
                    {
                        // Clear list in case application is written to handle exception
                        // by not starting up channel, etc...
                        if (null != ConfigLoader.resolvedEndpoints)
                        {
                            ConfigLoader.resolvedBindings = null;
                        }
                        throw;
                    }

                    if (null != ConfigLoader.resolvedEndpoints &&
                        0 == ConfigLoader.resolvedEndpoints.Count)
                    {
                        ConfigLoader.resolvedEndpoints = null;
                    }

                    configuredEndpointFound = true;
                }
            }
            if (!configuredEndpointFound)
            {
                // We expected to find an instance, but didn't.
                // Return null. 
                retval = null;
            }

            if (retval == null && String.IsNullOrEmpty(endpointConfiguration))
            {
                StandardEndpointElement standardEndpointElement = endpointCollectionElement.GetDefaultStandardEndpointElement();
                ConfigureEndpoint(standardEndpointElement, serviceEndpointElement, context, host, description, out retval, omitSettingEndpointAddress);
            }

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, object> values = new Dictionary<string, object>(3);
                values["FoundEndpoint"] = retval != null;
                bool usingDefault = string.IsNullOrEmpty(endpointConfiguration);
                int traceCode;
                string traceDescription;
                if (usingDefault)
                {
                    traceCode = TraceCode.GetDefaultConfiguredEndpoint;
                    traceDescription = SR.GetString(SR.TraceCodeGetDefaultConfiguredEndpoint);
                }
                else
                {
                    traceCode = TraceCode.GetConfiguredEndpoint;
                    traceDescription = SR.GetString(SR.TraceCodeGetConfiguredEndpoint);
                    values["Name"] = endpointConfiguration;
                }
                values["Endpoint"] = serviceEndpointElement.Kind;
                TraceUtility.TraceEvent(TraceEventType.Verbose, traceCode, traceDescription,
                    new DictionaryTraceRecord(values), null, null);
            }

            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        void ConfigureEndpoint(StandardEndpointElement standardEndpointElement, ServiceEndpointElement serviceEndpointElement,
            ContextInformation context, ServiceHostBase host, ServiceDescription description, out ServiceEndpoint endpoint, bool omitSettingEndpointAddress = false)
        {
            // copy serviceEndpointElement so that it can potentially be modified by the StandardEndpointElement
            // the properties collection of the instance seviceEndpointElement created by System.Configuration is read-only.
            // keeping original serviceEndpointElement so that its context can be used to lookup endpoint behaviors.
            ServiceEndpointElement serviceEndpointElementCopy = new ServiceEndpointElement();
            serviceEndpointElementCopy.Copy(serviceEndpointElement);

            standardEndpointElement.InitializeAndValidate(serviceEndpointElementCopy);

            //contract
            ContractDescription contract = null;
            if (!string.IsNullOrEmpty(serviceEndpointElementCopy.Contract))
            {
                contract = LookupContractForStandardEndpoint(serviceEndpointElementCopy.Contract, description.Name);
            }

            endpoint = standardEndpointElement.CreateServiceEndpoint(contract);
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ConfigNoEndpointCreated, standardEndpointElement.GetType().AssemblyQualifiedName,
                    (standardEndpointElement.EndpointType == null) ? string.Empty : standardEndpointElement.EndpointType.AssemblyQualifiedName)));
            }

            //binding
            Binding binding = null;
            if (!string.IsNullOrEmpty(serviceEndpointElementCopy.Binding))
            {
                string bindingKey = serviceEndpointElementCopy.Binding + ":" + serviceEndpointElementCopy.BindingConfiguration;
                if (bindingTable.TryGetValue(bindingKey, out binding) == false)
                {
                    binding = ConfigLoader.LookupBinding(serviceEndpointElementCopy.Binding, serviceEndpointElementCopy.BindingConfiguration, context);
                    bindingTable.Add(bindingKey, binding);
                }
            }
            else
            {
                binding = endpoint.Binding;
            }

            if (binding != null)
            {
                if (!string.IsNullOrEmpty(serviceEndpointElementCopy.BindingName))
                {
                    binding.Name = serviceEndpointElementCopy.BindingName;
                }
                if (!string.IsNullOrEmpty(serviceEndpointElementCopy.BindingNamespace))
                {
                    binding.Namespace = serviceEndpointElementCopy.BindingNamespace;
                }
                endpoint.Binding = binding;

                if (!omitSettingEndpointAddress)
                {
                    ConfigureEndpointAddress(serviceEndpointElementCopy, host, endpoint);
                    ConfigureEndpointListenUri(serviceEndpointElementCopy, host, endpoint);
                }
            }

            //listenUriMode
            endpoint.ListenUriMode = serviceEndpointElementCopy.ListenUriMode;

            //name
            if (!string.IsNullOrEmpty(serviceEndpointElementCopy.Name))
            {
                endpoint.Name = serviceEndpointElementCopy.Name;
            }

            //behaviors
            KeyedByTypeCollection<IEndpointBehavior> behaviors = endpoint.Behaviors;

            EndpointBehaviorElement behaviorEndpointElement = ConfigLoader.LookupEndpointBehaviors(serviceEndpointElementCopy.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(serviceEndpointElement));
            if (behaviorEndpointElement != null)
            {
                LoadBehaviors<IEndpointBehavior>(behaviorEndpointElement, behaviors, false/*commonBehaviors*/);
            }

            //isSystemEndpoint
            if (serviceEndpointElementCopy.ElementInformation.Properties[ConfigurationStrings.IsSystemEndpoint].ValueOrigin != PropertyValueOrigin.Default)
            {
                endpoint.IsSystemEndpoint = serviceEndpointElementCopy.IsSystemEndpoint;
            }

            standardEndpointElement.ApplyConfiguration(endpoint, serviceEndpointElementCopy);
        }

        internal static void ConfigureEndpointAddress(ServiceEndpointElement serviceEndpointElement, ServiceHostBase host, ServiceEndpoint endpoint)
        {
            Fx.Assert(endpoint.Binding != null, "The endpoint must be set by the caller.");
            if (serviceEndpointElement.Address != null)
            {
                Uri via = ServiceHost.MakeAbsoluteUri(serviceEndpointElement.Address, endpoint.Binding, host.InternalBaseAddresses);
                endpoint.Address = new EndpointAddress(via, LoadIdentity(serviceEndpointElement.Identity), serviceEndpointElement.Headers.Headers);
                endpoint.UnresolvedAddress = serviceEndpointElement.Address;
            }
        }

        internal static void ConfigureEndpointListenUri(ServiceEndpointElement serviceEndpointElement, ServiceHostBase host, ServiceEndpoint endpoint)
        {
            Fx.Assert(endpoint.Binding != null, "The endpoint must be set by the caller.");
            if (serviceEndpointElement.ListenUri != null)
            {
                endpoint.ListenUri = ServiceHost.MakeAbsoluteUri(serviceEndpointElement.ListenUri, endpoint.Binding, host.InternalBaseAddresses);
                endpoint.UnresolvedListenUri = serviceEndpointElement.ListenUri;
            }
        }

        internal static Binding LookupBinding(string bindingSectionName, string configurationName)
        {
            return ConfigLoader.LookupBinding(bindingSectionName, configurationName, null);
        }

        internal static ComContractElement LookupComContract(Guid contractIID)
        {
            ComContractsSection comContracts = (ComContractsSection)ConfigurationHelpers.GetSection(ConfigurationStrings.ComContractsSectionPath);
            foreach (ComContractElement contract in comContracts.ComContracts)
            {
                Guid interfaceID;
                if (DiagnosticUtility.Utility.TryCreateGuid(contract.Contract, out interfaceID))
                {
                    if (interfaceID == contractIID)
                    {
                        return contract;
                    }
                }
            }
            return null;
        }

        /// <SecurityNote>
        /// Critical - handles config objects, which should not be leaked
        /// Safe - doesn't leak config objects out of SecurityCritical code
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal static ProtocolMappingItem LookupProtocolMapping(String scheme)
        {
            ProtocolMappingSection protocolMapping = (ProtocolMappingSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ProtocolMappingSectionPath);
            foreach (ProtocolMappingElement pm in protocolMapping.ProtocolMappingCollection)
            {
                if (pm.Scheme == scheme)
                {
                    return new ProtocolMappingItem(pm.Binding, pm.BindingConfiguration);
                }
            }
            return null;
        }

        [Fx.Tag.SecurityNote(Critical = "Leaks config objects, caller must ensure that these don't leak to user code.")]
        [SecurityCritical]
        static BindingCollectionElement GetBindingCollectionElement(string bindingSectionName, ContextInformation context)
        {
            if (string.IsNullOrEmpty(bindingSectionName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigBindingTypeCannotBeNullOrEmpty)));
            }
            if (context == null)
            {
                // If no context is passed in, assume that the caller can consume the AppDomain's 
                // current configuration file.
                return (BindingCollectionElement)ConfigurationHelpers.UnsafeGetBindingCollectionElement(bindingSectionName);
            }
            else
            {
                // Use the configuration file associated with the passed in context.
                // This may or may not be the same as the file for the current AppDomain.
                return (BindingCollectionElement)ConfigurationHelpers.UnsafeGetAssociatedBindingCollectionElement(context, bindingSectionName);
            }
        }

        // This method should only return null when bindingConfiguration is specified on the BindingElement and no BindingElement matching the 
        // bindingConfiguration name is found.  All other error conditions should throw.        
        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        internal static Binding LookupBinding(string bindingSectionName, string configurationName, ContextInformation context)
        {
            BindingCollectionElement bindingCollectionElement = GetBindingCollectionElement(bindingSectionName, context);
            Binding retval;
            if (configurationName == null)
            {
                retval = bindingCollectionElement.GetDefault();
            }
            else
            {
                Binding defaultBinding = bindingCollectionElement.GetDefault();
                retval = LookupBinding(bindingSectionName, configurationName, bindingCollectionElement, defaultBinding);
                if (retval == null && configurationName == ConfigurationStrings.DefaultName)
                {
                    retval = defaultBinding;
                }
            }

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, object> values = new Dictionary<string, object>(3);
                values["FoundBinding"] = retval != null;
                bool usingDefault = string.IsNullOrEmpty(configurationName);
                int traceCode;
                string traceDescription;
                if (usingDefault)
                {
                    traceCode = TraceCode.GetDefaultConfiguredBinding;
                    traceDescription = SR.GetString(SR.TraceCodeGetDefaultConfiguredBinding);
                }
                else
                {
                    traceCode = TraceCode.GetConfiguredBinding;
                    traceDescription = SR.GetString(SR.TraceCodeGetConfiguredBinding);
                    values["Name"] = string.IsNullOrEmpty(configurationName) ?
                        SR.GetString(SR.Default) : configurationName;
                }
                values["Binding"] = bindingSectionName;
                TraceUtility.TraceEvent(TraceEventType.Verbose, traceCode, traceDescription,
                    new DictionaryTraceRecord(values), null, null);
            }
            return retval;
        }

        static Binding LookupBinding(string bindingSectionName, string configurationName, BindingCollectionElement bindingCollectionElement, Binding defaultBinding)
        {
            Binding retval = defaultBinding;
            if (configurationName != null)
            {
                // We are looking for a specific instance, not the default. 
                bool configuredBindingFound = false;

                // The Bindings property is always public
                foreach (object configElement in bindingCollectionElement.ConfiguredBindings)
                {
                    IBindingConfigurationElement bindingElement = configElement as IBindingConfigurationElement;
                    if (bindingElement != null)
                    {
                        if (bindingElement.Name.Equals(configurationName, StringComparison.Ordinal))
                        {
                            if (null == ConfigLoader.resolvedBindings)
                            {
                                ConfigLoader.resolvedBindings = new List<string>();
                            }
                            string resolvedBindingID = bindingSectionName + "/" + configurationName;
                            if (ConfigLoader.resolvedBindings.Contains(resolvedBindingID))
                            {
                                ConfigurationElement configErrorElement = (ConfigurationElement)configElement;
                                System.Text.StringBuilder detectedCycle = new System.Text.StringBuilder();
                                foreach (string resolvedBinding in ConfigLoader.resolvedBindings)
                                {
                                    detectedCycle = detectedCycle.AppendFormat("{0}, ", resolvedBinding);
                                }

                                detectedCycle = detectedCycle.Append(resolvedBindingID);

                                // Clear list in case application is written to handle exception
                                // by not starting up channel, etc...
                                ConfigLoader.resolvedBindings = null;

                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigBindingReferenceCycleDetected, detectedCycle.ToString()),
                                    configErrorElement.ElementInformation.Source,
                                    configErrorElement.ElementInformation.LineNumber));
                            }

                            try
                            {
                                CheckAccess(configElement as IConfigurationContextProviderInternal);

                                ConfigLoader.resolvedBindings.Add(resolvedBindingID);
                                bindingElement.ApplyConfiguration(retval);
                                ConfigLoader.resolvedBindings.Remove(resolvedBindingID);
                            }
                            catch
                            {
                                // Clear list in case application is written to handle exception
                                // by not starting up channel, etc...
                                if (null != ConfigLoader.resolvedBindings)
                                {
                                    ConfigLoader.resolvedBindings = null;
                                }
                                throw;
                            }

                            if (null != ConfigLoader.resolvedBindings &&
                                0 == ConfigLoader.resolvedBindings.Count)
                            {
                                ConfigLoader.resolvedBindings = null;
                            }
                            configuredBindingFound = true;
                        }
                    }
                }
                if (!configuredBindingFound)
                {
                    // We expected to find an instance, but didn't.
                    // Return null. 
                    retval = null;
                }
            }

            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Leaks config objects, caller must ensure that these don't leak to user code.")]
        [SecurityCritical]
        static EndpointBehaviorElement LookupEndpointBehaviors(string behaviorName, ContextInformation context)
        {
            EndpointBehaviorElement retval = null;
            if (behaviorName != null)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.GetBehaviorElement,
                        SR.GetString(SR.TraceCodeGetBehaviorElement),
                        new StringTraceRecord("BehaviorName", behaviorName), null, null);
                }
                BehaviorsSection behaviors = null;
                if (context == null)
                {
                    behaviors = BehaviorsSection.UnsafeGetSection();
                }
                else
                {
                    behaviors = BehaviorsSection.UnsafeGetAssociatedSection(context);
                }
                if (behaviors.EndpointBehaviors.ContainsKey(behaviorName))
                {
                    retval = behaviors.EndpointBehaviors[behaviorName];
                }
            }
            if (retval != null)
            {
                CheckAccess(retval);
            }
            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Leaks config objects, caller must ensure that these don't leak to user code.")]
        [SecurityCritical]
        static ServiceBehaviorElement LookupServiceBehaviors(string behaviorName, ContextInformation context)
        {
            ServiceBehaviorElement retval = null;
            if (behaviorName != null)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.GetBehaviorElement,
                        SR.GetString(SR.TraceCodeGetBehaviorElement),
                        new StringTraceRecord("BehaviorName", behaviorName), null, null);
                }
                BehaviorsSection behaviors = null;
                if (context == null)
                {
                    behaviors = BehaviorsSection.UnsafeGetSection();
                }
                else
                {
                    behaviors = BehaviorsSection.UnsafeGetAssociatedSection(context);
                }
                if (behaviors.ServiceBehaviors.ContainsKey(behaviorName))
                {
                    retval = behaviors.ServiceBehaviors[behaviorName];
                }
            }
            if (retval != null)
            {
                CheckAccess(retval);
            }
            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Leaks config objects, caller must ensure that these don't leak to user code.")]
        [SecurityCritical]
        static CommonBehaviorsSection LookupCommonBehaviors(ContextInformation context)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.GetCommonBehaviors,
                    SR.GetString(SR.TraceCodeGetCommonBehaviors), (object)null);
            }
            return context == null
                ? CommonBehaviorsSection.UnsafeGetSection()
                : CommonBehaviorsSection.UnsafeGetAssociatedSection(context);
        }

        static bool IsChannelElementMatch(ChannelEndpointElement channelElement, ContractDescription contract, EndpointAddress address, bool useChannelElementKind, out ServiceEndpoint serviceEndpoint)
        {
            serviceEndpoint = null;
            if (string.IsNullOrEmpty(channelElement.Kind))
            {
                return channelElement.Contract == contract.ConfigurationName;
            }

            if (useChannelElementKind)
            {
                serviceEndpoint = LookupEndpoint(channelElement, null, address, contract);
                if (serviceEndpoint != null)
                {
                    if (serviceEndpoint.Contract.ConfigurationName == contract.ConfigurationName &&
                        (string.IsNullOrEmpty(channelElement.Contract) || contract.ConfigurationName == channelElement.Contract))
                    {
                        return true;
                    }
                    else
                    {
                        serviceEndpoint = null;
                        return false;
                    }
                }
                else
                {
                    return false;  // this should not happen with a valid client section since serviceEndpoint will never be null.
                }
            }
            else
            {
                // A standard endpoint should not be returned in the case of useChannelElementKind = false.
                // This is because useChannelElementKind = false only when this method is called by 
                // LoadChannelBehaviors (the overload that takes a ServiceEndpoint and a string(configurationName)).
                // LoadChannelBehaviors is called for the purposes of applying channel behaviors to a newly created service endpoint.
                // In the case of standard endpoints, the service endpoints are already fully configured.  
                // Reapplying behaviors would not only be redundant but may cause exceptions to be thrown.

                return false;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Leaks config objects, caller must ensure that these don't leak to user code.")]
        [SecurityCritical]
        static ChannelEndpointElement LookupChannel(ContextInformation configurationContext, string configurationName, ContractDescription contract,
            EndpointAddress address, bool wildcard, bool useChannelElementKind, out ServiceEndpoint serviceEndpoint)
        {
            serviceEndpoint = null;
            ClientSection clientSection = (configurationContext == null ? ClientSection.UnsafeGetSection() : ClientSection.UnsafeGetSection(configurationContext));
            ChannelEndpointElement retval = null;
            ServiceEndpoint standardEndpoint;
            foreach (ChannelEndpointElement channelElement in clientSection.Endpoints)
            {
                if (IsChannelElementMatch(channelElement, contract, address, useChannelElementKind, out standardEndpoint))
                {
                    if (channelElement.Name == configurationName || wildcard) // match name (or wildcard)
                    {
                        if (retval != null) // oops: >1
                        {
                            if (wildcard)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxConfigLoaderMultipleEndpointMatchesWildcard1, contract.ConfigurationName)));
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxConfigLoaderMultipleEndpointMatchesSpecified2, contract.ConfigurationName, configurationName)));
                            }
                        }
                        retval = channelElement;
                        serviceEndpoint = standardEndpoint;
                    }
                }
            }

            if (retval != null)
            {
                CheckAccess(retval);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, object> values = new Dictionary<string, object>(8);
                values["FoundChannelElement"] = retval != null;
                values["Name"] = configurationName;
                values["ContractName"] = contract.ConfigurationName;

                if (null != retval)
                {
                    if (!string.IsNullOrEmpty(retval.Binding))
                    {
                        values["Binding"] = retval.Binding;
                    }
                    if (!string.IsNullOrEmpty(retval.BindingConfiguration))
                    {
                        values["BindingConfiguration"] = retval.BindingConfiguration;
                    }
                    if (retval.Address != null)
                    {
                        values["RemoteEndpointUri"] = retval.Address.ToString();
                    }
                    if (!string.IsNullOrEmpty(retval.ElementInformation.Source))
                    {
                        values["ConfigurationFileSource"] = retval.ElementInformation.Source;
                        values["ConfigurationFileLineNumber"] = retval.ElementInformation.LineNumber;
                    }
                }

                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.GetChannelEndpointElement,
                    SR.GetString(SR.TraceCodeGetChannelEndpointElement),
                    new DictionaryTraceRecord(values), null, null);
            }

            return retval;
        }

        internal ContractDescription LookupContract(string contractName, string serviceName)
        {
            ContractDescription contract = LookupContractForStandardEndpoint(contractName, serviceName);
            if (contract == null)
            {
                if (contractName == String.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxReflectedContractKeyNotFoundEmpty, serviceName)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxReflectedContractKeyNotFound2, contractName, serviceName)));
                }
            }

            return contract;
        }

        internal ContractDescription LookupContractForStandardEndpoint(string contractName, string serviceName)
        {
            ContractDescription contract = contractResolver.ResolveContract(contractName);
            if (contract == null)
            {
                if (contractName == ServiceMetadataBehavior.MexContractName)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxReflectedContractKeyNotFoundIMetadataExchange, serviceName)));
                }
            }
            return contract;
        }

        [Fx.Tag.SecurityNote(Critical = "Leaks config objects, caller must ensure that these don't leak to user code.")]
        [SecurityCritical]
        public ServiceElement LookupService(string serviceConfigurationName)
        {
            ServicesSection servicesSection = ServicesSection.UnsafeGetSection();
            return LookupService(serviceConfigurationName, servicesSection);
        }

        public ServiceElement LookupService(string serviceConfigurationName, ServicesSection servicesSection)
        {
            ServiceElement retval = null;

            ServiceElementCollection services = servicesSection.Services;
            for (int i = 0; i < services.Count; i++)
            {
                ServiceElement serviceElement = services[i];
                if (serviceElement.Name == serviceConfigurationName)
                {
                    retval = serviceElement;
                }
            }

            if (retval != null)
            {
                CheckAccess(retval);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.GetServiceElement,
                    SR.GetString(SR.TraceCodeGetServiceElement),
                    new ServiceConfigurationTraceRecord(retval), null, null);
            }
            return retval;
        }

        static bool IsWildcardMatch(string endpointConfigurationName)
        {
            return String.Equals(endpointConfigurationName, "*", StringComparison.Ordinal);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - used in a security decision.")]
        static bool IsConfigAboveApplication(ContextInformation contextInformation)
        {
            if (contextInformation != null)
            {
                if (contextInformation.IsMachineLevel)
                {
                    return true;
                }

                bool isAppConfig = contextInformation.HostingContext is ExeContext;
                if (isAppConfig)
                {
                    return false; // for app.config, the only higher-scope config file is machine.config
                }
                else
                {
                    return IsWebConfigAboveApplication(contextInformation);
                }
            }

            return true; // err on the safe side: absent context information assume a PT app doesn't have access
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - used in a security decision.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool IsWebConfigAboveApplication(ContextInformation contextInformation)
        {
            return AspNetEnvironment.Current.IsWebConfigAboveApplication(contextInformation.HostingContext);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - enforces a security decision.")]
        static void CheckAccess(IConfigurationContextProviderInternal element)
        {
            if (IsConfigAboveApplication(ConfigurationHelpers.GetOriginalEvaluationContext(element)))
            {
                ConfigurationPermission.Demand();
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Used in a security decision.")]
        [SecurityCritical]
        static ConfigurationPermission configurationPermission;

        static ConfigurationPermission ConfigurationPermission
        {
            [Fx.Tag.SecurityNote(Critical = "Inits the configurationPermission field.",
                Safe = "Safe for readonly access.")]
            [SecuritySafeCritical]
            get
            {
                if (configurationPermission == null)
                {
                    configurationPermission = new ConfigurationPermission(System.Security.Permissions.PermissionState.Unrestricted);
                }
                return configurationPermission;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses critical field configurationPermission.")]
        [SecurityCritical]
        static bool ThreadHasConfigurationPermission()
        {
            try
            {
                ConfigurationPermission.Demand();
            }
            catch (SecurityException)
            {
                return false;
            }
            return true;
        }
    }

    class ProtocolMappingItem
    {
        public ProtocolMappingItem(string binding, string bindingConfiguration)
        {
            this.Binding = binding;
            this.BindingConfiguration = bindingConfiguration;
        }

        public string Binding { get; set; }

        public string BindingConfiguration { get; set; }
    }
}
