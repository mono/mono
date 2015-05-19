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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Jonathan Pobst		monkey@jpobst.com
//

namespace System.Windows.Forms
{
	public sealed class HtmlElementErrorEventArgs : EventArgs
	{
		#region Fields
		private string description;
		private bool handled;
		private int line_number;
		private Uri url;
		#endregion

		#region Internal Constructor
		internal HtmlElementErrorEventArgs (string description, int lineNumber, Uri url)
		{
			this.description = description;
			this.line_number = lineNumber;
			this.url = url;
		}
		#endregion

		#region Public Properties
		public string Description {
			get { return this.description; }
		}
		
		public bool Handled {
			get { return this.handled; }
			set { this.handled = value; }
		}
		
		public int LineNumber {
			get { return this.line_number; }
		}
		
		public Uri Url {
			get { return this.url; }
		}
		#endregion
	}
}
