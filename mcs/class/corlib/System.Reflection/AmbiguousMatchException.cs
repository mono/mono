//
// System.Reflection.AmbiguousMatchException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Reflection {

	public sealed class AmbiguousMatchException : SystemException {
		// Constructors
		public AmbiguousMatchException ()
			: base ("Ambiguous matching in method resolution")
		{
		}

		public AmbiguousMatchException (string message)
			: base (message)
		{
		}

		public AmbiguousMatchException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
