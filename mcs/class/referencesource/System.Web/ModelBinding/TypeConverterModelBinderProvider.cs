namespace System.Web.ModelBinding {
    using System.ComponentModel;

    // Returns a binder that can perform conversions using a .NET TypeConverter.
    public sealed class TypeConverterModelBinderProvider : ModelBinderProvider {

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            ValueProviderResult vpResult = bindingContext.UnvalidatedValueProvider.GetValue(bindingContext.ModelName, skipValidation: !bindingContext.ValidateRequest);
            if (vpResult == null) {
                return null; // no value to convert
            }

            if (!TypeDescriptor.GetConverter(bindingContext.ModelType).CanConvertFrom(typeof(string))) {
                return null; // this type cannot be converted
            }

            return new TypeConverterModelBinder();
        }

    }
}
