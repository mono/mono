namespace System.Web.ModelBinding {
    using System;
    using System.Globalization;

    // Provides configuration settings common to the new model binding system.
    public static class ModelBinderErrorMessageProviders {

        private static ModelBinderErrorMessageProvider _typeConversionErrorMessageProvider;
        private static ModelBinderErrorMessageProvider _valueRequiredErrorMessageProvider;

        public static ModelBinderErrorMessageProvider TypeConversionErrorMessageProvider {
            get {
                if (_typeConversionErrorMessageProvider == null) {
                    _typeConversionErrorMessageProvider = DefaultTypeConversionErrorMessageProvider;
                }
                return _typeConversionErrorMessageProvider;
            }
            set {
                _typeConversionErrorMessageProvider = value;
            }
        }

        public static ModelBinderErrorMessageProvider ValueRequiredErrorMessageProvider {
            get {
                if (_valueRequiredErrorMessageProvider == null) {
                    _valueRequiredErrorMessageProvider = DefaultValueRequiredErrorMessageProvider;
                }
                return _valueRequiredErrorMessageProvider;
            }
            set {
                _valueRequiredErrorMessageProvider = value;
            }
        }

        private static string DefaultTypeConversionErrorMessageProvider(ModelBindingExecutionContext modelBindingExecutionContext, ModelMetadata modelMetadata, object incomingValue) {
            return GetResourceCommon(modelBindingExecutionContext, modelMetadata, incomingValue, GetValueInvalidResource);
        }

        private static string DefaultValueRequiredErrorMessageProvider(ModelBindingExecutionContext modelBindingExecutionContext, ModelMetadata modelMetadata, object incomingValue) {
            return GetResourceCommon(modelBindingExecutionContext, modelMetadata, incomingValue, GetValueRequiredResource);
        }

        private static string GetResourceCommon(ModelBindingExecutionContext modelBindingExecutionContext, ModelMetadata modelMetadata, object incomingValue, Func<ModelBindingExecutionContext, string> resourceAccessor) {
            string displayName = modelMetadata.GetDisplayName();
            string errorMessageTemplate = resourceAccessor(modelBindingExecutionContext);
            string errorMessage = String.Format(CultureInfo.CurrentCulture, errorMessageTemplate, incomingValue, displayName);
            return errorMessage;
        }

        private static string GetUserResourceString(ModelBindingExecutionContext modelBindingExecutionContext, string resourceName) {
#if UNDEF
            return GetUserResourceString(modelBindingExecutionContext, resourceName, DefaultModelBinder.ResourceClassKey);
#endif
            return GetUserResourceString(modelBindingExecutionContext, resourceName, String.Empty);
        }

        // If the user specified a ResourceClassKey try to load the resource they specified.
        // If the class key is invalid, an exception will be thrown.
        // If the class key is valid but the resource is not found, it returns null, in which
        // case it will fall back to the MVC default error message.
        internal static string GetUserResourceString(ModelBindingExecutionContext modelBindingExecutionContext, string resourceName, string resourceClassKey) {
            return (!String.IsNullOrEmpty(resourceClassKey) && (modelBindingExecutionContext != null) && (modelBindingExecutionContext.HttpContext != null))
                ? modelBindingExecutionContext.HttpContext.GetGlobalResourceObject(resourceClassKey, resourceName, CultureInfo.CurrentUICulture) as string
                : null;
        }

        private static string GetValueInvalidResource(ModelBindingExecutionContext modelBindingExecutionContext) {
            return GetUserResourceString(modelBindingExecutionContext, "PropertyValueInvalid") ?? SR.GetString(SR.ModelBinderConfig_ValueInvalid);
        }

        private static string GetValueRequiredResource(ModelBindingExecutionContext modelBindingExecutionContext) {
            return GetUserResourceString(modelBindingExecutionContext, "PropertyValueRequired") ?? SR.GetString(SR.ModelBinderConfig_ValueRequired);
        }
    }
}
