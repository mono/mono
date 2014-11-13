//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Xml;
    using System.ComponentModel;

    public sealed class TextMessageEncodingBindingElement : MessageEncodingBindingElement, IWsdlExportExtension, IPolicyExportExtension
    {
        int maxReadPoolSize;
        int maxWritePoolSize;
        XmlDictionaryReaderQuotas readerQuotas;
        MessageVersion messageVersion;
        Encoding writeEncoding;

        public TextMessageEncodingBindingElement()
            : this(MessageVersion.Default, TextEncoderDefaults.Encoding)
        {
        }

        public TextMessageEncodingBindingElement(MessageVersion messageVersion, Encoding writeEncoding)
        {
            if (messageVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");

            if (writeEncoding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");

            TextEncoderDefaults.ValidateEncoding(writeEncoding);

            this.maxReadPoolSize = EncoderDefaults.MaxReadPoolSize;
            this.maxWritePoolSize = EncoderDefaults.MaxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            this.messageVersion = messageVersion;
            this.writeEncoding = writeEncoding;
        }

        TextMessageEncodingBindingElement(TextMessageEncodingBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.maxReadPoolSize = elementToBeCloned.maxReadPoolSize;
            this.maxWritePoolSize = elementToBeCloned.maxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            elementToBeCloned.readerQuotas.CopyTo(this.readerQuotas);
            this.writeEncoding = elementToBeCloned.writeEncoding;
            this.messageVersion = elementToBeCloned.messageVersion;
        }

        [DefaultValue(EncoderDefaults.MaxReadPoolSize)]
        public int MaxReadPoolSize
        {
            get
            {
                return this.maxReadPoolSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBePositive)));
                }
                this.maxReadPoolSize = value;
            }
        }

        [DefaultValue(EncoderDefaults.MaxWritePoolSize)]
        public int MaxWritePoolSize
        {
            get
            {
                return this.maxWritePoolSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBePositive)));
                }
                this.maxWritePoolSize = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                value.CopyTo(this.readerQuotas);
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.messageVersion = value;
            }
        }

        [TypeConverter(typeof(System.ServiceModel.Configuration.EncodingConverter))]
        public Encoding WriteEncoding
        {
            get
            {
                return this.writeEncoding;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                TextEncoderDefaults.ValidateEncoding(value);
                this.writeEncoding = value;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return InternalBuildChannelFactory<TChannel>(context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            return InternalBuildChannelListener<TChannel>(context);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return InternalCanBuildChannelListener<TChannel>(context);
        }

        public override BindingElement Clone()
        {
            return new TextMessageEncodingBindingElement(this);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new TextMessageEncoderFactory(MessageVersion, WriteEncoding, this.MaxReadPoolSize, this.MaxWritePoolSize, this.ReaderQuotas);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return (T)(object)this.readerQuotas;
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context) { }
        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            SoapHelper.SetSoapVersion(context, exporter, this.messageVersion.Envelope);
        }

        internal override bool CheckEncodingVersion(EnvelopeVersion version)
        {
            return messageVersion.Envelope == version;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
                return false;

            TextMessageEncodingBindingElement text = b as TextMessageEncodingBindingElement;
            if (text == null)
                return false;
            if (this.maxReadPoolSize != text.MaxReadPoolSize)
                return false;
            if (this.maxWritePoolSize != text.MaxWritePoolSize)
                return false;

            // compare XmlDictionaryReaderQuotas
            if (this.readerQuotas.MaxStringContentLength != text.ReaderQuotas.MaxStringContentLength)
                return false;
            if (this.readerQuotas.MaxArrayLength != text.ReaderQuotas.MaxArrayLength)
                return false;
            if (this.readerQuotas.MaxBytesPerRead != text.ReaderQuotas.MaxBytesPerRead)
                return false;
            if (this.readerQuotas.MaxDepth != text.ReaderQuotas.MaxDepth)
                return false;
            if (this.readerQuotas.MaxNameTableCharCount != text.ReaderQuotas.MaxNameTableCharCount)
                return false;

            if (this.WriteEncoding.EncodingName != text.WriteEncoding.EncodingName)
                return false;
            if (!this.MessageVersion.IsMatch(text.MessageVersion))
                return false;

            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeWriteEncoding()
        {
            return (this.WriteEncoding != TextEncoderDefaults.Encoding);
        }
    }
}
