// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime;

    /// <summary>
    /// A static extension methods class for getting either an <see cref="HttpRequestMessage"/>
    /// or <see cref="HttpResponseMessage"/> instance from a <see cref="Message"/> instance.
    /// </summary>
    public static class MessageExtensionMethods
    {
        private const string MessageHeadersPropertyKey = "System.ServiceModel.Channels.MessageHeaders";
        private const string ToHttpRequestMessageMethodName = "ToHttpRequestMessage()";
        private const string ToHttpResponseMessageMethodName = "ToHttpResponseMessage()";
        private static readonly string HttpRequestMessagePropertyTypeName = typeof(HttpRequestMessageProperty).Name;
        private static readonly string HttpResponseMessagePropertyTypeName = typeof(HttpResponseMessageProperty).Name;

        /// <summary>
        /// An extension method for getting a <see cref="HttpRequestMessage"/> instance
        /// from an <see cref="Message"/> instance.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> instance from which to
        /// get the <see cref="HttpRequestMessage"/> instance.</param>
        /// <returns>The <see cref="HttpRequestMessage"/> instance.</returns>
        public static HttpRequestMessage ToHttpRequestMessage(this Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }

            HttpRequestMessage httpRequestMessage = HttpRequestMessageProperty.GetHttpRequestMessageFromMessage(message);
            if (httpRequestMessage == null)
            {
                HttpRequestMessageProperty requestMessageProperty = message.Properties.GetValue<HttpRequestMessageProperty>(HttpRequestMessageProperty.Name);
                if (requestMessageProperty == null)
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(
                            SR.MissingHttpMessageProperty(
                                ToHttpRequestMessageMethodName,
                                HttpRequestMessagePropertyTypeName)));
                }

                httpRequestMessage = CreateRequestMessage(message, requestMessageProperty);
            }

            return httpRequestMessage;
        }

        /// <summary>
        /// An extension method for getting a <see cref="HttpResponseMessage"/> instance
        /// from an <see cref="Message"/> instance.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> instance from which to
        /// get the <see cref="HttpResponseMessage"/> instance.</param>
        /// <returns>The <see cref="HttpResponseMessage"/> instance.</returns>
        public static HttpResponseMessage ToHttpResponseMessage(this Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }

            HttpResponseMessage httpResponseMessage = HttpResponseMessageProperty.GetHttpResponseMessageFromMessage(message);
            if (httpResponseMessage == null)
            {
                HttpResponseMessageProperty responseMessageProperty = message.Properties.GetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name);
                if (responseMessageProperty == null)
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(
                            SR.MissingHttpMessageProperty(
                                ToHttpResponseMessageMethodName,
                                HttpResponseMessagePropertyTypeName)));
                }

                httpResponseMessage = CreateResponseMessage(message, responseMessageProperty);
            }

            return httpResponseMessage;
        }

        internal static void ConfigureAsHttpMessage(this Message message, HttpRequestMessage httpRequestMessage)
        {
            Fx.Assert(message != null, "The 'message' parameter should never be null.");
            Fx.Assert(httpRequestMessage != null, "The 'httpRequestMessage' parameter should never be null.");

            message.Properties.Add(HttpRequestMessageProperty.Name, new HttpRequestMessageProperty(httpRequestMessage));
            CopyPropertiesToMessage(message, httpRequestMessage.Properties);
        }

        internal static void ConfigureAsHttpMessage(this Message message, HttpResponseMessage httpResponseMessage)
        {
            Fx.Assert(message != null, "The 'message' parameter should never be null.");
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");

            message.Properties.Add(HttpResponseMessageProperty.Name, new HttpResponseMessageProperty(httpResponseMessage));
            HttpRequestMessage httpRequestMessage = httpResponseMessage.RequestMessage;
            if (httpRequestMessage != null)
            {
                CopyPropertiesToMessage(message, httpRequestMessage.Properties);
            }
        }

        private static void CopyPropertiesToMessage(Message message, IDictionary<string, object> properties)
        {
            Fx.Assert(message != null, "The 'message' parameter should not be null.");
            Fx.Assert(properties != null, "The 'properties' parameter should not be null.");

            foreach (KeyValuePair<string, object> property in properties)
            {
                MessageHeaders messageHeaders = property.Value as MessageHeaders;
                if (messageHeaders != null &&
                    messageHeaders.MessageVersion == MessageVersion.None &&
                    string.Equals(property.Key, MessageHeadersPropertyKey, StringComparison.Ordinal))
                {
                    foreach (MessageHeader header in messageHeaders)
                    {
                        message.Headers.Add(header);
                    }
                }
                else
                {
                    message.Properties.Add(property.Key, property.Value);
                }
            }
        }

        private static HttpRequestMessage CreateRequestMessage(Message message, HttpRequestMessageProperty requestMessageProperty)
        {
            Fx.Assert(message != null, "The 'message' parameter should not be null.");
            Fx.Assert(requestMessageProperty != null, "The 'requestMessageProperty' parameter should not be null.");
            
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = message.Properties.Via;

            Fx.Assert(requestMessageProperty.Method != null, "The HttpRequestMessageProperty class ensures the 'Method' property will never be null.");
            request.Method = new HttpMethod(requestMessageProperty.Method);

            request.Content = CreateMessageContent(message, requestMessageProperty.SuppressEntityBody);

            WebHeaderCollection headers = requestMessageProperty.Headers;
            foreach (string headerKey in headers.AllKeys)
            {
                request.AddHeader(headerKey, headers[headerKey]);
            }

            request.CopyPropertiesFromMessage(message);

            return request;
        }

        private static HttpResponseMessage CreateResponseMessage(Message message, HttpResponseMessageProperty responseMessageProperty)
        {
            Fx.Assert(message != null, "The 'message' parameter should not be null.");
            Fx.Assert(responseMessageProperty != null, "The 'responseMessageProperty' parameter should not be null.");
            
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = responseMessageProperty.HasStatusCodeBeenSet ?
                responseMessageProperty.StatusCode :
                message.IsFault ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;

            string reasonPhrase = responseMessageProperty.StatusDescription;
            if (!string.IsNullOrEmpty(reasonPhrase))
            {
                response.ReasonPhrase = reasonPhrase;
            }

            response.Content = CreateMessageContent(message, responseMessageProperty.SuppressEntityBody);

            WebHeaderCollection headers = responseMessageProperty.Headers;
            foreach (string headerKey in headers.AllKeys)
            {
                response.AddHeader(headerKey, headers[headerKey]);
            }

            return response;
        }

        private static HttpContent CreateMessageContent(Message message, bool suppressEntityBody)
        {
            Fx.Assert(message != null, "The 'message' parameter should not be null.");

            if (suppressEntityBody || message.IsEmpty)
            {
                return new ByteArrayContent(EmptyArray<byte>.Instance);
            }

            return new StreamContent(message.GetBody<Stream>());
        }
    }
}
