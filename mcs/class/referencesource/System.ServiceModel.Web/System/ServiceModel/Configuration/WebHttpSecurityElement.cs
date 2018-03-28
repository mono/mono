//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;

    public sealed partial class WebHttpSecurityElement : ServiceModelConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue = WebHttpSecurity.DefaultMode)]
        [InternalEnumValidator(typeof(WebHttpSecurityModeHelper))]
        public WebHttpSecurityMode Mode
        {
            get { return (WebHttpSecurityMode)base[ConfigurationStrings.Mode]; }
            set { base[ConfigurationStrings.Mode] = value; }
        }


        [ConfigurationProperty(ConfigurationStrings.Transport)]
        public HttpTransportSecurityElement Transport
        {
            get { return (HttpTransportSecurityElement)base[ConfigurationStrings.Transport]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(WebHttpSecurityMode), System.ServiceModel.WebHttpSecurityMode.None, null, new InternalEnumValidator(typeof(WebHttpSecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(HttpTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        internal void ApplyConfiguration(WebHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            if (this.ElementInformation.Properties["mode"].IsModified)
            {
                security.Mode = this.Mode;
                this.Transport.ApplyConfiguration(security.Transport);
            }

        }

        internal void InitializeFrom(WebHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Mode, security.Mode);
            this.InitializeTransportSecurity(security.Transport);
        }

        void ApplyConfiguration(HttpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ClientCredentialType = this.Transport.ClientCredentialType;
            security.ProxyCredentialType = this.Transport.ProxyCredentialType;
            security.Realm = this.Transport.Realm;
        }

        void InitializeTransportSecurity(HttpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            // Can't call this.Transport.SetPropertyValueIfNotDefaultValue directly because it's protected, so we check
            // the defaults here instead.
            if (IsNonDefaultValue(this.Transport, ConfigurationStrings.ClientCredentialType, security.ClientCredentialType))
            {
                this.Transport.ClientCredentialType = security.ClientCredentialType;
            }
            if (IsNonDefaultValue(this.Transport, ConfigurationStrings.ProxyCredentialType, security.ProxyCredentialType))
            {
                this.Transport.ProxyCredentialType = security.ProxyCredentialType;
            }
            if (IsNonDefaultValue(this.Transport, ConfigurationStrings.Realm, security.Realm))
            {
                this.Transport.Realm = security.Realm;
            }
        }

        static bool IsNonDefaultValue<T>(ServiceModelConfigurationElement element, string propertyName, T value)
        {
            PropertyInformation configurationPropertyInfo = element.ElementInformation.Properties[propertyName];
            return configurationPropertyInfo != null && !object.Equals(configurationPropertyInfo.DefaultValue, value);
        }
    }
}
