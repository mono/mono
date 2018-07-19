//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml;
    using SMTD = System.ServiceModel.Diagnostics.Application.TD;
    using WebTD = System.ServiceModel.Web.Diagnostics.Application.TD;

    class JsonMessageEncoderFactory : MessageEncoderFactory
    {
        static readonly TextMessageEncoderFactory.ContentEncoding[] ApplicationJsonContentEncoding = GetContentEncodingMap(JsonGlobals.applicationJsonMediaType);
        JsonMessageEncoder messageEncoder;

        public JsonMessageEncoderFactory(Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas, bool crossDomainScriptAccessEnabled)
        {
            messageEncoder = new JsonMessageEncoder(writeEncoding, maxReadPoolSize, maxWritePoolSize, quotas, crossDomainScriptAccessEnabled);
        }

        public override MessageEncoder Encoder
        {
            get { return messageEncoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return messageEncoder.MessageVersion; }
        }

        internal static string GetContentType(WebMessageEncodingBindingElement encodingElement)
        {
            if (encodingElement == null)
            {
                return WebMessageEncoderFactory.GetContentType(JsonGlobals.applicationJsonMediaType, TextEncoderDefaults.Encoding);
            }
            else
            {
                return WebMessageEncoderFactory.GetContentType(JsonGlobals.applicationJsonMediaType, encodingElement.WriteEncoding);
            }
        }

        static TextMessageEncoderFactory.ContentEncoding[] GetContentEncodingMap(string mediaType)
        {
            Encoding[] readEncodings = TextMessageEncoderFactory.GetSupportedEncodings();
            TextMessageEncoderFactory.ContentEncoding[] map = new TextMessageEncoderFactory.ContentEncoding[readEncodings.Length];
            for (int i = 0; i < readEncodings.Length; i++)
            {
                TextMessageEncoderFactory.ContentEncoding contentEncoding = new TextMessageEncoderFactory.ContentEncoding();
                contentEncoding.contentType = WebMessageEncoderFactory.GetContentType(mediaType, readEncodings[i]);
                contentEncoding.encoding = readEncodings[i];
                map[i] = contentEncoding;
            }
            return map;
        }

        class JsonMessageEncoder : MessageEncoder
        {
            const int maxPooledXmlReadersPerMessage = 2;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile SynchronizedPool<JsonBufferedMessageData> bufferedReaderPool;
            volatile SynchronizedPool<JsonBufferedMessageWriter> bufferedWriterPool;
            string contentType;
            int maxReadPoolSize;
            int maxWritePoolSize;
            OnXmlDictionaryReaderClose onStreamedReaderClose;
            XmlDictionaryReaderQuotas readerQuotas;
            XmlDictionaryReaderQuotas bufferedReadReaderQuotas;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile SynchronizedPool<RecycledMessageState> recycledStatePool;
            volatile SynchronizedPool<XmlDictionaryReader> streamedReaderPool;
            volatile SynchronizedPool<XmlDictionaryWriter> streamedWriterPool;
            object thisLock;
            Encoding writeEncoding;
            bool crossDomainScriptAccessEnabled;
            byte[] encodedClosingFunctionCall;

            public JsonMessageEncoder(Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas, bool crossDomainScriptAccessEnabled)
            {
                if (writeEncoding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");
                }

                thisLock = new object();

                TextEncoderDefaults.ValidateEncoding(writeEncoding);
                this.writeEncoding = writeEncoding;

                this.maxReadPoolSize = maxReadPoolSize;
                this.maxWritePoolSize = maxWritePoolSize;

                this.readerQuotas = new XmlDictionaryReaderQuotas();
                this.onStreamedReaderClose = new OnXmlDictionaryReaderClose(ReturnStreamedReader);
                quotas.CopyTo(this.readerQuotas);

                this.bufferedReadReaderQuotas = EncoderHelpers.GetBufferedReadQuotas(this.readerQuotas);

                this.contentType = WebMessageEncoderFactory.GetContentType(JsonGlobals.applicationJsonMediaType, writeEncoding);
                this.crossDomainScriptAccessEnabled = crossDomainScriptAccessEnabled;
                this.encodedClosingFunctionCall = this.writeEncoding.GetBytes(");");
            }

            public override string ContentType
            {
                get { return contentType; }
            }

            public override string MediaType
            {
                get { return JsonGlobals.applicationJsonMediaType; }
            }

            public override MessageVersion MessageVersion
            {
                get { return MessageVersion.None; }
            }

            SynchronizedPool<RecycledMessageState> RecycledStatePool
            {
                get
                {
                    if (recycledStatePool == null)
                    {
                        lock (ThisLock)
                        {
                            if (recycledStatePool == null)
                            {
                                recycledStatePool = new SynchronizedPool<RecycledMessageState>(maxReadPoolSize);
                            }
                        }
                    }
                    return recycledStatePool;
                }
            }

            object ThisLock
            {
                get { return thisLock; }
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                if (contentType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
                }
                return IsJsonContentType(contentType);
            }

            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                if (bufferManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bufferManager"));
                }

                if (WebTD.JsonMessageDecodingStartIsEnabled())
                {
                    WebTD.JsonMessageDecodingStart();
                }

                Message message;

                JsonBufferedMessageData messageData = TakeBufferedReader();
                messageData.Encoding = TextMessageEncoderFactory.GetEncodingFromContentType(contentType, JsonMessageEncoderFactory.ApplicationJsonContentEncoding);
                messageData.Open(buffer, bufferManager);
                RecycledMessageState messageState = messageData.TakeMessageState();
                if (messageState == null)
                {
                    messageState = new RecycledMessageState();
                }
                message = new BufferedMessage(messageData, messageState);

                message.Properties.Encoder = this;

                if (SMTD.MessageReadByEncoderIsEnabled() && buffer != null)
                {
                    SMTD.MessageReadByEncoder(
                        EventTraceActivityHelper.TryExtractActivity(message, true),
                        buffer.Count,
                        this);
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
                }

                return message;
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                if (stream == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
                }

                if (WebTD.JsonMessageDecodingStartIsEnabled())
                {
                    WebTD.JsonMessageDecodingStart();
                }

                XmlReader reader = TakeStreamedReader(stream, TextMessageEncoderFactory.GetEncodingFromContentType(contentType, JsonMessageEncoderFactory.ApplicationJsonContentEncoding));
                Message message = Message.CreateMessage(reader, maxSizeOfHeaders, MessageVersion.None);
                message.Properties.Encoder = this;

                if (SMTD.StreamedMessageReadByEncoderIsEnabled())
                {
                    SMTD.StreamedMessageReadByEncoder(EventTraceActivityHelper.TryExtractActivity(message, true));
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
                }
                return message;
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

                EventTraceActivity eventTraceActivity = null;
                if (WebTD.JsonMessageEncodingStartIsEnabled())
                {
                    eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                    WebTD.JsonMessageEncodingStart(eventTraceActivity);
                }

                ThrowIfMismatchedMessageVersion(message);
                message.Properties.Encoder = this;
                JsonBufferedMessageWriter messageWriter = TakeBufferedWriter();

                JavascriptCallbackResponseMessageProperty javascriptResponseMessageProperty;
                if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty) &&
                    javascriptResponseMessageProperty != null)
                {
                    if (this.crossDomainScriptAccessEnabled)
                    {
                        messageWriter.SetJavascriptCallbackProperty(javascriptResponseMessageProperty);
                    }
                    else
                    {
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotEnabled), message);
                    }
                }

                ArraySegment<byte> messageData = messageWriter.WriteMessage(message, bufferManager, messageOffset, maxMessageSize);
                ReturnMessageWriter(messageWriter);

                if (SMTD.MessageWrittenByEncoderIsEnabled() && messageData != null)
                {
                    SMTD.MessageWrittenByEncoder(
                        eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(message),
                        messageData.Count,
                        this);
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    XmlDictionaryReader xmlDictionaryReader = JsonReaderWriterFactory.CreateJsonReader(
                        messageData.Array, messageData.Offset, messageData.Count, null, XmlDictionaryReaderQuotas.Max, null);
                    MessageLogger.LogMessage(ref message, xmlDictionaryReader, MessageLoggingSource.TransportSend);
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

                EventTraceActivity eventTraceActivity = null;
                if (WebTD.JsonMessageEncodingStartIsEnabled())
                {
                    eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                    WebTD.JsonMessageEncodingStart(eventTraceActivity);
                }

                message.Properties.Encoder = this;
                XmlDictionaryWriter xmlWriter = TakeStreamedWriter(stream);
                JavascriptCallbackResponseMessageProperty javascriptResponseMessageProperty;
                if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty)
                    && javascriptResponseMessageProperty != null
                    && !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                {
                    if (!this.crossDomainScriptAccessEnabled)
                    {
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotEnabled), message);
                    }
                    byte[] buffer = this.writeEncoding.GetBytes(String.Format(CultureInfo.InvariantCulture, "{0}(", javascriptResponseMessageProperty.CallbackFunctionName));
                    stream.Write(buffer, 0, buffer.Length);
                }
                xmlWriter.WriteStartDocument();
                message.WriteMessage(xmlWriter);
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                ReturnStreamedWriter(xmlWriter);
                if (javascriptResponseMessageProperty != null
                    && !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                {
                    if (javascriptResponseMessageProperty.StatusCode != null && (int)javascriptResponseMessageProperty.StatusCode != 200)
                    {
                        byte[] buffer = this.writeEncoding.GetBytes(String.Format(CultureInfo.InvariantCulture, ",{0}", (int)javascriptResponseMessageProperty.StatusCode));
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    stream.Write(this.encodedClosingFunctionCall, 0, this.encodedClosingFunctionCall.Length);
                }

                if (SMTD.StreamedMessageWrittenByEncoderIsEnabled())
                {
                    SMTD.StreamedMessageWrittenByEncoder(
                        eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(message));
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
                }
            }

            internal override bool IsCharSetSupported(string charSet)
            {
                Encoding tmp;
                return TextEncoderDefaults.TryGetEncoding(charSet, out tmp);
            }

            bool IsJsonContentType(string contentType)
            {
                return IsContentTypeSupported(contentType, JsonGlobals.applicationJsonMediaType, JsonGlobals.applicationJsonMediaType) || IsContentTypeSupported(contentType, JsonGlobals.textJsonMediaType, JsonGlobals.textJsonMediaType);
            }

            void ReturnBufferedData(JsonBufferedMessageData messageData)
            {
                bufferedReaderPool.Return(messageData);
            }

            void ReturnMessageWriter(JsonBufferedMessageWriter messageWriter)
            {
                bufferedWriterPool.Return(messageWriter);
            }

            void ReturnStreamedReader(XmlDictionaryReader xmlReader)
            {
                streamedReaderPool.Return(xmlReader);
            }

            void ReturnStreamedWriter(XmlWriter xmlWriter)
            {
                xmlWriter.Close();
                streamedWriterPool.Return((XmlDictionaryWriter)xmlWriter);
            }

            JsonBufferedMessageData TakeBufferedReader()
            {
                if (bufferedReaderPool == null)
                {
                    lock (ThisLock)
                    {
                        if (bufferedReaderPool == null)
                        {
                            bufferedReaderPool = new SynchronizedPool<JsonBufferedMessageData>(maxReadPoolSize);
                        }
                    }
                }
                JsonBufferedMessageData messageData = bufferedReaderPool.Take();
                if (messageData == null)
                {
                    messageData = new JsonBufferedMessageData(this, maxPooledXmlReadersPerMessage);
                }
                return messageData;
            }

            JsonBufferedMessageWriter TakeBufferedWriter()
            {
                if (bufferedWriterPool == null)
                {
                    lock (ThisLock)
                    {
                        if (bufferedWriterPool == null)
                        {
                            bufferedWriterPool = new SynchronizedPool<JsonBufferedMessageWriter>(maxWritePoolSize);
                        }
                    }
                }
                JsonBufferedMessageWriter messageWriter = bufferedWriterPool.Take();
                if (messageWriter == null)
                {
                    messageWriter = new JsonBufferedMessageWriter(this);
                }
                return messageWriter;
            }

            XmlDictionaryReader TakeStreamedReader(Stream stream, Encoding enc)
            {
                if (streamedReaderPool == null)
                {
                    lock (ThisLock)
                    {
                        if (streamedReaderPool == null)
                        {
                            streamedReaderPool = new SynchronizedPool<XmlDictionaryReader>(maxReadPoolSize);
                        }
                    }
                }
                XmlDictionaryReader xmlReader = streamedReaderPool.Take();
                if (xmlReader == null)
                {
                    xmlReader = JsonReaderWriterFactory.CreateJsonReader(stream, enc, this.readerQuotas, this.onStreamedReaderClose);
                }
                else
                {
                    ((IXmlJsonReaderInitializer)xmlReader).SetInput(stream, enc, this.readerQuotas, this.onStreamedReaderClose);
                }
                return xmlReader;
            }

            XmlDictionaryWriter TakeStreamedWriter(Stream stream)
            {
                if (streamedWriterPool == null)
                {
                    lock (ThisLock)
                    {
                        if (streamedWriterPool == null)
                        {
                            streamedWriterPool = new SynchronizedPool<XmlDictionaryWriter>(maxWritePoolSize);
                        }
                    }
                }
                XmlDictionaryWriter xmlWriter = streamedWriterPool.Take();
                if (xmlWriter == null)
                {
                    xmlWriter = JsonReaderWriterFactory.CreateJsonWriter(stream, this.writeEncoding, false);
                }
                else
                {
                    ((IXmlJsonWriterInitializer)xmlWriter).SetOutput(stream, this.writeEncoding, false);
                }
                return xmlWriter;
            }

            class JsonBufferedMessageData : BufferedMessageData
            {
                Encoding encoding;
                JsonMessageEncoder messageEncoder;
                OnXmlDictionaryReaderClose onClose;
                Pool<XmlDictionaryReader> readerPool;

                public JsonBufferedMessageData(JsonMessageEncoder messageEncoder, int maxReaderPoolSize)
                    : base(messageEncoder.RecycledStatePool)
                {
                    this.messageEncoder = messageEncoder;
                    readerPool = new Pool<XmlDictionaryReader>(maxReaderPoolSize);
                    onClose = new OnXmlDictionaryReaderClose(OnXmlReaderClosed);
                }

                public override MessageEncoder MessageEncoder
                {
                    get { return messageEncoder; }
                }

                public override XmlDictionaryReaderQuotas Quotas
                {
                    get { return messageEncoder.bufferedReadReaderQuotas; }
                }

                internal Encoding Encoding
                {
                    set
                    {
                        this.encoding = value;
                    }
                }

                protected override void OnClosed()
                {
                    messageEncoder.ReturnBufferedData(this);
                }

                protected override void ReturnXmlReader(XmlDictionaryReader xmlReader)
                {
                    if (xmlReader != null)
                    {
                        readerPool.Return(xmlReader);
                    }
                }

                protected override XmlDictionaryReader TakeXmlReader()
                {
                    ArraySegment<byte> buffer = this.Buffer;

                    XmlDictionaryReader xmlReader = readerPool.Take();
                    if (xmlReader == null)
                    {
                        xmlReader = JsonReaderWriterFactory.CreateJsonReader(buffer.Array, buffer.Offset, buffer.Count, this.encoding, this.Quotas, onClose);
                    }
                    else
                    {
                        ((IXmlJsonReaderInitializer)xmlReader).SetInput(buffer.Array, buffer.Offset, buffer.Count, this.encoding, this.Quotas, onClose);
                    }

                    return xmlReader;
                }
            }

            class JsonBufferedMessageWriter : BufferedMessageWriter
            {
                JsonMessageEncoder messageEncoder;
                XmlDictionaryWriter returnedWriter;
                JavascriptXmlWriterWrapper javascriptWrapper;

                public JsonBufferedMessageWriter(JsonMessageEncoder messageEncoder)
                {
                    this.messageEncoder = messageEncoder;
                }

                public void SetJavascriptCallbackProperty(JavascriptCallbackResponseMessageProperty javascriptResponseMessageProperty)
                {
                    if (this.javascriptWrapper == null)
                    {
                        this.javascriptWrapper = new JavascriptXmlWriterWrapper(this.messageEncoder.writeEncoding)
                        {
                            JavascriptResponseMessageProperty = javascriptResponseMessageProperty
                        };
                    }
                    else
                    {
                        this.javascriptWrapper.JavascriptResponseMessageProperty = javascriptResponseMessageProperty;
                    }
                }

                protected override void OnWriteEndMessage(XmlDictionaryWriter writer)
                {
                    writer.WriteEndDocument();
                }

                protected override void OnWriteStartMessage(XmlDictionaryWriter writer)
                {
                    writer.WriteStartDocument();
                }

                protected override void ReturnXmlWriter(XmlDictionaryWriter writer)
                {
                    writer.Close();

                    if (writer is JavascriptXmlWriterWrapper)
                    {
                        if (this.javascriptWrapper == null)
                        {
                            this.javascriptWrapper = (JavascriptXmlWriterWrapper)writer;
                            this.javascriptWrapper.JavascriptResponseMessageProperty = null;
                            writer = this.javascriptWrapper.XmlJsonWriter;
                        }
                    }

                    if (this.returnedWriter == null)
                    {
                        this.returnedWriter = writer;
                    }
                }

                protected override XmlDictionaryWriter TakeXmlWriter(Stream stream)
                {
                    XmlDictionaryWriter writer = null;
                    if (this.returnedWriter == null)
                    {
                        writer = JsonReaderWriterFactory.CreateJsonWriter(stream, messageEncoder.writeEncoding, false);
                    }
                    else
                    {
                        writer = this.returnedWriter;
                        ((IXmlJsonWriterInitializer)writer).SetOutput(stream, messageEncoder.writeEncoding, false);
                        this.returnedWriter = null;
                    }

                    if (this.javascriptWrapper != null && this.javascriptWrapper.JavascriptResponseMessageProperty != null)
                    {
                        this.javascriptWrapper.SetOutput(stream, writer);
                        writer = this.javascriptWrapper;
                        this.javascriptWrapper = null;
                    }

                    return writer;
                }
            }
        }
    }
}
