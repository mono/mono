//
// System.ComponentModel.Win32Exception.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Collections;
using System.Globalization;

namespace System.ComponentModel
{
	[Serializable, SuppressUnmanagedCodeSecurity]
	public class Win32Exception : ExternalException
	{
		private int native_error_code;
		
		public Win32Exception ()
			: base (W32ErrorMessage(Marshal.GetLastWin32Error()),
				Marshal.GetLastWin32Error()) {
			native_error_code=Marshal.GetLastWin32Error();
		}

		public Win32Exception(int error)
			: base (W32ErrorMessage(error), error) {
			native_error_code=error;
		}

		public Win32Exception(int error, string message) 
			: base (message, error) {
			native_error_code=error;
		}

		protected Win32Exception(SerializationInfo info,
					 StreamingContext context)
			: base (info, context) {
		}

		public int NativeErrorCode {
			get {
				return(native_error_code);
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info==null)
				throw new ArgumentNullException ("info");

			info.AddValue ("native_error_code", native_error_code);
			base.GetObjectData (info, context);
		}

		static string W32ErrorMessage (int error_code)
		{
			string message;

			switch (error_code) {
			case 2:
				message = Locale.GetText ("Cannot find the specified file");
				break;
			case 3:
				message = Locale.GetText ("Cannot find the specified file");
				break;
			case 10004:
				message = Locale.GetText ("interrupted");
				break;
			case 10013:
				message = Locale.GetText ("Access denied");
				break;
			case 10022:
				message = Locale.GetText ("Invalid arguments");
				break;
			case 10035:
				message = Locale.GetText ("Operation on non-blocking socket would block");
				break;
			case 10036:
				message = Locale.GetText ("Operation in progress");
				break;
			case 10038:
				message = Locale.GetText ("The descriptor is not a socket");
				break;
			case 10040:
				message = Locale.GetText ("Message too long");
				break;
			case 10043:
				message = Locale.GetText ("proto no supported");
				break;
			case 10044:
				message = Locale.GetText ("socket not supproted");
				break;
			case 10045:
				message = Locale.GetText ("Operation not supported");
				break;
			case 10047:
				message = Locale.GetText ("AF not supported");
				break;
			case 10048:
				message = Locale.GetText ("Address already in use");
				break;
			case 10050:
				message = Locale.GetText ("Network subsystem is down");
				break;
			case 10051:
				message = Locale.GetText ("Network is unreachable");
				break;
			case 10054:
				message = Locale.GetText ("Connection reset by peer");
				break;
			case 10055:
				message = Locale.GetText ("Not enough buffer space is available");
				break;
			case 10056:
				message = Locale.GetText ("Socket is already connected");
				break;
			case 10057:
				message = Locale.GetText ("The socket is not connected");
				break;
			case 10058:
				message = Locale.GetText ("The socket has been shut down");
				break;
			case 10060:
				message = Locale.GetText ("Connection timed out");
				break;
			case 10061:
				message = Locale.GetText ("Connection refused");
				break;
			case 10065:
				message = Locale.GetText ("No route to host");
				break;
			case 10093:
				message = Locale.GetText ("Winsock not initialized");
				break;
			case 10107:
				message = Locale.GetText ("System call failed");
				break;
			case 11001:
				message = Locale.GetText ("No such host is known");
				break;
			case 11002:
				message = Locale.GetText ("A temporary error occurred on an " +
							  "authoritative name server. Try  again later.");
				break;
			default:
				message = Locale.GetText ("Some sort of w32 error occurred: ") + error_code;
				break;
			}

			return message;
		}
	}
}
