//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Web;

    public class WebHttpBehavior : IEndpointBehavior, IWmiInstanceProvider
    {
        internal const string GET = "GET";
        internal const string POST = "POST";
        internal const string WildcardAction = "*";
        internal const string WildcardMethod = "*";
        internal static readonly string defaultStreamContentType = "application/octet-stream";
        internal static readonly string defaultCallbackParameterName = "callback";
        const string AddressPropertyName = "Address";
        WebMessageBodyStyle defaultBodyStyle;
        WebMessageFormat defaultOutgoingReplyFormat;
        WebMessageFormat defaultOutgoingRequestFormat;
        XmlSerializerOperationBehavior.Reflector reflector;
        UnwrappedTypesXmlSerializerManager xmlSerializerManager;

        public WebHttpBehavior()
        {
            defaultOutgoingRequestFormat = WebMessageFormat.Xml;
            defaultOutgoingReplyFormat = WebMessageFormat.Xml;
            this.defaultBodyStyle = WebMessageBodyStyle.Bare;
            xmlSerializerManager = new UnwrappedTypesXmlSerializerManager();
        }

        internal delegate void Effect();

        public virtual WebMessageBodyStyle DefaultBodyStyle
        {
            get { return this.defaultBodyStyle; }
            set
            {
                if (!WebMessageBodyStyleHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.defaultBodyStyle = value;
            }
        }

        public virtual WebMessageFormat DefaultOutgoingRequestFormat
        {
            get
            {
                return this.defaultOutgoingRequestFormat;
            }
            set
            {
                if (!WebMessageFormatHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.defaultOutgoingRequestFormat = value;
            }
        }

        public virtual WebMessageFormat DefaultOutgoingResponseFormat
        {
            get
            {
                return this.defaultOutgoingReplyFormat;
            }
            set
            {
                if (!WebMessageFormatHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.defaultOutgoingReplyFormat = value;
            }
        }

        public virtual bool HelpEnabled { get; set; }

        public virtual bool AutomaticFormatSelectionEnabled { get; set; }

        public virtual bool FaultExceptionEnabled { get; set; }

        internal Uri HelpUri { get; set; }

        protected internal string JavascriptCallbackParameterName { get; set; }

        public virtual void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // do nothing
        }

        public virtual void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (clientRuntime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clientRuntime");
            }
            WebMessageEncodingBindingElement webEncodingBindingElement = endpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>();
            if (webEncodingBindingElement != null && webEncodingBindingElement.CrossDomainScriptAccessEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.CrossDomainJavascriptNotsupported));
            }
#pragma warning disable 56506 // [....], endpoint.Contract is never null
            this.reflector = new XmlSerializerOperationBehavior.Reflector(endpoint.Contract.Namespace, null);
            foreach (OperationDescription od in endpoint.Contract.Operations)
#pragma warning restore 56506
            {
#pragma warning disable 56506 // [....], clientRuntime.Operations is never null
                if (clientRuntime.Operations.Contains(od.Name))
#pragma warning restore 56506
                {
                    ClientOperation cop = clientRuntime.Operations[od.Name];
                    IClientMessageFormatter requestClient = GetRequestClientFormatter(od, endpoint);
                    IClientMessageFormatter replyClient = GetReplyClientFormatter(od, endpoint);
                    cop.Formatter = new CompositeClientFormatter(requestClient, replyClient);
                    cop.SerializeRequest = true;
                    cop.DeserializeReply = od.Messages.Count > 1 && !IsUntypedMessage(od.Messages[1]);
                }
            }
            AddClientErrorInspector(endpoint, clientRuntime);
        }

        public virtual void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (endpointDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointDispatcher");
            }
            WebMessageEncodingBindingElement webEncodingBindingElement = endpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>();
            if (webEncodingBindingElement != null && webEncodingBindingElement.CrossDomainScriptAccessEnabled)
            {
                ISecurityCapabilities securityCapabilities = endpoint.Binding.GetProperty<ISecurityCapabilities>(new BindingParameterCollection());
                if (securityCapabilities.SupportsClientAuthentication)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.CrossDomainJavascriptAuthNotSupported));
                }
                if (endpoint.Contract.Behaviors.Contains(typeof(JavascriptCallbackBehaviorAttribute)))
                {
                    JavascriptCallbackBehaviorAttribute behavior = endpoint.Contract.Behaviors[typeof(JavascriptCallbackBehaviorAttribute)] as JavascriptCallbackBehaviorAttribute;
                    this.JavascriptCallbackParameterName = behavior.UrlParameterName;
                }
                else
                {
                    this.JavascriptCallbackParameterName = defaultCallbackParameterName;
                }
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new JavascriptCallbackMessageInspector(this.JavascriptCallbackParameterName));
            }
            if (this.HelpEnabled)
            {
                this.HelpUri = new UriTemplate(HelpPage.OperationListHelpPageUriTemplate).BindByPosition(endpoint.ListenUri);
            }
#pragma warning disable 56506 // [....], endpoint.Contract is never null
            this.reflector = new XmlSerializerOperationBehavior.Reflector(endpoint.Contract.Namespace, null);
#pragma warning restore 56506

            // endpoint filter
            endpointDispatcher.AddressFilter = new PrefixEndpointAddressMessageFilter(endpoint.Address);
            endpointDispatcher.ContractFilter = new MatchAllMessageFilter();
            // operation selector
#pragma warning disable 56506 // [....], endpointDispatcher.DispatchRuntime is never null
            endpointDispatcher.DispatchRuntime.OperationSelector = this.GetOperationSelector(endpoint);
#pragma warning restore 56506
            // unhandled operation
            string actionStarOperationName = null;
#pragma warning disable 56506 // [....], endpoint.Contract is never null
            foreach (OperationDescription od in endpoint.Contract.Operations)
