namespace System.Web.ModelBinding {

    // Returns a binder that can bind ComplexModel objects.
    public sealed class ComplexModelBinderProvider : ModelBinderProvider {

        // This is really just a simple binder.
        private static readonly SimpleModelBinderProvider _underlyingProvider = GetUnderlyingProvider();

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            return _underlyingProvider.GetBinder(modelBindingExecutionContext, bindingContext);
        }

        private static SimpleModelBinderProvider GetUnderlyingProvider() {
            return new SimpleModelBinderProvider(typeof(ComplexModel), new ComplexModelBinder()) {
                SuppressPrefixCheck = true
            };
        }

    }
}
