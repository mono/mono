namespace System.Web.Routing {
    using System.Runtime.CompilerServices;
    using System.Diagnostics.CodeAnalysis;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class RequestContext {
        public RequestContext() {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public RequestContext(HttpContextBase httpContext, RouteData routeData) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }
            if (routeData == null) {
                throw new ArgumentNullException("routeData");
            }
            HttpContext = httpContext;
            RouteData = routeData;
        }

        public virtual HttpContextBase HttpContext {
            get;
            set;
        }

        public virtual RouteData RouteData {
            get;
            set;
        }
    }
}
