//
// System.Runtime.InteropServices.InvalidComObjectException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	public class InvalidComObjectException : SystemException
	{
		private const int ErrorCode = -2146233049; // = 0x80131527

		public InvalidComObjectException ()
			: base (Locale.GetText ("Invalid COM object is used"))
		{
			this.HResult = ErrorCode;
		}

		public InvalidComObjectException (string message)
			: base (message)
		{
			this.HResult = ErrorCode;
		}

		public InvalidComObjectException (string message, Exception inner)
			: base (message, inner)
		{
			this.HResult = ErrorCode;
		}

		protected InvalidComObjectException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
