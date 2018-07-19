namespace System.Web.Routing {
    using System.Web;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public interface IRouteConstraint {
        bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection);
    }
}
