//
// System.ArgumentOutOfRangeException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	public class ArgumentOutOfRangeException : ArgumentException {
		private object actual_value;

		// Constructors
		public ArgumentOutOfRangeException ()
			: base (Locale.GetText ("Argument is out of range"))
		{
		}

		public ArgumentOutOfRangeException (string param_name)
			: base (Locale.GetText ("Argument is out of range"), param_name)
		{
		}

		public ArgumentOutOfRangeException (string param_name, string message)
			: base (message, param_name)
		{
		}

		public ArgumentOutOfRangeException (string param_name, object actual_value, string message)
			: base (message, param_name)
		{
			this.actual_value = actual_value;
		}

		// Properties
		public virtual object ActualValue {
			get {
				return actual_value;
			}
		}
	}
}
