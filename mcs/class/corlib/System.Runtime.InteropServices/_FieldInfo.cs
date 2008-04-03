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
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("8A7C1442-A9FB-366B-80D8-4939FFA6DBE0")]
	[TypeLibImportClass (typeof (FieldInfo))]
#if NET_2_0
	[ComVisible (true)]
#endif
	public interface _FieldInfo
	{
		bool Equals (object other);

		object[] GetCustomAttributes (bool inherit);

		object[] GetCustomAttributes (Type attributeType, bool inherit);

		int GetHashCode ();

		Type GetType ();

		void GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

		void GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo);

		void GetTypeInfoCount (out uint pcTInfo);

		void Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

		object GetValue (object obj);

#if !NET_2_0
		[CLSCompliant (false)]
#endif
		object GetValueDirect (TypedReference obj);

		bool IsDefined (Type attributeType, bool inherit);

		void SetValue (object obj, object value);

		void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

#if !NET_2_0
		[CLSCompliant (false)]
#endif
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
