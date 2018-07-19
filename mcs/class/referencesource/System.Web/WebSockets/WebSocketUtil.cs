//------------------------------------------------------------------------------
// <copyright file="WebSocketUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Web;

    // Contains miscellaneous utility methods for working with WebSockets

    internal static class WebSocketUtil {

        // Determines whether or not a given HttpWorkerRequest meets the requirements for "same-origin"
        // as called out in these two documents:
        // - http://tools.ietf.org/html/rfc6454 (Web Origin)
        // - http://tools.ietf.org/html/rfc6455 (WebSockets)
        public static bool IsSameOriginRequest(HttpWorkerRequest workerRequest) {
            string hostHeader = workerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderHost);
            if (String.IsNullOrEmpty(hostHeader)) {
                // RFC 6455 (Sec. 4.1) and RFC 2616 (Sec. 14.23) make the "Host" header mandatory
                return false;
            }

            string secWebSocketOriginHeader = workerRequest.GetUnknownRequestHeader("Origin");
            if (String.IsNullOrEmpty(secWebSocketOriginHeader)) {
                // RFC 6455 (Sec. 4.1) makes the "Origin" header mandatory for browser clients.
                // Phone apps, console clients, and similar non-browser clients aren't required to send the header,
                // but this method isn't intended for those use cases anyway, so we can fail them. (Note: it's still
                // possible for a non-browser app to send the appropriate Origin header.)
                return false;
            }

            // create URI instances from both the "Host" and the "Origin" headers
            Uri hostHeaderUri = null;
            Uri originHeaderUri = null;
            bool urisCreatedSuccessfully = Uri.TryCreate(workerRequest.GetProtocol() + "://" + hostHeader.Trim(), UriKind.Absolute, out hostHeaderUri) // RFC 2616 (Sec. 14.23): "Host" header doesn't contain the scheme, so we need to prepend
                && Uri.TryCreate(secWebSocketOriginHeader.Trim(), UriKind.Absolute, out originHeaderUri);

            if (!urisCreatedSuccessfully) {
                // construction of one of the Uri instances failed
                return false;
            }

            // RFC 6454 (Sec. 4), schemes must be normalized to lowercase. (And for WebSockets we only
            // support HTTP / HTTPS anyway.)
            if (originHeaderUri.Scheme != "http" && originHeaderUri.Scheme != "https") {
                return false;
            }

            // RFC 6454 (Sec. 5), comparisons should be ordinal. The Uri class will automatically
            // fill in the Port property using the default value for the scheme if the provided input doesn't
            // explicitly contain a port number.
            return hostHeaderUri.Scheme == originHeaderUri.Scheme
                && hostHeaderUri.Host == originHeaderUri.Host
                && hostHeaderUri.Port == originHeaderUri.Port;
        }

    }
}
