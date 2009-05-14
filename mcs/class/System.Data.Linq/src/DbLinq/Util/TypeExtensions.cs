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
using System.Data.Linq.Mapping;
using System.Reflection;

namespace DbLinq.Util
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Determines if a given type can have a null value
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool CanBeNull(this Type t)
        {
            return IsNullable(t) || !t.IsValueType;
        }

        /// <summary>
        /// Returns a unique MemberInfo
        /// </summary>
        /// <param name="t">The declaring type</param>
        /// <param name="name">The member name</param>
        /// <returns>A MemberInfo or null</returns>
        public static MemberInfo GetSingleMember(this Type t, string name)
        {
            return GetSingleMember(t, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Returns a unique MemberInfo
        /// </summary>
        /// <param name="t">The declaring type</param>
        /// <param name="name">The member name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>A MemberInfo or null</returns>
        public static MemberInfo GetSingleMember(this Type t, string name, BindingFlags bindingFlags)
        {
            var members = t.GetMember(name, bindingFlags);
            if (members.Length > 0)
                return members[0];
            return null;
        }

        /// <summary>
        /// Determines if a Type is specified as nullable
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsNullable(this Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// If the type is nullable, returns the underlying type
        /// Undefined behavior otherwise (it's user responsibility to check for Nullable first)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Type GetNullableType(this Type t)
        {
            return Nullable.GetUnderlyingType(t);
        }

        /// <summary>
        /// Returns default value for provided type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object GetDefault(this Type t)
        {
            return TypeConvert.GetDefault(t);
        }

        /// <summary>
        /// Returns type name without generic specification
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetShortName(this Type t)
        {
            var name = t.Name;
            if (t.IsGenericTypeDefinition)
                return name.Split('`')[0];
            return name;
        }
    }
}
