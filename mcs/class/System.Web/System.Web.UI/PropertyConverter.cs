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
			} catch(Exception e)
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
				} catch(Exception e)
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
					} catch(Exception e)
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
