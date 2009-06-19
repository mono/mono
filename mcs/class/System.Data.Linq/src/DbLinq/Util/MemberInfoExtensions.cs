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
    /// Extensions to handle FieldInfo and PropertyInfo as a single class, their MemberInfo class
    /// </summary>
#if !MONO_STRICT
    public
#endif
    static class MemberInfoExtensions
    {
        /// <summary>
        /// Returns the type of the specified member
        /// </summary>
        /// <param name="memberInfo">member to get type from</param>
        /// <returns>Member type</returns>
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).FieldType;
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).PropertyType;
            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).ReturnType;
            if (memberInfo is ConstructorInfo)
                return null;
            if (memberInfo is Type)
                return (Type)memberInfo;
            throw new ArgumentException();
        }

        public static bool GetIsStaticMember(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).IsStatic;
            if (memberInfo is PropertyInfo)
            {
                MethodInfo propertyMethod;
                PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                if ((propertyMethod = propertyInfo.GetGetMethod()) != null || (propertyMethod = propertyInfo.GetSetMethod()) != null)
                    return GetIsStaticMember(propertyMethod);

            }
            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).IsStatic; ;
            throw new ArgumentException();
        }

        /// <summary>
        /// Gets a field/property
        /// </summary>
        /// <param name="memberInfo">The memberInfo specifying the object</param>
        /// <param name="o">The object</param>
        public static object GetMemberValue(this MemberInfo memberInfo, object o)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).GetValue(o);
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).GetGetMethod().Invoke(o, new object[0]);
            throw new ArgumentException();
        }

        /// <summary>
        /// Sets a field/property
        /// </summary>
        /// <param name="memberInfo">The memberInfo specifying the object</param>
        /// <param name="o">The object</param>
        /// <param name="value">The field/property value to assign</param>
        public static void SetMemberValue(this MemberInfo memberInfo, object o, object value)
        {
            if (memberInfo is FieldInfo)
                ((FieldInfo)memberInfo).SetValue(o, value);
            else if (memberInfo is PropertyInfo)
                ((PropertyInfo)memberInfo).GetSetMethod().Invoke(o, new[] { value });
            else throw new ArgumentException();
        }

        /// <summary>
        /// If memberInfo is a method related to a property, returns the PropertyInfo
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static PropertyInfo GetExposingProperty(this MemberInfo memberInfo)
        {
            var reflectedType = memberInfo.ReflectedType;
            foreach (var propertyInfo in reflectedType.GetProperties())
            {
                if (propertyInfo.GetGetMethod() == memberInfo || propertyInfo.GetSetMethod() == memberInfo)
                    return propertyInfo;
            }
            return null;
        }

		/// <summary>
		/// This function returns the type that is the "return type" of the member.
		/// If it is a template it returns the first template parameter type.
		/// </summary>
		/// <param name="memberInfo">The member info.</param>
		/// TODO: better function name
		public static Type GetFirstInnerReturnType(this MemberInfo memberInfo)
		{
			var type = memberInfo.GetMemberType();

			if (type == null)
				return null;

			if (type.IsGenericType)
			{
				return type.GetGenericArguments()[0];
			}

			return type;
		}
    }
}
