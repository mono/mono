namespace System.Web.ModelBinding {
    using System;
    using System.Diagnostics.CodeAnalysis;

    public sealed class TypeConverterModelBinder : IModelBinder {

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is recorded to be acted upon later.")]
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.ModelBinding.ValueProviderResult.ConvertTo(System.Type)", Justification = "The ValueProviderResult already has the necessary context to perform a culture-aware conversion.")]
        public bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            ValueProviderResult vpResult = bindingContext.UnvalidatedValueProvider.GetValue(bindingContext.ModelName, skipValidation: !bindingContext.ValidateRequest);
            if (vpResult == null) {
                return false; // no entry
            }

            object newModel;
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, vpResult);
            try {
                newModel = vpResult.ConvertTo(bindingContext.ModelType);
            }
            catch (Exception ex) {
                if (IsFormatException(ex)) {
                    // there was a type conversion failure
                    string errorString = ModelBinderErrorMessageProviders.TypeConversionErrorMessageProvider(modelBindingExecutionContext, bindingContext.ModelMetadata, vpResult.AttemptedValue);
                    if (errorString != null) {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, errorString);
                    }
                }
                else {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
                }
                return false;
            }

            ModelBinderUtil.ReplaceEmptyStringWithNull(bindingContext.ModelMetadata, ref newModel);
            bindingContext.Model = newModel;
            return true;
        }

        private static bool IsFormatException(Exception ex) {
            for (; ex != null; ex = ex.InnerException) {
                if (ex is FormatException) {
                    return true;
                }
            }
            return false;
        }

    }
}
