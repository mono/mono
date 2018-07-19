//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime;

    public sealed class HttpRequestMessageProperty : IMessageProperty, IMergeEnabledMessageProperty
    {
        TraditionalHttpRequestMessageProperty traditionalProperty;
        HttpRequestMessageBackedProperty httpBackedProperty;
        bool initialCopyPerformed;
        bool useHttpBackedProperty;

        public HttpRequestMessageProperty()
            : this((IHttpHeaderProvider)null)
        {
        }

        internal HttpRequestMessageProperty(IHttpHeaderProvider httpHeaderProvider)
        {
            this.traditionalProperty = new TraditionalHttpRequestMessageProperty(httpHeaderProvider);
            this.useHttpBackedProperty = false;
        }

        internal HttpRequestMessageProperty(HttpRequestMessage httpRequestMessage)
        {
            this.httpBackedProperty = new HttpRequestMessageBackedProperty(httpRequestMessage);
            this.useHttpBackedProperty = true;
        }

        public static string Name
        {
            get { return "httpRequest"; }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this.useHttpBackedProperty ?
                    this.httpBackedProperty.Headers :
                    this.traditionalProperty.Headers;
            }
        }

        public string Method
        {
            get
            {
                return this.useHttpBackedProperty ?
                    this.httpBackedProperty.Method :
                    this.traditionalProperty.Method;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (this.useHttpBackedProperty)
                {
                    this.httpBackedProperty.Method = value;
                }
                else
                {
                    this.traditionalProperty.Method = value;
                }
            }
        }

        public string QueryString
        {
            get
            {
                return this.useHttpBackedProperty ?
                    this.httpBackedProperty.QueryString :
                    this.traditionalProperty.QueryString;
            }

            set
            {

                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (this.useHttpBackedProperty)
                {
                    this.httpBackedProperty.QueryString = value;
                }
                else
                {
                    this.traditionalProperty.QueryString = value;
                }
            }
        }

        public bool SuppressEntityBody
        {
            get
            {
                return this.useHttpBackedProperty ?
                    this.httpBackedProperty.SuppressEntityBody :
                    this.traditionalProperty.SuppressEntityBody;
            }

            set
            {
                if (this.useHttpBackedProperty)
                {
                    this.httpBackedProperty.SuppressEntityBody = value;
                }
                else
                {
                    this.traditionalProperty.SuppressEntityBody = value;
                }
            }
        }

        private HttpRequestMessage HttpRequestMessage
        {
            get
            {
                if (this.useHttpBackedProperty)
                {
                    return this.httpBackedProperty.HttpRequestMessage;
                }

                return null;
            }
        }

        internal static HttpRequestMessage GetHttpRequestMessageFromMessage(Message message)
        {
            HttpRequestMessage httpRequestMessage = null;

            HttpRequestMessageProperty property = message.Properties.GetValue<HttpRequestMessageProperty>(HttpRequestMessageProperty.Name);
            if (property != null)
            {
                httpRequestMessage = property.HttpRequestMessage;
                if (httpRequestMessage != null)
                {
                    httpRequestMessage.CopyPropertiesFromMessage(message);
                    message.EnsureReadMessageState();
                }
            }

            return httpRequestMessage;
        }

        IMessageProperty IMessageProperty.CreateCopy()
        {
            if (!this.useHttpBackedProperty ||
                !this.initialCopyPerformed)
            {
                this.initialCopyPerformed = true;
                return this;
            }

            return this.httpBackedProperty.CreateTraditionalRequestMessageProperty();
        }

        bool IMergeEnabledMessageProperty.TryMergeWithProperty(object propertyToMerge)
        {
            // The ImmutableDispatchRuntime will merge MessageProperty instances from the
            //  OperationContext (that were created before the response message was created) with
            //  MessageProperty instances on the message itself.  The message's version of the 
            //  HttpRequestMessageProperty may hold a reference to an HttpRequestMessage, and this 
            //  cannot be discarded, so values from the OperationContext's property must be set on 
            //  the message's version without completely replacing the message's property.
            if (this.useHttpBackedProperty)
            {
                HttpRequestMessageProperty requestProperty = propertyToMerge as HttpRequestMessageProperty;
                if (requestProperty != null)
                {
                    if (!requestProperty.useHttpBackedProperty)
                    {
                        this.httpBackedProperty.MergeWithTraditionalProperty(requestProperty.traditionalProperty);
                        requestProperty.traditionalProperty = null;
                        requestProperty.httpBackedProperty = this.httpBackedProperty;
                        requestProperty.useHttpBackedProperty = true;
                    }

                    return true;
                }
            }

            return false;
        }

        internal interface IHttpHeaderProvider
        {
            void CopyHeaders(WebHeaderCollection headers);
        }

        private class TraditionalHttpRequestMessageProperty
        {
            public const string DefaultMethod = "POST";
            public const string DefaultQueryString = "";

            WebHeaderCollection headers;
            IHttpHeaderProvider httpHeaderProvider;
            string method;

            public TraditionalHttpRequestMessageProperty(IHttpHeaderProvider httpHeaderProvider)
            {
                this.httpHeaderProvider = httpHeaderProvider;
                this.method = DefaultMethod;
                this.QueryString = DefaultQueryString;
            }

            public WebHeaderCollection Headers
            {
                get
                {
                    if (this.headers == null)
                    {
                        this.headers = new WebHeaderCollection();
                        if (this.httpHeaderProvider != null)
                        {
                            this.httpHeaderProvider.CopyHeaders(this.headers);
                            this.httpHeaderProvider = null;
                        }
                    }

                    return this.headers;
                }
            }

            public string Method
            {
                get
                {
                    return this.method;
                }

                set
                {
                    this.method = value;
                    this.HasMethodBeenSet = true;
                }
            }

            public bool HasMethodBeenSet { get; private set; }

            public string QueryString { get; set; }

            public bool SuppressEntityBody { get; set; }
        }

        private class HttpRequestMessageBackedProperty
        {
            private HttpHeadersWebHeaderCollection headers;

            public HttpRequestMessageBackedProperty(HttpRequestMessage httpRequestMessage)
            {
                Fx.Assert(httpRequestMessage != null, "The 'httpRequestMessage' property should never be null.");

                this.HttpRequestMessage = httpRequestMessage;
            }

            public HttpRequestMessage HttpRequestMessage { get; private set; }

            public WebHeaderCollection Headers
            {
                get
                {
                    if (this.headers == null)
                    {
                        this.headers = new HttpHeadersWebHeaderCollection(this.HttpRequestMessage);
                    }

                    return this.headers;
                }
            }

            public string Method
            {
                get
                {
                    return this.HttpRequestMessage.Method.Method;
                }

                set
                {
                    this.HttpRequestMessage.Method = new HttpMethod(value);
                }
            }

            public string QueryString
            {
                get
                {
                    string query = this.HttpRequestMessage.RequestUri.Query;
                    return query.Length > 0 ? query.Substring(1) : string.Empty;                  
                }

                set
                {
                    UriBuilder uriBuilder = new UriBuilder(this.HttpRequestMessage.RequestUri);
                    uriBuilder.Query = value;
                    this.HttpRequestMessage.RequestUri = uriBuilder.Uri;
                }
            }

            public bool SuppressEntityBody
            {
                get
                {
                    HttpContent content = this.HttpRequestMessage.Content;
                    if (content != null)
                    {
                        long? contentLength = content.Headers.ContentLength;

                        if (!contentLength.HasValue ||
                            (contentLength.HasValue && contentLength.Value > 0))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                set
                {
                    HttpContent content = this.HttpRequestMessage.Content;
                    if (value && content != null &&
                        (!content.Headers.ContentLength.HasValue ||
                        content.Headers.ContentLength.Value > 0))
                    {
                        HttpContent newContent = new ByteArrayContent(EmptyArray<byte>.Instance);
                        foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
                        {
                            newContent.Headers.AddHeaderWithoutValidation(header);
                        }

                        this.HttpRequestMessage.Content = newContent;
                        content.Dispose();
                    }
                    else if (!value && content == null)
                    {
                        this.HttpRequestMessage.Content = new ByteArrayContent(EmptyArray<byte>.Instance);
                    }
                }
            }

            public HttpRequestMessageProperty CreateTraditionalRequestMessageProperty()
            {
                HttpRequestMessageProperty copiedProperty = new HttpRequestMessageProperty();

                copiedProperty.Headers.Add(this.Headers);

                if (this.Method != TraditionalHttpRequestMessageProperty.DefaultMethod)
                {
                    copiedProperty.Method = this.Method;
                }

                copiedProperty.QueryString = this.QueryString;
                copiedProperty.SuppressEntityBody = this.SuppressEntityBody;

                return copiedProperty;
            }

            public void MergeWithTraditionalProperty(TraditionalHttpRequestMessageProperty propertyToMerge)
            {
                if (propertyToMerge.HasMethodBeenSet)
                {
                    this.Method = propertyToMerge.Method;
                }

                if (propertyToMerge.QueryString != TraditionalHttpRequestMessageProperty.DefaultQueryString)
                {
                    this.QueryString = propertyToMerge.QueryString;
                }

                this.SuppressEntityBody = propertyToMerge.SuppressEntityBody;

                WebHeaderCollection headersToMerge = propertyToMerge.Headers;
                foreach (string headerKey in headersToMerge.AllKeys)
                {
                    this.Headers[headerKey] = headersToMerge[headerKey];
                }
            }
        }
    }
}
