//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace System.IdentityModel.Configuration
{
#pragma warning disable 1591
    /// <summary>
    /// Manages the configuration of an audience uri element within the audienceUris
    /// configuration collection.
    /// </summary>
    public sealed partial class AudienceUriElement : ConfigurationElement
    {
        const string DefaultValue = " ";

        [ConfigurationProperty(ConfigurationStrings.Value, IsRequired = true, DefaultValue = DefaultValue, IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Value
        {
            get { return (string)this[ConfigurationStrings.Value]; }
            set { this[ConfigurationStrings.Value] = value; }
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return ElementInformation.Properties[ConfigurationStrings.Value].ValueOrigin != PropertyValueOrigin.Default;
            }
        }
    }
#pragma warning restore 1591
}
