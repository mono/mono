//------------------------------------------------------------------------------
// <copyright file="HttpPostServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    internal class HttpPostServerProtocolFactory : ServerProtocolFactory {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request) {
            if (request.PathInfo.Length < 2)
                return null;
            if (request.HttpMethod != "POST")
                // MethodNotAllowed = 405,
                return new UnsupportedRequestProtocol(405);

            return new HttpPostServerProtocol();
        }
    }

    internal class HttpPostServerProtocol : HttpServerProtocol {
        internal HttpPostServerProtocol() : base(true) { }
    }
}
