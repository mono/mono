//
// System.OverflowExceptionException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class OverflowExceptionException : ArithmeticException {
		// Constructors
		public OverflowExceptionException ()
			: base ("Number overflow")
		{
		}

		public OverflowExceptionException (string message)
			: base (message)
		{
		}

		public OverflowExceptionException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}