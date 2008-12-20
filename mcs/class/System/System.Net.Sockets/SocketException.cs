//
// System.Net.Sockets.NetworkStream.cs
//
// Author:
//	Dick Porter <dick@ximian.com>
//
// (C) 2002 Ximian, Inc.
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

using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Net.Sockets
{
	[Serializable]
	public class SocketException : Win32Exception
	{
#if TARGET_JVM
		public SocketException ()
			: base (-2147467259)
#else
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern int WSAGetLastError_internal ();
		public SocketException ()
			: base (WSAGetLastError_internal ())
#endif
		{
		}

		public SocketException (int error)
			: base (error) {
		}

		protected SocketException (SerializationInfo info,
					StreamingContext context)
			: base (info, context) {
		}


		internal SocketException (int error, string message)
			: base (error, message)
		{
		}

		public override int ErrorCode {
			get {
				return NativeErrorCode;
			}
		}

#if NET_2_0
		public SocketError SocketErrorCode {
			get {
				return (SocketError) NativeErrorCode;
			}
		}

		public override string Message {
			get {
				return base.Message;
			}
		}
#endif

	}
}
