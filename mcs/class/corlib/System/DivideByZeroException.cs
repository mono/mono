//
// System.DivideByZeroException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class DivideByZeroException : ArithmeticException {
		// Constructors
		public DivideByZeroException ()
			: base ("Division by zero")
		{
		}

		public DivideByZeroException (string message)
			: base (message)
		{
		}

		public DivideByZeroException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}