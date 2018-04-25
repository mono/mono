namespace System.Web.ModelBinding {

    public interface IModelBinder {
        bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext);
    }
}
