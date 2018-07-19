//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Globalization;

    public sealed partial class IssuedTokenClientBehaviorsElement : ConfigurationElement
    {
        public IssuedTokenClientBehaviorsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.IssuerAddress, DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string IssuerAddress
        {
            get { return (string)base[ConfigurationStrings.IssuerAddress]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.IssuerAddress] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BehaviorConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string BehaviorConfiguration
        {
            get { return (string)base[ConfigurationStrings.BehaviorConfiguration]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BehaviorConfiguration] = value;
            }
        }
    }
}



