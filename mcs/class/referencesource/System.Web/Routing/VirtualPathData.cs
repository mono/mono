namespace System.Web.Routing {
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class VirtualPathData {
        private string _virtualPath;
        private RouteValueDictionary _dataTokens = new RouteValueDictionary();

        public VirtualPathData(RouteBase route, string virtualPath) {
            Route = route;
            VirtualPath = virtualPath;
        }

        public RouteValueDictionary DataTokens {
            get {
                return _dataTokens;
            }
        }

        public RouteBase Route {
            get;
            set;
        }

        public string VirtualPath {
            get {
                return _virtualPath ?? String.Empty;
            }
            set {
                _virtualPath = value;
            }
        }
    }
}
