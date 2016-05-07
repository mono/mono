
namespace System.Web.ModelBinding {

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class ViewStateAttribute : ValueProviderSourceAttribute {

        public string Key {
            get;
            private set;
        }

        public ViewStateAttribute()
            : this(null) {
        }

        public ViewStateAttribute(string key) {
            Key = key;
        }

        public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            return new ViewStateValueProvider(modelBindingExecutionContext);
        }

        public override string GetModelName() {
            return Key;
        }
    }
}
