//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.ServiceModel.Channels;

    public sealed partial class TextMessageEncodingElement : BindingElementExtensionElement
    {
        public TextMessageEncodingElement()
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            TextMessageEncodingBindingElement binding = (TextMessageEncodingBindingElement)bindingElement;
            binding.MessageVersion = this.MessageVersion;
            binding.WriteEncoding = this.WriteEncoding;
            binding.MaxReadPoolSize = this.MaxReadPoolSize;
            binding.MaxWritePoolSize = this.MaxWritePoolSize;
#pragma warning suppress 56506 //Microsoft; base.ApplyConfiguration() checks for 'binding' being null
            this.ReaderQuotas.ApplyConfiguration(binding.ReaderQuotas);
        }

        public override Type BindingElementType
        {
            get { return typeof(TextMessageEncodingBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            TextMessageEncodingElement source = (TextMessageEncodingElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.MessageVersion = source.MessageVersion;
            this.WriteEncoding = source.WriteEncoding;
            this.MaxReadPoolSize = source.MaxReadPoolSize;
            this.MaxWritePoolSize = source.MaxWritePoolSize;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            TextMessageEncodingBindingElement binding = new TextMessageEncodingBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            TextMessageEncodingBindingElement binding = (TextMessageEncodingBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MessageVersion, binding.MessageVersion);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.WriteEncoding, binding.WriteEncoding);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReadPoolSize, binding.MaxReadPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxWritePoolSize, binding.MaxWritePoolSize);
            this.ReaderQuotas.InitializeFrom(binding.ReaderQuotas);
        }

        [ConfigurationProperty(ConfigurationStrings.MaxReadPoolSize, DefaultValue = EncoderDefaults.MaxReadPoolSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxReadPoolSize
        {
            get { return (int)base[ConfigurationStrings.MaxReadPoolSize]; }
            set { base[ConfigurationStrings.MaxReadPoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxWritePoolSize, DefaultValue = EncoderDefaults.MaxWritePoolSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxWritePoolSize
        {
            get { return (int)base[ConfigurationStrings.MaxWritePoolSize]; }
            set { base[ConfigurationStrings.MaxWritePoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageVersion, DefaultValue = TextEncoderDefaults.MessageVersionString)]
        [TypeConverter(typeof(MessageVersionConverter))]
        public MessageVersion MessageVersion
        {
            get { return (MessageVersion)base[ConfigurationStrings.MessageVersion]; }
            set { base[ConfigurationStrings.MessageVersion] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement) base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WriteEncoding, DefaultValue = TextEncoderDefaults.EncodingString)]
        [TypeConverter(typeof(EncodingConverter))]
        public Encoding WriteEncoding
        {
            get { return (Encoding)base[ConfigurationStrings.WriteEncoding]; }
            set { base[ConfigurationStrings.WriteEncoding] = value; }
        }
    }
}



