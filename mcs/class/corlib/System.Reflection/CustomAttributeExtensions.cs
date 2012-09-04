//
// CustomAttributeExtensions.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_4_5

namespace System.Reflection
{
	public static class CustomAttributeExtensions
	{
		public static T GetCustomAttribute<T> (this Assembly element) where T : Attribute
		{
			return (T) Attribute.GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T> (this MemberInfo element) where T : Attribute
		{
			return (T) Attribute.GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T> (this Module element) where T : Attribute
		{
			return (T) Attribute.GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T> (this ParameterInfo element) where T : Attribute
		{
			return (T) Attribute.GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T> (this MemberInfo element, bool inherit) where T : Attribute
		{
			return (T) Attribute.GetCustomAttribute (element, typeof (T), inherit);
		}

		public static T GetCustomAttribute<T> (this ParameterInfo element, bool inherit) where T : Attribute
		{
			return (T) Attribute.GetCustomAttribute (element, typeof (T), inherit);
		}

		public static Attribute GetCustomAttribute (this Assembly element, Type attributeType)
		{
			return Attribute.GetCustomAttribute (element, attributeType);
		}

		public static Attribute GetCustomAttribute (this MemberInfo element, Type attributeType)
		{
			return Attribute.GetCustomAttribute (element, attributeType);
		}

		public static Attribute GetCustomAttribute (this MemberInfo element, Type attributeType, bool inherit)
		{
			return Attribute.GetCustomAttribute (element, attributeType, inherit);
		}

		public static Attribute GetCustomAttribute (this Module element, Type attributeType)
		{
			return Attribute.GetCustomAttribute (element, attributeType);
		}

		public static Attribute GetCustomAttribute (this ParameterInfo element, Type attributeType)
		{
			return Attribute.GetCustomAttribute (element, attributeType);
		}

		public static Attribute GetCustomAttribute (this ParameterInfo element, Type attributeType, bool inherit)
		{
			return Attribute.GetCustomAttribute (element, attributeType, inherit);
		}

		public static bool IsDefined (this Assembly element, Type attributeType)
		{
			return Attribute.IsDefined (element, attributeType);
		}

		public static bool IsDefined (this MemberInfo element, Type attributeType)
		{
			return Attribute.IsDefined (element, attributeType);
		}

		public static bool IsDefined (this Module element, Type attributeType)
		{
			return Attribute.IsDefined (element, attributeType);
		}

		public static bool IsDefined (this ParameterInfo element, Type attributeType)
		{
			return Attribute.IsDefined (element, attributeType);
		}

		public static bool IsDefined (this MemberInfo element, Type attributeType, bool inherit)
		{
			return Attribute.IsDefined (element, attributeType, inherit);
		}

		public static bool IsDefined (this ParameterInfo element, Type attributeType, bool inherit)
		{
			return Attribute.IsDefined (element, attributeType, inherit);
		}
	}
}

#endif