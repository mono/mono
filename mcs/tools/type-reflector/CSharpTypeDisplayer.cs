//
// CSharpTypeDisplayer.cs: Displays type information as a tree
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
	public class CSharpTypeDisplayer : IndentingTypeDisplayer {

		public CSharpTypeDisplayer (TextWriter writer)
			: base (writer)
		{
		}

		private void PrintAttributes (MemberInfo m)
		{
			PrintAttributes (m, true);
		}

		private void PrintAttributes (MemberInfo m, bool newline)
		{
			PrintCilAttributes (m, newline);
			PrintCustomAttributes (m, "", newline);
		}

		private void PrintCilAttributes (MemberInfo m, bool newline)
		{
			PrintCilAttributes (m as Type, newline);
			PrintCilAttributes (m as MethodBase, newline);
		}

		private void PrintCilAttributes (MethodBase m, bool newline)
		{
			if (m == null)
				return;

			MethodImplAttributes attr = m.GetMethodImplementationFlags ();
			if ((attr & MethodImplAttributes.InternalCall) != 0) {
				Write ("[MethodImplAttribute(MethodImplOptions.InternalCall)]");
				if (newline)
					WriteLine ();
			}
		}

		private void PrintCilAttributes (Type t, bool newline)
		{
			if (t == null)
				return;
			if (t.IsSerializable) {
				Write ("[Serializable]");
				if (newline)
					WriteLine ();
			}
		}

		private void PrintCustomAttributes (ICustomAttributeProvider m, string attributeType, bool newline)
		{
			object[] attrs = m.GetCustomAttributes (true);
			foreach (object a in attrs) {
				Type type = a.GetType();
				Write("[{0}{1}", attributeType, type.FullName);

				string p = GetPropertyValues (type.GetProperties(), a);
				string f = GetFieldValues (type.GetFields(), a);

				if ((p.Length > 0) || (f.Length > 0)) {
					Write ("(");
					if (p.Length > 0) {
						Write (p);
						if (f.Length > 0)
							Write (", ");
					}
					if (f.Length > 0)
						Write (f);
					Write (")");
				}

				Write ("] ");

				if (newline)
					WriteLine ();
			}
		}

		private string GetPropertyValues (PropertyInfo[] props, object instance)
		{
			int len = props.Length;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i != len; ++i) {
				sb.Append (props[i].Name);
				sb.Append ("=");
				sb.Append (GetEncodedValue (props[i].GetValue (instance, null)));
				if (i != (len-1))
					sb.Append (", ");
			}
			return sb.ToString();
		}

		private string GetEncodedValue (object value)
		{
			if (value == null)
				return "null";

			switch (Type.GetTypeCode(value.GetType())) {
				case TypeCode.Char:
					return String.Format ("'{0}'", value.ToString());
				case TypeCode.Decimal:
					return String.Format ("{0}m", value.ToString());
				case TypeCode.Double:
					return String.Format ("{0}d", value.ToString());
				case TypeCode.Int64:
					return String.Format ("{0}L", value.ToString());
				case TypeCode.Single:
					return String.Format ("{0}f", value.ToString());
				case TypeCode.String:
					return String.Format ("\"{0}\"", value.ToString());
				case TypeCode.UInt32:
					return String.Format ("{0}U", value.ToString());
				case TypeCode.UInt64:
					return String.Format ("{0}UL", value.ToString());
				case TypeCode.Object:
					return String.Format ("typeof({0})", value.ToString());
			}
			// not special-cased; just return it's value
			return value.ToString();
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

		protected override void OnTypeBody (Type t, BindingFlags bf)
		{
			using (Indenter n1 = GetIndenter()) {
				WriteLine ("{{");
			}

			base.OnTypeBody (t, bf);

			using (Indenter n2 = GetIndenter()) {
				WriteLine ("}}");
				WriteLine ();
			}
		}

		protected override void OnIndentedType (TypeEventArgs e)
		{
			PrintAttributes (e.Type);
			WriteLine (GetTypeHeader (e.Type));
		}

		protected override void OnIndentedBaseType (BaseTypeEventArgs e)
		{
			WriteLine ("// Base Type");
			WriteLine ("  : {0}", e.BaseType);
		}

		protected override void OnIndentedInterfaces (InterfacesEventArgs e)
		{ 
			if (e.Interfaces.Length != 0) {
				WriteLine ();
				WriteLine ("// Implemented Interfaces");
				bool needComma = true;
				// We only want one `:', and OnIndentedBaseType would print one.
				if (!base.ShowBase) {
					Write ("  :");
					needComma = false;
				}
				using (Indenter n1 = GetIndenter()) {
					foreach (Type i in e.Interfaces) {
						if (needComma)
							WriteLine (", {0}", i);
						else {
							WriteLine ("{0}", i);
						}
					}
				}
			}
		}

		protected void PrintFieldInfo (FieldInfo f)
		{
			PrintAttributes (f);

			if (!f.DeclaringType.IsEnum || f.IsSpecialName) {
				if (f.IsPublic)
					Write ("public ");
				if (f.IsPrivate)
					Write ("private ");
				if (f.IsAssembly)
					Write ("internal ");
				if (f.IsFamily)
					Write ("protected ");
				if (f.IsLiteral)
					Write ("const ");
				else if (f.IsStatic)
					Write ("static ");

				Write ("{0} ", f.FieldType);
			}

			Write ("{0} ", f.Name);
			if (((f.Attributes & FieldAttributes.HasDefault) != 0) || f.IsStatic) {
				Write ("= {0}", FieldValue (f));
			}

			if (!f.DeclaringType.IsEnum || f.IsSpecialName)
				WriteLine (";");
			else
				WriteLine (",");
		}

		protected override void OnIndentedFields (FieldsEventArgs e)
		{
			WriteLine ("//");
			WriteLine ("// Fields");
			WriteLine ("//");
			foreach (FieldInfo f in e.Fields) {
				PrintFieldInfo (f);
				WriteLine ();
			}
		}

		protected void PrintPropertyInfo (PropertyInfo p)
		{
			PrintAttributes (p);

			Write ("{0} {1} {{ ", p.PropertyType, p.Name);
			if (p.CanRead)
				Write ("get; ");
			if (p.CanWrite)
				Write ("set; ");
			WriteLine ("}}");
		}

		protected override void OnIndentedProperties (PropertiesEventArgs e)
		{
			WriteLine ("//");
			WriteLine ("// Properties");
			WriteLine ("//");
			foreach (PropertyInfo p in e.Properties) {
				PrintPropertyInfo (p);
				WriteLine ();
			}
		}

		protected void PrintEventInfo (EventInfo i)
		{
			if (i.IsMulticast)
				Write ("event ");
			WriteLine ("{0} {1};", i.EventHandlerType, i.Name);
		}

		protected override void OnIndentedEvents (EventsEventArgs e)
		{
			WriteLine ("//");
			WriteLine ("// Events");
			WriteLine ("//");
			foreach (EventInfo i in e.Events) {
				PrintEventInfo (i);
				WriteLine ();
			}
		}

		private void PrintMethodQualifiers (MethodBase m)
		{
			if (m.IsPublic)
				Write ("public ");
			if (m.IsPrivate)
				Write ("private ");
			if (m.IsStatic)
				Write ("static ");
			if (m.IsVirtual)
				Write ("virtual ");
			if (m.IsAbstract)
				Write ("abstract ");
			if (m.IsAssembly)
				Write ("internal ");
			if (m.IsFamily)
				Write ("protected ");
			if (m.IsFinal)
				Write ("sealed ");
		}

		private void PrintMethodArgs (MethodBase m)
		{
			Write ("(");
			ParameterInfo[] parms = m.GetParameters ();
			if (parms.Length != 0) {
				int cur = 0;
				foreach (ParameterInfo pi in parms) {
					PrintCustomAttributes (pi, "", false);
					if (pi.IsOut)
						Write ("out ");
					Write ("{0} {1}", pi.ParameterType, pi.Name);
					if (cur++ != (parms.Length-1))
						Write (",");
				}
			}
			WriteLine (");");
		}

		protected void PrintConstructorInfo (ConstructorInfo c)
		{
			if (ShowMonoBroken) {
				PrintAttributes (c);
			}
			PrintMethodQualifiers (c);

			Write ("{0} ", c.DeclaringType.Name);
			PrintMethodArgs (c);
		}

		protected override void OnIndentedConstructors (ConstructorsEventArgs e)
		{
			WriteLine ("//");
			WriteLine ("// Constructors");
			WriteLine ("//");
			foreach (ConstructorInfo c in e.Constructors) {
				PrintConstructorInfo (c);
				WriteLine ();
			}
		}

		protected void PrintMethodInfo (MethodInfo m)
		{
			PrintAttributes (m);
			if (m.ReturnTypeCustomAttributes != null)
				PrintCustomAttributes (m.ReturnTypeCustomAttributes, "return: ", true);
			PrintMethodQualifiers (m);

			Write ("{0} {1} ", m.ReturnType, m.Name);
			PrintMethodArgs (m);
		}

		protected override void OnIndentedMethods (MethodsEventArgs e)
		{
			WriteLine ("//");
			WriteLine ("// Methods");
			WriteLine ("//");
			foreach (MethodInfo m in e.Methods) {
				if ((m.Attributes & MethodAttributes.SpecialName) == 0) {
					PrintMethodInfo (m);
					WriteLine ();
				}
			}
		}
	}
}

