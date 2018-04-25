//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

    class ContentTypeSettingDispatchMessageFormatter : IDispatchMessageFormatter
    {
        IDispatchMessageFormatter innerFormatter;
        string outgoingContentType;

        public ContentTypeSettingDispatchMessageFormatter(string outgoingContentType, IDispatchMessageFormatter innerFormatter)
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

        public void DeserializeRequest(Message message, object[] parameters)
        {
            this.innerFormatter.DeserializeRequest(message, parameters);
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            Message message = this.innerFormatter.SerializeReply(messageVersion, parameters, result);
            if (message != null)
            {
                AddResponseContentTypeProperty(message, this.outgoingContentType);
            }
            return message;
        }

        static void AddResponseContentTypeProperty(Message message, string contentType)
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
                if (string.IsNullOrEmpty(WebOperationContext.Current.OutgoingResponse.ContentType))
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = contentType;
                }
            }
            else
            {
                object prop;
                message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out prop);
                HttpResponseMessageProperty httpProperty;
                if (prop != null)
                {
                    httpProperty = (HttpResponseMessageProperty) prop;
                }
                else
                {
                    httpProperty = new HttpResponseMessageProperty();
                    message.Properties.Add(HttpResponseMessageProperty.Name, httpProperty);
                }
                if (string.IsNullOrEmpty(httpProperty.Headers[HttpResponseHeader.ContentType]))
                {
                    httpProperty.Headers[HttpResponseHeader.ContentType] = contentType;
                }
            }
        }
    }
}
