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

    public partial class MessageSecurityOverHttpElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.ClientCredentialType, DefaultValue = MessageSecurityOverHttp.DefaultClientCredentialType)]
        [ServiceModelEnumValidator(typeof(MessageCredentialTypeHelper))]
        public MessageCredentialType ClientCredentialType
        {
            get { return (MessageCredentialType)base[ConfigurationStrings.ClientCredentialType]; }
            set { base[ConfigurationStrings.ClientCredentialType] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.NegotiateServiceCredential, DefaultValue = MessageSecurityOverHttp.DefaultNegotiateServiceCredential)]
        public bool NegotiateServiceCredential
        {
            get { return (bool)base[ConfigurationStrings.NegotiateServiceCredential]; }
            set { base[ConfigurationStrings.NegotiateServiceCredential] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.AlgorithmSuite, DefaultValue = ConfigurationStrings.Default)]
        [TypeConverter(typeof(SecurityAlgorithmSuiteConverter))]
        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get { return (SecurityAlgorithmSuite)base[ConfigurationStrings.AlgorithmSuite]; }
            set { base[ConfigurationStrings.AlgorithmSuite] = value; }
        }

        internal void ApplyConfiguration(MessageSecurityOverHttp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ClientCredentialType = this.ClientCredentialType;
            security.NegotiateServiceCredential = this.NegotiateServiceCredential;
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.AlgorithmSuite].ValueOrigin)
            {
                security.AlgorithmSuite = this.AlgorithmSuite;
            }
        }

        internal void InitializeFrom(MessageSecurityOverHttp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientCredentialType, security.ClientCredentialType);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.NegotiateServiceCredential, security.NegotiateServiceCredential);
            if (security.WasAlgorithmSuiteSet)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AlgorithmSuite, security.AlgorithmSuite);
            }
        }

        internal MessageSecurityOverHttpElement() { }
    }
}
