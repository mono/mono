// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime;

    internal static class HttpResponseMessageExtensionMethods
    {
        internal static void AddHeader(this HttpResponseMessage httpResponseMessage, string header, string value)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");
            Fx.Assert(!string.IsNullOrWhiteSpace(header), "The 'header' parameter should never be null or whitespace.");

            HttpHeaderInfo headerInfo = HttpHeaderInfo.Create(header);
            EnsureNotRequestHeader(headerInfo);
            AddHeader(httpResponseMessage, headerInfo, value);
        }

        internal static void SetHeader(this HttpResponseMessage httpResponseMessage, string header, string value)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");
            Fx.Assert(!string.IsNullOrWhiteSpace(header), "The 'header' parameter should never be null or whitespace.");

            HttpHeaderInfo headerInfo = HttpHeaderInfo.Create(header);
            EnsureNotRequestHeader(headerInfo);
            RemoveHeader(httpResponseMessage, headerInfo);
            AddHeader(httpResponseMessage, headerInfo, value);
        }

        internal static IEnumerable<string> GetHeader(this HttpResponseMessage httpResponseMessage, string header)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");
            Fx.Assert(!string.IsNullOrWhiteSpace(header), "The 'header' parameter should never be null or whitespace.");

            HttpHeaderInfo headerInfo = HttpHeaderInfo.Create(header);
            EnsureNotRequestHeader(headerInfo);
            return GetHeader(httpResponseMessage, headerInfo);
        }

        internal static void RemoveHeader(this HttpResponseMessage httpResponseMessage, string header)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");
            Fx.Assert(!string.IsNullOrWhiteSpace(header), "The 'header' parameter should never be null or whitespace.");

            HttpHeaderInfo headerInfo = HttpHeaderInfo.Create(header);
            EnsureNotRequestHeader(headerInfo);
            RemoveHeader(httpResponseMessage, headerInfo);
        }

        internal static HttpResponseMessage CreateBufferedCopy(this HttpResponseMessage httpResponseMessage)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");

            HttpResponseMessage bufferedHttpResponseMessage = new HttpResponseMessage();
            bufferedHttpResponseMessage.ReasonPhrase = httpResponseMessage.ReasonPhrase;
            bufferedHttpResponseMessage.StatusCode = httpResponseMessage.StatusCode;
            bufferedHttpResponseMessage.Version = (Version)(httpResponseMessage.Version != null ? httpResponseMessage.Version.Clone() : null);

            if (httpResponseMessage.RequestMessage != null)
            {
                bufferedHttpResponseMessage.RequestMessage = httpResponseMessage.RequestMessage.CreateBufferedCopy();
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in httpResponseMessage.Headers)
            {
                bufferedHttpResponseMessage.Headers.AddHeaderWithoutValidation(header);
            }

            bufferedHttpResponseMessage.Content = HttpRequestMessageExtensionMethods.CreateBufferedCopyOfContent(httpResponseMessage.Content);

            return bufferedHttpResponseMessage;
        }

        internal static void CopyPropertiesFromMessage(this HttpResponseMessage httpResponseMessage, Message message)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpRequestMessage' parameter should not be null.");
            Fx.Assert(message != null, "The 'message' parameter should not be null.");

            HttpRequestMessage request = httpResponseMessage.RequestMessage;
            if (request != null)
            {
                request.CopyPropertiesFromMessage(message);
            }
        }

        private static void EnsureNotRequestHeader(HttpHeaderInfo headerInfo)
        {
            if (!headerInfo.IsResponseHeader && !headerInfo.IsContentHeader && headerInfo.IsResponseHeader)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(
                        SR.GetString(SR.RequestHeaderWithResponseHeadersCollection, headerInfo.Name)));
            }
        }

        private static IEnumerable<string> GetHeader(HttpResponseMessage httpResponseMessage, HttpHeaderInfo headerInfo)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");
            Fx.Assert(headerInfo != null, "The 'headerInfo' parameter should never be null.");
            Fx.Assert(headerInfo.IsResponseHeader || headerInfo.IsContentHeader, "The 'headerInfo' parameter should be either a response or content header.");

            IEnumerable<string> values = null;

            if (headerInfo.IsResponseHeader)
            {
                values = headerInfo.TryGetHeader(httpResponseMessage.Headers);
            }

            if (values == null &&
                headerInfo.IsContentHeader &&
                httpResponseMessage.Content != null)
            {
                values = headerInfo.TryGetHeader(httpResponseMessage.Content.Headers);
            }

            return values;
        }

        private static void RemoveHeader(HttpResponseMessage httpResponseMessage, HttpHeaderInfo headerInfo)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");
            Fx.Assert(headerInfo != null, "The 'headerInfo' parameter should never be null.");
            Fx.Assert(headerInfo.IsResponseHeader || headerInfo.IsContentHeader, "The 'headerInfo' parameter should be either a response or content header.");

            if (headerInfo.IsResponseHeader)
            {
                headerInfo.TryRemoveHeader(httpResponseMessage.Headers);
            }

            if (headerInfo.IsContentHeader && httpResponseMessage.Content != null)
            {
                headerInfo.TryRemoveHeader(httpResponseMessage.Content.Headers);
            }
        }

        private static void AddHeader(HttpResponseMessage httpResponseMessage, HttpHeaderInfo headerInfo, string value)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");
            Fx.Assert(headerInfo != null, "The 'headerInfo' parameter should never be null.");
            Fx.Assert(headerInfo.IsResponseHeader || headerInfo.IsContentHeader, "The 'headerInfo' parameter should be either a response or content header.");

            if (headerInfo.IsResponseHeader)
            {
                if (headerInfo.TryAddHeader(httpResponseMessage.Headers, value))
                {
                    return;
                }
            }

            if (headerInfo.IsContentHeader)
            {
                CreateContentIfNull(httpResponseMessage);
                headerInfo.TryAddHeader(httpResponseMessage.Content.Headers, value);
            }
        }

        private static bool CreateContentIfNull(HttpResponseMessage httpResponseMessage)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");

            if (httpResponseMessage.Content == null)
            {
                httpResponseMessage.Content = new ByteArrayContent(EmptyArray<byte>.Instance);
                return true;
            }

            return false;
        }
    }
}
