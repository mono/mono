//
// System.InvalidCastException.cs
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
	public class InvalidCastException : SystemException
	{
		// Constructors
		public InvalidCastException ()
			: base (Locale.GetText ("Cannot cast from source type to destination type."))
		{
		}

		public InvalidCastException (string message)
			: base (message)
		{
		}

		public InvalidCastException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected InvalidCastException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
