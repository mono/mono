//
// System.Diagnostics.ICollectData.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("73386977-D6FD-11D2-BED5-00C04F79E3AE")]
	[ComImport]
	public interface ICollectData {
		void CloseData ();
		[return: MarshalAs(UnmanagedType.I4)]
		void CollectData (
			[In] [MarshalAs(UnmanagedType.I4)] int id, 
			[In] [MarshalAs(UnmanagedType.SysInt)] IntPtr valueName, 
			[In] [MarshalAs(UnmanagedType.SysInt)] IntPtr data, 
			[In] [MarshalAs(UnmanagedType.I4)] int totalBytes, 
			[MarshalAs(UnmanagedType.SysInt)] out IntPtr res);
	}
}
