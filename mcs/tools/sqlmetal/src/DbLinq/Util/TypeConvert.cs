#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Reflection;

namespace DbLinq.Util
{
    /// <summary>
    /// Types conversion.
    /// A "smart" extension to System.Convert (at least that's what we hope)
    /// </summary>
#if !MONO_STRICT
    public
#endif
    static class TypeConvert
    {
        public static object ToNumber(object o, Type numberType)
        {
            if (o.GetType() == numberType)
                return o;
            string methodName = string.Format("To{0}", numberType.Name);
            MethodInfo convertMethod = typeof(Convert).GetMethod(methodName, new[] { o.GetType() });
            if (convertMethod != null)
                return convertMethod.Invoke(null, new[] { o });
            throw new InvalidCastException(string.Format("Can't convert type {0} in Convert.{1}()", o.GetType().Name, methodName));
        }

        public static U ToNumber<U>(object o)
        {
            return (U)ToNumber(o, typeof(U));
        }

        /// <summary>
        /// Returns the default value for a specified type.
        /// Reflection equivalent of default(T)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object GetDefault(Type t)
        {
            if (!t.IsValueType)
                return null;
            return Activator.CreateInstance(t);
        }

        /// <summary>
        /// Converts a value to an enum
        /// (work with literals string values and numbers)
        /// </summary>
        /// <param name="o">The literal to convert</param>
        /// <param name="enumType">The target enum type</param>
        /// <returns></returns>
        public static int ToEnum(object o, Type enumType)
        {
            var e = (int)Enum.Parse(enumType, o.ToString());
            return e;
        }

        public static E ToEnum<E>(object o)
        {
            return (E)(object)ToEnum(o, typeof(E));
        }

        public static bool ToBoolean(object o)
        {
            if (o is bool)
                return (bool)o;
            // if it is a string, we may have "T"/"F" or "True"/"False"
            if (o is string)
            {
                // regular literals
                var lb = (string)o;
                bool ob;
                if (bool.TryParse(lb, out ob))
                    return ob;
                // alternative literals
                if (lb == "T" || lb == "F")
                    return lb == "T";
                if (lb == "Y" || lb == "N")
                    return lb == "Y";
            }
            return ToNumber<int>(o) != 0;
        }

        public static string ToString(object o)
        {
            if (o == null)
                return null;
            return o.ToString();
        }

        public static char ToChar(object c)
        {
            if (c is char)
                return (char)c;
            if (c is string)
            {
                var sc = (string)c;
                if (sc.Length == 1)
                    return sc[0];
            }
            if (c == null)
                return '\0';
            throw new InvalidCastException(string.Format("Can't convert type {0} in GetAsChar()", c.GetType().Name));
        }

        public static Guid ToGuid(object o)
        {
            if (o is Guid)
                return (Guid)o;
            return new Guid(ToString(o));
        }

        public static object To(object o, Type targetType)
        {
            if (targetType.IsNullable())
            {
                if (o == null)
                    return null;
                return Activator.CreateInstance(targetType, To(o, targetType.GetNullableType()));
            }
            if (targetType == typeof(string))
                return ToString(o);
            if (targetType == typeof(bool))
                return ToBoolean(o);
            if (targetType == typeof(char))
                return ToChar(o);
            if (targetType == typeof(byte))
                return ToNumber<byte>(o);
            if (targetType == typeof(sbyte))
                return ToNumber<sbyte>(o);
            if (targetType == typeof(short))
                return ToNumber<short>(o);
            if (targetType == typeof(ushort))
                return ToNumber<ushort>(o);
            if (targetType == typeof(int))
                return ToNumber<int>(o);
            if (targetType == typeof(uint))
                return ToNumber<uint>(o);
            if (targetType == typeof(long))
                return ToNumber<long>(o);
            if (targetType == typeof(ulong))
                return ToNumber<ulong>(o);
            if (targetType == typeof(float))
                return ToNumber<float>(o);
            if (targetType == typeof(double))
                return ToNumber<double>(o);
            if (targetType == typeof(decimal))
                return ToNumber<decimal>(o);
            if (targetType == typeof(DateTime))
                return (DateTime)o;
            if (targetType == typeof(Guid))
                return ToGuid(o);
            if (targetType.IsEnum)
                return ToEnum(o, targetType);
            throw new ArgumentException(string.Format("L0117: Unhandled type {0}", targetType));
        }

        public static T To<T>(object o)
        {
            return (T)To(o, typeof(T));
        }
    }
}
