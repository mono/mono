//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.Web
{
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class OutgoingWebRequestContext
    {
        OperationContext operationContext;
        internal OutgoingWebRequestContext(OperationContext operationContext)
        {
            Fx.Assert(operationContext != null, "operationContext is null");
            this.operationContext = operationContext;
        }

        public string Accept
        {
            get { return this.MessageProperty.Headers[HttpRequestHeader.Accept]; }
            set { this.MessageProperty.Headers[HttpRequestHeader.Accept] = value; }
        }

        public long ContentLength
        {
            get { return long.Parse(this.MessageProperty.Headers[HttpRequestHeader.ContentLength], CultureInfo.InvariantCulture); }
            set { this.MessageProperty.Headers[HttpRequestHeader.ContentLength] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public string ContentType
        {
            get { return this.MessageProperty.Headers[HttpRequestHeader.ContentType]; }
            set { this.MessageProperty.Headers[HttpRequestHeader.ContentType] = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return this.MessageProperty.Headers; }
        }

        public string IfMatch
        {
            get { return this.MessageProperty.Headers[HttpRequestHeader.IfMatch]; }
            set { this.MessageProperty.Headers[HttpRequestHeader.IfMatch] = value; }
        }

        public string IfModifiedSince
        {
            get { return this.MessageProperty.Headers[HttpRequestHeader.IfModifiedSince]; }
            set { this.MessageProperty.Headers[HttpRequestHeader.IfModifiedSince] = value; }
        }

        public string IfNoneMatch
        {
            get { return this.MessageProperty.Headers[HttpRequestHeader.IfNoneMatch]; }
            set { this.MessageProperty.Headers[HttpRequestHeader.IfNoneMatch] = value; }
        }

        public string IfUnmodifiedSince
        {
            get { return this.MessageProperty.Headers[HttpRequestHeader.IfUnmodifiedSince]; }
            set { this.MessageProperty.Headers[HttpRequestHeader.IfUnmodifiedSince] = value; }
        }

        public string Method
        {
            get { return this.MessageProperty.Method; }
            set { this.MessageProperty.Method = value; }
        }

        public bool SuppressEntityBody
        {
            get { return this.MessageProperty.SuppressEntityBody; }
            set { this.MessageProperty.SuppressEntityBody = value; }
        }

        public string UserAgent
        {
            get { return this.MessageProperty.Headers[HttpRequestHeader.UserAgent]; }
            set { this.MessageProperty.Headers[HttpRequestHeader.UserAgent] = value; }
        }

        HttpRequestMessageProperty MessageProperty
        {
            get
            {
                if (!operationContext.OutgoingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
                {
                    operationContext.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, new HttpRequestMessageProperty());
                }
                return operationContext.OutgoingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            }
        }
    }
}
