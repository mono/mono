//
// System.IO.FileLoadException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.IO {

	public class FileLoadException : SystemException {
		// Constructors
		public FileLoadException ()
			: base ("I/O Error")
		{
		}

		public FileLoadException (string message)
			: base (message)
		{
		}

		public FileLoadException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
