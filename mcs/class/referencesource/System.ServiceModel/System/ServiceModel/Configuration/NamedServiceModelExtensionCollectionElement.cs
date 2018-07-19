//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public abstract class NamedServiceModelExtensionCollectionElement<TServiceModelExtensionElement> : ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>
        where TServiceModelExtensionElement : ServiceModelExtensionElement
    {
        ConfigurationPropertyCollection properties = null;

        internal NamedServiceModelExtensionCollectionElement(string extensionCollectionName, string name)
            : base(extensionCollectionName)
        {
            if (!String.IsNullOrEmpty(name))
            {
                this.Name = name;
            }
            else
            {
                this.Name = String.Empty;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.Name]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Name] = value;
                this.SetIsModified();
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = base.Properties;
                    this.properties.Add(new ConfigurationProperty(ConfigurationStrings.Name, typeof(System.String), null, null, new StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                }
                return this.properties;
            }
        }
    }
}
