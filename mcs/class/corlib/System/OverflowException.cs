//
// System.OverflowExceptionException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class OverflowException : ArithmeticException {
		// Constructors
		public OverflowException ()
			: base ("Number overflow")
		{
		}

		public OverflowException (string message)
			: base (message)
		{
		}

		public OverflowException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
