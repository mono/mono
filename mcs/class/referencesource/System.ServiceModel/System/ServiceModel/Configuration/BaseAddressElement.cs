//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    public sealed partial class BaseAddressElement : ConfigurationElement
    {
        public BaseAddressElement() : base() { }

        // BaseAddress is exposed as a string instead of an Uri so that WCF can do
        // special parsing of wildcards (e.g. '*').
        [ConfigurationProperty(ConfigurationStrings.BaseAddress, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string BaseAddress
        {
            get { return (string)base[ConfigurationStrings.BaseAddress]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BaseAddress] = value;
            }
        }
    }
}