#pragma warning restore 56506
            {
                if (od.Messages[0].Direction == MessageDirection.Input
                    && od.Messages[0].Action == WildcardAction)
                {
                    actionStarOperationName = od.Name;
                    break;
                }
            }
            if (actionStarOperationName != null)
            {
                // WCF v1 installs any Action="*" op into UnhandledDispatchOperation, but WebHttpBehavior
                // doesn't want this, so we 'move' that operation back into normal set of operations
#pragma warning disable 56506 // [....], endpointDispatcher.DispatchRuntime.{Operations,UnhandledDispatchOperation} is never null
                endpointDispatcher.DispatchRuntime.Operations.Add(
                    endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation);
#pragma warning restore 56506
            }

            FormatSelectingMessageInspector formatSelectingMessageInspector = null;
            string xmlContentType = null;
            string jsonContentType = null;

 
            if (webEncodingBindingElement != null)
            {
                XmlFormatMapping xmlFormatMapping = new XmlFormatMapping(webEncodingBindingElement.WriteEncoding, webEncodingBindingElement.ContentTypeMapper);
                JsonFormatMapping jsonFormatMapping = new JsonFormatMapping(webEncodingBindingElement.WriteEncoding, webEncodingBindingElement.ContentTypeMapper);
 
                xmlContentType = xmlFormatMapping.DefaultContentType.ToString();
                jsonContentType = jsonFormatMapping.DefaultContentType.ToString();
 
                if (AutomaticFormatSelectionEnabled)
                {
                    formatSelectingMessageInspector = new FormatSelectingMessageInspector(this, new List<MultiplexingFormatMapping> { xmlFormatMapping, jsonFormatMapping });
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add(formatSelectingMessageInspector);
                }
            }
            else
            {
                xmlContentType = TextMessageEncoderFactory.GetContentType(XmlFormatMapping.defaultMediaType, TextEncoderDefaults.Encoding);
                jsonContentType = JsonMessageEncoderFactory.GetContentType(null);
            }

