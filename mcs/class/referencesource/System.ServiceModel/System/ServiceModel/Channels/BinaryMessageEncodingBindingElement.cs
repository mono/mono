//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Reflection;
    using System.ServiceModel.Description;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;
    using System.Collections.Generic;
    using System.ComponentModel;

    public sealed class BinaryMessageEncodingBindingElement : MessageEncodingBindingElement, IWsdlExportExtension, IPolicyExportExtension
    {
        int maxReadPoolSize;
        int maxWritePoolSize;
        XmlDictionaryReaderQuotas readerQuotas;
        int maxSessionSize;
        BinaryVersion binaryVersion;
        MessageVersion messageVersion;
        CompressionFormat compressionFormat;
        long maxReceivedMessageSize;

        public BinaryMessageEncodingBindingElement()
        {
            this.maxReadPoolSize = EncoderDefaults.MaxReadPoolSize;
            this.maxWritePoolSize = EncoderDefaults.MaxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            this.maxSessionSize = BinaryEncoderDefaults.MaxSessionSize;
            this.binaryVersion = BinaryEncoderDefaults.BinaryVersion;
            this.messageVersion = MessageVersion.CreateVersion(BinaryEncoderDefaults.EnvelopeVersion);
            this.compressionFormat = EncoderDefaults.DefaultCompressionFormat;
        }

        BinaryMessageEncodingBindingElement(BinaryMessageEncodingBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.maxReadPoolSize = elementToBeCloned.maxReadPoolSize;
            this.maxWritePoolSize = elementToBeCloned.maxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            elementToBeCloned.readerQuotas.CopyTo(this.readerQuotas);
            this.MaxSessionSize = elementToBeCloned.MaxSessionSize;
            this.BinaryVersion = elementToBeCloned.BinaryVersion;
            this.messageVersion = elementToBeCloned.messageVersion;
            this.CompressionFormat = elementToBeCloned.CompressionFormat;
            this.maxReceivedMessageSize = elementToBeCloned.maxReceivedMessageSize;
        }

        [DefaultValue(EncoderDefaults.DefaultCompressionFormat)]
        public CompressionFormat CompressionFormat
        {
            get
            {
                return this.compressionFormat;
            }
            set
            {
                this.compressionFormat = value;
            }
        }

        /* public */
        BinaryVersion BinaryVersion
        {
            get
            {
                return binaryVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.binaryVersion = value;
            }
        }

        public override MessageVersion MessageVersion
        {
            get { return this.messageVersion; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value.Envelope != BinaryEncoderDefaults.EnvelopeVersion)
                {
                    string errorMsg = SR.GetString(SR.UnsupportedEnvelopeVersion, this.GetType().FullName, BinaryEncoderDefaults.EnvelopeVersion, value.Envelope);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(errorMsg));
                }

                this.messageVersion = MessageVersion.CreateVersion(BinaryEncoderDefaults.EnvelopeVersion, value.Addressing);
            }
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

        [DefaultValue(BinaryEncoderDefaults.MaxSessionSize)]
        public int MaxSessionSize
        {
            get
            {
                return this.maxSessionSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
                }

                this.maxSessionSize = value;
            }
        }

        private void VerifyCompression(BindingContext context)
        {
            if (this.compressionFormat != CompressionFormat.None)
            {
                ITransportCompressionSupport compressionSupport = context.GetInnerProperty<ITransportCompressionSupport>();
                if (compressionSupport == null || !compressionSupport.IsCompressionFormatSupported(this.compressionFormat))
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.GetString(
                        SR.TransportDoesNotSupportCompression,
                        this.compressionFormat.ToString(),
                        this.GetType().Name,
                        CompressionFormat.None.ToString())));
                }
            }
        }

        void SetMaxReceivedMessageSizeFromTransport(BindingContext context)
        {
            TransportBindingElement transport = context.Binding.Elements.Find<TransportBindingElement>();
            if (transport != null)
            {
                // We are guaranteed that a transport exists when building a binding;  
                // Allow the regular flow/checks to happen rather than throw here 
                // (InternalBuildChannelListener will call into the BindingContext. Validation happens there and it will throw) 
                this.maxReceivedMessageSize = transport.MaxReceivedMessageSize;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            VerifyCompression(context);
            SetMaxReceivedMessageSizeFromTransport(context);
            return InternalBuildChannelFactory<TChannel>(context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            VerifyCompression(context);
            SetMaxReceivedMessageSizeFromTransport(context);
            return InternalBuildChannelListener<TChannel>(context);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return InternalCanBuildChannelListener<TChannel>(context);
        }

        public override BindingElement Clone()
        {
            return new BinaryMessageEncodingBindingElement(this);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new BinaryMessageEncoderFactory(
                this.MessageVersion, 
                this.MaxReadPoolSize, 
                this.MaxWritePoolSize, 
                this.MaxSessionSize, 
                this.ReaderQuotas, 
                this.maxReceivedMessageSize,
                this.BinaryVersion, 
                this.CompressionFormat);
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
                MessageEncodingPolicyConstants.BinaryEncodingPrefix,
                MessageEncodingPolicyConstants.BinaryEncodingName,
                MessageEncodingPolicyConstants.BinaryEncodingNamespace));
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context) { }
        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            SoapHelper.SetSoapVersion(context, exporter, MessageVersion.Soap12WSAddressing10.Envelope);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
                return false;

            BinaryMessageEncodingBindingElement binary = b as BinaryMessageEncodingBindingElement;
            if (binary == null)
                return false;
            if (this.maxReadPoolSize != binary.MaxReadPoolSize)
                return false;
            if (this.maxWritePoolSize != binary.MaxWritePoolSize)
                return false;

            // compare XmlDictionaryReaderQuotas
            if (this.readerQuotas.MaxStringContentLength != binary.ReaderQuotas.MaxStringContentLength)
                return false;
            if (this.readerQuotas.MaxArrayLength != binary.ReaderQuotas.MaxArrayLength)
                return false;
            if (this.readerQuotas.MaxBytesPerRead != binary.ReaderQuotas.MaxBytesPerRead)
                return false;
            if (this.readerQuotas.MaxDepth != binary.ReaderQuotas.MaxDepth)
                return false;
            if (this.readerQuotas.MaxNameTableCharCount != binary.ReaderQuotas.MaxNameTableCharCount)
                return false;

            if (this.MaxSessionSize != binary.MaxSessionSize)
                return false;
            if (this.CompressionFormat != binary.CompressionFormat)
                return false;
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessageVersion()
        {
            return (!this.messageVersion.IsMatch(MessageVersion.Default));
        }
    }
}
