//
// System.Windows.Forms.UpDownEventHandler.cs
//
// Authors:
//	//unknown
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;

namespace System.Windows.Forms {

	public class UserControl : ContainerControl{
		// --- Events ---
		public event EventHandler Load;

		// --- Properties ---
		protected override Size DefaultSize {
			get {
				return new Size(150, 150);
			}
		}

		public override string Text {

			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		// --- Constructor ---
		public UserControl()
		{
			// Nothing to do at this time
		}

		// --- Methods ---
		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			OnLoad(EventArgs.Empty);
		}

		protected virtual void OnLoad(EventArgs e)
		{
			if (Load!=null) {
				Load(this, e);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
		}
	}
}
