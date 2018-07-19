//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.IO;
    using System.Net.Mime;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Text;
    using System.Xml;

    class TextMessageEncoderFactory : MessageEncoderFactory
    {
        TextMessageEncoder messageEncoder;
        internal static ContentEncoding[] Soap11Content = GetContentEncodingMap(MessageVersion.Soap11WSAddressing10);
        internal static ContentEncoding[] Soap12Content = GetContentEncodingMap(MessageVersion.Soap12WSAddressing10);
        internal static ContentEncoding[] SoapNoneContent = GetContentEncodingMap(MessageVersion.None);
        internal const string Soap11MediaType = "text/xml";
        internal const string Soap12MediaType = "application/soap+xml";
        const string XmlMediaType = "application/xml";

        public TextMessageEncoderFactory(MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas)
        {
            messageEncoder = new TextMessageEncoder(version, writeEncoding, maxReadPoolSize, maxWritePoolSize, quotas);
        }

        public override MessageEncoder Encoder
        {
            get { return messageEncoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return messageEncoder.MessageVersion; }
        }

        public int MaxWritePoolSize
        {
            get { return messageEncoder.MaxWritePoolSize; }
        }

        public int MaxReadPoolSize
        {
            get { return messageEncoder.MaxReadPoolSize; }
        }

        public static Encoding[] GetSupportedEncodings()
        {
            Encoding[] supported = TextEncoderDefaults.SupportedEncodings;
            Encoding[] enc = new Encoding[supported.Length];
            Array.Copy(supported, enc, supported.Length);
            return enc;
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return messageEncoder.ReaderQuotas;
            }
        }

        internal static string GetMediaType(MessageVersion version)
        {
            string mediaType = null;
            if (version.Envelope == EnvelopeVersion.Soap12)
            {
                mediaType = TextMessageEncoderFactory.Soap12MediaType;
            }
            else if (version.Envelope == EnvelopeVersion.Soap11)
            {
                mediaType = TextMessageEncoderFactory.Soap11MediaType;
            }
            else if (version.Envelope == EnvelopeVersion.None)
            {
                mediaType = TextMessageEncoderFactory.XmlMediaType;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.EnvelopeVersionNotSupported, version.Envelope)));
            }
            return mediaType;
        }

        internal static string GetContentType(string mediaType, Encoding encoding)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}; charset={1}", mediaType, TextEncoderDefaults.EncodingToCharSet(encoding));
        }

        static ContentEncoding[] GetContentEncodingMap(MessageVersion version)
        {
            Encoding[] readEncodings = TextMessageEncoderFactory.GetSupportedEncodings();
            string media = GetMediaType(version);
            ContentEncoding[] map = new ContentEncoding[readEncodings.Length];
            for (int i = 0; i < readEncodings.Length; i++)
            {
                ContentEncoding contentEncoding = new ContentEncoding();
                contentEncoding.contentType = GetContentType(media, readEncodings[i]);
                contentEncoding.encoding = readEncodings[i];
                map[i] = contentEncoding;
            }
            return map;
        }

        internal static Encoding GetEncodingFromContentType(string contentType, ContentEncoding[] contentMap)
        {
            if (contentType == null)
            {
                return null;
            }

            // Check for known/expected content types
            for (int i = 0; i < contentMap.Length; i++)
            {
                if (contentMap[i].contentType == contentType)
                {
                    return contentMap[i].encoding;
                }
            }

            // then some heuristic matches (since System.Mime.ContentType is a performance hit)
            // start by looking for a parameter. 

            // If none exists, we don't have an encoding
            int semiColonIndex = contentType.IndexOf(';');
            if (semiColonIndex == -1)
            {
                return null;
            }

            // optimize for charset being the first parameter
            int charsetValueIndex = -1;

            // for Indigo scenarios, we'll have "; charset=", so check for the c
            if ((contentType.Length > semiColonIndex + 11) // need room for parameter + charset + '=' 
                && contentType[semiColonIndex + 2] == 'c'
                && string.Compare("charset=", 0, contentType, semiColonIndex + 2, 8, StringComparison.OrdinalIgnoreCase) == 0)
            {
                charsetValueIndex = semiColonIndex + 10;
            }
            else
            {
                // look for charset= somewhere else in the message
                int paramIndex = contentType.IndexOf("charset=", semiColonIndex + 1, StringComparison.OrdinalIgnoreCase);
                if (paramIndex != -1)
                {
                    // validate there's only whitespace or semi-colons beforehand
                    for (int i = paramIndex - 1; i >= semiColonIndex; i--)
                    {
                        if (contentType[i] == ';')
                        {
                            charsetValueIndex = paramIndex + 8;
                            break;
                        }

                        if (contentType[i] == '\n')
                        {
                            if (i == semiColonIndex || contentType[i - 1] != '\r')
                            {
                                break;
                            }

                            i--;
                            continue;
                        }

                        if (contentType[i] != ' '
                            && contentType[i] != '\t')
                        {
                            break;
                        }
                    }
                }
            }

            string charSet;
            Encoding enc;

            // we have a possible charset value. If it's easy to parse, do so
            if (charsetValueIndex != -1)
            {
                // get the next semicolon
                semiColonIndex = contentType.IndexOf(';', charsetValueIndex);
                if (semiColonIndex == -1)
                {
                    charSet = contentType.Substring(charsetValueIndex);
                }
                else
                {
                    charSet = contentType.Substring(charsetValueIndex, semiColonIndex - charsetValueIndex);
                }

                // and some minimal quote stripping
                if (charSet.Length > 2 && charSet[0] == '"' && charSet[charSet.Length - 1] == '"')
                {
                    charSet = charSet.Substring(1, charSet.Length - 2);
                }

                Fx.Assert(charSet == (new ContentType(contentType)).CharSet,
                        "CharSet parsing failed to correctly parse the ContentType header.");

                if (TryGetEncodingFromCharSet(charSet, out enc))
                {
                    return enc;
                }
            }

            // our quick heuristics failed. fall back to System.Net
            try
            {
                ContentType parsedContentType = new ContentType(contentType);
                charSet = parsedContentType.CharSet;
            }
            catch (FormatException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.EncoderBadContentType), e));
            }

            if (TryGetEncodingFromCharSet(charSet, out enc))
                return enc;

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.EncoderUnrecognizedCharSet, charSet)));
        }

        internal static bool TryGetEncodingFromCharSet(string charSet, out Encoding encoding)
        {
            encoding = null;
            if (charSet == null || charSet.Length == 0)
                return true;

            return TextEncoderDefaults.TryGetEncoding(charSet, out encoding);
        }

        internal class ContentEncoding
        {
            internal string contentType;
            internal Encoding encoding;
        }

        class TextMessageEncoder : MessageEncoder, ITraceSourceStringProvider
        {
            int maxReadPoolSize;
            int maxWritePoolSize;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile SynchronizedPool<XmlDictionaryWriter> streamedWriterPool;
            volatile SynchronizedPool<XmlDictionaryReader> streamedReaderPool;
            volatile SynchronizedPool<UTF8BufferedMessageData> bufferedReaderPool;
            volatile SynchronizedPool<TextBufferedMessageWriter> bufferedWriterPool;
            volatile SynchronizedPool<RecycledMessageState> recycledStatePool;

            object thisLock;
            string contentType;
            string mediaType;
            Encoding writeEncoding;
            MessageVersion version;
            bool optimizeWriteForUTF8;
            const int maxPooledXmlReadersPerMessage = 2;
            XmlDictionaryReaderQuotas readerQuotas;
            XmlDictionaryReaderQuotas bufferedReadReaderQuotas;
            OnXmlDictionaryReaderClose onStreamedReaderClose;
            ContentEncoding[] contentEncodingMap;

            public TextMessageEncoder(MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas)
            {
                if (version == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
                if (writeEncoding == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");

                TextEncoderDefaults.ValidateEncoding(writeEncoding);
                this.writeEncoding = writeEncoding;
                optimizeWriteForUTF8 = IsUTF8Encoding(writeEncoding);

                thisLock = new object();

                this.version = version;
                this.maxReadPoolSize = maxReadPoolSize;
                this.maxWritePoolSize = maxWritePoolSize;

                this.readerQuotas = new XmlDictionaryReaderQuotas();
                quotas.CopyTo(this.readerQuotas);

                this.bufferedReadReaderQuotas = EncoderHelpers.GetBufferedReadQuotas(this.readerQuotas);

                this.onStreamedReaderClose = new OnXmlDictionaryReaderClose(ReturnStreamedReader);

                this.mediaType = TextMessageEncoderFactory.GetMediaType(version);
                this.contentType = TextMessageEncoderFactory.GetContentType(mediaType, writeEncoding);
                if (version.Envelope == EnvelopeVersion.Soap12)
                {
                    contentEncodingMap = TextMessageEncoderFactory.Soap12Content;
                }
                else if (version.Envelope == EnvelopeVersion.Soap11)
                {
                    contentEncodingMap = TextMessageEncoderFactory.Soap11Content;
                }
                else if (version.Envelope == EnvelopeVersion.None)
                {
                    contentEncodingMap = TextMessageEncoderFactory.SoapNoneContent;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.EnvelopeVersionNotSupported, version.Envelope)));
                }
            }

            static bool IsUTF8Encoding(Encoding encoding)
            {
                return encoding.WebName == "utf-8";
            }

            public override string ContentType
            {
                get { return contentType; }
            }

            public int MaxWritePoolSize
            {
                get { return maxWritePoolSize; }
            }

            public int MaxReadPoolSize
            {
                get { return maxReadPoolSize; }
            }

            public XmlDictionaryReaderQuotas ReaderQuotas
            {
                get
                {
                    return readerQuotas;
                }
            }

            public override string MediaType
            {
                get { return mediaType; }
            }

            public override MessageVersion MessageVersion
            {
                get { return version; }
            }

            object ThisLock
            {
                get { return thisLock; }
            }


            internal override bool IsCharSetSupported(string charSet)
            {
                Encoding tmp;
                return TextEncoderDefaults.TryGetEncoding(charSet, out tmp);
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                if (contentType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
                }

                if (base.IsContentTypeSupported(contentType))
                {
                    return true;
                }

                // we support a few extra content types for "none"
                if (MessageVersion == MessageVersion.None)
                {
                    const string rss1MediaType = "text/xml";
                    const string rss2MediaType = "application/rss+xml";
                    const string atomMediaType = "application/atom+xml";
                    const string htmlMediaType = "text/html";

                    if (IsContentTypeSupported(contentType, rss1MediaType, rss1MediaType))
                    {
                        return true;
                    }
                    if (IsContentTypeSupported(contentType, rss2MediaType, rss2MediaType))
                    {
                        return true;
                    }
                    if (IsContentTypeSupported(contentType, htmlMediaType, atomMediaType))
                    {
                        return true;
                    }
                    if (IsContentTypeSupported(contentType, atomMediaType, atomMediaType))
                    {
                        return true;
                    }
                    // application/xml checked by base method
                }

                return false;
            }

            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                if (bufferManager == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bufferManager"));

                if (TD.TextMessageDecodingStartIsEnabled())
                {
                    TD.TextMessageDecodingStart();
                }

                Message message;

                UTF8BufferedMessageData messageData = TakeBufferedReader();
                messageData.Encoding = GetEncodingFromContentType(contentType, this.contentEncodingMap);
                messageData.Open(buffer, bufferManager);
                RecycledMessageState messageState = messageData.TakeMessageState();
                if (messageState == null)
                    messageState = new RecycledMessageState();
                message = new BufferedMessage(messageData, messageState);

                message.Properties.Encoder = this;

                if (TD.MessageReadByEncoderIsEnabled() && buffer != null)
                {
                    TD.MessageReadByEncoder(
                        EventTraceActivityHelper.TryExtractActivity(message, true),
                        buffer.Count,
                        this);
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);

                return message;
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                if (stream == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));

                if (TD.TextMessageDecodingStartIsEnabled())
                {
                    TD.TextMessageDecodingStart();
                }

                XmlReader reader = TakeStreamedReader(stream, GetEncodingFromContentType(contentType, this.contentEncodingMap));
                Message message = Message.CreateMessage(reader, maxSizeOfHeaders, version);
                message.Properties.Encoder = this;

                if (TD.StreamedMessageReadByEncoderIsEnabled())
                {
                    TD.StreamedMessageReadByEncoder(EventTraceActivityHelper.TryExtractActivity(message, true));
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
                return message;
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                if (message == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                if (bufferManager == null)
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("bufferManager"), message);
                if (maxMessageSize < 0)
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxMessageSize", maxMessageSize,
                                                                SR.GetString(SR.ValueMustBeNonNegative)), message);
                if (messageOffset < 0 || messageOffset > maxMessageSize)
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageOffset", messageOffset,
                                                    SR.GetString(SR.ValueMustBeInRange, 0, maxMessageSize)), message);

                ThrowIfMismatchedMessageVersion(message);

                EventTraceActivity eventTraceActivity = null;
                if (TD.TextMessageEncodingStartIsEnabled())
                {
                    eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                    TD.TextMessageEncodingStart(eventTraceActivity);
                }

                message.Properties.Encoder = this;
                TextBufferedMessageWriter messageWriter = TakeBufferedWriter();
                ArraySegment<byte> messageData = messageWriter.WriteMessage(message, bufferManager, messageOffset, maxMessageSize);
                ReturnMessageWriter(messageWriter);

                if (TD.MessageWrittenByEncoderIsEnabled() && messageData != null)
                {
                    TD.MessageWrittenByEncoder(
                        eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(message),
                        messageData.Count,
                        this);
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(messageData.Array, messageData.Offset, messageData.Count, null, XmlDictionaryReaderQuotas.Max, null);
                    MessageLogger.LogMessage(ref message, xmlDictionaryReader, MessageLoggingSource.TransportSend);
                }

                return messageData;
            }

            public override void WriteMessage(Message message, Stream stream)
            {
                if (message == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                if (stream == null)
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("stream"), message);
                ThrowIfMismatchedMessageVersion(message);

                EventTraceActivity eventTraceActivity = null;
                if (TD.TextMessageEncodingStartIsEnabled())
                {
                    eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                    TD.TextMessageEncodingStart(eventTraceActivity);
                }

                message.Properties.Encoder = this;
                XmlDictionaryWriter xmlWriter = TakeStreamedWriter(stream);
                if (optimizeWriteForUTF8)
                {
                    message.WriteMessage(xmlWriter);
                }
                else
                {
                    xmlWriter.WriteStartDocument();
                    message.WriteMessage(xmlWriter);
                    xmlWriter.WriteEndDocument();
                }
                xmlWriter.Flush();
                ReturnStreamedWriter(xmlWriter);

                if (TD.StreamedMessageWrittenByEncoderIsEnabled())
                {
                    TD.StreamedMessageWrittenByEncoder(eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(message));
                }

                if (MessageLogger.LogMessagesAtTransportLevel)
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
            }

            public override IAsyncResult BeginWriteMessage(Message message, Stream stream, AsyncCallback callback, object state)
            {
                if (message == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                if (stream == null)
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("stream"), message);

                ThrowIfMismatchedMessageVersion(message);
                message.Properties.Encoder = this;

                return new WriteMessageAsyncResult(message, stream, this, callback, state);
            }

            public override void EndWriteMessage(IAsyncResult result)
            {
                WriteMessageAsyncResult.End(result);
            }

            class WriteMessageAsyncResult : AsyncResult
            {
                static AsyncCompletion onWriteMessage = new AsyncCompletion(OnWriteMessage);
                Message message;                
                TextMessageEncoder textEncoder;
                XmlDictionaryWriter xmlWriter;
                EventTraceActivity eventTraceActivity;

                public WriteMessageAsyncResult(Message message, Stream stream, TextMessageEncoder textEncoder, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.message = message;                    
                    this.textEncoder = textEncoder;
                    this.xmlWriter = textEncoder.TakeStreamedWriter(stream);


                    this.eventTraceActivity = null;
                    if (TD.TextMessageEncodingStartIsEnabled())
                    {
                        this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                        TD.TextMessageEncodingStart(this.eventTraceActivity);
                    }

                    if (!textEncoder.optimizeWriteForUTF8)
                    {
                        xmlWriter.WriteStartDocument();
                    }

                    IAsyncResult result = message.BeginWriteMessage(this.xmlWriter, PrepareAsyncCompletion(onWriteMessage), this);                    
                    if (SyncContinue(result))
                    {
                        this.Complete(true);
                    }
                }

                static bool OnWriteMessage(IAsyncResult result)
                {
                    WriteMessageAsyncResult thisPtr = (WriteMessageAsyncResult)result.AsyncState;
                    return thisPtr.HandleWriteMessage(result);
                }

                bool HandleWriteMessage(IAsyncResult result)
                {
                    message.EndWriteMessage(result);
                    if (!textEncoder.optimizeWriteForUTF8)
                    {
                        this.xmlWriter.WriteEndDocument();
                    }

                    xmlWriter.Flush();  // blocking call
                    textEncoder.ReturnStreamedWriter(this.xmlWriter);

                    if (TD.MessageWrittenAsynchronouslyByEncoderIsEnabled())
                    {
                        TD.MessageWrittenAsynchronouslyByEncoder(
                            this.eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(message));
                    }

                    if (MessageLogger.LogMessagesAtTransportLevel)
                        MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);

                    return true;
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<WriteMessageAsyncResult>(result);
                }
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
                    xmlWriter = XmlDictionaryWriter.CreateTextWriter(stream, this.writeEncoding, false);
                    if (TD.WritePoolMissIsEnabled())
                    {
                        TD.WritePoolMiss(xmlWriter.GetType().Name);
                    }
                }
                else
                {
                    ((IXmlTextWriterInitializer)xmlWriter).SetOutput(stream, this.writeEncoding, false);
                }
                return xmlWriter;
            }

            void ReturnStreamedWriter(XmlWriter xmlWriter)
            {
                xmlWriter.Close();
                streamedWriterPool.Return((XmlDictionaryWriter)xmlWriter);
            }

            TextBufferedMessageWriter TakeBufferedWriter()
            {
                if (bufferedWriterPool == null)
                {
                    lock (ThisLock)
                    {
                        if (bufferedWriterPool == null)
                        {
                            bufferedWriterPool = new SynchronizedPool<TextBufferedMessageWriter>(maxWritePoolSize);
                        }
                    }
                }

                TextBufferedMessageWriter messageWriter = bufferedWriterPool.Take();
                if (messageWriter == null)
                {
                    messageWriter = new TextBufferedMessageWriter(this);
                    if (TD.WritePoolMissIsEnabled())
                    {
                        TD.WritePoolMiss(messageWriter.GetType().Name);
                    }
                }
                return messageWriter;
            }

            void ReturnMessageWriter(TextBufferedMessageWriter messageWriter)
            {
                bufferedWriterPool.Return(messageWriter);
            }

            XmlReader TakeStreamedReader(Stream stream, Encoding enc)
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
                    xmlReader = XmlDictionaryReader.CreateTextReader(stream, enc, this.readerQuotas, null);
                    if (TD.ReadPoolMissIsEnabled())
                    {
                        TD.ReadPoolMiss(xmlReader.GetType().Name);
                    }
                }
                else
                {
                    ((IXmlTextReaderInitializer)xmlReader).SetInput(stream, enc, this.readerQuotas, onStreamedReaderClose);
                }
                return xmlReader;
            }

            void ReturnStreamedReader(XmlDictionaryReader xmlReader)
            {
                streamedReaderPool.Return(xmlReader);
            }

            XmlDictionaryWriter CreateWriter(Stream stream)
            {
                return XmlDictionaryWriter.CreateTextWriter(stream, writeEncoding, false);
            }

            UTF8BufferedMessageData TakeBufferedReader()
            {
                if (bufferedReaderPool == null)
                {
                    lock (ThisLock)
                    {
                        if (bufferedReaderPool == null)
                        {
                            bufferedReaderPool = new SynchronizedPool<UTF8BufferedMessageData>(maxReadPoolSize);
                        }
                    }
                }
                UTF8BufferedMessageData messageData = bufferedReaderPool.Take();
                if (messageData == null)
                {
                    messageData = new UTF8BufferedMessageData(this, maxPooledXmlReadersPerMessage);
                    if (TD.ReadPoolMissIsEnabled())
                    {
                        TD.ReadPoolMiss(messageData.GetType().Name);
                    }
                }
                return messageData;
            }

            void ReturnBufferedData(UTF8BufferedMessageData messageData)
            {
                bufferedReaderPool.Return(messageData);
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

            string ITraceSourceStringProvider.GetSourceString()
            {
                return base.GetTraceSourceString();
            }

            static readonly byte[] xmlDeclarationStartText = { (byte)'<', (byte)'?', (byte)'x', (byte)'m', (byte)'l' };
            static readonly byte[] version10Text = { (byte)'v', (byte)'e', (byte)'r', (byte)'s', (byte)'i', (byte)'o', (byte)'n', (byte)'=', (byte)'"', (byte)'1', (byte)'.', (byte)'0', (byte)'"' };
            static readonly byte[] encodingText = { (byte)'e', (byte)'n', (byte)'c', (byte)'o', (byte)'d', (byte)'i', (byte)'n', (byte)'g', (byte)'=' };

            class UTF8BufferedMessageData : BufferedMessageData
            {
                TextMessageEncoder messageEncoder;
                Pool<XmlDictionaryReader> readerPool;
                OnXmlDictionaryReaderClose onClose;
                Encoding encoding;

                const int additionalNodeSpace = 1024;

                public UTF8BufferedMessageData(TextMessageEncoder messageEncoder, int maxReaderPoolSize)
                    : base(messageEncoder.RecycledStatePool)
                {
                    this.messageEncoder = messageEncoder;
                    readerPool = new Pool<XmlDictionaryReader>(maxReaderPoolSize);
                    onClose = new OnXmlDictionaryReaderClose(OnXmlReaderClosed);
                }

                internal Encoding Encoding
                {
                    set
                    {
                        this.encoding = value;
                    }
                }

                public override MessageEncoder MessageEncoder
                {
                    get { return messageEncoder; }
                }

                public override XmlDictionaryReaderQuotas Quotas
                {
                    get { return messageEncoder.bufferedReadReaderQuotas; }
                }

                protected override void OnClosed()
                {
                    messageEncoder.ReturnBufferedData(this);
                }

                protected override XmlDictionaryReader TakeXmlReader()
                {
                    ArraySegment<byte> buffer = this.Buffer;

                    XmlDictionaryReader xmlReader = readerPool.Take();
                    if (xmlReader == null)
                    {
                        xmlReader = XmlDictionaryReader.CreateTextReader(buffer.Array, buffer.Offset, buffer.Count, this.encoding, this.Quotas, onClose);
                        if (TD.ReadPoolMissIsEnabled())
                        {
                            TD.ReadPoolMiss(xmlReader.GetType().Name);
                        }
                    }
                    else
                    {
                        ((IXmlTextReaderInitializer)xmlReader).SetInput(buffer.Array, buffer.Offset, buffer.Count, this.encoding, this.Quotas, onClose);
                    }

                    return xmlReader;
                }

                protected override void ReturnXmlReader(XmlDictionaryReader xmlReader)
                {
                    if (xmlReader != null)
                    {
                        readerPool.Return(xmlReader);
                    }
                }
            }

            class TextBufferedMessageWriter : BufferedMessageWriter
            {
                TextMessageEncoder messageEncoder;
                XmlDictionaryWriter writer;

                public TextBufferedMessageWriter(TextMessageEncoder messageEncoder)
                {
                    this.messageEncoder = messageEncoder;
                }

                protected override void OnWriteStartMessage(XmlDictionaryWriter writer)
                {
                    if (!messageEncoder.optimizeWriteForUTF8)
                        writer.WriteStartDocument();
                }

                protected override void OnWriteEndMessage(XmlDictionaryWriter writer)
                {
                    if (!messageEncoder.optimizeWriteForUTF8)
                        writer.WriteEndDocument();
                }

                protected override XmlDictionaryWriter TakeXmlWriter(Stream stream)
                {
                    if (messageEncoder.optimizeWriteForUTF8)
                    {
                        XmlDictionaryWriter returnedWriter = writer;
                        if (returnedWriter == null)
                        {
                            returnedWriter = XmlDictionaryWriter.CreateTextWriter(stream, messageEncoder.writeEncoding, false);
                        }
                        else
                        {
                            writer = null;
                            ((IXmlTextWriterInitializer)returnedWriter).SetOutput(stream, messageEncoder.writeEncoding, false);
                        }
                        return returnedWriter;
                    }
                    else
                    {
                        return messageEncoder.CreateWriter(stream);
                    }
                }

                protected override void ReturnXmlWriter(XmlDictionaryWriter writer)
                {
                    writer.Close();

                    if (messageEncoder.optimizeWriteForUTF8)
                    {
                        if (this.writer == null)
                            this.writer = writer;
                    }
                }
            }
        }
    }
}
