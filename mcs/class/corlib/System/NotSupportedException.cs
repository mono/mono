//
// System.NotSupportedException.cs
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
	public class NotSupportedException : SystemException
	{
		// Constructors
		public NotSupportedException ()
			: base (Locale.GetText ("Operation is not supported."))
		{
		}

		public NotSupportedException (string message)
			: base (message)
		{
		}

		public NotSupportedException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected NotSupportedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
