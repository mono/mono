namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc.Async;

    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Unsealed so that subclassed types can set properties in the default constructor.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AsyncTimeoutAttribute : ActionFilterAttribute {

        // duration is specified in milliseconds
        public AsyncTimeoutAttribute(int duration) {
            if (duration < -1) {
                throw Error.AsyncCommon_InvalidTimeout("duration");
            }

            Duration = duration;
        }

        public int Duration {
            get;
            private set;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            if (filterContext == null) {
                throw new ArgumentNullException("filterContext");
            }

            IAsyncManagerContainer container = filterContext.Controller as IAsyncManagerContainer;
            if (container == null) {
                throw Error.AsyncCommon_ControllerMustImplementIAsyncManagerContainer(filterContext.Controller.GetType());
            }

            container.AsyncManager.Timeout = Duration;

            base.OnActionExecuting(filterContext);
        }

    }
}
