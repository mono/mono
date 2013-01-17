namespace System.Web.Mvc {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class ActionFilterAttribute : FilterAttribute, IActionFilter, IResultFilter {

        // The OnXxx() methods are virtual rather than abstract so that a developer need override
        // only the ones that interest him.

        public virtual void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public virtual void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        public virtual void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}
