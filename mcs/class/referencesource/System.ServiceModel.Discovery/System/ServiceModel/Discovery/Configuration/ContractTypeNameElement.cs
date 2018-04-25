//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Description;

    [Fx.Tag.XamlVisible(false)]
    public sealed class ContractTypeNameElement : ConfigurationElement
    {        
        ConfigurationPropertyCollection properties;

        public ContractTypeNameElement()
        { 
        }

        public ContractTypeNameElement(string name, string ns)
        {
            this.Name = name;
            this.Namespace = ns;
        }

        [ConfigurationProperty(ConfigurationStrings.Namespace, DefaultValue = NamingHelper.DefaultNamespace, Options = ConfigurationPropertyOptions.IsKey)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "Validator not requiered")]
        public string Namespace
        {
            get
            {
                return (string)base[ConfigurationStrings.Namespace];
            }

            set
            {
                base[ConfigurationStrings.Namespace] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get
            {
                return (string)base[ConfigurationStrings.Name];
            }

            set
            {
                base[ConfigurationStrings.Name] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Namespace,
                        typeof(string),
                        NamingHelper.DefaultNamespace,
                        null,
                        null,
                        System.Configuration.ConfigurationPropertyOptions.IsKey));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Name,
                        typeof(string),
                        null,
                        null,
                        new StringValidator(1),
                        System.Configuration.ConfigurationPropertyOptions.IsKey | 
                        System.Configuration.ConfigurationPropertyOptions.IsRequired));

                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}
