//
// System.Net.Sockets.NetworkStream.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc.
//
using System;
using System.Runtime.Serialization;
using System.Globalization;
using System.ComponentModel;

namespace System.Net.Sockets
{
	public class SocketException : Win32Exception {
		// Constructors
		public SocketException ()
			: base (Locale.GetText ("Socket exception"))
		{
		}

		public SocketException (int code)
			: base (Locale.GetText ("Socket exception"), code)
		{
		}
		
		protected SocketException(SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}
		
		public override int ErrorCode {
			get {
				return NativeErrorCode;
			}
		}
	}
}
