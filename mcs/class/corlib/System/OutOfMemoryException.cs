//
// System.OutOfMemoryException.cs
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
	public class OutOfMemoryException : SystemException
	{
		// Constructors
		public OutOfMemoryException ()
			: base (Locale.GetText ("There is insufficient memory to continue execution."))
		{
		}

		public OutOfMemoryException (string message)
			: base (message)
		{
		}

		public OutOfMemoryException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected OutOfMemoryException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
