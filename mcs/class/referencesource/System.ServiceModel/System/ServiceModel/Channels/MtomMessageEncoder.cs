//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;
    using System.Runtime.Diagnostics;

    class MtomMessageEncoderFactory : MessageEncoderFactory
    {
        MtomMessageEncoder messageEncoder;

        public MtomMessageEncoderFactory(MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, int maxBufferSize, XmlDictionaryReaderQuotas quotas)
        {
            messageEncoder = new MtomMessageEncoder(version, writeEncoding, maxReadPoolSize, maxWritePoolSize, maxBufferSize, quotas);
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

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return messageEncoder.ReaderQuotas;
            }
        }

        public int MaxBufferSize
        {
            get { return messageEncoder.MaxBufferSize; }
        }

        public static Encoding[] GetSupportedEncodings()
        {
            Encoding[] supported = TextEncoderDefaults.SupportedEncodings;
            Encoding[] enc = new Encoding[supported.Length];
            Array.Copy(supported, enc, supported.Length);
            return enc;
        }

    }

    // Some notes:
    // The Encoding passed in is used for the SOAP envelope
    class MtomMessageEncoder : MessageEncoder, ITraceSourceStringProvider
    {
        Encoding writeEncoding;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile SynchronizedPool<XmlDictionaryWriter> streamedWriterPool;
        volatile SynchronizedPool<XmlDictionaryReader> streamedReaderPool;
        volatile SynchronizedPool<MtomBufferedMessageData> bufferedReaderPool;
        volatile SynchronizedPool<MtomBufferedMessageWriter> bufferedWriterPool;
        volatile SynchronizedPool<RecycledMessageState> recycledStatePool;

        object thisLock;
        MessageVersion version;
        const int maxPooledXmlReadersPerMessage = 2;
        int maxReadPoolSize;
        int maxWritePoolSize;
        static UriGenerator mimeBoundaryGenerator;
        XmlDictionaryReaderQuotas readerQuotas;
        XmlDictionaryReaderQuotas bufferedReadReaderQuotas;
        int maxBufferSize;
        OnXmlDictionaryReaderClose onStreamedReaderClose;

        internal TextMessageEncoderFactory.ContentEncoding[] contentEncodingMap;

        const string mtomMediaType = "multipart/related";
        const string mtomContentType = mtomMediaType + "; type=\"application/xop+xml\"";
        const string mtomStartUri = NamingHelper.DefaultNamespace + "0";

        public MtomMessageEncoder(MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, int maxBufferSize, XmlDictionaryReaderQuotas quotas)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            if (writeEncoding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");

            TextEncoderDefaults.ValidateEncoding(writeEncoding);
            this.writeEncoding = writeEncoding;

            this.maxReadPoolSize = maxReadPoolSize;
            this.maxWritePoolSize = maxWritePoolSize;

            this.readerQuotas = new XmlDictionaryReaderQuotas();
            quotas.CopyTo(this.readerQuotas);

            this.bufferedReadReaderQuotas = EncoderHelpers.GetBufferedReadQuotas(this.readerQuotas);

            this.maxBufferSize = maxBufferSize;
            this.onStreamedReaderClose = new OnXmlDictionaryReaderClose(ReturnStreamedReader);

            this.thisLock = new object();

            if (version.Envelope == EnvelopeVersion.Soap12)
            {
                this.contentEncodingMap = TextMessageEncoderFactory.Soap12Content;
            }
            else if (version.Envelope == EnvelopeVersion.Soap11)
            {
                this.contentEncodingMap = TextMessageEncoderFactory.Soap11Content;
            }
            else
            {
                Fx.Assert("Invalid MessageVersion");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Invalid MessageVersion")));
            }

            this.version = version;
        }

        static UriGenerator MimeBoundaryGenerator
        {
            get
            {
                if (mimeBoundaryGenerator == null)
                    mimeBoundaryGenerator = new UriGenerator("uuid", "+");
                return mimeBoundaryGenerator;
            }
        }

        public override string ContentType
        {
            get { return mtomContentType; }
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

        public int MaxBufferSize
        {
            get { return maxBufferSize; }
        }

        public override string MediaType
        {
            get { return mtomMediaType; }
        }

        public override MessageVersion MessageVersion
        {
            get { return version; }
        }

        internal bool IsMTOMContentType(string contentType)
        {
            // check for MTOM contentType: multipart/related; type=\"application/xop+xml\"
            return IsContentTypeSupported(contentType, this.ContentType, this.MediaType);
        }

        internal bool IsTextContentType(string contentType)
        {
            // check for Text contentType: text/xml or application/soap+xml
            string textMediaType = TextMessageEncoderFactory.GetMediaType(version);
            string textContentType = TextMessageEncoderFactory.GetContentType(textMediaType, writeEncoding);
            return IsContentTypeSupported(contentType, textContentType, textMediaType);
        }

        public override bool IsContentTypeSupported(string contentType)
        {
            if (contentType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contentType"));
            return (IsMTOMContentType(contentType) || IsTextContentType(contentType));
        }

        internal override bool IsCharSetSupported(string charSet)
        {
            if (charSet == null || charSet.Length == 0)
                return true;

            Encoding tmp;
            return TextEncoderDefaults.TryGetEncoding(charSet, out tmp);
        }

        string GenerateStartInfoString()
        {
            return (version.Envelope == EnvelopeVersion.Soap12) ? TextMessageEncoderFactory.Soap12MediaType : TextMessageEncoderFactory.Soap11MediaType;
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            if (bufferManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bufferManager");

            if (contentType == this.ContentType)
                contentType = null;

            if (TD.MtomMessageDecodingStartIsEnabled())
            {
                TD.MtomMessageDecodingStart();
            }

            MtomBufferedMessageData messageData = TakeBufferedReader();
            messageData.ContentType = contentType;
            messageData.Open(buffer, bufferManager);
            RecycledMessageState messageState = messageData.TakeMessageState();
            if (messageState == null)
                messageState = new RecycledMessageState();
            Message message = new BufferedMessage(messageData, messageState);
            message.Properties.Encoder = this;
            if (MessageLogger.LogMessagesAtTransportLevel)
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);

            if (TD.MessageReadByEncoderIsEnabled() && buffer != null)
            {
                TD.MessageReadByEncoder(
                    EventTraceActivityHelper.TryExtractActivity(message, true),
                    buffer.Count,
                    this);
            }

            return message;
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));

            if (contentType == this.ContentType)
                contentType = null;

            if (TD.MtomMessageDecodingStartIsEnabled())
            {
                TD.MtomMessageDecodingStart();
            }

            XmlReader reader = TakeStreamedReader(stream, contentType);
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
            return WriteMessage(message, maxMessageSize, bufferManager, messageOffset, GenerateStartInfoString(), null, null, true /*writeMessageHeaders*/);
        }

        internal string GetContentType(out string boundary)
        {
            string startInfo = GenerateStartInfoString();
            boundary = MimeBoundaryGenerator.Next();

            return FormatContentType(boundary, startInfo);
        }

        internal string FormatContentType(string boundary, string startInfo)
        {
            return String.Format(CultureInfo.InvariantCulture,
                "{0};start=\"<{1}>\";boundary=\"{2}\";start-info=\"{3}\"",
                mtomContentType, mtomStartUri, boundary, startInfo);
        }

        internal ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset, string boundary)
        {
            return WriteMessage(message, maxMessageSize, bufferManager, messageOffset, GenerateStartInfoString(), boundary, mtomStartUri, false /*writeMessageHeaders*/);
        }

        ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset, string startInfo, string boundary, string startUri, bool writeMessageHeaders)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            if (bufferManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bufferManager");
            if (maxMessageSize < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxMessageSize", maxMessageSize,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
            if (messageOffset < 0 || messageOffset > maxMessageSize)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageOffset", messageOffset,
                                                    SR.GetString(SR.ValueMustBeInRange, 0, maxMessageSize)));
            ThrowIfMismatchedMessageVersion(message);

            EventTraceActivity eventTraceActivity = null;
            if (TD.MtomMessageEncodingStartIsEnabled())
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                TD.MtomMessageEncodingStart(eventTraceActivity);
            }

            message.Properties.Encoder = this;

            MtomBufferedMessageWriter messageWriter = TakeBufferedWriter();
            messageWriter.StartInfo = startInfo;
            messageWriter.Boundary = boundary;
            messageWriter.StartUri = startUri;
            messageWriter.WriteMessageHeaders = writeMessageHeaders;
            messageWriter.MaxSizeInBytes = maxMessageSize;
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
                string contentType = null;
                if (boundary != null)
                    contentType = FormatContentType(boundary, startInfo ?? GenerateStartInfoString());

                XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateMtomReader(messageData.Array, messageData.Offset, messageData.Count, MtomMessageEncoderFactory.GetSupportedEncodings(), contentType, XmlDictionaryReaderQuotas.Max, int.MaxValue, null);
                MessageLogger.LogMessage(ref message, xmlDictionaryReader, MessageLoggingSource.TransportSend);
            }

            return messageData;
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            WriteMessage(message, stream, GenerateStartInfoString(), null, null, true /*writeMessageHeaders*/);
        }

        internal void WriteMessage(Message message, Stream stream, string boundary)
        {
            WriteMessage(message, stream, GenerateStartInfoString(), boundary, mtomStartUri, false /*writeMessageHeaders*/);
        }

        public override IAsyncResult BeginWriteMessage(Message message, Stream stream, AsyncCallback callback, object state)
        {
            return new WriteMessageAsyncResult(message, stream, this, callback, state);
        }

        internal IAsyncResult BeginWriteMessage(Message message, Stream stream, string boundary, AsyncCallback callback, object state)
        {
            return new WriteMessageAsyncResult(message, stream, boundary, this, callback, state);
        }

        public override void EndWriteMessage(IAsyncResult result)
        {
            WriteMessageAsyncResult.End(result);
        }

        void WriteMessage(Message message, Stream stream, string startInfo, string boundary, string startUri, bool writeMessageHeaders)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            if (stream == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("stream"), message);
            ThrowIfMismatchedMessageVersion(message);

            EventTraceActivity eventTraceActivity = null;
            if (TD.MtomMessageEncodingStartIsEnabled())
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                TD.MtomMessageEncodingStart(eventTraceActivity);
            }

            message.Properties.Encoder = this;
            if (MessageLogger.LogMessagesAtTransportLevel)
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
            XmlDictionaryWriter xmlWriter = TakeStreamedWriter(stream, startInfo, boundary, startUri, writeMessageHeaders);
            if (this.writeEncoding.WebName == "utf-8")
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
        }

        XmlDictionaryWriter TakeStreamedWriter(Stream stream, string startInfo, string boundary, string startUri, bool writeMessageHeaders)
        {
            if (streamedWriterPool == null)
            {
                lock (thisLock)
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
                xmlWriter = XmlDictionaryWriter.CreateMtomWriter(stream, this.writeEncoding, int.MaxValue, startInfo, boundary, startUri, writeMessageHeaders, false);
                if (TD.WritePoolMissIsEnabled())
                {
                    TD.WritePoolMiss(xmlWriter.GetType().Name);
                }
            }
            else
            {
                ((IXmlMtomWriterInitializer)xmlWriter).SetOutput(stream, this.writeEncoding, int.MaxValue, startInfo, boundary, startUri, writeMessageHeaders, false);
            }
            return xmlWriter;
        }

        void ReturnStreamedWriter(XmlDictionaryWriter xmlWriter)
        {
            xmlWriter.Close();
            streamedWriterPool.Return(xmlWriter);
        }

        MtomBufferedMessageWriter TakeBufferedWriter()
        {
            if (bufferedWriterPool == null)
            {
                lock (thisLock)
                {
                    if (bufferedWriterPool == null)
                    {
                        bufferedWriterPool = new SynchronizedPool<MtomBufferedMessageWriter>(maxWritePoolSize);
                    }
                }
            }

            MtomBufferedMessageWriter messageWriter = bufferedWriterPool.Take();
            if (messageWriter == null)
            {
                messageWriter = new MtomBufferedMessageWriter(this);
                if (TD.WritePoolMissIsEnabled())
                {
                    TD.WritePoolMiss(messageWriter.GetType().Name);
                }
            }
            return messageWriter;
        }

        void ReturnMessageWriter(MtomBufferedMessageWriter messageWriter)
        {
            bufferedWriterPool.Return(messageWriter);
        }

        MtomBufferedMessageData TakeBufferedReader()
        {
            if (bufferedReaderPool == null)
            {
                lock (thisLock)
                {
                    if (bufferedReaderPool == null)
                    {
                        bufferedReaderPool = new SynchronizedPool<MtomBufferedMessageData>(maxReadPoolSize);
                    }
                }
            }
            MtomBufferedMessageData messageData = bufferedReaderPool.Take();
            if (messageData == null)
            {
                messageData = new MtomBufferedMessageData(this, maxPooledXmlReadersPerMessage);
                if (TD.ReadPoolMissIsEnabled())
                {
                    TD.ReadPoolMiss(messageData.GetType().Name);
                }
            }
            return messageData;
        }

        void ReturnBufferedData(MtomBufferedMessageData messageData)
        {
            bufferedReaderPool.Return(messageData);
        }

        XmlReader TakeStreamedReader(Stream stream, string contentType)
        {
            if (streamedReaderPool == null)
            {
                lock (thisLock)
                {
                    if (streamedReaderPool == null)
                    {
                        streamedReaderPool = new SynchronizedPool<XmlDictionaryReader>(maxReadPoolSize);
                    }
                }
            }
            XmlDictionaryReader xmlReader = streamedReaderPool.Take();
            try
            {
                if (contentType == null || IsMTOMContentType(contentType))
                {
                    if (xmlReader != null && xmlReader is IXmlMtomReaderInitializer)
                    {
                        ((IXmlMtomReaderInitializer)xmlReader).SetInput(stream, MtomMessageEncoderFactory.GetSupportedEncodings(), contentType, this.readerQuotas, this.maxBufferSize, onStreamedReaderClose);
                    }
                    else
                    {
                        xmlReader = XmlDictionaryReader.CreateMtomReader(stream, MtomMessageEncoderFactory.GetSupportedEncodings(), contentType, this.readerQuotas, this.maxBufferSize, onStreamedReaderClose);
                        if (TD.ReadPoolMissIsEnabled())
                        {
                            TD.ReadPoolMiss(xmlReader.GetType().Name);
                        }
                    }
                }
                else
                {
                    if (xmlReader != null && xmlReader is IXmlTextReaderInitializer)
                    {
                        ((IXmlTextReaderInitializer)xmlReader).SetInput(stream, TextMessageEncoderFactory.GetEncodingFromContentType(contentType, this.contentEncodingMap), this.readerQuotas, onStreamedReaderClose);
                    }
                    else
                    {
                        xmlReader = XmlDictionaryReader.CreateTextReader(stream, TextMessageEncoderFactory.GetEncodingFromContentType(contentType, this.contentEncodingMap), this.readerQuotas, onStreamedReaderClose);
                        if (TD.ReadPoolMissIsEnabled())
                        {
                            TD.ReadPoolMiss(xmlReader.GetType().Name);
                        }
                    }
                }
            }
            catch (FormatException fe)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorCreatingMtomReader), fe));
            }
            catch (XmlException xe)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorCreatingMtomReader), xe));
            }

            return xmlReader;
        }

        void ReturnStreamedReader(XmlDictionaryReader xmlReader)
        {
            streamedReaderPool.Return(xmlReader);
        }

        SynchronizedPool<RecycledMessageState> RecycledStatePool
        {
            get
            {
                if (recycledStatePool == null)
                {
                    lock (thisLock)
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

        class MtomBufferedMessageData : BufferedMessageData
        {
            MtomMessageEncoder messageEncoder;
            Pool<XmlDictionaryReader> readerPool;
            internal string ContentType;
            OnXmlDictionaryReaderClose onClose;

            public MtomBufferedMessageData(MtomMessageEncoder messageEncoder, int maxReaderPoolSize)
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

            protected override void OnClosed()
            {
                messageEncoder.ReturnBufferedData(this);
            }

            protected override XmlDictionaryReader TakeXmlReader()
            {
                try
                {
                    ArraySegment<byte> buffer = this.Buffer;

                    XmlDictionaryReader xmlReader = readerPool.Take();
                    if (ContentType == null || messageEncoder.IsMTOMContentType(ContentType))
                    {
                        if (xmlReader != null && xmlReader is IXmlMtomReaderInitializer)
                        {
                            ((IXmlMtomReaderInitializer)xmlReader).SetInput(buffer.Array, buffer.Offset, buffer.Count, MtomMessageEncoderFactory.GetSupportedEncodings(), ContentType, this.Quotas, this.messageEncoder.MaxBufferSize, onClose);
                        }
                        else
                        {
                            xmlReader = XmlDictionaryReader.CreateMtomReader(buffer.Array, buffer.Offset, buffer.Count, MtomMessageEncoderFactory.GetSupportedEncodings(), ContentType, this.Quotas, this.messageEncoder.MaxBufferSize, onClose);
                            if (TD.ReadPoolMissIsEnabled())
                            {
                                TD.ReadPoolMiss(xmlReader.GetType().Name);
                            }
                        }
                    }
                    else
                    {
                        if (xmlReader != null && xmlReader is IXmlTextReaderInitializer)
                        {
                            ((IXmlTextReaderInitializer)xmlReader).SetInput(buffer.Array, buffer.Offset, buffer.Count, TextMessageEncoderFactory.GetEncodingFromContentType(ContentType, this.messageEncoder.contentEncodingMap), this.Quotas, onClose);
                        }
                        else
                        {
                            xmlReader = XmlDictionaryReader.CreateTextReader(buffer.Array, buffer.Offset, buffer.Count, TextMessageEncoderFactory.GetEncodingFromContentType(ContentType, this.messageEncoder.contentEncodingMap), this.Quotas, onClose);
                            if (TD.ReadPoolMissIsEnabled())
                            {
                                TD.ReadPoolMiss(xmlReader.GetType().Name);
                            }
                        }
                    }
                    return xmlReader;
                }
                catch (FormatException fe)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                        SR.GetString(SR.SFxErrorCreatingMtomReader), fe));
                }
                catch (XmlException xe)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                        SR.GetString(SR.SFxErrorCreatingMtomReader), xe));
                }
            }

            protected override void ReturnXmlReader(XmlDictionaryReader xmlReader)
            {
                if (xmlReader != null)
                    readerPool.Return(xmlReader);
            }
        }

        class MtomBufferedMessageWriter : BufferedMessageWriter
        {
            MtomMessageEncoder messageEncoder;
            internal bool WriteMessageHeaders;
            internal string StartInfo;
            internal string StartUri;
            internal string Boundary;
            internal int MaxSizeInBytes = int.MaxValue;
            XmlDictionaryWriter writer;

            public MtomBufferedMessageWriter(MtomMessageEncoder messageEncoder)
            {
                this.messageEncoder = messageEncoder;
            }

            protected override XmlDictionaryWriter TakeXmlWriter(Stream stream)
            {
                XmlDictionaryWriter returnedWriter = writer;
                if (returnedWriter == null)
                {
                    returnedWriter = XmlDictionaryWriter.CreateMtomWriter(stream, messageEncoder.writeEncoding, MaxSizeInBytes, StartInfo, Boundary, StartUri, WriteMessageHeaders, false);
                }
                else
                {
                    writer = null;
                    ((IXmlMtomWriterInitializer)returnedWriter).SetOutput(stream, messageEncoder.writeEncoding, MaxSizeInBytes, StartInfo, Boundary, StartUri, WriteMessageHeaders, false);
                }
                if (messageEncoder.writeEncoding.WebName != "utf-8")
                    returnedWriter.WriteStartDocument();
                return returnedWriter;
            }

            protected override void ReturnXmlWriter(XmlDictionaryWriter writer)
            {
                writer.Close();

                if (this.writer == null)
                    this.writer = writer;
            }
        }

        class WriteMessageAsyncResult : ScheduleActionItemAsyncResult
        {
            string boundary;
            MtomMessageEncoder encoder;
            Message message;
            Stream stream;
            bool writeBoundary;

            public WriteMessageAsyncResult(Message message, Stream stream, MtomMessageEncoder encoder, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(encoder != null, "encoder should never be null");

                this.encoder = encoder;
                this.message = message;
                this.stream = stream;

                Schedule();
            }

            public WriteMessageAsyncResult(Message message, Stream stream, string boundary, MtomMessageEncoder encoder, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(encoder != null, "encoder should never be null");

                this.encoder = encoder;
                this.message = message;
                this.stream = stream;
                this.boundary = boundary;

                this.writeBoundary = true;

                Schedule();
            }

            protected override void OnDoWork()
            {
                this.encoder.WriteMessage(this.message, this.stream, this.encoder.GenerateStartInfoString(), string.IsNullOrEmpty(this.boundary) ? null : this.boundary, this.writeBoundary ? MtomMessageEncoder.mtomStartUri : null, !this.writeBoundary /*writeMessageHeaders*/);
            }
        }
    }
}
