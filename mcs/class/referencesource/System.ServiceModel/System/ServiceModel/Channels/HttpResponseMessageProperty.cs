//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime;

    public sealed class HttpResponseMessageProperty : IMessageProperty, IMergeEnabledMessageProperty
    {
        TraditionalHttpResponseMessageProperty traditionalProperty;
        HttpResponseMessageBackedProperty httpBackedProperty;
        bool useHttpBackedProperty;
        bool initialCopyPerformed;

        public HttpResponseMessageProperty()
            : this((WebHeaderCollection)null)
        {
        }

        internal HttpResponseMessageProperty(WebHeaderCollection originalHeaders)
        {
            this.traditionalProperty = new TraditionalHttpResponseMessageProperty(originalHeaders);
            this.useHttpBackedProperty = false;
        }

        internal HttpResponseMessageProperty(HttpResponseMessage httpResponseMessage)
        {
            this.httpBackedProperty = new HttpResponseMessageBackedProperty(httpResponseMessage);
            this.useHttpBackedProperty = true;
        }

        public static string Name
        {
            get { return "httpResponse"; }
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

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.useHttpBackedProperty ?
                    this.httpBackedProperty.StatusCode :
                    this.traditionalProperty.StatusCode;
            }

            set
            {
                int valueInt = (int)value;
                if (valueInt < 100 || valueInt > 599)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeInRange, 100, 599)));
                }

                if (this.useHttpBackedProperty)
                {
                    this.httpBackedProperty.StatusCode = value;
                }
                else
                {
                    this.traditionalProperty.StatusCode = value;
                }
            }
        }

        internal bool HasStatusCodeBeenSet
        {
            get
            {
                return this.useHttpBackedProperty ?
                    true :
                    this.traditionalProperty.HasStatusCodeBeenSet;
            }
        }

        public string StatusDescription
        {
            get
            {
                return this.useHttpBackedProperty ?
                    this.httpBackedProperty.StatusDescription :
                    this.traditionalProperty.StatusDescription;
            }

            set
            {
                if (this.useHttpBackedProperty)
                {
                    this.httpBackedProperty.StatusDescription = value;
                }
                else
                {
                    this.traditionalProperty.StatusDescription = value;
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

        public bool SuppressPreamble
        {
            get
            {
                return this.useHttpBackedProperty ?
                    false :
                    this.traditionalProperty.SuppressPreamble;
            }

            set
            {
                if (!this.useHttpBackedProperty)
                {
                    this.traditionalProperty.SuppressPreamble = value;
                }
            }
        }

        private HttpResponseMessage HttpResponseMessage
        {
            get
            {
                if (this.useHttpBackedProperty)
                {
                    return this.httpBackedProperty.HttpResponseMessage;
                }

                return null;
            }
        }

        internal static HttpResponseMessage GetHttpResponseMessageFromMessage(Message message)
        {
            HttpResponseMessage httpResponseMessage = null;

            HttpResponseMessageProperty property = message.Properties.GetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name);
            if (property != null)
            {
                httpResponseMessage = property.HttpResponseMessage;
                if (httpResponseMessage != null)
                {
                    httpResponseMessage.CopyPropertiesFromMessage(message);
                    message.EnsureReadMessageState();
                }
            }

            return httpResponseMessage;
        }

        IMessageProperty IMessageProperty.CreateCopy()
        {
            if (!this.useHttpBackedProperty ||
                !this.initialCopyPerformed)
            {
                this.initialCopyPerformed = true;
                return this;
            }

            return this.httpBackedProperty.CreateTraditionalResponseMessageProperty();
        }

        bool IMergeEnabledMessageProperty.TryMergeWithProperty(object propertyToMerge)
        {
            // The ImmutableDispatchRuntime will merge MessageProperty instances from the
            //  OperationContext (that were created before the response message was created) with
            //  MessageProperty instances on the message itself.  The message's version of the 
            //  HttpResponseMessageProperty may hold a reference to an HttpResponseMessage, and this 
            //  cannot be discarded, so values from the OperationContext's property must be set on 
            //  the message's version without completely replacing the message's property.
            if (this.useHttpBackedProperty)
            {
                HttpResponseMessageProperty responseProperty = propertyToMerge as HttpResponseMessageProperty;
                if (responseProperty != null)
                {
                    if (!responseProperty.useHttpBackedProperty)
                    {
                        this.httpBackedProperty.MergeWithTraditionalProperty(responseProperty.traditionalProperty);
                        responseProperty.traditionalProperty = null;
                        responseProperty.httpBackedProperty = this.httpBackedProperty;
                        responseProperty.useHttpBackedProperty = true;
                    }

                    return true;
                }
            }
                
            return false;
        }

        private class TraditionalHttpResponseMessageProperty
        {
            public const HttpStatusCode DefaultStatusCode = HttpStatusCode.OK;
            public const string DefaultStatusDescription = null; // null means use description from status code

            WebHeaderCollection headers;
            WebHeaderCollection originalHeaders;
            HttpStatusCode statusCode;

            public TraditionalHttpResponseMessageProperty(WebHeaderCollection originalHeaders)
            {
                this.originalHeaders = originalHeaders;
                this.statusCode = DefaultStatusCode;
                this.StatusDescription = DefaultStatusDescription;
            }

            public WebHeaderCollection Headers
            {
                get
                {
                    if (this.headers == null)
                    {
                        this.headers = new WebHeaderCollection();
                        if (this.originalHeaders != null)
                        {
                            this.headers.Add(originalHeaders);
                            this.originalHeaders = null;
                        }
                    }

                    return this.headers;
                }
            }

            public HttpStatusCode StatusCode
            {
                get
                {
                    return this.statusCode;
                }

                set
                {
                    this.statusCode = value;
                    this.HasStatusCodeBeenSet = true;
                }
            }

            public bool HasStatusCodeBeenSet { get; private set; }

            public string StatusDescription { get; set; }

            public bool SuppressEntityBody { get; set; }

            public bool SuppressPreamble { get; set; }
        }

        private class HttpResponseMessageBackedProperty
        {
            private HttpHeadersWebHeaderCollection headers;

            public HttpResponseMessageBackedProperty(HttpResponseMessage httpResponseMessage)
            {
                Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' property should never be null.");

                this.HttpResponseMessage = httpResponseMessage;
            }

            public HttpResponseMessage HttpResponseMessage { get; private set; }

            public WebHeaderCollection Headers
            {
                get
                {
                    if (this.headers == null)
                    {
                        this.headers = new HttpHeadersWebHeaderCollection(this.HttpResponseMessage);
                    }

                    return this.headers;
                }
            }

            public HttpStatusCode StatusCode
            {
                get
                {
                    return this.HttpResponseMessage.StatusCode;
                }

                set
                {
                    this.HttpResponseMessage.StatusCode = value;
                }
            }

            public string StatusDescription
            {
                get
                {
                    return this.HttpResponseMessage.ReasonPhrase;
                }

                set
                {
                    this.HttpResponseMessage.ReasonPhrase = value;
                }
            }

            public bool SuppressEntityBody
            {
                get
                {
                    HttpContent content = this.HttpResponseMessage.Content;
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
                    HttpContent content = this.HttpResponseMessage.Content;
                    if (value && content != null &&
                        (!content.Headers.ContentLength.HasValue ||
                        content.Headers.ContentLength.Value > 0))
                    {
                        HttpContent newContent = new ByteArrayContent(EmptyArray<byte>.Instance);
                        foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
                        {
                            newContent.Headers.AddHeaderWithoutValidation(header);
                        }

                        this.HttpResponseMessage.Content = newContent;
                        content.Dispose();
                    }
                    else if (!value && content == null)
                    {
                        this.HttpResponseMessage.Content = new ByteArrayContent(EmptyArray<byte>.Instance); 
                    }
                }
            }

            public HttpResponseMessageProperty CreateTraditionalResponseMessageProperty()
            {
                HttpResponseMessageProperty copiedProperty = new HttpResponseMessageProperty();

                copiedProperty.Headers.Add(this.Headers);

                if (this.StatusCode != TraditionalHttpResponseMessageProperty.DefaultStatusCode)
                {
                    copiedProperty.StatusCode = this.StatusCode;
                }

                copiedProperty.StatusDescription = this.StatusDescription;
                copiedProperty.SuppressEntityBody = this.SuppressEntityBody;

                return copiedProperty;
            }

            public void MergeWithTraditionalProperty(TraditionalHttpResponseMessageProperty propertyToMerge)
            {
                if (propertyToMerge.HasStatusCodeBeenSet)
                {
                    this.StatusCode = propertyToMerge.StatusCode;
                }

                if (propertyToMerge.StatusDescription != TraditionalHttpResponseMessageProperty.DefaultStatusDescription)
                {
                    this.StatusDescription = propertyToMerge.StatusDescription;
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

