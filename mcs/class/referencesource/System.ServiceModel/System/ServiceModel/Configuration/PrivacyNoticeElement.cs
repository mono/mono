//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public partial class PrivacyNoticeElement : BindingElementExtensionElement
    {
        [ConfigurationProperty(ConfigurationStrings.Url)]
        public Uri Url
        {
            get { return (Uri)base[ConfigurationStrings.Url]; }
            set { base[ConfigurationStrings.Url] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Version, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int Version
        {
            get { return (int)base[ConfigurationStrings.Version]; }
            set { base[ConfigurationStrings.Version] = value; }
        }

        public override Type BindingElementType
        {
            get { return typeof( PrivacyNoticeBindingElement ); }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            PrivacyNoticeBindingElement binding = (PrivacyNoticeBindingElement)bindingElement;
            binding.Url = this.Url;
            binding.Version = this.Version;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            PrivacyNoticeBindingElement binding = new PrivacyNoticeBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            PrivacyNoticeElement source = (PrivacyNoticeElement) from;
    #pragma warning suppress 56506 // Microsoft, base.CopyFrom() validates the argument
            this.Url = source.Url;
            this.Version = source.Version;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            PrivacyNoticeBindingElement binding = (PrivacyNoticeBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Url, binding.Url);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Version, binding.Version);
        }

    }
}
