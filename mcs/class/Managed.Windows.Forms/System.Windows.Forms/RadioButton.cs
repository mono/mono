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
// $Log: RadioButton.cs,v $
// Revision 1.2  2004/09/01 20:44:11  pbartok
// - Fixed state
//
// Revision 1.1  2004/09/01 20:40:02  pbartok
// - Functional initial check-in
//
//
//

// COMPLETE

using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
	public class RadioButton : ButtonBase {
		#region Local Variables
		private Appearance		appearance;
		private bool			auto_check;
		private ContentAlignment	radiobutton_alignment;
		private ContentAlignment	text_alignment;
		private CheckState		check_state;
		private bool			is_tabstop;
		private int 			radiobutton_size = 12;		// Might not be correct
		#endregion	// Local Variables

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
					value = appearance;
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
				return new Size(104,24);
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
		internal override void Redraw() {
			StringFormat	text_format;
			Rectangle 	client_rectangle;
			Rectangle	text_rectangle;
			Rectangle 	radiobutton_rectangle;
			SolidBrush	sb;

			client_rectangle = ClientRectangle;
			text_rectangle = client_rectangle;
			radiobutton_rectangle = new Rectangle(text_rectangle.X, text_rectangle.Y, radiobutton_size, radiobutton_size);

			text_format = new StringFormat();
			text_format.Alignment = StringAlignment.Near;
			text_format.LineAlignment = StringAlignment.Center;

			/* Calculate the position of text and checkbox rectangle */
			if (appearance!=Appearance.Button) {
				switch(radiobutton_alignment) {
				case ContentAlignment.BottomCenter: {
					if (client_rectangle.Height<radiobutton_size*2) {
						ClientSize=new Size(client_rectangle.Width, radiobutton_size*2);
						client_rectangle = ClientRectangle;
					}
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width;
					break;
				}

				case ContentAlignment.BottomLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X+radiobutton_size;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size;
					break;
				}

				case ContentAlignment.BottomRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size;
					break;
				}

				case ContentAlignment.MiddleCenter: {
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width;
					break;
				}

				default:
				case ContentAlignment.MiddleLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X+radiobutton_size;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size;
					break;
				}

				case ContentAlignment.MiddleRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size;
					break;
				}

				case ContentAlignment.TopCenter: {
					if (client_rectangle.Height<radiobutton_size*2) {
						ClientSize=new Size(client_rectangle.Width, radiobutton_size*2);
						client_rectangle = ClientRectangle;
					}
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Y=radiobutton_size;
					text_rectangle.Width=client_rectangle.Width;
					text_rectangle.Height=client_rectangle.Height-radiobutton_size;
					break;
				}

				case ContentAlignment.TopLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X+radiobutton_size;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size;
					break;
				}

				case ContentAlignment.TopRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size;
					break;
				}
				}
			} else {
				text_rectangle.X=client_rectangle.X;
				text_rectangle.Width=client_rectangle.Width;
			}

			/* Set the horizontal alignment of our text */
			switch(text_alignment) {
			case ContentAlignment.BottomLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.TopLeft: {
				text_format.Alignment=StringAlignment.Near;
				break;
			}

			case ContentAlignment.BottomCenter:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.TopCenter: {
				text_format.Alignment=StringAlignment.Center;
				break;
			}

			case ContentAlignment.BottomRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.TopRight: {
				text_format.Alignment=StringAlignment.Far;
				break;
			}
			}

			/* Set the vertical alignment of our text */
			switch(text_alignment) {
			case ContentAlignment.TopLeft: 
			case ContentAlignment.TopCenter: 
			case ContentAlignment.TopRight: {
				text_format.LineAlignment=StringAlignment.Near;
				break;
			}

			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight: {
				text_format.LineAlignment=StringAlignment.Far;
				break;
			}

			case ContentAlignment.MiddleLeft:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.MiddleRight: {
				text_format.LineAlignment=StringAlignment.Center;
				break;
			}
			}

			ButtonState state = ButtonState.Normal;
			if (FlatStyle == FlatStyle.Flat) {
				state |= ButtonState.Flat;
			}
			
			if (Checked) {
				state |= ButtonState.Checked;
			}

			// Start drawing

			sb=new SolidBrush(BackColor);
			this.DeviceContext.FillRectangle(sb, ClientRectangle);
			sb.Dispose();

			if (appearance!=Appearance.Button) {
				ControlPaint.DrawRadioButton(this.DeviceContext, radiobutton_rectangle, state);
			} else {
				ControlPaint.DrawButton(this.DeviceContext, text_rectangle, state);
			}

			/* Place the text; to be compatible with Windows place it after the radiobutton has been drawn */
			sb=new SolidBrush(ForeColor);
			this.DeviceContext.DrawString(Text, Font, sb, text_rectangle, text_format);
			sb.Dispose();

			if (Focused) {
				ControlPaint.DrawFocusRectangle(this.DeviceContext, text_rectangle);
			}

			Refresh();
		}
		#endregion	// Internal Drawing Code
	}
}
