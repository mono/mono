//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;


    public sealed partial class CustomBindingCollectionElement : BindingCollectionElement
    {

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public CustomBindingElementCollection Bindings
        {
            get { return (CustomBindingElementCollection)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        public override Type BindingType
        {
            get { return typeof(CustomBinding); }
        }

        public override ReadOnlyCollection<IBindingConfigurationElement> ConfiguredBindings
        {
            get
            {
                List<IBindingConfigurationElement> configuredBindings = new List<IBindingConfigurationElement>();
                foreach (IBindingConfigurationElement configuredBinding in this.Bindings)
                {
                    configuredBindings.Add(configuredBinding);
                }

                return new ReadOnlyCollection<IBindingConfigurationElement>(configuredBindings);
            }
        }

        public override bool ContainsKey(string name)
        {
            // This line needed because of the IBindingSection implementation
            return this.Bindings.ContainsKey(name);
        }

        protected internal override Binding GetDefault()
        {
            return System.Activator.CreateInstance<CustomBinding>();
        }
        internal static CustomBindingCollectionElement GetBindingCollectionElement()
        {
            return (CustomBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.CustomBindingCollectionElementName);
        }

        bool TryCreateMatchingExtension(BindingElement bindingElement, ExtensionElementCollection collection, bool allowDerivedTypes, string assemblyName, out BindingElementExtensionElement result)
        {
            result = null;
            foreach (ExtensionElement element in collection)
            {
                BindingElementExtensionElement bindingElementExtension = Activator.CreateInstance(Type.GetType(element.Type, true)) as BindingElementExtensionElement;
                if (null == bindingElementExtension)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidExtensionType,
                        element.Type,
                        assemblyName,
                        ConfigurationStrings.BindingElementExtensions)));
                }

                bool isMatch;
                if (allowDerivedTypes)
                {
                    isMatch = bindingElementExtension.BindingElementType.IsAssignableFrom(bindingElement.GetType());
                }
                else
                {
                    isMatch = bindingElementExtension.BindingElementType.Equals(bindingElement.GetType());
                }

                if (isMatch)
                {
                    result = bindingElementExtension;
                    return true;
                }
            }
            return false;
        }

        protected internal override bool TryAdd(string name, Binding binding, Configuration config)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            if (null == binding)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }

            if (null == config)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }

            ServiceModelSectionGroup sg = ServiceModelSectionGroup.GetSectionGroup(config);
            CustomBindingElementCollection customBindings = sg.Bindings.CustomBinding.Bindings;
            CustomBindingElement configElement = new CustomBindingElement(name);
            customBindings.Add(configElement);

            ExtensionElementCollection collection = sg.Extensions.BindingElementExtensions;

            CustomBinding customBinding = (CustomBinding)binding;
            foreach (BindingElement bindingElement in customBinding.Elements)
            {
                BindingElementExtensionElement bindingElementExtension;
                bool foundMatch = TryCreateMatchingExtension(bindingElement, collection, false, configElement.CollectionElementBaseType.AssemblyQualifiedName, out bindingElementExtension);
                if (!foundMatch)
                {
                    foundMatch = TryCreateMatchingExtension(bindingElement, collection, true, configElement.CollectionElementBaseType.AssemblyQualifiedName, out bindingElementExtension);
                }
                if (!foundMatch)
                {
                    break;
                }
                bindingElementExtension.InitializeFrom(bindingElement);
                configElement.Add(bindingElementExtension);
            }

            bool retval = configElement.Count == customBinding.Elements.Count;
            if (!retval)
            {
                customBindings.Remove(configElement);
            }

            return retval;
        }
    }
}

