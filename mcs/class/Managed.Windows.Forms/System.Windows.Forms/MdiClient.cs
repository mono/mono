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

namespace System.Windows.Forms {
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public sealed class MdiClient : Control {
		#region Local Variables
		private int mdi_created;
		private Form active;
		private HScrollBar hbar;
		private VScrollBar vbar;
		private SizeGrip sizegrip;
		private int hbar_value;
		private int vbar_value;
		private bool lock_sizing;
		
		#endregion	// Local Variables

		#region Public Classes
		public new class ControlCollection : Control.ControlCollection {
			MdiClient	owner;
			
			public ControlCollection(MdiClient owner) : base(owner) {
				this.owner = owner;
				controls = new ArrayList ();
			}

			public override void Add(Control value) {
				if ((value is Form) == false || !(((Form)value).IsMdiChild)) {
					throw new ArgumentException("Form must be MdiChild");
				}
				base.Add (value);
				SetChildIndex (value, 0); // always insert at front
				// newest member is the active one
				owner.ActiveMdiChild = (Form) value;

				value.LocationChanged += new EventHandler (owner.FormLocationChanged);
			}

			public override void Remove(Control value) {
				base.Remove (value);
			}
		}
		#endregion	// Public Classes

		#region Public Constructors
		public MdiClient() {
			BackColor = SystemColors.AppWorkspace;
			Dock = DockStyle.Fill;
			SetStyle (ControlStyles.Selectable, false);
		}
		#endregion	// Public Constructors

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
			base.WndProc (ref m);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);

			// Should probably make this into one loop
			SizeScrollBars ();
			SizeMaximized ();
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

		public Form[] MdiChildren {
			get {
				Form[]	children;

				children = new Form[Controls.Count];
				Controls.CopyTo(children, 0);

				return children;
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
		public void LayoutMdi(MdiLayout value) {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		#endregion	// Protected Instance Methods

		private void SizeScrollBars ()
		{
			if (lock_sizing)
				return;

			if (Controls.Count == 0 || ((Form) Controls [0]).WindowState == FormWindowState.Maximized) {
				if (hbar != null)
					hbar.Visible = false;
				if (vbar != null)
					vbar.Visible = false;
				return;
			}
				
			bool hbar_required = false;
			bool vbar_required = false;

			int right = 0;
			int left = 0;
			foreach (Form child in Controls) {
				if (!child.Visible)
					continue;
				if (child.Right > right)
					right = child.Right;
				if (child.Left < left) {
					hbar_required = true;
					left = child.Left;
				}
			}

			int top = 0;
			int bottom = 0;
			foreach (Form child in Controls) {
				if (!child.Visible)
					continue;
				if (child.Bottom > bottom)
					bottom = child.Bottom;
				if (child.Top < 0) {
					vbar_required = true;
					top = child.Top;
				}
			}

			int right_edge = Right;
			int bottom_edge = Bottom;
			int prev_right_edge;
			int prev_bottom_edge;

			bool need_hbar = false;
			bool need_vbar = false;

			do {
				prev_right_edge = right_edge;
				prev_bottom_edge = bottom_edge;

				if (hbar_required || right > right_edge) {
					need_hbar = true;
					bottom_edge = Bottom - SystemInformation.HorizontalScrollBarHeight;
				} else {
					need_hbar = false;
					bottom_edge = Bottom;
				}

				if (vbar_required || bottom > bottom_edge) {
					need_vbar = true;
					right_edge = Right - SystemInformation.VerticalScrollBarWidth;
				} else {
					need_vbar = false;
					right_edge = Right;
				}

			} while (right_edge != prev_right_edge || bottom_edge != prev_bottom_edge);

			if (need_hbar) {
				if (hbar == null) {
					hbar = new HScrollBar ();
					Controls.AddImplicit (hbar);
				}
				hbar.Visible = true;
				CalcHBar (left, right, right_edge, need_vbar);
			} else if (hbar != null)
				hbar.Visible = false;

			if (need_vbar) {
				if (vbar == null) {
					vbar = new VScrollBar ();
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
			int virtual_left = Math.Min (left, 0);
			int virtual_right = Math.Max (right, right_edge);
			int diff = (virtual_right - virtual_left) - right_edge;
			hbar.Left = 0;
			hbar.Top = Height - hbar.Height;
			hbar.Width = Width - (vert_vis ? SystemInformation.VerticalScrollBarWidth : 0);
			hbar.LargeChange = 50;
			hbar.Maximum = diff + 51;
			hbar.Value = -virtual_left;
			hbar.ValueChanged += new EventHandler (HBarValueChanged);
		}

		private void CalcVBar (int top, int bottom, int bottom_edge, bool horz_vis)
		{
			int virtual_top = Math.Min (top, 0);
			int virtual_bottom = Math.Max (bottom, bottom_edge);
			int diff = (virtual_bottom - virtual_top) - bottom_edge;
			vbar.Top = 0;
			vbar.Left = Width - vbar.Width;
			vbar.Height = Height - (horz_vis ? SystemInformation.HorizontalScrollBarHeight : 0);
			vbar.LargeChange = 50;
			vbar.Maximum = diff + 51;
			vbar.Value = -virtual_top;
			vbar.ValueChanged += new EventHandler (VBarValueChanged);
			
		}

		private void HBarValueChanged (object sender, EventArgs e)
		{
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

		private void SizeMaximized ()
		{
			foreach (Form child in Controls) {
				if (!child.Visible)
					continue;
				MdiWindowManager wm = (MdiWindowManager) child.WindowManager;
				if (wm.GetWindowState () == FormWindowState.Maximized)
					wm.SizeMaximized ();
			}
		}

		private void FormLocationChanged (object sender, EventArgs e)
		{
			SizeScrollBars ();
		}

		private int iconic_x = -1;
		private int iconic_y = -1;
		internal void ArrangeIconicWindows ()
		{
			int xspacing = 160;
			int yspacing = 25;

			if (iconic_x == -1 && iconic_y == -1) {
				iconic_x = Left;
				iconic_y = Bottom - yspacing;
			}

			lock_sizing = true;
			foreach (Form form in Controls) {
				if (form.WindowState != FormWindowState.Minimized)
					continue;

				MdiWindowManager wm = (MdiWindowManager) form.WindowManager;

				if (wm.IconicBounds != Rectangle.Empty) {
					form.Bounds = wm.IconicBounds;
					continue;
				}
					
				// The extra one pixel is a cheap hack for now until we
				// handle 0 client sizes properly in the driver
				int height = wm.TitleBarHeight + (wm.BorderWidth * 2) + 1; 
				Rectangle rect = new Rectangle (iconic_x, iconic_y, xspacing, height);
				form.Bounds = wm.IconicBounds = rect;

				iconic_x += xspacing;
				if (iconic_x >= Right) {
					iconic_x = Left;
					iconic_y -= height;
				}
			}
			lock_sizing = false;
		}

		internal void ActivateChild (Form form)
		{
			form.BringToFront ();
			active = form;

			foreach (Form child in Controls) {
				if (child == form)
					continue;
				// TODO: We need to repaint the decorations here
			}
		}

		internal int ChildrenCreated {
			get { return mdi_created; }
			set { mdi_created = value; }
		}

		internal Form ActiveMdiChild {
			get { return active; }
			set { active = value; }
		}
	}
}
