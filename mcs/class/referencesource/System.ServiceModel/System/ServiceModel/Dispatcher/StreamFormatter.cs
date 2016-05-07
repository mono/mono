//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    class StreamFormatter
    {
        string wrapperName;
        string wrapperNS;
        string partName;
        string partNS;
        int streamIndex;
        bool isRequest;
        string operationName;
        const int returnValueIndex = -1;

        internal static StreamFormatter Create(MessageDescription messageDescription, string operationName, bool isRequest)
        {
            MessagePartDescription streamPart = ValidateAndGetStreamPart(messageDescription, isRequest, operationName);
            if (streamPart == null)
                return null;
            return new StreamFormatter(messageDescription, streamPart, operationName, isRequest);
        }

        StreamFormatter(MessageDescription messageDescription, MessagePartDescription streamPart, string operationName, bool isRequest)
        {
            if ((object)streamPart == (object)messageDescription.Body.ReturnValue)
                this.streamIndex = returnValueIndex;
            else
                this.streamIndex = streamPart.Index;
            wrapperName = messageDescription.Body.WrapperName;
            wrapperNS = messageDescription.Body.WrapperNamespace;
            partName = streamPart.Name;
            partNS = streamPart.Namespace;
            this.isRequest = isRequest;
            this.operationName = operationName;
        }

        internal void Serialize(XmlDictionaryWriter writer, object[] parameters, object returnValue)
        {
            Stream streamValue = GetStreamAndWriteStartWrapperIfNecessary(writer, parameters, returnValue);
            writer.WriteValue(new OperationStreamProvider(streamValue));
            WriteEndWrapperIfNecessary(writer);
        }

        Stream GetStreamAndWriteStartWrapperIfNecessary(XmlDictionaryWriter writer, object[] parameters, object returnValue)
        {
            Stream streamValue = GetStreamValue(parameters, returnValue);
            if (streamValue == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(partName);
            if (WrapperName != null)
                writer.WriteStartElement(WrapperName, WrapperNamespace);
            writer.WriteStartElement(PartName, PartNamespace);
            return streamValue;
        }

        void WriteEndWrapperIfNecessary(XmlDictionaryWriter writer)
        {
            writer.WriteEndElement();
            if (wrapperName != null)
                writer.WriteEndElement();
        }

        internal IAsyncResult BeginSerialize(XmlDictionaryWriter writer, object[] parameters, object returnValue, AsyncCallback callback, object state)
        {
            return new SerializeAsyncResult(this, writer, parameters, returnValue, callback, state);
        }

        public void EndSerialize(IAsyncResult result)
        {
            SerializeAsyncResult.End(result);
        }

        class SerializeAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndSerialize = new AsyncCompletion(HandleEndSerialize);

            StreamFormatter streamFormatter;
            XmlDictionaryWriter writer;

            internal SerializeAsyncResult(StreamFormatter streamFormatter, XmlDictionaryWriter writer, object[] parameters, object returnValue,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.streamFormatter = streamFormatter;
                this.writer = writer;
                bool completeSelf = true;

                Stream streamValue = streamFormatter.GetStreamAndWriteStartWrapperIfNecessary(writer, parameters, returnValue);
                IAsyncResult result = writer.WriteValueAsync(new OperationStreamProvider(streamValue)).AsAsyncResult(PrepareAsyncCompletion(handleEndSerialize), this);
                completeSelf = SyncContinue(result);

                // Note:  The current task implementation hard codes the "IAsyncResult.CompletedSynchronously" property to false, so this fast path will never
                // be hit, and we will always hop threads.  CSDMain #210220
                if (completeSelf)
                {
                    Complete(true);
                }
            }

            static bool HandleEndSerialize(IAsyncResult result)
            {
                SerializeAsyncResult thisPtr = (SerializeAsyncResult)result.AsyncState;
                thisPtr.streamFormatter.WriteEndWrapperIfNecessary(thisPtr.writer);
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SerializeAsyncResult>(result);
            }
        }

        internal void Deserialize(object[] parameters, ref object retVal, Message message)
        {
            SetStreamValue(parameters, ref retVal, new MessageBodyStream(message, WrapperName, WrapperNamespace, PartName, PartNamespace, isRequest));
        }

        internal string WrapperName
        {
            get { return wrapperName; }
            set { wrapperName = value; }
        }

        internal string WrapperNamespace
        {
            get { return wrapperNS; }
            set { wrapperNS = value; }
        }

        internal string PartName
        {
            get { return partName; }
        }

        internal string PartNamespace
        {
            get { return partNS; }
        }


        Stream GetStreamValue(object[] parameters, object returnValue)
        {
            if (streamIndex == returnValueIndex)
                return (Stream)returnValue;
            return (Stream)parameters[streamIndex];
        }

        void SetStreamValue(object[] parameters, ref object returnValue, Stream streamValue)
        {
            if (streamIndex == returnValueIndex)
                returnValue = streamValue;
            else
                parameters[streamIndex] = streamValue;
        }

        static MessagePartDescription ValidateAndGetStreamPart(MessageDescription messageDescription, bool isRequest, string operationName)
        {
            MessagePartDescription part = GetStreamPart(messageDescription);
            if (part != null)
                return part;
            if (HasStream(messageDescription))
            {
                if (messageDescription.IsTypedMessage)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidStreamInTypedMessage, messageDescription.MessageName)));
                else if (isRequest)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidStreamInRequest, operationName)));
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidStreamInResponse, operationName)));
            }
            return null;
        }

        private static bool HasStream(MessageDescription messageDescription)
        {
            if (messageDescription.Body.ReturnValue != null && messageDescription.Body.ReturnValue.Type == typeof(Stream))
                return true;
            foreach (MessagePartDescription part in messageDescription.Body.Parts)
            {
                if (part.Type == typeof(Stream))
                    return true;
            }
            return false;
        }

        static MessagePartDescription GetStreamPart(MessageDescription messageDescription)
        {
            if (OperationFormatter.IsValidReturnValue(messageDescription.Body.ReturnValue))
            {
                if (messageDescription.Body.Parts.Count == 0)
                    if (messageDescription.Body.ReturnValue.Type == typeof(Stream))
                        return messageDescription.Body.ReturnValue;
            }
            else
            {
                if (messageDescription.Body.Parts.Count == 1)
                    if (messageDescription.Body.Parts[0].Type == typeof(Stream))
                        return messageDescription.Body.Parts[0];
            }
            return null;
        }

        internal static bool IsStream(MessageDescription messageDescription)
        {
            return GetStreamPart(messageDescription) != null;
        }

        internal class MessageBodyStream : Stream
        {
            Message message;
            XmlDictionaryReader reader;
            long position;
            string wrapperName, wrapperNs;
            string elementName, elementNs;
            bool isRequest;
            internal MessageBodyStream(Message message, string wrapperName, string wrapperNs, string elementName, string elementNs, bool isRequest)
            {
                this.message = message;
                this.position = 0;
                this.wrapperName = wrapperName;
                this.wrapperNs = wrapperNs;
                this.elementName = elementName;
                this.elementNs = elementNs;
                this.isRequest = isRequest;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                EnsureStreamIsOpen();
                if (buffer == null)
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("buffer"), this.message);
                if (offset < 0)
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset,
                                                    SR.GetString(SR.ValueMustBeNonNegative)), this.message);
                if (count < 0)
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", count,
                                                    SR.GetString(SR.ValueMustBeNonNegative)), this.message);
                if (buffer.Length - offset < count)
                    throw TraceUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxInvalidStreamOffsetLength, offset + count)), this.message);

                try
                {

                    if (reader == null)
                    {
                        reader = message.GetReaderAtBodyContents();
                        if (wrapperName != null)
                        {
                            reader.MoveToContent();
                            reader.ReadStartElement(wrapperName, wrapperNs);
                        }
                        reader.MoveToContent();
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            return 0;
                        }

                        reader.ReadStartElement(elementName, elementNs);
                    }
                    if (reader.MoveToContent() != XmlNodeType.Text)
                    {
                        Exhaust(reader);
                        return 0;
                    }
                    int bytesRead = reader.ReadContentAsBase64(buffer, offset, count);
                    position += bytesRead;
                    if (bytesRead == 0)
                    {
                        Exhaust(reader);
                    }
                    return bytesRead;
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new IOException(SR.GetString(SR.SFxStreamIOException), ex));
                }
            }

            private void EnsureStreamIsOpen()
            {
                if (message.State == MessageState.Closed)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(SR.GetString(
                        isRequest ? SR.SFxStreamRequestMessageClosed : SR.SFxStreamResponseMessageClosed)));
            }

            static void Exhaust(XmlDictionaryReader reader)
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        // drain
                    }
                }
            }

            public override long Position
            {
                get
                {
                    EnsureStreamIsOpen();
                    return position;
                }
                set { throw TraceUtility.ThrowHelperError(new NotSupportedException(), message); }
            }

            public override void Close()
            {
                message.Close();
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                base.Close();
            }
            public override bool CanRead { get { return message.State != MessageState.Closed; } }
            public override bool CanSeek { get { return false; } }
            public override bool CanWrite { get { return false; } }
            public override long Length
            {
                get
                {
#pragma warning suppress 56503 // [....], not a seekable stream, it is ok to throw NotSupported in this case
                    throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message);
                }
            }
            public override void Flush() { throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message); }
            public override long Seek(long offset, SeekOrigin origin) { throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message); }
            public override void SetLength(long value) { throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message); }
            public override void Write(byte[] buffer, int offset, int count) { throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message); }
        }

        class OperationStreamProvider : IStreamProvider
        {
            Stream stream;

            internal OperationStreamProvider(Stream stream)
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
    }



}
