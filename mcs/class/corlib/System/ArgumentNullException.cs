//
// System.ArgumentNullException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class ArgumentNullException : ArgumentException {
		// Constructors
		public ArgumentNullException ()
			: base ("Argument cannot be null")
		{
		}

		public ArgumentNullException (string param_name)
			: base ("Argument cannot be null", param_name)
		{
		}

		public ArgumentNullException (string param_name, string message)
			: base (message, param_name)
		{
		}
	}
}