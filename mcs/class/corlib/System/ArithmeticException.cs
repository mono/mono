//
// System.ArithmeticException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	public class ArithmeticException : SystemException {
		// Constructors
		public ArithmeticException ()
			: base (Locale.GetText ("The arithmetic operation is not allowed"))
		{
		}

		public ArithmeticException (string message)
			: base (message)
		{
		}

		public ArithmeticException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
