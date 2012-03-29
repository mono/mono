namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class FilterProviderCollection : Collection<IFilterProvider> {

        private static FilterComparer _filterComparer = new FilterComparer();
        private IResolver<IEnumerable<IFilterProvider>> _serviceResolver;

        public FilterProviderCollection() {
            _serviceResolver = new MultiServiceResolver<IFilterProvider>(() => Items);
        }

        public FilterProviderCollection(IList<IFilterProvider> providers)
            : base(providers) {
            _serviceResolver = new MultiServiceResolver<IFilterProvider>(() => Items);
        }

        internal FilterProviderCollection(IResolver<IEnumerable<IFilterProvider>> serviceResolver, params IFilterProvider[] providers)
            : base(providers) {
            _serviceResolver = serviceResolver ?? new MultiServiceResolver<IFilterProvider>(
                    () => Items
                    );
        }

        private IEnumerable<IFilterProvider> CombinedItems {
            get {
                return _serviceResolver.Current;
            }
        }

        private static bool AllowMultiple(object filterInstance) {
            IMvcFilter mvcFilter = filterInstance as IMvcFilter;
            if (mvcFilter == null) {
                return true;
            }

            return mvcFilter.AllowMultiple;
        }

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (actionDescriptor == null) {
                throw new ArgumentNullException("actionDescriptor");
            }

            IEnumerable<Filter> combinedFilters =
                CombinedItems.SelectMany(fp => fp.GetFilters(controllerContext, actionDescriptor))
                             .OrderBy(filter => filter, _filterComparer);

            // Remove duplicates from the back forward
            return RemoveDuplicates(combinedFilters.Reverse()).Reverse();
        }

        private IEnumerable<Filter> RemoveDuplicates(IEnumerable<Filter> filters) {
            HashSet<Type> visitedTypes = new HashSet<Type>();

            foreach (Filter filter in filters) {
                object filterInstance = filter.Instance;
                Type filterInstanceType = filterInstance.GetType();

                if (!visitedTypes.Contains(filterInstanceType) || AllowMultiple(filterInstance)) {
                    yield return filter;
                    visitedTypes.Add(filterInstanceType);
                }
            }
        }

        private class FilterComparer : IComparer<Filter> {
            public int Compare(Filter x, Filter y) {
                // Nulls always have to be less than non-nulls
                if (x == null && y == null) {
                    return 0;
                }
                if (x == null) {
                    return -1;
                }
                if (y == null) {
                    return 1;
                }

                // Sort first by order...

                if (x.Order < y.Order) {
                    return -1;
                }
                if (x.Order > y.Order) {
                    return 1;
                }

                // ...then by scope

                if (x.Scope < y.Scope) {
                    return -1;
                }
                if (x.Scope > y.Scope) {
                    return 1;
                }

                return 0;
            }
        }
    }
}
