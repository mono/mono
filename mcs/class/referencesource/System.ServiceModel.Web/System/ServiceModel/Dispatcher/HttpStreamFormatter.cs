//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#pragma warning disable 1634 // Stops compiler from warning about unknown warnings (for Presharp)

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
    using System.IO;
    using System.Collections.Specialized;
    using System.Net;

    class HttpStreamFormatter : IDispatchMessageFormatter, IClientMessageFormatter
    {
        string contractName;
        string contractNs;
        string operationName;

        public HttpStreamFormatter(OperationDescription operation)
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            this.operationName = operation.Name;
            this.contractName = operation.DeclaringContract.Name;
            this.contractNs = operation.DeclaringContract.Namespace;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            return GetStreamFromMessage(message, false);
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            parameters[0] = GetStreamFromMessage(message, true);
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            Message message = CreateMessageFromStream(result);
            if (result == null)
            {
                SingleBodyParameterMessageFormatter.SuppressReplyEntityBody(message);
            }
            return message;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            Message message = CreateMessageFromStream(parameters[0]);
            if (parameters[0] == null)
            {
                SingleBodyParameterMessageFormatter.SuppressRequestEntityBody(message);
            }
            return message;
        }

        internal static bool IsEmptyMessage(Message message)
        {
            return message.IsEmpty;
        }

        Message CreateMessageFromStream(object data)
        {
            Message result;
            if (data == null)
            {
                result = Message.CreateMessage(MessageVersion.None, (string) null);
            }
            else
            {
                Stream streamData = data as Stream;
                if (streamData == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR2.GetString(SR2.ParameterIsNotStreamType, data.GetType(), this.operationName, this.contractName, this.contractNs)));
                }
                result = ByteStreamMessage.CreateMessage(streamData);
                result.Properties[WebBodyFormatMessageProperty.Name] = WebBodyFormatMessageProperty.RawProperty;
            }
            return result;
        }

        Stream GetStreamFromMessage(Message message, bool isRequest)
        {
            object prop;
            message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out prop);
            WebBodyFormatMessageProperty formatProperty = (prop as WebBodyFormatMessageProperty);
            if (formatProperty == null)
            {
                // GET and DELETE do not go through the encoder
                if (IsEmptyMessage(message))
                {
                    return new MemoryStream();
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.MessageFormatPropertyNotFound, this.operationName, this.contractName, this.contractNs)));
                }
            }
            if (formatProperty.Format != WebContentFormat.Raw)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.InvalidHttpMessageFormat, this.operationName, this.contractName, this.contractNs, formatProperty.Format, WebContentFormat.Raw)));
            }
            return new StreamFormatter.MessageBodyStream(message, null, null, HttpStreamMessage.StreamElementName, string.Empty, isRequest);
        }
    }
}

