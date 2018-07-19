//------------------------------------------------------------------------------
// <copyright file="ObjectConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Web.Resources;

namespace System.Web.Script.Serialization {

    internal static class ObjectConverter {

        private static readonly Type[] s_emptyTypeArray = new Type[] { };
        private static Type _listGenericType = typeof(List<>);
        private static Type _enumerableGenericType = typeof(IEnumerable<>);
        private static Type _dictionaryGenericType = typeof(Dictionary<,>);
        private static Type _idictionaryGenericType = typeof(IDictionary<,>);

        // Helper method that recursively convert individual items in the old array
        private static bool AddItemToList(IList oldList, IList newList, Type elementType, JavaScriptSerializer serializer, bool throwOnError) {
            object convertedObject;
            foreach (Object propertyValue in oldList) {
                if (!ConvertObjectToTypeMain(propertyValue, elementType, serializer, throwOnError, out convertedObject)) {
                    return false;
                }

                newList.Add(convertedObject);
            }

            return true;
        }

        // Helper method that assigns the propertyValue to object o's member (memberName)
        private static bool AssignToPropertyOrField(object propertyValue, object o, string memberName, JavaScriptSerializer serializer, bool throwOnError) {
            IDictionary dictionary = o as IDictionary;
            // if o is already an idictionary, assign the value to the dictionary
            if (dictionary != null) {
                if (!ConvertObjectToTypeMain(propertyValue, null, serializer, throwOnError, out propertyValue)) {
                    return false;
                }
                dictionary[memberName] = propertyValue;
                return true;
            }

            Type serverType = o.GetType();
            // First, look for a property
            PropertyInfo propInfo = serverType.GetProperty(memberName,
                BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

            if (propInfo != null) {
                // Ignore it if the property has no setter
                MethodInfo setter = propInfo.GetSetMethod();
                if (setter != null) {
                    // Deserialize the property value, with knownledge of the property type
                    if (!ConvertObjectToTypeMain(propertyValue, propInfo.PropertyType, serializer, throwOnError, out propertyValue)) {
                        return false;
                    }

                    // Set the property in the object
                    try {
                        setter.Invoke(o, new Object[] { propertyValue });
                        return true;
                    }
                    catch {
                        if (throwOnError) {
                            throw;
                        }
                        else {
                            return false;
                        }
                    }
                }
            }

            // We couldn't find a property, so try a field
            FieldInfo fieldInfo = serverType.GetField(memberName,
                BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

            if (fieldInfo != null) {
                // Deserialize the field value, with knownledge of the field type
                if (!ConvertObjectToTypeMain(propertyValue, fieldInfo.FieldType, serializer, throwOnError, out propertyValue)) {
                    return false;
                }

                // Set the field in the object
                try {
                    fieldInfo.SetValue(o, propertyValue);
                    return true;
                }
                catch {
                    if (throwOnError) {
                        throw;
                    }
                    else {
                        return false;
                    }
                }
            }

            // not a property or field, so it is ignored
            return true;
        }

        // Method that converts an IDictionary<string, object> to an object of the right type
        private static bool ConvertDictionaryToObject(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer, bool throwOnError, out object convertedObject) {

            // The target type to instantiate.
            Type targetType = type;
            object s;
            string serverTypeName = null;
            object o = dictionary;

            // Check if __serverType exists in the dictionary, use it as the type.
            if (dictionary.TryGetValue(JavaScriptSerializer.ServerTypeFieldName, out s)) {

                // Convert the __serverType value to a string.
                if (!ConvertObjectToTypeMain(s, typeof(String), serializer, throwOnError, out s)) {
                    convertedObject = false;
                    return false;
                }

                serverTypeName = (string)s;

                if (serverTypeName != null) {
                    // If we don't have the JavaScriptTypeResolver, we can't use it
                    if (serializer.TypeResolver != null) {
                        // Get the actual type from the resolver.
                        targetType = serializer.TypeResolver.ResolveType(serverTypeName);

                        // In theory, we should always find the type.  If not, it may be some kind of attack.
                        if (targetType == null) {
                            if (throwOnError) {
                                throw new InvalidOperationException();
                            }

                            convertedObject = null;
                            return false;
                        }
                    }

                    // Remove the serverType from the dictionary, even if the resolver was null
                    dictionary.Remove(JavaScriptSerializer.ServerTypeFieldName);
                }
            }

            JavaScriptConverter converter = null;
            if (targetType != null && serializer.ConverterExistsForType(targetType, out converter)) {
                try {
                    convertedObject = converter.Deserialize(dictionary, targetType, serializer);
                    return true;
                }
                catch {
                    if (throwOnError) {
                        throw;
                    }

                    convertedObject = null;
                    return false;
                }
            }

            // Instantiate the type if it's coming from the __serverType argument.
            if (serverTypeName != null || IsClientInstantiatableType(targetType, serializer)) {

                // First instantiate the object based on the type.
                o = Activator.CreateInstance(targetType);
            }

#if INDIGO
			StructuralContract contract = null;
			if (suggestedType != null && 
                suggestedType.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0)
				contract = StructuralContract.Create(suggestedType);
#endif

            // Use a different collection to avoid modifying the original during keys enumeration.
            List<String> memberNames = new List<String>(dictionary.Keys);

            // Try to handle the IDictionary<K, V> case
            if (IsGenericDictionary(type)) {

                Type keyType = type.GetGenericArguments()[0];
                if (keyType != typeof(string) && keyType != typeof(object)) {
                    if (throwOnError) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DictionaryTypeNotSupported, type.FullName));
                    }

                    convertedObject = null;
                    return false;
                }

                Type valueType = type.GetGenericArguments()[1];
                IDictionary dict = null;
                if (IsClientInstantiatableType(type, serializer)) {
                    dict = (IDictionary)Activator.CreateInstance(type);
                }
                else {
                    // Get the strongly typed Dictionary<K, V>
                    Type t = _dictionaryGenericType.MakeGenericType(keyType, valueType);
                    dict = (IDictionary)Activator.CreateInstance(t);
                }

                if (dict != null) {
                    foreach (string memberName in memberNames) {
                        object memberObject;
                        if (!ConvertObjectToTypeMain(dictionary[memberName], valueType, serializer, throwOnError, out memberObject)) {
                            convertedObject = null;
                            return false;
                        }
                        dict[memberName] = memberObject;
                    }

                    convertedObject = dict;
                    return true;
                }
            }

