//
// System.Runtime.InteropServices.ExternalException.cs
//
// Author:
//   Miguel De Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Globalization;

namespace System.Runtime.InteropServices {
	[Serializable]
	public class ExternalException : SystemException {
		// Constructors
		public ExternalException ()
			: base (Locale.GetText ("External exception"))
		{
		}

		public ExternalException (string message)
			: base (message)
		{
		}

		protected ExternalException(SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}
		
		public ExternalException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public ExternalException (string message, int errorCode)
			: base (message)
		{
			HResult = errorCode;
		}

		public virtual int ErrorCode {
			get {
				return HResult;
			}
		}
	}
}
