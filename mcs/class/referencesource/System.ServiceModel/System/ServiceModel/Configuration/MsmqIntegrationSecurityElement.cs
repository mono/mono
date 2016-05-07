//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.MsmqIntegration;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed partial class MsmqIntegrationSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue = MsmqIntegrationSecurity.DefaultMode)]
        [ServiceModelEnumValidator(typeof(MsmqIntegrationSecurityModeHelper))]
        public MsmqIntegrationSecurityMode Mode
        {
            get { return (MsmqIntegrationSecurityMode)base[ConfigurationStrings.Mode]; }
            set { base[ConfigurationStrings.Mode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Transport)]
        public MsmqTransportSecurityElement Transport
        {
            get { return (MsmqTransportSecurityElement)base[ConfigurationStrings.Transport]; }
        }

        internal void ApplyConfiguration(MsmqIntegrationSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Transport.ApplyConfiguration(security.Transport);
        }

        internal void InitializeFrom(MsmqIntegrationSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Mode, security.Mode);
            this.Transport.InitializeFrom(security.Transport);
        }
    }
}
