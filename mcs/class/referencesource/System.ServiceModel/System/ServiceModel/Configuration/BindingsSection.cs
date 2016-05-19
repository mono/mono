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
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Runtime.Diagnostics;

    public sealed partial class BindingsSection : ConfigurationSection, IConfigurationContextProviderInternal
    {
        static Configuration configuration;
        ConfigurationPropertyCollection properties;

        public BindingsSection() { }

        Dictionary<string, BindingCollectionElement> BindingCollectionElements
        {
            get
            {
                Dictionary<string, BindingCollectionElement> bindingCollectionElements = new Dictionary<string, BindingCollectionElement>();
                
                foreach (ConfigurationProperty property in this.Properties)
                {
                    bindingCollectionElements.Add(property.Name, this[property.Name]);
                }

                return bindingCollectionElements;
            }
        }

        new public BindingCollectionElement this[string binding]
        {
            get
            {
                return (BindingCollectionElement)base[binding];
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

                this.UpdateBindingSections();
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BasicHttpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public BasicHttpBindingCollectionElement BasicHttpBinding
        {
            get { return (BasicHttpBindingCollectionElement)base[ConfigurationStrings.BasicHttpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.BasicHttpsBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public BasicHttpsBindingCollectionElement BasicHttpsBinding
        {
            get { return (BasicHttpsBindingCollectionElement)base[ConfigurationStrings.BasicHttpsBindingCollectionElementName]; }
        }

        // This property should only be called/set from BindingsSectionGroup TryAdd
        static Configuration Configuration
        {
            get { return BindingsSection.configuration; }
            set { BindingsSection.configuration = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.CustomBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public CustomBindingCollectionElement CustomBinding
        {
            get { return (CustomBindingCollectionElement)base[ConfigurationStrings.CustomBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.MsmqIntegrationBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public MsmqIntegrationBindingCollectionElement MsmqIntegrationBinding
        {
            get { return (MsmqIntegrationBindingCollectionElement)base[ConfigurationStrings.MsmqIntegrationBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NetHttpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public NetHttpBindingCollectionElement NetHttpBinding
        {
            get { return (NetHttpBindingCollectionElement)base[ConfigurationStrings.NetHttpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NetHttpsBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public NetHttpsBindingCollectionElement NetHttpsBinding
        {
            get { return (NetHttpsBindingCollectionElement)base[ConfigurationStrings.NetHttpsBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NetPeerTcpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
        public NetPeerTcpBindingCollectionElement NetPeerTcpBinding
        {
            get { return (NetPeerTcpBindingCollectionElement)base[ConfigurationStrings.NetPeerTcpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NetMsmqBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public NetMsmqBindingCollectionElement NetMsmqBinding
        {
            get { return (NetMsmqBindingCollectionElement)base[ConfigurationStrings.NetMsmqBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NetNamedPipeBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public NetNamedPipeBindingCollectionElement NetNamedPipeBinding
        {
            get { return (NetNamedPipeBindingCollectionElement)base[ConfigurationStrings.NetNamedPipeBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NetTcpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public NetTcpBindingCollectionElement NetTcpBinding
        {
            get { return (NetTcpBindingCollectionElement)base[ConfigurationStrings.NetTcpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WSFederationHttpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public WSFederationHttpBindingCollectionElement WSFederationHttpBinding
        {
            get { return (WSFederationHttpBindingCollectionElement)base[ConfigurationStrings.WSFederationHttpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WS2007FederationHttpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public WS2007FederationHttpBindingCollectionElement WS2007FederationHttpBinding
        {
            get { return (WS2007FederationHttpBindingCollectionElement)base[ConfigurationStrings.WS2007FederationHttpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WSHttpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public WSHttpBindingCollectionElement WSHttpBinding
        {
            get { return (WSHttpBindingCollectionElement)base[ConfigurationStrings.WSHttpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WS2007HttpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public WS2007HttpBindingCollectionElement WS2007HttpBinding
        {
            get { return (WS2007HttpBindingCollectionElement)base[ConfigurationStrings.WS2007HttpBindingCollectionElementName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WSDualHttpBindingCollectionElementName, Options = ConfigurationPropertyOptions.None)]
        public WSDualHttpBindingCollectionElement WSDualHttpBinding
        {
            get { return (WSDualHttpBindingCollectionElement)base[ConfigurationStrings.WSDualHttpBindingCollectionElementName]; }
        }

        public static BindingsSection GetSection(Configuration config)
        {
            if (config == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }

            return (BindingsSection)config.GetSection(ConfigurationStrings.BindingsSectionGroupPath);
        }

        public List<BindingCollectionElement> BindingCollections
        {
            get
            {
                List<BindingCollectionElement> bindingCollections = new List<BindingCollectionElement>();
                foreach (ConfigurationProperty property in this.Properties)
                {
                    bindingCollections.Add(this[property.Name]);
                }

                return bindingCollections;
            }
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ConfigurationErrorsException(SR.GetString(SR.ConfigBindingExtensionNotFound,
                        ConfigurationHelpers.GetBindingsSectionPath(elementName))));
        }

        internal static bool TryAdd(string name, Binding binding, Configuration config, out string bindingSectionName)
        {
            bool retval = false;
            BindingsSection.Configuration = config;
            try
            {
                retval = BindingsSection.TryAdd(name, binding, out bindingSectionName);
            }
            finally
            {
                BindingsSection.Configuration = null;
            }
            return retval;
        }

        internal static bool TryAdd(string name, Binding binding, out string bindingSectionName)
        {
            // TryAdd built on assumption that BindingsSectionGroup.Configuration is valid.
            // This should be protected at the callers site.  If assumption is invalid, then
            // configuration system is in an indeterminate state.  Need to stop in a manner that
            // user code can not capture.
            if (null == BindingsSection.Configuration)
            {
                Fx.Assert("The TryAdd(string name, Binding binding, Configuration config, out string binding) variant of this function should always be called first. The Configuration object is not set.");
                DiagnosticUtility.FailFast("The TryAdd(string name, Binding binding, Configuration config, out string binding) variant of this function should always be called first. The Configuration object is not set.");
            }

            bool retval = false;
            string outBindingSectionName = null;
            BindingsSection sectionGroup = BindingsSection.GetSection(BindingsSection.Configuration);
            sectionGroup.UpdateBindingSections();
            foreach (string sectionName in sectionGroup.BindingCollectionElements.Keys)
            {
                BindingCollectionElement bindingCollectionElement = sectionGroup.BindingCollectionElements[sectionName];

                // Save the custom bindings as the last choice
                if (!(bindingCollectionElement is CustomBindingCollectionElement))
                {
                    MethodInfo tryAddMethod = bindingCollectionElement.GetType().GetMethod("TryAdd", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (tryAddMethod != null)
                    {
                        retval = (bool)tryAddMethod.Invoke(bindingCollectionElement, new object[] { name, binding, BindingsSection.Configuration });
                        if (retval)
                        {
                            outBindingSectionName = sectionName;
                            break;
                        }
                    }
                }
            }
            if (!retval)
            {
                // Much of the time, the custombinding should come out ok.
                CustomBindingCollectionElement customBindingSection = CustomBindingCollectionElement.GetBindingCollectionElement();
                retval = customBindingSection.TryAdd(name, binding, BindingsSection.Configuration);
                if (retval)
                {
                    outBindingSectionName = ConfigurationStrings.CustomBindingCollectionElementName;
                }
            }

            // This little oddity exists to make sure that the out param is assigned to before the method
            // exits.
            bindingSectionName = outBindingSectionName;
            return retval;
        }

        void UpdateBindingSections()
        {
            UpdateBindingSections(ConfigurationHelpers.GetEvaluationContext(this));
        }

        [Fx.Tag.SecurityNote(Critical = "Calls UnsafeLookupCollection which elevates.",
            Safe = "Doesn't leak resultant config.")]
        [SecuritySafeCritical]
        internal void UpdateBindingSections(ContextInformation evaluationContext)
        {
            ExtensionElementCollection bindingExtensions = ExtensionsSection.UnsafeLookupCollection(ConfigurationStrings.BindingExtensions, evaluationContext);

            // Extension collections are additive only (BasicMap) and do not allow for <clear>
            // or <remove> tags, nor do they allow for overriding an entry.  This allows us
            // to optimize this to only walk the binding extension collection if the counts 
            // mismatch.
            if (bindingExtensions.Count != this.properties.Count)
            {
                foreach (ExtensionElement bindingExtension in bindingExtensions)
                {
                    if (null != bindingExtension)
                    {
                        if (!this.properties.Contains(bindingExtension.Name))
                        {
                            Type extensionType = Type.GetType(bindingExtension.Type, false);
                            if (extensionType == null)
                            {
                                ConfigurationHelpers.TraceExtensionTypeNotFound(bindingExtension);
                            }
                            else
                            {
                                ConfigurationProperty property = new ConfigurationProperty(bindingExtension.Name,
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

        [Fx.Tag.SecurityNote(Critical = "Calls UnsafeGetAssociatedBindingCollectionElement which elevates.",
            Safe = "Doesn't leak resultant config.")]
        [SecuritySafeCritical]
        internal static void ValidateBindingReference(string binding, string bindingConfiguration, ContextInformation evaluationContext, ConfigurationElement configurationElement)
        {
            // ValidateBindingReference built on assumption that evaluationContext is valid.
            // This should be protected at the callers site.  If assumption is invalid, then
            // configuration system is in an indeterminate state.  Need to stop in a manner that
            // user code can not capture.
            if (null == evaluationContext)
            {
                Fx.Assert("ValidateBindingReference() should only called with valid ContextInformation");
                DiagnosticUtility.FailFast("ValidateBindingReference() should only called with valid ContextInformation");
            }

            if (!String.IsNullOrEmpty(binding))
            {
                BindingCollectionElement bindingCollectionElement = null;

                if (null != evaluationContext)
                {
                    bindingCollectionElement = ConfigurationHelpers.UnsafeGetAssociatedBindingCollectionElement(evaluationContext, binding);
                }
                else
                {
                    bindingCollectionElement = ConfigurationHelpers.UnsafeGetBindingCollectionElement(binding);
                }

                if (bindingCollectionElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidSection,
                        ConfigurationHelpers.GetBindingsSectionPath(binding)),
                        configurationElement.ElementInformation.Source,
                        configurationElement.ElementInformation.LineNumber));
                }

                if (!String.IsNullOrEmpty(bindingConfiguration))
                {
                    if (!bindingCollectionElement.ContainsKey(bindingConfiguration))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidBindingName,
                            bindingConfiguration,
                            ConfigurationHelpers.GetBindingsSectionPath(binding),
                            ConfigurationStrings.BindingConfiguration),
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

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - the return value will be used for a security decision -- see comment in interface definition.")]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            Fx.Assert("Not implemented: IConfigurationContextProviderInternal.GetOriginalEvaluationContext");
            return null;
        }
    }
}
