//
// System.OverflowExceptionException.cs
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
	public class OverflowException : ArithmeticException
	{
		const int Result = unchecked ((int)0x80131516);

		// Constructors
		public OverflowException ()
			: base (Locale.GetText ("Number overflow."))
		{
			HResult = Result;
		}

		public OverflowException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public OverflowException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected OverflowException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
