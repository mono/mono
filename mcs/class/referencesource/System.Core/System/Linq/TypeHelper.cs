using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq {
    internal static class TypeHelper {
        internal static bool IsEnumerableType(Type enumerableType) {
            return FindGenericType(typeof(IEnumerable<>), enumerableType) != null;
        }
        internal static bool IsKindOfGeneric(Type type, Type definition) {
            return FindGenericType(definition, type) != null;
        }
        internal static Type GetElementType(Type enumerableType) {
            Type ienumType = FindGenericType(typeof(IEnumerable<>), enumerableType);
            if (ienumType != null)
                return ienumType.GetGenericArguments()[0];
            return enumerableType;
        }
        internal static Type FindGenericType(Type definition, Type type) {
            while (type != null && type != typeof(object)) {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == definition)
                    return type;
                if (definition.IsInterface) {
                    foreach(Type itype in type.GetInterfaces()) {
                        Type found = FindGenericType(definition, itype);
                        if (found != null)
                            return found;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }
        internal static bool IsNullableType(Type type) {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        internal static Type GetNonNullableType(Type type) {
            if (IsNullableType(type)) {
                return type.GetGenericArguments()[0];
            }
            return type;
        }
    }
}
