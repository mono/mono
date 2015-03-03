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
	class Color {
		#region	Local Variables
		private int		red;
		private int		green;
		private int		blue;
		private int		num;
		private Color		next;
		#endregion	// Local Variables

		#region Constructors
		public Color(RTF rtf) {
			red = -1;
			green = -1;
			blue = -1;
			num = -1;

			lock (rtf) {
				if (rtf.Colors == null) {
					rtf.Colors = this;
				} else {
					Color c = rtf.Colors;
					while (c.next != null)
						c = c.next;
					c.next = this;
				}
			}
		}
		#endregion	// Constructors

		#region Properties
		public int Red {
			get {
				return red;
			}

			set {
				red = value;
			}
		}

		public int Green {
			get {
				return green;
			}

			set {
				green = value;
			}
		}

		public int Blue {
			get {
				return blue;
			}

			set {
				blue = value;
			}
		}

		public int Num {
			get {
				return num;
			}

			set {
				num = value;
			}
		}
		#endregion	// Properties

		#region Methods
		static public Color GetColor(RTF rtf, int color_number) {
			Color	c;

			lock (rtf) {
				c = GetColor(rtf.Colors, color_number);
			}
			return c;
		}

		static private Color GetColor(Color start, int color_number) {
			Color	c;

			if (color_number == -1) {
				return start;
			}

			c = start;

			while ((c != null) && (c.num != color_number)) {
				c = c.next;
			}
			return c;
		}
		#endregion	// Methods
	}
}
