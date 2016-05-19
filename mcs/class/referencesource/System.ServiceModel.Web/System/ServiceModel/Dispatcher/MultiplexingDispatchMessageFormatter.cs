//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mime;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;

    class MultiplexingDispatchMessageFormatter : IDispatchMessageFormatter
    {
        Dictionary<WebMessageFormat, IDispatchMessageFormatter> formatters;
        WebMessageFormat defaultFormat;
        Dictionary<WebMessageFormat, string> defaultContentTypes;

        public WebMessageFormat DefaultFormat
        {
            get { return this.defaultFormat; }
        }

        public Dictionary<WebMessageFormat, string> DefaultContentTypes
        {
            get
            {
                return this.defaultContentTypes;
            }
        }

        public MultiplexingDispatchMessageFormatter(Dictionary<WebMessageFormat, IDispatchMessageFormatter> formatters, WebMessageFormat defaultFormat)
        {
            if (formatters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatters");
            }
            this.formatters = formatters;
            this.defaultFormat = defaultFormat;
            this.defaultContentTypes = new Dictionary<WebMessageFormat, string>();
            Fx.Assert(this.formatters.ContainsKey(this.defaultFormat), "The default format should always be included in the dictionary of formatters.");
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.SerializingRequestNotSupportedByFormatter, this)));
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            WebOperationContext currentContext = WebOperationContext.Current;
            OutgoingWebResponseContext outgoingResponse = null;

            if (currentContext != null)
            {
                outgoingResponse = currentContext.OutgoingResponse;
            }
            
            WebMessageFormat format = this.defaultFormat;
            if (outgoingResponse != null)
            {
                WebMessageFormat? nullableFormat = outgoingResponse.Format;
                if (nullableFormat.HasValue)
                {
                    format = nullableFormat.Value;
                }
            }

            if (!this.formatters.ContainsKey(format))
            {
                string operationName = "<null>";

                if (OperationContext.Current != null)
                {
                    MessageProperties messageProperties = OperationContext.Current.IncomingMessageProperties;
                    if (messageProperties.ContainsKey(WebHttpDispatchOperationSelector.HttpOperationNamePropertyName))
                    {
                        operationName = messageProperties[WebHttpDispatchOperationSelector.HttpOperationNamePropertyName] as string;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.OperationDoesNotSupportFormat, operationName, format.ToString())));
            }

            if (outgoingResponse != null && string.IsNullOrEmpty(outgoingResponse.ContentType))
            {
                string automatedSelectionContentType = outgoingResponse.AutomatedFormatSelectionContentType;
                if (!string.IsNullOrEmpty(automatedSelectionContentType))
                {
                    // Don't set the content-type if it is default xml for backwards compatiabilty
                    if (!string.Equals(automatedSelectionContentType, defaultContentTypes[WebMessageFormat.Xml], StringComparison.OrdinalIgnoreCase))
                    {
                        outgoingResponse.ContentType = automatedSelectionContentType;
                    }
                }
                else
                {
                    // Don't set the content-type if it is default xml for backwards compatiabilty
                    if (format != WebMessageFormat.Xml)
                    {
                        outgoingResponse.ContentType = defaultContentTypes[format];
                    }
                }
            }

            Message message = this.formatters[format].SerializeReply(messageVersion, parameters, result);

            return message;
        }

        public bool SupportsMessageFormat(WebMessageFormat format)
        {
            return this.formatters.ContainsKey(format);
        }
    }
}

