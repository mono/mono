//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed partial class TransportConfigurationTypeElement : ConfigurationElement
    {
        public TransportConfigurationTypeElement()
        {
        }

        public TransportConfigurationTypeElement(string name)
            : this()
        {
            if (String.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            this.Name = name;
        }

        public TransportConfigurationTypeElement(string name, string transportConfigurationTypeName)
            : this(name)
        {
            if (String.IsNullOrEmpty(transportConfigurationTypeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transportConfigurationTypeName");
            }
            this.TransportConfigurationType = transportConfigurationTypeName;
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

        [ConfigurationProperty(ConfigurationStrings.TransportConfigurationType, Options = ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string TransportConfigurationType
        {
            get { return (string)base[ConfigurationStrings.TransportConfigurationType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.TransportConfigurationType] = value;
            }
        }

    }
}
