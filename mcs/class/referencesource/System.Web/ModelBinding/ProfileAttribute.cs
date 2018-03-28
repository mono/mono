namespace System.Web.ModelBinding {
    using System;

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class ProfileAttribute : ValueProviderSourceAttribute {

        public string Key {
            get;
            private set;
        }

        public ProfileAttribute()
            : this(null) {
        }

        public ProfileAttribute(string key) {
            Key = key;
        }

        public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            return new ProfileValueProvider(modelBindingExecutionContext);
        }

        public override string GetModelName() {
            return Key;
        }

    }
}
