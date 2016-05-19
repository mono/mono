//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed partial class WsdlImporterElement : ConfigurationElement
    {
        public WsdlImporterElement()
        {
        }

        public WsdlImporterElement(string type)
        {
            this.Type = type;
        }

        public WsdlImporterElement(Type type)
        {
            SubclassTypeValidator validator = new SubclassTypeValidator(typeof(IWsdlImportExtension));
            validator.Validate(type);
            this.Type = type.AssemblyQualifiedName;
        }

        [ConfigurationProperty(ConfigurationStrings.Type, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 1)]
        public string Type
        {
            get
            { return (string)base[ConfigurationStrings.Type]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Type] = value;
            }
        }
    }
}
