namespace System.Web.ModelBinding {
    using System.Collections.Generic;

    public sealed class KeyValuePairModelBinderProvider : ModelBinderProvider {

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            string keyFieldName = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, "key");
            string valueFieldName = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, "value");

            if (bindingContext.UnvalidatedValueProvider.ContainsPrefix(keyFieldName) && bindingContext.UnvalidatedValueProvider.ContainsPrefix(valueFieldName)) {
                return ModelBinderUtil.GetPossibleBinderInstance(bindingContext.ModelType, typeof(KeyValuePair<,>) /* supported model type */, typeof(KeyValuePairModelBinder<,>) /* binder type */);
            }
            else {
                // 'key' or 'value' missing
                return null;
            }
        }

    }
}