#pragma warning disable 56506 // [....], endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation is never null
            // always install UnhandledDispatchOperation (WebHttpDispatchOperationSelector may choose not to use it)
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation = new DispatchOperation(endpointDispatcher.DispatchRuntime, "*", WildcardAction, WildcardAction);
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.DeserializeRequest = false;
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.SerializeReply = false;
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker = new HttpUnhandledOperationInvoker { HelpUri = this.HelpUri };
#pragma warning restore 56506
            // install formatters and parameter inspectors
            foreach (OperationDescription od in endpoint.Contract.Operations)
            {
                DispatchOperation dop = null;
#pragma warning disable 56506 // [....], endpointDispatcher.DispatchRuntime, DispatchRuntime.Operations are never null
                if (endpointDispatcher.DispatchRuntime.Operations.Contains(od.Name))
#pragma warning restore 56506
                {
                    dop = endpointDispatcher.DispatchRuntime.Operations[od.Name];
                }
#pragma warning disable 56506 // [....], endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation is never null
                else if (endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Name == od.Name)
                {
                    dop = endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation;
                }
#pragma warning restore 56506
                if (dop != null)
                {
                    IDispatchMessageFormatter requestDispatch = GetRequestDispatchFormatter(od, endpoint);
                    IDispatchMessageFormatter replyDispatch = GetReplyDispatchFormatter(od, endpoint);

                    MultiplexingDispatchMessageFormatter replyDispatchAsMultiplexing = replyDispatch as MultiplexingDispatchMessageFormatter;

                    if (replyDispatchAsMultiplexing != null)
                    {
                        // here we are adding all default content types, despite the fact that
                        // some of the formatters in MultiplexingDispatchMessageFormatter might not be present
                        // i.e. the JSON formatter

                        replyDispatchAsMultiplexing.DefaultContentTypes.Add(WebMessageFormat.Xml, xmlContentType);
                        replyDispatchAsMultiplexing.DefaultContentTypes.Add(WebMessageFormat.Json, jsonContentType);

                        if (formatSelectingMessageInspector != null)
                        {
                            formatSelectingMessageInspector.RegisterOperation(od.Name, replyDispatchAsMultiplexing);
                        }
                    }

                    dop.Formatter = new CompositeDispatchFormatter(requestDispatch, replyDispatch);
                    dop.FaultFormatter = new WebFaultFormatter(dop.FaultFormatter);
                    dop.DeserializeRequest = (requestDispatch != null);
                    dop.SerializeReply = od.Messages.Count > 1 && (replyDispatch != null);
                }
            }
            
            if (this.HelpEnabled)
            {
                HelpPage helpPage = new HelpPage(this, endpoint.Contract);
                DispatchOperation dispatchOperation = new DispatchOperation(endpointDispatcher.DispatchRuntime, HelpOperationInvoker.OperationName, null, null)
                {
                    DeserializeRequest = false,
                    SerializeReply = false,
                    Invoker = new HelpOperationInvoker(helpPage, endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker),
                };
                endpointDispatcher.DispatchRuntime.Operations.Add(dispatchOperation);
            }
            AddServerErrorHandlers(endpoint, endpointDispatcher);
        }

        internal virtual Dictionary<string, string> GetWmiProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("DefaultBodyStyle", this.DefaultBodyStyle.ToString());
            result.Add("DefaultOutgoingRequestFormat", this.DefaultOutgoingRequestFormat.ToString());
            result.Add("DefaultOutgoingResponseFormat", this.DefaultOutgoingResponseFormat.ToString());
            return result;
        }

        internal virtual string GetWmiTypeName()
        {
            return "WebHttpBehavior";
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            if (wmiInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("wmiInstance");
            }
            Dictionary<string, string> properties = this.GetWmiProperties();
            foreach (string key in properties.Keys)
            {
                wmiInstance.SetProperty(key, properties[key]);
            }
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return GetWmiTypeName();
        }

        public virtual void Validate(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            ValidateNoMessageHeadersPresent(endpoint);
            ValidateBinding(endpoint);
            ValidateContract(endpoint);
        }

        void ValidateNoMessageHeadersPresent(ServiceEndpoint endpoint)
        {
            if (endpoint == null || endpoint.Address == null)
            {
                return;
            }
            EndpointAddress address = endpoint.Address;
            if (address.Headers.Count > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.WebHttpServiceEndpointCannotHaveMessageHeaders, address)));
            }
        }

        protected virtual void ValidateBinding(ServiceEndpoint endpoint)
        {
            ValidateIsWebHttpBinding(endpoint, this.GetType().ToString());
        }

        internal static string GetWebMethod(OperationDescription od)
        {
            WebGetAttribute wga = od.Behaviors.Find<WebGetAttribute>();
            WebInvokeAttribute wia = od.Behaviors.Find<WebInvokeAttribute>();
            EnsureOk(wga, wia, od);
            if (wga != null)
            {
                return GET;
            }
            else if (wia != null)
            {
                return wia.Method ?? POST;
            }
            else
            {
                return POST;
            }
        }

        internal static string GetWebUriTemplate(OperationDescription od)
        {
            // return exactly what is on the attribute
            WebGetAttribute wga = od.Behaviors.Find<WebGetAttribute>();
            WebInvokeAttribute wia = od.Behaviors.Find<WebInvokeAttribute>();
            EnsureOk(wga, wia, od);
            if (wga != null)
            {
                return wga.UriTemplate;
            }
            else if (wia != null)
            {
                return wia.UriTemplate;
            }
            else
            {
                return null;
            }
        }

        internal static string GetDescription(OperationDescription od)
        {
            object[] attributes = null;
            if (od.SyncMethod != null)
            {
                attributes = od.SyncMethod.GetCustomAttributes(typeof(DescriptionAttribute), true);
            }
            else if (od.BeginMethod != null)
            {
                attributes = od.BeginMethod.GetCustomAttributes(typeof(DescriptionAttribute), true);
            }

            if (attributes != null && attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
            else
            {
                return String.Empty;
            }
        }

        internal static bool IsTypedMessage(MessageDescription message)
        {
            return (message != null && message.MessageType != null);
        }

        internal static bool IsUntypedMessage(MessageDescription message)
        {
            if (message == null)
            {
                return false;
            }
            return (message.Body.ReturnValue != null && message.Body.Parts.Count == 0 && message.Body.ReturnValue.Type == typeof(Message)) ||
                (message.Body.ReturnValue == null && message.Body.Parts.Count == 1 && message.Body.Parts[0].Type == typeof(Message));
        }

        internal static MessageDescription MakeDummyMessageDescription(MessageDirection direction)
        {
            MessageDescription messageDescription = new MessageDescription("urn:dummyAction", direction);
            return messageDescription;
        }

        internal static bool SupportsJsonFormat(OperationDescription od)
        {
            // if the type is XmlSerializable, then we cannot create a json serializer for it
            DataContractSerializerOperationBehavior dcsob = od.Behaviors.Find<DataContractSerializerOperationBehavior>();
            return (dcsob != null);
        }

        internal static void ValidateIsWebHttpBinding(ServiceEndpoint serviceEndpoint, string behaviorName)
        {
            Binding binding = serviceEndpoint.Binding;
            if (binding.Scheme != "http" && binding.Scheme != "https")
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.WCFBindingCannotBeUsedWithUriOperationSelectorBehaviorBadScheme,
                    serviceEndpoint.Contract.Name, behaviorName)));
            }
            if (binding.MessageVersion != MessageVersion.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.WCFBindingCannotBeUsedWithUriOperationSelectorBehaviorBadMessageVersion,
                    serviceEndpoint.Address.Uri.AbsoluteUri, behaviorName)));
            }
            TransportBindingElement transportBindingElement = binding.CreateBindingElements().Find<TransportBindingElement>();
            if (transportBindingElement != null && !transportBindingElement.ManualAddressing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.ManualAddressingCannotBeFalseWithTransportBindingElement,
                    serviceEndpoint.Address.Uri.AbsoluteUri, behaviorName, transportBindingElement.GetType().Name)));
            }
        }

        internal WebMessageBodyStyle GetBodyStyle(OperationDescription od)
        {
            WebGetAttribute wga = od.Behaviors.Find<WebGetAttribute>();
            WebInvokeAttribute wia = od.Behaviors.Find<WebInvokeAttribute>();
            EnsureOk(wga, wia, od);
            if (wga != null)
            {
                return wga.GetBodyStyleOrDefault(this.DefaultBodyStyle);
            }
            else if (wia != null)
            {
                return wia.GetBodyStyleOrDefault(this.DefaultBodyStyle);
            }
            else
            {
                return this.DefaultBodyStyle;
            }
        }

        internal IClientMessageFormatter GetDefaultClientFormatter(OperationDescription od, bool useJson, bool isWrapped)
        {
            DataContractSerializerOperationBehavior dcsob = od.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (useJson)
            {
                if (dcsob == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.JsonFormatRequiresDataContract, od.Name, od.DeclaringContract.Name, od.DeclaringContract.Namespace)));
                }
                return CreateDataContractJsonSerializerOperationFormatter(od, dcsob, isWrapped);
            }
            else
            {
                ClientRuntime clientRuntime = new ClientRuntime("name", "");
                ClientOperation cop = new ClientOperation(clientRuntime, "dummyClient", "urn:dummy");
                cop.Formatter = null;

                if (dcsob != null)
                {
                    (dcsob as IOperationBehavior).ApplyClientBehavior(od, cop);
                    return cop.Formatter;
                }
                XmlSerializerOperationBehavior xsob = od.Behaviors.Find<XmlSerializerOperationBehavior>();
                if (xsob != null)
                {
                    xsob = new XmlSerializerOperationBehavior(od, xsob.XmlSerializerFormatAttribute, this.reflector);
                    (xsob as IOperationBehavior).ApplyClientBehavior(od, cop);
                    return cop.Formatter;
                }
            }
            return null;
        }

        protected virtual void AddClientErrorInspector(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (!this.FaultExceptionEnabled)
            {
                clientRuntime.MessageInspectors.Add(new WebFaultClientMessageInspector());
            }
            else
            {
                clientRuntime.MessageVersionNoneFaultsEnabled = true;
            }
        }

        protected virtual void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (!this.FaultExceptionEnabled)
            {
                WebErrorHandler errorHandler = new WebErrorHandler(this, endpoint.Contract, endpointDispatcher.DispatchRuntime.ChannelDispatcher.IncludeExceptionDetailInFaults);
                endpointDispatcher.DispatchRuntime.ChannelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }

        protected virtual WebHttpDispatchOperationSelector GetOperationSelector(ServiceEndpoint endpoint)
        {
            return new WebHttpDispatchOperationSelector(endpoint);
        }

        protected virtual QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return new QueryStringConverter();
        }

        protected virtual IClientMessageFormatter GetReplyClientFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (operationDescription.Messages.Count < 2)
            {
                return null;
            }
            ValidateBodyParameters(operationDescription, false);
            Type type;
            if (TryGetStreamParameterType(operationDescription.Messages[1], operationDescription, false, out type))
            {
                return new HttpStreamFormatter(operationDescription);
            }
            if (IsUntypedMessage(operationDescription.Messages[1]))
            {
                return new MessagePassthroughFormatter();
            }
            WebMessageBodyStyle style = GetBodyStyle(operationDescription);
            Type parameterType;
            if (UseBareReplyFormatter(style, operationDescription, GetResponseFormat(operationDescription), out parameterType))
            {
                return SingleBodyParameterMessageFormatter.CreateXmlAndJsonClientFormatter(operationDescription, parameterType, false, this.xmlSerializerManager);
            }
            else
            {
                MessageDescription temp = operationDescription.Messages[0];
                operationDescription.Messages[0] = MakeDummyMessageDescription(MessageDirection.Input);
                IClientMessageFormatter result;
                result = GetDefaultXmlAndJsonClientFormatter(operationDescription, !IsBareResponse(style));
                operationDescription.Messages[0] = temp;
                return result;
            }
        }

        internal virtual bool UseBareReplyFormatter(WebMessageBodyStyle style, OperationDescription operationDescription, WebMessageFormat responseFormat, out Type parameterType)
        {
            parameterType = null;
            return IsBareResponse(style) && TryGetNonMessageParameterType(operationDescription.Messages[1], operationDescription, false, out parameterType);
        }

        protected virtual IDispatchMessageFormatter GetReplyDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (operationDescription.Messages.Count < 2)
            {
                return null;
            }
            ValidateBodyParameters(operationDescription, false);
            WebMessageFormat responseFormat = GetResponseFormat(operationDescription);

            //  Determine if we should add a json formatter; If the ResponseFormat is json, we always add the json formatter even if the
            //  operation is XmlSerializerFormat because the formatter constructor throws the exception: "json not valid with XmlSerializerFormat" [[....]]
            bool useJson = (responseFormat == WebMessageFormat.Json || SupportsJsonFormat(operationDescription));

            IDispatchMessageFormatter innerFormatter;
            Type type;

            if (TryGetStreamParameterType(operationDescription.Messages[1], operationDescription, false, out type))
            {
                innerFormatter = new ContentTypeSettingDispatchMessageFormatter(defaultStreamContentType, new HttpStreamFormatter(operationDescription));
            }
            else if (IsUntypedMessage(operationDescription.Messages[1]))
            {
                innerFormatter = new MessagePassthroughFormatter();
            }
            else
            {
                Type parameterType;
                WebMessageBodyStyle style = GetBodyStyle(operationDescription);
                Dictionary<WebMessageFormat, IDispatchMessageFormatter> formatters = new Dictionary<WebMessageFormat, IDispatchMessageFormatter>();

                if (UseBareReplyFormatter(style, operationDescription, responseFormat, out parameterType))
                {
                    formatters.Add(WebMessageFormat.Xml, SingleBodyParameterMessageFormatter.CreateDispatchFormatter(operationDescription, parameterType, false, false, this.xmlSerializerManager, null));
                    if (useJson)
                    {
                        formatters.Add(WebMessageFormat.Json, SingleBodyParameterMessageFormatter.CreateDispatchFormatter(operationDescription, parameterType, false, true, this.xmlSerializerManager, this.JavascriptCallbackParameterName));
                    }
                }
                else
                {
                    MessageDescription temp = operationDescription.Messages[0];
                    operationDescription.Messages[0] = MakeDummyMessageDescription(MessageDirection.Input);
                    formatters.Add(WebMessageFormat.Xml, GetDefaultDispatchFormatter(operationDescription, false, !IsBareResponse(style)));
                    if (useJson)
                    {
                        formatters.Add(WebMessageFormat.Json, GetDefaultDispatchFormatter(operationDescription, true, !IsBareResponse(style)));
                    }
                    operationDescription.Messages[0] = temp;
                }
                innerFormatter = new MultiplexingDispatchMessageFormatter(formatters, responseFormat);
            }

            return innerFormatter;
        }

        protected virtual IClientMessageFormatter GetRequestClientFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            WebMessageFormat requestFormat = GetRequestFormat(operationDescription);
            bool useJson = (requestFormat == WebMessageFormat.Json);
            WebMessageEncodingBindingElement webEncoding = (useJson) ? endpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>() : null;
            IClientMessageFormatter innerFormatter = null;

            // get some validation errors by creating "throwAway" formatter

            // validate that endpoint.Address is not null before accessing the endpoint.Address.Uri. This is to avoid throwing a NullRefException while constructing a UriTemplateClientFormatter
            if (endpoint.Address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR2.GetString(SR2.ServiceEndpointMustHaveNonNullAddress, typeof(ServiceEndpoint), typeof(ChannelFactory), typeof(WebHttpEndpoint), AddressPropertyName, typeof(ServiceEndpoint))));
            }

            UriTemplateClientFormatter throwAway = new UriTemplateClientFormatter(operationDescription, null, GetQueryStringConverter(operationDescription), endpoint.Address.Uri, false, endpoint.Contract.Name);
            int numUriVariables = throwAway.pathMapping.Count + throwAway.queryMapping.Count;
            bool isStream = false;
            HideReplyMessage(operationDescription, delegate()
            {
                WebMessageBodyStyle style = GetBodyStyle(operationDescription);
                bool isUntypedWhenUriParamsNotConsidered = false;
                Effect doBodyFormatter = delegate()
                {
                    if (numUriVariables != 0)
                    {
                        EnsureNotUntypedMessageNorMessageContract(operationDescription);
                    }
                    // get body formatter
                    ValidateBodyParameters(operationDescription, true);
                    IClientMessageFormatter baseFormatter;
                    Type parameterType;
                    if (TryGetStreamParameterType(operationDescription.Messages[0], operationDescription, true, out parameterType))
                    {
                        isStream = true;
                        baseFormatter = new HttpStreamFormatter(operationDescription);
                    }
                    else if (UseBareRequestFormatter(style, operationDescription, out parameterType))
                    {
                        baseFormatter = SingleBodyParameterMessageFormatter.CreateClientFormatter(operationDescription, parameterType, true, useJson, this.xmlSerializerManager);
                    }
                    else
                    {
                        baseFormatter = GetDefaultClientFormatter(operationDescription, useJson, !IsBareRequest(style));
                    }
                    innerFormatter = baseFormatter;
                    isUntypedWhenUriParamsNotConsidered = IsUntypedMessage(operationDescription.Messages[0]);
                };
                if (numUriVariables == 0)
                {
                    if (IsUntypedMessage(operationDescription.Messages[0]))
                    {
                        ValidateBodyParameters(operationDescription, true);
                        innerFormatter = new MessagePassthroughFormatter();
                        isUntypedWhenUriParamsNotConsidered = true;
                    }
                    else if (IsTypedMessage(operationDescription.Messages[0]))
                    {
                        ValidateBodyParameters(operationDescription, true);
                        innerFormatter = GetDefaultClientFormatter(operationDescription, useJson, !IsBareRequest(style));
                    }
                    else
                    {
                        doBodyFormatter();
                    }
                }
                else
                {
                    HideRequestUriTemplateParameters(operationDescription, throwAway, delegate()
                    {
                        CloneMessageDescriptionsBeforeActing(operationDescription, delegate()
                        {
                            doBodyFormatter();
                        });
                    });
                }
                innerFormatter = new UriTemplateClientFormatter(operationDescription, innerFormatter, GetQueryStringConverter(operationDescription), endpoint.Address.Uri, isUntypedWhenUriParamsNotConsidered, endpoint.Contract.Name);
            });
            string defaultContentType = GetDefaultContentType(isStream, useJson, webEncoding);
            if (!string.IsNullOrEmpty(defaultContentType))
            {
                innerFormatter = new ContentTypeSettingClientMessageFormatter(defaultContentType, innerFormatter);
            }
            return innerFormatter;
        }

        protected virtual IDispatchMessageFormatter GetRequestDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            IDispatchMessageFormatter result = null;
            // get some validation errors by creating "throwAway" formatter
            UriTemplateDispatchFormatter throwAway = new UriTemplateDispatchFormatter(operationDescription, null, GetQueryStringConverter(operationDescription), endpoint.Contract.Name, endpoint.Address.Uri);
            int numUriVariables = throwAway.pathMapping.Count + throwAway.queryMapping.Count;
            HideReplyMessage(operationDescription, delegate()
            {
                WebMessageBodyStyle style = GetBodyStyle(operationDescription);
                Effect doBodyFormatter = delegate()
                {
                    if (numUriVariables != 0)
                    {
                        EnsureNotUntypedMessageNorMessageContract(operationDescription);
                    }
                    // get body formatter
                    ValidateBodyParameters(operationDescription, true);
                    Type type;
                    if (TryGetStreamParameterType(operationDescription.Messages[0], operationDescription, true, out type))
                    {
                        result = new HttpStreamFormatter(operationDescription);
                    }
                    else
                    {
                        Type parameterType;
                        if (UseBareRequestFormatter(style, operationDescription, out parameterType))
                        {
                            result = SingleBodyParameterMessageFormatter.CreateXmlAndJsonDispatchFormatter(operationDescription, parameterType, true, this.xmlSerializerManager, this.JavascriptCallbackParameterName);
                        }
                        else
                        {
                            result = GetDefaultXmlAndJsonDispatchFormatter(operationDescription, !IsBareRequest(style));
                        }
                    }
                };
                if (numUriVariables == 0)
                {
                    if (IsUntypedMessage(operationDescription.Messages[0]))
                    {
                        ValidateBodyParameters(operationDescription, true);
                        result = new MessagePassthroughFormatter();
                    }
                    else if (IsTypedMessage(operationDescription.Messages[0]))
                    {
                        ValidateBodyParameters(operationDescription, true);
                        result = GetDefaultXmlAndJsonDispatchFormatter(operationDescription, !IsBareRequest(style));
                    }
                    else
                    {
                        doBodyFormatter();
                    }
                }
                else
                {
                    HideRequestUriTemplateParameters(operationDescription, throwAway, delegate()
                    {
                        CloneMessageDescriptionsBeforeActing(operationDescription, delegate()
                        {
                            doBodyFormatter();
                        });
                    });
                }
                result = new UriTemplateDispatchFormatter(operationDescription, result, GetQueryStringConverter(operationDescription), endpoint.Contract.Name, endpoint.Address.Uri);
            });
            return result;
        }

        static void CloneMessageDescriptionsBeforeActing(OperationDescription operationDescription, Effect effect)
        {
            MessageDescription originalRequest = operationDescription.Messages[0];
            bool thereIsAReply = operationDescription.Messages.Count > 1;
            MessageDescription originalReply = thereIsAReply ? operationDescription.Messages[1] : null;
            operationDescription.Messages[0] = originalRequest.Clone();
            if (thereIsAReply)
            {
                operationDescription.Messages[1] = originalReply.Clone();
            }
            effect();
            operationDescription.Messages[0] = originalRequest;
            if (thereIsAReply)
            {
                operationDescription.Messages[1] = originalReply;
            }
        }

        internal virtual bool UseBareRequestFormatter(WebMessageBodyStyle style, OperationDescription operationDescription, out Type parameterType)
        {
            parameterType = null;
            return IsBareRequest(style) && TryGetNonMessageParameterType(operationDescription.Messages[0], operationDescription, true, out parameterType);
        }

        static Collection<MessagePartDescription> CloneParts(MessageDescription md)
        {
            MessagePartDescriptionCollection bodyParameters = md.Body.Parts;
            Collection<MessagePartDescription> bodyParametersClone = new Collection<MessagePartDescription>();
            for (int i = 0; i < bodyParameters.Count; ++i)
            {
                MessagePartDescription copy = bodyParameters[i].Clone();
                bodyParametersClone.Add(copy);
            }
            return bodyParametersClone;
        }

        static void EnsureNotUntypedMessageNorMessageContract(OperationDescription operationDescription)
        {
            // Called when there are UriTemplate parameters.  UT does not compose with Message
            // or MessageContract because the SOAP and REST programming models must be uniform here.
            bool isUnadornedWebGet = false;
            if (GetWebMethod(operationDescription) == GET && GetWebUriTemplate(operationDescription) == null)
            {
                isUnadornedWebGet = true;
            }
            if (IsTypedMessage(operationDescription.Messages[0]))
            {
                if (isUnadornedWebGet)
                {
                    // WebGet will give you UriTemplate parameters by default.
                    // We need a special error message for this case to prevent confusion.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR2.GetString(SR2.GETCannotHaveMCParameter, operationDescription.Name, operationDescription.DeclaringContract.Name, operationDescription.Messages[0].MessageType.Name)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(
                        SR2.UTParamsDoNotComposeWithMessageContract, operationDescription.Name, operationDescription.DeclaringContract.Name)));
                }
            }

            if (IsUntypedMessage(operationDescription.Messages[0]))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(
                    SR2.UTParamsDoNotComposeWithMessage, operationDescription.Name, operationDescription.DeclaringContract.Name)));
            }
        }

        static void EnsureOk(WebGetAttribute wga, WebInvokeAttribute wia, OperationDescription od)
        {
            if (wga != null && wia != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.MultipleWebAttributes, od.Name, od.DeclaringContract.Name)));
            }
        }

        static void HideReplyMessage(OperationDescription operationDescription, Effect effect)
        {
            MessageDescription temp = null;
            if (operationDescription.Messages.Count > 1)
            {
                temp = operationDescription.Messages[1];
                operationDescription.Messages[1] = MakeDummyMessageDescription(MessageDirection.Output);
            }
            effect();
            if (operationDescription.Messages.Count > 1)
            {
                operationDescription.Messages[1] = temp;
            }
        }

        static void HideRequestUriTemplateParameters(OperationDescription operationDescription, UriTemplateClientFormatter throwAway, Effect effect)
        {
            HideRequestUriTemplateParameters(operationDescription, throwAway.pathMapping, throwAway.queryMapping, effect);
        }

        internal static void HideRequestUriTemplateParameters(OperationDescription operationDescription, UriTemplateDispatchFormatter throwAway, Effect effect)
        {
            HideRequestUriTemplateParameters(operationDescription, throwAway.pathMapping, throwAway.queryMapping, effect);
        }

        static void HideRequestUriTemplateParameters(OperationDescription operationDescription, Dictionary<int, string> pathMapping, Dictionary<int, KeyValuePair<string, Type>> queryMapping, Effect effect)
        {
            // mutate description to hide UriTemplate parameters
            Collection<MessagePartDescription> originalParts = CloneParts(operationDescription.Messages[0]);
            Collection<MessagePartDescription> parts = CloneParts(operationDescription.Messages[0]);
            operationDescription.Messages[0].Body.Parts.Clear();
            int newIndex = 0;
            for (int i = 0; i < parts.Count; ++i)
            {
                if (!pathMapping.ContainsKey(i) && !queryMapping.ContainsKey(i))
                {
                    operationDescription.Messages[0].Body.Parts.Add(parts[i]);
                    parts[i].Index = newIndex++;
                }
            }
            effect();
            // unmutate description
            operationDescription.Messages[0].Body.Parts.Clear();
            for (int i = 0; i < originalParts.Count; ++i)
            {
                operationDescription.Messages[0].Body.Parts.Add(originalParts[i]);
            }
        }

        static bool IsBareRequest(WebMessageBodyStyle style)
        {
            return (style == WebMessageBodyStyle.Bare || style == WebMessageBodyStyle.WrappedResponse);
        }

        static bool IsBareResponse(WebMessageBodyStyle style)
        {
            return (style == WebMessageBodyStyle.Bare || style == WebMessageBodyStyle.WrappedRequest);
        }

        internal static bool TryGetNonMessageParameterType(MessageDescription message, OperationDescription declaringOperation, bool isRequest, out Type type)
        {
            type = null;
            if (message == null)
            {
                return true;
            }
            if (IsTypedMessage(message) || IsUntypedMessage(message))
            {
                return false;
            }
            if (isRequest)
            {
                if (message.Body.Parts.Count > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.AtMostOneRequestBodyParameterAllowedForUnwrappedMessages, declaringOperation.Name, declaringOperation.DeclaringContract.Name)));
                }
                if (message.Body.Parts.Count == 1 && message.Body.Parts[0].Type != typeof(void))
                {
                    type = message.Body.Parts[0].Type;
                }
                return true;
            }
            else
            {
                if (message.Body.Parts.Count > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.OnlyReturnValueBodyParameterAllowedForUnwrappedMessages, declaringOperation.Name, declaringOperation.DeclaringContract.Name)));
                }
                if (message.Body.ReturnValue != null && message.Body.ReturnValue.Type != typeof(void))
                {
                    type = message.Body.ReturnValue.Type;
                }
                return true;
            }
        }

        static bool TryGetStreamParameterType(MessageDescription message, OperationDescription declaringOperation, bool isRequest, out Type type)
        {
            type = null;
            if (message == null || IsTypedMessage(message) || IsUntypedMessage(message))
            {
                return false;
            }
            if (isRequest)
            {
                bool hasStream = false;
                for (int i = 0; i < message.Body.Parts.Count; ++i)
                {
                    if (typeof(Stream) == message.Body.Parts[i].Type)
                    {
                        type = message.Body.Parts[i].Type;
                        hasStream = true;
                        break;
                    }

                }
                if (hasStream && message.Body.Parts.Count > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR2.GetString(SR2.AtMostOneRequestBodyParameterAllowedForStream, declaringOperation.Name, declaringOperation.DeclaringContract.Name)));
                }
                return hasStream;
            }
            else
            {
                // validate that the stream is not an out or ref param
                for (int i = 0; i < message.Body.Parts.Count; ++i)
                {
                    if (typeof(Stream) == message.Body.Parts[i].Type)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR2.GetString(SR2.NoOutOrRefStreamParametersAllowed, message.Body.Parts[i].Name, declaringOperation.Name, declaringOperation.DeclaringContract.Name)));
                    }
                }
                if (message.Body.ReturnValue != null && typeof(Stream) == message.Body.ReturnValue.Type)
                {
                    // validate that there are no out or ref params
                    if (message.Body.Parts.Count > 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR2.GetString(SR2.NoOutOrRefParametersAllowedWithStreamResult, declaringOperation.Name, declaringOperation.DeclaringContract.Name)));
                    }
                    type = message.Body.ReturnValue.Type;
                    return true;
                }

                else
                {
                    return false;
                }
            }
        }

        static void ValidateAtMostOneStreamParameter(OperationDescription operation, bool request)
        {
            Type dummy;
            if (request)
            {
                TryGetStreamParameterType(operation.Messages[0], operation, true, out dummy);
            }
            else
            {
                if (operation.Messages.Count > 1)
                {
                    TryGetStreamParameterType(operation.Messages[1], operation, false, out dummy);
                }
            }
        }

        string GetDefaultContentType(bool isStream, bool useJson, WebMessageEncodingBindingElement webEncoding)
        {
            if (isStream)
            {
                return defaultStreamContentType;
            }
            else if (useJson)
            {
                return JsonMessageEncoderFactory.GetContentType(webEncoding);
            }
            else
            {
                return null;
            }
        }

        IDispatchMessageFormatter GetDefaultDispatchFormatter(OperationDescription od, bool useJson, bool isWrapped)
        {
            DataContractSerializerOperationBehavior dcsob = od.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (useJson)
            {
                if (dcsob == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.JsonFormatRequiresDataContract, od.Name, od.DeclaringContract.Name, od.DeclaringContract.Namespace)));
                }
                return CreateDataContractJsonSerializerOperationFormatter(od, dcsob, isWrapped);
            }
            else
            {
                EndpointDispatcher dummyED = new EndpointDispatcher(new EndpointAddress("http://localhost/"), "name", "");
                DispatchRuntime dispatchRuntime = dummyED.DispatchRuntime;
                DispatchOperation dop = new DispatchOperation(dispatchRuntime, "dummyDispatch", "urn:dummy");
                dop.Formatter = null;

                if (dcsob != null)
                {
                    (dcsob as IOperationBehavior).ApplyDispatchBehavior(od, dop);
                    return dop.Formatter;
                }
                XmlSerializerOperationBehavior xsob = od.Behaviors.Find<XmlSerializerOperationBehavior>();
                if (xsob != null)
                {
                    xsob = new XmlSerializerOperationBehavior(od, xsob.XmlSerializerFormatAttribute, this.reflector);
                    (xsob as IOperationBehavior).ApplyDispatchBehavior(od, dop);
                    return dop.Formatter;
                }
            }
            return null;
        }

        internal virtual DataContractJsonSerializerOperationFormatter CreateDataContractJsonSerializerOperationFormatter(OperationDescription od, DataContractSerializerOperationBehavior dcsob, bool isWrapped)
        {
            return new DataContractJsonSerializerOperationFormatter(od, dcsob.MaxItemsInObjectGraph, dcsob.IgnoreExtensionDataObject, dcsob.DataContractSurrogate, isWrapped, false, JavascriptCallbackParameterName);
        }

        IClientMessageFormatter GetDefaultXmlAndJsonClientFormatter(OperationDescription od, bool isWrapped)
        {
            IClientMessageFormatter xmlFormatter = GetDefaultClientFormatter(od, false, isWrapped);
            if (!SupportsJsonFormat(od))
            {
                return xmlFormatter;
            }
            IClientMessageFormatter jsonFormatter = GetDefaultClientFormatter(od, true, isWrapped);
            Dictionary<WebContentFormat, IClientMessageFormatter> map = new Dictionary<WebContentFormat, IClientMessageFormatter>();
            map.Add(WebContentFormat.Xml, xmlFormatter);
            map.Add(WebContentFormat.Json, jsonFormatter);
            // In case there is no format property, the default formatter to use is XML
            return new DemultiplexingClientMessageFormatter(map, xmlFormatter);
        }

        IDispatchMessageFormatter GetDefaultXmlAndJsonDispatchFormatter(OperationDescription od, bool isWrapped)
        {
            IDispatchMessageFormatter xmlFormatter = GetDefaultDispatchFormatter(od, false, isWrapped);
            if (!SupportsJsonFormat(od))
            {
                return xmlFormatter;
            }
            IDispatchMessageFormatter jsonFormatter = GetDefaultDispatchFormatter(od, true, isWrapped);
            Dictionary<WebContentFormat, IDispatchMessageFormatter> map = new Dictionary<WebContentFormat, IDispatchMessageFormatter>();
            map.Add(WebContentFormat.Xml, xmlFormatter);
            map.Add(WebContentFormat.Json, jsonFormatter);
            return new DemultiplexingDispatchMessageFormatter(map, xmlFormatter);
        }

        internal WebMessageFormat GetRequestFormat(OperationDescription od)
        {
            WebGetAttribute wga = od.Behaviors.Find<WebGetAttribute>();
            WebInvokeAttribute wia = od.Behaviors.Find<WebInvokeAttribute>();
            EnsureOk(wga, wia, od);
            if (wga != null)
            {
                return wga.IsRequestFormatSetExplicitly ? wga.RequestFormat : this.DefaultOutgoingRequestFormat;
            }
            else if (wia != null)
            {
                return wia.IsRequestFormatSetExplicitly ? wia.RequestFormat : this.DefaultOutgoingRequestFormat;
            }
            else
            {
                return this.DefaultOutgoingRequestFormat;
            }
        }

        internal WebMessageFormat GetResponseFormat(OperationDescription od)
        {
            WebGetAttribute wga = od.Behaviors.Find<WebGetAttribute>();
            WebInvokeAttribute wia = od.Behaviors.Find<WebInvokeAttribute>();
            EnsureOk(wga, wia, od);
            if (wga != null)
            {
                return wga.IsResponseFormatSetExplicitly ? wga.ResponseFormat : this.DefaultOutgoingResponseFormat;
            }
            else if (wia != null)
            {
                return wia.IsResponseFormatSetExplicitly ? wia.ResponseFormat : this.DefaultOutgoingResponseFormat;
            }
            else
            {
                return this.DefaultOutgoingResponseFormat;
            }
        }

        void ValidateBodyParameters(OperationDescription operation, bool request)
        {
            string method = GetWebMethod(operation);
            if (request)
            {
                ValidateGETHasNoBody(operation, method);
            }
            // validate that if bare is chosen for request/response, then at most 1 parameter is possible
            ValidateBodyStyle(operation, request);
            // validate if the request or response body is a stream, no other body parameters
            // can be specified
            ValidateAtMostOneStreamParameter(operation, request);
        }

        void ValidateBodyStyle(OperationDescription operation, bool request)
        {
            WebMessageBodyStyle style = GetBodyStyle(operation);
            Type dummy;
            if (request && IsBareRequest(style))
            {
                TryGetNonMessageParameterType(operation.Messages[0], operation, true, out dummy);
            }
            if (!request && operation.Messages.Count > 1 && IsBareResponse(style))
            {
                TryGetNonMessageParameterType(operation.Messages[1], operation, false, out dummy);
            }
        }

        void ValidateGETHasNoBody(OperationDescription operation, string method)
        {
            if (method == GET)
            {
                if (!IsUntypedMessage(operation.Messages[0]) && operation.Messages[0].Body.Parts.Count != 0)
                {
                    if (!IsTypedMessage(operation.Messages[0]))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR2.GetString(SR2.GETCannotHaveBody, operation.Name, operation.DeclaringContract.Name, operation.Messages[0].Body.Parts[0].Name)));
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR2.GetString(SR2.GETCannotHaveMCParameter, operation.Name, operation.DeclaringContract.Name, operation.Messages[0].MessageType.Name)));
                    }
                }
            }
        }

        void ValidateContract(ServiceEndpoint endpoint)
        {
            foreach (OperationDescription od in endpoint.Contract.Operations)
            {
                ValidateNoOperationHasEncodedXmlSerializer(od);
                ValidateNoMessageContractHeaders(od.Messages[0], od.Name, endpoint.Contract.Name);
                ValidateNoBareMessageContractWithMultipleParts(od.Messages[0], od.Name, endpoint.Contract.Name);
                ValidateNoMessageContractWithStream(od.Messages[0], od.Name, endpoint.Contract.Name);
                if (od.Messages.Count > 1)
                {
                    ValidateNoMessageContractHeaders(od.Messages[1], od.Name, endpoint.Contract.Name);
                    ValidateNoBareMessageContractWithMultipleParts(od.Messages[1], od.Name, endpoint.Contract.Name);
                    ValidateNoMessageContractWithStream(od.Messages[1], od.Name, endpoint.Contract.Name);
                }
            }
        }

        internal static bool IsXmlSerializerFaultFormat(OperationDescription operationDescription)
        {
            XmlSerializerOperationBehavior xsob = operationDescription.Behaviors.Find<XmlSerializerOperationBehavior>();
            return (xsob != null && xsob.XmlSerializerFormatAttribute.SupportFaults);
        }

        void ValidateNoMessageContractWithStream(MessageDescription md, string opName, string contractName)
        {
            if (IsTypedMessage(md))
            {
                foreach (MessagePartDescription description in md.Body.Parts)
                {
                    if (description.Type == typeof(Stream))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(System.ServiceModel.SR2.GetString(System.ServiceModel.SR2.StreamBodyMemberNotSupported, this.GetType().ToString(), contractName, opName, md.MessageType.ToString(), description.Name)));
                    }
                }
            }
        }

        void ValidateNoOperationHasEncodedXmlSerializer(OperationDescription od)
        {
            XmlSerializerOperationBehavior xsob = od.Behaviors.Find<XmlSerializerOperationBehavior>();
            if (xsob != null && (xsob.XmlSerializerFormatAttribute.Style == OperationFormatStyle.Rpc || xsob.XmlSerializerFormatAttribute.IsEncoded))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.RpcEncodedNotSupportedForNoneMessageVersion, od.Name, od.DeclaringContract.Name, od.DeclaringContract.Namespace)));
            }
        }

        void ValidateNoBareMessageContractWithMultipleParts(MessageDescription md, string opName, string contractName)
        {
            if (IsTypedMessage(md) && md.Body.WrapperName == null)
            {
                if (md.Body.Parts.Count > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR2.GetString(SR2.InvalidMessageContractWithoutWrapperName, opName, contractName, md.MessageType)));
                }
                if (md.Body.Parts.Count == 1 && md.Body.Parts[0].Multiple)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.MCAtMostOneRequestBodyParameterAllowedForUnwrappedMessages, opName, contractName, md.MessageType)));
                }
            }
        }

        void ValidateNoMessageContractHeaders(MessageDescription md, string opName, string contractName)
        {
            if (md.Headers.Count != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.InvalidMethodWithSOAPHeaders, opName, contractName)));
            }
        }

        internal class MessagePassthroughFormatter : IClientMessageFormatter, IDispatchMessageFormatter
        {
            public object DeserializeReply(Message message, object[] parameters)
            {
                return message;
            }

            public void DeserializeRequest(Message message, object[] parameters)
            {
                parameters[0] = message;
            }

            public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
            {
                return result as Message;
            }

            public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
            {
                return parameters[0] as Message;
            }
        }

        static internal JavascriptCallbackResponseMessageProperty TrySetupJavascriptCallback(string callbackParameterName)
        {
            JavascriptCallbackResponseMessageProperty javascriptProperty = null;
            if (!String.IsNullOrEmpty(callbackParameterName) &&
                !OperationContext.Current.OutgoingMessageProperties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptProperty))
            {
                UriTemplateMatch match = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
                if (match != null &&
                    match.QueryParameters.AllKeys.Contains(callbackParameterName))
                {
                    string callbackName = match.QueryParameters[callbackParameterName];

                    if (!String.IsNullOrEmpty(callbackName))
                    {
                        javascriptProperty = new JavascriptCallbackResponseMessageProperty
                        {
                            CallbackFunctionName = callbackName
                        };
                        OperationContext.Current.OutgoingMessageProperties.Add(JavascriptCallbackResponseMessageProperty.Name, javascriptProperty);
                    }
                }
            }
            return javascriptProperty;
        }
    }
}
