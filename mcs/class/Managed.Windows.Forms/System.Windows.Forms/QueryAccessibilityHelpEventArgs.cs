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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

namespace System.Windows.Forms {
	public class QueryAccessibilityHelpEventArgs : EventArgs {
		private string	help_namespace;
		private string	help_string;
		private string	help_keyword;

		#region Public Constructors
		public QueryAccessibilityHelpEventArgs() {
			this.help_namespace = null;
			this.help_string = null;
			this.help_keyword = null;
		}

		public QueryAccessibilityHelpEventArgs(string helpNamespace, string helpString, string helpKeyword) {
			this.help_namespace=helpNamespace;
			this.help_string=helpString;
			this.help_keyword=helpKeyword;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public string HelpKeyword {
			get  {
				return this.help_keyword;
			}

			set {
				this.help_keyword = value;
			}
		}

		public string HelpNamespace {
			get {
				return this.help_namespace;
			}

			set {
				this.help_namespace = value;
			}
		}

		public string HelpString {
			get {
				return this.help_string;
			}

			set {
				this.help_string = value;
			}
		}
		#endregion	// Public Instance Properties
	}
}
