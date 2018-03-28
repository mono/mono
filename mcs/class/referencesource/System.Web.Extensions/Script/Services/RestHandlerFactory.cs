//------------------------------------------------------------------------------
// <copyright file="RestHandlerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 


namespace System.Web.Script.Services {

    internal class RestHandlerFactory : IHttpHandlerFactory {
        internal const string ClientProxyRequestPathInfo = "/js";
        internal const string ClientDebugProxyRequestPathInfo = "/jsdebug";

        public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            if (IsClientProxyRequest(context.Request.PathInfo)) {
                // It's a request for client side proxies
                return new RestClientProxyHandler();
            }
            else {
                // The request is an actual call to a server method
                return RestHandler.CreateHandler(context);
            }
        }

        public virtual void ReleaseHandler(IHttpHandler handler) {
        }

        // Detects if this is a request we want to intercept, i.e. invocation or proxy request
        internal static bool IsRestRequest(HttpContext context) {
            return IsRestMethodCall(context.Request) || IsClientProxyRequest(context.Request.PathInfo);
        }

        // Detects if this is a method invocation, i.e. webservice call or page method call
        internal static bool IsRestMethodCall(HttpRequest request) {
            return
                !String.IsNullOrEmpty(request.PathInfo) &&
                (request.ContentType.StartsWith("application/json;", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase));
        }

        internal static bool IsClientProxyDebugRequest(string pathInfo) {
            return string.Equals(pathInfo, ClientDebugProxyRequestPathInfo, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsClientProxyRequest(string pathInfo) {
            return (string.Equals(pathInfo, ClientProxyRequestPathInfo, StringComparison.OrdinalIgnoreCase) ||
                IsClientProxyDebugRequest(pathInfo));
        }
    }
}
