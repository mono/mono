//
// System.InvalidOperationException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class InvalidOperationException : SystemException
	{
		const int Result = unchecked ((int)0x80131509);

		// Constructors
		public InvalidOperationException ()
			: base (Locale.GetText ("The requested operation could be performed."))
		{
			HResult = Result;
		}

		public InvalidOperationException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public InvalidOperationException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected InvalidOperationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
