//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using System.Globalization;

    public sealed partial class ClaimTypeElement : ConfigurationElement
    {
        public ClaimTypeElement()
        {
        }

        public ClaimTypeElement(string claimType, bool isOptional)
        {
            this.ClaimType = claimType;
            this.IsOptional = isOptional;
        }

        [ConfigurationProperty(ConfigurationStrings.ClaimType, DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string ClaimType
        {
            get { return (string)base[ConfigurationStrings.ClaimType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.ClaimType] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.IsOptional, DefaultValue = ClaimTypeRequirement.DefaultIsOptional)]
        public bool IsOptional
        {
            get { return (bool)base[ConfigurationStrings.IsOptional]; }
            set { base[ConfigurationStrings.IsOptional] = value; }
        }
    }
}



