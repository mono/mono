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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms {
	public class TextBox : TextBoxBase {
		#region Local Variables
		internal bool			accepts_return;
		internal CharacterCasing	character_casing;
		internal char			password_char;
		internal ScrollBars		scrollbars;
		internal HorizontalAlignment	alignment;
		#endregion	// Local Variables

		#region Public Constructors
		public TextBox() {
			accepts_return = false;
			character_casing = CharacterCasing.Normal;
			password_char = '\u25cf';
			scrollbars = ScrollBars.None;
			alignment = HorizontalAlignment.Left;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public bool AcceptsReturn {
			get {
				return accepts_return;
			}

			set {
				if (value != accepts_return) {
					accepts_return = value;
				}	
			}
		}

		public CharacterCasing CharacterCasing {
			get {
				return character_casing;
			}

			set {
				if (value != character_casing) {
					character_casing = value;
				}
			}
		}

		public char PasswordChar {
			get {
				return password_char;
			}

			set {
				if (value != password_char) {
					password_char = value;
				}
			}
		}

		public ScrollBars ScrollBars {
			get {
				return scrollbars;
			}

			set {
				if (value != scrollbars) {
					scrollbars = value;
				}
			}
		}

		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		public HorizontalAlignment TextAlign {
			get {
				return alignment;
			}

			set {
				if (value != alignment) {
					alignment = value;
					OnTextAlignChanged(EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {
				return base.DefaultImeMode;
			}
		}
		#endregion	// Protected Instance Methods

		#region Protected Instance Methods
		protected override bool IsInputKey(Keys keyData) {
			return base.IsInputKey (keyData);
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus (e);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
		}

		protected virtual void OnTextAlignChanged(EventArgs e) {
			if (TextAlignChanged != null) {
				TextAlignChanged(this, e);
			}
		}

		protected virtual void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler TextAlignChanged;
		#endregion	// Events
	}
}
