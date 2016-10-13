//
// MonoBtlsError.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// #if SECURITY_DEP
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if MONOTOUCH
using MonoTouch;
#endif

namespace Mono.Btls
{
	static class MonoBtlsError
	{
		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_error_peek_error ();

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_error_get_error ();

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_error_clear_error ();

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_error_get_error_string_n (int error, IntPtr buf, int len);

		public static int PeekError ()
		{
			return mono_btls_error_peek_error ();
		}

		public static int GetError ()
		{
			return mono_btls_error_get_error ();
		}

		public static void ClearError ()
		{
			mono_btls_error_clear_error ();
		}

		public static string GetErrorString (int error)
		{
			var size = 1024;
			var buffer = Marshal.AllocHGlobal (size);
			if (buffer == IntPtr.Zero)
				throw new OutOfMemoryException ();
			try {
				mono_btls_error_get_error_string_n (error, buffer, size);
				return Marshal.PtrToStringAnsi (buffer);
			} finally {
				Marshal.FreeHGlobal (buffer);
			}
		}
	}
}
// #endif
