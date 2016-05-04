//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    public sealed partial class AllowedAudienceUriElement : ConfigurationElement
    {
        public AllowedAudienceUriElement() : base() { }

        // AudienceUri is exposed as a string instead of an Uri so that WCF can do as there is no standard for the actual string.
        [ConfigurationProperty(ConfigurationStrings.AllowedAudienceUri, Options = ConfigurationPropertyOptions.IsKey )]
        [StringValidator(MinLength = 1)]
        public string AllowedAudienceUri
        {
            get { return (string)base[ConfigurationStrings.AllowedAudienceUri]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.AllowedAudienceUri] = value;
            }
        }

    }
}
