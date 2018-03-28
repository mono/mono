//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Net;

    public sealed partial class DefaultPortElement : ConfigurationElement
    {
        public DefaultPortElement() 
        {
        }

        public DefaultPortElement(DefaultPortElement other)
        {
            this.Scheme = other.Scheme;
            this.Port = other.Port;
        }

        [ConfigurationProperty(ConfigurationStrings.Scheme, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
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

        [ConfigurationProperty(ConfigurationStrings.Port, DefaultValue = 0, Options = ConfigurationPropertyOptions.IsRequired)]
        [IntegerValidator(MinValue = IPEndPoint.MinPort, MaxValue = IPEndPoint.MaxPort)]
        public int Port
        {
            get { return (int)base[ConfigurationStrings.Port]; }
            set { base[ConfigurationStrings.Port] = value; }
        }
    }
}
