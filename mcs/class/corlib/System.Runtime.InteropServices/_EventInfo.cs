//
// System.Runtime.InteropServices._EventInfo interface
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_1_1

using System;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsDual)]
	[Guid ("9DE59C64-D889-35A1-B897-587D74469E5B")]
	public interface _EventInfo
	{
		void AddEventHandler (object target, Delegate handler);

		bool Equals (object obj);

		MethodInfo GetAddMethod ();

		MethodInfo GetAddMethod (bool nonPublic);

		object[] GetCustomAttributes (bool inherit);

		object[] GetCustomAttributes (Type attributeType, bool inherit);

		int GetHashCode ();

		MethodInfo GetRaiseMethod ();

		MethodInfo GetRaiseMethod (bool nonPublic);

		MethodInfo GetRemoveMethod ();

		MethodInfo GetRemoveMethod (bool nonPublic);

		Type GetType ();

		bool IsDefined (Type attributeType, bool inherit);

		void RemoveEventHandler (object target, Delegate handler);

		string ToString ();

		EventAttributes Attributes {get;}

		Type DeclaringType {get;}

		Type EventHandlerType {get;}

		bool IsMulticast {get;}

		bool IsSpecialName {get;}

		MemberTypes MemberType {get;}

		string Name {get;}

		Type ReflectedType {get;}
	}
}
#endif
