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
    internal static class ReflectionExtensions
    {
        private static A GetSingleAttribute<A>(object[] attributes)
            where A : Attribute
        {
            if (attributes.Length > 0)
                return (A)attributes[0];
            return null;
        }

        /// <summary>
        /// Returns a requested attribute for a given assembly
        /// </summary>
        /// <typeparam name="A">The requested attribute type</typeparam>
        /// <param name="a">The assembly supposed to provide that attribute</param>
        /// <returns>An attribute of type A or null if none</returns>
        public static A GetAttribute<A>(this Assembly a)
            where A : Attribute
        {
            return GetSingleAttribute<A>(a.GetCustomAttributes(typeof(A), true));
        }

        /// <summary>
        /// Returns a requested attribute for a given type
        /// </summary>
        /// <typeparam name="A">The requested attribute type</typeparam>
        /// <param name="t">The class supposed to provide that attribute</param>
        /// <returns>An attribute of type A or null if none</returns>
        public static A GetAttribute<A>(this Type t)
            where A: Attribute
        {
            return GetSingleAttribute<A>(t.GetCustomAttributes(typeof(A), true));
        }

        /// <summary>
        /// Returns a requested attribute for a given member
        /// </summary>
        /// <typeparam name="A">The requested attribute type</typeparam>
        /// <param name="m">The member supposed to provide that attribute</param>
        /// <returns>An attribute of type A or null if none</returns>
        public static A GetAttribute<A>(this MemberInfo m)
            where A : Attribute
        {
            return GetSingleAttribute<A>(m.GetCustomAttributes(typeof(A), true));
        }

    }
}
