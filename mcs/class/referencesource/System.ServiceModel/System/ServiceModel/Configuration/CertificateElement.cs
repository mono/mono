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

    public sealed partial class CertificateElement : ConfigurationElement
    {
        public CertificateElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.EncodedValue, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string EncodedValue
        {
            get { return (string)base[ConfigurationStrings.EncodedValue]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.EncodedValue] = value;
            }
        }
    }

}
