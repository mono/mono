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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;
    using System.ServiceModel.Diagnostics;
    using System.IO;
    using System.Xml.Schema;

    class XmlSerializerOperationFormatter : OperationFormatter
    {
        const string soap11Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
        const string soap12Encoding = "http://www.w3.org/2003/05/soap-encoding";

        bool isEncoded;

        MessageInfo requestMessageInfo;
        MessageInfo replyMessageInfo;

        public XmlSerializerOperationFormatter(OperationDescription description, XmlSerializerFormatAttribute xmlSerializerFormatAttribute,
            MessageInfo requestMessageInfo, MessageInfo replyMessageInfo) :
            base(description, xmlSerializerFormatAttribute.Style == OperationFormatStyle.Rpc, xmlSerializerFormatAttribute.IsEncoded)
        {
            if (xmlSerializerFormatAttribute.IsEncoded && xmlSerializerFormatAttribute.Style != OperationFormatStyle.Rpc)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDocEncodedNotSupported, description.Name)));
            this.isEncoded = xmlSerializerFormatAttribute.IsEncoded;

            this.requestMessageInfo = requestMessageInfo;
            this.replyMessageInfo = replyMessageInfo;
        }

        protected override void AddHeadersToMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            XmlSerializer serializer;
            MessageHeaderDescriptionTable headerDescriptionTable;
            MessageHeaderDescription unknownHeaderDescription;
            bool mustUnderstand;
            bool relay;
            string actor;
            try
            {
                if (isRequest)
                {
                    serializer = requestMessageInfo.HeaderSerializer;
                    headerDescriptionTable = requestMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = requestMessageInfo.UnknownHeaderDescription;
                }
                else
                {
                    serializer = replyMessageInfo.HeaderSerializer;
                    headerDescriptionTable = replyMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = replyMessageInfo.UnknownHeaderDescription;
                }
                if (serializer != null)
                {
                    object[] headerValues = new object[headerDescriptionTable.Count];
                    MessageHeaderOfTHelper messageHeaderOfTHelper = null;
                    int headerIndex = 0;

                    foreach (MessageHeaderDescription headerDescription in messageDescription.Headers)
                    {
                        object parameterValue = parameters[headerDescription.Index];
                        if (!headerDescription.IsUnknownHeaderCollection)
                        {
                            if (headerDescription.TypedHeader)
                            {
                                if (messageHeaderOfTHelper == null)
                                    messageHeaderOfTHelper = new MessageHeaderOfTHelper(parameters.Length);
                                headerValues[headerIndex++] = messageHeaderOfTHelper.GetContentAndSaveHeaderAttributes(parameters[headerDescription.Index], headerDescription);
                            }
                            else
                                headerValues[headerIndex++] = parameterValue;
                        }
                    }

                    MemoryStream memoryStream = new MemoryStream();
                    XmlDictionaryWriter bufferWriter = XmlDictionaryWriter.CreateTextWriter(memoryStream);
                    bufferWriter.WriteStartElement("root");
                    serializer.Serialize(bufferWriter, headerValues, null, isEncoded ? GetEncoding(message.Version.Envelope) : null);
                    bufferWriter.WriteEndElement();
                    bufferWriter.Flush();
                    XmlDocument doc = new XmlDocument();
                    memoryStream.Position = 0;
                    doc.Load(new XmlTextReader(memoryStream) { DtdProcessing = DtdProcessing.Prohibit });
                    //doc.Save(Console.Out);
                    foreach (XmlElement element in doc.DocumentElement.ChildNodes)
                    {
                        MessageHeaderDescription matchingHeaderDescription = headerDescriptionTable.Get(element.LocalName, element.NamespaceURI);
                        if (matchingHeaderDescription == null)
                            message.Headers.Add(new XmlElementMessageHeader(this, message.Version, element.LocalName, element.NamespaceURI,
                                                                            false/*mustUnderstand*/, null/*actor*/, false/*relay*/, element));
                        else
                        {
                            if (matchingHeaderDescription.TypedHeader)
                                messageHeaderOfTHelper.GetHeaderAttributes(matchingHeaderDescription, out mustUnderstand, out relay, out actor);
                            else
                            {
                                mustUnderstand = matchingHeaderDescription.MustUnderstand;
                                relay = matchingHeaderDescription.Relay;
                                actor = matchingHeaderDescription.Actor;
                            }
                            message.Headers.Add(new XmlElementMessageHeader(this, message.Version, element.LocalName, element.NamespaceURI,
                                                                            mustUnderstand, actor, relay, element));
                        }
                    }
                }
                if (unknownHeaderDescription != null && parameters[unknownHeaderDescription.Index] != null)
                {
                    foreach (object unknownHeader in (IEnumerable)parameters[unknownHeaderDescription.Index])
                    {

                        XmlElement element = (XmlElement)GetContentOfMessageHeaderOfT(unknownHeaderDescription, unknownHeader, out mustUnderstand, out relay, out actor);
                        if (element != null)
                            message.Headers.Add(new XmlElementMessageHeader(this, message.Version, element.LocalName, element.NamespaceURI,
                                                                  mustUnderstand, actor, relay, element));
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorSerializingHeader, messageDescription.MessageName, e.Message), e));
            }
        }

        protected override void GetHeadersFromMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            try
            {
                XmlSerializer serializer;
                MessageHeaderDescriptionTable headerDescriptionTable;
                MessageHeaderDescription unknownHeaderDescription;
                if (isRequest)
                {
                    serializer = requestMessageInfo.HeaderSerializer;
                    headerDescriptionTable = requestMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = requestMessageInfo.UnknownHeaderDescription;
                }
                else
                {
                    serializer = replyMessageInfo.HeaderSerializer;
                    headerDescriptionTable = replyMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = replyMessageInfo.UnknownHeaderDescription;
                }
                MessageHeaders headers = message.Headers;
                ArrayList unknownHeaders = null;
                XmlDocument xmlDoc = null;
                if (unknownHeaderDescription != null)
                {
                    unknownHeaders = new ArrayList();
                    xmlDoc = new XmlDocument();
                }
                if (serializer == null)
                {
                    if (unknownHeaderDescription != null)
                    {
                        for (int headerIndex = 0; headerIndex < headers.Count; headerIndex++)
                            AddUnknownHeader(unknownHeaderDescription, unknownHeaders, xmlDoc, null/*bufferWriter*/, headers[headerIndex], headers.GetReaderAtHeader(headerIndex));
                        parameters[unknownHeaderDescription.Index] = unknownHeaders.ToArray(unknownHeaderDescription.TypedHeader ? typeof(MessageHeader<XmlElement>) : typeof(XmlElement));
                    }
                    return;
                }


                MemoryStream memoryStream = new MemoryStream();
                XmlDictionaryWriter bufferWriter = XmlDictionaryWriter.CreateTextWriter(memoryStream);
                message.WriteStartEnvelope(bufferWriter);
                message.WriteStartHeaders(bufferWriter);
                MessageHeaderOfTHelper messageHeaderOfTHelper = null;
                for (int headerIndex = 0; headerIndex < headers.Count; headerIndex++)
                {
                    MessageHeaderInfo header = headers[headerIndex];
                    XmlDictionaryReader headerReader = headers.GetReaderAtHeader(headerIndex);
                    MessageHeaderDescription matchingHeaderDescription = headerDescriptionTable.Get(header.Name, header.Namespace);
                    if (matchingHeaderDescription != null)
                    {
                        if (header.MustUnderstand)
                            headers.UnderstoodHeaders.Add(header);
                        if (matchingHeaderDescription.TypedHeader)
                        {
                            if (messageHeaderOfTHelper == null)
                                messageHeaderOfTHelper = new MessageHeaderOfTHelper(parameters.Length);
                            messageHeaderOfTHelper.SetHeaderAttributes(matchingHeaderDescription, header.MustUnderstand, header.Relay, header.Actor);
                        }

                    }
                    if (matchingHeaderDescription == null && unknownHeaderDescription != null)
                        AddUnknownHeader(unknownHeaderDescription, unknownHeaders, xmlDoc, bufferWriter, header, headerReader);
                    else
                        bufferWriter.WriteNode(headerReader, false);
                    headerReader.Close();
                }
                bufferWriter.WriteEndElement();
                bufferWriter.WriteEndElement();
                bufferWriter.Flush();

                /*
                XmlDocument doc = new XmlDocument();
                memoryStream.Position = 0;
                doc.Load(memoryStream);
                doc.Save(Console.Out);
                */

                memoryStream.Position = 0;
                XmlDictionaryReader bufferReader = XmlDictionaryReader.CreateTextReader(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, XmlDictionaryReaderQuotas.Max);

                bufferReader.ReadStartElement();
                bufferReader.MoveToContent();
                if (!bufferReader.IsEmptyElement)
                {
                    bufferReader.ReadStartElement();
                    object[] headerValues = (object[])serializer.Deserialize(bufferReader, isEncoded ? GetEncoding(message.Version.Envelope) : null);
                    int headerIndex = 0;
                    foreach (MessageHeaderDescription headerDescription in messageDescription.Headers)
                    {
                        if (!headerDescription.IsUnknownHeaderCollection)
                        {
                            object parameterValue = headerValues[headerIndex++];
                            if (headerDescription.TypedHeader && parameterValue != null)
                                parameterValue = messageHeaderOfTHelper.CreateMessageHeader(headerDescription, parameterValue);
                            parameters[headerDescription.Index] = parameterValue;
                        }
                    }
                    bufferReader.Close();
                }
                if (unknownHeaderDescription != null)
                    parameters[unknownHeaderDescription.Index] = unknownHeaders.ToArray(unknownHeaderDescription.TypedHeader ? typeof(MessageHeader<XmlElement>) : typeof(XmlElement));
            }
            catch (InvalidOperationException e)
            {
                // all exceptions from XmlSerializer get wrapped in InvalidOperationException,
                // so we must be conservative and never turn this into a fault
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorDeserializingHeader, messageDescription.MessageName), e));
            }
        }

        private static void AddUnknownHeader(MessageHeaderDescription unknownHeaderDescription, ArrayList unknownHeaders, XmlDocument xmlDoc, XmlDictionaryWriter bufferWriter, MessageHeaderInfo header, XmlDictionaryReader headerReader)
        {
            object unknownHeader = xmlDoc.ReadNode(headerReader);
            if (bufferWriter != null)
                ((XmlElement)unknownHeader).WriteTo(bufferWriter);
            if (unknownHeader != null && unknownHeaderDescription.TypedHeader)
                unknownHeader = TypedHeaderManager.Create(unknownHeaderDescription.Type, unknownHeader, header.MustUnderstand, header.Relay, header.Actor);

            unknownHeaders.Add(unknownHeader);
        }

        protected override void WriteBodyAttributes(XmlDictionaryWriter writer, MessageVersion version)
        {
            if (isEncoded && version.Envelope == EnvelopeVersion.Soap11)
            {
                string encoding = GetEncoding(version.Envelope);
                writer.WriteAttributeString("encodingStyle", version.Envelope.Namespace, encoding);
            }
            writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
            writer.WriteAttributeString("xmlns", "xsd", null, XmlSchema.Namespace);
        }

        protected override void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, string action, MessageDescription messageDescription, object returnValue, object[] parameters, bool isRequest)
        {
            if (writer == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            if (parameters == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            try
            {
                MessageInfo messageInfo;
                if (isRequest)
                    messageInfo = requestMessageInfo;
                else
                    messageInfo = replyMessageInfo;
                if (messageInfo.RpcEncodedTypedMessageBodyParts == null)
                {
                    SerializeBody(writer, version, messageInfo.BodySerializer, messageDescription.Body.ReturnValue, messageDescription.Body.Parts, returnValue, parameters);
                    return;
                }

                object[] bodyPartValues = new object[messageInfo.RpcEncodedTypedMessageBodyParts.Count];
                object bodyObject = parameters[messageDescription.Body.Parts[0].Index];
                if (bodyObject == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxBodyCannotBeNull, messageDescription.MessageName)));
                int i = 0;
                foreach (MessagePartDescription bodyPart in messageInfo.RpcEncodedTypedMessageBodyParts)
                {
                    MemberInfo member = bodyPart.MemberInfo;
                    FieldInfo field = member as FieldInfo;
                    if (field != null)
                        bodyPartValues[i++] = field.GetValue(bodyObject);
                    else
                    {
                        PropertyInfo property = member as PropertyInfo;
                        if (property != null)
                            bodyPartValues[i++] = property.GetValue(bodyObject, null);
                    }
                }
                SerializeBody(writer, version, messageInfo.BodySerializer, null/*returnPart*/, messageInfo.RpcEncodedTypedMessageBodyParts, null/*returnValue*/, bodyPartValues);
            }
            catch (InvalidOperationException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(SR.SFxErrorSerializingBody, messageDescription.MessageName, e.Message), e));
            }
        }

        void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, XmlSerializer serializer, MessagePartDescription returnPart, MessagePartDescriptionCollection bodyParts, object returnValue, object[] parameters)
        {
            if (serializer == null)
            {
                return;
            }

            bool hasReturnValue = IsValidReturnValue(returnPart);
            object[] bodyParameters = new object[bodyParts.Count + (hasReturnValue ? 1 : 0)];
            int paramIndex = 0;

            if (hasReturnValue)
                bodyParameters[paramIndex++] = returnValue;

            for (int i = 0; i < bodyParts.Count; i++)
                bodyParameters[paramIndex++] = parameters[bodyParts[i].Index];

            string encoding = isEncoded ? GetEncoding(version.Envelope) : null;
            serializer.Serialize(writer, bodyParameters, null, encoding);
        }


        protected override object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, string action, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            MessageInfo messageInfo;
            if (isRequest)
                messageInfo = requestMessageInfo;
            else
                messageInfo = replyMessageInfo;
            if (messageInfo.RpcEncodedTypedMessageBodyParts == null)
                return DeserializeBody(reader, version, messageInfo.BodySerializer, messageDescription.Body.ReturnValue, messageDescription.Body.Parts, parameters, isRequest);

            object[] bodyPartValues = new object[messageInfo.RpcEncodedTypedMessageBodyParts.Count];
            DeserializeBody(reader, version, messageInfo.BodySerializer, null/*returnPart*/, messageInfo.RpcEncodedTypedMessageBodyParts, bodyPartValues, isRequest);
            object bodyObject = Activator.CreateInstance(messageDescription.Body.Parts[0].Type);
            int i = 0;
            foreach (MessagePartDescription bodyPart in messageInfo.RpcEncodedTypedMessageBodyParts)
            {
                MemberInfo member = bodyPart.MemberInfo;
                FieldInfo field = member as FieldInfo;
                if (field != null)
                    field.SetValue(bodyObject, bodyPartValues[i++]);
                else
                {
                    PropertyInfo property = member as PropertyInfo;
                    if (property != null)
                        property.SetValue(bodyObject, bodyPartValues[i++], null);
                }
            }
            parameters[messageDescription.Body.Parts[0].Index] = bodyObject;
            return null;
        }

        object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, XmlSerializer serializer, MessagePartDescription returnPart, MessagePartDescriptionCollection bodyParts, object[] parameters, bool isRequest)
        {
            try
            {
                if (reader == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
                if (parameters == null) throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
                object returnValue = null;
                if (serializer == null)
                {
                    return null;
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                    return null;

                object[] bodyParameters = (object[])serializer.Deserialize(reader, isEncoded ? GetEncoding(version.Envelope) : null);

                int paramIndex = 0;
                if (IsValidReturnValue(returnPart))
                    returnValue = bodyParameters[paramIndex++];

                for (int i = 0; i < bodyParts.Count; i++)
                    parameters[bodyParts[i].Index] = bodyParameters[paramIndex++];
                return returnValue;
            }
            catch (InvalidOperationException e)
            {
                // all exceptions from XmlSerializer get wrapped in InvalidOperationException,
                // so we must be conservative and never turn this into a fault
                string resourceKey = isRequest ? SR.SFxErrorDeserializingRequestBody : SR.SFxErrorDeserializingReplyBody;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    SR.GetString(resourceKey, OperationName), e));
            }
        }

        internal static string GetEncoding(EnvelopeVersion version)
        {
            if (version == EnvelopeVersion.Soap11)
            {
                return soap11Encoding;
            }
            else if (version == EnvelopeVersion.Soap12)
            {
                return soap12Encoding;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("version",
                    SR.GetString(SR.EnvelopeVersionNotSupported, version));
            }
        }

        internal abstract class MessageInfo
        {
            internal abstract XmlSerializer BodySerializer { get; }
            internal abstract XmlSerializer HeaderSerializer { get; }
            internal abstract MessageHeaderDescriptionTable HeaderDescriptionTable { get; }
            internal abstract MessageHeaderDescription UnknownHeaderDescription { get; }
            internal abstract MessagePartDescriptionCollection RpcEncodedTypedMessageBodyParts { get; }
        }

        class MessageHeaderOfTHelper
        {
            object[] attributes;
            internal MessageHeaderOfTHelper(int parameterCount)
            {
                attributes = new object[parameterCount];
            }
            internal object GetContentAndSaveHeaderAttributes(object parameterValue, MessageHeaderDescription headerDescription)
            {
                if (parameterValue == null)
                    return null;
                bool mustUnderstand;
                bool relay;
                string actor;
                if (headerDescription.Multiple)
                {
                    object[] messageHeaderOfTArray = (object[])parameterValue;
                    MessageHeader<object>[] messageHeaderOfTAttributes = new MessageHeader<object>[messageHeaderOfTArray.Length];
                    Array tArray = Array.CreateInstance(headerDescription.Type, messageHeaderOfTArray.Length);
                    for (int i = 0; i < tArray.Length; i++)
                    {
                        tArray.SetValue(GetContentOfMessageHeaderOfT(headerDescription, messageHeaderOfTArray[i], out mustUnderstand, out relay, out actor), i);
                        messageHeaderOfTAttributes[i] = new MessageHeader<object>(null, mustUnderstand, actor, relay);
                    }
                    attributes[headerDescription.Index] = messageHeaderOfTAttributes;
                    return tArray;
                }
                else
                {
                    object content = GetContentOfMessageHeaderOfT(headerDescription, parameterValue, out mustUnderstand, out relay, out actor);
                    attributes[headerDescription.Index] = new MessageHeader<object>(null, mustUnderstand, actor, relay);
                    return content;
                }

            }

            internal void GetHeaderAttributes(MessageHeaderDescription headerDescription, out bool mustUnderstand, out bool relay, out string actor)
            {
                MessageHeader<object> matchingMessageHeaderOfTAttribute = null;
                if (headerDescription.Multiple)
                {
                    MessageHeader<object>[] messageHeaderOfTAttributes = (MessageHeader<object>[])attributes[headerDescription.Index];
                    for (int i = 0; i < messageHeaderOfTAttributes.Length; i++)
                    {
                        if (messageHeaderOfTAttributes[i] != null)
                        {
                            matchingMessageHeaderOfTAttribute = messageHeaderOfTAttributes[i];
                            messageHeaderOfTAttributes[i] = null;
                            break;
                        }
                    }
                    //assert(matchingMessageHeaderOfTAttribute != null);

                }
                else
                    matchingMessageHeaderOfTAttribute = (MessageHeader<object>)attributes[headerDescription.Index];
                mustUnderstand = matchingMessageHeaderOfTAttribute.MustUnderstand;
                relay = matchingMessageHeaderOfTAttribute.Relay;
                actor = matchingMessageHeaderOfTAttribute.Actor;
            }

            internal void SetHeaderAttributes(MessageHeaderDescription headerDescription, bool mustUnderstand, bool relay, string actor)
            {
                if (headerDescription.Multiple)
                {
                    if (attributes[headerDescription.Index] == null)
                        attributes[headerDescription.Index] = new List<MessageHeader<object>>();
                    ((List<MessageHeader<object>>)attributes[headerDescription.Index]).Add(new MessageHeader<object>(null, mustUnderstand, actor, relay));
                }
                else
                    attributes[headerDescription.Index] = new MessageHeader<object>(null, mustUnderstand, actor, relay);

            }
            internal object CreateMessageHeader(MessageHeaderDescription headerDescription, object headerValue)
            {
                if (headerDescription.Multiple)
                {
                    IList<MessageHeader<object>> messageHeaderOfTAttributes = (IList<MessageHeader<object>>)attributes[headerDescription.Index];
                    object[] messageHeaderOfTArray = (object[])Array.CreateInstance(TypedHeaderManager.GetMessageHeaderType(headerDescription.Type), messageHeaderOfTAttributes.Count);
                    Array headerValues = (Array)headerValue;
                    for (int i = 0; i < messageHeaderOfTArray.Length; i++)
                    {
                        MessageHeader<object> messageHeaderOfTAttribute = messageHeaderOfTAttributes[i];
                        messageHeaderOfTArray[i] = TypedHeaderManager.Create(headerDescription.Type, headerValues.GetValue(i),
                                                                      messageHeaderOfTAttribute.MustUnderstand, messageHeaderOfTAttribute.Relay, messageHeaderOfTAttribute.Actor);
                    }
                    return messageHeaderOfTArray;
                }
                else
                {
                    MessageHeader<object> messageHeaderOfTAttribute = (MessageHeader<object>)attributes[headerDescription.Index];
                    return TypedHeaderManager.Create(headerDescription.Type, headerValue,
                                                                  messageHeaderOfTAttribute.MustUnderstand, messageHeaderOfTAttribute.Relay, messageHeaderOfTAttribute.Actor);

                }
            }
        }
    }
}
