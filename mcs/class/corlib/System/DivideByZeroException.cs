//
// System.DivideByZeroException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class DivideByZeroException : ArithmeticException
	{
		const int Result = unchecked ((int)0x80020012);

		// Constructors
		public DivideByZeroException ()
			: base (Locale.GetText ("Division by zero"))
		{
			HResult = Result;
		}

		public DivideByZeroException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public DivideByZeroException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected DivideByZeroException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
