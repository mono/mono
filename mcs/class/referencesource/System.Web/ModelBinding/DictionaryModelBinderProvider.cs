namespace System.Web.ModelBinding {
    using System.Collections.Generic;

    public sealed class DictionaryModelBinderProvider : ModelBinderProvider {

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (bindingContext.UnvalidatedValueProvider.ContainsPrefix(bindingContext.ModelName)) {
                return CollectionModelBinderUtil.GetGenericBinder(typeof(IDictionary<,>), typeof(Dictionary<,>), typeof(DictionaryModelBinder<,>), bindingContext.ModelMetadata);
            }
            else {
                return null;
            }
        }

    }
}
