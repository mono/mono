//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.Xml;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System.Runtime;

    class PrimitiveOperationFormatter : IClientMessageFormatter, IDispatchMessageFormatter
    {
        OperationDescription operation;
        MessageDescription responseMessage;
        MessageDescription requestMessage;
        XmlDictionaryString action;
        XmlDictionaryString replyAction;
        ActionHeader actionHeaderNone;
        ActionHeader actionHeader10;
        ActionHeader actionHeaderAugust2004;
        ActionHeader replyActionHeaderNone;
        ActionHeader replyActionHeader10;
        ActionHeader replyActionHeaderAugust2004;
        XmlDictionaryString requestWrapperName;
        XmlDictionaryString requestWrapperNamespace;
        XmlDictionaryString responseWrapperName;
        XmlDictionaryString responseWrapperNamespace;
        PartInfo[] requestParts;
        PartInfo[] responseParts;
        PartInfo returnPart;
        XmlDictionaryString xsiNilLocalName;
        XmlDictionaryString xsiNilNamespace;

        public PrimitiveOperationFormatter(OperationDescription description, bool isRpc)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            OperationFormatter.Validate(description, isRpc, false/*isEncoded*/);

            this.operation = description;
#pragma warning suppress 56506 // Microsoft, OperationDescription.Messages never be null
            this.requestMessage = description.Messages[0];
            if (description.Messages.Count == 2)
                this.responseMessage = description.Messages[1];

            int stringCount = 3 + requestMessage.Body.Parts.Count;
            if (responseMessage != null)
                stringCount += 2 + responseMessage.Body.Parts.Count;

            XmlDictionary dictionary = new XmlDictionary(stringCount * 2);

            xsiNilLocalName = dictionary.Add("nil");
            xsiNilNamespace = dictionary.Add(System.Xml.Schema.XmlSchema.InstanceNamespace);

            OperationFormatter.GetActions(description, dictionary, out this.action, out this.replyAction);

            if (requestMessage.Body.WrapperName != null)
            {
                requestWrapperName = AddToDictionary(dictionary, requestMessage.Body.WrapperName);
                requestWrapperNamespace = AddToDictionary(dictionary, requestMessage.Body.WrapperNamespace);
            }

            requestParts = AddToDictionary(dictionary, requestMessage.Body.Parts, isRpc);

            if (responseMessage != null)
            {
                if (responseMessage.Body.WrapperName != null)
                {
                    responseWrapperName = AddToDictionary(dictionary, responseMessage.Body.WrapperName);
                    responseWrapperNamespace = AddToDictionary(dictionary, responseMessage.Body.WrapperNamespace);
                }

                responseParts = AddToDictionary(dictionary, responseMessage.Body.Parts, isRpc);

                if (responseMessage.Body.ReturnValue != null && responseMessage.Body.ReturnValue.Type != typeof(void))
                {
                    returnPart = AddToDictionary(dictionary, responseMessage.Body.ReturnValue, isRpc);
                }
            }
        }

        ActionHeader ActionHeaderNone
        {
            get
            {
                if (actionHeaderNone == null)
                {
                    actionHeaderNone =
                        ActionHeader.Create(this.action, AddressingVersion.None);
                }

                return actionHeaderNone;
            }
        }

        ActionHeader ActionHeader10
        {
            get
            {
                if (actionHeader10 == null)
                {
                    actionHeader10 =
                        ActionHeader.Create(this.action, AddressingVersion.WSAddressing10);
                }

                return actionHeader10;
            }
        }

        ActionHeader ActionHeaderAugust2004
        {
            get
            {
                if (actionHeaderAugust2004 == null)
                {
                    actionHeaderAugust2004 =
                        ActionHeader.Create(this.action, AddressingVersion.WSAddressingAugust2004);
                }

                return actionHeaderAugust2004;
            }
        }

        ActionHeader ReplyActionHeaderNone
        {
            get
            {
                if (replyActionHeaderNone == null)
                {
                    replyActionHeaderNone =
                        ActionHeader.Create(this.replyAction, AddressingVersion.None);
                }

                return replyActionHeaderNone;
            }
        }

        ActionHeader ReplyActionHeader10
        {
            get
            {
                if (replyActionHeader10 == null)
                {
                    replyActionHeader10 =
                        ActionHeader.Create(this.replyAction, AddressingVersion.WSAddressing10);
                }

                return replyActionHeader10;
            }
        }

        ActionHeader ReplyActionHeaderAugust2004
        {
            get
            {
                if (replyActionHeaderAugust2004 == null)
                {
                    replyActionHeaderAugust2004 =
                        ActionHeader.Create(this.replyAction, AddressingVersion.WSAddressingAugust2004);
                }

                return replyActionHeaderAugust2004;
            }
        }

        static XmlDictionaryString AddToDictionary(XmlDictionary dictionary, string s)
        {
            XmlDictionaryString dictionaryString;
            if (!dictionary.TryLookup(s, out dictionaryString))
            {
                dictionaryString = dictionary.Add(s);
            }
            return dictionaryString;
        }

        static PartInfo[] AddToDictionary(XmlDictionary dictionary, MessagePartDescriptionCollection parts, bool isRpc)
        {
            PartInfo[] partInfos = new PartInfo[parts.Count];
            for (int i = 0; i < parts.Count; i++)
            {
                partInfos[i] = AddToDictionary(dictionary, parts[i], isRpc);
            }
            return partInfos;
        }

        ActionHeader GetActionHeader(AddressingVersion addressing)
        {
            if (this.action == null)
            {
                return null;
            }

            if (addressing == AddressingVersion.WSAddressingAugust2004)
            {
                return ActionHeaderAugust2004;
            }
            else if (addressing == AddressingVersion.WSAddressing10)
            {
                return ActionHeader10;
            }
            else if (addressing == AddressingVersion.None)
            {
                return ActionHeaderNone;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.AddressingVersionNotSupported, addressing)));
            }
        }

        ActionHeader GetReplyActionHeader(AddressingVersion addressing)
        {
            if (this.replyAction == null)
            {
                return null;
            }

            if (addressing == AddressingVersion.WSAddressingAugust2004)
            {
                return ReplyActionHeaderAugust2004;
            }
            else if (addressing == AddressingVersion.WSAddressing10)
            {
                return ReplyActionHeader10;
            }
            else if (addressing == AddressingVersion.None)
            {
                return ReplyActionHeaderNone;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.AddressingVersionNotSupported, addressing)));
            }
        }

        static string GetArrayItemName(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "boolean";
                case TypeCode.DateTime:
                    return "dateTime";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.Single:
                    return "float";
                case TypeCode.Double:
                    return "double";
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidUseOfPrimitiveOperationFormatter)));
            }
        }

        static PartInfo AddToDictionary(XmlDictionary dictionary, MessagePartDescription part, bool isRpc)
        {
            Type type = part.Type;
            XmlDictionaryString itemName = null;
            XmlDictionaryString itemNamespace = null;
            if (type.IsArray && type != typeof(byte[]))
            {
                const string ns = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
                string name = GetArrayItemName(type.GetElementType());
                itemName = AddToDictionary(dictionary, name);
                itemNamespace = AddToDictionary(dictionary, ns);
            }
            return new PartInfo(part,
                AddToDictionary(dictionary, part.Name),
                AddToDictionary(dictionary, isRpc ? string.Empty : part.Namespace),
                itemName, itemNamespace);
        }

        public static bool IsContractSupported(OperationDescription description)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            OperationDescription operation = description;
