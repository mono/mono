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

using System.Drawing;

namespace System.Windows.Forms {
	public sealed class AmbientProperties {
		#region Local Variables
		private Color	fore_color;
		private Color	back_color;
		private Font	font;
		private Cursor	cursor;
		#endregion	// Local Variables

		#region Public Constructors
		public AmbientProperties() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Color BackColor {
			get {
				return back_color;
			}

			set {
				back_color = value;
			}
		}

		public Cursor Cursor {
			get {
				return cursor;
			}

			set {
				cursor = value;
			}
		}

		public Font Font {
			get {
				return font;
			}

			set {
				font = value;
			}
		}

		public Color ForeColor {
			get {
				return fore_color;
			}

			set {
				fore_color = value;
			}
		}
		#endregion	// Public Instance Properties
	}
}
