// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// NetHttpBindingElement for NetHttpBinding
    /// </summary>
    public partial class NetHttpBindingElement : HttpBindingBaseElement
    {
        public NetHttpBindingElement(string name)
            : base(name)
        {
        }

        public NetHttpBindingElement()
            : this(null)
        {
        }

        [ConfigurationProperty(ConfigurationStrings.MessageEncoding, DefaultValue = NetHttpBindingDefaults.MessageEncoding)]
        [ServiceModelEnumValidator(typeof(NetHttpMessageEncodingHelper))]
        public NetHttpMessageEncoding MessageEncoding
        {
            get { return (NetHttpMessageEncoding)base[ConfigurationStrings.MessageEncoding]; }
            set { base[ConfigurationStrings.MessageEncoding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReliableSession)]
        public StandardBindingOptionalReliableSessionElement ReliableSession
        {
            get { return (StandardBindingOptionalReliableSessionElement)base[ConfigurationStrings.ReliableSession]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public BasicHttpSecurityElement Security
        {
            get { return (BasicHttpSecurityElement)base[ConfigurationStrings.Security]; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Configuration, "Configuration104", 
                        Justification = "Don't need a validator for this strong typed element.")]
        [ConfigurationProperty(ConfigurationStrings.WebSocketSettingsSectionName)]
        public NetHttpWebSocketTransportSettingsElement WebSocketSettings
        {
            get { return (NetHttpWebSocketTransportSettingsElement)base[ConfigurationStrings.WebSocketSettingsSectionName]; }
            set { base[ConfigurationStrings.WebSocketSettingsSectionName] = value; }
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetHttpBinding); }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetHttpBinding netHttpBinding = (NetHttpBinding)binding;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MessageEncoding, netHttpBinding.MessageEncoding);

            this.WebSocketSettings.InitializeFrom(netHttpBinding.WebSocketSettings);
            this.ReliableSession.InitializeFrom(netHttpBinding.ReliableSession);
            this.Security.InitializeFrom(netHttpBinding.Security);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            NetHttpBinding netHttpBinding = (NetHttpBinding)binding;
            netHttpBinding.MessageEncoding = this.MessageEncoding;

            this.WebSocketSettings.ApplyConfiguration(netHttpBinding.WebSocketSettings);
            this.ReliableSession.ApplyConfiguration(netHttpBinding.ReliableSession);
            this.Security.ApplyConfiguration(netHttpBinding.Security);
        }
    }
}
