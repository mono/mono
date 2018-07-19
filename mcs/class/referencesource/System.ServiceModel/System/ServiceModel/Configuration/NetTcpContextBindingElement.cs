//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public partial class NetTcpContextBindingElement : NetTcpBindingElement
    {
        const string ContextManagementEnabledName = ContextBindingElementExtensionElement.ContextManagementEnabledName;
        const string ContextProtectionLevelName = "contextProtectionLevel";

        public NetTcpContextBindingElement()
            : base()
        {
        }

        public NetTcpContextBindingElement(string name)
            : base(name)
        {
        }

        [ConfigurationProperty(ConfigurationStrings.ClientCallbackAddressName, DefaultValue = null)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            Justification = "Is of type Uri, we don't have a validator for it")]
        public Uri ClientCallbackAddress
        {
            get { return (Uri)base[ConfigurationStrings.ClientCallbackAddressName]; }
            set { base[ConfigurationStrings.ClientCallbackAddressName] = value; }
        }

        [ConfigurationProperty(ContextManagementEnabledName, DefaultValue = ContextBindingElement.DefaultContextManagementEnabled)]
        public bool ContextManagementEnabled
        {
            get { return (bool)base[ContextManagementEnabledName]; }
            set { base[ContextManagementEnabledName] = value; }
        }

        [ConfigurationProperty(ContextProtectionLevelName, DefaultValue = ContextBindingElement.DefaultProtectionLevel)]
        [ServiceModelEnumValidator(typeof(ProtectionLevelHelper))]
        public ProtectionLevel ContextProtectionLevel
        {
            get { return (ProtectionLevel) base[ContextProtectionLevelName]; }
            set { base[ContextProtectionLevelName] = value; }
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetTcpContextBinding); }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetTcpContextBinding netTcpContextBinding = (NetTcpContextBinding)binding;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientCallbackAddressName, netTcpContextBinding.ClientCallbackAddress);
            SetPropertyValueIfNotDefaultValue(NetTcpContextBindingElement.ContextManagementEnabledName, netTcpContextBinding.ContextManagementEnabled);
            SetPropertyValueIfNotDefaultValue(NetTcpContextBindingElement.ContextProtectionLevelName, netTcpContextBinding.ContextProtectionLevel);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            NetTcpContextBinding netTcpContextBinding = (NetTcpContextBinding)binding;
            netTcpContextBinding.ClientCallbackAddress = this.ClientCallbackAddress;
            netTcpContextBinding.ContextManagementEnabled = this.ContextManagementEnabled;
            netTcpContextBinding.ContextProtectionLevel = this.ContextProtectionLevel;
        }
    }
}
