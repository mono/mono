//
// System.NotFiniteNumberException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class NotFiniteNumberException : ArithmeticException {
		double offending_number;

		// Constructors
		public NotFiniteNumberException ()
			: base ("The number encountered was not a finite quantity")
		{
		}

		public NotFiniteNumberException (double offending_number)
		{
			this.offending_number = offending_number;
		}

		public NotFiniteNumberException (string message)
			: base (message)
		{
		}

		public NotFiniteNumberException (string message, double offending_number)
		{
			this.offending_number = offending_number;
		}

		public NotFiniteNumberException (string message, double offending_number, Exception inner)
			: base (message, inner)
		{
			this.offending_number = offending_number;
		}

		// Properties
		public virtual double OffendingNumber {
			get {
				return offending_number;
			}
		}
	}
}