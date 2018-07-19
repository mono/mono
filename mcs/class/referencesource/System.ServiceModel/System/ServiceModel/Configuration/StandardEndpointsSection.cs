//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Runtime.Diagnostics;

    public sealed partial class StandardEndpointsSection : ConfigurationSection, IConfigurationContextProviderInternal
    {
        static Configuration configuration;
        ConfigurationPropertyCollection properties;

        public StandardEndpointsSection() { }

        Dictionary<string, EndpointCollectionElement> EndpointCollectionElements
        {
            get
            {
                Dictionary<string, EndpointCollectionElement> endpointCollectionElements = new Dictionary<string, EndpointCollectionElement>();

                foreach (ConfigurationProperty property in this.Properties)
                {
                    endpointCollectionElements.Add(property.Name, this[property.Name]);
                }

                return endpointCollectionElements;
            }
        }

        new public EndpointCollectionElement this[string endpoint]
        {
            get
            {
                return (EndpointCollectionElement)base[endpoint];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ConfigurationPropertyCollection();
                }

                this.UpdateEndpointSections();
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MexStandardEndpointCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public ServiceMetadataEndpointCollectionElement MexEndpoint
        {
            get { return (ServiceMetadataEndpointCollectionElement)base[ConfigurationStrings.MexStandardEndpointCollectionElementName]; }
        }

        // This property should only be called/set from EndpointsSectionGroup TryAdd
        static Configuration Configuration
        {
            get { return StandardEndpointsSection.configuration; }
            set { StandardEndpointsSection.configuration = value; }
        }

        public static StandardEndpointsSection GetSection(Configuration config)
        {
            if (config == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }

            return (StandardEndpointsSection)config.GetSection(ConfigurationStrings.StandardEndpointsSectionPath);
        }

        public List<EndpointCollectionElement> EndpointCollections
        {
            get
            {
                List<EndpointCollectionElement> endpointCollections = new List<EndpointCollectionElement>();
                foreach (ConfigurationProperty property in this.Properties)
                {
                    endpointCollections.Add(this[property.Name]);
                }

                return endpointCollections;
            }
        }

        internal static bool TryAdd(string name, ServiceEndpoint endpoint, Configuration config, out string endpointSectionName)
        {
            bool retval = false;
            StandardEndpointsSection.Configuration = config;
            try
            {
                retval = StandardEndpointsSection.TryAdd(name, endpoint, out endpointSectionName);
            }
            finally
            {
                StandardEndpointsSection.Configuration = null;
            }
            return retval;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ConfigurationErrorsException(SR.GetString(SR.ConfigEndpointExtensionNotFound,
                        ConfigurationHelpers.GetEndpointsSectionPath(elementName))));
        }

        internal static bool TryAdd(string name, ServiceEndpoint endpoint, out string endpointSectionName)
        {
            // TryAdd built on assumption that StandardEndpointsSectionGroup.Configuration is valid.
            // This should be protected at the callers site.  If assumption is invalid, then
            // configuration system is in an indeterminate state.  Need to stop in a manner that
            // user code can not capture.
            if (null == StandardEndpointsSection.Configuration)
            {
                Fx.Assert("The TryAdd(string name, ServiceEndpoint endpoint, Configuration config, out string endpointSectionName) variant of this function should always be called first. The Configuration object is not set.");
                DiagnosticUtility.FailFast("The TryAdd(string name, ServiceEndpoint endpoint, Configuration config, out string endpointSectionName) variant of this function should always be called first. The Configuration object is not set.");
            }

            bool retval = false;
            string outEndpointSectionName = null;
            StandardEndpointsSection sectionGroup = StandardEndpointsSection.GetSection(StandardEndpointsSection.Configuration);
            sectionGroup.UpdateEndpointSections();
            foreach (string sectionName in sectionGroup.EndpointCollectionElements.Keys)
            {
                EndpointCollectionElement endpointCollectionElement = sectionGroup.EndpointCollectionElements[sectionName];

                MethodInfo tryAddMethod = endpointCollectionElement.GetType().GetMethod("TryAdd", BindingFlags.Instance | BindingFlags.NonPublic);
                if (tryAddMethod != null)
                {
                    retval = (bool)tryAddMethod.Invoke(endpointCollectionElement, new object[] { name, endpoint, StandardEndpointsSection.Configuration });
                    if (retval)
                    {
                        outEndpointSectionName = sectionName;
                        break;
                    }
                }
            }

            // This little oddity exists to make sure that the out param is assigned to before the method
            // exits.
            endpointSectionName = outEndpointSectionName;
            return retval;
        }

        void UpdateEndpointSections()
        {
            UpdateEndpointSections(ConfigurationHelpers.GetEvaluationContext(this));
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical methods UnsafeLookupCollection which elevates in order to load config.",
            Safe = "Does not leak any config objects.")]
        [SecuritySafeCritical]
        internal void UpdateEndpointSections(ContextInformation evaluationContext)
        {
            ExtensionElementCollection endpointExtensions = ExtensionsSection.UnsafeLookupCollection(ConfigurationStrings.EndpointExtensions, evaluationContext);

            // Extension collections are additive only (BasicMap) and do not allow for <clear>
            // or <remove> tags, nor do they allow for overriding an entry.  This allows us
            // to optimize this to only walk the binding extension collection if the counts 
            // mismatch.
            if (endpointExtensions.Count != this.properties.Count)
            {
                foreach (ExtensionElement endpointExtension in endpointExtensions)
                {
                    if (null != endpointExtension)
                    {
                        if (!this.properties.Contains(endpointExtension.Name))
                        {
                            Type extensionType = Type.GetType(endpointExtension.Type, false);
                            if (extensionType == null)
                            {
                                ConfigurationHelpers.TraceExtensionTypeNotFound(endpointExtension);
                            }
                            else
                            {
                                ConfigurationProperty property = new ConfigurationProperty(endpointExtension.Name,
                                    extensionType,
                                    null,
                                    ConfigurationPropertyOptions.None);

                                this.properties.Add(property);
                            }
                        }
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical methods UnsafeGetAssociatedBindingCollectionElement which elevates in order to load config.",
            Safe = "Does not leak any config objects.")]
        [SecuritySafeCritical]
        internal static void ValidateEndpointReference(string endpoint, string endpointConfiguration, ContextInformation evaluationContext, ConfigurationElement configurationElement)
        {
            // ValidateEndpointReference built on assumption that evaluationContext is valid.
            // This should be protected at the callers site.  If assumption is invalid, then
            // configuration system is in an indeterminate state.  Need to stop in a manner that
            // user code can not capture.
            if (null == evaluationContext)
            {
                Fx.Assert("ValidateEndpointReference() should only called with valid ContextInformation");
                DiagnosticUtility.FailFast("ValidateEndpointReference() should only called with valid ContextInformation");
            }

            if (!String.IsNullOrEmpty(endpoint))
            {
                EndpointCollectionElement endpointCollectionElement = null;

                if (null != evaluationContext)
                {
                    endpointCollectionElement = ConfigurationHelpers.UnsafeGetAssociatedEndpointCollectionElement(evaluationContext, endpoint);
                }
                else
                {
                    endpointCollectionElement = ConfigurationHelpers.UnsafeGetEndpointCollectionElement(endpoint);
                }

                if (endpointCollectionElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidSection,
                        ConfigurationHelpers.GetEndpointsSectionPath(endpoint)),
                        configurationElement.ElementInformation.Source,
                        configurationElement.ElementInformation.LineNumber));
                }

                if (!String.IsNullOrEmpty(endpointConfiguration))
                {
                    if (!endpointCollectionElement.ContainsKey(endpointConfiguration))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidEndpointName,
                            endpointConfiguration,
                            ConfigurationHelpers.GetEndpointsSectionPath(endpoint),
                            ConfigurationStrings.EndpointConfiguration),
                            configurationElement.ElementInformation.Source,
                            configurationElement.ElementInformation.LineNumber));
                    }
                }
            }
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview -- the return value will be used for a security decision -- see comment in interface definition.")]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            Fx.Assert("Not implemented: IConfigurationContextProviderInternal.GetOriginalEvaluationContext");
            return null;
        }
    }
}
