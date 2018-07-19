//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed partial class ProtocolMappingElement : ConfigurationElement
    {
        public ProtocolMappingElement()
        {
        }

        public ProtocolMappingElement(string schemeType, string binding, string bindingConfiguration)
        {
            if (String.IsNullOrEmpty(schemeType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schemeType");
            }
            this.Scheme = schemeType;

            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            this.Binding = binding;
            this.BindingConfiguration = bindingConfiguration;
        }

        [ConfigurationProperty(ConfigurationStrings.Scheme, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 1)]
        public string Scheme
        {
            get { return (string)base[ConfigurationStrings.Scheme]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.Scheme] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Binding, Options = ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string Binding
        {
            get { return (String)base[ConfigurationStrings.Binding]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.Binding] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingConfiguration, Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string BindingConfiguration
        {
            get { return (String)base[ConfigurationStrings.BindingConfiguration]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.BindingConfiguration] = value;
            }
        }
    }
}

