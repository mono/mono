using System.Reflection;

namespace System.Configuration {
    internal static partial class TypeUtil {
        internal static Type GetTypeWithReflectionPermission(string typeString, bool throwOnError) {
            return GetType(typeString, throwOnError);
        }

        internal static object CreateInstanceWithReflectionPermission(Type type) {
            object result = Activator.CreateInstance(type, true); // create non-public types
            return result;
        }

        // Check if the type is allowed to be used in config by checking the APTCA bit
        internal static bool IsTypeAllowedInConfig(Type t) {
            // if partial trust is important, port the original. betting otherwise.
            return true;
        }
    }
}