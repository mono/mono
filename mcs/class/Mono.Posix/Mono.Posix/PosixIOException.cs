//
// Mono.Posix/PosixIOException.cs
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
using Mono.Posix;

namespace Mono.Posix {

	[Serializable]
	public class PosixIOException : IOException
	{
		public PosixIOException ()
			: this (Marshal.GetLastWin32Error())
		{}
		
		public PosixIOException (int error)
		{
			this.error = error;
		}
		
		public PosixIOException (int error, Exception inner)
			: base ("POSIX-generated exception", inner)
		{
			this.error = error;
		}

		public PosixIOException (Error error)
		{
			this.error = PosixConvert.FromError (error);
		}

		public PosixIOException (Error error, Exception inner)
			: base ("POSIX-generated exception", inner)
		{
			this.error = PosixConvert.FromError (error);
		}

		protected PosixIOException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public int NativeErrorCode {
			get {return error;}
		}
		
		public Error ErrorCode {
			get {return PosixConvert.ToError (error);}
		}

		private int error;

		public override string ToString ()
		{
			return PosixMarshal.GetErrorDescription (ErrorCode);
		}
	}
}

// vim: noexpandtab
