//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    public sealed partial class BaseAddressPrefixFilterElement : ConfigurationElement
    {
        public BaseAddressPrefixFilterElement()
        {
        }

        public BaseAddressPrefixFilterElement(Uri prefix)
            : this()
        {
            if (prefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }
            this.Prefix = prefix;
        }

        [ConfigurationProperty(ConfigurationStrings.Prefix, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        public Uri Prefix
        {
            get { return (Uri)base[ConfigurationStrings.Prefix]; }
            set { base[ConfigurationStrings.Prefix] = value; }
        }
    }
}
