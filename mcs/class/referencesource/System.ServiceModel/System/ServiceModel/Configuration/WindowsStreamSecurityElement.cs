//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Security;
    using System.Text;
    using System.ServiceModel.Channels;

    public sealed partial class WindowsStreamSecurityElement : BindingElementExtensionElement
    {
        public WindowsStreamSecurityElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.ProtectionLevel, DefaultValue = ConnectionOrientedTransportDefaults.ProtectionLevel)]
        [StandardRuntimeEnumValidator(typeof(ProtectionLevel))]
        public ProtectionLevel ProtectionLevel
        {
            get { return (ProtectionLevel)base[ConfigurationStrings.ProtectionLevel]; }
            set { base[ConfigurationStrings.ProtectionLevel] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            WindowsStreamSecurityBindingElement windowsBindingElement =
                (WindowsStreamSecurityBindingElement)bindingElement;
            windowsBindingElement.ProtectionLevel = this.ProtectionLevel;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            WindowsStreamSecurityBindingElement windowsBindingElement
                = new WindowsStreamSecurityBindingElement();

            this.ApplyConfiguration(windowsBindingElement);
            return windowsBindingElement;
        }

        public override Type BindingElementType
        {
            get { return typeof(WindowsStreamSecurityBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            WindowsStreamSecurityElement source = (WindowsStreamSecurityElement)from;
#pragma warning suppress 56506 // [....], base.CopyFrom() validates the argument
            this.ProtectionLevel = source.ProtectionLevel;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            WindowsStreamSecurityBindingElement windowsBindingElement
                = (WindowsStreamSecurityBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProtectionLevel, windowsBindingElement.ProtectionLevel);
        }
    }
}



