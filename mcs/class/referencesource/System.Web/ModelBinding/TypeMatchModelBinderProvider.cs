namespace System.Web.ModelBinding {

    // Returns a binder that can extract a ValueProviderResult.RawValue and return it directly.
    [ModelBinderProviderOptions(FrontOfList = true)]
    public sealed class TypeMatchModelBinderProvider : ModelBinderProvider {

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            return (TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext) != null)
                ? new TypeMatchModelBinder()
                : null /* no match */;
        }

    }
}
