namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web.Mvc.Resources;

    public class ModelBindingContext {

        private static readonly Predicate<string> _defaultPropertyFilter = _ => true;

        private string _modelName;
        private ModelStateDictionary _modelState;
        private Predicate<string> _propertyFilter;
        private Dictionary<string, ModelMetadata> _propertyMetadata;

        public ModelBindingContext()
            : this(null) {
        }

        // copies certain values that won't change between parent and child objects,
        // e.g. ValueProvider, ModelState
        public ModelBindingContext(ModelBindingContext bindingContext) {
            if (bindingContext != null) {
                ModelState = bindingContext.ModelState;
                ValueProvider = bindingContext.ValueProvider;
            }
        }

        public bool FallbackToEmptyPrefix {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "Cannot remove setter as that's a breaking change")]
        public object Model {
            get {
                return ModelMetadata.Model;
            }
            set {
                throw new InvalidOperationException(MvcResources.ModelMetadata_PropertyNotSettable);
            }
        }

        public ModelMetadata ModelMetadata {
            get;
            set;
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

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The containing type is mutable.")]
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

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "Cannot remove setter as that's a breaking change")]
        public Type ModelType {
            get {
                return ModelMetadata.ModelType;
            }
            set {
                throw new InvalidOperationException(MvcResources.ModelMetadata_PropertyNotSettable);
            }
        }

        public Predicate<string> PropertyFilter {
            get {
                if (_propertyFilter == null) {
                    _propertyFilter = _defaultPropertyFilter;
                }
                return _propertyFilter;
            }
            set {
                _propertyFilter = value;
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

        public IValueProvider ValueProvider {
            get;
            set;
        }

        internal IUnvalidatedValueProvider UnvalidatedValueProvider {
            get {
                return (ValueProvider as IUnvalidatedValueProvider) ?? new UnvalidatedValueProviderWrapper(ValueProvider);
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
