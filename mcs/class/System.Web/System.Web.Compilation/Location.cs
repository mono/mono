//
// System.Web.Compilation.Location
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web.Compilation
{
	class Location : ILocation
	{
		int beginLine, endLine, beginColumn, endColumn;
		string fileName, plainText;

		public Location (ILocation location)
		{
			Init (location);
		}

		public void Init (ILocation location)
		{
			if (location == null) {
				beginLine = 0;
				endLine = 0;
				beginColumn = 0;
				endColumn = 0;
				fileName = null;
				plainText = null;
			} else {
				beginLine = location.BeginLine;
				endLine = location.EndLine;
				beginColumn = location.BeginColumn;
				endColumn = location.EndColumn;
				fileName = location.Filename;
				plainText = location.PlainText;
			}
		}

		public string Filename {
			get { return fileName; }
		}

		public int BeginLine {
			get { return beginLine; }
		}

		public int EndLine {
			get { return endLine; }
		}

		public int BeginColumn {
			get { return beginColumn; }
		}

		public int EndColumn {
			get { return endColumn; }
		}

		public string PlainText {
			get { return plainText; }
		}
	}
}

