//
// System.RankException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class RankException : SystemException {
		// Constructors
		public RankException ()
			: base ("Two arrays must have the same number of dimensions")
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
	}
}