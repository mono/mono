//
// System.NotImplementedException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	public class NotImplementedException : SystemException {
		// Constructors
		public NotImplementedException ()
			: base (Locale.GetText ("The requested feature is not yet implemented"))
		{
		}

		public NotImplementedException (string message)
			: base (message)
		{
		}

		public NotImplementedException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
