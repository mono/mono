//
// System.OverflowExceptionException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class OverflowException : ArithmeticException
	{
		// Constructors
		public OverflowException ()
			: base (Locale.GetText ("Number overflow."))
		{
		}

		public OverflowException (string message)
			: base (message)
		{
		}

		public OverflowException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected OverflowException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
