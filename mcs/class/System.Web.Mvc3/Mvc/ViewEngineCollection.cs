namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Mvc.Resources;

    public class ViewEngineCollection : Collection<IViewEngine> {
        private IResolver<IEnumerable<IViewEngine>> _serviceResolver;

        public ViewEngineCollection() {
            _serviceResolver = new MultiServiceResolver<IViewEngine>(() => Items);
        }

        public ViewEngineCollection(IList<IViewEngine> list)
            : base(list) {
            _serviceResolver = new MultiServiceResolver<IViewEngine>(() => Items);
        }

        internal ViewEngineCollection(IResolver<IEnumerable<IViewEngine>> serviceResolver, params IViewEngine[] engines)
            : base(engines) {
            _serviceResolver = serviceResolver ?? new MultiServiceResolver<IViewEngine>(
                () => Items
                );
        }
        private IEnumerable<IViewEngine> CombinedItems {
            get {
                return _serviceResolver.Current;
            }
        }

        protected override void InsertItem(int index, IViewEngine item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IViewEngine item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        private ViewEngineResult Find(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator) {
            // First, look up using the cacheLocator and do not track the searched paths in non-matching view engines
            // Then, look up using the normal locator and track the searched paths so that an error view engine can be returned
            return Find(cacheLocator, trackSearchedPaths: false)
                ?? Find(locator, trackSearchedPaths: true);
        }

        private ViewEngineResult Find(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths) {
            // Returns
            //    1st result
            // OR list of searched paths (if trackSearchedPaths == true)
            // OR null
            ViewEngineResult result;

            List<string> searched = null;
            if (trackSearchedPaths) {
                searched = new List<string>();
            }

            foreach (IViewEngine engine in CombinedItems) {
                if (engine != null) {
                    result = lookup(engine);

                    if (result.View != null) {
                        return result;
                    }

                    if (trackSearchedPaths) {
                        searched.AddRange(result.SearchedLocations);
                    }
                }
            }

            if (trackSearchedPaths) {
                // Remove duplicate search paths since multiple view engines could have potentially looked at the same path
                return new ViewEngineResult(searched.Distinct().ToList());
            }
            else {
                return null;
            }
        }


        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(partialViewName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "partialViewName");
            }

            return Find(e => e.FindPartialView(controllerContext, partialViewName, true),
                        e => e.FindPartialView(controllerContext, partialViewName, false));
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(viewName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "viewName");
            }

            return Find(e => e.FindView(controllerContext, viewName, masterName, true),
                        e => e.FindView(controllerContext, viewName, masterName, false));
        }
    }
}
