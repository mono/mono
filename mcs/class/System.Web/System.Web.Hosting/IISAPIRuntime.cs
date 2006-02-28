//
// System.Web.Hosting.IISAPIRuntime.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
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
using System.Runtime.InteropServices;

namespace System.Web.Hosting
{
	//[Guid ("c4918956-485b-3503-bd10-9083e3f6b66c")] -> 1.1 pre service pack
	[Guid ("08A2C56F-7C16-41C1-A8BE-432917A1A2D1")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImportAttribute]
	public interface IISAPIRuntime
	{
		void DoGCCollect ();
		[return: MarshalAs (UnmanagedType.I4)]
		int ProcessRequest ([In] IntPtr ecb, [In, MarshalAs(UnmanagedType.I4)] int useProcessModel);
		void StartProcessing ();
		void StopProcessing ();
	}
}
