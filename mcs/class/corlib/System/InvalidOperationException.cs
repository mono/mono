//
// System.InvalidOperationException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
namespace System {

	public class InvalidOperationException : SystemException {
		// Constructors
		public InvalidOperationException ()
			: base (Locale.GetText ("The requested operation cannot be performed"))
		{
		}

		public InvalidOperationException (string message)
			: base (message)
		{
		}

		public InvalidOperationException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
