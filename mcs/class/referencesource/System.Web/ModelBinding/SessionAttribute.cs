namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class SessionAttribute : ValueProviderSourceAttribute {

        public string Name {
            get;
            private set;
        }

        public SessionAttribute()
            : this(null) {
        }

        public SessionAttribute(string name) {
            Name = name;
        }

        public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            HttpSessionStateBase session = modelBindingExecutionContext.HttpContext.Session;
            if (session == null) {
                // session is disabled
                return null;
            }

            Dictionary<string, object> backingStore = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in session) {
                if (key != null) {
                    backingStore[key] = session[key]; // copy to backing store
                }
            }

            // use the invariant culture since Session contains serialized objects
            return new DictionaryValueProvider<object>(backingStore, CultureInfo.InvariantCulture);
        }

        public override string GetModelName() {
            return Name;
        }

    }
}
