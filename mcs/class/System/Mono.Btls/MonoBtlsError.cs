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
#if SECURITY_DEP && MONO_FEATURE_BTLS
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
		[DllImport (MonoBtlsObject.BTLS_DYLIB)]
		extern static int mono_btls_error_peek_error ();

		[DllImport (MonoBtlsObject.BTLS_DYLIB)]
		extern static int mono_btls_error_get_error ();

		[DllImport (MonoBtlsObject.BTLS_DYLIB)]
		extern static void mono_btls_error_clear_error ();

		[DllImport (MonoBtlsObject.BTLS_DYLIB)]
		extern static int mono_btls_error_peek_error_line (out IntPtr file, out int line);

		[DllImport (MonoBtlsObject.BTLS_DYLIB)]
		extern static int mono_btls_error_get_error_line (out IntPtr file, out int line);

		[DllImport (MonoBtlsObject.BTLS_DYLIB)]
		extern static void mono_btls_error_get_error_string_n (int error, IntPtr buf, int len);

		[DllImport (MonoBtlsObject.BTLS_DYLIB)]
		extern static int mono_btls_error_get_reason (int error);

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

		public static int PeekError (out string file, out int line)
		{
			IntPtr filePtr;
			var error = mono_btls_error_peek_error_line (out filePtr, out line);
			if (filePtr != IntPtr.Zero)
				file = Marshal.PtrToStringAnsi (filePtr);
			else
				file = null;
			return error;
		}

		public static int GetError (out string file, out int line)
		{
			IntPtr filePtr;
			var error = mono_btls_error_get_error_line (out filePtr, out line);
			if (filePtr != IntPtr.Zero)
				file = Marshal.PtrToStringAnsi (filePtr);
			else
				file = null;
			return error;
		}

		public static int GetErrorReason (int error)
		{
			return mono_btls_error_get_reason (error);
		}
	}
}
#endif
