//
// System.InvalidCastException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
namespace System {

	public class InvalidCastException : SystemException {
		// Constructors
		public InvalidCastException ()
			: base (Locale.GetText ("Cannot cast from source type to destination type"))
		{
		}

		public InvalidCastException (string message)
			: base (message)
		{
		}

		public InvalidCastException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
