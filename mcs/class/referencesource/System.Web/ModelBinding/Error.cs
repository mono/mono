namespace System.Web.ModelBinding {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    internal static class Error {

        public static InvalidOperationException BindingBehavior_ValueNotFound(string fieldName) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.BindingBehavior_ValueNotFound),
                fieldName);
            return new InvalidOperationException(errorString);
        }

        public static ArgumentException Common_TypeMustImplementInterface(Type providedType, Type requiredInterfaceType, string parameterName) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Common_TypeMustImplementInterface),
                providedType, requiredInterfaceType);
            return new ArgumentException(errorString, parameterName);
        }

        public static ArgumentException GenericModelBinderProvider_ParameterMustSpecifyOpenGenericType(Type specifiedType, string parameterName) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.GenericModelBinderProvider_ParameterMustSpecifyOpenGenericType),
                specifiedType);
            return new ArgumentException(errorString, parameterName);
        }

        public static ArgumentException GenericModelBinderProvider_TypeArgumentCountMismatch(Type modelType, Type modelBinderType) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.GenericModelBinderProvider_TypeArgumentCountMismatch),
                modelType, modelType.GetGenericArguments().Length, modelBinderType, modelBinderType.GetGenericArguments().Length);
            return new ArgumentException(errorString, "modelBinderType");
        }

        public static InvalidOperationException ModelBinderProviderCollection_BinderForTypeNotFound(Type modelType) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ModelBinderProviderCollection_BinderForTypeNotFound),
                modelType);
            return new InvalidOperationException(errorString);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The purpose of this class is to throw errors on behalf of other methods")]
        public static ArgumentException ModelBinderUtil_ModelCannotBeNull(Type expectedType) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ModelBinderUtil_ModelCannotBeNull),
                expectedType);
            return new ArgumentException(errorString, "bindingContext");
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The purpose of this class is to throw errors on behalf of other methods")]
        public static ArgumentException ModelBinderUtil_ModelInstanceIsWrong(Type actualType, Type expectedType) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ModelBinderUtil_ModelInstanceIsWrong),
                actualType, expectedType);
            return new ArgumentException(errorString, "bindingContext");
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The purpose of this class is to throw errors on behalf of other methods")]
        public static ArgumentException ModelBinderUtil_ModelMetadataCannotBeNull() {
            return new ArgumentException(SR.GetString(SR.ModelBinderUtil_ModelMetadataCannotBeNull), "bindingContext");
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The purpose of this class is to throw errors on behalf of other methods")]
        public static ArgumentException ModelBinderUtil_ModelTypeIsWrong(Type actualType, Type expectedType) {
            string errorString = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ModelBinderUtil_ModelTypeIsWrong),
                actualType, expectedType);
            return new ArgumentException(errorString, "bindingContext");
        }

        public static InvalidOperationException ModelBindingContext_ModelMetadataMustBeSet() {
            return new InvalidOperationException(SR.GetString(SR.ModelBindingContext_ModelMetadataMustBeSet));
        }

    }
}
