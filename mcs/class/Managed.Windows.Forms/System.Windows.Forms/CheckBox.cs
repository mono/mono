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
//	Dennis Hayes	dennish@raytek.com
//	Peter Bartok	pbartok@novell.com
//
//
// $Log: CheckBox.cs,v $
// Revision 1.9  2004/10/15 13:25:50  ravindra
// 	- Removed Redraw (), we get it from ButtonBase.
// 	- Implemented Paint (), to do class specific painting.
//
// Revision 1.8  2004/10/13 02:46:22  pbartok
// - Fix from John BouAntoun: Now properly sets the Appearance property
//
// Revision 1.7  2004/09/28 18:44:25  pbartok
// - Streamlined Theme interfaces:
//   * Each DrawXXX method for a control now is passed the object for the
//     control to be drawn in order to allow accessing any state the theme
//     might require
//
//   * ControlPaint methods for the theme now have a CP prefix to avoid
//     name clashes with the Draw methods for controls
//
//   * Every control now retrieves it's DefaultSize from the current theme
//
// Revision 1.6  2004/09/01 20:01:24  pbartok
// - Added missing default
// - Added missing region mark
//
// Revision 1.5  2004/09/01 01:55:58  pbartok
// - Fixed to match the removal of the needs_redraw concept
//
// Revision 1.4  2004/08/31 18:48:31  pbartok
// - Finished (famous last words)
//
// Revision 1.3  2004/08/30 20:42:26  pbartok
// - Implemented CheckBox drawing code
//
// Revision 1.2  2004/08/30 15:44:20  pbartok
// - Updated to fix broken build. Not complete yet.
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms {
	public class CheckBox : ButtonBase {
		#region Local Variables
		internal Appearance		appearance;
		internal bool			auto_check;
		internal ContentAlignment	check_alignment;
		internal ContentAlignment	text_alignment;
		internal CheckState		check_state;
		internal bool			three_state;
		#endregion	// Local Variables


		#region Public Constructors
		public CheckBox() {
			appearance = Appearance.Normal;
			auto_check = true;
			check_alignment = ContentAlignment.MiddleLeft;
			text_alignment = ContentAlignment.MiddleLeft;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Appearance Appearance {
			get {
				return appearance;
			}

			set {
				if (value != appearance) {
					appearance = value;
					if (AppearanceChanged != null) {
						AppearanceChanged(this, EventArgs.Empty);
					}
					Redraw();
				}
			}
		}

		public bool AutoCheck {
			get {
				return auto_check;
			}

			set {
				auto_check = value;
			}
		}

		public ContentAlignment CheckAlign {
			get {
				return check_alignment;
			}

			set {
				if (value != check_alignment) {
					check_alignment = value;

					Redraw();
				}
			}
		}

		public bool Checked {
			get {
				if (check_state != CheckState.Unchecked) {
					return true;
				}
				return false;
			}

			set {
				if (value && (check_state != CheckState.Checked)) {
					check_state = CheckState.Checked;
					Redraw();
					OnCheckedChanged(EventArgs.Empty);
				} else if (!value && (check_state != CheckState.Unchecked)) {
					check_state = CheckState.Unchecked;
					Redraw();
					OnCheckedChanged(EventArgs.Empty);
				}
			}
		}

		public CheckState CheckState {
			get {
				return check_state;
			}

			set {
				if (value != check_state) {
					bool	was_checked = (check_state != CheckState.Unchecked);

					check_state = value;

					if (was_checked != (check_state != CheckState.Unchecked)) {
						OnCheckedChanged(EventArgs.Empty);
					}

					OnCheckStateChanged(EventArgs.Empty);
					Redraw();
				}
			}
		}

		public override ContentAlignment TextAlign {
			get {
				return text_alignment;
			}

			set {
				if (value != text_alignment) {
					text_alignment = value;
					Redraw();
				}
			}
		}


		public bool ThreeState {
			get {
				return three_state;
			}

			set {
				three_state = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size(104, 24);
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public override string ToString() {
			return base.ToString() + ", CheckState: " + (int)check_state;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override AccessibleObject CreateAccessibilityInstance() {
			return base.CreateAccessibilityInstance ();
		}

		protected virtual void OnAppearanceChanged(EventArgs e) {
			if (AppearanceChanged != null) {
				AppearanceChanged(this, e);
			}
		}

		protected virtual void OnCheckedChanged(EventArgs e) {
			if (CheckedChanged != null) {
				CheckedChanged(this, e);
			}
		}

		protected virtual void OnCheckStateChanged(EventArgs e) {
			if (CheckStateChanged != null) {
				CheckStateChanged(this, e);
			}
		}

		protected override void OnClick(EventArgs e) {
			if (auto_check) {
				switch(check_state) {
					case CheckState.Unchecked: {
						if (three_state) {
							CheckState = CheckState.Indeterminate;
						} else {
							CheckState = CheckState.Checked;
						}
						break;
					}

					case CheckState.Indeterminate: {
						CheckState = CheckState.Checked;
						break;
					}

					case CheckState.Checked: {
						CheckState = CheckState.Unchecked;
						break;
					}
				}
			}
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
		}

		protected override bool ProcessMnemonic(char charCode) {
			return base.ProcessMnemonic (charCode);
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler	AppearanceChanged;
		public event EventHandler	CheckedChanged;
		public event EventHandler	CheckStateChanged;
		#endregion	// Events

		#region	Internal drawing code
		internal override void Paint (PaintEventArgs pe) {
			if (redraw) {
				ThemeEngine.Current.DrawCheckBox (this.DeviceContext, this.ClientRectangle, this);
				redraw = false;
			}
			pe.Graphics.DrawImage (this.ImageBuffer, pe.ClipRectangle, pe.ClipRectangle, GraphicsUnit.Pixel);
		}
		#endregion	// Internal drawing code
	}
}
