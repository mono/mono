//
// System.OutOfMemoryException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
namespace System {

	public class OutOfMemoryException : SystemException {
		// Constructors
		public OutOfMemoryException ()
			: base (Locale.GetText ("There is insufficient memory to continue execution"))
		{
		}

		public OutOfMemoryException (string message)
			: base (message)
		{
		}

		public OutOfMemoryException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
