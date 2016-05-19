//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.Web
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class IncomingWebResponseContext
    {
        OperationContext operationContext;
        internal IncomingWebResponseContext(OperationContext operationContext)
        {
            Fx.Assert(operationContext != null, "operationContext is null");
            this.operationContext = operationContext;
        }

        public long ContentLength
        { get { return long.Parse(EnsureMessageProperty().Headers[HttpResponseHeader.ContentLength], CultureInfo.InvariantCulture); } }

        public string ContentType
        { get { return EnsureMessageProperty().Headers[HttpResponseHeader.ContentType]; } }

        public string ETag
        { get { return EnsureMessageProperty().Headers[HttpResponseHeader.ETag]; } }

        public WebHeaderCollection Headers
        { get { return EnsureMessageProperty().Headers; } }

        public string Location
        { get { return EnsureMessageProperty().Headers[HttpResponseHeader.Location]; } }

        public HttpStatusCode StatusCode
        { get { return this.EnsureMessageProperty().StatusCode; } }

        public string StatusDescription
        { get { return this.EnsureMessageProperty().StatusDescription; } }

        HttpResponseMessageProperty MessageProperty
        { get
            {
                if (operationContext.IncomingMessageProperties == null)
                {
                    return null;
                }
                if (!operationContext.IncomingMessageProperties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    return null;
                }
                return operationContext.IncomingMessageProperties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            }
        }

        HttpResponseMessageProperty EnsureMessageProperty()
        {
            if (this.MessageProperty == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.HttpContextNoIncomingMessageProperty, typeof(HttpResponseMessageProperty).Name)));
            }
            return this.MessageProperty;
        }
    }
}
