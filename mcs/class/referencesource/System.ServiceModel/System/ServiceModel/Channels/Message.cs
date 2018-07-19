//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Xml;

    public abstract class Message : IDisposable
    {
        MessageState state;
        SeekableMessageNavigator messageNavigator;
        internal const int InitialBufferSize = 1024;

        public abstract MessageHeaders Headers { get; } // must never return null

        protected bool IsDisposed
        {
            get { return state == MessageState.Closed; }
        }

        public virtual bool IsFault
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);

                return false;
            }
        }

        public virtual bool IsEmpty
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);

                return false;
            }
        }

        public abstract MessageProperties Properties { get; }

        public abstract MessageVersion Version { get; } // must never return null

        internal virtual void SetProperty(string name, object value)
        {
            MessageProperties prop = Properties;

            if (prop != null)
            {
                prop[name] = value;
            }
        }

        internal virtual bool GetProperty(string name, out object result)
        {
            MessageProperties prop = Properties;

            if (prop != null)
            {
                return prop.TryGetValue(name, out result);
            }

            result = null;
            return false;
        }

        internal virtual RecycledMessageState RecycledMessageState
        {
            get { return null; }
        }

        public MessageState State
        {
            get { return state; }
        }

        internal void BodyToString(XmlDictionaryWriter writer)
        {
            OnBodyToString(writer);
        }

        public void Close()
        {
            if (state != MessageState.Closed)
            {
                state = MessageState.Closed;
                OnClose();
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.MessageClosed,
                        SR.GetString(SR.TraceCodeMessageClosed), this);
                }
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.MessageClosedAgain,
                        SR.GetString(SR.TraceCodeMessageClosedAgain), this);
                }
            }
        }

        public MessageBuffer CreateBufferedCopy(int maxBufferSize)
        {
            if (maxBufferSize < 0)
                throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize,
                                                    SR.GetString(SR.ValueMustBeNonNegative)), this);
            switch (state)
            {
                case MessageState.Created:
                    state = MessageState.Copied;
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.MessageCopied,
                            SR.GetString(SR.TraceCodeMessageCopied), this, this);
                    }
                    break;
                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenCopied)), this);
                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenRead)), this);
                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenWritten)), this);
                default:
                    Fx.Assert(SR.GetString(SR.InvalidMessageState));
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidMessageState)), this);
            }
            return OnCreateBufferedCopy(maxBufferSize);
        }

        static Type GetObjectType(object value)
        {
            return (value == null) ? typeof(object) : value.GetType();
        }

        static public Message CreateMessage(MessageVersion version, string action, object body)
        {
            return CreateMessage(version, action, body, DataContractSerializerDefaults.CreateSerializer(GetObjectType(body), int.MaxValue/*maxItems*/));
        }

        static public Message CreateMessage(MessageVersion version, string action, object body, XmlObjectSerializer serializer)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            if (serializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            return new BodyWriterMessage(version, action, new XmlObjectSerializerBodyWriter(body, serializer));
        }

        static public Message CreateMessage(MessageVersion version, string action, XmlReader body)
        {
            return CreateMessage(version, action, XmlDictionaryReader.CreateDictionaryReader(body));
        }

        static public Message CreateMessage(MessageVersion version, string action, XmlDictionaryReader body)
        {
            if (body == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("body");
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");

            return CreateMessage(version, action, new XmlReaderBodyWriter(body, version.Envelope));
        }

        static public Message CreateMessage(MessageVersion version, string action, BodyWriter body)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            if (body == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("body"));
            return new BodyWriterMessage(version, action, body);
        }

        static internal Message CreateMessage(MessageVersion version, ActionHeader actionHeader, BodyWriter body)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            if (body == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("body"));
            return new BodyWriterMessage(version, actionHeader, body);
        }

        static public Message CreateMessage(MessageVersion version, string action)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            return new BodyWriterMessage(version, action, EmptyBodyWriter.Value);
        }

        static internal Message CreateMessage(MessageVersion version, ActionHeader actionHeader)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            return new BodyWriterMessage(version, actionHeader, EmptyBodyWriter.Value);
        }

        static public Message CreateMessage(XmlReader envelopeReader, int maxSizeOfHeaders, MessageVersion version)
        {
            return CreateMessage(XmlDictionaryReader.CreateDictionaryReader(envelopeReader), maxSizeOfHeaders, version);
        }

        static public Message CreateMessage(XmlDictionaryReader envelopeReader, int maxSizeOfHeaders, MessageVersion version)
        {
            if (envelopeReader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("envelopeReader"));
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            Message message = new StreamedMessage(envelopeReader, maxSizeOfHeaders, version);
            return message;
        }

        static public Message CreateMessage(MessageVersion version, FaultCode faultCode, string reason, string action)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            if (faultCode == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("faultCode"));
            if (reason == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reason"));

            return CreateMessage(version, MessageFault.CreateFault(faultCode, reason), action);
        }

        static public Message CreateMessage(MessageVersion version, FaultCode faultCode, string reason, object detail, string action)
        {
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            if (faultCode == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("faultCode"));
            if (reason == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reason"));

            return CreateMessage(version, MessageFault.CreateFault(faultCode, new FaultReason(reason), detail), action);
        }

        static public Message CreateMessage(MessageVersion version, MessageFault fault, string action)
        {
            if (fault == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("fault"));
            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            return new BodyWriterMessage(version, action, new FaultBodyWriter(fault, version.Envelope));
        }

        internal Exception CreateMessageDisposedException()
        {
            return new ObjectDisposedException("", SR.GetString(SR.MessageClosed));
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        public T GetBody<T>()
        {
            XmlDictionaryReader reader = GetReaderAtBodyContents();   // This call will change the message state to Read.
            return OnGetBody<T>(reader);
        }

        protected virtual T OnGetBody<T>(XmlDictionaryReader reader)
        {
            return this.GetBodyCore<T>(reader, DataContractSerializerDefaults.CreateSerializer(typeof(T), int.MaxValue/*maxItems*/));
        }

        public T GetBody<T>(XmlObjectSerializer serializer)
        {
            if (serializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            return this.GetBodyCore<T>(GetReaderAtBodyContents(), serializer);
        }

        T GetBodyCore<T>(XmlDictionaryReader reader, XmlObjectSerializer serializer)
        {
            T value;
            using (reader)
            {
                value = (T)serializer.ReadObject(reader);
                this.ReadFromBodyContentsToEnd(reader);
            }
            return value;
        }

        internal virtual XmlDictionaryReader GetReaderAtHeader()
        {
            XmlBuffer buffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            WriteStartEnvelope(writer);
            MessageHeaders headers = this.Headers;
            for (int i = 0; i < headers.Count; i++)
                headers.WriteHeader(i, writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
            buffer.CloseSection();
            buffer.Close();
            XmlDictionaryReader reader = buffer.GetReader(0);
            reader.ReadStartElement();
            reader.MoveToStartElement();
            return reader;
        }

        public XmlDictionaryReader GetReaderAtBodyContents()
        {
            EnsureReadMessageState();
            if (IsEmpty)
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageIsEmpty)), this);
            return OnGetReaderAtBodyContents();
        }

        internal void EnsureReadMessageState()
        {
            switch (state)
            {
                case MessageState.Created:
                    state = MessageState.Read;
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.MessageRead, SR.GetString(SR.TraceCodeMessageRead), this);
                    }
                    break;
                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenCopied)), this);
                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenRead)), this);
                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenWritten)), this);
                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                default:
                    Fx.Assert(SR.GetString(SR.InvalidMessageState));
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidMessageState)), this);
            }
        }

        internal SeekableMessageNavigator GetNavigator(bool navigateBody, int maxNodes)
        {
            if (IsDisposed)
                throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
            if (null == this.messageNavigator)
            {
                this.messageNavigator = new SeekableMessageNavigator(this, maxNodes, XmlSpace.Default, navigateBody, false);
            }
            else
            {
                this.messageNavigator.ForkNodeCount(maxNodes);
            }

            return this.messageNavigator;
        }

        internal void InitializeReply(Message request)
        {
            UniqueId requestMessageID = request.Headers.MessageId;
            if (requestMessageID == null)
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.RequestMessageDoesNotHaveAMessageID)), request);
            Headers.RelatesTo = requestMessageID;
        }

        static internal bool IsFaultStartElement(XmlDictionaryReader reader, EnvelopeVersion version)
        {
            return reader.IsStartElement(XD.MessageDictionary.Fault, version.DictionaryNamespace);
        }

        protected virtual void OnBodyToString(XmlDictionaryWriter writer)
        {
            writer.WriteString(SR.GetString(SR.MessageBodyIsUnknown));
        }

        protected virtual MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            return OnCreateBufferedCopy(maxBufferSize, XmlDictionaryReaderQuotas.Max);
        }

        internal MessageBuffer OnCreateBufferedCopy(int maxBufferSize, XmlDictionaryReaderQuotas quotas)
        {
            XmlBuffer msgBuffer = new XmlBuffer(maxBufferSize);
            XmlDictionaryWriter writer = msgBuffer.OpenSection(quotas);
            OnWriteMessage(writer);
            msgBuffer.CloseSection();
            msgBuffer.Close();
            return new DefaultMessageBuffer(this, msgBuffer);
        }

        protected virtual void OnClose()
        {
        }

        protected virtual XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            XmlBuffer bodyBuffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = bodyBuffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                OnWriteStartEnvelope(writer);
                OnWriteStartBody(writer);
            }
            OnWriteBodyContents(writer);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            bodyBuffer.CloseSection();
            bodyBuffer.Close();
            XmlDictionaryReader reader = bodyBuffer.GetReader(0);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                reader.ReadStartElement();
                reader.ReadStartElement();
            }
            reader.MoveToContent();
            return reader;
        }

        protected virtual void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            MessageDictionary messageDictionary = XD.MessageDictionary;
            writer.WriteStartElement(messageDictionary.Prefix.Value, messageDictionary.Body, Version.Envelope.DictionaryNamespace);
        }

        public void WriteBodyContents(XmlDictionaryWriter writer)
        {
            EnsureWriteMessageState(writer);
            OnWriteBodyContents(writer);
        }

        public IAsyncResult BeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            EnsureWriteMessageState(writer);
            return this.OnBeginWriteBodyContents(writer, callback, state);
        }

        public void EndWriteBodyContents(IAsyncResult result)
        {
            this.OnEndWriteBodyContents(result);
        }

        protected abstract void OnWriteBodyContents(XmlDictionaryWriter writer);

        protected virtual IAsyncResult OnBeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            return new OnWriteBodyContentsAsyncResult(writer, this, callback, state);
        }

        protected virtual void OnEndWriteBodyContents(IAsyncResult result)
        {
            OnWriteBodyContentsAsyncResult.End(result);
        }

        public void WriteStartEnvelope(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("writer"), this);

            OnWriteStartEnvelope(writer);
        }

        protected virtual void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelopeVersion = Version.Envelope;
            if (envelopeVersion != EnvelopeVersion.None)
            {
                MessageDictionary messageDictionary = XD.MessageDictionary;
                writer.WriteStartElement(messageDictionary.Prefix.Value, messageDictionary.Envelope, envelopeVersion.DictionaryNamespace);
                WriteSharedHeaderPrefixes(writer);
            }
        }

        protected virtual void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelopeVersion = Version.Envelope;
            if (envelopeVersion != EnvelopeVersion.None)
            {
                MessageDictionary messageDictionary = XD.MessageDictionary;
                writer.WriteStartElement(messageDictionary.Prefix.Value, messageDictionary.Header, envelopeVersion.DictionaryNamespace);
            }
        }

        public override string ToString()
        {
            if (IsDisposed)
            {
                return base.ToString();
            }

            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            EncodingFallbackAwareXmlTextWriter textWriter = new EncodingFallbackAwareXmlTextWriter(stringWriter);
            textWriter.Formatting = Formatting.Indented;
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(textWriter);
            try
            {
                ToString(writer);
                writer.Flush();
                return stringWriter.ToString();
            }
            catch (XmlException e)
            {
                return SR.GetString(SR.MessageBodyToStringError, e.GetType().ToString(), e.Message);
            }
        }

        internal void ToString(XmlDictionaryWriter writer)
        {
            if (IsDisposed)
            {
                throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
            }

            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                WriteStartEnvelope(writer);
                WriteStartHeaders(writer);
                MessageHeaders headers = this.Headers;
                for (int i = 0; i < headers.Count; i++)
                {
                    headers.WriteHeader(i, writer);
                }

                writer.WriteEndElement();
                MessageDictionary messageDictionary = XD.MessageDictionary;
                WriteStartBody(writer);
            }

            BodyToString(writer);

            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public string GetBodyAttribute(string localName, string ns)
        {
            if (localName == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("localName"), this);
            if (ns == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("ns"), this);
            switch (state)
            {
                case MessageState.Created:
                    break;
                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenCopied)), this);
                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenRead)), this);
                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenWritten)), this);
                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                default:
                    Fx.Assert(SR.GetString(SR.InvalidMessageState));
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidMessageState)), this);
            }
            return OnGetBodyAttribute(localName, ns);
        }

        protected virtual string OnGetBodyAttribute(string localName, string ns)
        {
            return null;
        }

        internal void ReadFromBodyContentsToEnd(XmlDictionaryReader reader)
        {
            Message.ReadFromBodyContentsToEnd(reader, this.Version.Envelope);
        }

        static void ReadFromBodyContentsToEnd(XmlDictionaryReader reader, EnvelopeVersion envelopeVersion)
        {
            if (envelopeVersion != EnvelopeVersion.None)
            {
                reader.ReadEndElement(); // </Body>
                reader.ReadEndElement(); // </Envelope>
            }
            reader.MoveToContent();
        }

        internal static bool ReadStartBody(XmlDictionaryReader reader, EnvelopeVersion envelopeVersion, out bool isFault, out bool isEmpty)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                isEmpty = true;
                isFault = false;
                reader.ReadEndElement();
                return false;
            }
            else
            {
                reader.Read();
                if (reader.NodeType != XmlNodeType.Element)
                    reader.MoveToContent();
                if (reader.NodeType == XmlNodeType.Element)
                {
                    isFault = IsFaultStartElement(reader, envelopeVersion);
                    isEmpty = false;
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    isEmpty = true;
                    isFault = false;
                    Message.ReadFromBodyContentsToEnd(reader, envelopeVersion);
                    return false;
                }
                else
                {
                    isEmpty = false;
                    isFault = false;
                }

                return true;
            }
        }

        public void WriteBody(XmlWriter writer)
        {
            WriteBody(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteBody(XmlDictionaryWriter writer)
        {
            WriteStartBody(writer);
            WriteBodyContents(writer);
            writer.WriteEndElement();
        }

        public void WriteStartBody(XmlWriter writer)
        {
            WriteStartBody(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteStartBody(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("writer"), this);
            OnWriteStartBody(writer);
        }

        internal void WriteStartHeaders(XmlDictionaryWriter writer)
        {
            OnWriteStartHeaders(writer);
        }

        public void WriteMessage(XmlWriter writer)
        {
            WriteMessage(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteMessage(XmlDictionaryWriter writer)
        {
            EnsureWriteMessageState(writer);
            OnWriteMessage(writer);
        }

        void EnsureWriteMessageState(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("writer"), this);
            switch (state)
            {
                case MessageState.Created:
                    state = MessageState.Written;
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.MessageWritten, SR.GetString(SR.TraceCodeMessageWritten), this);
                    }
                    break;
                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenCopied)), this);
                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenRead)), this);
                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageHasBeenWritten)), this);
                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                default:
                    Fx.Assert(SR.GetString(SR.InvalidMessageState));
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidMessageState)), this);
            }
        }

        public IAsyncResult BeginWriteMessage(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            EnsureWriteMessageState(writer);
            return OnBeginWriteMessage(writer, callback, state);
        }

        public void EndWriteMessage(IAsyncResult result)
        {
            OnEndWriteMessage(result);
        }

        protected virtual void OnWriteMessage(XmlDictionaryWriter writer)
        {
            WriteMessagePreamble(writer);
            OnWriteBodyContents(writer);
            WriteMessagePostamble(writer);
        }

        internal void WriteMessagePreamble(XmlDictionaryWriter writer)
        {
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                OnWriteStartEnvelope(writer);

                MessageHeaders headers = this.Headers;
                int headersCount = headers.Count;
                if (headersCount > 0)
                {
                    OnWriteStartHeaders(writer);
                    for (int i = 0; i < headersCount; i++)
                    {
                        headers.WriteHeader(i, writer);
                    }
                    writer.WriteEndElement();
                }

                OnWriteStartBody(writer);
            }
        }

        internal void WriteMessagePostamble(XmlDictionaryWriter writer)
        {
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        protected virtual IAsyncResult OnBeginWriteMessage(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            return new OnWriteMessageAsyncResult(writer, this, callback, state);
        }

        protected virtual void OnEndWriteMessage(IAsyncResult result)
        {
            OnWriteMessageAsyncResult.End(result);
        }

        void WriteSharedHeaderPrefixes(XmlDictionaryWriter writer)
        {
            MessageHeaders headers = Headers;
            int count = headers.Count;
            int prefixesWritten = 0;
            for (int i = 0; i < count; i++)
            {
                if (this.Version.Addressing == AddressingVersion.None && headers[i].Namespace == AddressingVersion.None.Namespace)
                {
                    continue;
                }

                IMessageHeaderWithSharedNamespace headerWithSharedNamespace = headers[i] as IMessageHeaderWithSharedNamespace;
                if (headerWithSharedNamespace != null)
                {
                    XmlDictionaryString prefix = headerWithSharedNamespace.SharedPrefix;
                    string prefixString = prefix.Value;
                    if (!((prefixString.Length == 1)))
                    {
                        Fx.Assert("Message.WriteSharedHeaderPrefixes: (prefixString.Length == 1) -- IMessageHeaderWithSharedNamespace must use a single lowercase letter prefix.");
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "IMessageHeaderWithSharedNamespace must use a single lowercase letter prefix.")), this);
                    }

                    int prefixIndex = prefixString[0] - 'a';
                    if (!((prefixIndex >= 0 && prefixIndex < 26)))
                    {
                        Fx.Assert("Message.WriteSharedHeaderPrefixes: (prefixIndex >= 0 && prefixIndex < 26) -- IMessageHeaderWithSharedNamespace must use a single lowercase letter prefix.");
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "IMessageHeaderWithSharedNamespace must use a single lowercase letter prefix.")), this);
                    }
                    int prefixBit = 1 << prefixIndex;
                    if ((prefixesWritten & prefixBit) == 0)
                    {
                        writer.WriteXmlnsAttribute(prefixString, headerWithSharedNamespace.SharedNamespace);
                        prefixesWritten |= prefixBit;
                    }
                }
            }
        }

        class OnWriteBodyContentsAsyncResult : ScheduleActionItemAsyncResult
        {
            Message message;
            XmlDictionaryWriter writer;

            public OnWriteBodyContentsAsyncResult(XmlDictionaryWriter writer, Message message, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(message != null, "message should never be null");

                this.message = message;
                this.writer = writer;

                Schedule();
            }

            protected override void OnDoWork()
            {
                this.message.OnWriteBodyContents(this.writer);
            }
        }

        class OnWriteMessageAsyncResult : ScheduleActionItemAsyncResult
        {
            Message message;
            XmlDictionaryWriter writer;

            public OnWriteMessageAsyncResult(XmlDictionaryWriter writer, Message message, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(message != null, "message should never be null");

                this.message = message;
                this.writer = writer;

                Schedule();
            }

            protected override void OnDoWork()
            {
                this.message.OnWriteMessage(this.writer);
            }
        }
    }

    class EmptyBodyWriter : BodyWriter
    {
        static EmptyBodyWriter value;

        EmptyBodyWriter()
            : base(true)
        {
        }

        public static EmptyBodyWriter Value
        {
            get
            {
                if (value == null)
                    value = new EmptyBodyWriter();
                return value;
            }
        }

        internal override bool IsEmpty
        {
            get { return true; }
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
        }
    }

    class FaultBodyWriter : BodyWriter
    {
        MessageFault fault;
        EnvelopeVersion version;

        public FaultBodyWriter(MessageFault fault, EnvelopeVersion version)
            : base(true)
        {
            this.fault = fault;
            this.version = version;
        }

        internal override bool IsFault
        {
            get { return true; }
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            fault.WriteTo(writer, version);
        }
    }

    class XmlObjectSerializerBodyWriter : BodyWriter
    {
        object body;
        XmlObjectSerializer serializer;

        public XmlObjectSerializerBodyWriter(object body, XmlObjectSerializer serializer)
            : base(true)
        {
            this.body = body;
            this.serializer = serializer;
        }

        object ThisLock
        {
            get { return this; }
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            lock (ThisLock)
            {
                serializer.WriteObject(writer, body);
            }
        }
    }

    class XmlReaderBodyWriter : BodyWriter
    {
        XmlDictionaryReader reader;
        bool isFault;

        public XmlReaderBodyWriter(XmlDictionaryReader reader, EnvelopeVersion version)
            : base(false)
        {
            this.reader = reader;
            if (reader.MoveToContent() != XmlNodeType.Element)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidReaderPositionOnCreateMessage), "reader"));

            this.isFault = Message.IsFaultStartElement(reader, version);
        }

        internal override bool IsFault
        {
            get
            {
                return this.isFault;
            }
        }

        protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
        {
            return OnCreateBufferedCopy(maxBufferSize, this.reader.Quotas);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            using (reader)
            {
                XmlNodeType type = reader.MoveToContent();
                while (!reader.EOF && type != XmlNodeType.EndElement)
                {
                    if (type != XmlNodeType.Element)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidReaderPositionOnCreateMessage), "reader"));
                    writer.WriteNode(reader, false);

                    type = reader.MoveToContent();
                }
            }
        }
    }

    class BodyWriterMessage : Message
    {
        MessageProperties properties;
        MessageHeaders headers;
        BodyWriter bodyWriter;

        BodyWriterMessage(BodyWriter bodyWriter)
        {
            this.bodyWriter = bodyWriter;
        }

        public BodyWriterMessage(MessageVersion version, string action, BodyWriter bodyWriter)
            : this(bodyWriter)
        {
            this.headers = new MessageHeaders(version);
            this.headers.Action = action;
        }

        public BodyWriterMessage(MessageVersion version, ActionHeader actionHeader, BodyWriter bodyWriter)
            : this(bodyWriter)
        {
            this.headers = new MessageHeaders(version);
            this.headers.SetActionHeader(actionHeader);
        }

        public BodyWriterMessage(MessageHeaders headers, KeyValuePair<string, object>[] properties, BodyWriter bodyWriter)
            : this(bodyWriter)
        {
            this.headers = new MessageHeaders(headers);
            this.properties = new MessageProperties(properties);
        }

        public override bool IsFault
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                return bodyWriter.IsFault;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                return bodyWriter.IsEmpty;
            }
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                return headers;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                if (properties == null)
                    properties = new MessageProperties();
                return properties;
            }
        }

        internal override void SetProperty(string name, object value)
        {
            MessageProperties prop = this.properties;

            if (prop != null)
            {
                prop[name] = value;
            }
        }

        internal override bool GetProperty(string name, out object result)
        {
            MessageProperties prop = this.properties;

            if (prop != null)
            {
                return prop.TryGetValue(name, out result);
            }

            result = null;
            return false;
        }


        public override MessageVersion Version
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                return headers.MessageVersion;
            }
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            BodyWriter bufferedBodyWriter;
            if (bodyWriter.IsBuffered)
            {
                bufferedBodyWriter = bodyWriter;
            }
            else
            {
                bufferedBodyWriter = bodyWriter.CreateBufferedCopy(maxBufferSize);
            }
            KeyValuePair<string, object>[] properties = new KeyValuePair<string, object>[Properties.Count];
            ((ICollection<KeyValuePair<string, object>>)Properties).CopyTo(properties, 0);
            return new BodyWriterMessageBuffer(headers, properties, bufferedBodyWriter);
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
                    throw;
                ex = e;
            }

            try
            {
                if (properties != null)
                    properties.Dispose();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
                if (ex == null)
                    ex = e;
            }

            if (ex != null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex);

            bodyWriter = null;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            bodyWriter.WriteBodyContents(writer);
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

        protected override IAsyncResult OnBeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            return bodyWriter.BeginWriteBodyContents(writer, callback, state);
        }

        protected override void OnEndWriteBodyContents(IAsyncResult result)
        {
            bodyWriter.EndWriteBodyContents(result);
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if (bodyWriter.IsBuffered)
            {
                bodyWriter.WriteBodyContents(writer);
            }
            else
            {
                writer.WriteString(SR.GetString(SR.MessageBodyIsStream));
            }
        }

        protected internal BodyWriter BodyWriter
        {
            get
            {
                return bodyWriter;
            }
        }

        class OnWriteMessageAsyncResult : AsyncResult
        {
            BodyWriterMessage message;
            XmlDictionaryWriter writer;

            public OnWriteMessageAsyncResult(XmlDictionaryWriter writer, BodyWriterMessage message, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.message = message;
                this.writer = writer;

                if (HandleWriteBodyContents(null))
                {
                    this.Complete(true);
                }
            }

            bool HandleWriteBodyContents(IAsyncResult result)
            {
                if (result == null)
                {
                    result = this.message.OnBeginWriteBodyContents(this.writer, PrepareAsyncCompletion(HandleWriteBodyContents), this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                this.message.OnEndWriteBodyContents(result);
                this.message.WriteMessagePostamble(this.writer);
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OnWriteMessageAsyncResult>(result);
            }
        }

    }

    abstract class ReceivedMessage : Message
    {
        bool isFault;
        bool isEmpty;

        public override bool IsEmpty
        {
            get { return isEmpty; }
        }

        public override bool IsFault
        {
            get { return isFault; }
        }

        protected static bool HasHeaderElement(XmlDictionaryReader reader, EnvelopeVersion envelopeVersion)
        {
            return reader.IsStartElement(XD.MessageDictionary.Header, envelopeVersion.DictionaryNamespace);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (!isEmpty)
            {
                using (XmlDictionaryReader bodyReader = OnGetReaderAtBodyContents())
                {
                    if (bodyReader.ReadState == ReadState.Error || bodyReader.ReadState == ReadState.Closed)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MessageBodyReaderInvalidReadState, bodyReader.ReadState.ToString())));

                    while (bodyReader.NodeType != XmlNodeType.EndElement && !bodyReader.EOF)
                    {
                        writer.WriteNode(bodyReader, false);
                    }

                    this.ReadFromBodyContentsToEnd(bodyReader);
                }
            }
        }

        protected bool ReadStartBody(XmlDictionaryReader reader)
        {
            return Message.ReadStartBody(reader, this.Version.Envelope, out this.isFault, out this.isEmpty);
        }

        protected static EnvelopeVersion ReadStartEnvelope(XmlDictionaryReader reader)
        {
            EnvelopeVersion envelopeVersion;

            if (reader.IsStartElement(XD.MessageDictionary.Envelope, XD.Message12Dictionary.Namespace))
                envelopeVersion = EnvelopeVersion.Soap12;
            else if (reader.IsStartElement(XD.MessageDictionary.Envelope, XD.Message11Dictionary.Namespace))
                envelopeVersion = EnvelopeVersion.Soap11;
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.MessageVersionUnknown)));
            if (reader.IsEmptyElement)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.MessageBodyMissing)));
            reader.Read();
            return envelopeVersion;
        }

        protected static void VerifyStartBody(XmlDictionaryReader reader, EnvelopeVersion version)
        {
            if (!reader.IsStartElement(XD.MessageDictionary.Body, version.DictionaryNamespace))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.MessageBodyMissing)));
        }
    }

    sealed class StreamedMessage : ReceivedMessage
    {
        MessageHeaders headers;
        XmlAttributeHolder[] envelopeAttributes;
        XmlAttributeHolder[] headerAttributes;
        XmlAttributeHolder[] bodyAttributes;
        string envelopePrefix;
        string headerPrefix;
        string bodyPrefix;
        MessageProperties properties;
        XmlDictionaryReader reader;
        XmlDictionaryReaderQuotas quotas;

        public StreamedMessage(XmlDictionaryReader reader, int maxSizeOfHeaders, MessageVersion desiredVersion)
        {
            properties = new MessageProperties();
            if (reader.NodeType != XmlNodeType.Element)
                reader.MoveToContent();

            if (desiredVersion.Envelope == EnvelopeVersion.None)
            {
                this.reader = reader;
                this.headerAttributes = XmlAttributeHolder.emptyArray;
                this.headers = new MessageHeaders(desiredVersion);
            }
            else
            {
                envelopeAttributes = XmlAttributeHolder.ReadAttributes(reader, ref maxSizeOfHeaders);
                envelopePrefix = reader.Prefix;
                EnvelopeVersion envelopeVersion = ReadStartEnvelope(reader);
                if (desiredVersion.Envelope != envelopeVersion)
                {
                    Exception versionMismatchException = new ArgumentException(SR.GetString(SR.EncoderEnvelopeVersionMismatch, envelopeVersion, desiredVersion.Envelope), "reader");
                    throw TraceUtility.ThrowHelperError(
                        new CommunicationException(versionMismatchException.Message, versionMismatchException),
                        this);
                }

                if (HasHeaderElement(reader, envelopeVersion))
                {
                    headerPrefix = reader.Prefix;
                    headerAttributes = XmlAttributeHolder.ReadAttributes(reader, ref maxSizeOfHeaders);
                    headers = new MessageHeaders(desiredVersion, reader, envelopeAttributes, headerAttributes, ref maxSizeOfHeaders);
                }
                else
                {
                    headerAttributes = XmlAttributeHolder.emptyArray;
                    headers = new MessageHeaders(desiredVersion);
                }

                if (reader.NodeType != XmlNodeType.Element)
                    reader.MoveToContent();
                bodyPrefix = reader.Prefix;
                VerifyStartBody(reader, envelopeVersion);
                bodyAttributes = XmlAttributeHolder.ReadAttributes(reader, ref maxSizeOfHeaders);
                if (ReadStartBody(reader))
                {
                    this.reader = reader;
                }
                else
                {
                    this.quotas = new XmlDictionaryReaderQuotas();
                    reader.Quotas.CopyTo(this.quotas);
                    reader.Close();
                }
            }
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                return headers;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                return headers.MessageVersion;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                return properties;
            }
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            writer.WriteString(SR.GetString(SR.MessageBodyIsStream));
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
                    throw;
                ex = e;
            }

            try
            {
                properties.Dispose();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
                if (ex == null)
                    ex = e;
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
                    throw;
                if (ex == null)
                    ex = e;
            }

            if (ex != null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex);
        }

        protected override XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            XmlDictionaryReader reader = this.reader;
            this.reader = null;
            return reader;
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            if (this.reader != null)
                return OnCreateBufferedCopy(maxBufferSize, this.reader.Quotas);
            return OnCreateBufferedCopy(maxBufferSize, this.quotas);
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(bodyPrefix, MessageStrings.Body, Version.Envelope.Namespace);
            XmlAttributeHolder.WriteAttributes(bodyAttributes, writer);
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelopeVersion = Version.Envelope;
            writer.WriteStartElement(envelopePrefix, MessageStrings.Envelope, envelopeVersion.Namespace);
            XmlAttributeHolder.WriteAttributes(envelopeAttributes, writer);
        }

        protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelopeVersion = Version.Envelope;
            writer.WriteStartElement(headerPrefix, MessageStrings.Header, envelopeVersion.Namespace);
            XmlAttributeHolder.WriteAttributes(headerAttributes, writer);
        }

        protected override string OnGetBodyAttribute(string localName, string ns)
        {
            return XmlAttributeHolder.GetAttribute(bodyAttributes, localName, ns);
        }
    }

    interface IBufferedMessageData
    {
        MessageEncoder MessageEncoder { get; }
        ArraySegment<byte> Buffer { get; }
        XmlDictionaryReaderQuotas Quotas { get; }
        void Close();
        void EnableMultipleUsers();
        XmlDictionaryReader GetMessageReader();
        void Open();
        void ReturnMessageState(RecycledMessageState messageState);
        RecycledMessageState TakeMessageState();
    }

    sealed class BufferedMessage : ReceivedMessage
    {
        MessageHeaders headers;
        MessageProperties properties;
        IBufferedMessageData messageData;
        RecycledMessageState recycledMessageState;
        XmlDictionaryReader reader;
        XmlAttributeHolder[] bodyAttributes;

        public BufferedMessage(IBufferedMessageData messageData, RecycledMessageState recycledMessageState)
            : this(messageData, recycledMessageState, null, false)
        {
        }

        public BufferedMessage(IBufferedMessageData messageData, RecycledMessageState recycledMessageState, bool[] understoodHeaders, bool understoodHeadersModified)
        {
            bool throwing = true;
            try
            {
                this.recycledMessageState = recycledMessageState;
                this.messageData = messageData;
                properties = recycledMessageState.TakeProperties();
                if (properties == null)
                    this.properties = new MessageProperties();
                XmlDictionaryReader reader = messageData.GetMessageReader();
                MessageVersion desiredVersion = messageData.MessageEncoder.MessageVersion;

                if (desiredVersion.Envelope == EnvelopeVersion.None)
                {
                    this.reader = reader;
                    this.headers = new MessageHeaders(desiredVersion);
                }
                else
                {
                    EnvelopeVersion envelopeVersion = ReadStartEnvelope(reader);
                    if (desiredVersion.Envelope != envelopeVersion)
                    {
                        Exception versionMismatchException = new ArgumentException(SR.GetString(SR.EncoderEnvelopeVersionMismatch, envelopeVersion, desiredVersion.Envelope), "reader");
                        throw TraceUtility.ThrowHelperError(
                            new CommunicationException(versionMismatchException.Message, versionMismatchException),
                            this);
                    }

                    if (HasHeaderElement(reader, envelopeVersion))
                    {
                        headers = recycledMessageState.TakeHeaders();
                        if (headers == null)
                        {
                            headers = new MessageHeaders(desiredVersion, reader, messageData, recycledMessageState, understoodHeaders, understoodHeadersModified);
                        }
                        else
                        {
                            headers.Init(desiredVersion, reader, messageData, recycledMessageState, understoodHeaders, understoodHeadersModified);
                        }
                    }
                    else
                    {
                        headers = new MessageHeaders(desiredVersion);
                    }

                    VerifyStartBody(reader, envelopeVersion);

                    int maxSizeOfAttributes = int.MaxValue;
                    bodyAttributes = XmlAttributeHolder.ReadAttributes(reader, ref maxSizeOfAttributes);
                    if (maxSizeOfAttributes < int.MaxValue - 4096)
                        bodyAttributes = null;
                    if (ReadStartBody(reader))
                    {
                        this.reader = reader;
                    }
                    else
                    {
                        reader.Close();
                    }
                }
                throwing = false;
            }
            finally
            {
                if (throwing && MessageLogger.LoggingEnabled)
                {
                    MessageLogger.LogMessage(messageData.Buffer, MessageLoggingSource.Malformed);
                }
            }
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                return headers;
            }
        }

        internal IBufferedMessageData MessageData
        {
            get
            {
                return messageData;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (IsDisposed)
#pragma warning suppress 56503 // Microsoft, Invalid State after dispose
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                return properties;
            }
        }

        internal override RecycledMessageState RecycledMessageState
        {
            get { return recycledMessageState; }
        }

        public override MessageVersion Version
        {
            get
            {
                return headers.MessageVersion;
            }
        }

        protected override XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            XmlDictionaryReader reader = this.reader;
            this.reader = null;
            return reader;
        }

        internal override XmlDictionaryReader GetReaderAtHeader()
        {
            if (!headers.ContainsOnlyBufferedMessageHeaders)
                return base.GetReaderAtHeader();
            XmlDictionaryReader reader = messageData.GetMessageReader();
            if (reader.NodeType != XmlNodeType.Element)
                reader.MoveToContent();
            reader.Read();
            if (HasHeaderElement(reader, headers.MessageVersion.Envelope))
                return reader;
            return base.GetReaderAtHeader();
        }

        public XmlDictionaryReader GetBufferedReaderAtBody()
        {
            XmlDictionaryReader reader = messageData.GetMessageReader();
            if (reader.NodeType != XmlNodeType.Element)
                reader.MoveToContent();
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                reader.Read();
                if (HasHeaderElement(reader, headers.MessageVersion.Envelope))
                    reader.Skip();
                if (reader.NodeType != XmlNodeType.Element)
                    reader.MoveToContent();
            }
            return reader;
        }

        public XmlDictionaryReader GetMessageReader()
        {
            return messageData.GetMessageReader();
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = GetBufferedReaderAtBody())
            {
                if (this.Version == MessageVersion.None)
                {
                    writer.WriteNode(reader, false);
                }
                else
                {
                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement();
                        while (reader.NodeType != XmlNodeType.EndElement)
                            writer.WriteNode(reader, false);
                    }
                }
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
                    throw;
                ex = e;
            }

            try
            {
                properties.Dispose();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
                if (ex == null)
                    ex = e;
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
                    throw;
                if (ex == null)
                    ex = e;
            }

            try
            {
                recycledMessageState.ReturnHeaders(headers);
                recycledMessageState.ReturnProperties(properties);
                messageData.ReturnMessageState(recycledMessageState);
                recycledMessageState = null;
                messageData.Close();
                messageData = null;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
                if (ex == null)
                    ex = e;
            }

            if (ex != null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex);
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = GetMessageReader())
            {
                reader.MoveToContent();
                EnvelopeVersion envelopeVersion = Version.Envelope;
                writer.WriteStartElement(reader.Prefix, MessageStrings.Envelope, envelopeVersion.Namespace);
                writer.WriteAttributes(reader, false);
            }
        }

        protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = GetMessageReader())
            {
                reader.MoveToContent();
                EnvelopeVersion envelopeVersion = Version.Envelope;
                reader.Read();
                if (HasHeaderElement(reader, envelopeVersion))
                {
                    writer.WriteStartElement(reader.Prefix, MessageStrings.Header, envelopeVersion.Namespace);
                    writer.WriteAttributes(reader, false);
                }
                else
                {
                    writer.WriteStartElement(MessageStrings.Prefix, MessageStrings.Header, envelopeVersion.Namespace);
                }
            }
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = GetBufferedReaderAtBody())
            {
                writer.WriteStartElement(reader.Prefix, MessageStrings.Body, Version.Envelope.Namespace);
                writer.WriteAttributes(reader, false);
            }
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            if (headers.ContainsOnlyBufferedMessageHeaders)
            {
                KeyValuePair<string, object>[] properties = new KeyValuePair<string, object>[Properties.Count];
                ((ICollection<KeyValuePair<string, object>>)Properties).CopyTo(properties, 0);
                messageData.EnableMultipleUsers();
                bool[] understoodHeaders = null;
                if (headers.HasMustUnderstandBeenModified)
                {
                    understoodHeaders = new bool[headers.Count];
                    for (int i = 0; i < headers.Count; i++)
                    {
                        understoodHeaders[i] = headers.IsUnderstood(i);
                    }
                }
                return new BufferedMessageBuffer(messageData, properties, understoodHeaders, headers.HasMustUnderstandBeenModified);
            }
            else
            {
                if (this.reader != null)
                    return OnCreateBufferedCopy(maxBufferSize, this.reader.Quotas);
                return OnCreateBufferedCopy(maxBufferSize, XmlDictionaryReaderQuotas.Max);
            }
        }

        protected override string OnGetBodyAttribute(string localName, string ns)
        {
            if (this.bodyAttributes != null)
                return XmlAttributeHolder.GetAttribute(this.bodyAttributes, localName, ns);
            using (XmlDictionaryReader reader = GetBufferedReaderAtBody())
            {
                return reader.GetAttribute(localName, ns);
            }
        }
    }

    struct XmlAttributeHolder
    {
        string prefix;
        string ns;
        string localName;
        string value;

        public static XmlAttributeHolder[] emptyArray = new XmlAttributeHolder[0];

        public XmlAttributeHolder(string prefix, string localName, string ns, string value)
        {
            this.prefix = prefix;
            this.localName = localName;
            this.ns = ns;
            this.value = value;
        }

        public string Prefix
        {
            get { return prefix; }
        }

        public string NamespaceUri
        {
            get { return ns; }
        }

        public string LocalName
        {
            get { return localName; }
        }

        public string Value
        {
            get { return value; }
        }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
            writer.WriteString(value);
            writer.WriteEndAttribute();
        }

        public static void WriteAttributes(XmlAttributeHolder[] attributes, XmlWriter writer)
        {
            for (int i = 0; i < attributes.Length; i++)
                attributes[i].WriteTo(writer);
        }

        public static XmlAttributeHolder[] ReadAttributes(XmlDictionaryReader reader)
        {
            int maxSizeOfHeaders = int.MaxValue;
            return ReadAttributes(reader, ref maxSizeOfHeaders);
        }

        public static XmlAttributeHolder[] ReadAttributes(XmlDictionaryReader reader, ref int maxSizeOfHeaders)
        {
            if (reader.AttributeCount == 0)
                return emptyArray;
            XmlAttributeHolder[] attributes = new XmlAttributeHolder[reader.AttributeCount];
            reader.MoveToFirstAttribute();
            for (int i = 0; i < attributes.Length; i++)
            {
                string ns = reader.NamespaceURI;
                string localName = reader.LocalName;
                string prefix = reader.Prefix;
                string value = string.Empty;
                while (reader.ReadAttributeValue())
                {
                    if (value.Length == 0)
                        value = reader.Value;
                    else
                        value += reader.Value;
                }
                Deduct(prefix, ref maxSizeOfHeaders);
                Deduct(localName, ref maxSizeOfHeaders);
                Deduct(ns, ref maxSizeOfHeaders);
                Deduct(value, ref maxSizeOfHeaders);
                attributes[i] = new XmlAttributeHolder(prefix, localName, ns, value);
                reader.MoveToNextAttribute();
            }
            reader.MoveToElement();
            return attributes;
        }

        static void Deduct(string s, ref int maxSizeOfHeaders)
        {
            int byteCount = s.Length * sizeof(char);
            if (byteCount > maxSizeOfHeaders)
            {
                string message = SR.GetString(SR.XmlBufferQuotaExceeded);
                Exception inner = new QuotaExceededException(message);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(message, inner));
            }
            maxSizeOfHeaders -= byteCount;
        }

        public static string GetAttribute(XmlAttributeHolder[] attributes, string localName, string ns)
        {
            for (int i = 0; i < attributes.Length; i++)
                if (attributes[i].LocalName == localName && attributes[i].NamespaceUri == ns)
                    return attributes[i].Value;
            return null;
        }
    }

    class RecycledMessageState
    {
        MessageHeaders recycledHeaders;
        MessageProperties recycledProperties;
        UriCache uriCache;
        HeaderInfoCache headerInfoCache;

        public HeaderInfoCache HeaderInfoCache
        {
            get
            {
                if (headerInfoCache == null)
                {
                    headerInfoCache = new HeaderInfoCache();
                }
                return headerInfoCache;
            }
        }

        public UriCache UriCache
        {
            get
            {
                if (uriCache == null)
                    uriCache = new UriCache();
                return uriCache;
            }
        }

        public MessageProperties TakeProperties()
        {
            MessageProperties taken = recycledProperties;
            recycledProperties = null;
            return taken;
        }

        public void ReturnProperties(MessageProperties properties)
        {
            if (properties.CanRecycle)
            {
                properties.Recycle();
                this.recycledProperties = properties;
            }
        }

        public MessageHeaders TakeHeaders()
        {
            MessageHeaders taken = recycledHeaders;
            recycledHeaders = null;
            return taken;
        }

        public void ReturnHeaders(MessageHeaders headers)
        {
            if (headers.CanRecycle)
            {
                headers.Recycle(this.HeaderInfoCache);
                this.recycledHeaders = headers;
            }
        }
    }

    class HeaderInfoCache
    {
        const int maxHeaderInfos = 4;
        HeaderInfo[] headerInfos;
        int index;

        public MessageHeaderInfo TakeHeaderInfo(XmlDictionaryReader reader, string actor, bool mustUnderstand, bool relay, bool isRefParam)
        {
            if (this.headerInfos != null)
            {
                int i = this.index;
                for (;;)
                {
                    HeaderInfo headerInfo = this.headerInfos[i];
                    if (headerInfo != null)
                    {
                        if (headerInfo.Matches(reader, actor, mustUnderstand, relay, isRefParam))
                        {
                            this.headerInfos[i] = null;
                            this.index = (i + 1) % maxHeaderInfos;
                            return headerInfo;
                        }
                    }
                    i = (i + 1) % maxHeaderInfos;
                    if (i == this.index)
                    {
                        break;
                    }
                }
            }

            return new HeaderInfo(reader, actor, mustUnderstand, relay, isRefParam);
        }

        public void ReturnHeaderInfo(MessageHeaderInfo headerInfo)
        {
            HeaderInfo headerInfoToReturn = headerInfo as HeaderInfo;
            if (headerInfoToReturn != null)
            {
                if (this.headerInfos == null)
                {
                    this.headerInfos = new HeaderInfo[maxHeaderInfos];
                }
                int i = this.index;
                for (;;)
                {
                    if (this.headerInfos[i] == null)
                    {
                        break;
                    }
                    i = (i + 1) % maxHeaderInfos;
                    if (i == this.index)
                    {
                        break;
                    }
                }
                this.headerInfos[i] = headerInfoToReturn;
                this.index = (i + 1) % maxHeaderInfos;
            }
        }

        class HeaderInfo : MessageHeaderInfo
        {
            string name;
            string ns;
            string actor;
            bool isReferenceParameter;
            bool mustUnderstand;
            bool relay;

            public HeaderInfo(XmlDictionaryReader reader, string actor, bool mustUnderstand, bool relay, bool isReferenceParameter)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
                this.isReferenceParameter = isReferenceParameter;
                reader.GetNonAtomizedNames(out name, out ns);
            }

            public override string Name
            {
                get { return name; }
            }

            public override string Namespace
            {
                get { return ns; }
            }

            public override bool IsReferenceParameter
            {
                get { return isReferenceParameter; }
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            public override bool Relay
            {
                get { return relay; }
            }

            public bool Matches(XmlDictionaryReader reader, string actor, bool mustUnderstand, bool relay, bool isRefParam)
            {
                return reader.IsStartElement(this.name, this.ns) &&
                    this.actor == actor && this.mustUnderstand == mustUnderstand && this.relay == relay && this.isReferenceParameter == isRefParam;
            }
        }
    }

    class UriCache
    {
        const int MaxKeyLength = 128;
        const int MaxEntries = 8;
        Entry[] entries;
        int count;

        public UriCache()
        {
            entries = new Entry[MaxEntries];
        }

        public Uri CreateUri(string uriString)
        {
            Uri uri = Get(uriString);
            if (uri == null)
            {
                uri = new Uri(uriString);
                Set(uriString, uri);
            }
            return uri;
        }

        Uri Get(string key)
        {
            if (key.Length > MaxKeyLength)
                return null;
            for (int i = count - 1; i >= 0; i--)
                if (entries[i].Key == key)
                    return entries[i].Value;
            return null;
        }

        void Set(string key, Uri value)
        {
            if (key.Length > MaxKeyLength)
                return;
            if (count < entries.Length)
            {
                entries[count++] = new Entry(key, value);
            }
            else
            {
                Array.Copy(entries, 1, entries, 0, entries.Length - 1);
                entries[count - 1] = new Entry(key, value);
            }
        }

        struct Entry
        {
            string key;
            Uri value;

            public Entry(string key, Uri value)
            {
                this.key = key;
                this.value = value;
            }

            public string Key
            {
                get { return key; }
            }

            public Uri Value
            {
                get { return value; }
            }
        }
    }
}
