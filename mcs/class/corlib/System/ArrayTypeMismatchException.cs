//
// System.ArrayTypeMismatchException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//
using System.Globalization;

namespace System {

	public class ArrayTypeMismatchException : SystemException {
		// Constructors
		public ArrayTypeMismatchException ()
			: base (Locale.GetText ("Source array type cannot be assigned to destination array type"))
		{
		}

		public ArrayTypeMismatchException (string message)
			: base (message)
		{
		}

		public ArrayTypeMismatchException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
