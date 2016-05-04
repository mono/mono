//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Syndication;
    using System.ServiceModel.Web;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    class WebErrorHandler : IErrorHandler
    {
        WebHttpBehavior webHttpBehavior;
        ContractDescription contractDescription;
        bool includeExceptionDetailInFaults;

        public WebErrorHandler(WebHttpBehavior webHttpBehavior, ContractDescription contractDescription, bool includeExceptionDetailInFaults)
        {
            this.webHttpBehavior = webHttpBehavior;
            this.contractDescription = contractDescription;
            this.includeExceptionDetailInFaults = includeExceptionDetailInFaults;
        }

        public bool HandleError(Exception error)
        {
            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (version != MessageVersion.None || error == null)
            {
                return;
            }

            // If the exception is not derived from FaultException and the fault message is already present
            //   then only another error handler could have provided the fault so we should not replace it
            FaultException errorAsFaultException = error as FaultException;
            if (errorAsFaultException == null && fault != null)
            {
                return;
            }

            try
            {
                if (error is IWebFaultException)
                {
                    IWebFaultException webFaultException = (IWebFaultException)error;
                    WebOperationContext context = WebOperationContext.Current;
                    context.OutgoingResponse.StatusCode = webFaultException.StatusCode;
                    string operationName;
                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue<string>(WebHttpDispatchOperationSelector.HttpOperationNamePropertyName, out operationName))
                    {
                        OperationDescription description = this.contractDescription.Operations.Find(operationName);
                        bool isXmlSerializerFaultFormat = WebHttpBehavior.IsXmlSerializerFaultFormat(description);
                        if (isXmlSerializerFaultFormat && WebOperationContext.Current.OutgoingResponse.Format == WebMessageFormat.Json)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.JsonFormatRequiresDataContract, description.Name, description.DeclaringContract.Name, description.DeclaringContract.Namespace)));
                        }
                        WebMessageFormat? nullableFormat = !isXmlSerializerFaultFormat ? context.OutgoingResponse.Format : WebMessageFormat.Xml;
                        WebMessageFormat format = nullableFormat.HasValue ? nullableFormat.Value : this.webHttpBehavior.GetResponseFormat(description);
                        if (webFaultException.DetailObject != null)
                        {
                            switch (format)
                            {
                                case WebMessageFormat.Json:
                                    fault = context.CreateJsonResponse(webFaultException.DetailObject, new DataContractJsonSerializer(webFaultException.DetailType, webFaultException.KnownTypes));
                                    break;
                                case WebMessageFormat.Xml:
                                    if (isXmlSerializerFaultFormat)
                                    {
                                        fault = context.CreateXmlResponse(webFaultException.DetailObject, new XmlSerializer(webFaultException.DetailType, webFaultException.KnownTypes));
                                    }
                                    else
                                    {
                                        fault = context.CreateXmlResponse(webFaultException.DetailObject, new DataContractSerializer(webFaultException.DetailType, webFaultException.KnownTypes));
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            HttpResponseMessageProperty property;
                            if (OperationContext.Current.OutgoingMessageProperties.TryGetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name, out property) &&
                                property != null)
                            {
                                property.SuppressEntityBody = true;
                            }
                            if (format == WebMessageFormat.Json)
                            {
                                fault.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
                            }
                        }
                    }
                    else
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.OperationNameNotFound));
                    }
                }
                else
                {
                    fault = CreateHtmlResponse(error);
                }

            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    System.ServiceModel.DiagnosticUtility.TraceHandledException(new InvalidOperationException(SR2.GetString(SR2.HelpPageFailedToCreateErrorMessage)), TraceEventType.Warning);
                }

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                fault = CreateHtmlResponse(ex);
            }
        }

        Message CreateHtmlResponse(Exception error)
        {
            // Note: WebOperationContext may not be present in case of an invalid HTTP request
            Uri helpUri = null;
            if (WebOperationContext.Current != null)
            {
                helpUri = this.webHttpBehavior.HelpUri != null ? UriTemplate.RewriteUri(this.webHttpBehavior.HelpUri, WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host]) : null;
            }
            StreamBodyWriter bodyWriter;
            if (this.includeExceptionDetailInFaults)
            {
                bodyWriter = StreamBodyWriter.CreateStreamBodyWriter(s => HelpHtmlBuilder.CreateServerErrorPage(helpUri, error).Save(s, SaveOptions.OmitDuplicateNamespaces));
            }
            else
            {
                bodyWriter = StreamBodyWriter.CreateStreamBodyWriter(s => HelpHtmlBuilder.CreateServerErrorPage(helpUri, null).Save(s, SaveOptions.OmitDuplicateNamespaces));
            }
            Message response = new HttpStreamMessage(bodyWriter);
            response.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);

            HttpResponseMessageProperty responseProperty = GetResponseProperty(WebOperationContext.Current, response);
            if (!responseProperty.HasStatusCodeBeenSet)
            {
                responseProperty.StatusCode = HttpStatusCode.BadRequest;
            }
            responseProperty.Headers[HttpResponseHeader.ContentType] = Atom10Constants.HtmlMediaType;
            return response;
        }

        static HttpResponseMessageProperty GetResponseProperty(WebOperationContext currentContext, Message response)
        {
            HttpResponseMessageProperty responseProperty; 
            if (currentContext != null)
            {
                responseProperty = currentContext.OutgoingResponse.MessageProperty;
            }
            else
            {
                responseProperty = new HttpResponseMessageProperty();
                response.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
            }
            return responseProperty;
        }
    }
}
