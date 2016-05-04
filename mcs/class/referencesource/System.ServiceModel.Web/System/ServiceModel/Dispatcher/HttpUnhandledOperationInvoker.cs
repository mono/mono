//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Web;
    using System.ServiceModel.Syndication;
    using System.Xml.Linq;
    using System.ServiceModel.Description;

    internal class HttpUnhandledOperationInvoker : IOperationInvoker
    {
        const string HtmlContentType = "text/html; charset=UTF-8";

        public bool IsSynchronous
        {
            get { return true; }
        }
        public object[] AllocateInputs()
        {
            return new object[1];
        }

        public Uri HelpUri { get; set; }

        [SuppressMessage("Reliability", "Reliability104:CaughtAndHandledExceptionsRule", Justification = "The exception is thrown for tracing purposes")]
        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {            
            Message message = inputs[0] as Message;
            outputs = null;
#pragma warning disable 56506 // [....], message.Properties is never null
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.HttpUnhandledOperationInvokerCalledWithoutMessage)));
            }
            // We might be here because we desire a redirect...
            Uri newLocation = null;
            Uri to = message.Headers.To;
            if (message.Properties.ContainsKey(WebHttpDispatchOperationSelector.RedirectPropertyName))
            {
                newLocation = message.Properties[WebHttpDispatchOperationSelector.RedirectPropertyName] as Uri;
            }
            if (newLocation != null && to != null)
            {
                // ...redirect
                Message redirectResult = WebOperationContext.Current.CreateStreamResponse(s => HelpHtmlBuilder.CreateTransferRedirectPage(to.AbsoluteUri, newLocation.AbsoluteUri).Save(s, SaveOptions.OmitDuplicateNamespaces), Atom10Constants.HtmlMediaType);
                WebOperationContext.Current.OutgoingResponse.Location = newLocation.AbsoluteUri;
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.TemporaryRedirect;
                WebOperationContext.Current.OutgoingResponse.ContentType = HtmlContentType;

                // Note that no exception is thrown along this path, even if the debugger is attached
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.WebRequestRedirect,
                        SR2.GetString(SR2.TraceCodeWebRequestRedirect, to, newLocation));
                }
                return redirectResult;
            }
            // otherwise we are here to issue either a 404 or a 405
            bool uriMatched = false;
            if (message.Properties.ContainsKey(WebHttpDispatchOperationSelector.HttpOperationSelectorUriMatchedPropertyName))
            {
                uriMatched = (bool) message.Properties[WebHttpDispatchOperationSelector.HttpOperationSelectorUriMatchedPropertyName];
            }
#pragma warning enable 56506
            Message result = null;
            Uri helpUri = this.HelpUri != null ? UriTemplate.RewriteUri(this.HelpUri, WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host]) : null;
            if (uriMatched)
            {
                WebHttpDispatchOperationSelectorData allowedMethodsData = null;
                if (message.Properties.TryGetValue(WebHttpDispatchOperationSelector.HttpOperationSelectorDataPropertyName, out allowedMethodsData))
                {
                    WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.Allow] = allowedMethodsData.AllowHeader;
                }
                result = WebOperationContext.Current.CreateStreamResponse(s => HelpHtmlBuilder.CreateMethodNotAllowedPage(helpUri).Save(s, SaveOptions.OmitDuplicateNamespaces), Atom10Constants.HtmlMediaType);
            }
            else
            {
                result = WebOperationContext.Current.CreateStreamResponse(s => HelpHtmlBuilder.CreateEndpointNotFound(helpUri).Save(s, SaveOptions.OmitDuplicateNamespaces), Atom10Constants.HtmlMediaType);

            }
            WebOperationContext.Current.OutgoingResponse.StatusCode = uriMatched ? HttpStatusCode.MethodNotAllowed : HttpStatusCode.NotFound;
            WebOperationContext.Current.OutgoingResponse.ContentType = HtmlContentType;

            try
            {
                if (!uriMatched)
                {
                    if (Debugger.IsAttached)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.WebRequestDidNotMatchOperation,
                            OperationContext.Current.IncomingMessageHeaders.To)));
                    }
                    else
                    {
                        DiagnosticUtility.TraceHandledException(new InvalidOperationException(SR2.GetString(SR2.WebRequestDidNotMatchOperation,
                                OperationContext.Current.IncomingMessageHeaders.To)), TraceEventType.Warning);
                    }
                }
                else
                {
                    if (Debugger.IsAttached)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.WebRequestDidNotMatchMethod,
                            WebOperationContext.Current.IncomingRequest.Method, OperationContext.Current.IncomingMessageHeaders.To)));
                    }
                    else
                    {
                        DiagnosticUtility.TraceHandledException(new InvalidOperationException(SR2.GetString(SR2.WebRequestDidNotMatchMethod,
                                WebOperationContext.Current.IncomingRequest.Method, OperationContext.Current.IncomingMessageHeaders.To)), TraceEventType.Warning);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // catch the exception - its only used for tracing
            }
            return result;
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }
    }
}

