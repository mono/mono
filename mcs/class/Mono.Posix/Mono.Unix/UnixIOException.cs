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
		public UnixIOException ()
			: this (Marshal.GetLastWin32Error())
		{}
		
		public UnixIOException (int error)
		{
			this.error = error;
		}
		
		public UnixIOException (int error, Exception inner)
			: base ("Unix-generated exception", inner)
		{
			this.error = error;
		}

		public UnixIOException (Error error)
		{
			this.error = UnixConvert.FromError (error);
		}

		public UnixIOException (Error error, Exception inner)
			: base ("Unix-generated exception", inner)
		{
			this.error = UnixConvert.FromError (error);
		}

		protected UnixIOException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public int NativeErrorCode {
			get {return error;}
		}
		
		public Error ErrorCode {
			get {return UnixConvert.ToError (error);}
		}

		private int error;

		public override string ToString ()
		{
			return UnixMarshal.GetErrorDescription (ErrorCode);
		}
	}
}

// vim: noexpandtab
