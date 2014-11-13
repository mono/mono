using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.IO;
using System.Reflection;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq {

    public static class DBConvert {
        private static Type[] StringArg = new Type[] { typeof(string) };

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "[....]: Generic parameters are required for strong-typing of the return type.")]
        public static T ChangeType<T>(object value) {
            return (T)ChangeType(value, typeof(T));
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "[....]: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        public static object ChangeType(object value, Type type) {
            if (value == null)
                return null;
            MethodInfo mi;
            Type toType = System.Data.Linq.SqlClient.TypeSystem.GetNonNullableType(type);
            Type fromType = value.GetType();
            if (toType.IsAssignableFrom(fromType))
                return value;

            if (toType == typeof(Binary)) {
                if (fromType == typeof(byte[])) {
                    return new Binary((byte[])value);
                }
                else if (fromType == typeof(Guid)) {
                    return new Binary(((Guid)value).ToByteArray());
                }
                else {
                    BinaryFormatter formatter = new BinaryFormatter();
                    byte[] streamArray;
                    using (MemoryStream stream = new MemoryStream()) {
                        formatter.Serialize(stream, value);
                        streamArray = stream.ToArray();
                    }
                    return new Binary(streamArray);
                }
            }
            else if (toType == typeof(byte[])) {
                if (fromType == typeof(Binary)) {
                    return ((Binary)value).ToArray();
                }
                else if (fromType == typeof(Guid)) {
                    return ((Guid)value).ToByteArray();
                }
                else {
                    BinaryFormatter formatter = new BinaryFormatter();
                    byte[] returnValue;
                    using (MemoryStream stream = new MemoryStream()) {
                        formatter.Serialize(stream, value);
                        returnValue = stream.ToArray();
                    }
                    return returnValue;
                }
            }
            else if (fromType == typeof(byte[])) {
                if (toType == typeof(Guid)) {
                    return new Guid((byte[])value);
                }
                else {
                    BinaryFormatter formatter = new BinaryFormatter();
                    object returnValue;
                    using (MemoryStream stream = new MemoryStream((byte[])value)) {
                        returnValue = ChangeType(formatter.Deserialize(stream), toType);
                    }
                    return returnValue; 
                }
            }
            else if (fromType == typeof(Binary)) {
                if (toType == typeof(Guid)) {
                    return new Guid(((Binary)value).ToArray());
                }
                else {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (MemoryStream stream = new MemoryStream(((Binary)value).ToArray(), false)) {
                        return ChangeType(formatter.Deserialize(stream), toType);
                    }
                }
            }
            else if (toType.IsEnum) {
                if (fromType == typeof(string)) {
                    string text = ((string)value).Trim();
                    return Enum.Parse(toType, text);
                }
                else {
                    return Enum.ToObject(toType, Convert.ChangeType(value, Enum.GetUnderlyingType(toType), Globalization.CultureInfo.InvariantCulture));
                }
            }
            else if (fromType.IsEnum) {
                if (toType == typeof(string)) {
                    return Enum.GetName(fromType, value);
                }
                else {
                    return Convert.ChangeType(Convert.ChangeType(value, 
                                                                Enum.GetUnderlyingType(fromType), 
                                                                Globalization.CultureInfo.InvariantCulture), 
                                              toType,
                                              Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (toType == typeof(TimeSpan)) {
                if (fromType == typeof(string)) {
                    return TimeSpan.Parse(value.ToString(), Globalization.CultureInfo.InvariantCulture);
                }
                else if (fromType == typeof(DateTime)) {
                    return DateTime.Parse(value.ToString(), Globalization.CultureInfo.InvariantCulture).TimeOfDay;
                }
                else if (fromType == typeof(DateTimeOffset)) {
                    return DateTimeOffset.Parse(value.ToString(), Globalization.CultureInfo.InvariantCulture).TimeOfDay;
                }
                else {
                    return new TimeSpan((long)Convert.ChangeType(value, typeof(long), Globalization.CultureInfo.InvariantCulture));
                }
            }
            else if (fromType == typeof(TimeSpan)) {
                if (toType == typeof(string)) {
                    return ((TimeSpan)value).ToString("", Globalization.CultureInfo.InvariantCulture);
                }
                else if (toType == typeof(DateTime)) {
                    DateTime dt = new DateTime();
                    return dt.Add((TimeSpan)value);
                }
                else if (toType == typeof(DateTimeOffset)) {
                    DateTimeOffset dto = new DateTimeOffset();
                    return dto.Add((TimeSpan)value);
                }
                else {
                    return Convert.ChangeType(((TimeSpan)value).Ticks, toType, Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (toType == typeof(DateTime) && fromType == typeof(DateTimeOffset)) {
                return ((DateTimeOffset)value).DateTime;
            }
            else if (toType == typeof(DateTimeOffset) && fromType == typeof(DateTime)) {
                return new DateTimeOffset((DateTime)value);
            }
            else if (toType == typeof(string) && !(typeof(IConvertible).IsAssignableFrom(fromType))) {
                if (fromType == typeof(char[])) {
                    return new String((char[])value);
                }
                else {
                    return value.ToString();
                }
            }
            else if (fromType == typeof(string)) {
                if (toType == typeof(Guid)) {
                    return new Guid((string)value);
                }
                else if (toType == typeof(char[])) {
                    return ((String)value).ToCharArray();
                }
                else if (toType == typeof(System.Xml.Linq.XDocument) && (string)value == string.Empty) {
                    return new System.Xml.Linq.XDocument();
                }
                else if (!(typeof(IConvertible).IsAssignableFrom(toType)) &&
                    (mi = toType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, StringArg, null)) != null) {
                    try {
                        return SecurityUtils.MethodInfoInvoke(mi, null, new object[] { value });
                    }
                    catch (TargetInvocationException t) {
                        throw t.GetBaseException();
                    }
                }
                else {
                    return Convert.ChangeType(value, toType, Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(IQueryable<>)
                && typeof(IEnumerable<>).MakeGenericType(toType.GetGenericArguments()[0]).IsAssignableFrom(fromType)
                ) {
                return Queryable.AsQueryable((IEnumerable)value);
            }
            else {
                try {
                    return Convert.ChangeType(value, toType, Globalization.CultureInfo.InvariantCulture);
                } catch (InvalidCastException) {
                    throw Error.CouldNotConvert(fromType, toType);
                }
            }
        }
    }
}
