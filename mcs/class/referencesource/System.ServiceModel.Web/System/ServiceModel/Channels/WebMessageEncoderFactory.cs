//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;
    using System.Diagnostics;

    class WebMessageEncoderFactory : MessageEncoderFactory
    {
        WebMessageEncoder messageEncoder;

        public WebMessageEncoderFactory(Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas, WebContentTypeMapper contentTypeMapper, bool javascriptCallbackEnabled)
        {
            messageEncoder = new WebMessageEncoder(writeEncoding, maxReadPoolSize, maxWritePoolSize, quotas, contentTypeMapper, javascriptCallbackEnabled);
        }

        public override MessageEncoder Encoder
        {
            get { return messageEncoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return messageEncoder.MessageVersion; }
        }

        internal static string GetContentType(string mediaType, Encoding encoding)
        {
            string charset = TextEncoderDefaults.EncodingToCharSet(encoding);
            if (!string.IsNullOrEmpty(charset))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}; charset={1}", mediaType, charset);
            }
            return mediaType;
        }

        class WebMessageEncoder : MessageEncoder
        {
            const string defaultMediaType = "application/xml";
            WebContentTypeMapper contentTypeMapper;
            string defaultContentType;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile MessageEncoder jsonMessageEncoder;
            int maxReadPoolSize;
            int maxWritePoolSize;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile MessageEncoder rawMessageEncoder;
            XmlDictionaryReaderQuotas readerQuotas;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile MessageEncoder textMessageEncoder;
            object thisLock;
            Encoding writeEncoding;
            bool javascriptCallbackEnabled;

            public WebMessageEncoder(Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas, WebContentTypeMapper contentTypeMapper, bool javascriptCallbackEnabled)
            {
                if (writeEncoding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");
                }

                this.thisLock = new object();

                TextEncoderDefaults.ValidateEncoding(writeEncoding);
                this.writeEncoding = writeEncoding;

                this.maxReadPoolSize = maxReadPoolSize;
                this.maxWritePoolSize = maxWritePoolSize;
                this.contentTypeMapper = contentTypeMapper;
                this.javascriptCallbackEnabled = javascriptCallbackEnabled;

                this.readerQuotas = new XmlDictionaryReaderQuotas();
                quotas.CopyTo(this.readerQuotas);

                this.defaultContentType = GetContentType(defaultMediaType, writeEncoding);
            }

            public override string ContentType
            {
                get { return this.defaultContentType; }
            }

            public override string MediaType
            {
                get { return defaultMediaType; }
            }

            public override MessageVersion MessageVersion
            {
                get { return MessageVersion.None; }
            }

            MessageEncoder JsonMessageEncoder
            {
                get
                {
                    if (jsonMessageEncoder == null)
                    {
                        lock (ThisLock)
                        {
                            if (jsonMessageEncoder == null)
                            {
                                jsonMessageEncoder = new JsonMessageEncoderFactory(writeEncoding, maxReadPoolSize, maxWritePoolSize, readerQuotas, javascriptCallbackEnabled).Encoder;
                            }
                        }
                    }
                    return jsonMessageEncoder;
                }
            }

            MessageEncoder RawMessageEncoder
            {
                get
                {
                    if (rawMessageEncoder == null)
                    {
                        lock (ThisLock)
                        {
                            if (rawMessageEncoder == null)
                            {
                                rawMessageEncoder = new ByteStreamMessageEncodingBindingElement(readerQuotas).CreateMessageEncoderFactory().Encoder;
                                ((IWebMessageEncoderHelper)rawMessageEncoder).EnableBodyReaderMoveToContent(); // see the comments in IWebMessageEncoderHelper for why this is done
                            }
                        }
                    }
                    return rawMessageEncoder;
                }
            }

            MessageEncoder TextMessageEncoder
            {
                get
                {
                    if (textMessageEncoder == null)
                    {
                        lock (ThisLock)
                        {
                            if (textMessageEncoder == null)
                            {
                                textMessageEncoder = new TextMessageEncoderFactory(MessageVersion.None, writeEncoding, maxReadPoolSize, maxWritePoolSize, readerQuotas).Encoder;
                            }
                        }
                    }
                    return textMessageEncoder;
                }
            }

            object ThisLock
            {
                get { return thisLock; }
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                if (contentType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
                }

                WebContentFormat messageFormat;
                if (TryGetContentTypeMapping(contentType, out messageFormat) &&
                    (messageFormat != WebContentFormat.Default))
                {
                    return true;
                }

                return RawMessageEncoder.IsContentTypeSupported(contentType) || JsonMessageEncoder.IsContentTypeSupported(contentType) || TextMessageEncoder.IsContentTypeSupported(contentType);
            }

            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                if (bufferManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bufferManager"));
                }

                WebContentFormat format = GetFormatForContentType(contentType);
                Message message;

                switch (format)
                {
                    case WebContentFormat.Json:
                        message = JsonMessageEncoder.ReadMessage(buffer, bufferManager, contentType);
                        message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
                        break;
                    case WebContentFormat.Xml:
                        message = TextMessageEncoder.ReadMessage(buffer, bufferManager, contentType);
                        message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
                        break;
                    case WebContentFormat.Raw:
                        message = RawMessageEncoder.ReadMessage(buffer, bufferManager, contentType);
                        message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);
                        break;
                    default:
                        throw Fx.AssertAndThrow("This should never get hit because GetFormatForContentType shouldn't return a WebContentFormat other than Json, Xml, and Raw");
                }
                return message;
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                if (stream == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
                }

                WebContentFormat format = GetFormatForContentType(contentType);
                Message message;
                switch (format)
                {
                    case WebContentFormat.Json:
                        message = JsonMessageEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
                        message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
                        break;
                    case WebContentFormat.Xml:
                        message = TextMessageEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
                        message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.XmlProperty);
                        break;
                    case WebContentFormat.Raw:
                        message = RawMessageEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
                        message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.RawProperty);
                        break;
                    default:
                        throw Fx.AssertAndThrow("This should never get hit because GetFormatForContentType shouldn't return a WebContentFormat other than Json, Xml, and Raw");
                }
                return message;
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (bufferManager == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("bufferManager"), message);
                }
                if (maxMessageSize < 0)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxMessageSize", maxMessageSize,
                        SR2.GetString(SR2.ValueMustBeNonNegative)), message);
                }
                if (messageOffset < 0 || messageOffset > maxMessageSize)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageOffset", messageOffset,
                        SR2.GetString(SR2.JsonValueMustBeInRange, 0, maxMessageSize)), message);
                }
                ThrowIfMismatchedMessageVersion(message);

                WebContentFormat messageFormat = ExtractFormatFromMessage(message);
                JavascriptCallbackResponseMessageProperty javascriptResponseMessageProperty;
                switch (messageFormat)
                {
                    case WebContentFormat.Json:
                        return JsonMessageEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                    case WebContentFormat.Xml:
                        if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty) &&
                            javascriptResponseMessageProperty != null &&
                            !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                        {
                            throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotsupported), message);
                        }
                        return TextMessageEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                    case WebContentFormat.Raw:
                        if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty) &&
                            javascriptResponseMessageProperty != null &&
                            !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                        {
                            throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotsupported), message);
                        }
                        return RawMessageEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                    default:
                        throw Fx.AssertAndThrow("This should never get hit because GetFormatForContentType shouldn't return a WebContentFormat other than Json, Xml, and Raw");
                }
            }

            public override void WriteMessage(Message message, Stream stream)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (stream == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("stream"), message);
                }
                ThrowIfMismatchedMessageVersion(message);

                WebContentFormat messageFormat = ExtractFormatFromMessage(message);
                JavascriptCallbackResponseMessageProperty javascriptResponseMessageProperty;
                switch (messageFormat)
                {
                    case WebContentFormat.Json:
                        JsonMessageEncoder.WriteMessage(message, stream);
                        break;
                    case WebContentFormat.Xml:
                        if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty) &&
                            javascriptResponseMessageProperty != null &&
                            !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                        {
                            throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotsupported), message);
                        }
                        TextMessageEncoder.WriteMessage(message, stream);
                        break;
                    case WebContentFormat.Raw:
                        if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty) &&
                            javascriptResponseMessageProperty != null &&
                            !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                        {
                            throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotsupported), message);
                        }
                        RawMessageEncoder.WriteMessage(message, stream);
                        break;
                    default:
                        throw Fx.AssertAndThrow("This should never get hit because GetFormatForContentType shouldn't return a WebContentFormat other than Json, Xml, and Raw");
                }
            }

            public override IAsyncResult BeginWriteMessage(Message message, Stream stream, AsyncCallback callback, object state)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (stream == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("stream"), message);
                }

                ThrowIfMismatchedMessageVersion(message);

                return new WriteMessageAsyncResult(message, stream, this, callback, state);
            }

            public override void EndWriteMessage(IAsyncResult result)
            {
                WriteMessageAsyncResult.End(result);
            }

            internal override bool IsCharSetSupported(string charSet)
            {
                Encoding tmp;
                return TextEncoderDefaults.TryGetEncoding(charSet, out tmp);
            }

            WebContentFormat ExtractFormatFromMessage(Message message)
            {
                object messageFormatProperty;
                message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out messageFormatProperty);
                if (messageFormatProperty == null)
                {
                    return WebContentFormat.Xml;
                }

                WebBodyFormatMessageProperty typedMessageFormatProperty = messageFormatProperty as WebBodyFormatMessageProperty;
                if ((typedMessageFormatProperty == null) ||
                    (typedMessageFormatProperty.Format == WebContentFormat.Default))
                {
                    return WebContentFormat.Xml;
                }

                return typedMessageFormatProperty.Format;
            }

            WebContentFormat GetFormatForContentType(string contentType)
            {
                WebContentFormat messageFormat;

                if (TryGetContentTypeMapping(contentType, out messageFormat) &&
                    (messageFormat != WebContentFormat.Default))
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        if (string.IsNullOrEmpty(contentType))
                        {
                            contentType = "<null>";
                        }
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.RequestFormatSelectedFromContentTypeMapper,
                            SR2.GetString(SR2.TraceCodeRequestFormatSelectedFromContentTypeMapper, messageFormat.ToString(), contentType));
                    }
                    return messageFormat;
                }

                // Don't pass on null content types to IsContentTypeSupported methods -- they might throw.
                // If null content type isn't already mapped, return the default format of Raw.

                if (contentType == null)
                {
                    messageFormat = WebContentFormat.Raw;
                }
                else if (JsonMessageEncoder.IsContentTypeSupported(contentType))
                {
                    messageFormat = WebContentFormat.Json;
                }
                else if (TextMessageEncoder.IsContentTypeSupported(contentType))
                {
                    messageFormat = WebContentFormat.Xml;
                }
                else
                {
                    messageFormat = WebContentFormat.Raw;
                }

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information,
                        TraceCode.RequestFormatSelectedByEncoderDefaults,
                        SR2.GetString(SR2.TraceCodeRequestFormatSelectedByEncoderDefaults, messageFormat.ToString(), contentType));
                }

                return messageFormat;
            }

            bool TryGetContentTypeMapping(string contentType, out WebContentFormat format)
            {
                if (contentTypeMapper == null)
                {
                    format = WebContentFormat.Default;
                    return false;
                }

                try
                {
                    format = contentTypeMapper.GetMessageFormatForContentType(contentType);
                    if (!WebContentFormatHelper.IsDefined(format))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR2.GetString(SR2.UnknownWebEncodingFormat, contentType, format)));
                    }
                    return true;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(
                        SR2.GetString(SR2.ErrorEncounteredInContentTypeMapper), e));
                }
            }

            class WriteMessageAsyncResult : ScheduleActionItemAsyncResult
            {
                Message message;
                Stream stream;
                MessageEncoder encoder;
                WebMessageEncoder webMessageEncoder;
                static AsyncCompletion handleEndWriteMessage;

                public WriteMessageAsyncResult(Message message, Stream stream, WebMessageEncoder webMessageEncoder, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.message = message;
                    this.stream = stream;
                    this.webMessageEncoder = webMessageEncoder;

                    WebContentFormat messageFormat = webMessageEncoder.ExtractFormatFromMessage(message);
                    JavascriptCallbackResponseMessageProperty javascriptResponseMessageProperty;

                    switch (messageFormat)
                    {
                        case WebContentFormat.Json:
                            this.encoder = webMessageEncoder.JsonMessageEncoder;
                            this.Schedule();
                            break;

                        case WebContentFormat.Xml:
                            if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty) &&
                                javascriptResponseMessageProperty != null &&
                                !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                            {
                                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotsupported), message);
                            }
                            this.encoder = webMessageEncoder.TextMessageEncoder;
                            this.Schedule();
                            break;

                        case WebContentFormat.Raw:
                            if (message.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out javascriptResponseMessageProperty) &&
                                javascriptResponseMessageProperty != null &&
                                !String.IsNullOrEmpty(javascriptResponseMessageProperty.CallbackFunctionName))
                            {
                                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR2.JavascriptCallbackNotsupported), message);
                            }

                            handleEndWriteMessage = new AsyncCompletion(HandleEndWriteMessage);
                            IAsyncResult result = webMessageEncoder.RawMessageEncoder.BeginWriteMessage(message, stream, PrepareAsyncCompletion(HandleEndWriteMessage), this);
                            if (SyncContinue(result))
                            {
                                this.Complete(true);
                            }
                            break;

                        default:
                            throw Fx.AssertAndThrow("This should never get hit because GetFormatForContentType shouldn't return a WebContentFormat other than Json, Xml, and Raw");
                    }
                }


                protected override void OnDoWork()
                {
                    this.encoder.WriteMessage(this.message, this.stream);
                }

                static bool HandleEndWriteMessage(IAsyncResult result)
                {
                    WriteMessageAsyncResult thisPtr = (WriteMessageAsyncResult)result.AsyncState;
                    thisPtr.webMessageEncoder.RawMessageEncoder.EndWriteMessage(result);
                    return true;
                }
            }
        }
    }
}
