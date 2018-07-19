namespace System.Web.ModelBinding {
    using System.Collections.Generic;

    public sealed class KeyValuePairModelBinder<TKey, TValue> : IModelBinder {

        private ModelMetadataProvider _metadataProvider;

        internal ModelMetadataProvider MetadataProvider {
            get {
                if (_metadataProvider == null) {
                    _metadataProvider = ModelMetadataProviders.Current;
                }
                return _metadataProvider;
            }
            set {
                _metadataProvider = value;
            }
        }

        public bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(KeyValuePair<TKey, TValue>), true /* allowNullModel */);

            TKey key;
            bool keyBindingSucceeded = KeyValuePairModelBinderUtil.TryBindStrongModel<TKey>(modelBindingExecutionContext, bindingContext, "key", MetadataProvider, out key);

            TValue value;
            bool valueBindingSucceeded = KeyValuePairModelBinderUtil.TryBindStrongModel<TValue>(modelBindingExecutionContext, bindingContext, "value", MetadataProvider, out value);

            if (keyBindingSucceeded && valueBindingSucceeded) {
                bindingContext.Model = new KeyValuePair<TKey, TValue>(key, value);
            }
            return keyBindingSucceeded || valueBindingSucceeded;
        }

    }
}
