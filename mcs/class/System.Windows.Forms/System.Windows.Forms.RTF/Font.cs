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
	class Font {
		#region	Local Variables
		private string		name;
		private string		alt_name;
		private int		num;
		private int		family;
		private CharsetType	charset;
		private int		pitch;
		private int		type;
		private int		codepage;
		private Font		next;
		private RTF		rtf;
		#endregion	// Local Variables

		#region Constructors
		public Font(RTF rtf) {
			this.rtf = rtf;
			num = -1;
			name = String.Empty;

			lock (rtf) {
				if (rtf.Fonts == null)
					rtf.Fonts = this;
				else {
					Font f = rtf.Fonts;
					while (f.next != null)
						f = f.next;
					f.next = this;
				}
			}
		}
		#endregion	// Constructors

		#region Properties
		public string Name {
			get {
				return name;
			}

			set {
				name = value;
			}
		}

		public string AltName {
			get {
				return alt_name;
			}

			set {
				alt_name = value;
			}
		}

		public int Num {
			get {
				return num;
			}

			set {
				// Whack any previous font with the same number
				DeleteFont(rtf, value);
				num = value;
			}
		}

		public int Family {
			get {
				return family;
			}

			set {
				family = value;
			}
		}

		public CharsetType Charset {
			get {
				return charset;
			}

			set {
				charset = value;
			}
		}


		public int Pitch {
			get {
				return pitch;
			}

			set {
				pitch = value;
			}
		}

		public int Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public int Codepage {
			get {
				return codepage;
			}

			set {
				codepage = value;
			}
		}
		#endregion	// Properties

		#region Methods
		static public bool DeleteFont(RTF rtf, int font_number) {
			Font	f;
			Font	prev;

			lock (rtf) {
				f = rtf.Fonts;
				prev = null;
				while ((f != null) && (f.num != font_number)) {
					prev = f;
					f = f.next;
				}

				if (f != null) {
					if (f == rtf.Fonts) {
						rtf.Fonts = f.next;
					} else {
						if (prev != null) {
							prev.next = f.next;
						} else {
							rtf.Fonts = f.next;
						}
					}
					return true;
				}
			}
			return false;
		}

		static public Font GetFont(RTF rtf, int font_number) {
			Font	f;

			lock (rtf) {
				f = GetFont(rtf.Fonts, font_number);
			}
			return f;
		}

		static public Font GetFont(Font start, int font_number) {
			Font	f;

			if (font_number == -1) {
				return start;
			}

			f = start;

			while ((f != null) && (f.num != font_number)) {
				f = f.next;
			}
			return f;
		}
		#endregion	// Methods
	}
}
