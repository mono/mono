//
// System.DuplicateWaitObjectException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class DuplicateWaitObjectException : ArgumentException {
		// Constructors
		public DuplicateWaitObjectException ()
			: base ("Duplicate objects in argument")
		{
		}

		public DuplicateWaitObjectException (string param_name)
			: base ("Duplicate objects in argument", param_name)
		{
		}

		public DuplicateWaitObjectException (string param_name, string message)
			: base (message, param_name)
		{
		}
	}
}