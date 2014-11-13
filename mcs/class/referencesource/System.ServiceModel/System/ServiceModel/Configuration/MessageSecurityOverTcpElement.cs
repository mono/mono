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

    public sealed partial class MessageSecurityOverTcpElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.ClientCredentialType, DefaultValue = MessageSecurityOverTcp.DefaultClientCredentialType)]
        [ServiceModelEnumValidator(typeof(MessageCredentialTypeHelper))]
        public MessageCredentialType ClientCredentialType
        {
            get { return (MessageCredentialType)base[ConfigurationStrings.ClientCredentialType]; }
            set { base[ConfigurationStrings.ClientCredentialType] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.AlgorithmSuite, DefaultValue = ConfigurationStrings.Default)]
        [TypeConverter(typeof(SecurityAlgorithmSuiteConverter))]
        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get { return (SecurityAlgorithmSuite)base[ConfigurationStrings.AlgorithmSuite]; }
            set { base[ConfigurationStrings.AlgorithmSuite] = value; }
        }

        internal void ApplyConfiguration(MessageSecurityOverTcp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ClientCredentialType = this.ClientCredentialType;
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.AlgorithmSuite].ValueOrigin)
            {
                security.AlgorithmSuite = this.AlgorithmSuite;
            }
        }

        internal void InitializeFrom(MessageSecurityOverTcp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientCredentialType, security.ClientCredentialType);
            if (security.WasAlgorithmSuiteSet)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AlgorithmSuite, security.AlgorithmSuite);
            }
        }
    }
}
