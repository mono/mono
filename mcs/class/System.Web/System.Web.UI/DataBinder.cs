//
// System.Web.UI.DataBinder.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Reflection;

namespace System.Web.UI {

	public sealed class DataBinder
	{
		public DataBinder ()
		{
		}

		public static object Eval (object container, string expression)
		{
			return GetPropertyValue (container, expression);
		}

		public static string Eval (object container, string expression, string format)
		{
			return GetPropertyValue (container, expression, format);
		}

		[MonoTODO]
		public static object GetIndexedPropertyValue (object container, string expr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetIndexedPropertyValue (object container, string expr, string format)
		{
			throw new NotImplementedException ();
		}

		public static object GetPropertyValue (object container, string propName)
		{
			if (container == null || propName == null)
				throw new ArgumentException ();

			Type type = container.GetType ();
			PropertyInfo prop = type.GetProperty (propName);
			if (prop == null)
				throw new HttpException ("Property " + propName + " not found in " +
							 type.ToString ());
			MethodInfo getm = prop.GetGetMethod ();
			if (getm == null)
				throw new HttpException ("Cannot find get accessor for " + propName +
							 " in " + type.ToString ());
			
			return getm.Invoke (container, null);
		}

		public static string GetPropertyValue (object container, string propName, string format)
		{
			object result;

			result = GetPropertyValue (container, propName);
			if (result == null)
				return String.Empty;

			if (format == null)
				return result.ToString ();

			return String.Format (format, result);
		}		
	}
}

