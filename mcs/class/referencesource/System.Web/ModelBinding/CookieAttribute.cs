namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class CookieAttribute : ValueProviderSourceAttribute, IUnvalidatedValueProviderSource {

        private bool _validateInput = true;

        public string Name {
            get;
            private set;
        }

        public CookieAttribute()
            : this(null) {
        }

        public CookieAttribute(string name) {
            Name = name;
        }

        public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {

            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            return new CookieValueProvider(modelBindingExecutionContext);
        }

        public override string GetModelName() {
            return Name;
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
