//------------------------------------------------------------------------------
// <copyright file="DefaultHttpHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;    
    using System.Security.Permissions;
    
    public class DefaultHttpHandler : IHttpAsyncHandler {

        private HttpContext _context;
        private NameValueCollection _executeUrlHeaders;

        public DefaultHttpHandler() {
        }

        protected HttpContext Context {
            get { return _context; }
        }

        // headers to provide to execute url        
        protected NameValueCollection ExecuteUrlHeaders {
            get {
                if (_executeUrlHeaders == null && _context != null) {
                    _executeUrlHeaders = new NameValueCollection(_context.Request.Headers);
                }

                return _executeUrlHeaders; 
            }
        }

        // called when we know a precondition for calling
        // execute URL has been violated
        public virtual void OnExecuteUrlPreconditionFailure() {
            // do nothing - the derived class might throw
        }

        // add a virtual method that provides the request target for the EXECUTE_URL call
        // if return null, the default calls EXECUTE_URL for the default target
        public virtual String OverrideExecuteUrlPath() {
            return null;
        }

        internal static bool IsClassicAspRequest(String filePath) {
            return StringUtil.StringEndsWithIgnoreCase(filePath, ".asp");
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        private static string MapPathWithAssert(HttpContext context, string virtualPath) {
            return context.Request.MapPath(virtualPath);
        }

        public virtual IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, Object state) {

           // DDB 168193: DefaultHttpHandler is obsolate in integrated mode
           if (HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Method_Not_Supported_By_Iis_Integrated_Mode, "DefaultHttpHandler.BeginProcessRequest"));
           }

            _context = context;
            HttpResponse response = _context.Response;

            if (response.CanExecuteUrlForEntireResponse) {
                // use ExecuteUrl
                String path = OverrideExecuteUrlPath();

                if (path != null && !HttpRuntime.IsFullTrust) {
                    // validate that an untrusted derived classes (not in GAC)
                    // didn't set the path to a place that CAS wouldn't let it access
                    if (!this.GetType().Assembly.GlobalAssemblyCache) {
                        HttpRuntime.CheckFilePermission(MapPathWithAssert(context, path));
                    }
                }

                return response.BeginExecuteUrlForEntireResponse(path, _executeUrlHeaders, callback, state);
            }
            else {
                // let the base class throw if it doesn't want static files handler
                OnExecuteUrlPreconditionFailure();

                // use static file handler
                _context = null; // don't keep request data alive in sync case

                HttpRequest request = context.Request;

                // refuse POST requests
                if (request.HttpVerb == HttpVerb.POST) {
                    throw new HttpException(405, SR.GetString(SR.Method_not_allowed, request.HttpMethod, request.Path));
                }

                // refuse .asp requests
                if (IsClassicAspRequest(request.FilePath)) {
                    throw new HttpException(403, SR.GetString(SR.Path_forbidden, request.Path));
                }

                // default to static file handler
                StaticFileHandler.ProcessRequestInternal(context, OverrideExecuteUrlPath());

                // return async result indicating completion
                return new HttpAsyncResult(callback, state, true, null, null);
            }
        }

        public virtual void EndProcessRequest(IAsyncResult result) {
            if (_context != null) {
                HttpResponse response = _context.Response;
                _context = null;
                response.EndExecuteUrlForEntireResponse(result);
            }
        }

        public virtual void ProcessRequest(HttpContext context) {
            // this handler should never be called synchronously
            throw new InvalidOperationException(SR.GetString(SR.Cannot_call_defaulthttphandler_sync));
        }

        public virtual bool IsReusable {
            get { return false; }
        }
    }
}

