//------------------------------------------------------------------------------
// <copyright file="PropertyConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public static class PropertyConverter {

        private static readonly Type[] s_parseMethodTypes = new Type[] { typeof(string) };
        private static readonly Type[] s_parseMethodTypesWithSOP = new Type[] { typeof(string), typeof(IServiceProvider) };

        /*
         * Contains helpers to convert properties from strings to their types and vice versa.
         */

        /*
         * Converts a persisted enumeration value into its numeric value.
         * Hyphen characters in the persisted format are converted to underscores.
         */

        /// <devdoc>
        /// </devdoc>
        public static object EnumFromString(Type enumType, string value) {
            try {
                return Enum.Parse(enumType, value, true);
            }
            catch {
                return null;
            }
        }

        /*
         * Converts a numeric enumerated value into its persisted form, which is the
         * code name with underscores replaced by hyphens.
         */

        /// <devdoc>
        /// </devdoc>
        public static string EnumToString(Type enumType, object enumValue) {
            string value = Enum.Format(enumType, enumValue, "G");

            // 

            return value.Replace('_','-');
        }

        /*
         * Converts the persisted string into an object using the object's
         * FromString method.
         */

        /// <devdoc>
        /// </devdoc>
        public static object ObjectFromString(Type objType, MemberInfo propertyInfo, string value) {
            if (value == null)
                return null;

            // Blank valued bools don't map with FromString. Return null to allow
            // caller to interpret.
            if (objType.Equals(typeof(bool)) && value.Length == 0) {
                return null;
            }

            bool useParseMethod = true;
            object ret = null;

            try {
                if (objType.IsEnum) {
                    useParseMethod = false;
                    ret = EnumFromString(objType, value);
                }
                else if (objType.Equals(typeof(string))) {
                    useParseMethod = false;
                    ret = value;
                }
                else {
                    PropertyDescriptor pd = null;
                    if (propertyInfo != null) {
                        pd = TypeDescriptor.GetProperties(propertyInfo.ReflectedType)[propertyInfo.Name];
                    }
                    if (pd != null) {
                        TypeConverter converter = pd.Converter;
                        if (converter != null && converter.CanConvertFrom(typeof(string))) {
                            useParseMethod = false;
                            ret = converter.ConvertFromInvariantString(value);
                        }
                    }
                }
            }
            catch {
            }

            if (useParseMethod) {
                // resort to Parse static method on the type

                // First try Parse(string, IServiceProvider);
                MethodInfo methodInfo = objType.GetMethod("Parse", s_parseMethodTypesWithSOP);

                if (methodInfo != null) {
                    object[] parameters = new object[2];

                    parameters[0] = value;
                    parameters[1] = CultureInfo.InvariantCulture;
                    try {
                        ret = Util.InvokeMethod(methodInfo, null, parameters);
                    }
                    catch {
                    }
                }
                else {
                    // Try the simpler: Parse(string);
                    methodInfo = objType.GetMethod("Parse", s_parseMethodTypes);

                    if (methodInfo != null) {
                        object[] parameters = new object[1];

                        parameters[0] = value;
                        try {
                            ret = Util.InvokeMethod(methodInfo, null, parameters);
                        }
                        catch {
                        }
                    }
                }
            }

            if (ret == null) {
                // Unhandled... throw an exception, so user sees an error at parse time
                // Note that we don't propagate inner exceptions here, since they usually
                // do not give any information about where the bad value existed on
                // the object being initialized, whereas, our exception gives that
                // information.
                throw new HttpException(SR.GetString(SR.Type_not_creatable_from_string,
                                                                         objType.FullName, value, propertyInfo.Name));
            }

            return ret;
        }
    }
}

