//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;
    using WebTD = System.ServiceModel.Web.Diagnostics.Application.TD;


    class WebScriptMetadataMessageEncoderFactory : MessageEncoderFactory
    {
        const string applicationJavaScriptMediaType = "application/x-javascript";
        WebScriptMetadataMessageEncoder messageEncoder;

        public WebScriptMetadataMessageEncoderFactory(XmlDictionaryReaderQuotas quotas)
        {
            messageEncoder = new WebScriptMetadataMessageEncoder(quotas);
        }

        public override MessageEncoder Encoder
        {
            get { return messageEncoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return messageEncoder.MessageVersion; }
        }

        class WebScriptMetadataMessageEncoder : MessageEncoder
        {
            static UTF8Encoding UTF8EncodingWithoutByteOrderMark = new UTF8Encoding(false);
            string contentType;
            MessageEncoder innerReadMessageEncoder;
            string mediaType;
            XmlDictionaryReaderQuotas readerQuotas;

            public WebScriptMetadataMessageEncoder(XmlDictionaryReaderQuotas quotas)
            {
                this.readerQuotas = new XmlDictionaryReaderQuotas();
                quotas.CopyTo(this.readerQuotas);
                this.mediaType = this.contentType = applicationJavaScriptMediaType;
                this.innerReadMessageEncoder = new TextMessageEncodingBindingElement(MessageVersion.None, Encoding.UTF8).CreateMessageEncoderFactory().Encoder;
            }

            public override string ContentType
            {
                get { return contentType; }
            }

            public override string MediaType
            {
                get { return mediaType; }
            }

            public override MessageVersion MessageVersion
            {
                get { return MessageVersion.None; }
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                return innerReadMessageEncoder.IsContentTypeSupported(contentType);
            }

            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                return innerReadMessageEncoder.ReadMessage(buffer, bufferManager, contentType);
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                return innerReadMessageEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (bufferManager == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("bufferManager"), message);
                }
                if (maxMessageSize < 0)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxMessageSize", maxMessageSize,
                        SR2.GetString(SR2.ValueMustBeNonNegative)), message);
                }
                if (messageOffset < 0 || messageOffset > maxMessageSize)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageOffset", messageOffset,
                        SR2.GetString(SR2.JsonValueMustBeInRange, 0, maxMessageSize)), message);
                }

                ThrowIfMismatchedMessageVersion(message);
                message.Properties.Encoder = this;
                BufferedMessageWriter messageWriter = new WebScriptMetadataBufferedMessageWriter(this);
                ArraySegment<byte> messageData = messageWriter.WriteMessage(message, bufferManager, messageOffset, maxMessageSize);
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
                }
                if (System.ServiceModel.Diagnostics.Application.TD.MessageWrittenByEncoderIsEnabled() && messageData != null)
                {
                    System.ServiceModel.Diagnostics.Application.TD.MessageWrittenByEncoder(EventTraceActivityHelper.TryExtractActivity(message), messageData.Count, this);
                }
                return messageData;
            }

            public override void WriteMessage(Message message, Stream stream)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (stream == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("stream"), message);
                }
                ThrowIfMismatchedMessageVersion(message);
                message.Properties.Encoder = this;
                XmlDictionaryWriter xmlWriter = CreateWriter(stream);
                xmlWriter.WriteStartDocument();
                message.WriteMessage(xmlWriter);
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                xmlWriter.Close();
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
                }
            }

            XmlDictionaryWriter CreateWriter(Stream stream)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Encoding = UTF8EncodingWithoutByteOrderMark;
                XmlWriter writer = XmlWriter.Create(stream, settings);
                return XmlDictionaryWriter.CreateDictionaryWriter(writer);
            }

            class WebScriptMetadataBufferedMessageWriter : BufferedMessageWriter
            {
                WebScriptMetadataMessageEncoder messageEncoder;

                public WebScriptMetadataBufferedMessageWriter(WebScriptMetadataMessageEncoder messageEncoder)
                {
                    this.messageEncoder = messageEncoder;
                }

                protected override void OnWriteEndMessage(XmlDictionaryWriter writer)
                {
                }

                protected override void OnWriteStartMessage(XmlDictionaryWriter writer)
                {
                }

                protected override void ReturnXmlWriter(XmlDictionaryWriter writer)
                {
                    writer.Close();
                }

                protected override XmlDictionaryWriter TakeXmlWriter(Stream stream)
                {
                    return messageEncoder.CreateWriter(stream);
                }
            }
        }
    }
}
