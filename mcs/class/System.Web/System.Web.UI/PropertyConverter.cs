
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
/**
 * Namespace: System.Web.UI
 * Class:     PropertyConverter
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Web.UI
{
	public sealed class PropertyConverter
	{
		private static Type[] parseMethodTypes;
		private static Type[] parseMethodTypesWithSOP;

		static PropertyConverter()
		{
			parseMethodTypes = new Type[1];
			parseMethodTypes[0] = typeof(string);
			parseMethodTypesWithSOP = new Type[2];
			parseMethodTypesWithSOP[0] = typeof(string);
			parseMethodTypesWithSOP[1] = typeof(IServiceProvider);
		}

		private PropertyConverter()
		{
			// Prevent any instance
		}

		public static object EnumFromString(Type enumType, string enumValue)
		{
			object retVal = null;
			try
			{
				retVal = Enum.Parse(enumType, enumValue, true);
			} catch
			{
				retVal = null;
			}
			return retVal;
		}

		public static string EnumToString(Type enumType, object enumValue)
		{
			string retVal = Enum.Format(enumType, enumValue, "G");
			return retVal.Replace('_','-');
		}

		public static object ObjectFromString(Type objType, MemberInfo propertyInfo, string objValue)
		{
			if(objValue == null)
				return null;
			if(! (!objType.Equals(typeof(Boolean)) || objValue.Length > 0) )
			{
				return null;
			}
			if(objType.IsEnum)
			{
				return EnumFromString(objType, objValue);
			}
			if(objType.Equals(typeof(string)))
			{
				return objValue;
			}
			PropertyDescriptor pc = null;
			if(propertyInfo != null)
			{
				pc = (TypeDescriptor.GetProperties(propertyInfo.ReflectedType))[propertyInfo.Name];
			}
			if(pc != null)
			{
				TypeConverter converter = pc.Converter;
				if(converter!=null && converter.CanConvertFrom(typeof(string)))
				{
					return converter.ConvertFromInvariantString(objValue);
				}
			}
			MethodInfo mi = objType.GetMethod("Parse", parseMethodTypesWithSOP);
			object o = null;
			if(mi != null)
			{
				object[] parameters = new object[2];
				parameters[0] = objValue;
				parameters[1] = CultureInfo.InvariantCulture;
				try
				{
					o = Utils.InvokeMethod(mi, null, parameters);
				} catch
				{
				}
			}
			if(o == null)
			{
				mi = objType.GetMethod("Parse", parseMethodTypes);
				if(mi!=null)
				{
					object[] parameters = new object[1];
					parameters[0] = objValue;
					try
					{
						o = Utils.InvokeMethod(mi, null, parameters);
					} catch
					{
					}
				}
			}
			if(o == null)
			{
				throw new HttpException(/*HttpRuntime.FormatResourceString(*/"Type_not_creatable_from_string"/*, objType.FullName, objValue, propertyInfo.Name)*/);
			}
			return o;
		}
	}
}
