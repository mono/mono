//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Web;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    class FormatSelectingMessageInspector : IDispatchMessageInspector
    {
        static readonly IEnumerable<string> wildcardMediaTypes = new List<string>() { "application", "text" };

        List<MultiplexingFormatMapping> mappings;
        Dictionary<string, MultiplexingDispatchMessageFormatter> formatters;
        Dictionary<string, NameValueCache<FormatContentTypePair>> caches;
        bool automaticFormatSelectionEnabled;
        
        // There are technically an infinite number of valid accept headers for just xml and json,
        // but to prevent DOS attacks, we need to set an upper limit. It is assumed that there would 
        // never be more than two dozen valid accept headers actually used out in the wild.
        static readonly int maxCachedAcceptHeaders = 25;

        public FormatSelectingMessageInspector(WebHttpBehavior webHttpBehavior, List<MultiplexingFormatMapping> mappings)
        {
            if (webHttpBehavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("webHttpBehavior");
            }
            if (mappings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("mappings");
            }

            this.automaticFormatSelectionEnabled = webHttpBehavior.AutomaticFormatSelectionEnabled;
            
            this.formatters = new Dictionary<string, MultiplexingDispatchMessageFormatter>();

            this.caches = new Dictionary<string, NameValueCache<FormatContentTypePair>>();

            this.mappings = mappings;
        }

        public void RegisterOperation(string operationName, MultiplexingDispatchMessageFormatter formatter)
        {
            if (formatter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatter");
            }
            Fx.Assert(!this.formatters.ContainsKey(operationName), "An operation should only be registered once.");
            this.formatters.Add(operationName, formatter);
            this.caches.Add(operationName, new NameValueCache<FormatContentTypePair>(maxCachedAcceptHeaders));
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (this.automaticFormatSelectionEnabled)
            {
                MessageProperties messageProperties = OperationContext.Current.IncomingMessageProperties;
                if (messageProperties.ContainsKey(WebHttpDispatchOperationSelector.HttpOperationNamePropertyName))
                {
                    string operationName = messageProperties[WebHttpDispatchOperationSelector.HttpOperationNamePropertyName] as string;
                    if (!string.IsNullOrEmpty(operationName) && this.formatters.ContainsKey(operationName))
                    {

                        string acceptHeader = WebOperationContext.Current.IncomingRequest.Accept;
                        if (!string.IsNullOrEmpty(acceptHeader))
                        {
                            if (TrySetFormatFromCache(operationName, acceptHeader) ||
                                TrySetFormatFromAcceptHeader(operationName, acceptHeader, true /* matchCharSet */) ||
                                TrySetFormatFromAcceptHeader(operationName, acceptHeader, false /* matchCharSet */))
                            {
                                return null;
                            }
                        }
                        if (TrySetFormatFromContentType(operationName))
                        {
                            return null;
                        }
                        SetFormatFromDefault(operationName);
                    }
                }
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            // do nothing
        }

        bool TrySetFormatFromCache(string operationName, string acceptHeader)
        {
            Fx.Assert(this.caches.ContainsKey(operationName), "The calling method is responsible for ensuring that the 'operationName' key exists in the caches dictionary.");
            Fx.Assert(acceptHeader != null, "The calling method is responsible for ensuring that 'acceptHeader' is not null");

            FormatContentTypePair pair = caches[operationName].Lookup(acceptHeader.ToUpperInvariant());
            if (pair != null)
            {
                SetFormatAndContentType(pair.Format, pair.ContentType);
                return true;
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Given that media types are generally in lowercase, it is awkward to normalize to uppercase.")]
        bool TrySetFormatFromAcceptHeader(string operationName, string acceptHeader, bool matchCharSet)
        {
            Fx.Assert(this.formatters.ContainsKey(operationName), "The calling method is responsible for ensuring that the 'operationName' key exists in the formatters dictionary.");
            
            IList<ContentType> acceptHeaderElements = WebOperationContext.Current.IncomingRequest.GetAcceptHeaderElements();

            for (int i = 0; i < acceptHeaderElements.Count; i++)
            {
                string[] typeAndSubType = acceptHeaderElements[i].MediaType.Split('/');
                string type = typeAndSubType[0].Trim().ToLowerInvariant();
                string subType = typeAndSubType[1].Trim();
                
                if ((subType[0] == '*' && subType.Length == 1) &&
                     ((type[0] == '*' && type.Length == 1) ||
                      wildcardMediaTypes.Contains(type)))
                {
                    SetFormatFromDefault(operationName, acceptHeader);
                    return true;
                }

                foreach (MultiplexingFormatMapping mapping in mappings)
                {
                    ContentType contentType;
                    WebMessageFormat format = mapping.MessageFormat;
                    if (this.formatters[operationName].SupportsMessageFormat(format) &&
                        mapping.CanFormatResponse(acceptHeaderElements[i], matchCharSet, out contentType))
                    {
                        string contentTypeStr = contentType.ToString();
                        this.caches[operationName].AddOrUpdate(acceptHeader.ToUpperInvariant(), new FormatContentTypePair(format, contentTypeStr));
                        SetFormatAndContentType(format, contentTypeStr);
                        return true;
                    }
                }
            }
            return false;
        }

        bool TrySetFormatFromContentType(string operationName)
        {
            Fx.Assert(this.formatters.ContainsKey(operationName), "The calling method is responsible for ensuring that the 'operationName' key exists in the formatters dictionary.");
            
            string contentTypeStr = WebOperationContext.Current.IncomingRequest.ContentType;
            if (contentTypeStr != null)
            {
                ContentType contentType = Web.Utility.GetContentType(contentTypeStr);
                if (contentType != null)
                {
                    foreach (MultiplexingFormatMapping mapping in mappings)
                    {
                        ContentType responseContentType;
                        if (this.formatters[operationName].SupportsMessageFormat(mapping.MessageFormat) &&
                            mapping.CanFormatResponse(contentType, false, out responseContentType))
                        {
                            SetFormatAndContentType(mapping.MessageFormat, responseContentType.ToString());
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        void SetFormatFromDefault(string operationName)
        {
            SetFormatFromDefault(operationName, null);
        }

        void SetFormatFromDefault(string operationName, string acceptHeader)
        {
            Fx.Assert(this.formatters.ContainsKey(operationName), "The calling method is responsible for ensuring that the 'operationName' key exists in the formatters dictionary.");
            WebMessageFormat format = this.formatters[operationName].DefaultFormat;
            
            if (!string.IsNullOrEmpty(acceptHeader))
            {
                this.caches[operationName].AddOrUpdate(acceptHeader.ToUpperInvariant(), new FormatContentTypePair(format, null));
            }

            WebOperationContext.Current.OutgoingResponse.Format = format;

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.AutomaticFormatSelectedOperationDefault, SR2.GetString(SR2.TraceCodeAutomaticFormatSelectedOperationDefault, format.ToString()));
            }
        }

        void SetFormatAndContentType(WebMessageFormat format, string contentType)
        {
            OutgoingWebResponseContext outgoingResponse = WebOperationContext.Current.OutgoingResponse;
            outgoingResponse.Format = format;
            outgoingResponse.AutomatedFormatSelectionContentType = contentType;

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.AutomaticFormatSelectedRequestBased, SR2.GetString(SR2.TraceCodeAutomaticFormatSelectedRequestBased, format.ToString(), contentType));
            }
        }

        class FormatContentTypePair
        {
            WebMessageFormat format;
            string contentType;

            public FormatContentTypePair(WebMessageFormat format, string contentType)
            {
                this.format = format;
                this.contentType = contentType;
            }

            public WebMessageFormat Format
            {
                get { return this.format; }
            }

            public string ContentType
            {
                get { return this.contentType; }
            }
        }
    }
}
