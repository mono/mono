namespace System.Web.Mvc {
    using System;
    using System.ComponentModel;
    using System.Reflection;

    internal static class ParameterInfoUtil {

        public static bool TryGetDefaultValue(ParameterInfo parameterInfo, out object value) {
            // this will get the default value as seen by the VB / C# compilers
            // if no value was baked in, RawDefaultValue returns DBNull.Value
            object defaultValue = parameterInfo.DefaultValue;
            if (defaultValue != DBNull.Value) {
                value = defaultValue;
                return true;
            }

            // if the compiler did not bake in a default value, check the [DefaultValue] attribute
            DefaultValueAttribute[] attrs = (DefaultValueAttribute[])parameterInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attrs == null || attrs.Length == 0) {
                value = default(object);
                return false;
            }
            else {
                value = attrs[0].Value;
                return true;
            }
        }

    }
}
