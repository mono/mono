//
// System.ComponentModel.Win32Exceptioncs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Collections;
using System.Globalization;

namespace System.ComponentModel
{
	[Serializable]
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

		[MonoTODO]
		public override void GetObjectData(SerializationInfo info,
						   StreamingContext context) {
			if(info==null) {
				throw new ArgumentNullException();
			}

			throw new NotImplementedException();
		}

		private static Hashtable w32_errors = new Hashtable();

		/* Initialise the list of error strings */
		static Win32Exception() {
			/* No need to list everything, just the ones
			 * the runtime can throw
			 */
			w32_errors.Add(10047,
				       Locale.GetText("AF not supported"));
			w32_errors.Add(10043,
				       Locale.GetText("proto no supported"));
			w32_errors.Add(10044,
				       Locale.GetText("socket not supproted"));
		}

		private static string W32ErrorMessage(int error_code) {
			string message=(string)w32_errors[error_code];
			
			if(message==null) {
				return(Locale.GetText("Some sort of w32 error occurred"));
			} else {
				return(message);
			}
		}
	}
}
