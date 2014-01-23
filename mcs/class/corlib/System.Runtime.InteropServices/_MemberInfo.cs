//
// System.Runtime.InteropServices._MemberInfo interface
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

using System;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("f7102fa9-cabb-3a74-a6da-b4567ef1b079")]
#if !FULL_AOT_RUNTIME
	[TypeLibImportClass (typeof (MemberInfo))]
#endif
	[ComVisible (true)]
	public interface _MemberInfo
	{
		bool Equals (object other);

		object[] GetCustomAttributes (bool inherit);

		object[] GetCustomAttributes (Type attributeType, bool inherit);

		int GetHashCode ();

		Type GetType ();
		
		bool IsDefined (Type attributeType, bool inherit);

		string ToString ();

		Type DeclaringType {get;}

		MemberTypes MemberType {get;}

		string Name {get;}

		Type ReflectedType {get;}

		void GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

		void GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo);

		void GetTypeInfoCount (out uint pcTInfo);

		void Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
	}
}
