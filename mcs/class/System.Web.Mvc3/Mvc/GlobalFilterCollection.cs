namespace System.Web.Mvc {
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public sealed class GlobalFilterCollection : IEnumerable<Filter>, IFilterProvider {
        private List<Filter> _filters = new List<Filter>();

        public int Count {
            get {
                return _filters.Count;
            }
        }

        public void Add(object filter) {
            AddInternal(filter, order: null);
        }

        public void Add(object filter, int order) {
            AddInternal(filter, order);
        }

        private void AddInternal(object filter, int? order) {
            _filters.Add(new Filter(filter, FilterScope.Global, order));
        }

        public void Clear() {
            _filters.Clear();
        }

        public bool Contains(object filter) {
            return _filters.Any(f => f.Instance == filter);
        }

        public IEnumerator<Filter> GetEnumerator() {
            return _filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _filters.GetEnumerator();
        }

        IEnumerable<Filter> IFilterProvider.GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor) {
            return this;
        }

        public void Remove(object filter) {
            _filters.RemoveAll(f => f.Instance == filter);
        }
    }
}
