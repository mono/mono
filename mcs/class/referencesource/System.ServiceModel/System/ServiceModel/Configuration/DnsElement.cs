//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Cryptography;
    using System.Xml;

    public sealed partial class DnsElement : ConfigurationElement
    {
        public DnsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Value, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public String Value
        {
            get { return (string)base[ConfigurationStrings.Value]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.Value] = value;
            }
        }
    }

}
