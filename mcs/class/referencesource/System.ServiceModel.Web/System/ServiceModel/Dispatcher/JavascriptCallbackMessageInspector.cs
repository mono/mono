//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Dispatcher
{
    using System.Net;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Web;
    using System.ServiceModel.Description;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    class JavascriptCallbackMessageInspector : IDispatchMessageInspector
    {
        internal static readonly string applicationJavaScriptMediaType = "application/x-javascript";

        public JavascriptCallbackMessageInspector(string callbackParameterName)
        {
            this.CallbackParameterName = callbackParameterName;
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.JsonpCallbackNameSet, SR2.GetString(SR2.TraceCodeJsonpCallbackNameSet, callbackParameterName));
            }
        }

        string CallbackParameterName { get; set; }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (HttpContext.Current != null &&
                HttpContext.Current.User != null &&
                HttpContext.Current.User.Identity != null &&
                HttpContext.Current.User.Identity.IsAuthenticated)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.CrossDomainJavascriptAuthNotSupported));
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            WebBodyFormatMessageProperty formatProperty;
            JavascriptCallbackResponseMessageProperty javascriptCallbackResponseMessageProperty = null;
            if (reply.Properties.TryGetValue<WebBodyFormatMessageProperty>(WebBodyFormatMessageProperty.Name, out formatProperty) &&
                formatProperty != null &&
                formatProperty.Format == WebContentFormat.Json)
            {
                if (!reply.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptCallbackResponseMessageProperty)
                    || javascriptCallbackResponseMessageProperty == null)
                {
                    javascriptCallbackResponseMessageProperty = WebHttpBehavior.TrySetupJavascriptCallback(this.CallbackParameterName);
                    if (javascriptCallbackResponseMessageProperty != null)
                    {
                        reply.Properties.Add(JavascriptCallbackResponseMessageProperty.Name, javascriptCallbackResponseMessageProperty);
                    }
                }
                if (javascriptCallbackResponseMessageProperty != null)
                {                    
                    HttpResponseMessageProperty property;
                    if (reply.Properties.TryGetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name, out property) &&
                        property != null)
                    {
                        property.Headers[HttpResponseHeader.ContentType] = applicationJavaScriptMediaType;
                        if (javascriptCallbackResponseMessageProperty.StatusCode == null)
                        {
                            javascriptCallbackResponseMessageProperty.StatusCode = property.StatusCode;
                        }
                        property.StatusCode = HttpStatusCode.OK;

                        if (property.SuppressEntityBody)
                        {
                            property.SuppressEntityBody = false;
                            Message nullJsonMessage = WebOperationContext.Current.CreateJsonResponse<object>(null);
                            nullJsonMessage.Properties.CopyProperties(reply.Properties);
                            reply = nullJsonMessage;
                        }
                    }
                }
            }
        }
    }
}
