//
// System.StackOverflowException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class StackOverflowException : SystemException {
		// Constructors
		public StackOverflowException ()
			: base ("The requested operation caused a stack overflow")
		{
		}

		public StackOverflowException (string message)
			: base (message)
		{
		}

		public StackOverflowException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}