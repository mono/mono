namespace System.Web.ModelBinding {

    public sealed class TypeMatchModelBinder : IModelBinder {

        public bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ValueProviderResult vpResult = GetCompatibleValueProviderResult(bindingContext);
            if (vpResult == null) {
                return false; // conversion would have failed
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, vpResult);
            object model = vpResult.RawValue;
            ModelBinderUtil.ReplaceEmptyStringWithNull(bindingContext.ModelMetadata, ref model);
            bindingContext.Model = model;

            return true;
        }

        internal static ValueProviderResult GetCompatibleValueProviderResult(ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            ValueProviderResult vpResult = bindingContext.UnvalidatedValueProvider.GetValue(bindingContext.ModelName, skipValidation: !bindingContext.ValidateRequest);
            if (vpResult == null) {
                return null; // the value doesn't exist
            }

            if (!TypeHelpers.IsCompatibleObject(bindingContext.ModelType, vpResult.RawValue)) {
                return null; // value is of incompatible type
            }

            return vpResult;
        }

    }
}
