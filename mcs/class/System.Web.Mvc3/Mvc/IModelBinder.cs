namespace System.Web.Mvc {

    public interface IModelBinder {
        object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext);
    }
}
