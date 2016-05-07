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
    /// NetHttpsBindingElement for NetHttpsBinding
    /// </summary>
    public partial class NetHttpsBindingElement : HttpBindingBaseElement
    {
        public NetHttpsBindingElement(string name)
            : base(name)
        {
        }

        public NetHttpsBindingElement()
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
        public BasicHttpsSecurityElement Security
        {
            get { return (BasicHttpsSecurityElement)base[ConfigurationStrings.Security]; }
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
            get { return typeof(NetHttpsBinding); }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetHttpsBinding netHttpsBinding = (NetHttpsBinding)binding;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MessageEncoding, netHttpsBinding.MessageEncoding);

            this.WebSocketSettings.InitializeFrom(netHttpsBinding.WebSocketSettings);
            this.ReliableSession.InitializeFrom(netHttpsBinding.ReliableSession);
            this.Security.InitializeFrom(netHttpsBinding.Security);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            NetHttpsBinding netHttpsBinding = (NetHttpsBinding)binding;
            netHttpsBinding.MessageEncoding = this.MessageEncoding;

            this.WebSocketSettings.ApplyConfiguration(netHttpsBinding.WebSocketSettings);
            this.ReliableSession.ApplyConfiguration(netHttpsBinding.ReliableSession);
            this.Security.ApplyConfiguration(netHttpsBinding.Security);
        }
    }
}
