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

		private static Hashtable w32_errors = new Hashtable();

		/* Initialise the list of error strings */
		static Win32Exception() {
			/* No need to list everything, just the ones
			 * the runtime can throw. A list of the errors
			 * can be found in class System.IO.MonoIOError.
			 */
			w32_errors.Add(2,
				       Locale.GetText("Cannot find the specified file"));
			w32_errors.Add(10004,
				       Locale.GetText("interrupted"));
			w32_errors.Add(10013,
				       Locale.GetText("Access denied"));
			w32_errors.Add(10022,
				       Locale.GetText("Invalid arguments"));
			w32_errors.Add(10035,
				       Locale.GetText("Operation on non-blocking socket would block"));
			w32_errors.Add(10036,
				       Locale.GetText("Operation in progress"));
			w32_errors.Add(10038,
				       Locale.GetText("The descriptor is not a socket"));
			w32_errors.Add(10043,
				       Locale.GetText("proto no supported"));
			w32_errors.Add(10044,
				       Locale.GetText("socket not supproted"));
			w32_errors.Add(10045,
				       Locale.GetText("Operation not supported"));
			w32_errors.Add(10047,
				       Locale.GetText("AF not supported"));
			w32_errors.Add(10048,
				       Locale.GetText("Address already in use"));
			w32_errors.Add(10050,
				       Locale.GetText("Network subsystem is down"));
			w32_errors.Add(10051,
				       Locale.GetText("Network is unreachable"));
			w32_errors.Add(10055,
				       Locale.GetText("Not enough buffer space is available"));
			w32_errors.Add(10056,
				       Locale.GetText("Socket is already connected"));
			w32_errors.Add(10057,
				       Locale.GetText("The socket is not connected"));
			w32_errors.Add(10058,
				       Locale.GetText("The socket has been shut down"));
			w32_errors.Add(10060,
				       Locale.GetText("Connection timed out"));
			w32_errors.Add(10061,
				       Locale.GetText("Connection refused"));
			w32_errors.Add(10065,
				       Locale.GetText("No route to host"));
			w32_errors.Add(10093,
				       Locale.GetText("Winsock not initialized"));
			w32_errors.Add(10107,
				       Locale.GetText("System call failed"));

			w32_errors.Add(11001,
				       Locale.GetText("No such host is known"));
			w32_errors.Add(11002,
				       Locale.GetText("A temporary error occurred on an  authoritative  name  server. Try  again later."));
		}

		private static string W32ErrorMessage(int error_code) {
			string message=(string)w32_errors[error_code];
			
			if(message==null) {
				return(Locale.GetText("Some sort of w32 error occurred: ") + error_code.ToString());
			} else {
				return(message);
			}
		}
	}
}
