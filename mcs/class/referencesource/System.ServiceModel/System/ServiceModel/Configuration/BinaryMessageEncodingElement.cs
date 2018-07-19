//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed partial class BinaryMessageEncodingElement : BindingElementExtensionElement
    {
        public BinaryMessageEncodingElement()
        {
        }

        public override Type BindingElementType
        {
            get { return typeof(BinaryMessageEncodingBindingElement); }
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

        [ConfigurationProperty(ConfigurationStrings.MaxSessionSize, DefaultValue = BinaryEncoderDefaults.MaxSessionSize)]
        [IntegerValidator(MinValue = 0)]
        public int MaxSessionSize
        {
            get { return (int)base[ConfigurationStrings.MaxSessionSize]; }
            set { base[ConfigurationStrings.MaxSessionSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement) base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStrings.CompressionFormat, DefaultValue = EncoderDefaults.DefaultCompressionFormat)]
        [ServiceModelEnumValidator(typeof(CompressionFormatHelper))]
        public CompressionFormat CompressionFormat
        {
            get { return (CompressionFormat)base[ConfigurationStrings.CompressionFormat]; }
            set { base[ConfigurationStrings.CompressionFormat] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            BinaryMessageEncodingBindingElement binding = (BinaryMessageEncodingBindingElement)bindingElement;
            binding.MaxSessionSize = this.MaxSessionSize;
            binding.MaxReadPoolSize = this.MaxReadPoolSize;
            binding.MaxWritePoolSize = this.MaxWritePoolSize;
#pragma warning suppress 56506 //Microsoft; base.ApplyConfiguration() checks for 'binding' being null
            this.ReaderQuotas.ApplyConfiguration(binding.ReaderQuotas);
            binding.CompressionFormat = this.CompressionFormat;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            BinaryMessageEncodingElement source = (BinaryMessageEncodingElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.MaxSessionSize = source.MaxSessionSize;
            this.MaxReadPoolSize = source.MaxReadPoolSize;
            this.MaxWritePoolSize = source.MaxWritePoolSize;
            this.CompressionFormat = source.CompressionFormat;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            BinaryMessageEncodingBindingElement binding = (BinaryMessageEncodingBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxSessionSize, binding.MaxSessionSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReadPoolSize, binding.MaxReadPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxWritePoolSize, binding.MaxWritePoolSize);
            this.ReaderQuotas.InitializeFrom(binding.ReaderQuotas);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.CompressionFormat, binding.CompressionFormat);
        }

        protected internal override BindingElement CreateBindingElement()
        {
            BinaryMessageEncodingBindingElement binding = new BinaryMessageEncodingBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }
    }
}

