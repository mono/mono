//
// System.ComponentModel.Win32Exception.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
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
			case 10024:
				message = Locale.GetText ("Too many open files");
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
			case 10042:
				message = Locale.GetText ("Protocol option not supported");
				break;
			case 10043:
				message = Locale.GetText ("Protocol not supported");
				break;
			case 10044:
				message = Locale.GetText ("Socket not supported");
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
