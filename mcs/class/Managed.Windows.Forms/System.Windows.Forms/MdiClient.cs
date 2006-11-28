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
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public sealed class MdiClient : Control {
		#region Local Variables
		private int mdi_created;
		private ImplicitHScrollBar hbar;
		private ImplicitVScrollBar vbar;
		private SizeGrip sizegrip;
		private int hbar_value;
		private int vbar_value;
		private bool lock_sizing;
		private bool initializing_scrollbars;
		private int prev_bottom;
		private LayoutEventHandler initial_layout_handler;
		private bool setting_windowstates = false;
		internal ArrayList mdi_child_list;
		private string form_text;
		private bool setting_form_text;
		internal ArrayList original_order = new ArrayList (); // The order the child forms are added (used by the main menu to show the window menu)

		#endregion	// Local Variables

		#region Public Classes
		public new class ControlCollection : Control.ControlCollection {

			private MdiClient owner;
			
			public ControlCollection(MdiClient owner) : base(owner) {
				this.owner = owner;
				owner.mdi_child_list = new ArrayList ();
			}

			public override void Add(Control value) {
				if ((value is Form) == false || !(((Form)value).IsMdiChild)) {
					throw new ArgumentException("Form must be MdiChild");
				}
				owner.mdi_child_list.Add (value);
				base.Add (value);

				// newest member is the active one
				Form form = (Form) value;
				owner.ActiveMdiChild = form;
			}

			public override void Remove(Control value)
			{
				Form form = value as Form;
				if (form != null) {
					MdiWindowManager wm = form.WindowManager as MdiWindowManager;
					if (wm != null) {
						form.Closed -= wm.form_closed_handler;
					}
				}

				owner.mdi_child_list.Remove (value);
				base.Remove (value);
			}
		}
		#endregion	// Public Classes

		#region Public Constructors
		public MdiClient()
		{
			BackColor = SystemColors.AppWorkspace;
			Dock = DockStyle.Fill;
			SetStyle (ControlStyles.Selectable, false);
		}
		#endregion	// Public Constructors


		internal void SetParentText(bool text_changed)
		{
			if (setting_form_text)
				return;

			setting_form_text = true;

			if (text_changed)
				form_text = ParentForm.Text;

			if (ParentForm.ActiveMaximizedMdiChild == null) {
				ParentForm.Text = form_text;
			} else {
				string childText = ParentForm.ActiveMaximizedMdiChild.form.Text;
				if (childText.Length > 0) {
					ParentForm.Text = form_text + " - [" + ParentForm.ActiveMaximizedMdiChild.form.Text + "]";
				} else {
					ParentForm.Text = form_text;
				}
			}

			setting_form_text = false;
		}

		internal override void OnPaintBackgroundInternal (PaintEventArgs pe)
		{
			if (BackgroundImage != null)
				return;

			if (Parent == null || Parent.BackgroundImage == null)
				return;
			Parent.PaintControlBackground (pe);
		}

		internal Form ParentForm {
			get { return (Form) Parent; }
		}

		protected override Control.ControlCollection CreateControlsInstance ()
		{
			return new MdiClient.ControlCollection (this);
		}

		protected override void WndProc(ref Message m) {
			/*
			switch ((Msg) m.Msg) {
				case Msg.WM_PAINT: {				
					Console.WriteLine ("ignoring paint");
					return;
				}
			}
			*/
			switch ((Msg)m.Msg) {
			case Msg.WM_NCCALCSIZE:
				XplatUIWin32.NCCALCSIZE_PARAMS	ncp;

				if (m.WParam == (IntPtr) 1) {
					ncp = (XplatUIWin32.NCCALCSIZE_PARAMS) Marshal.PtrToStructure (m.LParam,
							typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

					int bw = 2;

					ncp.rgrc1.top += bw;
					ncp.rgrc1.bottom -= bw;
					ncp.rgrc1.left += bw;
					ncp.rgrc1.right -= bw;
					
					Marshal.StructureToPtr (ncp, m.LParam, true);
				}

				break;

			case Msg.WM_NCPAINT:
				PaintEventArgs pe = XplatUI.PaintEventStart (Handle, false);

				Rectangle clip;
				clip = new Rectangle (0, 0, Width, Height);

				ControlPaint.DrawBorder3D (pe.Graphics, clip, Border3DStyle.Sunken);
				XplatUI.PaintEventEnd (Handle, false);
				m.Result = IntPtr.Zero;
				return ;
			}

			base.WndProc (ref m);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);

			if (Parent != null)
				XplatUI.InvalidateNC (Parent.Handle);
			// Should probably make this into one loop
			SizeScrollBars ();
			ArrangeWindows ();
		}

		protected override void ScaleCore (float dx, float dy)
		{
			base.ScaleCore (dx, dy);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		#region Public Instance Properties
		[Localizable(true)]
		public override System.Drawing.Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

		public Form [] MdiChildren {
			get {
				if (mdi_child_list == null)
					return new Form [0];
				return (Form []) mdi_child_list.ToArray (typeof (Form));
			}
		}
		#endregion	// Public Instance Properties

#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void LayoutMdi (MdiLayout value) {

			int max_width = Int32.MaxValue;
			int max_height = Int32.MaxValue;

			if (Parent != null) {
				max_width = Parent.Width;
				max_height = Parent.Height;
			}

			switch (value) {
			case MdiLayout.Cascade:
				int i = 0;
				for (int c = Controls.Count - 1; c >= 0; c--) {
					Form form = (Form) Controls [c];

					int l = 22 * i;
					int t = 22 * i;

					if (i != 0 && (l + form.Width > max_width || t + form.Height > max_height)) {
						i = 0;
						l = 22 * i;
						t = 22 * i;
					}

					form.Left = l;
					form.Top = t;

					i++;
				}
				break;
			default:
				throw new NotImplementedException();
			}
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		#endregion	// Protected Instance Methods

		internal void SizeScrollBars ()
		{
			if (lock_sizing)
				return;

			if (Controls.Count == 0 || ((Form) Controls [0]).WindowState == FormWindowState.Maximized) {
				if (hbar != null)
					hbar.Visible = false;
				if (vbar != null)
					vbar.Visible = false;
				if (sizegrip != null)
					sizegrip.Visible = false;
				return;
			}
				
			bool hbar_required = false;
			bool vbar_required = false;

			int right = 0;
			int left = 0;
			int top = 0;
			int bottom = 0;

			foreach (Form child in Controls) {
				if (!child.Visible)
					continue;
				if (child.Right > right)
					right = child.Right;
				if (child.Left < left) {
					hbar_required = true;
					left = child.Left;
				}
				
				if (child.Bottom > bottom)
					bottom = child.Bottom;
				if (child.Top < 0) {
					vbar_required = true;
					top = child.Top;
				}
			}

			int first_right = Width;
			int first_bottom = Height;
			int right_edge = first_right;
			int bottom_edge = first_bottom;
			int prev_right_edge;
			int prev_bottom_edge;

			bool need_hbar = false;
			bool need_vbar = false;

			do {
				prev_right_edge = right_edge;
				prev_bottom_edge = bottom_edge;

				if (hbar_required || right > right_edge) {
					need_hbar = true;
					bottom_edge = first_bottom - SystemInformation.HorizontalScrollBarHeight;
				} else {
					need_hbar = false;
					bottom_edge = first_bottom;
				}

				if (vbar_required || bottom > bottom_edge) {
					need_vbar = true;
					right_edge = first_right - SystemInformation.VerticalScrollBarWidth;
				} else {
					need_vbar = false;
					right_edge = first_right;
				}

			} while (right_edge != prev_right_edge || bottom_edge != prev_bottom_edge);

			if (need_hbar) {
				if (hbar == null) {
					hbar = new ImplicitHScrollBar ();
					Controls.AddImplicit (hbar);
				}
				hbar.Visible = true;
				CalcHBar (left, right, right_edge, need_vbar);
			} else if (hbar != null)
				hbar.Visible = false;

			if (need_vbar) {
				if (vbar == null) {
					vbar = new ImplicitVScrollBar ();
					Controls.AddImplicit (vbar);
				}
				vbar.Visible = true;
				CalcVBar (top, bottom, bottom_edge, need_hbar);
			} else if (vbar != null)
				vbar.Visible = false;

			if (need_hbar && need_vbar) {
				if (sizegrip == null) {
					sizegrip = new SizeGrip ();
					Controls.AddImplicit (sizegrip);
				}
				sizegrip.Location = new Point (hbar.Right, vbar.Bottom);
				sizegrip.Width = vbar.Width;
				sizegrip.Height = hbar.Height;
				sizegrip.Visible = true;
			} else if (sizegrip != null) {
				sizegrip.Visible = false;
			}
		}

		private void CalcHBar (int left, int right, int right_edge, bool vert_vis)
		{
			initializing_scrollbars = true;
			int virtual_left = Math.Min (left, 0);
			int virtual_right = Math.Max (right, right_edge);
			int diff = (virtual_right - virtual_left) - right_edge;

			hbar.Left = 0;
			hbar.Top = ClientRectangle.Bottom - hbar.Height;
			hbar.Width = ClientRectangle.Width - (vert_vis ? SystemInformation.VerticalScrollBarWidth : 0);
			hbar.LargeChange = 50;
			hbar.Maximum = diff + 51 + (vert_vis ? SystemInformation.VerticalScrollBarWidth : 0);
			hbar.Value = -virtual_left;
			hbar.ValueChanged += new EventHandler (HBarValueChanged);
			XplatUI.SetZOrder (hbar.Handle, IntPtr.Zero, true, false);
			initializing_scrollbars = false;
		}

		private void CalcVBar (int top, int bottom, int bottom_edge, bool horz_vis)
		{
			initializing_scrollbars = true;
			int virtual_top = Math.Min (top, 0);
			int virtual_bottom = Math.Max (bottom, bottom_edge);
			int diff = (virtual_bottom - virtual_top) - bottom_edge;
			
			vbar.Top = 0;
			vbar.Left = ClientRectangle.Right - vbar.Width;
			vbar.Height = ClientRectangle.Height - (horz_vis ? SystemInformation.HorizontalScrollBarHeight : 0);
			vbar.LargeChange = 50;
			vbar.Minimum = virtual_top;
			vbar.Maximum = diff + 51 + (horz_vis ? SystemInformation.HorizontalScrollBarHeight : 0);
			vbar.ValueChanged += new EventHandler (VBarValueChanged);
			XplatUI.SetZOrder (vbar.Handle, IntPtr.Zero, true, false);
			initializing_scrollbars = false;
		}

		private void HBarValueChanged (object sender, EventArgs e)
		{
			if (initializing_scrollbars)
				return;
			
			if (hbar.Value == hbar_value)
				return;

			lock_sizing = true;

			try {
				foreach (Form child in Controls) {
					child.Left += hbar_value - hbar.Value;
				}
			} finally {
				lock_sizing = false;
			}

			hbar_value = hbar.Value;
			lock_sizing = false;
		}

		private void VBarValueChanged (object sender, EventArgs e)
		{
			if (initializing_scrollbars)
				return;
				
			if (vbar.Value == vbar_value)
				return;

			lock_sizing = true;

			try {
				foreach (Form child in Controls) {
					child.Top += vbar_value - vbar.Value;
				}
			} finally {
				lock_sizing = false;
			}

			vbar_value = vbar.Value;
			lock_sizing = false;
		}

		private void ArrangeWindows ()
		{
			int change = 0;
			if (prev_bottom != -1)
				change = Bottom - prev_bottom;

			foreach (Control c in Controls) {
				Form child = c as Form;

				if (c == null || !child.Visible)
					continue;

				MdiWindowManager wm = child.WindowManager as MdiWindowManager;
				if (wm.GetWindowState () == FormWindowState.Maximized)
					wm.SizeMaximized ();

				if (wm.GetWindowState () == FormWindowState.Minimized) {
					child.Top += change;
				}
					
			}

			prev_bottom = Bottom;
		}

		private void FormLocationChanged (object sender, EventArgs e)
		{
			SizeScrollBars ();
		}

		internal void ArrangeIconicWindows ()
		{
			int xspacing = 160;
			int yspacing = 25;

			Rectangle rect = new Rectangle (0, 0, xspacing, yspacing);

			lock_sizing = true;
			foreach (Form form in Controls) {
				if (form.WindowState != FormWindowState.Minimized)
					continue;

				MdiWindowManager wm = (MdiWindowManager) form.WindowManager;
				
				if (wm.IconicBounds != Rectangle.Empty) {
					if (form.Bounds != wm.IconicBounds)
						form.Bounds = wm.IconicBounds;
					continue;
				}
				
				// Need to get the width in the loop cause some themes might have
				// different widths for different styles
				int bw = ThemeEngine.Current.ManagedWindowBorderWidth (wm);
				
				// The extra one pixel is a cheap hack for now until we
				// handle 0 client sizes properly in the driver
				int height = wm.TitleBarHeight + (bw * 2) + 1;
				
				bool success = true;
				int startx, starty, currentx, currenty;
				
				startx = 0;
				starty = Bottom - yspacing - 1;
				if (this.hbar != null && this.hbar.Visible)
					starty -= this.hbar.Height;
				currentx = startx;
				currenty = starty;
				
				do {
					rect.X = currentx;
					rect.Y = currenty;
					rect.Height = height;
					success = true;
					foreach (Form form2 in Controls) {
						if (form2 == form || form2.window_state != FormWindowState.Minimized)
							continue;
						
						if (form2.Bounds.IntersectsWith(rect)) {
							success = false;
							break;
						}
					}
					if (!success) {	
						currentx += xspacing;
						if (currentx + xspacing > Right) {
							currentx = startx;
							currenty -= Math.Max(yspacing, height);
						} 
					}
				} while (!success);
				Console.WriteLine("IconicBounds = {0}", rect);
				wm.IconicBounds = rect;
				form.Bounds = wm.IconicBounds;
			}
			lock_sizing = false;
		}

		internal void CloseChildForm (Form form)
		{
			if (Controls.Count > 1) {
				Form next = (Form) Controls [1];
				if (form.WindowState == FormWindowState.Maximized)
					next.WindowState = FormWindowState.Maximized;
				ActivateChild (next);
			}

			Controls.Remove (form);
			form.Close ();
		}

		internal void ActivateNextChild ()
		{
			if (Controls.Count < 1)
				return;
			if (Controls.Count == 1 && Controls[0] == ActiveMdiChild)
				return;
				
			Form front = (Form) Controls [0];
			Form form = (Form) Controls [1];

			front.SendToBack ();
			ActivateChild (form);
		}

		internal void ActivateChild (Form form)
		{
			if (Controls.Count < 1)
				return;

			Form current = (Form) Controls [0];
			form.SuspendLayout ();
			form.BringToFront ();
			if (vbar != null && vbar.Visible)
				XplatUI.SetZOrder (vbar.Handle, IntPtr.Zero, true, false);
			if (hbar != null && hbar.Visible)
				XplatUI.SetZOrder (hbar.Handle, IntPtr.Zero, true, false);
			SetWindowStates ((MdiWindowManager) form.window_manager);
			form.ResumeLayout (false);
			if (current != form) {
				XplatUI.InvalidateNC (current.Handle);
				XplatUI.InvalidateNC (form.Handle);
			}
		}
		
		internal bool SetWindowStates (MdiWindowManager wm)
		{
		/*
			MDI WindowState behaviour:
			- If the active window is maximized, all other maximized windows are normalized.
			- If a normal window gets focus and the original active window was maximized, 
			  the normal window gets maximized and the original window gets normalized.
			- If a minimized window gets focus and the original window was maximized, 
			  the minimzed window gets maximized and the original window gets normalized. 
			  If the ex-minimized window gets deactivated, it will be normalized.
		*/
			Form form = wm.form;

			if (setting_windowstates) {
				return false;
			}
			
			if (!form.Visible)
				return false;
			
			bool is_active = wm.IsActive();
			bool maximize_this = false;
			
			if (!is_active){
				return false;
			}

			setting_windowstates = true;
			foreach (Form frm in mdi_child_list) {
				if (frm == form) {
					continue;
				} else if (!frm.Visible){
					continue;
				}
				if (frm.WindowState == FormWindowState.Maximized && is_active) {
					maximize_this = true;	
					if (((MdiWindowManager) frm.window_manager).was_minimized)
						frm.WindowState = FormWindowState.Minimized;
					else
						frm.WindowState = FormWindowState.Normal;//
				}
			}
			if (maximize_this) {
				wm.was_minimized = form.window_state == FormWindowState.Minimized;
				form.WindowState = FormWindowState.Maximized;
			}
			SetParentText(false);
			
			XplatUI.RequestNCRecalc(ParentForm.Handle);
			XplatUI.RequestNCRecalc (Handle);

			SizeScrollBars ();

			setting_windowstates = false;

			return maximize_this;
		}

		internal int ChildrenCreated {
			get { return mdi_created; }
			set { mdi_created = value; }
		}

		internal Form ActiveMdiChild {
			get {
				if (Controls.Count < 1)
					return null;
				return (Form) Controls [0];
			}
			set {
				ActivateChild (value);
			}
		}
	}
}

