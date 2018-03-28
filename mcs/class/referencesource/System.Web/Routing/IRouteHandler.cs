namespace System.Web.Routing {
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public interface IRouteHandler {
        IHttpHandler GetHttpHandler(RequestContext requestContext);
    }
}
