//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Xml;

    public sealed partial class StandardBindingOptionalReliableSessionElement : StandardBindingReliableSessionElement
    {
        public StandardBindingOptionalReliableSessionElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Enabled, DefaultValue = ReliableSessionDefaults.Enabled)]
        public bool Enabled
        {
            get { return (bool)base[ConfigurationStrings.Enabled]; }
            set { base[ConfigurationStrings.Enabled] = value; }
        }

        public void InitializeFrom(OptionalReliableSession optionalReliableSession)
        {
            if (null == optionalReliableSession)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalReliableSession");
            }
            base.InitializeFrom(optionalReliableSession);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Enabled, optionalReliableSession.Enabled);
        }

        public void ApplyConfiguration(OptionalReliableSession optionalReliableSession)
        {
            if (null == optionalReliableSession)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalReliableSession");
            }
            base.ApplyConfiguration(optionalReliableSession);
            optionalReliableSession.Enabled = this.Enabled;
        }
    }
}

