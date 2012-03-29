namespace System.Web.Mvc {

    public interface IModelBinderProvider {
        IModelBinder GetBinder(Type modelType);
    }
}
