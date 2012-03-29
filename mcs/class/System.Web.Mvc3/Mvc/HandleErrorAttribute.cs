namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc.Resources;

    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is AllowMultiple = true and users might want to override behavior.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class HandleErrorAttribute : FilterAttribute, IExceptionFilter {

        private const string _defaultView = "Error";

        private readonly object _typeId = new object();

        private Type _exceptionType = typeof(Exception);
        private string _master;
        private string _view;

        public Type ExceptionType {
            get {
                return _exceptionType;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                if (!typeof(Exception).IsAssignableFrom(value)) {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                        MvcResources.ExceptionViewAttribute_NonExceptionType, value.FullName));
                }

                _exceptionType = value;
            }
        }

        public string Master {
            get {
                return _master ?? String.Empty;
            }
            set {
                _master = value;
            }
        }

        public override object TypeId {
            get {
                return _typeId;
            }
        }

        public string View {
            get {
                return (!String.IsNullOrEmpty(_view)) ? _view : _defaultView;
            }
            set {
                _view = value;
            }
        }

        public virtual void OnException(ExceptionContext filterContext) {
            if (filterContext == null) {
                throw new ArgumentNullException("filterContext");
            }
            if (filterContext.IsChildAction) {
                return;
            }

            // If custom errors are disabled, we need to let the normal ASP.NET exception handler
            // execute so that the user can see useful debugging information.
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled) {
                return;
            }

            Exception exception = filterContext.Exception;

            // If this is not an HTTP 500 (for example, if somebody throws an HTTP 404 from an action method),
            // ignore it.
            if (new HttpException(null, exception).GetHttpCode() != 500) {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(exception)) {
                return;
            }

            string controllerName = (string)filterContext.RouteData.Values["controller"];
            string actionName = (string)filterContext.RouteData.Values["action"];
            HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
            filterContext.Result = new ViewResult {
                ViewName = View,
                MasterName = Master,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                TempData = filterContext.Controller.TempData
            };
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            // Certain versions of IIS will sometimes use their own error page when
            // they detect a server error. Setting this property indicates that we
            // want it to try to render ASP.NET MVC's error page instead.
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}