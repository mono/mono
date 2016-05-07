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
    public partial class WSHttpContextBindingElement : WSHttpBindingElement
    {
        const string ContextManagementEnabledName = ContextBindingElementExtensionElement.ContextManagementEnabledName;
        const string ContextProtectionLevelName = "contextProtectionLevel";

        public WSHttpContextBindingElement()
            : base()
        {
        }

        public WSHttpContextBindingElement(string name)
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
            get { return typeof(WSHttpContextBinding); }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            WSHttpContextBinding contextBinding = (WSHttpContextBinding)binding;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientCallbackAddressName, contextBinding.ClientCallbackAddress);
            SetPropertyValueIfNotDefaultValue(WSHttpContextBindingElement.ContextManagementEnabledName, contextBinding.ContextManagementEnabled);
            SetPropertyValueIfNotDefaultValue(WSHttpContextBindingElement.ContextProtectionLevelName, contextBinding.ContextProtectionLevel);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            WSHttpContextBinding contextBinding = (WSHttpContextBinding)binding;
            contextBinding.ClientCallbackAddress = this.ClientCallbackAddress;
            contextBinding.ContextProtectionLevel = this.ContextProtectionLevel;
            contextBinding.ContextManagementEnabled = this.ContextManagementEnabled;
        }
    }
}