#pragma warning suppress 56506 // Microsoft, OperationDescription.Messages never be null
            MessageDescription requestMessage = description.Messages[0];
            MessageDescription responseMessage = null;
            if (description.Messages.Count == 2)
                responseMessage = description.Messages[1];

            if (requestMessage.Headers.Count > 0)
                return false;
            if (requestMessage.Properties.Count > 0)
                return false;
            if (requestMessage.IsTypedMessage)
                return false;
            if (responseMessage != null)
            {
                if (responseMessage.Headers.Count > 0)
                    return false;
                if (responseMessage.Properties.Count > 0)
                    return false;
                if (responseMessage.IsTypedMessage)
                    return false;
            }
            if (!AreTypesSupported(requestMessage.Body.Parts))
                return false;
            if (responseMessage != null)
            {
                if (!AreTypesSupported(responseMessage.Body.Parts))
                    return false;
                if (responseMessage.Body.ReturnValue != null && !IsTypeSupported(responseMessage.Body.ReturnValue))
                    return false;
            }
            return true;
        }

        static bool AreTypesSupported(MessagePartDescriptionCollection bodyDescriptions)
        {
            for (int i = 0; i < bodyDescriptions.Count; i++)
                if (!IsTypeSupported(bodyDescriptions[i]))
                    return false;
            return true;
        }

        static bool IsTypeSupported(MessagePartDescription bodyDescription)
        {
            Fx.Assert(bodyDescription != null, "");
            Type type = bodyDescription.Type;
            if (type == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMessagePartDescriptionMissingType, bodyDescription.Name, bodyDescription.Namespace)));

            if (bodyDescription.Multiple)
                return false;

            if (type == typeof(void))
                return true;
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.String:
                    return true;
                case TypeCode.Object:
                    if (type.IsArray && type.GetArrayRank() == 1 && IsArrayTypeSupported(type.GetElementType()))
                        return true;
                    break;
                default:
                    break;
            }
            return false;
        }

        static bool IsArrayTypeSupported(Type type)
        {
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Boolean:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            if (messageVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");

            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");

            return Message.CreateMessage(messageVersion, GetActionHeader(messageVersion.Addressing), new PrimitiveRequestBodyWriter(parameters, this));
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            if (messageVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");

            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");

            return Message.CreateMessage(messageVersion, GetReplyActionHeader(messageVersion.Addressing), new PrimitiveResponseBodyWriter(parameters, result, this));
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            if (parameters == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("parameters"), message);
            try
            {
                if (message.IsEmpty)
                {
                    if (responseWrapperName == null)
                        return null;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.SFxInvalidMessageBodyEmptyMessage)));
                }

                XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents();
                using (bodyReader)
                {
                    object returnValue = DeserializeResponse(bodyReader, parameters);
                    message.ReadFromBodyContentsToEnd(bodyReader);
                    return returnValue;
                }
            }
            catch (XmlException xe)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorDeserializingReplyBodyMore, operation.Name, xe.Message), xe));
            }
            catch (FormatException fe)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorDeserializingReplyBodyMore, operation.Name, fe.Message), fe));
            }
            catch (SerializationException se)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorDeserializingReplyBodyMore, operation.Name, se.Message), se));
            }
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            if (parameters == null)
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("parameters"), message);
            try
            {
                if (message.IsEmpty)
                {
                    if (requestWrapperName == null)
                        return;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.SFxInvalidMessageBodyEmptyMessage)));
                }

                XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents();
                using (bodyReader)
                {
                    DeserializeRequest(bodyReader, parameters);
                    message.ReadFromBodyContentsToEnd(bodyReader);
                }
            }
            catch (XmlException xe)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    OperationFormatter.CreateDeserializationFailedFault(
                        SR.GetString(SR.SFxErrorDeserializingRequestBodyMore, operation.Name, xe.Message), 
                        xe));
            }
            catch (FormatException fe)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    OperationFormatter.CreateDeserializationFailedFault(
                        SR.GetString(SR.SFxErrorDeserializingRequestBodyMore, operation.Name, fe.Message), 
                        fe));
            }
            catch (SerializationException se)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorDeserializingRequestBodyMore, operation.Name, se.Message), 
                    se));
            }
        }

        void DeserializeRequest(XmlDictionaryReader reader, object[] parameters)
        {
            if (requestWrapperName != null)
            {
                if (!reader.IsStartElement(requestWrapperName, requestWrapperNamespace))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.SFxInvalidMessageBody, requestWrapperName, requestWrapperNamespace, reader.NodeType, reader.Name, reader.NamespaceURI)));
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (isEmptyElement)
                {
                    return;
                }
            }

            DeserializeParameters(reader, requestParts, parameters);

            if (requestWrapperName != null)
            {
                reader.ReadEndElement();
            }
        }

        object DeserializeResponse(XmlDictionaryReader reader, object[] parameters)
        {
            if (responseWrapperName != null)
            {
                if (!reader.IsStartElement(responseWrapperName, responseWrapperNamespace))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.SFxInvalidMessageBody, responseWrapperName, responseWrapperNamespace, reader.NodeType, reader.Name, reader.NamespaceURI)));
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (isEmptyElement)
                {
                    return null;
                }
            }

            object returnValue = null;
            if (returnPart != null)
            {
                while (true)
                {
                    if (IsPartElement(reader, returnPart))
                    {
                        returnValue = DeserializeParameter(reader, returnPart);
                        break;
                    }
                    if (!reader.IsStartElement())
                        break;
                    if (IsPartElements(reader, responseParts))
                        break;
                    OperationFormatter.TraceAndSkipElement(reader);
                }
            }
            DeserializeParameters(reader, responseParts, parameters);

            if (responseWrapperName != null)
            {
                reader.ReadEndElement();
            }

            return returnValue;
        }


        void DeserializeParameters(XmlDictionaryReader reader, PartInfo[] parts, object[] parameters)
        {
            if (parts.Length != parameters.Length)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentException(SR.GetString(SR.SFxParameterCountMismatch, "parts", parts.Length, "parameters", parameters.Length), "parameters"));

            int nextPartIndex = 0;
            while (reader.IsStartElement())
            {
                for (int i = nextPartIndex; i < parts.Length; i++)
                {
                    PartInfo part = parts[i];
                    if (IsPartElement(reader, part))
                    {
                        parameters[part.Description.Index] = DeserializeParameter(reader, parts[i]);
                        nextPartIndex = i + 1;
                    }
                    else
                        parameters[part.Description.Index] = null;
                }

                if (reader.IsStartElement())
                    OperationFormatter.TraceAndSkipElement(reader);
            }
        }

        private bool IsPartElements(XmlDictionaryReader reader, PartInfo[] parts)
        {
            foreach (PartInfo part in parts)
                if (IsPartElement(reader, part))
                    return true;
            return false;
        }

        bool IsPartElement(XmlDictionaryReader reader, PartInfo part)
        {
            return reader.IsStartElement(part.DictionaryName, part.DictionaryNamespace);
        }

        object DeserializeParameter(XmlDictionaryReader reader, PartInfo part)
        {
            if (reader.AttributeCount > 0 &&
                reader.MoveToAttribute(xsiNilLocalName.Value, xsiNilNamespace.Value) &&
                reader.ReadContentAsBoolean())
            {
                reader.Skip();
                return null;
            }
            return part.ReadValue(reader);
        }

        void SerializeParameter(XmlDictionaryWriter writer, PartInfo part, object graph)
        {

            writer.WriteStartElement(part.DictionaryName, part.DictionaryNamespace);
            if (graph == null)
            {
                writer.WriteStartAttribute(xsiNilLocalName, xsiNilNamespace);
                writer.WriteValue(true);
                writer.WriteEndAttribute();
            }
            else
                part.WriteValue(writer, graph);
            writer.WriteEndElement();
        }

        void SerializeParameters(XmlDictionaryWriter writer, PartInfo[] parts, object[] parameters)
        {
            if (parts.Length != parameters.Length)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentException(SR.GetString(SR.SFxParameterCountMismatch, "parts", parts.Length, "parameters", parameters.Length), "parameters"));


            for (int i = 0; i < parts.Length; i++)
            {
                PartInfo part = parts[i];
                SerializeParameter(writer, part, parameters[part.Description.Index]);
            }
        }

        void SerializeRequest(XmlDictionaryWriter writer, object[] parameters)
        {
            if (requestWrapperName != null)
                writer.WriteStartElement(requestWrapperName, requestWrapperNamespace);

            SerializeParameters(writer, requestParts, parameters);

            if (requestWrapperName != null)
                writer.WriteEndElement();
        }

        void SerializeResponse(XmlDictionaryWriter writer, object returnValue, object[] parameters)
        {
            if (responseWrapperName != null)
                writer.WriteStartElement(responseWrapperName, responseWrapperNamespace);

            if (returnPart != null)
                SerializeParameter(writer, returnPart, returnValue);

            SerializeParameters(writer, responseParts, parameters);

            if (responseWrapperName != null)
                writer.WriteEndElement();
        }

        class PartInfo
        {
            XmlDictionaryString dictionaryName;
            XmlDictionaryString dictionaryNamespace;
            XmlDictionaryString itemName;
            XmlDictionaryString itemNamespace;
            MessagePartDescription description;
            TypeCode typeCode;
            bool isArray;

            public PartInfo(MessagePartDescription description, XmlDictionaryString dictionaryName, XmlDictionaryString dictionaryNamespace, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
            {
                this.dictionaryName = dictionaryName;
                this.dictionaryNamespace = dictionaryNamespace;
                this.itemName = itemName;
                this.itemNamespace = itemNamespace;
                this.description = description;
                if (description.Type.IsArray)
                {
                    this.isArray = true;
                    this.typeCode = Type.GetTypeCode(description.Type.GetElementType());
                }
                else
                {
                    this.isArray = false;
                    this.typeCode = Type.GetTypeCode(description.Type);
                }
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

            public object ReadValue(XmlDictionaryReader reader)
            {
                object value;
                if (isArray)
                {
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            value = reader.ReadElementContentAsBase64();
                            break;
                        case TypeCode.Boolean:
                            if (!reader.IsEmptyElement)
                            {
                                reader.ReadStartElement();
                                value = reader.ReadBooleanArray(itemName, itemNamespace);
                                reader.ReadEndElement();
                            }
                            else
                            {
                                reader.Read();
                                value = new bool[0];
                            }
                            break;
                        case TypeCode.DateTime:
                            if (!reader.IsEmptyElement)
                            {
                                reader.ReadStartElement();
                                value = reader.ReadDateTimeArray(itemName, itemNamespace);
                                reader.ReadEndElement();
                            }
                            else
                            {
                                reader.Read();
                                value = new DateTime[0];
                            }
                            break;
                        case TypeCode.Decimal:
                            if (!reader.IsEmptyElement)
                            {
                                reader.ReadStartElement();
                                value = reader.ReadDecimalArray(itemName, itemNamespace);
                                reader.ReadEndElement();
                            }
                            else
                            {
                                reader.Read();
                                value = new Decimal[0];
                            }
                            break;
                        case TypeCode.Int32:
                            if (!reader.IsEmptyElement)
                            {
                                reader.ReadStartElement();
                                value = reader.ReadInt32Array(itemName, itemNamespace);
                                reader.ReadEndElement();
                            }
                            else
                            {
                                reader.Read();
                                value = new Int32[0];
                            }
                            break;
                        case TypeCode.Int64:
                            if (!reader.IsEmptyElement)
                            {
                                reader.ReadStartElement();
                                value = reader.ReadInt64Array(itemName, itemNamespace);
                                reader.ReadEndElement();
                            }
                            else
                            {
                                reader.Read();
                                value = new Int64[0];
                            }
                            break;
                        case TypeCode.Single:
                            if (!reader.IsEmptyElement)
                            {
                                reader.ReadStartElement();
                                value = reader.ReadSingleArray(itemName, itemNamespace);
                                reader.ReadEndElement();
                            }
                            else
                            {
                                reader.Read();
                                value = new Single[0];
                            }
                            break;
                        case TypeCode.Double:
                            if (!reader.IsEmptyElement)
                            {
                                reader.ReadStartElement();
                                value = reader.ReadDoubleArray(itemName, itemNamespace);
                                reader.ReadEndElement();
                            }
                            else
                            {
                                reader.Read();
                                value = new Double[0];
                            }
                            break;
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidUseOfPrimitiveOperationFormatter)));
                    }
                }
                else
                {
                    switch (typeCode)
                    {
                        case TypeCode.Boolean:
                            value = reader.ReadElementContentAsBoolean();
                            break;
                        case TypeCode.DateTime:
                            value = reader.ReadElementContentAsDateTime();
                            break;
                        case TypeCode.Decimal:
                            value = reader.ReadElementContentAsDecimal();
                            break;
                        case TypeCode.Double:
                            value = reader.ReadElementContentAsDouble();
                            break;
                        case TypeCode.Int32:
                            value = reader.ReadElementContentAsInt();
                            break;
                        case TypeCode.Int64:
                            value = reader.ReadElementContentAsLong();
                            break;
                        case TypeCode.Single:
                            value = reader.ReadElementContentAsFloat();
                            break;
                        case TypeCode.String:
                            return reader.ReadElementContentAsString();
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidUseOfPrimitiveOperationFormatter)));
                    }
                }
                return value;
            }

            public void WriteValue(XmlDictionaryWriter writer, object value)
            {
                if (isArray)
                {
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            {
                                byte[] arrayValue = (byte[])value;
                                writer.WriteBase64(arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        case TypeCode.Boolean:
                            {
                                bool[] arrayValue = (bool[])value;
                                writer.WriteArray(null, itemName, itemNamespace, arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        case TypeCode.DateTime:
                            {
                                DateTime[] arrayValue = (DateTime[])value;
                                writer.WriteArray(null, itemName, itemNamespace, arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        case TypeCode.Decimal:
                            {
                                decimal[] arrayValue = (decimal[])value;
                                writer.WriteArray(null, itemName, itemNamespace, arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        case TypeCode.Int32:
                            {
                                Int32[] arrayValue = (Int32[])value;
                                writer.WriteArray(null, itemName, itemNamespace, arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        case TypeCode.Int64:
                            {
                                Int64[] arrayValue = (Int64[])value;
                                writer.WriteArray(null, itemName, itemNamespace, arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        case TypeCode.Single:
                            {
                                float[] arrayValue = (float[])value;
                                writer.WriteArray(null, itemName, itemNamespace, arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        case TypeCode.Double:
                            {
                                double[] arrayValue = (double[])value;
                                writer.WriteArray(null, itemName, itemNamespace, arrayValue, 0, arrayValue.Length);
                            }
                            break;
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidUseOfPrimitiveOperationFormatter)));
                    }
                }
                else
                {
                    switch (typeCode)
                    {
                        case TypeCode.Boolean:
                            writer.WriteValue((bool)value);
                            break;
                        case TypeCode.DateTime:
                            writer.WriteValue((DateTime)value);
                            break;
                        case TypeCode.Decimal:
                            writer.WriteValue((Decimal)value);
                            break;
                        case TypeCode.Double:
                            writer.WriteValue((double)value);
                            break;
                        case TypeCode.Int32:
                            writer.WriteValue((int)value);
                            break;
                        case TypeCode.Int64:
                            writer.WriteValue((long)value);
                            break;
                        case TypeCode.Single:
                            writer.WriteValue((float)value);
                            break;
                        case TypeCode.String:
                            writer.WriteString((string)value);
                            break;
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidUseOfPrimitiveOperationFormatter)));
                    }
                }
            }
        }

        class PrimitiveRequestBodyWriter : BodyWriter
        {
            object[] parameters;
            PrimitiveOperationFormatter primitiveOperationFormatter;

            public PrimitiveRequestBodyWriter(object[] parameters, PrimitiveOperationFormatter primitiveOperationFormatter)
                : base(true)
            {
                this.parameters = parameters;
                this.primitiveOperationFormatter = primitiveOperationFormatter;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                primitiveOperationFormatter.SerializeRequest(writer, parameters);
            }
        }

        class PrimitiveResponseBodyWriter : BodyWriter
        {
            object[] parameters;
            object returnValue;
            PrimitiveOperationFormatter primitiveOperationFormatter;

            public PrimitiveResponseBodyWriter(object[] parameters, object returnValue,
                PrimitiveOperationFormatter primitiveOperationFormatter)
                : base(true)
            {
                this.parameters = parameters;
                this.returnValue = returnValue;
                this.primitiveOperationFormatter = primitiveOperationFormatter;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                primitiveOperationFormatter.SerializeResponse(writer, returnValue, parameters);
            }
        }
    }
}
