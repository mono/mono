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
// $Revision: 1.4 $
// $Log: ToolTip.cs,v $
// Revision 1.4  2004/11/08 20:49:35  pbartok
// - Fixed arguments for updated SetTopmost function
// - Fixed usage of PointToClient
//
// Revision 1.3  2004/10/19 06:04:59  ravindra
// Fixed constructor.
//
// Revision 1.2  2004/10/18 06:28:30  ravindra
// Suppressed a warning message.
//
// Revision 1.1  2004/10/18 05:19:57  pbartok
// - Complete implementation
//
//
//

// COMPLETE

using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	public sealed class ToolTip : System.ComponentModel.Component, System.ComponentModel.IExtenderProvider {
		#region Local variables
		internal bool		is_active;
		internal int		automatic_delay;
		internal int		autopop_delay;
		internal int		initial_delay;
		internal int		re_show_delay;
		internal bool		show_always;

		internal ToolTipWindow	tooltip_window;			// The actual tooltip window
		internal Hashtable	tooltip_strings;		// List of strings for each control, indexed by control
		internal Control	active_control;			// Control for which the tooltip is currently displayed
		internal Control	last_control;			// last control the mouse was in; null if the last control did not have a tooltip
		internal Size		display_size;			// Size of the screen
		internal Timer		timer;				// Used for the various intervals
		#endregion	// Local variables

		#region ToolTipWindow Class
		internal class ToolTipWindow : Control {
			#region ToolTipWindow Class Local Variables
			internal StringFormat	string_format;
			internal ToolTip	owner;
			#endregion	// ToolTipWindow Class Local Variables

			#region ToolTipWindow Class Constructor
			internal ToolTipWindow(ToolTip owner) : base() {
				this.owner = owner;

				string_format = new StringFormat();
				string_format.LineAlignment = StringAlignment.Center;
				string_format.Alignment = StringAlignment.Center;
				string_format.FormatFlags = StringFormatFlags.NoWrap;

				Visible = false;
				Size = new Size(100, 20);
				ForeColor = ThemeEngine.Current.ColorInfoText;
				BackColor = ThemeEngine.Current.ColorInfoWindow;

				VisibleChanged += new EventHandler(ToolTipWindow_VisibleChanged);

				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
			}

			#endregion	// ToolTipWindow Class Constructor

			#region ToolTipWindow Class Protected Instance Methods
			protected override void OnCreateControl() {
				base.OnCreateControl ();
				XplatUI.SetTopmost(this.window.Handle, IntPtr.Zero, true);
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams cp;

					cp = base.CreateParams;

					cp.Style = (int)WindowStyles.WS_POPUP;
					cp.Style |= (int)WindowStyles.WS_CLIPSIBLINGS;

					cp.ExStyle |= (int)WindowStyles.WS_EX_TOOLWINDOW;

					return cp;
				}
			}

			protected override void OnPaint(PaintEventArgs pevent) {
				// We don't do double-buffering on purpose:
				// 1) we'd have to meddle with is_visible, it destroys the buffers if !visible
				// 2) We don't draw much, no need to double buffer
				ThemeEngine.Current.DrawToolTip(pevent.Graphics, ClientRectangle, owner);
			}

			protected override void Dispose(bool disposing) {
				if (disposing) {
					this.string_format.Dispose();
				}
				base.Dispose (disposing);
			}

			protected override void WndProc(ref Message m) {
				if (m.Msg == (int)Msg.WM_SETFOCUS) {
					if (m.WParam != IntPtr.Zero) {
						// FIXME - still need to teach XplatUI this call
						//XplatUI.SetFocus(m.WParam);
					}
				}
				base.WndProc (ref m);
			}


			#endregion	// ToolTipWindow Class Protected Instance Methods

			#region ToolTipWindow Class Private Methods
			private void ToolTipWindow_VisibleChanged(object sender, EventArgs e) {
				Control control = (Control)sender;

				if (control.is_visible) {
					XplatUI.SetTopmost(control.window.Handle, IntPtr.Zero, true);
				} else {
					XplatUI.SetTopmost(control.window.Handle, IntPtr.Zero, false);
				}
			}
			#endregion	// ToolTipWindow Class Protected Instance Methods
		}
		#endregion	// ToolTipWindow Class

		#region Public Constructors & Destructors
		public ToolTip() {
			XplatUI.GetDisplaySize(out display_size);

			// Defaults from MS
			is_active = true;
			automatic_delay = 500;
			autopop_delay = 5000;
			initial_delay = 500;
			re_show_delay = 100;
			show_always = false;

			tooltip_strings = new Hashtable(5);

			tooltip_window = new ToolTipWindow(this);
			tooltip_window.MouseLeave += new EventHandler(control_MouseLeave);

			timer = new Timer();
			timer.Enabled = false;
			timer.Tick +=new EventHandler(timer_Tick);
		}

		public ToolTip(System.ComponentModel.IContainer cont) : this() {
			// Dunno why I'd need the container
		}

		~ToolTip() {
		}
		#endregion	// Public Constructors & Destructors

		#region Public Instance Properties
		public bool Active {
			get {
				return is_active;
			}

			set {
				if (is_active != value) {
					is_active = value;

					if (tooltip_window.Visible) {
						tooltip_window.Visible = false;
						active_control = null;
					}
				}
			}
		}

		public int AutomaticDelay {
			get {
				return automatic_delay;
			}

			set {
				if (automatic_delay != value) {
					automatic_delay = value;
					autopop_delay = automatic_delay * 10;
					initial_delay = automatic_delay;
					re_show_delay = automatic_delay / 5;
				}
			}
		}

		public int AutoPopDelay {
			get {
				return autopop_delay;
			}

			set {
				if (autopop_delay != value) {
					autopop_delay = value;
				}
			}
		}

		public int InitialDelay {
			get {
				return initial_delay;
			}

			set {
				if (initial_delay != value) {
					initial_delay = value;
				}
			}
		}

		public int ReshowDelay {
			get {
				return re_show_delay;
			}

			set {
				if (re_show_delay != value) {
					re_show_delay = value;
				}
			}
		}

		public bool ShowAlways {
			get {
				return show_always;
			}

			set {
				if (show_always != value) {
					show_always = value;
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public bool CanExtend(object target) {
			return false;
		}

		public string GetToolTip(Control control) {
			return (string)tooltip_strings[control];
		}

		public void RemoveAll() {
			tooltip_strings.Clear();
		}

		public void SetToolTip(Control control, string caption) {
			tooltip_strings[control] = caption;

			control.MouseEnter += new EventHandler(control_MouseEnter);
			control.MouseMove += new MouseEventHandler(control_MouseMove);
			control.MouseLeave += new EventHandler(control_MouseLeave);
		}

		public override string ToString() {
			return base.ToString() + " InitialDelay: " + initial_delay + ", ShowAlways: " + show_always;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			if (disposing) {
				// Mop up the mess; or should we wait for the GC to kick in?
				timer.Stop();
				timer.Dispose();

				// Not sure if we should clean up tooltip_window
				tooltip_window.Dispose();

				tooltip_strings.Clear();
			}
		}
		#endregion	// Protected Instance Methods

		#region Private Methods
		private void control_MouseEnter(object sender, EventArgs e) {
			string	text;

			// Whatever we're displaying right now, we don't want it anymore
			tooltip_window.Visible = false;
			timer.Stop();

			// if we're in the same control as before (how'd that happen?) or if we're not active, leave
			if (!is_active || (active_control == (Control)sender)) {
				return;
			}

			// As of this writing, our MWF implementation had no clue what an active control was :-(
			if (!show_always) {
				if (((Control)sender).GetContainerControl().ActiveControl == null) {
					return;
				}
			}

			text = (string)tooltip_strings[sender];
			if (text != null) {
				Size size;

				size = ThemeEngine.Current.ToolTipSize(this, text);
				tooltip_window.Width = size.Width;
				tooltip_window.Height = size.Height;
				tooltip_window.Text = text;

				// FIXME - this needs to be improved; the tooltip will show up under the mouse, which is annoying; use cursor size once implemented

				if ((Control.MousePosition.X+1+tooltip_window.Width) < display_size.Width) {
					tooltip_window.Left = Control.MousePosition.X+1;
				} else {
					tooltip_window.Left = display_size.Width-tooltip_window.Width;
				}

				if ((Control.MousePosition.Y+tooltip_window.Height)<display_size.Height) {
					tooltip_window.Top = Control.MousePosition.Y;
				} else {
					tooltip_window.Top = Control.MousePosition.Y-tooltip_window.Height;
				}

				// Since we get the mouse enter before the mouse leave, active_control will still be non-null if we were in a 
				// tooltip'd control; should prolly check on X11 too, and make sure that driver behaves the same way
				if (active_control == null) {
					timer.Interval = initial_delay;
				} else {
					timer.Interval = re_show_delay;
				}

				active_control = (Control)sender;

				// We're all set, lets wake the timer (which will then make us visible)
				timer.Enabled = true;
			}
		}

		private void timer_Tick(object sender, EventArgs e) {
			// Show our pretty selves
			timer.Stop();
			if (!tooltip_window.Visible) {
				// The initial_delay timer kicked in
				tooltip_window.Visible = true;
				timer.Interval = autopop_delay;
				timer.Start();
			} else {
				// The autopop_delay timer happened
				tooltip_window.Visible = false;
			}
		}


		private bool MouseInControl(Control control) {
			Point	m;
			Point	c;
			Size	cw;

			if (control == null) {
				return false;
			}

			m = Control.MousePosition;
			c = new Point(control.Bounds.X, control.Bounds.Y);
			if (control.parent != null) {
				c = control.parent.PointToScreen(c);
			}
			cw = control.ClientSize;

			if (c.X<=m.X && m.X<(c.X+cw.Width) &&
				c.Y<=m.Y && m.Y<(c.Y+cw.Height)) {
				return true;
			}
			return false;
		}

		private void control_MouseLeave(object sender, EventArgs e) {
			// In case the timer is still running, stop it
			timer.Stop();

			if (!MouseInControl(tooltip_window) && !MouseInControl(active_control)) {
				active_control = null;
				tooltip_window.Visible = false;
			}
		}

		private void control_MouseMove(object sender, MouseEventArgs e) {
			// Restart the interval, the mouse moved
			timer.Stop();
			timer.Start();

		}
		#endregion	// Private Methods
	}
}
