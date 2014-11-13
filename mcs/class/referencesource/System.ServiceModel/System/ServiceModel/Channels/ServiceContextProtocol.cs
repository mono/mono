//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    class ServiceContextProtocol : ContextProtocol
    {
        public ServiceContextProtocol(ContextExchangeMechanism contextExchangeMechanism)
            : base(contextExchangeMechanism)
        {
            // empty
        }

        public override void OnIncomingMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (this.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                this.OnReceiveHttpCookies(message);
            }
            else
            {
                this.OnReceiveSoapContextHeader(message);
            }

            // deserialize the callback context header, if present
            int headerIndex = message.Headers.FindHeader(CallbackContextMessageHeader.CallbackContextHeaderName, CallbackContextMessageHeader.CallbackContextHeaderNamespace);
            if (headerIndex > 0)
            {
                CallbackContextMessageProperty property = CallbackContextMessageHeader.ParseCallbackContextHeader(message.Headers.GetReaderAtHeader(headerIndex), message.Version.Addressing);
                message.Properties.Add(CallbackContextMessageProperty.Name, property);
            }

            ContextExchangeCorrelationHelper.AddIncomingContextCorrelationData(message);
        }

        public override void OnOutgoingMessage(Message message, RequestContext requestContext)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            ContextMessageProperty messageContext;
            if (ContextMessageProperty.TryGet(message, out messageContext))
            {
                if (this.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
                {
                    Fx.Assert(requestContext != null, "DuplexChannel shape cannot have ContextExchangeMechanism = HttpCookie");
                    Uri requestUri = null;

                    if (requestContext.RequestMessage.Properties != null)
                    {
                        requestUri = requestContext.RequestMessage.Properties.Via;
                    }
                    if (requestUri == null)
                    {
                        requestUri = requestContext.RequestMessage.Headers.To;
                    }
                    this.OnSendHttpCookies(message, messageContext, requestUri);
                }
                else
                {
                    this.OnSendSoapContextHeader(message, messageContext);
                }
            }
            // verify that the callback context was not attached to an outgoing message
            CallbackContextMessageProperty dummy;
            if (CallbackContextMessageProperty.TryGet(message, out dummy))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CallbackContextNotExpectedOnOutgoingMessageAtServer, message.Headers.Action)));
            }
        }

        void OnReceiveHttpCookies(Message message)
        {
            object property;
            if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out property))
            {
                HttpRequestMessageProperty httpRequest = property as HttpRequestMessageProperty;
                if (httpRequest != null)
                {
                    string cookieHeader = httpRequest.Headers[HttpRequestHeader.Cookie];
                    ContextMessageProperty messageContext;
                    if (!string.IsNullOrEmpty(cookieHeader) && HttpCookieToolbox.TryCreateFromHttpCookieHeader(cookieHeader, out messageContext))
                    {
                        messageContext.AddOrReplaceInMessage(message);
                    }
                }
            }
        }

        void OnReceiveSoapContextHeader(Message message)
        {
            ContextMessageProperty messageContext = ContextMessageHeader.GetContextFromHeaderIfExists(message);
            if (messageContext != null)
            {
                messageContext.AddOrReplaceInMessage(message);

                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                        TraceCode.ContextProtocolContextRetrievedFromMessage,
                        SR.GetString(SR.TraceCodeContextProtocolContextRetrievedFromMessage),
                        this);
                }
            }
        }

        void OnSendHttpCookies(Message message, ContextMessageProperty context, Uri requestUri)
        {
            object tmpProperty;
            HttpResponseMessageProperty property = null;
            if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out tmpProperty))
            {
                property = tmpProperty as HttpResponseMessageProperty;
            }
            if (property == null)
            {
                property = new HttpResponseMessageProperty();
                message.Properties.Add(HttpResponseMessageProperty.Name, property);
            }
            string setCookieHeader = HttpCookieToolbox.EncodeContextAsHttpSetCookieHeader(context, requestUri);
            property.Headers.Add(HttpResponseHeader.SetCookie, setCookieHeader);
        }
    }
}
