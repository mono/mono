namespace System.Web.ModelBinding {
    using System;

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class QueryStringAttribute : ValueProviderSourceAttribute, IUnvalidatedValueProviderSource {

        private bool _validateInput = true;

        public string Key {
            get;
            private set;
        }

        public QueryStringAttribute()
            : this(null) {
        }

        public QueryStringAttribute(string key) {
            Key = key;
        }

        public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            return new QueryStringValueProvider(modelBindingExecutionContext);
        }

        public override string GetModelName() {
            return Key;
        }

        public bool ValidateInput {
            get {
                return _validateInput;
            }
            set {
                _validateInput = value;
            }
        }
    }
}
