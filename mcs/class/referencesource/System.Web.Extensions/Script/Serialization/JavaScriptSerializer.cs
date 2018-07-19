//------------------------------------------------------------------------------
// <copyright file="JavaScriptSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Serialization {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.Resources;

    public class JavaScriptSerializer {
        internal const string ServerTypeFieldName = "__type";
        internal const int DefaultRecursionLimit = 100;
        internal const int DefaultMaxJsonLength = 2097152;

        internal static string SerializeInternal(object o) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(o);
        }

        internal static object Deserialize(JavaScriptSerializer serializer, string input, Type type, int depthLimit) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }
            if (input.Length > serializer.MaxJsonLength) {
                throw new ArgumentException(AtlasWeb.JSON_MaxJsonLengthExceeded, "input");
            }

            object o = JavaScriptObjectDeserializer.BasicDeserialize(input, depthLimit, serializer);
            return ObjectConverter.ConvertObjectToType(o, type, serializer);
        }

        // INSTANCE fields/methods
        private JavaScriptTypeResolver _typeResolver;
        private int _recursionLimit;
        private int _maxJsonLength;

        public JavaScriptSerializer() : this(null) { }

        public JavaScriptSerializer(JavaScriptTypeResolver resolver) {
            _typeResolver = resolver;
            RecursionLimit = DefaultRecursionLimit;
            MaxJsonLength = DefaultMaxJsonLength;
        }

        public int MaxJsonLength {
            get {
                return _maxJsonLength;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException(AtlasWeb.JSON_InvalidMaxJsonLength);
                }
                _maxJsonLength = value;
            }
        }

        public int RecursionLimit {
            get {
                return _recursionLimit;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException(AtlasWeb.JSON_InvalidRecursionLimit);
                }
                _recursionLimit = value;
            }
        }

        internal JavaScriptTypeResolver TypeResolver {
            get {
                return _typeResolver;
            }
        }

        private Dictionary<Type, JavaScriptConverter> _converters;
        private Dictionary<Type, JavaScriptConverter> Converters {
            get {
                if (_converters == null) {
                    _converters = new Dictionary<Type, JavaScriptConverter>();
                }
                return _converters;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "Converters", Justification = "This is for managed types which need to have custom type converters for JSon serialization, I don't think there will be any com interop types for this scenario.")]
        public void RegisterConverters(IEnumerable<JavaScriptConverter> converters)
        {
            if (converters == null) {
                throw new ArgumentNullException("converters");
            }

            foreach (JavaScriptConverter converter in converters) {
                IEnumerable<Type> supportedTypes = converter.SupportedTypes;
                if (supportedTypes != null) {
                    foreach (Type supportedType in supportedTypes) {
                        Converters[supportedType] = converter;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "_converters", Justification = "This is for managed types which need to have custom type converters for JSon serialization, I don't think there will be any com interop types for this scenario.")]
        private JavaScriptConverter GetConverter(Type t)
        {
            if (_converters != null) {
                while (t != null) {
                    if (_converters.ContainsKey(t)) {
                        return _converters[t];
                    }
                    t = t.BaseType;
                }
            }
            return null;
        }

        internal bool ConverterExistsForType(Type t, out JavaScriptConverter converter) {
            converter = GetConverter(t);
            return converter != null;
        }

        public object DeserializeObject(string input) {
            return Deserialize(this, input, null /*type*/, RecursionLimit);
        }

        [
        SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Generic parameter is preferable to forcing caller to downcast. " +
                "Has has been approved by API review board. " +
                "Dev10 701126: Overload added afterall, to allow runtime determination of the type.")
        ]
        public T Deserialize<T>(string input) {
            return (T)Deserialize(this, input, typeof(T), RecursionLimit);
        }

        public object Deserialize(string input, Type targetType) {
            return Deserialize(this, input, targetType, RecursionLimit);
        }

        [
        SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Generic parameter is preferable to forcing caller to downcast. " +
                "Has has been approved by API review board. " + 
                "Dev10 701126: Overload added afterall, to allow runtime determination of the type."),
        SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
            Justification = "Cannot change parameter name as would break binary compatibility with legacy apps.")
        ]
        public T ConvertToType<T>(object obj) {
            return (T)ObjectConverter.ConvertObjectToType(obj, typeof(T), this);
        }

        [
        SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
            Justification = "Consistent with previously existing overload which cannot be changed.")
        ]
        public object ConvertToType(object obj, Type targetType) {
            return ObjectConverter.ConvertObjectToType(obj, targetType, this);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
            Justification = "Cannot change parameter name as would break binary compatibility with legacy apps.")]
        public string Serialize(object obj) {
            return Serialize(obj, SerializationFormat.JSON);
        }

        internal string Serialize(object obj, SerializationFormat serializationFormat) {
            StringBuilder sb = new StringBuilder();
            Serialize(obj, sb, serializationFormat);
            return sb.ToString();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
            Justification = "Cannot change parameter name as would break binary compatibility with legacy apps.")]
        public void Serialize(object obj, StringBuilder output) {
            Serialize(obj, output, SerializationFormat.JSON);
        }

        internal void Serialize(object obj, StringBuilder output, SerializationFormat serializationFormat) {
            SerializeValue(obj, output, 0, null, serializationFormat);
            // DevDiv Bugs 96574: Max JSON length does not apply when serializing to Javascript for ScriptDescriptors
            if (serializationFormat == SerializationFormat.JSON && output.Length > MaxJsonLength) {
                throw new InvalidOperationException(AtlasWeb.JSON_MaxJsonLengthExceeded);
            }
        }

        private static void SerializeBoolean(bool o, StringBuilder sb) {
            if (o) {
                sb.Append("true");
            }
            else {
                sb.Append("false");
            }
        }

        private static void SerializeUri(Uri uri, StringBuilder sb) {
            sb.Append("\"").Append(uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped)).Append("\"");
        }

        private static void SerializeGuid(Guid guid, StringBuilder sb) {
            sb.Append("\"").Append(guid.ToString()).Append("\"");
        }

        internal static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        private static void SerializeDateTime(DateTime datetime, StringBuilder sb, SerializationFormat serializationFormat) {
            Debug.Assert(serializationFormat == SerializationFormat.JSON || serializationFormat == SerializationFormat.JavaScript);
            if (serializationFormat == SerializationFormat.JSON) {
                // DevDiv 41127: Never confuse atlas serialized strings with dates
                // Serialized date: "\/Date(123)\/"
                sb.Append(@"""\/Date(");
                sb.Append((datetime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
                sb.Append(@")\/""");
            }
            else {
                // DevDiv 96574: Need to be able to serialize to javascript dates for script descriptors 
                // new Date(ticks)
                sb.Append("new Date(");
                sb.Append((datetime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
                sb.Append(@")");
            }
        }

        // Serialize custom object graph
        private void SerializeCustomObject(object o, StringBuilder sb, int depth, Hashtable objectsInUse, SerializationFormat serializationFormat) {
            bool first = true;
            Type type = o.GetType();
            sb.Append('{');

            // Serialize the object type if we have a type resolver
            if (TypeResolver != null) {

                // Only do this if the context is actually aware of this type
                string typeString = TypeResolver.ResolveTypeId(type);
                if (typeString != null) {
                    SerializeString(ServerTypeFieldName, sb);
                    sb.Append(':');
                    SerializeValue(typeString, sb, depth, objectsInUse, serializationFormat);
                    first = false;
                }
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fieldInfo in fields) {

                // Ignore all fields marked as [ScriptIgnore]
                if (CheckScriptIgnoreAttribute(fieldInfo)) {
                    continue;
                }

                if (!first) sb.Append(',');
                SerializeString(fieldInfo.Name, sb);
                sb.Append(':');
                SerializeValue(SecurityUtils.FieldInfoGetValue(fieldInfo, o), sb, depth, objectsInUse, serializationFormat, currentMember: fieldInfo);
                first = false;
            }

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            foreach (PropertyInfo propInfo in props) {

                // Ignore all properties marked as [ScriptIgnore]
                if (CheckScriptIgnoreAttribute(propInfo))
                    continue;

                MethodInfo getMethodInfo = propInfo.GetGetMethod();

                // Skip property if it has no get
                if (getMethodInfo == null) {
                    continue;
                }

                // Ignore indexed properties
                if (getMethodInfo.GetParameters().Length > 0) continue;

                if (!first) sb.Append(',');
                SerializeString(propInfo.Name, sb);
                sb.Append(':');
                SerializeValue(SecurityUtils.MethodInfoInvoke(getMethodInfo, o, null), sb, depth, objectsInUse, serializationFormat, currentMember: propInfo);
                first = false;
            }

            sb.Append('}');
        }

        private bool CheckScriptIgnoreAttribute(MemberInfo memberInfo) {
            // Ignore all members marked as [ScriptIgnore]
            if (memberInfo.IsDefined(typeof(ScriptIgnoreAttribute), true /*inherits*/)) return true;

            //The above API does not consider the inherit boolean and the right API for that is Attribute.IsDefined.
            //However, to keep the backward compatibility with 4.0, we decided to define an opt-in mechanism (ApplyToOverrides property on ScriptIgnoreAttribute)
            //to consider the inherit boolean.
            ScriptIgnoreAttribute scriptIgnoreAttr = (ScriptIgnoreAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(ScriptIgnoreAttribute), inherit: true);
            if (scriptIgnoreAttr != null && scriptIgnoreAttr.ApplyToOverrides) {
                return true;
            }

            return false;
        }

        private void SerializeDictionary(IDictionary o, StringBuilder sb, int depth, Hashtable objectsInUse, SerializationFormat serializationFormat) {
            sb.Append('{');
            bool isFirstElement = true;
            bool isTypeEntrySet = false;

            //make sure __type field is the first to be serialized if it exists
            if (o.Contains(ServerTypeFieldName)) {
                isFirstElement = false;
                isTypeEntrySet = true;
                SerializeDictionaryKeyValue(ServerTypeFieldName, o[ServerTypeFieldName], sb, depth, objectsInUse, serializationFormat);
            }

            foreach (DictionaryEntry entry in (IDictionary)o) {
                
                string key = entry.Key as string;
                if (key == null) {
                   throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DictionaryTypeNotSupported, o.GetType().FullName));
                }
                if (isTypeEntrySet && String.Equals(key, ServerTypeFieldName, StringComparison.Ordinal)) {
                    // The dictionay only contains max one entry for __type key, and it has been iterated
                    // through, so don't need to check for is anymore.
                    isTypeEntrySet = false;
                    continue;
                }
                if (!isFirstElement) {
                    sb.Append(',');
                }
                SerializeDictionaryKeyValue(key, entry.Value, sb, depth, objectsInUse, serializationFormat);
                isFirstElement = false;
            }
            sb.Append('}');
        }

        private void SerializeDictionaryKeyValue(string key, object value, StringBuilder sb, int depth, Hashtable objectsInUse, SerializationFormat serializationFormat) {
            
            SerializeString(key, sb);
            sb.Append(':');
            SerializeValue(value, sb, depth, objectsInUse, serializationFormat);
        }

        private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb, int depth, Hashtable objectsInUse, SerializationFormat serializationFormat) {
            sb.Append('[');
            bool isFirstElement = true;
            foreach (object o in enumerable) {
                if (!isFirstElement) {
                    sb.Append(',');
                }

                SerializeValue(o, sb, depth, objectsInUse, serializationFormat);
                isFirstElement = false;
            }
            sb.Append(']');
        }

        private static void SerializeString(string input, StringBuilder sb) {
            sb.Append('"');
            sb.Append(HttpUtility.JavaScriptStringEncode(input));
            sb.Append('"');
        }

        private void SerializeValue(object o, StringBuilder sb, int depth, Hashtable objectsInUse, SerializationFormat serializationFormat, MemberInfo currentMember = null) {
            if (++depth > _recursionLimit) {
                throw new ArgumentException(AtlasWeb.JSON_DepthLimitExceeded);
            }

            // Check whether a custom converter is available for this type.
            JavaScriptConverter converter = null;
            if (o != null && ConverterExistsForType(o.GetType(), out converter)) {
                IDictionary<string, object> dict = converter.Serialize(o, this);

                if (TypeResolver != null) {
                    string typeString = TypeResolver.ResolveTypeId(o.GetType());
                    if (typeString != null) {
                        dict[ServerTypeFieldName] = typeString;
                    }
                }

                sb.Append(Serialize(dict, serializationFormat));
                return;
            }

            SerializeValueInternal(o, sb, depth, objectsInUse, serializationFormat, currentMember);
        }

        // We use this for our cycle detection for the case where objects override equals/gethashcode
        private class ReferenceComparer : IEqualityComparer {
            bool IEqualityComparer.Equals(object x, object y) {
                return x == y;
            }

            int IEqualityComparer.GetHashCode(object obj) {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }

        private void SerializeValueInternal(object o, StringBuilder sb, int depth, Hashtable objectsInUse, SerializationFormat serializationFormat, MemberInfo currentMember) {
            // 'null' is a special JavaScript token
            if (o == null || DBNull.Value.Equals(o)) {
                sb.Append("null");
                return;
            }

            // Strings and chars are represented as quoted (single or double) in Javascript.
            string os = o as String;
            if (os != null) {
                SerializeString(os, sb);
                return;
            }

            if (o is Char) {
                // Special case the null char as we don't want it to turn into a null string
                if ((char)o == '\0') {
                    sb.Append("null");
                    return;
                }
                SerializeString(o.ToString(), sb);
                return;
            }

            // Bools are represented as 'true' and 'false' (no quotes) in Javascript.
            if (o is bool) {
                SerializeBoolean((bool)o, sb);
                return;
            }

            if (o is DateTime) {
                SerializeDateTime((DateTime)o, sb, serializationFormat);
                return;
            }

            if (o is DateTimeOffset) {
                // DateTimeOffset is converted to a UTC DateTime and serialized as usual.
                SerializeDateTime(((DateTimeOffset)o).UtcDateTime, sb, serializationFormat);
                return;
            }

            if (o is Guid) {
                SerializeGuid((Guid)o, sb);
                return;
            }

            Uri uri = o as Uri;
            if (uri != null) {
                SerializeUri(uri, sb);
                return;
            }

            // Have to special case floats to get full precision
            if (o is double) {
                sb.Append(((double)o).ToString("r", CultureInfo.InvariantCulture));
                return;
            }

            if (o is float) {
                sb.Append(((float)o).ToString("r", CultureInfo.InvariantCulture));
                return;
            }

            // Deal with any server type that can be represented as a number in JavaScript
            if (o.GetType().IsPrimitive || o is Decimal) {
                IConvertible convertible = o as IConvertible;
                if (convertible != null) {
                    sb.Append(convertible.ToString(CultureInfo.InvariantCulture));
                }
                else {
                    // In theory, all primitive types implement IConvertible
                    Debug.Assert(false);
                    sb.Append(o.ToString());
                }
                return;
            }

            // Serialize enums as their integer value
            Type type = o.GetType();
            if (type.IsEnum) {
                // Int64 and UInt64 result in numbers too big for JavaScript
                Type underlyingType = Enum.GetUnderlyingType(type);
                if ((underlyingType == typeof(Int64)) || (underlyingType == typeof(UInt64))) {
                    // DevDiv #286382 - Try to provide a better error message by saying exactly what property failed.
                    string errorMessage = (currentMember != null)
                        ? String.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_CannotSerializeMemberGeneric, currentMember.Name, currentMember.ReflectedType.FullName) + " " + AtlasWeb.JSON_InvalidEnumType
                        : AtlasWeb.JSON_InvalidEnumType;
                    throw new InvalidOperationException(errorMessage);
                }
                // DevDiv Bugs 154763: call ToString("D") rather than cast to int
                // to support enums that are based on other integral types
                sb.Append(((Enum)o).ToString("D"));
                return;
            }

            try {
                // The following logic performs circular reference detection
                if (objectsInUse == null) {
                    // Create the table on demand
                    objectsInUse = new Hashtable(new ReferenceComparer());
                }
                else if (objectsInUse.ContainsKey(o)) {
                    // If the object is already there, we have a circular reference!
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_CircularReference, type.FullName));
                }
                // Add the object to the objectsInUse
                objectsInUse.Add(o, null);

                // Dictionaries are represented as Javascript objects.  e.g. { name1: val1, name2: val2 }
                IDictionary od = o as IDictionary;
                if (od != null) {
                    SerializeDictionary(od, sb, depth, objectsInUse, serializationFormat);
                    return;
                }

                // Enumerations are represented as Javascript arrays.  e.g. [ val1, val2 ]
                IEnumerable oenum = o as IEnumerable;
                if (oenum != null) {
                    SerializeEnumerable(oenum, sb, depth, objectsInUse, serializationFormat);
                    return;
                }

                // Serialize all public fields and properties.
                SerializeCustomObject(o, sb, depth, objectsInUse, serializationFormat);
            }
            finally {
                // Remove the object from the circular reference detection table
                if (objectsInUse != null) {
                    objectsInUse.Remove(o);
                }
            }
        }

        internal enum SerializationFormat {
            JSON,
            JavaScript
        }
    }
}
