//
// System.SystemException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class SystemException : Exception {
		// Constructors
		public SystemException ()
			: base ("A system exception has occurred.");
		{
		}

		public SystemException (string message)
			: base (message)
		{
		}

		public SystemException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}