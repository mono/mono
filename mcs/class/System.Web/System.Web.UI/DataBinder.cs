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
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI {

	public sealed class DataBinder
	{
		public DataBinder ()
		{
		}

		private static string FormatResult (object result, string format)
		{
			if (result == null)
				return String.Empty;

			if (format == null)
				return result.ToString ();

			return String.Format (format, result);
		}
		
		public static object Eval (object container, string expression)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");

			object current = container;

			while (current != null) {
				int dot = expression.IndexOf ('.');
				int size = (dot == -1) ? expression.Length : dot;
				string prop = expression.Substring (0, size);
				if (prop.IndexOf ('[') != -1)
					current = GetIndexedPropertyValue (current, prop);
				else
					current = GetPropertyValue (current, prop);

				if (dot == -1)
					break;
				
				expression = expression.Substring (prop.Length + 1);
			}

			return current;
		}

		public static string Eval (object container, string expression, string format)
		{
			object result = Eval (container, expression);
			return FormatResult (result, format);
		}

		public static object GetIndexedPropertyValue (object container, string expr)
		{
			if (expr == null)
				throw new ArgumentNullException ("expr");

			int openIdx = expr.IndexOf ('[');
			int closeIdx = expr.IndexOf (']'); // see the test case. MS ignores all after the first ]
			if (openIdx < 0 || closeIdx < 0 || closeIdx - openIdx <= 1)
				throw new ArgumentException (expr + " is not a valid indexed expression.");

			string val = expr.Substring (openIdx + 1, closeIdx - openIdx - 1);
			val = val.Trim ();
			int valLength = val.Length;
			if (valLength == 0)
				throw new ArgumentException (expr + " is not a valid indexed expression.");

			int intVal = 0;
			bool is_string;
			char first = val [0];
			if (first >= '0' && first <= '9') {
				is_string = false;
				try {
					intVal = Int32.Parse (val);
				} catch {
					throw new ArgumentException (expr + " is not a valid indexed expression.");
				}
				
			} else if (first == '"' && val [valLength - 1] == '"') {
				is_string = true;
				val = val.Substring (0, val.Length - 1).Substring (1);
			} else {
				throw new ArgumentException (expr + " is not a valid indexed expression.");
			}

			string property = null;
			if (openIdx > 0) {
				property = expr.Substring (0, openIdx);
				if (property != null && property != String.Empty)
					container = GetPropertyValue (container, property);
			}

			if (container == null)
				return null;

			Type t = container.GetType ();
			// MS does not seem to look for any other than "Item"!!!
			object [] atts = t.GetCustomAttributes (typeof (DefaultMemberAttribute), false);
			if (atts.Length != 1)
				throw new ArgumentException (expr + " indexer not found.");
				
			property = ((DefaultMemberAttribute) atts [0]).MemberName;

			Type [] argTypes = new Type [] { (is_string) ? typeof (string) : typeof (int) };
			PropertyInfo prop = t.GetProperty (property, argTypes);
			if (prop == null)
				throw new ArgumentException (expr + " indexer not found.");

			object [] args = new object [1];
			if (is_string)
				args [0] = val;
			else
				args [0] = intVal;

			return prop.GetValue (container, args);
		}

		public static string GetIndexedPropertyValue (object container, string expr, string format)
		{
			object result = GetIndexedPropertyValue (container, expr);
			return FormatResult (result, format);
		}

		public static object GetPropertyValue (object container, string propName)
		{
			if (propName == null)
				throw new ArgumentNullException ("propName");

			PropertyDescriptor prop = TypeDescriptor.GetProperties (container).Find (propName, true);
			if (prop == null) {
				throw new HttpException ("Property " + propName + " not found in " +
							 container.GetType ());
			}

			return prop.GetValue (container);
		}

		public static string GetPropertyValue (object container, string propName, string format)
		{
			object result = GetPropertyValue (container, propName);
			return FormatResult (result, format);
		}		
	}
}

