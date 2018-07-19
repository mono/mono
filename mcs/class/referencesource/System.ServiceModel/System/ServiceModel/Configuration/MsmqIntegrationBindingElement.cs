//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel.Security;
    using System.ServiceModel.Channels;
    using System.ServiceModel.MsmqIntegration;
    using System.Net.Security;

    public partial class MsmqIntegrationBindingElement : MsmqBindingElementBase
    {
        public MsmqIntegrationBindingElement(string name)
            : base(name)
        {
        }

        public MsmqIntegrationBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(MsmqIntegrationBinding); }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public MsmqIntegrationSecurityElement Security
        {
            get { return (MsmqIntegrationSecurityElement)base[ConfigurationStrings.Security]; }
        }

        [ConfigurationProperty(ConfigurationStrings.SerializationFormat, DefaultValue = MsmqIntegrationDefaults.SerializationFormat)]
        [ServiceModelEnumValidator(typeof(MsmqMessageSerializationFormatHelper))]
        public MsmqMessageSerializationFormat SerializationFormat
        {
            get { return (MsmqMessageSerializationFormat)base[ConfigurationStrings.SerializationFormat]; }
            set { base[ConfigurationStrings.SerializationFormat] = value; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            MsmqIntegrationBinding miBinding = (MsmqIntegrationBinding) binding;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SerializationFormat, miBinding.SerializationFormat);
            this.Security.InitializeFrom(miBinding.Security);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            MsmqIntegrationBinding miBinding = (MsmqIntegrationBinding) binding;
            miBinding.SerializationFormat = this.SerializationFormat;
            this.Security.ApplyConfiguration(miBinding.Security);
        }
    }
}
