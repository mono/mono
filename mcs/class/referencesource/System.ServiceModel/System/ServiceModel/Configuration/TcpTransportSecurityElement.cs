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
    using System.Security.Authentication.ExtendedProtection.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed partial class TcpTransportSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.ClientCredentialType, DefaultValue = TcpTransportSecurity.DefaultClientCredentialType)]
        [ServiceModelEnumValidator(typeof(TcpClientCredentialTypeHelper))]
        public TcpClientCredentialType ClientCredentialType
        {
            get { return (TcpClientCredentialType)base[ConfigurationStrings.ClientCredentialType]; }
            set { base[ConfigurationStrings.ClientCredentialType] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ProtectionLevel, DefaultValue = TcpTransportSecurity.DefaultProtectionLevel)]
        [ServiceModelEnumValidator(typeof(ProtectionLevelHelper))]
        public ProtectionLevel ProtectionLevel
        {
            get { return (ProtectionLevel)base[ConfigurationStrings.ProtectionLevel]; }
            set { base[ConfigurationStrings.ProtectionLevel] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ExtendedProtectionPolicy)]
        public ExtendedProtectionPolicyElement ExtendedProtectionPolicy
        {
            get { return (ExtendedProtectionPolicyElement)base[ConfigurationStrings.ExtendedProtectionPolicy]; }
            private set { base[ConfigurationStrings.ExtendedProtectionPolicy] = value; }
        }

        internal void ApplyConfiguration(TcpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ClientCredentialType = this.ClientCredentialType;
            security.ProtectionLevel = this.ProtectionLevel;
            security.ExtendedProtectionPolicy = ChannelBindingUtility.BuildPolicy(this.ExtendedProtectionPolicy);
        }

        internal void InitializeFrom(TcpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientCredentialType, security.ClientCredentialType);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProtectionLevel, security.ProtectionLevel);
            ChannelBindingUtility.InitializeFrom(security.ExtendedProtectionPolicy, this.ExtendedProtectionPolicy);
        }
    }
}
