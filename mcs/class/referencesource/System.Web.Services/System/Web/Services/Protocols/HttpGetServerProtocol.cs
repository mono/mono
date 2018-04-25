//------------------------------------------------------------------------------
// <copyright file="HttpGetServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    internal class HttpGetServerProtocolFactory : ServerProtocolFactory {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request) {
            if (request.PathInfo.Length < 2)
                return null;
            if (request.HttpMethod != "GET")
                // MethodNotAllowed = 405,
                return new UnsupportedRequestProtocol(405);


            return new HttpGetServerProtocol();
        }
    }

    internal class HttpGetServerProtocol : HttpServerProtocol {
        internal HttpGetServerProtocol() : base(false) { }
    }
}
