//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel;
    using System.Text;
    using System.Xml;

    sealed class WebScriptMetadataMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        XmlDictionaryReaderQuotas readerQuotas;

        public WebScriptMetadataMessageEncodingBindingElement()
        {
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
        }

        WebScriptMetadataMessageEncodingBindingElement(WebScriptMetadataMessageEncodingBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            elementToBeCloned.readerQuotas.CopyTo(this.readerQuotas);
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

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
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

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return InternalCanBuildChannelFactory<TChannel>(context);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return InternalCanBuildChannelListener<TChannel>(context);
        }

        public override BindingElement Clone()
        {
            return new WebScriptMetadataMessageEncodingBindingElement(this);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new WebScriptMetadataMessageEncoderFactory(this.ReaderQuotas);
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
    }
}
