//
// System.IO.IOException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.IO {

	public class IOException : SystemException {
		// Constructors
		public IOException ()
			: base ("I/O Error")
		{
		}

		public IOException (string message)
			: base (message)
		{
		}

		public IOException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
