//
// DefaultNodeFormatter.cs: Formats NodeInfo instances for display
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

namespace Mono.TypeReflector.Formatters
{
	public class DefaultNodeFormatter : NodeFormatter {

		protected override string GetFieldDescription (FieldInfo field, object instance)
		{
			try {
				return string.Format ("{0}={1}", field.Name, 
						GetValue (field.GetValue(instance)));
			} catch {
				return field.Name;
			}
		}

		protected override string GetMethodDescription (MethodInfo mb, object instance)
		{
			if (mb.GetParameters().Length == 0) {
        StringBuilder sb = new StringBuilder ();
        AddMethodReturnValue (sb, mb.Name + "()={0}", mb, instance);
        if (sb.Length != 0)
          return sb.ToString();
			}
			return mb.Name;
		}

		protected override string GetPropertyDescription (PropertyInfo property, object instance)
		{
			string v = "";
			try {
				// object o = property.GetGetMethod(true).Invoke(instance, null);
				object o = property.GetValue (instance, null);
				v = string.Format ("={0}", GetValue (o));
			} catch {
			}
			return string.Format ("{0}{1}", property.Name, v);
		}
	}
}

