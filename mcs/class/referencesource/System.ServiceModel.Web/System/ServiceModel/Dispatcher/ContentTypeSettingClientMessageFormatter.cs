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
    using System.Net;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Web;

    class ContentTypeSettingClientMessageFormatter : IClientMessageFormatter
    {
        IClientMessageFormatter innerFormatter;
        string outgoingContentType;

        public ContentTypeSettingClientMessageFormatter(string outgoingContentType, IClientMessageFormatter innerFormatter)
        {
            if (outgoingContentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outgoingContentType");
            }
            if (innerFormatter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerFormatter");
            }
            this.outgoingContentType = outgoingContentType;
            this.innerFormatter = innerFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            return this.innerFormatter.DeserializeReply(message, parameters);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            Message message = this.innerFormatter.SerializeRequest(messageVersion, parameters);
            if (message != null)
            {
                AddRequestContentTypeProperty(message, this.outgoingContentType);
            }
            return message;
        }

        static void AddRequestContentTypeProperty(Message message, string contentType)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
            }
            if (OperationContext.Current != null && OperationContext.Current.HasOutgoingMessageProperties)
            {
                if (string.IsNullOrEmpty(WebOperationContext.Current.OutgoingRequest.ContentType))
                {
                    WebOperationContext.Current.OutgoingRequest.ContentType = contentType;
                }
            }
            else
            {
                object prop;
                message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out prop);
                HttpRequestMessageProperty httpProperty;
                if (prop != null)
                {
                    httpProperty = (HttpRequestMessageProperty) prop;
                }
                else
                {
                    httpProperty = new HttpRequestMessageProperty();
                    message.Properties.Add(HttpRequestMessageProperty.Name, httpProperty);
                }
                if (string.IsNullOrEmpty(httpProperty.Headers[HttpRequestHeader.ContentType]))
                {
                    httpProperty.Headers[HttpRequestHeader.ContentType] = contentType;
                }
            }
        }
    }
}
