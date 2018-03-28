//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    public abstract class MessageEncoder
    {
        private string traceSourceString;

        public abstract string ContentType { get; }

        public abstract string MediaType { get; }

        public abstract MessageVersion MessageVersion { get; }

        public virtual T GetProperty<T>() where T : class
        {
            if (typeof(T) == typeof(FaultConverter))
            {
                return (T)(object)FaultConverter.GetDefaultFaultConverter(this.MessageVersion);
            }

            return null;
        }

        public Message ReadMessage(Stream stream, int maxSizeOfHeaders)
        {
            return ReadMessage(stream, maxSizeOfHeaders, null);
        }

        public abstract Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType);

        public Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            Message message = ReadMessage(buffer, bufferManager, null);
            return message;
        }

        public abstract Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType);

        // used for buffered streaming
        internal ArraySegment<byte> BufferMessageStream(Stream stream, BufferManager bufferManager, int maxBufferSize)
        {
            byte[] buffer = bufferManager.TakeBuffer(ConnectionOrientedTransportDefaults.ConnectionBufferSize);
            int offset = 0;
            int currentBufferSize = Math.Min(buffer.Length, maxBufferSize);

            while (offset < currentBufferSize)
            {
                int count = stream.Read(buffer, offset, currentBufferSize - offset);
                if (count == 0)
                {
                    stream.Close();
                    break;
                }

                offset += count;
                if (offset == currentBufferSize)
                {
                    if (currentBufferSize >= maxBufferSize)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(maxBufferSize));
                    }

                    currentBufferSize = Math.Min(currentBufferSize * 2, maxBufferSize);
                    byte[] temp = bufferManager.TakeBuffer(currentBufferSize);
                    Buffer.BlockCopy(buffer, 0, temp, 0, offset);
                    bufferManager.ReturnBuffer(buffer);
                    buffer = temp;
                }
            }

            return new ArraySegment<byte>(buffer, 0, offset);
        }

        // used for buffered streaming
        internal virtual Message ReadMessage(Stream stream, BufferManager bufferManager, int maxBufferSize, string contentType)
        {
            return ReadMessage(BufferMessageStream(stream, bufferManager, maxBufferSize), bufferManager, contentType);
        }

        public override string ToString()
        {
            return ContentType;
        }

        public abstract void WriteMessage(Message message, Stream stream);

        public virtual IAsyncResult BeginWriteMessage(Message message, Stream stream, AsyncCallback callback, object state)
        {
            return new WriteMessageAsyncResult(message, stream, this, callback, state);
        }

        public virtual void EndWriteMessage(IAsyncResult result)
        {
            WriteMessageAsyncResult.End(result);
        }

        public ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager)
        {
            ArraySegment<byte> arraySegment = WriteMessage(message, maxMessageSize, bufferManager, 0);
            return arraySegment;
        }

        public abstract ArraySegment<byte> WriteMessage(Message message, int maxMessageSize,
            BufferManager bufferManager, int messageOffset);

        public virtual bool IsContentTypeSupported(string contentType)
        {
            if (contentType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contentType"));

            return IsContentTypeSupported(contentType, this.ContentType, this.MediaType);
        }

        internal bool IsContentTypeSupported(string contentType, string supportedContentType, string supportedMediaType)
        {
            if (supportedContentType == contentType)
                return true;

            if (contentType.Length > supportedContentType.Length &&
                contentType.StartsWith(supportedContentType, StringComparison.Ordinal) &&
                contentType[supportedContentType.Length] == ';')
                return true;

            // now check case-insensitively
            if (contentType.StartsWith(supportedContentType, StringComparison.OrdinalIgnoreCase))
            {
                if (contentType.Length == supportedContentType.Length)
                {
                    return true;
                }
                else if (contentType.Length > supportedContentType.Length)
                {
                    char ch = contentType[supportedContentType.Length];

                    // Linear Whitespace is allowed to appear between the end of one property and the semicolon.
                    // LWS = [CRLF]? (SP | HT)+
                    if (ch == ';')
                    {
                        return true;
                    }

                    // Consume the [CRLF]?
                    int i = supportedContentType.Length;
                    if (ch == '\r' && contentType.Length > supportedContentType.Length + 1 && contentType[i + 1] == '\n')
                    {
                        i += 2;
                        ch = contentType[i];
                    }

                    // Look for a ';' or nothing after (SP | HT)+
                    if (ch == ' ' || ch == '\t')
                    {
                        i++;
                        while (i < contentType.Length)
                        {
                            ch = contentType[i];
                            if (ch != ' ' && ch != '\t')
                                break;
                            ++i;
                        }
                    }
                    if (ch == ';' || i == contentType.Length)
                        return true;
                }
            }

            // sometimes we get a contentType that has parameters, but our encoders
            // merely expose the base content-type, so we will check a stripped version
            try
            {
                ContentType parsedContentType = new ContentType(contentType);

                if (supportedMediaType.Length > 0 && !supportedMediaType.Equals(parsedContentType.MediaType, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (!IsCharSetSupported(parsedContentType.CharSet))
                    return false;
            }
            catch (FormatException)
            {
                // bad content type, so we definitely don't support it!
                return false;
            }

            return true;
        }

        internal virtual bool IsCharSetSupported(string charset)
        {
            return false;
        }

        internal void ThrowIfMismatchedMessageVersion(Message message)
        {
            if (message.Version != MessageVersion)
            {
                throw TraceUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.EncoderMessageVersionMismatch, message.Version, MessageVersion)),
                    message);
            }
        }

        internal string GetTraceSourceString()
        {
            if (this.traceSourceString == null)
            {
                this.traceSourceString = DiagnosticTraceBase.CreateDefaultSourceString(this);
            }

            return this.traceSourceString;
        }

        class WriteMessageAsyncResult : ScheduleActionItemAsyncResult
        {
            MessageEncoder encoder;
            Message message;
            Stream stream;

            public WriteMessageAsyncResult(Message message, Stream stream, MessageEncoder encoder, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(encoder != null, "encoder should never be null");

                this.encoder = encoder;
                this.message = message;
                this.stream = stream;

                Schedule();
            }

            protected override void OnDoWork()
            {
                this.encoder.WriteMessage(this.message, this.stream);
            }
        }
    }
}
