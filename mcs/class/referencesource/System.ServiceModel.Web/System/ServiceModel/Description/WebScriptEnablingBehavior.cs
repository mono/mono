//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Description
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Web;
    using System.Xml;

    public sealed class WebScriptEnablingBehavior : WebHttpBehavior
    {
        static readonly DataContractJsonSerializer jsonFaultSerializer = new DataContractJsonSerializer(typeof(JsonFaultDetail));
        static readonly WebMessageBodyStyle webScriptBodyStyle = WebMessageBodyStyle.WrappedRequest;
        static readonly WebMessageFormat webScriptDefaultMessageFormat = WebMessageFormat.Json;
        const int MaxMetadataEndpointBufferSize = 2048;
        WebMessageFormat requestMessageFormat = webScriptDefaultMessageFormat;
        WebMessageFormat responseMessageFormat = webScriptDefaultMessageFormat;

        public WebScriptEnablingBehavior()
        {
        }

        public override WebMessageBodyStyle DefaultBodyStyle
        {
            get
            {
                return webScriptBodyStyle;
            }
            set
            {
                if (value != webScriptBodyStyle)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.BodyStyleNotSupportedByWebScript, value, this.GetType().Name, webScriptBodyStyle)));
                }
            }
        }

        public override WebMessageFormat DefaultOutgoingRequestFormat
        {
            get
            {
                return this.requestMessageFormat;
            }
            set
            {
                if (!WebMessageFormatHelper.IsDefined(value))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.requestMessageFormat = value;
            }
        }

        public override WebMessageFormat DefaultOutgoingResponseFormat
        {
            get
            {
                return this.responseMessageFormat;
            }
            set
            {
                if (!WebMessageFormatHelper.IsDefined(value))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.responseMessageFormat = value;
            }
        }

        public override bool HelpEnabled
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.HelpPageNotSupportedInScripts)));
                }
            }
        }

        public override bool AutomaticFormatSelectionEnabled
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.AutomaticFormatSelectionNotSupportedInScripts)));
                }
            }
        }

        public override bool FaultExceptionEnabled
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.FaultExceptionEnabledNotSupportedInScripts)));
                }
            }
        }

        public override void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            base.ApplyClientBehavior(endpoint, clientRuntime);
#pragma warning disable 56506 // Microsoft, clientRuntime.MessageInspectors is never null
            clientRuntime.MessageInspectors.Add(new JsonClientMessageInspector());
