//
// System.IndexOutOfRangeException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
namespace System {

	[Serializable]
	public sealed class IndexOutOfRangeException : SystemException {
		// Constructors
		public IndexOutOfRangeException ()
			: base (Locale.GetText ("Array index is out of range"))
		{
		}

		public IndexOutOfRangeException (string message)
			: base (message)
		{
		}

		public IndexOutOfRangeException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
