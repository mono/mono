//
// System.Windows.Forms.FileDialogCustomPlace.cs
//
//
// Copyright (C) 2007 Novell (http://www.novell.com)
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


using System;

// Net 2.0 SP1 class

namespace System.Windows.Forms
{
	public class FileDialogCustomPlace
	{
		private string path;
		private Guid guid;
		
		public FileDialogCustomPlace (string path)
		{
			this.path = path;
			this.guid = Guid.Empty;
		}

		public FileDialogCustomPlace (Guid knownFolderGuid)
		{
			this.path = string.Empty;
			this.guid = knownFolderGuid;
		}

		public string Path {
			get { return path; }
			set {
				path = value;
				guid = Guid.Empty;
			}
		}

		public Guid KnownFolderGuid {
			get { return guid; }
			set {
				guid = value;
				path = string.Empty;
			}
		}

		public override string ToString ()
		{
			return string.Format ("{0} Path: {1} KnownFolderGuid: {2}", GetType ().ToString (), path, guid);
		}
	}
}
