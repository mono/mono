//
// NodeFormatter.cs: Formats NodeInfo instances for display
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002-2003 Jonathan Pryor
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
        /*
			case NodeTypes.CustomAttributeProvider:
				r = GetCustomAttributeProviderDescription ((ICustomAttributeProvider) node.ReflectionObject, node.ReflectionInstance);
				break;
         */
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
			return string.Format ("{0} {1}", 
					GetTypeKeyword(type), type.FullName);
		}

		protected virtual string GetBaseTypeDescription (Type type, object instance)
		{
      if (type != null)
        return type.Name;
      return "No Base Type";
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

		protected virtual string GetOtherDescription (NodeInfo node)
		{
			if (node.Description != null)
				return node.Description.ToString();
			return string.Format (
					"(** Error: Invalid NodeInfo.Description or unhandled type; " + 
						"NodeType={0}, ReflectionObject={1}, " +
						"ReflectionInstance={2}}}\n{3})", 
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

