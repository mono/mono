//
// System.Runtime.Remoting.IObjectHandle.cs
//
// Authors:
//	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;

namespace System.Runtime.Remoting
{
	[Guid("")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IObjectHandle
	{
		object Unwrap ();
	}
	
}

