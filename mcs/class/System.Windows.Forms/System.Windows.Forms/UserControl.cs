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

		// --- Properties ---
		[MonoTODO]
		protected override Size DefaultSize {
			get { return new Size ( 150, 150 ); }
		}

		[MonoTODO]
		public override string Text {

			get {
				//FIXME:
				return base.Text;
			}
			set {
				//FIXME:
				base.Text = value;
			}
		}

		// --- Constructor ---
		[MonoTODO]
		public UserControl()
		{
			
		}

		// --- Methods ---
		[MonoTODO]
		protected override void OnCreateControl()
		{
			//FIXME:
			base.OnCreateControl();
		}

		[MonoTODO]
		protected virtual void OnLoad(EventArgs e)
		{
			//FIXME:
		}

		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs e)
		{
			//FIXME:
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			//FIXME:
		}

		// --- Events ---
		public event EventHandler Load;
	}
}
