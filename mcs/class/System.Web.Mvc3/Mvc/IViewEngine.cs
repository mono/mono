namespace System.Web.Mvc {

    public interface IViewEngine {
        ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache);
        ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache);
        void ReleaseView(ControllerContext controllerContext, IView view);
    }
}
