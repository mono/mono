namespace System.Web.ModelBinding {

    public sealed class MutableObjectModelBinderProvider : ModelBinderProvider {

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (!bindingContext.UnvalidatedValueProvider.ContainsPrefix(bindingContext.ModelName)) {
                // no values to bind
                return null;
            }

            if (bindingContext.ModelType == typeof(ComplexModel)) {
                // forbidden type - will cause a stack overflow if we try binding this type
                return null;
            }

            return new MutableObjectModelBinder();
        }

    }
}
