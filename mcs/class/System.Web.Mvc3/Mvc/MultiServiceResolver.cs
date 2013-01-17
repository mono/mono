namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class MultiServiceResolver<TService> : IResolver<IEnumerable<TService>> where TService : class {
        private IEnumerable<TService> _itemsFromService;
        private Func<IEnumerable<TService>> _itemsThunk;
        private Func<IDependencyResolver> _resolverThunk;

        public MultiServiceResolver(Func<IEnumerable<TService>> itemsThunk) {
            if (itemsThunk == null) {
                throw new ArgumentNullException("itemsThunk");
            }

            _itemsThunk = itemsThunk;
            _resolverThunk = () => DependencyResolver.Current;
        }

        internal MultiServiceResolver(Func<IEnumerable<TService>> itemsThunk, IDependencyResolver resolver)
            : this(itemsThunk) {
            if (resolver != null) {
                _resolverThunk = () => resolver;
            }
        }

        public IEnumerable<TService> Current {
            get {
                if (_itemsFromService == null) {
                    lock (_itemsThunk) {
                        if (_itemsFromService == null) {
                            _itemsFromService = _resolverThunk().GetServices<TService>();
                        }
                    }
                }
                return _itemsFromService.Concat(_itemsThunk());
            }
        }
    }
}

