namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Mvc.Async;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;
    using System.Web.WebPages;
    using System.Web.WebPages.Scope;

    public abstract class ControllerBase : IController {

        private readonly SingleEntryGate _executeWasCalledGate = new SingleEntryGate();

        private DynamicViewDataDictionary _dynamicViewDataDictionary;
        private TempDataDictionary _tempDataDictionary;
        private bool _validateRequest = true;
        private IValueProvider _valueProvider;
        private ViewDataDictionary _viewDataDictionary;

        public ControllerContext ControllerContext {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This property is settable so that unit tests can provide mock implementations.")]
        public TempDataDictionary TempData {
            get {
                if (ControllerContext != null && ControllerContext.IsChildAction) {
                    return ControllerContext.ParentActionViewContext.TempData;
                }
                if (_tempDataDictionary == null) {
                    _tempDataDictionary = new TempDataDictionary();
                }
                return _tempDataDictionary;
            }
            set {
                _tempDataDictionary = value;
            }
        }

        public bool ValidateRequest {
            get {
                return _validateRequest;
            }
            set {
                _validateRequest = value;
            }
        }

        public IValueProvider ValueProvider {
            get {
                if (_valueProvider == null) {
                    _valueProvider = ValueProviderFactories.Factories.GetValueProvider(ControllerContext);
                }
                return _valueProvider;
            }
            set {
                _valueProvider = value;
            }
        }

        public dynamic ViewBag {
            get {
                if (_dynamicViewDataDictionary == null) {
                    _dynamicViewDataDictionary = new DynamicViewDataDictionary(() => ViewData);
                }
                return _dynamicViewDataDictionary;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This property is settable so that unit tests can provide mock implementations.")]
        public ViewDataDictionary ViewData {
            get {
                if (_viewDataDictionary == null) {
                    _viewDataDictionary = new ViewDataDictionary();
                }
                return _viewDataDictionary;
            }
            set {
                _viewDataDictionary = value;
            }
        }

        protected virtual void Execute(RequestContext requestContext) {
            if (requestContext == null) {
                throw new ArgumentNullException("requestContext");
            }
            if (requestContext.HttpContext == null) {
                throw new ArgumentException(MvcResources.ControllerBase_CannotExecuteWithNullHttpContext, "requestContext");
            }

            VerifyExecuteCalledOnce();
            Initialize(requestContext);

            using (ScopeStorage.CreateTransientScope()) {
                ExecuteCore();
            }
        }

        protected abstract void ExecuteCore();

        protected virtual void Initialize(RequestContext requestContext) {
            ControllerContext = new ControllerContext(requestContext, this);
        }

        internal void VerifyExecuteCalledOnce() {
            if (!_executeWasCalledGate.TryEnter()) {
                string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ControllerBase_CannotHandleMultipleRequests, GetType());
                throw new InvalidOperationException(message);
            }
        }

        #region IController Members
        void IController.Execute(RequestContext requestContext) {
            Execute(requestContext);
        }
        #endregion
    }
}
