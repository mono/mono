//
// System.ArithmeticException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ArithmeticException : SystemException
	{
		const int Result = unchecked ((int)0x80070216);

		// Constructors
		public ArithmeticException ()
			: base (Locale.GetText ("The arithmetic operation is not allowed."))
		{
			HResult = Result;
		}

		public ArithmeticException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public ArithmeticException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected ArithmeticException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
