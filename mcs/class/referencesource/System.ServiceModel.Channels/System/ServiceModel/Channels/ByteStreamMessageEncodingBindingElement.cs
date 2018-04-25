//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime;
    using System.Xml;

    [Fx.Tag.XamlVisible(false)]
    public sealed class ByteStreamMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        XmlDictionaryReaderQuotas readerQuotas;

        public ByteStreamMessageEncodingBindingElement() : this((XmlDictionaryReaderQuotas)null)
        {
        }

        public ByteStreamMessageEncodingBindingElement(XmlDictionaryReaderQuotas quota)
        {
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            if (quota != null)
            {
                quota.CopyTo(this.readerQuotas);
            }
        }

        ByteStreamMessageEncodingBindingElement(ByteStreamMessageEncodingBindingElement byteStreamEncoderBindingElement) 
            : this(byteStreamEncoderBindingElement.readerQuotas)
        {
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return MessageVersion.None;
            }
            set
            {
                if (value != MessageVersion.None)
                {
                    throw FxTrace.Exception.Argument("MessageVersion", SR.ByteStreamMessageEncoderMessageVersionNotSupported(value));
                }
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
                    throw FxTrace.Exception.ArgumentNull("ReaderQuotas");
                value.CopyTo(this.ReaderQuotas);
            }
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return InternalCanBuildChannelFactory<TChannel>(context);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return InternalBuildChannelFactory<TChannel>(context);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return InternalCanBuildChannelListener<TChannel>(context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            return InternalBuildChannelListener<TChannel>(context);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new ByteStreamMessageEncoderFactory(this.readerQuotas);
        }

        public override BindingElement Clone()
        {
            return new ByteStreamMessageEncodingBindingElement(this);
        }
     
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessageVersion()
        {
            // Always MessageVersion.None in ByteStreamMessageEncoder
            return false; 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

    }
}
