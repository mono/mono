//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;
    using System.Diagnostics.CodeAnalysis;

    public sealed partial class ByteStreamMessageEncodingElement : BindingElementExtensionElement
    {
        public ByteStreamMessageEncodingElement()
        {
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule, Justification = "this property not a configuration property")]
        public override Type BindingElementType
        {
            get { return typeof(ByteStreamMessageEncodingBindingElement); }
        }

        [ConfigurationProperty(ByteStreamConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement)base[ByteStreamConfigurationStrings.ReaderQuotas]; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            ByteStreamMessageEncodingBindingElement binding = (ByteStreamMessageEncodingBindingElement)bindingElement;

            this.ApplyConfiguration(binding.ReaderQuotas);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ByteStreamMessageEncodingElement source = (ByteStreamMessageEncodingElement)from;

            this.CopyFrom(source.ReaderQuotas);
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            ByteStreamMessageEncodingBindingElement element = (ByteStreamMessageEncodingBindingElement)bindingElement;

            this.InitializeFrom(element.ReaderQuotas);
        }

        protected internal override BindingElement CreateBindingElement()
        {
            ByteStreamMessageEncodingBindingElement binding = new ByteStreamMessageEncodingBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        void ApplyConfiguration(XmlDictionaryReaderQuotas readerQuotas)
        {
            if (readerQuotas == null)
            {
                throw FxTrace.Exception.ArgumentNull("readerQuotas");
            }

            XmlDictionaryReaderQuotasElement oldQuotas = this.ReaderQuotas;

            if (oldQuotas.MaxDepth != 0)
            {
                readerQuotas.MaxDepth = oldQuotas.MaxDepth;
            }
            if (oldQuotas.MaxStringContentLength != 0)
            {
                readerQuotas.MaxStringContentLength = oldQuotas.MaxStringContentLength;
            }
            if (oldQuotas.MaxArrayLength != 0)
            {
                readerQuotas.MaxArrayLength = oldQuotas.MaxArrayLength;
            }
            if (oldQuotas.MaxBytesPerRead != 0)
            {
                readerQuotas.MaxBytesPerRead = oldQuotas.MaxBytesPerRead;
            }
            if (oldQuotas.MaxNameTableCharCount != 0)
            {
                readerQuotas.MaxNameTableCharCount = oldQuotas.MaxNameTableCharCount;
            }
        }

        void InitializeFrom(XmlDictionaryReaderQuotas readerQuotas)
        {
            if (readerQuotas == null)
            {
                throw FxTrace.Exception.ArgumentNull("readerQuotas");
            }
            
            XmlDictionaryReaderQuotasElement thisQuotas = this.ReaderQuotas;
            
            // Can't call thisQuotas.InitializeFrom() because it's internal to System.ServiceModel.dll, so we duplicate the logic
            if (readerQuotas.MaxDepth != EncoderDefaults.MaxDepth && readerQuotas.MaxDepth != 0)
            {
                thisQuotas.MaxDepth = readerQuotas.MaxDepth;
            }
            if (readerQuotas.MaxStringContentLength != EncoderDefaults.MaxStringContentLength && readerQuotas.MaxStringContentLength != 0)
            {
                thisQuotas.MaxStringContentLength = readerQuotas.MaxStringContentLength;
            }
            if (readerQuotas.MaxArrayLength != EncoderDefaults.MaxArrayLength && readerQuotas.MaxArrayLength != 0)
            {
                thisQuotas.MaxArrayLength = readerQuotas.MaxArrayLength;
            }
            if (readerQuotas.MaxBytesPerRead != EncoderDefaults.MaxBytesPerRead && readerQuotas.MaxBytesPerRead != 0)
            {
                thisQuotas.MaxBytesPerRead = readerQuotas.MaxBytesPerRead;
            }
            if (readerQuotas.MaxNameTableCharCount != EncoderDefaults.MaxNameTableCharCount && readerQuotas.MaxNameTableCharCount != 0)
            {
                thisQuotas.MaxNameTableCharCount = readerQuotas.MaxNameTableCharCount;
            } 
        }

        void CopyFrom(XmlDictionaryReaderQuotasElement readerQuotas)
        {
            XmlDictionaryReaderQuotasElement thisQuotas = this.ReaderQuotas;

            thisQuotas.MaxDepth = readerQuotas.MaxDepth;
            thisQuotas.MaxStringContentLength = readerQuotas.MaxStringContentLength;
            thisQuotas.MaxArrayLength = readerQuotas.MaxArrayLength;
            thisQuotas.MaxBytesPerRead = readerQuotas.MaxBytesPerRead;
            thisQuotas.MaxNameTableCharCount = readerQuotas.MaxNameTableCharCount;
        }
    }
}
