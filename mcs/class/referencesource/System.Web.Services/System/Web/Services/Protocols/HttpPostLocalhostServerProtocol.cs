//------------------------------------------------------------------------------
// <copyright file="HttpPostServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    using System.Net;

    internal class HttpPostLocalhostServerProtocolFactory : ServerProtocolFactory {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request) {
            if (request.PathInfo.Length < 2)
                return null;
            if (request.HttpMethod != "POST")
                // MethodNotAllowed = 405,
                return new UnsupportedRequestProtocol(405);

            bool isLocal = request.Url.IsLoopback || request.IsLocal;
            if (!isLocal)
                return null;

            return new HttpPostServerProtocol();
        }
    }
}