#pragma warning restore 56506
        }

        public override void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            base.ApplyDispatchBehavior(endpoint, endpointDispatcher);

            try
            {
                AddMetadataEndpoint(endpoint, endpointDispatcher, false); //  debugMode 
                AddMetadataEndpoint(endpoint, endpointDispatcher, true); //  debugMode 
            }
            catch (XmlException exception)
            {
                // Microsoft, need to reference this resource string although fix for 13332 was removed
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.InvalidXmlCharactersInNameUsedWithPOSTMethod, string.Empty, string.Empty, string.Empty), exception));
            }
        }

        public override void Validate(ServiceEndpoint endpoint)
        {
            base.Validate(endpoint);

#pragma warning disable 56506 // Microsoft, endpoint.Contract is never null
            foreach (OperationDescription operation in endpoint.Contract.Operations)
#pragma warning restore 56506
            {
                if (operation.Behaviors.Find<XmlSerializerOperationBehavior>() != null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR2.GetString(SR2.WebScriptNotSupportedForXmlSerializerFormat, typeof(XmlSerializerFormatAttribute).Name, this.GetType().ToString())));
                }
                string method = WebHttpBehavior.GetWebMethod(operation);
                if (method != WebHttpBehavior.GET
                    && method != WebHttpBehavior.POST)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR2.GetString(SR2.WebScriptInvalidHttpRequestMethod, operation.Name,
                        endpoint.Contract.Name, method, this.GetType().ToString())));
                }
                WebGetAttribute webGetAttribute = operation.Behaviors.Find<WebGetAttribute>();
                if (webGetAttribute != null && webGetAttribute.UriTemplate != null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR2.GetString(SR2.WebScriptNotSupportedForXmlSerializerFormat, typeof(UriTemplate).Name, this.GetType().ToString())));
                }
                WebInvokeAttribute webInvokeAttribute = operation.Behaviors.Find<WebInvokeAttribute>();
                if (webInvokeAttribute != null && webInvokeAttribute.UriTemplate != null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR2.GetString(SR2.WebScriptNotSupportedForXmlSerializerFormat, typeof(UriTemplate).Name, this.GetType().ToString())));
                }
                WebMessageBodyStyle bodyStyle = GetBodyStyle(operation);
                if (bodyStyle != webScriptBodyStyle)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.BodyStyleNotSupportedByWebScript, bodyStyle, this.GetType().Name, webScriptBodyStyle)));
                }

                foreach (MessageDescription messageDescription in operation.Messages)
                {
                    if (!messageDescription.IsTypedMessage &&
                        (messageDescription.Direction == MessageDirection.Output) &&
                        (messageDescription.Body.Parts.Count > 0))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR2.GetString(SR2.WebScriptOutRefOperationsNotSupported, operation.Name,
                            endpoint.Contract.Name)));
                    }
                }
            }
        }

        internal override DataContractJsonSerializerOperationFormatter CreateDataContractJsonSerializerOperationFormatter(OperationDescription od, DataContractSerializerOperationBehavior dcsob, bool isWrapped)
        {
            return new DataContractJsonSerializerOperationFormatter(od, dcsob.MaxItemsInObjectGraph, dcsob.IgnoreExtensionDataObject, dcsob.DataContractSurrogate, isWrapped, true, this.JavascriptCallbackParameterName);
        }

        internal override string GetWmiTypeName()
        {
            return "WebScriptEnablingBehavior";
        }

        internal override bool UseBareReplyFormatter(WebMessageBodyStyle style, OperationDescription operationDescription, WebMessageFormat responseFormat, out Type parameterType)
        {
            if (responseFormat == WebMessageFormat.Json)
            {
                parameterType = null;
                return false;
            }
            return base.UseBareReplyFormatter(style, operationDescription, responseFormat, out parameterType);
        }

        protected override void AddClientErrorInspector(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new JsonClientMessageInspector());
        }

        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher.ChannelDispatcher == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "endpointDispatcher", SR2.GetString(SR2.ChannelDispatcherMustBePresent));
            }
