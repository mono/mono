namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    /// This class provides all the external things that the Model Binding System requires.
    /// These include HttpContext and ModelState.
    /// </summary>
    public class ModelBindingExecutionContext {

        private Dictionary<Type, object> _services = new Dictionary<Type,object>();
        private HttpContextBase _httpContext;
        private ModelStateDictionary _modelState;

        public ModelBindingExecutionContext(HttpContextBase httpContext, ModelStateDictionary modelState) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }

            if (modelState == null) {
                throw new ArgumentNullException("modelState");
            }

            _httpContext = httpContext;
            _modelState = modelState;
        }

        public virtual HttpContextBase HttpContext {
            get {
                return _httpContext;
            }
        }

        public virtual ModelStateDictionary ModelState {
            get {
                return _modelState;
            }
        }

        public virtual void PublishService<TService>(TService service) {
            _services[typeof(TService)] = service;
        }

        public virtual TService GetService<TService>() {
            return (TService)_services[typeof(TService)];
        }

        public virtual TService TryGetService<TService>() {
            if (_services.ContainsKey(typeof(TService))) {
                return (TService)_services[typeof(TService)];
            }

            return default(TService);
        }

    }
}
