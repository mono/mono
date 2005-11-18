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
//			CalcBars ();
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

		internal void EnsureScrollBars (int right, int bottom)
		{
			int width = Width;
			int height = Height;

			if (vbar != null && vbar.Visible)
				width -= vbar.Width;
			if (hbar != null && hbar.Visible)
				height -= hbar.Width;

			if (right > width) {
				if (hbar == null) {
					hbar = new HScrollBar ();
					Controls.AddImplicit (hbar);
				}
				hbar.Visible = true;
			} else {
				if (hbar != null) {
					bool found = false;
					foreach (Form child in Controls) {
						if (child == ActiveMdiChild)
							continue;
						if (child.Right < width)
							continue;
						found = true;
						break;
					}
					hbar.Visible = found;
				}
			}

			if (bottom > height) {
				if (vbar == null) {
					vbar = new VScrollBar ();
					Controls.AddImplicit (vbar);
				}
				vbar.Visible = true;
			} else {
				if (vbar != null) {
					bool found = false;
					foreach (Form child in Controls) {
						if (child == ActiveMdiChild)
							continue;
						if (child.Bottom < height)
							continue;
						found = true;
						break;
					}
					vbar.Visible = found;
				}
			}

			if (hbar != null && hbar.Visible)
				CalcHBar (right, vbar != null && vbar.Visible);
			if (vbar != null && vbar.Visible)
				CalcVBar (bottom, hbar != null && hbar.Visible);
		}

		private void CalcHBar (int right, bool vert_vis)
		{
			hbar.Left = 0;
			hbar.Top = Height - hbar.Height;
			hbar.Width = Width - (vert_vis ? vbar.Width : 0);
			hbar.LargeChange = right;
			hbar.SmallChange = right / 10;
			hbar.Maximum = right - 1;
		}

		private void CalcVBar (int bottom, bool horz_vis)
		{
			vbar.Left = Width - vbar.Width;
			vbar.Top = 0;
			vbar.Height = Height - (horz_vis ? hbar.Height : 0);
			vbar.LargeChange = bottom;
			vbar.SmallChange = bottom / 10;
			vbar.Maximum = bottom - 1;
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
