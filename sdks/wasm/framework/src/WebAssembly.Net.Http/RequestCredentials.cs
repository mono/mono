// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace WebAssembly.Net.Http.HttpClient
{
    /// <summary>
    /// Specifies a value for the 'credentials' option on outbound HTTP requests.
    /// </summary>
    public enum RequestCredentials
    {
        /// <summary>
        /// Advises the browser never to send credentials (such as cookies or HTTP auth headers).
        /// </summary>
        Omit,

        /// <summary>
        /// Advises the browser to send credentials (such as cookies or HTTP auth headers)
        /// only if the target URL is on the same origin as the calling application.
        /// </summary>
        SameOrigin,

        /// <summary>
        /// Advises the browser to send credentials (such as cookies or HTTP auth headers)
        /// even for cross-origin requests.
        /// </summary>
        Include,
    }
}
