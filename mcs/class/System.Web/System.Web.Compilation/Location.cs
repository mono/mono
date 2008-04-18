//
// System.Web.Compilation.Location
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Web.Compilation
{
	class Location : ILocation
	{
		int beginLine, endLine, beginColumn, endColumn;
		string fileName, plainText;
		ILocation location;
		
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
			this.location = location;
		}

		public string Filename {
			get { return fileName; }
			set { fileName = value; }
		}

		public int BeginLine {
			get { return beginLine; }
			set { beginLine = value; }
		}

		public int EndLine {
			get { return endLine; }
			set { endLine = value; }
		}

		public int BeginColumn {
			get { return beginColumn; }
			set { beginColumn = value; }
		}

		public int EndColumn {
			get { return endColumn; }
			set { endColumn = value; }
		}

		public string PlainText {
			get { return plainText; }
			set { plainText = value; }
		}

		public string FileText {
			get {
				if (location != null)
					return location.FileText;

				return null;
			}
		}
	}
}

