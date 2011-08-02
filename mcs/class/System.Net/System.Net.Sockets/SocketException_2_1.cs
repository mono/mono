//
// System.Net.Sockets.SocketException
//
// Author:
//	Dick Porter <dick@ximian.com>
//
// (C) 2002 Ximian, Inc.
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using System.Runtime.CompilerServices;

namespace System.Net.Sockets {

	public class SocketException : Exception {

		int error_code;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern int WSAGetLastError_internal ();

		public SocketException ()
		{
			error_code = WSAGetLastError_internal ();
		}

		public SocketException (int errorCode)
		{
			error_code = errorCode;
		}

		internal SocketException (int error, string message)
			: base (message)
		{
			error_code = error;
		}

		public int ErrorCode {
			get { return error_code; }
		}

		public SocketError SocketErrorCode {
			get { return (SocketError) error_code; }
		}
	}
}
