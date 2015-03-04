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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

namespace System.Windows.Forms.RTF {

#if RTF_LIB
	public
#else
	internal
#endif
	class StyleElement {
		#region Local Variables
		private TokenClass	token_class;
		private Major		major;
		private Minor		minor;
		private int		param;
		private string		text;
		private StyleElement	next;
		#endregion Local Variables

		#region Constructors
		public StyleElement(Style s, TokenClass token_class, Major major, Minor minor, int param, string text) {
			this.token_class = token_class;
			this.major = major;
			this.minor = minor;
			this.param = param;
			this.text = text;

			lock (s) {
				if (s.Elements == null) {
					s.Elements = this;
				} else {
					StyleElement se = s.Elements;
					while (se.next != null)
						se = se.next;
					se.next = this;
				}
			}
		}
		#endregion	// Constructors

		#region Properties
		public TokenClass TokenClass {
			get {
				return token_class;
			}

			set {
				token_class = value;
			}
		}

		public Major Major {
			get {
				return major;
			}

			set {
				major = value;
			}
		}

		public Minor Minor {
			get {
				return minor;
			}

			set {
				minor = value;
			}
		}

		public int Param {
			get {
				return param;
			}

			set {
				param = value;
			}
		}

		public string Text {
			get {
				return text;
			}

			set {
				text = value;
			}
		}
		#endregion	// Properties

		#region	Methods
		#endregion	// Methods
	}
}
