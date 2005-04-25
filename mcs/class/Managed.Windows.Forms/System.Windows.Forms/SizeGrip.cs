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

// TODO: Eventually we need to handle the cursor and resizing the parent

using System;
using System.Drawing;

namespace System.Windows.Forms {

	internal class SizeGrip : Control {
		#region Local Variables
		private bool	redraw;
		private Point	capture_point;
		private int	window_w;
		private int	window_h;
		private bool	show_grip;
		private bool	hide_pending;
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
		protected override void OnPaint (PaintEventArgs pe) {
			base.OnPaint (pe);

			if (redraw && show_grip) {
                                ControlPaint.DrawSizeGrip (pe.Graphics, BackColor, ClientRectangle);
			}
		}

		protected override void OnSizeChanged (EventArgs e) {
			base.OnSizeChanged (e);
			redraw = true;
		}

		protected override void OnVisibleChanged (EventArgs e) {
			base.OnVisibleChanged (e);
			redraw = true;
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			Capture = true;
			
			capture_point = Control.MousePosition;

			window_w = parent.Width;
			window_h = parent.Height;
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			if (this.is_captured) {
				int	delta_x;
				int	delta_y;
				Point	current_point;

				current_point = Control.MousePosition;

				delta_x = current_point.X - capture_point.X;
				delta_y = current_point.Y - capture_point.Y;

				this.parent.Size = new Size(window_w + delta_x, window_h + delta_y);
				XplatUI.DoEvents();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			if (Capture) {
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


