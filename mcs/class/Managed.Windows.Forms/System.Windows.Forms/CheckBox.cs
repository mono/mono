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
// $Revision: 1.4 $
// $Modtime: $
// $Log: CheckBox.cs,v $
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
		private Appearance		appearance;
		private bool			auto_check;
		private ContentAlignment	check_alignment;
		private ContentAlignment	text_alignment;
		private CheckState		check_state;
		private bool			three_state;
		private int			checkmark_size=13;		// Keep it configurable for accessability


		#region Public Constructors
		public CheckBox() {
			appearance = Appearance.Normal;
			auto_check = true;
			check_alignment = ContentAlignment.MiddleLeft;

			SizeChanged+=new System.EventHandler(RedrawEvent);
			TextChanged+=new System.EventHandler(RedrawEvent);
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
					CheckRedraw();
					Refresh();
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

					CheckRedraw();
					Refresh();
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
					CheckRedraw();
					Refresh();
					OnCheckedChanged(EventArgs.Empty);
				} else if (!value && (check_state != CheckState.Unchecked)) {
					check_state = CheckState.Unchecked;
					CheckRedraw();
					Refresh();
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
					CheckRedraw();
					Refresh();
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
					CheckRedraw();
					Refresh();
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
		internal override bool CheckRedraw() {
			this.needs_redraw = true;

			return true;
		}

		internal override void Redraw() {
			StringFormat		text_format;
			Rectangle		client_rectangle;
			Rectangle		text_rectangle;
			Rectangle		checkbox_rectangle;
			SolidBrush		sb;

			client_rectangle = ClientRectangle;
			text_rectangle = client_rectangle;
			checkbox_rectangle = new Rectangle(text_rectangle.X, text_rectangle.Y, checkmark_size, checkmark_size);

			text_format = new StringFormat();
			text_format.Alignment=StringAlignment.Near;
			text_format.LineAlignment=StringAlignment.Center;

			/* Calculate the position of text and checkbox rectangle */
			if (appearance!=Appearance.Button) {
				switch(check_alignment) {
					case ContentAlignment.BottomCenter: {
						if (client_rectangle.Height<checkmark_size*2) {
							ClientSize=new Size(client_rectangle.Width, checkmark_size*2);
							client_rectangle = ClientRectangle;
						}
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						break;
					}

					case ContentAlignment.BottomLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X+checkmark_size;
						text_rectangle.Width=client_rectangle.Width-checkmark_size;
						break;
					}

					case ContentAlignment.BottomRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size;
						break;
					}

					case ContentAlignment.MiddleCenter: {
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						break;
					}

					default:
					case ContentAlignment.MiddleLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X+checkmark_size;
						text_rectangle.Width=client_rectangle.Width-checkmark_size;
						break;
					}

					case ContentAlignment.MiddleRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size;
						break;
					}

					case ContentAlignment.TopCenter: {
						if (client_rectangle.Height<checkmark_size*2) {
							ClientSize=new Size(client_rectangle.Width, checkmark_size*2);
							client_rectangle = ClientRectangle;
						}
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=client_rectangle.Top;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Y=checkmark_size;
						text_rectangle.Width=client_rectangle.Width;
						text_rectangle.Height=client_rectangle.Height-checkmark_size;
						break;
					}

					case ContentAlignment.TopLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						checkbox_rectangle.Y=client_rectangle.Top;
						text_rectangle.X=client_rectangle.X+checkmark_size;
						text_rectangle.Width=client_rectangle.Width-checkmark_size;
						break;
					}

					case ContentAlignment.TopRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						checkbox_rectangle.Y=client_rectangle.Top;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size;
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
			
			if (ThreeState && (CheckState == CheckState.Indeterminate)) {
				state |= ButtonState.Checked;
				state |= ButtonState.Pushed;
			}

			// Start drawing

			sb=new SolidBrush(BackColor);
			this.DeviceContext.FillRectangle(sb, ClientRectangle);
			sb.Dispose();

			if (appearance!=Appearance.Button) {
				ControlPaint.DrawCheckBox(this.DeviceContext, checkbox_rectangle, state);
			} else {
				ControlPaint.DrawButton(this.DeviceContext, text_rectangle, state);
			}

			/* Place the text; to be compatible with Windows place it after the checkbox has been drawn */
			sb=new SolidBrush(ForeColor);
			this.DeviceContext.DrawString(Text, Font, sb, text_rectangle, text_format);
			sb.Dispose();

			if (Focused) {
				ControlPaint.DrawFocusRectangle(this.DeviceContext, text_rectangle);
			}
		}

		private void RedrawEvent(object sender, System.EventArgs e) {
			CheckRedraw();
		}
			
		#endregion	// Internal drawing code
	}
}
