namespace System.Web.ModelBinding {

    internal static class KeyValuePairModelBinderUtil {

        public static bool TryBindStrongModel<TModel>(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext parentBindingContext, string propertyName, ModelMetadataProvider metadataProvider, out TModel model) {
            ModelBindingContext propertyBindingContext = new ModelBindingContext(parentBindingContext) {
                ModelMetadata = metadataProvider.GetMetadataForType(null, typeof(TModel)),
                ModelName = ModelBinderUtil.CreatePropertyModelName(parentBindingContext.ModelName, propertyName)
            };

            IModelBinder binder = parentBindingContext.ModelBinderProviders.GetBinder(modelBindingExecutionContext, propertyBindingContext);
            if (binder != null) {
                if (binder.BindModel(modelBindingExecutionContext, propertyBindingContext)) {
                    object untypedModel = propertyBindingContext.Model;
                    model = ModelBinderUtil.CastOrDefault<TModel>(untypedModel);
                    parentBindingContext.ValidationNode.ChildNodes.Add(propertyBindingContext.ValidationNode);
                    return true;
                }
            }

            model = default(TModel);
            return false;
        }

    }
}
