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

namespace System.Diagnostics {
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ICollectData {
		void CloseData ();
		void CollectData (
			int id, 
			IntPtr valueName, 
			IntPtr data, 
			int totalBytes, 
			out IntPtr res);
	}
}

