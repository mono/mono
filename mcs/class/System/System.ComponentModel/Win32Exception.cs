//
// System.ComponentModel.cs
//
// Author:
//   Miguel de Icaza  (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Globalization;

namespace System.ComponentModel {

	public class Win32Exception : ExternalException {
		int native_error_code;
		
		// Constructors
		public Win32Exception ()
			: base (Locale.GetText ("Win32 exception"))
		{
		}

		public Win32Exception (string message)
			: base (message)
		{
		}

		protected Win32Exception(SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}
		
		public Win32Exception (string message, Exception inner)
			: base (message, inner)
		{
		}

		public Win32Exception (string message, int errorCode)
		{
			native_error_code = errorCode;
		}

		public int NativeErrorCode {
			get {
				return native_error_code;
			}
		}
	}
}