            // Fail if we know we cannot possibly return the required type.
            if (type != null && !type.IsAssignableFrom(o.GetType())) {
                
                if (!throwOnError) {
                    convertedObject = null;
                    return false;
                }

                ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, s_emptyTypeArray, null);
                if (constructorInfo == null) {
                    throw new MissingMethodException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_NoConstructor, type.FullName));
                }

                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DeserializerTypeMismatch, type.FullName));
            }

            foreach (string memberName in memberNames) {
                object propertyValue = dictionary[memberName];
#if INDIGO
	            if (contract != null) {
		            Member member = contract.FindMember(memberName);
		            // 

		            if (member == null)
			            throw new InvalidOperationException();

		            if (member.MemberType == MemberTypes.Field) {
			            member.SetValue(o, propertyValue);
		            }
		            else {
			            member.SetValue(o, propertyValue);
		            }

                    continue;
	            }
#endif
                // Assign the value into a property or field of the object
                if (!AssignToPropertyOrField(propertyValue, o, memberName, serializer, throwOnError)) {
                    convertedObject = null;
                    return false;
                }
            }

            convertedObject = o;
            return true;
        }

        internal static object ConvertObjectToType(object o, Type type, JavaScriptSerializer serializer) {
            object convertedObject;
            ConvertObjectToTypeMain(o, type, serializer, true, out convertedObject);
            return convertedObject;
        }

        private static bool ConvertObjectToTypeMain(object o, Type type, JavaScriptSerializer serializer, bool throwOnError, out object convertedObject) {
            // If it's null, there is nothing to convert
            if (o == null) {
                // need to special case Char, as we convert \0 to null
                if (type == typeof(char)) {
                    convertedObject = '\0';
                    return true;
                }
                // Throw if its a value type and not a nullable
                if (IsNonNullableValueType(type)) {
                    if (throwOnError) {
                        throw new InvalidOperationException(AtlasWeb.JSON_ValueTypeCannotBeNull);
                    }
                    else {
                        convertedObject = null;
                        return false;
                    }
                }

                convertedObject = null;
                return true;
            }

            // simply return the current object if the current type is same as return type.
            if (o.GetType() == type) {
                convertedObject = o;
                return true;
            }

            // otherwise use the converters to convert object into target type.
            return ConvertObjectToTypeInternal(o, type, serializer, throwOnError, out convertedObject);
        }

        // Helper method that converts the object to the corresponding type using converters.
        // Items in IDictionary<string, object> and ArrayList needs to be converted as well.
        // Note this method does not invoke the custom converter for deserialization.
        private static bool ConvertObjectToTypeInternal(object o, Type type, JavaScriptSerializer serializer, bool throwOnError, out object convertedObject) {

            // First checks if the object is an IDictionary<string, object>
            IDictionary<string, object> dictionary = o as IDictionary<string, object>;
            if (dictionary != null) {
                return ConvertDictionaryToObject(dictionary, type, serializer, throwOnError, out convertedObject);
            }

            // If it is an IList try to convert it to the requested type.
            IList list = o as IList;
            if (list != null) {
                IList convertedList;
                if (ConvertListToObject(list, type, serializer, throwOnError, out convertedList)) {
                    convertedObject = convertedList;
                    return true;
                }
                else {
                    convertedObject = null;
                    return false;
                }
            }

            // simply return the current object if
            // 1) the caller does not specify the return type.
            // 2) if the current type is same as return type.
            if (type == null || o.GetType() == type) {
                convertedObject = o;
                return true;
            }

            // Otherwise use the type converter to convert the string to the target type.
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            // Use the memberType's converter to directly conver if supported.
            if (converter.CanConvertFrom(o.GetType())) {
                try {
                    convertedObject = converter.ConvertFrom(null, CultureInfo.InvariantCulture, o);
                    return true;
                }
                catch {
                    if (throwOnError) {
                        throw;
                    }
                    else {
                        convertedObject = null;
                        return false;
                    }
                }
            }

            // Otherwise if the target type can be converted from a string
            // 1. first use the propertyValue's converter to convert object to string,
            // 2. then use the target converter to convert the string to target type.
            if (converter.CanConvertFrom(typeof(String))) {

                try {
                    string s;
                    if (o is DateTime) {
                        // when converting from DateTime it is important to use the 'u' format
                        // so it contains the 'Z' indicating that it is UTC time.
                        // If converting to DateTimeOffset this ensures the value is correct, since otherwise
                        // the deafult offset would be assumed, which is the server's timezone.
                        s = ((DateTime)o).ToUniversalTime().ToString("u", CultureInfo.InvariantCulture);
                    }
                    else {
                        TypeConverter propertyConverter = TypeDescriptor.GetConverter(o);
                        s = propertyConverter.ConvertToInvariantString(o);
                    }
                    convertedObject = converter.ConvertFromInvariantString(s);
                    return true;
                }
                catch {
                    if (throwOnError) {
                        throw;
                    }
                    else {
                        convertedObject = null;
                        return false;
                    }
                }
            }

            // We can't convert object o to the target type, but perhaps o can be
            // assigned directly to type?
            if (type.IsAssignableFrom(o.GetType())) {
                convertedObject = o;
                return true;
            }

            // Nothing works
            if (throwOnError) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_CannotConvertObjectToType, o.GetType(), type));
            }
            else {
                convertedObject = null;
                return false;
            }
        }

        // Method that converts client array to the request type. It handles the following cases:
        // 1. type is not passed in - An ArrayList will be returned.
        // 2. type is an array - An array of the right type will be returned.
        // 3. type is an abstract collection interface, e.g. IEnumerable, ICollection -
        //    An ArrayList will be returned.
        // 4. type is an generic abstract collection interface, e.g. IEnumerable<T> -
        //    An List<T> will be returned.
        // 5. type is a concrete type that implements IList -
        //    The type will be instantiated and returned.
        // Otherwise we throw InvalidOperationException.
        private static bool ConvertListToObject(IList list, Type type, JavaScriptSerializer serializer, bool throwOnError, out IList convertedList) {

            // Add the items into an ArrayList then convert to custom type when
            // 1. Type is null or typeof(Object)
            // 2. Type is an Array, in which case we call ArrayList.ToArray(type) or
            // 3. Type is already an ArrayList
            if (type == null || type == typeof(Object) || IsArrayListCompatible(type)) {
                Type elementType = typeof(Object);
                if (type != null && type != typeof(Object)) {
                    elementType = type.GetElementType();
                }

                ArrayList newList = new ArrayList();

                // Add the items to the new List and recursive into each item.
                if (!AddItemToList(list, newList, elementType, serializer, throwOnError)) {
                    convertedList = null;
                    return false;
                }

                if (type == typeof(ArrayList) || type == typeof(IEnumerable) || type == typeof(IList) || type == typeof(ICollection)) {
                    convertedList = newList;
                    return true;
                }

                convertedList = newList.ToArray(elementType);
                return true;
            }
            // Add the items into an List<T> then convert to the custom generic type when
            // 1. Type is a generic collection type
            // 2. Type only has one generic parameter, eg. List<string> vs MyCustom<T, V>
            // 3. Type implements IEnumerable<T>
            else if (type.IsGenericType &&
                type.GetGenericArguments().Length == 1) {

                // gets the T of List<T> as the elementType
                Type elementType = type.GetGenericArguments()[0];

                // Get the strongly typed IEnumerable<T>
                Type strongTypedEnumerable = _enumerableGenericType.MakeGenericType(elementType);

                // Make sure the custom type can be assigned to IEnumerable<T>
                if (strongTypedEnumerable.IsAssignableFrom(type)) {

                    // Get the strongly typed List<T>
                    Type t = _listGenericType.MakeGenericType(elementType);

                    // Create the List<T> instance or a MyList<T>
                    IList newList = null;
                    if (IsClientInstantiatableType(type, serializer) && typeof(IList).IsAssignableFrom(type)) {
                        newList = (IList)Activator.CreateInstance(type);
                    }
                    else {
                        // If this is MyList<T> and we can't assign to it, throw
                        if (t.IsAssignableFrom(type)) {
                            if (throwOnError) {
                                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_CannotCreateListType, type.FullName));
                            }
                            else {
                                convertedList = null;
                                return false;
                            }
                        }
                        newList = (IList)Activator.CreateInstance(t);
                    }

                    // Add the items to the new List and recursive into each item.
                    if (!AddItemToList(list, newList, elementType, serializer, throwOnError)) {
                        convertedList = null;
                        return false;
                    }

                    convertedList = newList;
                    return true;
                }
            }
            // If the custom type implements IList and it's instantiable. Use that type.
            else if (IsClientInstantiatableType(type, serializer) && typeof(IList).IsAssignableFrom(type)) {
                IList newList = (IList)Activator.CreateInstance(type);

                // Add the items to the new List and recursive into each item.
                if (!AddItemToList(list, newList, null, serializer, throwOnError)) {
                    convertedList = null;
                    return false;
                }

                convertedList = newList;
                return true;
            }

            if (throwOnError) {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture, AtlasWeb.JSON_ArrayTypeNotSupported, type.FullName));
            }
            else {
                convertedList = null;
                return false;
            }
        }

        private static bool IsArrayListCompatible(Type type) {
            return type.IsArray || type == typeof(ArrayList) || type == typeof(IEnumerable) || type == typeof(IList) || type == typeof(ICollection);
        }

        // Is this a type for which we want to instantiate based on the client stub
        internal static bool IsClientInstantiatableType(Type t, JavaScriptSerializer serializer) {
            // Abstract classes and interfaces can't be instantiated
            // 
            if (t == null || t.IsAbstract || t.IsInterface || t.IsArray)
                return false;

            // Even though 'object' is instantiatable, it is never useful to do this
            if (t == typeof(object))
                return false;

            // Return true if a converter is registered for the given type, so the converter
            // can generate code on the client to instantiate it.
            JavaScriptConverter converter = null;
            if (serializer.ConverterExistsForType(t, out converter)) {
                return true;
            }

            // Value types are okay (i.e. structs);
            if (t.IsValueType) {
                return true;
            }

            // Ignore types that don't have a public default ctor
            ConstructorInfo constructorInfo = t.GetConstructor(BindingFlags.Public | BindingFlags.Instance,
                null, s_emptyTypeArray, null);
            if (constructorInfo == null)
                return false;

            return true;
        }

	// these helper methods replace inline code.
	// they simplify the code and reduce our cyclomatic complexity

        private static bool IsGenericDictionary(Type type) {
            return type != null &&
                type.IsGenericType &&
                (typeof(IDictionary).IsAssignableFrom(type) || type.GetGenericTypeDefinition() == _idictionaryGenericType) &&
                type.GetGenericArguments().Length == 2;
        }

        private static bool IsNonNullableValueType(Type type) {
            // the the type a value type, and if it is, is it not the nullable variety
            return type != null && type.IsValueType &&
                !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        internal static bool TryConvertObjectToType(object o, Type type, JavaScriptSerializer serializer, out object convertedObject) {
            return ConvertObjectToTypeMain(o, type, serializer, false, out convertedObject);
        }
    }
}
