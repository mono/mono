//
// System.Reflection.IReflect.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[Guid("AFBF15E5-C37C-11d2-B88E-00A0C9B471B8")]
	public interface IReflect {

		Type UnderlyingSystemType {
			get;
		}

		FieldInfo    GetField   (string name, BindingFlags binding_attr);
		FieldInfo [] GetFields  (BindingFlags binding_attr);
		MemberInfo[] GetMember  (string name, BindingFlags binding_attr);
		MemberInfo[] GetMembers (BindingFlags binding_attr);
		MethodInfo   GetMethod  (string name, BindingFlags binding_attr);
		MethodInfo   GetMethod  (string name, BindingFlags binding_attr,
					 Binder binder, Type [] types, ParameterModifier [] modifiers);
		MethodInfo[] GetMethods (BindingFlags binding_attr);

		PropertyInfo [] GetProperties (BindingFlags binding_attr);
		PropertyInfo    GetProperty   (string name, BindingFlags binding_attr);
		PropertyInfo    GetProperty   (string name, BindingFlags binding_attr,
					       Binder binder, Type return_type, Type [] types,
					       ParameterModifier [] modifiers);

		object InvokeMember (string name, BindingFlags invoke_attr,
				     Binder binder, object target, object [] args,
				     ParameterModifier [] modifiers,
				     CultureInfo culture,
				     string [] named_parameters);
				     
	}
}
