//
// System.FormatException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class FormatException : SystemException
	{
		// Constructors
		public FormatException ()
			: base (Locale.GetText ("Invalid format."))
		{
		}

		public FormatException (string message)
			: base (message)
		{
		}

		public FormatException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected FormatException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
