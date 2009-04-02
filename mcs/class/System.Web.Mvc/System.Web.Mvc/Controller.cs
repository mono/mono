/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public abstract class Controller : ControllerBase, IActionFilter, IAuthorizationFilter, IDisposable, IExceptionFilter, IResultFilter {

        private IActionInvoker _actionInvoker;
        private ModelBinderDictionary _binders;
        private RouteCollection _routeCollection;
        private ITempDataProvider _tempDataProvider;

        public IActionInvoker ActionInvoker {
            get {
                if (_actionInvoker == null) {
                    _actionInvoker = new ControllerActionInvoker();
                }
                return _actionInvoker;
            }
            set {
                _actionInvoker = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Property is settable so that the dictionary can be provided for unit testing purposes.")]
        protected internal ModelBinderDictionary Binders {
            get {
                if (_binders == null) {
                    _binders = ModelBinders.Binders;
                }
                return _binders;
            }
            set {
                _binders = value;
            }
        }

        public HttpContextBase HttpContext {
            get {
                return ControllerContext == null ? null : ControllerContext.HttpContext;
            }
        }

        public ModelStateDictionary ModelState {
            get {
                return ViewData.ModelState;
            }
        }

        public HttpRequestBase Request {
            get {
                return HttpContext == null ? null : HttpContext.Request;
            }
        }

        public HttpResponseBase Response {
            get {
                return HttpContext == null ? null : HttpContext.Response;
            }
        }

        internal RouteCollection RouteCollection {
            get {
                if (_routeCollection == null) {
                    _routeCollection = RouteTable.Routes;
                }
                return _routeCollection;
            }
            set {
                _routeCollection = value;
            }
        }

        public RouteData RouteData {
            get {
                return ControllerContext == null ? null : ControllerContext.RouteData;
            }
        }

        public HttpServerUtilityBase Server {
            get {
                return HttpContext == null ? null : HttpContext.Server;
            }
        }

        public HttpSessionStateBase Session {
            get {
                return HttpContext == null ? null : HttpContext.Session;
            }
        }

        public ITempDataProvider TempDataProvider {
            get {
                if (_tempDataProvider == null) {
                    _tempDataProvider = new SessionStateTempDataProvider();
                }
                return _tempDataProvider;
            }
            set {
                _tempDataProvider = value;
            }
        }

        public UrlHelper Url {
            get;
            set;
        }

        public IPrincipal User {
            get {
                return HttpContext == null ? null : HttpContext.User;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "'Content' refers to ContentResult type; 'content' refers to ContentResult.Content property.")]
        protected internal ContentResult Content(string content) {
            return Content(content, null /* contentType */);
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "'Content' refers to ContentResult type; 'content' refers to ContentResult.Content property.")]
        protected internal ContentResult Content(string content, string contentType) {
            return Content(content, contentType, null /* contentEncoding */);
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "'Content' refers to ContentResult type; 'content' refers to ContentResult.Content property.")]
        protected internal virtual ContentResult Content(string content, string contentType, Encoding contentEncoding) {
            return new ContentResult {
                Content = content,
                ContentType = contentType,
                ContentEncoding = contentEncoding
            };
        }

        // The default invoker will never match methods defined on the Controller type, so
        // the Dispose() method is not web-callable.  However, in general, since implicitly-
        // implemented interface methods are public, they are web-callable unless decorated with
        // [NonAction].
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Dispose() {
            Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
        }

        protected override void ExecuteCore() {
            TempData.Load(ControllerContext, TempDataProvider);

            try {
                string actionName = RouteData.GetRequiredString("action");
                if (!ActionInvoker.InvokeAction(ControllerContext, actionName)) {
                    HandleUnknownAction(actionName);
                }
            }
            finally {
                TempData.Save(ControllerContext, TempDataProvider);
            }
        }

        protected internal FileContentResult File(byte[] fileContents, string contentType) {
            return File(fileContents, contentType, null /* fileDownloadName */);
        }

        protected internal virtual FileContentResult File(byte[] fileContents, string contentType, string fileDownloadName) {
            return new FileContentResult(fileContents, contentType) { FileDownloadName = fileDownloadName };
        }

        protected internal FileStreamResult File(Stream fileStream, string contentType) {
            return File(fileStream, contentType, null /* fileDownloadName */);
        }

        protected internal virtual FileStreamResult File(Stream fileStream, string contentType, string fileDownloadName) {
            return new FileStreamResult(fileStream, contentType) { FileDownloadName = fileDownloadName };
        }

        protected internal FilePathResult File(string fileName, string contentType) {
            return File(fileName, contentType, null /* fileDownloadName */);
        }

        protected internal virtual FilePathResult File(string fileName, string contentType, string fileDownloadName) {
            return new FilePathResult(fileName, contentType) { FileDownloadName = fileDownloadName };
        }

        protected virtual void HandleUnknownAction(string actionName) {
            throw new HttpException(404, String.Format(CultureInfo.CurrentUICulture,
                MvcResources.Controller_UnknownAction, actionName, GetType().FullName));
        }

        protected internal virtual JavaScriptResult JavaScript(string script) {
            return new JavaScriptResult { Script = script };
        }

        protected internal JsonResult Json(object data) {
            return Json(data, null /* contentType */);
        }

        protected internal JsonResult Json(object data, string contentType) {
            return Json(data, contentType, null /* contentEncoding */);
        }

        protected internal virtual JsonResult Json(object data, string contentType, Encoding contentEncoding) {
            return new JsonResult {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding
            };
        }

        protected override void Initialize(RequestContext requestContext) {
            base.Initialize(requestContext);
            Url = new UrlHelper(requestContext);
        }

        protected virtual void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        protected virtual void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        protected virtual void OnAuthorization(AuthorizationContext filterContext) {
        }

        protected virtual void OnException(ExceptionContext filterContext) {
        }

        protected virtual void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        protected virtual void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        protected internal PartialViewResult PartialView() {
            return PartialView(null /* viewName */, null /* model */);
        }

        protected internal PartialViewResult PartialView(object model) {
            return PartialView(null /* viewName */, model);
        }

        protected internal PartialViewResult PartialView(string viewName) {
            return PartialView(viewName, null /* model */);
        }

        protected internal virtual PartialViewResult PartialView(string viewName, object model) {
            if (model != null) {
                ViewData.Model = model;
            }

            return new PartialViewResult {
                ViewName = viewName,
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Instance method for consistency with other helpers.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "Response.Redirect() takes its URI as a string parameter.")]
        protected internal virtual RedirectResult Redirect(string url) {
            if (String.IsNullOrEmpty(url)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "url");
            }
            return new RedirectResult(url);
        }

        protected internal RedirectToRouteResult RedirectToAction(string actionName) {
            return RedirectToAction(actionName, (RouteValueDictionary)null);
        }

        protected internal RedirectToRouteResult RedirectToAction(string actionName, object routeValues) {
            return RedirectToAction(actionName, new RouteValueDictionary(routeValues));
        }

        protected internal RedirectToRouteResult RedirectToAction(string actionName, RouteValueDictionary routeValues) {
            return RedirectToAction(actionName, null /* controllerName */, routeValues);
        }

        protected internal RedirectToRouteResult RedirectToAction(string actionName, string controllerName) {
            return RedirectToAction(actionName, controllerName, (RouteValueDictionary)null);
        }

        protected internal RedirectToRouteResult RedirectToAction(string actionName, string controllerName, object routeValues) {
            return RedirectToAction(actionName, controllerName, new RouteValueDictionary(routeValues));
        }

        protected internal virtual RedirectToRouteResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues) {
            RouteValueDictionary mergedRouteValues;

            if (RouteData == null) {
                mergedRouteValues = RouteValuesHelpers.MergeRouteValues(actionName, controllerName, null, routeValues, true /* includeImplicitMvcValues */);
            }
            else {
                mergedRouteValues = RouteValuesHelpers.MergeRouteValues(actionName, controllerName, RouteData.Values, routeValues, true /* includeImplicitMvcValues */);
            }

            return new RedirectToRouteResult(mergedRouteValues);
        }

        protected internal RedirectToRouteResult RedirectToRoute(object routeValues) {
            return RedirectToRoute(new RouteValueDictionary(routeValues));
        }

        protected internal RedirectToRouteResult RedirectToRoute(RouteValueDictionary routeValues) {
            return RedirectToRoute(null /* routeName */, routeValues);
        }

        protected internal RedirectToRouteResult RedirectToRoute(string routeName) {
            return RedirectToRoute(routeName, (RouteValueDictionary)null);
        }

        protected internal RedirectToRouteResult RedirectToRoute(string routeName, object routeValues) {
            return RedirectToRoute(routeName, new RouteValueDictionary(routeValues));
        }

        protected internal virtual RedirectToRouteResult RedirectToRoute(string routeName, RouteValueDictionary routeValues) {
            return new RedirectToRouteResult(routeName, RouteValuesHelpers.GetRouteValues(routeValues));
        }

        protected internal bool TryUpdateModel<TModel>(TModel model) where TModel : class {
            return TryUpdateModel(model, null, null, null, ValueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string prefix) where TModel : class {
            return TryUpdateModel(model, prefix, null, null, ValueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string[] includeProperties) where TModel : class {
            return TryUpdateModel(model, null, includeProperties, null, ValueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties) where TModel : class {
            return TryUpdateModel(model, prefix, includeProperties, null, ValueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties, ValueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            return TryUpdateModel(model, null, null, null, valueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            return TryUpdateModel(model, prefix, null, null, valueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string[] includeProperties, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            return TryUpdateModel(model, null, includeProperties, null, valueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            return TryUpdateModel(model, prefix, includeProperties, null, valueProvider);
        }

        protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            if (model == null) {
                throw new ArgumentNullException("model");
            }
            if (valueProvider == null) {
                throw new ArgumentNullException("valueProvider");
            }

            Predicate<string> propertyFilter = propertyName => BindAttribute.IsPropertyAllowed(propertyName, includeProperties, excludeProperties);
            IModelBinder binder = Binders.GetBinder(typeof(TModel));

            ModelBindingContext bindingContext = new ModelBindingContext() {
                Model = model,
                ModelName = prefix,
                ModelState = ModelState,
                ModelType = typeof(TModel),
                PropertyFilter = propertyFilter,
                ValueProvider = valueProvider
            };
            binder.BindModel(ControllerContext, bindingContext);
            return ModelState.IsValid;
        }

        protected internal void UpdateModel<TModel>(TModel model) where TModel : class {
            UpdateModel(model, null, null, null, ValueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string prefix) where TModel : class {
            UpdateModel(model, prefix, null, null, ValueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string[] includeProperties) where TModel : class {
            UpdateModel(model, null, includeProperties, null, ValueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties) where TModel : class {
            UpdateModel(model, prefix, includeProperties, null, ValueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class {
            UpdateModel(model, prefix, includeProperties, excludeProperties, ValueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            UpdateModel(model, null, null, null, valueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string prefix, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            UpdateModel(model, prefix, null, null, valueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string[] includeProperties, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            UpdateModel(model, null, includeProperties, null, valueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            UpdateModel(model, prefix, includeProperties, null, valueProvider);
        }

        protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties, IDictionary<string, ValueProviderResult> valueProvider) where TModel : class {
            bool success = TryUpdateModel(model, prefix, includeProperties, excludeProperties, valueProvider);
            if (!success) {
                string message = String.Format(CultureInfo.CurrentUICulture, MvcResources.Controller_UpdateModel_UpdateUnsuccessful,
                    typeof(TModel).FullName);
                throw new InvalidOperationException(message);
            }
        }

        protected internal ViewResult View() {
            return View(null /* viewName */, null /* masterName */, null /* model */);
        }

        protected internal ViewResult View(object model) {
            return View(null /* viewName */, null /* masterName */, model);
        }

        protected internal ViewResult View(string viewName) {
            return View(viewName, null /* masterName */, null /* model */);
        }

        protected internal ViewResult View(string viewName, string masterName) {
            return View(viewName, masterName, null /* model */);
        }

        protected internal ViewResult View(string viewName, object model) {
            return View(viewName, null /* masterName */, model);
        }

        protected internal virtual ViewResult View(string viewName, string masterName, object model) {
            if (model != null) {
                ViewData.Model = model;
            }

            return new ViewResult {
                ViewName = viewName,
                MasterName = masterName,
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "The method name 'View' is a convenient shorthand for 'CreateViewResult'.")]
        protected internal ViewResult View(IView view) {
            return View(view, null /* model */);
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "The method name 'View' is a convenient shorthand for 'CreateViewResult'.")]
        protected internal virtual ViewResult View(IView view, object model) {
            if (model != null) {
                ViewData.Model = model;
            }

            return new ViewResult {
                View = view,
                ViewData = ViewData,
                TempData = TempData
            };
        }

        #region IActionFilter Members
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext) {
            OnActionExecuting(filterContext);
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext) {
            OnActionExecuted(filterContext);
        }
        #endregion

        #region IAuthorizationFilter Members
        void IAuthorizationFilter.OnAuthorization(AuthorizationContext filterContext) {
            OnAuthorization(filterContext);
        }
        #endregion

        #region IExceptionFilter Members
        void IExceptionFilter.OnException(ExceptionContext filterContext) {
            OnException(filterContext);
        }
        #endregion

        #region IResultFilter Members
        void IResultFilter.OnResultExecuting(ResultExecutingContext filterContext) {
            OnResultExecuting(filterContext);
        }

        void IResultFilter.OnResultExecuted(ResultExecutedContext filterContext) {
            OnResultExecuted(filterContext);
        }
        #endregion
    }
}
