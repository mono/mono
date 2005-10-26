//
// Mono.Unix/UnixIOException.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
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
		private int error;

		public UnixIOException ()
			: this (Marshal.GetLastWin32Error())
		{}
		
		public UnixIOException (int error)
			: base (UnixMarshal.GetErrorDescription (Native.NativeConvert.ToErrno (error)))
		{
			this.error = error;
		}
		
		public UnixIOException (int error, Exception inner)
			: base (UnixMarshal.GetErrorDescription (Native.NativeConvert.ToErrno (error)), inner)
		{
			this.error = error;
		}

		[Obsolete ("Use UnixIOException (Mono.Unix.Native.Errno) constructor")]
		public UnixIOException (Error error)
			: base (UnixMarshal.GetErrorDescription (error))
		{
			this.error = UnixConvert.FromError (error);
		}

		[Obsolete ("Use UnixIOException (Mono.Unix.Native.Errno, System.Exception) constructor")]
		public UnixIOException (Error error, Exception inner)
			: base (UnixMarshal.GetErrorDescription (error), inner)
		{
			this.error = UnixConvert.FromError (error);
		}

		[CLSCompliant (false)]
		public UnixIOException (Native.Errno error)
			: base (UnixMarshal.GetErrorDescription (error))
		{
			this.error = Native.NativeConvert.FromErrno (error);
		}

		[Obsolete ("Use UnixIOException (Mono.Unix.Native.Errno, System.Exception) constructor")]
		[CLSCompliant (false)]
		public UnixIOException (Native.Errno error, Exception inner)
			: base (UnixMarshal.GetErrorDescription (error), inner)
		{
			this.error = Native.NativeConvert.FromErrno (error);
		}

		public UnixIOException (string message)
			: base (message)
		{
			this.error = 0;
		}

		public UnixIOException (string message, Exception inner)
			: base (message, inner)
		{
			this.error = 0;
		}

		protected UnixIOException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public int NativeErrorCode {
			get {return error;}
		}
		
		[Obsolete ("The type of this property will change in the next release")]
		public Error ErrorCode {
			get {return UnixConvert.ToError (error);}
		}
	}
}

// vim: noexpandtab
