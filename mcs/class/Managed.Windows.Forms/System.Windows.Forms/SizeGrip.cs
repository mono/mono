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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)

using System;
using System.Drawing;

namespace System.Windows.Forms {

	internal class SizeGrip : Control {
		#region Local Variables
		private bool	redraw;
		private Point	capture_point;
		private Control captured_control;
		private int	window_w;
		private int	window_h;
		private bool	show_grip;
		private bool	hide_pending;
		private bool	captured;
		#endregion	// Local Variables

		#region Constructors
		public SizeGrip ()
		{
			this.Cursor = Cursors.SizeNWSE;
			show_grip = true;
			redraw = true;
			hide_pending = false;
		}
		#endregion	// Constructors

		#region Properties
		
		public Control CapturedControl {
			get {
				if (captured_control != null)
					return captured_control;
				else
					return Parent;
			}
			set {
				captured_control = value;
			}
		}
		
		public bool ShowGrip {
			get {
				return show_grip;
			}

			set {
				show_grip = value;
				redraw = true;
			}
		}
		#endregion	// Properties

		#region Methods
		protected override void OnEnabledChanged (EventArgs e) {
			base.OnEnabledChanged (e);
			if (Enabled) {
				this.Cursor = Cursors.SizeNWSE;			
			} else {
				this.Cursor = Cursors.Default;
			}
		}
		
		protected override void OnPaint (PaintEventArgs pe) {
			if (redraw && show_grip) {
				pe.Graphics.FillRectangle (new SolidBrush (ThemeEngine.Current.ColorControl), ClientRectangle);
				if (Enabled)
					ControlPaint.DrawSizeGrip (pe.Graphics, BackColor, ClientRectangle);
			}
			base.OnPaint (pe);
		}

		protected override void OnSizeChanged (EventArgs e) {
			base.OnSizeChanged (e);
			redraw = true;
		}

		protected override void OnVisibleChanged (EventArgs e) {
			base.OnVisibleChanged (e);
			redraw = true;
		}

#if NET_2_0
		protected override void OnMouseCaptureChanged (EventArgs e) {
			base.OnMouseCaptureChanged (e);
			
			if (captured && !Capture) {
				captured = false;
				CapturedControl.Size = new Size (window_w, window_h);
			}
		}
#endif
		protected override void OnMouseDown(MouseEventArgs e) {
			if (Enabled) {
				Capture = true;
				captured = true;
				
				capture_point = Control.MousePosition;

				window_w = CapturedControl.Width;
				window_h = CapturedControl.Height;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			if (Capture) {
				int	delta_x;
				int	delta_y;
				Point	current_point;

				current_point = Control.MousePosition;

				delta_x = current_point.X - capture_point.X;
				delta_y = current_point.Y - capture_point.Y;

				Control parent = CapturedControl;
				Form form_parent = parent as Form;
				Size new_size = new Size (window_w + delta_x, window_h + delta_y);
				Size max_size = form_parent != null ? form_parent.MaximumSize : Size.Empty;
				Size min_size = form_parent != null ? form_parent.MinimumSize : Size.Empty;
				
				if (new_size.Width > max_size.Width && max_size.Width > 0)
					new_size.Width = max_size.Width;
				else if (new_size.Width < min_size.Width)
					new_size.Width = min_size.Width;
				
				if (new_size.Height > max_size.Height && max_size.Height > 0)
					new_size.Height = max_size.Height;
				else if (new_size.Height < min_size.Height)
					new_size.Height = min_size.Height;

				if (new_size != parent.Size) {
					parent.Size = new_size;
					XplatUI.DoEvents();
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			if (Capture) {
				captured = false;
				Capture = false;
				if (hide_pending) {
					Hide();
					hide_pending = false;
				}
			}
		}


		protected override void SetVisibleCore(bool value) {
			if (Capture) {
				if (value == false) {
					hide_pending = true;
				} else {
					hide_pending = false;
				}
				return;
			}
			base.SetVisibleCore (value);
		}

		#endregion	// Methods
	}
}


