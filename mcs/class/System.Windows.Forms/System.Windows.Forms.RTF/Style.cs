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
	class Style {
		#region Local Variables
		public const int	NoStyleNum = 222;
		public const int	NormalStyleNum = 0;

		private string		name;
		private StyleType	type;
		private bool		additive;
		private int		num;
		private int		based_on;
		private int		next_par;
		private bool		expanding;
		private StyleElement	elements;
		private Style		next;
		#endregion Local Variables

		#region Constructors
		public Style(RTF rtf) {
			num = -1;
			type = StyleType.Paragraph;
			based_on = NoStyleNum;
			next_par = -1;

			lock (rtf) {
				if (rtf.Styles == null) {
					rtf.Styles = this;
				} else {
					Style s = rtf.Styles;
					while (s.next != null)
						s = s.next;
					s.next = this;
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

		public StyleType Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public bool Additive {
			get {
				return additive;
			}

			set {
				additive = value;
			}
		}

		public int BasedOn {
			get {
				return based_on;
			}

			set {
				based_on = value;
			}
		}

		public StyleElement Elements {
			get {
				return elements;
			}

			set {
				elements = value;
			}
		}

		public bool Expanding {
			get {
				return expanding;
			}

			set {
				expanding = value;
			}
		}

		public int NextPar {
			get {
				return next_par;
			}

			set {
				next_par = value;
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
		public void Expand(RTF rtf) {
			StyleElement	se;

			if (num == -1) {
				return;
			}

			if (expanding) {
				throw new Exception("Recursive style expansion");
			}
			expanding = true;

			if (num != based_on) {
				rtf.SetToken(TokenClass.Control, Major.ParAttr, Minor.StyleNum, based_on, "\\s");
				rtf.RouteToken();
			}

			se = elements;
			while (se != null) {
				rtf.TokenClass = se.TokenClass;
				rtf.Major = se.Major;
				rtf.Minor = se.Minor;
				rtf.Param = se.Param;
				rtf.Text = se.Text;
				rtf.RouteToken();
			}

			expanding = false;
		}

		static public Style GetStyle(RTF rtf, int style_number) {
			Style s;

			lock (rtf) {
				s = GetStyle(rtf.Styles, style_number);
			}
			return s;
		}

		static public Style GetStyle(Style start, int style_number) {
			Style	s;

			if (style_number == -1) {
				return start;
			}

			s = start;

			while ((s != null) && (s.num != style_number)) {
				s = s.next;
			}
			return s;
		}
		#endregion	// Methods
	}
}
