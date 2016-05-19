namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Linq;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Resources;
    using System.Globalization;

    internal static class DataSourceHelper {
        public static object SaveViewState(ParameterCollection parameters) {
            if (parameters != null) {
                return ((IStateManager)parameters).SaveViewState();
            }
            return null;
        }

        public static void TrackViewState(ParameterCollection parameters) {
            if (parameters != null) {
                ((IStateManager)parameters).TrackViewState();
            }
        }        

        public static IDictionary<string, object> ToDictionary(this ParameterCollection parameters, HttpContext context, Control control) {
            return ToDictionary(parameters.GetValues(context, control));
        }

        internal static IDictionary<string, object> ToDictionary(this IOrderedDictionary parameterValues) {
            Dictionary<string, object> values = new Dictionary<string, object>(parameterValues.Count, StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry entry in parameterValues) {
                values[(string)entry.Key] = entry.Value;
            }

            return values;
        }     

        public static IOrderedDictionary ToCaseInsensitiveDictionary(this IDictionary dictionary) {
            if (dictionary != null) {
                IOrderedDictionary destination = new OrderedDictionary(dictionary.Count, StringComparer.OrdinalIgnoreCase);

                foreach (DictionaryEntry de in dictionary) {
                    destination[de.Key] = de.Value;
                }

                return destination;
            }
            return null;
        }

        internal static object CreateObjectInstance(Type type) {
            // FastCreatePublicInstance is faster than Activator.CreateInstance since it caches the type factories.
            return HttpRuntime.FastCreatePublicInstance(type);
        }

        public static bool MergeDictionaries(object dataObjectType, ParameterCollection referenceValues, IDictionary source,
                               IDictionary destination, IDictionary<string, Exception> validationErrors) {
            return MergeDictionaries(dataObjectType, referenceValues, source, destination, null, validationErrors);
        }

        public static bool MergeDictionaries(object dataObjectType, ParameterCollection reference, IDictionary source,
                                              IDictionary destination, IDictionary destinationCopy, IDictionary<string, Exception> validationErrors) {            
            if (source != null) {
                foreach (DictionaryEntry de in source) {
                    object value = de.Value;
                    // search for a parameter that corresponds to this dictionary entry.
                    Parameter referenceParameter = null;
                    string parameterName = (string)de.Key;
                    foreach (Parameter p in reference) {
                        if (String.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase)) {
                            referenceParameter = p;
                            break;
                        }
                    }
                    // use the parameter for type conversion, default value and/or converting empty string to null.
                    if (referenceParameter != null) {
                        try {
                            value = referenceParameter.GetValue(value, true);
                        }
                        catch (Exception e) {
                            // catch conversion exceptions so they can be handled. Note that conversion throws various
                            // types of exceptions like InvalidCastException, FormatException, OverflowException, etc.
                            validationErrors[referenceParameter.Name] = e;
                        }
                    }
                    // save the value to the merged dictionaries.
                    destination[parameterName] = value;
                    if (destinationCopy != null) {
                        destinationCopy[parameterName] = value;
                    }
                }
            }
            return validationErrors.Count == 0;
        }

        public static Type GetType(string typeName) {
            return BuildManager.GetType(typeName, true /* throwOnError */, true /* ignoreCase */);
        }

        private static object ConvertType(object value, Type type, string paramName) {
            // NOTE: This method came from ObjectDataSource with no changes made.
            string s = value as string;
            if (s != null) {
                // Get the type converter for the destination type
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter != null) {
                    // Perform the conversion
                    try {
                        value = converter.ConvertFromString(s);
                    }
                    catch (NotSupportedException) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_CannotConvertType, paramName, typeof(string).FullName,
                            type.FullName));
                    }
                    catch (FormatException) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_CannotConvertType, paramName, typeof(string).FullName,
                            type.FullName));
                    }
                }
            }
            return value;
        }

        public static object BuildDataObject(Type dataObjectType, IDictionary inputParameters, IDictionary<string, Exception> validationErrors) {
            object dataObject = CreateObjectInstance(dataObjectType);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(dataObject);
            foreach (DictionaryEntry de in inputParameters) {
                string propName = (de.Key == null ? String.Empty : de.Key.ToString());
                PropertyDescriptor property = props.Find(propName, /*ignoreCase*/true);
                // NOTE: No longer throws when a property is not found or is read only.  This makes
                // Delete, Insert and Update operations more optimistic, allowing scenarios such as:
                // 1) Deletes and Updates after projecting data in the Selecting event.
                // 2) Deletes and Updates after selecting children of the data object type in the
                //    Selecting event.
                if ((property != null) && (!property.IsReadOnly)) {
                    try {
                        object value = BuildObjectValue(de.Value, property.PropertyType, propName);
                        property.SetValue(dataObject, value);
                    }
                    catch (Exception e) {
                        validationErrors[property.Name] = e;
                    }
                }
            }

            if (validationErrors.Any()) {
                return null;
            }

            return dataObject;
        }

        internal static object BuildObjectValue(object value, Type destinationType, string paramName) {
            // NOTE: This method came from ObjectDataSource with no changes made.
            // Only consider converting the type if the value is non-null and the types don't match
            if ((value != null) && (!destinationType.IsInstanceOfType(value))) {
                Type innerDestinationType = destinationType;
                bool isNullable = false;
                if (destinationType.IsGenericType &&
                    (destinationType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    innerDestinationType = destinationType.GetGenericArguments()[0];
                    isNullable = true;
                }
                else {
                    if (destinationType.IsByRef) {
                        innerDestinationType = destinationType.GetElementType();
                    }
                }

                // Try to convert from for example string to DateTime, so that
                // afterwards we can convert DateTime to Nullable<DateTime>

                // If the value is a string, we attempt to use a TypeConverter to convert it
                value = ConvertType(value, innerDestinationType, paramName);

                // Special-case the value when the destination is Nullable<T>
                if (isNullable) {
                    Type paramValueType = value.GetType();
                    if (innerDestinationType != paramValueType) {
                        // Throw if for example, we are trying to convert from int to Nullable<bool>
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_CannotConvertType, paramName, paramValueType.FullName,
                            String.Format(CultureInfo.InvariantCulture, "Nullable<{0}>",
                            destinationType.GetGenericArguments()[0].FullName)));
                    }
                }
            }
            return value;
        }
    }
}
