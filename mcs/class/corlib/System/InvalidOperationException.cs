//
// System.InvalidOperationException.cs
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
	public class InvalidOperationException : SystemException
	{
		// Constructors
		public InvalidOperationException ()
			: base (Locale.GetText ("The requested operation could be performed."))
		{
		}

		public InvalidOperationException (string message)
			: base (message)
		{
		}

		public InvalidOperationException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected InvalidOperationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
