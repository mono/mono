// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

#pragma warning disable 1634 // Stops compiler from warning about unknown warnings (for Presharp)

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    public sealed partial class WebMessageEncodingElement : BindingElementExtensionElement
    {

        const string ConfigurationStringsWebContentTypeMapperType = "webContentTypeMapperType";
        public WebMessageEncodingElement()
        {
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Configuration.WebMessageEncodingElement.BindingElementType",
            Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BindingElementType
        {
            get { return typeof(WebMessageEncodingBindingElement); }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxReadPoolSize, DefaultValue = EncoderDefaults.MaxReadPoolSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxReadPoolSize
        {
            get { return (int) base[ConfigurationStrings.MaxReadPoolSize]; }
            set { base[ConfigurationStrings.MaxReadPoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxWritePoolSize, DefaultValue = EncoderDefaults.MaxWritePoolSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxWritePoolSize
        {
            get { return (int) base[ConfigurationStrings.MaxWritePoolSize]; }
            set { base[ConfigurationStrings.MaxWritePoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement) base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStringsWebContentTypeMapperType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string WebContentTypeMapperType
        {
            get { return (string) base[ConfigurationStringsWebContentTypeMapperType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStringsWebContentTypeMapperType] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.WriteEncoding, DefaultValue = TextEncoderDefaults.EncodingString)]
        [TypeConverter(typeof(EncodingConverter))]
        [WebEncodingValidator]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule", MessageId = "System.ServiceModel.Configuration.WebMessageEncodingElement.WriteEncoding",
            Justification = "Bug with internal FxCop assembly flags this property as not having a validator.")]
        public Encoding WriteEncoding
        {
            get { return (Encoding) base[ConfigurationStrings.WriteEncoding]; }
            set { base[ConfigurationStrings.WriteEncoding] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            WebMessageEncodingBindingElement binding = (WebMessageEncodingBindingElement) bindingElement;
            binding.WriteEncoding = this.WriteEncoding;
            binding.MaxReadPoolSize = this.MaxReadPoolSize;
            binding.MaxWritePoolSize = this.MaxWritePoolSize;
            if (!string.IsNullOrEmpty(this.WebContentTypeMapperType))
            {
                Type CTMType = Type.GetType(this.WebContentTypeMapperType, true);
                if (!typeof(WebContentTypeMapper).IsAssignableFrom(CTMType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR2.GetString(SR2.ConfigInvalidWebContentTypeMapper,
                        CTMType,
                        ConfigurationStringsWebContentTypeMapperType,
                        typeof(WebMessageEncodingBindingElement),
                        typeof(WebContentTypeMapper))));
                }
                try
                {
                    binding.ContentTypeMapper = (WebContentTypeMapper) Activator.CreateInstance(CTMType);
                }
                catch (MissingMethodException innerException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR2.GetString(SR2.ConfigWebContentTypeMapperNoConstructor,
                        CTMType,
                        ConfigurationStringsWebContentTypeMapperType,
                        typeof(WebMessageEncodingBindingElement),
                        typeof(WebContentTypeMapper)),
                        innerException));
                }
            }
#pragma warning suppress 56506 // bindingElement is checked for null in base.ApplyConfiguration()
            ApplyConfiguration(this.ReaderQuotas, binding.ReaderQuotas);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            WebMessageEncodingElement source = (WebMessageEncodingElement) from;
#pragma warning suppress 56506 // base.CopyFrom() checks for 'from' being null
            this.WriteEncoding = source.WriteEncoding;
            this.MaxReadPoolSize = source.MaxReadPoolSize;
            this.MaxWritePoolSize = source.MaxWritePoolSize;
            this.WebContentTypeMapperType = source.WebContentTypeMapperType;
            this.ReaderQuotas.MaxArrayLength = source.ReaderQuotas.MaxArrayLength;
            this.ReaderQuotas.MaxBytesPerRead = source.ReaderQuotas.MaxBytesPerRead;
            this.ReaderQuotas.MaxDepth = source.ReaderQuotas.MaxDepth;
            this.ReaderQuotas.MaxNameTableCharCount = source.ReaderQuotas.MaxNameTableCharCount;
            this.ReaderQuotas.MaxStringContentLength = source.ReaderQuotas.MaxStringContentLength;
        }

        internal protected override BindingElement CreateBindingElement()
        {
            WebMessageEncodingBindingElement binding = new WebMessageEncodingBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        internal void ApplyConfiguration(XmlDictionaryReaderQuotasElement currentQuotas, XmlDictionaryReaderQuotas readerQuotas)
        {
            if (readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("readerQuotas");
            }
            if (currentQuotas.MaxDepth != 0)
            {
                readerQuotas.MaxDepth = currentQuotas.MaxDepth;
            }
            if (currentQuotas.MaxStringContentLength != 0)
            {
                readerQuotas.MaxStringContentLength = currentQuotas.MaxStringContentLength;
            }
            if (currentQuotas.MaxArrayLength != 0)
            {
                readerQuotas.MaxArrayLength = currentQuotas.MaxArrayLength;
            }
            if (currentQuotas.MaxBytesPerRead != 0)
            {
                readerQuotas.MaxBytesPerRead = currentQuotas.MaxBytesPerRead;
            }
            if (currentQuotas.MaxNameTableCharCount != 0)
            {
                readerQuotas.MaxNameTableCharCount = currentQuotas.MaxNameTableCharCount;
            }
        }

    }
}

