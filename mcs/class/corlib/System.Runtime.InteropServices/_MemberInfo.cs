//
// System.Runtime.InteropServices._MemberInfo interface
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsDual)]
	[Guid ("f7102fa9-cabb-3a74-a6da-b4567ef1b079")]
	public interface _MemberInfo
	{
		bool Equals (object obj);

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
	}
}
#endif