namespace System.Web.Mvc {
    using System.Web.Routing;

    public interface IController {
        void Execute(RequestContext requestContext);
    }
}
