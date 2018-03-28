//------------------------------------------------------------------------------
// <copyright file="WebFormRouteHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Routing {

    using System;
    using System.Web.UI;
    using System.Web.Compilation;
    using System.Web.Security;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    public class PageRouteHandler : IRouteHandler {
        public PageRouteHandler(string virtualPath)
            : this(virtualPath, true) {
        }

        public PageRouteHandler(string virtualPath, bool checkPhysicalUrlAccess) {
            if (string.IsNullOrEmpty(virtualPath) || !virtualPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentException(SR.GetString(SR.PageRouteHandler_InvalidVirtualPath), "virtualPath");
            }

            this.VirtualPath = virtualPath;
            this.CheckPhysicalUrlAccess = checkPhysicalUrlAccess;
            _useRouteVirtualPath = VirtualPath.Contains("{");
        }

        /// <summary>
        /// This is the full virtual path (using tilde syntax) to the WebForm page.
        /// </summary>
        /// <remarks>
        /// Needs to be thread safe so this is only settable via ctor.
        /// </remarks>
        public string VirtualPath { get; private set; }

        /// <summary>
        /// Because we're not actually rewriting the URL, ASP.NET's URL Auth will apply 
        /// to the incoming request URL and not the URL of the physical WebForm page.
        /// Setting this to true (default) will apply URL access rules against the 
        /// physical file.
        /// </summary>
        /// <value>True by default</value>
        public bool CheckPhysicalUrlAccess { get; private set; }

        private bool _useRouteVirtualPath;
        private Route _routeVirtualPath;
        private Route RouteVirtualPath {
            get {
                if (_routeVirtualPath == null) {
                    //Trim off ~/
                    _routeVirtualPath = new Route(VirtualPath.Substring(2), this);
                }
                return _routeVirtualPath;
            }
        }

        private bool CheckUrlAccess(string virtualPath, RequestContext requestContext) {
            IPrincipal user = requestContext.HttpContext.User;
            // If there's no authenticated user, use the default identity
            if (user == null) {
                user = new GenericPrincipal(new GenericIdentity(String.Empty, String.Empty), new string[0]);
            }
            return CheckUrlAccessWithAssert(virtualPath, requestContext, user);
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        private bool CheckUrlAccessWithAssert(string virtualPath, RequestContext requestContext, IPrincipal user) {
            return UrlAuthorizationModule.CheckUrlAccessForPrincipal(virtualPath, user, requestContext.HttpContext.Request.HttpMethod);
        }

        public virtual IHttpHandler GetHttpHandler(RequestContext requestContext) {
            if (requestContext == null) {
                throw new ArgumentNullException("requestContext");
            }

            string virtualPath = GetSubstitutedVirtualPath(requestContext);
            // Virtual Path ----s up with query strings, so we need to strip them off
            int qmark = virtualPath.IndexOf('?');
            if (qmark != -1) {
                virtualPath = virtualPath.Substring(0, qmark);
            }
            if (this.CheckPhysicalUrlAccess && !CheckUrlAccess(virtualPath, requestContext)) {
                return new UrlAuthFailureHandler();
            }

            Page page = BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(Page)) as Page;
            return page;
        }

        /// <summary>
        /// Gets the virtual path to the resource after applying substitutions based on route data.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        public string GetSubstitutedVirtualPath(RequestContext requestContext) {
            if (requestContext == null) {
                throw new ArgumentNullException("requestContext");
            }

            if (!_useRouteVirtualPath)
                return VirtualPath;

            VirtualPathData vpd = RouteVirtualPath.GetVirtualPath(requestContext, requestContext.RouteData.Values);
            // 
            if (vpd == null)
                return VirtualPath;
            return "~/" + vpd.VirtualPath;
        }
    }
}
