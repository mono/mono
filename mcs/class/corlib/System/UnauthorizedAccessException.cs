//
// System.UnauthorizedAccessException.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Duncan Mak  (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class UnauthorizedAccessException : SystemException
	{
		const int Result = unchecked ((int)0x80131500);

		// Constructors
		public UnauthorizedAccessException ()
			: base (Locale.GetText ("Access to the requested resource is not authorized."))
		{
			HResult = Result;
		}

		public UnauthorizedAccessException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public UnauthorizedAccessException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected UnauthorizedAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
