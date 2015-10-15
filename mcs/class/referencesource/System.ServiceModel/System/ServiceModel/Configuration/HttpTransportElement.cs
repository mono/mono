//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection.Configuration;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.ComponentModel;

    [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            Justification = "The ExtendedProtectionPolicyElement configuration object and the configuration validation is owned by the NCL team.")]
    public partial class HttpTransportElement : TransportElement
    {
        public HttpTransportElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.AllowCookies, DefaultValue = HttpTransportDefaults.AllowCookies)]
        public bool AllowCookies
        {
            get { return (bool)base[ConfigurationStrings.AllowCookies]; }
            set { base[ConfigurationStrings.AllowCookies] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.RequestInitializationTimeout, DefaultValue = HttpTransportDefaults.RequestInitializationTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan RequestInitializationTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.RequestInitializationTimeout]; }
            set { base[ConfigurationStrings.RequestInitializationTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.AuthenticationScheme, DefaultValue = HttpTransportDefaults.AuthenticationScheme)]
        [StandardRuntimeFlagEnumValidator(typeof(AuthenticationSchemes))]
        public AuthenticationSchemes AuthenticationScheme
        {
            get { return (AuthenticationSchemes)base[ConfigurationStrings.AuthenticationScheme]; }
            set { base[ConfigurationStrings.AuthenticationScheme] = value; }
        }

        public override Type BindingElementType
        {
            get { return typeof(HttpTransportBindingElement); }
        }

        [ConfigurationProperty(ConfigurationStrings.BypassProxyOnLocal, DefaultValue = HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get { return (bool)base[ConfigurationStrings.BypassProxyOnLocal]; }
            set { base[ConfigurationStrings.BypassProxyOnLocal] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.DecompressionEnabled, DefaultValue = HttpTransportDefaults.DecompressionEnabled)]
        public bool DecompressionEnabled
        {
            get { return (bool)base[ConfigurationStrings.DecompressionEnabled]; }
            set { base[ConfigurationStrings.DecompressionEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HostNameComparisonMode, DefaultValue = HttpTransportDefaults.HostNameComparisonMode)]
        [ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper))]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return (HostNameComparisonMode)base[ConfigurationStrings.HostNameComparisonMode]; }
            set { base[ConfigurationStrings.HostNameComparisonMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.KeepAliveEnabled, DefaultValue = HttpTransportDefaults.KeepAliveEnabled)]
        public bool KeepAliveEnabled
        {
            get { return (bool)base[ConfigurationStrings.KeepAliveEnabled]; }
            set { base[ConfigurationStrings.KeepAliveEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBufferSize, DefaultValue = TransportDefaults.MaxBufferSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxBufferSize
        {
            get { return (int)base[ConfigurationStrings.MaxBufferSize]; }
            set { base[ConfigurationStrings.MaxBufferSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingAccepts, DefaultValue = HttpTransportDefaults.DefaultMaxPendingAccepts)]
        [IntegerValidator(MinValue = 0, MaxValue = HttpTransportDefaults.MaxPendingAcceptsUpperLimit)]
        public int MaxPendingAccepts
        {
            get { return (int)base[ConfigurationStrings.MaxPendingAccepts]; }
            set { base[ConfigurationStrings.MaxPendingAccepts] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageHandlerFactory, DefaultValue = HttpTransportDefaults.MessageHandlerFactory)]
        [HttpMessageHandlerFactoryValidator]
        public HttpMessageHandlerFactoryElement MessageHandlerFactory
        {
            get { return (HttpMessageHandlerFactoryElement)this[ConfigurationStrings.MessageHandlerFactory]; }
            set { base[ConfigurationStrings.MessageHandlerFactory] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ProxyAddress, DefaultValue = HttpTransportDefaults.ProxyAddress)]
        public Uri ProxyAddress
        {
            get { return (Uri)base[ConfigurationStrings.ProxyAddress]; }
            set { base[ConfigurationStrings.ProxyAddress] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ProxyAuthenticationScheme, DefaultValue = HttpTransportDefaults.ProxyAuthenticationScheme)]
        [StandardRuntimeEnumValidator(typeof(AuthenticationSchemes))]
        public AuthenticationSchemes ProxyAuthenticationScheme
        {
            get { return (AuthenticationSchemes)base[ConfigurationStrings.ProxyAuthenticationScheme]; }
            set { base[ConfigurationStrings.ProxyAuthenticationScheme] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Realm, DefaultValue = HttpTransportDefaults.Realm)]
        [StringValidator(MinLength = 0)]
        public string Realm
        {
            get { return (string)base[ConfigurationStrings.Realm]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Realm] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.TransferMode, DefaultValue = HttpTransportDefaults.TransferMode)]
        [ServiceModelEnumValidator(typeof(TransferModeHelper))]
        public TransferMode TransferMode
        {
            get { return (TransferMode)base[ConfigurationStrings.TransferMode]; }
            set { base[ConfigurationStrings.TransferMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UnsafeConnectionNtlmAuthentication, DefaultValue = HttpTransportDefaults.UnsafeConnectionNtlmAuthentication)]
        public bool UnsafeConnectionNtlmAuthentication
        {
            get { return (bool)base[ConfigurationStrings.UnsafeConnectionNtlmAuthentication]; }
            set { base[ConfigurationStrings.UnsafeConnectionNtlmAuthentication] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseDefaultWebProxy, DefaultValue = HttpTransportDefaults.UseDefaultWebProxy)]
        public bool UseDefaultWebProxy
        {
            get { return (bool)base[ConfigurationStrings.UseDefaultWebProxy]; }
            set { base[ConfigurationStrings.UseDefaultWebProxy] = value; }
        }


        [ConfigurationProperty(ConfigurationStrings.ExtendedProtectionPolicy)]
        public ExtendedProtectionPolicyElement ExtendedProtectionPolicy
        {
            get { return (ExtendedProtectionPolicyElement)base[ConfigurationStrings.ExtendedProtectionPolicy]; }
            private set { base[ConfigurationStrings.ExtendedProtectionPolicy] = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Configuration, "Configuration104")]
        [ConfigurationProperty(ConfigurationStrings.WebSocketSettingsSectionName)]
        public WebSocketTransportSettingsElement WebSocketSettings
        {
            get { return (WebSocketTransportSettingsElement)base[ConfigurationStrings.WebSocketSettingsSectionName]; }
            set { base[ConfigurationStrings.WebSocketSettingsSectionName] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            HttpTransportBindingElement binding = (HttpTransportBindingElement)bindingElement;

            binding.AllowCookies = this.AllowCookies;
            binding.AuthenticationScheme = this.AuthenticationScheme;
            binding.BypassProxyOnLocal = this.BypassProxyOnLocal;
            binding.DecompressionEnabled = this.DecompressionEnabled;
            binding.KeepAliveEnabled = this.KeepAliveEnabled;
            binding.HostNameComparisonMode = this.HostNameComparisonMode;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MaxBufferSize].ValueOrigin != PropertyValueOrigin.Default)
            {
                binding.MaxBufferSize = this.MaxBufferSize;
            }
            binding.MaxPendingAccepts = this.MaxPendingAccepts;
            binding.ProxyAddress = this.ProxyAddress;
            binding.ProxyAuthenticationScheme = this.ProxyAuthenticationScheme;
            binding.Realm = this.Realm;
            binding.RequestInitializationTimeout = this.RequestInitializationTimeout;
            binding.TransferMode = this.TransferMode;
            binding.UnsafeConnectionNtlmAuthentication = this.UnsafeConnectionNtlmAuthentication;
            binding.UseDefaultWebProxy = this.UseDefaultWebProxy;
            binding.ExtendedProtectionPolicy = ChannelBindingUtility.BuildPolicy(this.ExtendedProtectionPolicy);
            this.WebSocketSettings.ApplyConfiguration(binding.WebSocketSettings);
            if (this.MessageHandlerFactory != null)
            {
                binding.MessageHandlerFactory = HttpMessageHandlerFactory.CreateFromConfigurationElement(this.MessageHandlerFactory);
            }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            HttpTransportElement source = (HttpTransportElement)from;
#pragma warning suppress 56506 // Microsoft, base.CopyFrom() validates the argument
            this.AllowCookies = source.AllowCookies;
            this.RequestInitializationTimeout = source.RequestInitializationTimeout;
            this.AuthenticationScheme = source.AuthenticationScheme;
            this.BypassProxyOnLocal = source.BypassProxyOnLocal;
            this.DecompressionEnabled = source.DecompressionEnabled;
            this.KeepAliveEnabled = source.KeepAliveEnabled;
            this.HostNameComparisonMode = source.HostNameComparisonMode;
            this.MaxBufferSize = source.MaxBufferSize;
            this.MaxPendingAccepts = source.MaxPendingAccepts;
            this.ProxyAddress = source.ProxyAddress;
            this.ProxyAuthenticationScheme = source.ProxyAuthenticationScheme;
            this.Realm = source.Realm;
            this.TransferMode = source.TransferMode;
            this.UnsafeConnectionNtlmAuthentication = source.UnsafeConnectionNtlmAuthentication;
            this.UseDefaultWebProxy = source.UseDefaultWebProxy;
            this.WebSocketSettings = source.WebSocketSettings;
            this.MessageHandlerFactory = source.MessageHandlerFactory;
            ChannelBindingUtility.CopyFrom(source.ExtendedProtectionPolicy, this.ExtendedProtectionPolicy);
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new HttpTransportBindingElement();
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            HttpTransportBindingElement source = (HttpTransportBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AllowCookies, source.AllowCookies);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AuthenticationScheme, source.AuthenticationScheme);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.DecompressionEnabled, source.DecompressionEnabled);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.BypassProxyOnLocal, source.BypassProxyOnLocal);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.KeepAliveEnabled, source.KeepAliveEnabled);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, source.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferSize, source.MaxBufferSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxPendingAccepts, source.MaxPendingAccepts);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProxyAddress, source.ProxyAddress);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProxyAuthenticationScheme, source.ProxyAuthenticationScheme);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Realm, source.Realm);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.RequestInitializationTimeout, source.RequestInitializationTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransferMode, source.TransferMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UnsafeConnectionNtlmAuthentication, source.UnsafeConnectionNtlmAuthentication);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseDefaultWebProxy, source.UseDefaultWebProxy);
            this.WebSocketSettings.InitializeFrom(source.WebSocketSettings);
            if (source.MessageHandlerFactory != null)
            {
                this.MessageHandlerFactory = source.MessageHandlerFactory.GenerateConfigurationElement();
            }

            ChannelBindingUtility.InitializeFrom(source.ExtendedProtectionPolicy, this.ExtendedProtectionPolicy);
        }
    }
}
