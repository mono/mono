//
// System.IO.FileNotFoundException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.IO {

	public class FileNotFoundException : SystemException {
		private string _fileName;

		// Constructors
		public FileNotFoundException ()
			: base ("File not found")
		{
		}

		public FileNotFoundException (string message)
			: base (message)
		{
		}

		public FileNotFoundException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public FileNotFoundException (string message, string fileName)
			: base (message)
		{
			_fileName = fileName;
		}

		public string FileName {
			get {
				return _fileName;
			}
		}

		[MonoTODO]
		public string FusionLog {
			get {
				// FIXME
				return null;
			}
		}
	}
}
