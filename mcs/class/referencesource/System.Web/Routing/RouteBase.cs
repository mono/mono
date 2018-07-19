namespace System.Web.Routing {
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class RouteBase {
        public abstract RouteData GetRouteData(HttpContextBase httpContext);
        public abstract VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values);

        // Default needs to be true to avoid breaking change
        private bool _routeExistingFiles = true;
        public bool RouteExistingFiles {
            get {
                return _routeExistingFiles;
            }
            set {
                _routeExistingFiles = value;
            }
        }
    }
}