#pragma warning disable 56506 // Microsoft, endpointDispatcher.ChannelDispatcher.ErrorHandlers never null
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new JsonErrorHandler(endpoint, endpointDispatcher.ChannelDispatcher.IncludeExceptionDetailInFaults));
#pragma warning restore 56506
        }

        protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return new JsonQueryStringConverter(operationDescription);
        }

        void AddMetadataEndpoint(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher, bool debugMode)
        {
            Uri baseAddress = endpoint.Address.Uri;
            if (baseAddress == null)
            {
                return;
            }

            ServiceHostBase host = endpointDispatcher.ChannelDispatcher.Host;

            UriBuilder builder = new UriBuilder(baseAddress);
            builder.Path += builder.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase)
                ? (WebScriptClientGenerator.GetMetadataEndpointSuffix(debugMode))
                : ("/" + WebScriptClientGenerator.GetMetadataEndpointSuffix(debugMode));
            EndpointAddress metadataAddress = new EndpointAddress(builder.Uri);

            foreach (ServiceEndpoint serviceEndpoint in host.Description.Endpoints)
            {
                if (EndpointAddress.UriEquals(serviceEndpoint.Address.Uri, metadataAddress.Uri, true, false))//  ignoreCase //  includeHostNameInComparison 
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.JsonNoEndpointAtMetadataAddress, this.GetType().ToString(), serviceEndpoint.Address, serviceEndpoint.Name, host.Description.Name)));
                }
            }

            HttpTransportBindingElement transportBindingElement;
            HttpTransportBindingElement existingTransportBindingElement = endpoint.Binding.CreateBindingElements().Find<HttpTransportBindingElement>();

            if (existingTransportBindingElement != null)
            {
                transportBindingElement = (HttpTransportBindingElement)existingTransportBindingElement.Clone();
            }
            else
            {
                if (baseAddress.Scheme == "https")
                {
                    transportBindingElement = new HttpsTransportBindingElement();
                }
                else
                {
                    transportBindingElement = new HttpTransportBindingElement();
                }
            }

            transportBindingElement.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            transportBindingElement.TransferMode = TransferMode.Buffered;
            transportBindingElement.MaxBufferSize = MaxMetadataEndpointBufferSize;
            transportBindingElement.MaxReceivedMessageSize = MaxMetadataEndpointBufferSize;
            Binding metadataBinding = new CustomBinding(
                new WebScriptMetadataMessageEncodingBindingElement(),
                transportBindingElement);
            BindingParameterCollection parameters = host.GetBindingParameters(endpoint);

            // build endpoint dispatcher
            ContractDescription metadataContract = ContractDescription.GetContract(typeof(ServiceMetadataExtension.IHttpGetMetadata));
            OperationDescription metadataOperation = metadataContract.Operations[0];
            EndpointDispatcher metadataEndpointDispatcher = new EndpointDispatcher(metadataAddress, metadataContract.Name, metadataContract.Namespace);
            DispatchOperation dispatchOperation = new DispatchOperation(metadataEndpointDispatcher.DispatchRuntime, metadataOperation.Name, metadataOperation.Messages[0].Action, metadataOperation.Messages[1].Action);
            dispatchOperation.Formatter = new WebScriptMetadataFormatter();
            dispatchOperation.Invoker = new SyncMethodInvoker(metadataOperation.SyncMethod);
            metadataEndpointDispatcher.DispatchRuntime.Operations.Add(dispatchOperation);
            metadataEndpointDispatcher.DispatchRuntime.SingletonInstanceContext = new InstanceContext(host, new WebScriptClientGenerator(endpoint, debugMode, !String.IsNullOrEmpty(this.JavascriptCallbackParameterName)));
            metadataEndpointDispatcher.DispatchRuntime.InstanceContextProvider = new SingletonInstanceContextProvider(metadataEndpointDispatcher.DispatchRuntime);

            // build channel dispatcher
            IChannelListener<IReplyChannel> listener = null;
            if (metadataBinding.CanBuildChannelListener<IReplyChannel>(parameters))
            {
                listener = metadataBinding.BuildChannelListener<IReplyChannel>(metadataAddress.Uri, parameters);
            }
            ChannelDispatcher metadataChannelDispatcher = new ChannelDispatcher(listener);
            metadataChannelDispatcher.MessageVersion = MessageVersion.None;
            metadataChannelDispatcher.Endpoints.Add(metadataEndpointDispatcher);

            host.ChannelDispatchers.Add(metadataChannelDispatcher);
        }

        class JsonClientMessageInspector : WebFaultClientMessageInspector
        {
            public override void AfterReceiveReply(ref Message reply, object correlationState)
            {
                bool callBase = true;
                if (reply != null)
                {
                    object responseProperty = reply.Properties[HttpResponseMessageProperty.Name];
                    if (responseProperty != null)
                    {
                        if (((HttpResponseMessageProperty)responseProperty).Headers[JsonGlobals.jsonerrorString] == JsonGlobals.trueString)
                        {
                            callBase = false;
                            XmlDictionaryReader reader = reply.GetReaderAtBodyContents();
                            JsonFaultDetail faultDetail = jsonFaultSerializer.ReadObject(reader) as JsonFaultDetail;
                            FaultCode faultCode = new FaultCode(FaultCodeConstants.Codes.InternalServiceFault, FaultCodeConstants.Namespaces.NetDispatch);
                            faultCode = FaultCode.CreateReceiverFaultCode(faultCode);
                            if (faultDetail != null)
                            {
                                if (faultDetail.ExceptionDetail != null)
                                {
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        new FaultException<ExceptionDetail>(faultDetail.ExceptionDetail, faultDetail.Message, faultCode));
                                }
                                else
                                {
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        new FaultException(MessageFault.CreateFault(faultCode, faultDetail.Message)));
                                }
                            }
                            else
                            {
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    new FaultException(MessageFault.CreateFault(faultCode,
                                    System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInternalServerError))));
                            }
                        }
                    }
                }
                if (callBase)
                {
                    base.AfterReceiveReply(ref reply, correlationState);
                }
            }
        }

        class JsonErrorHandler : IErrorHandler
        {
            bool includeExceptionDetailInFaults;
            string outgoingContentType;

            public JsonErrorHandler(ServiceEndpoint endpoint, bool includeExceptionDetailInFaults)
            {
                WebMessageEncodingBindingElement webMEBE = endpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>();
                outgoingContentType = JsonMessageEncoderFactory.GetContentType(webMEBE);
                this.includeExceptionDetailInFaults = includeExceptionDetailInFaults;
            }

            public bool HandleError(Exception error)
            {
                return false;
            }

            public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
            {
                HttpResponseMessageProperty responseProperty;
                if (fault == null)
                {
                    FaultCode code = new FaultCode(FaultCodeConstants.Codes.InternalServiceFault, FaultCodeConstants.Namespaces.NetDispatch);
                    code = FaultCode.CreateReceiverFaultCode(code);
                    string action = FaultCodeConstants.Actions.NetDispatcher;

                    MessageFault innerFault;
                    innerFault = MessageFault.CreateFault(code, new FaultReason(error.Message, CultureInfo.CurrentCulture), new ExceptionDetail(error));
                    fault = Message.CreateMessage(version, action, new JsonFaultBodyWriter(innerFault, this.includeExceptionDetailInFaults));

                    responseProperty = new HttpResponseMessageProperty();
                    fault.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
                }
                else
                {
                    MessageFault innerFault = MessageFault.CreateFault(fault, TransportDefaults.MaxFaultSize);
                    Message newMessage = Message.CreateMessage(version, fault.Headers.Action, new JsonFaultBodyWriter(innerFault, this.includeExceptionDetailInFaults));
                    newMessage.Headers.To = fault.Headers.To;
                    newMessage.Properties.CopyProperties(fault.Properties);

                    object property = null;
                    if (newMessage.Properties.TryGetValue(HttpResponseMessageProperty.Name, out property))
                    {
                        responseProperty = (HttpResponseMessageProperty)property;
                    }
                    else
                    {
                        responseProperty = new HttpResponseMessageProperty();
                        newMessage.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
                    }

                    fault.Close();
                    fault = newMessage;
                }
                responseProperty.Headers.Add(HttpResponseHeader.ContentType, outgoingContentType);
                responseProperty.Headers.Add(JsonGlobals.jsonerrorString, JsonGlobals.trueString);
                responseProperty.StatusCode = System.Net.HttpStatusCode.InternalServerError;

                object bodyFormatPropertyObject;
                if (fault.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out bodyFormatPropertyObject))
                {
                    WebBodyFormatMessageProperty bodyFormatProperty = bodyFormatPropertyObject as WebBodyFormatMessageProperty;
                    if ((bodyFormatProperty == null) ||
                        (bodyFormatProperty.Format != WebContentFormat.Json))
                    {
                        fault.Properties[WebBodyFormatMessageProperty.Name] = WebBodyFormatMessageProperty.JsonProperty;
                    }
                }
                else
                {
                    fault.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
                }
            }

            class JsonFaultBodyWriter : BodyWriter
            {
                JsonFaultDetail faultDetail;

                public JsonFaultBodyWriter(MessageFault fault, bool includeExceptionDetailInFaults)
                    : base(false)
                {
                    faultDetail = new JsonFaultDetail();
                    if (includeExceptionDetailInFaults)
                    {
                        faultDetail.Message = fault.Reason.ToString();
                        if (fault.HasDetail)
                        {
                            try
                            {
                                ExceptionDetail originalFaultDetail = fault.GetDetail<ExceptionDetail>();
                                faultDetail.StackTrace = originalFaultDetail.StackTrace;
                                faultDetail.ExceptionType = originalFaultDetail.Type;
                                faultDetail.ExceptionDetail = originalFaultDetail;
                            }
                            catch (SerializationException exception)
                            {
                                System.ServiceModel.DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                                // A SerializationException will be thrown if the detail isn't of type ExceptionDetail
                                // In that case, we want to just move on.
                            }
                            catch (SecurityException exception)
                            {
                                System.ServiceModel.DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                                // A SecurityException will be thrown if the detail can't be obtained in partial trust
                                // (This is guaranteed to happen unless there's an Assert for MemberAccessPermission, since ExceptionDetail
                                //     has DataMembers that have private setters.)
                                // In that case, we want to just move on.
                            }
                        }
                    }
                    else
                    {
                        faultDetail.Message = System.ServiceModel.SR.GetString(System.ServiceModel.SR.SFxInternalServerError);
                    }
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    jsonFaultSerializer.WriteObject(writer, faultDetail);
                }
            }
        }
    }
}
