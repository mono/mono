namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ValueProviderFactoryCollection : Collection<ValueProviderFactory> {
        private IResolver<IEnumerable<ValueProviderFactory>> _serviceResolver;

        public ValueProviderFactoryCollection() {
            _serviceResolver = new MultiServiceResolver<ValueProviderFactory>(() => Items);
        }

        public ValueProviderFactoryCollection(IList<ValueProviderFactory> list)
            : base(list) {
            _serviceResolver = new MultiServiceResolver<ValueProviderFactory>(() => Items);
        }

        internal ValueProviderFactoryCollection(IResolver<IEnumerable<ValueProviderFactory>> serviceResolver, params ValueProviderFactory[] valueProviderFactories)
            : base(valueProviderFactories) {
            _serviceResolver = serviceResolver ?? new MultiServiceResolver<ValueProviderFactory>(
                   () => Items
                   );
        }

        public IValueProvider GetValueProvider(ControllerContext controllerContext) {
            var valueProviders = from factory in _serviceResolver.Current
                                 let valueProvider = factory.GetValueProvider(controllerContext)
                                 where valueProvider != null
                                 select valueProvider;

            return new ValueProviderCollection(valueProviders.ToList());
        }


        protected override void InsertItem(int index, ValueProviderFactory item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ValueProviderFactory item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

    }
}
