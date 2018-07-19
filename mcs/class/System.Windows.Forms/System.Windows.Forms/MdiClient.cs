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
	[ComVisible (true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
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
		private bool setting_windowstates = false;
		internal ArrayList mdi_child_list;
		private string form_text;
		private bool setting_form_text;
		private Form active_child;
		private Point next_child_stack_location;

		#endregion	// Local Variables

		#region Public Classes
		[ComVisible (false)]
		public new class ControlCollection : Control.ControlCollection {

			private MdiClient owner;
			
			public ControlCollection(MdiClient owner) : base(owner) {
				this.owner = owner;
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
			mdi_child_list = new ArrayList ();
			BackColor = SystemColors.AppWorkspace;
			Dock = DockStyle.Fill;
			SetStyle (ControlStyles.Selectable, false);
		}
		#endregion	// Public Constructors

		internal void SendFocusToActiveChild ()
		{
			Form active = this.ActiveMdiChild;
			if (active == null) {
				ParentForm.SendControlFocus (this);
			} else {
				active.SendControlFocus (active);
				ParentForm.ActiveControl = active;
			}
		}

		internal bool HorizontalScrollbarVisible {
			get { return hbar != null && hbar.Visible; }
		}
		internal bool VerticalScrollbarVisible {
			get { return vbar != null && vbar.Visible; }
		}

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
			switch ((Msg)m.Msg) {
			case Msg.WM_NCPAINT:
				PaintEventArgs pe = XplatUI.PaintEventStart (ref m, Handle, false);

				Rectangle clip;
				clip = new Rectangle (0, 0, Width, Height);

				ControlPaint.DrawBorder3D (pe.Graphics, clip, Border3DStyle.Sunken);
				XplatUI.PaintEventEnd (ref m, Handle, false, pe);
				m.Result = IntPtr.Zero;
				return ;
			}

			base.WndProc (ref m);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);

			if (Parent != null && Parent.IsHandleCreated)
				XplatUI.InvalidateNC (Parent.Handle);
			// Should probably make this into one loop
			SizeScrollBars ();
			ArrangeWindows ();
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			// Never change the MdiClient's location
			specified &= ~BoundsSpecified.Location;

			base.ScaleControl (factor, specified);
		}
		
		[System.ComponentModel.EditorBrowsable (EditorBrowsableState.Never)]
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override ImageLayout BackgroundImageLayout {
			get {
				return base.BackgroundImageLayout;
			}
			set {
				base.BackgroundImageLayout = value;
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
				CreateParams result = base.CreateParams;
				result.ExStyle |= (int) WindowExStyles.WS_EX_CLIENTEDGE;
				return result;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void LayoutMdi (MdiLayout value) {

			// Don't forget to always call ArrangeIconicWindows 
			ArrangeIconicWindows (true);

			switch (value) {
			case MdiLayout.Cascade: {
				int i = 0;
				for (int c = Controls.Count - 1; c >= 0; c--) {
					Form form = (Form) Controls [c];

					if (form.WindowState == FormWindowState.Minimized)
						continue;

					if (form.WindowState == FormWindowState.Maximized)
						form.WindowState = FormWindowState.Normal;

					form.Width = System.Convert.ToInt32 (ClientSize.Width * 0.8);
					form.Height = Math.Max (
								System.Convert.ToInt32 (ClientSize.Height * 0.8),
								SystemInformation.MinimumWindowSize.Height + 2);

					int l = 22 * i;
					int t = 22 * i;

					if (i != 0 && (l + form.Width > ClientSize.Width || t + form.Height > ClientSize.Height)) {
						i = 0;
						l = 22 * i;
						t = 22 * i;
					}

					form.Left = l;
					form.Top = t;

					i++;
				}
				break;
				}
			case MdiLayout.TileHorizontal:
			case MdiLayout.TileVertical: {
				// First count number of windows to tile
				int total = 0;
				
				// And space used by iconic windows
				int clientHeight = ClientSize.Height;
				
				for (int i = 0; i < Controls.Count; i++) {
					Form form = Controls [i] as Form;
					
					if (form == null)
						continue;
					
					if (!form.Visible)
						continue;

					if (form.WindowState == FormWindowState.Maximized)
						form.WindowState = FormWindowState.Normal;
					else if (form.WindowState == FormWindowState.Minimized) {
						if (form.Bounds.Top < clientHeight)
							clientHeight = form.Bounds.Top;
						continue;
					}
						
					total++;
				}
				if (total <= 0)
					return;

				// Calculate desired height and width
				Size newSize;
				Size offset;

				if (value == MdiLayout.TileHorizontal) {
					newSize = new Size(ClientSize.Width, clientHeight / total);
					offset = new Size (0, newSize.Height);
				} else {
					newSize = new Size(ClientSize.Width / total, clientHeight);
					offset = new Size (newSize.Width, 0);
				}
				
				// Loop again and set the size and location.
				Point nextLocation = Point.Empty;
				
				for (int i = 0; i < Controls.Count; i++) {
					Form form = Controls [i] as Form;

					if (form == null)
						continue;

					if (!form.Visible)
						continue;

					if (form.WindowState == FormWindowState.Minimized)
						continue;

					form.Size = newSize;
					form.Location = nextLocation;
					nextLocation += offset;
				}
				
				break;
				}
			}
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		#endregion	// Protected Instance Methods

		internal void SizeScrollBars ()
		{
			if (lock_sizing)
				return;
			
			if (!IsHandleCreated)
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
					left = child.Left;
				}
				
				if (child.Bottom > bottom)
					bottom = child.Bottom;
				if (child.Top < 0) {
					top = child.Top;
				}
			}

			int available_width = ClientSize.Width;
			int available_height = ClientSize.Height;

			bool need_hbar = false;
			bool need_vbar = false;

			if (right - left > available_width || left < 0) {
				need_hbar = true;
				available_height -= SystemInformation.HorizontalScrollBarHeight;
			}
			if (bottom - top > available_height || top < 0) {
				need_vbar = true;
				available_width -= SystemInformation.VerticalScrollBarWidth;

				if (!need_hbar && (right - left > available_width || left < 0)) {
					need_hbar = true;
					available_height -= SystemInformation.HorizontalScrollBarHeight;
				}
			}
			
			if (need_hbar) {
				if (hbar == null) {
					hbar = new ImplicitHScrollBar ();
					Controls.AddImplicit (hbar);
				}
				hbar.Visible = true;
				CalcHBar (left, right, need_vbar);
			} else if (hbar != null)
				hbar.Visible = false;

			if (need_vbar) {
				if (vbar == null) {
					vbar = new ImplicitVScrollBar ();
					Controls.AddImplicit (vbar);
				}
				vbar.Visible = true;
				CalcVBar (top, bottom, need_hbar);
			} else if (vbar != null)
				vbar.Visible = false;

			if (need_hbar && need_vbar) {
				if (sizegrip == null) {
					sizegrip = new SizeGrip (this.ParentForm);
					Controls.AddImplicit (sizegrip);
				}
				sizegrip.Location = new Point (hbar.Right, vbar.Bottom);
				sizegrip.Visible = true;
				XplatUI.SetZOrder (sizegrip.Handle, vbar.Handle, false, false);
			} else if (sizegrip != null) {
				sizegrip.Visible = false;
			}
			
			XplatUI.InvalidateNC (Handle);
		}

		private void CalcHBar (int left, int right, bool vert_vis)
		{
			initializing_scrollbars = true;

			hbar.Left = 0;
			hbar.Top = ClientRectangle.Bottom - hbar.Height;
			hbar.Width = ClientRectangle.Width - (vert_vis ? SystemInformation.VerticalScrollBarWidth : 0);
			hbar.LargeChange = 50;
			hbar.Minimum = Math.Min (left, 0);
			hbar.Maximum = Math.Max (right - ClientSize.Width + 51 + (vert_vis ? SystemInformation.VerticalScrollBarWidth : 0), 0);
			hbar.Value = 0;
			hbar_value = 0;
			hbar.ValueChanged += new EventHandler (HBarValueChanged);
			XplatUI.SetZOrder (hbar.Handle, IntPtr.Zero, true, false);
			
			initializing_scrollbars = false;
		}

		private void CalcVBar (int top, int bottom, bool horz_vis)
		{
			initializing_scrollbars = true;
			
			vbar.Top = 0;
			vbar.Left = ClientRectangle.Right - vbar.Width;
			vbar.Height = ClientRectangle.Height - (horz_vis ? SystemInformation.HorizontalScrollBarHeight : 0);
			vbar.LargeChange = 50;
			vbar.Minimum = Math.Min (top, 0);
			vbar.Maximum = Math.Max (bottom - ClientSize.Height + 51 + (horz_vis ? SystemInformation.HorizontalScrollBarHeight : 0), 0);
			vbar.Value = 0;
			vbar_value = 0;
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
				int diff = hbar_value - hbar.Value;
				foreach (Form child in Controls) {
					child.Left += diff;
				}
			} finally {
				lock_sizing = false;
			}

			hbar_value = hbar.Value;
		}

		private void VBarValueChanged (object sender, EventArgs e)
		{
			if (initializing_scrollbars)
				return;
				
			if (vbar.Value == vbar_value)
				return;

			lock_sizing = true;

			try {
				int diff = vbar_value - vbar.Value;
				foreach (Form child in Controls) {
					child.Top += diff;
				}
			} finally {
				lock_sizing = false;
			}

			vbar_value = vbar.Value;
		}

		private void ArrangeWindows ()
		{
			if (!IsHandleCreated)
				return;
				
			int change = 0;
			if (prev_bottom != -1)
				change = Bottom - prev_bottom;

			foreach (Control c in Controls) {
				Form child = c as Form;

				if (c == null || !child.Visible)
					continue;

				MdiWindowManager wm = child.WindowManager as MdiWindowManager;
				if (wm.GetWindowState () == FormWindowState.Maximized)
					child.Bounds = wm.MaximizedBounds;

				if (wm.GetWindowState () == FormWindowState.Minimized) {
					child.Top += change;
				}
					
			}

			prev_bottom = Bottom;
		}

		internal void ArrangeIconicWindows (bool rearrange_all)
		{
			Rectangle rect = Rectangle.Empty;

			lock_sizing = true;
			foreach (Form form in Controls) {
				if (form.WindowState != FormWindowState.Minimized)
					continue;

				MdiWindowManager wm = (MdiWindowManager) form.WindowManager;
				
				if (wm.IconicBounds != Rectangle.Empty && !rearrange_all) {
					if (form.Bounds != wm.IconicBounds)
						form.Bounds = wm.IconicBounds;
					continue;
				}
				
				bool success = true;
				int startx, starty, currentx, currenty;

				rect.Size = wm.IconicSize;
				
				startx = 0;
				starty = ClientSize.Height - rect.Height;
				currentx = startx;
				currenty = starty;
				
				do {
					rect.X = currentx;
					rect.Y = currenty;
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
						currentx += rect.Width;
						if (currentx + rect.Width > Right) {
							currentx = startx;
							currenty -= rect.Height;
						} 
					}
				} while (!success);
				wm.IconicBounds = rect;
				form.Bounds = wm.IconicBounds;
			}
			lock_sizing = false;
		}

		internal void ChildFormClosed (Form form)
		{
			FormWindowState closed_form_windowstate = form.WindowState;
	
			form.Visible = false;
			Controls.Remove (form);
			
			if (Controls.Count == 0) {
				((MdiWindowManager) form.window_manager).RaiseDeactivate ();
			} else if (closed_form_windowstate == FormWindowState.Maximized) {
				Form current = (Form) Controls [0];
				current.WindowState = FormWindowState.Maximized;
				ActivateChild(current);
			}

			if (Controls.Count == 0) {
				XplatUI.RequestNCRecalc (Parent.Handle);
				ParentForm.PerformLayout ();

				// If we closed the last child, unmerge the menus.
				// If it's not the last child, the menu will be unmerged
				// when another child takes focus.
				MenuStrip parent_menu = form.MdiParent.MainMenuStrip;

				if (parent_menu != null)
					if (parent_menu.IsCurrentlyMerged)
						ToolStripManager.RevertMerge (parent_menu);
			}
			SizeScrollBars ();
			SetParentText (false);
			form.Dispose();
		}

		internal void ActivateNextChild ()
		{
			if (Controls.Count < 1)
				return;
			if (Controls.Count == 1 && Controls[0] == ActiveMdiChild)
				return;
				
			Form front = (Form) Controls [0];
			Form form = (Form) Controls [1];

			ActivateChild (form);
			front.SendToBack ();
		}

		internal void ActivatePreviousChild ()
		{
			if (Controls.Count <= 1)
				return;
			
			Form back = (Form) Controls [Controls.Count - 1];
			
			ActivateChild (back);
		}

		internal void ActivateChild (Form form)
		{
			if (Controls.Count < 1)
				return;

			if (ParentForm.is_changing_visible_state > 0)
				return;
			
			Form current = (Form) Controls [0];
			bool raise_deactivate = ParentForm.ActiveControl == current;

			// We want to resize the new active form before it is 
			// made active to avoid flickering. Can't do it in the
			// normal way (form.WindowState = Maximized) since it's not
			// active yet and everything would just return to before. 
			// We also won't suspend layout, this way the layout will
			// happen before the form is made active (and in many cases
			// before it is visible, which avoids flickering as well).
			MdiWindowManager wm = (MdiWindowManager)form.WindowManager;
			
			if (current.WindowState == FormWindowState.Maximized && form.WindowState != FormWindowState.Maximized && form.Visible) {
				FormWindowState old_state = form.window_state;
				SetWindowState (form, old_state, FormWindowState.Maximized, true);
				wm.was_minimized = form.window_state == FormWindowState.Minimized;
				form.window_state = FormWindowState.Maximized;
				SetParentText (false);
			}

			form.BringToFront ();
			form.SendControlFocus (form);
			SetWindowStates (wm);
			if (current != form) {
				form.has_focus = false;
				if (current.IsHandleCreated)
					XplatUI.InvalidateNC (current.Handle);
				if (form.IsHandleCreated)
					XplatUI.InvalidateNC (form.Handle);
				if (raise_deactivate) {
					MdiWindowManager current_wm = (MdiWindowManager) current.window_manager;
					current_wm.RaiseDeactivate ();
					
				}
			}
			active_child = (Form) Controls [0];
			
			if (active_child.Visible) {
				bool raise_activated = ParentForm.ActiveControl != active_child;
				ParentForm.ActiveControl = active_child;
				if (raise_activated) {
					MdiWindowManager active_wm = (MdiWindowManager) active_child.window_manager;
					active_wm.RaiseActivated ();
				}
			}
		}

		internal override IntPtr AfterTopMostControl ()
		{
			// order of scrollbars:
			// top = vertical
			//       sizegrid
			// bottom = horizontal
			if (hbar != null && hbar.Visible)
				return hbar.Handle;
			// no need to check for sizegrip since it will only
			// be visible if hbar is visible.
			if (vbar != null && vbar.Visible)
				return vbar.Handle;
				
			return base.AfterTopMostControl ();
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
			
			bool is_active = wm.IsActive;
			bool maximize_this = false;
			
			if (!is_active){
				return false;
			}
			
			ArrayList minimize_these = new ArrayList ();
			ArrayList normalize_these = new ArrayList ();

			setting_windowstates = true;
			foreach (Form frm in mdi_child_list) {
				if (frm == form) {
					continue;
				} else if (!frm.Visible){
					continue;
				}
				if (frm.WindowState == FormWindowState.Maximized && is_active) {
					maximize_this = true;	
					if (((MdiWindowManager) frm.window_manager).was_minimized) {
						minimize_these.Add (frm); 
					} else {
						normalize_these.Add (frm); 
					}
				}
			}

			if (maximize_this && form.WindowState != FormWindowState.Maximized) {
				wm.was_minimized = form.window_state == FormWindowState.Minimized;
				form.WindowState = FormWindowState.Maximized;
			}
			
			foreach (Form frm in minimize_these)
				frm.WindowState = FormWindowState.Minimized;

			foreach (Form frm in normalize_these)
				frm.WindowState = FormWindowState.Normal;


			SetParentText (false);
			
			XplatUI.RequestNCRecalc (ParentForm.Handle);
			XplatUI.RequestNCRecalc (Handle);

			SizeScrollBars ();

			setting_windowstates = false;

			if (form.MdiParent.MainMenuStrip != null)
				form.MdiParent.MainMenuStrip.RefreshMdiItems ();

			// Implicit menu strip merging
			// - When child is activated
			// - Parent form must have a MainMenuStrip
			// - Find the first menustrip on the child
			// - Merge
			MenuStrip parent_menu = form.MdiParent.MainMenuStrip;

			if (parent_menu != null) {
				if (parent_menu.IsCurrentlyMerged)
					ToolStripManager.RevertMerge (parent_menu);
					
				MenuStrip child_menu = LookForChildMenu (form);

				if (form.WindowState != FormWindowState.Maximized)
					RemoveControlMenuItems (wm);
				
				if (form.WindowState == FormWindowState.Maximized) {
					bool found = false;
					
					foreach (ToolStripItem tsi in parent_menu.Items) {
						if (tsi is MdiControlStrip.SystemMenuItem) {
							(tsi as MdiControlStrip.SystemMenuItem).MdiForm = form;
							found = true;
						} else if (tsi is MdiControlStrip.ControlBoxMenuItem) {
							(tsi as MdiControlStrip.ControlBoxMenuItem).MdiForm = form;
							found = true;
						}
					}	
					
					if (!found) {
						parent_menu.SuspendLayout ();
						parent_menu.Items.Insert (0, new MdiControlStrip.SystemMenuItem (form));
						parent_menu.Items.Add (new MdiControlStrip.ControlBoxMenuItem (form, MdiControlStrip.ControlBoxType.Close));
						parent_menu.Items.Add (new MdiControlStrip.ControlBoxMenuItem (form, MdiControlStrip.ControlBoxType.Max));
						parent_menu.Items.Add (new MdiControlStrip.ControlBoxMenuItem (form, MdiControlStrip.ControlBoxType.Min));
						parent_menu.ResumeLayout ();
					}
				}
				
				if (child_menu != null)
					ToolStripManager.Merge (child_menu, parent_menu);
			}

			return maximize_this;
		}

		private MenuStrip LookForChildMenu (Control parent)
		{
			foreach (Control c in parent.Controls) {
				if (c is MenuStrip)
					return (MenuStrip)c;
					
				if (c is ToolStripContainer || c is ToolStripPanel) {
					MenuStrip ms = LookForChildMenu (c);
					
					if (ms != null)
						return ms;
				}
			}
			
			return null;
		}
		
		internal void RemoveControlMenuItems (MdiWindowManager wm)
		{
			Form form = wm.form;
			MenuStrip parent_menu = form.MdiParent.MainMenuStrip;

			// Only remove the items if the form requesting still owns the menu items
			if (parent_menu != null) {
				parent_menu.SuspendLayout ();

				for (int i = parent_menu.Items.Count - 1; i >= 0; i--) {
					if (parent_menu.Items[i] is MdiControlStrip.SystemMenuItem) {
						if ((parent_menu.Items[i] as MdiControlStrip.SystemMenuItem).MdiForm == form)
							parent_menu.Items.RemoveAt (i);
					} else if (parent_menu.Items[i] is MdiControlStrip.ControlBoxMenuItem) {
						if ((parent_menu.Items[i] as MdiControlStrip.ControlBoxMenuItem).MdiForm == form)
							parent_menu.Items.RemoveAt (i);
					}
				}
				
				parent_menu.ResumeLayout ();
			}
		}

		internal void SetWindowState (Form form, FormWindowState old_window_state, FormWindowState new_window_state, bool is_activating_child)
		{
			bool mdiclient_layout;

			MdiWindowManager wm = (MdiWindowManager) form.window_manager;

			if (!is_activating_child && new_window_state == FormWindowState.Maximized && !wm.IsActive) {
				ActivateChild (form);
				return;
			}
				
			if (old_window_state == FormWindowState.Normal)
				wm.NormalBounds = form.Bounds;

			if (SetWindowStates (wm))
				return;

			if (old_window_state == new_window_state)
				return;

			mdiclient_layout = old_window_state == FormWindowState.Maximized || new_window_state == FormWindowState.Maximized;

			switch (new_window_state) {
			case FormWindowState.Minimized:
				ArrangeIconicWindows (false);
				break;
			case FormWindowState.Maximized:
				form.Bounds = wm.MaximizedBounds;
				break;
			case FormWindowState.Normal:
				form.Bounds = wm.NormalBounds;
				break;
			}

			wm.UpdateWindowDecorations (new_window_state);

			form.ResetCursor ();

			if (mdiclient_layout)
				Parent.PerformLayout ();

			XplatUI.RequestNCRecalc (Parent.Handle);
			XplatUI.RequestNCRecalc (form.Handle);
			if (!setting_windowstates)
				SizeScrollBars ();
		}
		internal int ChildrenCreated {
			get { return mdi_created; }
			set { mdi_created = value; }
		}

		internal Form ActiveMdiChild {
			get {
				if (ParentForm != null && !ParentForm.Visible)
					return null;

				if (Controls.Count < 1)
					return null;
					
				if (!ParentForm.IsHandleCreated)
					return null;
				
				if (!ParentForm.has_been_visible)
					return null;
					
				if (!ParentForm.Visible)
					return active_child;
				
				active_child = null;
				for (int i = 0; i < Controls.Count; i++) {
					if (Controls [i].Visible) {
						active_child = (Form) Controls [i];
						break;
					}
				}
				return active_child;
			}
			set {
				ActivateChild (value);
			}
		}
		
		internal void ActivateActiveMdiChild ()
		{
			if (ParentForm.is_changing_visible_state > 0)
				return;
				
			for (int i = 0; i < Controls.Count; i++) {
				if (Controls [i].Visible) {
					ActivateChild ((Form) Controls [i]);
					return;
				}
			}
		}

		internal Point GetNextStackedFormLocation (CreateParams cp)
		{
			Point previous = next_child_stack_location;
			next_child_stack_location = new Point (previous.X + 22, previous.Y + 22);
			if (!ClientRectangle.Contains (next_child_stack_location.X * 3, next_child_stack_location.Y * 3)) {
				next_child_stack_location = Point.Empty;
			}
			return previous;
		}
	}
}

