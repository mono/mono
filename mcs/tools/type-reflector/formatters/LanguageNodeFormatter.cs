//
// LanguageNodeFormatter.cs: 
//   Common NodeInfo formatting code for Language-specific output
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class LanguageNodeFormatter : NodeFormatter {

		protected override string GetTypeDescription (Type type, object instance)
		{
			return string.Format ("{0} {1}", GetTypeKeyword(type), type.Name);
		}

		protected override string GetBaseTypeDescription (Type type, object instance)
		{
			return type.Name;
		}

		protected override string GetInterfaceDescription (Type type, object instance)
		{
			return type.Name;
		}

		protected override string GetConstructorDescription (ConstructorInfo ctor, object instance)
		{
			return string.Format ("{0} ({1})", ctor.Name, ctor.GetParameters().Length);
			// return ctor.Name;
		}

		protected override string GetEventDescription (EventInfo e, object instance)
		{
			return e.Name;
		}

		protected override string GetFieldDescription (FieldInfo field, object instance)
		{
			try {
				return string.Format ("{0}={1}", field.Name, 
						// GetValue (field, instance));
						GetValue (instance));
			} catch {
				return field.Name;
			}
		}

		protected override string GetMethodDescription (MethodInfo mb, object instance)
		{
			if (mb.GetParameters().Length == 0) {
				try {
					object r = mb.Invoke (instance, null);
					string s = GetValue (r);
					return string.Format ("{0}()={1}", mb.Name, s);
				}
				catch {
				}
			}
			return string.Format ("{0} ({1})", mb.Name, mb.GetParameters().Length);
		}

		protected override string GetParameterDescription (ParameterInfo param, object instance)
		{
			return param.Name;
		}

		protected override string GetPropertyDescription (PropertyInfo property, object instance)
		{
			string v = "";
			try {
				// object o = property.GetGetMethod(true).Invoke(instance, null);
				object o = property.GetValue (instance, null);
				v = string.Format ("={0}", GetValue (o));
			} catch {
				v = "";
			}
			return string.Format ("{0}{1}", property.Name, v);
			// return property.Name;
			// return string.Format ("{0}={1}", property.Name, property.GetValue(null));
		}

		protected override string GetCustomAttributeProviderDescription (ICustomAttributeProvider m, object instance)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (object a in m.GetCustomAttributes(true)) {
				sb.Append (string.Format ("\n\t{0}", GetCustomAttribute(a)));
			}
			return string.Format ("GetCustomAttributes()={0}{1}", 
					sb.Length == 0 ? "<none/>" : "",
					sb.ToString());
		}

		protected override string GetCustomAttributes (ICustomAttributeProvider m, string attributeType, bool newLine)
		{
			StringBuilder sb = new StringBuilder ();

			object[] attrs = m.GetCustomAttributes (true);

			foreach (object a in attrs) {

				GetCustomAttribute (sb, a, attributeType);

				if (newLine)
					sb.Append ("\n");
			}

			return sb.ToString ();
		}

		protected override string GetCustomAttribute (object attribute)
		{
			StringBuilder sb = new StringBuilder ();
			GetCustomAttribute (sb, attribute, "");
			return sb.ToString();
		}

		protected override void GetCustomAttribute (StringBuilder sb, object attribute, string attributeType)
		{
			Type type = attribute.GetType();
			char[] delims = GetAttributeDelimeters ();
			sb.AppendFormat ("{0}{1}{2}", delims[0], attributeType, type.FullName);

			string p = GetPropertyValues (type.GetProperties(), attribute);
			string f = GetFieldValues (type.GetFields(), attribute);

			if ((p.Length > 0) || (f.Length > 0)) {
				sb.Append ("(");
				if (p.Length > 0) {
					sb.Append (p);
					if (f.Length > 0)
						sb.Append (", ");
				}
				if (f.Length > 0)
					sb.Append (f);
				sb.Append (")");
			}

			sb.Append (delims[1]);
		}

		protected override char[] GetAttributeDelimeters ()
		{
			return new char[]{'[', ']'};
		}

		private string GetPropertyValues (PropertyInfo[] props, object instance)
		{
			int len = props.Length;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i != len; ++i) {
				sb.Append (props[i].Name);
				sb.Append ("=");
				try {
					sb.Append (GetEncodedValue (props[i].GetValue (instance, null)));
				} catch {
					sb.Append ("<exception/>");
				}
				if (i != (len-1))
					sb.Append (", ");
			}
			return sb.ToString();
		}

		private string GetFieldValues (FieldInfo[] fields, object instance)
		{
			int len = fields.Length;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i != len; ++i) {
				sb.Append (fields[i].Name);
				sb.Append ("=");
				sb.Append (GetEncodedValue (fields[i].GetValue (instance)));
				if (i != (len-1))
					sb.Append (", ");
			}
			return sb.ToString();
		}

		protected override string GetEncodedValue (object value)
		{
			if (value == null)
				return "null";

			switch (Type.GetTypeCode(value.GetType())) {
				case TypeCode.Char:
					return GetEncodedCharValue (value);
				case TypeCode.Decimal:
					return GetEncodedDecimalValue (value);
				case TypeCode.Double:
					return GetEncodedDoubleValue (value);
				case TypeCode.Int64:
					return GetEncodedInt64Value (value);
				case TypeCode.Single:
					return GetEncodedSingleValue (value);
				case TypeCode.String:
					return GetEncodedStringValue (value);
				case TypeCode.UInt32:
					return GetEncodedUInt32Value (value);
				case TypeCode.UInt64:
					return GetEncodedUInt64Value (value);
				case TypeCode.Object:
					return GetEncodedObjectValue (value);
			}
			// not special-cased; just return it's value
			return value.ToString();
		}

		protected override string GetEncodedCharValue (object value)
		{
			return String.Format ("'{0}'", value.ToString());
		}

		protected override string GetEncodedDecimalValue (object value)
		{
			return String.Format ("{0}m", value.ToString());
		}

		protected override string GetEncodedDoubleValue (object value)
		{
			return String.Format ("{0}d", value.ToString());
		}

		protected override string GetEncodedInt64Value (object value)
		{
			return String.Format ("{0}L", value.ToString());
		}

		protected override string GetEncodedSingleValue (object value)
		{
			return String.Format ("{0}f", value.ToString());
		}

		protected override string GetEncodedStringValue (object value)
		{
			return String.Format ("\"{0}\"", value.ToString());
		}

		protected override string GetEncodedUInt32Value (object value)
		{
			return String.Format ("{0}U", value.ToString());
		}

		protected override string GetEncodedUInt64Value (object value)
		{
			return String.Format ("{0}UL", value.ToString());
		}

		protected override string GetEncodedObjectValue (object value)
		{
			return String.Format ("typeof({0})", value.ToString());
		}

		protected override string GetOtherDescription (NodeInfo node)
		{
			if (node.Description != null)
				return node.Description.ToString();
			return "<null other description/>";
		}
	}
}

