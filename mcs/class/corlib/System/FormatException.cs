//
// System.FormatException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class FormatException : SystemException {
		// Constructors
		public FormatException ()
			: base ("Invalid format")
		{
		}

		public FormatException (string message)
			: base (message)
		{
		}

		public FormatException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}