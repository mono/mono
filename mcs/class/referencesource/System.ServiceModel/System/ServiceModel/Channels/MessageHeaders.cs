//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    public sealed class MessageHeaders : IEnumerable<MessageHeaderInfo>
    {
        int collectionVersion;
        int headerCount;
        Header[] headers;
        MessageVersion version;
        IBufferedMessageData bufferedMessageData;
        UnderstoodHeaders understoodHeaders;
        const int InitialHeaderCount = 4;
        const int MaxRecycledArrayLength = 8;
        static XmlDictionaryString[] localNames;

        internal const string WildcardAction = "*";

        // The highest node and attribute counts reached by the BVTs were 1829 and 667 respectively.
        const int MaxBufferedHeaderNodes = 4096;
        const int MaxBufferedHeaderAttributes = 2048;
        int nodeCount = 0;
        int attrCount = 0;
        bool understoodHeadersModified;

        public MessageHeaders(MessageVersion version, int initialSize)
        {
            Init(version, initialSize);
        }

        public MessageHeaders(MessageVersion version)
            : this(version, InitialHeaderCount)
        {
        }

        internal MessageHeaders(MessageVersion version, XmlDictionaryReader reader, XmlAttributeHolder[] envelopeAttributes, XmlAttributeHolder[] headerAttributes, ref int maxSizeOfHeaders)
            : this(version)
        {
            if (maxSizeOfHeaders < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("maxSizeOfHeaders", maxSizeOfHeaders,
                    SR.GetString(SR.ValueMustBeNonNegative)));
            }

            if (version == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }
            XmlBuffer xmlBuffer = null;
            EnvelopeVersion envelopeVersion = version.Envelope;
            reader.ReadStartElement(XD.MessageDictionary.Header, envelopeVersion.DictionaryNamespace);
            while (reader.IsStartElement())
            {
                if (xmlBuffer == null)
                    xmlBuffer = new XmlBuffer(maxSizeOfHeaders);
                BufferedHeader bufferedHeader = new BufferedHeader(version, xmlBuffer, reader, envelopeAttributes, headerAttributes);
                HeaderProcessing processing = bufferedHeader.MustUnderstand ? HeaderProcessing.MustUnderstand : 0;
                HeaderKind kind = GetHeaderKind(bufferedHeader);
                if (kind != HeaderKind.Unknown)
                {
                    processing |= HeaderProcessing.Understood;
                    MessageHeaders.TraceUnderstood(bufferedHeader);
                }
                Header newHeader = new Header(kind, bufferedHeader, processing);
                AddHeader(newHeader);
            }
            if (xmlBuffer != null)
            {
                xmlBuffer.Close();
                maxSizeOfHeaders -= xmlBuffer.BufferSize;
            }
            reader.ReadEndElement();
            this.collectionVersion = 0;
        }

        internal MessageHeaders(MessageVersion version, XmlDictionaryReader reader, IBufferedMessageData bufferedMessageData, RecycledMessageState recycledMessageState, bool[] understoodHeaders, bool understoodHeadersModified)
        {
            this.headers = new Header[InitialHeaderCount];
            Init(version, reader, bufferedMessageData, recycledMessageState, understoodHeaders, understoodHeadersModified);
        }

        internal MessageHeaders(MessageVersion version, MessageHeaders headers, IBufferedMessageData bufferedMessageData)
        {
            this.version = version;
            this.bufferedMessageData = bufferedMessageData;
            this.headerCount = headers.headerCount;
            this.headers = new Header[headerCount];
            Array.Copy(headers.headers, this.headers, headerCount);
            this.collectionVersion = 0;
        }

        public MessageHeaders(MessageHeaders collection)
        {
            if (collection == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("collection");

            Init(collection.version, collection.headers.Length);
            CopyHeadersFrom(collection);
            this.collectionVersion = 0;
        }

        public string Action
        {
            get
            {
                int index = FindHeaderProperty(HeaderKind.Action);
                if (index < 0)
                    return null;
                ActionHeader actionHeader = headers[index].HeaderInfo as ActionHeader;
                if (actionHeader != null)
                    return actionHeader.Action;
                using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                {
                    return ActionHeader.ReadHeaderValue(reader, version.Addressing);
                }
            }
            set
            {
                if (value != null)
                    SetActionHeader(ActionHeader.Create(value, version.Addressing));
                else
                    SetHeaderProperty(HeaderKind.Action, null);
            }
        }

        internal bool CanRecycle
        {
            get { return headers.Length <= MaxRecycledArrayLength; }
        }

        internal bool ContainsOnlyBufferedMessageHeaders
        {
            get { return (bufferedMessageData != null && collectionVersion == 0); }
        }

        internal int CollectionVersion
        {
            get { return collectionVersion; }
        }

        public int Count
        {
            get { return headerCount; }
        }

        public EndpointAddress FaultTo
        {
            get
            {
                int index = FindHeaderProperty(HeaderKind.FaultTo);
                if (index < 0)
                    return null;
                FaultToHeader faultToHeader = headers[index].HeaderInfo as FaultToHeader;
                if (faultToHeader != null)
                    return faultToHeader.FaultTo;
                using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                {
                    return FaultToHeader.ReadHeaderValue(reader, version.Addressing);
                }
            }
            set
            {
                if (value != null)
                    SetFaultToHeader(FaultToHeader.Create(value, version.Addressing));
                else
                    SetHeaderProperty(HeaderKind.FaultTo, null);
            }
        }

        public EndpointAddress From
        {
            get
            {
                int index = FindHeaderProperty(HeaderKind.From);
                if (index < 0)
                    return null;
                FromHeader fromHeader = headers[index].HeaderInfo as FromHeader;
                if (fromHeader != null)
                    return fromHeader.From;
                using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                {
                    return FromHeader.ReadHeaderValue(reader, version.Addressing);
                }
            }
            set
            {
                if (value != null)
                    SetFromHeader(FromHeader.Create(value, version.Addressing));
                else
                    SetHeaderProperty(HeaderKind.From, null);
            }
        }

        internal bool HasMustUnderstandBeenModified
        {
            get
            {
                if (understoodHeaders != null)
                {
                    return understoodHeaders.Modified;
                }
                else
                {
                    return this.understoodHeadersModified;
                }
            }
        }

        public UniqueId MessageId
        {
            get
            {
                int index = FindHeaderProperty(HeaderKind.MessageId);
                if (index < 0)
                    return null;
                MessageIDHeader messageIDHeader = headers[index].HeaderInfo as MessageIDHeader;
                if (messageIDHeader != null)
                    return messageIDHeader.MessageId;
                using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                {
                    return MessageIDHeader.ReadHeaderValue(reader, version.Addressing);
                }
            }
            set
            {
                if (value != null)
                    SetMessageIDHeader(MessageIDHeader.Create(value, version.Addressing));
                else
                    SetHeaderProperty(HeaderKind.MessageId, null);
            }
        }

        public MessageVersion MessageVersion
        {
            get { return version; }
        }

        public UniqueId RelatesTo
        {
            get
            {
                return GetRelatesTo(RelatesToHeader.ReplyRelationshipType);
            }
            set
            {
                SetRelatesTo(RelatesToHeader.ReplyRelationshipType, value);
            }
        }

        public EndpointAddress ReplyTo
        {
            get
            {
                int index = FindHeaderProperty(HeaderKind.ReplyTo);
                if (index < 0)
                    return null;
                ReplyToHeader replyToHeader = headers[index].HeaderInfo as ReplyToHeader;
                if (replyToHeader != null)
                    return replyToHeader.ReplyTo;
                using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                {
                    return ReplyToHeader.ReadHeaderValue(reader, version.Addressing);
                }
            }
            set
            {
                if (value != null)
                    SetReplyToHeader(ReplyToHeader.Create(value, version.Addressing));
                else
                    SetHeaderProperty(HeaderKind.ReplyTo, null);
            }
        }

        public Uri To
        {
            get
            {
                int index = FindHeaderProperty(HeaderKind.To);
                if (index < 0)
                    return null;
                ToHeader toHeader = headers[index].HeaderInfo as ToHeader;
                if (toHeader != null)
                    return toHeader.To;
                using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                {
                    return ToHeader.ReadHeaderValue(reader, version.Addressing);
                }
            }
            set
            {
                if (value != null)
                    SetToHeader(ToHeader.Create(value, version.Addressing));
                else
                    SetHeaderProperty(HeaderKind.To, null);
            }
        }

        public UnderstoodHeaders UnderstoodHeaders
        {
            get
            {
                if (understoodHeaders == null)
                    understoodHeaders = new UnderstoodHeaders(this, understoodHeadersModified);
                return understoodHeaders;
            }
        }

        public MessageHeaderInfo this[int index]
        {
            get
            {
                if (index < 0 || index >= headerCount)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("index", index,
                        SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
                }

                return headers[index].HeaderInfo;
            }
        }

        public void Add(MessageHeader header)
        {
            Insert(headerCount, header);
        }

        internal void AddActionHeader(ActionHeader actionHeader)
        {
            Insert(headerCount, actionHeader, HeaderKind.Action);
        }

        internal void AddMessageIDHeader(MessageIDHeader messageIDHeader)
        {
            Insert(headerCount, messageIDHeader, HeaderKind.MessageId);
        }

        internal void AddRelatesToHeader(RelatesToHeader relatesToHeader)
        {
            Insert(headerCount, relatesToHeader, HeaderKind.RelatesTo);
        }

        internal void AddReplyToHeader(ReplyToHeader replyToHeader)
        {
            Insert(headerCount, replyToHeader, HeaderKind.ReplyTo);
        }

        internal void AddToHeader(ToHeader toHeader)
        {
            Insert(headerCount, toHeader, HeaderKind.To);
        }

        void Add(MessageHeader header, HeaderKind kind)
        {
            Insert(headerCount, header, kind);
        }

        void AddHeader(Header header)
        {
            InsertHeader(headerCount, header);
        }

        internal void AddUnderstood(int i)
        {
            headers[i].HeaderProcessing |= HeaderProcessing.Understood;
            MessageHeaders.TraceUnderstood(headers[i].HeaderInfo);
        }

        internal void AddUnderstood(MessageHeaderInfo headerInfo)
        {
            if (headerInfo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("headerInfo"));
            for (int i = 0; i < headerCount; i++)
            {
                if ((object)headers[i].HeaderInfo == (object)headerInfo)
                {
                    if ((headers[i].HeaderProcessing & HeaderProcessing.Understood) != 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(
                            SR.GetString(SR.HeaderAlreadyUnderstood, headerInfo.Name, headerInfo.Namespace), "headerInfo"));
                    }

                    AddUnderstood(i);
                }
            }
        }

        void CaptureBufferedHeaders()
        {
            CaptureBufferedHeaders(-1);
        }

        void CaptureBufferedHeaders(int exceptIndex)
        {
            using (XmlDictionaryReader reader = GetBufferedMessageHeaderReaderAtHeaderContents(bufferedMessageData))
            {
                for (int i = 0; i < headerCount; i++)
                {
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        if (reader.MoveToContent() != XmlNodeType.Element)
                            break;
                    }

                    Header header = headers[i];
                    if (i == exceptIndex || header.HeaderType != HeaderType.BufferedMessageHeader)
                    {
                        reader.Skip();
                    }
                    else
                    {
                        headers[i] = new Header(header.HeaderKind, CaptureBufferedHeader(reader,
                            header.HeaderInfo), header.HeaderProcessing);
                    }
                }
            }
            bufferedMessageData = null;
        }

        BufferedHeader CaptureBufferedHeader(XmlDictionaryReader reader, MessageHeaderInfo headerInfo)
        {
            XmlBuffer buffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = buffer.OpenSection(bufferedMessageData.Quotas);
            writer.WriteNode(reader, false);
            buffer.CloseSection();
            buffer.Close();
            return new BufferedHeader(version, buffer, 0, headerInfo);
        }

        BufferedHeader CaptureBufferedHeader(IBufferedMessageData bufferedMessageData, MessageHeaderInfo headerInfo, int bufferedMessageHeaderIndex)
        {
            XmlBuffer buffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = buffer.OpenSection(bufferedMessageData.Quotas);
            WriteBufferedMessageHeader(bufferedMessageData, bufferedMessageHeaderIndex, writer);
            buffer.CloseSection();
            buffer.Close();
            return new BufferedHeader(version, buffer, 0, headerInfo);
        }

        BufferedHeader CaptureWriteableHeader(MessageHeader writeableHeader)
        {
            XmlBuffer buffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            writeableHeader.WriteHeader(writer, this.version);
            buffer.CloseSection();
            buffer.Close();
            return new BufferedHeader(version, buffer, 0, writeableHeader);
        }

        public void Clear()
        {
            for (int i = 0; i < headerCount; i++)
                headers[i] = new Header();
            headerCount = 0;
            collectionVersion++;
            bufferedMessageData = null;
        }

        public void CopyHeaderFrom(Message message, int headerIndex)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            CopyHeaderFrom(message.Headers, headerIndex);
        }

        public void CopyHeaderFrom(MessageHeaders collection, int headerIndex)
        {
            if (collection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("collection");
            }

            if (collection.version != version)
            {
#pragma warning suppress 56506 // [....], collection.version is never null
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MessageHeaderVersionMismatch, collection.version.ToString(), version.ToString()), "collection"));
            }

            if (headerIndex < 0 || headerIndex >= collection.headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", headerIndex,
                    SR.GetString(SR.ValueMustBeInRange, 0, collection.headerCount)));
            }
            Header header = collection.headers[headerIndex];
            HeaderProcessing processing = header.HeaderInfo.MustUnderstand ? HeaderProcessing.MustUnderstand : 0;
            if ((header.HeaderProcessing & HeaderProcessing.Understood) != 0 || header.HeaderKind != HeaderKind.Unknown)
                processing |= HeaderProcessing.Understood;
            switch (header.HeaderType)
            {
                case HeaderType.BufferedMessageHeader:
                    AddHeader(new Header(header.HeaderKind, CaptureBufferedHeader(collection.bufferedMessageData,
                        header.HeaderInfo, headerIndex), processing));
                    break;
                case HeaderType.ReadableHeader:
                    AddHeader(new Header(header.HeaderKind, header.ReadableHeader, processing));
                    break;
                case HeaderType.WriteableHeader:
                    AddHeader(new Header(header.HeaderKind, header.MessageHeader, processing));
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidEnumValue, header.HeaderType)));
            }
        }

        public void CopyHeadersFrom(Message message)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            CopyHeadersFrom(message.Headers);
        }

        public void CopyHeadersFrom(MessageHeaders collection)
        {
            if (collection == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("collection"));
            for (int i = 0; i < collection.headerCount; i++)
                CopyHeaderFrom(collection, i);
        }

        public void CopyTo(MessageHeaderInfo[] array, int index)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("array");
            }

            if (index < 0 || (index + headerCount) > array.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("index", index,
                    SR.GetString(SR.ValueMustBeInRange, 0, array.Length - headerCount)));
            }
            for (int i = 0; i < headerCount; i++)
                array[i + index] = headers[i].HeaderInfo;
        }

        Exception CreateDuplicateHeaderException(HeaderKind kind)
        {
            string name;
            switch (kind)
            {
                case HeaderKind.Action:
                    name = AddressingStrings.Action;
                    break;
                case HeaderKind.FaultTo:
                    name = AddressingStrings.FaultTo;
                    break;
                case HeaderKind.From:
                    name = AddressingStrings.From;
                    break;
                case HeaderKind.MessageId:
                    name = AddressingStrings.MessageId;
                    break;
                case HeaderKind.ReplyTo:
                    name = AddressingStrings.ReplyTo;
                    break;
                case HeaderKind.To:
                    name = AddressingStrings.To;
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidEnumValue, kind)));
            }

            return new MessageHeaderException(
                SR.GetString(SR.MultipleMessageHeaders, name, this.version.Addressing.Namespace),
                name,
                this.version.Addressing.Namespace,
                true);
        }

        public int FindHeader(string name, string ns)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            if (ns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));

            if (ns == this.version.Addressing.Namespace)
            {
                return FindAddressingHeader(name, ns);
            }
            else
            {
                return FindNonAddressingHeader(name, ns, version.Envelope.UltimateDestinationActorValues);
            }
        }

        int FindAddressingHeader(string name, string ns)
        {
            int foundAt = -1;
            for (int i = 0; i < headerCount; i++)
            {
                if (headers[i].HeaderKind != HeaderKind.Unknown)
                {
                    MessageHeaderInfo info = headers[i].HeaderInfo;
                    if (info.Name == name && info.Namespace == ns)
                    {
                        if (foundAt >= 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new MessageHeaderException(SR.GetString(SR.MultipleMessageHeaders, name, ns), name, ns, true));
                        }
                        foundAt = i;
                    }
                }
            }
            return foundAt;
        }

        int FindNonAddressingHeader(string name, string ns, string[] actors)
        {
            int foundAt = -1;
            for (int i = 0; i < headerCount; i++)
            {
                if (headers[i].HeaderKind == HeaderKind.Unknown)
                {
                    MessageHeaderInfo info = headers[i].HeaderInfo;
                    if (info.Name == name && info.Namespace == ns)
                    {
                        for (int j = 0; j < actors.Length; j++)
                        {
                            if (actors[j] == info.Actor)
                            {
                                if (foundAt >= 0)
                                {
                                    if (actors.Length == 1)
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(SR.GetString(SR.MultipleMessageHeadersWithActor, name, ns, actors[0]), name, ns, true));
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(SR.GetString(SR.MultipleMessageHeaders, name, ns), name, ns, true));
                                }
                                foundAt = i;
                            }
                        }
                    }
                }
            }
            return foundAt;
        }

        public int FindHeader(string name, string ns, params string[] actors)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            if (ns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            if (actors == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("actors"));
            int foundAt = -1;
            for (int i = 0; i < headerCount; i++)
            {
                MessageHeaderInfo info = headers[i].HeaderInfo;
                if (info.Name == name && info.Namespace == ns)
                {
                    for (int j = 0; j < actors.Length; j++)
                    {
                        if (actors[j] == info.Actor)
                        {
                            if (foundAt >= 0)
                            {
                                if (actors.Length == 1)
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(SR.GetString(SR.MultipleMessageHeadersWithActor, name, ns, actors[0]), name, ns, true));
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(SR.GetString(SR.MultipleMessageHeaders, name, ns), name, ns, true));
                            }
                            foundAt = i;
                        }
                    }
                }
            }
            return foundAt;
        }

        int FindHeaderProperty(HeaderKind kind)
        {
            int index = -1;
            for (int i = 0; i < headerCount; i++)
            {
                if (headers[i].HeaderKind == kind)
                {
                    if (index >= 0)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateDuplicateHeaderException(kind));
                    index = i;
                }
            }
            return index;
        }

        int FindRelatesTo(Uri relationshipType, out UniqueId messageId)
        {
            UniqueId foundValue = null;
            int foundIndex = -1;
            for (int i = 0; i < headerCount; i++)
            {
                if (headers[i].HeaderKind == HeaderKind.RelatesTo)
                {
                    Uri tempRelationship;
                    UniqueId tempValue;
                    GetRelatesToValues(i, out tempRelationship, out tempValue);

                    if (relationshipType == tempRelationship)
                    {
                        if (foundValue != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new MessageHeaderException(
                                    SR.GetString(SR.MultipleRelatesToHeaders, relationshipType.AbsoluteUri),
                                    AddressingStrings.RelatesTo,
                                    this.version.Addressing.Namespace,
                                    true));
                        }
                        foundValue = tempValue;
                        foundIndex = i;
                    }
                }
            }

            messageId = foundValue;
            return foundIndex;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<MessageHeaderInfo> GetEnumerator()
        {
            MessageHeaderInfo[] headers = new MessageHeaderInfo[headerCount];
            CopyTo(headers, 0);
            return GetEnumerator(headers);
        }

        IEnumerator<MessageHeaderInfo> GetEnumerator(MessageHeaderInfo[] headers)
        {
            IList<MessageHeaderInfo> list = Array.AsReadOnly<MessageHeaderInfo>(headers);
            return list.GetEnumerator();
        }

        internal IEnumerator<MessageHeaderInfo> GetUnderstoodEnumerator()
        {
            List<MessageHeaderInfo> understoodHeaders = new List<MessageHeaderInfo>();

            for (int i = 0; i < headerCount; i++)
            {
                if ((headers[i].HeaderProcessing & HeaderProcessing.Understood) != 0)
                {
                    understoodHeaders.Add(headers[i].HeaderInfo);
                }
            }

            return understoodHeaders.GetEnumerator();
        }

        static XmlDictionaryReader GetBufferedMessageHeaderReaderAtHeaderContents(IBufferedMessageData bufferedMessageData)
        {
            XmlDictionaryReader reader = bufferedMessageData.GetMessageReader();
            if (reader.NodeType == XmlNodeType.Element)
                reader.Read();
            else
                reader.ReadStartElement();
            if (reader.NodeType == XmlNodeType.Element)
                reader.Read();
            else
                reader.ReadStartElement();
            return reader;
        }

        XmlDictionaryReader GetBufferedMessageHeaderReader(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex)
        {
            // Check if we need to change representations
            if (this.nodeCount > MaxBufferedHeaderNodes || this.attrCount > MaxBufferedHeaderAttributes)
            {
                CaptureBufferedHeaders();
                return headers[bufferedMessageHeaderIndex].ReadableHeader.GetHeaderReader();
            }

            XmlDictionaryReader reader = GetBufferedMessageHeaderReaderAtHeaderContents(bufferedMessageData);
            for (;;)
            {
                if (reader.NodeType != XmlNodeType.Element)
                    reader.MoveToContent();
                if (bufferedMessageHeaderIndex == 0)
                    break;
                Skip(reader);
                bufferedMessageHeaderIndex--;
            }

            return reader;
        }

        void Skip(XmlDictionaryReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element && !reader.IsEmptyElement)
            {
                int depth = reader.Depth;
                do
                {
                    this.attrCount += reader.AttributeCount;
                    this.nodeCount++;
                } while (reader.Read() && depth < reader.Depth);

                // consume end tag
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    this.nodeCount++;
                    reader.Read();
                }
            }
            else
            {
                this.attrCount += reader.AttributeCount;
                this.nodeCount++;
                reader.Read();
            }
        }

        public T GetHeader<T>(string name, string ns)
        {
            return GetHeader<T>(name, ns, DataContractSerializerDefaults.CreateSerializer(typeof(T), name, ns, int.MaxValue/*maxItems*/));
        }

        public T GetHeader<T>(string name, string ns, params string[] actors)
        {
            int index = FindHeader(name, ns, actors);
            if (index < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(SR.GetString(SR.HeaderNotFound, name, ns), name, ns));
            return GetHeader<T>(index);

        }

        public T GetHeader<T>(string name, string ns, XmlObjectSerializer serializer)
        {
            if (serializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            int index = FindHeader(name, ns);
            if (index < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(SR.GetString(SR.HeaderNotFound, name, ns), name, ns));
            return GetHeader<T>(index, serializer);
        }

        public T GetHeader<T>(int index)
        {
            if (index < 0 || index >= headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("index", index,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }

            MessageHeaderInfo headerInfo = headers[index].HeaderInfo;
            return GetHeader<T>(index, DataContractSerializerDefaults.CreateSerializer(typeof(T), headerInfo.Name, headerInfo.Namespace, int.MaxValue/*maxItems*/));
        }

        public T GetHeader<T>(int index, XmlObjectSerializer serializer)
        {
            if (serializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            using (XmlDictionaryReader reader = GetReaderAtHeader(index))
            {
                return (T)serializer.ReadObject(reader);
            }
        }

        HeaderKind GetHeaderKind(MessageHeaderInfo headerInfo)
        {
            HeaderKind headerKind = HeaderKind.Unknown;

            if (headerInfo.Namespace == this.version.Addressing.Namespace)
            {
                if (version.Envelope.IsUltimateDestinationActor(headerInfo.Actor))
                {
                    string name = headerInfo.Name;
                    if (name.Length > 0)
                    {
                        switch (name[0])
                        {
                            case 'A':
                                if (name == AddressingStrings.Action)
                                {
                                    headerKind = HeaderKind.Action;
                                }
                                break;
                            case 'F':
                                if (name == AddressingStrings.From)
                                {
                                    headerKind = HeaderKind.From;
                                }
                                else if (name == AddressingStrings.FaultTo)
                                {
                                    headerKind = HeaderKind.FaultTo;
                                }
                                break;
                            case 'M':
                                if (name == AddressingStrings.MessageId)
                                {
                                    headerKind = HeaderKind.MessageId;
                                }
                                break;
                            case 'R':
                                if (name == AddressingStrings.ReplyTo)
                                {
                                    headerKind = HeaderKind.ReplyTo;
                                }
                                else if (name == AddressingStrings.RelatesTo)
                                {
                                    headerKind = HeaderKind.RelatesTo;
                                }
                                break;
                            case 'T':
                                if (name == AddressingStrings.To)
                                {
                                    headerKind = HeaderKind.To;
                                }
                                break;
                        }
                    }
                }
            }

            ValidateHeaderKind(headerKind);
            return headerKind;
        }

        void ValidateHeaderKind(HeaderKind headerKind)
        {
            if (this.version.Envelope == EnvelopeVersion.None)
            {
                if (headerKind != HeaderKind.Action && headerKind != HeaderKind.To)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.HeadersCannotBeAddedToEnvelopeVersion, this.version.Envelope)));
                }
            }

            if (this.version.Addressing == AddressingVersion.None)
            {
                if (headerKind != HeaderKind.Unknown && headerKind != HeaderKind.Action && headerKind != HeaderKind.To)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.AddressingHeadersCannotBeAddedToAddressingVersion, this.version.Addressing)));
                }
            }
        }

        public XmlDictionaryReader GetReaderAtHeader(int headerIndex)
        {
            if (headerIndex < 0 || headerIndex >= headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", headerIndex,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }

            switch (headers[headerIndex].HeaderType)
            {
                case HeaderType.ReadableHeader:
                    return headers[headerIndex].ReadableHeader.GetHeaderReader();
                case HeaderType.WriteableHeader:
                    MessageHeader writeableHeader = headers[headerIndex].MessageHeader;
                    BufferedHeader bufferedHeader = CaptureWriteableHeader(writeableHeader);
                    headers[headerIndex] = new Header(headers[headerIndex].HeaderKind, bufferedHeader, headers[headerIndex].HeaderProcessing);
                    collectionVersion++;
                    return bufferedHeader.GetHeaderReader();
                case HeaderType.BufferedMessageHeader:
                    return GetBufferedMessageHeaderReader(bufferedMessageData, headerIndex);
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidEnumValue, headers[headerIndex].HeaderType)));
            }
        }

        internal UniqueId GetRelatesTo(Uri relationshipType)
        {
            if (relationshipType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("relationshipType"));

            UniqueId messageId;
            FindRelatesTo(relationshipType, out messageId);
            return messageId;
        }

        void GetRelatesToValues(int index, out Uri relationshipType, out UniqueId messageId)
        {
            RelatesToHeader relatesToHeader = headers[index].HeaderInfo as RelatesToHeader;
            if (relatesToHeader != null)
            {
                relationshipType = relatesToHeader.RelationshipType;
                messageId = relatesToHeader.UniqueId;
            }
            else
            {
                using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                {
                    RelatesToHeader.ReadHeaderValue(reader, version.Addressing, out relationshipType, out messageId);
                }
            }
        }

        internal string[] GetHeaderAttributes(string localName, string ns)
        {
            string[] attrs = null;

            if (ContainsOnlyBufferedMessageHeaders)
            {
                XmlDictionaryReader reader = bufferedMessageData.GetMessageReader();
                reader.ReadStartElement(); // Envelope
                reader.ReadStartElement(); // Header
                for (int index = 0; reader.IsStartElement(); index++)
                {
                    string value = reader.GetAttribute(localName, ns);
                    if (value != null)
                    {
                        if (attrs == null)
                            attrs = new string[headerCount];
                        attrs[index] = value;
                    }
                    if (index == headerCount - 1)
                        break;
                    reader.Skip();
                }
                reader.Close();
            }
            else
            {
                for (int index = 0; index < headerCount; index++)
                {
                    if (headers[index].HeaderType != HeaderType.WriteableHeader)
                    {
                        using (XmlDictionaryReader reader = GetReaderAtHeader(index))
                        {
                            string value = reader.GetAttribute(localName, ns);
                            if (value != null)
                            {
                                if (attrs == null)
                                    attrs = new string[headerCount];
                                attrs[index] = value;
                            }
                        }
                    }
                }
            }

            return attrs;
        }

        internal MessageHeader GetMessageHeader(int index)
        {
            if (index < 0 || index >= headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", index,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }
            MessageHeader messageHeader;
            switch (headers[index].HeaderType)
            {
                case HeaderType.WriteableHeader:
                case HeaderType.ReadableHeader:
                    return headers[index].MessageHeader;
                case HeaderType.BufferedMessageHeader:
                    messageHeader = CaptureBufferedHeader(bufferedMessageData, headers[index].HeaderInfo, index);
                    headers[index] = new Header(headers[index].HeaderKind, messageHeader, headers[index].HeaderProcessing);
                    collectionVersion++;
                    return messageHeader;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidEnumValue, headers[index].HeaderType)));
            }
        }

        internal Collection<MessageHeaderInfo> GetHeadersNotUnderstood()
        {
            Collection<MessageHeaderInfo> notUnderstoodHeaders = null;

            for (int headerIndex = 0; headerIndex < headerCount; headerIndex++)
            {
                if (headers[headerIndex].HeaderProcessing == HeaderProcessing.MustUnderstand)
                {
                    if (notUnderstoodHeaders == null)
                        notUnderstoodHeaders = new Collection<MessageHeaderInfo>();

                    MessageHeaderInfo headerInfo = headers[headerIndex].HeaderInfo;
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.DidNotUnderstandMessageHeader,
                            SR.GetString(SR.TraceCodeDidNotUnderstandMessageHeader),
                            new MessageHeaderInfoTraceRecord(headerInfo), null, null);
                    }

                    notUnderstoodHeaders.Add(headerInfo);
                }
            }

            return notUnderstoodHeaders;
        }

        public bool HaveMandatoryHeadersBeenUnderstood()
        {
            return HaveMandatoryHeadersBeenUnderstood(version.Envelope.MustUnderstandActorValues);
        }

        public bool HaveMandatoryHeadersBeenUnderstood(params string[] actors)
        {
            if (actors == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("actors"));

            for (int headerIndex = 0; headerIndex < headerCount; headerIndex++)
            {
                if (headers[headerIndex].HeaderProcessing == HeaderProcessing.MustUnderstand)
                {
                    for (int actorIndex = 0; actorIndex < actors.Length; ++actorIndex)
                    {
                        if (headers[headerIndex].HeaderInfo.Actor == actors[actorIndex])
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        internal void Init(MessageVersion version, int initialSize)
        {
            this.nodeCount = 0;
            this.attrCount = 0;
            if (initialSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("initialSize", initialSize,
                    SR.GetString(SR.ValueMustBeNonNegative)));
            }

            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }

            this.version = version;
            headers = new Header[initialSize];
        }

        internal void Init(MessageVersion version)
        {
            this.nodeCount = 0;
            this.attrCount = 0;
            this.version = version;
            this.collectionVersion = 0;
        }

        internal void Init(MessageVersion version, XmlDictionaryReader reader, IBufferedMessageData bufferedMessageData, RecycledMessageState recycledMessageState, bool[] understoodHeaders, bool understoodHeadersModified)
        {
            this.nodeCount = 0;
            this.attrCount = 0;
            this.version = version;
            this.bufferedMessageData = bufferedMessageData;

            if (version.Envelope != EnvelopeVersion.None)
            {
                this.understoodHeadersModified = (understoodHeaders != null) && understoodHeadersModified;
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                    return;
                }
                EnvelopeVersion envelopeVersion = version.Envelope;
                Fx.Assert(reader.IsStartElement(XD.MessageDictionary.Header, envelopeVersion.DictionaryNamespace), "");
                reader.ReadStartElement();

                AddressingDictionary dictionary = XD.AddressingDictionary;

                if (localNames == null)
                {
                    XmlDictionaryString[] strings = new XmlDictionaryString[7];
                    strings[(int)HeaderKind.To] = dictionary.To;
                    strings[(int)HeaderKind.Action] = dictionary.Action;
                    strings[(int)HeaderKind.MessageId] = dictionary.MessageId;
                    strings[(int)HeaderKind.RelatesTo] = dictionary.RelatesTo;
                    strings[(int)HeaderKind.ReplyTo] = dictionary.ReplyTo;
                    strings[(int)HeaderKind.From] = dictionary.From;
                    strings[(int)HeaderKind.FaultTo] = dictionary.FaultTo;
                    System.Threading.Thread.MemoryBarrier();
                    localNames = strings;
                }


                int i = 0;
                while (reader.IsStartElement())
                {
                    ReadBufferedHeader(reader, recycledMessageState, localNames, (understoodHeaders == null) ? false : understoodHeaders[i++]);
                }

                reader.ReadEndElement();
            }
            this.collectionVersion = 0;
        }

        public void Insert(int headerIndex, MessageHeader header)
        {
            if (header == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("header"));
            if (!header.IsMessageVersionSupported(this.version))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MessageHeaderVersionNotSupported,
                    header.GetType().FullName, this.version.Envelope.ToString()), "header"));
            Insert(headerIndex, header, GetHeaderKind(header));
        }

        void Insert(int headerIndex, MessageHeader header, HeaderKind kind)
        {
            ReadableMessageHeader readableMessageHeader = header as ReadableMessageHeader;
            HeaderProcessing processing = header.MustUnderstand ? HeaderProcessing.MustUnderstand : 0;
            if (kind != HeaderKind.Unknown)
                processing |= HeaderProcessing.Understood;
            if (readableMessageHeader != null)
                InsertHeader(headerIndex, new Header(kind, readableMessageHeader, processing));
            else
                InsertHeader(headerIndex, new Header(kind, header, processing));
        }

        void InsertHeader(int headerIndex, Header header)
        {
            ValidateHeaderKind(header.HeaderKind);

            if (headerIndex < 0 || headerIndex > headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", headerIndex,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }

            if (headerCount == headers.Length)
            {
                if (headers.Length == 0)
                {
                    headers = new Header[1];
                }
                else
                {
                    Header[] newHeaders = new Header[headers.Length * 2];
                    headers.CopyTo(newHeaders, 0);
                    headers = newHeaders;
                }
            }
            if (headerIndex < headerCount)
            {
                if (bufferedMessageData != null)
                {
                    for (int i = headerIndex; i < headerCount; i++)
                    {
                        if (headers[i].HeaderType == HeaderType.BufferedMessageHeader)
                        {
                            CaptureBufferedHeaders();
                            break;
                        }
                    }
                }
                Array.Copy(headers, headerIndex, headers, headerIndex + 1, headerCount - headerIndex);
            }
            headers[headerIndex] = header;
            headerCount++;
            collectionVersion++;
        }

        internal bool IsUnderstood(int i)
        {
            return (headers[i].HeaderProcessing & HeaderProcessing.Understood) != 0;
        }

        internal bool IsUnderstood(MessageHeaderInfo headerInfo)
        {
            if (headerInfo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("headerInfo"));
            for (int i = 0; i < headerCount; i++)
            {
                if ((object)headers[i].HeaderInfo == (object)headerInfo)
                {
                    if (IsUnderstood(i))
                        return true;
                }
            }

            return false;
        }

        void ReadBufferedHeader(XmlDictionaryReader reader, RecycledMessageState recycledMessageState, XmlDictionaryString[] localNames, bool understood)
        {
            string actor;
            bool mustUnderstand;
            bool relay;
            bool isRefParam;

            if (this.version.Addressing == AddressingVersion.None && reader.NamespaceURI == AddressingVersion.None.Namespace)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.AddressingHeadersCannotBeAddedToAddressingVersion, this.version.Addressing)));
            }

            MessageHeader.GetHeaderAttributes(reader, version, out actor, out mustUnderstand, out relay, out isRefParam);

            HeaderKind kind = HeaderKind.Unknown;
            MessageHeaderInfo info = null;

            if (version.Envelope.IsUltimateDestinationActor(actor))
            {
                Fx.Assert(version.Addressing.DictionaryNamespace != null, "non-None Addressing requires a non-null DictionaryNamespace");
                kind = (HeaderKind)reader.IndexOfLocalName(localNames, version.Addressing.DictionaryNamespace);
                switch (kind)
                {
                    case HeaderKind.To:
                        info = ToHeader.ReadHeader(reader, version.Addressing, recycledMessageState.UriCache, actor, mustUnderstand, relay);
                        break;
                    case HeaderKind.Action:
                        info = ActionHeader.ReadHeader(reader, version.Addressing, actor, mustUnderstand, relay);
                        break;
                    case HeaderKind.MessageId:
                        info = MessageIDHeader.ReadHeader(reader, version.Addressing, actor, mustUnderstand, relay);
                        break;
                    case HeaderKind.RelatesTo:
                        info = RelatesToHeader.ReadHeader(reader, version.Addressing, actor, mustUnderstand, relay);
                        break;
                    case HeaderKind.ReplyTo:
                        info = ReplyToHeader.ReadHeader(reader, version.Addressing, actor, mustUnderstand, relay);
                        break;
                    case HeaderKind.From:
                        info = FromHeader.ReadHeader(reader, version.Addressing, actor, mustUnderstand, relay);
                        break;
                    case HeaderKind.FaultTo:
                        info = FaultToHeader.ReadHeader(reader, version.Addressing, actor, mustUnderstand, relay);
                        break;
                    default:
                        kind = HeaderKind.Unknown;
                        break;
                }
            }

            if (info == null)
            {
                info = recycledMessageState.HeaderInfoCache.TakeHeaderInfo(reader, actor, mustUnderstand, relay, isRefParam);
                reader.Skip();
            }

            HeaderProcessing processing = mustUnderstand ? HeaderProcessing.MustUnderstand : 0;
            if (kind != HeaderKind.Unknown || understood)
            {
                processing |= HeaderProcessing.Understood;
                MessageHeaders.TraceUnderstood(info);
            }
            AddHeader(new Header(kind, info, processing));
        }

        internal void Recycle(HeaderInfoCache headerInfoCache)
        {
            for (int i = 0; i < headerCount; i++)
            {
                if (headers[i].HeaderKind == HeaderKind.Unknown)
                {
                    headerInfoCache.ReturnHeaderInfo(headers[i].HeaderInfo);
                }
            }
            Clear();
            collectionVersion = 0;
            if (understoodHeaders != null)
            {
                understoodHeaders.Modified = false;
            }
        }

        internal void RemoveUnderstood(MessageHeaderInfo headerInfo)
        {
            if (headerInfo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("headerInfo"));
            for (int i = 0; i < headerCount; i++)
            {
                if ((object)headers[i].HeaderInfo == (object)headerInfo)
                {
                    if ((headers[i].HeaderProcessing & HeaderProcessing.Understood) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(
                            SR.GetString(SR.HeaderAlreadyNotUnderstood, headerInfo.Name, headerInfo.Namespace), "headerInfo"));
                    }

                    headers[i].HeaderProcessing &= ~HeaderProcessing.Understood;
                }
            }
        }

        public void RemoveAll(string name, string ns)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            if (ns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            for (int i = headerCount - 1; i >= 0; i--)
            {
                MessageHeaderInfo info = headers[i].HeaderInfo;
                if (info.Name == name && info.Namespace == ns)
                {
                    RemoveAt(i);
                }
            }
        }

        public void RemoveAt(int headerIndex)
        {
            if (headerIndex < 0 || headerIndex >= headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", headerIndex,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }
            if (bufferedMessageData != null && headers[headerIndex].HeaderType == HeaderType.BufferedMessageHeader)
                CaptureBufferedHeaders(headerIndex);
            Array.Copy(headers, headerIndex + 1, headers, headerIndex, headerCount - headerIndex - 1);
            headers[--headerCount] = new Header();
            collectionVersion++;
        }

        internal void ReplaceAt(int headerIndex, MessageHeader header)
        {
            if (headerIndex < 0 || headerIndex >= headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", headerIndex,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }

            if (header == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("header");
            }

            ReplaceAt(headerIndex, header, GetHeaderKind(header));
        }

        void ReplaceAt(int headerIndex, MessageHeader header, HeaderKind kind)
        {
            HeaderProcessing processing = header.MustUnderstand ? HeaderProcessing.MustUnderstand : 0;
            if (kind != HeaderKind.Unknown)
                processing |= HeaderProcessing.Understood;
            ReadableMessageHeader readableMessageHeader = header as ReadableMessageHeader;
            if (readableMessageHeader != null)
                headers[headerIndex] = new Header(kind, readableMessageHeader, processing);
            else
                headers[headerIndex] = new Header(kind, header, processing);
            collectionVersion++;
        }

        public void SetAction(XmlDictionaryString action)
        {
            if (action == null)
                SetHeaderProperty(HeaderKind.Action, null);
            else
                SetActionHeader(ActionHeader.Create(action, version.Addressing));
        }

        internal void SetActionHeader(ActionHeader actionHeader)
        {
            SetHeaderProperty(HeaderKind.Action, actionHeader);
        }

        internal void SetFaultToHeader(FaultToHeader faultToHeader)
        {
            SetHeaderProperty(HeaderKind.FaultTo, faultToHeader);
        }

        internal void SetFromHeader(FromHeader fromHeader)
        {
            SetHeaderProperty(HeaderKind.From, fromHeader);
        }

        internal void SetMessageIDHeader(MessageIDHeader messageIDHeader)
        {
            SetHeaderProperty(HeaderKind.MessageId, messageIDHeader);
        }

        internal void SetRelatesTo(Uri relationshipType, UniqueId messageId)
        {
            if (relationshipType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("relationshipType");
            }

            RelatesToHeader relatesToHeader;
            if (!object.ReferenceEquals(messageId, null))
            {
                relatesToHeader = RelatesToHeader.Create(messageId, version.Addressing, relationshipType);
            }
            else
            {
                relatesToHeader = null;
            }

            SetRelatesTo(RelatesToHeader.ReplyRelationshipType, relatesToHeader);
        }

        void SetRelatesTo(Uri relationshipType, RelatesToHeader relatesToHeader)
        {
            UniqueId previousUniqueId;
            int index = FindRelatesTo(relationshipType, out previousUniqueId);
            if (index >= 0)
            {
                if (relatesToHeader == null)
                {
                    RemoveAt(index);
                }
                else
                {
                    ReplaceAt(index, relatesToHeader, HeaderKind.RelatesTo);
                }
            }
            else if (relatesToHeader != null)
            {
                Add(relatesToHeader, HeaderKind.RelatesTo);
            }
        }

        internal void SetReplyToHeader(ReplyToHeader replyToHeader)
        {
            SetHeaderProperty(HeaderKind.ReplyTo, replyToHeader);
        }

        internal void SetToHeader(ToHeader toHeader)
        {
            SetHeaderProperty(HeaderKind.To, toHeader);
        }

        void SetHeaderProperty(HeaderKind kind, MessageHeader header)
        {
            int index = FindHeaderProperty(kind);
            if (index >= 0)
            {
                if (header == null)
                {
                    RemoveAt(index);
                }
                else
                {
                    ReplaceAt(index, header, kind);
                }
            }
            else if (header != null)
            {
                Add(header, kind);
            }
        }

        public void WriteHeader(int headerIndex, XmlWriter writer)
        {
            WriteHeader(headerIndex, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteHeader(int headerIndex, XmlDictionaryWriter writer)
        {
            WriteStartHeader(headerIndex, writer);
            WriteHeaderContents(headerIndex, writer);
            writer.WriteEndElement();
        }

        public void WriteStartHeader(int headerIndex, XmlWriter writer)
        {
            WriteStartHeader(headerIndex, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteStartHeader(int headerIndex, XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (headerIndex < 0 || headerIndex >= headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", headerIndex,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }
            switch (headers[headerIndex].HeaderType)
            {
                case HeaderType.ReadableHeader:
                case HeaderType.WriteableHeader:
                    headers[headerIndex].MessageHeader.WriteStartHeader(writer, this.version);
                    break;
                case HeaderType.BufferedMessageHeader:
                    WriteStartBufferedMessageHeader(bufferedMessageData, headerIndex, writer);
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidEnumValue, headers[headerIndex].HeaderType)));
            }
        }

        public void WriteHeaderContents(int headerIndex, XmlWriter writer)
        {
            WriteHeaderContents(headerIndex, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteHeaderContents(int headerIndex, XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (headerIndex < 0 || headerIndex >= headerCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("headerIndex", headerIndex,
                    SR.GetString(SR.ValueMustBeInRange, 0, headerCount)));
            }
            switch (headers[headerIndex].HeaderType)
            {
                case HeaderType.ReadableHeader:
                case HeaderType.WriteableHeader:
                    headers[headerIndex].MessageHeader.WriteHeaderContents(writer, this.version);
                    break;
                case HeaderType.BufferedMessageHeader:
                    WriteBufferedMessageHeaderContents(bufferedMessageData, headerIndex, writer);
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidEnumValue, headers[headerIndex].HeaderType)));
            }
        }

        static void TraceUnderstood(MessageHeaderInfo info)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.UnderstoodMessageHeader,
                    SR.GetString(SR.TraceCodeUnderstoodMessageHeader),
                    new MessageHeaderInfoTraceRecord(info), null, null);
            }
        }

        void WriteBufferedMessageHeader(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex, XmlWriter writer)
        {
            using (XmlReader reader = GetBufferedMessageHeaderReader(bufferedMessageData, bufferedMessageHeaderIndex))
            {
                writer.WriteNode(reader, false);
            }
        }

        void WriteStartBufferedMessageHeader(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex, XmlWriter writer)
        {
            using (XmlReader reader = GetBufferedMessageHeaderReader(bufferedMessageData, bufferedMessageHeaderIndex))
            {
                writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                writer.WriteAttributes(reader, false);
            }
        }

        void WriteBufferedMessageHeaderContents(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex, XmlWriter writer)
        {
            using (XmlReader reader = GetBufferedMessageHeaderReader(bufferedMessageData, bufferedMessageHeaderIndex))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        writer.WriteNode(reader, false);
                    }
                    reader.ReadEndElement();
                }
            }
        }

        enum HeaderType : byte
        {
            Invalid,
            ReadableHeader,
            BufferedMessageHeader,
            WriteableHeader
        }

        enum HeaderKind : byte
        {
            Action,
            FaultTo,
            From,
            MessageId,
            ReplyTo,
            RelatesTo,
            To,
            Unknown,
        }

        [Flags]
        enum HeaderProcessing : byte
        {
            MustUnderstand = 0x1,
            Understood = 0x2,
        }

        struct Header
        {
            HeaderType type;
            HeaderKind kind;
            HeaderProcessing processing;
            MessageHeaderInfo info;

            public Header(HeaderKind kind, MessageHeaderInfo info, HeaderProcessing processing)
            {
                this.kind = kind;
                this.type = HeaderType.BufferedMessageHeader;
                this.info = info;
                this.processing = processing;
            }

            public Header(HeaderKind kind, ReadableMessageHeader readableHeader, HeaderProcessing processing)
            {
                this.kind = kind;
                this.type = HeaderType.ReadableHeader;
                this.info = readableHeader;
                this.processing = processing;
            }

            public Header(HeaderKind kind, MessageHeader header, HeaderProcessing processing)
            {
                this.kind = kind;
                this.type = HeaderType.WriteableHeader;
                this.info = header;
                this.processing = processing;
            }

            public HeaderType HeaderType
            {
                get { return type; }
            }

            public HeaderKind HeaderKind
            {
                get { return kind; }
            }

            public MessageHeaderInfo HeaderInfo
            {
                get { return info; }
            }

            public MessageHeader MessageHeader
            {
                get
                {
                    Fx.Assert(type == HeaderType.WriteableHeader || type == HeaderType.ReadableHeader, "");
                    return (MessageHeader)info;
                }
            }

            public HeaderProcessing HeaderProcessing
            {
                get { return processing; }
                set { processing = value; }
            }

            public ReadableMessageHeader ReadableHeader
            {
                get
                {
                    Fx.Assert(type == HeaderType.ReadableHeader, "");
                    return (ReadableMessageHeader)info;
                }
            }
        }
    }
}
