//
// System.Net.Sockets.NetworkStream.cs
//
// Author:
//	Dick Porter <dick@ximian.com>
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Net.Sockets
{
	[Serializable]
	public class SocketException : Win32Exception
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int WSAGetLastError_internal();
		
		public SocketException ()
			: base (WSAGetLastError_internal()) {
		}

		public SocketException (int error)
			: base (error) {
		}

		protected SocketException (SerializationInfo info,
					StreamingContext context)
			: base (info, context) {
		}
		
		public override int ErrorCode {
			get {
				return NativeErrorCode;
			}
		}
	}
}
