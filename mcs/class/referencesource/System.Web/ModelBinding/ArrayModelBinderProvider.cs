namespace System.Web.ModelBinding {
    using System;

    public sealed class ArrayModelBinderProvider : ModelBinderProvider {

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (!bindingContext.ModelMetadata.IsReadOnly && bindingContext.ModelType.IsArray &&
                bindingContext.UnvalidatedValueProvider.ContainsPrefix(bindingContext.ModelName)) {
                Type elementType = bindingContext.ModelType.GetElementType();
                return (IModelBinder)Activator.CreateInstance(typeof(ArrayModelBinder<>).MakeGenericType(elementType));
            }

            return null;
        }

    }
}
