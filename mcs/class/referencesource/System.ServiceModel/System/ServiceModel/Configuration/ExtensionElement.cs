//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public partial class ExtensionElement : ConfigurationElement
    {
        string typeName;

        public ExtensionElement()
        {
        }

        public ExtensionElement(string name)
            : this()
        {
            if (String.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            this.Name = name;
        }

        public ExtensionElement(string name, string type)
            : this(name)
        {
            if (String.IsNullOrEmpty(type))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("type");
            }

            this.Type = type;
        }

        [ConfigurationProperty(ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 1)]
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
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Type, Options = ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string Type
        {
            get { return (string)base[ConfigurationStrings.Type]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }

                base[ConfigurationStrings.Type] = value;
            }
        }

        internal string TypeName
        {
            get
            {
                if (string.IsNullOrEmpty(this.typeName))
                {
                    this.typeName = GetTypeName(this.Type);
                }

                return this.typeName;
            }
        }

        internal static string GetTypeName(string fullyQualifiedName)
        {
            string typeName = fullyQualifiedName.Split(',')[0];
            return typeName.Trim();
        }
    }
}
