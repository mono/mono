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
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed partial class WSFederationHttpSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue = WSFederationHttpSecurity.DefaultMode)]
        [ServiceModelEnumValidator(typeof(WSFederationHttpSecurityModeHelper))]
        public WSFederationHttpSecurityMode Mode
        {
            get { return (WSFederationHttpSecurityMode)base[ConfigurationStrings.Mode]; }
            set { base[ConfigurationStrings.Mode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Message)]
        public FederatedMessageSecurityOverHttpElement Message
        {
            get { return (FederatedMessageSecurityOverHttpElement)base[ConfigurationStrings.Message]; }
        }

        internal void ApplyConfiguration(WSFederationHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Message.ApplyConfiguration(security.Message);
        }

        internal void InitializeFrom(WSFederationHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Mode, security.Mode);
            this.Message.InitializeFrom(security.Message);
        }
    }
}
