namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ModelBinderProviderCollection : Collection<IModelBinderProvider> {

        private IResolver<IEnumerable<IModelBinderProvider>> _serviceResolver;

        public ModelBinderProviderCollection() {
            _serviceResolver = new MultiServiceResolver<IModelBinderProvider>(() => Items);
        }

        public ModelBinderProviderCollection(IList<IModelBinderProvider> list)
            : base(list) {
            _serviceResolver = new MultiServiceResolver<IModelBinderProvider>(() => Items);
        }

        internal ModelBinderProviderCollection(IResolver<IEnumerable<IModelBinderProvider>> resolver, params IModelBinderProvider[] providers)
            : base(providers) {
            _serviceResolver = resolver ?? new MultiServiceResolver<IModelBinderProvider>(() => Items);
        }

        private IEnumerable<IModelBinderProvider> CombinedItems {
            get {
                return _serviceResolver.Current;
            }
        }

        protected override void InsertItem(int index, IModelBinderProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IModelBinderProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        public IModelBinder GetBinder(Type modelType) {

            if (modelType == null) {
                throw new ArgumentNullException("modelType");
            }

            var modelBinders = from providers in CombinedItems
                               let modelBinder = providers.GetBinder(modelType)
                               where modelBinder != null
                               select modelBinder;

            return modelBinders.FirstOrDefault();
        }
    }
}
