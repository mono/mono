//
// System.Runtime.InteropServices._ConstructorInfo interface
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;
using System.Globalization;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsDual)]
	[Guid ("E9A19478-9646-3679-9B10-8411AE1FD57D")]
	public interface _ConstructorInfo
	{
		bool Equals (object obj);

		object[] GetCustomAttributes (bool inherit);

		object[] GetCustomAttributes (Type attributeType, bool inherit);

		int GetHashCode ();

		MethodImplAttributes GetMethodImplementationFlags ();

		ParameterInfo[] GetParameters ();

		Type GetType ();

		object Invoke (object[] parameters);

		object Invoke (object obj, object[] parameters);

		object Invoke (BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);

		object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);

		bool IsDefined (Type attributeType, bool inherit);

		string ToString ();

		MethodAttributes Attributes {get;}

		CallingConventions CallingConvention {get;}

		Type DeclaringType {get;}

		bool IsAbstract {get;}

		bool IsAssembly {get;}

		bool IsConstructor {get;}

		bool IsFamily {get;}

		bool IsFamilyAndAssembly {get;}

		bool IsFamilyOrAssembly {get;}

		bool IsFinal {get;}

		bool IsHideBySig {get;}

		bool IsPrivate {get;}

		bool IsPublic {get;}

		bool IsSpecialName {get;}

		bool IsStatic {get;}

		bool IsVirtual {get;}

		MemberTypes MemberType {get;}

		RuntimeMethodHandle MethodHandle {get;}

		string Name {get;}

		Type ReflectedType {get;}
	}
}
#endif