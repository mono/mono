//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Net.Security;
    using System.ServiceModel.Channels;
    using Msmq = System.ServiceModel.MsmqIntegration;

    public sealed partial class MsmqIntegrationElement : MsmqElementBase
    {
        public override Type BindingElementType
        {
            get { return typeof(Msmq.MsmqIntegrationBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            MsmqIntegrationElement source = from as MsmqIntegrationElement;
            if (null != source)
                this.SerializationFormat = source.SerializationFormat;
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new Msmq.MsmqIntegrationBindingElement();
        }


        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);

            Msmq.MsmqIntegrationBindingElement binding = bindingElement as Msmq.MsmqIntegrationBindingElement;
            binding.SerializationFormat = this.SerializationFormat;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);

            Msmq.MsmqIntegrationBindingElement binding = bindingElement as Msmq.MsmqIntegrationBindingElement;

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SerializationFormat, binding.SerializationFormat);
        }

        [ConfigurationProperty(ConfigurationStrings.SerializationFormat, DefaultValue = MsmqIntegrationDefaults.SerializationFormat)]
        [ServiceModelEnumValidator(typeof(Msmq.MsmqMessageSerializationFormatHelper))]
        public Msmq.MsmqMessageSerializationFormat SerializationFormat
        {
            get { return (Msmq.MsmqMessageSerializationFormat)base[ConfigurationStrings.SerializationFormat]; }
            set { base[ConfigurationStrings.SerializationFormat] = value; }
        }
    }
}



