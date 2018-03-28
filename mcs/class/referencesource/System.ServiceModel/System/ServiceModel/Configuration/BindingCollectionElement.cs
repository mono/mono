//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract partial class BindingCollectionElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        string bindingName = string.Empty;

        protected internal abstract Binding GetDefault();

        public string BindingName
        {
            get
            {
                if (String.IsNullOrEmpty(this.bindingName))
                {
                    this.bindingName = this.GetBindingName();
                }

                return this.bindingName;
            }
        }

        public abstract Type BindingType
        {
            get;
        }

        public abstract ReadOnlyCollection<IBindingConfigurationElement> ConfiguredBindings
        {
            get;
        }

        public abstract bool ContainsKey(string name);

        [Fx.Tag.SecurityNote(Critical = "Calls UnsafeLookupCollection which elevates.",
            Safe = "Doesn't leak config objects.")]
        [SecuritySafeCritical]
        string GetBindingName()
        {
            string configuredSectionName = String.Empty;
            ExtensionElementCollection collection = null;
            Type extensionSectionType = this.GetType();

            collection = ExtensionsSection.UnsafeLookupCollection(ConfigurationStrings.BindingExtensions, ConfigurationHelpers.GetEvaluationContext(this));

            if (null == collection)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigExtensionCollectionNotFound,
                    ConfigurationStrings.BindingExtensions),
                    this.ElementInformation.Source,
                    this.ElementInformation.LineNumber));
            }

            for (int i = 0; i < collection.Count; i++)
            {
                ExtensionElement collectionElement = collection[i];

                // Optimize for assembly qualified names.
                if (collectionElement.Type.Equals(extensionSectionType.AssemblyQualifiedName, StringComparison.Ordinal))
                {
                    configuredSectionName = collectionElement.Name;
                    break;
                }

                // Check type directly for the case that the extension is registered with something less than
                // an full assembly qualified name.
                Type collectionElementType = Type.GetType(collectionElement.Type, false);
                if (null != collectionElementType && extensionSectionType.Equals(collectionElementType))
                {
                    configuredSectionName = collectionElement.Name;
                    break;
                }
            }

            if (String.IsNullOrEmpty(configuredSectionName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigExtensionTypeNotRegisteredInCollection,
                    extensionSectionType.AssemblyQualifiedName,
                    ConfigurationStrings.BindingExtensions),
                    this.ElementInformation.Source,
                    this.ElementInformation.LineNumber));
            }

            return configuredSectionName;
        }

        protected internal abstract bool TryAdd(string name, Binding binding, Configuration config);

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



