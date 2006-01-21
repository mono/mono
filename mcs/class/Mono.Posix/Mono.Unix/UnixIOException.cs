//
// Mono.Unix/UnixIOException.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Mono.Unix;

namespace Mono.Unix {

	[Serializable]
	public class UnixIOException : IOException
	{
		private int errno;

		public UnixIOException ()
			: this (Marshal.GetLastWin32Error())
		{}
		
		public UnixIOException (int errno)
			: base (GetMessage (Native.NativeConvert.ToErrno (errno)))
		{
			this.errno = errno;
		}
		
		public UnixIOException (int errno, Exception inner)
			: base (GetMessage (Native.NativeConvert.ToErrno (errno)), inner)
		{
			this.errno = errno;
		}

		public UnixIOException (Native.Errno errno)
			: base (GetMessage (errno))
		{
			this.errno = Native.NativeConvert.FromErrno (errno);
		}

		public UnixIOException (Native.Errno errno, Exception inner)
			: base (GetMessage (errno), inner)
		{
			this.errno = Native.NativeConvert.FromErrno (errno);
		}

		public UnixIOException (string message)
			: base (message)
		{
			this.errno = 0;
		}

		public UnixIOException (string message, Exception inner)
			: base (message, inner)
		{
			this.errno = 0;
		}

		protected UnixIOException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public int NativeErrorCode {
			get {return errno;}
		}
		
		public Native.Errno ErrorCode {
			get {return Native.NativeConvert.ToErrno (errno);}
		}

		private static string GetMessage (Native.Errno errno)
		{
			return string.Format ("{0} [{1}].",
					UnixMarshal.GetErrorDescription (errno),
					errno);
		}
	}
}

// vim: noexpandtab
