namespace System.Web.Mvc {
    using System.Web.Routing;
    using System.Web.SessionState;

    public interface IControllerFactory {
        IController CreateController(RequestContext requestContext, string controllerName);
        SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName);
        void ReleaseController(IController controller);
    }
}
