//
// System.Runtime.InteropServices._MethodBase interface
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

using System;
using System.Globalization;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("6240837A-707F-3181-8E98-A36AE086766B")]
#if !FULL_AOT_RUNTIME
	[TypeLibImportClass (typeof (MethodBase))]
#endif
	[ComVisible (true)]
	public interface _MethodBase
	{
		bool Equals (object other);
		
		object[] GetCustomAttributes (bool inherit);
		
		object[] GetCustomAttributes (Type attributeType, bool inherit);
		
		int GetHashCode ();

		MethodImplAttributes GetMethodImplementationFlags ();
		
		ParameterInfo[] GetParameters ();

		void GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

		void GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo);

		void GetTypeInfoCount (out uint pcTInfo);

		void Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
		
		Type GetType ();

		object Invoke (object obj, object[] parameters);
		
		object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);
		
		bool IsDefined(Type attributeType, bool inherit);
		
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
