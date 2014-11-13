//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Description;

    public sealed class WebMessageEncodingBindingElement : MessageEncodingBindingElement, IWsdlExportExtension, IWmiInstanceProvider
    {
        WebContentTypeMapper contentTypeMapper;

        int maxReadPoolSize;
        int maxWritePoolSize;
        XmlDictionaryReaderQuotas readerQuotas;
        Encoding writeEncoding;

        public WebMessageEncodingBindingElement()
            : this(TextEncoderDefaults.Encoding)
        {
        }

        public WebMessageEncodingBindingElement(Encoding writeEncoding)
        {
            if (writeEncoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");
            }

            TextEncoderDefaults.ValidateEncoding(writeEncoding);
            this.maxReadPoolSize = EncoderDefaults.MaxReadPoolSize;
            this.maxWritePoolSize = EncoderDefaults.MaxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            this.writeEncoding = writeEncoding;
        }

        WebMessageEncodingBindingElement(WebMessageEncodingBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.maxReadPoolSize = elementToBeCloned.maxReadPoolSize;
            this.maxWritePoolSize = elementToBeCloned.maxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            elementToBeCloned.readerQuotas.CopyTo(this.readerQuotas);
            this.writeEncoding = elementToBeCloned.writeEncoding;
            this.contentTypeMapper = elementToBeCloned.contentTypeMapper;
            this.CrossDomainScriptAccessEnabled = elementToBeCloned.CrossDomainScriptAccessEnabled;
        }
        public WebContentTypeMapper ContentTypeMapper
        {
            get
            {
                return contentTypeMapper;
            }
            set
            {
                contentTypeMapper = value;
            }
        }

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
                        SR2.GetString(SR2.ValueMustBePositive)));
                }
                this.maxReadPoolSize = value;
            }
        }

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
                        SR2.GetString(SR2.ValueMustBePositive)));
                }
                this.maxWritePoolSize = value;
            }
        }


        public override MessageVersion MessageVersion
        {
            get
            {
                return MessageVersion.None;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value != MessageVersion.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR2.GetString(SR2.JsonOnlySupportsMessageVersionNone));
                }
            }
        }

        internal override bool IsWsdlExportable
        {
            get { return false; }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

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

        public bool CrossDomainScriptAccessEnabled
        {
            get;
            set;
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
            return new WebMessageEncodingBindingElement(this);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new WebMessageEncoderFactory(this.WriteEncoding, this.MaxReadPoolSize, this.MaxWritePoolSize, this.ReaderQuotas, this.ContentTypeMapper, this.CrossDomainScriptAccessEnabled);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return (T)(object) this.readerQuotas;
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty(AdministrationStrings.MessageVersion, this.MessageVersion.ToString());
            wmiInstance.SetProperty(AdministrationStrings.Encoding, this.writeEncoding.WebName);
            wmiInstance.SetProperty(AdministrationStrings.MaxReadPoolSize, this.maxReadPoolSize);
            wmiInstance.SetProperty(AdministrationStrings.MaxWritePoolSize, this.maxWritePoolSize);
            if (this.ReaderQuotas != null)
            {
                IWmiInstance readerQuotasInstance = wmiInstance.NewInstance(AdministrationStrings.XmlDictionaryReaderQuotas);
                readerQuotasInstance.SetProperty(AdministrationStrings.MaxArrayLength, this.readerQuotas.MaxArrayLength);
                readerQuotasInstance.SetProperty(AdministrationStrings.MaxBytesPerRead, this.readerQuotas.MaxBytesPerRead);
                readerQuotasInstance.SetProperty(AdministrationStrings.MaxDepth, this.readerQuotas.MaxDepth);
                readerQuotasInstance.SetProperty(AdministrationStrings.MaxNameTableCharCount, this.readerQuotas.MaxNameTableCharCount);
                readerQuotasInstance.SetProperty(AdministrationStrings.MaxStringContentLength, this.readerQuotas.MaxStringContentLength);
                wmiInstance.SetProperty(AdministrationStrings.ReaderQuotas, readerQuotasInstance);
            }
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return typeof(WebMessageEncodingBindingElement).Name;
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            SoapHelper.SetSoapVersion(context, exporter, this.MessageVersion.Envelope);
        }

        internal override bool CheckEncodingVersion(EnvelopeVersion version)
        {
            return MessageVersion.Envelope == version;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
            {
                return false;
            }

            WebMessageEncodingBindingElement other = b as WebMessageEncodingBindingElement;
            if (other == null)
            {
                return false;
            }
            if (this.maxReadPoolSize != other.MaxReadPoolSize)
            {
                return false;
            }
            if (this.maxWritePoolSize != other.MaxWritePoolSize)
            {
                return false;
            }

            // compare XmlDictionaryReaderQuotas
            if (this.readerQuotas.MaxStringContentLength != other.ReaderQuotas.MaxStringContentLength)
            {
                return false;
            }
            if (this.readerQuotas.MaxArrayLength != other.ReaderQuotas.MaxArrayLength)
            {
                return false;
            }
            if (this.readerQuotas.MaxBytesPerRead != other.ReaderQuotas.MaxBytesPerRead)
            {
                return false;
            }
            if (this.readerQuotas.MaxDepth != other.ReaderQuotas.MaxDepth)
            {
                return false;
            }
            if (this.readerQuotas.MaxNameTableCharCount != other.ReaderQuotas.MaxNameTableCharCount)
            {
                return false;
            }

            if (this.WriteEncoding.EncodingName != other.WriteEncoding.EncodingName)
            {
                return false;
            }
            if (!this.MessageVersion.IsMatch(other.MessageVersion))
            {
                return false;
            }
            if (this.ContentTypeMapper != other.ContentTypeMapper)
            {
                return false;
            }

            return true;
        }
    }
}
