namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ModelValidatorProviderCollection : Collection<ModelValidatorProvider> {
        private IResolver<IEnumerable<ModelValidatorProvider>> _serviceResolver;

        public ModelValidatorProviderCollection() {
            _serviceResolver = new MultiServiceResolver<ModelValidatorProvider>(() => Items);
        }

        public ModelValidatorProviderCollection(IList<ModelValidatorProvider> list)
            : base(list) {
            _serviceResolver = new MultiServiceResolver<ModelValidatorProvider>(() => Items);
        }

        internal ModelValidatorProviderCollection(IResolver<IEnumerable<ModelValidatorProvider>> serviceResolver, params ModelValidatorProvider[] validatorProvidors)
            : base(validatorProvidors) {
            _serviceResolver = serviceResolver ??  new MultiServiceResolver<ModelValidatorProvider>(
                ()=>Items
                );
        }

        private IEnumerable<ModelValidatorProvider> CombinedItems {
            get {
                return _serviceResolver.Current;
            }
        }

        protected override void InsertItem(int index, ModelValidatorProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ModelValidatorProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        public IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context) {
            return CombinedItems.SelectMany(provider => provider.GetValidators(metadata, context));
        }

    }
}
