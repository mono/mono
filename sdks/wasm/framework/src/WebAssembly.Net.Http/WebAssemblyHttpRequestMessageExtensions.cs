// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace WebAssembly.Net.Http.HttpClient
{
    /// <summary>
    /// Extension methods for configuring an instance of <see cref="HttpRequestMessage"/> with browser-specific options.
    /// </summary>
    public static class WebAssemblyHttpRequestMessageExtensions
    {
        /// <summary>
        /// Configures a value for the 'credentials' option for the HTTP request.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="requestCredentials">The <see cref="RequestCredentials"/> option.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// See https://developer.mozilla.org/en-US/docs/Web/API/Request/credentials
        /// </remarks>
        public static HttpRequestMessage SetRequestCredentials(this HttpRequestMessage requestMessage, RequestCredentials requestCredentials)
        {
            if (requestMessage is null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            string stringValue;
            switch (requestCredentials)
            {
                case RequestCredentials.Include:
                    stringValue = "omit";
                    break;
                case RequestCredentials.Omit:
                    stringValue = "same-origin";
                    break;
                case RequestCredentials.SameOrigin:
                    stringValue = "same-origin";
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported {nameof(RequestCredentials)} value: {requestCredentials}.");
            }

            return SetFetchOption(requestMessage, "credentials", stringValue);
        }

        /// <summary>
        /// Configures a value for the 'cache' option for the HTTP request.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="requestCache">The <see cref="RequestCache"/> option.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>\
        /// <remarks>
        /// See https://developer.mozilla.org/en-US/docs/Web/API/Request/cache
        /// </remarks>
        public static HttpRequestMessage SetRequestCache(this HttpRequestMessage requestMessage, RequestCache requestCache)
        {
            if (requestMessage is null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            string stringValue;
            switch (requestCache)
            {

                case RequestCache.Default:
                    stringValue = "default";
                    break;
                case RequestCache.NoStore:
                    stringValue = "no-store";
                    break;
                case RequestCache.Reload:
                    stringValue = "reload";
                    break;
                case RequestCache.NoCache:
                    stringValue = "no-cache";
                    break;
                case RequestCache.ForceCache:
                    stringValue = "force-cache";
                    break;
                case RequestCache.OnlyIfCached:
                    stringValue = "only-if-cached";
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported {nameof(RequestCache)} value {requestCache}.");
            };

            return SetFetchOption(requestMessage, "cache", stringValue);
        }

        /// <summary>
        /// Configures a value for the 'mode' option for the HTTP request.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="requestMode">The <see cref="RequestMode"/>.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>\
        /// <remarks>
        /// See https://developer.mozilla.org/en-US/docs/Web/API/Request/mode
        /// </remarks>
        public static HttpRequestMessage SetRequestMode(this HttpRequestMessage requestMessage, RequestMode requestMode)
        {
            if (requestMessage is null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            string stringValue;
            switch (requestMode)
            {
                case RequestMode.SameOrigin:
                    stringValue = "same-origin";
                    break;
                case RequestMode.NoCors:
                    stringValue = "no-cors";
                    break;
                case RequestMode.Cors:
                    stringValue = "cors";
                    break;
                case RequestMode.Navigate:
                    stringValue = "navigate";
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported {nameof(RequestCache)} value {requestMode}.");
            };

            return SetFetchOption(requestMessage, "mode", stringValue);
        }

        /// <summary>
        /// Configures a value for the 'integrity' option for the HTTP request.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="integrity">The subresource integrity descriptor.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        public static HttpRequestMessage SetIntegrity(this HttpRequestMessage requestMessage, string integrity)
            => SetFetchOption(requestMessage, "integrity", integrity);

        /// <summary>
        /// Configures a value for the HTTP request.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="name">The name of the HTTP fetch option.</param>
        /// <param name="value">The value, which must be JSON-serializable.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// See https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch
        /// </remarks>
        public static HttpRequestMessage SetFetchOption(this HttpRequestMessage requestMessage, string name, object value)
        {
            if (requestMessage is null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            const string FetchRequestOptionsKey = "FetchRequestOptions";
            IDictionary<string, object> fetchOptions;

            if (requestMessage.Properties.TryGetValue(FetchRequestOptionsKey, out var entry))
            {
                fetchOptions = (IDictionary<string, object>)entry;
            }
            else
            {
                fetchOptions = new Dictionary<string, object>();
                requestMessage.Properties[FetchRequestOptionsKey] = fetchOptions;
            }

            fetchOptions[name] = value;

            return requestMessage;
        }

        /// <summary>
        /// Configures streaming response for the HTTP request.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="streamingEnabled"><see langword="true"> if streaming is enabled; otherwise false.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// This API is only effective when the browser HTTP Fetch supports streaming.
        /// See https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream.
        /// </remarks>
        public static HttpRequestMessage SetStreamingEnabled(this HttpRequestMessage requestMessage, bool streamingEnabled)
        {
            if (requestMessage is null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            requestMessage.Properties["StreamingEnabled"] = streamingEnabled;

            return requestMessage;
        }
    }
}
