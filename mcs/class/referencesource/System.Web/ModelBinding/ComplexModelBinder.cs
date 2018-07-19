namespace System.Web.ModelBinding {

    public sealed class ComplexModelBinder : IModelBinder {

        public bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(ComplexModel), false /* allowNullModel */);

            ComplexModel complexModel = (ComplexModel)bindingContext.Model;
            foreach (ModelMetadata propertyMetadata in complexModel.PropertyMetadata) {
                ModelBindingContext propertyBindingContext = new ModelBindingContext(bindingContext) {
                    ModelMetadata = propertyMetadata,
                    ModelName = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, propertyMetadata.PropertyName)
                };

                // bind and propagate the values
                IModelBinder propertyBinder = bindingContext.ModelBinderProviders.GetBinder(modelBindingExecutionContext, propertyBindingContext);
                if (propertyBinder != null) {
                    if (propertyBinder.BindModel(modelBindingExecutionContext, propertyBindingContext)) {
                        complexModel.Results[propertyMetadata] = new ComplexModelResult(propertyBindingContext.Model, propertyBindingContext.ValidationNode);
                    }
                    else {
                        complexModel.Results[propertyMetadata] = null;
                    }
                }
            }

            return true;
        }

    }
}
