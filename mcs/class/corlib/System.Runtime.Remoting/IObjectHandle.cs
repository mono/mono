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
	[Guid("C460E2B4-E199-412a-8456-84DC3E4838C3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IObjectHandle
	{
		object Unwrap ();
	}
	
}

