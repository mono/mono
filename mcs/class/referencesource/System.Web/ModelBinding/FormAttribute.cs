namespace System.Web.ModelBinding {
    using System;

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class FormAttribute : ValueProviderSourceAttribute, IUnvalidatedValueProviderSource {

        private bool _validateInput = true;

        public string FieldName {
            get;
            private set;
        }

        public FormAttribute()
            : this(null) {
        }

        public FormAttribute(string fieldName) {
            FieldName = fieldName;
        }

        public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            return new FormValueProvider(modelBindingExecutionContext);
        }

        public override string GetModelName() {
            return FieldName;
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
