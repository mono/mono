//
// System.ArithmeticException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ArithmeticException : SystemException
	{
		// Constructors
		public ArithmeticException ()
			: base (Locale.GetText ("The arithmetic operation is not allowed."))
		{
		}

		public ArithmeticException (string message)
			: base (message)
		{
		}

		public ArithmeticException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected ArithmeticException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
