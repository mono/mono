//
// System.Runtime.InteropServices._FieldInfo interface
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_1_1

using System;
using System.Globalization;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsDual)]
	[Guid ("8A7C1442-A9FB-366B-80D8-4939FFA6DBE0")]
	public interface _FieldInfo
	{
		bool Equals (object obj);

		object[] GetCustomAttributes (bool inherit);

		object[] GetCustomAttributes (Type attributeType, bool inherit);

		int GetHashCode ();

		Type GetType ();

		object GetValue (object obj);

		object GetValueDirect (TypedReference obj);

		bool IsDefined (Type attributeType, bool inherit);

		void SetValue (object obj, object value);

		void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

		void SetValueDirect (TypedReference obj, object value);

		string ToString ();

		FieldAttributes Attributes {get;}

		Type DeclaringType {get;}

		RuntimeFieldHandle FieldHandle {get;}

		Type FieldType {get;}

		bool IsAssembly {get;}

		bool IsFamily {get;}

		bool IsFamilyAndAssembly {get;}

		bool IsFamilyOrAssembly {get;}

		bool IsInitOnly {get;}

		bool IsLiteral {get;}

		bool IsNotSerialized {get;}

		bool IsPinvokeImpl {get;}

		bool IsPrivate {get;}

		bool IsPublic {get;}

		bool IsSpecialName {get;}

		bool IsStatic {get;}

		MemberTypes MemberType {get;}

		string Name {get;}

		Type ReflectedType {get;}
	}
}
#endif
