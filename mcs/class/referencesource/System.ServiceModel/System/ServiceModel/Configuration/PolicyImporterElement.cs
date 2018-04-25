//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed partial class PolicyImporterElement : ConfigurationElement
    {
        public PolicyImporterElement()
        {
        }

        public PolicyImporterElement(string type)
        {
            this.Type = type;
        }

        public PolicyImporterElement(Type type)
        {
            SubclassTypeValidator validator = new SubclassTypeValidator(typeof(IPolicyImportExtension));
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
