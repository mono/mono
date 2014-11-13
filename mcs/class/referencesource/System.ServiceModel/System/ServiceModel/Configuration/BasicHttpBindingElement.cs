//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public partial class BasicHttpBindingElement : HttpBindingBaseElement
    {
        public BasicHttpBindingElement(string name)
            : base(name)
        {
        }

        public BasicHttpBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(BasicHttpBinding); }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageEncoding, DefaultValue = BasicHttpBindingDefaults.MessageEncoding)]
        [ServiceModelEnumValidator(typeof(WSMessageEncodingHelper))]
        public WSMessageEncoding MessageEncoding
        {
            get { return (WSMessageEncoding)base[ConfigurationStrings.MessageEncoding]; }
            set { base[ConfigurationStrings.MessageEncoding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public BasicHttpSecurityElement Security
        {
            get { return (BasicHttpSecurityElement)base[ConfigurationStrings.Security]; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            BasicHttpBinding bpBinding = (BasicHttpBinding)binding;

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MessageEncoding, bpBinding.MessageEncoding);
            this.Security.InitializeFrom(bpBinding.Security);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            BasicHttpBinding bpBinding = (BasicHttpBinding)binding;
            bpBinding.MessageEncoding = this.MessageEncoding;
            this.Security.ApplyConfiguration(bpBinding.Security);
        }
    }
}
