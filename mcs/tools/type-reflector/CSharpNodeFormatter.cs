//
// CSharpNodeFormatter.cs: Formats nodes with C# syntax
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
	public class CSharpNodeFormatter : LanguageNodeFormatter {

		public CSharpNodeFormatter ()
		{
		}

		protected override string GetTypeDescription (Type type, object instance)
		{
			StringBuilder sb = new StringBuilder ();
			GetAttributes (sb, type);
			sb.Append (base.GetTypeDescription (type, instance));
			return sb.ToString();
		}

		private void GetAttributes (StringBuilder sb, MemberInfo m)
		{
			GetAttributes (sb, m, true);
		}

		private void GetAttributes (StringBuilder sb, MemberInfo m, bool newline)
		{
			GetCilAttributes (sb, m, newline);
			sb.Append (GetCustomAttributes (m, "", newline));
		}

		private void GetCilAttributes (StringBuilder sb, MemberInfo m, bool newline)
		{
			Type t = m as Type;
			MethodBase mb = m as MethodBase;
			if (t != null)
				GetCilAttributes (sb, t, newline);
			else if (mb != null)
				GetCilAttributes (sb, mb, newline);
		}

		private void GetCilAttributes (StringBuilder sb, Type t, bool newline)
		{
			if (t.IsSerializable) {
				sb.Append ("[Serializable]");
				if (newline)
					sb.Append ("\n");
			}
		}

		private void GetCilAttributes (StringBuilder sb, MethodBase m, bool newline)
		{
			MethodImplAttributes attr = m.GetMethodImplementationFlags ();
			if ((attr & MethodImplAttributes.InternalCall) != 0) {
				sb.Append ("[MethodImplAttribute(MethodImplOptions.InternalCall)]");
				if (newline)
					sb.Append ("\n");
			}
		}

		protected override string GetBaseTypeDescription (Type type, object instance)
		{
			return ": " + type.Name;
		}

		protected override string GetInterfaceDescription (Type type, object instance)
		{
			return ", " + type.Name;
		}

		protected override string GetConstructorDescription (ConstructorInfo ctor, object instance)
		{
			StringBuilder sb = new StringBuilder ();
			GetAttributes (sb, ctor);
			GetMethodQualifiers (sb, ctor);
			sb.AppendFormat ("{0} ", ctor.DeclaringType.Name);
			GetMethodArgs (sb, ctor);

			return sb.ToString();
		}

		private void GetMethodQualifiers (StringBuilder sb, MethodBase m)
		{
			if (m.IsPublic)
				sb.Append ("public ");
			if (m.IsFamily)
				sb.Append ("protected ");
			if (m.IsAssembly)
				sb.Append ("internal ");
			if (m.IsPrivate)
				sb.Append ("private ");
			if (m.IsStatic)
				sb.Append ("static ");
			if (m.IsFinal)
				sb.Append ("sealed ");
			if (m.IsAbstract)
				sb.Append ("abstract ");
			else if (m.IsVirtual)
				sb.Append ("virtual ");
		}

		private void GetMethodArgs (StringBuilder sb, MethodBase m)
		{
			sb.Append ("(");
			ParameterInfo[] parms = m.GetParameters ();
			if (parms.Length != 0) {
				int cur = 0;
				foreach (ParameterInfo pi in parms) {
					sb.Append (GetParameterDescription (pi, pi));
					if (cur++ != (parms.Length-1))
						sb.Append (", ");
				}
			}
			sb.Append (")");
		}

		protected override string GetEventDescription (EventInfo e, object instance)
		{
			StringBuilder sb = new StringBuilder ();
			GetMethodQualifiers (sb, e.GetAddMethod (true));
			return String.Format ("{0}{1}{2} {3}", 
					sb.ToString(),
					e.IsMulticast ? "event " : "",
					e.EventHandlerType,
					e.Name);
		}

		protected override string GetFieldDescription (FieldInfo field, object instance)
		{
			StringBuilder sb = new StringBuilder ();

			GetAttributes (sb, field);

			if (!field.DeclaringType.IsEnum || field.IsSpecialName) {
				if (field.IsPublic)
					sb.Append ("public ");
				if (field.IsPrivate)
					sb.Append ("private ");
				if (field.IsAssembly)
					sb.Append ("internal ");
				if (field.IsFamily)
					sb.Append ("protected ");
				if (field.IsLiteral)
					sb.Append ("const ");
				else if (field.IsStatic)
					sb.Append ("static ");

				sb.AppendFormat ("{0} ", field.FieldType);
			}

			sb.AppendFormat ("{0}", field.Name);

			try {
				sb.AppendFormat (" = {0}", GetValue (field.GetValue (instance)));
			}
			catch {
			}

			if (!field.DeclaringType.IsEnum || field.IsSpecialName)
				sb.Append (";");
			else
				sb.Append (",");

			return sb.ToString ();
		}

		protected override string GetMethodDescription (MethodInfo method, object instance)
		{
			StringBuilder sb = new StringBuilder ();

			if (method.IsSpecialName)
				sb.Append ("/* Method is a specially named method:\n");

			GetAttributes (sb, method);
			if (method.ReturnTypeCustomAttributes != null) {
				sb.Append (GetCustomAttributes (method.ReturnTypeCustomAttributes, "return: ", true));
			}
			GetMethodQualifiers (sb, method);
			sb.AppendFormat ("{0} {1} ", method.ReturnType, method.Name);
			GetMethodArgs (sb, method);
			
			if (method.GetParameters().Length == 0) {
				try {
					object r = method.Invoke (instance, null);
					string s = GetValue (r);
					sb.AppendFormat ("={0}", s);
				}
				catch {
				}
			}

			if (method.IsSpecialName) {
				sb.Append ("\n */");
			}

			return sb.ToString();
		}

		protected override string GetParameterDescription (ParameterInfo param, object instance)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (GetCustomAttributes (param, "", false));
			if (param.IsOut)
				sb.Append ("out ");
			sb.AppendFormat ("{0} {1}", param.ParameterType, param.Name);

			return sb.ToString();
		}

		protected override string GetPropertyDescription (PropertyInfo property, object instance)
		{
			StringBuilder sb = new StringBuilder ();
			GetAttributes (sb, property);
			GetMethodQualifiers (sb, property.GetAccessors(true)[0]);

			sb.AppendFormat ("{0} {1} {{", property.PropertyType, property.Name);
			if (property.CanRead) {
				sb.Append ("get");
				try {
					sb.AppendFormat (" /* = {0} */", GetValue (property.GetValue (instance, null)));
				}
				catch {
				}
				sb.Append ("; ");
			}
			if (property.CanWrite)
				sb.Append ("set; ");
			sb.Append ("}}");

			return sb.ToString();
		}

		protected override string GetReturnValueDescription (NodeInfo n)
		{
			return string.Format ("/* ReturnValue={0} */", GetOtherDescription (n));
		}
	}
}

