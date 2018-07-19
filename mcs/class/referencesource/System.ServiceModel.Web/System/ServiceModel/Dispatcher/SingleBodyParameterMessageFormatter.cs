//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Xml;
    using System.Runtime.Serialization;
    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;
    using System.ServiceModel.Web;

    abstract class SingleBodyParameterMessageFormatter : IDispatchMessageFormatter, IClientMessageFormatter
    {
        string contractName;
        string contractNs;
        bool isRequestFormatter;
        string operationName;
        string serializerType;

        protected SingleBodyParameterMessageFormatter(OperationDescription operation, bool isRequestFormatter, string serializerType)
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            this.contractName = operation.DeclaringContract.Name;
            this.contractNs = operation.DeclaringContract.Namespace;
            this.operationName = operation.Name;
            this.isRequestFormatter = isRequestFormatter;
            this.serializerType = serializerType;
        }

        protected string ContractName
        {
            get { return this.contractName; }
        }

        protected string ContractNs
        {
            get { return this.contractNs; }
        }

        protected string OperationName
        {
            get { return this.operationName; }
        }

        public static IClientMessageFormatter CreateXmlAndJsonClientFormatter(OperationDescription operation, Type type, bool isRequestFormatter, UnwrappedTypesXmlSerializerManager xmlSerializerManager)
        {
            IClientMessageFormatter xmlFormatter = CreateClientFormatter(operation, type, isRequestFormatter, false, xmlSerializerManager);
            if (!WebHttpBehavior.SupportsJsonFormat(operation))
            {
                return xmlFormatter;
            }
            IClientMessageFormatter jsonFormatter = CreateClientFormatter(operation, type, isRequestFormatter, true, xmlSerializerManager);
            Dictionary<WebContentFormat, IClientMessageFormatter> map = new Dictionary<WebContentFormat, IClientMessageFormatter>();
            map.Add(WebContentFormat.Xml, xmlFormatter);
            map.Add(WebContentFormat.Json, jsonFormatter);
            return new DemultiplexingClientMessageFormatter(map, xmlFormatter);
        }

        public static IDispatchMessageFormatter CreateXmlAndJsonDispatchFormatter(OperationDescription operation, Type type, bool isRequestFormatter, UnwrappedTypesXmlSerializerManager xmlSerializerManager, string callbackParameterName)
        {
            IDispatchMessageFormatter xmlFormatter = CreateDispatchFormatter(operation, type, isRequestFormatter, false, xmlSerializerManager, null);
            if (!WebHttpBehavior.SupportsJsonFormat(operation))
            {
                return xmlFormatter;
            }
            IDispatchMessageFormatter jsonFormatter = CreateDispatchFormatter(operation, type, isRequestFormatter, true, xmlSerializerManager, callbackParameterName);
            Dictionary<WebContentFormat, IDispatchMessageFormatter> map = new Dictionary<WebContentFormat, IDispatchMessageFormatter>();
            map.Add(WebContentFormat.Xml, xmlFormatter);
            map.Add(WebContentFormat.Json, jsonFormatter);
            return new DemultiplexingDispatchMessageFormatter(map, xmlFormatter);
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            if (isRequestFormatter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.FormatterCannotBeUsedForReplyMessages)));
            }
            return ReadObject(message);
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            if (!isRequestFormatter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.FormatterCannotBeUsedForRequestMessages)));
            }

            parameters[0] = ReadObject(message);
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            if (isRequestFormatter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.FormatterCannotBeUsedForReplyMessages)));
            }
            Message message = Message.CreateMessage(messageVersion, (string)null, CreateBodyWriter(result));
            if (result == null)
            {
                SuppressReplyEntityBody(message);
            }
            AttachMessageProperties(message, false);
            return message;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            if (!isRequestFormatter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.FormatterCannotBeUsedForRequestMessages)));
            }
            Message message = Message.CreateMessage(messageVersion, (string)null, CreateBodyWriter(parameters[0]));
            if (parameters[0] == null)
            {
                SuppressRequestEntityBody(message);
            }
            AttachMessageProperties(message, true);
            return message;
        }

        internal static IClientMessageFormatter CreateClientFormatter(OperationDescription operation, Type type, bool isRequestFormatter, bool useJson, UnwrappedTypesXmlSerializerManager xmlSerializerManager)
        {
            if (type == null)
            {
                return new NullMessageFormatter(false, null);
            }
            else if (useJson)
            {
                return CreateJsonFormatter(operation, type, isRequestFormatter);
            }
            else
            {
                return CreateXmlFormatter(operation, type, isRequestFormatter, xmlSerializerManager);
            }
        }

        internal static IDispatchMessageFormatter CreateDispatchFormatter(OperationDescription operation, Type type, bool isRequestFormatter, bool useJson, UnwrappedTypesXmlSerializerManager xmlSerializerManager, string callbackParameterName)
        {
            if (type == null)
            {
                return new NullMessageFormatter(useJson, callbackParameterName);
            }
            else if (useJson)
            {
                return CreateJsonFormatter(operation, type, isRequestFormatter);
            }
            else
            {
                return CreateXmlFormatter(operation, type, isRequestFormatter, xmlSerializerManager);
            }
        }

        internal static void SuppressReplyEntityBody(Message message)
        {
            WebOperationContext currentContext = WebOperationContext.Current;
            if (currentContext != null)
            {
                OutgoingWebResponseContext responseContext = currentContext.OutgoingResponse;
                if (responseContext != null)
                {
                    responseContext.SuppressEntityBody = true;
                }
            }
            else
            {
                object untypedProp;
                message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out untypedProp);
                HttpResponseMessageProperty prop = untypedProp as HttpResponseMessageProperty;
                if (prop == null)
                {
                    prop = new HttpResponseMessageProperty();
                    message.Properties[HttpResponseMessageProperty.Name] = prop;
                }
                prop.SuppressEntityBody = true;
            }
        }

        internal static void SuppressRequestEntityBody(Message message)
        {
            WebOperationContext currentContext = WebOperationContext.Current;
            if (currentContext != null)
            {
                OutgoingWebRequestContext requestContext = currentContext.OutgoingRequest;
                if (requestContext != null)
                {
                    requestContext.SuppressEntityBody = true;
                }
            }
            else
            {
                object untypedProp;
                message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out untypedProp);
                HttpRequestMessageProperty prop = untypedProp as HttpRequestMessageProperty;
                if (prop == null)
                {
                    prop = new HttpRequestMessageProperty();
                    message.Properties[HttpRequestMessageProperty.Name] = prop;
                }
                prop.SuppressEntityBody = true;
            }
        }

        protected virtual void AttachMessageProperties(Message message, bool isRequest)
        {
        }

        protected abstract XmlObjectSerializer[] GetInputSerializers();

        protected abstract XmlObjectSerializer GetOutputSerializer(Type type);

        protected virtual void ValidateMessageFormatProperty(Message message)
        {
        }

        protected Type GetTypeForSerializer(Type type, Type parameterType, IList<Type> knownTypes)
        {
            if (type == parameterType)
            {
                return type;
            }
            else if (knownTypes != null)
            {
                for (int i = 0; i < knownTypes.Count; ++i)
                {
                    if (type == knownTypes[i])
                    {
                        return type;
                    }
                }
            }
            return parameterType;
        }

        public static SingleBodyParameterMessageFormatter CreateXmlFormatter(OperationDescription operation, Type type, bool isRequestFormatter, UnwrappedTypesXmlSerializerManager xmlSerializerManager)
        {
            DataContractSerializerOperationBehavior dcsob = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (dcsob != null)
            {
                return new SingleBodyParameterDataContractMessageFormatter(operation, type, isRequestFormatter, false, dcsob);
            }
            XmlSerializerOperationBehavior xsob = operation.Behaviors.Find<XmlSerializerOperationBehavior>();
            if (xsob != null)
            {
                return new SingleBodyParameterXmlSerializerMessageFormatter(operation, type, isRequestFormatter, xsob, xmlSerializerManager);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.OnlyDataContractAndXmlSerializerTypesInUnWrappedMode, operation.Name)));
        }

        public static SingleBodyParameterMessageFormatter CreateJsonFormatter(OperationDescription operation, Type type, bool isRequestFormatter)
        {
            DataContractSerializerOperationBehavior dcsob = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (dcsob == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.JsonFormatRequiresDataContract, operation.Name, operation.DeclaringContract.Name, operation.DeclaringContract.Namespace)));
            }
            return new SingleBodyParameterDataContractMessageFormatter(operation, type, isRequestFormatter, true, dcsob);
        }

        BodyWriter CreateBodyWriter(object body)
        {
            XmlObjectSerializer serializer;
            if (body != null)
            {
                serializer = GetOutputSerializer(body.GetType());
                if (serializer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.CannotSerializeType, body.GetType(), this.operationName, this.contractName, this.contractNs, this.serializerType)));
                }
            }
            else
            {
                serializer = null;
            }
            return new SingleParameterBodyWriter(body, serializer);
        }

        protected virtual object ReadObject(Message message)
        {
            if (HttpStreamFormatter.IsEmptyMessage(message))
            {
                return null;
            }
            XmlObjectSerializer[] inputSerializers = GetInputSerializers();
            XmlDictionaryReader reader = message.GetReaderAtBodyContents();
            if (inputSerializers != null)
            {
                for (int i = 0; i < inputSerializers.Length; ++i)
                {
                    if (inputSerializers[i].IsStartObject(reader))
                    {
                        return inputSerializers[i].ReadObject(reader, false);
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR2.GetString(SR2.CannotDeserializeBody, reader.LocalName, reader.NamespaceURI, operationName, contractName, contractNs, this.serializerType)));
        }

        class NullMessageFormatter : IDispatchMessageFormatter, IClientMessageFormatter
        {
            bool useJson;
            string callbackParameterName;

            public NullMessageFormatter(bool useJson, string callbackParameterName)
            {
                this.useJson = useJson;
                this.callbackParameterName = callbackParameterName;
            }

            public object DeserializeReply(Message message, object[] parameters)
            {
                return null;
            }

            public void DeserializeRequest(Message message, object[] parameters)
            {
            }

            public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
            {
                Message reply = Message.CreateMessage(messageVersion, (string)null);
                SuppressReplyEntityBody(reply);
                if (useJson && WebHttpBehavior.TrySetupJavascriptCallback(callbackParameterName) != null)
                {
                    reply.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
                }
                return reply;
            }

            public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
            {
                Message request = Message.CreateMessage(messageVersion, (string)null);
                SuppressRequestEntityBody(request);
                return request;
            }
        }

        class SingleParameterBodyWriter : BodyWriter
        {
            object body;
            XmlObjectSerializer serializer;

            public SingleParameterBodyWriter(object body, XmlObjectSerializer serializer)
                : base(false)
            {
                if (body != null && serializer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
                }
                this.body = body;
                this.serializer = serializer;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                if (body != null)
                {
                    this.serializer.WriteObject(writer, body);
                }
            }
        }
    }
}

