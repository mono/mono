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
//
// System.Web.UI.PropertyConverter.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)

using System.Reflection;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	public static class PropertyConverter {
#else
	public sealed class PropertyConverter {

		PropertyConverter ()
		{
			// no instantiation for you
		}
#endif
		public static object EnumFromString (Type enumType, string value)
		{
			object res = null;

			try {
				res = Enum.Parse (enumType, value, true);
			} catch {
				res = null;
			}
			return res;
		}

		public static string EnumToString (Type enumType, object enumValue)
		{
			return Enum.Format (enumType, enumValue, "G");
		}

		public static object ObjectFromString (Type objType,
				MemberInfo propertyInfo, string value)
		{
			if (objType == typeof (string))
				return value;

			// Is there a less kludgy way to get the converter?
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (
				propertyInfo.ReflectedType);
			PropertyDescriptor pd = col.Find (propertyInfo.Name, false);
			if (pd.Converter == null || !pd.Converter.CanConvertFrom (typeof (string))) {
				throw new HttpException (Locale.GetText ("Cannot create an object " +
				      "of type '{0}' from its string representation '{1}' for the " +
				      "'{2}' property", objType, value, propertyInfo.Name));
			}
			return pd.Converter.ConvertFromInvariantString (value);
		}
	}
}

