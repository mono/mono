//
// NodeFormatter.cs: Formats NodeInfo instances for display
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
	public class NodeFormatter : INodeFormatter {

		public string GetDescription (NodeInfo node)
		{
			string r = "";
			switch (node.NodeType) {
			case NodeTypes.Type:
				r = GetTypeDescription ((Type)node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.BaseType:
				r = GetBaseTypeDescription ((Type)node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Interface:
				r = GetInterfaceDescription ((Type)node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Field:
				r = GetFieldDescription ((FieldInfo)node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Constructor:
				r = GetConstructorDescription ((ConstructorInfo)node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Method:
				r = GetMethodDescription ((MethodInfo) node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Parameter:
				r = GetParameterDescription ((ParameterInfo) node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Property:
				r = GetPropertyDescription ((PropertyInfo) node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Event:
				r = GetEventDescription ((EventInfo) node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.CustomAttributeProvider:
				r = GetCustomAttributeProviderDescription ((ICustomAttributeProvider) node.ReflectionObject, node.ReflectionInstance);
				break;
			case NodeTypes.Other:
			case NodeTypes.Alias:
				r = GetOtherDescription (node);
				break;
			case NodeTypes.ReturnValue:
				r = GetReturnValueDescription (node);
				break;
			default:
				Debug.Assert (false, 
					String.Format ("Unhandled NodeInfo value: {0}", node.NodeType));
				break;
			}
			return r;
		}

		public string GetValue (object o)
		{
			if (o == null)
				return "null";
			if (o.GetType().IsEnum)
				return GetEnumValue (o.GetType(), o);
			return o.ToString();
		}

		public static string GetEnumValue (Type enumType, object value)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (Enum.Format(enumType, value, "f"));
			sb.Append (" (");
			sb.Append (String.Format ("0x{0}", Enum.Format (enumType, value, "x")));
			sb.Append (")");
			return sb.ToString ();
		}

		public static string GetTypeKeyword (Type type)
		{
			string t = null;

			if (type.IsClass)
				t = "class";
			else if (type.IsEnum)
				t = "enum";
			else if (type.IsValueType)
				t = "struct";
			else if (type.IsInterface)
				t = "interface";
			else
				t = "type";

			return t;
		}

		protected virtual string GetTypeDescription (Type type, object instance)
		{
			return string.Format ("{0} {1}", GetTypeKeyword(type), type.FullName);
		}

		protected virtual string GetBaseTypeDescription (Type type, object instance)
		{
			return type.Name;
		}

		protected virtual string GetInterfaceDescription (Type type, object instance)
		{
			return type.Name;
		}

		protected virtual string GetConstructorDescription (ConstructorInfo ctor, object instance)
		{
			return ctor.Name;
		}

		protected virtual string GetEventDescription (EventInfo e, object instance)
		{
			return e.Name;
		}

		protected virtual string GetFieldDescription (FieldInfo field, object instance)
		{
			return field.Name;
		}

		protected virtual string GetMethodDescription (MethodInfo method, object instance)
		{
      return method.Name;
		}

		protected virtual string GetParameterDescription (ParameterInfo param, object instance)
		{
			return param.Name;
		}

		protected virtual string GetPropertyDescription (PropertyInfo property, object instance)
		{
			return property.Name;
		}

		protected virtual string GetCustomAttributeProviderDescription (ICustomAttributeProvider m, object instance)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (object a in m.GetCustomAttributes(true)) {
				sb.Append (string.Format ("\n\t{0}", GetCustomAttribute(a)));
			}
			return string.Format ("GetCustomAttributes()={0}{1}", 
					sb.Length == 0 ? "<none/>" : "",
					sb.ToString());
		}

		protected virtual string GetCustomAttributes (ICustomAttributeProvider m, string attributeType, bool newLine)
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

		protected virtual string GetCustomAttribute (object attribute)
		{
			StringBuilder sb = new StringBuilder ();
			GetCustomAttribute (sb, attribute, "");
			return sb.ToString();
		}

		protected virtual void GetCustomAttribute (StringBuilder sb, object attribute, string attributeType)
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

		protected virtual char[] GetAttributeDelimeters ()
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

		protected virtual string GetEncodedValue (object value)
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

		protected virtual string GetEncodedCharValue (object value)
		{
			return String.Format ("'{0}'", value.ToString());
		}

		protected virtual string GetEncodedDecimalValue (object value)
		{
			return String.Format ("{0}m", value.ToString());
		}

		protected virtual string GetEncodedDoubleValue (object value)
		{
			return String.Format ("{0}d", value.ToString());
		}

		protected virtual string GetEncodedInt64Value (object value)
		{
			return String.Format ("{0}L", value.ToString());
		}

		protected virtual string GetEncodedSingleValue (object value)
		{
			return String.Format ("{0}f", value.ToString());
		}

		protected virtual string GetEncodedStringValue (object value)
		{
			return String.Format ("\"{0}\"", value.ToString());
		}

		protected virtual string GetEncodedUInt32Value (object value)
		{
			return String.Format ("{0}U", value.ToString());
		}

		protected virtual string GetEncodedUInt64Value (object value)
		{
			return String.Format ("{0}UL", value.ToString());
		}

		protected virtual string GetEncodedObjectValue (object value)
		{
			return String.Format ("typeof({0})", value.ToString());
		}

		protected virtual string GetOtherDescription (NodeInfo node)
		{
			if (node.Description != null)
				return node.Description.ToString();
			return string.Format (
					"{{** Error: Invalid NodeInfo.Description or unhandled type; " + 
						"NodeType={0}, ReflectionObject={1}, " +
						"ReflectionInstance={2}}}\n{3}", 
					node.NodeType, 
					node.ReflectionObject,
					node.ReflectionInstance,
					new StackTrace());
		}

		protected virtual string GetReturnValueDescription (NodeInfo node)
		{
			return "ReturnValue=" + GetOtherDescription (node);
		}
	}
}

