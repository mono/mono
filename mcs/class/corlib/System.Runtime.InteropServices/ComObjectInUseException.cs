//
// System.Runtime.InteropServices.ComObjectInUseException
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	[Serializable]
	public class ComObjectInUseException : SystemException
	{
		private const int ErrorCode = -2146233046;

		public ComObjectInUseException () : base ()
		{
			HResult = ErrorCode;
		}

		public ComObjectInUseException (string message) : base (message)
		{
			HResult = ErrorCode;
		}

		protected ComObjectInUseException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
		}

		public ComObjectInUseException (string message, Exception inner) : base (message, inner)
		{
			HResult = ErrorCode;
		}
	}
}
#endif