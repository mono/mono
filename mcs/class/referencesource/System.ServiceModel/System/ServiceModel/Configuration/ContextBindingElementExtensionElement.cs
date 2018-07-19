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
    public partial class ContextBindingElementExtensionElement : BindingElementExtensionElement
    {
        internal const string ContextExchangeMechanismName = "contextExchangeMechanism";
        internal const string ContextManagementEnabledName = "contextManagementEnabled";
        const string ProtectionLevelName = "protectionLevel";

        public ContextBindingElementExtensionElement()
            : base()
        {
        }


        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule, MessageId = "System.ServiceModel.Configuration.ContextBindingElementExtensionElement.BindingElementType",
            Justification = "This property is not supposed to be exposed in config.")]
        public override Type BindingElementType
        {
            get { return typeof(ContextBindingElement); }
        }

        [ConfigurationProperty(ConfigurationStrings.ClientCallbackAddressName, DefaultValue = null)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            Justification = "Is of type Uri, we don't have a validator for it")]
        public Uri ClientCallbackAddress
        {
            get { return (Uri)base[ConfigurationStrings.ClientCallbackAddressName]; }
            set { base[ConfigurationStrings.ClientCallbackAddressName] = value; }
        }

        [ConfigurationProperty(ContextExchangeMechanismName, DefaultValue = ContextBindingElement.DefaultContextExchangeMechanism)]
        [ServiceModelEnumValidator(typeof(ContextExchangeMechanismHelper))]
        public ContextExchangeMechanism ContextExchangeMechanism
        {
            get { return (ContextExchangeMechanism)base[ContextExchangeMechanismName]; }
            set { base[ContextExchangeMechanismName] = value; }
        }

        [ConfigurationProperty(ProtectionLevelName, DefaultValue = ContextBindingElement.DefaultProtectionLevel)]
        [ServiceModelEnumValidator(typeof(ProtectionLevelHelper))]
        public ProtectionLevel ProtectionLevel
        {
            get { return (ProtectionLevel) base[ProtectionLevelName]; }
            set { base[ProtectionLevelName] = value; }
        }

        [ConfigurationProperty(ContextManagementEnabledName, DefaultValue = ContextBindingElement.DefaultContextManagementEnabled)]
        public bool ContextManagementEnabled
        {
            get { return (bool)base[ContextManagementEnabledName]; }
            set { base[ContextManagementEnabledName] = value; }
        }

        protected internal override BindingElement CreateBindingElement()
        {
            return new ContextBindingElement(this.ProtectionLevel, this.ContextExchangeMechanism, this.ClientCallbackAddress, this.ContextManagementEnabled);
        }
    }
}
