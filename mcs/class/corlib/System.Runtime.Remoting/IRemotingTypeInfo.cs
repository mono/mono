//
// System.Runtime.Remoting.IRemotingTypeInfo.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting {

	public interface IRemotingTypeInfo
	{
		string TypeName { get; set; }
		bool CanCastTo (Type fromType, object o);
	}
}
