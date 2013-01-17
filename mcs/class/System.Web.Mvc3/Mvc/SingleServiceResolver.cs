namespace System.Web.Mvc {
    using System.Globalization;
    using System.Web.Mvc.Resources;

    internal class SingleServiceResolver<TService> : IResolver<TService> where TService : class {

        private TService _currentValueFromResolver;
        private Func<TService> _currentValueThunk;
        private TService _defaultValue;
        private Func<IDependencyResolver> _resolverThunk;
        private string _callerMethodName;

        public SingleServiceResolver(Func<TService> currentValueThunk, TService defaultValue, string callerMethodName) {
            if (currentValueThunk == null) {
                throw new ArgumentNullException("currentValueThunk");
            }
            if (defaultValue == null) {
                throw new ArgumentNullException("defaultValue");
            }

            _resolverThunk = () => DependencyResolver.Current;
            _currentValueThunk = currentValueThunk;
            _defaultValue = defaultValue;
            _callerMethodName = callerMethodName;
        }

        internal SingleServiceResolver(Func<TService> staticAccessor, TService defaultValue, IDependencyResolver resolver, string callerMethodName)
            : this(staticAccessor, defaultValue, callerMethodName) {
            if (resolver != null) {
                _resolverThunk = () => resolver;
            }
        }

        public TService Current {
            get {
                if (_resolverThunk != null) {
                    lock (_currentValueThunk) {
                        if (_resolverThunk != null) {
                            _currentValueFromResolver = _resolverThunk().GetService<TService>();
                            _resolverThunk = null;

                            if (_currentValueFromResolver != null && _currentValueThunk() != null) {
                                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, MvcResources.SingleServiceResolver_CannotRegisterTwoInstances, typeof(TService).Name.ToString(), _callerMethodName));
                            }
                        }
                    }
                }
                return _currentValueFromResolver ?? _currentValueThunk() ?? _defaultValue;
            }
        }
    }
}
