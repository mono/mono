//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Configuration;

namespace System.IdentityModel.Configuration
{
#pragma warning disable 1591
    [ConfigurationCollection(typeof(IdentityConfigurationElement), AddItemName = ConfigurationStrings.IdentityConfiguration, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed partial class IdentityConfigurationElementCollection : ConfigurationElementCollection
    {
        // Note:This is a BasicMap collection type with ThrowOnDuplicate true.
        // If there are two configuration elements defined with the same key the configuration system throws an error. 

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IdentityConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            IdentityConfigurationElement elementAsServiceElement = element as IdentityConfigurationElement;

            if (elementAsServiceElement == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7013));
            }

            return elementAsServiceElement.Name;
        }

        /// <summary>
        /// Retrieves the ServiceElement with the specified name.
        /// </summary>
        /// <param name="name">The name of the ServiceElement to retrieve</param>
        /// <returns>A ServiceElement instance</returns>
        public IdentityConfigurationElement GetElement(string name)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            IdentityConfigurationElement result = base.BaseGet(name) as IdentityConfigurationElement;

            if (!StringComparer.Ordinal.Equals(name, ConfigurationStrings.DefaultConfigurationElementName) && result == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7012, name));
            }

            return result;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            string name = GetElementKey(element) as string;
            IdentityConfigurationElement result = base.BaseGet(name) as IdentityConfigurationElement;

            if (result != null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7029, "<identityConfiguation>", name));
            }

            base.BaseAdd(element);
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return (Count > 0);
            }
        }
    }
#pragma warning restore 1591
}
