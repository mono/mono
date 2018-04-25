namespace System.Web.ModelBinding {
    using System.Collections.Generic;

    public sealed class CollectionModelBinderProvider : ModelBinderProvider {

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (bindingContext.UnvalidatedValueProvider.ContainsPrefix(bindingContext.ModelName)) {
                return CollectionModelBinderUtil.GetGenericBinder(typeof(ICollection<>), typeof(List<>), typeof(CollectionModelBinder<>), bindingContext.ModelMetadata);
            }
            else {
                return null;
            }
        }

    }
}
