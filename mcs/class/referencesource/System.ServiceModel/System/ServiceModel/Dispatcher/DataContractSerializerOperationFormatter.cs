//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Linq;

    static class DataContractSerializerDefaults
    {
        internal const bool IgnoreExtensionDataObject = false;
        internal const int MaxItemsInObjectGraph = int.MaxValue;

        internal static DataContractSerializer CreateSerializer(Type type, int maxItems)
        {
            return CreateSerializer(type, null, maxItems);
        }

        internal static DataContractSerializer CreateSerializer(Type type, IList<Type> knownTypes, int maxItems)
        {
            return new DataContractSerializer(
                type,
                knownTypes,
                maxItems,
                DataContractSerializerDefaults.IgnoreExtensionDataObject,
                false/*preserveObjectReferences*/,
                null/*dataContractSurrage*/);
        }

        internal static DataContractSerializer CreateSerializer(Type type, string rootName, string rootNs, int maxItems)
        {
            return CreateSerializer(type, null, rootName, rootNs, maxItems);
        }

        internal static DataContractSerializer CreateSerializer(Type type, IList<Type> knownTypes, string rootName, string rootNs, int maxItems)
        {
            return new DataContractSerializer(
                type,
                rootName,
                rootNs,
                knownTypes,
                maxItems,
                DataContractSerializerDefaults.IgnoreExtensionDataObject,
                false/*preserveObjectReferences*/,
                null/*dataContractSurrage*/);
        }
        internal static DataContractSerializer CreateSerializer(Type type, XmlDictionaryString rootName, XmlDictionaryString rootNs, int maxItems)
        {
            return CreateSerializer(type, null, rootName, rootNs, maxItems);
        }

        internal static DataContractSerializer CreateSerializer(Type type, IList<Type> knownTypes, XmlDictionaryString rootName, XmlDictionaryString rootNs, int maxItems)
        {
            return new DataContractSerializer(
                type,
                rootName,
                rootNs,
                knownTypes,
                maxItems,
                DataContractSerializerDefaults.IgnoreExtensionDataObject,
                false/*preserveObjectReferences*/,
                null/*dataContractSurrage*/);
        }
    }

    class DataContractSerializerOperationFormatter : OperationFormatter
    {
        static Type typeOfIQueryable = typeof(IQueryable);
        static Type typeOfIQueryableGeneric = typeof(IQueryable<>);
        static Type typeOfIEnumerable = typeof(IEnumerable);
        static Type typeOfIEnumerableGeneric = typeof(IEnumerable<>);

        protected MessageInfo requestMessageInfo;
        protected MessageInfo replyMessageInfo;
        IList<Type> knownTypes;
        XsdDataContractExporter dataContractExporter;
        DataContractSerializerOperationBehavior serializerFactory;

        public DataContractSerializerOperationFormatter(OperationDescription description, DataContractFormatAttribute dataContractFormatAttribute,
            DataContractSerializerOperationBehavior serializerFactory)
            : base(description, dataContractFormatAttribute.Style == OperationFormatStyle.Rpc, false/*isEncoded*/)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            this.serializerFactory = serializerFactory ?? new DataContractSerializerOperationBehavior(description);
            foreach (Type type in description.KnownTypes)
            {
                if (knownTypes == null)
                    knownTypes = new List<Type>();
                if (type == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxKnownTypeNull, description.Name)));
                ValidateDataContractType(type);
                knownTypes.Add(type);
            }
            requestMessageInfo = CreateMessageInfo(dataContractFormatAttribute, RequestDescription, this.serializerFactory);
            if (ReplyDescription != null)
                replyMessageInfo = CreateMessageInfo(dataContractFormatAttribute, ReplyDescription, this.serializerFactory);
        }

        MessageInfo CreateMessageInfo(DataContractFormatAttribute dataContractFormatAttribute,
            MessageDescription messageDescription, DataContractSerializerOperationBehavior serializerFactory)
        {
            if (messageDescription.IsUntypedMessage)
                return null;
            MessageInfo messageInfo = new MessageInfo();

            MessageBodyDescription body = messageDescription.Body;
            if (body.WrapperName != null)
            {
                messageInfo.WrapperName = AddToDictionary(body.WrapperName);
                messageInfo.WrapperNamespace = AddToDictionary(body.WrapperNamespace);
            }
            MessagePartDescriptionCollection parts = body.Parts;
            messageInfo.BodyParts = new PartInfo[parts.Count];
            for (int i = 0; i < parts.Count; i++)
                messageInfo.BodyParts[i] = CreatePartInfo(parts[i], dataContractFormatAttribute.Style, serializerFactory);
            if (IsValidReturnValue(messageDescription.Body.ReturnValue))
                messageInfo.ReturnPart = CreatePartInfo(messageDescription.Body.ReturnValue, dataContractFormatAttribute.Style, serializerFactory);
            messageInfo.HeaderDescriptionTable = new MessageHeaderDescriptionTable();
            messageInfo.HeaderParts = new PartInfo[messageDescription.Headers.Count];
            for (int i = 0; i < messageDescription.Headers.Count; i++)
            {
                MessageHeaderDescription headerDescription = messageDescription.Headers[i];
                if (headerDescription.IsUnknownHeaderCollection)
                    messageInfo.UnknownHeaderDescription = headerDescription;
                else
                {
                    ValidateDataContractType(headerDescription.Type);
                    messageInfo.HeaderDescriptionTable.Add(headerDescription.Name, headerDescription.Namespace, headerDescription);
                }
                messageInfo.HeaderParts[i] = CreatePartInfo(headerDescription, OperationFormatStyle.Document, serializerFactory);
            }
            messageInfo.AnyHeaders = messageInfo.UnknownHeaderDescription != null || messageInfo.HeaderDescriptionTable.Count > 0;
            return messageInfo;
        }

        private void ValidateDataContractType(Type type)
        {
            if (dataContractExporter == null)
            {
                dataContractExporter = new XsdDataContractExporter();
                if (serializerFactory != null && serializerFactory.DataContractSurrogate != null)
                {
                    ExportOptions options = new ExportOptions();
                    options.DataContractSurrogate = serializerFactory.DataContractSurrogate;
                    dataContractExporter.Options = options;
                }
            }
            dataContractExporter.GetSchemaTypeName(type); //Throws if the type is not a valid data contract
        }

        PartInfo CreatePartInfo(MessagePartDescription part, OperationFormatStyle style, DataContractSerializerOperationBehavior serializerFactory)
        {
            string ns = (style == OperationFormatStyle.Rpc || part.Namespace == null) ? string.Empty : part.Namespace;
            PartInfo partInfo = new PartInfo(part, AddToDictionary(part.Name), AddToDictionary(ns), knownTypes, serializerFactory);
            ValidateDataContractType(partInfo.ContractType);
            return partInfo;
        }

        protected override void AddHeadersToMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            MessageInfo messageInfo = isRequest ? requestMessageInfo : replyMessageInfo;
            PartInfo[] headerParts = messageInfo.HeaderParts;
            if (headerParts == null || headerParts.Length == 0)
                return;
            MessageHeaders headers = message.Headers;
            for (int i = 0; i < headerParts.Length; i++)
            {
                PartInfo headerPart = headerParts[i];
                MessageHeaderDescription headerDescription = (MessageHeaderDescription)headerPart.Description;
                object headerValue = parameters[headerDescription.Index];

                if (headerDescription.Multiple)
                {
                    if (headerValue != null)
                    {
                        bool isXmlElement = headerDescription.Type == typeof(XmlElement);
                        foreach (object headerItemValue in (IEnumerable)headerValue)
                            AddMessageHeaderForParameter(headers, headerPart, message.Version, headerItemValue, isXmlElement);
                    }
                }
                else
                    AddMessageHeaderForParameter(headers, headerPart, message.Version, headerValue, false/*isXmlElement*/);
            }
        }

        void AddMessageHeaderForParameter(MessageHeaders headers, PartInfo headerPart, MessageVersion messageVersion, object parameterValue, bool isXmlElement)
        {
            string actor;
            bool mustUnderstand;
            bool relay;
            MessageHeaderDescription headerDescription = (MessageHeaderDescription)headerPart.Description;
            object valueToSerialize = GetContentOfMessageHeaderOfT(headerDescription, parameterValue, out mustUnderstand, out relay, out actor);

            if (isXmlElement)
            {
                if (valueToSerialize == null)
                    return;
                XmlElement xmlElement = (XmlElement)valueToSerialize;
                headers.Add(new XmlElementMessageHeader(this, messageVersion, xmlElement.LocalName, xmlElement.NamespaceURI, mustUnderstand, actor, relay, xmlElement));
                return;
            }
            headers.Add(new DataContractSerializerMessageHeader(headerPart, valueToSerialize, mustUnderstand, actor, relay));
        }

        protected override void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, string action, MessageDescription messageDescription, object returnValue, object[] parameters, bool isRequest)
        {
            if (writer == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            if (parameters == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));

            MessageInfo messageInfo;
            if (isRequest)
                messageInfo = requestMessageInfo;
            else
                messageInfo = replyMessageInfo;
            if (messageInfo.WrapperName != null)
                writer.WriteStartElement(messageInfo.WrapperName, messageInfo.WrapperNamespace);
            if (messageInfo.ReturnPart != null)
                SerializeParameter(writer, messageInfo.ReturnPart, returnValue);
            SerializeParameters(writer, messageInfo.BodyParts, parameters);
            if (messageInfo.WrapperName != null)
                writer.WriteEndElement();
        }

        void SerializeParameters(XmlDictionaryWriter writer, PartInfo[] parts, object[] parameters)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                PartInfo part = parts[i];
                object graph = parameters[part.Description.Index];
                SerializeParameter(writer, part, graph);
            }
        }

        void SerializeParameter(XmlDictionaryWriter writer, PartInfo part, object graph)
        {
            if (part.Description.Multiple)
            {
                if (graph != null)
                {
                    foreach (object item in (IEnumerable)graph)
                        SerializeParameterPart(writer, part, item);
                }
            }
            else
                SerializeParameterPart(writer, part, graph);
        }

        void SerializeParameterPart(XmlDictionaryWriter writer, PartInfo part, object graph)
        {
            try
            {
                part.Serializer.WriteObject(writer, graph);
            }
            catch (SerializationException sx)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxInvalidMessageBodyErrorSerializingParameter, part.Description.Namespace, part.Description.Name, sx.Message), sx));
            }
        }

        protected override void GetHeadersFromMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            MessageInfo messageInfo = isRequest ? requestMessageInfo : replyMessageInfo;
            if (!messageInfo.AnyHeaders)
                return;
            MessageHeaders headers = message.Headers;
            KeyValuePair<Type, ArrayList>[] multipleHeaderValues = null;
            ArrayList elementList = null;
            if (messageInfo.UnknownHeaderDescription != null)
                elementList = new ArrayList();

            for (int i = 0; i < headers.Count; i++)
            {
                MessageHeaderInfo header = headers[i];
                MessageHeaderDescription headerDescription = messageInfo.HeaderDescriptionTable.Get(header.Name, header.Namespace);
                if (headerDescription != null)
                {
                    if (header.MustUnderstand)
                        headers.UnderstoodHeaders.Add(header);

                    object item = null;
                    XmlDictionaryReader headerReader = headers.GetReaderAtHeader(i);
                    try
                    {
                        object dataValue = DeserializeHeaderContents(headerReader, messageDescription, headerDescription);
                        if (headerDescription.TypedHeader)
                            item = TypedHeaderManager.Create(headerDescription.Type, dataValue, headers[i].MustUnderstand, headers[i].Relay, headers[i].Actor);
                        else
                            item = dataValue;
                    }
                    finally
                    {
                        headerReader.Close();
                    }

                    if (headerDescription.Multiple)
                    {
                        if (multipleHeaderValues == null)
                            multipleHeaderValues = new KeyValuePair<Type, ArrayList>[parameters.Length];
                        if (multipleHeaderValues[headerDescription.Index].Key == null)
                        {
                            multipleHeaderValues[headerDescription.Index] = new KeyValuePair<System.Type, System.Collections.ArrayList>(headerDescription.TypedHeader ? TypedHeaderManager.GetMessageHeaderType(headerDescription.Type) : headerDescription.Type, new ArrayList());
                        }
                        multipleHeaderValues[headerDescription.Index].Value.Add(item);
                    }
                    else
                        parameters[headerDescription.Index] = item;
                }
                else if (messageInfo.UnknownHeaderDescription != null)
                {
                    MessageHeaderDescription unknownHeaderDescription = messageInfo.UnknownHeaderDescription;
                    XmlDictionaryReader headerReader = headers.GetReaderAtHeader(i);
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        object dataValue = doc.ReadNode(headerReader);
                        if (dataValue != null && unknownHeaderDescription.TypedHeader)
                            dataValue = TypedHeaderManager.Create(unknownHeaderDescription.Type, dataValue, headers[i].MustUnderstand, headers[i].Relay, headers[i].Actor);
                        elementList.Add(dataValue);
                    }
                    finally
                    {
                        headerReader.Close();
                    }
                }
            }
            if (multipleHeaderValues != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (multipleHeaderValues[i].Key != null)
                        parameters[i] = multipleHeaderValues[i].Value.ToArray(multipleHeaderValues[i].Key);
                }
            }
            if (messageInfo.UnknownHeaderDescription != null)
                parameters[messageInfo.UnknownHeaderDescription.Index] = elementList.ToArray(messageInfo.UnknownHeaderDescription.TypedHeader ? typeof(MessageHeader<XmlElement>) : typeof(XmlElement));
        }

        object DeserializeHeaderContents(XmlDictionaryReader reader, MessageDescription messageDescription, MessageHeaderDescription headerDescription)
        {
            bool isQueryable;
            Type dataContractType = DataContractSerializerOperationFormatter.GetSubstituteDataContractType(headerDescription.Type, out isQueryable);
            XmlObjectSerializer serializerLocal = serializerFactory.CreateSerializer(dataContractType, headerDescription.Name, headerDescription.Namespace, this.knownTypes);
            object val = serializerLocal.ReadObject(reader);
            if (isQueryable && val != null)
            {
                return Queryable.AsQueryable((IEnumerable)val);
            }
            return val;
        }

        protected override object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, string action, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            if (reader == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            if (parameters == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));

            MessageInfo messageInfo;
            if (isRequest)
                messageInfo = requestMessageInfo;
            else
                messageInfo = replyMessageInfo;

            if (messageInfo.WrapperName != null)
            {
                if (!reader.IsStartElement(messageInfo.WrapperName, messageInfo.WrapperNamespace))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.SFxInvalidMessageBody, messageInfo.WrapperName, messageInfo.WrapperNamespace, reader.NodeType, reader.Name, reader.NamespaceURI)));
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (isEmptyElement)
                    return null;
            }
            object returnValue = null;
            if (messageInfo.ReturnPart != null)
            {
                while (true)
                {
                    PartInfo part = messageInfo.ReturnPart;
                    if (part.Serializer.IsStartObject(reader))
                    {
                        returnValue = DeserializeParameter(reader, part, isRequest);
                        break;
                    }
                    if (!reader.IsStartElement())
                        break;
                    OperationFormatter.TraceAndSkipElement(reader);
                }
            }
            DeserializeParameters(reader, messageInfo.BodyParts, parameters, isRequest);
            if (messageInfo.WrapperName != null)
                reader.ReadEndElement();
            return returnValue;
        }

        void DeserializeParameters(XmlDictionaryReader reader, PartInfo[] parts, object[] parameters, bool isRequest)
        {
            int nextPartIndex = 0;
            while (reader.IsStartElement())
            {
                for (int i = nextPartIndex; i < parts.Length; i++)
                {
                    PartInfo part = parts[i];
                    if (part.Serializer.IsStartObject(reader))
                    {
                        object parameterValue = DeserializeParameter(reader, part, isRequest);
                        parameters[part.Description.Index] = parameterValue;
                        nextPartIndex = i + 1;
                    }
                    else
                        parameters[part.Description.Index] = null;
                }

                if (reader.IsStartElement())
                    OperationFormatter.TraceAndSkipElement(reader);
            }
        }

        object DeserializeParameter(XmlDictionaryReader reader, PartInfo part, bool isRequest)
        {
            if (part.Description.Multiple)
            {
                ArrayList items = new ArrayList();
                while (part.Serializer.IsStartObject(reader))
                    items.Add(DeserializeParameterPart(reader, part, isRequest));
                return items.ToArray(part.Description.Type);
            }
            return DeserializeParameterPart(reader, part, isRequest);
        }

        object DeserializeParameterPart(XmlDictionaryReader reader, PartInfo part, bool isRequest)
        {
            object val;
            try
            {
                val = part.ReadObject(reader);
            }
            catch (System.InvalidOperationException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
            }
            catch (System.Runtime.Serialization.InvalidDataContractException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(
                    SR.GetString(SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
            }
            catch (System.FormatException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    OperationFormatter.CreateDeserializationFailedFault(
                        SR.GetString(SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                                     part.Description.Namespace, part.Description.Name, e.Message), 
                                     e));
            }
            catch (System.Runtime.Serialization.SerializationException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    OperationFormatter.CreateDeserializationFailedFault(
                        SR.GetString(SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                                     part.Description.Namespace, part.Description.Name, e.Message), 
                                     e));
            }

            return val;
        }

        internal static Type GetSubstituteDataContractType(Type type, out bool isQueryable)
        {
            if (type == typeOfIQueryable)
            {
                isQueryable = true;
                return typeOfIEnumerable;
            }

            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeOfIQueryableGeneric)
            {
                isQueryable = true;
                return typeOfIEnumerableGeneric.MakeGenericType(type.GetGenericArguments());
            }

            isQueryable = false;
            return type;
        }


        class DataContractSerializerMessageHeader : XmlObjectSerializerHeader
        {
            PartInfo headerPart;

            public DataContractSerializerMessageHeader(PartInfo headerPart, object headerValue, bool mustUnderstand, string actor, bool relay)
                : base(headerPart.DictionaryName.Value, headerPart.DictionaryNamespace.Value, headerValue, headerPart.Serializer, mustUnderstand, actor ?? string.Empty, relay)
            {
                this.headerPart = headerPart;
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                //Prefix needed since there may be xsi:type attribute at toplevel with qname value where ns = ""
                string prefix = (this.Namespace == null || this.Namespace.Length == 0) ? string.Empty : "h";
                writer.WriteStartElement(prefix, headerPart.DictionaryName, headerPart.DictionaryNamespace);
                WriteHeaderAttributes(writer, messageVersion);
            }
        }


        protected class MessageInfo
        {
            internal PartInfo[] HeaderParts;
            internal XmlDictionaryString WrapperName;
            internal XmlDictionaryString WrapperNamespace;
            internal PartInfo[] BodyParts;
            internal PartInfo ReturnPart;
            internal MessageHeaderDescriptionTable HeaderDescriptionTable;
            internal MessageHeaderDescription UnknownHeaderDescription;
            internal bool AnyHeaders;
        }

        protected class PartInfo
        {
            XmlDictionaryString dictionaryName;
            XmlDictionaryString dictionaryNamespace;
            MessagePartDescription description;
            XmlObjectSerializer serializer;
            IList<Type> knownTypes;
            DataContractSerializerOperationBehavior serializerFactory;
            Type contractType;
            bool isQueryable;

            public PartInfo(MessagePartDescription description, XmlDictionaryString dictionaryName, XmlDictionaryString dictionaryNamespace,
                IList<Type> knownTypes, DataContractSerializerOperationBehavior behavior)
            {
                this.dictionaryName = dictionaryName;
                this.dictionaryNamespace = dictionaryNamespace;
                this.description = description;
                this.knownTypes = knownTypes;
                this.serializerFactory = behavior;

                this.contractType = DataContractSerializerOperationFormatter.GetSubstituteDataContractType(description.Type, out this.isQueryable);
            }

            public Type ContractType
            {
                get { return this.contractType; }
            }

            public MessagePartDescription Description
            {
                get { return description; }
            }

            public XmlDictionaryString DictionaryName
            {
                get { return dictionaryName; }
            }

            public XmlDictionaryString DictionaryNamespace
            {
                get { return dictionaryNamespace; }
            }

            public XmlObjectSerializer Serializer
            {
                get
                {
                    if (serializer == null)
                    {
                        serializer = serializerFactory.CreateSerializer(contractType, DictionaryName, DictionaryNamespace, knownTypes);
                    }
                    return serializer;
                }
            }

            public object ReadObject(XmlDictionaryReader reader)
            {
                return this.ReadObject(reader, this.Serializer);
            }

            public object ReadObject(XmlDictionaryReader reader, XmlObjectSerializer serializer)
            {
                object val = this.serializer.ReadObject(reader, false /* verifyObjectName */);
                if (this.isQueryable && val != null)
                {
                    return Queryable.AsQueryable((IEnumerable)val);
                }
                return val;
            }
        }
    }
}
