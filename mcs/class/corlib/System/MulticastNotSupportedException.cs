//
// System.MulticastNotSupportedException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[Serializable]
	public sealed class MulticastNotSupportedException : SystemException
	{
		// Constructors
		public MulticastNotSupportedException ()
			: base (Locale.GetText ("This operation cannot be performed with the specified delagates."))
		{
		}

		public MulticastNotSupportedException (string message)
			: base (message)
		{
		}

		public MulticastNotSupportedException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
