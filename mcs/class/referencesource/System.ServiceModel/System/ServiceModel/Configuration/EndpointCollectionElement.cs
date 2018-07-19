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
    using System.ServiceModel.Description;

    public abstract partial class EndpointCollectionElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        string endpointName = string.Empty;

        protected internal abstract StandardEndpointElement GetDefaultStandardEndpointElement();

        public string EndpointName
        {
            get
            {
                if (String.IsNullOrEmpty(this.endpointName))
                {
                    this.endpointName = this.GetEndpointName();
                }

                return this.endpointName;
            }
        }

        public abstract Type EndpointType
        {
            get;
        }

        public abstract ReadOnlyCollection<StandardEndpointElement> ConfiguredEndpoints
        {
            get;
        }

        public abstract bool ContainsKey(string name);

        protected internal abstract bool TryAdd(string name, ServiceEndpoint endpoint, Configuration config);

        [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method UnsafeLookupCollection which elevates.",
            Safe = "Does not leak config objects.")]
        [SecuritySafeCritical]
        string GetEndpointName()
        {
            string configuredSectionName = String.Empty;
            ExtensionElementCollection collection = null;
            Type extensionSectionType = this.GetType();

            collection = ExtensionsSection.UnsafeLookupCollection(ConfigurationStrings.EndpointExtensions, ConfigurationHelpers.GetEvaluationContext(this));

            if (null == collection)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigExtensionCollectionNotFound,
                    ConfigurationStrings.EndpointExtensions),
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
                    ConfigurationStrings.EndpointExtensions),
                    this.ElementInformation.Source,
                    this.ElementInformation.LineNumber));
            }

            return configuredSectionName;
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Miscellaneous =
            "RequiresReview - the return value will be used for a security decision -- see comment in interface definition")]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            Fx.Assert("Not implemented: IConfigurationContextProviderInternal.GetOriginalEvaluationContext");
            return null;
        }
    }
}



