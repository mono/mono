//
// System.IndexOutOfRangeException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public sealed class IndexOutOfRangeException : SystemException {
		// Constructors
		public IndexOutOfRangeException ()
			: base ("Array index is out of range")
		{
		}

		public IndexOutOfRangeException (string message)
			: base (message)
		{
		}

		public IndexOutOfRangeException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}