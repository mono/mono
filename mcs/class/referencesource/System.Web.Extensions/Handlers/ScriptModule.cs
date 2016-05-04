//------------------------------------------------------------------------------
// <copyright file="ScriptModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Handlers {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Web.ApplicationServices;
    using System.Web.UI;
    using System.Web.Resources;
    using System.Web.Script.Services;
    using System.Web.Security;

    public class ScriptModule : IHttpModule {
        private static readonly object _contextKey = new Object();

        private static Type _authenticationServiceType = typeof(System.Web.Security.AuthenticationService); // disambiguation required
        private static int _isHandlerRegistered;

        private static bool ShouldSkipAuthorization(HttpContext context) {
            if (context == null || context.Request == null) {
                return false;
            }

            string path = context.Request.FilePath;
            if (ScriptResourceHandler.IsScriptResourceRequest(path)) {
                return true;
            }

            // if auth service is disabled, dont bother checking.
            // (NOTE: if a custom webservice is used, it will be up to them to enable anon access to it)
            // if it isn't a rest request dont bother checking.
            if(!ApplicationServiceHelper.AuthenticationServiceEnabled || !RestHandlerFactory.IsRestRequest(context)) {
                return false;
            }

            if(context.SkipAuthorization) {
                return true;
            }

            // it may be a rest request to a webservice. It must end in axd if it is an app service.
            if((path == null) || !path.EndsWith(".axd", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            // WebServiceData caches the object in cache, so this should be a quick lookup.
            // If it hasnt been cached yet, this will cause it to be cached, so later in the request
            // it will be a cache-hit anyway.
            WebServiceData wsd = WebServiceData.GetWebServiceData(context, path, false, false);
            if((wsd != null) && (_authenticationServiceType == wsd.TypeData.Type)) {
                return true;
            }

            return false;
        }

        protected virtual void Dispose() {
        }

        private void AuthenticateRequestHandler(object sender, EventArgs e) {
            // flags the request with SkipAuthorization if it is a request for
            // the script Authentication webservice.
            HttpApplication app = (HttpApplication)sender;
            if (app != null && ShouldSkipAuthorization(app.Context)) {
                app.Context.SetSkipAuthorizationNoDemand(true, false);
            }
        }

        private void EndRequestHandler(object sender, EventArgs e){
            // DevDiv 100198: Send error response from EndRequest so Page and Application error handlers still fire on async posts
            // DevDiv 118737: Call Response.Clear as well as Response.ClearHeaders to force status code reset in integrated mode and
            // to ensure there are no errant headers such as a caching policy. Do not call Response.End or app.CompleteRequest as they
            // are pointless from the EndRequest event.
            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;
            object o = context.Items[PageRequestManager.AsyncPostBackErrorKey];

            if ((o != null) && ((bool)o == true)) {
                context.ClearError();
                context.Response.ClearHeaders();
                context.Response.Clear();
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.ContentType = "text/plain";

                string errorMessage = (string)context.Items[PageRequestManager.AsyncPostBackErrorMessageKey];
                o = context.Items[PageRequestManager.AsyncPostBackErrorHttpCodeKey];
                // o should definitely be an int, but user code could overwrite it
                int httpCode = (o is int) ? (int)o : 500;

                PageRequestManager.EncodeString(context.Response.Output,
                    PageRequestManager.ErrorToken,
                    httpCode.ToString(CultureInfo.InvariantCulture),
                    errorMessage);
            }
        }

        protected virtual void Init(HttpApplication app) {

            //////////////////////////////////////////////////////////////////
            // Check if this module has been already addded
            if (app.Context.Items[_contextKey] != null) {
                return; // already added to the pipeline
            }
            app.Context.Items[_contextKey] = _contextKey;

            // use the static HttpResponse.Redirecting event to hook all Response.Redirects.
            // Only hook the event once. Multiple pipelines may cause multiple instances of this
            // module to be created.
            if (Interlocked.Exchange(ref _isHandlerRegistered, 1) == 0) {
                HttpResponse.Redirecting += new EventHandler(HttpResponse_Redirecting);
            }
            app.PostAcquireRequestState += new EventHandler(OnPostAcquireRequestState);
            app.AuthenticateRequest += new EventHandler(AuthenticateRequestHandler);
            // DevDiv 100198: Send error response from EndRequest so Page and Application error handlers still fire on async posts
            app.EndRequest += new EventHandler(EndRequestHandler);
        }

        private static void HttpResponse_Redirecting(object sender, EventArgs e) {
            HttpResponse response = (HttpResponse)sender;
            HttpContext context = response.Context;
            // Is in async postback, get status code and check for 302
            if (PageRequestManager.IsAsyncPostBackRequest(new HttpRequestWrapper(context.Request))) {
                // Save the redirect location and other data before we clear it
                string redirectLocation = response.RedirectLocation;
                List<HttpCookie> cookies = new List<HttpCookie>(response.Cookies.Count);
                for (int i = 0; i < response.Cookies.Count; i++) {
                    cookies.Add(response.Cookies[i]);
                }

                // Clear the entire response and send a custom response that the client script can process
                response.ClearContent();
                response.ClearHeaders();
                for (int i = 0; i < cookies.Count; i++) {
                    response.AppendCookie(cookies[i]);
                }
                response.Cache.SetCacheability(HttpCacheability.NoCache);
                response.ContentType = "text/plain";

                // DevDiv#961281
                // Allow apps to access to the redirect location
                context.Items[PageRequestManager.AsyncPostBackRedirectLocationKey] = redirectLocation;

                // Preserve redirected state: TFS#882879
                response.IsRequestBeingRedirected = true;

                PageRequestManager.EncodeString(response.Output, PageRequestManager.UpdatePanelVersionToken, String.Empty, PageRequestManager.UpdatePanelVersionNumber);
                // url encode the location in a way that javascript unescape() will be able to reverse
                redirectLocation = String.Join(" ", redirectLocation.Split(' ').Select(part => HttpUtility.UrlEncode(part)));
                PageRequestManager.EncodeString(response.Output, PageRequestManager.PageRedirectToken, String.Empty, redirectLocation);
            }
            else if (RestHandlerFactory.IsRestRequest(context)) {
                // We need to special case webservice redirects, as we want them to fail (always are auth failures)
                RestHandler.WriteExceptionJsonString(context, new InvalidOperationException(AtlasWeb.WebService_RedirectError), (int)HttpStatusCode.Unauthorized);
            }
        }

        private void OnPostAcquireRequestState(object sender, EventArgs eventArgs) {
            HttpApplication app = (HttpApplication)sender;
            HttpRequest request = app.Context.Request;

            if (app.Context.Handler is Page && RestHandlerFactory.IsRestMethodCall(request)) {
                // Get the data about the web service being invoked
                WebServiceData webServiceData = WebServiceData.GetWebServiceData(HttpContext.Current, request.FilePath, false, true);

                // Get the method name
                string methodName = request.PathInfo.Substring(1);

                // Get the data about the specific method being called
                WebServiceMethodData methodData = webServiceData.GetMethodData(methodName);
                RestHandler.ExecuteWebServiceCall(HttpContext.Current, methodData);

                // Skip the rest of the page lifecycle
                app.CompleteRequest();
            }
        }

        #region IHttpModule Members
        void IHttpModule.Dispose() {
            Dispose();
        }

        void IHttpModule.Init(HttpApplication context) {
            Init(context);
        }
        #endregion
    }
}
