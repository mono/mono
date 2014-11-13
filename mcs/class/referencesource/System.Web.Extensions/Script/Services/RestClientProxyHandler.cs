//------------------------------------------------------------------------------
// <copyright file="RestClientProxyHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Web.Script.Services {
    internal class RestClientProxyHandler : IHttpHandler {
        public void ProcessRequest(HttpContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            string clientProxyString = WebServiceClientProxyGenerator.GetClientProxyScript(context);
            if (clientProxyString != null) {
                context.Response.ContentType = "application/x-javascript";
                context.Response.Write(clientProxyString);
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}
