//
// System.ApplicationException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class ApplicationException : Exception {
		// Constructors
		public ApplicationException ()
			: base ("An application exception has occurred.")
		{
		}

		public ApplicationException (string message)
			: base (message)
		{
		}

		public ApplicationException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
