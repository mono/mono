//
// System.Diagnostics.ICollectData.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("73386977-D6FD-11D2-BED5-00C04F79E3AE")]
	public interface ICollectData {
		void CloseData ();
		void CollectData (
			[In] int id, 
			[In] IntPtr valueName, 
			[In] IntPtr data, 
			[In] int totalBytes, 
			out IntPtr res);
	}
}
