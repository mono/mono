//
// System.UnauthorizedAccessException.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class UnauthorizedAccessException : SystemException
	{
		// Constructors
		public UnauthorizedAccessException ()
			: base (Locale.GetText ("Access to the requested resource is not authorized."))
		{
		}

		public UnauthorizedAccessException (string message)
			: base (message)
		{
		}

		public UnauthorizedAccessException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected UnauthorizedAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
