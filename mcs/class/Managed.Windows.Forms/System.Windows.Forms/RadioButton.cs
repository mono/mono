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

using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
	public class RadioButton : ButtonBase {
		#region Local Variables
		internal Appearance		appearance;
		internal bool			auto_check;
		internal ContentAlignment	radiobutton_alignment;
		internal ContentAlignment	text_alignment;
		internal CheckState		check_state;
		private bool			is_tabstop;
		#endregion	// Local Variables

		#region RadioButtonAccessibleObject Subclass
		public class RadioButtonAccessibleObject : ControlAccessibleObject {
			#region RadioButtonAccessibleObject Local Variables
			private RadioButton	owner;
			#endregion	// RadioButtonAccessibleObject Local Variables

			#region RadioButtonAccessibleObject Constructors
			public RadioButtonAccessibleObject(RadioButton owner) : base(owner) {
				this.owner = owner;
			}
			#endregion	// RadioButtonAccessibleObject Constructors

			#region RadioButtonAccessibleObject Properties
			public override string DefaultAction {
				get {
					return "Select";
				}
			}

			public override AccessibleRole Role {
				get {
					return AccessibleRole.RadioButton;
				}
			}

			public override AccessibleStates State {
				get {
					AccessibleStates	retval;

					retval = AccessibleStates.Default;

					if (owner.check_state == CheckState.Checked) {
						retval |= AccessibleStates.Checked;
					}

					if (owner.Focused) {
						retval |= AccessibleStates.Focused;
					}

					if (owner.CanFocus) {
						retval |= AccessibleStates.Focusable;
					}

					return retval;
				}
			}
			#endregion	// RadioButtonAccessibleObject Properties

			#region RadioButtonAccessibleObject Methods
			public override void DoDefaultAction() {
				owner.PerformClick();
			}
			#endregion	// RadioButtonAccessibleObject Methods
		}
		#endregion	// RadioButtonAccessibleObject Sub-class

		#region Public Constructors
		public RadioButton() {
			appearance = Appearance.Normal;
			auto_check = true;
			radiobutton_alignment = ContentAlignment.MiddleLeft;
			text_alignment = ContentAlignment.MiddleLeft;
			is_tabstop = false;
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
				return radiobutton_alignment;
			}

			set {
				if (value != radiobutton_alignment) {
					radiobutton_alignment = value;

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

		public bool TabStop {
			get {
				return is_tabstop;
			}

			set {
				is_tabstop = value;
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
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				SetStyle(ControlStyles.UserPaint, true);

				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return ThemeEngine.Current.RadioButtonDefaultSize;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void PerformClick() {
			OnClick(EventArgs.Empty);
		}

		public override string ToString() {
			return base.ToString() + ", Checked: " + this.Checked;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override AccessibleObject CreateAccessibilityInstance() {
			return base.CreateAccessibilityInstance ();
		}

		protected virtual void OnCheckedChanged(EventArgs e) {
			if (CheckedChanged != null) {
				CheckedChanged(this, e);
			}
		}

		protected override void OnClick(EventArgs e) {
			if (auto_check) {
				Checked = !Checked;
			}
		}

		protected override void OnEnter(EventArgs e) {
			base.OnEnter(e);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
		}

		protected override void OnMouseUp(MouseEventArgs mevent) {
			base.OnMouseUp(mevent);
		}

		protected override bool ProcessMnemonic(char charCode) {
			return base.ProcessMnemonic(charCode);
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler	AppearanceChanged;
		public event EventHandler	CheckedChanged;
		#endregion	// Events

		#region Internal Drawing Code
		internal override void Draw (PaintEventArgs pe) {
			if (redraw) {
				ThemeEngine.Current.DrawRadioButton(this.DeviceContext, this.ClientRectangle, this);
				redraw = false;
			}
		}
		#endregion	// Internal Drawing Code
	}
}
