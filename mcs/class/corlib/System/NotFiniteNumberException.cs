//
// System.NotFiniteNumberException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class NotFiniteNumberException : ArithmeticException
	{
		const int Result = unchecked ((int)0x80131528);

		double offending_number;

		// Constructors
		public NotFiniteNumberException ()
			: base (Locale.GetText ("The number encountered was not a finite quantity."))
		{
		}

		public NotFiniteNumberException (double offending_number)
		{
			this.offending_number = offending_number;
			HResult = Result;
		}

		public NotFiniteNumberException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public NotFiniteNumberException (string message, double offending_number)
			: base (message)
		{
			this.offending_number = offending_number;
			HResult = Result;
		}

		public NotFiniteNumberException (string message, double offending_number, Exception inner)
			: base (message, inner)
		{
			this.offending_number = offending_number;
			HResult = Result;
		}

		protected NotFiniteNumberException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			offending_number = info.GetDouble ("OffendingNumber");
		}

		// Properties
		public double OffendingNumber {
			get {
				return offending_number;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("OffendingNumber", offending_number);
		}
	}
}
