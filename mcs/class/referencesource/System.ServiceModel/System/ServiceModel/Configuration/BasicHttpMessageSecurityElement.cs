//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed partial class BasicHttpMessageSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.ClientCredentialType, DefaultValue = BasicHttpMessageSecurity.DefaultClientCredentialType)]
        [ServiceModelEnumValidator(typeof(BasicHttpMessageCredentialTypeHelper))]
        public BasicHttpMessageCredentialType ClientCredentialType
        {
            get { return (BasicHttpMessageCredentialType)base[ConfigurationStrings.ClientCredentialType]; }
            set { base[ConfigurationStrings.ClientCredentialType] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.AlgorithmSuite, DefaultValue = ConfigurationStrings.Default)]
        [TypeConverter(typeof(SecurityAlgorithmSuiteConverter))]
        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get { return (SecurityAlgorithmSuite)base[ConfigurationStrings.AlgorithmSuite]; }
            set { base[ConfigurationStrings.AlgorithmSuite] = value; }
        }

        internal void ApplyConfiguration(BasicHttpMessageSecurity security)
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

        internal void InitializeFrom(BasicHttpMessageSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientCredentialType, security.ClientCredentialType);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AlgorithmSuite, security.AlgorithmSuite);
        }
    }
}
