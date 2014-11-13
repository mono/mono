//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.Web
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.ServiceModel.Dispatcher;

    public class WebOperationContext : IExtension<OperationContext>
    {
        internal static readonly string DefaultTextMediaType = "text/plain";
        internal static readonly string DefaultJsonMediaType = JsonGlobals.applicationJsonMediaType;
        internal static readonly string DefaultXmlMediaType = "application/xml";
        internal static readonly string DefaultAtomMediaType = "application/atom+xml";
        internal static readonly string DefaultStreamMediaType = WebHttpBehavior.defaultStreamContentType;

        OperationContext operationContext;

        public WebOperationContext(OperationContext operationContext)
        {
            if (operationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationContext");
            }
            this.operationContext = operationContext;
#pragma warning disable 56506 // [....], operationContext.Extensions is never null
            if (operationContext.Extensions.Find<WebOperationContext>() == null)
            {
                operationContext.Extensions.Add(this);
            }
#pragma warning enable 56506
        }

        public static WebOperationContext Current
        {
            get
            {
                if (OperationContext.Current == null)
                {
                    return null;
                }
                WebOperationContext existing = OperationContext.Current.Extensions.Find<WebOperationContext>();
                if (existing != null)
                {
                    return existing;
                }
                return new WebOperationContext(OperationContext.Current);
            }
        }

        public IncomingWebRequestContext IncomingRequest
        { 
            get { return new IncomingWebRequestContext(this.operationContext); } 
        }

        public IncomingWebResponseContext IncomingResponse
        { 
            get { return new IncomingWebResponseContext(this.operationContext); } 
        }

        public OutgoingWebRequestContext OutgoingRequest
        { 
            get { return new OutgoingWebRequestContext(this.operationContext); } 
        }

        public OutgoingWebResponseContext OutgoingResponse
        { 
            get { return new OutgoingWebResponseContext(this.operationContext); } 
        }

        public void Attach(OperationContext owner)
        {
        }
        
        public void Detach(OperationContext owner)
        {
        }        

        public Message CreateJsonResponse<T>(T instance)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            return CreateJsonResponse<T>(instance, serializer);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "CreateJsonMessage requires the DataContractJsonSerializer.  Allowing the base type XmlObjectSerializer would let deveopers supply a non-Json Serializer.")]
        public Message CreateJsonResponse<T>(T instance, DataContractJsonSerializer serializer)
        {
            if (serializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            Message message = Message.CreateMessage(MessageVersion.None, (string)null, instance, serializer);
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
            AddContentType(WebOperationContext.DefaultJsonMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        public Message CreateXmlResponse<T>(T instance)
        {
            System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            return CreateXmlResponse(instance, serializer);
        }

        public Message CreateXmlResponse<T>(T instance, System.Runtime.Serialization.XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            Message message = Message.CreateMessage(MessageVersion.None, (string)null, instance, serializer);
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
            AddContentType(WebOperationContext.DefaultXmlMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        public Message CreateXmlResponse<T>(T instance, XmlSerializer serializer)
        {
            if (serializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            Message message = Message.CreateMessage(MessageVersion.None, (string)null, new XmlSerializerBodyWriter(instance, serializer));
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
            AddContentType(WebOperationContext.DefaultXmlMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Other XNode derived types such as XAttribute don't make sense in this context, so we are not using the XNode base type.")]
        public Message CreateXmlResponse(XDocument document)
        {
            if (document == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("document");
            }
            Message message;
            if (document.FirstNode == null)
            {
                message = Message.CreateMessage(MessageVersion.None, (string)null);
            }
            else
            {
                message = Message.CreateMessage(MessageVersion.None, (string)null, document.CreateReader());
            }
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
            AddContentType(WebOperationContext.DefaultXmlMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Other XNode derived types such as XAttribute don't make sense in this context, so we are not using the XNode base type.")]
        public Message CreateXmlResponse(XElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            Message message = Message.CreateMessage(MessageVersion.None, (string)null, element.CreateReader());
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
            AddContentType(WebOperationContext.DefaultXmlMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        public Message CreateAtom10Response(SyndicationItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            Message message = Message.CreateMessage(MessageVersion.None, (string)null, item.GetAtom10Formatter());
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
            AddContentType(WebOperationContext.DefaultAtomMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        public Message CreateAtom10Response(SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            Message message = Message.CreateMessage(MessageVersion.None, (string)null, feed.GetAtom10Formatter());
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
            AddContentType(WebOperationContext.DefaultAtomMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        public Message CreateAtom10Response(ServiceDocument document)
        {
            if (document == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("document");
            }
            Message message = Message.CreateMessage(MessageVersion.None, (string)null, document.GetFormatter());
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
            AddContentType(WebOperationContext.DefaultAtomMediaType, this.OutgoingResponse.BindingWriteEncoding);
            return message;
        }

        public Message CreateTextResponse(string text)
        {
            return CreateTextResponse(text, WebOperationContext.DefaultTextMediaType, Encoding.UTF8);
        }

        public Message CreateTextResponse(string text, string contentType)
        {
            return CreateTextResponse(text, contentType, Encoding.UTF8);
        }

        public Message CreateTextResponse(string text, string contentType, Encoding encoding)
        {
            if (text == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("text");
            }
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
            }
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            }

            Message message = new HttpStreamMessage(StreamBodyWriter.CreateStreamBodyWriter((stream) =>
            {
                byte[] preamble = encoding.GetPreamble();
                if (preamble.Length > 0)
                {
                    stream.Write(preamble, 0, preamble.Length);
                }
                byte[] bytes = encoding.GetBytes(text);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }));
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);
            AddContentType(contentType, null);
            return message;
        }

        public Message CreateTextResponse(Action<TextWriter> textWriter, string contentType)
        {
            Encoding encoding = this.OutgoingResponse.BindingWriteEncoding;
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return CreateTextResponse(textWriter, contentType, encoding);
        }

        public Message CreateTextResponse(Action<TextWriter> textWriter, string contentType, Encoding encoding)
        {
            if (textWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("textWriter");
            }
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
            }
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            }

            Message message = new HttpStreamMessage(StreamBodyWriter.CreateStreamBodyWriter((stream) =>
            {
                using (TextWriter writer = new StreamWriter(stream, encoding))
                {
                    textWriter(writer);
                }
            }));
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);
            AddContentType(contentType, null);
            return message;
        }

        public Message CreateStreamResponse(Stream stream, string contentType)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
            }
            Message message = ByteStreamMessage.CreateMessage(stream);
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);
            AddContentType(contentType, null);
            return message;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Using the StreamBodyWriter type instead of the BodyWriter type for discoverability.  The StreamBodyWriter provides a helpful overload of the OnWriteBodyContents method that takes a Stream")]
        public Message CreateStreamResponse(StreamBodyWriter bodyWriter, string contentType)
        {
            if (bodyWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bodyWriter");
            }
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
            }
            Message message = new HttpStreamMessage(bodyWriter);
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);
            AddContentType(contentType, null);
            return message;
        }

        public Message CreateStreamResponse(Action<Stream> streamWriter, string contentType)
        {
            if (streamWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("streamWriter");
            }
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
            }
            Message message = new HttpStreamMessage(StreamBodyWriter.CreateStreamBodyWriter(streamWriter));
            message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);
            AddContentType(contentType, null);
            return message;
        }

        public UriTemplate GetUriTemplate(string operationName)
        {
            if (String.IsNullOrEmpty(operationName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationName");
            }

            WebHttpDispatchOperationSelector selector = OperationContext.Current.EndpointDispatcher.DispatchRuntime.OperationSelector as WebHttpDispatchOperationSelector;
            if (selector == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.OperationSelectorNotWebSelector, typeof(WebHttpDispatchOperationSelector))));
            }
            return selector.GetUriTemplate(operationName);
        }

        void AddContentType(string contentType, Encoding encoding)
        {      
            if (string.IsNullOrEmpty(this.OutgoingResponse.ContentType))
            {
                if (encoding != null)
                {
                    contentType = WebMessageEncoderFactory.GetContentType(contentType, encoding);
                }
                this.OutgoingResponse.ContentType = contentType;
            }
        }

        class XmlSerializerBodyWriter : BodyWriter
        {
            object instance;
            XmlSerializer serializer;

            public XmlSerializerBodyWriter(object instance, XmlSerializer serializer)
                : base(false)
            {
                if (instance == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
                }
                if (serializer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
                }
                this.instance = instance;
                this.serializer = serializer;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                serializer.Serialize(writer, instance);
            }
        }
    }
}

