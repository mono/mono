//
// System.Runtime.InteropServices.SafeArrayRankMismatchException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	public class SafeArrayRankMismatchException : SystemException
	{
		private const int ErrorCode = -2146233032; // = 0x80131538

		public SafeArrayRankMismatchException ()
			: base (Locale.GetText ("The incoming SAVEARRAY does not match the rank of the expected managed signature"))
		{
			this.HResult = ErrorCode;
		}

		public SafeArrayRankMismatchException (string message)
			: base (message)
		{
			this.HResult = ErrorCode;
		}

		public SafeArrayRankMismatchException (string message, Exception inner)
			: base (message, inner)
		{
			this.HResult = ErrorCode;
		}

		protected SafeArrayRankMismatchException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
