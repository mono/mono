namespace System.Web.ModelBinding {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;

    internal static class ModelBinderUtil {

        public static TModel CastOrDefault<TModel>(object model) {
            return (model is TModel) ? (TModel)model : default(TModel);
        }

        public static string CreateIndexModelName(string parentName, int index) {
            return CreateIndexModelName(parentName, index.ToString(CultureInfo.InvariantCulture));
        }

        public static string CreateIndexModelName(string parentName, string index) {
            return (parentName.Length == 0) ? "[" + index + "]" : parentName + "[" + index + "]";
        }

        public static string CreatePropertyModelName(string prefix, string propertyName) {
            if (String.IsNullOrEmpty(prefix)) {
                return propertyName ?? String.Empty;
            }
            else if (String.IsNullOrEmpty(propertyName)) {
                return prefix ?? String.Empty;
            }
            else {
                return prefix + "." + propertyName;
            }
        }

        public static IModelBinder GetPossibleBinderInstance(Type closedModelType, Type openModelType, Type openBinderType) {
            Type[] typeArguments = TypeHelpers.GetTypeArgumentsIfMatch(closedModelType, openModelType);
            return (typeArguments != null) ? (IModelBinder)Activator.CreateInstance(openBinderType.MakeGenericType(typeArguments)) : null;
        }

        public static object[] RawValueToObjectArray(object rawValue) {
            // precondition: rawValue is not null

            // Need to special-case String so it's not caught by the IEnumerable check which follows
            if (rawValue is string) {
                return new object[] { rawValue };
            }

            object[] rawValueAsObjectArray = rawValue as object[];
            if (rawValueAsObjectArray != null) {
                return rawValueAsObjectArray;
            }

            IEnumerable rawValueAsEnumerable = rawValue as IEnumerable;
            if (rawValueAsEnumerable != null) {
                return rawValueAsEnumerable.Cast<object>().ToArray();
            }

            // fallback
            return new object[] { rawValue };
        }

        public static void ReplaceEmptyStringWithNull(ModelMetadata modelMetadata, ref object model) {
            if (modelMetadata.ConvertEmptyStringToNull && StringIsEmptyOrWhitespace(model as string)) {
                model = null;
            }
        }

        // Based on String.IsNullOrWhitespace
        private static bool StringIsEmptyOrWhitespace(string s) {
            if (s == null) {
                return false;
            }

            if (s.Length != 0) {
                for (int i = 0; i < s.Length; i++) {
                    if (!Char.IsWhiteSpace(s[i])) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void ValidateBindingContext(ModelBindingContext bindingContext) {
            if (bindingContext == null) {
                throw new ArgumentNullException("bindingContext");
            }

            if (bindingContext.ModelMetadata == null) {
                throw Error.ModelBinderUtil_ModelMetadataCannotBeNull();
            }
        }

        public static void ValidateBindingContext(ModelBindingContext bindingContext, Type requiredType, bool allowNullModel) {
            ValidateBindingContext(bindingContext);

            if (bindingContext.ModelType != requiredType) {
                throw Error.ModelBinderUtil_ModelTypeIsWrong(bindingContext.ModelType, requiredType);
            }

            if (!allowNullModel && bindingContext.Model == null) {
                throw Error.ModelBinderUtil_ModelCannotBeNull(requiredType);
            }

            if (bindingContext.Model != null && !requiredType.IsInstanceOfType(bindingContext.Model)) {
                throw Error.ModelBinderUtil_ModelInstanceIsWrong(bindingContext.Model.GetType(), requiredType);
            }
        }

    }
}
