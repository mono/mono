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
    using System.Reflection;
    using System.Xml;
    using System.ServiceModel.Diagnostics;
    using System.Runtime.Serialization;
    using System.Net;
    using System.Runtime.Serialization.Json;

    class DataContractJsonSerializerOperationFormatter : DataContractSerializerOperationFormatter
    {
        bool isBareMessageContractReply;
        bool isBareMessageContractRequest;
        // isWrapped is true when the user has explicitly chosen the response or request format to be Wrapped (allowed only in WebHttpBehavior)
        bool isWrapped;
        bool useAspNetAjaxJson;
        string callbackParameterName;

        public DataContractJsonSerializerOperationFormatter(OperationDescription description, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate, bool isWrapped, bool useAspNetAjaxJson, string callbackParameterName)
            : base(description, TypeLoader.DefaultDataContractFormatAttribute, new DataContractJsonSerializerOperationBehavior(description, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, useAspNetAjaxJson))
        {
            if (this.requestMessageInfo != null)
            {
                if (this.requestMessageInfo.WrapperName == null)
                {
                    isBareMessageContractRequest = true;
                }
                else
                {
                    this.requestMessageInfo.WrapperName = JsonGlobals.rootDictionaryString;
                    this.requestMessageInfo.WrapperNamespace = XmlDictionaryString.Empty;
                }
            }

            if (this.replyMessageInfo != null)
            {
                if (this.replyMessageInfo.WrapperName == null)
                {
                    isBareMessageContractReply = true;
                }
                else
                {
                    if (useAspNetAjaxJson)
                    {
                        this.replyMessageInfo.WrapperName = JsonGlobals.dDictionaryString;
                    }
                    else
                    {
                        this.replyMessageInfo.WrapperName = JsonGlobals.rootDictionaryString;
                    }
                    this.replyMessageInfo.WrapperNamespace = XmlDictionaryString.Empty;
                }
            }

            if ((this.requestStreamFormatter != null) && (this.requestStreamFormatter.WrapperName != null))
            {
                this.requestStreamFormatter.WrapperName = JsonGlobals.rootString;
                this.requestStreamFormatter.WrapperNamespace = string.Empty;
            }

            if ((this.replyStreamFormatter != null) && (this.replyStreamFormatter.WrapperName != null))
            {
                this.replyStreamFormatter.WrapperName = JsonGlobals.rootString;
                this.replyStreamFormatter.WrapperNamespace = string.Empty;
            }
            this.isWrapped = isWrapped;
            this.useAspNetAjaxJson = useAspNetAjaxJson;
            this.callbackParameterName = callbackParameterName;
        }

        internal static bool IsJsonLocalName(XmlDictionaryReader reader, string elementName)
        {
            if (reader.IsStartElement(JsonGlobals.itemDictionaryString, JsonGlobals.itemDictionaryString))
            {
                if (reader.MoveToAttribute(JsonGlobals.itemString))
                {
                    return (reader.Value == elementName);
                }
            }
            return false;
        }

        internal static bool IsStartElement(XmlDictionaryReader reader, string elementName)
        {
            if (reader.IsStartElement(elementName))
            {
                return true;
            }
            return IsJsonLocalName(reader, elementName);
        }

        internal static bool IsStartElement(XmlDictionaryReader reader, XmlDictionaryString elementName, XmlDictionaryString elementNamespace)
        {
            if (reader.IsStartElement(elementName, elementNamespace))
            {
                return true;
            }
            return IsJsonLocalName(reader, (elementName == null) ? null : elementName.Value);
        }

        protected override void AddHeadersToMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            if (message != null)
            {
                message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
            }
            base.AddHeadersToMessage(message, messageDescription, parameters, isRequest);
        }

        protected override object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, string action, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            if (reader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }

            if (reader.EOF)
            {
                return null;
            }

            if ((isRequest && this.isBareMessageContractRequest) || (!isRequest && isBareMessageContractReply))
            {
                return DeserializeBareMessageContract(reader, parameters, isRequest);
            }

            object returnValue = null;

            if (isRequest || (isWrapped && !useAspNetAjaxJson))
            {
                ValidateTypeObjectAttribute(reader, isRequest);
                returnValue = DeserializeBodyCore(reader, parameters, isRequest);
            }
            else
            {
                if (useAspNetAjaxJson)
                {
                    ReadRootElement(reader);
                }
                if (useAspNetAjaxJson && messageDescription.IsVoid)
                {
                    ReadVoidReturn(reader);
                }
                else if (replyMessageInfo.ReturnPart != null)
                {
                    PartInfo part = replyMessageInfo.ReturnPart;
                    DataContractJsonSerializer serializer = part.Serializer as DataContractJsonSerializer;

                    if (useAspNetAjaxJson)
                    {
                        serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.dString);
                        VerifyIsStartElement(reader, JsonGlobals.dString);
                    }
                    else
                    {
                        serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.rootString);
                        VerifyIsStartElement(reader, JsonGlobals.rootString);
                    }

                    if (serializer.IsStartObject(reader))
                    {
                        try
                        {
                            returnValue = part.ReadObject(reader, serializer);
                        }
                        catch (System.InvalidOperationException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
                        }
                        catch (System.Runtime.Serialization.InvalidDataContractException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new System.Runtime.Serialization.InvalidDataContractException(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
                        }
                        catch (System.FormatException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                OperationFormatter.CreateDeserializationFailedFault(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                                part.Description.Namespace, part.Description.Name, e.Message), 
                                e));
                        }
                        catch (System.Runtime.Serialization.SerializationException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                OperationFormatter.CreateDeserializationFailedFault(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                                part.Description.Namespace, part.Description.Name, e.Message), 
                                e));
                        }
                    }
                }
                else if (replyMessageInfo.BodyParts != null)
                {
                    ValidateTypeObjectAttribute(reader, isRequest);
                    returnValue = DeserializeBodyCore(reader, parameters, isRequest);
                }

                while (reader.IsStartElement())
                {
                    OperationFormatter.TraceAndSkipElement(reader);
                }

                if (useAspNetAjaxJson)
                {
                    reader.ReadEndElement();
                }
            }

            return returnValue;
        }

        protected override void GetHeadersFromMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            if (message != null)
            {
                object prop;
                message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out prop);
                WebBodyFormatMessageProperty formatProperty = (prop as WebBodyFormatMessageProperty);
                if (formatProperty == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.MessageFormatPropertyNotFound2, this.OperationName)));
                }
                if (formatProperty.Format != WebContentFormat.Json)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.InvalidHttpMessageFormat3, this.OperationName, formatProperty.Format, WebContentFormat.Json)));
                }
            }
            base.GetHeadersFromMessage(message, messageDescription, parameters, isRequest);
        }

        protected override void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, string action, MessageDescription messageDescription, object returnValue, object[] parameters, bool isRequest)
        {
            if ((isRequest && this.isBareMessageContractRequest) || (!isRequest && isBareMessageContractReply))
            {
                SerializeBareMessageContract(writer, parameters, isRequest);
            }
            else
            {
                bool isJsonp = WebHttpBehavior.TrySetupJavascriptCallback(callbackParameterName) != null;
                bool useAspNetJsonWrapper = !isJsonp && useAspNetAjaxJson;

                if (isRequest || (isWrapped && !useAspNetJsonWrapper))
                {
                    SerializeBody(writer, returnValue, parameters, isRequest);
                }
                else
                {
                    if (useAspNetJsonWrapper)
                    {
                        writer.WriteStartElement(JsonGlobals.rootString);
                        writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.objectString);
                    }

                    if (useAspNetJsonWrapper && messageDescription.IsVoid)
                    {
                        WriteVoidReturn(writer);
                    }
                    else if (isJsonp && messageDescription.IsVoid)
                    {
                        writer.WriteStartElement(JsonGlobals.rootString);
                        writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.nullString);
                        writer.WriteEndElement();
                    }
                    else if (replyMessageInfo.ReturnPart != null)
                    {
                        DataContractJsonSerializer serializer = replyMessageInfo.ReturnPart.Serializer as DataContractJsonSerializer;
                        if (useAspNetJsonWrapper)
                        {
                            serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.dString);
                        }
                        else
                        {
                            serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.rootString);
                        }

                        try
                        {
                            serializer.WriteObject(writer, returnValue);
                        }
                        catch (SerializationException sx)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorSerializingParameter, replyMessageInfo.ReturnPart.Description.Namespace, replyMessageInfo.ReturnPart.Description.Name, sx.Message), sx));
                        }
                    }
                    else if (replyMessageInfo.BodyParts != null)
                    {
                        SerializeBody(writer, returnValue, parameters, isRequest);
                    }

                    if (useAspNetJsonWrapper)
                    {
                        writer.WriteEndElement();
                    }
                }
            }
        }

        static DataContractJsonSerializer RecreateDataContractJsonSerializer(DataContractJsonSerializer serializer, string newRootName)
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings
            {
                RootName = newRootName,
                KnownTypes = serializer.KnownTypes,
                MaxItemsInObjectGraph = serializer.MaxItemsInObjectGraph,
                IgnoreExtensionDataObject = serializer.IgnoreExtensionDataObject,
                DataContractSurrogate = serializer.DataContractSurrogate,
                EmitTypeInformation = serializer.EmitTypeInformation,
                DateTimeFormat = serializer.DateTimeFormat,
                UseSimpleDictionaryFormat = serializer.UseSimpleDictionaryFormat
            };
            return new DataContractJsonSerializer(serializer.GetDeserializeType(), settings);
        }

        object DeserializeBareMessageContract(XmlDictionaryReader reader, object[] parameters, bool isRequest)
        {
            MessageInfo messageInfo;
            if (isRequest)
            {
                messageInfo = this.requestMessageInfo;
            }
            else
            {
                messageInfo = this.replyMessageInfo;
            }

            if (useAspNetAjaxJson && !isRequest)
            {
                ReadRootElement(reader);
                if (messageInfo.BodyParts.Length == 0)
                {
                    ReadVoidReturn(reader);
                }
            }
            if (messageInfo.BodyParts.Length > 0)
            {
                PartInfo part = messageInfo.BodyParts[0];
                DataContractJsonSerializer serializer = part.Serializer as DataContractJsonSerializer;
                if (useAspNetAjaxJson && !isRequest)
                {
                    serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.dString);
                }
                else
                {
                    serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.rootString);
                }
                while (reader.IsStartElement())
                {
                    if (serializer.IsStartObject(reader))
                    {
                        try
                        {
                            parameters[part.Description.Index] = part.ReadObject(reader, serializer);
                            break;
                        }
                        catch (System.InvalidOperationException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
                        }
                        catch (System.Runtime.Serialization.InvalidDataContractException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
                        }
                        catch (System.FormatException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                OperationFormatter.CreateDeserializationFailedFault(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                                part.Description.Namespace, part.Description.Name, e.Message), 
                                e));
                        }
                        catch (System.Runtime.Serialization.SerializationException e)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                OperationFormatter.CreateDeserializationFailedFault(
                                System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                                part.Description.Namespace, part.Description.Name, e.Message), 
                                e));
                        }
                    }
                    else
                    {
                        OperationFormatter.TraceAndSkipElement(reader);
                    }
                }
                while (reader.IsStartElement())
                {
                    OperationFormatter.TraceAndSkipElement(reader);
                }
            }
            if (this.useAspNetAjaxJson && !isRequest)
            {
                reader.ReadEndElement();
            }
            return null;
        }

        object DeserializeBodyCore(XmlDictionaryReader reader, object[] parameters, bool isRequest)
        {
            MessageInfo messageInfo;
            if (isRequest)
            {
                messageInfo = requestMessageInfo;
            }
            else
            {
                messageInfo = replyMessageInfo;
            }

            if (messageInfo.WrapperName != null)
            {
                VerifyIsStartElement(reader, messageInfo.WrapperName, messageInfo.WrapperNamespace);
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (isEmptyElement)
                {
                    return null;
                }
            }

            object returnValue = null;
            DeserializeParameters(reader, messageInfo.BodyParts, parameters, messageInfo.ReturnPart, ref returnValue);
            if (messageInfo.WrapperName != null)
            {
                reader.ReadEndElement();
            }
            return returnValue;
        }

        object DeserializeParameter(XmlDictionaryReader reader, PartInfo part)
        {
            if (part.Description.Multiple)
            {
                ArrayList items = new ArrayList();
                while (part.Serializer.IsStartObject(reader))
                {
                    items.Add(DeserializeParameterPart(reader, part));
                }
                return items.ToArray(part.Description.Type);
            }
            return DeserializeParameterPart(reader, part);
        }

        object DeserializeParameterPart(XmlDictionaryReader reader, PartInfo part)
        {
            object val;
            try
            {
                val = part.ReadObject(reader);
            }
            catch (System.InvalidOperationException e)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
            }
            catch (System.Runtime.Serialization.InvalidDataContractException e)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(
                    System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameter, part.Description.Namespace, part.Description.Name), e));
            }
            catch (System.FormatException e)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    OperationFormatter.CreateDeserializationFailedFault(
                    System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                    part.Description.Namespace, part.Description.Name, e.Message), 
                    e));
            }
            catch (System.Runtime.Serialization.SerializationException e)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    OperationFormatter.CreateDeserializationFailedFault(
                    System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorDeserializingParameterMore,
                    part.Description.Namespace, part.Description.Name, e.Message), 
                    e));
            }

            return val;
        }

        void DeserializeParameters(XmlDictionaryReader reader, PartInfo[] parts, object[] parameters, PartInfo returnInfo, ref object returnValue)
        {
            bool[] setParameters = new bool[parameters.Length];
            bool hasReadReturnValue = false;
            int currentIndex = 0;

            while (reader.IsStartElement())
            {
                bool hasReadParameter = false;

                for (int i = 0, index = currentIndex; i < parts.Length; i++, index = (index + 1) % parts.Length)
                {
                    PartInfo part = parts[index];
                    if (part.Serializer.IsStartObject(reader))
                    {
                        currentIndex = i;
                        parameters[part.Description.Index] = DeserializeParameter(reader, part);
                        setParameters[part.Description.Index] = true;
                        hasReadParameter = true;
                    }
                }

                if (!hasReadParameter)
                {
                    if ((returnInfo != null) && !hasReadReturnValue && returnInfo.Serializer.IsStartObject(reader))
                    {
                        returnValue = DeserializeParameter(reader, returnInfo);
                        hasReadReturnValue = true;
                    }
                    else
                    {
                        OperationFormatter.TraceAndSkipElement(reader);
                    }
                }
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!setParameters[i])
                {
                    parameters[i] = null;
                }
            }
        }

        void ReadRootElement(XmlDictionaryReader reader)
        {
            if (!IsStartElement(reader, JsonGlobals.rootString))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBody, JsonGlobals.rootString, string.Empty, reader.NodeType, reader.Name, reader.NamespaceURI)));
            }
            string typeAttribute = reader.GetAttribute(JsonGlobals.typeString);
            if (!typeAttribute.Equals(JsonGlobals.objectString, StringComparison.Ordinal))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    XmlObjectSerializer.CreateSerializationException(SR2.GetString(SR2.JsonFormatterExpectedAttributeObject, typeAttribute)));
            }

            bool isEmptyElement = reader.IsEmptyElement;
            reader.Read();
            if (isEmptyElement)
            {
                //throw in aspnet case
            }
        }

        void ReadVoidReturn(XmlDictionaryReader reader)
        {
            VerifyIsStartElement(reader, JsonGlobals.dString);
            string typeAttribute = reader.GetAttribute(JsonGlobals.typeString);
            if (!typeAttribute.Equals(JsonGlobals.nullString, StringComparison.Ordinal))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    XmlObjectSerializer.CreateSerializationException(SR2.GetString(SR2.JsonFormatterExpectedAttributeNull, typeAttribute)));
            }
            OperationFormatter.TraceAndSkipElement(reader);
        }

        void SerializeBareMessageContract(XmlDictionaryWriter writer, object[] parameters, bool isRequest)
        {
            bool useAspNetJsonWrapper = WebHttpBehavior.TrySetupJavascriptCallback(callbackParameterName) == null && useAspNetAjaxJson;

            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }

            if (parameters == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }

            MessageInfo messageInfo;
            if (isRequest)
            {
                messageInfo = this.requestMessageInfo;
            }
            else
            {
                messageInfo = this.replyMessageInfo;
            }
            if (useAspNetJsonWrapper && !isRequest)
            {
                writer.WriteStartElement(JsonGlobals.rootString);
                writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.objectString);
                if (messageInfo.BodyParts.Length == 0)
                {
                    WriteVoidReturn(writer);
                }
            }
            if (messageInfo.BodyParts.Length > 0)
            {
                PartInfo part = messageInfo.BodyParts[0];
                DataContractJsonSerializer serializer = part.Serializer as DataContractJsonSerializer;
                if (useAspNetJsonWrapper && !isRequest)
                {
                    serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.dString);
                }
                else
                {
                    serializer = RecreateDataContractJsonSerializer(serializer, JsonGlobals.rootString);
                }

                object graph = parameters[part.Description.Index];
                try
                {
                    serializer.WriteObject(writer, graph);
                }
                catch (SerializationException sx)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                        System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorSerializingParameter, part.Description.Namespace, part.Description.Name, sx.Message), sx));
                }
            }
            if (useAspNetJsonWrapper && !isRequest)
            {
                writer.WriteEndElement();
            }
        }

        void SerializeBody(XmlDictionaryWriter writer, object returnValue, object[] parameters, bool isRequest)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }

            if (parameters == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }

            MessageInfo messageInfo;
            if (isRequest)
            {
                messageInfo = requestMessageInfo;
            }
            else
            {
                messageInfo = replyMessageInfo;
            }

            if (messageInfo.WrapperName != null)
            {
                if (WebHttpBehavior.TrySetupJavascriptCallback(callbackParameterName) != null)
                {
                    writer.WriteStartElement(JsonGlobals.rootString);
                }
                else
                {
                    writer.WriteStartElement(messageInfo.WrapperName, messageInfo.WrapperNamespace);
                }
                writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.objectString);
            }

            if (messageInfo.ReturnPart != null)
            {
                SerializeParameter(writer, messageInfo.ReturnPart, returnValue);
            }
            SerializeParameters(writer, messageInfo.BodyParts, parameters);

            if (messageInfo.WrapperName != null)
            {
                writer.WriteEndElement();
            }
        }

        void SerializeParameter(XmlDictionaryWriter writer, PartInfo part, object graph)
        {
            if (part.Description.Multiple)
            {
                if (graph != null)
                {
                    foreach (object item in (IEnumerable)graph)
                    {
                        SerializeParameterPart(writer, part, item);
                    }
                }
            }
            else
            {
                SerializeParameterPart(writer, part, graph);
            }
        }

        void SerializeParameterPart(XmlDictionaryWriter writer, PartInfo part, object graph)
        {
            try
            {
                part.Serializer.WriteObject(writer, graph);
            }
            catch (SerializationException sx)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                    System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBodyErrorSerializingParameter, part.Description.Namespace, part.Description.Name, sx.Message), sx));
            }
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

        void ValidateTypeObjectAttribute(XmlDictionaryReader reader, bool isRequest)
        {
            MessageInfo messageInfo = isRequest ? requestMessageInfo : replyMessageInfo;
            if (messageInfo.WrapperName != null)
            {
                if (!IsStartElement(reader, messageInfo.WrapperName, messageInfo.WrapperNamespace))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBody, messageInfo.WrapperName, messageInfo.WrapperNamespace, reader.NodeType, reader.Name, reader.NamespaceURI)));
                }
                string typeAttribute = reader.GetAttribute(JsonGlobals.typeString);
                if (!typeAttribute.Equals(JsonGlobals.objectString, StringComparison.Ordinal))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        XmlObjectSerializer.CreateSerializationException(SR2.GetString(SR2.JsonFormatterExpectedAttributeObject, typeAttribute)));
                }
            }
        }

        void VerifyIsStartElement(XmlDictionaryReader reader, string elementName)
        {
            bool foundElement = false;
            while (reader.IsStartElement())
            {
                if (IsStartElement(reader, elementName))
                {
                    foundElement = true;
                    break;
                }
                else
                {
                    OperationFormatter.TraceAndSkipElement(reader);
                }
            }
            if (!foundElement)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBody, elementName, string.Empty, reader.NodeType, reader.Name, reader.NamespaceURI)));
            }
        }

        void VerifyIsStartElement(XmlDictionaryReader reader, XmlDictionaryString elementName, XmlDictionaryString elementNamespace)
        {
            bool foundElement = false;
            while (reader.IsStartElement())
            {
                if (IsStartElement(reader, elementName, elementNamespace))
                {
                    foundElement = true;
                    break;
                }
                else
                {
                    OperationFormatter.TraceAndSkipElement(reader);
                }
            }
            if (!foundElement)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInvalidMessageBody, elementName, elementNamespace, reader.NodeType, reader.Name, reader.NamespaceURI)));
            }
        }

        void WriteVoidReturn(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(JsonGlobals.dString);
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.nullString);
            writer.WriteEndElement();
        }
    }
}
