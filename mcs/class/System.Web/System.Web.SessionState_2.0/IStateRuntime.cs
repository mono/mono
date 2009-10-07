//
// System.Web.SessionState.IStateRuntime.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
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

namespace System.Web.SessionState
{
	[Guid ("7297744b-e188-40bf-b7e9-56698d25cf44")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
#if NET_2_0
	[ComImportAttribute]
#endif
	public interface IStateRuntime
	{
		void ProcessRequest (
			[In, MarshalAs(UnmanagedType.SysInt)] IntPtr tracker, 
			[In, MarshalAs(UnmanagedType.I4)] int verb,
			[In, MarshalAs(UnmanagedType.LPWStr)] string uri,
			[In, MarshalAs(UnmanagedType.I4)] int exclusive, 
			[In, MarshalAs(UnmanagedType.I4)] int timeout,
			[In, MarshalAs(UnmanagedType.I4)] int lockCookieExists,
			[In, MarshalAs(UnmanagedType.I4)] int lockCookie,
			[In, MarshalAs(UnmanagedType.I4)] int contentLength,
			[In, MarshalAs(UnmanagedType.SysInt)] IntPtr content);

#if NET_2_0
		void ProcessRequest (
			[In, MarshalAs(UnmanagedType.SysInt)] IntPtr tracker, 
			[In, MarshalAs(UnmanagedType.I4)] int verb,
			[In, MarshalAs(UnmanagedType.LPWStr)] string uri,
			[In, MarshalAs(UnmanagedType.I4)] int exclusive, 
			[In, MarshalAs(UnmanagedType.I4)] int extraFlags, 
			[In, MarshalAs(UnmanagedType.I4)] int timeout,
			[In, MarshalAs(UnmanagedType.I4)] int lockCookieExists,
			[In, MarshalAs(UnmanagedType.I4)] int lockCookie,
			[In, MarshalAs(UnmanagedType.I4)] int contentLength,
			[In, MarshalAs(UnmanagedType.SysInt)] IntPtr content);
#endif
		void StopProcessing ();
	}
}
