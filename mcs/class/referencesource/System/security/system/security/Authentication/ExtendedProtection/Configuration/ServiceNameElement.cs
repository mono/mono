//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//------------------------------------------------------------------------------

using System.Configuration;

namespace System.Security.Authentication.ExtendedProtection.Configuration
{
    public sealed class ServiceNameElement : ConfigurationElement
    {
        public ServiceNameElement()
        {
            this.properties.Add(this.name);
        }

        [ConfigurationProperty(ExtendedProtectionConfigurationStrings.Name)]
        public string Name
        {
            get { return (string)this[this.name]; }
            set { this[this.name] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        internal string Key
        {
            get { return this.Name; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty name =
            new ConfigurationProperty(ExtendedProtectionConfigurationStrings.Name,
                typeof(string), null,
                ConfigurationPropertyOptions.IsRequired);
    }
}
