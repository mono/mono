//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(ExtensionElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ExtensionElementCollection : ServiceModelConfigurationElementCollection<ExtensionElement>
    {
        public ExtensionElementCollection()
            : base(ConfigurationElementCollectionType.BasicMap, ConfigurationStrings.Add)
        {
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (!this.InheritedElementExists((ExtensionElement)element))
            {
                this.EnforceUniqueElement((ExtensionElement)element);
                base.BaseAdd(element);
            }
        }

        protected override void BaseAdd(int index, ConfigurationElement element)
        {
            if (!this.InheritedElementExists((ExtensionElement)element))
            {
                this.EnforceUniqueElement((ExtensionElement)element);
                base.BaseAdd(index, element);
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (null == element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ExtensionElement configElementKey = (ExtensionElement)element;
            return configElementKey.Name;
        }

        bool InheritedElementExists(ExtensionElement element)
        {
            // This is logic from ServiceModelEnhancedConfigurationElementCollection
            // The idea is to allow duplicate identitcal extension definition in different level (i.e. app level and machine level)
            // We however do not allow them on the same level.
            // Identical extension is defined by same name and type.
            object newElementKey = this.GetElementKey(element);
            if (this.ContainsKey(newElementKey))
            {
                ExtensionElement oldElement = (ExtensionElement)this.BaseGet(newElementKey);
                if (null != oldElement)
                {
                    // Is oldElement present in the different level of original config
                    // and name/type matching
                    if (!oldElement.ElementInformation.IsPresent &&
                        element.Type.Equals(oldElement.Type, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void EnforceUniqueElement(ExtensionElement element)
        {
            foreach (ExtensionElement extension in this)
            {
                if (element.Name.Equals(extension.Name, StringComparison.Ordinal))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigDuplicateExtensionName, element.Name)));
                }

                bool foundDuplicateType = false;
                if (element.Type.Equals(extension.Type, StringComparison.OrdinalIgnoreCase))
                {
                    foundDuplicateType = true;
                }
                else if (element.TypeName.Equals(extension.TypeName, StringComparison.Ordinal))
                {
                    // In order to avoid extra assemblies being loaded, we perform type comparison only if the type names
                    // are the same. See 
                    Type elementType = Type.GetType(element.Type, false);
                    if (null != elementType && elementType.Equals(Type.GetType(extension.Type, false)))
                    {
                        foundDuplicateType = true;
                    }
                }

                if (foundDuplicateType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigDuplicateExtensionType, element.Type)));
                }
            }
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }
    }
}
