//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Security.Principal;

    public sealed partial class SecurityIdentifierElement : ConfigurationElement
    {
        public SecurityIdentifierElement()
            : base()
        {
        }

        public SecurityIdentifierElement(SecurityIdentifier sid)
            : this()
        {
            this.SecurityIdentifier = sid;
        }

        [ConfigurationProperty(ConfigurationStrings.SecurityIdentifier, DefaultValue = null, Options = ConfigurationPropertyOptions.IsKey)]
        [TypeConverter(typeof(SecurityIdentifierConverter))]
        public SecurityIdentifier SecurityIdentifier
        {
            get { return (SecurityIdentifier)base[ConfigurationStrings.SecurityIdentifier]; }
            set { base[ConfigurationStrings.SecurityIdentifier] = value; }
        }
    }
}
