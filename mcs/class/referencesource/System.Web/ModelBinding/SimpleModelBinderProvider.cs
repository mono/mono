namespace System.Web.ModelBinding {
    using System;

    // Returns a user-specified binder for a given type.
    public sealed class SimpleModelBinderProvider : ModelBinderProvider {

        private readonly Func<IModelBinder> _modelBinderFactory;
        private readonly Type _modelType;

        public SimpleModelBinderProvider(Type modelType, IModelBinder modelBinder) {
            if (modelType == null) {
                throw new ArgumentNullException("modelType");
            }
            if (modelBinder == null) {
                throw new ArgumentNullException("modelBinder");
            }

            _modelType = modelType;
            _modelBinderFactory = () => modelBinder;
        }

        public SimpleModelBinderProvider(Type modelType, Func<IModelBinder> modelBinderFactory) {
            if (modelType == null) {
                throw new ArgumentNullException("modelType");
            }
            if (modelBinderFactory == null) {
                throw new ArgumentNullException("modelBinderFactory");
            }

            _modelType = modelType;
            _modelBinderFactory = modelBinderFactory;
        }

        public Type ModelType {
            get {
                return _modelType;
            }
        }

        public bool SuppressPrefixCheck {
            get;
            set;
        }

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (bindingContext.ModelType == ModelType) {
                if (SuppressPrefixCheck || bindingContext.UnvalidatedValueProvider.ContainsPrefix(bindingContext.ModelName)) {
                    return _modelBinderFactory();
                }
            }

            return null;
        }

    }
}
