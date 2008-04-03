//
// System.Reflection.IReflect.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Reflection {

#if NET_2_0
	[ComVisible (true)]
#endif
	[Guid("AFBF15E5-C37C-11d2-B88E-00A0C9B471B8")]
	public interface IReflect {

		Type UnderlyingSystemType {
			get;
		}

		FieldInfo    GetField   (string name, BindingFlags bindingAttr);
		FieldInfo [] GetFields  (BindingFlags bindingAttr);
		MemberInfo[] GetMember  (string name, BindingFlags bindingAttr);
		MemberInfo[] GetMembers (BindingFlags bindingAttr);
		MethodInfo   GetMethod  (string name, BindingFlags bindingAttr);
		MethodInfo   GetMethod  (string name, BindingFlags bindingAttr,
					 Binder binder, Type [] types, ParameterModifier [] modifiers);
		MethodInfo[] GetMethods (BindingFlags bindingAttr);

		PropertyInfo [] GetProperties (BindingFlags bindingAttr);
		PropertyInfo    GetProperty   (string name, BindingFlags bindingAttr);
		PropertyInfo    GetProperty   (string name, BindingFlags bindingAttr,
					       Binder binder, Type returnType, Type [] types,
					       ParameterModifier [] modifiers);

		object InvokeMember (string name, BindingFlags invokeAttr,
				     Binder binder, object target, object [] args,
				     ParameterModifier [] modifiers,
				     CultureInfo culture,
				     string [] namedParameters);
				     
	}
}
