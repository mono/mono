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
		private Point	capture_point;
		private Control captured_control;
		private int	window_w;
		private int	window_h;
		private bool	hide_pending;
		private bool	captured;
		private bool	is_virtual; // If virtual the size grip is painted directly on the captured controls' surface.
		private bool	enabled;
		private bool	fill_background;
		private Rectangle	last_painted_area; // The last area that was painted (to know which area to invalidate when resizing).
		#endregion	// Local Variables

		#region Constructors
		public SizeGrip (Control CapturedControl)
		{
			this.Cursor = Cursors.SizeNWSE;
			enabled = true;
			fill_background = true;
			this.Size = GetDefaultSize ();
			this.CapturedControl = CapturedControl;
		}
		#endregion	// Constructors

		#region Properties
		public bool FillBackground {
			get {
				return fill_background;
			}
			set {
				fill_background = value;
			}
		}
		
		public bool Virtual {
			get { 
				return is_virtual;
			}
			set {
				if (is_virtual == value)	
					return;
					
				is_virtual = value;
				if (is_virtual) {
					CapturedControl.MouseMove += new MouseEventHandler(HandleMouseMove);
					CapturedControl.MouseUp += new MouseEventHandler(HandleMouseUp);
					CapturedControl.MouseDown += new MouseEventHandler(HandleMouseDown);
					CapturedControl.EnabledChanged += new EventHandler(HandleEnabledChanged);
					CapturedControl.Resize += new EventHandler(HandleResize);
				} else {
					CapturedControl.MouseMove -= new MouseEventHandler (HandleMouseMove);
					CapturedControl.MouseUp -= new MouseEventHandler (HandleMouseUp);
					CapturedControl.MouseDown -= new MouseEventHandler (HandleMouseDown);
					CapturedControl.EnabledChanged -= new EventHandler (HandleEnabledChanged);
					CapturedControl.Resize -= new EventHandler (HandleResize);
				}
			}
		}
		
		public Control CapturedControl {
			get {
				return captured_control;
			}
			set {
				captured_control = value;
			}
		}
		
		#endregion	// Properties

		#region Methods
		static internal Size GetDefaultSize () {
			return new Size (SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
		}
		
		static internal Rectangle GetDefaultRectangle (Control Parent)
		{
			Size size = GetDefaultSize ();
			return new Rectangle (Parent.ClientSize.Width - size.Width, Parent.ClientSize.Height - size.Height, size.Width, size.Height);
		}
		
		private void HandleResize (object sender, EventArgs e)
		{
			Control ctrl = (Control) sender;
			ctrl.Invalidate (last_painted_area);
		}

		private void HandleEnabledChanged (object sender, EventArgs e)
		{
			Control ctrl = (Control) sender;
			enabled = ctrl.Enabled;
			Cursor cursor;
			if (enabled) {
				cursor = Cursors.SizeNWSE;
			} else {
				cursor = Cursors.Default;
			}
			if (is_virtual) {
				if (CapturedControl != null)
					CapturedControl.Cursor = cursor;
			} else {
				this.Cursor = cursor;
			}
			ctrl.Invalidate (GetDefaultRectangle (ctrl));
			
		}

		// This method needs to be internal, since the captured control must be able to call
		// it. We can't use events to hook it up, since then the paint ordering won't be correct
		internal void HandlePaint (object sender, PaintEventArgs e)
		{
			if (Visible) {
				Control destination = (Control) sender;
				Graphics gr = e.Graphics;
				Rectangle rect = GetDefaultRectangle (destination);
			
				if (!is_virtual || fill_background) {
					gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorControl), rect);
				}
				if (enabled) {
					ControlPaint.DrawSizeGrip (gr, BackColor, rect);
				}
				last_painted_area = rect;
			}
		}

		private void HandleMouseCaptureChanged (object sender, EventArgs e)
		{
			Control ctrl = (Control) sender;
			if (captured && !ctrl.Capture) {
				captured = false;
				CapturedControl.Size = new Size (window_w, window_h);
			}
		}

		internal void HandleMouseDown (object sender, MouseEventArgs e)
		{
			if (enabled) {
				Control ctrl = (Control)sender;
				if (!GetDefaultRectangle (ctrl).Contains (e.X, e.Y)) {
					return;
				}
				
				ctrl.Capture = true;
				captured = true;
				capture_point = Control.MousePosition;

				window_w = CapturedControl.Width;
				window_h = CapturedControl.Height;
			}
		}

		internal void HandleMouseMove (object sender, MouseEventArgs e)
		{
			Control ctrl = (Control) sender;
			Rectangle rect = GetDefaultRectangle (ctrl);
			
			if (rect.Contains (e.X, e.Y)) {
				ctrl.Cursor = Cursors.SizeNWSE;
			} else {
				ctrl.Cursor = Cursors.Default;
			}
			
			if (captured) {
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
				}
			}
		}

		internal void HandleMouseUp (object sender, MouseEventArgs e)
		{
			if (captured) {
				Control ctrl = (Control) sender;
				captured = false;
				ctrl.Capture = false;
				ctrl.Invalidate (last_painted_area);
				
				if (Parent is ScrollableControl) {
					((ScrollableControl)Parent).UpdateSizeGripVisible ();
				}
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

		protected override void OnPaint (PaintEventArgs pe)
		{
			HandlePaint (this, pe);
			base.OnPaint (pe);
		}

		protected override void OnMouseCaptureChanged (EventArgs e)
		{
			base.OnMouseCaptureChanged (e);
			HandleMouseCaptureChanged (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
			HandleEnabledChanged (this, e);
		}
		
		protected override void OnMouseDown (MouseEventArgs e)
		{
			HandleMouseDown (this, e);
		}
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			HandleMouseMove (this, e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			HandleMouseUp (this, e);
		}
		#endregion	// Methods
	}
}


