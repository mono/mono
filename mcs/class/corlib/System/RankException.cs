//
// System.RankException.cs
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
	public class RankException : SystemException
	{
		// Constructors
		public RankException ()
			: base (Locale.GetText ("Two arrays must have the same number of dimensions."))
		{
		}

		public RankException (string message)
			: base (message)
		{
		}

		public RankException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected RankException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
