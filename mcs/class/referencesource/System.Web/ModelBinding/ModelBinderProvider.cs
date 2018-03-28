namespace System.Web.ModelBinding {

    public abstract class ModelBinderProvider {
        public abstract IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext);
    }
}
