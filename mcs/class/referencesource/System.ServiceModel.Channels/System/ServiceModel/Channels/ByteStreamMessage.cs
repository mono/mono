//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Runtime;
    using System.Xml;

    public static class ByteStreamMessage
    {
        public static Message CreateMessage(Stream stream)
        {
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }
            return CreateMessage(stream, XmlDictionaryReaderQuotas.Max, true); // moveBodyReaderToContent is true, for consistency with the other implementations of Message (including the Message base class itself)
        }

        public static Message CreateMessage(ArraySegment<byte> buffer)
        {
            return CreateMessage(buffer, null);
        }

        public static Message CreateMessage(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            if (buffer.Array == null)
            {
                throw FxTrace.Exception.ArgumentNull("buffer.Array", SR.ArgumentPropertyShouldNotBeNullError("buffer.Array"));
            }

            ByteStreamBufferedMessageData data = new ByteStreamBufferedMessageData(buffer, bufferManager);
            return CreateMessage(data, XmlDictionaryReaderQuotas.Max, true); // moveBodyReaderToContent is true, for consistency with the other implementations of Message (including the Message base class itself)
        }

        internal static Message CreateMessage(Stream stream, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
        {
            return new InternalByteStreamMessage(stream, quotas, moveBodyReaderToContent);
        }

        internal static Message CreateMessage(HttpRequestMessage httpRequestMessage, XmlDictionaryReaderQuotas quotas)
        {
            return new InternalByteStreamMessage(httpRequestMessage, quotas, true); // moveBodyReaderToContent is true, for consistency with the other implementations of Message (including the Message base class itself)
        }

        internal static Message CreateMessage(HttpResponseMessage httpResponseMessage, XmlDictionaryReaderQuotas quotas)
        {
            return new InternalByteStreamMessage(httpResponseMessage, quotas, true); // moveBodyReaderToContent is true, for consistency with the other implementations of Message (including the Message base class itself)
        }

        internal static Message CreateMessage(ByteStreamBufferedMessageData bufferedMessageData, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
        {
            return new InternalByteStreamMessage(bufferedMessageData, quotas, moveBodyReaderToContent);
        }

        internal static bool IsInternalByteStreamMessage(Message message)
        {
            Fx.Assert(message != null, "message should not be null");
            return message is InternalByteStreamMessage;
        }

        class InternalByteStreamMessage : Message
        {
            BodyWriter bodyWriter;
            MessageHeaders headers;
            MessageProperties properties;
            XmlByteStreamReader reader;

            /// <summary>
            /// If set to true, OnGetReaderAtBodyContents() calls MoveToContent() on the reader before returning it.
            /// If set to false, the reader is positioned on None, just before the root element of the body message.
            /// </summary>
            /// <remarks>
            /// We use this flag to preserve compatibility between .net 4.0 (or previous) and .net 4.5 (or later).
            ///
            /// In .net 4.0:
            /// - WebMessageEncodingBindingElement uses a raw encoder, different than ByteStreamMessageEncoder.
            /// - ByteStreamMessageEncodingBindingElement uses the ByteStreamMessageEncoder.
            /// - When the WebMessageEncodingBindingElement is used, the Message.GetReaderAtBodyContents() method returns 
            ///   an XmlDictionaryReader positioned initially on content (the root element of the xml); that's because MoveToContent() is called
            ///   on the reader before it's returned. 
            /// - When the ByteStreamMessageEncodingBindingElement is used, the Message.GetReaderAtBodyContents() method returns an 
            ///   XmlDictionaryReader positioned initially on None (just before the root element).
            /// 
            /// In .net 4.5:
            /// - Both WebMessageEncodingBindingElement and ByteStreamMessageEncodingBindingElement use the ByteStreamMessageEncoder.
            /// - So we need the ByteStreamMessageEncoder to call MoveToContent() when used by WebMessageEncodingBindingElement, and not do so
            ///   when used by the ByteStreamMessageEncodingBindingElement.
            /// - Preserving the compatibility with 4.0 is important especially because 4.5 is an in-place upgrade of 4.0.
            /// 
            /// See 252277 @ CSDMain for other info.
            /// </remarks>
            bool moveBodyReaderToContent;

            public InternalByteStreamMessage(ByteStreamBufferedMessageData bufferedMessageData, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
            {
                // Assign both writer and reader here so that we can CreateBufferedCopy without the need to
                // abstract between a streamed or buffered message. We're protected here by the state on Message
                // preventing both a read/write. 

                quotas = ByteStreamMessageUtility.EnsureQuotas(quotas);

                this.bodyWriter = new BufferedBodyWriter(bufferedMessageData);
                this.headers = new MessageHeaders(MessageVersion.None);
                this.properties = new MessageProperties();
                this.reader = new XmlBufferedByteStreamReader(bufferedMessageData, quotas);
                this.moveBodyReaderToContent = moveBodyReaderToContent;
            }

            public InternalByteStreamMessage(Stream stream, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
            {
                // Assign both writer and reader here so that we can CreateBufferedCopy without the need to
                // abstract between a streamed or buffered message. We're protected here by the state on Message
                // preventing both a read/write on the same stream. 

                quotas = ByteStreamMessageUtility.EnsureQuotas(quotas);

                this.bodyWriter = StreamedBodyWriter.Create(stream);
                this.headers = new MessageHeaders(MessageVersion.None);
                this.properties = new MessageProperties();
                this.reader = XmlStreamedByteStreamReader.Create(stream, quotas);
                this.moveBodyReaderToContent = moveBodyReaderToContent;
            }

            public InternalByteStreamMessage(HttpRequestMessage httpRequestMessage, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
            {
                Fx.Assert(httpRequestMessage != null, "The 'httpRequestMessage' parameter should not be null.");

                // Assign both writer and reader here so that we can CreateBufferedCopy without the need to
                // abstract between a streamed or buffered message. We're protected here by the state on Message
                // preventing both a read/write on the same stream. 

                quotas = ByteStreamMessageUtility.EnsureQuotas(quotas);

                this.bodyWriter = StreamedBodyWriter.Create(httpRequestMessage);
                this.headers = new MessageHeaders(MessageVersion.None);
                this.properties = new MessageProperties();
                this.reader = XmlStreamedByteStreamReader.Create(httpRequestMessage, quotas);
                this.moveBodyReaderToContent = moveBodyReaderToContent;
            }

            public InternalByteStreamMessage(HttpResponseMessage httpResponseMessage, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
            {
                Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should not be null.");

                // Assign both writer and reader here so that we can CreateBufferedCopy without the need to
                // abstract between a streamed or buffered message. We're protected here by the state on Message
                // preventing both a read/write on the same stream. 

                quotas = ByteStreamMessageUtility.EnsureQuotas(quotas);

                this.bodyWriter = StreamedBodyWriter.Create(httpResponseMessage);
                this.headers = new MessageHeaders(MessageVersion.None);
                this.properties = new MessageProperties();
                this.reader = XmlStreamedByteStreamReader.Create(httpResponseMessage, quotas);
                this.moveBodyReaderToContent = moveBodyReaderToContent;
            }

            InternalByteStreamMessage(ByteStreamBufferedMessageData messageData, MessageHeaders headers, MessageProperties properties, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
            {
                this.headers = new MessageHeaders(headers);
                this.properties = new MessageProperties(properties);
                this.bodyWriter = new BufferedBodyWriter(messageData);
                this.reader = new XmlBufferedByteStreamReader(messageData, quotas);
                this.moveBodyReaderToContent = moveBodyReaderToContent;
            }

            public override MessageHeaders Headers
            {
                get
                {
                    if (this.IsDisposed)
                    {
                        throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed("message"));
                    }
                    return this.headers;
                }
            }

            public override bool IsEmpty
            {
                get
                {
                    if (this.IsDisposed)
                    {
                        throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed("message"));
                    }
                    return false;
                }
            }

            public override bool IsFault
            {
                get
                {
                    if (this.IsDisposed)
                    {
                        throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed("message"));
                    }
                    return false;
                }
            }

            public override MessageProperties Properties
            {
                get
                {
                    if (this.IsDisposed)
                    {
                        throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed("message"));
                    }
                    return this.properties;
                }
            }

            public override MessageVersion Version
            {
                get
                {
                    if (this.IsDisposed)
                    {
                        throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed("message"));
                    }
                    return MessageVersion.None;
                }
            }

            protected override void OnBodyToString(XmlDictionaryWriter writer)
            {
                if (this.bodyWriter.IsBuffered)
                {
                    bodyWriter.WriteBodyContents(writer);
                }
                else
                {
                    writer.WriteString(SR.MessageBodyIsStream);
                }
            }

            protected override void OnClose()
            {
                Exception ex = null;
                try
                {
                    base.OnClose();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    ex = e;
                }

                try
                {
                    if (properties != null)
                    {
                        properties.Dispose();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (ex == null)
                    {
                        ex = e;
                    }
                }

                try
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (ex == null)
                    {
                        ex = e;
                    }
                }

                if (ex != null)
                {
                    throw FxTrace.Exception.AsError(ex);
                }

                this.bodyWriter = null;
            }

            protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
            {
                BufferedBodyWriter bufferedBodyWriter;
                if (this.bodyWriter.IsBuffered)
                {
                    // Can hand this off in buffered case without making a new one. 
                    bufferedBodyWriter = (BufferedBodyWriter)this.bodyWriter;
                }
                else
                {
                    bufferedBodyWriter = (BufferedBodyWriter)this.bodyWriter.CreateBufferedCopy(maxBufferSize);
                }

                // Protected by Message state to be called only once. 
                this.bodyWriter = null;
                return new ByteStreamMessageBuffer(bufferedBodyWriter.MessageData, this.headers, this.properties, this.reader.Quotas, this.moveBodyReaderToContent);
            }

            protected override T OnGetBody<T>(XmlDictionaryReader reader)
            {
                Fx.Assert(reader is XmlByteStreamReader, "reader should be XmlByteStreamReader");
                if (this.IsDisposed)
                {
                    throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed("message"));
                }

                Type typeT = typeof(T);
                if (typeof(Stream) == typeT)
                {
                    Stream stream = (reader as XmlByteStreamReader).ToStream();
                    reader.Close();
                    return (T)(object)stream;
                }
                else if (typeof(byte[]) == typeT)
                {
                    byte[] buffer = (reader as XmlByteStreamReader).ToByteArray();
                    reader.Close();
                    return (T)(object)buffer;
                }
                throw FxTrace.Exception.AsError(
                    new NotSupportedException(SR.ByteStreamMessageGetTypeNotSupported(typeT.FullName)));
            }

            protected override XmlDictionaryReader OnGetReaderAtBodyContents()
            {
                XmlDictionaryReader r = this.reader;
                this.reader = null;

                if ((r != null) && this.moveBodyReaderToContent)
                {
                    r.MoveToContent();
                }

                return r;
            }

            protected override IAsyncResult OnBeginWriteMessage(XmlDictionaryWriter writer, AsyncCallback callback, object state)
            {
                WriteMessagePreamble(writer);
                return new OnWriteMessageAsyncResult(writer, this, callback, state);
            }

            protected override void OnEndWriteMessage(IAsyncResult result)
            {
                OnWriteMessageAsyncResult.End(result);
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                this.bodyWriter.WriteBodyContents(writer);
            }

            protected override IAsyncResult OnBeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
            {
                return this.bodyWriter.BeginWriteBodyContents(writer, callback, state);
            }

            protected override void OnEndWriteBodyContents(IAsyncResult result)
            {
                this.bodyWriter.EndWriteBodyContents(result);
            }

            class OnWriteMessageAsyncResult : AsyncResult
            {
                InternalByteStreamMessage message;
                XmlDictionaryWriter writer;

                public OnWriteMessageAsyncResult(XmlDictionaryWriter writer, InternalByteStreamMessage message, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.message = message;
                    this.writer = writer;

                    IAsyncResult result = this.message.OnBeginWriteBodyContents(this.writer, PrepareAsyncCompletion(HandleWriteBodyContents), this);
                    bool completeSelf = SyncContinue(result);

                    if (completeSelf)
                    {
                        this.Complete(true);
                    }
                }

                static bool HandleWriteBodyContents(IAsyncResult result)
                {
                    OnWriteMessageAsyncResult thisPtr = (OnWriteMessageAsyncResult)result.AsyncState;
                    thisPtr.message.OnEndWriteBodyContents(result);
                    thisPtr.message.WriteMessagePostamble(thisPtr.writer);
                    return true;
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<OnWriteMessageAsyncResult>(result);
                }
            }
            
            class BufferedBodyWriter : BodyWriter
            {
                ByteStreamBufferedMessageData bufferedMessageData;

                public BufferedBodyWriter(ByteStreamBufferedMessageData bufferedMessageData)
                    : base(true)
                {
                    this.bufferedMessageData = bufferedMessageData;
                }

                internal ByteStreamBufferedMessageData MessageData
                {
                    get { return bufferedMessageData; }
                }

                protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
                {
                    // Never called because when copying a Buffered message, we simply hand off the existing BodyWriter 
                    // to the new message.
                    Fx.Assert(false, "This is never called");
                    return null;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    writer.WriteStartElement(ByteStreamMessageUtility.StreamElementName, string.Empty);
                    writer.WriteBase64(this.bufferedMessageData.Buffer.Array, this.bufferedMessageData.Buffer.Offset, this.bufferedMessageData.Buffer.Count);
                    writer.WriteEndElement();
                }
            }

            abstract class StreamedBodyWriter : BodyWriter
            {
                private StreamedBodyWriter()
                    : base(false)
                {
                }

                public static StreamedBodyWriter Create(Stream stream)
                {
                    return new StreamBasedStreamedBodyWriter(stream);
                }

                public static StreamedBodyWriter Create(HttpRequestMessage httpRequestMessage)
                {
                    return new HttpRequestMessageStreamedBodyWriter(httpRequestMessage);
                }

                public static StreamedBodyWriter Create(HttpResponseMessage httpResponseMessage)
                {
                    return new HttpResponseMessageStreamedBodyWriter(httpResponseMessage);
                }

                // OnCreateBufferedCopy / OnWriteBodyContents can only be called once - protected by state on Message (either copied or written once)
                protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
                {
                    using (BufferManagerOutputStream bufferedStream = new BufferManagerOutputStream(SR.MaxReceivedMessageSizeExceeded("{0}"), maxBufferSize))
                    {
                        using (XmlDictionaryWriter writer = new XmlByteStreamWriter(bufferedStream, true))
                        {
                            OnWriteBodyContents(writer);
                            writer.Flush();
                            int size;
                            byte[] bytesArray = bufferedStream.ToArray(out size);
                            ByteStreamBufferedMessageData bufferedMessageData = new ByteStreamBufferedMessageData(new ArraySegment<byte>(bytesArray, 0, size));
                            return new BufferedBodyWriter(bufferedMessageData);
                        }
                    }
                }

                // OnCreateBufferedCopy / OnWriteBodyContents can only be called once - protected by state on Message (either copied or written once)
                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    writer.WriteStartElement(ByteStreamMessageUtility.StreamElementName, string.Empty);
                    writer.WriteValue(new ByteStreamStreamProvider(this.GetStream()));
                    writer.WriteEndElement();
                }

                protected override IAsyncResult OnBeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
                {
                    return new WriteBodyContentsAsyncResult(writer, this.GetStream(), callback, state);
                }

                protected override void OnEndWriteBodyContents(IAsyncResult result)
                {
                    WriteBodyContentsAsyncResult.End(result);
                }

                protected abstract Stream GetStream();

                class ByteStreamStreamProvider : IStreamProvider
                {
                    Stream stream;

                    internal ByteStreamStreamProvider(Stream stream)
                    {
                        this.stream = stream;
                    }

                    public Stream GetStream()
                    {
                        return stream;
                    }

                    public void ReleaseStream(Stream stream)
                    {
                        //Noop
                    }
                }

                class WriteBodyContentsAsyncResult : AsyncResult
                {
                    XmlDictionaryWriter writer;

                    public WriteBodyContentsAsyncResult(XmlDictionaryWriter writer, Stream stream, AsyncCallback callback, object state)
                        : base(callback, state)
                    {
                        this.writer = writer;

                        this.writer.WriteStartElement(ByteStreamMessageUtility.StreamElementName, string.Empty);
                        IAsyncResult result = this.writer.WriteValueAsync(new ByteStreamStreamProvider(stream)).AsAsyncResult(PrepareAsyncCompletion(HandleWriteBodyContents), this);
                        bool completeSelf = SyncContinue(result);

                        // Note:  The current task implementation hard codes the "IAsyncResult.CompletedSynchronously" property to false, so this fast path will never
                        // be hit, and we will always hop threads.  CSDMain #210220
                        if (completeSelf)
                        {
                            this.Complete(true);
                        }
                    }

                    static bool HandleWriteBodyContents(IAsyncResult result)
                    {
                        WriteBodyContentsAsyncResult thisPtr = (WriteBodyContentsAsyncResult)result.AsyncState;
                        thisPtr.writer.WriteEndElement();
                        return true;
                    }

                    public static void End(IAsyncResult result)
                    {
                        AsyncResult.End<WriteBodyContentsAsyncResult>(result);
                    }
                }

                class StreamBasedStreamedBodyWriter : StreamedBodyWriter
                {
                    private Stream stream;

                    public StreamBasedStreamedBodyWriter(Stream stream)
                    {

                        this.stream = stream;
                    }

                    protected override Stream GetStream()
                    {
                        return this.stream;
                    }
                }

                class HttpRequestMessageStreamedBodyWriter : StreamedBodyWriter
                {
                    private HttpRequestMessage httpRequestMessage;

                    public HttpRequestMessageStreamedBodyWriter(HttpRequestMessage httpRequestMessage)
                    {
                        Fx.Assert(httpRequestMessage != null, "The 'httpRequestMessage' parameter should not be null.");

                        this.httpRequestMessage = httpRequestMessage;
                    }

                    protected override Stream GetStream()
                    {
                        HttpContent content = this.httpRequestMessage.Content;
                        if (content != null)
                        {
                            return content.ReadAsStreamAsync().Result;
                        }

                        return new MemoryStream(EmptyArray<byte>.Instance);
                    }

                    protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
                    {
                        HttpContent content = this.httpRequestMessage.Content;
                        if (content != null)
                        {
                            content.LoadIntoBufferAsync(maxBufferSize).Wait();
                        }

                        return base.OnCreateBufferedCopy(maxBufferSize);
                    }
                }

                class HttpResponseMessageStreamedBodyWriter : StreamedBodyWriter
                {
                    private HttpResponseMessage httpResponseMessage;

                    public HttpResponseMessageStreamedBodyWriter(HttpResponseMessage httpResponseMessage)
                    {
                        Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should not be null.");

                        this.httpResponseMessage = httpResponseMessage;
                    }

                    protected override Stream GetStream()
                    {
                        HttpContent content = this.httpResponseMessage.Content;
                        if (content != null)
                        {
                            return content.ReadAsStreamAsync().Result;
                        }

                        return new MemoryStream(EmptyArray<byte>.Instance);
                    }

                    protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
                    {
                        HttpContent content = this.httpResponseMessage.Content;
                        if (content != null)
                        {
                            content.LoadIntoBufferAsync(maxBufferSize).Wait();
                        }

                        return base.OnCreateBufferedCopy(maxBufferSize);
                    }
                }
            }

            class ByteStreamMessageBuffer : MessageBuffer
            {
                bool closed;
                MessageHeaders headers;
                ByteStreamBufferedMessageData messageData;
                MessageProperties properties;
                XmlDictionaryReaderQuotas quotas;
                bool moveBodyReaderToContent;
                object thisLock = new object();

                public ByteStreamMessageBuffer(ByteStreamBufferedMessageData messageData, MessageHeaders headers, MessageProperties properties, XmlDictionaryReaderQuotas quotas, bool moveBodyReaderToContent)
                    : base()
                {
                    this.messageData = messageData;
                    this.headers = new MessageHeaders(headers);
                    this.properties = new MessageProperties(properties);
                    this.quotas = new XmlDictionaryReaderQuotas();
                    quotas.CopyTo(this.quotas);
                    this.moveBodyReaderToContent = moveBodyReaderToContent;

                    this.messageData.Open();                
                }

                public override int BufferSize
                {
                    get { return this.messageData.Buffer.Count; }
                }

                object ThisLock
                {
                    get { return this.thisLock; }
                }

                public override void Close()
                {
                    lock (ThisLock)
                    {
                        if (!closed)
                        {
                            closed = true;
                            this.headers = null;
                            if (properties != null)
                            {
                                properties.Dispose();
                                properties = null;
                            }
                            this.messageData.Close();
                            this.messageData = null;
                            this.quotas = null;
                        }
                    }
                }

                public override Message CreateMessage()
                {
                    lock (ThisLock)
                    {
                        if (closed)
                        {
                            throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed("message"));
                        }

                        return new InternalByteStreamMessage(this.messageData, this.headers, this.properties, this.quotas, this.moveBodyReaderToContent);
                    }
                }
            }
        }
    }
}
