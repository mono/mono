//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.IO;
    using System.Runtime.Diagnostics;
    using SMTD = System.ServiceModel.Diagnostics.Application.TD;
    using System.Diagnostics;

    class ByteStreamMessageEncoder : MessageEncoder, IStreamedMessageEncoder, IWebMessageEncoderHelper, ITraceSourceStringProvider
    {
        string traceSourceString;
        string maxReceivedMessageSizeExceededResourceString;
        string maxSentMessageSizeExceededResourceString;
        XmlDictionaryReaderQuotas quotas;
        XmlDictionaryReaderQuotas bufferedReadReaderQuotas;
        
        /// <summary>
        /// Specifies if this encoder produces Messages that provide a body reader (with the Message.GetReaderAtBodyContents() method) positioned on content.
        /// See the comments on 'IWebMessageEncoderHelper' for more info.
        /// </summary>
        bool moveBodyReaderToContent = false; // false because we want the ByteStreamMessageEncoder to be compatible with previous releases

        public ByteStreamMessageEncoder(XmlDictionaryReaderQuotas quotas)
        {
            this.quotas = new XmlDictionaryReaderQuotas();
            quotas.CopyTo(this.quotas);

            this.bufferedReadReaderQuotas = EncoderHelpers.GetBufferedReadQuotas(this.quotas);

            this.maxSentMessageSizeExceededResourceString = SR.MaxSentMessageSizeExceeded("{0}");
            this.maxReceivedMessageSizeExceededResourceString = SR.MaxReceivedMessageSizeExceeded("{0}");
        }

        void IWebMessageEncoderHelper.EnableBodyReaderMoveToContent()
        {
            this.moveBodyReaderToContent = true;
        }

        public override string ContentType
        {
            get { return null; }
        }

        public override string MediaType
        {
            get { return null; }
        }

        public override MessageVersion MessageVersion
        {
            get { return MessageVersion.None; }
        }

        public override bool IsContentTypeSupported(string contentType)
        {
            return true;
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }

            if (TD.ByteStreamMessageDecodingStartIsEnabled())
            {
                TD.ByteStreamMessageDecodingStart();
            }

            Message message = ByteStreamMessage.CreateMessage(stream, this.quotas, this.moveBodyReaderToContent);
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

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            if (buffer.Array == null)
            {
                throw FxTrace.Exception.ArgumentNull("buffer.Array");
            }

            if (bufferManager == null)
            {
                throw FxTrace.Exception.ArgumentNull("bufferManager");
            }

            if (TD.ByteStreamMessageDecodingStartIsEnabled())
            {
                TD.ByteStreamMessageDecodingStart();
            }

            ByteStreamBufferedMessageData messageData = new ByteStreamBufferedMessageData(buffer, bufferManager);

            Message message = ByteStreamMessage.CreateMessage(messageData, this.bufferedReadReaderQuotas, this.moveBodyReaderToContent);
            message.Properties.Encoder = this;

            if (SMTD.MessageReadByEncoderIsEnabled())
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

        public override void WriteMessage(Message message, Stream stream)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }

            ThrowIfMismatchedMessageVersion(message);

            EventTraceActivity eventTraceActivity = null;
            if (TD.ByteStreamMessageEncodingStartIsEnabled())
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                TD.ByteStreamMessageEncodingStart(eventTraceActivity);
            }

            message.Properties.Encoder = this;

            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
            }

            using (XmlWriter writer = new XmlByteStreamWriter(stream, false))
            {
                message.WriteMessage(writer);
                writer.Flush();
            }

            if (SMTD.StreamedMessageWrittenByEncoderIsEnabled())
            {
                SMTD.StreamedMessageWrittenByEncoder(eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(message));
            }
        }

        public override IAsyncResult BeginWriteMessage(Message message, Stream stream, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }

            ThrowIfMismatchedMessageVersion(message);
            message.Properties.Encoder = this;

            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
            }

            return new WriteMessageAsyncResult(message, stream, callback, state);
        }

        public override void EndWriteMessage(IAsyncResult result)
        {
            WriteMessageAsyncResult.End(result);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }
            if (bufferManager == null)
            {
                throw FxTrace.Exception.ArgumentNull("bufferManager");
            }
            if (maxMessageSize < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("maxMessageSize", maxMessageSize, SR.ArgumentOutOfMinRange(0));
            }
            if (messageOffset < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("messageOffset", messageOffset, SR.ArgumentOutOfMinRange(0));
            }

            EventTraceActivity eventTraceActivity = null;
            if (TD.ByteStreamMessageEncodingStartIsEnabled())
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                TD.ByteStreamMessageEncodingStart(eventTraceActivity);
            }

            ThrowIfMismatchedMessageVersion(message);
            message.Properties.Encoder = this;

            ArraySegment<byte> messageBuffer;
            int size;

            using (BufferManagerOutputStream stream = new BufferManagerOutputStream(maxSentMessageSizeExceededResourceString, 0, maxMessageSize, bufferManager))
            {
                stream.Skip(messageOffset);
                using (XmlWriter writer = new XmlByteStreamWriter(stream, true))
                {
                    message.WriteMessage(writer);
                    writer.Flush();
                    byte[] bytes = stream.ToArray(out size);
                    messageBuffer = new ArraySegment<byte>(bytes, messageOffset, size - messageOffset);
                }
            }

            if (SMTD.MessageWrittenByEncoderIsEnabled())
            {
                SMTD.MessageWrittenByEncoder(
                    eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(message),
                    messageBuffer.Count,
                    this);
            }

            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                // DevDiv#486728
                // Don't pass in a buffer manager to avoid returning 'messageBuffer" to the bufferManager twice.
                ByteStreamBufferedMessageData messageData = new ByteStreamBufferedMessageData(messageBuffer, null);
                using (XmlReader reader = new XmlBufferedByteStreamReader(messageData, this.quotas))
                {
                    MessageLogger.LogMessage(ref message, reader, MessageLoggingSource.TransportSend);
                }
            }

            return messageBuffer;
        }

        public override string ToString()
        {
            return ByteStreamMessageUtility.EncoderName;
        }

        public Stream GetResponseMessageStream(Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }

            ThrowIfMismatchedMessageVersion(message);

            if (!ByteStreamMessage.IsInternalByteStreamMessage(message))
            {
                return null;
            }

            return message.GetBody<Stream>();
        }

        string ITraceSourceStringProvider.GetSourceString()
        {
            // Other MessageEncoders use base.GetTraceSourceString but that would require a public api change in MessageEncoder
            // as ByteStreamMessageEncoder is in a different assemly. The same logic is reimplemented here.
            if (this.traceSourceString == null)
            {
                this.traceSourceString = DiagnosticTraceBase.CreateDefaultSourceString(this);
            }

            return this.traceSourceString;
        }

        class WriteMessageAsyncResult : AsyncResult
        {
            Message message;
            Stream stream;
            static Action<IAsyncResult, Exception> onCleanup;
            XmlByteStreamWriter writer;
            EventTraceActivity eventTraceActivity;

            public WriteMessageAsyncResult(Message message, Stream stream, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.message = message;
                this.stream = stream;
                this.writer = new XmlByteStreamWriter(stream, false);
                if (onCleanup == null)
                {
                    onCleanup = new Action<IAsyncResult, Exception>(Cleanup);
                }
                this.OnCompleting += onCleanup;
                Exception completionException = null;
                bool completeSelf = false;

                this.eventTraceActivity = null;
                if (TD.ByteStreamMessageEncodingStartIsEnabled())
                {
                    this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                    TD.ByteStreamMessageEncodingStart(this.eventTraceActivity);
                }

                try
                {
                    IAsyncResult result = message.BeginWriteMessage(writer, PrepareAsyncCompletion(HandleWriteMessage), this);
                    completeSelf = SyncContinue(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = ex;
                }

                if (completeSelf)
                {
                    this.Complete(true, completionException);
                }
            }

            static bool HandleWriteMessage(IAsyncResult result)
            {
                WriteMessageAsyncResult thisPtr = (WriteMessageAsyncResult)result.AsyncState;

                thisPtr.message.EndWriteMessage(result);
                thisPtr.writer.Flush();

                if (SMTD.MessageWrittenAsynchronouslyByEncoderIsEnabled())
                {
                    SMTD.MessageWrittenAsynchronouslyByEncoder(
                        thisPtr.eventTraceActivity ?? EventTraceActivityHelper.TryExtractActivity(thisPtr.message));
                }

                return true;
            }

            static void Cleanup(IAsyncResult result, Exception ex)
            {
                WriteMessageAsyncResult thisPtr = (WriteMessageAsyncResult)result;
                bool success = false;
                try
                {
                    thisPtr.writer.Close();
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        FxTrace.Exception.TraceHandledException(ex, TraceEventType.Information);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteMessageAsyncResult>(result);
            }
        }

    }
}
