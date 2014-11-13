namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class ModelBindingContext {

        private ModelBinderProviderCollection _modelBinderProviders;
        private ModelMetadata _modelMetadata;
        private string _modelName;
        private ModelStateDictionary _modelState;
        private Dictionary<string, ModelMetadata> _propertyMetadata;
        private ModelValidationNode _validationNode;
        private IValueProvider _valueProvider;

        public ModelBindingContext()
            : this(null) {
                ValidateRequest = true;
        }

        // copies certain values that won't change between parent and child objects,
        // e.g. ValueProvider, ModelState
        public ModelBindingContext(ModelBindingContext bindingContext) {
            if (bindingContext != null) {
                ModelBinderProviders = bindingContext.ModelBinderProviders;
                ModelState = bindingContext.ModelState;
                ValueProvider = bindingContext.ValueProvider;
                ValidateRequest = bindingContext.ValidateRequest;
            }
        }

        public object Model {
            get {
                EnsureModelMetadata();
                return ModelMetadata.Model;
            }
            set {
                EnsureModelMetadata();
                ModelMetadata.Model = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is writeable to support unit testing")]
        public ModelBinderProviderCollection ModelBinderProviders {
            get {
                if (_modelBinderProviders == null) {
                    _modelBinderProviders = System.Web.ModelBinding.ModelBinderProviders.Providers;
                }
                return _modelBinderProviders;
            }
            set {
                _modelBinderProviders = value;
            }
        }

        public ModelMetadata ModelMetadata {
            get {
                return _modelMetadata;
            }
            set {
                _modelMetadata = value;
            }
        }

        public string ModelName {
            get {
                if (_modelName == null) {
                    _modelName = String.Empty;
                }
                return _modelName;
            }
            set {
                _modelName = value;
            }
        }

        public bool ValidateRequest {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is writeable to support unit testing")]
        public ModelStateDictionary ModelState {
            get {
                if (_modelState == null) {
                    _modelState = new ModelStateDictionary();
                }
                return _modelState;
            }
            set {
                _modelState = value;
            }
        }

        public Type ModelType {
            get {
                EnsureModelMetadata();
                return ModelMetadata.ModelType;
            }
        }

        public IDictionary<string, ModelMetadata> PropertyMetadata {
            get {
                if (_propertyMetadata == null) {
                    _propertyMetadata = ModelMetadata.Properties.ToDictionary(m => m.PropertyName, StringComparer.OrdinalIgnoreCase);
                }

                return _propertyMetadata;
            }
        }

        public ModelValidationNode ValidationNode {
            get {
                if (_validationNode == null) {
                    _validationNode = new ModelValidationNode(ModelMetadata, ModelName);
                }
                return _validationNode;
            }
            set {
                _validationNode = value;
            }
        }

        public IValueProvider ValueProvider {
            get {
                return _valueProvider;
            }
            set {
                _valueProvider = value;
            }
        }

        internal IUnvalidatedValueProvider UnvalidatedValueProvider {
            get {
                return (ValueProvider as IUnvalidatedValueProvider ?? new UnvalidatedValueProviderWrapper(ValueProvider));
            }
        }

        private void EnsureModelMetadata() {
            if (ModelMetadata == null) {
                throw Error.ModelBindingContext_ModelMetadataMustBeSet();
            }
        }

        // Used to wrap an IValueProvider in an IUnvalidatedValueProvider
        private sealed class UnvalidatedValueProviderWrapper : IValueProvider, IUnvalidatedValueProvider {
            private readonly IValueProvider _backingProvider;

            public UnvalidatedValueProviderWrapper(IValueProvider backingProvider) {
                _backingProvider = backingProvider;
            }

            public ValueProviderResult GetValue(string key, bool skipValidation) {
                // 'skipValidation' isn't understood by the backing provider and can be ignored
                return GetValue(key);
            }

            public bool ContainsPrefix(string prefix) {
                return _backingProvider.ContainsPrefix(prefix);
            }

            public ValueProviderResult GetValue(string key) {
                return _backingProvider.GetValue(key);
            }
        }
    }
}
