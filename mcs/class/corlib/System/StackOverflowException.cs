//
// System.StackOverflowException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	public class StackOverflowException : SystemException {
		// Constructors
		public StackOverflowException ()
			: base (Locale.GetText ("The requested operation caused a stack overflow"))
		{
		}

		public StackOverflowException (string message)
			: base (message)
		{
		}

		public StackOverflowException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected StackOverflowException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
