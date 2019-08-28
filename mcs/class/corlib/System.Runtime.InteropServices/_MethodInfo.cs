//
// System.Runtime.InteropServices._MethodInfo interface
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
	[Guid ("FFCC1B5D-ECB8-38DD-9B01-3DC8ABC2AA5F")]
#if !FULL_AOT_RUNTIME
	[TypeLibImportClass (typeof (MethodInfo))]
#endif
	[ComVisible (true)]
	public interface _MethodInfo
	{
		void GetTypeInfoCount(out uint pcTInfo);
		void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
		void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
		void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

		String ToString();
		bool Equals(Object other);
		int GetHashCode();
		Type GetType();

		MemberTypes MemberType { get; }
		String Name { get; }
		Type DeclaringType { get; }
		Type ReflectedType { get; }
		Object[] GetCustomAttributes(Type attributeType, bool inherit);
		Object[] GetCustomAttributes(bool inherit);
		bool IsDefined(Type attributeType, bool inherit);

		ParameterInfo[] GetParameters();
		MethodImplAttributes GetMethodImplementationFlags();
		RuntimeMethodHandle MethodHandle { get; }
		MethodAttributes Attributes { get; }
		CallingConventions CallingConvention { get; }
		Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture);
		bool IsPublic { get; }
		bool IsPrivate { get; }
		bool IsFamily { get; }
		bool IsAssembly { get; }
		bool IsFamilyAndAssembly { get; }
		bool IsFamilyOrAssembly { get; }
		bool IsStatic { get; }
		bool IsFinal { get; }
		bool IsVirtual { get; }
		bool IsHideBySig { get; }
		bool IsAbstract { get; }
		bool IsSpecialName { get; }
		bool IsConstructor { get; }
		Object Invoke(Object obj, Object[] parameters);

		Type ReturnType { get; }
		ICustomAttributeProvider ReturnTypeCustomAttributes { get; }
		MethodInfo GetBaseDefinition();
	}
}
