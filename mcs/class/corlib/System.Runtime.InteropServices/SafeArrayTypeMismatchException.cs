//
// System.Runtime.InteropServices.SafeArrayTypeMismatchException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	public class SafeArrayTypeMismatchException : SystemException
	{
		private const int ErrorCode = -2146233037; // = 0x80131533

		public SafeArrayTypeMismatchException ()
			: base (Locale.GetText ("The incoming SAVEARRAY does not match the expected managed signature"))
		{
			this.HResult = ErrorCode;
		}

		public SafeArrayTypeMismatchException (string message)
			: base (message)
		{
			this.HResult = ErrorCode;
		}

		public SafeArrayTypeMismatchException (string message, Exception inner)
			: base (message, inner)
		{
			this.HResult = ErrorCode;
		}

		protected SafeArrayTypeMismatchException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
