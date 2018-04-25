//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.ComponentModel;

    public sealed class MtomMessageEncodingBindingElement : MessageEncodingBindingElement, IWsdlExportExtension, IPolicyExportExtension
    {
        int maxReadPoolSize;
        int maxWritePoolSize;
        XmlDictionaryReaderQuotas readerQuotas;
        int maxBufferSize;
        Encoding writeEncoding;
        MessageVersion messageVersion;

        public MtomMessageEncodingBindingElement()
            : this(MessageVersion.Default, TextEncoderDefaults.Encoding)
        {
        }

        public MtomMessageEncodingBindingElement(MessageVersion messageVersion, Encoding writeEncoding)
        {
            if (messageVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");

            if (messageVersion == MessageVersion.None)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MtomEncoderBadMessageVersion, messageVersion.ToString()), "messageVersion"));

            if (writeEncoding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");

            TextEncoderDefaults.ValidateEncoding(writeEncoding);
            this.maxReadPoolSize = EncoderDefaults.MaxReadPoolSize;
            this.maxWritePoolSize = EncoderDefaults.MaxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            this.maxBufferSize = MtomEncoderDefaults.MaxBufferSize;
            this.messageVersion = messageVersion;
            this.writeEncoding = writeEncoding;
        }

        MtomMessageEncodingBindingElement(MtomMessageEncodingBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.maxReadPoolSize = elementToBeCloned.maxReadPoolSize;
            this.maxWritePoolSize = elementToBeCloned.maxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            elementToBeCloned.readerQuotas.CopyTo(this.readerQuotas);
            this.maxBufferSize = elementToBeCloned.maxBufferSize;
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
        }

        [DefaultValue(MtomEncoderDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBePositive)));
                }
                this.maxBufferSize = value;
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                TextEncoderDefaults.ValidateEncoding(value);
                this.writeEncoding = value;
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
                if (value == MessageVersion.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MtomEncoderBadMessageVersion, value.ToString()), "value"));
                }

                this.messageVersion = value;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return InternalBuildChannelFactory<TChannel>(context);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return InternalCanBuildChannelFactory<TChannel>(context);
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
            return new MtomMessageEncodingBindingElement(this);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new MtomMessageEncoderFactory(MessageVersion, WriteEncoding, this.MaxReadPoolSize, this.MaxWritePoolSize, this.MaxBufferSize, this.ReaderQuotas);
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

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            if (policyContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
            }
            XmlDocument document = new XmlDocument();

            policyContext.GetBindingAssertions().Add(document.CreateElement(
                MessageEncodingPolicyConstants.OptimizedMimeSerializationPrefix,
                MessageEncodingPolicyConstants.MtomEncodingName,
                MessageEncodingPolicyConstants.OptimizedMimeSerializationNamespace));
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
            MtomMessageEncodingBindingElement mtom = b as MtomMessageEncodingBindingElement;
            if (mtom == null)
                return false;
            if (this.maxReadPoolSize != mtom.MaxReadPoolSize)
                return false;
            if (this.maxWritePoolSize != mtom.MaxWritePoolSize)
                return false;

            // compare XmlDictionaryReaderQuotas
            if (this.readerQuotas.MaxStringContentLength != mtom.ReaderQuotas.MaxStringContentLength)
                return false;
            if (this.readerQuotas.MaxArrayLength != mtom.ReaderQuotas.MaxArrayLength)
                return false;
            if (this.readerQuotas.MaxBytesPerRead != mtom.ReaderQuotas.MaxBytesPerRead)
                return false;
            if (this.readerQuotas.MaxDepth != mtom.ReaderQuotas.MaxDepth)
                return false;
            if (this.readerQuotas.MaxNameTableCharCount != mtom.ReaderQuotas.MaxNameTableCharCount)
                return false;

            if (this.maxBufferSize != mtom.MaxBufferSize)
                return false;

            if (this.WriteEncoding.EncodingName != mtom.WriteEncoding.EncodingName)
                return false;
            if (!this.MessageVersion.IsMatch(mtom.MessageVersion))
                return false;

            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessageVersion()
        {
            return (!this.messageVersion.IsMatch(MessageVersion.Default));
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
